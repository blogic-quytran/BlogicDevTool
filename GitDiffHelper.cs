using System.Diagnostics;

namespace BLogicDevTool;

public static class GitDiffHelper
{
    /// <summary>
    /// Walks up from the given path looking for a directory containing a .git folder.
    /// Returns the repo root (folder that contains .git) or null if none found.
    /// </summary>
    public static string? DetectRepoFromPath(string startPath)
    {
        if (string.IsNullOrWhiteSpace(startPath)) return null;
        string fullStart;
        try { fullStart = Path.GetFullPath(startPath); }
        catch { return null; }
        var dir = File.Exists(fullStart) ? new DirectoryInfo(Path.GetDirectoryName(fullStart) ?? "")
                                         : new DirectoryInfo(fullStart);
        while (dir != null)
        {
            try
            {
                if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                    return dir.FullName;
            }
            catch { /* perm error, keep walking */ }
            dir = dir.Parent;
        }
        return null;
    }

    public static async Task<bool> IsGitInstalledAsync()
    {
        try
        {
            var result = await RunGitAsync(Environment.CurrentDirectory, "--version", 5_000);
            return result.ExitCode == 0;
        }
        catch { return false; }
    }

    /// <summary>
    /// If baseRef looks like a remote-tracking ref ("remote/branch"), runs
    /// `git fetch remote branch` to refresh it. Never modifies the working tree
    /// or local branches. Returns (true, message) on success and (false, reason)
    /// when fetch is skipped or fails — callers should log but not abort, since
    /// the cached ref may still be usable.
    /// </summary>
    public static async Task<(bool Ok, string Message)> TryFetchRefAsync(
        string repoPath, string baseRef)
    {
        if (string.IsNullOrWhiteSpace(baseRef))
            return (false, "Base ref is empty.");

        int slash = baseRef.IndexOf('/');
        if (slash <= 0 || slash >= baseRef.Length - 1)
            return (false, $"'{baseRef}' is not a remote-tracking ref — skipping fetch.");

        var remote = baseRef.Substring(0, slash);
        var branch = baseRef.Substring(slash + 1);

        try
        {
            var result = await RunGitAsync(repoPath, $"fetch {remote} {branch}", 60_000);
            if (result.ExitCode != 0)
                return (false, $"git fetch failed (exit {result.ExitCode}): {result.Output.Trim()}");
            return (true, $"Fetched {remote}/{branch}.");
        }
        catch (Exception ex)
        {
            return (false, $"git fetch error: {ex.Message}");
        }
    }

    public static async Task<List<string>> GetChangedFilesAsync(
        string repoPath, string baseRef, string compareRef)
    {
        if (!Directory.Exists(repoPath))
            throw new DirectoryNotFoundException($"Repo path not found: {repoPath}");

        if (string.IsNullOrWhiteSpace(baseRef))
            throw new ArgumentException("Base ref is empty.");
        if (string.IsNullOrWhiteSpace(compareRef))
            compareRef = "HEAD";

        var args = $"diff --name-only --diff-filter=ACMR {baseRef}...{compareRef}";
        var result = await RunGitAsync(repoPath, args, 60_000);
        if (result.ExitCode != 0)
            throw new InvalidOperationException(
                $"git diff failed (exit {result.ExitCode}):\n{result.Output}");

        return result.Output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().TrimEnd('\r'))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }

    private static Task<(int ExitCode, string Output)> RunGitAsync(
        string workingDir, string args, int timeoutMs)
    {
        return Task.Run(() =>
        {
            var psi = new ProcessStartInfo("git", args)
            {
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi)
                ?? throw new InvalidOperationException(
                    "Cannot start git.exe — is Git installed and on PATH?");
            var stdout = proc.StandardOutput.ReadToEnd();
            var stderr = proc.StandardError.ReadToEnd();
            if (!proc.WaitForExit(timeoutMs))
            {
                try { proc.Kill(); } catch { }
                throw new TimeoutException($"git timed out after {timeoutMs / 1000}s");
            }
            var combined = stdout + (string.IsNullOrWhiteSpace(stderr) ? "" : "\n" + stderr);
            return (proc.ExitCode, combined);
        });
    }
}
