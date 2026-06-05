using System.IO.Compression;

namespace BLogicDevTool;

public partial class UnzipWorkbenchForm : UserControl
{
    private SqlConnectionProfile? _profile;
    private readonly Dictionary<string, string> _sqlContents = new();

    public UnzipWorkbenchForm(SqlConnectionProfile? profile = null)
    {
        _profile = profile ?? SqlSessionStore.Current;
        InitializeComponent();
        SqlSessionStore.ProfileChanged += OnSessionProfileChanged;
        Disposed += (_, _) => SqlSessionStore.ProfileChanged -= OnSessionProfileChanged;
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        LoadConfigToGrid();
        UpdateButtonStates();
        UpdateLoginButtonText();
        if (_profile != null)
            await LoadDatabasesAsync();
        else
        {
            cboDatabase.Enabled = false;
            btnRun.Enabled = false;
        }
    }

    private void OnSessionProfileChanged()
    {
        if (IsDisposed) return;
        if (InvokeRequired) { BeginInvoke(OnSessionProfileChanged); return; }
        _profile = SqlSessionStore.Current;
        UpdateLoginButtonText();
        if (_profile != null) _ = LoadDatabasesAsync();
        else
        {
            cboDatabase.Items.Clear();
            cboDatabase.Text = "";
            cboDatabase.Enabled = false;
        }
        UpdateButtonStates();
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
            await LoadDatabasesAsync();
        }
    }

    // ── Settings overlay ──────────────────────────────────────────────────────

    private void btnToggleSettings_Click(object sender, EventArgs e)
    {
        pnlSettings.Visible = !pnlSettings.Visible;
        if (pnlSettings.Visible)
        {
            pnlSettings.BringToFront();
            LoadConfigToGrid();
        }
    }

    private void btnCloseSettings_Click(object sender, EventArgs e)
        => pnlSettings.Visible = false;

    private void LoadConfigToGrid()
    {
        var mappings = UnzipConfigStore.Load();
        dgvMappings.Rows.Clear();
        foreach (var m in mappings)
            dgvMappings.Rows.Add(m.FolderName, m.DestinationPath);
    }

    private List<UnzipFolderMapping> ReadConfigFromGrid()
    {
        var list = new List<UnzipFolderMapping>();
        foreach (DataGridViewRow row in dgvMappings.Rows)
        {
            var folder = row.Cells["colFolder"].Value?.ToString()?.Trim() ?? "";
            var dest   = row.Cells["colDest"].Value?.ToString()?.Trim() ?? "";
            if (!string.IsNullOrEmpty(folder) && !string.IsNullOrEmpty(dest))
                list.Add(new UnzipFolderMapping { FolderName = folder, DestinationPath = dest });
        }
        return list;
    }

    private void btnAddRow_Click(object sender, EventArgs e)
    {
        dgvMappings.Rows.Add("", "");
        var row = dgvMappings.Rows[dgvMappings.Rows.Count - 1];
        dgvMappings.CurrentCell = row.Cells["colFolder"];
        dgvMappings.BeginEdit(true);
    }

    private void btnDeleteRow_Click(object sender, EventArgs e)
    {
        if (dgvMappings.SelectedRows.Count == 0) return;
        var idx = dgvMappings.SelectedRows[0].Index;
        if (idx >= 0 && idx < dgvMappings.Rows.Count)
            dgvMappings.Rows.RemoveAt(idx);
    }

    private void btnSaveConfig_Click(object sender, EventArgs e)
    {
        var mappings = ReadConfigFromGrid();
        if (mappings.Count == 0)
        {
            MessageBox.Show("No configuration to save.", "Save Configuration",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        UnzipConfigStore.Save(mappings);
        MessageBox.Show("✔ Configuration saved successfully.", "Save Configuration",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ── Database list ─────────────────────────────────────────────────────────

    private async Task LoadDatabasesAsync()
    {
        try
        {
            var helper = _profile!.CreateHelper();
            var dbs    = await helper.GetDatabasesAsync();
            cboDatabase.BeginUpdate();
            cboDatabase.Items.Clear();
            cboDatabase.Items.AddRange(dbs.Cast<object>().ToArray());
            cboDatabase.EndUpdate();

            var def = dbs.FirstOrDefault(d =>
                d.Equals("BLogicPOS7", StringComparison.OrdinalIgnoreCase))
                ?? dbs.FirstOrDefault();
            if (def != null) cboDatabase.Text = def;
        }
        catch (Exception ex)
        {
            SetStatus($"⚠ Failed to load DB list: {ex.Message}");
        }
    }

    // ── ZIP loading ───────────────────────────────────────────────────────────

    private void btnBrowseZip_Click(object sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title  = "Select ZIP file",
            Filter = "ZIP files (*.zip)|*.zip|All files (*.*)|*.*",
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        txtZipPath.Text = dlg.FileName;
        LoadZipFile(dlg.FileName);
    }

    private void btnLoadZip_Click(object sender, EventArgs e)
    {
        var path = txtZipPath.Text.Trim();
        if (!File.Exists(path))
        {
            MessageBox.Show("Please select a valid ZIP file.", "Load ZIP",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        LoadZipFile(path);
    }

    private void txtZipPath_TextChanged(object sender, EventArgs e) => UpdateButtonStates();

    private void LoadZipFile(string zipPath)
    {
        _sqlContents.Clear();
        lstSqlFiles.Items.Clear();
        txtSqlPreview.Clear();

        try
        {
            using var archive = ZipFile.OpenRead(zipPath);
            foreach (var entry in archive.Entries
                .Where(e => e.Name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.FullName))
            {
                using var reader = new StreamReader(entry.Open());
                _sqlContents[entry.FullName] = reader.ReadToEnd();
                lstSqlFiles.Items.Add(entry.FullName);
            }

            SetStatus(lstSqlFiles.Items.Count == 0
                ? "⚠ No .sql files found in the ZIP."
                : $"✔ Loaded {lstSqlFiles.Items.Count} .sql file(s).");
        }
        catch (Exception ex)
        {
            SetStatus($"✘ ZIP read error: {ex.Message}");
        }

        UpdateButtonStates();
    }

    private void lstSqlFiles_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lstSqlFiles.SelectedItem is not string key) return;
        txtSqlPreview.Text = _sqlContents.TryGetValue(key, out var content) ? content : "";
        txtSqlPreview.SelectionStart = 0;
        txtSqlPreview.ScrollToCaret();
    }

    // ── Run SQL ───────────────────────────────────────────────────────────────

    private async void btnRun_Click(object sender, EventArgs e)
    {
        if (_profile == null)
        {
            MessageBox.Show("No SQL connection — open this tool from the menu with an active login session.", "Run SQL",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (lstSqlFiles.SelectedItem is not string key)
        {
            MessageBox.Show("Please select a .sql file from the list.", "Run SQL",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var dbName = cboDatabase.Text.Trim();
        if (string.IsNullOrEmpty(dbName))
        {
            MessageBox.Show("Please select or enter a Database name.", "Run SQL",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var sql = txtSqlPreview.Text;
        if (string.IsNullOrWhiteSpace(sql))
        {
            MessageBox.Show("SQL content is empty.", "Run SQL",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"Confirm run SQL script?\n\nFile: {key}\nDatabase: {dbName}",
            "Confirm Run SQL",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        SetBusy(true, $"Running '{Path.GetFileName(key)}' on [{dbName}]...");
        AppendLog($"\n>>> SQL: {key}  →  [{dbName}]  at {DateTime.Now:HH:mm:ss}");

        try
        {
            var helper = _profile.CreateHelper();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await helper.ExecuteSqlBatchesAsync(sql, dbName);
            sw.Stop();
            AppendLog($"✔ SQL completed in {sw.Elapsed.TotalSeconds:F1}s.");
            SetStatus($"✔ Executed: {Path.GetFileName(key)}");
        }
        catch (Exception ex)
        {
            AppendLog($"✘ SQL Error: {ex.Message}");
            SetStatus("✘ Script execution failed.");
        }
        finally
        {
            SetBusy(false);
        }
    }

    // ── Deploy ────────────────────────────────────────────────────────────────

    private async void btnDeploy_Click(object sender, EventArgs e)
    {
        var zipPath  = txtZipPath.Text.Trim();
        var mappings = UnzipConfigStore.Load();

        if (!File.Exists(zipPath))
        {
            MessageBox.Show("Please select a valid ZIP file.", "Deploy",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (mappings.Count == 0)
        {
            MessageBox.Show("No folder configuration found.\nPlease open ⚙ Settings to configure.", "Deploy",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"Confirm deployment?\n\nZIP file: {Path.GetFileName(zipPath)}\n" +
            $"Folders to update: {mappings.Count}\n\n" +
            "Process will:\n  1. Stop IIS\n  2. Back up files being replaced, then extract & copy\n  3. Restart IIS",
            "Confirm Deployment",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        SetBusy(true, "Deploying...");
        txtLog.Clear();

        try
        {
            await DeployAsync(zipPath, mappings);
        }
        catch (Exception ex)
        {
            AppendLog($"\n✘ Fatal error: {ex.Message}");
            SetStatus("✘ Deployment failed.");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task DeployAsync(string zipPath, List<UnzipFolderMapping> mappings)
    {
        // Backup root for files that are about to be overwritten. Placed under the tool's
        // own folder ("backups\backup_<timestamp>"), created lazily on the first replaced
        // file. Layout mirrors the ZIP/mapping structure: <FolderName>/<relativePath>.
        var backupRoot = Path.Combine(
            AppContext.BaseDirectory, "backups",
            $"backup_{DateTime.Now:yyyyMMdd_HHmmss}");

        AppendLog(">>> Step 1: Stopping IIS...");
        SetStatus("Stopping IIS...");
        try
        {
            await IisHelper.StopIisAsync();
            AppendLog("✔ IIS stopped.");
        }
        catch (Exception ex)
        {
            AppendLog($"⚠ Warning while stopping IIS: {ex.Message}");
            AppendLog("  Continuing deployment...");
        }

        AppendLog($"\n>>> Step 2: Extracting '{Path.GetFileName(zipPath)}'...");
        SetStatus("Extracting and copying files...");
        int totalFiles = 0, skippedFolders = 0, backedUp = 0;
        var createdBaseline = UnzipBaselineStore.LoadCreated();

        try
        {
        await Task.Run(() =>
        {
            using var archive = ZipFile.OpenRead(zipPath);
            foreach (var mapping in mappings)
            {
                var prefix  = mapping.FolderName.TrimEnd('/') + "/";
                var destDir = mapping.DestinationPath;
                var entries = archive.Entries
                    .Where(e => e.FullName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (entries.Count == 0)
                {
                    AppendLog($"  ⚠ Folder '{mapping.FolderName}' not found in ZIP — skipping.");
                    skippedFolders++;
                    continue;
                }

                AppendLog($"\n  📁 {mapping.FolderName}  →  {destDir}");
                var backupFolderName = mapping.FolderName.Trim().Trim('/', '\\');
                int fileCount = 0;

                foreach (var entry in entries)
                {
                    if (string.IsNullOrEmpty(entry.Name)) continue;
                    var relativePath = entry.FullName.Substring(prefix.Length)
                                            .Replace('/', Path.DirectorySeparatorChar);
                    var destPath   = Path.Combine(destDir, relativePath);
                    var destSubDir = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(destSubDir))
                        Directory.CreateDirectory(destSubDir);

                    var fullDest = Path.GetFullPath(destPath);
                    bool existedBefore = File.Exists(destPath);

                    if (existedBefore)
                    {
                        // Per-deploy backup: the file's state right before THIS deploy.
                        try
                        {
                            var backupPath = Path.Combine(backupRoot, backupFolderName, relativePath);
                            var backupSubDir = Path.GetDirectoryName(backupPath);
                            if (!string.IsNullOrEmpty(backupSubDir))
                                Directory.CreateDirectory(backupSubDir);
                            File.Copy(destPath, backupPath, overwrite: true);
                            backedUp++;
                        }
                        catch (Exception bex)
                        {
                            AppendLog($"     ⚠ Backup failed for {relativePath}: {bex.Message}");
                        }

                        // Write-once original baseline (skip files a previous deploy created —
                        // those have no true original).
                        if (!createdBaseline.Contains(fullDest))
                        {
                            try { UnzipBaselineStore.SaveOriginalIfAbsent(destPath); }
                            catch (Exception bx)
                            {
                                AppendLog($"     ⚠ Baseline failed for {relativePath}: {bx.Message}");
                            }
                        }
                    }
                    else
                    {
                        // Brand-new file → record so Restore Original can remove it.
                        createdBaseline.Add(fullDest);
                    }

                    entry.ExtractToFile(destPath, overwrite: true);
                    fileCount++;
                    totalFiles++;
                    AppendLog($"     → {relativePath}");
                }

                AppendLog($"  ✔ Copied {fileCount} file(s).");
            }
        });

        UnzipBaselineStore.SaveCreated(createdBaseline);

        AppendLog($"\n  Total: {totalFiles} file(s) updated.");
        if (backedUp > 0)
            AppendLog($"  💾 Backed up {backedUp} replaced file(s) → {backupRoot}");
        else
            AppendLog($"  💾 No existing files were replaced — no per-deploy backup created.");
        AppendLog($"  ↩ Original baseline kept at: {UnzipBaselineStore.BaselineLocation}  (use 'Restore Original' to roll back)");
        if (skippedFolders > 0)
            AppendLog($"  ⚠ {skippedFolders} folder(s) skipped (not found in ZIP).");
        }
        finally
        {
            // Always restart IIS — even if extraction failed midway — so the machine
            // never gets left with IIS stopped.
            AppendLog("\n>>> Step 3: Restarting IIS...");
            SetStatus("Restarting IIS...");
            try
            {
                await IisHelper.StartIisAsync();
                AppendLog("✔ IIS started.");
            }
            catch (Exception ex)
            {
                AppendLog($"⚠ Warning while starting IIS: {ex.Message}");
            }
        }

        AppendLog($"\n✔ Deployment completed at {DateTime.Now:HH:mm:ss}.");
        SetStatus($"✔ Deployment complete — {totalFiles} file(s) updated.");
    }

    // ── Restore Original ────────────────────────────────────────────────────────

    private async void btnRestoreOriginal_Click(object sender, EventArgs e)
    {
        if (!UnzipBaselineStore.HasAnyBaseline())
        {
            MessageBox.Show(
                "No original baseline has been captured yet.\nDeploy at least once first.",
                "Restore Original", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(
            "Restore ALL deployed files to their ORIGINAL state?\n\n" +
            "Process will:\n" +
            "  1. Stop IIS\n" +
            "  2. Overwrite current files with the captured originals\n" +
            "  3. Delete files that were added by deploys\n" +
            "  4. Restart IIS\n\n" +
            "(Original = state before the very first deploy made with this tool.)",
            "Confirm Restore Original",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        SetBusy(true, "Restoring original state...");
        txtLog.Clear();
        try
        {
            await RestoreOriginalAsync();
        }
        catch (Exception ex)
        {
            AppendLog($"\n✘ Fatal error: {ex.Message}");
            SetStatus("✘ Restore failed.");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task RestoreOriginalAsync()
    {
        AppendLog(">>> Step 1: Stopping IIS...");
        SetStatus("Stopping IIS...");
        try
        {
            await IisHelper.StopIisAsync();
            AppendLog("✔ IIS stopped.");
        }
        catch (Exception ex)
        {
            AppendLog($"⚠ Warning while stopping IIS: {ex.Message}");
            AppendLog("  Continuing restore...");
        }

        try
        {
            AppendLog("\n>>> Step 2: Restoring original files...");
            SetStatus("Restoring files...");
            int restored = 0, removed = 0, failed = 0;
            var created = UnzipBaselineStore.LoadCreated();

            await Task.Run(() =>
            {
                // 1) Put back the captured originals (overwrite current).
                foreach (var baselineFile in UnzipBaselineStore.EnumerateBaselineFiles())
                {
                    var orig = UnzipBaselineStore.DecodeOriginalPath(baselineFile);
                    if (string.IsNullOrEmpty(orig))
                    {
                        AppendLog($"  ⚠ Cannot map baseline file: {baselineFile}");
                        continue;
                    }
                    try
                    {
                        var dir = Path.GetDirectoryName(orig);
                        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                        if (File.Exists(orig)) File.SetAttributes(orig, FileAttributes.Normal);
                        File.Copy(baselineFile, orig, overwrite: true);
                        restored++;
                        AppendLog($"  ↩ {orig}");
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        AppendLog($"  ✘ {orig}: {ex.Message}");
                    }
                }

                // 2) Delete files that deploys created (did not exist originally).
                foreach (var p in created)
                {
                    try
                    {
                        if (File.Exists(p))
                        {
                            File.SetAttributes(p, FileAttributes.Normal);
                            File.Delete(p);
                            removed++;
                            AppendLog($"  🗑 {p}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        AppendLog($"  ✘ delete {p}: {ex.Message}");
                    }
                }
            });

            AppendLog($"\n  Restored {restored} original file(s); removed {removed} added file(s)."
                      + (failed > 0 ? $"  ⚠ {failed} failure(s)." : ""));
            SetStatus($"✔ Restored to original — {restored} restored, {removed} removed.");
        }
        finally
        {
            AppendLog("\n>>> Step 3: Restarting IIS...");
            SetStatus("Restarting IIS...");
            try
            {
                await IisHelper.StartIisAsync();
                AppendLog("✔ IIS started.");
            }
            catch (Exception ex)
            {
                AppendLog($"⚠ Warning while starting IIS: {ex.Message}");
            }
        }

        AppendLog($"\n✔ Restore completed at {DateTime.Now:HH:mm:ss}.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void AppendLog(string message)
    {
        if (InvokeRequired) { Invoke(() => AppendLog(message)); return; }
        txtLog.AppendText(message + Environment.NewLine);
        txtLog.ScrollToCaret();
    }

    private void UpdateButtonStates()
    {
        var zipExists = File.Exists(txtZipPath.Text.Trim());
        btnDeploy.Enabled = zipExists;
        btnRun.Enabled    = zipExists && lstSqlFiles.Items.Count > 0 && _profile != null;
    }

    private void SetBusy(bool busy, string? statusText = null)
    {
        if (InvokeRequired) { Invoke(() => SetBusy(busy, statusText)); return; }
        AppBusyState.IsBusy = busy;
        btnRun.Enabled            = !busy && lstSqlFiles.Items.Count > 0 && _profile != null;
        btnDeploy.Enabled         = !busy && File.Exists(txtZipPath.Text.Trim());
        btnRestoreOriginal.Enabled = !busy;
        btnBrowseZip.Enabled      = !busy;
        btnToggleSettings.Enabled = !busy;
        lstSqlFiles.Enabled       = !busy;
        cboDatabase.Enabled       = !busy && _profile != null;
        progressBar.Visible       = busy;
        if (statusText != null) SetStatus(statusText);
    }

    private void SetStatus(string text)
    {
        if (InvokeRequired) { Invoke(() => SetStatus(text)); return; }
        lblStatus.Text      = text;
        lblStatus.ForeColor = text.StartsWith("✔") ? Color.DarkGreen
            : (text.StartsWith("✘") || text.Contains("Error")) ? Color.DarkRed
            : text.StartsWith("⚠") ? Color.DarkOrange
            : SystemColors.ControlText;
    }
}
