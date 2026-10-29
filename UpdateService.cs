using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;

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
    /// <summary>Hex SHA-256 from the release's BLogicDevTool.exe.sha256 asset, when present.</summary>
    public string? Sha256 { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
}

/// <summary>Self-update from GitHub Releases ("vX.Y.Z" tag + BLogicDevTool.exe asset, see release.ps1).
/// Windows allows renaming a running exe, so install = rename to *.old, move new exe in, restart.</summary>
public static class UpdateService
{
    public const string Repo = "blogic-quytran/BlogicDevTool";
    private const string AssetName = "BLogicDevTool.exe";

    // Must stay above Http: static initializers run in source order and CreateClient reads it.
    public static Version CurrentVersion { get; } = Normalize(Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0));

    private static readonly HttpClient Http = CreateClient();

    private static HttpClient CreateClient()
    {
        var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("BLogicDevTool", CurrentVersion.ToString()));
        return http;
    }

    public static string CurrentVersionText => $"v{CurrentVersion.ToString(3)}";

    /// <summary>Returns the latest release when it is newer than the running build, otherwise null.</summary>
    // Reads releases.atom, not api.github.com: the API allows only 60 anonymous calls/hour per IP,
    // and the whole office shares one IP.
    public static async Task<UpdateInfo?> CheckAsync(CancellationToken ct = default)
    {
        var feed = XDocument.Parse(await Http.GetStringAsync($"https://github.com/{Repo}/releases.atom", ct));
        XNamespace a = "http://www.w3.org/2005/Atom";

        // Entry id ends with the tag: "tag:github.com,2008:Repository/123/v1.0.1".
        XElement? entry = null;
        string tag = "";
        Version? version = null;
        foreach (var e in feed.Root!.Elements(a + "entry"))
        {
            var t = ((string?)e.Element(a + "id") ?? "").Split('/')[^1];
            if (!Version.TryParse(t.TrimStart('v', 'V'), out var v)) continue;
            v = Normalize(v);
            if (version == null || v > version) (entry, tag, version) = (e, t, v);
        }
        if (entry == null || version == null || version <= CurrentVersion) return null;

        var downloadUrl = $"https://github.com/{Repo}/releases/download/{tag}/{AssetName}";

        // HEAD follows the redirect to the CDN; 404 = release without the exe, nothing installable.
        long size = 0;
        using (var head = await Http.SendAsync(new HttpRequestMessage(HttpMethod.Head, downloadUrl), ct))
        {
            if (head.StatusCode == HttpStatusCode.NotFound) return null;
            if (head.IsSuccessStatusCode) size = head.Content.Headers.ContentLength ?? 0;
        }

        var title = (string?)entry.Element(a + "title") ?? tag;
        var summary = title.StartsWith(tag, StringComparison.OrdinalIgnoreCase)
            ? title[tag.Length..].TrimStart(' ', '-', '—', '–', ':')
            : title;

        return new UpdateInfo
        {
            Version     = version,
            Tag         = tag,
            Summary     = summary.Trim(),
            Notes       = HtmlToText((string?)entry.Element(a + "content") ?? ""),
            HtmlUrl     = (string?)entry.Element(a + "link")?.Attribute("href") ?? $"https://github.com/{Repo}/releases",
            DownloadUrl = downloadUrl,
            Size        = size,
            Sha256      = await TryGetSha256Async(downloadUrl + ".sha256", ct),
            PublishedAt = DateTimeOffset.TryParse((string?)entry.Element(a + "updated"), out var dt) ? dt : null,
        };
    }

    /// <summary>First 64-hex token of the "*.sha256" asset release.ps1 uploads; null when absent.</summary>
    private static async Task<string?> TryGetSha256Async(string url, CancellationToken ct)
    {
        try
        {
            var m = Regex.Match(await Http.GetStringAsync(url, ct), @"\b[0-9a-fA-F]{64}\b");
            return m.Success ? m.Value : null;
        }
        catch (HttpRequestException) { return null; }
    }

    /// <summary>The feed carries release notes as rendered HTML; turn it back into plain lines.</summary>
    private static string HtmlToText(string html)
    {
        var s = Regex.Replace(html, @"<li[^>]*>", "- ", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"<br\s*/?>|</(p|li|h\d|div|pre)>", "\n", RegexOptions.IgnoreCase);
        s = WebUtility.HtmlDecode(Regex.Replace(s, "<[^>]+>", ""));
        return Regex.Replace(s, @"\n{3,}", "\n\n").Trim();
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
