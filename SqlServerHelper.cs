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
        // Split on GO statements
        var batches = System.Text.RegularExpressions.Regex.Split(
            sql, @"^\s*GO\s*$",
            System.Text.RegularExpressions.RegexOptions.Multiline |
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (var batch in batches)
        {
            var trimmed = batch.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;
            using var cmd = new SqlCommand(trimmed, conn)
            {
                CommandTimeout = 600
            };
            await cmd.ExecuteNonQueryAsync();
        }
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

        // Parameter binding is used for the password to prevent injection
        const string sql = @"
USE [master];
ALTER LOGIN [sa] WITH PASSWORD = @pwd, CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;
ALTER LOGIN [sa] ENABLE;
IF IS_SRVROLEMEMBER('sysadmin', 'sa') = 0
    ALTER SERVER ROLE [sysadmin] ADD MEMBER [sa];";

        using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 30 };
        cmd.Parameters.Add("@pwd", System.Data.SqlDbType.NVarChar, 128).Value = saPassword;
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>Creates a new SQL login with the given name and password.</summary>
    public async Task CreateLoginAsync(string loginName, string password, bool isSysAdmin)
    {
        ValidateIdentifier(loginName);

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        // CREATE LOGIN does not support parameterized name/password — use quoted identifier
        var sql = $"CREATE LOGIN [{EscapeIdentifier(loginName)}] WITH PASSWORD = @pwd, CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;";
        using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 30 };
        cmd.Parameters.Add("@pwd", System.Data.SqlDbType.NVarChar, 128).Value = password;
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

        var sql = $"ALTER LOGIN [{EscapeIdentifier(loginName)}] WITH PASSWORD = @pwd, CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;";
        using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 30 };
        cmd.Parameters.Add("@pwd", System.Data.SqlDbType.NVarChar, 128).Value = newPassword;
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
}
