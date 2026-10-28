using System.Text.Json;

namespace BLogicDevTool;

/// <summary>One connection string's target server/database inside a preset.</summary>
public sealed class IisEnvPresetItem
{
    public string SiteName { get; set; } = "";
    public string AppPath { get; set; } = "/";

    /// <summary>"connection.connection_string" for NHibernate, otherwise the add/@name.</summary>
    public string EntryName { get; set; } = "";

    public string DataSource { get; set; } = "";
    public string Database { get; set; } = "";

    public string Key => $"{SiteName}|{AppPath}|{EntryName}";
}

/// <summary>A named set of targets, e.g. AGENT = BLogicPOS7_Agent + Merchant_Agent.</summary>
public sealed class IisEnvPreset
{
    public string Name { get; set; } = "";
    public List<IisEnvPresetItem> Items { get; set; } = new();
    public DateTime SavedAt { get; set; } = DateTime.Now;

    /// <summary>Alias database to fill with synonyms, or "" when the preset does not use one.</summary>
    public string MerchantAliasDb { get; set; } = "";

    /// <summary>Real merchant database the alias forwards to.</summary>
    public string MerchantRealDb { get; set; } = "";

    public override string ToString() => Name;
}

/// <summary>Presets stored in the "IisEnvPresets" section of <see cref="AppConfigStore"/>.</summary>
public static class IisEnvPresetStore
{
    public static List<IisEnvPreset> LoadAll() =>
        AppConfigStore.Get(c => c.IisEnvPresets, (c, v) => c.IisEnvPresets = v, () => new List<IisEnvPreset>())
            .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();

    public static IisEnvPreset? Get(string name) =>
        LoadAll().FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Adds or replaces a preset by name.</summary>
    public static void Save(IisEnvPreset preset)
    {
        if (string.IsNullOrWhiteSpace(preset.Name))
            throw new ArgumentException("Preset name is empty.");

        var all = LoadAll();
        all.RemoveAll(p => p.Name.Equals(preset.Name, StringComparison.OrdinalIgnoreCase));
        preset.SavedAt = DateTime.Now;
        all.Add(preset);
        Persist(all);
    }

    public static void Delete(string name)
    {
        var all = LoadAll();
        if (all.RemoveAll(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) > 0)
            Persist(all);
    }

    private static void Persist(List<IisEnvPreset> all) =>
        AppConfigStore.Update(c => c.IisEnvPresets = all);
}
