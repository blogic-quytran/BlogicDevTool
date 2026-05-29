using Microsoft.Win32;
using System.ServiceProcess;

namespace BLogicDevTool;

/// <summary>
/// Static helper for discovering local SQL Server instances,
/// enabling Mixed Mode authentication via registry, and
/// restarting the SQL Server Windows service.
/// Requires administrator privileges (enforced by app.manifest).
/// </summary>
public static class SqlInstanceHelper
{
    private const string InstanceNamesKey =
        @"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL";

    // ── Instance discovery ────────────────────────────────────────────────

    /// <summary>
    /// Returns display names (e.g. "." or ".\SQLEXPRESS") for all SQL Server
    /// instances found in the local machine registry.
    /// </summary>
    public static List<string> GetLocalInstances()
    {
        var result = new List<string>();
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(InstanceNamesKey);
            if (key != null)
            {
                foreach (var valueName in key.GetValueNames())
                {
                    var display = string.Equals(valueName, "MSSQLSERVER", StringComparison.OrdinalIgnoreCase)
                        ? "."
                        : $@".\{valueName}";
                    result.Add(display);
                }
            }
        }
        catch { /* registry not accessible */ }

        if (result.Count == 0)
            result.Add(".");

        return result;
    }

    // ── Registry: Mixed Mode ──────────────────────────────────────────────

    /// <summary>
    /// Enables SQL Server Mixed Mode authentication (LoginMode = 2) for the
    /// specified instance by updating the registry.
    /// Changes take effect after the SQL Server service is restarted.
    /// </summary>
    public static void EnableMixedMode(string serverSpec)
    {
        var instanceName = ExtractInstanceName(serverSpec);
        var instanceId   = GetInstanceId(instanceName)
            ?? throw new InvalidOperationException(
                $"Cannot find registry entry for SQL instance '{instanceName}'.\n" +
                "Make sure SQL Server is installed and the instance name is correct.");

        var regPath = $@"SOFTWARE\Microsoft\Microsoft SQL Server\{instanceId}\MSSQLServer";
        using var key = Registry.LocalMachine.OpenSubKey(regPath, writable: true)
            ?? throw new InvalidOperationException(
                $"Cannot open registry key for writing:\n{regPath}\n" +
                "Run this application as Administrator.");

        key.SetValue("LoginMode", 2, RegistryValueKind.DWord);
    }

    /// <summary>
    /// Reads the current LoginMode value for the specified instance.
    /// Returns 2 if Mixed Mode, 1 if Windows Only, -1 if not found.
    /// </summary>
    public static int GetLoginMode(string serverSpec)
    {
        try
        {
            var instanceName = ExtractInstanceName(serverSpec);
            var instanceId   = GetInstanceId(instanceName);
            if (instanceId == null) return -1;

            var regPath = $@"SOFTWARE\Microsoft\Microsoft SQL Server\{instanceId}\MSSQLServer";
            using var key = Registry.LocalMachine.OpenSubKey(regPath);
            return key?.GetValue("LoginMode") is int v ? v : -1;
        }
        catch { return -1; }
    }

    // ── Service management ────────────────────────────────────────────────

    /// <summary>Returns the Windows service name for a SQL Server instance.</summary>
    public static string GetServiceName(string serverSpec)
    {
        var instanceName = ExtractInstanceName(serverSpec);
        return string.Equals(instanceName, "MSSQLSERVER", StringComparison.OrdinalIgnoreCase)
            ? "MSSQLSERVER"
            : $"MSSQL${instanceName}";
    }

    /// <summary>
    /// Stops then starts the SQL Server Windows service for the given instance.
    /// Waits up to 90 seconds for each transition.
    /// </summary>
    public static void RestartSqlService(string serverSpec)
    {
        var serviceName = GetServiceName(serverSpec);
        using var svc   = new ServiceController(serviceName);

        if (svc.Status != ServiceControllerStatus.Stopped &&
            svc.Status != ServiceControllerStatus.StopPending)
        {
            svc.Stop();
            svc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(90));
        }

        svc.Start();
        svc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(90));
    }

    // ── TCP/IP protocol ───────────────────────────────────────────────────

    private static string TcpRegPath(string instanceId)
        => $@"SOFTWARE\Microsoft\Microsoft SQL Server\{instanceId}\MSSQLServer\SuperSocketNetLib\Tcp";

    /// <summary>Returns whether TCP/IP is enabled for the given instance (reads registry).</summary>
    public static bool GetTcpEnabled(string serverSpec)
    {
        try
        {
            var id = GetInstanceId(ExtractInstanceName(serverSpec));
            if (id == null) return false;
            using var key = Registry.LocalMachine.OpenSubKey(TcpRegPath(id));
            return key?.GetValue("Enabled") is int v && v == 1;
        }
        catch { return false; }
    }

    /// <summary>
    /// Returns the current static TCP port ("1433") or "" if using dynamic ports.
    /// </summary>
    public static string GetTcpPort(string serverSpec)
    {
        try
        {
            var id = GetInstanceId(ExtractInstanceName(serverSpec));
            if (id == null) return "";
            using var ipAll = Registry.LocalMachine.OpenSubKey(TcpRegPath(id) + @"\IPAll");
            return ipAll?.GetValue("TcpPort")?.ToString() ?? "";
        }
        catch { return ""; }
    }

    /// <summary>
    /// Enables TCP/IP for the specified instance and sets a static port.
    /// Pass port = 0 to keep the existing port or use 1433 as default.
    /// Changes take effect after the SQL Server service is restarted.
    /// </summary>
    public static void EnableTcpIp(string serverSpec, int port = 1433)
    {
        var instanceName = ExtractInstanceName(serverSpec);
        var instanceId   = GetInstanceId(instanceName)
            ?? throw new InvalidOperationException(
                $"Cannot find registry entry for SQL instance '{instanceName}'.");

        var tcpPath = TcpRegPath(instanceId);

        // Enable TCP/IP
        using (var key = Registry.LocalMachine.OpenSubKey(tcpPath, writable: true)
            ?? throw new InvalidOperationException(
                $"Cannot open registry key:\n{tcpPath}\nRun as Administrator."))
        {
            key.SetValue("Enabled", 1, RegistryValueKind.DWord);
        }

        // Set static port on IPAll, clear dynamic port
        using (var ipAll = Registry.LocalMachine.OpenSubKey(tcpPath + @"\IPAll", writable: true)
            ?? throw new InvalidOperationException(
                $"Cannot open IPAll registry key.\nRun as Administrator."))
        {
            ipAll.SetValue("TcpPort",        port.ToString(), RegistryValueKind.String);
            ipAll.SetValue("TcpDynamicPorts", "",             RegistryValueKind.String);
        }

        // Also enable on all IP addresses (IP1..IP10)
        using var tcpRoot = Registry.LocalMachine.OpenSubKey(tcpPath, writable: true);
        if (tcpRoot != null)
        {
            foreach (var sub in tcpRoot.GetSubKeyNames())
            {
                if (!sub.StartsWith("IP", StringComparison.OrdinalIgnoreCase)) continue;
                if (sub.Equals("IPAll", StringComparison.OrdinalIgnoreCase)) continue;

                using var ipKey = tcpRoot.OpenSubKey(sub, writable: true);
                if (ipKey != null)
                    ipKey.SetValue("Enabled", 1, RegistryValueKind.DWord);
            }
        }
    }

    /// <summary>
    /// Disables TCP/IP for the specified instance in the registry.
    /// Changes take effect after the SQL Server service is restarted.
    /// </summary>
    public static void DisableTcpIp(string serverSpec)
    {
        var instanceName = ExtractInstanceName(serverSpec);
        var instanceId   = GetInstanceId(instanceName)
            ?? throw new InvalidOperationException(
                $"Cannot find registry entry for SQL instance '{instanceName}'.");

        var tcpPath = TcpRegPath(instanceId);
        using var key = Registry.LocalMachine.OpenSubKey(tcpPath, writable: true)
            ?? throw new InvalidOperationException(
                $"Cannot open registry key:\n{tcpPath}\nRun as Administrator.");

        key.SetValue("Enabled", 0, RegistryValueKind.DWord);
    }

    // ── Windows Firewall ──────────────────────────────────────────────────

    /// <summary>
    /// Adds an inbound Windows Firewall rule to allow TCP on the given port.
    /// If a rule with the same name already exists, it is replaced.
    /// </summary>
    public static void AddFirewallRule(int port, string ruleName)
    {
        // Remove existing rule with same name (ignore errors)
        RunNetsh($"advfirewall firewall delete rule name=\"{ruleName}\"");

        // Add new rule
        var result = RunNetsh(
            $"advfirewall firewall add rule name=\"{ruleName}\" " +
            $"protocol=TCP dir=in localport={port} action=allow");

        if (result.ExitCode != 0)
            throw new InvalidOperationException(
                $"netsh failed (exit {result.ExitCode}):\n{result.Output}");
    }

    /// <summary>Checks whether any inbound firewall rule already allows the given TCP port.</summary>
    public static bool IsFirewallPortOpen(int port)
    {
        var result = RunNetsh(
            $"advfirewall firewall show rule name=all dir=in protocol=TCP");
        return result.Output.Contains($"LocalPort{new string(' ', 1)}{port}",
                   StringComparison.OrdinalIgnoreCase) ||
               result.Output.Contains($"LocalPort{new string(' ', 0)}                    {port}",
                   StringComparison.OrdinalIgnoreCase) ||
               result.Output.Contains($":{port}",   StringComparison.OrdinalIgnoreCase);
    }

    private static (int ExitCode, string Output) RunNetsh(string args)
    {
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName               = "netsh",
            Arguments              = args,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true
        };

        using var proc = System.Diagnostics.Process.Start(psi)!;
        string stdout  = proc.StandardOutput.ReadToEnd();
        string stderr  = proc.StandardError.ReadToEnd();
        proc.WaitForExit();
        return (proc.ExitCode, stdout + stderr);
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private static string? GetInstanceId(string instanceName)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(InstanceNamesKey);
            return key?.GetValue(instanceName)?.ToString();
        }
        catch { return null; }
    }

    /// <summary>
    /// Extracts the SQL Server instance name from a server-spec string.
    /// "." / "localhost" / "SERVERNAME"    → "MSSQLSERVER"
    /// ".\SQLEXPRESS" / "SERVER\INSTANCE"  → "SQLEXPRESS" / "INSTANCE"
    /// </summary>
    private static string ExtractInstanceName(string serverSpec)
    {
        var idx = serverSpec.IndexOf('\\');
        if (idx < 0) return "MSSQLSERVER";

        var part = serverSpec[(idx + 1)..].Trim();
        return string.IsNullOrEmpty(part) ? "MSSQLSERVER" : part.ToUpperInvariant();
    }
}
