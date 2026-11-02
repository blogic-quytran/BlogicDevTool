namespace BLogicDevTool;

/// <summary>Searchable git-branch drop-down that fills a TextBox. Branches come from the
/// repos returned by <c>repos()</c>, newest commit first; "Fetch" runs git fetch --all --prune.</summary>
internal sealed class GitBranchPicker : IDisposable
{
    private sealed record Item(string Name, long Time, int RepoCount);

    private readonly TextBox _target;
    private readonly Func<IReadOnlyList<string>> _repos;
    private readonly string[] _pinned;
    private readonly ToolStripDropDown _drop;
    private readonly TextBox _search;
    private readonly Button _fetch;
    private readonly ListBox _list;
    private readonly Label _status;
    private readonly Font _smallFont = new("Segoe UI", 8F);
    private List<Item> _all = new();
    private int _repoTotal;
    private bool _busy;
    private long _closedAt;

    public GitBranchPicker(TextBox target, Func<IReadOnlyList<string>> repos, params string[] pinned)
    {
        _target = target;
        _repos = repos;
        _pinned = pinned;

        _search = new TextBox { PlaceholderText = "Search branch… (space = AND)", Location = new Point(6, 6), Width = 392 };
        _fetch = new Button { Text = "⟳ Fetch", Location = new Point(402, 4), Size = new Size(72, 26), UseVisualStyleBackColor = true };
        _list = new ListBox
        {
            Location = new Point(6, 34),
            Size = new Size(468, 300),
            IntegralHeight = false,
            DrawMode = DrawMode.OwnerDrawFixed,
            ItemHeight = 20,
            BorderStyle = BorderStyle.FixedSingle
        };
        _status = new Label { Location = new Point(6, 338), Size = new Size(468, 18), ForeColor = Color.DimGray, Font = _smallFont };

        var panel = new Panel { Size = new Size(480, 360), BackColor = Color.White };
        panel.Controls.AddRange(new Control[] { _search, _fetch, _list, _status });

        var host = new ToolStripControlHost(panel) { AutoSize = false, Padding = Padding.Empty, Margin = Padding.Empty, Size = panel.Size };
        _drop = new ToolStripDropDown { Padding = Padding.Empty, AutoSize = true };
        _drop.Items.Add(host);
        _drop.Closed += (_, _) => _closedAt = Environment.TickCount64;

        _search.TextChanged += (_, _) => ApplyFilter();
        _search.PreviewKeyDown += (_, e) => e.IsInputKey = e.KeyCode is Keys.Up or Keys.Down or Keys.Enter or Keys.Escape;
        _search.KeyDown += Search_KeyDown;
        _list.PreviewKeyDown += (_, e) => e.IsInputKey = e.KeyCode is Keys.Enter or Keys.Escape;
        _list.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Pick(); }
            else if (e.KeyCode == Keys.Escape) { e.SuppressKeyPress = true; _drop.Close(); }
        };
        _list.MouseClick += (_, e) => { if (_list.IndexFromPoint(e.Location) >= 0) Pick(); };
        _list.DrawItem += List_DrawItem;
        _fetch.Click += async (_, _) => await LoadAsync(fetchFirst: true);
    }

    /// <summary>Opens the drop-down under the target (or closes it when it was just open).</summary>
    public async void Toggle()
    {
        // Clicking the ▼ button while open first auto-closes the drop-down — don't reopen it.
        if (Environment.TickCount64 - _closedAt < 250) return;
        _search.Text = "";
        ApplyFilter();
        _drop.Show(_target, 0, _target.Height);
        _search.Focus();
        await LoadAsync(fetchFirst: false);
    }

    private void Search_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Down:
                if (_list.Items.Count > 0) _list.SelectedIndex = Math.Min(_list.SelectedIndex + 1, _list.Items.Count - 1);
                e.Handled = true;
                break;
            case Keys.Up:
                if (_list.Items.Count > 0) _list.SelectedIndex = Math.Max(_list.SelectedIndex - 1, 0);
                e.Handled = true;
                break;
            case Keys.Enter:
                e.SuppressKeyPress = true;
                Pick();
                break;
            case Keys.Escape:
                e.SuppressKeyPress = true;
                _drop.Close();
                break;
        }
    }

    private void Pick()
    {
        var item = _list.SelectedItem as Item ?? (_list.Items.Count > 0 ? _list.Items[0] as Item : null);
        if (item == null) return;
        _drop.Close();
        _target.Text = item.Name;
        _target.Focus();
        _target.SelectionStart = _target.TextLength;
    }

    private async Task LoadAsync(bool fetchFirst)
    {
        if (_busy) return;
        var repos = _repos();
        if (repos.Count == 0)
        {
            _status.Text = "No git repo found — check the configs' solution paths.";
            return;
        }

        _busy = true;
        _fetch.Enabled = false;
        try
        {
            if (fetchFirst)
            {
                var failed = new List<string>();
                for (int i = 0; i < repos.Count; i++)
                {
                    _status.Text = $"Fetching {Path.GetFileName(repos[i])} ({i + 1}/{repos.Count})…";
                    var (ok, _) = await GitDiffHelper.FetchAllAsync(repos[i]);
                    if (!ok) failed.Add(Path.GetFileName(repos[i]));
                }
                if (failed.Count > 0)
                    MessageBox.Show($"git fetch failed for: {string.Join(", ", failed)}", "Fetch",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (_all.Count == 0)
            {
                _status.Text = "Loading branches…";
            }

            var merged = new Dictionary<string, Item>(StringComparer.OrdinalIgnoreCase);
            var errors = new List<string>();
            foreach (var repo in repos)
            {
                try
                {
                    foreach (var (name, time) in await GitDiffHelper.ListBranchesAsync(repo))
                    {
                        merged[name] = merged.TryGetValue(name, out var cur)
                            ? cur with { Time = Math.Max(cur.Time, time), RepoCount = cur.RepoCount + 1 }
                            : new Item(name, time, 1);
                    }
                }
                catch (Exception ex) { errors.Add($"{Path.GetFileName(repo)}: {ex.Message}"); }
            }

            if (_drop.IsDisposed) return;
            _repoTotal = repos.Count;
            _all = merged.Values.OrderByDescending(i => i.Time).ThenBy(i => i.Name).ToList();
            _busy = false;
            ApplyFilter();
            if (errors.Count > 0) _status.Text = "⚠ " + string.Join(" | ", errors);
        }
        finally
        {
            _busy = false;
            if (!_fetch.IsDisposed) _fetch.Enabled = true;
        }
    }

    private void ApplyFilter()
    {
        var tokens = _search.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        bool Match(string name) => tokens.All(t => name.Contains(t, StringComparison.OrdinalIgnoreCase));

        var items = _pinned.Select(p => new Item(p, 0, -1))
            .Concat(_all)
            .Where(i => Match(i.Name))
            .ToList();

        _list.BeginUpdate();
        _list.Items.Clear();
        foreach (var i in items) _list.Items.Add(i);
        _list.EndUpdate();

        if (_list.Items.Count > 0)
        {
            var current = _target.Text.Trim();
            int idx = tokens.Length == 0 ? items.FindIndex(i => string.Equals(i.Name, current, StringComparison.OrdinalIgnoreCase)) : -1;
            _list.SelectedIndex = idx >= 0 ? idx : 0;
        }

        // While loading/fetching the status line shows progress instead.
        if (!_busy)
            _status.Text = $"{items.Count} / {_all.Count + _pinned.Length} branches · {_repoTotal} repo(s) · Enter = select, Esc = close";
    }

    private void List_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || _list.Items[e.Index] is not Item item) return;
        e.DrawBackground();
        bool selected = (e.State & DrawItemState.Selected) != 0;
        var fore = selected ? SystemColors.HighlightText : _list.ForeColor;
        var dim = selected ? SystemColors.HighlightText : Color.Gray;

        string right = item.RepoCount < 0
            ? "(current checkout)"
            : (item.Time > 0 ? DateTimeOffset.FromUnixTimeSeconds(item.Time).LocalDateTime.ToString("yyyy-MM-dd") : "")
              + (_repoTotal > 1 && item.RepoCount < _repoTotal ? $"  ⚠ {item.RepoCount}/{_repoTotal} repos" : "");
        var rightSize = TextRenderer.MeasureText(right, _smallFont);
        var rightRect = new Rectangle(e.Bounds.Right - rightSize.Width - 4, e.Bounds.Top, rightSize.Width + 4, e.Bounds.Height);
        var nameRect = new Rectangle(e.Bounds.Left + 4, e.Bounds.Top, e.Bounds.Width - rightRect.Width - 8, e.Bounds.Height);

        TextRenderer.DrawText(e.Graphics, item.Name, _list.Font, nameRect, fore,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(e.Graphics, right, _smallFont, rightRect, dim,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        e.DrawFocusRectangle();
    }

    public void Dispose()
    {
        _drop.Dispose();
        _smallFont.Dispose();
    }
}
