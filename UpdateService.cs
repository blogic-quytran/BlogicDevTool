using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace BLogicDevTool;

/// <summary>A newer release found on GitHub.</summary>
public sealed class UpdateInfo
{
    public required Version Version { get; init; }
    public required string Tag { get; init; }
    /// <summary>One-line description: the release title minus the "vX.Y.Z —" prefix.</summary>
    public required string Summary { get; init; }
    public required string Notes { get; init; }
    public required string HtmlUrl { get; init; }
    public required string DownloadUrl { get; init; }
    public long Size { get; init; }
    /// <summary>Hex SHA-256 from the GitHub asset "digest" field, when GitHub provides one.</summary>
    public string? Sha256 { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
}

/// <summary>Self-update from GitHub Releases ("vX.Y.Z" tag + BLogicDevTool.exe asset, see release.ps1).
/// Windows allows renaming a running exe, so install = rename to *.old, move new exe in, restart.</summary>
public static class UpdateService
{
    public const string Repo = "blogic-quytran/BlogicDevTool";
    private const string AssetName = "BLogicDevTool.exe";

    private static readonly HttpClient Http = CreateClient();

    private static HttpClient CreateClient()
    {
        var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("BLogicDevTool", CurrentVersion.ToString()));
        return http;
    }

    public static Version CurrentVersion { get; } = Normalize(Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0));

    public static string CurrentVersionText => $"v{CurrentVersion.ToString(3)}";

    /// <summary>Returns the latest release when it is newer than the running build, otherwise null.</summary>
    public static async Task<UpdateInfo?> CheckAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{Repo}/releases/latest");
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        using var resp = await Http.SendAsync(req, ct);
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null; // no releases yet
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        var root = doc.RootElement;
        var tag = root.GetProperty("tag_name").GetString() ?? "";
        if (!Version.TryParse(tag.TrimStart('v', 'V'), out var latest)) return null;
        latest = Normalize(latest);
        if (latest <= CurrentVersion) return null;

        foreach (var asset in root.GetProperty("assets").EnumerateArray())
        {
            if (!string.Equals(asset.GetProperty("name").GetString(), AssetName, StringComparison.OrdinalIgnoreCase))
                continue;

            string? sha = null;
            if (asset.TryGetProperty("digest", out var d) && d.ValueKind == JsonValueKind.String
                && d.GetString() is { } digest && digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
                sha = digest["sha256:".Length..];

            var title = root.TryGetProperty("name", out var nm) ? nm.GetString() ?? "" : "";
            var summary = title.StartsWith(tag, StringComparison.OrdinalIgnoreCase)
                ? title[tag.Length..].TrimStart(' ', '-', '—', '–', ':')
                : title;

            return new UpdateInfo
            {
                Version     = latest,
                Tag         = tag,
                Summary     = summary.Trim(),
                Notes       = root.TryGetProperty("body", out var b) ? b.GetString() ?? "" : "",
                HtmlUrl     = root.GetProperty("html_url").GetString() ?? $"https://github.com/{Repo}/releases",
                DownloadUrl = asset.GetProperty("browser_download_url").GetString()!,
                Size        = asset.GetProperty("size").GetInt64(),
                Sha256      = sha,
                PublishedAt = root.TryGetProperty("published_at", out var p) && p.TryGetDateTimeOffset(out var dt) ? dt : null,
            };
        }
        return null; // release without the exe asset — nothing installable
    }

    /// <summary>Downloads the new exe, swaps it in and starts it. The caller must exit right after.</summary>
    public static async Task InstallAsync(UpdateInfo info, IProgress<int>? progress, CancellationToken ct = default)
    {
        var exePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Cannot determine the running exe path.");
        var dir     = Path.GetDirectoryName(exePath)!;
        var newPath = exePath + ".new";
        var oldPath = exePath + ".old";

        // Download into the same folder so the final move is a same-volume rename.
        using (var resp = await Http.GetAsync(info.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, ct))
        {
            resp.EnsureSuccessStatusCode();
            var total = resp.Content.Headers.ContentLength ?? info.Size;
            await using var src = await resp.Content.ReadAsStreamAsync(ct);
            await using var dst = new FileStream(newPath, FileMode.Create, FileAccess.Write, FileShare.None);
            var buffer = new byte[81920];
            long read = 0;
            int n, lastPct = -1;
            while ((n = await src.ReadAsync(buffer, ct)) > 0)
            {
                await dst.WriteAsync(buffer.AsMemory(0, n), ct);
                read += n;
                if (total > 0)
                {
                    var pct = (int)(read * 100 / total);
                    if (pct != lastPct) progress?.Report(lastPct = pct);
                }
            }
        }

        try
        {
            if (info.Size > 0 && new FileInfo(newPath).Length != info.Size)
                throw new InvalidDataException("Downloaded file size does not match the release asset.");
            if (info.Sha256 != null)
            {
                await using var fs = File.OpenRead(newPath);
                var hash = Convert.ToHexString(await SHA256.HashDataAsync(fs, ct));
                if (!hash.Equals(info.Sha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Downloaded file checksum (SHA-256) does not match the release asset.");
            }
        }
        catch
        {
            TryDelete(newPath);
            throw;
        }

        TryDelete(oldPath);
        File.Move(exePath, oldPath);
        try
        {
            File.Move(newPath, exePath);
        }
        catch
        {
            File.Move(oldPath, exePath); // put the running build back
            throw;
        }

        Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true, WorkingDirectory = dir });
    }

    /// <summary>Removes the *.old exe left behind by the previous update. Call once at startup.</summary>
    public static void CleanupPreviousUpdate()
    {
        if (Environment.ProcessPath is not { } exePath) return;
        // The previous instance may still be shutting down for a moment.
        Task.Run(async () =>
        {
            for (var i = 0; i < 10 && File.Exists(exePath + ".old"); i++)
            {
                TryDelete(exePath + ".old");
                await Task.Delay(1000);
            }
            TryDelete(exePath + ".new");
        });
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { /* locked — retry next time */ }
    }

    private static Version Normalize(Version v) => new(v.Major, Math.Max(v.Minor, 0), Math.Max(v.Build, 0));
}
