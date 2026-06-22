namespace BLogicDevTool;

public partial class IisServiceManagerForm : UserControl
{
    private SqlConnectionProfile? _profile;
    private List<IisAppEntry> _apps = new();
    private IisAppEntry? _selected;

    public IisServiceManagerForm(SqlConnectionProfile? profile)
    {
        _profile = profile ?? SqlSessionStore.Current;
        InitializeComponent();
        SqlSessionStore.ProfileChanged += OnSessionProfileChanged;
        Disposed += (_, _) => SqlSessionStore.ProfileChanged -= OnSessionProfileChanged;
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        UpdateLoginButtonText();
        await LoadDbSuggestionsAsync();
    }

    private void OnSessionProfileChanged()
    {
        if (IsDisposed) return;
        if (InvokeRequired) { BeginInvoke(OnSessionProfileChanged); return; }
        _profile = SqlSessionStore.Current;
        UpdateLoginButtonText();
        _ = LoadDbSuggestionsAsync();
    }

    private void UpdateLoginButtonText() =>
        btnLoginDb.Text = SqlSessionStore.Current == null ? "🔑 Login" : "🔄 Refresh";

    private async void btnLoginDb_Click(object? sender, EventArgs e)
    {
        if (SqlSessionStore.Current == null)
        {
            using var dlg = new SqlLoginForm();
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK || dlg.Profile == null) return;
            SqlSessionStore.SetProfile(dlg.Profile);
        }
        else
        {
            await LoadDbSuggestionsAsync();
        }
    }

    private async Task LoadDbSuggestionsAsync()
    {
        try
        {
            if (_profile == null) return;
            var helper = _profile.CreateHelper();
            var dbNames = await helper.GetDatabasesAsync();
            txtNewDb.BeginUpdate();
            txtNewDb.Items.Clear();
            txtNewDb.Items.AddRange(dbNames.Cast<object>().ToArray());
            txtNewDb.EndUpdate();
        }
        catch
        {
            // Keep IIS features usable even if SQL suggestions cannot be loaded.
        }
        finally
        {
            // Always load IIS data regardless of SQL availability
            btnRefresh.PerformClick();
        }
    }

    // ── Load / grid ───────────────────────────────────────────────────────────

    private async void btnRefresh_Click(object sender, EventArgs e)
    {
        SetBusy(true, "Loading IIS list...");
        try
        {
            _apps = await Task.Run(IisHelper.GetApplications);
            dgvApps.DataSource = null;
            dgvApps.DataSource = _apps.Select(a => new
            {
                Site = a.SiteName,
                App = a.AppPath,
                a.AppPoolName,
                PhysicalPath = a.PhysicalPath,
                Database = a.CurrentDatabase
            }).ToList();

            // column widths
            if (dgvApps.Columns.Count > 0)
            {
                dgvApps.Columns["Site"].Width = 100;
                dgvApps.Columns["App"].Width = 160;
                dgvApps.Columns["AppPoolName"].Width = 200;
                dgvApps.Columns["PhysicalPath"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvApps.Columns["Database"].Width = 0;
            }

            SetStatus("Loaded successfully. Select a row to view details.");

            // Save initial snapshots for entries that have never been recorded
            foreach (var app in _apps)
                IisLocalStore.SaveInitialSnapshot(app);
        }
        catch (Exception ex)
        {
            ShowError("Failed to load IIS list:\n" + ex.Message);
            SetStatus("Load failed.");
        }
        finally { SetBusy(false); }
    }

    private void dgvApps_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvApps.SelectedRows.Count == 0) return;
        var idx = dgvApps.SelectedRows[0].Index;
        if (idx < 0 || idx >= _apps.Count) return;

        _selected = _apps[idx];
        PopulateDetail(_selected);
    }

    private void PopulateDetail(IisAppEntry app)
    {
        lblDetailSite.Text = $"{app.SiteName}{app.AppPath}";
        txtDetailPath.Text = app.PhysicalPath;
        txtDetailDb.Text   = app.CurrentDatabase;

        // If DB is empty, suggest the currently selected item or keep previous text
        if (!string.IsNullOrWhiteSpace(app.CurrentDatabase))
            txtNewDb.Text = app.CurrentDatabase;
        // else leave txtNewDb as-is so user's selection is preserved

        txtNewPath.Text = "";

        // show saved snapshot info if any
        var snap = IisLocalStore.GetSnapshot(app.SiteName, app.AppPath);
        if (snap != null)
        {
            lblSnapshot.ForeColor = System.Drawing.Color.DarkBlue;
            lblSnapshot.Text =
                $"Snapshot saved at {snap.SavedAt:dd/MM/yyyy HH:mm} — DB: {snap.Database} | Path: {snap.PhysicalPath}";
        }
        else
        {
            lblSnapshot.ForeColor = System.Drawing.Color.DarkGray;
            lblSnapshot.Text = "No snapshot available.";
        }
    }

    // ── Browse new path ───────────────────────────────────────────────────────

    private void btnBrowseNewPath_Click(object sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Select new service path",
            SelectedPath = txtNewPath.Text.Trim().Length > 0
                ? txtNewPath.Text.Trim()
                : (_selected?.PhysicalPath ?? "")
        };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            txtNewPath.Text = dlg.SelectedPath;
            // Auto-read DB from new path's web.config
            var db = IisHelper.ReadDatabaseFromWebConfig(dlg.SelectedPath);
            if (!string.IsNullOrWhiteSpace(db))
                txtNewDb.Text = db;
        }
    }

    // ── Update / Restore ─────────────────────────────────────────────────────

    private async void btnChange_Click(object sender, EventArgs e)
    {
        if (_selected == null) { ShowWarn("Please select an application."); return; }

        var newDb = txtNewDb.Text.Trim();
        if (string.IsNullOrWhiteSpace(newDb))
        {
            ShowWarn("Please enter a new DB name.");
            return;
        }

        var confirmMsg =
            $"Update DB for: {_selected.SiteName}{_selected.AppPath}\n\n" +
            $"• New DB: {newDb}\n" +
            $"• Current path: {_selected.PhysicalPath}\n\n" +
            "Will backup current snapshot and restart IIS. Continue?";

        if (MessageBox.Show(confirmMsg, "Confirm Update DB",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        SetBusy(true, "Updating DB...");
        try
        {
            IisLocalStore.SaveSnapshot(_selected);

            await Task.Run(() =>
                IisHelper.SetDatabaseInWebConfig(_selected.PhysicalPath, newDb));

            SetStatus("Restarting IIS...");
            await IisHelper.RestartIisAsync();

            SetStatus($"✔ DB updated successfully. DB: {newDb}");
            MessageBox.Show("DB updated successfully!\nIIS has been restarted.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            await RefreshSelectedEntry();
        }
        catch (Exception ex)
        {
            ShowError("Error updating DB:\n" + ex.Message);
            SetStatus("Error.");
        }
        finally { SetBusy(false); refreshDgv(); }
    }

    private async void btnChangePath_Click(object sender, EventArgs e)
    {
        if (_selected == null) { ShowWarn("Please select an application."); return; }

        var newPath = txtNewPath.Text.Trim();
        if (string.IsNullOrWhiteSpace(newPath))
        {
            ShowWarn("Please select a new path first.");
            return;
        }

        if (newPath.Equals(_selected.PhysicalPath, StringComparison.OrdinalIgnoreCase))
        {
            ShowWarn("New path is the same as the current path.");
            return;
        }

        var confirmMsg =
            $"Update Patch for: {_selected.SiteName}{_selected.AppPath}\n\n" +
            $"• Old path: {_selected.PhysicalPath}\n" +
            $"• New path: {newPath}\n\n" +
            "Will backup current snapshot and restart IIS. Continue?";

        if (MessageBox.Show(confirmMsg, "Confirm Update Patch",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        SetBusy(true, "Updating Patch...");
        try
        {
            IisLocalStore.SaveSnapshot(_selected);

            await Task.Run(() =>
                IisHelper.SetPhysicalPath(_selected.SiteName, _selected.AppPath, newPath));

            SetStatus("Restarting IIS...");
            await IisHelper.RestartIisAsync();

            SetStatus($"✔ Patch updated successfully → {newPath}");
            MessageBox.Show("Patch updated successfully!\nIIS has been restarted.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            await RefreshSelectedEntry();
        }
        catch (Exception ex)
        {
            ShowError("Error updating Patch:\n" + ex.Message);
            SetStatus("Error.");
        }
        finally { SetBusy(false); refreshDgv(); }
    }

    private async void btnResetDb_Click(object sender, EventArgs e)
    {
        if (_selected == null) { ShowWarn("Please select an application."); return; }

        var snap = IisLocalStore.GetSnapshot(_selected.SiteName, _selected.AppPath);
        if (snap == null)
        {
            ShowWarn("No snapshot found to restore.");
            return;
        }

        var confirmMsg =
            $"Restore DB from snapshot saved at {snap.SavedAt:dd/MM/yyyy HH:mm}?\n\n" +
            $"• DB: {snap.Database}\n" +
            $"• Snapshot path: {snap.PhysicalPath}\n\n" +
            "Will restart IIS. Continue?";

        if (MessageBox.Show(confirmMsg, "Confirm Restore DB",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        SetBusy(true, "Restoring DB...");
        try
        {
            await Task.Run(() =>
                IisHelper.SetDatabaseInWebConfig(_selected.PhysicalPath, snap.Database));

            SetStatus("Restarting IIS...");
            await IisHelper.RestartIisAsync();

            SetStatus($"✔ DB restored successfully. DB: {snap.Database}");
            MessageBox.Show("DB restored successfully!\nIIS has been restarted.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            await RefreshSelectedEntry();
        }
        catch (Exception ex)
        {
            ShowError("Error restoring DB:\n" + ex.Message);
            SetStatus("Error.");
        }
        finally { SetBusy(false); refreshDgv(); }
    }

    private async void btnResetPath_Click(object sender, EventArgs e)
    {
        if (_selected == null) { ShowWarn("Please select an application."); return; }

        var defPath = IisDefaultPathStore.GetDefaultPath(_selected.SiteName, _selected.AppPath);
        if (string.IsNullOrWhiteSpace(defPath))
        {
            ShowWarn($"No default path defined for {_selected.SiteName}{_selected.AppPath}.");
            return;
        }

        static string Norm(string p) => p.TrimEnd('\\', '/');
        if (Norm(defPath).Equals(Norm(_selected.PhysicalPath), StringComparison.OrdinalIgnoreCase))
        {
            ShowWarn("Current path already matches the default.");
            return;
        }

        var confirmMsg =
            $"Reset path to DEFAULT for: {_selected.SiteName}{_selected.AppPath}\n\n" +
            $"• Current path: {_selected.PhysicalPath}\n" +
            $"• Default path: {defPath}\n\n" +
            "Will backup current snapshot and restart IIS. Continue?";

        if (MessageBox.Show(confirmMsg, "Confirm Reset Path",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        SetBusy(true, "Resetting path to default...");
        try
        {
            IisLocalStore.SaveSnapshot(_selected);

            await Task.Run(() =>
                IisHelper.SetPhysicalPath(_selected.SiteName, _selected.AppPath, defPath));

            SetStatus("Restarting IIS...");
            await IisHelper.RestartIisAsync();

            SetStatus($"✔ Path reset to default → {defPath}");
            MessageBox.Show("Path reset to default successfully!\nIIS has been restarted.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            await RefreshSelectedEntry();
        }
        catch (Exception ex)
        {
            ShowError("Error resetting path:\n" + ex.Message);
            SetStatus("Error.");
        }
        finally { SetBusy(false); refreshDgv(); }
    }

    private async void btnRestorePatch_Click(object sender, EventArgs e)
    {
        if (_selected == null) { ShowWarn("Please select an application."); return; }

        var snap = IisLocalStore.GetSnapshot(_selected.SiteName, _selected.AppPath);
        if (snap == null)
        {
            ShowWarn("No snapshot found to restore.");
            return;
        }

        if (snap.PhysicalPath.Equals(_selected.PhysicalPath, StringComparison.OrdinalIgnoreCase))
        {
            ShowWarn("Current path already matches the snapshot.");
            return;
        }

        var confirmMsg =
            $"Restore Patch from snapshot saved at {snap.SavedAt:dd/MM/yyyy HH:mm}?\n\n" +
            $"• Current path: {_selected.PhysicalPath}\n" +
            $"• Snapshot path: {snap.PhysicalPath}\n\n" +
            "Will restart IIS. Continue?";

        if (MessageBox.Show(confirmMsg, "Confirm Restore Patch",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        SetBusy(true, "Restoring Patch...");
        try
        {
            await Task.Run(() =>
                IisHelper.SetPhysicalPath(snap.SiteName, snap.AppPath, snap.PhysicalPath));

            SetStatus("Restarting IIS...");
            await IisHelper.RestartIisAsync();

            SetStatus($"✔ Patch restored successfully → {snap.PhysicalPath}");
            MessageBox.Show("Patch restored successfully!\nIIS has been restarted.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            await RefreshSelectedEntry();
        }
        catch (Exception ex)
        {
            ShowError("Error restoring Patch:\n" + ex.Message);
            SetStatus("Error.");
        }
        finally { SetBusy(false); refreshDgv(); }
    }

    private async void btnRestartIis_Click(object sender, EventArgs e)
    {
        var confirmMsg =
            "Restart IIS now?\n\n" +
            "All IIS applications on this machine will be temporarily affected.";

        if (MessageBox.Show(confirmMsg, "Confirm Restart IIS",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        SetBusy(true, "Restarting IIS...");
        try
        {
            await IisHelper.RestartIisAsync();
            SetStatus("✔ IIS restarted successfully.");
            MessageBox.Show("IIS restarted successfully.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            await RefreshSelectedEntry();
        }
        catch (Exception ex)
        {
            ShowError("Error restarting IIS:\n" + ex.Message);
            SetStatus("Error.");
        }
        finally { SetBusy(false); refreshDgv(); }
    }

    private void btnTroubleshoot_Click(object sender, EventArgs e)
    {
        using var dlg = new IisTroubleshootForm();
        dlg.ShowDialog(this.FindForm());
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task RefreshSelectedEntry()
    {
        _apps = await Task.Run(IisHelper.GetApplications);
        dgvApps.DataSource = null;
        dgvApps.DataSource = _apps.Select(a => new
        {
            Site = a.SiteName,
            App = a.AppPath,
            a.AppPoolName,
            PhysicalPath = a.PhysicalPath,
            Database = a.CurrentDatabase
        }).ToList();

        if (_selected != null)
        {
            var refreshed = _apps.FirstOrDefault(a =>
                a.SiteName == _selected.SiteName && a.AppPath == _selected.AppPath);
            if (refreshed != null)
            {
                _selected = refreshed;
                PopulateDetail(_selected);
            }
        }
    }

    private void SetBusy(bool busy, string? msg = null)
    {
        AppBusyState.IsBusy = busy;
        progressBar.Visible = busy;
        btnRefresh.Enabled = !busy;
        btnTroubleshoot.Enabled = !busy;
        btnChange.Enabled = !busy;
        btnResetDb.Enabled = !busy;
        btnChangePath.Enabled = !busy;
        btnResetPath.Enabled = !busy;
        btnRestorePatch.Enabled = !busy;
        btnRestartIis.Enabled = !busy;
        if (msg != null) SetStatus(msg);
    }

    private void SetStatus(string msg)
    {
        lblStatus.Text = msg;
        lblStatus.ForeColor = msg.StartsWith("✔")
            ? System.Drawing.Color.Green
            : msg.StartsWith("Error") || msg.StartsWith("✘")
                ? System.Drawing.Color.Red
                : System.Drawing.Color.DarkGray;
    }

    private void refreshDgv()
    {
        if (dgvApps.Columns.Count > 0)
        {
            dgvApps.Columns["Site"].Width = 160;
            dgvApps.Columns["App"].Width = 100;
            dgvApps.Columns["AppPoolName"].Width = 150;
            dgvApps.Columns["PhysicalPath"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvApps.Columns["Database"].Width = 200;
        }
    }

    private static void ShowError(string msg) =>
        MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

    private static void ShowWarn(string msg) =>
        MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
