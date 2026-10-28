namespace BLogicDevTool;

/// <summary>Browses a WebDAV share and returns the URL of the file the user picked.</summary>
public partial class WebDavBrowserForm : Form
{
    private WebDavClient? _client;
    private string _currentUrl = "";
    private List<WebDavEntry> _entries = new();
    private WebDavSettings _settings = new();

    /// <summary>Absolute URL of the chosen file; set only when DialogResult is OK.</summary>
    public string SelectedUrl { get; private set; } = "";
    public string SelectedName { get; private set; } = "";
    public long SelectedLength { get; private set; }

    /// <summary>Credentials of the live connection, so the caller can reuse them to download.</summary>
    public string BaseUrl { get; private set; } = "";
    public string Username { get; private set; } = "";
    public string Password { get; private set; } = "";

    public WebDavBrowserForm()
    {
        InitializeComponent();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _settings = WebDavSettingsStore.Load();
        txtUrl.Text = _settings.BaseUrl;
        txtUser.Text = _settings.Username;
        chkRemember.Checked = _settings.RememberPassword;
        chkZipOnly.Checked = _settings.ZipOnly;
        if (_settings.RememberPassword)
            txtPass.Text = WebDavSettingsStore.Unprotect(_settings.ProtectedPassword);

        // Credentials already on file → connect straight away and land on the last folder,
        // without ever painting them on screen.
        if (!string.IsNullOrWhiteSpace(txtUser.Text) && txtPass.Text.Length > 0)
        {
            ShowConnectionBar(false);
            await ConnectAsync(_settings.LastFolderUrl);
        }
    }

    private async void btnConnect_Click(object sender, EventArgs e) => await ConnectAsync(null);

    private async Task ConnectAsync(string? startUrl)
    {
        var url = txtUrl.Text.Trim();
        if (url.Length == 0)
        {
            SetStatus("⚠ Enter the server URL.");
            ShowConnectionBar(true);
            return;
        }

        _client?.Dispose();
        try
        {
            _client = new WebDavClient(url, txtUser.Text.Trim(), txtPass.Text);
        }
        catch (Exception ex)
        {
            _client = null;
            SetStatus($"✘ Invalid URL: {ex.Message}");
            ShowConnectionBar(true);
            return;
        }

        BaseUrl = url;
        Username = txtUser.Text.Trim();
        Password = txtPass.Text;

        var target = !string.IsNullOrWhiteSpace(startUrl)
                     && startUrl.StartsWith(url, StringComparison.OrdinalIgnoreCase)
            ? startUrl
            : _client.BaseUri.AbsoluteUri;

        var ok = await NavigateAsync(target);
        if (!ok && target != _client.BaseUri.AbsoluteUri)
            ok = await NavigateAsync(_client.BaseUri.AbsoluteUri);   // stale saved folder → fall back to root

        // Connected → fold the credential bar away so the user name and password are
        // not left sitting on screen for whoever walks past. 🔐 brings it back.
        ShowConnectionBar(!ok);   // failed → put the bar back so the credentials can be fixed
    }

    /// <summary>Shows or hides the server/user/password row.</summary>
    private void ShowConnectionBar(bool show)
    {
        pnlConn.Visible = show;
        btnServer.Text = show ? "🔒" : "🔐";
        if (show) txtPass.Focus();
    }

    private void btnServer_Click(object sender, EventArgs e)
        => ShowConnectionBar(!pnlConn.Visible);

    private async Task<bool> NavigateAsync(string url)
    {
        if (_client == null) return false;

        SetBusy(true, "Loading...");
        try
        {
            _entries = await _client.ListAsync(url);
            _currentUrl = url.EndsWith("/") ? url : url + "/";
            txtPath.Text = Uri.UnescapeDataString(new Uri(_currentUrl).AbsolutePath);
            RenderList();
            SetStatus($"✔ {_entries.Count} item(s).");
            CaptureSettings();
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            SetStatus("✘ " + ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            SetStatus($"✘ {ex.Message}");
            return false;
        }
        finally
        {
            SetBusy(false);
        }
    }

    // ── Column sorting ────────────────────────────────────────────────────────

    private int _sortColumn;
    private bool _sortAscending = true;

    private void lstItems_ColumnClick(object sender, ColumnClickEventArgs e)
    {
        if (e.Column == _sortColumn) _sortAscending = !_sortAscending;
        else { _sortColumn = e.Column; _sortAscending = true; }
        RenderList();
    }

    /// <summary>Folders stay above files as in Explorer; the clicked column orders within each group.</summary>
    private IEnumerable<WebDavEntry> SortedEntries()
    {
        var byKind = _entries.OrderByDescending(e => e.IsFolder);
        return (_sortColumn, _sortAscending) switch
        {
            (1, true)  => byKind.ThenBy(e => e.Length),
            (1, false) => byKind.ThenByDescending(e => e.Length),
            (2, true)  => byKind.ThenBy(e => e.Modified ?? DateTime.MinValue),
            (2, false) => byKind.ThenByDescending(e => e.Modified ?? DateTime.MinValue),
            (_, false) => byKind.ThenByDescending(e => e.Name, NaturalComparer.Instance),
            _          => byKind.ThenBy(e => e.Name, NaturalComparer.Instance)
        };
    }

    private void UpdateSortIndicators()
    {
        ColumnHeader[] cols = { colName, colSize, colModified };
        string[] titles = { "Name", "Size", "Modified" };
        for (var i = 0; i < cols.Length; i++)
            cols[i].Text = titles[i] + (i == _sortColumn ? (_sortAscending ? "  ▲" : "  ▼") : "");
    }

    private void RenderList()
    {
        UpdateSortIndicators();
        lstItems.BeginUpdate();
        lstItems.Items.Clear();
        foreach (var entry in SortedEntries())
        {
            if (!entry.IsFolder && chkZipOnly.Checked &&
                !entry.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                continue;

            var item = new ListViewItem((entry.IsFolder ? "📁  " : "🗄  ") + entry.Name)
            {
                Tag = entry
            };
            item.SubItems.Add(entry.SizeText);
            item.SubItems.Add(entry.Modified?.ToString("yyyy-MM-dd HH:mm") ?? "");
            lstItems.Items.Add(item);
        }
        lstItems.EndUpdate();
        UpdateOpenButton();
    }

    private void chkZipOnly_CheckedChanged(object sender, EventArgs e)
    {
        RenderList();
        CaptureSettings();
    }

    private async void btnRefresh_Click(object sender, EventArgs e)
    {
        if (_currentUrl.Length > 0) await NavigateAsync(_currentUrl);
    }

    private async void btnUp_Click(object sender, EventArgs e)
    {
        if (_client == null || _currentUrl.Length == 0) return;
        var root = _client.BaseUri.AbsoluteUri;
        if (string.Equals(_currentUrl, root, StringComparison.OrdinalIgnoreCase)) return;

        var trimmed = _currentUrl.TrimEnd('/');
        var slash = trimmed.LastIndexOf('/');
        if (slash <= 0) return;
        var parent = trimmed.Substring(0, slash + 1);
        await NavigateAsync(parent.Length < root.Length ? root : parent);
    }

    private WebDavEntry? Selected =>
        lstItems.SelectedItems.Count > 0 ? lstItems.SelectedItems[0].Tag as WebDavEntry : null;

    private void lstItems_SelectedIndexChanged(object sender, EventArgs e) => UpdateOpenButton();

    private void UpdateOpenButton()
    {
        var sel = Selected;
        btnOpen.Enabled = sel != null;
        btnOpen.Text = sel is { IsFolder: true } ? "📂 Open" : "✔ Select";
    }

    private async void lstItems_DoubleClick(object sender, EventArgs e) => await ActivateSelectionAsync();

    private async void lstItems_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.Handled = true;
            await ActivateSelectionAsync();
        }
        else if (e.KeyCode == Keys.Back)
        {
            e.Handled = true;
            btnUp_Click(sender, EventArgs.Empty);
        }
    }

    private async void btnOpen_Click(object sender, EventArgs e) => await ActivateSelectionAsync();

    private async Task ActivateSelectionAsync()
    {
        var sel = Selected;
        if (sel == null) return;

        if (sel.IsFolder)
        {
            await NavigateAsync(sel.Url);
            return;
        }

        SelectedUrl = sel.Url;
        SelectedName = sel.Name;
        SelectedLength = sel.Length;
        CaptureSettings();          // OnFormClosed writes it
        DialogResult = DialogResult.OK;
        Close();
    }

    /// <summary>Copies the UI into <see cref="_settings"/> without touching the disk.</summary>
    private void CaptureSettings()
    {
        _settings.BaseUrl = txtUrl.Text.Trim();
        // "Remember" off → keep nothing about the account on disk, not even the user name.
        _settings.Username = chkRemember.Checked ? txtUser.Text.Trim() : "";
        _settings.RememberPassword = chkRemember.Checked;
        _settings.ProtectedPassword = chkRemember.Checked
            ? WebDavSettingsStore.Protect(txtPass.Text)
            : "";
        _settings.LastFolderUrl = _currentUrl;
        _settings.ZipOnly = chkZipOnly.Checked;
    }

    private void SaveSettings()
    {
        CaptureSettings();
        try { WebDavSettingsStore.Save(_settings); }
        catch { /* a settings write must never break browsing */ }
    }

    private void SetBusy(bool busy, string? status = null)
    {
        UseWaitCursor = busy;
        btnConnect.Enabled = !busy;
        btnRefresh.Enabled = !busy;
        btnUp.Enabled = !busy;
        lstItems.Enabled = !busy;
        if (status != null) SetStatus(status);
    }

    private void SetStatus(string text)
    {
        lblStatus.Text = text;
        lblStatus.ForeColor = text.StartsWith("✔") ? Color.DarkGreen
            : text.StartsWith("✘") ? Color.DarkRed
            : text.StartsWith("⚠") ? Color.DarkOrange
            : SystemColors.ControlText;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        // Persist once on the way out instead of DPAPI-encrypting and rewriting the file on
        // every folder change. Only after a successful connect, so a cancelled credential
        // prompt cannot wipe what is already on file.
        if (_currentUrl.Length > 0) SaveSettings();
        _client?.Dispose();
        base.OnFormClosed(e);
    }
}
