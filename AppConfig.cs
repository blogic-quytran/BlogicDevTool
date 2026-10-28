using System.Text.Json;

namespace BLogicDevTool;

/// <summary>Everything the tool persists, in one file.</summary>
public sealed class AppConfig
{
    public int Version { get; set; } = 1;
    public BuildSettings? Build { get; set; }
    public List<UnzipFolderMapping>? UnzipMappings { get; set; }
    public WebDavSettings? WebDav { get; set; }
    public List<IisEnvPreset>? IisEnvPresets { get; set; }

    /// <summary>Pre-switch baselines keyed "site|app|entry"; machine state, never seeded from defaults.</summary>
    public Dictionary<string, IisEnvOriginal>? IisEnvOriginals { get; set; }

    /// <summary>Keyed by "site|app".</summary>
    public Dictionary<string, IisSnapshot>? IisSnapshots { get; set; }

    /// <summary>Factory PhysicalPath per IIS app, used by Reset Path.</summary>
    public List<IisDefaultPathEntry>? IisDefaultPaths { get; set; }
}

/// <summary>Single JSON config beside the exe; first run imports the old per-feature files
/// (left on disk, no longer read).</summary>
public static class AppConfigStore
{
    public static readonly string StorePath = Path.Combine(
        AppContext.BaseDirectory, "blogic_devtool_config.json");

    private static readonly string LegacyAppDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BLogicDevTool");

    // Embedded (not copied to output) so the exe folder holds only the one config file.
    private const string DefaultsResource = "BLogicDevTool.default_config.json";

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };
    private static readonly object Gate = new();
    private static AppConfig? _cache;
    private static AppConfig? _defaults;

    /// <summary>Returns a copy of a section (so unsaved edits never leak into the cache),
    /// creating and persisting its default when missing.</summary>
    public static T Get<T>(Func<AppConfig, T?> get, Action<AppConfig, T> set, Func<T> createDefault)
        where T : class
    {
        lock (Gate)
        {
            var cfg = LoadLocked();
            var value = get(cfg);
            if (value == null)
            {
                value = get(Defaults()) ?? createDefault();
                set(cfg, value);
                TrySaveLocked(cfg);
            }
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
        }
    }

    /// <summary>Mutates the config and writes it to disk.</summary>
    public static void Update(Action<AppConfig> mutate)
    {
        lock (Gate)
        {
            var cfg = LoadLocked();
            mutate(cfg);
            SaveLocked(cfg);
        }
    }

    private static AppConfig LoadLocked()
    {
        if (_cache != null) return _cache;

        if (File.Exists(StorePath))
        {
            try
            {
                _cache = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(StorePath)) ?? new AppConfig();
                return _cache;
            }
            catch
            {
                // Keep the unreadable file for inspection instead of silently overwriting it.
                try { File.Copy(StorePath, StorePath + ".corrupt", overwrite: true); } catch { }
                _cache = new AppConfig();
                return _cache;
            }
        }

        _cache = MigrateLegacy();
        FillFromDefaults(_cache);
        TrySaveLocked(_cache);
        return _cache;
    }

    /// <summary>The shipped default_config.json; empty sections when the resource is missing.</summary>
    private static AppConfig Defaults()
    {
        if (_defaults != null) return _defaults;
        try
        {
            using var stream = typeof(AppConfigStore).Assembly.GetManifestResourceStream(DefaultsResource);
            _defaults = stream != null ? JsonSerializer.Deserialize<AppConfig>(stream) : null;
        }
        catch { /* fall back to the stores' built-in defaults */ }
        return _defaults ??= new AppConfig();
    }

    // Snapshots are machine state, so they are never seeded from the defaults.
    private static void FillFromDefaults(AppConfig cfg)
    {
        var d = Defaults();
        cfg.Build ??= d.Build;
        cfg.UnzipMappings ??= d.UnzipMappings;
        cfg.WebDav ??= d.WebDav;
        cfg.IisEnvPresets ??= d.IisEnvPresets;
        cfg.IisDefaultPaths ??= d.IisDefaultPaths;
    }

    private static void SaveLocked(AppConfig cfg)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);
        var tmp = StorePath + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(cfg, JsonOpts));
        File.Move(tmp, StorePath, overwrite: true);
    }

    private static void TrySaveLocked(AppConfig cfg)
    {
        try { SaveLocked(cfg); } catch { /* read-only location: keep working in memory */ }
    }

    // ── Legacy migration ─────────────────────────────────────────────────────

    private static AppConfig MigrateLegacy()
    {
        var cfg = new AppConfig();

        var buildPath = FirstExisting(
            Path.Combine(AppContext.BaseDirectory, "build_config.json"),
            Path.Combine(LegacyAppDataDir, "build_config.json"));
        if (buildPath != null)
            cfg.Build = BuildConfigStore.ParseLegacy(File.ReadAllText(buildPath));

        cfg.UnzipMappings = ReadLegacy<List<UnzipFolderMapping>>(
            Path.Combine(AppContext.BaseDirectory, "unzip_config.json"),
            Path.Combine(LegacyAppDataDir, "unzip_config.json"));
        if (cfg.UnzipMappings is { Count: 0 }) cfg.UnzipMappings = null;

        cfg.WebDav = ReadLegacy<WebDavSettings>(Path.Combine(LegacyAppDataDir, "webdav_settings.json"));
        cfg.IisEnvPresets = ReadLegacy<List<IisEnvPreset>>(Path.Combine(LegacyAppDataDir, "iis_env_presets.json"));
        cfg.IisSnapshots = ReadLegacy<Dictionary<string, IisSnapshot>>(Path.Combine(LegacyAppDataDir, "iis_snapshots.json"));
        cfg.IisDefaultPaths = ReadLegacy<IisDefaultPaths>(
            Path.Combine(AppContext.BaseDirectory, "default_iis_paths.json"))?.Defaults;
        if (cfg.IisDefaultPaths is { Count: 0 }) cfg.IisDefaultPaths = null;

        return cfg;
    }

    private static string? FirstExisting(params string[] paths) =>
        paths.FirstOrDefault(File.Exists);

    private static T? ReadLegacy<T>(params string[] paths) where T : class
    {
        var path = FirstExisting(paths);
        if (path == null) return null;
        try { return JsonSerializer.Deserialize<T>(File.ReadAllText(path)); }
        catch { return null; }
    }
}
