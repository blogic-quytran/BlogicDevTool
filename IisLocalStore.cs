using System.Text.Json;

namespace BLogicDevTool;

/// <summary>Persisted snapshot of an IIS app entry used for undo/reset.</summary>
public class IisSnapshot
{
    public string SiteName { get; set; } = "";
    public string AppPath { get; set; } = "";
    public string PhysicalPath { get; set; } = "";
    public string Database { get; set; } = "";
    public DateTime SavedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// JSON-backed local store for IIS change history.
/// File: %APPDATA%\BLogicDevTool\iis_snapshots.json
/// </summary>
public class IisLocalStore
{
    private static readonly string StorePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BLogicDevTool",
        "iis_snapshots.json");

    private static readonly JsonSerializerOptions _jsonOpts =
        new() { WriteIndented = true };

    // ── Snapshot key: "{siteName}|{appPath}" ─────────────────────────────────

    private static string Key(string siteName, string appPath) =>
        $"{siteName}|{appPath}";

    // ── Persistence ───────────────────────────────────────────────────────────

    private static Dictionary<string, IisSnapshot> Load()
    {
        if (!File.Exists(StorePath)) return new();
        try
        {
            var json = File.ReadAllText(StorePath);
            return JsonSerializer.Deserialize<Dictionary<string, IisSnapshot>>(json) ?? new();
        }
        catch { return new(); }
    }

    private static void Save(Dictionary<string, IisSnapshot> data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);
        File.WriteAllText(StorePath, JsonSerializer.Serialize(data, _jsonOpts));
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Save a snapshot (overwrites previous for same site+app).</summary>
    public static void SaveSnapshot(IisAppEntry entry)
    {
        var data = Load();
        data[Key(entry.SiteName, entry.AppPath)] = new IisSnapshot
        {
            SiteName = entry.SiteName,
            AppPath = entry.AppPath,
            PhysicalPath = entry.PhysicalPath,
            Database = entry.CurrentDatabase,
            SavedAt = DateTime.Now
        };
        Save(data);
    }

    /// <summary>Retrieve the saved snapshot for a site/app, or null.</summary>
    public static IisSnapshot? GetSnapshot(string siteName, string appPath)
    {
        var data = Load();
        data.TryGetValue(Key(siteName, appPath), out var snap);
        return snap;
    }

    /// <summary>
    /// Save snapshot only if no snapshot has been recorded yet for this site+app.
    /// Used to capture the original baseline on first load.
    /// </summary>
    public static void SaveInitialSnapshot(IisAppEntry entry)
    {
        var data = Load();
        var key  = Key(entry.SiteName, entry.AppPath);
        if (!data.ContainsKey(key))
        {
            data[key] = new IisSnapshot
            {
                SiteName     = entry.SiteName,
                AppPath      = entry.AppPath,
                PhysicalPath = entry.PhysicalPath,
                Database     = entry.CurrentDatabase,
                SavedAt      = DateTime.Now
            };
            Save(data);
        }
    }

    /// <summary>Remove saved snapshot (after successful reset).</summary>
    public static void DeleteSnapshot(string siteName, string appPath)
    {
        var data = Load();
        if (data.Remove(Key(siteName, appPath)))
            Save(data);
    }
}
