namespace BLogicDevTool;

partial class DatabaseToolsForm
{
    private System.ComponentModel.IContainer components = null;

    // Top status
    private System.Windows.Forms.Label lblConnStatus;

    // Tab control
    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.TabPage tabBackup;
    private System.Windows.Forms.TabPage tabRestoreZip;

    // ── Backup tab ─────────────────────────────────────────────────────────
    private System.Windows.Forms.GroupBox grpBackupInfo;
    private System.Windows.Forms.Label lblDbNameB;
    private System.Windows.Forms.ComboBox cboDbNameB;
    private System.Windows.Forms.Button btnLoginB;
    private System.Windows.Forms.Label lblBackupDir;
    private System.Windows.Forms.TextBox txtBackupDir;
    private System.Windows.Forms.Button btnBrowseDir;
    private System.Windows.Forms.GroupBox grpSqlPreviewB;
    private System.Windows.Forms.TextBox txtSqlPreviewB;
    private System.Windows.Forms.Label lblBackupStatus;
    private System.Windows.Forms.Button btnExecuteBackup;

    // ── Restore from ZIP tab ──────────────────────────────────────────────
    private System.Windows.Forms.GroupBox grpZipFile;
    private System.Windows.Forms.Label lblZipPath;
    private System.Windows.Forms.TextBox txtZipPath;
    private System.Windows.Forms.Button btnBrowseZip;
    private System.Windows.Forms.GroupBox grpBakList;
    private System.Windows.Forms.ListView lvwBakFiles;
    private System.Windows.Forms.ColumnHeader colBakName;
    private System.Windows.Forms.ColumnHeader colBakPathInZip;
    private System.Windows.Forms.ColumnHeader colBakSize;
    private System.Windows.Forms.GroupBox grpZipRestoreInfo;
    private System.Windows.Forms.Label lblDbNameZ;
    private System.Windows.Forms.ComboBox cboDbNameZ;
    private System.Windows.Forms.Button btnLoginZ;
    private System.Windows.Forms.Label lblDataPathZ;
    private System.Windows.Forms.TextBox txtDataPathZ;
    private System.Windows.Forms.Label lblTypeZ;
    private System.Windows.Forms.RadioButton rbBlogicZ;
    private System.Windows.Forms.RadioButton rbMerchantZ;
    private System.Windows.Forms.RadioButton rbMailZ;
    private System.Windows.Forms.GroupBox grpSqlPreviewZ;
    private System.Windows.Forms.TextBox txtSqlPreviewZ;
    private System.Windows.Forms.Label lblZipRestoreStatus;
    private System.Windows.Forms.Button btnExecuteRestoreFromZip;
    private System.Windows.Forms.CheckBox chkClearMailData;
    private System.Windows.Forms.GroupBox grpBakDirect;
    private System.Windows.Forms.Label lblBakDirectPath;
    private System.Windows.Forms.TextBox txtBakDirectPath;
    private System.Windows.Forms.Button btnBrowseBakDirect;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblConnStatus = new Label();
        tabControl = new TabControl();
        tabBackup = new TabPage();
        grpBackupInfo = new GroupBox();
        lblDbNameB = new Label();
        cboDbNameB = new ComboBox();
        btnLoginB = new Button();
        lblBackupDir = new Label();
        txtBackupDir = new TextBox();
        btnBrowseDir = new Button();
        grpSqlPreviewB = new GroupBox();
        txtSqlPreviewB = new TextBox();
        lblBackupStatus = new Label();
        btnExecuteBackup = new Button();
        tabRestoreZip = new TabPage();
        grpZipFile = new GroupBox();
        lblZipPath = new Label();
        txtZipPath = new TextBox();
        btnBrowseZip = new Button();
        grpBakList = new GroupBox();
        lvwBakFiles = new ListView();
        colBakName = new ColumnHeader();
        colBakPathInZip = new ColumnHeader();
        colBakSize = new ColumnHeader();
        grpBakDirect = new GroupBox();
        lblBakDirectPath = new Label();
        txtBakDirectPath = new TextBox();
        btnBrowseBakDirect = new Button();
        grpZipRestoreInfo = new GroupBox();
        lblDbNameZ = new Label();
        cboDbNameZ = new ComboBox();
        btnLoginZ = new Button();
        lblDataPathZ = new Label();
        txtDataPathZ = new TextBox();
        lblTypeZ = new Label();
        rbBlogicZ = new RadioButton();
        rbMerchantZ = new RadioButton();
        rbMailZ = new RadioButton();
        chkClearMailData = new CheckBox();
        grpSqlPreviewZ = new GroupBox();
        txtSqlPreviewZ = new TextBox();
        lblZipRestoreStatus = new Label();
        btnExecuteRestoreFromZip = new Button();
        tabControl.SuspendLayout();
        tabBackup.SuspendLayout();
        grpBackupInfo.SuspendLayout();
        grpSqlPreviewB.SuspendLayout();
        tabRestoreZip.SuspendLayout();
        grpZipFile.SuspendLayout();
        grpBakList.SuspendLayout();
        grpBakDirect.SuspendLayout();
        grpZipRestoreInfo.SuspendLayout();
        grpSqlPreviewZ.SuspendLayout();
        SuspendLayout();
        // 
        // lblConnStatus
        // 
        lblConnStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblConnStatus.Font = new Font("Segoe UI", 9F);
        lblConnStatus.ForeColor = Color.DarkGray;
        lblConnStatus.Location = new Point(10, 5);
        lblConnStatus.Name = "lblConnStatus";
        lblConnStatus.Size = new Size(860, 22);
        lblConnStatus.TabIndex = 0;
        lblConnStatus.Text = "Connecting...";
        // 
        // tabControl
        // 
        tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        tabControl.Controls.Add(tabRestoreZip);
        tabControl.Controls.Add(tabBackup);
        tabControl.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        tabControl.Location = new Point(0, 30);
        tabControl.Name = "tabControl";
        tabControl.SelectedIndex = 0;
        tabControl.Size = new Size(880, 635);
        tabControl.TabIndex = 0;
        // 
        // tabBackup
        // 
        tabBackup.Controls.Add(grpBackupInfo);
        tabBackup.Controls.Add(grpSqlPreviewB);
        tabBackup.Controls.Add(lblBackupStatus);
        tabBackup.Controls.Add(btnExecuteBackup);
        tabBackup.Font = new Font("Segoe UI", 9F);
        tabBackup.Location = new Point(4, 26);
        tabBackup.Name = "tabBackup";
        tabBackup.Padding = new Padding(3);
        tabBackup.Size = new Size(872, 605);
        tabBackup.TabIndex = 1;
        tabBackup.Text = "💾 Backup DB";
        tabBackup.UseVisualStyleBackColor = true;
        // 
        // grpBackupInfo
        // 
        grpBackupInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpBackupInfo.Controls.Add(lblDbNameB);
        grpBackupInfo.Controls.Add(cboDbNameB);
        grpBackupInfo.Controls.Add(btnLoginB);
        grpBackupInfo.Controls.Add(lblBackupDir);
        grpBackupInfo.Controls.Add(txtBackupDir);
        grpBackupInfo.Controls.Add(btnBrowseDir);
        grpBackupInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpBackupInfo.Location = new Point(8, 6);
        grpBackupInfo.Name = "grpBackupInfo";
        grpBackupInfo.Size = new Size(856, 100);
        grpBackupInfo.TabIndex = 0;
        grpBackupInfo.TabStop = false;
        grpBackupInfo.Text = "Backup Info";
        // 
        // lblDbNameB
        // 
        lblDbNameB.AutoSize = true;
        lblDbNameB.Font = new Font("Segoe UI", 9F);
        lblDbNameB.Location = new Point(10, 30);
        lblDbNameB.Name = "lblDbNameB";
        lblDbNameB.Size = new Size(58, 15);
        lblDbNameB.TabIndex = 0;
        lblDbNameB.Text = "Database:";
        // 
        // cboDbNameB
        // 
        cboDbNameB.Font = new Font("Segoe UI", 9F);
        cboDbNameB.Location = new Point(122, 27);
        cboDbNameB.Name = "cboDbNameB";
        cboDbNameB.Size = new Size(622, 23);
        cboDbNameB.TabIndex = 0;
        cboDbNameB.TextChanged += cboDbNameB_TextChanged;
        // 
        // btnLoginB
        // 
        btnLoginB.Font = new Font("Segoe UI", 9F);
        btnLoginB.Location = new Point(751, 27);
        btnLoginB.Name = "btnLoginB";
        btnLoginB.Size = new Size(93, 26);
        btnLoginB.TabIndex = 1;
        btnLoginB.Text = "🔑 Login";
        btnLoginB.UseVisualStyleBackColor = true;
        btnLoginB.Click += btnLogin_Click;
        // 
        // lblBackupDir
        // 
        lblBackupDir.AutoSize = true;
        lblBackupDir.Font = new Font("Segoe UI", 9F);
        lblBackupDir.Location = new Point(10, 65);
        lblBackupDir.Name = "lblBackupDir";
        lblBackupDir.Size = new Size(83, 15);
        lblBackupDir.TabIndex = 1;
        lblBackupDir.Text = "Backup folder:";
        // 
        // txtBackupDir
        // 
        txtBackupDir.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtBackupDir.Font = new Font("Segoe UI", 9F);
        txtBackupDir.Location = new Point(122, 62);
        txtBackupDir.Name = "txtBackupDir";
        txtBackupDir.Size = new Size(622, 23);
        txtBackupDir.TabIndex = 1;
        txtBackupDir.TextChanged += txtBackupDir_TextChanged;
        // 
        // btnBrowseDir
        // 
        btnBrowseDir.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowseDir.Font = new Font("Segoe UI", 9F);
        btnBrowseDir.Location = new Point(751, 61);
        btnBrowseDir.Name = "btnBrowseDir";
        btnBrowseDir.Size = new Size(93, 26);
        btnBrowseDir.TabIndex = 2;
        btnBrowseDir.Text = "Select folder...";
        btnBrowseDir.UseVisualStyleBackColor = true;
        btnBrowseDir.Click += btnBrowseDir_Click;
        // 
        // grpSqlPreviewB
        // 
        grpSqlPreviewB.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grpSqlPreviewB.Controls.Add(txtSqlPreviewB);
        grpSqlPreviewB.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpSqlPreviewB.Location = new Point(8, 114);
        grpSqlPreviewB.Name = "grpSqlPreviewB";
        grpSqlPreviewB.Size = new Size(856, 443);
        grpSqlPreviewB.TabIndex = 1;
        grpSqlPreviewB.TabStop = false;
        grpSqlPreviewB.Text = "SQL Preview";
        // 
        // txtSqlPreviewB
        // 
        txtSqlPreviewB.BackColor = Color.FromArgb(30, 30, 30);
        txtSqlPreviewB.Dock = DockStyle.Fill;
        txtSqlPreviewB.Font = new Font("Consolas", 9F);
        txtSqlPreviewB.ForeColor = Color.LightGreen;
        txtSqlPreviewB.Location = new Point(3, 19);
        txtSqlPreviewB.Multiline = true;
        txtSqlPreviewB.Name = "txtSqlPreviewB";
        txtSqlPreviewB.ScrollBars = ScrollBars.Vertical;
        txtSqlPreviewB.Size = new Size(850, 421);
        txtSqlPreviewB.TabIndex = 0;
        // 
        // lblBackupStatus
        // 
        lblBackupStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblBackupStatus.Font = new Font("Segoe UI", 9F);
        lblBackupStatus.ForeColor = Color.DarkGray;
        lblBackupStatus.Location = new Point(11, 563);
        lblBackupStatus.Name = "lblBackupStatus";
        lblBackupStatus.Size = new Size(640, 22);
        lblBackupStatus.TabIndex = 2;
        lblBackupStatus.Text = "Ready.";
        // 
        // btnExecuteBackup
        // 
        btnExecuteBackup.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnExecuteBackup.BackColor = Color.FromArgb(0, 90, 180);
        btnExecuteBackup.FlatStyle = FlatStyle.Flat;
        btnExecuteBackup.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnExecuteBackup.ForeColor = Color.White;
        btnExecuteBackup.Location = new Point(764, 567);
        btnExecuteBackup.Name = "btnExecuteBackup";
        btnExecuteBackup.Size = new Size(100, 32);
        btnExecuteBackup.TabIndex = 2;
        btnExecuteBackup.Text = "▶ Backup";
        btnExecuteBackup.UseVisualStyleBackColor = false;
        btnExecuteBackup.Click += btnExecuteBackup_Click;
        // 
        // tabRestoreZip
        // 
        tabRestoreZip.Controls.Add(grpZipFile);
        tabRestoreZip.Controls.Add(grpBakList);
        tabRestoreZip.Controls.Add(grpBakDirect);
        tabRestoreZip.Controls.Add(grpZipRestoreInfo);
        tabRestoreZip.Controls.Add(grpSqlPreviewZ);
        tabRestoreZip.Controls.Add(lblZipRestoreStatus);
        tabRestoreZip.Controls.Add(btnExecuteRestoreFromZip);
        tabRestoreZip.Font = new Font("Segoe UI", 9F);
        tabRestoreZip.Location = new Point(4, 26);
        tabRestoreZip.Name = "tabRestoreZip";
        tabRestoreZip.Padding = new Padding(3);
        tabRestoreZip.Size = new Size(872, 605);
        tabRestoreZip.TabIndex = 2;
        tabRestoreZip.Text = "📦 Restore from ZIP";
        tabRestoreZip.UseVisualStyleBackColor = true;
        // 
        // grpZipFile
        // 
        grpZipFile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpZipFile.Controls.Add(lblZipPath);
        grpZipFile.Controls.Add(txtZipPath);
        grpZipFile.Controls.Add(btnBrowseZip);
        grpZipFile.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpZipFile.Location = new Point(8, 6);
        grpZipFile.Name = "grpZipFile";
        grpZipFile.Size = new Size(856, 64);
        grpZipFile.TabIndex = 0;
        grpZipFile.TabStop = false;
        grpZipFile.Text = "ZIP File";
        // 
        // lblZipPath
        // 
        lblZipPath.AutoSize = true;
        lblZipPath.Font = new Font("Segoe UI", 9F);
        lblZipPath.Location = new Point(10, 28);
        lblZipPath.Name = "lblZipPath";
        lblZipPath.Size = new Size(46, 15);
        lblZipPath.TabIndex = 0;
        lblZipPath.Text = "ZIP file:";
        // 
        // txtZipPath
        // 
        txtZipPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtZipPath.Font = new Font("Segoe UI", 9F);
        txtZipPath.Location = new Point(72, 25);
        txtZipPath.Name = "txtZipPath";
        txtZipPath.ReadOnly = true;
        txtZipPath.Size = new Size(676, 23);
        txtZipPath.TabIndex = 1;
        // 
        // btnBrowseZip
        // 
        btnBrowseZip.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowseZip.Font = new Font("Segoe UI", 9F);
        btnBrowseZip.Location = new Point(755, 25);
        btnBrowseZip.Name = "btnBrowseZip";
        btnBrowseZip.Size = new Size(85, 26);
        btnBrowseZip.TabIndex = 2;
        btnBrowseZip.Text = "Browse...";
        btnBrowseZip.UseVisualStyleBackColor = true;
        btnBrowseZip.Click += btnBrowseZip_Click;
        // 
        // grpBakList
        // 
        grpBakList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpBakList.Controls.Add(lvwBakFiles);
        grpBakList.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpBakList.Location = new Point(8, 78);
        grpBakList.Name = "grpBakList";
        grpBakList.Size = new Size(856, 148);
        grpBakList.TabIndex = 1;
        grpBakList.TabStop = false;
        grpBakList.Text = ".bak files found in ZIP";
        // 
        // lvwBakFiles
        // 
        lvwBakFiles.Columns.AddRange(new ColumnHeader[] { colBakName, colBakPathInZip, colBakSize });
        lvwBakFiles.Dock = DockStyle.Fill;
        lvwBakFiles.Font = new Font("Segoe UI", 9F);
        lvwBakFiles.FullRowSelect = true;
        lvwBakFiles.GridLines = true;
        lvwBakFiles.Location = new Point(3, 19);
        lvwBakFiles.MultiSelect = false;
        lvwBakFiles.Name = "lvwBakFiles";
        lvwBakFiles.Size = new Size(850, 126);
        lvwBakFiles.TabIndex = 0;
        lvwBakFiles.UseCompatibleStateImageBehavior = false;
        lvwBakFiles.View = View.Details;
        lvwBakFiles.SelectedIndexChanged += lvwBakFiles_SelectedIndexChanged;
        // 
        // colBakName
        // 
        colBakName.Text = "File name";
        colBakName.Width = 220;
        // 
        // colBakPathInZip
        // 
        colBakPathInZip.Text = "Path in ZIP";
        colBakPathInZip.Width = 480;
        // 
        // colBakSize
        // 
        colBakSize.Text = "Size";
        colBakSize.Width = 120;
        // 
        // grpBakDirect
        // 
        grpBakDirect.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpBakDirect.Controls.Add(lblBakDirectPath);
        grpBakDirect.Controls.Add(txtBakDirectPath);
        grpBakDirect.Controls.Add(btnBrowseBakDirect);
        grpBakDirect.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpBakDirect.Location = new Point(8, 232);
        grpBakDirect.Name = "grpBakDirect";
        grpBakDirect.Size = new Size(856, 55);
        grpBakDirect.TabIndex = 6;
        grpBakDirect.TabStop = false;
        grpBakDirect.Text = "Or select .bak file directly";
        // 
        // lblBakDirectPath
        // 
        lblBakDirectPath.AutoSize = true;
        lblBakDirectPath.Font = new Font("Segoe UI", 9F);
        lblBakDirectPath.Location = new Point(10, 25);
        lblBakDirectPath.Name = "lblBakDirectPath";
        lblBakDirectPath.Size = new Size(51, 15);
        lblBakDirectPath.TabIndex = 0;
        lblBakDirectPath.Text = ".bak file:";
        // 
        // txtBakDirectPath
        // 
        txtBakDirectPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtBakDirectPath.Font = new Font("Segoe UI", 9F);
        txtBakDirectPath.Location = new Point(72, 22);
        txtBakDirectPath.Name = "txtBakDirectPath";
        txtBakDirectPath.Size = new Size(676, 23);
        txtBakDirectPath.TabIndex = 1;
        txtBakDirectPath.TextChanged += txtBakDirectPath_TextChanged;
        // 
        // btnBrowseBakDirect
        // 
        btnBrowseBakDirect.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowseBakDirect.Font = new Font("Segoe UI", 9F);
        btnBrowseBakDirect.Location = new Point(755, 21);
        btnBrowseBakDirect.Name = "btnBrowseBakDirect";
        btnBrowseBakDirect.Size = new Size(89, 26);
        btnBrowseBakDirect.TabIndex = 2;
        btnBrowseBakDirect.Text = "Browse .bak...";
        btnBrowseBakDirect.UseVisualStyleBackColor = true;
        btnBrowseBakDirect.Click += btnBrowseBakDirect_Click;
        // 
        // grpZipRestoreInfo
        // 
        grpZipRestoreInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpZipRestoreInfo.Controls.Add(lblDbNameZ);
        grpZipRestoreInfo.Controls.Add(cboDbNameZ);
        grpZipRestoreInfo.Controls.Add(btnLoginZ);
        grpZipRestoreInfo.Controls.Add(lblDataPathZ);
        grpZipRestoreInfo.Controls.Add(txtDataPathZ);
        grpZipRestoreInfo.Controls.Add(lblTypeZ);
        grpZipRestoreInfo.Controls.Add(rbBlogicZ);
        grpZipRestoreInfo.Controls.Add(rbMerchantZ);
        grpZipRestoreInfo.Controls.Add(rbMailZ);
        grpZipRestoreInfo.Controls.Add(chkClearMailData);
        grpZipRestoreInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpZipRestoreInfo.Location = new Point(8, 294);
        grpZipRestoreInfo.Name = "grpZipRestoreInfo";
        grpZipRestoreInfo.Size = new Size(856, 155);
        grpZipRestoreInfo.TabIndex = 2;
        grpZipRestoreInfo.TabStop = false;
        grpZipRestoreInfo.Text = "Restore Settings";
        // 
        // lblDbNameZ
        // 
        lblDbNameZ.AutoSize = true;
        lblDbNameZ.Font = new Font("Segoe UI", 9F);
        lblDbNameZ.Location = new Point(10, 30);
        lblDbNameZ.Name = "lblDbNameZ";
        lblDbNameZ.Size = new Size(91, 15);
        lblDbNameZ.TabIndex = 0;
        lblDbNameZ.Text = "Database name:";
        // 
        // cboDbNameZ
        // 
        cboDbNameZ.Font = new Font("Segoe UI", 9F);
        cboDbNameZ.Location = new Point(110, 27);
        cboDbNameZ.Name = "cboDbNameZ";
        cboDbNameZ.Size = new Size(638, 23);
        cboDbNameZ.TabIndex = 0;
        cboDbNameZ.TextChanged += cboDbNameZ_TextChanged;
        // 
        // btnLoginZ
        // 
        btnLoginZ.Font = new Font("Segoe UI", 9F);
        btnLoginZ.Location = new Point(755, 25);
        btnLoginZ.Name = "btnLoginZ";
        btnLoginZ.Size = new Size(89, 26);
        btnLoginZ.TabIndex = 1;
        btnLoginZ.Text = "🔑 Login";
        btnLoginZ.UseVisualStyleBackColor = true;
        btnLoginZ.Click += btnLogin_Click;
        // 
        // lblDataPathZ
        // 
        lblDataPathZ.AutoSize = true;
        lblDataPathZ.Font = new Font("Segoe UI", 9F);
        lblDataPathZ.Location = new Point(10, 65);
        lblDataPathZ.Name = "lblDataPathZ";
        lblDataPathZ.Size = new Size(61, 15);
        lblDataPathZ.TabIndex = 1;
        lblDataPathZ.Text = "Data path:";
        // 
        // txtDataPathZ
        // 
        txtDataPathZ.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtDataPathZ.Font = new Font("Segoe UI", 9F);
        txtDataPathZ.Location = new Point(110, 62);
        txtDataPathZ.Name = "txtDataPathZ";
        txtDataPathZ.Size = new Size(734, 23);
        txtDataPathZ.TabIndex = 1;
        txtDataPathZ.TextChanged += txtDataPathZ_TextChanged;
        // 
        // lblTypeZ
        // 
        lblTypeZ.AutoSize = true;
        lblTypeZ.Font = new Font("Segoe UI", 9F);
        lblTypeZ.Location = new Point(10, 100);
        lblTypeZ.Name = "lblTypeZ";
        lblTypeZ.Size = new Size(58, 15);
        lblTypeZ.TabIndex = 2;
        lblTypeZ.Text = "Template:";
        // 
        // rbBlogicZ
        // 
        rbBlogicZ.AutoSize = true;
        rbBlogicZ.Checked = true;
        rbBlogicZ.Font = new Font("Segoe UI", 9F);
        rbBlogicZ.Location = new Point(80, 98);
        rbBlogicZ.Name = "rbBlogicZ";
        rbBlogicZ.Size = new Size(89, 19);
        rbBlogicZ.TabIndex = 2;
        rbBlogicZ.TabStop = true;
        rbBlogicZ.Text = "BLogicPOS7";
        rbBlogicZ.CheckedChanged += rbBlogicZ_CheckedChanged;
        // 
        // rbMerchantZ
        // 
        rbMerchantZ.AutoSize = true;
        rbMerchantZ.Font = new Font("Segoe UI", 9F);
        rbMerchantZ.Location = new Point(183, 98);
        rbMerchantZ.Name = "rbMerchantZ";
        rbMerchantZ.Size = new Size(76, 19);
        rbMerchantZ.TabIndex = 3;
        rbMerchantZ.Text = "Merchant";
        rbMerchantZ.CheckedChanged += rbMerchantZ_CheckedChanged;
        // 
        // rbMailZ
        // 
        rbMailZ.AutoSize = true;
        rbMailZ.Font = new Font("Segoe UI", 9F);
        rbMailZ.Location = new Point(273, 98);
        rbMailZ.Name = "rbMailZ";
        rbMailZ.Size = new Size(127, 19);
        rbMailZ.TabIndex = 4;
        rbMailZ.Text = "BLogicEmailService";
        rbMailZ.Visible = false;
        rbMailZ.CheckedChanged += rbMailZ_CheckedChanged;
        // 
        // chkClearMailData
        // 
        chkClearMailData.AutoSize = true;
        chkClearMailData.Checked = true;
        chkClearMailData.CheckState = CheckState.Checked;
        chkClearMailData.Font = new Font("Segoe UI", 9F);
        chkClearMailData.Location = new Point(10, 128);
        chkClearMailData.Name = "chkClearMailData";
        chkClearMailData.Size = new Size(359, 19);
        chkClearMailData.TabIndex = 4;
        chkClearMailData.Text = "Xoá dữ liệu mail sau khi restore (EmailInfos, SevenShift configs)";
        chkClearMailData.UseVisualStyleBackColor = true;
        // 
        // grpSqlPreviewZ
        // 
        grpSqlPreviewZ.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grpSqlPreviewZ.Controls.Add(txtSqlPreviewZ);
        grpSqlPreviewZ.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpSqlPreviewZ.Location = new Point(8, 456);
        grpSqlPreviewZ.Name = "grpSqlPreviewZ";
        grpSqlPreviewZ.Size = new Size(856, 101);
        grpSqlPreviewZ.TabIndex = 3;
        grpSqlPreviewZ.TabStop = false;
        grpSqlPreviewZ.Text = "SQL Preview";
        // 
        // txtSqlPreviewZ
        // 
        txtSqlPreviewZ.BackColor = Color.FromArgb(30, 30, 30);
        txtSqlPreviewZ.Dock = DockStyle.Fill;
        txtSqlPreviewZ.Font = new Font("Consolas", 9F);
        txtSqlPreviewZ.ForeColor = Color.LightGreen;
        txtSqlPreviewZ.Location = new Point(3, 19);
        txtSqlPreviewZ.Multiline = true;
        txtSqlPreviewZ.Name = "txtSqlPreviewZ";
        txtSqlPreviewZ.ScrollBars = ScrollBars.Vertical;
        txtSqlPreviewZ.Size = new Size(850, 79);
        txtSqlPreviewZ.TabIndex = 0;
        // 
        // lblZipRestoreStatus
        // 
        lblZipRestoreStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblZipRestoreStatus.Font = new Font("Segoe UI", 9F);
        lblZipRestoreStatus.ForeColor = Color.DarkGray;
        lblZipRestoreStatus.Location = new Point(8, 560);
        lblZipRestoreStatus.Name = "lblZipRestoreStatus";
        lblZipRestoreStatus.Size = new Size(640, 22);
        lblZipRestoreStatus.TabIndex = 4;
        lblZipRestoreStatus.Text = "Ready.";
        // 
        // btnExecuteRestoreFromZip
        // 
        btnExecuteRestoreFromZip.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnExecuteRestoreFromZip.BackColor = Color.FromArgb(16, 124, 16);
        btnExecuteRestoreFromZip.FlatStyle = FlatStyle.Flat;
        btnExecuteRestoreFromZip.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnExecuteRestoreFromZip.ForeColor = Color.White;
        btnExecuteRestoreFromZip.Location = new Point(750, 563);
        btnExecuteRestoreFromZip.Name = "btnExecuteRestoreFromZip";
        btnExecuteRestoreFromZip.Size = new Size(116, 32);
        btnExecuteRestoreFromZip.TabIndex = 5;
        btnExecuteRestoreFromZip.Text = "▶ Restore ZIP";
        btnExecuteRestoreFromZip.UseVisualStyleBackColor = false;
        btnExecuteRestoreFromZip.Click += btnExecuteRestoreFromZip_Click;
        // 
        // DatabaseToolsForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(lblConnStatus);
        Controls.Add(tabControl);
        Name = "DatabaseToolsForm";
        Size = new Size(880, 665);
        tabControl.ResumeLayout(false);
        tabBackup.ResumeLayout(false);
        grpBackupInfo.ResumeLayout(false);
        grpBackupInfo.PerformLayout();
        grpSqlPreviewB.ResumeLayout(false);
        grpSqlPreviewB.PerformLayout();
        tabRestoreZip.ResumeLayout(false);
        grpZipFile.ResumeLayout(false);
        grpZipFile.PerformLayout();
        grpBakList.ResumeLayout(false);
        grpBakDirect.ResumeLayout(false);
        grpBakDirect.PerformLayout();
        grpZipRestoreInfo.ResumeLayout(false);
        grpZipRestoreInfo.PerformLayout();
        grpSqlPreviewZ.ResumeLayout(false);
        grpSqlPreviewZ.PerformLayout();
        ResumeLayout(false);
    }
}
