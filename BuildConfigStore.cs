using System.Text.Json;

namespace BLogicDevTool;

public static class BuildConfigStore
{
    public static BuildSettings Load()
    {
        var s = AppConfigStore.Get(c => c.Build, (c, v) => c.Build = v, BuildSettings.CreateDefault);
        // Filter-by-git used to be per config; lift it to the shared setting once.
        s.FilterByGit ??= s.Configs.Any(c => c.FilterByGit == true);
        foreach (var c in s.Configs) c.FilterByGit = null;
        return s;
    }

    public static void Save(BuildSettings settings) =>
        AppConfigStore.Update(c => c.Build = settings);

    /// <summary>Reads an old build_config.json, including the pre-BuildSettings array format.</summary>
    internal static BuildSettings ParseLegacy(string json)
    {
        try
        {
            var trimmed = json.TrimStart();
            BuildSettings settings;
            JsonElement rootElement;

            if (trimmed.StartsWith("["))
            {
                using var legacyDoc = JsonDocument.Parse(json);
                settings = new BuildSettings();
                foreach (var item in legacyDoc.RootElement.EnumerateArray())
                {
                    if (settings.OutputBasePath == ""
                        && item.TryGetProperty("OutputBasePath", out var basePath)
                        && basePath.ValueKind == JsonValueKind.String)
                    {
                        settings.OutputBasePath = basePath.GetString() ?? "";
                    }
                    var cfg = item.Deserialize<BuildConfig>();
                    if (cfg != null) settings.Configs.Add(cfg);
                }
                rootElement = legacyDoc.RootElement;
                MigrateGitFieldsFromConfigsArray(rootElement, settings);
                MigrateConfigurationFromConfigsArray(rootElement, settings);
                return settings;
            }

            settings = JsonSerializer.Deserialize<BuildSettings>(json) ?? new();
            using var doc = JsonDocument.Parse(json);
            bool hasTopLevelConfig = doc.RootElement.TryGetProperty("Configuration", out _);
            if (doc.RootElement.TryGetProperty("Configs", out var configsEl)
                && configsEl.ValueKind == JsonValueKind.Array)
            {
                MigrateGitFieldsFromConfigsArray(configsEl, settings);
                if (!hasTopLevelConfig)
                    MigrateConfigurationFromConfigsArray(configsEl, settings);
            }
            return settings;
        }
        catch { return new(); }
    }

    private static void MigrateGitFieldsFromConfigsArray(JsonElement configsArray, BuildSettings settings)
    {
        // If global settings still hold defaults, lift first non-empty per-config values.
        bool baseEmpty = string.IsNullOrEmpty(settings.GitBaseBranch)
                         || settings.GitBaseBranch == "dev";
        bool compareEmpty = string.IsNullOrEmpty(settings.GitCompareBranch)
                            || settings.GitCompareBranch == "HEAD";

        if (!baseEmpty && !compareEmpty) return;
        if (configsArray.ValueKind != JsonValueKind.Array) return;

        foreach (var item in configsArray.EnumerateArray())
        {
            if (baseEmpty
                && item.TryGetProperty("GitBaseBranch", out var gb)
                && gb.ValueKind == JsonValueKind.String)
            {
                var v = gb.GetString();
                if (!string.IsNullOrWhiteSpace(v))
                {
                    settings.GitBaseBranch = v;
                    baseEmpty = false;
                }
            }
            if (compareEmpty
                && item.TryGetProperty("GitCompareBranch", out var gc)
                && gc.ValueKind == JsonValueKind.String)
            {
                var v = gc.GetString();
                if (!string.IsNullOrWhiteSpace(v))
                {
                    settings.GitCompareBranch = v;
                    compareEmpty = false;
                }
            }
            if (!baseEmpty && !compareEmpty) break;
        }
    }

    private static void MigrateConfigurationFromConfigsArray(JsonElement configsArray, BuildSettings settings)
    {
        // Old per-config "Configuration" is now a single shared setting.
        // Lift the first non-empty per-config value when the global one is still default.
        if (configsArray.ValueKind != JsonValueKind.Array) return;
        if (!string.IsNullOrWhiteSpace(settings.Configuration)
            && settings.Configuration != "Release") return;

        foreach (var item in configsArray.EnumerateArray())
        {
            if (item.TryGetProperty("Configuration", out var cfg)
                && cfg.ValueKind == JsonValueKind.String)
            {
                var v = cfg.GetString();
                if (!string.IsNullOrWhiteSpace(v))
                {
                    settings.Configuration = v;
                    return;
                }
            }
        }
    }
}
