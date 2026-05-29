using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace BLogicDevTool;

public static class BuildHelper
{
    public static async Task<(int ExitCode, string FullOutput)> BuildSolutionAsync(
        string solutionPath, string configuration, string engine, Action<string>? logger = null)
    {
        var workingDir = Path.GetDirectoryName(solutionPath) ?? Environment.CurrentDirectory;

        if (string.Equals(engine, "msbuild", StringComparison.OrdinalIgnoreCase))
        {
            var msbuild = await FindMsBuildPathAsync();
            if (string.IsNullOrEmpty(msbuild))
            {
                var msg = "MSBuild.exe not found via vswhere. " +
                          "Install Visual Studio (or VS Build Tools) with the MSBuild component.";
                logger?.Invoke("[ERROR] " + msg);
                return (-1, msg);
            }
            logger?.Invoke($"[INFO] Using MSBuild: {msbuild}");
            var msArgs = $"\"{solutionPath}\" /t:Build " +
                         $"/p:Configuration={configuration} " +
                         $"/m /nologo /v:minimal /clp:Summary";
            return await RunProcessAsync(msbuild, msArgs, workingDir,
                timeoutMs: 1_800_000, logger);
        }

        var dotnetArgs = $"build \"{solutionPath}\" -c {configuration} --nologo -v minimal";
        return await RunProcessAsync("dotnet", dotnetArgs, workingDir,
            timeoutMs: 1_800_000, logger);
    }

    private static string? _cachedMsBuildPath;

    public static async Task<string?> FindMsBuildPathAsync()
    {
        if (!string.IsNullOrEmpty(_cachedMsBuildPath) && File.Exists(_cachedMsBuildPath))
            return _cachedMsBuildPath;

        var pf86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)")
                   ?? @"C:\Program Files (x86)";
        var vswhere = Path.Combine(pf86,
            "Microsoft Visual Studio", "Installer", "vswhere.exe");
        if (!File.Exists(vswhere)) return null;

        try
        {
            var (exit, output) = await RunProcessAsync(vswhere,
                "-latest -prerelease -products * " +
                "-requires Microsoft.Component.MSBuild " +
                "-find MSBuild\\**\\Bin\\MSBuild.exe",
                Environment.CurrentDirectory, timeoutMs: 10_000, logger: null);
            if (exit != 0) return null;

            var path = output.Split('\n')
                .Select(s => s.Trim())
                .FirstOrDefault(s => s.EndsWith("MSBuild.exe", StringComparison.OrdinalIgnoreCase)
                                     && File.Exists(s));
            _cachedMsBuildPath = path;
            return path;
        }
        catch { return null; }
    }

    public static string SaveBuildLog(BuildConfig config, string configuration, string outputPath, int exitCode, string output)
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "build_logs");
        Directory.CreateDirectory(dir);

        var safeName = SanitizeFileName(string.IsNullOrWhiteSpace(config.Name) ? "build" : config.Name);
        var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var status = exitCode == 0 ? "ok" : "failed";
        var path = Path.Combine(dir, $"{safeName}_{ts}_{status}.log");

        var sb = new StringBuilder();
        sb.AppendLine("=== BLogicDevTool Build Log ===");
        sb.AppendLine($"Timestamp:     {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Config name:   {config.Name}");
        sb.AppendLine($"Solution:      {config.SolutionPath}");
        sb.AppendLine($"Configuration: {configuration}");
        sb.AppendLine($"Output path:   {outputPath}");
        sb.AppendLine($"Exit code:     {exitCode}");
        sb.AppendLine(new string('-', 70));
        sb.AppendLine(output);

        File.WriteAllText(path, sb.ToString());
        return path;
    }

    /// <summary>
    /// For each unique output folder among the artifacts, look for the named sub-folders
    /// and recursively copy them into the destination. If <paramref name="filenameFilter"/>
    /// is provided, only files whose basename is in the set are copied (used to filter by
    /// git-changed filenames). Returns total file count copied.
    /// </summary>
    public static int CopyExtraFolders(List<BuildArtifact> artifacts,
        IReadOnlyList<string> folderNames, string destFolder,
        HashSet<string>? filenameFilter = null,
        Action<string>? logger = null)
    {
        if (folderNames.Count == 0) return 0;

        var sourceFolders = artifacts
            .Select(a => Path.GetDirectoryName(a.OutputDllPath))
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(p => p!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var processedSrc = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int totalCopied = 0;

        foreach (var srcRoot in sourceFolders)
        {
            foreach (var rawName in folderNames)
            {
                var folderName = rawName.Trim().Trim('/', '\\');
                if (string.IsNullOrEmpty(folderName)) continue;

                var srcPath = Path.Combine(srcRoot, folderName);
                if (!processedSrc.Add(srcPath)) continue;

                if (!Directory.Exists(srcPath))
                {
                    logger?.Invoke($"⚠ Extra folder not found: {srcPath}");
                    continue;
                }

                var destPath = Path.Combine(destFolder, folderName);
                try
                {
                    var (copied, skipped) = CopyDirectoryRecursive(srcPath, destPath, filenameFilter);
                    if (filenameFilter != null)
                        logger?.Invoke($"✔ {folderName}/  ({copied} copied, {skipped} skipped by git filter)");
                    else
                        logger?.Invoke($"✔ {folderName}/  ({copied} file(s))");
                    totalCopied += copied;
                }
                catch (Exception ex)
                {
                    logger?.Invoke($"✘ {folderName}/: {ex.Message}");
                }
            }
        }
        return totalCopied;
    }

    private static (int Copied, int Skipped) CopyDirectoryRecursive(
        string sourceDir, string destDir, HashSet<string>? filenameFilter)
    {
        int copied = 0, skipped = 0;
        bool destCreated = false;
        foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            if (filenameFilter != null && !filenameFilter.Contains(Path.GetFileName(file)))
            {
                skipped++;
                continue;
            }
            if (!destCreated) { Directory.CreateDirectory(destDir); destCreated = true; }

            var rel = Path.GetRelativePath(sourceDir, file);
            var destFile = Path.Combine(destDir, rel);
            var destSubDir = Path.GetDirectoryName(destFile);
            if (!string.IsNullOrEmpty(destSubDir))
                Directory.CreateDirectory(destSubDir);
            if (File.Exists(destFile))
                File.SetAttributes(destFile, FileAttributes.Normal);
            File.Copy(file, destFile, overwrite: true);
            copied++;
        }
        return (copied, skipped);
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = name.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        return new string(chars);
    }

    public static List<string> FindAllProjectsInSolution(string solutionPath)
    {
        var projects = new List<string>();
        if (!File.Exists(solutionPath)) return projects;

        var slnDir = Path.GetDirectoryName(solutionPath) ?? "";
        var regex = new Regex(
            @"^Project\(""\{[^}]+\}""\)\s*=\s*""[^""]+"",\s*""([^""]+\.(csproj|vbproj|fsproj))""",
            RegexOptions.IgnoreCase);

        foreach (var line in File.ReadAllLines(solutionPath))
        {
            var m = regex.Match(line);
            if (!m.Success) continue;
            var rel = m.Groups[1].Value.Replace('\\', Path.DirectorySeparatorChar);
            try
            {
                var full = Path.GetFullPath(Path.Combine(slnDir, rel));
                projects.Add(full);
            }
            catch { /* skip malformed entry */ }
        }
        return projects;
    }

    /// <summary>
    /// Walks up from the changed-file path looking for the nearest *.csproj.
    /// Returns that csproj only if it is part of the candidate set (the solution).
    /// If the deepest containing csproj is OUTSIDE the solution, returns null
    /// (rather than mis-attributing the change to an ancestor csproj).
    /// </summary>
    public static string? FindOwningCsproj(string filePath, IEnumerable<string> candidateProjects)
    {
        string fileDir;
        try { fileDir = Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? ""; }
        catch { return null; }
        if (string.IsNullOrEmpty(fileDir)) return null;

        var candidateSet = new HashSet<string>(
            candidateProjects.Select(p =>
            {
                try { return Path.GetFullPath(p); } catch { return p; }
            }),
            StringComparer.OrdinalIgnoreCase);

        var dir = new DirectoryInfo(fileDir);
        while (dir != null)
        {
            FileInfo[] csprojs;
            try { csprojs = dir.GetFiles("*.csproj"); }
            catch { dir = dir.Parent; continue; }

            foreach (var cs in csprojs)
            {
                if (candidateSet.Contains(cs.FullName))
                    return cs.FullName;
            }
            if (csprojs.Length > 0)
                return null; // file owned by a project not in the solution

            dir = dir.Parent;
        }
        return null;
    }

    public static List<BuildArtifact> ResolveOutputDlls(string csprojPath, string configuration)
    {
        var artifacts = new List<BuildArtifact>();
        if (!File.Exists(csprojPath)) return artifacts;

        string assemblyName;
        string extension = ".dll";
        string? customOutputPath = null;
        var tfms = new List<string>();

        try
        {
            var doc = XDocument.Load(csprojPath);
            var ns = doc.Root?.Name.Namespace ?? XNamespace.None;

            assemblyName = (FirstValueAnyNs(doc, ns, "AssemblyName") ?? "").Trim();
            if (string.IsNullOrEmpty(assemblyName))
                assemblyName = Path.GetFileNameWithoutExtension(csprojPath);

            var outputType = (FirstValueAnyNs(doc, ns, "OutputType") ?? "Library").Trim();
            if (outputType.Equals("Exe", StringComparison.OrdinalIgnoreCase)
                || outputType.Equals("WinExe", StringComparison.OrdinalIgnoreCase))
            {
                extension = ".exe";
            }

            var single = (FirstValueAnyNs(doc, ns, "TargetFramework") ?? "").Trim();
            if (!string.IsNullOrEmpty(single)) tfms.Add(single);

            var multi = (FirstValueAnyNs(doc, ns, "TargetFrameworks") ?? "").Trim();
            if (!string.IsNullOrEmpty(multi))
                tfms.AddRange(multi.Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).Where(s => s.Length > 0));

            // <OutputPath> — legacy non-SDK projects often redirect to a shared bin folder.
            // Walk all PropertyGroups; if Condition is present, accept only when it matches
            // our Configuration. Last applicable wins (MSBuild evaluation order).
            foreach (var pg in doc.Descendants(ns + "PropertyGroup")
                                  .Concat(doc.Descendants("PropertyGroup"))
                                  .Distinct())
            {
                var condition = pg.Attribute("Condition")?.Value ?? "";
                if (!string.IsNullOrEmpty(condition)
                    && !ConditionMatchesConfig(condition, configuration))
                    continue;
                var op = (pg.Element(ns + "OutputPath")?.Value
                          ?? pg.Element("OutputPath")?.Value)?.Trim();
                if (!string.IsNullOrEmpty(op))
                    customOutputPath = op;
            }
        }
        catch
        {
            assemblyName = Path.GetFileNameWithoutExtension(csprojPath);
        }

        if (tfms.Count == 0) tfms.Add(""); // legacy non-SDK project

        var projDir = Path.GetDirectoryName(Path.GetFullPath(csprojPath))!;

        if (!string.IsNullOrEmpty(customOutputPath))
        {
            var resolved = customOutputPath
                .Replace("$(Configuration)", configuration, StringComparison.OrdinalIgnoreCase)
                .Replace("$(Platform)", "AnyCPU", StringComparison.OrdinalIgnoreCase);
            if (!Path.IsPathRooted(resolved))
                resolved = Path.Combine(projDir, resolved);
            try { resolved = Path.GetFullPath(resolved); } catch { /* keep as-is */ }

            artifacts.Add(new BuildArtifact
            {
                ProjectPath = csprojPath,
                AssemblyName = assemblyName,
                TargetFramework = "",
                OutputDllPath = Path.Combine(resolved, assemblyName + extension)
            });
        }
        else
        {
            foreach (var tfm in tfms)
            {
                var binDir = string.IsNullOrEmpty(tfm)
                    ? Path.Combine(projDir, "bin", configuration)
                    : Path.Combine(projDir, "bin", configuration, tfm);
                artifacts.Add(new BuildArtifact
                {
                    ProjectPath = csprojPath,
                    AssemblyName = assemblyName,
                    TargetFramework = tfm,
                    OutputDllPath = Path.Combine(binDir, assemblyName + extension)
                });
            }
        }

        return artifacts;
    }

    private static string? FirstValueAnyNs(XDocument doc, XNamespace ns, string elementName) =>
        doc.Descendants(ns + elementName).FirstOrDefault()?.Value
        ?? doc.Descendants(elementName).FirstOrDefault()?.Value;

    private static bool ConditionMatchesConfig(string condition, string configuration)
    {
        // Match: '$(Configuration)' == 'Release'
        //        '$(Configuration)|$(Platform)' == 'Release|AnyCPU'
        var m = Regex.Match(condition, @"==\s*'([^']*)'");
        if (!m.Success) return false;
        var rhs = m.Groups[1].Value;
        var parts = rhs.Split('|');
        return parts[0].Trim().Equals(configuration, StringComparison.OrdinalIgnoreCase);
    }

    public static int CopyArtifacts(List<BuildArtifact> artifacts, string destFolder,
        IReadOnlyList<string> extensions, Action<string>? logger = null)
    {
        Directory.CreateDirectory(destFolder);
        int copied = 0;
        foreach (var art in artifacts)
        {
            var srcDir = Path.GetDirectoryName(art.OutputDllPath) ?? "";
            var anyFound = false;
            foreach (var ext in extensions)
            {
                var srcFile = Path.Combine(srcDir, art.AssemblyName + ext);
                if (!File.Exists(srcFile)) continue;

                var destFile = Path.Combine(destFolder, art.AssemblyName + ext);
                try
                {
                    if (File.Exists(destFile))
                        File.SetAttributes(destFile, FileAttributes.Normal);
                    File.Copy(srcFile, destFile, overwrite: true);
                    logger?.Invoke($"✔ {art.AssemblyName + ext}");
                    copied++;
                    anyFound = true;
                }
                catch (Exception ex)
                {
                    logger?.Invoke($"✘ {art.AssemblyName + ext}: {ex.Message}");
                }
            }
            if (!anyFound)
                logger?.Invoke($"⚠ No matching files for {art.AssemblyName} in {srcDir}");
        }
        return copied;
    }

    private static Task<(int ExitCode, string FullOutput)> RunProcessAsync(
        string exe, string args, string workingDir, int timeoutMs, Action<string>? logger)
    {
        return Task.Run(() =>
        {
            var psi = new ProcessStartInfo(exe, args)
            {
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi)
                ?? throw new InvalidOperationException($"Cannot start: {exe}");

            var captured = new StringBuilder();
            var sync = new object();
            void Capture(string? line)
            {
                if (line == null) return;
                lock (sync) captured.AppendLine(line);
                logger?.Invoke(line);
            }
            proc.OutputDataReceived += (_, e) => Capture(e.Data);
            proc.ErrorDataReceived  += (_, e) => Capture(e.Data);
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            if (!proc.WaitForExit(timeoutMs))
            {
                try { proc.Kill(entireProcessTree: true); } catch { }
                throw new TimeoutException($"{exe} timed out after {timeoutMs / 1000}s");
            }
            // Ensure async stdout/stderr have fully drained before reading StringBuilder.
            // Without this, ToString() can race with in-flight OutputDataReceived events.
            proc.WaitForExit();

            string finalOutput;
            lock (sync) finalOutput = captured.ToString();
            return (proc.ExitCode, finalOutput);
        });
    }
}
