using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace BLogicDevTool;

public partial class UnzipWorkbenchForm : UserControl
{
    private SqlConnectionProfile? _profile;

    /// <summary>ZIP the list was built from; .sql bodies are read out of it on selection.</summary>
    private string _loadedZipPath = "";

    // Errors collected during the current operation — listed in the result banner.
    private readonly List<string> _runErrors = new();
    private static readonly Font _bannerFont = new("Consolas", 18F, FontStyle.Bold);
    private readonly RichLogBuffer _log;

    public UnzipWorkbenchForm(SqlConnectionProfile? profile = null)
    {
        _profile = profile ?? SqlSessionStore.Current;
        InitializeComponent();
        _log = new RichLogBuffer(txtLog, LineColor);
        SqlSessionStore.ProfileChanged += OnSessionProfileChanged;
        Disposed += (_, _) =>
        {
            SqlSessionStore.ProfileChanged -= OnSessionProfileChanged;
            _log.Dispose();
        };
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
            UpdateClearDownloadsButton();
        }
    }

    private static long DownloadFolderSize()
    {
        if (!Directory.Exists(DownloadCacheDir)) return 0;
        try
        {
            return Directory.GetFiles(DownloadCacheDir, "*", SearchOption.AllDirectories)
                .Sum(f => new FileInfo(f).Length);
        }
        catch { return 0; }
    }

    private void UpdateClearDownloadsButton()
    {
        var size = DownloadFolderSize();
        btnClearDownloads.Text = size > 0
            ? $"🗑 Clear downloads ({WebDavEntry.FormatSize(size)})"
            : "🗑 Clear downloads (empty)";
    }

    private void btnClearDownloads_Click(object sender, EventArgs e)
    {
        if (!Directory.Exists(DownloadCacheDir))
        {
            MessageBox.Show($"Nothing has been downloaded yet.\n\n{DownloadCacheDir}",
                "Clear Downloads", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // The ZIP currently loaded in the workbench is kept, so Run / Deploy keep working.
        var loaded = txtZipPath.Text.Trim();
        var all = Directory.GetFiles(DownloadCacheDir, "*", SearchOption.AllDirectories);
        var files = all
            .Where(f => !string.Equals(f, loaded, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var kept = all.Length - files.Count;

        if (files.Count == 0)
        {
            MessageBox.Show(
                kept > 0
                    ? "Only the ZIP currently loaded is left — it is kept so Run / Deploy keep working."
                    : "The downloads folder is already empty.",
                "Clear Downloads", MessageBoxButtons.OK, MessageBoxIcon.Information);
            UpdateClearDownloadsButton();
            return;
        }

        long total = 0;
        foreach (var f in files)
        {
            try { total += new FileInfo(f).Length; } catch { /* size is only for the prompt */ }
        }

        if (MessageBox.Show(
                $"Delete {files.Count} downloaded file(s) — {WebDavEntry.FormatSize(total)}?\n\n" +
                $"{DownloadCacheDir}\n\n" +
                (kept > 0 ? "The ZIP currently loaded is kept.\n\n" : "") +
                "They can be downloaded from the server again at any time.",
                "Clear Downloads", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        int deleted = 0, failed = 0;
        foreach (var f in files)
        {
            try
            {
                File.SetAttributes(f, FileAttributes.Normal);
                File.Delete(f);
                deleted++;
            }
            catch (Exception ex)
            {
                failed++;
                AppendLog($"  ✘ delete {f}: {ex.Message}");
            }
        }

        AppendLog($"🗑 Cleared {deleted} download(s), {WebDavEntry.FormatSize(total)} freed."
                  + (failed > 0 ? $"  ⚠ {failed} failure(s)." : ""));
        SetStatus(failed == 0
            ? $"✔ Cleared {deleted} download(s) — {WebDavEntry.FormatSize(total)} freed."
            : $"⚠ Cleared {deleted}, {failed} could not be deleted.");
        UpdateClearDownloadsButton();
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
        // A profile is present → the DB combo must be usable (it may have been
        // disabled earlier when the tab opened without a login).
        cboDatabase.Enabled = true;
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

    // ── ZIP from a WebDAV / HTTP URL ──────────────────────────────────────────

    /// <summary>Non-null only while a download is in flight; then btnFromUrl cancels it.</summary>
    private CancellationTokenSource? _downloadCts;

    /// <summary>Next to the exe, like backups/ and build_logs/ — keeps the ZIPs where the
    /// tool lives instead of hiding 300 MB downloads in %TEMP%.</summary>
    private static string DownloadCacheDir =>
        Path.Combine(AppContext.BaseDirectory, "downloads");

    private async void btnFromUrl_Click(object sender, EventArgs e)
    {
        if (_downloadCts != null) { _downloadCts.Cancel(); return; }

        // A URL pasted straight into the path box is fetched with the saved credentials;
        // without them the browser has to run once (it is what stores them). TryCreate, not
        // StartsWith: a half-typed "http://" would throw out of this async void and kill the app.
        var pasted = txtZipPath.Text.Trim();
        var shareLink = WebDavClient.IsShareLink(pasted) ? pasted : null;
        if (shareLink != null)   // soteria://files/... → http URL under the saved server
            pasted = WebDavClient.ResolveLink(WebDavSettingsStore.Load().BaseUrl, shareLink) ?? "";
        var isHttp = Uri.TryCreate(pasted, UriKind.Absolute, out var pastedUri)
                     && (pastedUri.Scheme == Uri.UriSchemeHttp || pastedUri.Scheme == Uri.UriSchemeHttps);
        if (isHttp && pastedUri!.AbsolutePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            var saved = WebDavSettingsStore.Load();
            var savedPass = WebDavSettingsStore.Unprotect(saved.ProtectedPassword);
            if (saved.Username.Length > 0 && savedPass.Length > 0 && saved.BaseUrl.Length > 0
                && pasted.StartsWith(saved.BaseUrl, StringComparison.OrdinalIgnoreCase))
            {
                var name = Uri.UnescapeDataString(pastedUri.Segments[^1].TrimEnd('/'));
                await DownloadZipAsync(saved.BaseUrl, saved.Username, savedPass, pasted, name, -1);
                return;
            }
        }

        // A folder link (or a file link without saved credentials) opens the browser right there.
        using var dlg = new WebDavBrowserForm(shareLink ?? (isHttp ? pasted : null));
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
        await DownloadZipAsync(dlg.BaseUrl, dlg.Username, dlg.Password,
            dlg.SelectedUrl, dlg.SelectedName, dlg.SelectedLength);
    }

    private async Task DownloadZipAsync(string baseUrl, string user, string pass,
        string fileUrl, string fileName, long remoteLength)
    {
        var destPath = Path.Combine(DownloadCacheDir, SanitizeFileName(fileName));

        // Always fetch a fresh copy and overwrite: a patch ZIP can be re-published under the
        // same name and size, so an older local file must never stand in for it.
        var replacing = File.Exists(destPath);

        _runErrors.Clear();
        // Everything after the field is set lives in the try: were anything below to throw,
        // a non-null _downloadCts would leave btnFromUrl stuck as a cancel button for good.
        var cts = new CancellationTokenSource();
        _downloadCts = cts;
        var sw = Stopwatch.StartNew();
        try
        {
            _log.Clear();
            SetBusy(true, $"Downloading {fileName}...");
            btnFromUrl.Text = "✖ Cancel";
            progressBar.Style = remoteLength != 0 ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
            progressBar.Value = 0;

            AppendLog($">>> Download: {fileUrl}");
            if (replacing) AppendLog($"  Overwriting the earlier download: {destPath}");

            using var client = new WebDavClient(baseUrl, user, pass);
            var progress = new Progress<(long Read, long Total)>(p => ReportDownload(p, sw));
            await client.DownloadAsync(fileUrl, destPath, progress, cts.Token);

            var size = new FileInfo(destPath).Length;
            AppendLog($"✔ {WebDavEntry.FormatSize(size)} in {sw.Elapsed.TotalSeconds:0.#}s");
            AppendLog($"  Saved to: {destPath}");

            UseDownloadedZip(destPath);
            // Say what the ZIP turned out to hold — the banner below would otherwise
            // overwrite LoadZipFile's status and leave an empty list unexplained.
            var found = lstSqlFiles.Items.Count;
            AppendLog(found == 0
                ? "⚠ No .sql / .exe inside — nothing to run. Deploy still applies any folder that matches a mapping."
                : $"✔ {found} runnable file(s) listed; '{lstSqlFiles.SelectedItem}' selected.");
            ShowResult(true, $"{fileName} downloaded ({WebDavEntry.FormatSize(size)})"
                + (found == 0 ? " — no .sql/.exe inside." : $" — {found} runnable file(s)."));
        }
        catch (OperationCanceledException)
        {
            AppendLog("⚠ Download cancelled.");
            SetStatus("⚠ Download cancelled.");
        }
        catch (Exception ex)
        {
            AppendLog($"✘ Download failed: {ex.Message}");
            _runErrors.Add($"{fileName}: {ex.Message}");
            ShowResult(false, null);
        }
        finally
        {
            _downloadCts = null;
            cts.Dispose();
            progressBar.Style = ProgressBarStyle.Marquee;
            btnFromUrl.Text = "🌐 URL...";
            SetBusy(false);
        }
    }

    private void ReportDownload((long Read, long Total) p, Stopwatch sw)
    {
        // Progress<T> marshals asynchronously, so a report can land after the download
        // finished and the UI was reset — that one must not put the bar back.
        if (_downloadCts == null) return;

        var elapsed = sw.Elapsed.TotalSeconds;
        var speed = elapsed > 0.5 ? $"  —  {WebDavEntry.FormatSize((long)(p.Read / elapsed))}/s" : "";
        if (p.Total > 0)
        {
            var pct = (int)(p.Read * 100 / p.Total);
            progressBar.Value = Math.Min(100, pct);
            SetStatus($"⬇ {WebDavEntry.FormatSize(p.Read)} / {WebDavEntry.FormatSize(p.Total)}" +
                      $"  ({pct}%){speed}");
        }
        else
        {
            // No Content-Length → percentage is unknowable, so fall back to the marquee.
            progressBar.Style = ProgressBarStyle.Marquee;
            SetStatus($"⬇ {WebDavEntry.FormatSize(p.Read)}{speed}");
        }
    }

    /// <summary>Points the workbench at the downloaded file — from here on it is a normal ZIP.</summary>
    private void UseDownloadedZip(string path)
    {
        txtZipPath.Text = path;
        LoadZipFile(path);
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
        return name.Trim().Length == 0 ? "download.zip" : name;
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
        _loadedZipPath = zipPath;
        lstSqlFiles.Items.Clear();
        txtSqlPreview.Clear();

        try
        {
            // Names only: reading every script here froze the UI on the big ZIPs the
            // download flow pulls in, and most of them are never previewed.
            using var archive = ZipFile.OpenRead(zipPath);
            foreach (var entry in archive.Entries
                .Where(e => e.Name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase)
                         || e.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.FullName))
            {
                lstSqlFiles.Items.Add(entry.FullName);
            }

            // Preselect the first entry so the preview fills in and Run works immediately.
            if (lstSqlFiles.Items.Count > 0) lstSqlFiles.SelectedIndex = 0;

            SetStatus(lstSqlFiles.Items.Count == 0
                ? "⚠ No .sql or .exe files found in the ZIP."
                : $"✔ Loaded {lstSqlFiles.Items.Count} runnable file(s) (.sql / .exe).");
        }
        catch (Exception ex)
        {
            SetStatus($"✘ ZIP read error: {ex.Message}");
        }

        UpdateButtonStates();
    }

    private static bool IsExe(string name) =>
        name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);

    /// <summary>Reads one .sql entry out of the loaded ZIP on demand; any problem is shown
    /// in the preview box instead of thrown, so selecting a file can never fail.</summary>
    private string ReadSqlEntry(string entryFullName)
    {
        if (!File.Exists(_loadedZipPath))
            return $"[ZIP not found]{Environment.NewLine}{_loadedZipPath}";
        try
        {
            using var archive = ZipFile.OpenRead(_loadedZipPath);
            var entry = archive.GetEntry(entryFullName);
            if (entry == null)
                return $"[Entry no longer in the ZIP]{Environment.NewLine}{entryFullName}";
            using var reader = new StreamReader(entry.Open());
            return NormalizeNewlines(reader.ReadToEnd());
        }
        catch (Exception ex)
        {
            return $"[Could not read]{Environment.NewLine}{entryFullName}{Environment.NewLine}{ex.Message}";
        }
    }

    /// <summary>Converts any mix of CR / LF / CRLF to CRLF so the multiline TextBox
    /// renders line breaks correctly (it only recognises CRLF).</summary>
    private static string NormalizeNewlines(string s) =>
        s.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");

    /// <summary>The store-database placeholder used in scripts; only USE statements
    /// targeting THIS database get re-pointed to the selected DB. Other databases
    /// (Merchant, master, BLogicEmailService, …) are left untouched.</summary>
    private const string StoreDbPlaceholder = "BLogicPOS7";

    /// <summary>
    /// Rewrites only line-leading <c>USE [BLogicPOS7]</c> / <c>USE BLogicPOS7</c>
    /// (case-insensitive) to <c>USE [dbName]</c> so the script targets the selected
    /// store database. USE statements for any other database are preserved, and a
    /// trailing comment on the line is kept.
    /// </summary>
    private static string ReplaceUseDatabase(string sql, string dbName) =>
        Regex.Replace(
            sql,
            @"(?im)^(\s*USE\s+)(\[[^\]]+\]|[^\s;]+)",
            m =>
            {
                var target = m.Groups[2].Value.Trim().Trim('[', ']');
                return target.Equals(StoreDbPlaceholder, StringComparison.OrdinalIgnoreCase)
                    ? m.Groups[1].Value + "[" + dbName + "]"
                    : m.Value; // leave Merchant / master / etc. as-is
            });

    private void lstSqlFiles_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lstSqlFiles.SelectedItem is not string key) return;

        if (IsExe(key))
        {
            txtSqlPreview.ReadOnly = true;
            txtSqlPreview.Text =
                $"[Executable]{Environment.NewLine}{key}{Environment.NewLine}{Environment.NewLine}" +
                "Click \"▶ Run EXE\" to extract this file (with the files in its folder) to a temp " +
                "directory and run it.";
            btnRun.Text = "▶ Run EXE";
        }
        else
        {
            txtSqlPreview.ReadOnly = false;
            txtSqlPreview.Text = ReadSqlEntry(key);
            btnRun.Text = "▶ Run SQL";
        }
        txtSqlPreview.SelectionStart = 0;
        txtSqlPreview.ScrollToCaret();
    }

    // ── Run SQL ───────────────────────────────────────────────────────────────

    private async void btnRun_Click(object sender, EventArgs e)
    {
        if (lstSqlFiles.SelectedItem is not string key)
        {
            MessageBox.Show("Please select a .sql or .exe file from the list.", "Run",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (IsExe(key))
        {
            await RunExeFromZipAsync(key);
            return;
        }

        if (_profile == null)
        {
            MessageBox.Show("No SQL connection — open this tool from the menu with an active login session.", "Run SQL",
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

        _runErrors.Clear();
        SetBusy(true, $"Running '{Path.GetFileName(key)}' on [{dbName}]...");
        AppendLog($"\n>>> SQL: {key}  →  [{dbName}]  at {DateTime.Now:HH:mm:ss}");

        try
        {
            // Re-target any USE [...] in the script to the selected database, so the
            // script always runs against the DB the user picked (not the one hardcoded
            // in the file, e.g. "USE [BLogicPOS7]").
            var runSql = ReplaceUseDatabase(sql, dbName);
            if (runSql != sql)
                AppendLog($"  ↳ Re-targeted USE [...] → [{dbName}]");

            var helper = _profile.CreateHelper();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            // Run the whole script like SSMS: a failing batch is logged but does NOT
            // stop the remaining batches.
            var runResult = await helper.ExecuteSqlBatchesContinueOnErrorAsync(
                runSql, dbName, msg => AppendLog(msg));
            sw.Stop();

            if (runResult.FailedBatches == 0)
            {
                AppendLog($"✔ SQL completed in {sw.Elapsed.TotalSeconds:F1}s — {runResult.TotalBatches} batch(es), no errors.");
                ShowResult(true, $"{Path.GetFileName(key)} executed on [{dbName}].");
            }
            else
            {
                var tally = $"{runResult.FailedBatches}/{runResult.TotalBatches} batch(es) failed, " +
                            $"{runResult.Errors.Count} error(s)";
                AppendLog(runResult.Aborted
                    ? $"✘ SQL stopped after {sw.Elapsed.TotalSeconds:F1}s — {tally}. " +
                      $"Aborted: {runResult.AbortReason}."
                    : $"⚠ SQL finished in {sw.Elapsed.TotalSeconds:F1}s — {tally}. " +
                      "Remaining batches were still run.");
                foreach (var err in runResult.Errors)
                    _runErrors.Add($"{Path.GetFileName(key)}: {err}");
                ShowResult(false, null);
            }
        }
        catch (Exception ex)
        {
            // Connection-level or unexpected failure (couldn't even start the run).
            AppendLog($"✘ SQL Error: {ex.Message}");
            _runErrors.Add($"{Path.GetFileName(key)}: {ex.Message}");
            ShowResult(false, null);
        }
        finally
        {
            SetBusy(false);
        }
    }

    // ── Run EXE ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Extracts the selected .exe — together with the other files in its folder inside
    /// the ZIP (so its dependencies come along) — into a temp directory, runs it, logs
    /// the output and exit code, then cleans up.
    /// </summary>
    private async Task RunExeFromZipAsync(string entryFullName)
    {
        var zipPath = txtZipPath.Text.Trim();
        if (!File.Exists(zipPath))
        {
            MessageBox.Show("Please select a valid ZIP file.", "Run EXE",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show(
                $"Run this executable?\n\n{entryFullName}\n\n" +
                "It will be extracted (with the files in its folder) to a temp directory and run.",
                "Confirm Run EXE", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        var tempDir = Path.Combine(Path.GetTempPath(), $"BLogicDevTool_run_{Guid.NewGuid():N}");
        _runErrors.Clear();
        SetBusy(true, $"Running '{Path.GetFileName(entryFullName)}'...");
        AppendLog($"\n>>> EXE: {entryFullName}  at {DateTime.Now:HH:mm:ss}");
        try
        {
            // Folder prefix of the exe inside the ZIP ("" if it sits at the root).
            int slash = entryFullName.LastIndexOf('/');
            var dirPrefix = slash >= 0 ? entryFullName.Substring(0, slash + 1) : "";
            string exePath = "";

            await Task.Run(() =>
            {
                using var archive = ZipFile.OpenRead(zipPath);
                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name)) continue;
                    if (!entry.FullName.StartsWith(dirPrefix, StringComparison.OrdinalIgnoreCase)) continue;

                    var rel = entry.FullName.Substring(dirPrefix.Length)
                                   .Replace('/', Path.DirectorySeparatorChar);
                    var outPath = Path.Combine(tempDir, rel);
                    var outDir = Path.GetDirectoryName(outPath);
                    if (!string.IsNullOrEmpty(outDir)) Directory.CreateDirectory(outDir);
                    entry.ExtractToFile(outPath, overwrite: true);
                    if (entry.FullName.Equals(entryFullName, StringComparison.OrdinalIgnoreCase))
                        exePath = outPath;
                }
            });

            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
            {
                AppendLog("✘ Could not extract the executable from the ZIP.");
                _runErrors.Add($"{Path.GetFileName(entryFullName)}: could not extract from ZIP.");
                ShowResult(false, null);
                return;
            }
            AppendLog($"  Extracted to: {tempDir}");

            var (exit, output) = await RunExternalProcessAsync(exePath, Path.GetDirectoryName(exePath)!);
            if (!string.IsNullOrWhiteSpace(output)) AppendLog(output.TrimEnd());
            if (exit == 0)
            {
                AppendLog("✔ Exited with code 0.");
                ShowResult(true, $"{Path.GetFileName(entryFullName)} ran successfully (exit 0).");
            }
            else
            {
                AppendLog($"✘ Exited with code {exit}.");
                _runErrors.Add($"{Path.GetFileName(entryFullName)}: exited with code {exit}.");
                ShowResult(false, null);
            }
        }
        catch (Exception ex)
        {
            AppendLog($"✘ EXE error: {ex.Message}");
            _runErrors.Add($"{Path.GetFileName(entryFullName)}: {ex.Message}");
            ShowResult(false, null);
        }
        finally
        {
            SetBusy(false);
            try { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true); }
            catch { /* best-effort cleanup */ }
        }
    }

    private static Task<(int ExitCode, string Output)> RunExternalProcessAsync(
        string exePath, string workingDir)
    {
        return Task.Run(() =>
        {
            var psi = new ProcessStartInfo(exePath)
            {
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi)
                ?? throw new InvalidOperationException("Cannot start the executable.");

            var sb = new StringBuilder();
            var sync = new object();
            void Capture(string? line) { if (line != null) lock (sync) sb.AppendLine(line); }
            proc.OutputDataReceived += (_, e) => Capture(e.Data);
            proc.ErrorDataReceived  += (_, e) => Capture(e.Data);
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            if (!proc.WaitForExit(1_800_000))
            {
                try { proc.Kill(entireProcessTree: true); } catch { }
                throw new TimeoutException("Executable timed out after 30 minutes.");
            }
            proc.WaitForExit();

            string output;
            lock (sync) output = sb.ToString();
            return (proc.ExitCode, output);
        });
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
            "Process will:\n  1. Stop IIS + Windows services running from target folders\n" +
            "  2. Back up files being replaced, then extract & copy\n  3. Restart services + IIS",
            "Confirm Deployment",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        _runErrors.Clear();
        SetBusy(true, "Deploying...");
        _log.Clear();

        try
        {
            await DeployAsync(zipPath, mappings);
            ShowResult(_runErrors.Count == 0, "Deployment finished.");
        }
        catch (Exception ex)
        {
            AppendLog($"\n✘ Fatal error: {ex.Message}");
            _runErrors.Add("Fatal: " + ex.Message);
            ShowResult(false, null);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task DeployAsync(string zipPath, List<UnzipFolderMapping> mappings)
    {
        // One backup per task, named after the ZIP (e.g. "PSS255"), under <toolDir>\backups\.
        // Write-once & accumulating: re-deploying the same task only adds files not yet
        // captured — so this single folder is the rollback point for the task.
        var taskName = UnzipBackupStore.TaskName(zipPath);
        var backupFolder = UnzipBackupStore.TaskFolder(taskName);
        var added = UnzipBackupStore.LoadAdded(taskName);
        AppendLog($">>> Task: {taskName}");

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

        var stoppedServices = await StopServicesInAsync(mappings.Select(m => m.DestinationPath));

        AppendLog($"\n>>> Step 2: Extracting '{Path.GetFileName(zipPath)}'...");
        SetStatus("Extracting and copying files...");
        int totalFiles = 0, skippedFolders = 0, newlyBackedUp = 0, newlyAdded = 0;

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
                        // Save the original into the task backup — write-once, so a later
                        // deploy of the same task only adds files not captured yet. Skip files
                        // the task itself created earlier (they have no true original).
                        if (!added.Contains(fullDest))
                        {
                            try
                            {
                                if (UnzipBackupStore.SaveOriginalIfAbsent(taskName, destPath))
                                    newlyBackedUp++;
                            }
                            catch (Exception bx)
                            {
                                AppendLog($"     ⚠ Backup failed for {relativePath}: {bx.Message}");
                            }
                        }
                    }
                    else
                    {
                        // Brand-new file → track so rollback removes it.
                        if (added.Add(fullDest)) newlyAdded++;
                    }

                    try
                    {
                        entry.ExtractToFile(destPath, overwrite: true);
                        fileCount++;
                        totalFiles++;
                        AppendLog($"     → {relativePath}");
                    }
                    catch (Exception ex)
                    {
                        _runErrors.Add($"{mapping.FolderName}/{relativePath}: {ex.Message}");
                        AppendLog($"     ✘ {relativePath}: {ex.Message}");
                    }
                }

                AppendLog($"  ✔ Copied {fileCount} file(s).");
            }
        });

        UnzipBackupStore.SaveAdded(taskName, added);

        AppendLog($"\n  Total: {totalFiles} file(s) updated.");
        AppendLog($"  💾 Backup '{taskName}' → {backupFolder}");
        AppendLog($"     {newlyBackedUp} original(s) newly saved, {newlyAdded} new file(s) tracked"
                  + $" (backup now holds {added.Count} added-file record(s)).");
        AppendLog($"  ↩ Use 'Rollback Task' to restore this task to its original state.");
        if (skippedFolders > 0)
            AppendLog($"  ⚠ {skippedFolders} folder(s) skipped (not found in ZIP).");
        }
        finally
        {
            // Always restart IIS — even if extraction failed midway — so the machine
            // never gets left with IIS stopped.
            await StartServicesAsync(stoppedServices);
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

    // ── Rollback Task ─────────────────────────────────────────────────────────

    private async void btnRestoreOriginal_Click(object sender, EventArgs e)
    {
        var zip = txtZipPath.Text.Trim();
        if (string.IsNullOrEmpty(zip))
        {
            MessageBox.Show(
                "Select the task ZIP first so the matching backup can be identified.",
                "Rollback Task", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var taskName = UnzipBackupStore.TaskName(zip);
        if (!UnzipBackupStore.HasBackup(taskName))
        {
            MessageBox.Show(
                $"No backup found for task '{taskName}'.\nDeploy this task at least once first.",
                "Rollback Task", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"Roll back task '{taskName}' to its ORIGINAL state?\n\n" +
            "Process will:\n" +
            "  1. Stop IIS + Windows services running from target folders\n" +
            "  2. Restore the original of every file this task replaced\n" +
            "  3. Delete files this task added\n" +
            "  4. Restart services + IIS",
            "Confirm Rollback",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        SetBusy(true, $"Rolling back '{taskName}'...");
        _log.Clear();
        try
        {
            await RollbackTaskAsync(taskName);
        }
        catch (Exception ex)
        {
            AppendLog($"\n✘ Fatal error: {ex.Message}");
            SetStatus("✘ Rollback failed.");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task RollbackTaskAsync(string taskName)
    {
        AppendLog($">>> Rolling back task: {taskName}");
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
            AppendLog("  Continuing rollback...");
        }

        var stoppedServices = await StopServicesInAsync(UnzipConfigStore.Load().Select(m => m.DestinationPath));

        try
        {
            AppendLog("\n>>> Step 2: Restoring original files...");
            SetStatus("Restoring files...");
            int restored = 0, removed = 0, failed = 0;
            var added = UnzipBackupStore.LoadAdded(taskName);

            await Task.Run(() =>
            {
                // 1) Put back the captured originals (overwrite current).
                foreach (var backupFile in UnzipBackupStore.EnumerateOriginals(taskName))
                {
                    var orig = UnzipBackupStore.DecodeOriginalPath(taskName, backupFile);
                    if (string.IsNullOrEmpty(orig))
                    {
                        AppendLog($"  ⚠ Cannot map backup file: {backupFile}");
                        continue;
                    }
                    try
                    {
                        var dir = Path.GetDirectoryName(orig);
                        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                        if (File.Exists(orig)) File.SetAttributes(orig, FileAttributes.Normal);
                        File.Copy(backupFile, orig, overwrite: true);
                        restored++;
                        AppendLog($"  ↩ {orig}");
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        AppendLog($"  ✘ {orig}: {ex.Message}");
                    }
                }

                // 2) Delete files this task added (did not exist originally).
                foreach (var p in added)
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
            SetStatus($"✔ Task '{taskName}' rolled back — {restored} restored, {removed} removed.");
        }
        finally
        {
            await StartServicesAsync(stoppedServices);
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

        AppendLog($"\n✔ Rollback completed at {DateTime.Now:HH:mm:ss}.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<List<string>> StopServicesInAsync(IEnumerable<string> folders)
    {
        var dirs = folders.ToList();
        var running = await Task.Run(() => WindowsServiceLock.FindRunningServicesIn(dirs));
        if (running.Count == 0) return running;
        AppendLog($">>> Stopping {running.Count} Windows service(s) running from target folders...");
        SetStatus("Stopping services...");
        return await Task.Run(() => WindowsServiceLock.Stop(running, AppendLog));
    }

    private async Task StartServicesAsync(List<string> services)
    {
        if (services.Count == 0) return;
        AppendLog("\n>>> Restarting Windows service(s)...");
        SetStatus("Starting services...");
        await Task.Run(() => WindowsServiceLock.Start(services, AppendLog));
    }

    private void AppendLog(string message)
    {
        // No marshalling on purpose: callers include worker threads echoing process output.
        _log.Append(message + Environment.NewLine);
    }

    /// <summary>Picks a colour from the line's leading marker (✔ green, ✘ red, ⚠ orange).</summary>
    private static Color LineColor(string message)
    {
        var t = message.TrimStart();
        if (t.StartsWith("✔") || t.StartsWith("↩")) return Color.LightGreen;
        if (t.StartsWith("✘")) return Color.FromArgb(255, 110, 110);
        if (t.StartsWith("⚠")) return Color.Orange;
        if (t.StartsWith(">>>")) return Color.FromArgb(120, 180, 255);
        return Color.Gainsboro;
    }

    private void AppendColored(string text, Color color, Font? font)
        => _log.Append(text, color, font);

    /// <summary>Result banner: green COMPLETE, or red FAILED listing what went wrong
    /// (which file / why) from <see cref="_runErrors"/>.</summary>
    private void ShowResult(bool success, string? summary)
    {
        if (InvokeRequired) { Invoke(() => ShowResult(success, summary)); return; }
        AppendColored(Environment.NewLine, Color.Gainsboro, null);
        if (success && _runErrors.Count == 0)
        {
            AppendColored("  ✔  COMPLETE" + Environment.NewLine, Color.LimeGreen, _bannerFont);
            if (!string.IsNullOrWhiteSpace(summary))
                AppendColored("  " + summary + Environment.NewLine, Color.LightGreen, null);
            SetStatus("✔ " + (summary ?? "Complete."));
        }
        else
        {
            AppendColored("  ✘  FAILED" + Environment.NewLine, Color.Red, _bannerFont);
            if (_runErrors.Count > 0)
            {
                AppendColored($"  {_runErrors.Count} error(s):" + Environment.NewLine,
                    Color.FromArgb(255, 110, 110), null);
                foreach (var err in _runErrors)
                    AppendColored("   • " + err + Environment.NewLine,
                        Color.FromArgb(255, 110, 110), null);
            }
            else if (!string.IsNullOrWhiteSpace(summary))
                AppendColored("  " + summary + Environment.NewLine,
                    Color.FromArgb(255, 110, 110), null);
            SetStatus("✘ Failed — see log.");
        }
        _log.Flush();   // the banner must land after every queued line
    }

    private void UpdateButtonStates()
    {
        var zipExists = File.Exists(txtZipPath.Text.Trim());
        btnDeploy.Enabled = zipExists;
        // Run is enabled whenever there are runnable files; the SQL path checks the
        // DB connection itself, the EXE path doesn't need one.
        btnRun.Enabled    = zipExists && lstSqlFiles.Items.Count > 0;
    }

    private void SetBusy(bool busy, string? statusText = null)
    {
        if (InvokeRequired) { Invoke(() => SetBusy(busy, statusText)); return; }
        AppBusyState.IsBusy = busy;
        btnRun.Enabled            = !busy && lstSqlFiles.Items.Count > 0;
        btnDeploy.Enabled         = !busy && File.Exists(txtZipPath.Text.Trim());
        btnRestoreOriginal.Enabled = !busy;
        btnBrowseZip.Enabled      = !busy;
        // Stays live through a download — it is the cancel button then.
        btnFromUrl.Enabled        = !busy || _downloadCts != null;
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
