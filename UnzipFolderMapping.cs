namespace BLogicDevTool;

/// <summary>Mapping from a top-level folder name inside the zip to its destination directory.</summary>
public class UnzipFolderMapping
{
    public string FolderName { get; set; } = "";
    public string DestinationPath { get; set; } = "";
}

/// <summary>Unzip folder mappings, stored in the "UnzipMappings" section of <see cref="AppConfigStore"/>.</summary>
public static class UnzipConfigStore
{
    /// <summary>Default mappings used when no config file exists yet.</summary>
    private static List<UnzipFolderMapping> Defaults() => new()
    {
        new() { FolderName = "POS",             DestinationPath = @"C:\Program Files (x86)\BLogic Systems\BLogic PointOfSale" },
        new() { FolderName = "BO",              DestinationPath = @"C:\Program Files (x86)\BLogic Systems\BLogic POSDashboard" },
        new() { FolderName = "BLogicConnector", DestinationPath = @"C:\Program Files (x86)\BLogic Systems\BLogicConnector" },
        new() { FolderName = "Server",          DestinationPath = @"C:\inetpub\wwwroot\BLogicService" },
        new() { FolderName = "BLogicReportService",     DestinationPath = @"C:\Program Files (x86)\BLogic Systems\BLogic Service\BLogicReportService" },
        new() { FolderName = "BLogic Payment Service",  DestinationPath = @"C:\Program Files (x86)\BLogic Systems\BLogic Merchant\BLogic Payment Service" },
        new() { FolderName = "BLogic Merchant Service", DestinationPath = @"C:\Program Files (x86)\BLogic Systems\BLogic Merchant\BLogic Merchant Service" },
        new() { FolderName = "BLogic Start Kiosk",      DestinationPath = @"C:\Program Files (x86)\BLogic Systems\BLogic Start Kiosk" },
    };

    public static List<UnzipFolderMapping> Load()
    {
        var list = AppConfigStore.Get(c => c.UnzipMappings, (c, v) => c.UnzipMappings = v, Defaults);
        return list.Count > 0 ? list : Defaults();
    }

    public static void Save(List<UnzipFolderMapping> mappings) =>
        AppConfigStore.Update(c => c.UnzipMappings = mappings);
}
