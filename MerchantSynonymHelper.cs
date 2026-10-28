using System.Text;

namespace BLogicDevTool;

/// <summary>Builds an alias database of synonyms so hardcoded three-part names keep resolving.</summary>
/// <remarks>Planet server code hardcodes MERCHANT.dbo.* in raw SQL, so renaming the merchant
/// database breaks reports and kiosk payment writes unless an alias DB forwards those names.</remarks>
public static class MerchantSynonymHelper
{
    /// <summary>Creates the alias database if needed, then points a synonym at every table and view
    /// of <paramref name="realDb"/>. Existing real objects in the alias DB are left alone.</summary>
    public static async Task CreateOrRefreshAsync(
        SqlServerHelper helper, string aliasDb, string realDb, Action<string> log)
    {
        if (string.IsNullOrWhiteSpace(aliasDb)) throw new ArgumentException("Alias database name is empty.");
        if (string.IsNullOrWhiteSpace(realDb)) throw new ArgumentException("Target database name is empty.");

        if (string.Equals(aliasDb, realDb, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Alias and target are the same database ('{aliasDb}') — nothing to alias.");

        if (!await helper.DatabaseExistsAsync(realDb))
            throw new InvalidOperationException($"Target database '{realDb}' does not exist or is offline.");

        var aliasExisted = await helper.DatabaseExistsAsync(aliasDb);
        if (!aliasExisted)
        {
            log($"Creating alias database [{aliasDb}] ...");
            await helper.ExecuteSqlBatchesAsync(
                $"IF DB_ID(N'{Escape(aliasDb)}') IS NULL CREATE DATABASE {Quote(aliasDb)};", "master");
        }
        else
        {
            log($"Alias database [{aliasDb}] already exists — refreshing its synonyms.");
        }

        var objects = await helper.GetTablesAndViewsAsync(realDb);
        if (objects.Count == 0)
            throw new InvalidOperationException($"'{realDb}' contains no tables or views to alias.");

        var schemas = objects.Select(o => o.Schema)
                             .Distinct(StringComparer.OrdinalIgnoreCase)
                             .ToList();

        var sql = new StringBuilder();
        foreach (var schema in schemas.Where(s => !s.Equals("dbo", StringComparison.OrdinalIgnoreCase)))
        {
            sql.AppendLine($"IF SCHEMA_ID(N'{Escape(schema)}') IS NULL EXEC(N'CREATE SCHEMA {Quote(schema)}');");
        }
        sql.AppendLine("GO");

        foreach (var (schema, name) in objects)
        {
            var full = $"{Quote(schema)}.{Quote(name)}";
            var literal = $"{Escape(schema)}.{Escape(name)}";

            // Drop only a previous synonym. A real table sharing the name means the alias DB is
            // not a pure alias, and silently dropping it would destroy data.
            sql.AppendLine($"IF OBJECT_ID(N'{literal}', 'SN') IS NOT NULL DROP SYNONYM {full};");
            sql.AppendLine($"IF OBJECT_ID(N'{literal}') IS NULL");
            sql.AppendLine($"    CREATE SYNONYM {full} FOR {Quote(realDb)}.{Quote(schema)}.{Quote(name)};");
        }

        log($"Applying {objects.Count} synonyms in [{aliasDb}] → [{realDb}] ...");
        await helper.ExecuteSqlBatchesAsync(sql.ToString(), aliasDb);

        var skipped = await CountNonSynonymsAsync(helper, aliasDb, objects);
        if (skipped > 0)
            log($"⚠ {skipped} name(s) already exist in [{aliasDb}] as real tables/views and were left untouched.");

        log($"✔ [{aliasDb}] now aliases {objects.Count - skipped} object(s) of [{realDb}].");
    }

    private static async Task<int> CountNonSynonymsAsync(
        SqlServerHelper helper, string aliasDb, List<(string Schema, string Name)> expected)
    {
        var actual = await helper.GetTablesAndViewsAsync(aliasDb);
        var set = new HashSet<string>(
            expected.Select(e => e.Schema + "." + e.Name), StringComparer.OrdinalIgnoreCase);
        return actual.Count(a => set.Contains(a.Schema + "." + a.Name));
    }

    /// <summary>The SQL a user can copy into SSMS instead of letting the tool run it.</summary>
    public static string PreviewScript(string aliasDb, string realDb) =>
$@"-- Alias database so hardcoded {aliasDb}.dbo.* names resolve to [{realDb}].
IF DB_ID(N'{Escape(aliasDb)}') IS NULL CREATE DATABASE {Quote(aliasDb)};
GO
USE {Quote(aliasDb)};
GO
-- One of these per table/view in [{realDb}]:
-- IF OBJECT_ID(N'dbo.TransactionDetail', 'SN') IS NOT NULL DROP SYNONYM [dbo].[TransactionDetail];
-- CREATE SYNONYM [dbo].[TransactionDetail] FOR {Quote(realDb)}.[dbo].[TransactionDetail];
";

    private static string Quote(string identifier) => "[" + identifier.Replace("]", "]]") + "]";

    private static string Escape(string literal) => literal.Replace("'", "''");
}
