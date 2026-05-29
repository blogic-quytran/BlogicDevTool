namespace BLogicDevTool;

/// <summary>
/// Modal dialog for IIS W3SVC troubleshooting.
/// Implements the same diagnostic steps as fix_iis_w3svc.bat.
/// </summary>
public partial class IisTroubleshootForm : Form
{
    public IisTroubleshootForm()
    {
        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Log("IIS W3SVC Troubleshoot ready.");
        Log("Click a step button or 'Run All Steps' to begin.");
        Log("─────────────────────────────────────────────────");
    }

    // ── Individual steps ──────────────────────────────────────────────────

    private async void BtnStep1_Click(object? sender, EventArgs e)
        => await RunStepAsync("Step 1 — Check Services", IisHelper.TroubleStep1_CheckServicesAsync);

    private async void BtnStep2_Click(object? sender, EventArgs e)
        => await RunStepAsync("Step 2 — Fix Startup Types + Start Dependencies",
            IisHelper.TroubleStep2_FixStartupAsync);

    private async void BtnStep3_Click(object? sender, EventArgs e)
        => await RunStepAsync("Step 3 — Backup IIS Config", IisHelper.TroubleStep3_BackupConfigAsync);

    private async void BtnStep4_Click(object? sender, EventArgs e)
        => await RunStepAsync("Step 4 — Validate IIS Config", IisHelper.TroubleStep4_ValidateConfigAsync);

    private async void BtnStep5_Click(object? sender, EventArgs e)
        => await RunStepAsync("Step 5 — Check Ports 80 / 443", IisHelper.TroubleStep5_CheckPortsAsync);

    private async void BtnStep6_Click(object? sender, EventArgs e)
        => await RunStepAsync("Step 6 — Start W3SVC", IisHelper.TroubleStep6_StartW3SvcAsync);

    private void BtnStep7_Click(object? sender, EventArgs e)
    {
        LogSection("Step 7 — Extra Diagnostics");
        Log(IisHelper.TroubleStep7_ExtraDiagnostics());
        SetStatus("Step 7 done.", Color.DarkGray);
    }

    private async void BtnStep8_Click(object? sender, EventArgs e)
        => await RunStepAsync("Step 8 — Final State", IisHelper.TroubleStep8_FinalStateAsync);

    // ── Run all ───────────────────────────────────────────────────────────

    private async void BtnRunAll_Click(object? sender, EventArgs e)
    {
        SetBusy(true, "Running full troubleshoot sequence...");
        txtLog.Clear();
        Log("╔══════════════════════════════════════════════╗");
        Log("  IIS W3SVC FULL TROUBLESHOOT");
        Log($"  {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        Log("╚══════════════════════════════════════════════╝");

        await RunStepCoreAsync("Step 1 — Check Services",
            IisHelper.TroubleStep1_CheckServicesAsync);
        await RunStepCoreAsync("Step 2 — Fix Startup Types + Start Dependencies",
            IisHelper.TroubleStep2_FixStartupAsync);
        await RunStepCoreAsync("Step 3 — Backup IIS Config",
            IisHelper.TroubleStep3_BackupConfigAsync);
        await RunStepCoreAsync("Step 4 — Validate IIS Config",
            IisHelper.TroubleStep4_ValidateConfigAsync);
        await RunStepCoreAsync("Step 5 — Check Ports 80 / 443",
            IisHelper.TroubleStep5_CheckPortsAsync);
        await RunStepCoreAsync("Step 6 — Start W3SVC",
            IisHelper.TroubleStep6_StartW3SvcAsync);

        LogSection("Step 7 — Extra Diagnostics");
        Log(IisHelper.TroubleStep7_ExtraDiagnostics());

        await RunStepCoreAsync("Step 8 — Final State",
            IisHelper.TroubleStep8_FinalStateAsync);

        Log("═══════════════════════════════════════════════");
        Log("✔ Full troubleshoot sequence completed.");
        SetBusy(false);
        SetStatus("✔ Completed.", Color.Green);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private async Task RunStepAsync(string title, Func<Task<string>> action)
    {
        SetBusy(true, $"{title}...");
        await RunStepCoreAsync(title, action);
        SetBusy(false);
        SetStatus($"Done: {title}", Color.DarkGray);
    }

    private async Task RunStepCoreAsync(string title, Func<Task<string>> action)
    {
        LogSection(title);
        try
        {
            var result = await action();
            Log(result);
        }
        catch (Exception ex)
        {
            Log($"[ERROR] {ex.Message}");
        }
    }

    private void LogSection(string title)
    {
        Log($"┌─────────────────────────────────────────────");
        Log($"│ {title}");
        Log($"└─────────────────────────────────────────────");
    }

    private void Log(string text)
    {
        if (txtLog.InvokeRequired)
            txtLog.Invoke(() => Log(text));
        else
        {
            if (!string.IsNullOrEmpty(text))
            {
                var ts = DateTime.Now.ToString("HH:mm:ss");
                foreach (var line in text.Split('\n'))
                {
                    var l = line.TrimEnd('\r');
                    txtLog.AppendText($"[{ts}] {l}{Environment.NewLine}");
                }
            }
            txtLog.ScrollToCaret();
        }
    }

    private void BtnClearLog_Click(object? sender, EventArgs e)
        => txtLog.Clear();

    private void SetBusy(bool busy, string? msg = null)
    {
        progressBar.Style   = busy ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
        progressBar.Visible = busy;

        btnStep1.Enabled  = !busy;
        btnStep2.Enabled  = !busy;
        btnStep3.Enabled  = !busy;
        btnStep4.Enabled  = !busy;
        btnStep5.Enabled  = !busy;
        btnStep6.Enabled  = !busy;
        btnStep7.Enabled  = !busy;
        btnStep8.Enabled  = !busy;
        btnRunAll.Enabled = !busy;

        if (msg != null) SetStatus(msg, Color.DarkGray);
    }

    private void SetStatus(string msg, Color color)
    {
        lblStatus.Text      = msg;
        lblStatus.ForeColor = color;
    }
}
