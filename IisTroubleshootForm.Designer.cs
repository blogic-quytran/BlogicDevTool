namespace BLogicDevTool;

partial class IisTroubleshootForm
{
    private System.ComponentModel.IContainer components = null;

    // Step buttons (left panel)
    private System.Windows.Forms.GroupBox    grpSteps;
    private System.Windows.Forms.Button      btnStep1;
    private System.Windows.Forms.Button      btnStep2;
    private System.Windows.Forms.Button      btnStep3;
    private System.Windows.Forms.Button      btnStep4;
    private System.Windows.Forms.Button      btnStep5;
    private System.Windows.Forms.Button      btnStep6;
    private System.Windows.Forms.Button      btnStep7;
    private System.Windows.Forms.Button      btnStep8;
    private System.Windows.Forms.Button      btnRunAll;

    // Log (right panel)
    private System.Windows.Forms.GroupBox    grpLog;
    private System.Windows.Forms.TextBox     txtLog;
    private System.Windows.Forms.Button      btnClearLog;

    // Status bar
    private System.Windows.Forms.ProgressBar progressBar;
    private System.Windows.Forms.Label       lblStatus;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        grpSteps  = new System.Windows.Forms.GroupBox();
        btnStep1  = new System.Windows.Forms.Button();
        btnStep2  = new System.Windows.Forms.Button();
        btnStep3  = new System.Windows.Forms.Button();
        btnStep4  = new System.Windows.Forms.Button();
        btnStep5  = new System.Windows.Forms.Button();
        btnStep6  = new System.Windows.Forms.Button();
        btnStep7  = new System.Windows.Forms.Button();
        btnStep8  = new System.Windows.Forms.Button();
        btnRunAll = new System.Windows.Forms.Button();
        grpLog    = new System.Windows.Forms.GroupBox();
        txtLog    = new System.Windows.Forms.TextBox();
        btnClearLog = new System.Windows.Forms.Button();
        progressBar = new System.Windows.Forms.ProgressBar();
        lblStatus   = new System.Windows.Forms.Label();

        grpSteps.SuspendLayout();
        grpLog.SuspendLayout();
        SuspendLayout();

        // ── Form ─────────────────────────────────────────────────────────
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(950, 660);
        FormBorderStyle     = FormBorderStyle.Sizable;
        MinimumSize         = new Size(800, 580);
        Name                = "IisTroubleshootForm";
        StartPosition       = FormStartPosition.CenterParent;
        Text                = "🔧  IIS W3SVC Troubleshoot";
        Controls.Add(grpSteps);
        Controls.Add(grpLog);
        Controls.Add(progressBar);
        Controls.Add(lblStatus);

        // ── grpSteps (left, x=10, w=258) ────────────────────────────────
        grpSteps.Anchor   = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        grpSteps.Controls.Add(btnStep1);
        grpSteps.Controls.Add(btnStep2);
        grpSteps.Controls.Add(btnStep3);
        grpSteps.Controls.Add(btnStep4);
        grpSteps.Controls.Add(btnStep5);
        grpSteps.Controls.Add(btnStep6);
        grpSteps.Controls.Add(btnStep7);
        grpSteps.Controls.Add(btnStep8);
        grpSteps.Controls.Add(btnRunAll);
        grpSteps.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpSteps.ForeColor = Color.FromArgb(30, 30, 50);
        grpSteps.Location  = new Point(10, 10);
        grpSteps.Name      = "grpSteps";
        grpSteps.Size      = new Size(258, 610);
        grpSteps.TabStop   = false;
        grpSteps.Text      = "Diagnostic Steps";

        // helper: create step button
        void MkStep(System.Windows.Forms.Button b, string text, Color back, int y, EventHandler h)
        {
            b.BackColor = back;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
            b.ForeColor = Color.White;
            b.Location  = new Point(12, y);
            b.Size      = new Size(234, 40);
            b.TextAlign = ContentAlignment.MiddleLeft;
            b.Padding   = new Padding(6, 0, 0, 0);
            b.UseVisualStyleBackColor = false;
            b.Text      = text;
            b.Click    += h;
            grpSteps.Controls.Add(b);
        }

        var stepBlue  = Color.FromArgb(0, 100, 180);
        var stepTeal  = Color.FromArgb(0, 120, 110);
        int y0 = 30, gap = 46;

        MkStep(btnStep1, "1.  Check Services",          stepBlue,               y0 + gap * 0, BtnStep1_Click);
        MkStep(btnStep2, "2.  Fix Startup Types",        Color.FromArgb(0,130,80), y0 + gap * 1, BtnStep2_Click);
        MkStep(btnStep3, "3.  Backup IIS Config",        stepTeal,               y0 + gap * 2, BtnStep3_Click);
        MkStep(btnStep4, "4.  Validate Config",          stepTeal,               y0 + gap * 3, BtnStep4_Click);
        MkStep(btnStep5, "5.  Check Ports 80 / 443",     Color.FromArgb(80,80,140), y0 + gap * 4, BtnStep5_Click);
        MkStep(btnStep6, "6.  Start W3SVC",              Color.FromArgb(140,60,0), y0 + gap * 5, BtnStep6_Click);
        MkStep(btnStep7, "7.  Extra Diagnostics",        Color.FromArgb(80,80,80), y0 + gap * 6, BtnStep7_Click);
        MkStep(btnStep8, "8.  Final State",              Color.FromArgb(40,80,40), y0 + gap * 7, BtnStep8_Click);

        // Run All
        btnRunAll.Anchor   = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        btnRunAll.BackColor = Color.FromArgb(0, 150, 50);
        btnRunAll.FlatStyle = FlatStyle.Flat;
        btnRunAll.FlatAppearance.BorderSize = 0;
        btnRunAll.Font      = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnRunAll.ForeColor = Color.White;
        btnRunAll.Location  = new Point(12, 560);
        btnRunAll.Size      = new Size(234, 40);
        btnRunAll.Text      = "▶  Run All Steps";
        btnRunAll.UseVisualStyleBackColor = false;
        btnRunAll.Click    += BtnRunAll_Click;

        // ── grpLog (right, x=278, fills remaining width) ─────────────────
        grpLog.Anchor   = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grpLog.Controls.Add(txtLog);
        grpLog.Controls.Add(btnClearLog);
        grpLog.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpLog.ForeColor = Color.FromArgb(30, 30, 50);
        grpLog.Location  = new Point(278, 10);
        grpLog.Name      = "grpLog";
        grpLog.Size      = new Size(662, 610);
        grpLog.TabStop   = false;
        grpLog.Text      = "Output Log";

        txtLog.Anchor     = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtLog.BackColor  = Color.FromArgb(18, 18, 30);
        txtLog.Font       = new Font("Consolas", 9F);
        txtLog.ForeColor  = Color.FromArgb(180, 230, 180);
        txtLog.Location   = new Point(8, 22);
        txtLog.Multiline  = true;
        txtLog.Name       = "txtLog";
        txtLog.ReadOnly   = true;
        txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        txtLog.Size       = new Size(646, 555);
        txtLog.TabIndex   = 0;
        txtLog.WordWrap   = false;

        btnClearLog.Anchor    = AnchorStyles.Bottom | AnchorStyles.Right;
        btnClearLog.BackColor = Color.FromArgb(50, 50, 70);
        btnClearLog.FlatStyle = FlatStyle.Flat;
        btnClearLog.FlatAppearance.BorderSize = 0;
        btnClearLog.Font      = new Font("Segoe UI", 8.5F);
        btnClearLog.ForeColor = Color.Silver;
        btnClearLog.Location  = new Point(558, 582);
        btnClearLog.Name      = "btnClearLog";
        btnClearLog.Size      = new Size(96, 24);
        btnClearLog.TabIndex  = 1;
        btnClearLog.Text      = "Clear Log";
        btnClearLog.UseVisualStyleBackColor = false;
        btnClearLog.Click    += BtnClearLog_Click;

        // ── Status bar ────────────────────────────────────────────────────
        progressBar.Anchor   = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        progressBar.Location = new Point(10, 628);
        progressBar.Name     = "progressBar";
        progressBar.Size     = new Size(930, 8);
        progressBar.Style    = ProgressBarStyle.Marquee;
        progressBar.Visible  = false;

        lblStatus.Anchor    = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.Font      = new Font("Segoe UI", 9F);
        lblStatus.ForeColor = Color.DarkGray;
        lblStatus.Location  = new Point(10, 640);
        lblStatus.Name      = "lblStatus";
        lblStatus.Size      = new Size(930, 18);
        lblStatus.Text      = "Ready.";

        grpSteps.ResumeLayout(false);
        grpLog.ResumeLayout(false);
        grpLog.PerformLayout();
        ResumeLayout(false);
    }
}
