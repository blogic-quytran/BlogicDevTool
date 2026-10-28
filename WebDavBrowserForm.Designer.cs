namespace BLogicDevTool;

partial class WebDavBrowserForm
{
    private System.ComponentModel.IContainer components = null;

    // ── Connection bar ────────────────────────────────────────────────────────
    private System.Windows.Forms.Panel    pnlConn;
    private System.Windows.Forms.Label    lblUrl;
    private System.Windows.Forms.TextBox  txtUrl;
    private System.Windows.Forms.Label    lblUser;
    private System.Windows.Forms.TextBox  txtUser;
    private System.Windows.Forms.Label    lblPass;
    private System.Windows.Forms.TextBox  txtPass;
    private System.Windows.Forms.CheckBox chkRemember;
    private System.Windows.Forms.Button   btnConnect;

    // ── Navigation bar ────────────────────────────────────────────────────────
    private System.Windows.Forms.Panel    pnlNav;
    private System.Windows.Forms.Button   btnUp;
    private System.Windows.Forms.Button   btnRefresh;
    private System.Windows.Forms.Button   btnServer;
    private System.Windows.Forms.TextBox  txtPath;
    private System.Windows.Forms.CheckBox chkZipOnly;

    // ── Listing ───────────────────────────────────────────────────────────────
    private System.Windows.Forms.ListView       lstItems;
    private System.Windows.Forms.ColumnHeader   colName;
    private System.Windows.Forms.ColumnHeader   colSize;
    private System.Windows.Forms.ColumnHeader   colModified;

    // ── Footer ────────────────────────────────────────────────────────────────
    private System.Windows.Forms.Panel  pnlFooter;
    private System.Windows.Forms.Label  lblStatus;
    private System.Windows.Forms.Button btnOpen;
    private System.Windows.Forms.Button btnCancel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    private void InitializeComponent()
    {
        pnlConn = new Panel();
        lblUrl = new Label();
        txtUrl = new TextBox();
        lblUser = new Label();
        txtUser = new TextBox();
        lblPass = new Label();
        txtPass = new TextBox();
        chkRemember = new CheckBox();
        btnConnect = new Button();
        pnlNav = new Panel();
        btnUp = new Button();
        btnRefresh = new Button();
        btnServer = new Button();
        txtPath = new TextBox();
        chkZipOnly = new CheckBox();
        lstItems = new ListView();
        colName = new ColumnHeader();
        colSize = new ColumnHeader();
        colModified = new ColumnHeader();
        pnlFooter = new Panel();
        lblStatus = new Label();
        btnOpen = new Button();
        btnCancel = new Button();
        pnlConn.SuspendLayout();
        pnlNav.SuspendLayout();
        pnlFooter.SuspendLayout();
        SuspendLayout();
        //
        // pnlConn
        //
        pnlConn.Controls.Add(lblUrl);
        pnlConn.Controls.Add(txtUrl);
        pnlConn.Controls.Add(lblUser);
        pnlConn.Controls.Add(txtUser);
        pnlConn.Controls.Add(lblPass);
        pnlConn.Controls.Add(txtPass);
        pnlConn.Controls.Add(chkRemember);
        pnlConn.Controls.Add(btnConnect);
        pnlConn.Dock = DockStyle.Top;
        pnlConn.Location = new Point(0, 0);
        pnlConn.Name = "pnlConn";
        pnlConn.Size = new Size(760, 66);
        pnlConn.TabIndex = 0;
        //
        // lblUrl
        //
        lblUrl.AutoSize = true;
        lblUrl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblUrl.Location = new Point(8, 12);
        lblUrl.Name = "lblUrl";
        lblUrl.Size = new Size(60, 15);
        lblUrl.TabIndex = 0;
        lblUrl.Text = "Server:";
        //
        // txtUrl
        //
        txtUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtUrl.Location = new Point(74, 8);
        txtUrl.Name = "txtUrl";
        txtUrl.PlaceholderText = "http://192.168.1.194:8000/dav";
        txtUrl.Size = new Size(560, 23);
        txtUrl.TabIndex = 1;
        //
        // lblUser
        //
        lblUser.AutoSize = true;
        lblUser.Location = new Point(8, 40);
        lblUser.Name = "lblUser";
        lblUser.Size = new Size(35, 15);
        lblUser.TabIndex = 2;
        lblUser.Text = "User:";
        //
        // txtUser
        //
        txtUser.Location = new Point(74, 36);
        txtUser.Name = "txtUser";
        txtUser.Size = new Size(150, 23);
        txtUser.TabIndex = 3;
        //
        // lblPass
        //
        lblPass.AutoSize = true;
        lblPass.Location = new Point(232, 40);
        lblPass.Name = "lblPass";
        lblPass.Size = new Size(60, 15);
        lblPass.TabIndex = 4;
        lblPass.Text = "Password:";
        //
        // txtPass
        //
        txtPass.Location = new Point(298, 36);
        txtPass.Name = "txtPass";
        txtPass.Size = new Size(150, 23);
        txtPass.TabIndex = 5;
        txtPass.UseSystemPasswordChar = true;
        //
        // chkRemember
        //
        chkRemember.AutoSize = true;
        chkRemember.Location = new Point(456, 38);
        chkRemember.Name = "chkRemember";
        chkRemember.Size = new Size(120, 19);
        chkRemember.TabIndex = 6;
        chkRemember.Text = "Remember";
        //
        // btnConnect
        //
        btnConnect.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnConnect.Location = new Point(640, 8);
        btnConnect.Name = "btnConnect";
        btnConnect.Size = new Size(108, 51);
        btnConnect.TabIndex = 7;
        btnConnect.Text = "🔌 Connect";
        btnConnect.Click += btnConnect_Click;
        //
        // pnlNav
        //
        pnlNav.Controls.Add(btnUp);
        pnlNav.Controls.Add(btnRefresh);
        pnlNav.Controls.Add(btnServer);
        pnlNav.Controls.Add(txtPath);
        pnlNav.Controls.Add(chkZipOnly);
        pnlNav.Dock = DockStyle.Top;
        pnlNav.Location = new Point(0, 66);
        pnlNav.Name = "pnlNav";
        pnlNav.Size = new Size(760, 36);
        pnlNav.TabIndex = 1;
        //
        // btnUp
        //
        btnUp.Location = new Point(8, 5);
        btnUp.Name = "btnUp";
        btnUp.Size = new Size(48, 25);
        btnUp.TabIndex = 0;
        btnUp.Text = "⬆";
        btnUp.Click += btnUp_Click;
        //
        // btnRefresh
        //
        btnRefresh.Location = new Point(60, 5);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(48, 25);
        btnRefresh.TabIndex = 1;
        btnRefresh.Text = "🔄";
        btnRefresh.Click += btnRefresh_Click;
        //
        // btnServer
        //
        btnServer.Location = new Point(112, 5);
        btnServer.Name = "btnServer";
        btnServer.Size = new Size(48, 25);
        btnServer.TabIndex = 2;
        btnServer.Text = "🔐";
        btnServer.Click += btnServer_Click;
        //
        // txtPath
        //
        txtPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtPath.Font = new Font("Consolas", 9F);
        txtPath.Location = new Point(166, 6);
        txtPath.Name = "txtPath";
        txtPath.ReadOnly = true;
        txtPath.Size = new Size(482, 23);
        txtPath.TabIndex = 3;
        //
        // chkZipOnly
        //
        chkZipOnly.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        chkZipOnly.AutoSize = true;
        chkZipOnly.Checked = true;
        chkZipOnly.CheckState = CheckState.Checked;
        chkZipOnly.Location = new Point(654, 8);
        chkZipOnly.Name = "chkZipOnly";
        chkZipOnly.Size = new Size(94, 19);
        chkZipOnly.TabIndex = 4;
        chkZipOnly.Text = ".zip only";
        chkZipOnly.CheckedChanged += chkZipOnly_CheckedChanged;
        //
        // lstItems
        //
        lstItems.Columns.AddRange(new ColumnHeader[] { colName, colSize, colModified });
        lstItems.Dock = DockStyle.Fill;
        lstItems.FullRowSelect = true;
        lstItems.Location = new Point(0, 102);
        lstItems.MultiSelect = false;
        lstItems.Name = "lstItems";
        lstItems.Size = new Size(760, 340);
        lstItems.TabIndex = 2;
        lstItems.UseCompatibleStateImageBehavior = false;
        lstItems.View = View.Details;
        lstItems.DoubleClick += lstItems_DoubleClick;
        lstItems.SelectedIndexChanged += lstItems_SelectedIndexChanged;
        lstItems.KeyDown += lstItems_KeyDown;
        lstItems.ColumnClick += lstItems_ColumnClick;
        //
        // colName
        //
        colName.Text = "Name";
        colName.Width = 440;
        //
        // colSize
        //
        colSize.Text = "Size";
        colSize.TextAlign = HorizontalAlignment.Right;
        colSize.Width = 100;
        //
        // colModified
        //
        colModified.Text = "Modified";
        colModified.Width = 170;
        //
        // pnlFooter
        //
        pnlFooter.Controls.Add(lblStatus);
        pnlFooter.Controls.Add(btnOpen);
        pnlFooter.Controls.Add(btnCancel);
        pnlFooter.Dock = DockStyle.Bottom;
        pnlFooter.Location = new Point(0, 442);
        pnlFooter.Name = "pnlFooter";
        pnlFooter.Size = new Size(760, 44);
        pnlFooter.TabIndex = 3;
        //
        // lblStatus
        //
        lblStatus.AutoEllipsis = true;
        lblStatus.Location = new Point(8, 14);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(500, 18);
        lblStatus.TabIndex = 0;
        lblStatus.Text = "Not connected.";
        //
        // btnOpen
        //
        btnOpen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnOpen.BackColor = Color.FromArgb(16, 124, 16);
        btnOpen.Enabled = false;
        btnOpen.FlatStyle = FlatStyle.Flat;
        btnOpen.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnOpen.ForeColor = Color.White;
        btnOpen.Location = new Point(548, 8);
        btnOpen.Name = "btnOpen";
        btnOpen.Size = new Size(100, 28);
        btnOpen.TabIndex = 1;
        btnOpen.Text = "✔ Select";
        btnOpen.UseVisualStyleBackColor = false;
        btnOpen.Click += btnOpen_Click;
        //
        // btnCancel
        //
        btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Location = new Point(654, 8);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(94, 28);
        btnCancel.TabIndex = 2;
        btnCancel.Text = "Cancel";
        //
        // WebDavBrowserForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnCancel;
        ClientSize = new Size(760, 486);
        Controls.Add(lstItems);
        Controls.Add(pnlFooter);
        Controls.Add(pnlNav);
        Controls.Add(pnlConn);
        MinimizeBox = false;
        MinimumSize = new Size(640, 400);
        Name = "WebDavBrowserForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Select ZIP from server (WebDAV)";
        pnlConn.ResumeLayout(false);
        pnlConn.PerformLayout();
        pnlNav.ResumeLayout(false);
        pnlNav.PerformLayout();
        pnlFooter.ResumeLayout(false);
        ResumeLayout(false);
    }
    #endregion
}
