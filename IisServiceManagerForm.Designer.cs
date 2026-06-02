namespace BLogicDevTool;

partial class IisServiceManagerForm
{
    private System.ComponentModel.IContainer components = null;

    // Toolbar row
    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.Button btnTroubleshoot;

    // Top grid
    private System.Windows.Forms.DataGridView dgvApps;

    // Detail panel
    private System.Windows.Forms.GroupBox grpDetail;
    private System.Windows.Forms.Label lblDetailSiteCaption;
    private System.Windows.Forms.Label lblDetailSite;
    private System.Windows.Forms.Label lblCurrentPathCaption;
    private System.Windows.Forms.TextBox txtDetailPath;
    private System.Windows.Forms.Label lblCurrentDbCaption;
    private System.Windows.Forms.TextBox txtDetailDb;
    private System.Windows.Forms.Label lblSnapshotCaption;
    private System.Windows.Forms.Label lblSnapshot;

    // Change section
    private System.Windows.Forms.GroupBox grpChange;
    private System.Windows.Forms.Label lblNewDb;
    private System.Windows.Forms.ComboBox txtNewDb;
    private System.Windows.Forms.Button btnLoginDb;
    private System.Windows.Forms.Label lblNewPath;
    private System.Windows.Forms.TextBox txtNewPath;
    private System.Windows.Forms.Button btnBrowseNewPath;

    // Action buttons
    private System.Windows.Forms.Button btnChange;
    private System.Windows.Forms.Button btnResetDb;
    private System.Windows.Forms.Button btnChangePath;
    private System.Windows.Forms.Button btnResetPath;
    private System.Windows.Forms.Button btnRestorePatch;
    private System.Windows.Forms.Button btnRestartIis;

    // Status
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.ProgressBar progressBar;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        btnRefresh = new Button();
        btnTroubleshoot = new Button();
        dgvApps = new DataGridView();
        grpDetail = new GroupBox();
        lblDetailSiteCaption = new Label();
        lblDetailSite = new Label();
        lblCurrentPathCaption = new Label();
        txtDetailPath = new TextBox();
        lblCurrentDbCaption = new Label();
        txtDetailDb = new TextBox();
        lblSnapshotCaption = new Label();
        lblSnapshot = new Label();
        grpChange = new GroupBox();
        lblNewDb = new Label();
        txtNewDb = new ComboBox();
        btnLoginDb = new Button();
        lblNewPath = new Label();
        txtNewPath = new TextBox();
        btnBrowseNewPath = new Button();
        btnChange = new Button();
        btnResetDb = new Button();
        btnChangePath = new Button();
        btnResetPath = new Button();
        btnRestorePatch = new Button();
        btnRestartIis = new Button();
        lblStatus = new Label();
        progressBar = new ProgressBar();
        ((System.ComponentModel.ISupportInitialize)dgvApps).BeginInit();
        grpDetail.SuspendLayout();
        grpChange.SuspendLayout();
        SuspendLayout();
        // 
        // btnRefresh
        // 
        btnRefresh.BackColor = Color.FromArgb(0, 120, 212);
        btnRefresh.FlatStyle = FlatStyle.Flat;
        btnRefresh.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnRefresh.ForeColor = Color.White;
        btnRefresh.Location = new Point(10, 10);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(170, 30);
        btnRefresh.TabIndex = 0;
        btnRefresh.Text = "⟳  Refresh IIS List";
        btnRefresh.UseVisualStyleBackColor = false;
        btnRefresh.Click += btnRefresh_Click;
        // 
        // btnTroubleshoot
        // 
        btnTroubleshoot.BackColor = Color.FromArgb(120, 60, 140);
        btnTroubleshoot.FlatStyle = FlatStyle.Flat;
        btnTroubleshoot.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnTroubleshoot.ForeColor = Color.White;
        btnTroubleshoot.Location = new Point(190, 10);
        btnTroubleshoot.Name = "btnTroubleshoot";
        btnTroubleshoot.Size = new Size(195, 30);
        btnTroubleshoot.TabIndex = 1;
        btnTroubleshoot.Text = "🔧  Troubleshoot IIS";
        btnTroubleshoot.UseVisualStyleBackColor = false;
        btnTroubleshoot.Click += btnTroubleshoot_Click;
        // 
        // dgvApps
        // 
        dgvApps.AllowUserToAddRows = false;
        dgvApps.AllowUserToDeleteRows = false;
        dgvApps.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        dgvApps.BackgroundColor = SystemColors.Window;
        dgvApps.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvApps.Location = new Point(10, 48);
        dgvApps.MultiSelect = false;
        dgvApps.Name = "dgvApps";
        dgvApps.ReadOnly = true;
        dgvApps.RowHeadersVisible = false;
        dgvApps.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvApps.Size = new Size(845, 200);
        dgvApps.TabIndex = 1;
        dgvApps.SelectionChanged += dgvApps_SelectionChanged;
        // 
        // grpDetail
        // 
        grpDetail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpDetail.Controls.Add(lblDetailSiteCaption);
        grpDetail.Controls.Add(lblDetailSite);
        grpDetail.Controls.Add(lblCurrentPathCaption);
        grpDetail.Controls.Add(txtDetailPath);
        grpDetail.Controls.Add(lblCurrentDbCaption);
        grpDetail.Controls.Add(txtDetailDb);
        grpDetail.Controls.Add(lblSnapshotCaption);
        grpDetail.Controls.Add(lblSnapshot);
        grpDetail.Location = new Point(10, 258);
        grpDetail.Name = "grpDetail";
        grpDetail.Size = new Size(845, 140);
        grpDetail.TabIndex = 2;
        grpDetail.TabStop = false;
        grpDetail.Text = "Current Info";
        // 
        // lblDetailSiteCaption
        // 
        lblDetailSiteCaption.AutoSize = true;
        lblDetailSiteCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblDetailSiteCaption.Location = new Point(10, 28);
        lblDetailSiteCaption.Name = "lblDetailSiteCaption";
        lblDetailSiteCaption.Size = new Size(65, 15);
        lblDetailSiteCaption.TabIndex = 0;
        lblDetailSiteCaption.Text = "Site / App:";
        // 
        // lblDetailSite
        // 
        lblDetailSite.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblDetailSite.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblDetailSite.ForeColor = Color.DarkBlue;
        lblDetailSite.Location = new Point(110, 28);
        lblDetailSite.Name = "lblDetailSite";
        lblDetailSite.Size = new Size(725, 20);
        lblDetailSite.TabIndex = 1;
        // 
        // lblCurrentPathCaption
        // 
        lblCurrentPathCaption.AutoSize = true;
        lblCurrentPathCaption.Location = new Point(10, 60);
        lblCurrentPathCaption.Name = "lblCurrentPathCaption";
        lblCurrentPathCaption.Size = new Size(80, 15);
        lblCurrentPathCaption.TabIndex = 2;
        lblCurrentPathCaption.Text = "Physical Path:";
        // 
        // txtDetailPath
        // 
        txtDetailPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtDetailPath.BackColor = SystemColors.Control;
        txtDetailPath.BorderStyle = BorderStyle.None;
        txtDetailPath.Location = new Point(110, 58);
        txtDetailPath.Name = "txtDetailPath";
        txtDetailPath.ReadOnly = true;
        txtDetailPath.Size = new Size(725, 16);
        txtDetailPath.TabIndex = 3;
        // 
        // lblCurrentDbCaption
        // 
        lblCurrentDbCaption.AutoSize = true;
        lblCurrentDbCaption.Location = new Point(10, 90);
        lblCurrentDbCaption.Name = "lblCurrentDbCaption";
        lblCurrentDbCaption.Size = new Size(101, 15);
        lblCurrentDbCaption.TabIndex = 4;
        lblCurrentDbCaption.Text = "Current Database:";
        // 
        // txtDetailDb
        // 
        txtDetailDb.BackColor = SystemColors.Control;
        txtDetailDb.BorderStyle = BorderStyle.None;
        txtDetailDb.Font = new Font("Consolas", 9.5F);
        txtDetailDb.ForeColor = Color.DarkGreen;
        txtDetailDb.Location = new Point(130, 88);
        txtDetailDb.Name = "txtDetailDb";
        txtDetailDb.ReadOnly = true;
        txtDetailDb.Size = new Size(500, 15);
        txtDetailDb.TabIndex = 5;
        // 
        // lblSnapshotCaption
        // 
        lblSnapshotCaption.AutoSize = true;
        lblSnapshotCaption.Location = new Point(10, 116);
        lblSnapshotCaption.Name = "lblSnapshotCaption";
        lblSnapshotCaption.Size = new Size(59, 15);
        lblSnapshotCaption.TabIndex = 6;
        lblSnapshotCaption.Text = "Snapshot:";
        // 
        // lblSnapshot
        // 
        lblSnapshot.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblSnapshot.ForeColor = Color.DarkGray;
        lblSnapshot.Location = new Point(80, 116);
        lblSnapshot.Name = "lblSnapshot";
        lblSnapshot.Size = new Size(755, 18);
        lblSnapshot.TabIndex = 7;
        lblSnapshot.Text = "No snapshot available.";
        // 
        // grpChange
        // 
        grpChange.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpChange.Controls.Add(lblNewDb);
        grpChange.Controls.Add(txtNewDb);
        grpChange.Controls.Add(btnLoginDb);
        grpChange.Controls.Add(lblNewPath);
        grpChange.Controls.Add(txtNewPath);
        grpChange.Controls.Add(btnBrowseNewPath);
        grpChange.Location = new Point(10, 408);
        grpChange.Name = "grpChange";
        grpChange.Size = new Size(845, 100);
        grpChange.TabIndex = 3;
        grpChange.TabStop = false;
        grpChange.Text = "Update / Restore";
        // 
        // lblNewDb
        // 
        lblNewDb.AutoSize = true;
        lblNewDb.Location = new Point(10, 32);
        lblNewDb.Name = "lblNewDb";
        lblNewDb.Size = new Size(109, 15);
        lblNewDb.TabIndex = 0;
        lblNewDb.Text = "New SQL DB name:";
        // 
        // txtNewDb
        // 
        txtNewDb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        txtNewDb.AutoCompleteSource = AutoCompleteSource.ListItems;
        txtNewDb.Font = new Font("Consolas", 9.5F);
        txtNewDb.FormattingEnabled = true;
        txtNewDb.Location = new Point(130, 29);
        txtNewDb.Name = "txtNewDb";
        txtNewDb.Size = new Size(605, 23);
        txtNewDb.TabIndex = 1;
        // 
        // btnLoginDb
        // 
        btnLoginDb.Font = new Font("Segoe UI", 9F);
        btnLoginDb.Location = new Point(745, 26);
        btnLoginDb.Name = "btnLoginDb";
        btnLoginDb.Size = new Size(90, 26);
        btnLoginDb.TabIndex = 2;
        btnLoginDb.Text = "🔑 Login";
        btnLoginDb.UseVisualStyleBackColor = true;
        btnLoginDb.Click += btnLoginDb_Click;
        // 
        // lblNewPath
        // 
        lblNewPath.AutoSize = true;
        lblNewPath.Location = new Point(10, 68);
        lblNewPath.Name = "lblNewPath";
        lblNewPath.Size = new Size(61, 15);
        lblNewPath.TabIndex = 2;
        lblNewPath.Text = "New path:";
        // 
        // txtNewPath
        // 
        txtNewPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtNewPath.Location = new Point(130, 65);
        txtNewPath.Name = "txtNewPath";
        txtNewPath.Size = new Size(605, 23);
        txtNewPath.TabIndex = 3;
        // 
        // btnBrowseNewPath
        // 
        btnBrowseNewPath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowseNewPath.Location = new Point(745, 63);
        btnBrowseNewPath.Name = "btnBrowseNewPath";
        btnBrowseNewPath.Size = new Size(90, 27);
        btnBrowseNewPath.TabIndex = 4;
        btnBrowseNewPath.Text = "Browse...";
        btnBrowseNewPath.Click += btnBrowseNewPath_Click;
        // 
        // btnChange
        // 
        btnChange.BackColor = Color.FromArgb(0, 120, 212);
        btnChange.FlatStyle = FlatStyle.Flat;
        btnChange.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnChange.ForeColor = Color.White;
        btnChange.Location = new Point(10, 522);
        btnChange.Name = "btnChange";
        btnChange.Size = new Size(150, 36);
        btnChange.TabIndex = 4;
        btnChange.Text = "✔ Update DB";
        btnChange.UseVisualStyleBackColor = false;
        btnChange.Click += btnChange_Click;
        // 
        // btnResetDb
        // 
        btnResetDb.BackColor = Color.FromArgb(200, 80, 50);
        btnResetDb.FlatStyle = FlatStyle.Flat;
        btnResetDb.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnResetDb.ForeColor = Color.White;
        btnResetDb.Location = new Point(563, 522);
        btnResetDb.Name = "btnResetDb";
        btnResetDb.Size = new Size(136, 36);
        btnResetDb.TabIndex = 5;
        btnResetDb.Text = "↺  Restore DB";
        btnResetDb.UseVisualStyleBackColor = false;
        btnResetDb.Visible = false;
        btnResetDb.Click += btnResetDb_Click;
        // 
        // btnChangePath
        // 
        btnChangePath.BackColor = Color.FromArgb(60, 140, 60);
        btnChangePath.FlatStyle = FlatStyle.Flat;
        btnChangePath.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnChangePath.ForeColor = Color.White;
        btnChangePath.Location = new Point(166, 522);
        btnChangePath.Name = "btnChangePath";
        btnChangePath.Size = new Size(150, 36);
        btnChangePath.TabIndex = 6;
        btnChangePath.Text = "📁  Update Patch";
        btnChangePath.UseVisualStyleBackColor = false;
        btnChangePath.Click += btnChangePath_Click;
        //
        // btnResetPath
        //
        btnResetPath.BackColor = Color.FromArgb(200, 120, 40);
        btnResetPath.FlatStyle = FlatStyle.Flat;
        btnResetPath.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnResetPath.ForeColor = Color.White;
        btnResetPath.Location = new Point(468, 522);
        btnResetPath.Name = "btnResetPath";
        btnResetPath.Size = new Size(150, 36);
        btnResetPath.TabIndex = 9;
        btnResetPath.Text = "↺  Reset Path";
        btnResetPath.UseVisualStyleBackColor = false;
        btnResetPath.Click += btnResetPath_Click;
        //
        // btnRestorePatch
        //
        btnRestorePatch.BackColor = Color.FromArgb(90, 90, 90);
        btnRestorePatch.FlatStyle = FlatStyle.Flat;
        btnRestorePatch.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnRestorePatch.ForeColor = Color.White;
        btnRestorePatch.Location = new Point(705, 522);
        btnRestorePatch.Name = "btnRestorePatch";
        btnRestorePatch.Size = new Size(140, 36);
        btnRestorePatch.TabIndex = 7;
        btnRestorePatch.Text = "↺  Restore Patch";
        btnRestorePatch.UseVisualStyleBackColor = false;
        btnRestorePatch.Visible = false;
        btnRestorePatch.Click += btnRestorePatch_Click;
        // 
        // btnRestartIis
        // 
        btnRestartIis.BackColor = Color.FromArgb(120, 90, 30);
        btnRestartIis.FlatStyle = FlatStyle.Flat;
        btnRestartIis.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnRestartIis.ForeColor = Color.White;
        btnRestartIis.Location = new Point(322, 522);
        btnRestartIis.Name = "btnRestartIis";
        btnRestartIis.Size = new Size(140, 36);
        btnRestartIis.TabIndex = 8;
        btnRestartIis.Text = "⟳  Restart IIS";
        btnRestartIis.UseVisualStyleBackColor = false;
        btnRestartIis.Click += btnRestartIis_Click;
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.ForeColor = Color.DarkGray;
        lblStatus.Location = new Point(10, 582);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(845, 22);
        lblStatus.TabIndex = 8;
        lblStatus.Text = "Press 'Refresh IIS List' to start.";
        // 
        // progressBar
        // 
        progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        progressBar.Location = new Point(10, 570);
        progressBar.MarqueeAnimationSpeed = 30;
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(845, 6);
        progressBar.Style = ProgressBarStyle.Marquee;
        progressBar.TabIndex = 7;
        progressBar.Visible = false;
        // 
        // IisServiceManagerForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(btnRefresh);
        Controls.Add(btnTroubleshoot);
        Controls.Add(dgvApps);
        Controls.Add(grpDetail);
        Controls.Add(grpChange);
        Controls.Add(btnChange);
        Controls.Add(btnResetDb);
        Controls.Add(btnChangePath);
        Controls.Add(btnResetPath);
        Controls.Add(btnRestorePatch);
        Controls.Add(btnRestartIis);
        Controls.Add(progressBar);
        Controls.Add(lblStatus);
        Name = "IisServiceManagerForm";
        Size = new Size(865, 695);
        ((System.ComponentModel.ISupportInitialize)dgvApps).EndInit();
        grpDetail.ResumeLayout(false);
        grpDetail.PerformLayout();
        grpChange.ResumeLayout(false);
        grpChange.PerformLayout();
        ResumeLayout(false);
    }
}
