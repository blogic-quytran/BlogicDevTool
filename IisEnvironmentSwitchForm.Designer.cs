namespace BLogicDevTool;

partial class IisEnvironmentSwitchForm
{
    private System.ComponentModel.IContainer components = null;

    // Header
    private System.Windows.Forms.Label lblSql;
    private System.Windows.Forms.Button btnReload;
    private System.Windows.Forms.Button btnLoginDb;
    private System.Windows.Forms.Label lblHint;

    // Grid
    private System.Windows.Forms.DataGridView dgvTargets;
    private System.Windows.Forms.DataGridViewTextBoxColumn colSite;
    private System.Windows.Forms.DataGridViewTextBoxColumn colApp;
    private System.Windows.Forms.DataGridViewTextBoxColumn colEntry;
    private System.Windows.Forms.DataGridViewTextBoxColumn colCurServer;
    private System.Windows.Forms.DataGridViewTextBoxColumn colCurDb;
    private System.Windows.Forms.DataGridViewComboBoxColumn colNewDb;
    private System.Windows.Forms.DataGridViewTextBoxColumn colOrigDb;

    // Advanced
    private System.Windows.Forms.CheckBox chkSynonym;
    private System.Windows.Forms.Label lblAlias;
    private System.Windows.Forms.TextBox txtAliasDb;
    private System.Windows.Forms.Label lblReal;
    private System.Windows.Forms.ComboBox cboRealDb;

    // Action row
    private System.Windows.Forms.Button btnApply;
    private System.Windows.Forms.Button btnReset;
    private System.Windows.Forms.Button btnClearNew;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.ProgressBar progressBar;
    private System.Windows.Forms.RichTextBox rtbLog;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _log?.Dispose();
            _boldFont?.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblSql = new Label();
        btnReload = new Button();
        btnLoginDb = new Button();
        lblHint = new Label();
        dgvTargets = new DataGridView();
        colSite = new DataGridViewTextBoxColumn();
        colApp = new DataGridViewTextBoxColumn();
        colEntry = new DataGridViewTextBoxColumn();
        colCurServer = new DataGridViewTextBoxColumn();
        colCurDb = new DataGridViewTextBoxColumn();
        colNewDb = new DataGridViewComboBoxColumn();
        colOrigDb = new DataGridViewTextBoxColumn();
        chkSynonym = new CheckBox();
        lblAlias = new Label();
        txtAliasDb = new TextBox();
        lblReal = new Label();
        cboRealDb = new ComboBox();
        btnApply = new Button();
        btnReset = new Button();
        btnClearNew = new Button();
        btnClose = new Button();
        lblStatus = new Label();
        progressBar = new ProgressBar();
        rtbLog = new RichTextBox();
        ((System.ComponentModel.ISupportInitialize)dgvTargets).BeginInit();
        SuspendLayout();
        //
        // lblSql
        //
        lblSql.AutoSize = true;
        lblSql.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblSql.Location = new Point(12, 17);
        lblSql.Name = "lblSql";
        lblSql.Size = new Size(160, 17);
        lblSql.TabIndex = 0;
        lblSql.Text = "SQL: (not logged in)";
        //
        // btnReload
        //
        btnReload.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnReload.Location = new Point(720, 12);
        btnReload.Name = "btnReload";
        btnReload.Size = new Size(120, 28);
        btnReload.TabIndex = 1;
        btnReload.Text = "⟳  Reload IIS";
        btnReload.UseVisualStyleBackColor = true;
        btnReload.Click += btnReload_Click;
        //
        // btnLoginDb
        //
        btnLoginDb.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnLoginDb.Location = new Point(848, 12);
        btnLoginDb.Name = "btnLoginDb";
        btnLoginDb.Size = new Size(120, 28);
        btnLoginDb.TabIndex = 2;
        btnLoginDb.Text = "🔑  SQL Login";
        btnLoginDb.UseVisualStyleBackColor = true;
        btnLoginDb.Click += btnLoginDb_Click;
        //
        // lblHint
        //
        lblHint.AutoSize = true;
        lblHint.ForeColor = Color.DimGray;
        lblHint.Location = new Point(12, 48);
        lblHint.Name = "lblHint";
        lblHint.Size = new Size(700, 15);
        lblHint.TabIndex = 3;
        lblHint.Text = "① Pick a New DB for each service to switch (blank = keep)   ② Apply — configs are backed up, IIS + BLogic services restart   ③ Reset to default puts them back.";
        //
        // dgvTargets
        //
        dgvTargets.AllowUserToAddRows = false;
        dgvTargets.AllowUserToDeleteRows = false;
        dgvTargets.AllowUserToResizeRows = false;
        dgvTargets.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dgvTargets.BackgroundColor = SystemColors.Window;
        dgvTargets.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvTargets.Columns.AddRange(colSite, colApp, colEntry, colCurServer, colCurDb, colNewDb, colOrigDb);
        dgvTargets.EditMode = DataGridViewEditMode.EditOnEnter;
        dgvTargets.Location = new Point(12, 72);
        dgvTargets.MultiSelect = false;
        dgvTargets.Name = "dgvTargets";
        dgvTargets.RowHeadersVisible = false;
        dgvTargets.RowTemplate.Height = 28;
        dgvTargets.SelectionMode = DataGridViewSelectionMode.CellSelect;
        dgvTargets.Size = new Size(956, 250);
        dgvTargets.TabIndex = 4;
        //
        // colSite
        //
        colSite.HeaderText = "Host";
        colSite.Name = "colSite";
        colSite.ReadOnly = true;
        colSite.Width = 110;
        //
        // colApp
        //
        colApp.HeaderText = "Service / App";
        colApp.Name = "colApp";
        colApp.ReadOnly = true;
        colApp.Width = 180;
        //
        // colEntry
        //
        colEntry.HeaderText = "Connection";
        colEntry.Name = "colEntry";
        colEntry.ReadOnly = true;
        colEntry.Width = 150;
        //
        // colCurServer
        //
        colCurServer.HeaderText = "Server";
        colCurServer.Name = "colCurServer";
        colCurServer.ReadOnly = true;
        colCurServer.Width = 80;
        //
        // colCurDb
        //
        colCurDb.HeaderText = "Current DB";
        colCurDb.Name = "colCurDb";
        colCurDb.ReadOnly = true;
        colCurDb.Width = 150;
        //
        // colNewDb
        //
        colNewDb.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        colNewDb.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
        colNewDb.FlatStyle = FlatStyle.Flat;
        colNewDb.HeaderText = "New DB  ▼";
        colNewDb.Name = "colNewDb";
        //
        // colOrigDb
        //
        colOrigDb.HeaderText = "Original DB";
        colOrigDb.Name = "colOrigDb";
        colOrigDb.ReadOnly = true;
        colOrigDb.Width = 140;
        //
        // chkSynonym
        //
        chkSynonym.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        chkSynonym.AutoSize = true;
        chkSynonym.Location = new Point(14, 334);
        chkSynonym.Name = "chkSynonym";
        chkSynonym.Size = new Size(270, 19);
        chkSynonym.TabIndex = 5;
        chkSynonym.Text = "Advanced: also create Merchant synonym DB";
        chkSynonym.UseVisualStyleBackColor = true;
        chkSynonym.CheckedChanged += chkSynonym_CheckedChanged;
        //
        // lblAlias
        //
        lblAlias.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblAlias.AutoSize = true;
        lblAlias.Location = new Point(300, 336);
        lblAlias.Name = "lblAlias";
        lblAlias.Size = new Size(36, 15);
        lblAlias.TabIndex = 6;
        lblAlias.Text = "Alias:";
        //
        // txtAliasDb
        //
        txtAliasDb.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        txtAliasDb.Font = new Font("Consolas", 9.5F);
        txtAliasDb.Location = new Point(342, 332);
        txtAliasDb.Name = "txtAliasDb";
        txtAliasDb.Size = new Size(150, 23);
        txtAliasDb.TabIndex = 7;
        txtAliasDb.Text = "Merchant";
        //
        // lblReal
        //
        lblReal.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblReal.AutoSize = true;
        lblReal.Location = new Point(500, 336);
        lblReal.Name = "lblReal";
        lblReal.Size = new Size(84, 15);
        lblReal.TabIndex = 8;
        lblReal.Text = "→  forwards to:";
        //
        // cboRealDb
        //
        cboRealDb.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        cboRealDb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        cboRealDb.AutoCompleteSource = AutoCompleteSource.ListItems;
        cboRealDb.Font = new Font("Consolas", 9.5F);
        cboRealDb.FormattingEnabled = true;
        cboRealDb.Location = new Point(590, 332);
        cboRealDb.Name = "cboRealDb";
        cboRealDb.Size = new Size(200, 23);
        cboRealDb.TabIndex = 9;
        //
        // btnApply
        //
        btnApply.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnApply.BackColor = Color.FromArgb(0, 120, 212);
        btnApply.FlatStyle = FlatStyle.Flat;
        btnApply.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnApply.ForeColor = Color.White;
        btnApply.Location = new Point(12, 368);
        btnApply.Name = "btnApply";
        btnApply.Size = new Size(220, 36);
        btnApply.TabIndex = 10;
        btnApply.Text = "✔  Apply && Restart IIS";
        btnApply.UseVisualStyleBackColor = false;
        btnApply.Click += btnApply_Click;
        //
        // btnReset
        //
        btnReset.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnReset.BackColor = Color.FromArgb(214, 120, 40);
        btnReset.FlatStyle = FlatStyle.Flat;
        btnReset.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnReset.ForeColor = Color.White;
        btnReset.Location = new Point(240, 368);
        btnReset.Name = "btnReset";
        btnReset.Size = new Size(210, 36);
        btnReset.TabIndex = 11;
        btnReset.Text = "↺  Reset to default";
        btnReset.UseVisualStyleBackColor = false;
        btnReset.Click += btnReset_Click;
        //
        // btnClearNew
        //
        btnClearNew.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnClearNew.Location = new Point(458, 368);
        btnClearNew.Name = "btnClearNew";
        btnClearNew.Size = new Size(110, 36);
        btnClearNew.TabIndex = 12;
        btnClearNew.Text = "Clear choices";
        btnClearNew.UseVisualStyleBackColor = true;
        btnClearNew.Click += btnClearNew_Click;
        //
        // btnClose
        //
        btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnClose.Location = new Point(868, 368);
        btnClose.Name = "btnClose";
        btnClose.Size = new Size(100, 36);
        btnClose.TabIndex = 13;
        btnClose.Text = "Close";
        btnClose.UseVisualStyleBackColor = true;
        btnClose.Click += btnClose_Click;
        //
        // lblStatus
        //
        lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.ForeColor = Color.DarkGray;
        lblStatus.Location = new Point(578, 378);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(282, 20);
        lblStatus.TabIndex = 14;
        lblStatus.Text = "";
        //
        // progressBar
        //
        progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        progressBar.Location = new Point(12, 410);
        progressBar.MarqueeAnimationSpeed = 30;
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(956, 6);
        progressBar.Style = ProgressBarStyle.Marquee;
        progressBar.TabIndex = 15;
        progressBar.Visible = false;
        //
        // rtbLog
        //
        rtbLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        rtbLog.BackColor = Color.FromArgb(30, 30, 30);
        rtbLog.BorderStyle = BorderStyle.FixedSingle;
        rtbLog.Font = new Font("Consolas", 9F);
        rtbLog.ForeColor = Color.Gainsboro;
        rtbLog.Location = new Point(12, 420);
        rtbLog.Name = "rtbLog";
        rtbLog.ReadOnly = true;
        rtbLog.Size = new Size(956, 188);
        rtbLog.TabIndex = 16;
        rtbLog.Text = "";
        rtbLog.WordWrap = false;
        //
        // IisEnvironmentSwitchForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(980, 620);
        Controls.Add(lblSql);
        Controls.Add(btnReload);
        Controls.Add(btnLoginDb);
        Controls.Add(lblHint);
        Controls.Add(dgvTargets);
        Controls.Add(chkSynonym);
        Controls.Add(lblAlias);
        Controls.Add(txtAliasDb);
        Controls.Add(lblReal);
        Controls.Add(cboRealDb);
        Controls.Add(btnApply);
        Controls.Add(btnReset);
        Controls.Add(btnClearNew);
        Controls.Add(btnClose);
        Controls.Add(lblStatus);
        Controls.Add(progressBar);
        Controls.Add(rtbLog);
        MinimumSize = new Size(996, 600);
        Name = "IisEnvironmentSwitchForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Switch Environment — IIS databases";
        ((System.ComponentModel.ISupportInitialize)dgvTargets).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
