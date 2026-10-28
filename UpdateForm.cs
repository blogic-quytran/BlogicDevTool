using System.Diagnostics;

namespace BLogicDevTool;

/// <summary>Shows the release notes of a newer GitHub release and installs it.</summary>
public sealed class UpdateForm : Form
{
    private readonly UpdateInfo _info;
    private readonly TextBox _txtNotes;
    private readonly ProgressBar _progress;
    private readonly Label _lblStatus;
    private readonly Button _btnInstall;
    private readonly Button _btnLater;

    public UpdateForm(UpdateInfo info)
    {
        _info = info;

        Text            = "BLogic Dev Tool — Update available";
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        ShowInTaskbar   = false;
        ClientSize      = new Size(560, 420);
        Font            = new Font("Segoe UI", 9F);

        var published = info.PublishedAt is { } p ? $"  ·  released {p.LocalDateTime:dd/MM/yyyy HH:mm}" : "";
        var lblHeader = new Label
        {
            Text      = $"🔔 New version {info.Tag} is available",
            Font      = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 102, 204),
            Location  = new Point(16, 14),
            AutoSize  = true,
        };
        var lblSub = new Label
        {
            Text      = $"Current version: {UpdateService.CurrentVersionText}  ·  download {info.Size / 1024.0 / 1024.0:0.0} MB{published}",
            ForeColor = Color.DimGray,
            Location  = new Point(18, 44),
            AutoSize  = true,
        };
        var lblNotes = new Label { Text = "What's new:", Location = new Point(16, 72), AutoSize = true };

        _txtNotes = new TextBox
        {
            Multiline  = true,
            ReadOnly   = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor  = Color.White,
            Location   = new Point(16, 92),
            Size       = new Size(528, 220),
            // GitHub returns LF; a WinForms TextBox only breaks lines on CRLF.
            Text       = string.IsNullOrWhiteSpace(info.Notes) ? "(no release notes)" : info.Notes.Replace("\r\n", "\n").Replace("\n", "\r\n"),
        };

        _progress  = new ProgressBar { Location = new Point(16, 322), Size = new Size(528, 16), Visible = false };
        _lblStatus = new Label { Location = new Point(16, 342), Size = new Size(528, 20), ForeColor = Color.DimGray };

        var lnkGitHub = new LinkLabel { Text = "View on GitHub", Location = new Point(16, 382), AutoSize = true };
        lnkGitHub.LinkClicked += (_, _) => Process.Start(new ProcessStartInfo(info.HtmlUrl) { UseShellExecute = true });

        _btnInstall = new Button
        {
            Text      = "⬇ Install && restart",
            Location  = new Point(300, 374),
            Size      = new Size(150, 32),
            BackColor = Color.FromArgb(0, 102, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
        };
        _btnInstall.FlatAppearance.BorderSize = 0;
        _btnInstall.Click += btnInstall_Click;

        _btnLater = new Button { Text = "Later", Location = new Point(456, 374), Size = new Size(88, 32), DialogResult = DialogResult.Cancel };

        Controls.AddRange(new Control[] { lblHeader, lblSub, lblNotes, _txtNotes, _progress, _lblStatus, lnkGitHub, _btnInstall, _btnLater });
        AcceptButton = _btnInstall;
        CancelButton = _btnLater;
    }

    private async void btnInstall_Click(object? sender, EventArgs e)
    {
        if (AppBusyState.IsBusy)
        {
            MessageBox.Show(this, "An operation is still running. Wait for it to finish, then install the update.",
                "Update", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _btnInstall.Enabled = _btnLater.Enabled = false;
        ControlBox          = false;
        _progress.Visible   = true;
        _lblStatus.Text     = "Downloading...";
        var progress = new Progress<int>(pct =>
        {
            _progress.Value = Math.Clamp(pct, 0, 100);
            _lblStatus.Text = $"Downloading... {pct}%";
        });

        try
        {
            await UpdateService.InstallAsync(_info, progress);
            _lblStatus.Text = "Restarting...";
            Application.Exit();
        }
        catch (Exception ex)
        {
            _progress.Visible   = false;
            _lblStatus.Text     = "";
            _btnInstall.Enabled = _btnLater.Enabled = true;
            ControlBox          = true;
            MessageBox.Show(this, $"Update failed:\n\n{ex.Message}\n\nYou can download it manually from GitHub.",
                "Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
