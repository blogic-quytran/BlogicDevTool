namespace BLogicDevTool;

partial class SqlServerManagerForm
{
    private System.ComponentModel.IContainer components = null;

    // ── Tab control (3 tabs) ──────────────────────────────────────────────
    private System.Windows.Forms.TabControl  tabMain;
    private System.Windows.Forms.TabPage     tabConnect;
    private System.Windows.Forms.TabPage     tabLogins;
    private System.Windows.Forms.TabPage     tabNetwork;

    // ── Tab 1: Connect ────────────────────────────────────────────────────
    private System.Windows.Forms.GroupBox    grpConn;
    private System.Windows.Forms.Label       lblConnServer;
    private System.Windows.Forms.ComboBox    cboConnInstance;
    private System.Windows.Forms.Label       lblConnAuth;
    private System.Windows.Forms.RadioButton rbConnWindows;
    private System.Windows.Forms.RadioButton rbConnSql;
    private System.Windows.Forms.Label       lblConnUser;
    private System.Windows.Forms.TextBox     txtConnUser;
    private System.Windows.Forms.Label       lblConnPass;
    private System.Windows.Forms.TextBox     txtConnPass;
    private System.Windows.Forms.Button      btnTestConnect;

    private System.Windows.Forms.GroupBox    grpInfo;
    private System.Windows.Forms.Label       lblInfoServer;
    private System.Windows.Forms.Label       lblInfoServerVal;
    private System.Windows.Forms.Label       lblInfoVersion;
    private System.Windows.Forms.Label       lblInfoVersionVal;
    private System.Windows.Forms.Label       lblInfoEdition;
    private System.Windows.Forms.Label       lblInfoEditionVal;
    private System.Windows.Forms.Label       lblInfoMachine;
    private System.Windows.Forms.Label       lblInfoMachineVal;
    private System.Windows.Forms.Label       lblInfoAuth;
    private System.Windows.Forms.Label       lblInfoAuthVal;

    // ── Tab 2: Login Accounts ─────────────────────────────────────────────
    // Grid toolbar
    private System.Windows.Forms.Button      btnRefreshLogins;
    private System.Windows.Forms.Button      btnToggleLogin;
    private System.Windows.Forms.Button      btnDeleteLogin;
    private System.Windows.Forms.DataGridView dgvLogins;

    // Edit / create panel
    private System.Windows.Forms.GroupBox    grpLoginEdit;
    private System.Windows.Forms.Label       lblNewLoginName;
    private System.Windows.Forms.TextBox     txtNewLoginName;
    private System.Windows.Forms.Label       lblNewLoginPass;
    private System.Windows.Forms.TextBox     txtNewLoginPass;
    private System.Windows.Forms.CheckBox    chkShowLoginPass;
    private System.Windows.Forms.CheckBox    chkNewSysAdmin;
    private System.Windows.Forms.Button      btnCreateLogin;
    private System.Windows.Forms.Button      btnChangeLoginPass;
    private System.Windows.Forms.Button      btnConfigureLogin;
    private System.Windows.Forms.Button      btnTestLogin;

    // Server actions panel
    private System.Windows.Forms.GroupBox    grpServerActions;
    private System.Windows.Forms.Label       lblServerInstance;
    private System.Windows.Forms.ComboBox    cboServerInstance;
    private System.Windows.Forms.Button      btnEnableMixedMode;
    private System.Windows.Forms.Button      btnRestartSqlService;

    // Log (right panel)
    private System.Windows.Forms.GroupBox    grpLog;
    private System.Windows.Forms.TextBox     txtLog;
    private System.Windows.Forms.Button      btnClearLog;

    // ── Tab 3: Network / TCP-IP ───────────────────────────────────────────
    private System.Windows.Forms.GroupBox    grpNetInstance;
    private System.Windows.Forms.Label       lblNetInstance;
    private System.Windows.Forms.ComboBox    cboNetInstance;
    private System.Windows.Forms.Label       lblNetPort;
    private System.Windows.Forms.TextBox     txtNetPort;
    private System.Windows.Forms.Button      btnCheckNetStatus;

    private System.Windows.Forms.GroupBox    grpNetStatus;
    private System.Windows.Forms.Label       lblNetTcpCaption;
    private System.Windows.Forms.Label       lblNetTcpStatus;
    private System.Windows.Forms.Label       lblNetPortCaption;
    private System.Windows.Forms.Label       lblNetPortStatus;

    private System.Windows.Forms.GroupBox    grpNetActions;
    private System.Windows.Forms.Button      btnEnableTcpIp;
    private System.Windows.Forms.Button      btnDisableTcpIp;
    private System.Windows.Forms.Button      btnAddFirewallRule;
    private System.Windows.Forms.Button      btnNetRestartService;

    // ── Status bar ────────────────────────────────────────────────────────
    private System.Windows.Forms.ProgressBar pbStatus;
    private System.Windows.Forms.Label       lblStatus;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        tabMain = new TabControl();
        tabConnect = new TabPage();
        grpConn = new GroupBox();
        lblConnServer = new Label();
        cboConnInstance = new ComboBox();
        lblConnAuth = new Label();
        rbConnWindows = new RadioButton();
        rbConnSql = new RadioButton();
        lblConnUser = new Label();
        txtConnUser = new TextBox();
        lblConnPass = new Label();
        txtConnPass = new TextBox();
        btnTestConnect = new Button();
        grpInfo = new GroupBox();
        lblInfoServer = new Label();
        lblInfoServerVal = new Label();
        lblInfoVersion = new Label();
        lblInfoVersionVal = new Label();
        lblInfoEdition = new Label();
        lblInfoEditionVal = new Label();
        lblInfoMachine = new Label();
        lblInfoMachineVal = new Label();
        lblInfoAuth = new Label();
        lblInfoAuthVal = new Label();
        tabLogins = new TabPage();
        btnRefreshLogins = new Button();
        btnToggleLogin = new Button();
        btnDeleteLogin = new Button();
        dgvLogins = new DataGridView();
        grpLoginEdit = new GroupBox();
        lblNewLoginName = new Label();
        txtNewLoginName = new TextBox();
        lblNewLoginPass = new Label();
        txtNewLoginPass = new TextBox();
        chkShowLoginPass = new CheckBox();
        chkNewSysAdmin = new CheckBox();
        btnCreateLogin = new Button();
        btnChangeLoginPass = new Button();
        btnConfigureLogin = new Button();
        btnTestLogin = new Button();
        lblHint = new Label();
        grpServerActions = new GroupBox();
        lblServerInstance = new Label();
        cboServerInstance = new ComboBox();
        btnEnableMixedMode = new Button();
        btnRestartSqlService = new Button();
        grpLog = new GroupBox();
        txtLog = new TextBox();
        btnClearLog = new Button();
        tabNetwork = new TabPage();
        grpNetInstance = new GroupBox();
        lblNetInstance = new Label();
        cboNetInstance = new ComboBox();
        lblNetPort = new Label();
        txtNetPort = new TextBox();
        btnCheckNetStatus = new Button();
        grpNetStatus = new GroupBox();
        lblNetTcpCaption = new Label();
        lblNetTcpStatus = new Label();
        lblNetPortCaption = new Label();
        lblNetPortStatus = new Label();
        grpNetActions = new GroupBox();
        btnEnableTcpIp = new Button();
        btnDisableTcpIp = new Button();
        btnAddFirewallRule = new Button();
        btnNetRestartService = new Button();
        pbStatus = new ProgressBar();
        lblStatus = new Label();
        tabMain.SuspendLayout();
        tabConnect.SuspendLayout();
        grpConn.SuspendLayout();
        grpInfo.SuspendLayout();
        tabLogins.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvLogins).BeginInit();
        grpLoginEdit.SuspendLayout();
        grpServerActions.SuspendLayout();
        grpLog.SuspendLayout();
        tabNetwork.SuspendLayout();
        grpNetInstance.SuspendLayout();
        grpNetStatus.SuspendLayout();
        grpNetActions.SuspendLayout();
        SuspendLayout();
        // 
        // tabMain
        // 
        tabMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        tabMain.Controls.Add(tabConnect);
        tabMain.Controls.Add(tabLogins);
        tabMain.Controls.Add(tabNetwork);
        tabMain.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        tabMain.Location = new Point(0, 0);
        tabMain.Name = "tabMain";
        tabMain.SelectedIndex = 0;
        tabMain.Size = new Size(880, 640);
        tabMain.TabIndex = 0;
        // 
        // tabConnect
        // 
        tabConnect.Controls.Add(grpConn);
        tabConnect.Controls.Add(grpInfo);
        tabConnect.Font = new Font("Segoe UI", 9F);
        tabConnect.Location = new Point(4, 26);
        tabConnect.Name = "tabConnect";
        tabConnect.Padding = new Padding(3);
        tabConnect.Size = new Size(872, 610);
        tabConnect.TabIndex = 0;
        tabConnect.Text = "🔍  Connect";
        // 
        // grpConn
        // 
        grpConn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpConn.Controls.Add(lblConnServer);
        grpConn.Controls.Add(cboConnInstance);
        grpConn.Controls.Add(lblConnAuth);
        grpConn.Controls.Add(rbConnWindows);
        grpConn.Controls.Add(rbConnSql);
        grpConn.Controls.Add(lblConnUser);
        grpConn.Controls.Add(txtConnUser);
        grpConn.Controls.Add(lblConnPass);
        grpConn.Controls.Add(txtConnPass);
        grpConn.Controls.Add(btnTestConnect);
        grpConn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpConn.ForeColor = Color.FromArgb(30, 30, 50);
        grpConn.Location = new Point(10, 10);
        grpConn.Name = "grpConn";
        grpConn.Size = new Size(850, 185);
        grpConn.TabIndex = 0;
        grpConn.TabStop = false;
        grpConn.Text = "Connection";
        // 
        // lblConnServer
        // 
        lblConnServer.AutoSize = true;
        lblConnServer.Font = new Font("Segoe UI", 9F);
        lblConnServer.ForeColor = Color.FromArgb(80, 80, 100);
        lblConnServer.Location = new Point(14, 35);
        lblConnServer.Name = "lblConnServer";
        lblConnServer.Size = new Size(42, 15);
        lblConnServer.TabIndex = 0;
        lblConnServer.Text = "Server:";
        // 
        // cboConnInstance
        // 
        cboConnInstance.Font = new Font("Segoe UI", 10F);
        cboConnInstance.Location = new Point(105, 32);
        cboConnInstance.Name = "cboConnInstance";
        cboConnInstance.Size = new Size(320, 25);
        cboConnInstance.TabIndex = 0;
        // 
        // lblConnAuth
        // 
        lblConnAuth.AutoSize = true;
        lblConnAuth.Font = new Font("Segoe UI", 9F);
        lblConnAuth.ForeColor = Color.FromArgb(80, 80, 100);
        lblConnAuth.Location = new Point(14, 72);
        lblConnAuth.Name = "lblConnAuth";
        lblConnAuth.Size = new Size(36, 15);
        lblConnAuth.TabIndex = 1;
        lblConnAuth.Text = "Auth:";
        // 
        // rbConnWindows
        // 
        rbConnWindows.AutoSize = true;
        rbConnWindows.Font = new Font("Segoe UI", 9F);
        rbConnWindows.ForeColor = Color.FromArgb(30, 30, 50);
        rbConnWindows.Location = new Point(105, 70);
        rbConnWindows.Name = "rbConnWindows";
        rbConnWindows.Size = new Size(103, 19);
        rbConnWindows.TabIndex = 1;
        rbConnWindows.Text = "Windows Auth";
        rbConnWindows.CheckedChanged += RbConnWindows_CheckedChanged;
        // 
        // rbConnSql
        // 
        rbConnSql.AutoSize = true;
        rbConnSql.Font = new Font("Segoe UI", 9F);
        rbConnSql.ForeColor = Color.FromArgb(30, 30, 50);
        rbConnSql.Location = new Point(260, 70);
        rbConnSql.Name = "rbConnSql";
        rbConnSql.Size = new Size(110, 19);
        rbConnSql.TabIndex = 2;
        rbConnSql.Text = "SQL Server Auth";
        // 
        // lblConnUser
        // 
        lblConnUser.AutoSize = true;
        lblConnUser.Enabled = false;
        lblConnUser.Font = new Font("Segoe UI", 9F);
        lblConnUser.ForeColor = Color.FromArgb(80, 80, 100);
        lblConnUser.Location = new Point(14, 108);
        lblConnUser.Name = "lblConnUser";
        lblConnUser.Size = new Size(33, 15);
        lblConnUser.TabIndex = 3;
        lblConnUser.Text = "User:";
        // 
        // txtConnUser
        // 
        txtConnUser.Enabled = false;
        txtConnUser.Font = new Font("Segoe UI", 10F);
        txtConnUser.Location = new Point(105, 105);
        txtConnUser.Name = "txtConnUser";
        txtConnUser.Size = new Size(320, 25);
        txtConnUser.TabIndex = 3;
        // 
        // lblConnPass
        // 
        lblConnPass.AutoSize = true;
        lblConnPass.Enabled = false;
        lblConnPass.Font = new Font("Segoe UI", 9F);
        lblConnPass.ForeColor = Color.FromArgb(80, 80, 100);
        lblConnPass.Location = new Point(14, 143);
        lblConnPass.Name = "lblConnPass";
        lblConnPass.Size = new Size(60, 15);
        lblConnPass.TabIndex = 4;
        lblConnPass.Text = "Password:";
        // 
        // txtConnPass
        // 
        txtConnPass.Enabled = false;
        txtConnPass.Font = new Font("Segoe UI", 10F);
        txtConnPass.Location = new Point(105, 140);
        txtConnPass.Name = "txtConnPass";
        txtConnPass.Size = new Size(320, 25);
        txtConnPass.TabIndex = 4;
        txtConnPass.UseSystemPasswordChar = true;
        // 
        // btnTestConnect
        // 
        btnTestConnect.Location = new Point(0, 0);
        btnTestConnect.Name = "btnTestConnect";
        btnTestConnect.Size = new Size(75, 23);
        btnTestConnect.TabIndex = 5;
        btnTestConnect.Click += BtnTestConnect_Click;
        // 
        // grpInfo
        // 
        grpInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpInfo.Controls.Add(lblInfoServer);
        grpInfo.Controls.Add(lblInfoServerVal);
        grpInfo.Controls.Add(lblInfoVersion);
        grpInfo.Controls.Add(lblInfoVersionVal);
        grpInfo.Controls.Add(lblInfoEdition);
        grpInfo.Controls.Add(lblInfoEditionVal);
        grpInfo.Controls.Add(lblInfoMachine);
        grpInfo.Controls.Add(lblInfoMachineVal);
        grpInfo.Controls.Add(lblInfoAuth);
        grpInfo.Controls.Add(lblInfoAuthVal);
        grpInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpInfo.ForeColor = Color.FromArgb(30, 30, 50);
        grpInfo.Location = new Point(10, 205);
        grpInfo.Name = "grpInfo";
        grpInfo.Size = new Size(850, 175);
        grpInfo.TabIndex = 1;
        grpInfo.TabStop = false;
        grpInfo.Text = "SQL Server Information";
        // 
        // lblInfoServer
        // 
        lblInfoServer.Location = new Point(0, 0);
        lblInfoServer.Name = "lblInfoServer";
        lblInfoServer.Size = new Size(100, 23);
        lblInfoServer.TabIndex = 0;
        // 
        // lblInfoServerVal
        // 
        lblInfoServerVal.Location = new Point(0, 0);
        lblInfoServerVal.Name = "lblInfoServerVal";
        lblInfoServerVal.Size = new Size(100, 23);
        lblInfoServerVal.TabIndex = 1;
        // 
        // lblInfoVersion
        // 
        lblInfoVersion.Location = new Point(0, 0);
        lblInfoVersion.Name = "lblInfoVersion";
        lblInfoVersion.Size = new Size(100, 23);
        lblInfoVersion.TabIndex = 2;
        // 
        // lblInfoVersionVal
        // 
        lblInfoVersionVal.Location = new Point(0, 0);
        lblInfoVersionVal.Name = "lblInfoVersionVal";
        lblInfoVersionVal.Size = new Size(100, 23);
        lblInfoVersionVal.TabIndex = 3;
        // 
        // lblInfoEdition
        // 
        lblInfoEdition.Location = new Point(0, 0);
        lblInfoEdition.Name = "lblInfoEdition";
        lblInfoEdition.Size = new Size(100, 23);
        lblInfoEdition.TabIndex = 4;
        // 
        // lblInfoEditionVal
        // 
        lblInfoEditionVal.Location = new Point(0, 0);
        lblInfoEditionVal.Name = "lblInfoEditionVal";
        lblInfoEditionVal.Size = new Size(100, 23);
        lblInfoEditionVal.TabIndex = 5;
        // 
        // lblInfoMachine
        // 
        lblInfoMachine.Location = new Point(0, 0);
        lblInfoMachine.Name = "lblInfoMachine";
        lblInfoMachine.Size = new Size(100, 23);
        lblInfoMachine.TabIndex = 6;
        // 
        // lblInfoMachineVal
        // 
        lblInfoMachineVal.Location = new Point(0, 0);
        lblInfoMachineVal.Name = "lblInfoMachineVal";
        lblInfoMachineVal.Size = new Size(100, 23);
        lblInfoMachineVal.TabIndex = 7;
        // 
        // lblInfoAuth
        // 
        lblInfoAuth.Location = new Point(0, 0);
        lblInfoAuth.Name = "lblInfoAuth";
        lblInfoAuth.Size = new Size(100, 23);
        lblInfoAuth.TabIndex = 8;
        // 
        // lblInfoAuthVal
        // 
        lblInfoAuthVal.Location = new Point(0, 0);
        lblInfoAuthVal.Name = "lblInfoAuthVal";
        lblInfoAuthVal.Size = new Size(100, 23);
        lblInfoAuthVal.TabIndex = 9;
        // 
        // tabLogins
        // 
        tabLogins.Controls.Add(btnRefreshLogins);
        tabLogins.Controls.Add(btnToggleLogin);
        tabLogins.Controls.Add(btnDeleteLogin);
        tabLogins.Controls.Add(dgvLogins);
        tabLogins.Controls.Add(grpLoginEdit);
        tabLogins.Controls.Add(grpServerActions);
        tabLogins.Controls.Add(grpLog);
        tabLogins.Font = new Font("Segoe UI", 9F);
        tabLogins.Location = new Point(4, 26);
        tabLogins.Name = "tabLogins";
        tabLogins.Padding = new Padding(3);
        tabLogins.Size = new Size(872, 610);
        tabLogins.TabIndex = 1;
        tabLogins.Text = "👥  Login Accounts";
        // 
        // btnRefreshLogins
        // 
        btnRefreshLogins.Location = new Point(0, 0);
        btnRefreshLogins.Name = "btnRefreshLogins";
        btnRefreshLogins.Size = new Size(75, 23);
        btnRefreshLogins.TabIndex = 0;
        btnRefreshLogins.Click += BtnRefreshLogins_Click;
        // 
        // btnToggleLogin
        // 
        btnToggleLogin.Location = new Point(0, 0);
        btnToggleLogin.Name = "btnToggleLogin";
        btnToggleLogin.Size = new Size(75, 23);
        btnToggleLogin.TabIndex = 1;
        btnToggleLogin.Click += BtnToggleLogin_Click;
        // 
        // btnDeleteLogin
        // 
        btnDeleteLogin.Location = new Point(0, 0);
        btnDeleteLogin.Name = "btnDeleteLogin";
        btnDeleteLogin.Size = new Size(75, 23);
        btnDeleteLogin.TabIndex = 2;
        btnDeleteLogin.Click += BtnDeleteLogin_Click;
        // 
        // dgvLogins
        // 
        dgvLogins.AllowUserToAddRows = false;
        dgvLogins.AllowUserToDeleteRows = false;
        dgvLogins.BackgroundColor = SystemColors.Window;
        dgvLogins.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvLogins.Location = new Point(10, 50);
        dgvLogins.MultiSelect = false;
        dgvLogins.Name = "dgvLogins";
        dgvLogins.ReadOnly = true;
        dgvLogins.RowHeadersVisible = false;
        dgvLogins.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvLogins.Size = new Size(535, 220);
        dgvLogins.TabIndex = 3;
        dgvLogins.SelectionChanged += DgvLogins_SelectionChanged;
        // 
        // grpLoginEdit
        // 
        grpLoginEdit.Controls.Add(lblNewLoginName);
        grpLoginEdit.Controls.Add(txtNewLoginName);
        grpLoginEdit.Controls.Add(lblNewLoginPass);
        grpLoginEdit.Controls.Add(txtNewLoginPass);
        grpLoginEdit.Controls.Add(chkShowLoginPass);
        grpLoginEdit.Controls.Add(chkNewSysAdmin);
        grpLoginEdit.Controls.Add(btnCreateLogin);
        grpLoginEdit.Controls.Add(btnChangeLoginPass);
        grpLoginEdit.Controls.Add(btnConfigureLogin);
        grpLoginEdit.Controls.Add(btnTestLogin);
        grpLoginEdit.Controls.Add(lblHint);
        grpLoginEdit.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpLoginEdit.ForeColor = Color.FromArgb(30, 30, 50);
        grpLoginEdit.Location = new Point(10, 278);
        grpLoginEdit.Name = "grpLoginEdit";
        grpLoginEdit.Size = new Size(535, 220);
        grpLoginEdit.TabIndex = 4;
        grpLoginEdit.TabStop = false;
        grpLoginEdit.Text = "Create / Edit Login";
        // 
        // lblNewLoginName
        // 
        lblNewLoginName.AutoSize = true;
        lblNewLoginName.Font = new Font("Segoe UI", 9F);
        lblNewLoginName.ForeColor = Color.FromArgb(80, 80, 100);
        lblNewLoginName.Location = new Point(12, 32);
        lblNewLoginName.Name = "lblNewLoginName";
        lblNewLoginName.Size = new Size(75, 15);
        lblNewLoginName.TabIndex = 0;
        lblNewLoginName.Text = "Login Name:";
        // 
        // txtNewLoginName
        // 
        txtNewLoginName.Font = new Font("Segoe UI", 10F);
        txtNewLoginName.Location = new Point(105, 29);
        txtNewLoginName.Name = "txtNewLoginName";
        txtNewLoginName.Size = new Size(220, 25);
        txtNewLoginName.TabIndex = 0;
        // 
        // lblNewLoginPass
        // 
        lblNewLoginPass.AutoSize = true;
        lblNewLoginPass.Font = new Font("Segoe UI", 9F);
        lblNewLoginPass.ForeColor = Color.FromArgb(80, 80, 100);
        lblNewLoginPass.Location = new Point(12, 67);
        lblNewLoginPass.Name = "lblNewLoginPass";
        lblNewLoginPass.Size = new Size(60, 15);
        lblNewLoginPass.TabIndex = 1;
        lblNewLoginPass.Text = "Password:";
        // 
        // txtNewLoginPass
        // 
        txtNewLoginPass.Font = new Font("Segoe UI", 10F);
        txtNewLoginPass.Location = new Point(105, 64);
        txtNewLoginPass.Name = "txtNewLoginPass";
        txtNewLoginPass.Size = new Size(220, 25);
        txtNewLoginPass.TabIndex = 1;
        txtNewLoginPass.UseSystemPasswordChar = true;
        // 
        // chkShowLoginPass
        // 
        chkShowLoginPass.AutoSize = true;
        chkShowLoginPass.Font = new Font("Segoe UI", 9F);
        chkShowLoginPass.ForeColor = Color.FromArgb(50, 50, 80);
        chkShowLoginPass.Location = new Point(336, 67);
        chkShowLoginPass.Name = "chkShowLoginPass";
        chkShowLoginPass.Size = new Size(55, 19);
        chkShowLoginPass.TabIndex = 2;
        chkShowLoginPass.Text = "Show";
        chkShowLoginPass.CheckedChanged += ChkShowLoginPass_CheckedChanged;
        // 
        // chkNewSysAdmin
        // 
        chkNewSysAdmin.AutoSize = true;
        chkNewSysAdmin.Font = new Font("Segoe UI", 9F);
        chkNewSysAdmin.ForeColor = Color.FromArgb(50, 50, 80);
        chkNewSysAdmin.Location = new Point(12, 100);
        chkNewSysAdmin.Name = "chkNewSysAdmin";
        chkNewSysAdmin.Size = new Size(165, 19);
        chkNewSysAdmin.TabIndex = 3;
        chkNewSysAdmin.Text = "Grant sysadmin server role";
        // 
        // btnCreateLogin
        // 
        btnCreateLogin.Location = new Point(0, 0);
        btnCreateLogin.Name = "btnCreateLogin";
        btnCreateLogin.Size = new Size(75, 23);
        btnCreateLogin.TabIndex = 4;
        btnCreateLogin.Click += BtnCreateLogin_Click;
        // 
        // btnChangeLoginPass
        // 
        btnChangeLoginPass.Location = new Point(0, 0);
        btnChangeLoginPass.Name = "btnChangeLoginPass";
        btnChangeLoginPass.Size = new Size(75, 23);
        btnChangeLoginPass.TabIndex = 5;
        btnChangeLoginPass.Click += BtnChangeLoginPass_Click;
        // 
        // btnConfigureLogin
        // 
        btnConfigureLogin.Location = new Point(0, 0);
        btnConfigureLogin.Name = "btnConfigureLogin";
        btnConfigureLogin.Size = new Size(75, 23);
        btnConfigureLogin.TabIndex = 6;
        btnConfigureLogin.Click += BtnConfigureLogin_Click;
        // 
        // btnTestLogin
        // 
        btnTestLogin.Location = new Point(0, 0);
        btnTestLogin.Name = "btnTestLogin";
        btnTestLogin.Size = new Size(75, 23);
        btnTestLogin.TabIndex = 7;
        btnTestLogin.Click += BtnTestLogin_Click;
        // 
        // lblHint
        // 
        lblHint.Location = new Point(0, 0);
        lblHint.Name = "lblHint";
        lblHint.Size = new Size(100, 23);
        lblHint.TabIndex = 8;
        // 
        // grpServerActions
        // 
        grpServerActions.Controls.Add(lblServerInstance);
        grpServerActions.Controls.Add(cboServerInstance);
        grpServerActions.Controls.Add(btnEnableMixedMode);
        grpServerActions.Controls.Add(btnRestartSqlService);
        grpServerActions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpServerActions.ForeColor = Color.FromArgb(30, 30, 50);
        grpServerActions.Location = new Point(10, 506);
        grpServerActions.Name = "grpServerActions";
        grpServerActions.Size = new Size(535, 90);
        grpServerActions.TabIndex = 5;
        grpServerActions.TabStop = false;
        grpServerActions.Text = "Server Actions";
        // 
        // lblServerInstance
        // 
        lblServerInstance.AutoSize = true;
        lblServerInstance.Font = new Font("Segoe UI", 9F);
        lblServerInstance.ForeColor = Color.FromArgb(80, 80, 100);
        lblServerInstance.Location = new Point(12, 30);
        lblServerInstance.Name = "lblServerInstance";
        lblServerInstance.Size = new Size(54, 15);
        lblServerInstance.TabIndex = 0;
        lblServerInstance.Text = "Instance:";
        // 
        // cboServerInstance
        // 
        cboServerInstance.Font = new Font("Segoe UI", 10F);
        cboServerInstance.Location = new Point(80, 27);
        cboServerInstance.Name = "cboServerInstance";
        cboServerInstance.Size = new Size(200, 25);
        cboServerInstance.TabIndex = 0;
        // 
        // btnEnableMixedMode
        // 
        btnEnableMixedMode.Location = new Point(0, 0);
        btnEnableMixedMode.Name = "btnEnableMixedMode";
        btnEnableMixedMode.Size = new Size(75, 23);
        btnEnableMixedMode.TabIndex = 1;
        btnEnableMixedMode.Click += BtnEnableMixedMode_Click;
        // 
        // btnRestartSqlService
        // 
        btnRestartSqlService.Location = new Point(0, 0);
        btnRestartSqlService.Name = "btnRestartSqlService";
        btnRestartSqlService.Size = new Size(75, 23);
        btnRestartSqlService.TabIndex = 2;
        btnRestartSqlService.Click += BtnRestartSqlService_Click;
        // 
        // grpLog
        // 
        grpLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
        grpLog.Controls.Add(txtLog);
        grpLog.Controls.Add(btnClearLog);
        grpLog.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpLog.ForeColor = Color.FromArgb(30, 30, 50);
        grpLog.Location = new Point(553, 10);
        grpLog.Name = "grpLog";
        grpLog.Size = new Size(308, 585);
        grpLog.TabIndex = 6;
        grpLog.TabStop = false;
        grpLog.Text = "Operation Log";
        // 
        // txtLog
        // 
        txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtLog.BackColor = Color.FromArgb(18, 18, 30);
        txtLog.Font = new Font("Consolas", 9F);
        txtLog.ForeColor = Color.FromArgb(200, 230, 200);
        txtLog.Location = new Point(8, 22);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Vertical;
        txtLog.Size = new Size(290, 525);
        txtLog.TabIndex = 0;
        // 
        // btnClearLog
        // 
        btnClearLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnClearLog.BackColor = Color.FromArgb(60, 60, 80);
        btnClearLog.FlatAppearance.BorderSize = 0;
        btnClearLog.FlatStyle = FlatStyle.Flat;
        btnClearLog.Font = new Font("Segoe UI", 8.5F);
        btnClearLog.ForeColor = Color.Silver;
        btnClearLog.Location = new Point(210, 553);
        btnClearLog.Name = "btnClearLog";
        btnClearLog.Size = new Size(88, 24);
        btnClearLog.TabIndex = 1;
        btnClearLog.Text = "Clear Log";
        btnClearLog.UseVisualStyleBackColor = false;
        btnClearLog.Click += BtnClearLog_Click;
        // 
        // tabNetwork
        // 
        tabNetwork.Controls.Add(grpNetInstance);
        tabNetwork.Controls.Add(grpNetStatus);
        tabNetwork.Controls.Add(grpNetActions);
        tabNetwork.Font = new Font("Segoe UI", 9F);
        tabNetwork.Location = new Point(4, 26);
        tabNetwork.Name = "tabNetwork";
        tabNetwork.Padding = new Padding(3);
        tabNetwork.Size = new Size(872, 610);
        tabNetwork.TabIndex = 2;
        tabNetwork.Text = "🌐  Network / TCP-IP";
        // 
        // grpNetInstance
        // 
        grpNetInstance.Controls.Add(lblNetInstance);
        grpNetInstance.Controls.Add(cboNetInstance);
        grpNetInstance.Controls.Add(lblNetPort);
        grpNetInstance.Controls.Add(txtNetPort);
        grpNetInstance.Controls.Add(btnCheckNetStatus);
        grpNetInstance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpNetInstance.ForeColor = Color.FromArgb(30, 30, 50);
        grpNetInstance.Location = new Point(10, 10);
        grpNetInstance.Name = "grpNetInstance";
        grpNetInstance.Size = new Size(500, 115);
        grpNetInstance.TabIndex = 0;
        grpNetInstance.TabStop = false;
        grpNetInstance.Text = "Instance & Port";
        // 
        // lblNetInstance
        // 
        lblNetInstance.AutoSize = true;
        lblNetInstance.Font = new Font("Segoe UI", 9F);
        lblNetInstance.ForeColor = Color.FromArgb(80, 80, 100);
        lblNetInstance.Location = new Point(12, 32);
        lblNetInstance.Name = "lblNetInstance";
        lblNetInstance.Size = new Size(78, 15);
        lblNetInstance.TabIndex = 0;
        lblNetInstance.Text = "SQL Instance:";
        // 
        // cboNetInstance
        // 
        cboNetInstance.Font = new Font("Segoe UI", 10F);
        cboNetInstance.Location = new Point(110, 29);
        cboNetInstance.Name = "cboNetInstance";
        cboNetInstance.Size = new Size(220, 25);
        cboNetInstance.TabIndex = 0;
        // 
        // lblNetPort
        // 
        lblNetPort.AutoSize = true;
        lblNetPort.Font = new Font("Segoe UI", 9F);
        lblNetPort.ForeColor = Color.FromArgb(80, 80, 100);
        lblNetPort.Location = new Point(12, 72);
        lblNetPort.Name = "lblNetPort";
        lblNetPort.Size = new Size(55, 15);
        lblNetPort.TabIndex = 1;
        lblNetPort.Text = "TCP Port:";
        // 
        // txtNetPort
        // 
        txtNetPort.Font = new Font("Segoe UI", 10F);
        txtNetPort.Location = new Point(110, 69);
        txtNetPort.Name = "txtNetPort";
        txtNetPort.Size = new Size(80, 25);
        txtNetPort.TabIndex = 1;
        txtNetPort.Text = "1433";
        // 
        // btnCheckNetStatus
        // 
        btnCheckNetStatus.Location = new Point(0, 0);
        btnCheckNetStatus.Name = "btnCheckNetStatus";
        btnCheckNetStatus.Size = new Size(75, 23);
        btnCheckNetStatus.TabIndex = 2;
        btnCheckNetStatus.Click += BtnCheckNetStatus_Click;
        // 
        // grpNetStatus
        // 
        grpNetStatus.Controls.Add(lblNetTcpCaption);
        grpNetStatus.Controls.Add(lblNetTcpStatus);
        grpNetStatus.Controls.Add(lblNetPortCaption);
        grpNetStatus.Controls.Add(lblNetPortStatus);
        grpNetStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpNetStatus.ForeColor = Color.FromArgb(30, 30, 50);
        grpNetStatus.Location = new Point(10, 135);
        grpNetStatus.Name = "grpNetStatus";
        grpNetStatus.Size = new Size(500, 90);
        grpNetStatus.TabIndex = 1;
        grpNetStatus.TabStop = false;
        grpNetStatus.Text = "Current Status";
        // 
        // lblNetTcpCaption
        // 
        lblNetTcpCaption.AutoSize = true;
        lblNetTcpCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblNetTcpCaption.ForeColor = Color.FromArgb(80, 80, 100);
        lblNetTcpCaption.Location = new Point(12, 28);
        lblNetTcpCaption.Name = "lblNetTcpCaption";
        lblNetTcpCaption.Size = new Size(97, 15);
        lblNetTcpCaption.TabIndex = 0;
        lblNetTcpCaption.Text = "TCP/IP Protocol:";
        // 
        // lblNetTcpStatus
        // 
        lblNetTcpStatus.AutoSize = true;
        lblNetTcpStatus.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblNetTcpStatus.ForeColor = Color.DarkGray;
        lblNetTcpStatus.Location = new Point(160, 28);
        lblNetTcpStatus.Name = "lblNetTcpStatus";
        lblNetTcpStatus.Size = new Size(21, 17);
        lblNetTcpStatus.TabIndex = 1;
        lblNetTcpStatus.Text = "—";
        // 
        // lblNetPortCaption
        // 
        lblNetPortCaption.AutoSize = true;
        lblNetPortCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblNetPortCaption.ForeColor = Color.FromArgb(80, 80, 100);
        lblNetPortCaption.Location = new Point(12, 58);
        lblNetPortCaption.Name = "lblNetPortCaption";
        lblNetPortCaption.Size = new Size(58, 15);
        lblNetPortCaption.TabIndex = 2;
        lblNetPortCaption.Text = "TCP Port:";
        // 
        // lblNetPortStatus
        // 
        lblNetPortStatus.AutoSize = true;
        lblNetPortStatus.Font = new Font("Segoe UI", 9.5F);
        lblNetPortStatus.ForeColor = Color.DarkGray;
        lblNetPortStatus.Location = new Point(160, 58);
        lblNetPortStatus.Name = "lblNetPortStatus";
        lblNetPortStatus.Size = new Size(21, 17);
        lblNetPortStatus.TabIndex = 3;
        lblNetPortStatus.Text = "—";
        // 
        // grpNetActions
        // 
        grpNetActions.Controls.Add(btnEnableTcpIp);
        grpNetActions.Controls.Add(btnDisableTcpIp);
        grpNetActions.Controls.Add(btnAddFirewallRule);
        grpNetActions.Controls.Add(btnNetRestartService);
        grpNetActions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpNetActions.ForeColor = Color.FromArgb(30, 30, 50);
        grpNetActions.Location = new Point(10, 235);
        grpNetActions.Name = "grpNetActions";
        grpNetActions.Size = new Size(500, 300);
        grpNetActions.TabIndex = 2;
        grpNetActions.TabStop = false;
        grpNetActions.Text = "Actions  (apply in order: 1 → 2 → 3)";
        // 
        // btnEnableTcpIp
        // 
        btnEnableTcpIp.Location = new Point(0, 0);
        btnEnableTcpIp.Name = "btnEnableTcpIp";
        btnEnableTcpIp.Size = new Size(75, 23);
        btnEnableTcpIp.TabIndex = 0;
        btnEnableTcpIp.Click += BtnEnableTcpIp_Click;
        // 
        // btnDisableTcpIp
        // 
        btnDisableTcpIp.Location = new Point(0, 0);
        btnDisableTcpIp.Name = "btnDisableTcpIp";
        btnDisableTcpIp.Size = new Size(75, 23);
        btnDisableTcpIp.TabIndex = 1;
        btnDisableTcpIp.Click += BtnDisableTcpIp_Click;
        // 
        // btnAddFirewallRule
        // 
        btnAddFirewallRule.Location = new Point(0, 0);
        btnAddFirewallRule.Name = "btnAddFirewallRule";
        btnAddFirewallRule.Size = new Size(75, 23);
        btnAddFirewallRule.TabIndex = 2;
        btnAddFirewallRule.Click += BtnAddFirewallRule_Click;
        // 
        // btnNetRestartService
        // 
        btnNetRestartService.Location = new Point(0, 0);
        btnNetRestartService.Name = "btnNetRestartService";
        btnNetRestartService.Size = new Size(75, 23);
        btnNetRestartService.TabIndex = 3;
        btnNetRestartService.Click += BtnNetRestartService_Click;
        // 
        // pbStatus
        // 
        pbStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        pbStatus.Location = new Point(10, 648);
        pbStatus.Name = "pbStatus";
        pbStatus.Size = new Size(200, 16);
        pbStatus.Style = ProgressBarStyle.Marquee;
        pbStatus.TabIndex = 10;
        pbStatus.Visible = false;
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.Font = new Font("Segoe UI", 9F);
        lblStatus.ForeColor = Color.DarkGray;
        lblStatus.Location = new Point(220, 644);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(640, 22);
        lblStatus.TabIndex = 11;
        lblStatus.Text = "Ready. Select a SQL Server instance and click Test Connection.";
        // 
        // SqlServerManagerForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(245, 247, 250);
        Controls.Add(tabMain);
        Controls.Add(pbStatus);
        Controls.Add(lblStatus);
        Name = "SqlServerManagerForm";
        Size = new Size(880, 670);
        tabMain.ResumeLayout(false);
        tabConnect.ResumeLayout(false);
        grpConn.ResumeLayout(false);
        grpConn.PerformLayout();
        grpInfo.ResumeLayout(false);
        tabLogins.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvLogins).EndInit();
        grpLoginEdit.ResumeLayout(false);
        grpLoginEdit.PerformLayout();
        grpServerActions.ResumeLayout(false);
        grpServerActions.PerformLayout();
        grpLog.ResumeLayout(false);
        grpLog.PerformLayout();
        tabNetwork.ResumeLayout(false);
        grpNetInstance.ResumeLayout(false);
        grpNetInstance.PerformLayout();
        grpNetStatus.ResumeLayout(false);
        grpNetStatus.PerformLayout();
        grpNetActions.ResumeLayout(false);
        ResumeLayout(false);
    }

    // ── Layout helpers ────────────────────────────────────────────────────

    private static void MakeInfoRow(Label caption, Label value,
        string text, int capX, int valX, int y, int yOffset)
    {
        caption.AutoSize  = true;
        caption.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
        caption.ForeColor = Color.FromArgb(80, 80, 100);
        caption.Location  = new Point(capX, y + yOffset);
        caption.Text      = text;

        value.AutoSize  = true;
        value.Font      = new Font("Segoe UI", 9.5F);
        value.ForeColor = Color.FromArgb(20, 20, 40);
        value.Location  = new Point(valX, y + yOffset);
        value.Text      = "—";
    }

    private static void MakeActionButton(Button btn, string text, Color back,
        int x, int y, int w, int h)
    {
        btn.BackColor = back;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btn.ForeColor = Color.White;
        btn.Location  = new Point(x, y);
        btn.Size      = new Size(w, h);
        btn.Text      = text;
        btn.UseVisualStyleBackColor = false;
    }

    private static void MakeToolButton(Button btn, string text, Color back,
        int x, int y, int w, int h)
    {
        btn.BackColor = back;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
        btn.ForeColor = Color.White;
        btn.Location  = new Point(x, y);
        btn.Size      = new Size(w, h);
        btn.Text      = text;
        btn.UseVisualStyleBackColor = false;
    }
    private Label lblHint;
}
