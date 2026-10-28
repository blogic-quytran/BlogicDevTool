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

/// <summary>IIS change history, stored in the "IisSnapshots" section of <see cref="AppConfigStore"/>.</summary>
public class IisLocalStore
{
    // ── Snapshot key: "{siteName}|{appPath}" ─────────────────────────────────

    private static string Key(string siteName, string appPath) =>
        $"{siteName}|{appPath}";

    // ── Persistence ───────────────────────────────────────────────────────────

    private static Dictionary<string, IisSnapshot> Load() =>
        AppConfigStore.Get(c => c.IisSnapshots, (c, v) => c.IisSnapshots = v, () => new Dictionary<string, IisSnapshot>());

    private static void Save(Dictionary<string, IisSnapshot> data) =>
        AppConfigStore.Update(c => c.IisSnapshots = data);

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
