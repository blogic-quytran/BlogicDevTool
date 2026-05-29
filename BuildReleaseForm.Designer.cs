namespace BLogicDevTool;

partial class BuildReleaseForm
{
    private System.ComponentModel.IContainer components = null!;

    private Panel pnlGlobal;
    private Label lblGlobalOutBase;
    private TextBox txtGlobalOutBase;
    private Button btnGlobalBrowseOutBase;
    private Label lblExtensions;
    private Button btnExtensions;

    private Panel pnlList;
    private Button btnAdd;
    private Button btnDelete;
    private Button btnMoveUp;
    private Button btnMoveDown;
    private CheckedListBox clbConfigs;

    private Panel pnlDetail;
    private Label lblName;
    private TextBox txtName;
    private Label lblSln;
    private TextBox txtSln;
    private Button btnBrowseSln;
    private Label lblConfig;
    private ComboBox cboConfig;
    private Label lblEngine;
    private ComboBox cboEngine;
    private Label lblOutFolder;
    private TextBox txtOutFolder;
    private Label lblOutPreview;
    private Label lblExtraFolders;
    private TextBox txtExtraFolders;
    private Label lblRedistribute;
    private Button btnRedistribute;
    private CheckBox chkFilterGit;
    private Label lblFilterInfo;
    private Button btnSaveConfig;

    private Label lblGitBase;
    private TextBox txtGitBase;
    private Label lblGitCompare;
    private TextBox txtGitCompare;

    private Panel pnlActions;
    private Button btnPreview;
    private Button btnBuildChecked;
    private Button btnBuildAll;
    private Label lblStatus;
    private ProgressBar progressBar;

    private TextBox txtLog;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlGlobal = new Panel();
        txtGitCompare = new TextBox();
        txtGitBase = new TextBox();
        lblGlobalOutBase = new Label();
        txtGlobalOutBase = new TextBox();
        btnGlobalBrowseOutBase = new Button();
        lblExtensions = new Label();
        btnExtensions = new Button();
        lblGitBase = new Label();
        lblGitCompare = new Label();
        pnlList = new Panel();
        btnAdd = new Button();
        btnDelete = new Button();
        btnMoveUp = new Button();
        btnMoveDown = new Button();
        clbConfigs = new CheckedListBox();
        pnlDetail = new Panel();
        lblName = new Label();
        txtName = new TextBox();
        lblSln = new Label();
        txtSln = new TextBox();
        btnBrowseSln = new Button();
        lblConfig = new Label();
        cboConfig = new ComboBox();
        lblEngine = new Label();
        cboEngine = new ComboBox();
        lblOutFolder = new Label();
        txtOutFolder = new TextBox();
        lblOutPreview = new Label();
        lblExtraFolders = new Label();
        txtExtraFolders = new TextBox();
        lblRedistribute = new Label();
        btnRedistribute = new Button();
        chkFilterGit = new CheckBox();
        lblFilterInfo = new Label();
        btnSaveConfig = new Button();
        pnlActions = new Panel();
        btnPreview = new Button();
        btnBuildChecked = new Button();
        btnBuildAll = new Button();
        lblStatus = new Label();
        progressBar = new ProgressBar();
        txtLog = new TextBox();
        pnlGlobal.SuspendLayout();
        pnlList.SuspendLayout();
        pnlDetail.SuspendLayout();
        pnlActions.SuspendLayout();
        SuspendLayout();
        // 
        // pnlGlobal
        // 
        pnlGlobal.BackColor = Color.FromArgb(245, 247, 250);
        pnlGlobal.BorderStyle = BorderStyle.FixedSingle;
        pnlGlobal.Controls.Add(txtGitCompare);
        pnlGlobal.Controls.Add(txtGitBase);
        pnlGlobal.Controls.Add(lblGlobalOutBase);
        pnlGlobal.Controls.Add(txtGlobalOutBase);
        pnlGlobal.Controls.Add(btnGlobalBrowseOutBase);
        pnlGlobal.Controls.Add(lblConfig);
        pnlGlobal.Controls.Add(cboConfig);
        pnlGlobal.Controls.Add(lblExtensions);
        pnlGlobal.Controls.Add(btnExtensions);
        pnlGlobal.Controls.Add(lblGitBase);
        pnlGlobal.Controls.Add(lblGitCompare);
        pnlGlobal.Location = new Point(8, 8);
        pnlGlobal.Name = "pnlGlobal";
        pnlGlobal.Size = new Size(864, 64);
        pnlGlobal.TabIndex = 0;
        // 
        // txtGitCompare
        // 
        txtGitCompare.Location = new Point(662, 36);
        txtGitCompare.Name = "txtGitCompare";
        txtGitCompare.Size = new Size(196, 23);
        txtGitCompare.TabIndex = 8;
        txtGitCompare.TextChanged += txtGitCompare_TextChanged;
        // 
        // txtGitBase
        // 
        txtGitBase.Location = new Point(394, 36);
        txtGitBase.Name = "txtGitBase";
        txtGitBase.Size = new Size(187, 23);
        txtGitBase.TabIndex = 6;
        txtGitBase.TextChanged += txtGitBase_TextChanged;
        // 
        // lblGlobalOutBase
        // 
        lblGlobalOutBase.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblGlobalOutBase.Location = new Point(8, 6);
        lblGlobalOutBase.Name = "lblGlobalOutBase";
        lblGlobalOutBase.Size = new Size(127, 20);
        lblGlobalOutBase.TabIndex = 0;
        lblGlobalOutBase.Text = "Output Base (shared):";
        lblGlobalOutBase.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtGlobalOutBase
        // 
        txtGlobalOutBase.Location = new Point(141, 4);
        txtGlobalOutBase.Name = "txtGlobalOutBase";
        txtGlobalOutBase.Size = new Size(430, 23);
        txtGlobalOutBase.TabIndex = 1;
        txtGlobalOutBase.TextChanged += txtGlobalOutBase_TextChanged;
        //
        // btnGlobalBrowseOutBase
        //
        btnGlobalBrowseOutBase.Location = new Point(575, 2);
        btnGlobalBrowseOutBase.Name = "btnGlobalBrowseOutBase";
        btnGlobalBrowseOutBase.Size = new Size(66, 26);
        btnGlobalBrowseOutBase.TabIndex = 2;
        btnGlobalBrowseOutBase.Text = "Browse";
        btnGlobalBrowseOutBase.UseVisualStyleBackColor = true;
        btnGlobalBrowseOutBase.Click += btnGlobalBrowseOutBase_Click;
        // 
        // lblExtensions
        // 
        lblExtensions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblExtensions.Location = new Point(8, 38);
        lblExtensions.Name = "lblExtensions";
        lblExtensions.Size = new Size(112, 20);
        lblExtensions.TabIndex = 3;
        lblExtensions.Text = "Copy Extensions:";
        lblExtensions.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // btnExtensions
        // 
        btnExtensions.Location = new Point(141, 36);
        btnExtensions.Name = "btnExtensions";
        btnExtensions.Padding = new Padding(4, 0, 18, 0);
        btnExtensions.Size = new Size(191, 26);
        btnExtensions.TabIndex = 4;
        btnExtensions.Text = "(none)";
        btnExtensions.TextAlign = ContentAlignment.MiddleLeft;
        btnExtensions.UseVisualStyleBackColor = true;
        btnExtensions.Click += btnExtensions_Click;
        // 
        // lblGitBase
        // 
        lblGitBase.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblGitBase.Location = new Point(336, 38);
        lblGitBase.Name = "lblGitBase";
        lblGitBase.Size = new Size(75, 20);
        lblGitBase.TabIndex = 5;
        lblGitBase.Text = "Git Base:";
        lblGitBase.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblGitCompare
        // 
        lblGitCompare.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblGitCompare.Location = new Point(584, 38);
        lblGitCompare.Name = "lblGitCompare";
        lblGitCompare.Size = new Size(80, 20);
        lblGitCompare.TabIndex = 7;
        lblGitCompare.Text = "Git Compare:";
        lblGitCompare.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // pnlList
        // 
        pnlList.BorderStyle = BorderStyle.FixedSingle;
        pnlList.Controls.Add(btnAdd);
        pnlList.Controls.Add(btnDelete);
        pnlList.Controls.Add(btnMoveUp);
        pnlList.Controls.Add(btnMoveDown);
        pnlList.Controls.Add(clbConfigs);
        pnlList.Location = new Point(8, 80);
        pnlList.Name = "pnlList";
        pnlList.Size = new Size(380, 340);
        pnlList.TabIndex = 1;
        // 
        // btnAdd
        // 
        btnAdd.Location = new Point(8, 8);
        btnAdd.Name = "btnAdd";
        btnAdd.Size = new Size(70, 28);
        btnAdd.TabIndex = 0;
        btnAdd.Text = "+ Add";
        btnAdd.UseVisualStyleBackColor = true;
        btnAdd.Click += btnAdd_Click;
        // 
        // btnDelete
        // 
        btnDelete.Location = new Point(82, 8);
        btnDelete.Name = "btnDelete";
        btnDelete.Size = new Size(78, 28);
        btnDelete.TabIndex = 1;
        btnDelete.Text = "- Delete";
        btnDelete.UseVisualStyleBackColor = true;
        btnDelete.Click += btnDelete_Click;
        // 
        // btnMoveUp
        // 
        btnMoveUp.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnMoveUp.Location = new Point(180, 8);
        btnMoveUp.Name = "btnMoveUp";
        btnMoveUp.Size = new Size(40, 28);
        btnMoveUp.TabIndex = 2;
        btnMoveUp.Text = "↑";
        btnMoveUp.UseVisualStyleBackColor = true;
        btnMoveUp.Click += btnMoveUp_Click;
        // 
        // btnMoveDown
        // 
        btnMoveDown.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnMoveDown.Location = new Point(224, 8);
        btnMoveDown.Name = "btnMoveDown";
        btnMoveDown.Size = new Size(40, 28);
        btnMoveDown.TabIndex = 3;
        btnMoveDown.Text = "↓";
        btnMoveDown.UseVisualStyleBackColor = true;
        btnMoveDown.Click += btnMoveDown_Click;
        // 
        // clbConfigs
        // 
        clbConfigs.CheckOnClick = true;
        clbConfigs.Font = new Font("Segoe UI", 9.5F);
        clbConfigs.IntegralHeight = false;
        clbConfigs.Location = new Point(8, 44);
        clbConfigs.Name = "clbConfigs";
        clbConfigs.Size = new Size(360, 286);
        clbConfigs.TabIndex = 4;
        clbConfigs.ItemCheck += clbConfigs_ItemCheck;
        clbConfigs.SelectedIndexChanged += clbConfigs_SelectedIndexChanged;
        clbConfigs.MouseDown += clbConfigs_MouseDown;
        // 
        // pnlDetail
        // 
        pnlDetail.BorderStyle = BorderStyle.FixedSingle;
        pnlDetail.Controls.Add(lblName);
        pnlDetail.Controls.Add(txtName);
        pnlDetail.Controls.Add(lblSln);
        pnlDetail.Controls.Add(txtSln);
        pnlDetail.Controls.Add(btnBrowseSln);
        pnlDetail.Controls.Add(lblEngine);
        pnlDetail.Controls.Add(cboEngine);
        pnlDetail.Controls.Add(lblOutFolder);
        pnlDetail.Controls.Add(txtOutFolder);
        pnlDetail.Controls.Add(lblOutPreview);
        pnlDetail.Controls.Add(lblExtraFolders);
        pnlDetail.Controls.Add(txtExtraFolders);
        pnlDetail.Controls.Add(lblRedistribute);
        pnlDetail.Controls.Add(btnRedistribute);
        pnlDetail.Controls.Add(chkFilterGit);
        pnlDetail.Controls.Add(lblFilterInfo);
        pnlDetail.Controls.Add(btnSaveConfig);
        pnlDetail.Location = new Point(394, 80);
        pnlDetail.Name = "pnlDetail";
        pnlDetail.Size = new Size(478, 340);
        pnlDetail.TabIndex = 2;
        // 
        // lblName
        // 
        lblName.Location = new Point(8, 12);
        lblName.Name = "lblName";
        lblName.Size = new Size(95, 20);
        lblName.TabIndex = 0;
        lblName.Text = "Name:";
        lblName.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtName
        // 
        txtName.Location = new Point(108, 10);
        txtName.Name = "txtName";
        txtName.Size = new Size(360, 23);
        txtName.TabIndex = 1;
        txtName.TextChanged += txtName_TextChanged;
        txtName.Leave += txtName_Leave;
        // 
        // lblSln
        // 
        lblSln.Location = new Point(8, 42);
        lblSln.Name = "lblSln";
        lblSln.Size = new Size(95, 20);
        lblSln.TabIndex = 2;
        lblSln.Text = "Solution (.sln):";
        lblSln.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtSln
        // 
        txtSln.Location = new Point(108, 40);
        txtSln.Name = "txtSln";
        txtSln.Size = new Size(290, 23);
        txtSln.TabIndex = 3;
        txtSln.TextChanged += txtSln_TextChanged;
        // 
        // btnBrowseSln
        // 
        btnBrowseSln.Location = new Point(402, 38);
        btnBrowseSln.Name = "btnBrowseSln";
        btnBrowseSln.Size = new Size(66, 28);
        btnBrowseSln.TabIndex = 4;
        btnBrowseSln.Text = "Browse";
        btnBrowseSln.UseVisualStyleBackColor = true;
        btnBrowseSln.Click += btnBrowseSln_Click;
        // 
        // lblConfig
        // 
        lblConfig.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblConfig.Location = new Point(648, 6);
        lblConfig.Name = "lblConfig";
        lblConfig.Size = new Size(52, 20);
        lblConfig.TabIndex = 9;
        lblConfig.Text = "Config:";
        lblConfig.TextAlign = ContentAlignment.MiddleLeft;
        //
        // cboConfig
        //
        cboConfig.DropDownStyle = ComboBoxStyle.DropDownList;
        cboConfig.Location = new Point(702, 4);
        cboConfig.Name = "cboConfig";
        cboConfig.Size = new Size(150, 23);
        cboConfig.TabIndex = 10;
        cboConfig.SelectedIndexChanged += cboConfig_SelectedIndexChanged;
        // 
        // lblEngine
        // 
        lblEngine.Location = new Point(8, 72);
        lblEngine.Name = "lblEngine";
        lblEngine.Size = new Size(95, 20);
        lblEngine.TabIndex = 7;
        lblEngine.Text = "Engine:";
        lblEngine.TextAlign = ContentAlignment.MiddleLeft;
        //
        // cboEngine
        //
        cboEngine.DropDownStyle = ComboBoxStyle.DropDownList;
        cboEngine.Location = new Point(108, 70);
        cboEngine.Name = "cboEngine";
        cboEngine.Size = new Size(360, 23);
        cboEngine.TabIndex = 8;
        cboEngine.SelectedIndexChanged += cboEngine_SelectedIndexChanged;
        // 
        // lblOutFolder
        // 
        lblOutFolder.Location = new Point(8, 102);
        lblOutFolder.Name = "lblOutFolder";
        lblOutFolder.Size = new Size(95, 20);
        lblOutFolder.TabIndex = 9;
        lblOutFolder.Text = "Output Folder:";
        lblOutFolder.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtOutFolder
        // 
        txtOutFolder.Location = new Point(108, 100);
        txtOutFolder.Name = "txtOutFolder";
        txtOutFolder.Size = new Size(360, 23);
        txtOutFolder.TabIndex = 10;
        txtOutFolder.TextChanged += txtOutFolder_TextChanged;
        // 
        // lblOutPreview
        // 
        lblOutPreview.AutoEllipsis = true;
        lblOutPreview.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
        lblOutPreview.ForeColor = Color.DarkBlue;
        lblOutPreview.Location = new Point(108, 128);
        lblOutPreview.Name = "lblOutPreview";
        lblOutPreview.Size = new Size(360, 18);
        lblOutPreview.TabIndex = 11;
        // 
        // lblExtraFolders
        // 
        lblExtraFolders.Location = new Point(8, 158);
        lblExtraFolders.Name = "lblExtraFolders";
        lblExtraFolders.Size = new Size(95, 20);
        lblExtraFolders.TabIndex = 14;
        lblExtraFolders.Text = "Extra Folders:";
        lblExtraFolders.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtExtraFolders
        // 
        txtExtraFolders.Location = new Point(108, 156);
        txtExtraFolders.Name = "txtExtraFolders";
        txtExtraFolders.PlaceholderText = "e.g. Language, Templates, vi-VN";
        txtExtraFolders.Size = new Size(360, 23);
        txtExtraFolders.TabIndex = 15;
        txtExtraFolders.TextChanged += txtExtraFolders_TextChanged;
        // 
        // lblRedistribute
        // 
        lblRedistribute.Location = new Point(8, 188);
        lblRedistribute.Name = "lblRedistribute";
        lblRedistribute.Size = new Size(95, 20);
        lblRedistribute.TabIndex = 16;
        lblRedistribute.Text = "Copy To:";
        lblRedistribute.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // btnRedistribute
        // 
        btnRedistribute.Location = new Point(108, 186);
        btnRedistribute.Name = "btnRedistribute";
        btnRedistribute.Padding = new Padding(4, 0, 18, 0);
        btnRedistribute.Size = new Size(360, 26);
        btnRedistribute.TabIndex = 17;
        btnRedistribute.Text = "(none)";
        btnRedistribute.TextAlign = ContentAlignment.MiddleLeft;
        btnRedistribute.UseVisualStyleBackColor = true;
        btnRedistribute.Click += btnRedistribute_Click;
        // 
        // chkFilterGit
        // 
        chkFilterGit.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        chkFilterGit.Location = new Point(8, 218);
        chkFilterGit.Name = "chkFilterGit";
        chkFilterGit.Size = new Size(220, 24);
        chkFilterGit.TabIndex = 12;
        chkFilterGit.Text = "Filter by Git Diff";
        chkFilterGit.UseVisualStyleBackColor = true;
        chkFilterGit.CheckedChanged += chkFilterGit_CheckedChanged;
        // 
        // lblFilterInfo
        // 
        lblFilterInfo.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
        lblFilterInfo.ForeColor = Color.Gray;
        lblFilterInfo.Location = new Point(28, 246);
        lblFilterInfo.Name = "lblFilterInfo";
        lblFilterInfo.Size = new Size(440, 36);
        lblFilterInfo.TabIndex = 13;
        lblFilterInfo.Text = "Uses global Git Base/Compare above; repo auto-detected from solution path.";
        // 
        // btnSaveConfig
        // 
        btnSaveConfig.BackColor = Color.FromArgb(0, 120, 212);
        btnSaveConfig.FlatAppearance.BorderSize = 0;
        btnSaveConfig.FlatStyle = FlatStyle.Flat;
        btnSaveConfig.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSaveConfig.ForeColor = Color.White;
        btnSaveConfig.Location = new Point(108, 290);
        btnSaveConfig.Name = "btnSaveConfig";
        btnSaveConfig.Size = new Size(260, 36);
        btnSaveConfig.TabIndex = 14;
        btnSaveConfig.Text = "Save Config";
        btnSaveConfig.UseVisualStyleBackColor = false;
        btnSaveConfig.Click += btnSaveConfig_Click;
        // 
        // pnlActions
        // 
        pnlActions.Controls.Add(progressBar);
        pnlActions.Controls.Add(btnPreview);
        pnlActions.Controls.Add(btnBuildChecked);
        pnlActions.Controls.Add(btnBuildAll);
        pnlActions.Controls.Add(lblStatus);
        pnlActions.Location = new Point(8, 424);
        pnlActions.Name = "pnlActions";
        pnlActions.Size = new Size(864, 44);
        pnlActions.TabIndex = 3;
        // 
        // btnPreview
        // 
        btnPreview.Location = new Point(0, 6);
        btnPreview.Name = "btnPreview";
        btnPreview.Size = new Size(180, 32);
        btnPreview.TabIndex = 0;
        btnPreview.Text = "Preview Changes";
        btnPreview.UseVisualStyleBackColor = true;
        btnPreview.Click += btnPreview_Click;
        // 
        // btnBuildChecked
        // 
        btnBuildChecked.BackColor = Color.FromArgb(46, 125, 50);
        btnBuildChecked.FlatAppearance.BorderSize = 0;
        btnBuildChecked.FlatStyle = FlatStyle.Flat;
        btnBuildChecked.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnBuildChecked.ForeColor = Color.White;
        btnBuildChecked.Location = new Point(190, 6);
        btnBuildChecked.Name = "btnBuildChecked";
        btnBuildChecked.Size = new Size(190, 32);
        btnBuildChecked.TabIndex = 1;
        btnBuildChecked.Text = "Build Checked";
        btnBuildChecked.UseVisualStyleBackColor = false;
        btnBuildChecked.Click += btnBuildChecked_Click;
        // 
        // btnBuildAll
        // 
        btnBuildAll.BackColor = Color.FromArgb(25, 90, 35);
        btnBuildAll.FlatAppearance.BorderSize = 0;
        btnBuildAll.FlatStyle = FlatStyle.Flat;
        btnBuildAll.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnBuildAll.ForeColor = Color.White;
        btnBuildAll.Location = new Point(386, 6);
        btnBuildAll.Name = "btnBuildAll";
        btnBuildAll.Size = new Size(160, 32);
        btnBuildAll.TabIndex = 2;
        btnBuildAll.Text = "Build All";
        btnBuildAll.UseVisualStyleBackColor = false;
        btnBuildAll.Click += btnBuildAll_Click;
        // 
        // lblStatus
        // 
        lblStatus.AutoEllipsis = true;
        lblStatus.ForeColor = Color.DarkGray;
        lblStatus.Location = new Point(556, 14);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(199, 18);
        lblStatus.TabIndex = 3;
        // 
        // progressBar
        // 
        progressBar.Location = new Point(753, 10);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(111, 24);
        progressBar.TabIndex = 4;
        progressBar.Visible = false;
        // 
        // txtLog
        // 
        txtLog.BackColor = Color.FromArgb(30, 30, 30);
        txtLog.Font = new Font("Consolas", 9F);
        txtLog.ForeColor = Color.LightGray;
        txtLog.Location = new Point(8, 472);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Both;
        txtLog.Size = new Size(864, 218);
        txtLog.TabIndex = 4;
        txtLog.WordWrap = false;
        // 
        // BuildReleaseForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(pnlGlobal);
        Controls.Add(pnlList);
        Controls.Add(pnlDetail);
        Controls.Add(pnlActions);
        Controls.Add(txtLog);
        Name = "BuildReleaseForm";
        Size = new Size(880, 696);
        pnlGlobal.ResumeLayout(false);
        pnlGlobal.PerformLayout();
        pnlList.ResumeLayout(false);
        pnlDetail.ResumeLayout(false);
        pnlDetail.PerformLayout();
        pnlActions.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}
