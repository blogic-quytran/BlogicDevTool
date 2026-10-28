namespace BLogicDevTool;

/// <summary>Points IIS apps and BLogic Windows services at other databases, restarts them,
/// and resets them to their original databases.</summary>
public partial class IisEnvironmentSwitchForm : Form
{
    private const string ServiceHost = "Windows Service";

    /// <summary>One grid line: a single connection string of an IIS app or a Windows service.</summary>
    private sealed class TargetRow
    {
        public string Host = "";          // IIS site name, or ServiceHost
        public string Name = "";          // IIS app path, or service name
        public string? ConfigPath;
        public string? ServiceName;       // set for Windows services only
        public string ServiceStatus = "";
        public ConnStringEntry Entry = null!;

        /// <summary>Pre-switch values; set only while the row is away from them.</summary>
        public IisEnvOriginal? Original;

        public bool IsService => ServiceName != null;
        public string Display => IsService ? ServiceName! : Host + Name;
        public string Key => IisEnvOriginalStore.Key(Host, Name, Entry.Name);
        public string Label => $"{Display} [{Entry.Label}]";
    }

    private static readonly Font BannerFont = new("Consolas", 16F, FontStyle.Bold);
    private static readonly Color PendingBack = Color.FromArgb(255, 246, 200);
    private static readonly Color SwitchedFore = Color.FromArgb(200, 100, 0);
    private static readonly Color ServiceFore = Color.SteelBlue;

    private SqlConnectionProfile? _profile;
    private readonly List<TargetRow> _rows = new();
    private List<string> _dbNames = new();
    private RichLogBuffer? _log;
    private bool _busy;
    private bool _binding;
    private Font? _boldFont;

    /// <summary>One-line outcome for the caller's status bar; null when nothing was written.</summary>
    public string? ResultSummary { get; private set; }

    public IisEnvironmentSwitchForm(SqlConnectionProfile? profile)
    {
        _profile = profile ?? SqlSessionStore.Current;
        InitializeComponent();

        _log = new RichLogBuffer(rtbLog, ColorOf);
        colNewDb.Items.Add("");

        // A combo cell only commits on focus loss, which would leave the row unstyled until then.
        dgvTargets.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (dgvTargets.IsCurrentCellDirty)
                dgvTargets.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        dgvTargets.CellValueChanged += dgvTargets_CellValueChanged;
        dgvTargets.CellClick += dgvTargets_CellClick;
        dgvTargets.DataError += (_, e) => e.ThrowException = false;

        SqlSessionStore.ProfileChanged += OnSessionProfileChanged;
        Disposed += (_, _) => SqlSessionStore.ProfileChanged -= OnSessionProfileChanged;
    }

    private static Color ColorOf(string line)
    {
        if (line.StartsWith("✔")) return Color.FromArgb(106, 200, 106);
        if (line.StartsWith("✘") || line.StartsWith("[ERROR]")) return Color.FromArgb(240, 100, 100);
        if (line.StartsWith("⚠")) return Color.FromArgb(230, 180, 80);
        if (line.StartsWith("──")) return Color.FromArgb(120, 170, 230);
        return Color.Gainsboro;
    }

    private void Log(string line) => _log?.Append(line + Environment.NewLine);

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        chkSynonym_CheckedChanged(this, EventArgs.Empty);
        UpdateSqlHeader();
        UpdateButtons();
        if (_profile == null) return;   // OnShown asks for the login first
        await LoadDbNamesAsync();
        await ReloadTargetsAsync();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (_profile != null) return;

        // The caller asks first; this covers any other way the form gets opened.
        using (var dlg = new SqlLoginForm())
        {
            if (dlg.ShowDialog(this) != DialogResult.OK || dlg.Profile == null)
            {
                ShowWarn("Switch Environment needs a SQL Server login to list the databases.");
                Close();
                return;
            }
            SqlSessionStore.SetProfile(dlg.Profile);   // ProfileChanged loads the DB names
        }
        await ReloadTargetsAsync();
    }

    // ── SQL session ───────────────────────────────────────────────────────────

    private void OnSessionProfileChanged()
    {
        if (IsDisposed) return;
        if (InvokeRequired) { BeginInvoke(OnSessionProfileChanged); return; }
        _profile = SqlSessionStore.Current;
        _ = LoadDbNamesAsync();
    }

    private void UpdateSqlHeader()
    {
        btnLoginDb.Text = SqlSessionStore.Current == null ? "🔑  SQL Login" : "🔄  Refresh DBs";
        if (_profile == null)
        {
            lblSql.Text = "⚠ Not logged in to SQL Server";
            lblSql.ForeColor = Color.Firebrick;
        }
        else
        {
            lblSql.Text = $"SQL: {_profile.Server}  —  {_dbNames.Count} database(s)";
            lblSql.ForeColor = Color.FromArgb(0, 110, 60);
        }
    }

    private async void btnLoginDb_Click(object? sender, EventArgs e)
    {
        if (SqlSessionStore.Current == null)
        {
            using var dlg = new SqlLoginForm();
            if (dlg.ShowDialog(this) != DialogResult.OK || dlg.Profile == null) return;
            SqlSessionStore.SetProfile(dlg.Profile);
            return;
        }
        await LoadDbNamesAsync();
    }

    /// <summary>Fills the New DB dropdowns, keeping the choices already made.</summary>
    private async Task LoadDbNamesAsync()
    {
        if (_profile == null)
        {
            UpdateSqlHeader();
            return;
        }

        try
        {
            _dbNames = await _profile.CreateHelper().GetDatabasesAsync();
            Log($"Loaded {_dbNames.Count} database name(s) from {_profile.Server}.");
        }
        catch (Exception ex)
        {
            _dbNames = new List<string>();
            Log("⚠ Could not list databases: " + ex.Message);
        }

        var pending = CurrentChoices();
        dgvTargets.EndEdit();
        colNewDb.Items.Clear();
        colNewDb.Items.Add("");
        colNewDb.Items.AddRange(_dbNames.Cast<object>().ToArray());

        var realDb = cboRealDb.Text;
        cboRealDb.BeginUpdate();
        cboRealDb.Items.Clear();
        cboRealDb.Items.AddRange(_dbNames.Cast<object>().ToArray());
        cboRealDb.EndUpdate();
        cboRealDb.Text = realDb;

        BindRows(pending);
        UpdateSqlHeader();
    }

    // ── Grid ──────────────────────────────────────────────────────────────────

    private async void btnReload_Click(object? sender, EventArgs e) => await ReloadTargetsAsync();

    private async Task ReloadTargetsAsync()
    {
        SetBusy(true, "Reading IIS applications and Windows services...");
        try
        {
            var apps = await Task.Run(IisHelper.GetApplications);
            var services = await Task.Run(WindowsServiceConfigs.GetBLogicServices);
            var originals = IisEnvOriginalStore.LoadAll();
            var settled = new List<string>();

            void Add(TargetRow row)
            {
                if (originals.TryGetValue(row.Key, out var orig))
                {
                    // Back on its original (by hand or a failed write) — nothing left to reset.
                    if (Same(orig.Database, row.Entry.Database) && Same(orig.DataSource, row.Entry.DataSource))
                        settled.Add(row.Key);
                    else
                        row.Original = orig;
                }
                _rows.Add(row);
            }

            _rows.Clear();
            foreach (var app in apps)
                foreach (var entry in app.ConnStrings.Where(c => c.Database.Length > 0 || c.DataSource.Length > 0))
                    Add(new TargetRow
                    {
                        Host = app.SiteName, Name = app.AppPath, ConfigPath = app.ConfigPath, Entry = entry
                    });

            foreach (var svc in services)
                foreach (var entry in svc.ConnStrings)
                    Add(new TargetRow
                    {
                        Host = ServiceHost, Name = svc.ServiceName, ConfigPath = svc.ConfigPath,
                        ServiceName = svc.ServiceName, ServiceStatus = svc.Status, Entry = entry
                    });

            if (settled.Count > 0) IisEnvOriginalStore.Remove(settled);
            BindRows(null);

            var switched = _rows.Count(r => r.Original != null);
            Log($"── Loaded {_rows.Count} connection string(s) from {apps.Count} IIS application(s) " +
                $"and {services.Count} Windows service(s)" +
                (switched > 0 ? $"; {switched} switched away from the original." : "."));
            foreach (var svc in services.Where(s => s.Status != "Running"))
                Log($"⚠ Service {svc.ServiceName} is {svc.Status}.");
            SetStatus(switched > 0
                ? $"{switched} service connection(s) are switched — Reset to default puts them back."
                : "Pick a New DB, then Apply.");
        }
        catch (Exception ex)
        {
            Log("✘ Failed to read IIS: " + ex.Message);
            ShowError("Failed to read IIS applications:\n" + ex.Message);
        }
        finally { SetBusy(false); }
    }

    private void BindRows(IReadOnlyDictionary<string, string>? choices)
    {
        dgvTargets.EndEdit();
        _binding = true;
        try
        {
            dgvTargets.Rows.Clear();
            foreach (var row in _rows)
            {
                var chosen = choices != null && choices.TryGetValue(row.Key, out var c) && colNewDb.Items.Contains(c)
                    ? c : "";

                var name = row.IsService && row.ServiceStatus != "Running"
                    ? $"{row.Name}  ({row.ServiceStatus})"
                    : row.Name;

                var i = dgvTargets.Rows.Add(
                    row.Host,
                    name,
                    row.Entry.Label,
                    row.Entry.DataSource,
                    row.Entry.Database,
                    chosen,
                    row.Original?.Database ?? "");

                var grid = dgvTargets.Rows[i];
                grid.Tag = row;
                grid.Cells[colApp.Index].ToolTipText = row.ConfigPath ?? "(no Web.config)";
                if (row.IsService)
                {
                    grid.Cells[colSite.Index].Style.ForeColor = ServiceFore;
                    grid.Cells[colApp.Index].Style.ForeColor = ServiceFore;
                }

                if (row.Original != null)
                {
                    grid.Cells[colCurDb.Index].Style.ForeColor = SwitchedFore;
                    grid.Cells[colOrigDb.Index].Style.ForeColor = SwitchedFore;
                    grid.Cells[colOrigDb.Index].ToolTipText =
                        $"Switched — original {row.Original.DataSource} / {row.Original.Database}, " +
                        $"recorded {row.Original.SavedAt:dd/MM/yyyy HH:mm}";
                }
                StyleRow(grid);
            }
        }
        finally { _binding = false; }
        UpdateButtons();
    }

    private void dgvTargets_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (_binding || e.RowIndex < 0 || e.ColumnIndex != colNewDb.Index) return;
        var grid = dgvTargets.Rows[e.RowIndex];
        StyleRow(grid);
        if (grid.Tag is TargetRow row) FollowSameDatabase(row, ChoiceOf(grid));
        UpdateButtons();
    }

    /// <summary>IIS apps and services on one environment share a database, so one pick fills the
    /// still-empty rows on that same server/database; each can still be changed or cleared.</summary>
    private void FollowSameDatabase(TargetRow source, string choice)
    {
        if (!IsPending(source, choice)) return;

        var filled = new List<string>();
        _binding = true;
        try
        {
            foreach (DataGridViewRow grid in dgvTargets.Rows)
            {
                if (grid.Tag is not TargetRow row || row == source || ChoiceOf(grid).Length > 0) continue;
                if (!Same(row.Entry.Database, source.Entry.Database) ||
                    !Same(row.Entry.DataSource, source.Entry.DataSource)) continue;

                grid.Cells[colNewDb.Index].Value = choice;
                StyleRow(grid);
                filled.Add(row.Label);
            }
        }
        finally { _binding = false; }

        if (filled.Count == 0) return;
        Log($"Also picked [{choice}] for {filled.Count} other connection(s) on [{source.Entry.Database}]:");
        foreach (var label in filled) Log("   • " + label);
        SetStatus($"{filled.Count} other connection(s) on [{source.Entry.Database}] follow — clear any you want to keep.");
    }

    // One click opens the list; otherwise EditOnEnter still needs a second click on the arrow.
    private void dgvTargets_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (_busy || e.RowIndex < 0 || e.ColumnIndex != colNewDb.Index) return;
        dgvTargets.BeginEdit(true);
        if (dgvTargets.EditingControl is ComboBox combo) combo.DroppedDown = true;
    }

    private void StyleRow(DataGridViewRow grid)
    {
        var pending = grid.Tag is TargetRow row && IsPending(row, ChoiceOf(grid));
        grid.DefaultCellStyle.BackColor = pending ? PendingBack : Color.Empty;
        grid.Cells[colNewDb.Index].Style.Font = pending ? _boldFont ??= new Font(dgvTargets.Font, FontStyle.Bold) : null;
    }

    private static bool IsPending(TargetRow row, string choice) =>
        choice.Length > 0 && !Same(choice, row.Entry.Database);

    private string ChoiceOf(DataGridViewRow grid) =>
        (grid.Cells[colNewDb.Index].Value as string)?.Trim() ?? "";

    private Dictionary<string, string> CurrentChoices()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (DataGridViewRow grid in dgvTargets.Rows)
            if (grid.Tag is TargetRow row && ChoiceOf(grid).Length > 0)
                map[row.Key] = ChoiceOf(grid);
        return map;
    }

    private List<(TargetRow Row, string Db)> PendingChanges()
    {
        var list = new List<(TargetRow, string)>();
        foreach (DataGridViewRow grid in dgvTargets.Rows)
            if (grid.Tag is TargetRow row && IsPending(row, ChoiceOf(grid)))
                list.Add((row, ChoiceOf(grid)));
        return list;
    }

    private void btnClearNew_Click(object? sender, EventArgs e)
    {
        dgvTargets.EndEdit();
        foreach (DataGridViewRow grid in dgvTargets.Rows)
            grid.Cells[colNewDb.Index].Value = "";
    }

    private void UpdateButtons()
    {
        var pending = PendingChanges().Count;
        btnApply.Text = pending == 0 ? "✔  Apply && Restart" : $"✔  Apply {pending} && Restart";
        btnApply.Enabled = !_busy && pending > 0;

        var switched = _rows.Count(r => r.Original != null);
        btnReset.Text = switched == 0 ? "↺  Reset to default" : $"↺  Reset to default ({switched})";
        btnReset.Enabled = !_busy && switched > 0;
    }

    // ── Apply ─────────────────────────────────────────────────────────────────

    private void chkSynonym_CheckedChanged(object? sender, EventArgs e)
    {
        var on = chkSynonym.Checked && !_busy;
        lblAlias.Enabled = txtAliasDb.Enabled = on;
        lblReal.Enabled = cboRealDb.Enabled = on;
    }

    private async void btnApply_Click(object? sender, EventArgs e)
    {
        dgvTargets.EndEdit();
        var changes = PendingChanges();
        if (changes.Count == 0)
        {
            ShowWarn("Pick a New DB (different from the current one) for at least one service.");
            return;
        }

        var runSynonym = chkSynonym.Checked;
        var aliasDb = txtAliasDb.Text.Trim();
        var realDb = cboRealDb.Text.Trim();
        if (runSynonym)
        {
            if (_profile == null) { ShowWarn("The synonym database needs a SQL Server login."); return; }
            if (aliasDb.Length == 0 || realDb.Length == 0)
            {
                ShowWarn("Fill in both the alias database and the database it forwards to.");
                return;
            }
            if (Same(aliasDb, realDb)) { ShowWarn("Alias and target are the same database — nothing to alias."); return; }
        }

        var summary = string.Join("\n", changes.Select(c => $"• {c.Row.Label}: {c.Row.Entry.Database} → {c.Db}"));
        if (MessageBox.Show(
                $"Switch {changes.Count} connection(s)?\n\n{summary}\n\n" +
                RestartNote(changes.Select(c => c.Row)) +
                (runSynonym ? $"\nSynonym DB [{aliasDb}] → [{realDb}] will be created/refreshed." : ""),
                "Confirm Switch Environment",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var plan = changes.Select(c => (c.Row, (string?)null, (string?)c.Db)).ToList();
        await RunAsync("Switching", "Switched", plan,
            beforeWrite: row => IisEnvOriginalStore.RecordIfMissing(row.Host, row.Name, row.Entry),
            afterSuccess: null,
            afterRestart: runSynonym
                ? () => MerchantSynonymHelper.CreateOrRefreshAsync(_profile!.CreateHelper(), aliasDb, realDb, Log)
                : null);
    }

    private static string RestartNote(IEnumerable<TargetRow> rows)
    {
        var list = rows.ToList();
        var parts = new List<string>();
        if (list.Any(r => !r.IsService)) parts.Add("IIS is restarted (iisreset)");
        var services = ServicesOf(list);
        if (services.Count > 0) parts.Add("these Windows services are restarted: " + string.Join(", ", services));
        return "Each config file is backed up first, then " + string.Join(" and ", parts) + ".";
    }

    private static List<string> ServicesOf(IEnumerable<TargetRow> rows) =>
        rows.Where(r => r.IsService).Select(r => r.ServiceName!)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    // ── Reset to default ─────────────────────────────────────────────────────

    private async void btnReset_Click(object? sender, EventArgs e)
    {
        dgvTargets.EndEdit();
        var targets = _rows.Where(r => r.Original != null).ToList();
        if (targets.Count == 0)
        {
            ShowWarn("Nothing to reset — every service is on its original database.");
            return;
        }

        var plan = new List<(TargetRow Row, string? Server, string? Db)>();
        var skipped = new List<string>();
        foreach (var row in targets)
        {
            var orig = row.Original!;
            var db = Same(orig.Database, row.Entry.Database) ? null : orig.Database.Trim();
            var server = Same(orig.DataSource, row.Entry.DataSource) ? null : orig.DataSource.Trim();

            // Writing an empty value back would leave "Initial Catalog=" and kill the service.
            if (db is { Length: 0 } || server is { Length: 0 })
                skipped.Add($"{row.Label}: the recorded original is empty — fix this one by hand.");
            else
                plan.Add((row, server, db));
        }

        var summary = string.Join("\n", plan.Select(p =>
            $"• {p.Row.Label}: {p.Row.Entry.Database} → {p.Row.Original!.Database}"));
        if (skipped.Count > 0)
            summary += "\n\nSkipped:\n" + string.Join("\n", skipped.Select(s => "• " + s));

        if (plan.Count == 0)
        {
            ShowWarn("Nothing can be reset automatically:\n\n" + summary);
            return;
        }

        if (MessageBox.Show(
                $"Reset {plan.Count} connection(s) to their original database?\n\n{summary}\n\n" +
                RestartNote(plan.Select(p => p.Row)),
                "Confirm Reset to default",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        foreach (var s in skipped) Log("⚠ " + s);

        await RunAsync("Resetting to default", "Reset to default", plan,
            beforeWrite: null,
            afterSuccess: row => IisEnvOriginalStore.Remove(new[] { row.Key }),
            afterRestart: null);
    }

    // ── Shared run ────────────────────────────────────────────────────────────

    /// <summary>Writes each config, restarts IIS once and each touched Windows service, then closes
    /// on success or stays open with the failures listed in the log.</summary>
    private async Task RunAsync(
        string title,
        string verb,
        List<(TargetRow Row, string? Server, string? Db)> plan,
        Action<TargetRow>? beforeWrite,
        Action<TargetRow>? afterSuccess,
        Func<Task>? afterRestart)
    {
        var failures = new List<string>();
        var written = new List<TargetRow>();
        var restarted = new List<string>();

        SetBusy(true, title + "...");
        try
        {
            Log($"── {title}: {plan.Count} change(s) at {DateTime.Now:HH:mm:ss} ─────────────");
            var backedUp = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var (row, server, db) in plan)
            {
                var configPath = row.ConfigPath;
                if (configPath == null)
                {
                    failures.Add($"{row.Label}: no config file found.");
                    Log($"✘ {row.Label}: no config file found.");
                    continue;
                }
                var makeBackup = !backedUp.Contains(configPath);

                beforeWrite?.Invoke(row);
                var result = await Task.Run(() => WebConfigConnStrings.Patch(
                    configPath, row.Entry.Kind, row.Entry.Name, server, db, makeBackup));

                if (result.BackupPath != null)
                    backedUp.Add(configPath);

                if (result.Success)
                {
                    written.Add(row);
                    afterSuccess?.Invoke(row);
                    Log($"✔ {row.Label}: {result.OldDataSource}/{result.OldDatabase} → " +
                        $"{server ?? result.OldDataSource}/{db ?? result.OldDatabase}");
                    if (result.BackupPath != null)
                        Log($"   backup: {result.BackupPath}");
                }
                else
                {
                    failures.Add($"{row.Label}: {result.Error}");
                    Log($"✘ {row.Label}: {result.Error}");
                    if (result.Changed && !result.Verified)
                        Log($"   ⚠ the file WAS written but did not verify — restore {result.BackupPath ?? "the backup"}.");
                }
            }

            if (written.Any(r => !r.IsService))
            {
                SetStatus("Restarting IIS (iisreset)...");
                Log("Restarting IIS (iisreset)...");
                try
                {
                    await IisHelper.RestartIisAsync();
                    restarted.Add("IIS");
                    Log("✔ IIS restarted.");
                }
                catch (Exception ex)
                {
                    failures.Add("IIS restart: " + ex.Message);
                    Log("✘ IIS restart failed: " + ex.Message);
                }
            }

            // After IIS, since a service such as BLogicReportService may call into the IIS apps.
            foreach (var service in ServicesOf(written))
            {
                SetStatus($"Restarting service {service}...");
                Log($"Restarting service {service}...");
                try
                {
                    await Task.Run(() => WindowsServiceConfigs.Restart(service));
                    restarted.Add(service);
                    Log($"✔ Service {service} restarted.");
                }
                catch (Exception ex)
                {
                    failures.Add($"Service {service}: {ex.Message}");
                    Log($"✘ Service {service}: {ex.Message}");
                }
            }

            if (written.Count == 0)
                Log("⚠ No config file was changed, so nothing was restarted.");

            if (afterRestart != null)
            {
                SetStatus("Building synonym database...");
                try { await afterRestart(); }
                catch (Exception ex)
                {
                    failures.Add("Synonym database: " + ex.Message);
                    Log("✘ Synonym database: " + ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            failures.Add(ex.Message);
            Log("✘ " + ex.Message);
        }
        finally
        {
            SetBusy(false);
        }

        Banner(failures);
        var done = $"✔ {verb} {written.Count} connection(s)" +
                   (restarted.Count > 0 ? "; restarted " + string.Join(", ", restarted) + "." : ".");
        ResultSummary = failures.Count == 0
            ? done
            : $"✘ {title}: {written.Count} applied, {failures.Count} failed — open Switch Environment for the log.";

        if (failures.Count == 0)
        {
            SetStatus(done);
            MessageBox.Show(done.TrimStart('✔', ' '), "Switch Environment",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        SetStatus($"✘ {written.Count} applied, {failures.Count} failed — see the log.");
        await ReloadTargetsAsync();
    }

    private void Banner(List<string> failures)
    {
        _log?.Flush();
        if (failures.Count == 0)
        {
            _log?.Append("  ✔  COMPLETE" + Environment.NewLine, Color.LimeGreen, BannerFont);
        }
        else
        {
            _log?.Append("  ✘  FAILED" + Environment.NewLine, Color.Red, BannerFont);
            foreach (var f in failures) Log("✘ " + f);
        }
        _log?.Flush();
    }

    private void btnClose_Click(object? sender, EventArgs e) => Close();

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_busy)
        {
            e.Cancel = true;
            ShowWarn("An operation is still running — wait for it to finish.");
            return;
        }
        base.OnFormClosing(e);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool Same(string? a, string? b) =>
        string.Equals(a?.Trim() ?? "", b?.Trim() ?? "", StringComparison.OrdinalIgnoreCase);

    private void SetBusy(bool busy, string? msg = null)
    {
        _busy = busy;
        AppBusyState.IsBusy = busy;
        progressBar.Visible = busy;

        foreach (var c in new Control[] { btnReload, btnLoginDb, btnClearNew, btnClose, dgvTargets, chkSynonym })
            c.Enabled = !busy;

        if (msg != null) SetStatus(msg);
        chkSynonym_CheckedChanged(this, EventArgs.Empty);
        UpdateButtons();
    }

    private void SetStatus(string msg)
    {
        lblStatus.Text = msg;
        lblStatus.ForeColor = msg.StartsWith("✔") ? Color.Green
            : msg.StartsWith("✘") || msg.StartsWith("Error") ? Color.Red
            : Color.DimGray;
    }

    private static void ShowError(string msg) =>
        MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

    private static void ShowWarn(string msg) =>
        MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
