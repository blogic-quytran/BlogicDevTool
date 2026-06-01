namespace BLogicDevTool;

public class BuildConfig
{
    public string Name { get; set; } = "";
    public string SolutionPath { get; set; } = "";
    public string BuildEngine { get; set; } = "dotnet"; // "dotnet" or "msbuild"
    public string OutputFolderName { get; set; } = "";
    public List<string> ExtraFolders { get; set; } = new();
    public List<string> RedistributeTo { get; set; } = new();
    public bool FilterByGit { get; set; } = false;
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

    /// <summary>When true, the destination output folder(s) of the configs being built
    /// (and their redistribute targets) are emptied once before the build run, so stale
    /// artifacts from previous builds don't leak into the release.</summary>
    public bool CleanOutputFirst { get; set; } = true;

    /// <summary>When true, after a successful build run the whole OutputBasePath
    /// folder is zipped into "&lt;folderName&gt;.zip" placed inside that folder.</summary>
    public bool ZipAfterBuild { get; set; } = true;

    public List<BuildConfig> Configs { get; set; } = new();

    public string GetFinalOutputPath(BuildConfig c)
    {
        if (string.IsNullOrWhiteSpace(OutputBasePath)) return "";
        var folder = (c.OutputFolderName ?? "").Replace('/', Path.DirectorySeparatorChar).Trim();
        return string.IsNullOrEmpty(folder)
            ? OutputBasePath
            : Path.Combine(OutputBasePath, folder);
    }

    public static BuildSettings CreateDefault() => new()
    {
        OutputBasePath = @"D:\BLOGIC\Task\dll20260519_PS-2283_RedesignSchedulePrepPrinterSetting",
        Configuration = "Release",
        CopyExtensions = new() { ".dll", ".exe", ".resources.dll" },
        GitBaseBranch = "origin/develop",
        GitCompareBranch = "HEAD",
        Configs = new()
        {
            new BuildConfig
            {
                Name = "Common",
                SolutionPath = @"D:\BLOGIC\Planet\Common\Source\BLogicSystems.xPos.Common.sln",
                BuildEngine = "msbuild",
                OutputFolderName = "Common",
                RedistributeTo = new() { "Server", "POS", "BO" },
                FilterByGit = true,
            },
            new BuildConfig
            {
                Name = "Server",
                SolutionPath = @"D:\BLOGIC\Planet\Server\Source\BLogicSystems.xPos.Server.sln",
                BuildEngine = "msbuild",
                OutputFolderName = "Server/bin",
                ExtraFolders = new() { "Language", "Templates" },
                FilterByGit = true,
            },
            new BuildConfig
            {
                Name = "POS",
                SolutionPath = @"D:\BLOGIC\Planet\PointOfSale\Source\BLogicSystems.xPos.PointOfSale.sln",
                BuildEngine = "msbuild",
                OutputFolderName = "POS",
                ExtraFolders = new() { "Language", "Templates" },
                FilterByGit = true,
            },
            new BuildConfig
            {
                Name = "BO",
                SolutionPath = @"D:\BLOGIC\Planet\BackOffice\Source\BLogicSystems.POS.BackOffice.sln",
                BuildEngine = "msbuild",
                OutputFolderName = "BO",
                ExtraFolders = new() { "Language", "Templates" },
                FilterByGit = true,
            },
            new BuildConfig
            {
                Name = "BLogicConnector",
                SolutionPath = @"D:\BLOGIC\BLogicConnector\BLogicConnector.sln",
                BuildEngine = "msbuild",
                OutputFolderName = "BLogicConnector",
                FilterByGit = false,
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
