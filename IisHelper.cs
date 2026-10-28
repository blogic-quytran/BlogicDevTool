using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.Web.Administration;

namespace BLogicDevTool;

/// <summary>Represents one IIS site/application entry shown in the grid.</summary>
public class IisAppEntry
{
    public string SiteName { get; set; } = "";
    public string AppPath { get; set; } = "/";
    public string PhysicalPath { get; set; } = "";
    public string CurrentDatabase { get; set; } = "";
    public string AppPoolName { get; set; } = "";

    /// <summary>Full path to this app's Web.config, or null when it has none.</summary>
    public string? ConfigPath { get; set; }

    /// <summary>Every connection string found in the Web.config.</summary>
    public List<ConnStringEntry> ConnStrings { get; set; } = new();

    public string Display => $"{SiteName}{AppPath}";

    /// <summary>All databases this app points at, so a second connection string cannot hide.</summary>
    public string DatabaseSummary
    {
        get
        {
            var dbs = ConnStrings
                .Where(c => c.Database.Length > 0)
                .Select(c => c.Database)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            return dbs.Count == 0 ? "" : string.Join(", ", dbs);
        }
    }
}

public static class IisHelper
{
    // ── Enumerate ─────────────────────────────────────────────────────────────

    public static List<IisAppEntry> GetApplications()
    {
        var result = new List<IisAppEntry>();
        using var mgr = new ServerManager();
        foreach (var site in mgr.Sites)
        {
            foreach (var app in site.Applications)
            {
                var vdir = app.VirtualDirectories["/"];
                var physPath = Environment.ExpandEnvironmentVariables(vdir?.PhysicalPath ?? "");
                var configPath = WebConfigConnStrings.ResolveConfigPath(physPath);
                var connStrings = WebConfigConnStrings.Read(configPath);

                result.Add(new IisAppEntry
                {
                    SiteName = site.Name,
                    AppPath = app.Path,
                    PhysicalPath = physPath,
                    AppPoolName = app.ApplicationPoolName,
                    ConfigPath = configPath,
                    ConnStrings = connStrings,
                    CurrentDatabase = WebConfigConnStrings.PickPrimary(connStrings)?.Database ?? ""
                });
            }
        }
        return result;
    }

    // ── Web.config helpers ────────────────────────────────────────────────────

    /// <summary>Database of this app's primary connection string, or "" when it has none.</summary>
    public static string ReadDatabaseFromWebConfig(string physicalPath)
    {
        var configPath = WebConfigConnStrings.ResolveConfigPath(physicalPath);
        var entries = WebConfigConnStrings.Read(configPath);
        return WebConfigConnStrings.PickPrimary(entries)?.Database ?? "";
    }

    /// <summary>Every connection string in this app's Web.config.</summary>
    public static List<ConnStringEntry> ReadConnStringsFromWebConfig(string physicalPath)
        => WebConfigConnStrings.Read(WebConfigConnStrings.ResolveConfigPath(physicalPath));

    /// <summary>Points one connection string at a new server and/or database, verifying the write.
    /// Only the named entry is touched — other connection strings in the same file keep their values.</summary>
    public static ConnStringPatchResult SetConnStringInWebConfig(
        string physicalPath,
        ConnStringKind kind,
        string entryName,
        string? newDataSource,
        string? newDatabase,
        bool makeBackup = true)
    {
        var configPath = WebConfigConnStrings.ResolveConfigPath(physicalPath);
        if (configPath == null)
            return new ConnStringPatchResult { Error = $"No Web.config under {physicalPath}." };

        return WebConfigConnStrings.Patch(
            configPath, kind, entryName, newDataSource, newDatabase, makeBackup);
    }

    /// <summary>Points this app's primary connection string at <paramref name="newDatabase"/>.</summary>
    public static ConnStringPatchResult SetDatabaseInWebConfig(
        string physicalPath, string newDatabase, bool makeBackup = true)
    {
        var configPath = WebConfigConnStrings.ResolveConfigPath(physicalPath);
        if (configPath == null)
            return new ConnStringPatchResult { Error = $"No Web.config under {physicalPath}." };

        var primary = WebConfigConnStrings.PickPrimary(WebConfigConnStrings.Read(configPath));
        if (primary == null)
            return new ConnStringPatchResult
            {
                Error = $"{Path.GetFileName(configPath)} holds no connection string to update."
            };

        return WebConfigConnStrings.Patch(
            configPath, primary.Kind, primary.Name, null, newDatabase, makeBackup);
    }

    // ── Physical path ─────────────────────────────────────────────────────────

    public static void SetPhysicalPath(string siteName, string appPath, string newPhysicalPath)
    {
        using var mgr = new ServerManager();
        var site = mgr.Sites[siteName]
            ?? throw new InvalidOperationException($"Site '{siteName}' not found.");
        var app = site.Applications[appPath]
            ?? throw new InvalidOperationException($"Application '{appPath}' not found.");
        var vdir = app.VirtualDirectories["/"]
            ?? throw new InvalidOperationException("Root virtual directory not found.");
        vdir.PhysicalPath = newPhysicalPath;
        mgr.CommitChanges();
    }

    // ── IIS restart ───────────────────────────────────────────────────────────

    public static async Task RestartIisAsync()
    {
        await RunProcessAsync("iisreset.exe", "/restart /noforce", timeoutMs: 60_000);
    }

    public static async Task StopIisAsync()
    {
        await RunProcessAsync("iisreset.exe", "/stop", timeoutMs: 30_000);
    }

    public static async Task StartIisAsync()
    {
        await RunProcessAsync("iisreset.exe", "/start", timeoutMs: 30_000);
    }

    /// <summary>Recycles one pool. Uses the full appcmd path — inetsrv is not on PATH by default.</summary>
    public static async Task RecycleAppPoolAsync(string poolName)
    {
        if (!File.Exists(AppCmd))
            throw new FileNotFoundException("appcmd.exe not found — IIS management tools are not installed.", AppCmd);

        await RunProcessAsync(AppCmd,
            $"recycle apppool /apppool.name:\"{poolName}\"", timeoutMs: 30_000);
    }

    /// <summary>Recycles each distinct pool, reporting per-pool outcome instead of aborting on the first failure.</summary>
    public static async Task<List<string>> RecycleAppPoolsAsync(IEnumerable<string> poolNames)
    {
        var log = new List<string>();
        foreach (var pool in poolNames.Where(p => !string.IsNullOrWhiteSpace(p))
                                      .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                await RecycleAppPoolAsync(pool);
                log.Add($"✔ Recycled app pool '{pool}'.");
            }
            catch (Exception ex)
            {
                log.Add($"✘ App pool '{pool}': {ex.Message}");
            }
        }
        return log;
    }

    private static Task RunProcessAsync(string exe, string args, int timeoutMs)
    {
        return Task.Run(() =>
        {
            var psi = new ProcessStartInfo(exe, args)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi)
                ?? throw new InvalidOperationException($"Cannot start {exe}");
            if (!proc.WaitForExit(timeoutMs))
            {
                proc.Kill();
                throw new TimeoutException($"{exe} timed out after {timeoutMs / 1000}s");
            }
            if (proc.ExitCode != 0)
            {
                var err = proc.StandardError.ReadToEnd();
                throw new InvalidOperationException(
                    $"{exe} exited with code {proc.ExitCode}: {err}");
            }
        });
    }

    // ── Troubleshoot helpers ──────────────────────────────────────────────────

    private static readonly string AppCmd =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                     "inetsrv", "appcmd.exe");

    /// <summary>Run a command and capture stdout + stderr. Never throws.</summary>
    public static async Task<string> RunCmdAsync(string exe, string args, int timeoutMs = 15_000)
    {
        return await Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo(exe, args)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    CreateNoWindow         = true
                };
                using var proc = Process.Start(psi);
                if (proc == null) return $"[ERROR] Cannot start: {exe}";
                var stdout = proc.StandardOutput.ReadToEnd();
                var stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit(timeoutMs);
                var result = stdout;
                if (!string.IsNullOrWhiteSpace(stderr))
                    result += (result.Length > 0 ? "\n" : "") + "STDERR: " + stderr;
                return result.TrimEnd();
            }
            catch (Exception ex)
            {
                return $"[ERROR] {ex.Message}";
            }
        });
    }

    // Step 1 — service state
    public static async Task<string> TroubleStep1_CheckServicesAsync()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var svc in new[] { "HTTP", "WAS", "W3SVC" })
        {
            sb.AppendLine($"── {svc} ──────────────────────────");
            sb.AppendLine(await RunCmdAsync("sc", $"query \"{svc}\""));
            var qc = await RunCmdAsync("sc", $"qc \"{svc}\"");
            // extract just START_TYPE line
            var startLine = qc.Split('\n')
                .FirstOrDefault(l => l.Contains("START_TYPE", StringComparison.OrdinalIgnoreCase));
            if (startLine != null) sb.AppendLine(startLine.Trim());
            sb.AppendLine();
        }
        return sb.ToString();
    }

    // Step 2 — fix startup types + start dependencies
    public static async Task<string> TroubleStep2_FixStartupAsync()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[RUN] sc config HTTP start= demand");
        sb.AppendLine(await RunCmdAsync("sc", "config HTTP start= demand"));
        sb.AppendLine("[RUN] sc config WAS start= auto");
        sb.AppendLine(await RunCmdAsync("sc", "config WAS start= auto"));
        sb.AppendLine("[RUN] sc config W3SVC start= auto");
        sb.AppendLine(await RunCmdAsync("sc", "config W3SVC start= auto"));
        sb.AppendLine("[RUN] net start HTTP");
        sb.AppendLine(await RunCmdAsync("net", "start HTTP"));
        sb.AppendLine("[RUN] net start WAS");
        sb.AppendLine(await RunCmdAsync("net", "start WAS"));
        return sb.ToString();
    }

    // Step 3 — backup IIS config
    public static async Task<string> TroubleStep3_BackupConfigAsync()
    {
        if (!File.Exists(AppCmd))
            return "[ERROR] appcmd.exe not found. IIS may not be fully installed.";
        var name = $"blogictool_{DateTime.Now:yyyyMMdd_HHmmss}";
        var result = await RunCmdAsync(AppCmd, $"add backup \"{name}\"");
        return $"Backup name: {name}\n{result}";
    }

    // Step 4 — validate config
    public static async Task<string> TroubleStep4_ValidateConfigAsync()
    {
        if (!File.Exists(AppCmd))
            return "[ERROR] appcmd.exe not found.";
        var sites = await RunCmdAsync(AppCmd, "list site");
        var pools = await RunCmdAsync(AppCmd, "list apppool");
        return $"── Sites ──\n{sites}\n── AppPools ──\n{pools}";
    }

    // Step 5 — check ports 80 / 443
    public static async Task<string> TroubleStep5_CheckPortsAsync()
    {
        var sb = new System.Text.StringBuilder();
        var netstat = await RunCmdAsync("netstat", "-ano");
        foreach (var port in new[] { 80, 443 })
        {
            var hits = netstat.Split('\n')
                .Where(l =>
                {
                    var t = l.Trim();
                    return (t.Contains($":{port} ") || t.Contains($":{port}\t"))
                           && t.StartsWith("TCP", StringComparison.OrdinalIgnoreCase);
                })
                .ToList();

            if (hits.Count == 0)
            {
                sb.AppendLine($"Port {port}: FREE (no process listening)");
            }
            else
            {
                sb.AppendLine($"Port {port}: OCCUPIED by:");
                foreach (var h in hits)
                {
                    var parts = h.Trim().Split(new[] { ' ', '\t' },
                        StringSplitOptions.RemoveEmptyEntries);
                    var pid = parts.LastOrDefault() ?? "?";
                    // try to resolve PID to process name
                    try
                    {
                        var proc = System.Diagnostics.Process.GetProcessById(int.Parse(pid));
                        sb.AppendLine($"  PID {pid}  ({proc.ProcessName})  {h.Trim()}");
                    }
                    catch { sb.AppendLine($"  PID {pid}  {h.Trim()}"); }
                }
            }
        }
        return sb.ToString();
    }

    // Step 6 — start W3SVC
    public static async Task<string> TroubleStep6_StartW3SvcAsync()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[RUN] net start W3SVC");
        sb.AppendLine(await RunCmdAsync("net", "start W3SVC"));

        var state = await RunCmdAsync("sc", "query W3SVC");
        if (state.Contains("RUNNING", StringComparison.OrdinalIgnoreCase))
        {
            sb.AppendLine("[SUCCESS] W3SVC is now RUNNING.");
            return sb.ToString();
        }

        sb.AppendLine("[WARN] W3SVC still not running. Trying iisreset /start ...");
        sb.AppendLine(await RunCmdAsync("iisreset", "/start"));

        state = await RunCmdAsync("sc", "query W3SVC");
        sb.AppendLine(state.Contains("RUNNING", StringComparison.OrdinalIgnoreCase)
            ? "[SUCCESS] W3SVC started after iisreset."
            : "[WARN] W3SVC still not running. See Step 7 for manual diagnostics.");
        return sb.ToString();
    }

    // Step 7 — extra diagnostics (read applicationHost.config health + instructions)
    public static string TroubleStep7_ExtraDiagnostics()
    {
        var sb = new System.Text.StringBuilder();
        var hostConfig = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            @"inetsrv\config\applicationHost.config");

        sb.AppendLine("── Manual checks ──────────────────────────────");
        sb.AppendLine("• Event Viewer → Windows Logs → System");
        sb.AppendLine("  Filter: Service Control Manager / WAS / W3SVC / IIS-W3SVC-WP");
        sb.AppendLine();
        sb.AppendLine("• Event Viewer → Windows Logs → Application");
        sb.AppendLine("  Filter: ASP.NET, W3SVC");
        sb.AppendLine();
        sb.AppendLine($"• IIS config file: {hostConfig}");

        if (File.Exists(hostConfig))
        {
            try
            {
                System.Xml.Linq.XDocument.Load(hostConfig);  // parse check
                sb.AppendLine("  → applicationHost.config XML is VALID.");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"  → [ERROR] applicationHost.config is INVALID XML: {ex.Message}");
                sb.AppendLine("    Restore with: appcmd restore backup \"BACKUP_NAME\"");
            }
        }
        else
        {
            sb.AppendLine("  → [WARN] applicationHost.config not found.");
        }

        sb.AppendLine();
        sb.AppendLine("── Quick commands ──────────────────────────────");
        if (File.Exists(AppCmd))
        {
            sb.AppendLine($"  {AppCmd} list apppool");
            sb.AppendLine($"  {AppCmd} list site");
            sb.AppendLine($"  {AppCmd} restore backup \"BACKUP_NAME\"");
        }
        return sb.ToString();
    }

    // Step 8 — final state
    public static async Task<string> TroubleStep8_FinalStateAsync()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("── Final service state ─────────────────────────");
        foreach (var svc in new[] { "HTTP", "WAS", "W3SVC" })
        {
            var q = await RunCmdAsync("sc", $"query \"{svc}\"");
            var running = q.Contains("RUNNING", StringComparison.OrdinalIgnoreCase);
            sb.AppendLine($"  {(running ? "✔" : "✘")} {svc,-8} {(running ? "RUNNING" : "NOT RUNNING")}");
        }
        sb.AppendLine();

        // IIS site summary
        if (File.Exists(AppCmd))
        {
            sb.AppendLine("── IIS Sites ───────────────────────────────────");
            sb.AppendLine(await RunCmdAsync(AppCmd, "list site /processModel.userName:*"));
        }
        return sb.ToString();
    }
}
