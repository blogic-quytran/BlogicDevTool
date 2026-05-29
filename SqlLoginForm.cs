namespace BLogicDevTool;

public partial class SqlLoginForm : Form
{
    // ── Modern style constants ──────────────────────────────────────────────
    internal static readonly Color BgColor    = Color.FromArgb(245, 247, 250);
    internal static readonly Color PanelColor = Color.White;
    internal static readonly Color AccentBlue = Color.FromArgb(0, 120, 212);
    internal static readonly Color HoverBlue  = Color.FromArgb(0, 99, 177);
    internal static readonly Color CancelGray = Color.FromArgb(200, 200, 205);
    internal static readonly Color HoverGray  = Color.FromArgb(180, 180, 186);
    internal static readonly Color TextDark   = Color.FromArgb(30, 30, 40);
    internal static readonly Color SubText    = Color.FromArgb(100, 100, 120);
    internal static readonly Font  LabelFont  = new("Segoe UI", 9f, FontStyle.Regular);
    internal static readonly Font  InputFont  = new("Segoe UI", 10.5f);
    internal static readonly Font  ButtonFont = new("Segoe UI Semibold", 10.5f, FontStyle.Bold);
    internal static readonly Font  TitleFont  = new("Segoe UI", 14f, FontStyle.Bold);

    public SqlConnectionProfile? Profile { get; private set; }

    public SqlLoginForm()
    {
        InitializeComponent();
        InitializeUI();
    }

    private void InitializeUI()
    {
        // ── Form ────────────────────────────────────────────────────────────
        BackColor = BgColor;
        Font      = LabelFont;

        // ── Card ────────────────────────────────────────────────────────────
        pnlCard.BackColor     = PanelColor;
        tableLayout.BackColor = PanelColor;
        radioPanel.BackColor  = PanelColor;

        // ── Labels ──────────────────────────────────────────────────────────
        lblServer.Font  = LabelFont;  lblServer.ForeColor  = SubText;
        lblAuth.Font    = LabelFont;  lblAuth.ForeColor    = SubText;
        _lblUser.Font   = LabelFont;  _lblUser.ForeColor   = SubText;
        _lblPass.Font   = LabelFont;  _lblPass.ForeColor   = SubText;
        _lblStatus.Font = LabelFont;  _lblStatus.ForeColor = SubText;

        // ── Inputs ──────────────────────────────────────────────────────────
        StyleInput(_txtServer);
        StyleInput(_txtUser);
        StyleInput(_txtPass);

        // ── Radio buttons ───────────────────────────────────────────────────
        StyleRadio(_rbWindows);
        StyleRadio(_rbSql);

        // ── Buttons ─────────────────────────────────────────────────────────
        StyleButton(_btnLogin,  AccentBlue, HoverBlue, Color.White);
        StyleButton(_btnCancel, CancelGray, HoverGray, TextDark);
    }

    private void PnlCard_Paint(object? sender, PaintEventArgs e)
    {
        using var pen = new Pen(Color.FromArgb(220, 220, 230), 1);
        e.Graphics.DrawRectangle(pen, 0, 0, pnlCard.Width - 1, pnlCard.Height - 1);
    }

    private void RbSql_CheckedChanged(object? sender, EventArgs e)
    {
        var sqlAuth       = _rbSql.Checked;
        _lblUser.ForeColor = sqlAuth ? TextDark : SubText;
        _txtUser.Enabled   = sqlAuth;
        _lblPass.ForeColor = sqlAuth ? TextDark : SubText;
        _txtPass.Enabled   = sqlAuth;
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    // ── Style helpers ────────────────────────────────────────────────────────

    internal static void StyleInput(TextBox tb)
    {
        tb.Font        = InputFont;
        tb.BorderStyle = BorderStyle.FixedSingle;
        tb.BackColor   = Color.FromArgb(250, 250, 252);
        tb.ForeColor   = TextDark;
        tb.Height      = 28;
        tb.Dock        = DockStyle.Fill;
    }

    internal static void StyleRadio(RadioButton rb)
    {
        rb.Font      = LabelFont;
        rb.ForeColor = TextDark;
        rb.FlatStyle = FlatStyle.Flat;
    }

    internal static void StyleButton(Button btn, Color back, Color hover, Color fore)
    {
        btn.Font        = ButtonFont;
        btn.ForeColor   = fore;
        btn.BackColor   = back;
        btn.FlatStyle   = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize         = 0;
        btn.FlatAppearance.MouseOverBackColor = hover;
        btn.Cursor      = Cursors.Hand;
        btn.TextAlign   = ContentAlignment.MiddleCenter;
    }

    // ── Event handlers ───────────────────────────────────────────────────────

    private async void btnLogin_Click(object? sender, EventArgs e)
    {
        var server = _txtServer.Text.Trim();
        if (string.IsNullOrWhiteSpace(server))
        {
            MessageBox.Show("Please enter server name.", "Missing Info",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _btnLogin.Enabled    = false;
        _lblStatus.ForeColor = SubText;
        _lblStatus.Text      = "Connecting...";

        try
        {
            var profile = new SqlConnectionProfile(
                server,
                _rbWindows.Checked,
                _txtUser.Text.Trim(),
                _txtPass.Text);

            var helper = profile.CreateHelper();
            await helper.TestConnectionAsync();

            Profile      = profile;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _lblStatus.ForeColor = Color.FromArgb(200, 30, 30);
            _lblStatus.Text      = "Connection failed: " + ex.Message;
        }
        finally
        {
            _btnLogin.Enabled = true;
        }
    }
}
