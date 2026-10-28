using System.ServiceProcess;
using Microsoft.Win32;

namespace BLogicDevTool;

// Services running from a deploy folder (e.g. Merchant Service) — Stop IIS doesn't release their DLLs.
public static class WindowsServiceLock
{
    /// <summary>Running services whose ImagePath is under any of <paramref name="folders"/>.</summary>
    public static List<string> FindRunningServicesIn(IEnumerable<string> folders)
    {
        var roots = folders
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Select(f => Path.GetFullPath(f).TrimEnd('\\') + "\\")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var result = new List<string>();
        if (roots.Count == 0) return result;

        foreach (var svc in ServiceController.GetServices())
        {
            using (svc)
            {
                try
                {
                    if (svc.Status is ServiceControllerStatus.Stopped or ServiceControllerStatus.StopPending)
                        continue;
                    var exe = GetExePath(svc.ServiceName);
                    if (exe != null && roots.Any(r => exe.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
                        result.Add(svc.ServiceName);
                }
                catch { /* inaccessible service — ignore */ }
            }
        }
        return result;
    }

    /// <summary>Stops each service (waits up to 60 s). Returns the ones actually stopped.</summary>
    public static List<string> Stop(IEnumerable<string> names, Action<string> log)
    {
        var stopped = new List<string>();
        foreach (var name in names)
        {
            try
            {
                using var svc = new ServiceController(name);
                log($"  ■ Stopping service '{svc.DisplayName}' ({name})...");
                svc.Stop();
                svc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(60));
                stopped.Add(name);
                log($"  ✔ Service '{name}' stopped.");
            }
            catch (Exception ex)
            {
                log($"  ⚠ Could not stop service '{name}': {ex.Message}");
            }
        }
        // Give the process a moment to fully exit and release file handles.
        if (stopped.Count > 0) Thread.Sleep(1000);
        return stopped;
    }

    /// <summary>Starts each service (waits up to 60 s).</summary>
    public static void Start(IEnumerable<string> names, Action<string> log)
    {
        foreach (var name in names)
        {
            try
            {
                using var svc = new ServiceController(name);
                log($"  ▶ Starting service '{svc.DisplayName}' ({name})...");
                svc.Start();
                svc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));
                log($"  ✔ Service '{name}' started.");
            }
            catch (Exception ex)
            {
                log($"  ⚠ Could not start service '{name}': {ex.Message}");
            }
        }
    }

    /// <summary>Extracts the full exe path from the service's registry ImagePath (quoted or not, with args).</summary>
    internal static string? GetExePath(string serviceName)
    {
        using var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}");
        var raw = key?.GetValue("ImagePath") as string;
        if (string.IsNullOrWhiteSpace(raw)) return null;
        raw = Environment.ExpandEnvironmentVariables(raw.Trim());

        string exe;
        if (raw.StartsWith('"'))
        {
            var end = raw.IndexOf('"', 1);
            exe = end > 1 ? raw.Substring(1, end - 1) : raw.Trim('"');
        }
        else
        {
            // Unquoted path with spaces: take the shortest prefix ending in ".exe".
            var idx = raw.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            exe = idx > 0 ? raw.Substring(0, idx + 4) : raw.Split(' ')[0];
        }

        if (exe.StartsWith(@"\??\")) exe = exe.Substring(4);
        try { return Path.GetFullPath(exe); } catch { return null; }
    }
}
