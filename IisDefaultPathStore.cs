using System.Text.Json;

namespace BLogicDevTool;

public class IisDefaultPathEntry
{
    public string SiteName { get; set; } = "";
    public string AppPath { get; set; } = "";
    public string PhysicalPath { get; set; } = "";
}

public class IisDefaultPaths
{
    public List<IisDefaultPathEntry> Defaults { get; set; } = new();
}

/// <summary>
/// Read-only store of the factory-default PhysicalPath for each IIS app.
/// Loaded from "default_iis_paths.json" shipped beside the executable; falls back
/// to a built-in table if the file is missing or unreadable. Keyed by "site|app"
/// (same convention as <see cref="IisLocalStore"/>). Used by the Reset Path button.
/// </summary>
public static class IisDefaultPathStore
{
    private static readonly string DefaultsFile = Path.Combine(
        AppContext.BaseDirectory, "default_iis_paths.json");

    private static Dictionary<string, string>? _cache;

    private static string Key(string siteName, string appPath) => $"{siteName}|{appPath}";

    private static Dictionary<string, string> Load()
    {
        if (_cache != null) return _cache;

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            if (File.Exists(DefaultsFile))
            {
                var json = File.ReadAllText(DefaultsFile);
                var parsed = JsonSerializer.Deserialize<IisDefaultPaths>(json);
                if (parsed?.Defaults != null)
                    foreach (var d in parsed.Defaults)
                        if (!string.IsNullOrWhiteSpace(d.SiteName) && !string.IsNullOrWhiteSpace(d.AppPath))
                            map[Key(d.SiteName, d.AppPath)] = d.PhysicalPath ?? "";
            }
        }
        catch { /* fall through to built-in */ }

        if (map.Count == 0)
            foreach (var d in BuiltInDefaults())
                map[Key(d.SiteName, d.AppPath)] = d.PhysicalPath;

        _cache = map;
        return map;
    }

    /// <summary>Default PhysicalPath for a site/app, or null if none is defined.</summary>
    public static string? GetDefaultPath(string siteName, string appPath)
    {
        Load().TryGetValue(Key(siteName, appPath), out var path);
        return path;
    }

    public static bool HasDefault(string siteName, string appPath) =>
        Load().ContainsKey(Key(siteName, appPath));

    private static IEnumerable<IisDefaultPathEntry> BuiltInDefaults() => new[]
    {
        new IisDefaultPathEntry { SiteName = "Default Web Site", AppPath = "/", PhysicalPath = @"C:\inetpub\wwwroot\" },
        new IisDefaultPathEntry { SiteName = "Default Web Site", AppPath = "/BLogicConnector", PhysicalPath = @"C:\Program Files (x86)\BLogic Systems\BLogicConnector" },
        new IisDefaultPathEntry { SiteName = "Default Web Site", AppPath = "/BLogicKioskWeb", PhysicalPath = @"C:\Program Files (x86)\BLogic Systems\BLogicKioskWeb\" },
        new IisDefaultPathEntry { SiteName = "Default Web Site", AppPath = "/BLogicService", PhysicalPath = @"C:\inetpub\wwwroot\BLogicService" },
        new IisDefaultPathEntry { SiteName = "Default Web Site", AppPath = "/BLogicTransferMessageService", PhysicalPath = @"C:\inetpub\wwwroot\BLogicTransferMessageService\" },
        new IisDefaultPathEntry { SiteName = "Default Web Site", AppPath = "/QOrderConnectorService", PhysicalPath = @"C:\inetpub\wwwroot\QOrderConnectorService\" },
        new IisDefaultPathEntry { SiteName = "Default Web Site", AppPath = "/BLogicEmailService", PhysicalPath = @"C:\Program Files (x86)\BLogic Systems\BLogic Email Service\" },
        new IisDefaultPathEntry { SiteName = "Default Web Site", AppPath = "/BLogicMerchantService", PhysicalPath = @"C:\inetpub\wwwroot\BLogicMerchantService\" },
        new IisDefaultPathEntry { SiteName = "Default Web Site", AppPath = "/BLogicPaymentService", PhysicalPath = @"C:\Program Files (x86)\BLogic Systems\BLogic Merchant\BLogic Payment Service\" },
    };
}
