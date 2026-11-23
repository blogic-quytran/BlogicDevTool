namespace BLogicDevTool;

public partial class Form1 : Form
{
    // Read from SqlSessionStore so logins from any tab or the auto-login are shared.
    private static SqlConnectionProfile? _sqlProfile => SqlSessionStore.Current;

    // ── Drag support (FormBorderStyle.None) ───────────────────────────
    private bool _dragging;
    private Point _dragStart;

    public Form1()
    {
        InitializeComponent();
        var lblVersion = AddVersionLabel();
        foreach (Control c in new Control[] { pnlNav, lblTitle, lblVersion })
        {
            c.MouseDown += Form_MouseDown;
            c.MouseMove += Form_MouseMove;
            c.MouseUp   += Form_MouseUp;
        }
        AppBusyState.Changed += OnAppBusyChanged;
        Disposed += (_, _) => AppBusyState.Changed -= OnAppBusyChanged;
    }

    // Small version tag after the title; the first nav button gives up its spare padding
    // (right edge unchanged) so nothing else in the nav bar moves.
    private Label AddVersionLabel()
    {
        var lbl = new Label
        {
            AutoSize  = true,
            Text      = UpdateService.CurrentVersionText,
            Font      = new Font("Segoe UI", 8F),
            ForeColor = Color.FromArgb(160, 160, 160),
            BackColor = Color.Transparent,
        };
        pnlNav.Controls.Add(lbl);
        lbl.BringToFront();
        // The title's real width is only known once its handle exists (emoji font fallback);
        // re-place on resize/DPI scaling, keeping the designer's 5px gap before the IIS button.
        void Place()
        {
            lbl.Location = new Point(lblTitle.Right + 2, lblTitle.Top + 2);
            btnNavDatabase.Left  = lbl.Right + 6;
            btnNavDatabase.Width = btnNavIis.Left - 5 - btnNavDatabase.Left;
        }
        lblTitle.SizeChanged += (_, _) => Place();
        btnNavIis.LocationChanged += (_, _) => Place();
        Place();
        return lbl;
    }

    /// <summary>
    /// While any feature operation is in progress, disable the navigation buttons
    /// so the user cannot switch tabs (which would dispose the running control).
    /// </summary>
    private void OnAppBusyChanged(bool busy)
    {
        if (IsDisposed) return;
        if (InvokeRequired) { BeginInvoke(() => OnAppBusyChanged(busy)); return; }
        foreach (var btn in new[] { btnNavDatabase, btnNavIis, btnNavUnzip, btnNavSqlManager, btnNavBuild })
            btn.Enabled = !busy;
    }

    private void Form_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _dragging  = true;
            _dragStart = e.Location;
        }
    }
    private void Form_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_dragging)
            Location = new Point(
                Location.X + e.X - _dragStart.X,
                Location.Y + e.Y - _dragStart.Y);
    }
    private void Form_MouseUp(object? sender, MouseEventArgs e)
        => _dragging = false;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        WindowState = FormWindowState.Normal;
        menuItemUnzip_Click(this, EventArgs.Empty);
        _ = TryAutoLoginSqlAsync();
        _ = CheckForUpdateAsync();
    }

    // Silently try the login dialog's defaults ("." + Windows Auth); on failure stay logged out.
    private async Task TryAutoLoginSqlAsync()
    {
        try
        {
            var profile = new SqlConnectionProfile(".", windowsAuth: true);
            await profile.CreateHelper().TestConnectionAsync();
            // Keep a manual login made meanwhile; never swap the profile under a running operation.
            if (IsDisposed || SqlSessionStore.Current != null || AppBusyState.IsBusy) return;
            SqlSessionStore.SetProfile(profile);
        }
        catch
        {
            // No local instance / no permission: stay logged out.
        }
    }

    // ── Update check (GitHub Releases) ───────────────────────────────
    private async Task CheckForUpdateAsync()
    {
        UpdateInfo? info;
        try
        {
            Text = $"BLogic Dev Tool {UpdateService.CurrentVersionText}";
            new ToolTip().SetToolTip(lblTitle, $"Version {UpdateService.CurrentVersionText}");
            UpdateService.CleanupPreviousUpdate();
            info = await UpdateService.CheckAsync();
        }
        catch { return; } // offline / rate-limited / any updater fault must never take the tool down
        if (info == null || IsDisposed) return;
        ShowUpdateBanner(info);
        // Cancel is not remembered on purpose: every launch asks again until the user updates.
        if (!AppBusyState.IsBusy)
            ShowUpdateDialog(info);
    }

    private void ShowUpdateDialog(UpdateInfo info)
    {
        using var dlg = new UpdateForm(info);
        dlg.ShowDialog(this);
    }

    private void ShowUpdateBanner(UpdateInfo info)
    {
        var banner = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = Color.FromArgb(255, 243, 205) };
        var lbl = new Label
        {
            Text      = string.IsNullOrEmpty(info.Summary)
                ? $"🔔 New version {info.Tag} is available (current {UpdateService.CurrentVersionText})."
                : $"🔔 New version {info.Tag}: {info.Summary}",
            AutoEllipsis = true,
            Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(102, 77, 3),
            Location  = new Point(10, 7),
        };
        // Long summaries get "..." so the button stays on screen; the full text is in the tooltip/dialog.
        lbl.Size = new Size(Math.Min(lbl.PreferredWidth, 700), 18);
        new ToolTip().SetToolTip(lbl, lbl.Text);
        var btnView = new Button
        {
            Text      = "View && install",
            Size      = new Size(110, 24),
            Location  = new Point(lbl.Right + 10, 3),
            BackColor = Color.FromArgb(0, 102, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
        };
        btnView.FlatAppearance.BorderSize = 0;
        btnView.Click += (_, _) => ShowUpdateDialog(info);
        var btnHide = new Button
        {
            Text      = "✕",
            Size      = new Size(30, 30),
            Dock      = DockStyle.Right,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.FromArgb(102, 77, 3),
        };
        btnHide.FlatAppearance.BorderSize = 0;
        btnHide.Click += (_, _) => banner.Dispose();

        banner.Controls.AddRange(new Control[] { lbl, btnView, btnHide });
        panel1.Controls.Add(banner);
        // Dock order follows z-order: keep the banner between pnlNav (top) and pnlMain (fill).
        panel1.Controls.SetChildIndex(banner, panel1.Controls.GetChildIndex(pnlNav));
    }

    // ── Helper: embed UserControl in pnlMain ─────────────────────────
    private void ShowInPanel(Func<UserControl> createControl)
    {
        foreach (Control c in pnlMain.Controls)
            c.Dispose();
        pnlMain.Controls.Clear();

        var control = createControl();
        control.Dock = DockStyle.Fill;
        pnlMain.Controls.Add(control);
    }

    private void ShowDatabaseTools()
    {
        SetActiveNav(btnNavDatabase);
        // If not logged in yet, show login dialog first
        if (_sqlProfile == null)
        {
            using var login = new SqlLoginForm();
            if (login.ShowDialog(this) == DialogResult.OK && login.Profile != null)
            {
                SqlSessionStore.SetProfile(login.Profile);
            }
            else
            {
                // User cancelled — show disabled DatabaseToolsForm
            }
        }

        foreach (Control c in pnlMain.Controls)
            c.Dispose();
        pnlMain.Controls.Clear();

        var form = new DatabaseToolsForm(_sqlProfile);
        form.Dock = DockStyle.Fill;
        pnlMain.Controls.Add(form);
    }

    // ── Nav handlers ─────────────────────────────────────────────
    private void menuItemDatabaseTools_Click(object sender, EventArgs e)
    {
        if (AppBusyState.IsBusy) return;
        ShowDatabaseTools();
    }

    private void menuItemIisManager_Click(object sender, EventArgs e)
    {
        if (AppBusyState.IsBusy) return;
        SetActiveNav(btnNavIis);
        ShowInPanel(() => new IisServiceManagerForm(_sqlProfile));
    }

    private void menuItemUnzip_Click(object sender, EventArgs e)
    {
        if (AppBusyState.IsBusy) return;
        SetActiveNav(btnNavUnzip);
        ShowInPanel(() => new UnzipWorkbenchForm(_sqlProfile));
    }

    private void menuItemSqlManager_Click(object sender, EventArgs e)
    {
        if (AppBusyState.IsBusy) return;
        SetActiveNav(btnNavSqlManager);
        ShowInPanel(() => new SqlServerManagerForm());
    }

    private void menuItemBuildRelease_Click(object sender, EventArgs e)
    {
        if (AppBusyState.IsBusy) return;
        SetActiveNav(btnNavBuild);
        ShowInPanel(() => new BuildReleaseForm());
    }

    private void btnCloseForm_Click(object sender, EventArgs e)
        => Close();

    private void SetActiveNav(Button active)
    {
        foreach (var btn in new[] { btnNavDatabase, btnNavIis, btnNavUnzip, btnNavSqlManager, btnNavBuild })
        {
            btn.BackColor = btn == active ? Color.FromArgb(0, 102, 204) : Color.Transparent;
            btn.ForeColor = btn == active ? Color.White : Color.Silver;
        }
    }
}


