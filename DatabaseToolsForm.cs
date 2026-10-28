namespace BLogicDevTool;

public partial class DatabaseToolsForm : UserControl
{
    private SqlConnectionProfile? _profile;
    private SqlServerHelper? _helper;

    // ── SQL Templates (Restore) ────────────────────────────────────────────

    // Snapshots must go first: RESTORE fails while any snapshot exists on the target.
    // TRY/CATCH per step because no GO separates this from the RESTORE below — a KILL
    // racing a disconnecting session would otherwise abort the restore itself.
    private const string PrepareDbBlock =
        "USE master;\r\nGO\r\n" +
        "IF EXISTS (SELECT name FROM sys.databases WHERE name = '{DB_NAME}')\r\n" +
        "BEGIN\r\n" +
        "    BEGIN TRY\r\n" +
        "        DECLARE @snapKill NVARCHAR(MAX) = N'';\r\n" +
        "        SELECT @snapKill += N'KILL ' + CAST(s.session_id AS NVARCHAR(10)) + N';'\r\n" +
        "        FROM sys.dm_exec_sessions s\r\n" +
        "        JOIN sys.databases d ON d.database_id = s.database_id\r\n" +
        "        WHERE d.source_database_id = DB_ID('{DB_NAME}') AND s.session_id <> @@SPID;\r\n" +
        "        EXEC(@snapKill);\r\n" +
        "    END TRY\r\n" +
        "    BEGIN CATCH\r\n" +
        "        PRINT N'[prepare] snapshot sessions: ' + ERROR_MESSAGE();\r\n" +
        "    END CATCH;\r\n" +
        "    BEGIN TRY\r\n" +
        "        DECLARE @dropSnap NVARCHAR(MAX) = N'';\r\n" +
        "        SELECT @dropSnap += N'DROP DATABASE [' + REPLACE(name, N']', N']]') + N'];'\r\n" +
        "        FROM sys.databases WHERE source_database_id = DB_ID('{DB_NAME}');\r\n" +
        "        IF @dropSnap <> N'' PRINT N'[prepare] dropping snapshots: ' + @dropSnap;\r\n" +
        "        EXEC(@dropSnap);\r\n" +
        "    END TRY\r\n" +
        "    BEGIN CATCH\r\n" +
        "        PRINT N'[prepare] drop snapshots: ' + ERROR_MESSAGE();\r\n" +
        "    END CATCH;\r\n" +
        "    BEGIN TRY\r\n" +
        "        DECLARE @kill NVARCHAR(MAX) = N'';\r\n" +
        "        SELECT @kill += N'KILL ' + CAST(session_id AS NVARCHAR(10)) + N';'\r\n" +
        "        FROM sys.dm_exec_sessions\r\n" +
        "        WHERE database_id = DB_ID('{DB_NAME}') AND session_id <> @@SPID;\r\n" +
        "        EXEC(@kill);\r\n" +
        "    END TRY\r\n" +
        "    BEGIN CATCH\r\n" +
        "        PRINT N'[prepare] kill sessions: ' + ERROR_MESSAGE();\r\n" +
        "    END CATCH;\r\n" +
        "    ALTER DATABASE [{DB_NAME}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;\r\n" +
        "END\r\n";

    private const string BlogicTemplate =
        PrepareDbBlock +
        "RESTORE DATABASE [{DB_NAME}]\r\n" +
        "FROM DISK = '{BAK_PATH}'\r\n" +
        "WITH\r\n" +
        "    MOVE 'BLogicPOS7' TO '{DATA_PATH}\\{DB_NAME}.mdf',\r\n" +
        "    MOVE 'BLogicPOS7_log' TO '{DATA_PATH}\\{DB_NAME}_log.ldf',\r\n" +
        "    MOVE 'BLogicPOS7_NONCLUSTERED_INDEXS' TO '{DATA_PATH}\\{DB_NAME}_INDEXS.mdf',\r\n" +
        "    REPLACE;\r\n" +
        "GO\r\n" +
        "ALTER DATABASE [{DB_NAME}] SET MULTI_USER;\r\n" +
        "GO";

    private const string MerchantTemplate =
        PrepareDbBlock +
        "RESTORE DATABASE [{DB_NAME}]\r\n" +
        "FROM DISK = '{BAK_PATH}'\r\n" +
        "WITH\r\n" +
        "    MOVE 'Merchant' TO '{DATA_PATH}\\{DB_NAME}.mdf',\r\n" +
        "    MOVE 'Merchant_log' TO '{DATA_PATH}\\{DB_NAME}_log.ldf',\r\n" +
        "    MOVE 'Merchant_NONCLUSTERED_INDEXS' TO '{DATA_PATH}\\{DB_NAME}_INDEXS.mdf',\r\n" +
        "    REPLACE;\r\n" +
        "GO\r\n" +
        "ALTER DATABASE [{DB_NAME}] SET MULTI_USER;\r\n" +
        "GO";

    private const string MailTemplate =
        PrepareDbBlock +
        "RESTORE DATABASE [{DB_NAME}]\r\n" +
        "FROM DISK = '{BAK_PATH}'\r\n" +
        "WITH\r\n" +
        "    MOVE 'BLogicEmailService' TO '{DATA_PATH}\\{DB_NAME}.mdf',\r\n" +
        "    MOVE 'BLogicEmailService_log' TO '{DATA_PATH}\\{DB_NAME}_log.ldf',\r\n" +
        "    REPLACE;\r\n" +
        "GO\r\n" +
        "ALTER DATABASE [{DB_NAME}] SET MULTI_USER;\r\n" +
        "GO";

    public DatabaseToolsForm(SqlConnectionProfile? profile)
    {
        _profile = profile ?? SqlSessionStore.Current;
        InitializeComponent();
        cboDbNameB.Text = "BLogicPOS7";
        cboDbNameZ.Text = "BLogicPOS7";
        // Block switching sub-tabs while a backup/restore is running.
        tabControl.Selecting += (_, ev) => { if (AppBusyState.IsBusy) ev.Cancel = true; };
        SqlSessionStore.ProfileChanged += OnSessionProfileChanged;
        Disposed += (_, _) => SqlSessionStore.ProfileChanged -= OnSessionProfileChanged;
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        UpdateLoginButtonText();
        if (_profile == null)
        {
            SetControlsEnabled(false);
            lblConnStatus.ForeColor = System.Drawing.Color.DarkOrange;
            lblConnStatus.Text = "Not connected. Click \"🔑 Login\" to sign in.";
            return;
        }
        await ConnectAndLoadAsync();
    }

    private void OnSessionProfileChanged()
    {
        if (IsDisposed) return;
        if (InvokeRequired) { BeginInvoke(OnSessionProfileChanged); return; }
        _profile = SqlSessionStore.Current;
        UpdateLoginButtonText();
        if (_profile != null) _ = ConnectAndLoadAsync();
        else
        {
            SetControlsEnabled(false);
            lblConnStatus.ForeColor = System.Drawing.Color.DarkOrange;
            lblConnStatus.Text = "Not connected.";
        }
    }

    private void UpdateLoginButtonText()
    {
        var text = SqlSessionStore.Current == null ? "🔑 Login" : "🔄 Refresh";
        btnLoginB.Text = text;
        btnLoginZ.Text = text;
    }

    private async void btnLogin_Click(object? sender, EventArgs e)
    {
        if (SqlSessionStore.Current == null)
        {
            using var dlg = new SqlLoginForm();
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK || dlg.Profile == null) return;
            SqlSessionStore.SetProfile(dlg.Profile);
            // ProfileChanged → OnSessionProfileChanged reloads
        }
        else
        {
            await ConnectAndLoadAsync();
        }
    }

    private void SetControlsEnabled(bool enabled)
    {
        tabControl.Enabled = enabled;
    }

    /// <summary>Enable/disable every control on a tab page (progress bar + status label
    /// are kept enabled so they stay visible/animating during the operation).</summary>
    private static void SetTabEnabled(TabPage tab, bool enabled, params Control[] keepEnabled)
    {
        foreach (Control c in tab.Controls)
            if (Array.IndexOf(keepEnabled, c) < 0)
                c.Enabled = enabled;
    }

    private async Task ConnectAndLoadAsync()
    {
        lblConnStatus.ForeColor = System.Drawing.Color.DarkGray;
        lblConnStatus.Text = "Connecting...";
        try
        {
            _helper = _profile!.CreateHelper();
            await _helper.TestConnectionAsync();

            lblConnStatus.ForeColor = System.Drawing.Color.Green;
            lblConnStatus.Text = $"✔ Connected: {_profile.Server}";

            var databases = await _helper.GetDatabasesAsync();
            cboDbNameB.Items.Clear();
            cboDbNameZ.Items.Clear();
            foreach (var db in databases)
            {
                cboDbNameB.Items.Add(db);
                cboDbNameZ.Items.Add(db);
            }

            const string defaultDb = "BLogicPOS7";
            cboDbNameB.Text = databases.Contains(defaultDb) ? defaultDb : (databases.FirstOrDefault() ?? defaultDb);
            cboDbNameZ.Text = cboDbNameB.Text;

            var dataPath = await _helper.GetDefaultDataPathAsync();
            txtDataPathZ.Text = dataPath;

            UpdateBackupSqlPreview();
            UpdateZipRestoreSqlPreview();
        }
        catch (Exception ex)
        {
            lblConnStatus.ForeColor = System.Drawing.Color.Red;
            lblConnStatus.Text = $"✘ Connection error: {ex.Message}";
            _helper = null;
        }
    }

    // ── Backup tab ─────────────────────────────────────────────────────────

    private void btnBrowseDir_Click(object sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Select folder to save backup"
        };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            txtBackupDir.Text = dlg.SelectedPath;
            UpdateBackupSqlPreview();
        }
    }

    private void UpdateBackupSqlPreview()
    {
        var dbName    = cboDbNameB.Text.Trim();
        var backupDir = txtBackupDir.Text.Trim();

        if (string.IsNullOrWhiteSpace(dbName))    dbName    = "TenDatabase";
        if (string.IsNullOrWhiteSpace(backupDir)) backupDir = @"D:\Backup";

        var timestamp = "{yyyy_dd_MM__HH_mm_ss}";
        txtSqlPreviewB.Text =
            $"BACKUP DATABASE [{dbName}]\r\n" +
            $"TO DISK = N'{backupDir}\\{dbName}_{timestamp}.bak'\r\n" +
            $"WITH\r\n" +
            $"    FORMAT,\r\n" +
            $"    INIT,\r\n" +
            $"    COMPRESSION,\r\n" +
            $"    STATS = 10;\r\n" +
            $"GO";
    }

    private void cboDbNameB_TextChanged(object sender, EventArgs e) => UpdateBackupSqlPreview();
    private void txtBackupDir_TextChanged(object sender, EventArgs e) => UpdateBackupSqlPreview();

    private async void btnExecuteBackup_Click(object sender, EventArgs e)
    {
        if (_helper == null)
        {
            MessageBox.Show("Please wait for SQL Server connection.", "Not Connected",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var dbName    = cboDbNameB.Text.Trim();
        var backupDir = txtBackupDir.Text.Trim();

        if (string.IsNullOrWhiteSpace(dbName))
        {
            MessageBox.Show("Please select database name.", "Missing Info",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(backupDir))
        {
            MessageBox.Show("Please select backup folder.", "Missing Info",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var timestamp = DateTime.Now.ToString("yyyy_dd_MM__HH_mm_ss");
        var filePath  = System.IO.Path.Combine(backupDir, $"{dbName}_{timestamp}.bak");
        var sql =
            $"BACKUP DATABASE [{dbName}]\r\n" +
            $"TO DISK = N'{filePath}'\r\n" +
            $"WITH FORMAT, INIT, COMPRESSION, STATS = 10;\r\nGO";

        AppBusyState.IsBusy = true;
        SetTabEnabled(tabBackup, false, pbBackup, lblBackupStatus);
        pbBackup.Visible = true;
        lblBackupStatus.ForeColor = System.Drawing.Color.DarkGray;
        lblBackupStatus.Text = "Executing backup, please wait...";

        try
        {
            await _helper.ExecuteSqlBatchesAsync(sql);
            lblBackupStatus.ForeColor = System.Drawing.Color.Green;
            lblBackupStatus.Text = $"✔ Backup '{dbName}' successful → {filePath}";
            MessageBox.Show($"Backup succeeded!\n{filePath}", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            lblBackupStatus.ForeColor = System.Drawing.Color.Red;
            lblBackupStatus.Text = $"✘ Error: {ex.Message}";
            MessageBox.Show($"Backup error:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetTabEnabled(tabBackup, true);
            pbBackup.Visible = false;
            AppBusyState.IsBusy = false;
        }
    }

    // ── Restore from ZIP tab ───────────────────────────────────────────────

    private void btnBrowseZip_Click(object sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title  = "Select ZIP file",
            Filter = "ZIP files (*.zip)|*.zip|All files (*.*)|*.*"
        };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            txtZipPath.Text = dlg.FileName;
            ScanZipForBakFiles(dlg.FileName);
        }
    }

    private void btnScanZip_Click(object sender, EventArgs e)
    {
        var zipPath = txtZipPath.Text.Trim();
        if (string.IsNullOrWhiteSpace(zipPath))
        {
            MessageBox.Show("Please select a ZIP file first.", "Missing Info",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        ScanZipForBakFiles(zipPath);
    }

    private void ScanZipForBakFiles(string zipPath)
    {
        lvwBakFiles.Items.Clear();
        lblZipRestoreStatus.ForeColor = System.Drawing.Color.DarkGray;
        lblZipRestoreStatus.Text = "Scanning ZIP...";
        try
        {
            using var archive = System.IO.Compression.ZipFile.OpenRead(zipPath);
            var bakEntries = archive.Entries
                .Where(e => e.FullName.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var entry in bakEntries)
            {
                var item = new ListViewItem(System.IO.Path.GetFileName(entry.FullName));
                item.SubItems.Add(entry.FullName);
                item.SubItems.Add((entry.Length / 1024L).ToString("N0") + " KB");
                item.Tag = entry.FullName;
                lvwBakFiles.Items.Add(item);
            }

            if (bakEntries.Count > 0)
            {
                lblZipRestoreStatus.ForeColor = System.Drawing.Color.DarkGreen;
                lblZipRestoreStatus.Text = $"Found {bakEntries.Count} .bak file(s). Select one to restore.";
            }
            else
            {
                lblZipRestoreStatus.ForeColor = System.Drawing.Color.DarkOrange;
                lblZipRestoreStatus.Text = "No .bak files found in ZIP.";
            }
        }
        catch (Exception ex)
        {
            lblZipRestoreStatus.ForeColor = System.Drawing.Color.Red;
            lblZipRestoreStatus.Text = $"✘ Error reading ZIP: {ex.Message}";
        }
    }

    private void lvwBakFiles_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lvwBakFiles.SelectedItems.Count > 0)
        {
            txtBakDirectPath.Clear();
            ApplyDefaultsForBak(lvwBakFiles.SelectedItems[0].Text);
        }
        UpdateZipRestoreSqlPreview();
    }

    /// <summary>
    /// Inspects the .bak file name and applies the matching defaults
    /// (DB name, template radio, "clear mail data" flag).
    /// </summary>
    private void ApplyDefaultsForBak(string bakName)
    {
        if (string.IsNullOrEmpty(bakName)) return;

        if (bakName.Contains("BLOGICPOS7", StringComparison.OrdinalIgnoreCase))
        {
            cboDbNameZ.Text = "BLogicPOS7";
            rbBlogicZ.Checked = true;
            chkClearMailData.Checked = true;
        }
        else if (bakName.Contains("MERCHANT", StringComparison.OrdinalIgnoreCase))
        {
            cboDbNameZ.Text = "Merchant";
            rbMerchantZ.Checked = true;
            chkClearMailData.Checked = false;
        }
        else if (bakName.Contains("BLogicEmailService", StringComparison.OrdinalIgnoreCase))
        {
            cboDbNameZ.Text = "BLogicEmailService";
            rbMailZ.Checked = true;
            chkClearMailData.Checked = false;
        }
    }

    private void cboDbNameZ_TextChanged(object sender, EventArgs e)    => UpdateZipRestoreSqlPreview();
    private void txtDataPathZ_TextChanged(object sender, EventArgs e)  => UpdateZipRestoreSqlPreview();
    private void rbBlogicZ_CheckedChanged(object sender, EventArgs e)  => UpdateZipRestoreSqlPreview();
    private void rbMerchantZ_CheckedChanged(object sender, EventArgs e) => UpdateZipRestoreSqlPreview();
    private void rbMailZ_CheckedChanged(object sender, EventArgs e)     => UpdateZipRestoreSqlPreview();

    private void txtBakDirectPath_TextChanged(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtBakDirectPath.Text))
        {
            if (lvwBakFiles.SelectedItems.Count > 0)
                lvwBakFiles.SelectedItems[0].Selected = false;
            ApplyDefaultsForBak(System.IO.Path.GetFileName(txtBakDirectPath.Text));
        }
        UpdateZipRestoreSqlPreview();
    }

    private void btnBrowseBakDirect_Click(object sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title  = "Select .bak file",
            Filter = "Backup files (*.bak)|*.bak|All files (*.*)|*.*"
        };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            if (lvwBakFiles.SelectedItems.Count > 0)
                lvwBakFiles.SelectedItems[0].Selected = false;
            txtBakDirectPath.Text = dlg.FileName;
        }
    }

    private void UpdateZipRestoreSqlPreview()
    {
        var dbName     = cboDbNameZ.Text.Trim();
        var dataPath   = txtDataPathZ.Text.Trim();
        var directPath = txtBakDirectPath.Text.Trim();

        string bakPlaceholder;
        if (!string.IsNullOrWhiteSpace(directPath))
        {
            bakPlaceholder = directPath;
            if (string.IsNullOrWhiteSpace(dbName))
                dbName = System.IO.Path.GetFileNameWithoutExtension(directPath);
        }
        else if (lvwBakFiles.SelectedItems.Count > 0)
        {
            var selectedName = lvwBakFiles.SelectedItems[0].Text;
            bakPlaceholder = $@"[TEMP]\{selectedName}";
            if (string.IsNullOrWhiteSpace(dbName))
                dbName = System.IO.Path.GetFileNameWithoutExtension(selectedName);
        }
        else
        {
            bakPlaceholder = "[select a .bak from the list above or browse directly]";
        }

        if (string.IsNullOrWhiteSpace(dbName))
            dbName = "TenDatabase";
        if (string.IsNullOrWhiteSpace(dataPath))
            dataPath = @"C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA";

        var template = rbMailZ.Checked ? MailTemplate
                     : rbMerchantZ.Checked ? MerchantTemplate
                     : BlogicTemplate;
        txtSqlPreviewZ.Text = template
            .Replace("{DB_NAME}",   dbName)
            .Replace("{BAK_PATH}",  bakPlaceholder)
            .Replace("{DATA_PATH}", dataPath);
    }

    private async void btnExecuteRestoreFromZip_Click(object sender, EventArgs e)
    {
        if (_helper == null)
        {
            MessageBox.Show("Please wait for SQL Server connection.", "Not Connected",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var dbName     = cboDbNameZ.Text.Trim();
        var dataPath   = txtDataPathZ.Text.Trim();
        var directPath = txtBakDirectPath.Text.Trim();
        var useZip     = string.IsNullOrWhiteSpace(directPath);

        if (useZip && string.IsNullOrWhiteSpace(txtZipPath.Text))
        {
            MessageBox.Show("Please select a ZIP file or browse a .bak file directly.", "Missing Info",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (useZip && lvwBakFiles.SelectedItems.Count == 0)
        {
            MessageBox.Show("Please select a .bak file from the list.", "Missing Info",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(dbName))
        {
            MessageBox.Show("Please enter database name.", "Missing Info",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(dataPath))
        {
            MessageBox.Show("Please enter data path.", "Missing Info",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"Are you sure you want to RESTORE database '{dbName}'?\n\nThis will overwrite existing data!",
            "Confirm Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        AppBusyState.IsBusy = true;
        SetTabEnabled(tabRestoreZip, false, pbRestore, lblZipRestoreStatus);
        pbRestore.Visible = true;
        lblZipRestoreStatus.ForeColor = System.Drawing.Color.DarkGray;

        var tempBakPath = string.Empty;
        var restoreSql = string.Empty;
        try
        {
            string bakPath;
            if (useZip)
            {
                var entryFullName = lvwBakFiles.SelectedItems[0].Tag as string ?? string.Empty;
                var zipPath = txtZipPath.Text.Trim();
                // Extract to SQL Server data directory so the service account can access it
                tempBakPath = System.IO.Path.Combine(
                    dataPath,
                    $"blogic_restore_{Guid.NewGuid():N}.bak");
                lblZipRestoreStatus.Text = "Extracting .bak from ZIP, please wait...";
                using (var archive = System.IO.Compression.ZipFile.OpenRead(zipPath))
                {
                    var entry = archive.GetEntry(entryFullName)
                        ?? throw new InvalidOperationException($"Entry '{entryFullName}' not found in ZIP.");
                    using var entryStream = entry.Open();
                    using var fileStream  = System.IO.File.Create(tempBakPath);
                    await entryStream.CopyToAsync(fileStream);
                }
                bakPath = tempBakPath;
            }
            else
            {
                // A .bak outside the data directory is usually unreadable by the SQL Server
                // service account (OS error 5), so stage a copy the service can always read.
                if (IsUnderDirectory(directPath, dataPath))
                {
                    bakPath = directPath;
                }
                else
                {
                    tempBakPath = System.IO.Path.Combine(
                        dataPath, $"blogic_restore_{Guid.NewGuid():N}.bak");
                    lblZipRestoreStatus.Text = "Staging .bak into data directory, please wait...";
                    await Task.Run(() => System.IO.File.Copy(directPath, tempBakPath, overwrite: true));
                    bakPath = tempBakPath;
                }
            }

            lblZipRestoreStatus.Text = "Executing restore, please wait...";

            var template = rbMailZ.Checked ? MailTemplate
                     : rbMerchantZ.Checked ? MerchantTemplate
                     : BlogicTemplate;
            var sql = template
                .Replace("{DB_NAME}",   dbName)
                .Replace("{BAK_PATH}",  bakPath)
                .Replace("{DATA_PATH}", dataPath);
            restoreSql = sql;

            await _helper.ExecuteSqlBatchesAsync(sql);

            if (chkClearMailData.Checked)
            {
                lblZipRestoreStatus.Text = "Clearing mail & SevenShift data...";
                const string cleanupSql =
                    "DELETE dbo.EmailInfos\r\nGO\r\n" +
                    "UPDATE dbo.Configurations SET ConfigurationValue = '' WHERE ConfigurationName = 'SystemConfigValue.SevenShiftCompanyID'\r\nGO\r\n" +
                    "UPDATE dbo.Configurations SET ConfigurationValue = '' WHERE ConfigurationName = 'SystemConfigValue.SevenShiftLocation'\r\nGO\r\n" +
                    "UPDATE dbo.Configurations SET ConfigurationValue = '' WHERE ConfigurationName = 'SystemConfigValue.SevenShiftAPIKey'\r\nGO\r\n" +
                    "UPDATE dbo.Configurations SET ConfigurationValue = '' WHERE ConfigurationName = 'SystemConfigValue.CurrentSevenShiftLocation'\r\nGO\r\n" +
                    "UPDATE dbo.Employees SET SevenShiftsUserID = 0\r\nGO\r\n" +
                    "UPDATE dbo.Roles SET SevenShiftsRoleID = 0\r\nGO\r\n" +
                    "UPDATE dbo.SevenShiftDepartment SET LocationID = 0\r\nGO";
                await _helper.ExecuteSqlBatchesAsync(cleanupSql, dbName);
            }

            var okLog = SaveRestoreLog(dbName, restoreSql, true, null);
            lblZipRestoreStatus.ForeColor = System.Drawing.Color.Green;
            lblZipRestoreStatus.Text = $"✔ Restore '{dbName}' successful!";
            MessageBox.Show(
                $"Restore database '{dbName}' succeeded!" +
                (okLog != null ? $"\n\nLog: {okLog}" : ""),
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            // A failed RESTORE can leave the DB stuck in SINGLE_USER (the template's
            // MULTI_USER batch never ran). Force it back so a retry isn't blocked.
            if (_helper != null && !string.IsNullOrWhiteSpace(dbName))
                await _helper.TrySetMultiUserAsync(dbName);

            var failLog = SaveRestoreLog(dbName, restoreSql, false, ex.ToString());
            lblZipRestoreStatus.ForeColor = System.Drawing.Color.Red;
            lblZipRestoreStatus.Text = $"✘ Error: {ex.Message}";
            MessageBox.Show(
                $"Restore error:\n{ex.Message}" +
                (failLog != null ? $"\n\nLog saved: {failLog}" : ""),
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetTabEnabled(tabRestoreZip, true);
            pbRestore.Visible = false;
            AppBusyState.IsBusy = false;
            if (!string.IsNullOrEmpty(tempBakPath) && System.IO.File.Exists(tempBakPath))
            {
                try { System.IO.File.Delete(tempBakPath); } catch { /* ignore cleanup error */ }
            }
        }
    }

    /// <summary>Writes the executed SQL plus outcome to restore_logs\, like the build logs.
    /// Returns null on failure — logging must never break the restore flow.</summary>
    private static string? SaveRestoreLog(string dbName, string sql, bool success, string? error)
    {
        try
        {
            var dir = System.IO.Path.Combine(AppContext.BaseDirectory, "restore_logs");
            System.IO.Directory.CreateDirectory(dir);

            var invalid = System.IO.Path.GetInvalidFileNameChars();
            var safe = new string((dbName ?? "db")
                .Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
            if (string.IsNullOrWhiteSpace(safe)) safe = "db";
            var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var path = System.IO.Path.Combine(dir, $"{safe}_{ts}_{(success ? "ok" : "failed")}.log");

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== BLogicDevTool Restore Log ===");
            sb.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Database:  {dbName}");
            sb.AppendLine($"Status:    {(success ? "SUCCESS" : "FAILED")}");
            sb.AppendLine(new string('-', 70));
            sb.AppendLine("[SQL]");
            sb.AppendLine(sql);
            if (!string.IsNullOrEmpty(error))
            {
                sb.AppendLine(new string('-', 70));
                sb.AppendLine("[ERROR]");
                sb.AppendLine(error);
            }
            System.IO.File.WriteAllText(path, sb.ToString());
            return path;
        }
        catch { return null; }
    }

    /// <summary>True if <paramref name="filePath"/> lives inside <paramref name="directory"/>
    /// (or the directory itself). Used to decide whether a .bak must be staged into the
    /// SQL data directory before restore.</summary>
    private static bool IsUnderDirectory(string filePath, string directory)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(directory))
                return false;
            var file = System.IO.Path.GetFullPath(filePath);
            var dir = System.IO.Path.GetFullPath(directory)
                .TrimEnd(System.IO.Path.DirectorySeparatorChar,
                         System.IO.Path.AltDirectorySeparatorChar);
            return file.StartsWith(dir + System.IO.Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }
}
