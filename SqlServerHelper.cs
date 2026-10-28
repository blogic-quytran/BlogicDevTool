using Microsoft.Data.SqlClient;

namespace BLogicDevTool;

public class SqlServerHelper
{
    private readonly string _connectionString;

    public SqlServerHelper(string server, bool windowsAuth, string username = "", string password = "")
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            TrustServerCertificate = true,
            ConnectTimeout = 10
        };

        if (windowsAuth)
            builder.IntegratedSecurity = true;
        else
        {
            builder.UserID = username;
            builder.Password = password;
        }

        _connectionString = builder.ConnectionString;
    }

    public async Task TestConnectionAsync()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
    }

    public async Task<List<string>> GetDatabasesAsync()
    {
        var list = new List<string>();
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "SELECT name FROM sys.databases WHERE state_desc = 'ONLINE' ORDER BY name", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(reader.GetString(0));
        return list;
    }

    /// <summary>Schema-qualified tables and views of a database, for synonym generation.</summary>
    public async Task<List<(string Schema, string Name)>> GetTablesAndViewsAsync(string databaseName)
    {
        var list = new List<(string, string)>();
        var builder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = databaseName
        };
        using var conn = new SqlConnection(builder.ConnectionString);
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
SELECT s.name AS SchemaName, o.name AS ObjectName
FROM sys.objects o WITH (NOLOCK)
JOIN sys.schemas s WITH (NOLOCK) ON s.schema_id = o.schema_id
WHERE o.type IN ('U', 'V') AND o.is_ms_shipped = 0
ORDER BY s.name, o.name", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add((reader.GetString(0), reader.GetString(1)));
        return list;
    }

    /// <summary>True when a database with this name exists and is online.</summary>
    public async Task<bool> DatabaseExistsAsync(string databaseName)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "SELECT COUNT(*) FROM sys.databases WITH (NOLOCK) WHERE name = @n AND state_desc = 'ONLINE'", conn);
        cmd.Parameters.AddWithValue("@n", databaseName);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
    }

    public async Task<string> GetDefaultDataPathAsync()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS NVARCHAR(512))", conn);
        var result = await cmd.ExecuteScalarAsync();
        return result?.ToString()?.TrimEnd('\\')
            ?? @"C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA";
    }

    public async Task ExecuteSqlBatchesAsync(string sql)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await ExecuteBatchesOnConnectionAsync(conn, sql);
    }

    /// <summary>Executes SQL batches against a specific database on this server.</summary>
    public async Task ExecuteSqlBatchesAsync(string sql, string databaseName)
    {
        var builder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = databaseName
        };
        using var conn = new SqlConnection(builder.ConnectionString);
        await conn.OpenAsync();
        await ExecuteBatchesOnConnectionAsync(conn, sql);
    }

    private static async Task ExecuteBatchesOnConnectionAsync(SqlConnection conn, string sql)
    {
        // One splitter for both run paths, so the same .sql file batches identically here
        // and in ExecuteSqlBatchesContinueOnErrorAsync.
        foreach (var (text, _) in SplitBatchesWithLines(sql))
        {
            var trimmed = text.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;
            using var cmd = new SqlCommand(trimmed, conn)
            {
                CommandTimeout = 600
            };
            await cmd.ExecuteNonQueryAsync();
        }
    }

    /// <summary>Runs a script the way SSMS does: a failing batch is logged (Msg/Level/State/
    /// Line) and the rest still run. Stops early only if the failed batch switched DB
    /// context, since everything after it would hit the wrong database.</summary>
    public async Task<SqlBatchRunResult> ExecuteSqlBatchesContinueOnErrorAsync(
        string sql, string databaseName, Action<string>? log = null)
    {
        var builder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = databaseName
        };
        using var conn = new SqlConnection(builder.ConnectionString);
        await conn.OpenAsync();

        var result = new SqlBatchRunResult();
        foreach (var (text, startLine) in SplitBatchesWithLines(sql))
        {
            var trimmed = text.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;
            result.TotalBatches++;
            var failed = false;
            using var cmd = new SqlCommand(trimmed, conn) { CommandTimeout = 600 };
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                foreach (SqlError err in ex.Errors)
                {
                    // Class < 11 are pure informational messages, not errors.
                    if (err.Class < 11) continue;
                    // err.LineNumber is relative to the batch — offset it back to the
                    // line in the original script so it matches what the user sees.
                    var absLine = startLine + Math.Max(0, err.LineNumber - 1);
                    var line = $"Msg {err.Number}, Level {err.Class}, State {err.State}, Line {absLine}: {err.Message}";
                    result.Errors.Add(line);
                    log?.Invoke("  ✘ " + line);
                    failed = true;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                log?.Invoke("  ✘ " + ex.Message);
                failed = true;
            }

            if (!failed) continue;      // nothing above informational level → not a failure
            result.FailedBatches++;

            // DB context lives on the connection: if the batch that switched it failed, every
            // later batch would run against the previous database. Stop instead.
            if (UseStatementRegex.IsMatch(trimmed))
            {
                result.Aborted = true;
                result.AbortReason = "a USE (database context) statement failed";
                log?.Invoke("  ✘ Stopped: that batch switches database context — running the "
                            + "rest would target the wrong database.");
                break;
            }
        }
        return result;
    }

    private static readonly System.Text.RegularExpressions.Regex UseStatementRegex = new(
        @"^\s*USE\s+", System.Text.RegularExpressions.RegexOptions.Multiline
                       | System.Text.RegularExpressions.RegexOptions.IgnoreCase
                       | System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly System.Text.RegularExpressions.Regex GoLineRegex = new(
        @"^\s*GO\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase
                       | System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>Splits on standalone GO lines, tracking each batch's 1-based start line so
    /// error line numbers can be reported against the whole file.</summary>
    private static List<(string Text, int StartLine)> SplitBatchesWithLines(string sql)
    {
        var result = new List<(string, int)>();
        var sb = new System.Text.StringBuilder();
        int batchStartLine = 1;
        int currentLine = 0;
        foreach (var raw in sql.Split('\n'))
        {
            currentLine++;
            var lineText = raw.TrimEnd('\r');
            if (GoLineRegex.IsMatch(lineText))
            {
                result.Add((sb.ToString(), batchStartLine));
                sb.Clear();
                batchStartLine = currentLine + 1;
            }
            else
            {
                sb.Append(lineText).Append('\n');
            }
        }
        if (sb.Length > 0)
            result.Add((sb.ToString(), batchStartLine));
        return result;
    }

    // ── SQL Server info ───────────────────────────────────────────────────

    public async Task<SqlServerInfo> GetServerInfoAsync()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
SELECT
    CAST(SERVERPROPERTY('ServerName')      AS NVARCHAR(256)) AS ServerName,
    CAST(SERVERPROPERTY('ProductVersion')  AS NVARCHAR(128)) AS Version,
    CAST(SERVERPROPERTY('Edition')         AS NVARCHAR(128)) AS Edition,
    CAST(SERVERPROPERTY('MachineName')     AS NVARCHAR(128)) AS MachineName,
    CAST(SERVERPROPERTY('IsIntegratedSecurityOnly') AS INT)  AS WinAuthOnly;";

        using var cmd    = new SqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return new SqlServerInfo();

        return new SqlServerInfo
        {
            ServerName  = reader["ServerName"]?.ToString()  ?? "",
            Version     = reader["Version"]?.ToString()     ?? "",
            Edition     = reader["Edition"]?.ToString()     ?? "",
            MachineName = reader["MachineName"]?.ToString() ?? "",
            IsMixedMode = (reader["WinAuthOnly"] is int v) && v == 0
        };
    }

    // ── Login management ─────────────────────────────────────────────────

    public async Task<List<SqlLoginInfo>> GetLoginsAsync()
    {
        var list = new List<SqlLoginInfo>();
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
SELECT
    p.name,
    p.type_desc,
    p.is_disabled,
    ISNULL(IS_SRVROLEMEMBER('sysadmin', p.name), 0) AS is_sysadmin,
    p.create_date,
    p.modify_date
FROM sys.server_principals p
WHERE p.type IN ('S', 'U', 'G')
ORDER BY p.name;";

        using var cmd    = new SqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new SqlLoginInfo
            {
                Name       = reader.GetString(0),
                Type       = reader.GetString(1),
                IsDisabled = reader.GetBoolean(2),
                IsSysAdmin = reader.GetInt32(3) == 1,
                CreateDate = reader.GetDateTime(4),
                ModifyDate = reader.GetDateTime(5)
            });
        }

        return list;
    }

    /// <summary>Sets SA password, enables the SA login, and ensures it has sysadmin role.</summary>
    public async Task ConfigureSaAccountAsync(string saPassword)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        // ALTER LOGIN can't be parameterized — embed the password as an escaped literal.
        var sql = $@"
USE [master];
ALTER LOGIN [sa] WITH PASSWORD = {QuoteLiteral(saPassword)}, CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;
ALTER LOGIN [sa] ENABLE;
IF IS_SRVROLEMEMBER('sysadmin', 'sa') = 0
    ALTER SERVER ROLE [sysadmin] ADD MEMBER [sa];";

        using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 30 };
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>Creates a new SQL login with the given name and password.</summary>
    public async Task CreateLoginAsync(string loginName, string password, bool isSysAdmin)
    {
        ValidateIdentifier(loginName);

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        // CREATE LOGIN does not support parameterized name/password — use quoted identifier
        // for the name and an escaped string literal for the password.
        var sql = $"CREATE LOGIN [{EscapeIdentifier(loginName)}] WITH PASSWORD = {QuoteLiteral(password)}, CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;";
        using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 30 };
        await cmd.ExecuteNonQueryAsync();

        if (isSysAdmin)
        {
            var sysAdminSql = $"ALTER SERVER ROLE [sysadmin] ADD MEMBER [{EscapeIdentifier(loginName)}];";
            using var cmd2 = new SqlCommand(sysAdminSql, conn) { CommandTimeout = 30 };
            await cmd2.ExecuteNonQueryAsync();
        }
    }

    /// <summary>Drops a SQL login by name.</summary>
    public async Task DropLoginAsync(string loginName)
    {
        ValidateIdentifier(loginName);

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = $"DROP LOGIN [{EscapeIdentifier(loginName)}];";
        using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 30 };
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>Enables or disables a SQL login.</summary>
    public async Task SetLoginEnabledAsync(string loginName, bool enabled)
    {
        ValidateIdentifier(loginName);

        var directive = enabled ? "ENABLE" : "DISABLE";
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = $"ALTER LOGIN [{EscapeIdentifier(loginName)}] {directive};";
        using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 30 };
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>Changes the password for a SQL login.</summary>
    public async Task SetLoginPasswordAsync(string loginName, string newPassword)
    {
        ValidateIdentifier(loginName);

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        // ALTER LOGIN can't be parameterized — embed the password as an escaped literal.
        var sql = $"ALTER LOGIN [{EscapeIdentifier(loginName)}] WITH PASSWORD = {QuoteLiteral(newPassword)}, CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;";
        using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 30 };
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>Grants sysadmin server role to the specified login.</summary>
    public async Task GrantSysAdminAsync(string loginName)
    {
        ValidateIdentifier(loginName);

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = $"IF IS_SRVROLEMEMBER('sysadmin', [{EscapeIdentifier(loginName)}]) = 0 " +
                  $"ALTER SERVER ROLE [sysadmin] ADD MEMBER [{EscapeIdentifier(loginName)}];";
        using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 30 };
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>Frees a DB left in SINGLE_USER by a half-done restore, so a retry isn't
    /// blocked by "a user is currently connected". Never throws.</summary>
    public async Task TrySetMultiUserAsync(string databaseName)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(_connectionString)
            {
                InitialCatalog = "master"
            };
            using var conn = new SqlConnection(builder.ConnectionString);
            await conn.OpenAsync();
            // QuoteLiteral already emits the N prefix — do not add a second one, or the
            // statement becomes NN'db' and fails with Msg 102 before it can recover anything.
            var sql =
                $"IF DB_ID({QuoteLiteral(databaseName)}) IS NOT NULL " +
                $"AND DATABASEPROPERTYEX({QuoteLiteral(databaseName)}, 'UserAccess') = 'SINGLE_USER' " +
                $"ALTER DATABASE [{EscapeIdentifier(databaseName)}] SET MULTI_USER WITH ROLLBACK IMMEDIATE;";
            using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 30 };
            await cmd.ExecuteNonQueryAsync();
        }
        catch { /* best effort — recovery must never mask the original error */ }
    }

    // ── Security helpers ──────────────────────────────────────────────────

    /// <summary>Validates that a login name contains only safe characters.</summary>
    private static void ValidateIdentifier(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Login name cannot be empty.");

        // Disallow bracket characters which could break the quoted identifier escaping
        if (name.Contains(']') || name.Contains('\0'))
            throw new ArgumentException($"Login name contains invalid characters: '{name}'.");
    }

    /// <summary>Escapes a SQL identifier by doubling any closing bracket.</summary>
    private static string EscapeIdentifier(string name) => name.Replace("]", "]]");

    /// <summary>Quotes a value as an N'...' literal for CREATE/ALTER LOGIN, which reject
    /// parameters. Doubling the quotes is what keeps it injection-safe.</summary>
    private static string QuoteLiteral(string value) => "N'" + (value ?? "").Replace("'", "''") + "'";
}

/// <summary>Summary of a "continue on error" batch run.</summary>
public class SqlBatchRunResult
{
    public int TotalBatches { get; set; }
    public int FailedBatches { get; set; }
    public List<string> Errors { get; } = new();

    /// <summary>Set when the run stopped early rather than continuing past a failure.</summary>
    public bool Aborted { get; set; }
    public string? AbortReason { get; set; }
}
