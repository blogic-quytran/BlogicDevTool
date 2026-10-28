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
/// Factory-default PhysicalPath per IIS app for the Reset Path button, from the
/// "IisDefaultPaths" section of <see cref="AppConfigStore"/> (seeded from the built-in table).
/// </summary>
public static class IisDefaultPathStore
{
    private static Dictionary<string, string>? _cache;

    private static string Key(string siteName, string appPath) => $"{siteName}|{appPath}";

    private static Dictionary<string, string> Load()
    {
        if (_cache != null) return _cache;

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var entries = AppConfigStore.Get(c => c.IisDefaultPaths, (c, v) => c.IisDefaultPaths = v,
            () => BuiltInDefaults().ToList());
        foreach (var d in entries)
            if (!string.IsNullOrWhiteSpace(d.SiteName) && !string.IsNullOrWhiteSpace(d.AppPath))
                map[Key(d.SiteName, d.AppPath)] = d.PhysicalPath ?? "";

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
