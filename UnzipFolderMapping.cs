using System.Text.Json;

namespace BLogicDevTool;

/// <summary>Mapping from a top-level folder name inside the zip to its destination directory.</summary>
public class UnzipFolderMapping
{
    public string FolderName { get; set; } = "";
    public string DestinationPath { get; set; } = "";
}

/// <summary>
/// JSON-backed store for unzip folder mappings.
/// File: %APPDATA%\BLogicDevTool\unzip_config.json
/// </summary>
public static class UnzipConfigStore
{
    private static readonly string StorePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BLogicDevTool",
        "unzip_config.json");

    private static readonly JsonSerializerOptions _jsonOpts =
        new() { WriteIndented = true };

    /// <summary>Default mappings shown in the image provided by the user.</summary>
    private static List<UnzipFolderMapping> Defaults() => new()
    {
        new() { FolderName = "POS",             DestinationPath = @"C:\Program Files (x86)\BLogic Systems\BLogic PointOfSale" },
        new() { FolderName = "BO",              DestinationPath = @"C:\Program Files (x86)\BLogic Systems\BLogic POSDashboard" },
        new() { FolderName = "BLogicConnector", DestinationPath = @"C:\Program Files (x86)\BLogic Systems\BLogicConnector\bin" },
        new() { FolderName = "Server",          DestinationPath = @"C:\inetpub\wwwroot\BLogicService" },
    };

    public static List<UnzipFolderMapping> Load()
    {
        if (!File.Exists(StorePath)) return Defaults();
        try
        {
            var json = File.ReadAllText(StorePath);
            var list = JsonSerializer.Deserialize<List<UnzipFolderMapping>>(json);
            return list != null && list.Count > 0 ? list : Defaults();
        }
        catch { return Defaults(); }
    }

    public static void Save(List<UnzipFolderMapping> mappings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);
        File.WriteAllText(StorePath, JsonSerializer.Serialize(mappings, _jsonOpts));
    }
}
