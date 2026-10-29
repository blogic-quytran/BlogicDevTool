using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BLogicDevTool;

/// <summary>One item returned by a PROPFIND listing.</summary>
public sealed class WebDavEntry
{
    public string Name { get; init; } = "";
    /// <summary>Absolute URL of the item, ready for GET.</summary>
    public string Url { get; init; } = "";
    public bool IsFolder { get; init; }
    public long Length { get; init; }
    public DateTime? Modified { get; init; }

    public string SizeText => IsFolder ? "" : FormatSize(Length);

    public static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => bytes + " B",
        < 1024L * 1024 => (bytes / 1024.0).ToString("0.#") + " KB",
        < 1024L * 1024 * 1024 => (bytes / (1024.0 * 1024)).ToString("0.#") + " MB",
        _ => (bytes / (1024.0 * 1024 * 1024)).ToString("0.##") + " GB"
    };
}

/// <summary>WebDAV PROPFIND + GET over HttpClient with Basic auth. Bypasses the Windows
/// WebClient redirector, which blocks Basic over plain HTTP (BasicAuthLevel=1) and caps
/// mapped-drive files at 50 MB.</summary>
public sealed class WebDavClient : IDisposable
{
    private static readonly XNamespace D = "DAV:";

    private const string PropfindBody =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
        "<D:propfind xmlns:D=\"DAV:\"><D:prop>" +
        "<D:resourcetype/><D:getcontentlength/><D:getlastmodified/><D:displayname/>" +
        "</D:prop></D:propfind>";

    private readonly HttpClient _http;

    /// <summary>Cap for PROPFIND/HEAD only: a server that accepts the socket and then goes
    /// quiet would otherwise wedge the browser form with no way out.</summary>
    private static readonly TimeSpan MetadataTimeout = TimeSpan.FromSeconds(30);

    public Uri BaseUri { get; }

    public WebDavClient(string baseUrl, string username, string password)
    {
        if (!baseUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            baseUrl = "http://" + baseUrl;
        BaseUri = new Uri(baseUrl.TrimEnd('/') + "/");

        _http = new HttpClient(new HttpClientHandler { PreAuthenticate = true })
        {
            // Large ZIPs (300 MB+) are normal here, so no overall send timeout — the
            // per-operation CancellationToken is what stops a download.
            Timeout = System.Threading.Timeout.InfiniteTimeSpan
        };
        if (!string.IsNullOrEmpty(username))
        {
            var raw = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", raw);
        }
    }

    /// <summary>Lists one level of <paramref name="url"/> (absolute, or empty for the root).</summary>
    public async Task<List<WebDavEntry>> ListAsync(string? url, CancellationToken ct = default)
    {
        var target = string.IsNullOrWhiteSpace(url) ? BaseUri : new Uri(url);
        if (!target.AbsoluteUri.EndsWith("/")) target = new Uri(target.AbsoluteUri + "/");

        using var req = new HttpRequestMessage(new HttpMethod("PROPFIND"), target);
        req.Headers.Add("Depth", "1");
        req.Content = new StringContent(PropfindBody, Encoding.UTF8, "text/xml");

        using var resp = await SendMetadataAsync(req, ct).ConfigureAwait(false);
        if (resp.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Wrong username or password (HTTP 401).");
        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"PROPFIND failed: HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}");

        var xml = XDocument.Parse(await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false));
        var selfPath = target.AbsolutePath.TrimEnd('/');
        var list = new List<WebDavEntry>();

        foreach (var r in xml.Descendants(D + "response"))
        {
            var href = r.Element(D + "href")?.Value;
            if (string.IsNullOrEmpty(href)) continue;

            var itemUri = HrefToUri(href);
            // Depth:1 includes the collection itself — drop it.
            if (string.Equals(itemUri.AbsolutePath.TrimEnd('/'), selfPath,
                    StringComparison.OrdinalIgnoreCase))
                continue;

            var prop = r.Descendants(D + "prop").FirstOrDefault();
            var isFolder = prop?.Element(D + "resourcetype")?.Element(D + "collection") != null;
            long.TryParse(prop?.Element(D + "getcontentlength")?.Value, out var len);
            DateTime? modified = DateTime.TryParse(prop?.Element(D + "getlastmodified")?.Value,
                out var dt) ? dt.ToLocalTime() : null;

            var name = prop?.Element(D + "displayname")?.Value;
            if (string.IsNullOrWhiteSpace(name))
                name = Uri.UnescapeDataString(itemUri.Segments[^1].TrimEnd('/'));

            list.Add(new WebDavEntry
            {
                Name = name,
                IsFolder = isFolder,
                Length = len,
                Modified = modified,
                Url = isFolder ? itemUri.AbsoluteUri : itemUri.AbsoluteUri.TrimEnd('/')
            });
        }

        return list
            .OrderByDescending(e => e.IsFolder)
            .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>Content-Length of a remote file, or -1 when the server does not report it.</summary>
    public async Task<long> GetLengthAsync(string url, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Head, url);
        using var resp = await SendMetadataAsync(req, ct, HttpCompletionOption.ResponseHeadersRead)
            .ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return resp.Content.Headers.ContentLength ?? -1;
    }

    /// <summary>Sends a metadata request under <see cref="MetadataTimeout"/> and reports a
    /// timeout as such, since the client itself has no overall timeout.</summary>
    private async Task<HttpResponseMessage> SendMetadataAsync(
        HttpRequestMessage req, CancellationToken ct,
        HttpCompletionOption completion = HttpCompletionOption.ResponseContentRead)
    {
        using var link = CancellationTokenSource.CreateLinkedTokenSource(ct);
        link.CancelAfter(MetadataTimeout);
        try
        {
            return await _http.SendAsync(req, completion, link.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"No answer from the server within {MetadataTimeout.TotalSeconds:0}s " +
                $"({req.Method} {req.RequestUri}).");
        }
    }

    /// <summary>Downloads to <paramref name="destPath"/>, reporting (read, total); total is
    /// -1 when the server sends no length. Writes a .part file first, so a cancelled
    /// download never leaves a truncated ZIP behind.</summary>
    public async Task DownloadAsync(string url, string destPath,
        IProgress<(long Read, long Total)>? progress = null, CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
        var partPath = destPath + ".part";

        using var resp = await _http
            .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        if (resp.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Wrong username or password (HTTP 401).");
        resp.EnsureSuccessStatusCode();

        var total = resp.Content.Headers.ContentLength ?? -1;
        try
        {
            await using (var src = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false))
            await using (var dst = new FileStream(partPath, FileMode.Create, FileAccess.Write,
                             FileShare.None, 1 << 20, useAsync: true))
            {
                var buffer = new byte[1 << 20];
                long read = 0, lastReport = 0;
                int n;
                while ((n = await src.ReadAsync(buffer, ct).ConfigureAwait(false)) > 0)
                {
                    await dst.WriteAsync(buffer.AsMemory(0, n), ct).ConfigureAwait(false);
                    read += n;
                    // Report at most every 1 MB — the UI thread does not need hundreds of
                    // marshalled calls per second on a fast LAN.
                    if (read - lastReport >= 1 << 20)
                    {
                        lastReport = read;
                        progress?.Report((read, total));
                    }
                }
                progress?.Report((read, total));
            }
        }
        catch
        {
            // Only a failed/cancelled transfer makes the part file worthless.
            try { if (File.Exists(partPath)) File.Delete(partPath); } catch { /* best effort */ }
            throw;
        }

        try
        {
            if (File.Exists(destPath)) File.Delete(destPath);
            File.Move(partPath, destPath);
        }
        catch (Exception ex)
        {
            // Destination locked (the ZIP still open elsewhere) — keep the complete .part
            // file rather than making the operator download hundreds of MB again.
            throw new IOException(
                $"Downloaded, but '{destPath}' could not be replaced ({ex.Message}). " +
                $"The complete file is kept at '{partPath}'.", ex);
        }
    }

    /// <summary>True for a share link like "soteria://files/POS/Quy/x.zip" (path relative to the DAV root).</summary>
    public static bool IsShareLink(string text)
        => Regex.IsMatch(text.Trim(), @"^(?!https?://)[a-z][a-z0-9+.-]*://files(/|$)", RegexOptions.IgnoreCase);

    /// <summary>Absolute http(s) URL for a share link, an http(s) URL, "/dav/POS/..." or a path
    /// relative to the DAV root ("POS/Quy"). Null when the text can't be turned into one.</summary>
    public static string? ResolveLink(string baseUrl, string text)
    {
        text = text.Trim().Replace('\\', '/');
        if (text.Length == 0) return null;
        if (!baseUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            baseUrl = "http://" + baseUrl;
        if (!Uri.TryCreate(baseUrl.TrimEnd('/') + "/", UriKind.Absolute, out var baseUri)) return null;

        if (Uri.TryCreate(text, UriKind.Absolute, out var abs)
            && (abs.Scheme == Uri.UriSchemeHttp || abs.Scheme == Uri.UriSchemeHttps))
            return abs.AbsoluteUri;

        string rel;
        var share = Regex.Match(text, @"^[a-z][a-z0-9+.-]*://files/?(.*)$", RegexOptions.IgnoreCase);
        if (share.Success)
            rel = share.Groups[1].Value;
        else if (text.Contains("://"))
            return null;
        else
        {
            // "/dav/POS" already carries the mount prefix; "/POS" or "POS" are relative to it.
            var prefix = baseUri.AbsolutePath;                                   // "/dav/"
            rel = text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? text[prefix.Length..] : text;
        }

        // Unescape first so an already-escaped link isn't double-escaped.
        var segments = rel.Split('?', '#')[0].Split('/', StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => Uri.EscapeDataString(Uri.UnescapeDataString(s)));
        var path = string.Join("/", segments);
        if (path.Length > 0 && rel.EndsWith("/")) path += "/";
        return new Uri(baseUri, path).AbsoluteUri;
    }

    /// <summary>Absolute URL from a PROPFIND href. SFTPGo reports hrefs relative to the DAV
    /// mount root ("/POS/" for a base of ".../dav"); other servers include the prefix.</summary>
    private Uri HrefToUri(string href)
    {
        if (Uri.TryCreate(href, UriKind.Absolute, out var abs)) return abs;

        var prefix = BaseUri.AbsolutePath.TrimEnd('/');            // "/dav"
        if (prefix.Length > 0 &&
            !href.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase) &&
            !href.Equals(prefix, StringComparison.OrdinalIgnoreCase))
            href = prefix + "/" + href.TrimStart('/');

        return new Uri($"{BaseUri.Scheme}://{BaseUri.Authority}{href}");
    }

    public void Dispose() => _http.Dispose();
}
