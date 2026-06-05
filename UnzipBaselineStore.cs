using System.Text.Json;

namespace BLogicDevTool;

/// <summary>
/// Write-once baseline that lets a deploy be rolled back to the ORIGINAL state,
/// no matter how many times the user has deployed. Two pieces:
///   • files\&lt;DRIVE&gt;\&lt;path…&gt; — the FIRST-seen copy of each file a deploy replaced
///     (never overwritten, so it always holds the true original).
///   • created.json — absolute paths of files that did NOT exist before the first deploy
///     (so Restore Original can delete them).
/// Location: %APPDATA%\BLogicDevTool\deploy_baseline\
/// </summary>
public static class UnzipBaselineStore
{
    private static readonly string Root = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BLogicDevTool", "deploy_baseline");

    private static string FilesRoot => Path.Combine(Root, "files");
    private static string CreatedManifestPath => Path.Combine(Root, "created.json");

    public static string BaselineLocation => Root;

    private static readonly JsonSerializerOptions _jsonOpts = new() { WriteIndented = true };

    // ── Baseline of replaced files (write-once) ─────────────────────────────────

    /// <summary>Maps an absolute destination path to its slot under the baseline tree.</summary>
    public static string EncodeBaselinePath(string destPath)
    {
        var full = Path.GetFullPath(destPath);
        var root = Path.GetPathRoot(full) ?? "";
        // Drive-letter paths: "C:\foo\bar" → files\C\foo\bar
        if (root.Length >= 2 && root[1] == ':')
        {
            var drive = root.Substring(0, 1);
            var rest = full.Substring(root.Length);
            return Path.Combine(FilesRoot, drive, rest);
        }
        // Fallback for UNC/other roots — not reversible, kept under _other_.
        var sanitized = full.Replace(":", "").TrimStart('\\', '/');
        return Path.Combine(FilesRoot, "_other_", sanitized);
    }

    /// <summary>Reconstructs the original absolute path from a baseline file, or null.</summary>
    public static string? DecodeOriginalPath(string baselineFile)
    {
        var rel = Path.GetRelativePath(FilesRoot, baselineFile);
        var parts = rel.Split(
            new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, 2);
        if (parts.Length < 2) return null;
        if (parts[0] == "_other_") return null; // not reversible
        return parts[0] + ":" + Path.DirectorySeparatorChar + parts[1];
    }

    /// <summary>Copy the current file into the baseline only if not already captured.</summary>
    public static void SaveOriginalIfAbsent(string destPath)
    {
        var bp = EncodeBaselinePath(destPath);
        if (File.Exists(bp)) return; // write-once: keep the first (true original)
        var dir = Path.GetDirectoryName(bp);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.Copy(destPath, bp, overwrite: false);
    }

    public static IEnumerable<string> EnumerateBaselineFiles() =>
        Directory.Exists(FilesRoot)
            ? Directory.EnumerateFiles(FilesRoot, "*", SearchOption.AllDirectories)
            : Enumerable.Empty<string>();

    public static bool HasAnyBaseline() =>
        (Directory.Exists(FilesRoot)
            && Directory.EnumerateFiles(FilesRoot, "*", SearchOption.AllDirectories).Any())
        || LoadCreated().Count > 0;

    // ── Manifest of files created by deploys ────────────────────────────────────

    public static HashSet<string> LoadCreated()
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            if (File.Exists(CreatedManifestPath))
            {
                var list = JsonSerializer.Deserialize<List<string>>(
                    File.ReadAllText(CreatedManifestPath));
                if (list != null) foreach (var p in list) set.Add(p);
            }
        }
        catch { /* ignore malformed manifest */ }
        return set;
    }

    public static void SaveCreated(HashSet<string> created)
    {
        Directory.CreateDirectory(Root);
        File.WriteAllText(CreatedManifestPath,
            JsonSerializer.Serialize(created.ToList(), _jsonOpts));
    }
}
