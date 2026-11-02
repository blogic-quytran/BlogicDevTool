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

    /// <summary>Local + remote-tracking branches of a repo, newest commit first (Name, unix time).</summary>
    public static async Task<List<(string Name, long Time)>> ListBranchesAsync(string repoPath)
    {
        var result = await RunGitAsync(repoPath,
            "for-each-ref --sort=-committerdate --format=%(refname)|%(committerdate:unix) refs/remotes refs/heads",
            30_000);
        if (result.ExitCode != 0)
            throw new InvalidOperationException($"git for-each-ref failed: {result.Output.Trim()}");

        var list = new List<(string, long)>();
        foreach (var line in SplitFileLines(result.Output))
        {
            var parts = line.Split('|');
            if (parts.Length != 2) continue;           // stderr noise
            var full = parts[0].Trim();
            if (full.EndsWith("/HEAD")) continue;      // origin/HEAD symref
            string name;
            if (full.StartsWith("refs/heads/")) name = full.Substring("refs/heads/".Length);
            else if (full.StartsWith("refs/remotes/")) name = full.Substring("refs/remotes/".Length);
            else continue;
            long.TryParse(parts[1].Trim(), out var t);
            list.Add((name, t));
        }
        return list;
    }

    /// <summary>`git fetch --all --prune` — refreshes remote-tracking branches only.</summary>
    public static async Task<(bool Ok, string Message)> FetchAllAsync(string repoPath)
    {
        try
        {
            var r = await RunGitAsync(repoPath, "fetch --all --prune", 120_000);
            return (r.ExitCode == 0, r.Output.Trim());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public static async Task<List<string>> GetChangedFilesAsync(
        string repoPath, string baseRef, string compareRef, bool includeUncommitted = false)
    {
        if (!Directory.Exists(repoPath))
            throw new DirectoryNotFoundException($"Repo path not found: {repoPath}");

        if (string.IsNullOrWhiteSpace(baseRef))
            throw new ArgumentException("Base ref is empty.");
        if (string.IsNullOrWhiteSpace(compareRef))
            compareRef = "HEAD";

        var files = new List<string>();

        if (includeUncommitted)
        {
            // base → working tree: committed + staged + unstaged changes to tracked files.
            // (Two-dot/implicit form; the compare ref is intentionally ignored here.)
            var diff = await RunGitAsync(repoPath,
                $"diff --name-only --diff-filter=ACMR {baseRef}", 60_000);
            if (diff.ExitCode != 0)
                throw new InvalidOperationException(
                    $"git diff failed (exit {diff.ExitCode}):\n{diff.Output}");
            files.AddRange(SplitFileLines(diff.Output));

            // New files that have never been added are invisible to git diff — add them too.
            var untracked = await RunGitAsync(repoPath,
                "ls-files --others --exclude-standard", 60_000);
            if (untracked.ExitCode == 0)
                files.AddRange(SplitFileLines(untracked.Output));
        }
        else
        {
            var result = await RunGitAsync(repoPath,
                $"diff --name-only --diff-filter=ACMR {baseRef}...{compareRef}", 60_000);
            if (result.ExitCode != 0)
                throw new InvalidOperationException(
                    $"git diff failed (exit {result.ExitCode}):\n{result.Output}");
            files.AddRange(SplitFileLines(result.Output));
        }

        return files.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static IEnumerable<string> SplitFileLines(string output) =>
        output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().TrimEnd('\r'))
            .Where(s => !string.IsNullOrWhiteSpace(s));

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
