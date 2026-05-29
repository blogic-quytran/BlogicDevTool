namespace BLogicDevTool;

partial class SqlLoginForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    private void InitializeComponent()
    {
        pnlCard = new Panel();
        _btnCancel = new Button();
        _btnLogin = new Button();
        tableLayout = new TableLayoutPanel();
        pnlServer = new Panel();
        _txtServer = new TextBox();
        lblServer = new Label();
        lblAuth = new Label();
        radioPanel = new FlowLayoutPanel();
        _rbWindows = new RadioButton();
        _rbSql = new RadioButton();
        pnlUser = new Panel();
        _txtUser = new TextBox();
        _lblUser = new Label();
        pnlPass = new Panel();
        _txtPass = new TextBox();
        _lblPass = new Label();
        _lblStatus = new Label();
        pnlCard.SuspendLayout();
        tableLayout.SuspendLayout();
        pnlServer.SuspendLayout();
        radioPanel.SuspendLayout();
        pnlUser.SuspendLayout();
        pnlPass.SuspendLayout();
        SuspendLayout();
        // 
        // pnlCard
        // 
        pnlCard.Controls.Add(_btnCancel);
        pnlCard.Controls.Add(_btnLogin);
        pnlCard.Controls.Add(tableLayout);
        pnlCard.Dock = DockStyle.Fill;
        pnlCard.Location = new Point(0, 0);
        pnlCard.Name = "pnlCard";
        pnlCard.Padding = new Padding(20);
        pnlCard.Size = new Size(480, 370);
        pnlCard.TabIndex = 1;
        pnlCard.Paint += PnlCard_Paint;
        // 
        // _btnCancel
        // 
        _btnCancel.Cursor = Cursors.Hand;
        _btnCancel.FlatAppearance.BorderSize = 0;
        _btnCancel.FlatStyle = FlatStyle.Flat;
        _btnCancel.Location = new Point(115, 299);
        _btnCancel.Name = "_btnCancel";
        _btnCancel.Size = new Size(120, 46);
        _btnCancel.TabIndex = 3;
        _btnCancel.Text = "Cancel";
        _btnCancel.Click += BtnCancel_Click;
        // 
        // _btnLogin
        // 
        _btnLogin.Cursor = Cursors.Hand;
        _btnLogin.FlatAppearance.BorderSize = 0;
        _btnLogin.FlatStyle = FlatStyle.Flat;
        _btnLogin.Location = new Point(245, 299);
        _btnLogin.Name = "_btnLogin";
        _btnLogin.Size = new Size(140, 46);
        _btnLogin.TabIndex = 2;
        _btnLogin.Text = "Connect";
        _btnLogin.Click += btnLogin_Click;
        // 
        // tableLayout
        // 
        tableLayout.ColumnCount = 2;
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tableLayout.Controls.Add(pnlServer, 0, 0);
        tableLayout.Controls.Add(lblAuth, 0, 1);
        tableLayout.Controls.Add(radioPanel, 0, 2);
        tableLayout.Controls.Add(pnlUser, 0, 3);
        tableLayout.Controls.Add(pnlPass, 1, 3);
        tableLayout.Controls.Add(_lblStatus, 0, 4);
        tableLayout.Dock = DockStyle.Fill;
        tableLayout.Location = new Point(20, 20);
        tableLayout.Name = "tableLayout";
        tableLayout.Padding = new Padding(20, 16, 20, 8);
        tableLayout.RowCount = 5;
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayout.Size = new Size(440, 330);
        tableLayout.TabIndex = 0;
        // 
        // pnlServer
        // 
        tableLayout.SetColumnSpan(pnlServer, 2);
        pnlServer.Controls.Add(_txtServer);
        pnlServer.Controls.Add(lblServer);
        pnlServer.Dock = DockStyle.Fill;
        pnlServer.Location = new Point(23, 19);
        pnlServer.Name = "pnlServer";
        pnlServer.Padding = new Padding(0, 0, 8, 0);
        pnlServer.Size = new Size(394, 42);
        pnlServer.TabIndex = 0;
        // 
        // _txtServer
        // 
        _txtServer.BorderStyle = BorderStyle.FixedSingle;
        _txtServer.Dock = DockStyle.Top;
        _txtServer.Location = new Point(0, 15);
        _txtServer.Name = "_txtServer";
        _txtServer.Size = new Size(386, 23);
        _txtServer.TabIndex = 0;
        _txtServer.Text = ".";
        // 
        // lblServer
        // 
        lblServer.AutoSize = true;
        lblServer.Dock = DockStyle.Top;
        lblServer.Location = new Point(0, 0);
        lblServer.Name = "lblServer";
        lblServer.Size = new Size(94, 15);
        lblServer.TabIndex = 1;
        lblServer.Text = "Server / Instance";
        // 
        // lblAuth
        // 
        lblAuth.AutoSize = true;
        tableLayout.SetColumnSpan(lblAuth, 2);
        lblAuth.Location = new Point(23, 64);
        lblAuth.Name = "lblAuth";
        lblAuth.Size = new Size(86, 15);
        lblAuth.TabIndex = 1;
        lblAuth.Text = "Authentication";
        // 
        // radioPanel
        // 
        tableLayout.SetColumnSpan(radioPanel, 2);
        radioPanel.Controls.Add(_rbWindows);
        radioPanel.Controls.Add(_rbSql);
        radioPanel.Dock = DockStyle.Fill;
        radioPanel.Location = new Point(23, 105);
        radioPanel.Name = "radioPanel";
        radioPanel.Size = new Size(394, 26);
        radioPanel.TabIndex = 2;
        // 
        // _rbWindows
        // 
        _rbWindows.AutoSize = true;
        _rbWindows.Checked = true;
        _rbWindows.FlatStyle = FlatStyle.Flat;
        _rbWindows.Location = new Point(0, 0);
        _rbWindows.Margin = new Padding(0, 0, 20, 0);
        _rbWindows.Name = "_rbWindows";
        _rbWindows.Size = new Size(155, 19);
        _rbWindows.TabIndex = 0;
        _rbWindows.TabStop = true;
        _rbWindows.Text = "Windows Authentication";
        // 
        // _rbSql
        // 
        _rbSql.AutoSize = true;
        _rbSql.FlatStyle = FlatStyle.Flat;
        _rbSql.Location = new Point(178, 3);
        _rbSql.Name = "_rbSql";
        _rbSql.Size = new Size(162, 19);
        _rbSql.TabIndex = 1;
        _rbSql.Text = "SQL Server Authentication";
        _rbSql.CheckedChanged += RbSql_CheckedChanged;
        // 
        // pnlUser
        // 
        pnlUser.Controls.Add(_txtUser);
        pnlUser.Controls.Add(_lblUser);
        pnlUser.Dock = DockStyle.Fill;
        pnlUser.Location = new Point(23, 137);
        pnlUser.Name = "pnlUser";
        pnlUser.Padding = new Padding(0, 0, 8, 0);
        pnlUser.Size = new Size(194, 42);
        pnlUser.TabIndex = 3;
        // 
        // _txtUser
        // 
        _txtUser.BorderStyle = BorderStyle.FixedSingle;
        _txtUser.Dock = DockStyle.Top;
        _txtUser.Enabled = false;
        _txtUser.Location = new Point(0, 15);
        _txtUser.Name = "_txtUser";
        _txtUser.Size = new Size(186, 23);
        _txtUser.TabIndex = 0;
        // 
        // _lblUser
        // 
        _lblUser.AutoSize = true;
        _lblUser.Dock = DockStyle.Top;
        _lblUser.Location = new Point(0, 0);
        _lblUser.Name = "_lblUser";
        _lblUser.Size = new Size(60, 15);
        _lblUser.TabIndex = 1;
        _lblUser.Text = "Username";
        // 
        // pnlPass
        // 
        pnlPass.Controls.Add(_txtPass);
        pnlPass.Controls.Add(_lblPass);
        pnlPass.Dock = DockStyle.Fill;
        pnlPass.Location = new Point(223, 137);
        pnlPass.Name = "pnlPass";
        pnlPass.Padding = new Padding(0, 0, 8, 0);
        pnlPass.Size = new Size(194, 42);
        pnlPass.TabIndex = 4;
        // 
        // _txtPass
        // 
        _txtPass.BorderStyle = BorderStyle.FixedSingle;
        _txtPass.Dock = DockStyle.Top;
        _txtPass.Enabled = false;
        _txtPass.Location = new Point(0, 15);
        _txtPass.Name = "_txtPass";
        _txtPass.Size = new Size(186, 23);
        _txtPass.TabIndex = 0;
        _txtPass.UseSystemPasswordChar = true;
        // 
        // _lblPass
        // 
        _lblPass.AutoSize = true;
        _lblPass.Dock = DockStyle.Top;
        _lblPass.Location = new Point(0, 0);
        _lblPass.Name = "_lblPass";
        _lblPass.Size = new Size(57, 15);
        _lblPass.TabIndex = 1;
        _lblPass.Text = "Password";
        // 
        // _lblStatus
        // 
        tableLayout.SetColumnSpan(_lblStatus, 2);
        _lblStatus.Location = new Point(23, 182);
        _lblStatus.Name = "_lblStatus";
        _lblStatus.Size = new Size(394, 73);
        _lblStatus.TabIndex = 5;
        _lblStatus.Text = "Enter credentials and click Connect.";
        _lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // SqlLoginForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(480, 370);
        Controls.Add(pnlCard);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "SqlLoginForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "SQL Server Login";
        pnlCard.ResumeLayout(false);
        tableLayout.ResumeLayout(false);
        tableLayout.PerformLayout();
        pnlServer.ResumeLayout(false);
        pnlServer.PerformLayout();
        radioPanel.ResumeLayout(false);
        radioPanel.PerformLayout();
        pnlUser.ResumeLayout(false);
        pnlUser.PerformLayout();
        pnlPass.ResumeLayout(false);
        pnlPass.PerformLayout();
        ResumeLayout(false);
    }

    #endregion
    private System.Windows.Forms.Panel            pnlCard     = null!;
    private System.Windows.Forms.TableLayoutPanel tableLayout = null!;
    private System.Windows.Forms.Panel            pnlServer   = null!;
    private System.Windows.Forms.Label            lblServer   = null!;
    private System.Windows.Forms.TextBox          _txtServer  = null!;
    private System.Windows.Forms.Label            lblAuth     = null!;
    private System.Windows.Forms.FlowLayoutPanel  radioPanel  = null!;
    private System.Windows.Forms.RadioButton      _rbWindows  = null!;
    private System.Windows.Forms.RadioButton      _rbSql      = null!;
    private System.Windows.Forms.Panel            pnlUser     = null!;
    private System.Windows.Forms.Label            _lblUser    = null!;
    private System.Windows.Forms.TextBox          _txtUser    = null!;
    private System.Windows.Forms.Panel            pnlPass     = null!;
    private System.Windows.Forms.Label            _lblPass    = null!;
    private System.Windows.Forms.TextBox          _txtPass    = null!;
    private System.Windows.Forms.Label            _lblStatus  = null!;
    private System.Windows.Forms.Button           _btnLogin   = null!;
    private System.Windows.Forms.Button           _btnCancel  = null!;
}
