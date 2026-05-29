namespace BLogicDevTool;

public partial class SqlServerManagerForm : UserControl
{
    private SqlServerHelper? _helper;
    private List<SqlLoginInfo> _logins = new();

    public SqlServerManagerForm()
    {
        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        LoadLocalInstances();
        rbConnWindows.Checked = true;
    }

    // ── Instance discovery ────────────────────────────────────────────────

    private void LoadLocalInstances()
    {
        var instances = SqlInstanceHelper.GetLocalInstances();

        cboConnInstance.Items.Clear();
        cboConnInstance.Items.AddRange(instances.ToArray());

        cboServerInstance.Items.Clear();
        cboServerInstance.Items.AddRange(instances.ToArray());

        cboNetInstance.Items.Clear();
        cboNetInstance.Items.AddRange(instances.ToArray());

        if (instances.Count > 0)
        {
            cboConnInstance.SelectedIndex   = 0;
            cboServerInstance.SelectedIndex = 0;
            cboNetInstance.SelectedIndex    = 0;
        }
    }

    // ── Tab 1 — Connect & Info ────────────────────────────────────────────

    private void RbConnWindows_CheckedChanged(object? sender, EventArgs e)
    {
        bool sql = rbConnSql.Checked;
        lblConnUser.Enabled = sql;
        txtConnUser.Enabled = sql;
        lblConnPass.Enabled = sql;
        txtConnPass.Enabled = sql;
    }

    private async void BtnTestConnect_Click(object? sender, EventArgs e)
    {
        var server = cboConnInstance.Text.Trim();
        if (string.IsNullOrWhiteSpace(server))
        {
            SetStatus("Enter or select a server name.", Color.DarkOrange);
            return;
        }

        SetBusy(true, "Connecting…");
        try
        {
            bool   winAuth = rbConnWindows.Checked;
            string user    = txtConnUser.Text.Trim();
            string pass    = txtConnPass.Text;

            var helper = new SqlServerHelper(server, winAuth, user, pass);
            await helper.TestConnectionAsync();
            var info = await helper.GetServerInfoAsync();

            _helper = helper;

            lblInfoServerVal.Text   = info.ServerName;
            lblInfoVersionVal.Text  = info.Version;
            lblInfoEditionVal.Text  = info.Edition;
            lblInfoMachineVal.Text  = info.MachineName;

            bool isMixed = info.IsMixedMode;
            lblInfoAuthVal.Text      = isMixed ? "Mixed Mode (SQL + Windows)" : "Windows Authentication Only";
            lblInfoAuthVal.ForeColor = isMixed ? Color.Green : Color.DarkOrange;

            SetStatus($"✔ Connected to {server}  |  {info.Edition}", Color.Green);
        }
        catch (Exception ex)
        {
            _helper = null;
            ClearServerInfo();
            SetStatus($"✘ {ex.Message}", Color.Red);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ClearServerInfo()
    {
        lblInfoServerVal.Text    = "";
        lblInfoVersionVal.Text   = "";
        lblInfoEditionVal.Text   = "";
        lblInfoMachineVal.Text   = "";
        lblInfoAuthVal.Text      = "";
        lblInfoAuthVal.ForeColor = SystemColors.ControlText;
    }

    // ── Tab 2 — Login Accounts ────────────────────────────────────────────

    private void ChkShowLoginPass_CheckedChanged(object? sender, EventArgs e)
        => txtNewLoginPass.UseSystemPasswordChar = !chkShowLoginPass.Checked;

    private async void BtnRefreshLogins_Click(object? sender, EventArgs e)
    {
        if (_helper == null) { ShowMsg("Please connect in the \"Connect\" tab first."); return; }

        SetBusy(true, "Loading logins…");
        try
        {
            _logins = await _helper.GetLoginsAsync();
            RefreshLoginGrid();
        }
        catch (Exception ex) { ShowMsg($"Failed to load logins:\n{ex.Message}"); }
        finally { SetBusy(false); }
    }

    private void RefreshLoginGrid()
    {
        dgvLogins.DataSource = null;
        dgvLogins.DataSource = _logins.Select(l => new
        {
            l.Name,
            Type     = l.Type.Replace("_", " "),
            Enabled  = !l.IsDisabled ? "✔" : "✘",
            SysAdmin = l.IsSysAdmin ? "✔" : "",
            Created  = l.CreateDate.ToString("yyyy-MM-dd")
        }).ToList();

        if (dgvLogins.Columns.Count > 0)
        {
            dgvLogins.Columns["Name"].Width     = 200;
            dgvLogins.Columns["Type"].Width     = 160;
            dgvLogins.Columns["Enabled"].Width  = 70;
            dgvLogins.Columns["SysAdmin"].Width = 80;
            dgvLogins.Columns["Created"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        SetStatus($"Loaded {_logins.Count} login(s).", Color.DarkGray);
    }

    private void DgvLogins_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvLogins.SelectedRows.Count == 0) return;
        var idx = dgvLogins.SelectedRows[0].Index;
        if (idx < 0 || idx >= _logins.Count) return;

        var login = _logins[idx];
        txtNewLoginName.Text = login.Name;
        btnToggleLogin.Text  = login.IsDisabled ? "✔ Enable" : "✘ Disable";
    }

    private async void BtnToggleLogin_Click(object? sender, EventArgs e)
    {
        if (_helper == null || dgvLogins.SelectedRows.Count == 0) return;

        var idx   = dgvLogins.SelectedRows[0].Index;
        if (idx < 0 || idx >= _logins.Count) return;

        var login  = _logins[idx];
        bool enable = login.IsDisabled;

        SetBusy(true, (enable ? "Enabling" : "Disabling") + " login…");
        try
        {
            await _helper.SetLoginEnabledAsync(login.Name, enable);
            Log($"✔ Login '{login.Name}' {(enable ? "enabled" : "disabled")}.");
            await RefreshLoginsAsync();
        }
        catch (Exception ex) { ShowMsg($"Failed:\n{ex.Message}"); }
        finally { SetBusy(false); }
    }

    private async void BtnDeleteLogin_Click(object? sender, EventArgs e)
    {
        if (_helper == null || dgvLogins.SelectedRows.Count == 0) return;

        var idx = dgvLogins.SelectedRows[0].Index;
        if (idx < 0 || idx >= _logins.Count) return;

        var loginName = _logins[idx].Name;

        if (MessageBox.Show(
                $"Delete login '{loginName}'?\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        SetBusy(true, "Deleting login…");
        try
        {
            await _helper.DropLoginAsync(loginName);
            Log($"✔ Login '{loginName}' deleted.");
            await RefreshLoginsAsync();
        }
        catch (Exception ex) { ShowMsg($"Failed:\n{ex.Message}"); }
        finally { SetBusy(false); }
    }

    private async void BtnCreateLogin_Click(object? sender, EventArgs e)
    {
        if (_helper == null) { ShowMsg("Please connect first."); return; }

        var name = txtNewLoginName.Text.Trim();
        var pass = txtNewLoginPass.Text;

        if (string.IsNullOrWhiteSpace(name)) { ShowMsg("Enter a login name."); return; }
        if (string.IsNullOrWhiteSpace(pass)) { ShowMsg("Enter a password.");   return; }

        SetBusy(true, "Creating login…");
        try
        {
            await _helper.CreateLoginAsync(name, pass, chkNewSysAdmin.Checked);
            Log($"✔ Login '{name}' created.{(chkNewSysAdmin.Checked ? " (sysadmin)" : "")}");
            txtNewLoginName.Clear();
            txtNewLoginPass.Clear();
            chkNewSysAdmin.Checked = false;
            await RefreshLoginsAsync();
        }
        catch (Exception ex) { Log($"✘ Create failed: {ex.Message}"); }
        finally { SetBusy(false); }
    }

    private async void BtnChangeLoginPass_Click(object? sender, EventArgs e)
    {
        if (_helper == null) { ShowMsg("Please connect first."); return; }

        var name = txtNewLoginName.Text.Trim();
        var pass = txtNewLoginPass.Text;

        if (string.IsNullOrWhiteSpace(name)) { ShowMsg("Enter the login name."); return; }
        if (string.IsNullOrWhiteSpace(pass)) { ShowMsg("Enter the new password."); return; }

        SetBusy(true, "Changing password…");
        try
        {
            await _helper.SetLoginPasswordAsync(name, pass);
            Log($"✔ Password for '{name}' changed.");
        }
        catch (Exception ex) { Log($"✘ {ex.Message}"); }
        finally { SetBusy(false); }
    }

    /// <summary>
    /// Configure: set password + enable + optionally grant sysadmin.
    /// Works for ANY login (including sa).
    /// </summary>
    private async void BtnConfigureLogin_Click(object? sender, EventArgs e)
    {
        if (_helper == null) { ShowMsg("Please connect first."); return; }

        var name = txtNewLoginName.Text.Trim();
        var pass = txtNewLoginPass.Text;

        if (string.IsNullOrWhiteSpace(name)) { ShowMsg("Enter the login name."); return; }
        if (string.IsNullOrWhiteSpace(pass)) { ShowMsg("Enter a password.");     return; }

        bool grantSysAdmin = chkNewSysAdmin.Checked;
        Log($"▶ Configuring '{name}' (password + enable{(grantSysAdmin ? " + sysadmin" : "")})…");
        SetBusy(true, $"Configuring '{name}'…");
        try
        {
            if (string.Equals(name, "sa", StringComparison.OrdinalIgnoreCase))
            {
                await _helper.ConfigureSaAccountAsync(pass);
                Log($"✔ SA: password set, enabled, sysadmin assigned.");
            }
            else
            {
                await _helper.SetLoginPasswordAsync(name, pass);
                await _helper.SetLoginEnabledAsync(name, true);
                if (grantSysAdmin)
                {
                    await _helper.GrantSysAdminAsync(name);
                    Log($"✔ '{name}': password set, enabled, sysadmin granted.");
                }
                else
                {
                    Log($"✔ '{name}': password set, enabled.");
                }
            }
            await RefreshLoginsAsync();
        }
        catch (Exception ex) { Log($"✘ {ex.Message}"); }
        finally { SetBusy(false); }
    }

    /// <summary>Test SQL login using credentials entered in the edit panel.</summary>
    private async void BtnTestLogin_Click(object? sender, EventArgs e)
    {
        var server = cboServerInstance.Text.Trim();
        if (string.IsNullOrEmpty(server)) server = ".";

        var name = txtNewLoginName.Text.Trim();
        var pass = txtNewLoginPass.Text;

        if (string.IsNullOrWhiteSpace(name)) { ShowMsg("Enter the login name."); return; }
        if (string.IsNullOrWhiteSpace(pass)) { ShowMsg("Enter the password.");   return; }

        Log($"▶ Testing login '{name}' on '{server}'…");
        SetBusy(true, $"Testing '{name}'…");
        try
        {
            var testHelper = new SqlServerHelper(server, false, name, pass);
            var info       = await testHelper.GetServerInfoAsync();
            Log($"✔ Login '{name}' OK!  Server: {info.ServerName}  v{info.Version}");
        }
        catch (Exception ex) { Log($"✘ '{name}' failed: {ex.Message}"); }
        finally { SetBusy(false); }
    }

    private async void BtnEnableMixedMode_Click(object? sender, EventArgs e)
    {
        var instance = cboServerInstance.Text.Trim();
        Log($"▶ Enabling Mixed Mode for '{(string.IsNullOrEmpty(instance) ? "." : instance)}'…");
        SetBusy(true, "Writing registry…");
        try
        {
            await Task.Run(() => SqlInstanceHelper.EnableMixedMode(instance));
            Log("✔ Mixed Mode enabled in registry. Restart the SQL service for changes to take effect.");
        }
        catch (Exception ex) { Log($"✘ {ex.Message}"); }
        finally { SetBusy(false); }
    }

    private async void BtnRestartSqlService_Click(object? sender, EventArgs e)
    {
        var instance    = cboServerInstance.Text.Trim();
        var serviceName = SqlInstanceHelper.GetServiceName(instance);
        Log($"▶ Restarting service [{serviceName}]…");
        SetBusy(true, "Restarting SQL service…");
        try
        {
            await Task.Run(() => SqlInstanceHelper.RestartSqlService(instance));
            Log($"✔ Service [{serviceName}] restarted successfully.");
        }
        catch (Exception ex) { Log($"✘ {ex.Message}"); }
        finally { SetBusy(false); }
    }

    private void BtnClearLog_Click(object? sender, EventArgs e)
        => txtLog.Clear();

    // ── Tab 3 — Network / TCP-IP ──────────────────────────────────────────

    private void BtnCheckNetStatus_Click(object? sender, EventArgs e)
    {
        var instance = cboNetInstance.Text.Trim();
        if (string.IsNullOrEmpty(instance)) instance = ".";

        bool   tcpEnabled = SqlInstanceHelper.GetTcpEnabled(instance);
        string port       = SqlInstanceHelper.GetTcpPort(instance);

        lblNetTcpStatus.Text      = tcpEnabled ? "✔  TCP/IP Enabled" : "✘  TCP/IP Disabled";
        lblNetTcpStatus.ForeColor = tcpEnabled ? Color.Green : Color.Red;

        lblNetPortStatus.Text      = string.IsNullOrEmpty(port)
            ? "Dynamic port (no static port set)"
            : $"Static port: {port}";
        lblNetPortStatus.ForeColor = string.IsNullOrEmpty(port) ? Color.DarkOrange : Color.DarkBlue;

        SetStatus("Status checked.", Color.DarkGray);
    }

    private async void BtnEnableTcpIp_Click(object? sender, EventArgs e)
    {
        var instance = cboNetInstance.Text.Trim();
        if (string.IsNullOrEmpty(instance)) instance = ".";

        if (!int.TryParse(txtNetPort.Text.Trim(), out int port) || port <= 0 || port > 65535)
        {
            ShowMsg("Enter a valid TCP port number (e.g. 1433).");
            return;
        }

        SetBusy(true, "Enabling TCP/IP in registry…");
        try
        {
            await Task.Run(() => SqlInstanceHelper.EnableTcpIp(instance, port));
            Log($"✔ TCP/IP enabled on port {port} for '{instance}'.");
            Log("   → Restart the SQL service for changes to take effect.");
            BtnCheckNetStatus_Click(sender, e);
        }
        catch (Exception ex) { Log($"✘ {ex.Message}"); }
        finally { SetBusy(false); }
    }

    private async void BtnDisableTcpIp_Click(object? sender, EventArgs e)
    {
        var instance = cboNetInstance.Text.Trim();
        if (string.IsNullOrEmpty(instance)) instance = ".";

        if (MessageBox.Show(
                "Disable TCP/IP for this instance?\nRemote connections will no longer be possible.",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        SetBusy(true, "Disabling TCP/IP…");
        try
        {
            await Task.Run(() => SqlInstanceHelper.DisableTcpIp(instance));
            Log($"✔ TCP/IP disabled for '{instance}'.");
            Log("   → Restart the SQL service for changes to take effect.");
            BtnCheckNetStatus_Click(sender, e);
        }
        catch (Exception ex) { Log($"✘ {ex.Message}"); }
        finally { SetBusy(false); }
    }

    private async void BtnAddFirewallRule_Click(object? sender, EventArgs e)
    {
        if (!int.TryParse(txtNetPort.Text.Trim(), out int port) || port <= 0 || port > 65535)
        {
            ShowMsg("Enter a valid TCP port number first.");
            return;
        }

        string ruleName = $"SQL Server TCP {port}";
        Log($"▶ Adding firewall rule '{ruleName}' (port {port})…");
        SetBusy(true, "Adding firewall rule…");
        try
        {
            await Task.Run(() => SqlInstanceHelper.AddFirewallRule(port, ruleName));
            Log($"✔ Firewall rule '{ruleName}' added.");
            Log($"   Machines on the network can now connect on port {port}.");
        }
        catch (Exception ex) { Log($"✘ {ex.Message}"); }
        finally { SetBusy(false); }
    }

    private async void BtnNetRestartService_Click(object? sender, EventArgs e)
    {
        var instance    = cboNetInstance.Text.Trim();
        var serviceName = SqlInstanceHelper.GetServiceName(instance);
        Log($"▶ Restarting service [{serviceName}]…");
        SetBusy(true, "Restarting SQL service…");
        try
        {
            await Task.Run(() => SqlInstanceHelper.RestartSqlService(instance));
            Log($"✔ Service [{serviceName}] restarted.");
            BtnCheckNetStatus_Click(sender, e);
        }
        catch (Exception ex) { Log($"✘ {ex.Message}"); }
        finally { SetBusy(false); }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private async Task RefreshLoginsAsync()
    {
        if (_helper == null) return;
        _logins = await _helper.GetLoginsAsync();
        RefreshLoginGrid();
    }

    private void Log(string message)
    {
        if (txtLog.InvokeRequired)
            txtLog.Invoke(() => Log(message));
        else
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            txtLog.ScrollToCaret();
        }
    }

    private void SetBusy(bool busy, string? message = null)
    {
        pbStatus.Style   = busy ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
        pbStatus.Visible = busy;
        if (message != null)
            SetStatus(message, Color.DarkGray);
    }

    private void SetStatus(string text, Color color)
    {
        lblStatus.Text      = text;
        lblStatus.ForeColor = color;
    }

    private static void ShowMsg(string msg)
        => MessageBox.Show(msg, "SQL Server Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
