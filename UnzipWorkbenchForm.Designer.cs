namespace BLogicDevTool;

partial class UnzipWorkbenchForm
{
    private System.ComponentModel.IContainer components = null;

    // ── Top bar ───────────────────────────────────────────────────────────────
    private System.Windows.Forms.Panel        pnlTop;
    private System.Windows.Forms.Label        lblZipFile;
    private System.Windows.Forms.TextBox      txtZipPath;
    private System.Windows.Forms.Button       btnBrowseZip;
    private System.Windows.Forms.Button       btnToggleSettings;
    private System.Windows.Forms.Label        lblDb;
    private System.Windows.Forms.ComboBox     cboDatabase;
    private System.Windows.Forms.Button       btnLoginDb;

    // ── Settings overlay ──────────────────────────────────────────────────────
    private System.Windows.Forms.Panel                     pnlSettings;
    private System.Windows.Forms.Label                     lblSettingsNote;
    private System.Windows.Forms.DataGridView              dgvMappings;
    private System.Windows.Forms.DataGridViewTextBoxColumn colFolder;
    private System.Windows.Forms.DataGridViewTextBoxColumn colDest;
    private System.Windows.Forms.Button                    btnAddRow;
    private System.Windows.Forms.Button                    btnDeleteRow;
    private System.Windows.Forms.Button                    btnSaveConfig;
    private System.Windows.Forms.Button                    btnCloseSettings;

    // ── SQL area (split: 1/3 list | 2/3 preview) ─────────────────────────────
    private System.Windows.Forms.SplitContainer splitMain;
    private System.Windows.Forms.Label          lblSqlFiles;
    private System.Windows.Forms.ListBox        lstSqlFiles;
    private System.Windows.Forms.TextBox        txtSqlPreview;
    private System.Windows.Forms.Button         btnRun;
    private System.Windows.Forms.Button         btnRestoreOriginal;

    // ── Bottom deploy area ────────────────────────────────────────────────────
    private System.Windows.Forms.Panel       pnlBottom;
    private System.Windows.Forms.Panel       pnlDeployBar;
    private System.Windows.Forms.Button      btnDeploy;
    private System.Windows.Forms.Label       lblStatus;
    private System.Windows.Forms.ProgressBar progressBar;
    private System.Windows.Forms.GroupBox    grpLog;
    private System.Windows.Forms.TextBox     txtLog;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlTop = new Panel();
        lblZipFile = new Label();
        txtZipPath = new TextBox();
        btnBrowseZip = new Button();
        btnToggleSettings = new Button();
        lblDb = new Label();
        cboDatabase = new ComboBox();
        btnLoginDb = new Button();
        pnlSettings = new Panel();
        lblSettingsNote = new Label();
        dgvMappings = new DataGridView();
        colFolder = new DataGridViewTextBoxColumn();
        colDest = new DataGridViewTextBoxColumn();
        btnAddRow = new Button();
        btnDeleteRow = new Button();
        btnSaveConfig = new Button();
        btnCloseSettings = new Button();
        splitMain = new SplitContainer();
        lstSqlFiles = new ListBox();
        lblSqlFiles = new Label();
        btnRun = new Button();
        btnDeploy = new Button();
        btnRestoreOriginal = new Button();
        txtSqlPreview = new TextBox();
        label1 = new Label();
        pnlBottom = new Panel();
        grpLog = new GroupBox();
        txtLog = new TextBox();
        pnlDeployBar = new Panel();
        progressBar = new ProgressBar();
        lblStatus = new Label();
        pnlTop.SuspendLayout();
        pnlSettings.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvMappings).BeginInit();
        ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
        splitMain.Panel1.SuspendLayout();
        splitMain.Panel2.SuspendLayout();
        splitMain.SuspendLayout();
        pnlBottom.SuspendLayout();
        grpLog.SuspendLayout();
        pnlDeployBar.SuspendLayout();
        SuspendLayout();
        // 
        // pnlTop
        // 
        pnlTop.Controls.Add(lblZipFile);
        pnlTop.Controls.Add(txtZipPath);
        pnlTop.Controls.Add(btnBrowseZip);
        pnlTop.Controls.Add(btnToggleSettings);
        pnlTop.Controls.Add(lblDb);
        pnlTop.Controls.Add(cboDatabase);
        pnlTop.Controls.Add(btnLoginDb);
        pnlTop.Dock = DockStyle.Top;
        pnlTop.Location = new Point(0, 0);
        pnlTop.Name = "pnlTop";
        pnlTop.Size = new Size(880, 60);
        pnlTop.TabIndex = 2;
        // 
        // lblZipFile
        // 
        lblZipFile.AutoSize = true;
        lblZipFile.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblZipFile.Location = new Point(8, 12);
        lblZipFile.Name = "lblZipFile";
        lblZipFile.Size = new Size(50, 15);
        lblZipFile.TabIndex = 0;
        lblZipFile.Text = "ZIP File:";
        // 
        // txtZipPath
        // 
        txtZipPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtZipPath.Location = new Point(68, 8);
        txtZipPath.Name = "txtZipPath";
        txtZipPath.PlaceholderText = "Select .zip file...";
        txtZipPath.Size = new Size(507, 23);
        txtZipPath.TabIndex = 1;
        txtZipPath.TextChanged += txtZipPath_TextChanged;
        // 
        // btnBrowseZip
        // 
        btnBrowseZip.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowseZip.Location = new Point(581, 8);
        btnBrowseZip.Name = "btnBrowseZip";
        btnBrowseZip.Size = new Size(92, 26);
        btnBrowseZip.TabIndex = 2;
        btnBrowseZip.Text = "Select file...";
        btnBrowseZip.Click += btnBrowseZip_Click;
        // 
        // btnToggleSettings
        // 
        btnToggleSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnToggleSettings.Location = new Point(679, 8);
        btnToggleSettings.Name = "btnToggleSettings";
        btnToggleSettings.Size = new Size(102, 26);
        btnToggleSettings.TabIndex = 4;
        btnToggleSettings.Text = "⚙ Settings";
        btnToggleSettings.Click += btnToggleSettings_Click;
        // 
        // lblDb
        // 
        lblDb.AutoSize = true;
        lblDb.Location = new Point(8, 38);
        lblDb.Name = "lblDb";
        lblDb.Size = new Size(58, 15);
        lblDb.TabIndex = 5;
        lblDb.Text = "Database:";
        // 
        // cboDatabase
        // 
        cboDatabase.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        cboDatabase.AutoCompleteSource = AutoCompleteSource.ListItems;
        cboDatabase.Font = new Font("Consolas", 9.5F);
        cboDatabase.Location = new Point(68, 34);
        cboDatabase.Name = "cboDatabase";
        cboDatabase.Size = new Size(507, 23);
        cboDatabase.TabIndex = 6;
        // 
        // btnLoginDb
        // 
        btnLoginDb.Font = new Font("Segoe UI", 9F);
        btnLoginDb.Location = new Point(581, 34);
        btnLoginDb.Name = "btnLoginDb";
        btnLoginDb.Size = new Size(92, 26);
        btnLoginDb.TabIndex = 7;
        btnLoginDb.Text = "🔑 Login";
        btnLoginDb.UseVisualStyleBackColor = true;
        btnLoginDb.Click += btnLoginDb_Click;
        // 
        // pnlSettings
        // 
        pnlSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlSettings.BackColor = Color.FromArgb(245, 247, 250);
        pnlSettings.BorderStyle = BorderStyle.FixedSingle;
        pnlSettings.Controls.Add(lblSettingsNote);
        pnlSettings.Controls.Add(dgvMappings);
        pnlSettings.Controls.Add(btnAddRow);
        pnlSettings.Controls.Add(btnDeleteRow);
        pnlSettings.Controls.Add(btnSaveConfig);
        pnlSettings.Controls.Add(btnCloseSettings);
        pnlSettings.Location = new Point(0, 60);
        pnlSettings.Name = "pnlSettings";
        pnlSettings.Size = new Size(880, 268);
        pnlSettings.TabIndex = 3;
        pnlSettings.Visible = false;
        // 
        // lblSettingsNote
        // 
        lblSettingsNote.AutoSize = true;
        lblSettingsNote.Font = new Font("Segoe UI", 9F);
        lblSettingsNote.ForeColor = Color.DimGray;
        lblSettingsNote.Location = new Point(8, 8);
        lblSettingsNote.Name = "lblSettingsNote";
        lblSettingsNote.Size = new Size(368, 15);
        lblSettingsNote.TabIndex = 0;
        lblSettingsNote.Text = "Configure folder names in ZIP and their extraction destination paths:";
        // 
        // dgvMappings
        // 
        dgvMappings.AllowUserToAddRows = false;
        dgvMappings.AllowUserToDeleteRows = false;
        dgvMappings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        dgvMappings.BackgroundColor = SystemColors.Window;
        dgvMappings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvMappings.Columns.AddRange(new DataGridViewColumn[] { colFolder, colDest });
        dgvMappings.Font = new Font("Segoe UI", 9.5F);
        dgvMappings.Location = new Point(8, 28);
        dgvMappings.MultiSelect = false;
        dgvMappings.Name = "dgvMappings";
        dgvMappings.RowHeadersVisible = false;
        dgvMappings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvMappings.Size = new Size(862, 190);
        dgvMappings.TabIndex = 1;
        // 
        // colFolder
        // 
        colFolder.HeaderText = "Folder Name (in ZIP)";
        colFolder.Name = "colFolder";
        colFolder.Width = 220;
        // 
        // colDest
        // 
        colDest.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        colDest.HeaderText = "Extraction Path";
        colDest.Name = "colDest";
        // 
        // btnAddRow
        // 
        btnAddRow.Location = new Point(8, 226);
        btnAddRow.Name = "btnAddRow";
        btnAddRow.Size = new Size(90, 28);
        btnAddRow.TabIndex = 2;
        btnAddRow.Text = "+ Add Row";
        btnAddRow.Click += btnAddRow_Click;
        // 
        // btnDeleteRow
        // 
        btnDeleteRow.Location = new Point(104, 226);
        btnDeleteRow.Name = "btnDeleteRow";
        btnDeleteRow.Size = new Size(95, 28);
        btnDeleteRow.TabIndex = 3;
        btnDeleteRow.Text = "- Delete Row";
        btnDeleteRow.Click += btnDeleteRow_Click;
        // 
        // btnSaveConfig
        // 
        btnSaveConfig.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSaveConfig.BackColor = Color.FromArgb(0, 120, 212);
        btnSaveConfig.FlatStyle = FlatStyle.Flat;
        btnSaveConfig.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnSaveConfig.ForeColor = Color.White;
        btnSaveConfig.Location = new Point(678, 226);
        btnSaveConfig.Name = "btnSaveConfig";
        btnSaveConfig.Size = new Size(90, 28);
        btnSaveConfig.TabIndex = 4;
        btnSaveConfig.Text = "💾 Save";
        btnSaveConfig.UseVisualStyleBackColor = false;
        btnSaveConfig.Click += btnSaveConfig_Click;
        // 
        // btnCloseSettings
        // 
        btnCloseSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCloseSettings.Location = new Point(774, 226);
        btnCloseSettings.Name = "btnCloseSettings";
        btnCloseSettings.Size = new Size(96, 28);
        btnCloseSettings.TabIndex = 5;
        btnCloseSettings.Text = "✕ Close";
        btnCloseSettings.Click += btnCloseSettings_Click;
        // 
        // splitMain
        // 
        splitMain.Dock = DockStyle.Fill;
        splitMain.Location = new Point(0, 60);
        splitMain.Name = "splitMain";
        // 
        // splitMain.Panel1
        // 
        splitMain.Panel1.Controls.Add(lstSqlFiles);
        splitMain.Panel1.Controls.Add(lblSqlFiles);
        splitMain.Panel1.Controls.Add(btnRestoreOriginal);
        splitMain.Panel1.Controls.Add(btnRun);
        splitMain.Panel1.Controls.Add(btnDeploy);
        // 
        // splitMain.Panel2
        // 
        splitMain.Panel2.Controls.Add(txtSqlPreview);
        splitMain.Panel2.Controls.Add(label1);
        splitMain.Size = new Size(880, 430);
        splitMain.SplitterDistance = 226;
        splitMain.SplitterWidth = 5;
        splitMain.TabIndex = 0;
        // 
        // lstSqlFiles
        // 
        lstSqlFiles.Dock = DockStyle.Fill;
        lstSqlFiles.Font = new Font("Consolas", 8.5F);
        lstSqlFiles.HorizontalScrollbar = true;
        lstSqlFiles.ItemHeight = 13;
        lstSqlFiles.Location = new Point(0, 22);
        lstSqlFiles.Name = "lstSqlFiles";
        lstSqlFiles.Size = new Size(226, 348);
        lstSqlFiles.TabIndex = 0;
        lstSqlFiles.SelectedIndexChanged += lstSqlFiles_SelectedIndexChanged;
        // 
        // lblSqlFiles
        // 
        lblSqlFiles.Dock = DockStyle.Top;
        lblSqlFiles.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblSqlFiles.Location = new Point(0, 0);
        lblSqlFiles.Name = "lblSqlFiles";
        lblSqlFiles.Padding = new Padding(4, 0, 0, 0);
        lblSqlFiles.Size = new Size(226, 22);
        lblSqlFiles.TabIndex = 1;
        lblSqlFiles.Text = "SQL Scripts:";
        lblSqlFiles.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // btnRun
        // 
        btnRun.BackColor = Color.FromArgb(0, 120, 212);
        btnRun.Dock = DockStyle.Bottom;
        btnRun.FlatStyle = FlatStyle.Flat;
        btnRun.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnRun.ForeColor = Color.White;
        btnRun.Location = new Point(0, 370);
        btnRun.Name = "btnRun";
        btnRun.Size = new Size(226, 30);
        btnRun.TabIndex = 0;
        btnRun.Text = "▶ Run SQL";
        btnRun.UseVisualStyleBackColor = false;
        btnRun.Click += btnRun_Click;
        // 
        // btnDeploy
        // 
        btnDeploy.BackColor = Color.FromArgb(16, 124, 16);
        btnDeploy.Dock = DockStyle.Bottom;
        btnDeploy.FlatStyle = FlatStyle.Flat;
        btnDeploy.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnDeploy.ForeColor = Color.White;
        btnDeploy.Location = new Point(0, 400);
        btnDeploy.Name = "btnDeploy";
        btnDeploy.Size = new Size(226, 30);
        btnDeploy.TabIndex = 0;
        btnDeploy.Text = "🚀 Deploy";
        btnDeploy.UseVisualStyleBackColor = false;
        btnDeploy.Click += btnDeploy_Click;
        //
        // btnRestoreOriginal
        //
        btnRestoreOriginal.BackColor = Color.FromArgb(120, 90, 30);
        btnRestoreOriginal.Dock = DockStyle.Bottom;
        btnRestoreOriginal.FlatStyle = FlatStyle.Flat;
        btnRestoreOriginal.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnRestoreOriginal.ForeColor = Color.White;
        btnRestoreOriginal.Location = new Point(0, 340);
        btnRestoreOriginal.Name = "btnRestoreOriginal";
        btnRestoreOriginal.Size = new Size(226, 30);
        btnRestoreOriginal.TabIndex = 0;
        btnRestoreOriginal.Text = "↩ Rollback Task";
        btnRestoreOriginal.UseVisualStyleBackColor = false;
        btnRestoreOriginal.Click += btnRestoreOriginal_Click;
        //
        // txtSqlPreview
        //
        txtSqlPreview.BackColor = Color.FromArgb(30, 30, 30);
        txtSqlPreview.Dock = DockStyle.Fill;
        txtSqlPreview.Font = new Font("Consolas", 9F);
        txtSqlPreview.ForeColor = Color.FromArgb(220, 220, 170);
        txtSqlPreview.Location = new Point(0, 22);
        txtSqlPreview.Multiline = true;
        txtSqlPreview.Name = "txtSqlPreview";
        txtSqlPreview.ScrollBars = ScrollBars.Both;
        txtSqlPreview.Size = new Size(649, 408);
        txtSqlPreview.TabIndex = 0;
        txtSqlPreview.WordWrap = false;
        // 
        // label1
        // 
        label1.Dock = DockStyle.Top;
        label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        label1.Location = new Point(0, 0);
        label1.Name = "label1";
        label1.Padding = new Padding(4, 0, 0, 0);
        label1.Size = new Size(649, 22);
        label1.TabIndex = 3;
        label1.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // pnlBottom
        // 
        pnlBottom.Controls.Add(grpLog);
        pnlBottom.Controls.Add(pnlDeployBar);
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Location = new Point(0, 490);
        pnlBottom.Name = "pnlBottom";
        pnlBottom.Size = new Size(880, 210);
        pnlBottom.TabIndex = 1;
        // 
        // grpLog
        // 
        grpLog.Controls.Add(txtLog);
        grpLog.Dock = DockStyle.Fill;
        grpLog.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpLog.Location = new Point(0, 51);
        grpLog.Name = "grpLog";
        grpLog.Size = new Size(880, 159);
        grpLog.TabIndex = 0;
        grpLog.TabStop = false;
        grpLog.Text = "Deployment / SQL Log";
        // 
        // txtLog
        // 
        txtLog.BackColor = Color.FromArgb(30, 30, 30);
        txtLog.Dock = DockStyle.Fill;
        txtLog.Font = new Font("Consolas", 9F);
        txtLog.ForeColor = Color.LightGreen;
        txtLog.Location = new Point(3, 19);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Vertical;
        txtLog.Size = new Size(874, 137);
        txtLog.TabIndex = 0;
        // 
        // pnlDeployBar
        // 
        pnlDeployBar.Controls.Add(progressBar);
        pnlDeployBar.Controls.Add(lblStatus);
        pnlDeployBar.Dock = DockStyle.Top;
        pnlDeployBar.Location = new Point(0, 0);
        pnlDeployBar.Name = "pnlDeployBar";
        pnlDeployBar.Padding = new Padding(4);
        pnlDeployBar.Size = new Size(880, 51);
        pnlDeployBar.TabIndex = 1;
        // 
        // progressBar
        // 
        progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        progressBar.Location = new Point(0, 10);
        progressBar.MarqueeAnimationSpeed = 40;
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(880, 18);
        progressBar.Style = ProgressBarStyle.Marquee;
        progressBar.TabIndex = 1;
        progressBar.Visible = false;
        // 
        // lblStatus
        // 
        lblStatus.Dock = DockStyle.Bottom;
        lblStatus.Font = new Font("Segoe UI", 9F);
        lblStatus.Location = new Point(4, 29);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(872, 18);
        lblStatus.TabIndex = 2;
        lblStatus.Text = "Ready.";
        lblStatus.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // UnzipWorkbenchForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(splitMain);
        Controls.Add(pnlBottom);
        Controls.Add(pnlTop);
        Controls.Add(pnlSettings);
        Name = "UnzipWorkbenchForm";
        Size = new Size(880, 700);
        pnlTop.ResumeLayout(false);
        pnlTop.PerformLayout();
        pnlSettings.ResumeLayout(false);
        pnlSettings.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvMappings).EndInit();
        splitMain.Panel1.ResumeLayout(false);
        splitMain.Panel2.ResumeLayout(false);
        splitMain.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
        splitMain.ResumeLayout(false);
        pnlBottom.ResumeLayout(false);
        grpLog.ResumeLayout(false);
        grpLog.PerformLayout();
        pnlDeployBar.ResumeLayout(false);
        ResumeLayout(false);
    }
    private Label label1;
}
