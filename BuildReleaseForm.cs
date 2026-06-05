namespace BLogicDevTool;

public partial class BuildReleaseForm : UserControl
{
    private static readonly string[] _knownExtensions = new[]
    {
        ".dll", ".exe", ".pdb", ".xml", ".exe.config",
        ".config", ".json", ".resources.dll"
    };

    private ToolStripDropDown? _extDropDown;
    private CheckedListBox? _extCheckedList;
    private ToolStripDropDown? _redistDropDown;
    private CheckedListBox? _redistCheckedList;

    private string _outputBasePath = "";
    private string _configuration = "Release";
    private List<string> _copyExtensions = new() { ".dll", ".exe", ".pdb", ".xml", ".exe.config" };
    private string _gitBaseBranch = "dev";
    private string _gitCompareBranch = "HEAD";
    private bool _includeUncommitted = false;
    private bool _zipAfterBuild = true;
    private bool _cleanOutputFirst = true;
    private List<BuildConfig> _configs = new();
    private int _selectedIndex = -1;
    private bool _suppressDetailEvents = false;
    private bool _suppressGlobalEvents = false;
    private bool _isBusy = false;

    public BuildReleaseForm()
    {
        InitializeComponent();
        cboConfig.Items.AddRange(new object[] { "Release", "Debug" });
        cboConfig.SelectedIndex = 0;
        cboEngine.Items.AddRange(new object[]
        {
            "dotnet build (SDK)",
            "MSBuild (Visual Studio)"
        });
        cboEngine.SelectedIndex = 0;
    }

    private static string EngineDisplayName(string engine) =>
        string.Equals(engine, "msbuild", StringComparison.OrdinalIgnoreCase)
            ? "MSBuild (Visual Studio)"
            : "dotnet build (SDK)";

    private static string EngineFromDisplay(string? display) =>
        (display ?? "").StartsWith("MSBuild", StringComparison.OrdinalIgnoreCase)
            ? "msbuild"
            : "dotnet";

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        var settings = BuildConfigStore.Load();
        _outputBasePath = settings.OutputBasePath;
        _configs = settings.Configs;
        if (!string.IsNullOrWhiteSpace(settings.Configuration))
            _configuration = settings.Configuration;
        if (settings.CopyExtensions != null && settings.CopyExtensions.Count > 0)
            _copyExtensions = settings.CopyExtensions;
        if (!string.IsNullOrWhiteSpace(settings.GitBaseBranch))
            _gitBaseBranch = settings.GitBaseBranch;
        if (!string.IsNullOrWhiteSpace(settings.GitCompareBranch))
            _gitCompareBranch = settings.GitCompareBranch;
        _zipAfterBuild = settings.ZipAfterBuild;
        _cleanOutputFirst = settings.CleanOutputFirst;
        _includeUncommitted = settings.IncludeUncommitted;
        _suppressGlobalEvents = true;
        txtGlobalOutBase.Text = _outputBasePath;
        cboConfig.SelectedItem = _configuration;
        if (cboConfig.SelectedIndex < 0) cboConfig.SelectedIndex = 0;
        UpdateExtensionsButtonText();
        txtGitBase.Text = _gitBaseBranch;
        txtGitCompare.Text = _gitCompareBranch;
        chkIncludeUncommitted.Checked = _includeUncommitted;
        chkZipOutput.Checked = _zipAfterBuild;
        chkCleanFirst.Checked = _cleanOutputFirst;
        _suppressGlobalEvents = false;
        RefreshList(preserveChecks: false);
        if (_configs.Count > 0)
            clbConfigs.SelectedIndex = 0;
        else
            LoadDetailFromConfig();
    }

    private string BuildFullOutputPath(BuildConfig c)
    {
        if (string.IsNullOrWhiteSpace(_outputBasePath)) return "";
        var folder = (c.OutputFolderName ?? "").Replace('/', Path.DirectorySeparatorChar).Trim();
        return string.IsNullOrEmpty(folder)
            ? _outputBasePath
            : Path.Combine(_outputBasePath, folder);
    }

    /// <summary>
    /// Where ExtraFolders (Templates, Language, …) should land for a given DLL
    /// destination. When DLLs go into a "bin" sub-folder (e.g. Server/bin), the
    /// assets belong BESIDE bin (Server/), matching the runtime layout — so we
    /// return the parent. Otherwise (POS, BO, …) they go into the destination itself.
    /// </summary>
    private static string ExtraFoldersDestination(string dllDestination)
    {
        if (string.IsNullOrWhiteSpace(dllDestination)) return dllDestination;
        var trimmed = dllDestination.TrimEnd('\\', '/');
        if (string.Equals(Path.GetFileName(trimmed), "bin", StringComparison.OrdinalIgnoreCase))
        {
            var parent = Path.GetDirectoryName(trimmed);
            if (!string.IsNullOrEmpty(parent)) return parent;
        }
        return dllDestination;
    }

    private BuildSettings BuildSettingsSnapshot() =>
        new()
        {
            OutputBasePath = _outputBasePath,
            Configuration = _configuration,
            CopyExtensions = _copyExtensions,
            GitBaseBranch = _gitBaseBranch,
            GitCompareBranch = _gitCompareBranch,
            IncludeUncommitted = _includeUncommitted,
            ZipAfterBuild = _zipAfterBuild,
            CleanOutputFirst = _cleanOutputFirst,
            Configs = _configs
        };

// ── Global Output Base ──

    private void txtGlobalOutBase_TextChanged(object? sender, EventArgs e)
    {
        if (_suppressGlobalEvents) return;
        _outputBasePath = txtGlobalOutBase.Text.Trim();
        UpdatePreview();
    }

    // ── Extensions multi-select dropdown ──

    private void UpdateExtensionsButtonText()
    {
        btnExtensions.Text = _copyExtensions.Count == 0
            ? "(none)"
            : string.Join(", ", _copyExtensions);
    }

    private void btnExtensions_Click(object? sender, EventArgs e)
    {
        BuildExtensionsDropdown();
        _extDropDown!.Show(btnExtensions, 0, btnExtensions.Height);
    }

    private void BuildExtensionsDropdown()
    {
        _extDropDown?.Dispose();
        _extCheckedList?.Dispose();

        var items = _knownExtensions
            .Concat(_copyExtensions.Where(x =>
                !_knownExtensions.Contains(x, StringComparer.OrdinalIgnoreCase)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        _extCheckedList = new CheckedListBox
        {
            CheckOnClick = true,
            IntegralHeight = false,
            BorderStyle = BorderStyle.None,
            Width = 220,
            Height = Math.Min(items.Count, 12) * 18 + 4
        };
        foreach (var ext in items)
        {
            var idx = _extCheckedList.Items.Add(ext);
            if (_copyExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                _extCheckedList.SetItemChecked(idx, true);
        }
        _extCheckedList.ItemCheck += ExtCheckedList_ItemCheck;

        var host = new ToolStripControlHost(_extCheckedList)
        {
            AutoSize = false,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
            Size = _extCheckedList.Size
        };
        _extDropDown = new ToolStripDropDown
        {
            Padding = Padding.Empty,
            AutoSize = true
        };
        _extDropDown.Items.Add(host);
    }

    private void ExtCheckedList_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        // ItemCheck fires BEFORE the state changes — defer reading until after.
        BeginInvoke(() => CommitExtensionsSelection());
    }

    private void CommitExtensionsSelection()
    {
        if (_extCheckedList == null) return;
        var selected = new List<string>();
        for (int i = 0; i < _extCheckedList.Items.Count; i++)
            if (_extCheckedList.GetItemChecked(i))
                selected.Add(_extCheckedList.Items[i].ToString()!);
        _copyExtensions = selected;
        UpdateExtensionsButtonText();
    }

    // ── Redistribute (Copy To Configs) multi-select dropdown ──

    private void UpdateRedistributeButtonText()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _configs.Count)
        {
            btnRedistribute.Text = "(none)";
            return;
        }
        var list = _configs[_selectedIndex].RedistributeTo ?? new();
        btnRedistribute.Text = list.Count == 0 ? "(none)" : string.Join(", ", list);
    }

    private void btnRedistribute_Click(object? sender, EventArgs e)
    {
        if (_selectedIndex < 0 || _selectedIndex >= _configs.Count) return;
        BuildRedistributeDropdown();
        _redistDropDown!.Show(btnRedistribute, 0, btnRedistribute.Height);
    }

    private void BuildRedistributeDropdown()
    {
        _redistDropDown?.Dispose();
        _redistCheckedList?.Dispose();

        var current = _configs[_selectedIndex];
        var selectedNames = new HashSet<string>(
            current.RedistributeTo ?? new(), StringComparer.OrdinalIgnoreCase);

        var others = _configs
            .Where(c => !ReferenceEquals(c, current))
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .ToList();

        _redistCheckedList = new CheckedListBox
        {
            CheckOnClick = true,
            IntegralHeight = false,
            BorderStyle = BorderStyle.None,
            Width = 240,
            Height = Math.Max(40, Math.Min(others.Count, 12) * 18 + 4)
        };
        if (others.Count == 0)
        {
            _redistCheckedList.Items.Add("(no other configs)");
            _redistCheckedList.Enabled = false;
        }
        else
        {
            foreach (var other in others)
            {
                var idx = _redistCheckedList.Items.Add(other.Name);
                if (selectedNames.Contains(other.Name))
                    _redistCheckedList.SetItemChecked(idx, true);
            }
            _redistCheckedList.ItemCheck += RedistCheckedList_ItemCheck;
        }

        var host = new ToolStripControlHost(_redistCheckedList)
        {
            AutoSize = false,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
            Size = _redistCheckedList.Size
        };
        _redistDropDown = new ToolStripDropDown
        {
            Padding = Padding.Empty,
            AutoSize = true
        };
        _redistDropDown.Items.Add(host);
    }

    private void RedistCheckedList_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        BeginInvoke(() => CommitRedistributeSelection());
    }

    private void CommitRedistributeSelection()
    {
        if (_redistCheckedList == null) return;
        if (_selectedIndex < 0 || _selectedIndex >= _configs.Count) return;
        var selected = new List<string>();
        for (int i = 0; i < _redistCheckedList.Items.Count; i++)
            if (_redistCheckedList.GetItemChecked(i))
                selected.Add(_redistCheckedList.Items[i].ToString()!);
        _configs[_selectedIndex].RedistributeTo = selected;
        UpdateRedistributeButtonText();
    }

    private void cboConfig_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_suppressGlobalEvents) return;
        _configuration = cboConfig.SelectedItem?.ToString() ?? "Release";
        RefreshList(preserveChecks: true);
        UpdatePreview();
    }

    private void txtGitBase_TextChanged(object? sender, EventArgs e)
    {
        if (_suppressGlobalEvents) return;
        _gitBaseBranch = txtGitBase.Text.Trim();
    }

    private void txtGitCompare_TextChanged(object? sender, EventArgs e)
    {
        if (_suppressGlobalEvents) return;
        _gitCompareBranch = txtGitCompare.Text.Trim();
    }

    private void chkZipOutput_CheckedChanged(object? sender, EventArgs e)
    {
        if (_suppressGlobalEvents) return;
        _zipAfterBuild = chkZipOutput.Checked;
    }

    private void chkCleanFirst_CheckedChanged(object? sender, EventArgs e)
    {
        if (_suppressGlobalEvents) return;
        _cleanOutputFirst = chkCleanFirst.Checked;
    }

    private void chkIncludeUncommitted_CheckedChanged(object? sender, EventArgs e)
    {
        if (_suppressGlobalEvents) return;
        _includeUncommitted = chkIncludeUncommitted.Checked;
    }

    private void btnGlobalBrowseOutBase_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Select base output folder (shared across all configs)",
            SelectedPath = Directory.Exists(_outputBasePath) ? _outputBasePath : ""
        };
        if (dlg.ShowDialog() == DialogResult.OK)
            txtGlobalOutBase.Text = dlg.SelectedPath;
    }

    // ── List management ──────────────────────────────────────────────────────

    private void RefreshList(bool preserveChecks)
    {
        var checkedSet = new HashSet<int>();
        if (preserveChecks)
        {
            for (int i = 0; i < clbConfigs.Items.Count; i++)
                if (clbConfigs.GetItemChecked(i)) checkedSet.Add(i);
        }

        clbConfigs.BeginUpdate();
        clbConfigs.Items.Clear();
        for (int i = 0; i < _configs.Count; i++)
        {
            var c = _configs[i];
            var label = FormatLabel(i, c);
            clbConfigs.Items.Add(label, checkedSet.Contains(i));
        }
        clbConfigs.EndUpdate();
    }

    private string FormatLabel(int index, BuildConfig c)
    {
        var name = string.IsNullOrWhiteSpace(c.Name) ? "(unnamed)" : c.Name;
        return $"{index + 1}. {name}  [{_configuration}]";
    }

    private void RefreshSelectedLabel()
    {
        if (_selectedIndex < 0 || _selectedIndex >= clbConfigs.Items.Count) return;
        var newLabel = FormatLabel(_selectedIndex, _configs[_selectedIndex]);
        if (!string.Equals(clbConfigs.Items[_selectedIndex]?.ToString(), newLabel))
        {
            bool wasChecked = clbConfigs.GetItemChecked(_selectedIndex);
            clbConfigs.Items[_selectedIndex] = newLabel;
            clbConfigs.SetItemChecked(_selectedIndex, wasChecked);
        }
    }

    private void clbConfigs_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _selectedIndex = clbConfigs.SelectedIndex;
        LoadDetailFromConfig();
    }

    private bool _suppressItemCheck = false;

    private void clbConfigs_MouseDown(object? sender, MouseEventArgs e)
    {
        _suppressItemCheck = false;
        int idx = clbConfigs.IndexFromPoint(e.Location);
        if (idx < 0) return;

        var itemRect = clbConfigs.GetItemRectangle(idx);
        var checkRect = new Rectangle(
            itemRect.Left,
            itemRect.Top,
            itemRect.Height,
            itemRect.Height);

        if (!checkRect.Contains(e.Location))
            _suppressItemCheck = true;
    }

    private void clbConfigs_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        if (_suppressItemCheck)
        {
            e.NewValue = e.CurrentValue;
            _suppressItemCheck = false;
        }
    }

    // ── Detail ↔ model sync ──────────────────────────────────────────────────

    private void LoadDetailFromConfig()
    {
        _suppressDetailEvents = true;
        try
        {
            if (_selectedIndex < 0 || _selectedIndex >= _configs.Count)
            {
                txtName.Text = "";
                txtSln.Text = "";
                cboEngine.SelectedIndex = 0;
                txtOutFolder.Text = "";
                txtExtraFolders.Text = "";
                btnRedistribute.Text = "(none)";
                chkFilterGit.Checked = false;
                lblOutPreview.Text = "";
                UpdateDetailEnabled();
                return;
            }
            var c = _configs[_selectedIndex];
            txtName.Text = c.Name;
            txtSln.Text = c.SolutionPath;
            cboEngine.SelectedItem = EngineDisplayName(c.BuildEngine);
            if (cboEngine.SelectedIndex < 0) cboEngine.SelectedIndex = 0;
            txtOutFolder.Text = c.OutputFolderName;
            txtExtraFolders.Text = string.Join(", ", c.ExtraFolders ?? new List<string>());
            UpdateRedistributeButtonText();
            chkFilterGit.Checked = c.FilterByGit;
            UpdatePreview();
            UpdateDetailEnabled();
        }
        finally { _suppressDetailEvents = false; }
    }

    private void SaveDetailToConfig()
    {
        if (_suppressDetailEvents) return;
        if (_selectedIndex < 0 || _selectedIndex >= _configs.Count) return;

        var c = _configs[_selectedIndex];
        c.Name = txtName.Text.Trim();
        c.SolutionPath = txtSln.Text.Trim();
        c.BuildEngine = EngineFromDisplay(cboEngine.SelectedItem?.ToString());
        c.OutputFolderName = txtOutFolder.Text.Trim();
        c.ExtraFolders = ParseExtraFolders(txtExtraFolders.Text);
        c.FilterByGit = chkFilterGit.Checked;

        UpdatePreview();
    }

    private static List<string> ParseExtraFolders(string input)
    {
        return input.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().Trim('/', '\\'))
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void txtName_Leave(object? sender, EventArgs e) => RefreshSelectedLabel();

    private void UpdatePreview()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _configs.Count)
        {
            lblOutPreview.Text = "";
            return;
        }
        var path = BuildFullOutputPath(_configs[_selectedIndex]);
        lblOutPreview.Text = string.IsNullOrEmpty(path) ? "" : "→ " + path;
    }

    private void UpdateDetailEnabled()
    {
        var on = _selectedIndex >= 0 && !_isBusy;
        txtName.Enabled = on;
        txtSln.Enabled = on;
        cboEngine.Enabled = on;
        txtOutFolder.Enabled = on;
        txtExtraFolders.Enabled = on;
        btnRedistribute.Enabled = on;
        chkFilterGit.Enabled = on;
        btnBrowseSln.Enabled = on;
        btnSaveConfig.Enabled = !_isBusy;

        txtGlobalOutBase.Enabled = !_isBusy;
        btnGlobalBrowseOutBase.Enabled = !_isBusy;
        cboConfig.Enabled = !_isBusy;
        btnExtensions.Enabled = !_isBusy;
        txtGitBase.Enabled = !_isBusy;
        txtGitCompare.Enabled = !_isBusy;
        chkIncludeUncommitted.Enabled = !_isBusy;
    }

    // ── Toolbar ──────────────────────────────────────────────────────────────

    private void btnAdd_Click(object? sender, EventArgs e)
    {
        _configs.Add(new BuildConfig
        {
            Name = $"Config {_configs.Count + 1}"
        });
        RefreshList(preserveChecks: true);
        clbConfigs.SelectedIndex = _configs.Count - 1;
    }

    private void btnDelete_Click(object? sender, EventArgs e)
    {
        if (_selectedIndex < 0 || _selectedIndex >= _configs.Count) return;
        var name = _configs[_selectedIndex].Name;
        if (MessageBox.Show($"Delete configuration '{name}'?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

        _configs.RemoveAt(_selectedIndex);
        var newIdx = Math.Min(_selectedIndex, _configs.Count - 1);
        _selectedIndex = -1;
        RefreshList(preserveChecks: false);
        if (newIdx >= 0) clbConfigs.SelectedIndex = newIdx;
        else LoadDetailFromConfig();
    }

    private void btnMoveUp_Click(object? sender, EventArgs e)
    {
        if (_selectedIndex <= 0) return;
        bool checkedNow = clbConfigs.GetItemChecked(_selectedIndex);
        bool checkedAbove = clbConfigs.GetItemChecked(_selectedIndex - 1);
        (_configs[_selectedIndex - 1], _configs[_selectedIndex]) =
            (_configs[_selectedIndex], _configs[_selectedIndex - 1]);
        var newIdx = _selectedIndex - 1;
        RefreshList(preserveChecks: false);
        clbConfigs.SetItemChecked(newIdx, checkedNow);
        clbConfigs.SetItemChecked(newIdx + 1, checkedAbove);
        clbConfigs.SelectedIndex = newIdx;
    }

    private void btnMoveDown_Click(object? sender, EventArgs e)
    {
        if (_selectedIndex < 0 || _selectedIndex >= _configs.Count - 1) return;
        bool checkedNow = clbConfigs.GetItemChecked(_selectedIndex);
        bool checkedBelow = clbConfigs.GetItemChecked(_selectedIndex + 1);
        (_configs[_selectedIndex + 1], _configs[_selectedIndex]) =
            (_configs[_selectedIndex], _configs[_selectedIndex + 1]);
        var newIdx = _selectedIndex + 1;
        RefreshList(preserveChecks: false);
        clbConfigs.SetItemChecked(newIdx, checkedNow);
        clbConfigs.SetItemChecked(newIdx - 1, checkedBelow);
        clbConfigs.SelectedIndex = newIdx;
    }

    // ── Detail change handlers ──

    private void txtName_TextChanged(object? sender, EventArgs e) => SaveDetailToConfig();
    private void txtSln_TextChanged(object? sender, EventArgs e) => SaveDetailToConfig();
    private void cboEngine_SelectedIndexChanged(object? sender, EventArgs e) => SaveDetailToConfig();
    private void txtOutFolder_TextChanged(object? sender, EventArgs e) => SaveDetailToConfig();
    private void txtExtraFolders_TextChanged(object? sender, EventArgs e) => SaveDetailToConfig();
    private void chkFilterGit_CheckedChanged(object? sender, EventArgs e)
    {
        SaveDetailToConfig();
        UpdateDetailEnabled();
    }

    // ── Browse buttons ──

    private void btnBrowseSln_Click(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title = "Select Solution File",
            Filter = "Solution files (*.sln)|*.sln|All files (*.*)|*.*"
        };
        if (!string.IsNullOrEmpty(txtSln.Text) && File.Exists(txtSln.Text))
            dlg.InitialDirectory = Path.GetDirectoryName(txtSln.Text);
        if (dlg.ShowDialog() == DialogResult.OK)
            txtSln.Text = dlg.FileName;
    }

    // ── Save ──

    private void btnSaveConfig_Click(object? sender, EventArgs e)
    {
        try
        {
            BuildConfigStore.Save(BuildSettingsSnapshot());
            SetStatus($"✔ Saved {_configs.Count} config(s).", Color.DarkGreen);
        }
        catch (Exception ex)
        {
            SetStatus($"✘ Save failed: {ex.Message}", Color.DarkRed);
        }
    }

    // ── Preview ──

    private async void btnPreview_Click(object? sender, EventArgs e)
    {
        var targets = GetCheckedOrSelectedConfigs();
        if (targets.Count == 0)
        {
            MessageBox.Show("Check at least one config, or select one.",
                "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        SetBusy(true, "Previewing...");
        ClearLog();
        try
        {
            for (int i = 0; i < targets.Count; i++)
            {
                AppendLog($"\n>>> [{i + 1}/{targets.Count}] Preview: {targets[i].Name}");
                await PreviewOneAsync(targets[i]);
            }
            SetStatus("✔ Preview complete.", Color.DarkGreen);
        }
        catch (Exception ex)
        {
            AppendLog($"✘ Preview error: {ex.Message}");
            SetStatus("✘ Preview failed.", Color.DarkRed);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task PreviewOneAsync(BuildConfig c)
    {
        if (!File.Exists(c.SolutionPath))
        {
            AppendLog($"  ✘ SLN not found: {c.SolutionPath}");
            return;
        }

        var projects = BuildHelper.FindAllProjectsInSolution(c.SolutionPath);
        AppendLog($"  Solution contains {projects.Count} project(s).");

        List<string> targetProjects;
        if (c.FilterByGit)
        {
            var (owning, _) = await ResolveAffectedProjectsAsync(c, projects);
            if (owning == null) return;
            targetProjects = owning;
            AppendLog($"  → {targetProjects.Count} affected project(s):");
        }
        else
        {
            targetProjects = projects;
            AppendLog($"  → Would build all {projects.Count} project(s):");
        }

        foreach (var p in targetProjects)
        {
            AppendLog($"     • {Path.GetFileNameWithoutExtension(p)}");
            foreach (var a in BuildHelper.ResolveOutputDlls(p, _configuration))
            {
                var tfm = string.IsNullOrEmpty(a.TargetFramework) ? "" : $"  [{a.TargetFramework}]";
                AppendLog($"        → {Path.GetFileName(a.OutputDllPath)}{tfm}");
            }
        }
        AppendLog($"  Destination: {BuildFullOutputPath(c)}");
    }

    // ── Build ──

    private async void btnBuildChecked_Click(object? sender, EventArgs e)
    {
        var targets = GetCheckedConfigs();
        if (targets.Count == 0)
        {
            MessageBox.Show("No configuration is checked.",
                "Build", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        await BuildManyAsync(targets);
    }

    private async void btnBuildAll_Click(object? sender, EventArgs e)
    {
        if (_configs.Count == 0)
        {
            MessageBox.Show("No configurations defined.",
                "Build", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        await BuildManyAsync(_configs.ToList());
    }

    private async Task BuildManyAsync(List<BuildConfig> targets)
    {
        var summary = string.Join("\n",
            targets.Select((c, i) => $"  {i + 1}. {c.Name}  [{_configuration}]"));
        if (MessageBox.Show(
                $"Build {targets.Count} configuration(s) sequentially?\n\n{summary}",
                "Confirm Build", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            != DialogResult.Yes) return;

        SetBusy(true, "Building...");
        ClearLog();
        try
        {
            try { BuildConfigStore.Save(BuildSettingsSnapshot()); } catch { /* non-fatal */ }

            if (_cleanOutputFirst)
                await Task.Run(() => CleanDestinations(targets));

            int idx = 0;
            int failures = 0;
            foreach (var c in targets)
            {
                idx++;
                AppendLog($"\n══════════════════════════════════════════════");
                AppendLog($" [{idx}/{targets.Count}] {c.Name}   [{_configuration}]");
                AppendLog($"══════════════════════════════════════════════");
                try
                {
                    var ok = await BuildOneAsync(c);
                    if (!ok) failures++;
                }
                catch (Exception ex)
                {
                    failures++;
                    AppendLog($"✘ Unhandled: {ex.Message}");
                }
            }

            if (failures == 0)
            {
                AppendLog($"\n✔ All {targets.Count} build(s) completed.");
                if (_zipAfterBuild)
                    ZipOutputBase();
                SetStatus("✔ Build complete.", Color.DarkGreen);
            }
            else
            {
                AppendLog($"\n⚠ Completed with {failures}/{targets.Count} failure(s).");
                if (_zipAfterBuild)
                    AppendLog($"  ⏭ Skipping zip — build had failure(s).");
                SetStatus($"⚠ {failures} failed.", Color.DarkOrange);
            }
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task<bool> BuildOneAsync(BuildConfig c)
    {
        if (!File.Exists(c.SolutionPath))
        {
            AppendLog($"✘ SLN not found: {c.SolutionPath}");
            return false;
        }
        var destPath = BuildFullOutputPath(c);
        if (string.IsNullOrWhiteSpace(destPath))
        {
            AppendLog($"✘ Output path is empty (set Output Base + Folder).");
            return false;
        }

        var projects = BuildHelper.FindAllProjectsInSolution(c.SolutionPath);
        AppendLog($"  Projects in solution: {projects.Count}");

        List<string> targetProjects;
        HashSet<string>? extraFolderFilter = null;
        if (c.FilterByGit)
        {
            var (owning, changedFiles) = await ResolveAffectedProjectsAsync(c, projects);
            if (owning == null) return false;
            targetProjects = owning;
            AppendLog($"  Affected projects: {targetProjects.Count}");
            if (changedFiles != null)
            {
                extraFolderFilter = new HashSet<string>(
                    changedFiles.Select(Path.GetFileName).Where(s => !string.IsNullOrEmpty(s))!,
                    StringComparer.OrdinalIgnoreCase);
            }
            if (targetProjects.Count == 0)
            {
                AppendLog($"  ⚠ No projects affected — skipping build & copy.");
                return true;
            }
        }
        else
        {
            targetProjects = projects;
        }

        AppendLog($"\n  ▶ {(c.BuildEngine == "msbuild" ? "MSBuild" : "dotnet build")}  [{_configuration}]");
        int exit;
        string fullOutput;
        try
        {
            (exit, fullOutput) = await BuildHelper.BuildSolutionAsync(
                c.SolutionPath, _configuration, c.BuildEngine,
                line => AppendLog($"    {line}"));
        }
        catch (Exception ex)
        {
            AppendLog($"  ✘ Build error: {ex.Message}");
            try
            {
                var errPath = BuildHelper.SaveBuildLog(c, _configuration, destPath, -1, ex.ToString());
                AppendLog($"  📄 Error log saved: {errPath}");
            }
            catch (Exception logEx)
            {
                AppendLog($"  ⚠ Could not save error log: {logEx.Message}");
            }
            return false;
        }
        if (exit != 0)
        {
            AppendLog($"  ✘ Build failed (exit {exit}).");
            try
            {
                var failPath = BuildHelper.SaveBuildLog(c, _configuration, destPath, exit, fullOutput);
                AppendLog($"  📄 Build log saved: {failPath}");
            }
            catch (Exception logEx)
            {
                AppendLog($"  ⚠ Could not save build log: {logEx.Message}");
            }
            return false;
        }
        AppendLog($"  ✔ Build OK.");
        try
        {
            var okPath = BuildHelper.SaveBuildLog(c, _configuration, destPath, exit, fullOutput);
            AppendLog($"  📄 Build log saved: {okPath}");
        }
        catch (Exception logEx)
        {
            AppendLog($"  ⚠ Could not save build log: {logEx.Message}");
        }

        AppendLog($"\n  Output → {destPath}");
        try { Directory.CreateDirectory(destPath); }
        catch (Exception ex)
        {
            AppendLog($"  ✘ Cannot create output folder: {ex.Message}");
            return false;
        }

        var allArts = new List<BuildArtifact>();
        foreach (var p in targetProjects)
            allArts.AddRange(BuildHelper.ResolveOutputDlls(p, _configuration));

        var exts = _copyExtensions.Count > 0 ? _copyExtensions
            : new List<string> { ".dll", ".exe", ".pdb", ".xml", ".exe.config" };
        AppendLog($"  Copying artifacts (extensions: {string.Join(", ", exts)})...");
        var copied = BuildHelper.CopyArtifacts(allArts, destPath, exts, msg => AppendLog($"    {msg}"));
        AppendLog($"  ✔ Copied {copied} file(s).");

        if (c.ExtraFolders != null && c.ExtraFolders.Count > 0)
        {
            var extraDest = ExtraFoldersDestination(destPath);
            var filterNote = extraFolderFilter != null
                ? $" (git filter: {extraFolderFilter.Count} filename(s))"
                : "";
            AppendLog($"\n  Copying extra folders{filterNote}: {string.Join(", ", c.ExtraFolders)}");
            if (!string.Equals(extraDest, destPath, StringComparison.OrdinalIgnoreCase))
                AppendLog($"    (beside bin → {extraDest})");
            var extraCopied = BuildHelper.CopyExtraFolders(
                allArts, c.ExtraFolders, extraDest, extraFolderFilter,
                msg => AppendLog($"    {msg}"));
            AppendLog($"  ✔ Extra folders: {extraCopied} file(s).");
        }

        if (c.RedistributeTo != null && c.RedistributeTo.Count > 0)
        {
            AppendLog($"\n  Redistributing to: {string.Join(", ", c.RedistributeTo)}");
            foreach (var targetName in c.RedistributeTo)
            {
                var target = _configs.FirstOrDefault(x =>
                    string.Equals(x.Name, targetName, StringComparison.OrdinalIgnoreCase));
                if (target == null)
                {
                    AppendLog($"    ⚠ Target config '{targetName}' not found.");
                    continue;
                }
                var targetPath = BuildFullOutputPath(target);
                if (string.IsNullOrWhiteSpace(targetPath))
                {
                    AppendLog($"    ⚠ Target '{targetName}' has no output path.");
                    continue;
                }
                try { Directory.CreateDirectory(targetPath); }
                catch (Exception ex)
                {
                    AppendLog($"    ✘ Cannot create '{targetPath}': {ex.Message}");
                    continue;
                }
                AppendLog($"    → {target.Name}: {targetPath}");
                var rCopied = BuildHelper.CopyArtifacts(
                    allArts, targetPath, exts, msg => AppendLog($"      {msg}"));
                AppendLog($"    ✔ Copied {rCopied} file(s) to '{target.Name}'.");

                if (c.ExtraFolders != null && c.ExtraFolders.Count > 0)
                {
                    var rExtraDest = ExtraFoldersDestination(targetPath);
                    var rExtra = BuildHelper.CopyExtraFolders(
                        allArts, c.ExtraFolders, rExtraDest, extraFolderFilter,
                        msg => AppendLog($"      {msg}"));
                    AppendLog($"    ✔ Extra folders to '{target.Name}': {rExtra} file(s).");
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Auto-detects the git repo from the solution path, runs git diff against
    /// the globally configured base/compare refs, and maps changed files to owning
    /// csprojs within the solution. Returns null on fatal error (already logged).
    /// </summary>
    private async Task<(List<string>? projects, List<string>? changedFiles)>
        ResolveAffectedProjectsAsync(BuildConfig c, List<string> projectsInSolution)
    {
        var repoPath = GitDiffHelper.DetectRepoFromPath(c.SolutionPath);
        if (string.IsNullOrEmpty(repoPath))
        {
            AppendLog($"  ✘ No git repo found above solution path.");
            return (null, null);
        }
        AppendLog($"  Git repo: {repoPath}");

        if (string.IsNullOrWhiteSpace(_gitBaseBranch))
        {
            AppendLog($"  ✘ Global Git Base ref is empty.");
            return (null, null);
        }
        var compareRef = string.IsNullOrWhiteSpace(_gitCompareBranch) ? "HEAD" : _gitCompareBranch;

        AppendLog($"  Fetching {_gitBaseBranch}...");
        var (fetchOk, fetchMsg) = await GitDiffHelper.TryFetchRefAsync(repoPath, _gitBaseBranch);
        AppendLog(fetchOk ? $"  ✔ {fetchMsg}" : $"  ⚠ {fetchMsg} (continuing with cached ref)");

        List<string> changed;
        try
        {
            changed = await GitDiffHelper.GetChangedFilesAsync(
                repoPath, _gitBaseBranch, compareRef, _includeUncommitted);
        }
        catch (Exception ex)
        {
            AppendLog($"  ✘ git diff failed: {ex.Message}");
            return (null, null);
        }
        var diffScope = _includeUncommitted
            ? $"{_gitBaseBranch} → working tree, incl. uncommitted"
            : $"{_gitBaseBranch}...{compareRef}";
        AppendLog($"  Git diff: {changed.Count} file(s) changed ({diffScope}).");

        var owning = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in changed)
        {
            try
            {
                var rel = f.TrimStart('/', '\\');
                var fullPath = Path.GetFullPath(Path.Combine(repoPath, rel));
                var owner = BuildHelper.FindOwningCsproj(fullPath, projectsInSolution);
                if (owner != null) owning.Add(owner);
            }
            catch { /* skip malformed entry */ }
        }
        return (owning.ToList(), changed);
    }

    /// <summary>
    /// Empties every destination folder this run will write to — each target config's
    /// own output folder plus any redistribute-target folders — exactly once, BEFORE
    /// the build loop. Doing it upfront (rather than per-config) means a config that
    /// redistributes into another config's folder won't have its files wiped after copy.
    /// Only the named sub-folders are cleared; files directly in OutputBasePath (e.g. a
    /// previous .zip) are left untouched.
    /// </summary>
    private void CleanDestinations(List<BuildConfig> targets)
    {
        var folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddFull(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            try { folders.Add(Path.GetFullPath(path)); } catch { /* skip */ }
        }

        // Adds the DLL destination plus each extra-folder destination (which, for a
        // "bin"-style layout, sits beside bin rather than inside it). This keeps the
        // clean step in sync with where the copy step actually writes.
        void AddDestination(string dllDest, IReadOnlyList<string>? extraFolders)
        {
            AddFull(dllDest);
            if (extraFolders == null || extraFolders.Count == 0) return;
            var extraDest = ExtraFoldersDestination(dllDest);
            foreach (var name in extraFolders)
            {
                var folderName = name.Trim().Trim('/', '\\');
                if (folderName.Length > 0)
                    AddFull(Path.Combine(extraDest, folderName));
            }
        }

        foreach (var c in targets)
        {
            AddDestination(BuildFullOutputPath(c), c.ExtraFolders);
            foreach (var name in c.RedistributeTo ?? new List<string>())
            {
                var t = _configs.FirstOrDefault(x =>
                    string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                if (t == null) continue;
                // Redistribute copies THIS config's artifacts + extra folders into the target.
                AddDestination(BuildFullOutputPath(t), c.ExtraFolders);
            }
        }

        if (folders.Count == 0) return;
        AppendLog($"\n🧹 Cleaning {folders.Count} destination folder(s)...");
        foreach (var folder in folders)
        {
            if (!Directory.Exists(folder)) continue;
            try
            {
                var n = BuildHelper.ClearDirectoryContents(folder);
                AppendLog($"  ✔ Cleaned {n} item(s): {folder}");
            }
            catch (Exception ex)
            {
                AppendLog($"  ⚠ Could not clean {folder}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Zips the whole Output Base folder into "&lt;folderName&gt;.zip" placed inside it.
    /// Called after a successful build run when "Zip output" is enabled.
    /// </summary>
    private void ZipOutputBase()
    {
        if (string.IsNullOrWhiteSpace(_outputBasePath))
        {
            AppendLog($"\n  ⚠ Zip skipped — Output Base is empty.");
            return;
        }
        if (!Directory.Exists(_outputBasePath))
        {
            AppendLog($"\n  ⚠ Zip skipped — Output Base not found: {_outputBasePath}");
            return;
        }
        AppendLog($"\n  Zipping output base: {_outputBasePath}");
        try
        {
            BuildHelper.ZipOutputFolder(_outputBasePath, msg => AppendLog(msg));
        }
        catch (Exception ex)
        {
            AppendLog($"  ✘ Zip failed: {ex.Message}");
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private List<BuildConfig> GetCheckedConfigs()
    {
        var result = new List<BuildConfig>();
        for (int i = 0; i < _configs.Count && i < clbConfigs.Items.Count; i++)
            if (clbConfigs.GetItemChecked(i)) result.Add(_configs[i]);
        return result;
    }

    private List<BuildConfig> GetCheckedOrSelectedConfigs()
    {
        var checkedList = GetCheckedConfigs();
        if (checkedList.Count > 0) return checkedList;
        if (_selectedIndex >= 0 && _selectedIndex < _configs.Count)
            return new List<BuildConfig> { _configs[_selectedIndex] };
        return new List<BuildConfig>();
    }

    private void SetBusy(bool busy, string? msg = null)
    {
        _isBusy = busy;
        AppBusyState.IsBusy = busy;
        progressBar.Style = busy ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
        progressBar.Visible = busy;
        btnAdd.Enabled = !busy;
        btnDelete.Enabled = !busy;
        btnMoveUp.Enabled = !busy;
        btnMoveDown.Enabled = !busy;
        btnPreview.Enabled = !busy;
        btnBuildChecked.Enabled = !busy;
        btnBuildAll.Enabled = !busy;
        chkZipOutput.Enabled = !busy;
        chkCleanFirst.Enabled = !busy;
        clbConfigs.Enabled = !busy;
        UpdateDetailEnabled();
        if (msg != null) SetStatus(msg, Color.DarkGray);
    }

    private void SetStatus(string text, Color color)
    {
        if (InvokeRequired) { Invoke(() => SetStatus(text, color)); return; }
        lblStatus.Text = text;
        lblStatus.ForeColor = color;
    }

    private void AppendLog(string text)
    {
        if (InvokeRequired) { Invoke(() => AppendLog(text)); return; }
        txtLog.AppendText(text + Environment.NewLine);
        txtLog.SelectionStart = txtLog.Text.Length;
        txtLog.ScrollToCaret();
    }

    private void ClearLog()
    {
        if (InvokeRequired) { Invoke(ClearLog); return; }
        txtLog.Clear();
    }
}
