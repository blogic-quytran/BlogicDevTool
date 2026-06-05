namespace BLogicDevTool;

/// <summary>
/// One backup per deployment task, named after the ZIP (e.g. "PSS255"), kept under
/// the tool folder: &lt;toolDir&gt;\backups\&lt;taskName&gt;\. The backup is WRITE-ONCE and
/// accumulates: deploying the same task again only adds the original of files it hasn't
/// captured yet, so the folder always holds the pre-first-deploy state of everything the
/// task ever touched. Used as the single rollback point for that task.
///
/// Layout inside the task folder:
///   &lt;DRIVE&gt;\&lt;path…&gt;  — original of each replaced file (mirrors its absolute path)
///   _added.txt        — absolute paths of files the task newly created (deleted on rollback)
/// </summary>
public static class UnzipBackupStore
{
    private static readonly string BackupsRoot =
        Path.Combine(AppContext.BaseDirectory, "backups");

    public static string TaskName(string zipPath) =>
        Sanitize(Path.GetFileNameWithoutExtension(zipPath));

    public static string TaskFolder(string taskName) =>
        Path.Combine(BackupsRoot, taskName);

    public static bool HasBackup(string taskName) =>
        Directory.Exists(TaskFolder(taskName));

    private static string AddedManifest(string taskName) =>
        Path.Combine(TaskFolder(taskName), "_added.txt");

    // ── encode/decode absolute path ↔ backup slot ───────────────────────────────

    private static string EncodePath(string taskName, string destPath)
    {
        var full = Path.GetFullPath(destPath);
        var root = Path.GetPathRoot(full) ?? "";
        if (root.Length >= 2 && root[1] == ':')
            return Path.Combine(TaskFolder(taskName), root.Substring(0, 1), full.Substring(root.Length));
        return Path.Combine(TaskFolder(taskName), "_other_", full.Replace(":", "").TrimStart('\\', '/'));
    }

    /// <summary>Original absolute path for a backup file, or null if not decodable.</summary>
    public static string? DecodeOriginalPath(string taskName, string backupFile)
    {
        var rel = Path.GetRelativePath(TaskFolder(taskName), backupFile);
        var parts = rel.Split(
            new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, 2);
        if (parts.Length < 2 || parts[0].StartsWith("_")) return null;
        return parts[0] + ":" + Path.DirectorySeparatorChar + parts[1];
    }

    // ── replaced-file backups (write-once) ──────────────────────────────────────

    /// <summary>Copy the original into this task's backup only if not already captured.
    /// Returns true if a new copy was made.</summary>
    public static bool SaveOriginalIfAbsent(string taskName, string destPath)
    {
        var bp = EncodePath(taskName, destPath);
        if (File.Exists(bp)) return false;
        var dir = Path.GetDirectoryName(bp);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.Copy(destPath, bp, overwrite: false);
        return true;
    }

    public static IEnumerable<string> EnumerateOriginals(string taskName)
    {
        var root = TaskFolder(taskName);
        if (!Directory.Exists(root)) yield break;
        foreach (var f in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(root, f);
            if (rel.StartsWith("_")) continue; // skip manifests like _added.txt
            yield return f;
        }
    }

    // ── added-files manifest ────────────────────────────────────────────────────

    public static HashSet<string> LoadAdded(string taskName)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var path = AddedManifest(taskName);
        try
        {
            if (File.Exists(path))
                foreach (var line in File.ReadAllLines(path))
                    if (!string.IsNullOrWhiteSpace(line)) set.Add(line.Trim());
        }
        catch { /* ignore */ }
        return set;
    }

    public static void SaveAdded(string taskName, IEnumerable<string> paths)
    {
        Directory.CreateDirectory(TaskFolder(taskName));
        File.WriteAllLines(AddedManifest(taskName), paths);
    }

    private static string Sanitize(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "task" : name;
    }
}
