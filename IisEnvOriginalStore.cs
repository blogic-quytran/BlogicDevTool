namespace BLogicDevTool;

/// <summary>Server/database a connection string had before Switch Environment first touched it.
/// For a Windows service SiteName is "Windows Service" and AppPath the service name.</summary>
public sealed class IisEnvOriginal
{
    public string SiteName { get; set; } = "";
    public string AppPath { get; set; } = "/";
    public string EntryName { get; set; } = "";
    public string DataSource { get; set; } = "";
    public string Database { get; set; } = "";
    public DateTime SavedAt { get; set; } = DateTime.Now;
}

/// <summary>Baselines for "Reset to default", stored in the "IisEnvOriginals" section of <see cref="AppConfigStore"/>.</summary>
public static class IisEnvOriginalStore
{
    public static string Key(string siteName, string appPath, string entryName) =>
        $"{siteName}|{appPath}|{entryName}";

    public static Dictionary<string, IisEnvOriginal> LoadAll() =>
        new(AppConfigStore.Get(c => c.IisEnvOriginals, (c, v) => c.IisEnvOriginals = v,
                () => new Dictionary<string, IisEnvOriginal>()),
            StringComparer.OrdinalIgnoreCase);

    /// <summary>Records the baseline only when none exists, so A→B→C still resets to A.</summary>
    public static void RecordIfMissing(string siteName, string appPath, ConnStringEntry entry)
    {
        var all = LoadAll();
        var key = Key(siteName, appPath, entry.Name);
        if (all.ContainsKey(key)) return;

        all[key] = new IisEnvOriginal
        {
            SiteName = siteName,
            AppPath = appPath,
            EntryName = entry.Name,
            DataSource = entry.DataSource,
            Database = entry.Database
        };
        Persist(all);
    }

    public static void Remove(IEnumerable<string> keys)
    {
        var all = LoadAll();
        var removed = keys.Count(k => all.Remove(k));
        if (removed > 0) Persist(all);
    }

    private static void Persist(Dictionary<string, IisEnvOriginal> all) =>
        AppConfigStore.Update(c => c.IisEnvOriginals = all);
}
