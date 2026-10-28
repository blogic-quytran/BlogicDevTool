using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace BLogicDevTool;

/// <summary>Coloured log sink for a RichTextBox. Appending one line at a time cost ~14 ms
/// per line and, because MSBuild's stdout reader thread was the caller, throttled the build
/// itself; producers now only enqueue and the UI drains in batches.</summary>
public sealed class RichLogBuffer : IDisposable
{
    private readonly record struct Entry(string Text, Color Color, Font? Font);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    private const int WM_SETREDRAW = 0x000B;

    private readonly RichTextBox _box;
    private readonly Func<string, Color> _colorOf;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly ConcurrentQueue<Entry> _queue = new();
    private int _flushing;

    /// <summary>Line count at which the oldest <see cref="TrimLines"/> lines are dropped.</summary>
    public int MaxLines { get; init; } = 6000;
    public int TrimLines { get; init; } = 1500;

    public RichLogBuffer(RichTextBox box, Func<string, Color> colorOf, int flushMs = 120)
    {
        _box = box;
        _colorOf = colorOf;
        _timer = new System.Windows.Forms.Timer { Interval = flushMs };
        _timer.Tick += (_, _) => Flush();
        _timer.Start();
    }

    /// <summary>Queues a line, colour picked by the owner's rules. Safe from any thread.</summary>
    public void Append(string text) => _queue.Enqueue(new Entry(text, _colorOf(text), null));

    /// <summary>Queues text with an explicit colour/font, for banners. Safe from any thread.</summary>
    public void Append(string text, Color color, Font? font = null)
        => _queue.Enqueue(new Entry(text, color, font));

    public void Clear()
    {
        while (_queue.TryDequeue(out _)) { }
        if (!Usable()) return;
        if (_box.InvokeRequired) { _box.Invoke(() => _box.Clear()); return; }
        _box.Clear();
    }

    /// <summary>Writes everything queued right now. Call it before a banner or before reading
    /// the box, otherwise the pending lines would land after it.</summary>
    public void Flush()
    {
        if (_queue.IsEmpty || !Usable()) return;
        if (_box.InvokeRequired)
        {
            try { _box.Invoke(Flush); }
            catch (ObjectDisposedException) { /* form closed while a flush was in flight */ }
            return;
        }

        // A nested Flush (a Tick landing inside an explicit one) would interleave entries.
        if (Interlocked.Exchange(ref _flushing, 1) == 1) return;
        try
        {
            SendMessage(_box.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            try
            {
                TrimIfHuge();

                // Consecutive entries sharing colour and font go in as ONE AppendText:
                // one append per line is quadratic (20k lines took 49 s that way, 1.1 s this way).
                var run = new System.Text.StringBuilder();
                var runColor = Color.Empty;
                Font? runFont = null;
                var open = false;

                while (_queue.TryDequeue(out var e))
                {
                    if (open && (e.Color.ToArgb() != runColor.ToArgb() || !Equals(e.Font, runFont)))
                        Write(run, runColor, runFont);
                    runColor = e.Color;
                    runFont = e.Font;
                    open = true;
                    run.Append(e.Text);
                }
                if (open) Write(run, runColor, runFont);

                TrimIfHuge();   // also after, or a single big batch would blow past the cap
            }
            finally
            {
                SendMessage(_box.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
                _box.Invalidate();
                _box.SelectionStart = _box.TextLength;
                _box.ScrollToCaret();
            }
        }
        finally { _flushing = 0; }
    }

    private void Write(System.Text.StringBuilder run, Color color, Font? font)
    {
        if (run.Length == 0) return;
        _box.SelectionStart = _box.TextLength;
        _box.SelectionLength = 0;
        _box.SelectionColor = color;
        if (font != null) _box.SelectionFont = font;
        _box.AppendText(run.ToString());
        _box.SelectionColor = _box.ForeColor;
        if (font != null) _box.SelectionFont = _box.Font;
        run.Clear();
    }

    private void TrimIfHuge()
    {
        var lines = _box.GetLineFromCharIndex(_box.TextLength);
        if (lines <= MaxLines) return;

        // Cut down to the cap in one go: dropping a fixed number of lines would leave a
        // single huge batch far above it, and a big buffer is what makes appending slow.
        var keep = Math.Max(1, MaxLines - TrimLines);
        var cut = _box.GetFirstCharIndexFromLine(Math.Max(0, lines - keep));
        if (cut <= 0) return;

        var wasReadOnly = _box.ReadOnly;
        _box.ReadOnly = false;
        _box.SelectionStart = 0;
        _box.SelectionLength = cut;
        _box.SelectedText = "";
        _box.ReadOnly = wasReadOnly;
    }

    private bool Usable() => !_box.IsDisposed && !_box.Disposing && _box.IsHandleCreated;

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}
