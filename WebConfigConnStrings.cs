using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BLogicDevTool;

/// <summary>Where a connection string lives inside a Web.config.</summary>
public enum ConnStringKind
{
    /// <summary>NHibernate: &lt;property name="connection.connection_string"&gt;...&lt;/property&gt;</summary>
    NHibernate,

    /// <summary>Standard: &lt;connectionStrings&gt;&lt;add name=".." connectionString=".."/&gt;</summary>
    ConnectionStrings
}

/// <summary>One connection string found in a Web.config, with its server/database parsed out.</summary>
public sealed class ConnStringEntry
{
    public ConnStringKind Kind { get; set; }

    /// <summary>"connection.connection_string" for NHibernate, otherwise the add/@name.</summary>
    public string Name { get; set; } = "";

    /// <summary>The connection string value exactly as the XML parser decoded it.</summary>
    public string Raw { get; set; } = "";

    public string Database { get; set; } = "";
    public string DataSource { get; set; } = "";

    /// <summary>True when the value is an EntityFramework "metadata=...;provider connection string=..." wrapper.</summary>
    public bool IsEntityFramework { get; set; }

    /// <summary>Short label for grids and combo boxes.</summary>
    public string Label =>
        Kind == ConnStringKind.NHibernate ? "NHibernate" : (Name.Length > 0 ? Name : "(unnamed)");

    public override string ToString() =>
        $"{Label}  —  {(DataSource.Length > 0 ? DataSource : "?")} / {(Database.Length > 0 ? Database : "?")}";
}

/// <summary>Outcome of a single connection-string patch, including post-write verification.</summary>
public sealed class ConnStringPatchResult
{
    public bool Changed { get; set; }
    public bool Verified { get; set; }
    public string? Error { get; set; }
    public string? BackupPath { get; set; }

    public string OldDataSource { get; set; } = "";
    public string NewDataSource { get; set; } = "";
    public string OldDatabase { get; set; } = "";
    public string NewDatabase { get; set; } = "";

    public bool Success => Changed && Verified && Error == null;
}

/// <summary>Reads and rewrites the connection strings inside an IIS application's Web.config.</summary>
/// <remarks>Reading is namespace-aware (the NHibernate section declares its own xmlns); writing is
/// anchored to one entry in the raw text so formatting and other connection strings survive.</remarks>
public static class WebConfigConnStrings
{
    // Alias sets, in the order SqlClient itself accepts them.
    private static readonly string[] DbKeys = { "Initial Catalog", "Database" };
    private static readonly string[] ServerKeys = { "Data Source", "Server", "Network Address", "Addr", "Address" };

    private const string NHibernatePropertyName = "connection.connection_string";

    // ── Locate ────────────────────────────────────────────────────────────────

    /// <summary>Returns the Web.config path for an IIS physical path, or null if there is none.</summary>
    public static string? ResolveConfigPath(string physicalPath)
    {
        if (string.IsNullOrWhiteSpace(physicalPath)) return null;
        try
        {
            var expanded = Environment.ExpandEnvironmentVariables(physicalPath);
            if (!Directory.Exists(expanded)) return null;

            // Windows is case-insensitive, but be explicit so the intent survives a port.
            foreach (var name in new[] { "Web.config", "web.config" })
            {
                var candidate = Path.Combine(expanded, name);
                if (File.Exists(candidate)) return candidate;
            }
        }
        catch { /* unreadable path — treat as "no config" */ }
        return null;
    }

    // ── Read ──────────────────────────────────────────────────────────────────

    /// <summary>Enumerates every connection string in the config. Never throws.</summary>
    public static List<ConnStringEntry> Read(string? configPath)
    {
        var list = new List<ConnStringEntry>();
        if (configPath == null || !File.Exists(configPath)) return list;

        try
        {
            var doc = XDocument.Load(configPath);

            // NHibernate — match on local name so the urn:nhibernate-configuration-2.2
            // default namespace does not hide the element.
            foreach (var el in doc.Descendants()
                         .Where(e => e.Name.LocalName == "property" &&
                                     (string?)e.Attribute("name") == NHibernatePropertyName))
            {
                list.Add(Describe(ConnStringKind.NHibernate, NHibernatePropertyName, el.Value));
            }

            // <connectionStrings><add .../></connectionStrings>
            foreach (var el in doc.Descendants()
                         .Where(e => e.Name.LocalName == "add" && e.Attribute("connectionString") != null))
            {
                var name = (string?)el.Attribute("name") ?? "";
                var val = (string?)el.Attribute("connectionString") ?? "";
                list.Add(Describe(ConnStringKind.ConnectionStrings, name, val));
            }
        }
        catch { /* malformed config — report nothing rather than guess */ }

        return list;
    }

    /// <summary>The entry a single-app DB switch targets: NHibernate first, else the first one naming a database.</summary>
    public static ConnStringEntry? PickPrimary(IEnumerable<ConnStringEntry> entries)
    {
        var all = entries.ToList();
        return all.FirstOrDefault(e => e.Kind == ConnStringKind.NHibernate && e.Database.Length > 0)
            ?? all.FirstOrDefault(e => e.Kind == ConnStringKind.NHibernate)
            ?? all.FirstOrDefault(e => e.Database.Length > 0)
            ?? all.FirstOrDefault();
    }

    private static ConnStringEntry Describe(ConnStringKind kind, string name, string raw)
    {
        var value = raw ?? "";
        return new ConnStringEntry
        {
            Kind = kind,
            Name = name,
            Raw = value.Trim(),
            Database = ReadKey(value, DbKeys),
            DataSource = ReadKey(value, ServerKeys),
            IsEntityFramework = value.IndexOf("provider connection string", StringComparison.OrdinalIgnoreCase) >= 0
        };
    }

    // ── Key-level parsing ─────────────────────────────────────────────────────

    /// <summary>Key matcher whose value stops at ; " or &lt;, so it cannot run past the attribute
    /// or swallow the closing XML tag.</summary>
    private static Regex KeyRegex(string[] aliases)
    {
        var alt = string.Join("|", aliases.Select(a =>
            string.Join(@"\s+", a.Split(' ').Select(Regex.Escape))));
        return new Regex(@"(\b(?:" + alt + @")\s*=\s*)([^;""<]*)", RegexOptions.IgnoreCase);
    }

    private static string ReadKey(string connStr, string[] aliases)
    {
        if (string.IsNullOrWhiteSpace(connStr)) return "";
        var m = KeyRegex(aliases).Match(connStr);
        return m.Success ? m.Groups[2].Value.Trim() : "";
    }

    /// <summary>Replaces the first matching key's value, keeping the spelling the file already uses.
    /// Returns the input unchanged when the key is absent and appendIfMissing is false.</summary>
    private static string WriteKey(string connStr, string[] aliases, string newValue, bool appendIfMissing)
    {
        var m = KeyRegex(aliases).Match(connStr);
        if (m.Success)
        {
            return connStr.Substring(0, m.Index)
                 + m.Groups[1].Value + newValue
                 + connStr.Substring(m.Index + m.Length);
        }

        if (!appendIfMissing) return connStr;

        // Preserve the surrounding whitespace of an element value spread over several lines.
        var trailingWs = connStr.Length - connStr.TrimEnd().Length;
        var core = connStr.TrimEnd();
        var tail = connStr.Substring(connStr.Length - trailingWs);
        var sep = core.EndsWith(";", StringComparison.Ordinal) ? "" : ";";
        return core + sep + aliases[0] + "=" + newValue + tail;
    }

    // ── Patch ─────────────────────────────────────────────────────────────────

    /// <summary>Rewrites one connection string's server and/or database, then re-reads the file to
    /// confirm the change landed. Pass null to leave a field alone.</summary>
    public static ConnStringPatchResult Patch(
        string configPath,
        ConnStringKind kind,
        string entryName,
        string? newDataSource,
        string? newDatabase,
        bool makeBackup)
    {
        var result = new ConnStringPatchResult
        {
            NewDataSource = newDataSource ?? "",
            NewDatabase = newDatabase ?? ""
        };

        if (!File.Exists(configPath))
        {
            result.Error = $"Web.config not found: {configPath}";
            return result;
        }

        var before = Read(configPath).FirstOrDefault(e => e.Kind == kind && e.Name == entryName);
        if (before == null)
        {
            result.Error = $"Connection string '{entryName}' no longer exists in {Path.GetFileName(configPath)}.";
            return result;
        }
        result.OldDataSource = before.DataSource;
        result.OldDatabase = before.Database;

        string text;
        Encoding encoding;
        byte[] bom;
        try
        {
            (text, encoding, bom) = ReadWithEncoding(configPath);
        }
        catch (Exception ex)
        {
            result.Error = "Cannot read config: " + ex.Message;
            return result;
        }

        // An EntityFramework value keeps the real settings inside a quoted inner string.
        // Rewriting an existing key there is safe; inventing a missing one is not.
        var appendIfMissing = !before.IsEntityFramework;

        string Transform(string value)
        {
            var updated = value;
            if (newDataSource != null)
                updated = WriteKey(updated, ServerKeys, newDataSource, appendIfMissing);
            if (newDatabase != null)
                updated = WriteKey(updated, DbKeys, newDatabase, appendIfMissing);
            return updated;
        }

        string patched;
        try
        {
            patched = kind == ConnStringKind.NHibernate
                ? PatchNHibernateProperty(text, entryName, Transform)
                : PatchConnectionStringsAdd(text, entryName, Transform);
        }
        catch (Exception ex)
        {
            result.Error = ex.Message;
            return result;
        }

        if (patched == text)
        {
            result.Error = before.IsEntityFramework && newDatabase != null && before.Database.Length == 0
                ? "The EntityFramework connection string has no Initial Catalog to replace — edit it by hand."
                : "Nothing changed — the connection string already holds these values, or the entry could not be located in the file text.";
            return result;
        }

        try
        {
            if (makeBackup) result.BackupPath = CreateBackup(configPath);
            WriteWithEncoding(configPath, patched, encoding, bom);
            result.Changed = true;
        }
        catch (Exception ex)
        {
            result.Error = "Cannot write config: " + ex.Message;
            return result;
        }

        // Verify from disk. A write that "succeeded" but did not take effect is the
        // failure mode this whole class exists to rule out.
        var after = Read(configPath).FirstOrDefault(e => e.Kind == kind && e.Name == entryName);
        if (after == null)
        {
            result.Error = "Config was written but can no longer be parsed — restore the backup.";
            return result;
        }

        var dbOk = newDatabase == null ||
                   string.Equals(after.Database, newDatabase, StringComparison.OrdinalIgnoreCase);
        var srvOk = newDataSource == null ||
                    string.Equals(after.DataSource, newDataSource, StringComparison.OrdinalIgnoreCase);

        result.Verified = dbOk && srvOk;
        if (!result.Verified)
            result.Error = $"Verification failed — file now reads {after.DataSource} / {after.Database}.";

        return result;
    }

    private static string PatchNHibernateProperty(string text, string propertyName, Func<string, string> transform)
    {
        var rx = new Regex(
            @"(<property\s[^>]*\bname\s*=\s*[""']" + Regex.Escape(propertyName) + @"[""'][^>]*>)(.*?)(</property\s*>)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        var matched = false;
        var output = rx.Replace(text, m =>
        {
            matched = true;
            return m.Groups[1].Value + transform(m.Groups[2].Value) + m.Groups[3].Value;
        }, 1);

        if (!matched)
            throw new InvalidOperationException(
                $"Could not locate <property name=\"{propertyName}\"> in the file text.");

        return output;
    }

    private static string PatchConnectionStringsAdd(string text, string entryName, Func<string, string> transform)
    {
        // Find the <add .../> element carrying this name, then rewrite only its
        // connectionString attribute. Matching the element first keeps a same-named
        // attribute elsewhere in the file out of scope.
        var addRx = new Regex(@"<add\s[^>]*?/?>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var nameRx = new Regex(@"\bname\s*=\s*[""']" + Regex.Escape(entryName) + @"[""']", RegexOptions.IgnoreCase);
        var csRx = new Regex(@"(\bconnectionString\s*=\s*"")([^""]*)("")", RegexOptions.IgnoreCase);

        var matched = false;
        var output = addRx.Replace(text, m =>
        {
            if (matched || !nameRx.IsMatch(m.Value)) return m.Value;

            var replaced = csRx.Replace(m.Value, inner =>
            {
                matched = true;
                // The attribute text is XML-encoded; the keys we touch contain no
                // special characters, so transforming it in place is safe.
                return inner.Groups[1].Value + transform(inner.Groups[2].Value) + inner.Groups[3].Value;
            }, 1);

            return replaced;
        });

        if (!matched)
            throw new InvalidOperationException(
                $"Could not locate <add name=\"{entryName}\" connectionString=\"...\"/> in the file text.");

        return output;
    }

    // ── Backup / encoding ─────────────────────────────────────────────────────

    /// <summary>Copies the config next to itself as Web.config.bak-yyyyMMdd_HHmmss.</summary>
    public static string CreateBackup(string configPath)
    {
        var backup = configPath + ".bak-" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var n = 1;
        while (File.Exists(backup)) backup = configPath + $".bak-{DateTime.Now:yyyyMMdd_HHmmss}-{n++}";
        File.Copy(configPath, backup);
        return backup;
    }

    /// <summary>Lists this config's backups, newest first.</summary>
    public static List<string> ListBackups(string configPath)
    {
        try
        {
            var dir = Path.GetDirectoryName(configPath);
            if (dir == null) return new List<string>();
            return Directory
                .EnumerateFiles(dir, Path.GetFileName(configPath) + ".bak-*")
                .OrderByDescending(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch { return new List<string>(); }
    }

    private static (string Text, Encoding Encoding, byte[] Bom) ReadWithEncoding(string path)
    {
        var bytes = File.ReadAllBytes(path);

        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return (new UTF8Encoding(false).GetString(bytes, 3, bytes.Length - 3),
                    new UTF8Encoding(false), new byte[] { 0xEF, 0xBB, 0xBF });

        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return (Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2),
                    new UnicodeEncoding(false, false), new byte[] { 0xFF, 0xFE });

        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return (Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2),
                    new UnicodeEncoding(true, false), new byte[] { 0xFE, 0xFF });

        return (new UTF8Encoding(false).GetString(bytes), new UTF8Encoding(false), Array.Empty<byte>());
    }

    private static void WriteWithEncoding(string path, string text, Encoding encoding, byte[] bom)
    {
        var body = encoding.GetBytes(text);
        var all = new byte[bom.Length + body.Length];
        Buffer.BlockCopy(bom, 0, all, 0, bom.Length);
        Buffer.BlockCopy(body, 0, all, bom.Length, body.Length);
        File.WriteAllBytes(path, all);
    }
}
