using System.ServiceProcess;

namespace BLogicDevTool;

/// <summary>A BLogic Windows service whose exe.config holds connection strings (e.g. BLogicReportService).</summary>
public sealed class ServiceConfigEntry
{
    public string ServiceName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Status { get; set; } = "";
    public string ConfigPath { get; set; } = "";
    public List<ConnStringEntry> ConnStrings { get; set; } = new();
}

/// <summary>Finds BLogic Windows services outside IIS and restarts them after their config changes.</summary>
public static class WindowsServiceConfigs
{
    /// <summary>Services named BLogic* or installed under a BLogic folder that have a connection string. Never throws.</summary>
    public static List<ServiceConfigEntry> GetBLogicServices()
    {
        var list = new List<ServiceConfigEntry>();
        ServiceController[] all;
        try { all = ServiceController.GetServices(); }
        catch { return list; }

        foreach (var svc in all)
        {
            using (svc)
            {
                try
                {
                    var exe = WindowsServiceLock.GetExePath(svc.ServiceName);
                    var isBLogic = svc.ServiceName.Contains("BLogic", StringComparison.OrdinalIgnoreCase)
                                   || (exe?.Contains("BLogic", StringComparison.OrdinalIgnoreCase) ?? false);
                    if (!isBLogic || exe == null) continue;

                    var config = exe + ".config";
                    var entries = WebConfigConnStrings.Read(config)
                        .Where(c => c.Database.Length > 0 || c.DataSource.Length > 0)
                        .ToList();
                    if (entries.Count == 0) continue;

                    list.Add(new ServiceConfigEntry
                    {
                        ServiceName = svc.ServiceName,
                        DisplayName = svc.DisplayName,
                        Status = svc.Status.ToString(),
                        ConfigPath = config,
                        ConnStrings = entries
                    });
                }
                catch { /* inaccessible service — skip it */ }
            }
        }
        return list.OrderBy(s => s.ServiceName, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>Stops the service if needed, then starts it so it rereads its config. Throws on failure.</summary>
    public static void Restart(string serviceName)
    {
        using var svc = new ServiceController(serviceName);
        if (svc.StartType == ServiceStartMode.Disabled)
            throw new InvalidOperationException("the service is Disabled — enable it to pick up the new database.");

        svc.Refresh();
        if (svc.Status != ServiceControllerStatus.Stopped)
        {
            if (svc.Status != ServiceControllerStatus.StopPending) svc.Stop();
            svc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(60));
        }

        svc.Start();
        svc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));
    }
}
