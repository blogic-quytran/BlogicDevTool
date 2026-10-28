using System.Security.Cryptography;
using System.Text;

namespace BLogicDevTool;

public sealed class WebDavSettings
{
    public string BaseUrl { get; set; } = "http://192.168.1.194:8000/dav";
    public string Username { get; set; } = "";
    /// <summary>DPAPI-protected password, base64. Empty when "remember" is off.</summary>
    public string ProtectedPassword { get; set; } = "";
    public bool RememberPassword { get; set; }
    /// <summary>Folder the browser reopens at, so the same share is one click away.</summary>
    public string LastFolderUrl { get; set; } = "";
    public bool ZipOnly { get; set; } = true;
}

/// <summary>WebDAV browser settings, stored in the "WebDav" section of <see cref="AppConfigStore"/>.</summary>
public static class WebDavSettingsStore
{
    // Ties the ciphertext to this tool, so another app's DPAPI blob cannot be decrypted here.
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("BLogicDevTool.WebDav.v1");

    public static WebDavSettings Load() =>
        AppConfigStore.Get(c => c.WebDav, (c, v) => c.WebDav = v, () => new WebDavSettings());

    public static void Save(WebDavSettings settings) =>
        AppConfigStore.Update(c => c.WebDav = settings);

    /// <summary>Encrypts for the current Windows user; the blob is useless on another account.</summary>
    public static string Protect(string plain)
    {
        if (string.IsNullOrEmpty(plain)) return "";
        try
        {
            return Convert.ToBase64String(ProtectedData.Protect(
                Encoding.UTF8.GetBytes(plain), Entropy, DataProtectionScope.CurrentUser));
        }
        catch { return ""; }
    }

    public static string Unprotect(string cipher)
    {
        if (string.IsNullOrEmpty(cipher)) return "";
        try
        {
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(
                Convert.FromBase64String(cipher), Entropy, DataProtectionScope.CurrentUser));
        }
        catch { return ""; }
    }
}
