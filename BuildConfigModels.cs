using System.Text.Json.Serialization;

namespace BLogicDevTool;

public class BuildConfig
{
    public string Name { get; set; } = "";
    public string SolutionPath { get; set; } = "";
    public string BuildEngine { get; set; } = "dotnet"; // "dotnet" or "msbuild"
    /// <summary>One or more destination folders (relative to OutputBasePath), separated
    /// by ';' or ',' — e.g. "POS; POS/CCP/pax". Every destination receives the same
    /// artifacts + extra folders. Empty means OutputBasePath itself.</summary>
    public string OutputFolderName { get; set; } = "";
    public List<string> ExtraFolders { get; set; } = new();

    /// <summary>Destination folders parsed from <see cref="OutputFolderName"/>,
    /// normalized to OS separators; a single "" entry when none is set.</summary>
    public List<string> GetOutputFolders()
    {
        var list = (OutputFolderName ?? "")
            .Split(new[] { ';', ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().Replace('/', Path.DirectorySeparatorChar)
                          .Trim(Path.DirectorySeparatorChar))
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (list.Count == 0) list.Add("");
        return list;
    }
    public List<string> RedistributeTo { get; set; } = new();
    /// <summary>Legacy per-config flag — read only to migrate into
    /// <see cref="BuildSettings.FilterByGit"/>; never written back.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? FilterByGit { get; set; }
}

public class BuildSettings
{
    public string OutputBasePath { get; set; } = "";

    /// <summary>Build configuration (e.g. "Release"/"Debug") shared across all configs.</summary>
    public string Configuration { get; set; } = "Release";

    public List<string> CopyExtensions { get; set; } =
        new() { ".dll", ".exe", ".pdb", ".xml", ".exe.config" };
    public string GitBaseBranch { get; set; } = "dev";
    public string GitCompareBranch { get; set; } = "HEAD";

    /// <summary>When true, the git-diff filter compares the base ref against the
    /// WORKING TREE (committed + staged + unstaged) and also includes new untracked
    /// files — instead of only committed changes (base...compare). The compare ref
    /// is ignored in this mode.</summary>
    public bool IncludeUncommitted { get; set; } = false;

    /// <summary>Shared by all configs: build/copy only the projects touched by the
    /// git diff (base vs compare). Null = not yet migrated from per-config flags.</summary>
    public bool? FilterByGit { get; set; }

    /// <summary>Full rebuild: empties the destination folder(s) AND runs a solution Clean.
    /// Off means an incremental build, which is the difference between seconds and
    /// recompiling every project on every run.</summary>
    public bool CleanOutputFirst { get; set; } = true;

    /// <summary>When true, after a successful build run the whole OutputBasePath
    /// folder is zipped into "&lt;folderName&gt;.zip" placed inside that folder.</summary>
    public bool ZipAfterBuild { get; set; } = true;

    public List<BuildConfig> Configs { get; set; } = new();

    /// <summary>Absolute destination folder(s) for a config; empty when OutputBasePath is unset.</summary>
    public List<string> GetFinalOutputPaths(BuildConfig c)
    {
        if (string.IsNullOrWhiteSpace(OutputBasePath)) return new();
        return c.GetOutputFolders()
            .Select(f => f.Length == 0 ? OutputBasePath : Path.Combine(OutputBasePath, f))
            .ToList();
    }

    // MerchantUI DLLs are loaded by both POS and Merchant Manager, including their CCP plug-in folders.
    public const string MerchantUiOutputFolders =
        "POS; POS/CCP/pax; POS/CCP/BLS; "
        + "BLogic Merchant/BLogic Merchant Manager; BLogic Merchant/BLogic Merchant Manager/CCP/pax";

    public static BuildSettings CreateDefault() => new()
    {
        OutputBasePath = @"D:\BLOGIC\Task\dll20260519_PS-2283_RedesignSchedulePrepPrinterSetting",
        Configuration = "Release",
        CopyExtensions = new() { ".dll", ".exe", ".resources.dll" },
        GitBaseBranch = "origin/develop",
        GitCompareBranch = "HEAD",
        FilterByGit = true,
        Configs = new()
        {
            new BuildConfig
            {
                Name = "Common",
                SolutionPath = @"D:\BLOGIC\Planet\Common\Source\BLogicSystems.xPos.Common.sln",
                BuildEngine = "msbuild",
                OutputFolderName = "Server/bin; POS; BO",
            },
            new BuildConfig
            {
                Name = "Server",
                SolutionPath = @"D:\BLOGIC\Planet\Server\Source\BLogicSystems.xPos.Server.sln",
                BuildEngine = "msbuild",
                OutputFolderName = "Server/bin",
                ExtraFolders = new() { "Language", "Templates" },
            },
            new BuildConfig
            {
                Name = "POS",
                SolutionPath = @"D:\BLOGIC\Planet\PointOfSale\Source\BLogicSystems.xPos.PointOfSale.sln",
                BuildEngine = "msbuild",
                OutputFolderName = "POS",
                ExtraFolders = new() { "Language", "Templates" },
            },
            new BuildConfig
            {
                Name = "BO",
                SolutionPath = @"D:\BLOGIC\Planet\BackOffice\Source\BLogicSystems.POS.BackOffice.sln",
                BuildEngine = "msbuild",
                OutputFolderName = "BO",
                ExtraFolders = new() { "Language", "Templates" },
            },
            new BuildConfig
            {
                Name = "MerchantCommon",
                SolutionPath = @"D:\BLOGIC\MerchantUI\MerchantCommon\Source\MerchantCommon.sln",
                BuildEngine = "msbuild",
                OutputFolderName = MerchantUiOutputFolders,
            },
            new BuildConfig
            {
                Name = "MerchantGUI",
                SolutionPath = @"D:\BLOGIC\MerchantUI\MerchantGUI\Source\MerchantGUI.sln",
                BuildEngine = "msbuild",
                OutputFolderName = MerchantUiOutputFolders,
            },
            new BuildConfig
            {
                Name = "MerchantService",
                SolutionPath = @"D:\BLOGIC\MerchantUI\MerchantService\Source\MerchantService.sln",
                BuildEngine = "msbuild",
                OutputFolderName = "BLogic Merchant/BLogic Merchant Service",
            },
            new BuildConfig
            {
                Name = "PaymentService",
                SolutionPath = @"D:\BLOGIC\Merchant\PaymentService.sln",
                BuildEngine = "msbuild",
                OutputFolderName = "BLogic Merchant/BLogic Payment Service/bin",
            },
            new BuildConfig
            {
                Name = "BLogicConnector",
                SolutionPath = @"D:\BLOGIC\BLogicConnector\BLogicConnector.sln",
                BuildEngine = "msbuild",
                OutputFolderName = "BLogicConnector",
            },
        },
    };
}

public class BuildArtifact
{
    public string ProjectPath { get; set; } = "";
    public string AssemblyName { get; set; } = "";
    public string TargetFramework { get; set; } = "";
    public string OutputDllPath { get; set; } = "";
}
