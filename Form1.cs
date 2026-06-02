namespace BLogicDevTool;

public partial class Form1 : Form
{
    private SqlConnectionProfile? _sqlProfile;

    // ── Drag support (FormBorderStyle.None) ───────────────────────────
    private bool _dragging;
    private Point _dragStart;

    public Form1()
    {
        InitializeComponent();
        foreach (Control c in new Control[] { pnlNav, lblTitle })
        {
            c.MouseDown += Form_MouseDown;
            c.MouseMove += Form_MouseMove;
            c.MouseUp   += Form_MouseUp;
        }
        AppBusyState.Changed += OnAppBusyChanged;
        Disposed += (_, _) => AppBusyState.Changed -= OnAppBusyChanged;
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
                _sqlProfile = login.Profile;
                SqlSessionStore.SetProfile(_sqlProfile);
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


