namespace BLogicDevTool;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Panel pnlNav;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Button btnNavDatabase;
    private System.Windows.Forms.Button btnNavIis;
    private System.Windows.Forms.Button btnNavUnzip;
    private System.Windows.Forms.Button btnNavSqlManager;
    private System.Windows.Forms.Button btnNavBuild;
    private System.Windows.Forms.Panel pnlMain;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
        pnlNav = new Panel();
        btnCloseForm = new Button();
        btnNavBuild = new Button();
        btnNavSqlManager = new Button();
        btnNavUnzip = new Button();
        btnNavIis = new Button();
        btnNavDatabase = new Button();
        lblTitle = new Label();
        pnlMain = new Panel();
        panel1 = new Panel();
        pnlNav.SuspendLayout();
        panel1.SuspendLayout();
        SuspendLayout();
        // 
        // pnlNav
        // 
        pnlNav.BackColor = Color.FromArgb(25, 25, 40);
        pnlNav.Controls.Add(btnCloseForm);
        pnlNav.Controls.Add(btnNavBuild);
        pnlNav.Controls.Add(btnNavSqlManager);
        pnlNav.Controls.Add(btnNavUnzip);
        pnlNav.Controls.Add(btnNavIis);
        pnlNav.Controls.Add(btnNavDatabase);
        pnlNav.Controls.Add(lblTitle);
        pnlNav.Dock = DockStyle.Top;
        pnlNav.Location = new Point(0, 0);
        pnlNav.Name = "pnlNav";
        pnlNav.Size = new Size(900, 36);
        pnlNav.TabIndex = 0;
        // 
        // btnCloseForm
        // 
        btnCloseForm.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCloseForm.BackColor = Color.Transparent;
        btnCloseForm.FlatAppearance.BorderSize = 0;
        btnCloseForm.FlatStyle = FlatStyle.Flat;
        btnCloseForm.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnCloseForm.ForeColor = Color.Silver;
        btnCloseForm.Location = new Point(865, 0);
        btnCloseForm.Name = "btnCloseForm";
        btnCloseForm.Size = new Size(35, 36);
        btnCloseForm.TabIndex = 4;
        btnCloseForm.Text = "✕";
        btnCloseForm.UseVisualStyleBackColor = false;
        btnCloseForm.Click += btnCloseForm_Click;
        // 
        // btnNavBuild
        // 
        btnNavBuild.BackColor = Color.Transparent;
        btnNavBuild.FlatAppearance.BorderSize = 0;
        btnNavBuild.FlatStyle = FlatStyle.Flat;
        btnNavBuild.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnNavBuild.ForeColor = Color.Silver;
        btnNavBuild.Location = new Point(741, 0);
        btnNavBuild.Name = "btnNavBuild";
        btnNavBuild.Size = new Size(120, 36);
        btnNavBuild.TabIndex = 6;
        btnNavBuild.Text = "🔨 Build";
        btnNavBuild.UseVisualStyleBackColor = false;
        btnNavBuild.Click += menuItemBuildRelease_Click;
        // 
        // btnNavSqlManager
        // 
        btnNavSqlManager.BackColor = Color.Transparent;
        btnNavSqlManager.FlatAppearance.BorderSize = 0;
        btnNavSqlManager.FlatStyle = FlatStyle.Flat;
        btnNavSqlManager.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnNavSqlManager.ForeColor = Color.Silver;
        btnNavSqlManager.Location = new Point(586, 0);
        btnNavSqlManager.Name = "btnNavSqlManager";
        btnNavSqlManager.Size = new Size(155, 36);
        btnNavSqlManager.TabIndex = 5;
        btnNavSqlManager.Text = "🗄 SQL Manager";
        btnNavSqlManager.UseVisualStyleBackColor = false;
        btnNavSqlManager.Click += menuItemSqlManager_Click;
        // 
        // btnNavUnzip
        // 
        btnNavUnzip.BackColor = Color.Transparent;
        btnNavUnzip.FlatAppearance.BorderSize = 0;
        btnNavUnzip.FlatStyle = FlatStyle.Flat;
        btnNavUnzip.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnNavUnzip.ForeColor = Color.Silver;
        btnNavUnzip.Location = new Point(491, 0);
        btnNavUnzip.Name = "btnNavUnzip";
        btnNavUnzip.Size = new Size(90, 36);
        btnNavUnzip.TabIndex = 3;
        btnNavUnzip.Text = "📂 Unzip";
        btnNavUnzip.UseVisualStyleBackColor = false;
        btnNavUnzip.Click += menuItemUnzip_Click;
        // 
        // btnNavIis
        // 
        btnNavIis.BackColor = Color.Transparent;
        btnNavIis.FlatAppearance.BorderSize = 0;
        btnNavIis.FlatStyle = FlatStyle.Flat;
        btnNavIis.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnNavIis.ForeColor = Color.Silver;
        btnNavIis.Location = new Point(351, 0);
        btnNavIis.Name = "btnNavIis";
        btnNavIis.Size = new Size(135, 36);
        btnNavIis.TabIndex = 2;
        btnNavIis.Text = "🌐 IIS Manager";
        btnNavIis.UseVisualStyleBackColor = false;
        btnNavIis.Click += menuItemIisManager_Click;
        // 
        // btnNavDatabase
        // 
        btnNavDatabase.BackColor = Color.Transparent;
        btnNavDatabase.FlatAppearance.BorderSize = 0;
        btnNavDatabase.FlatStyle = FlatStyle.Flat;
        btnNavDatabase.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnNavDatabase.ForeColor = Color.Silver;
        btnNavDatabase.Location = new Point(168, 0);
        btnNavDatabase.Name = "btnNavDatabase";
        btnNavDatabase.Size = new Size(178, 36);
        btnNavDatabase.TabIndex = 1;
        btnNavDatabase.Text = "💾 Backup / Restore DB";
        btnNavDatabase.UseVisualStyleBackColor = false;
        btnNavDatabase.Click += menuItemDatabaseTools_Click;
        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(8, 10);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(131, 17);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "🔧 BLogic Dev Tool";
        // 
        // pnlMain
        // 
        pnlMain.Dock = DockStyle.Fill;
        pnlMain.Location = new Point(0, 36);
        pnlMain.Name = "pnlMain";
        pnlMain.Size = new Size(900, 684);
        pnlMain.TabIndex = 0;
        // 
        // panel1
        // 
        panel1.Controls.Add(pnlMain);
        panel1.Controls.Add(pnlNav);
        panel1.Dock = DockStyle.Fill;
        panel1.Location = new Point(0, 0);
        panel1.Name = "panel1";
        panel1.Size = new Size(900, 720);
        panel1.TabIndex = 0;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(900, 720);
        ControlBox = false;
        Controls.Add(panel1);
        FormBorderStyle = FormBorderStyle.None;
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "BLogic Dev Tool";
        WindowState = FormWindowState.Minimized;
        pnlNav.ResumeLayout(false);
        pnlNav.PerformLayout();
        panel1.ResumeLayout(false);
        ResumeLayout(false);
    }
    private Panel panel1;
    private Button btnCloseForm;
}

