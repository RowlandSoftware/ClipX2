using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ClipX2.TextEngine;
using ClipX2.UI;

public enum CaseCycleState
{
    SentenceCase,
    InitCaps,
    AllCaps,
    Lower
}

[Flags]
public enum FontStyleFlags
{
    None = 0,
    Italics = 1 << 0,
    Bold = 1 << 1,
    Underline = 1 << 2
}
public class ClipXRichTextBox : RichTextBox
{
    private static readonly CaseCycleState[] SentenceCycle = [
    CaseCycleState.SentenceCase,
    CaseCycleState.InitCaps,
    CaseCycleState.AllCaps,
    CaseCycleState.Lower ];

    private static readonly CaseCycleState[] PhraseCycle = [
    CaseCycleState.InitCaps,
    CaseCycleState.AllCaps,
    CaseCycleState.Lower ];

    /********** Settled-text notification (live word count etc.) **********/

    /// <summary>
    /// Raised once editing pauses (after <see cref="SettleDelay"/> ms with no change), or once at
    /// the end of a bulk edit. Use it for work that needn't run on every keystroke, such as the
    /// word count. It covers every kind of change: typing, deleting, pasting, drag and drop, undo,
    /// and tools.
    /// </summary>
    public event EventHandler TextSettled;

    /// <summary>
    /// Raised once the selection stops changing (after <see cref="SelectionSettleDelay"/> ms), e.g.
    /// when a mouse drag pauses, or at the end of a bulk edit. For the selection word count.
    /// </summary>
    public event EventHandler SelectionSettled;

    /// <summary>Pause in editing, in milliseconds, before <see cref="TextSettled"/> is raised.</summary>
    [DefaultValue(300)]
    public int SettleDelay
    {
        get => _textTimer.Interval;
        set => _textTimer.Interval = value;
    }

    /// <summary>Pause in selecting, in milliseconds, before <see cref="SelectionSettled"/> is raised.</summary>
    [DefaultValue(150)]
    public int SelectionSettleDelay
    {
        get => _selectionTimer.Interval;
        set => _selectionTimer.Interval = value;
    }

    private readonly Timer _textTimer = new Timer { Interval = 300 };
    private readonly Timer _selectionTimer = new Timer { Interval = 150 };
    private int _bulkEditDepth;
    private bool _textChangedDuringBulkEdit;
    private bool _selectionChangedDuringBulkEdit;

    public ClipXRichTextBox()
    {
        _textTimer.Tick += (s, e) => { _textTimer.Stop(); TextSettled?.Invoke(this, EventArgs.Empty); };
        _selectionTimer.Tick += (s, e) => { _selectionTimer.Stop(); SelectionSettled?.Invoke(this, EventArgs.Empty); };
    }

    /// <summary>
    /// Starts a bulk edit (a tool such as case change or global replace). Until the returned
    /// object is disposed, <see cref="TextSettled"/> and <see cref="SelectionSettled"/> are held
    /// back; each is then raised once if its text or selection changed. Nests safely.
    /// Usage: <c>using (textBox.BeginBulkEdit()) { ... }</c>
    /// </summary>
    public IDisposable BeginBulkEdit()
    {
        _bulkEditDepth++;
        return new BulkEdit(this);
    }

    private void EndBulkEdit()
    {
        if (--_bulkEditDepth > 0) return;

        bool text = _textChangedDuringBulkEdit, selection = _selectionChangedDuringBulkEdit;
        _textChangedDuringBulkEdit = _selectionChangedDuringBulkEdit = false;
        if (text) { _textTimer.Stop(); TextSettled?.Invoke(this, EventArgs.Empty); }
        if (selection) { _selectionTimer.Stop(); SelectionSettled?.Invoke(this, EventArgs.Empty); }
    }

    private sealed class BulkEdit : IDisposable
    {
        private ClipXRichTextBox _owner;
        public BulkEdit(ClipXRichTextBox owner) => _owner = owner;
        public void Dispose() { _owner?.EndBulkEdit(); _owner = null; } // only once
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        if (_bulkEditDepth > 0)
            _textChangedDuringBulkEdit = true;
        else
            Restart(_textTimer);
    }

    protected override void OnSelectionChanged(EventArgs e)
    {
        base.OnSelectionChanged(e);
        if (_bulkEditDepth > 0)
            _selectionChangedDuringBulkEdit = true;
        else
            Restart(_selectionTimer);
    }

    private static void Restart(Timer timer) { timer.Stop(); timer.Start(); }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _textTimer.Dispose(); _selectionTimer.Dispose(); }
        base.Dispose(disposing);
    }

    private bool _mouseDown;
    private int _mouseDownCharIndex;
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<bool> IsRtfModeGetter { get; set; } // delegate to query Form1

    /// <summary>
    /// If true, use normal RichTextBox word-selection behaviour.
    /// If false, dragging selects characters exactly (ClipX1 style).
    /// </summary>
    [DefaultValue(true)]
    public bool PreciseSelection { get; set; } = true;

    /// <summary>
    /// Mirrors the built-in AutoWordSelection, but only applies
    /// when PreciseSelection is false.
    /// </summary>
    [DefaultValue(false)]
    public bool AutoWordSelectionEnabled
    {
        get => this.AutoWordSelection;
        set => this.AutoWordSelection = value;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (PreciseSelection)
        {
            _mouseDown = true;
            _mouseDownCharIndex = this.GetCharIndexFromPosition(e.Location);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (PreciseSelection && _mouseDown)
        {
            int current = this.GetCharIndexFromPosition(e.Location);

            // Force character-accurate selection
            this.Select(
                Math.Min(_mouseDownCharIndex, current),
                Math.Abs(current - _mouseDownCharIndex)
            );
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _mouseDown = false;
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        bool isRtfMode = IsRtfModeGetter?.Invoke() ?? false;

        /**** Ctrl+ keypresses ***/
        if (e.Control)
        {
            switch (e.KeyCode)
            {
                case Keys.M:
                    if (isRtfMode) ToggleStyle(FontStyleFlags.None);
                    goto Handle; // jump to shared
                case Keys.I:
                    if (isRtfMode) ToggleStyle(FontStyleFlags.Italics);
                    goto Handle; // jump to shared
                case Keys.B:
                    if (isRtfMode) ToggleStyle(FontStyleFlags.Bold);
                    goto Handle; // jump to shared
                case Keys.U:
                    if (isRtfMode) ToggleStyle(FontStyleFlags.Underline);
                    goto Handle; // jump to shared
                case Keys.V:
                    HandlePaste(e.Shift);
                    goto Handle; // jump to shared
                case Keys.L:
                    KeyCase(e.Shift ? CaseMode.Upper : CaseMode.Lower);
                    goto Handle; // jump to shared
                case Keys.K:
                    KeyCase(CaseMode.InitCaps);
                    goto Handle; // jump to shared
                case Keys.S:
                    KeyCase(CaseMode.Reverse);
                    goto Handle; // jump to shared
            }
        }
        base.OnKeyDown(e);
        return; // no match - exit

        Handle:
        e.SuppressKeyPress = true;
        e.Handled = true;
        return;
    }

    /// <summary>
    /// Applies a case conversion from the text engine to the current selection.
    /// In RTF mode only the characters that change are replaced, so formatting is preserved.
    /// </summary>
    public void ApplyCase(CaseMode mode)
    {
        int selstart = SelectionStart;
        int sellength = SelectionLength;
        if (sellength == 0) return;

        string selected = SelectedText;
        string transformed = CaseTransformer.Transform(Text, selstart, sellength, mode);
        if (selected == transformed) return;

        bool isRtfMode = IsRtfModeGetter?.Invoke() ?? false;
        using (BeginBulkEdit())
        {
            if (!isRtfMode)
                SelectedText = transformed;
            else
                ReplaceKeepingFormat(selstart, selected, transformed);

            Select(selstart, sellength);
        }
    }

    /// <summary>
    /// Replaces text starting at <paramref name="start"/> with a same-length replacement, keeping
    /// all character formatting (bold, italic, font, colour...). Uses the RichEdit Text Object Model
    /// with repainting and change notifications off, and groups the edits into one Undo step.
    /// </summary>
    internal void ReplaceKeepingFormat(int start, string original, string replacement)
    {
        if (original.Length != replacement.Length)
            throw new ArgumentException("Replacement must be the same length as the original.");

        if (!EditWithTom(doc => ReplaceWithinFormatRuns(doc, start, original, replacement)))
            ReplaceCharByChar(start, original, replacement);
    }

    /// <summary>
    /// Applies a tool's edits (e.g. from Check Punctuation) in place, so the formatting of
    /// everything else is kept. An inserted space takes the formatting of the character before it,
    /// as when typing. The edits are positions in the current text, in order, not overlapping;
    /// they are applied from last to first so earlier positions stay valid. One Undo step.
    /// </summary>
    public void ApplyEdits(IReadOnlyList<TextEdit> edits)
    {
        if (edits == null || edits.Count == 0) return;

        using (BeginBulkEdit())
        {
            bool done = EditWithTom(doc =>
            {
                ITextRange range = doc.Range(0, 0);
                try
                {
                    for (int k = edits.Count - 1; k >= 0; k--)
                    {
                        range.SetRange(edits[k].Start, edits[k].Start + edits[k].Length);
                        range.Text = edits[k].Replacement;
                    }
                }
                finally { Marshal.ReleaseComObject(range); }
            });

            if (!done) // TOM not available
            {
                for (int k = edits.Count - 1; k >= 0; k--)
                {
                    Select(edits[k].Start, edits[k].Length);
                    SelectedText = edits[k].Replacement;
                }
            }
        }
    }

    /// <summary>
    /// Runs a set of TOM edits with repainting and change notifications off, as one Undo step,
    /// then raises TextChanged once. Returns false (doing nothing) if TOM isn't available.
    /// </summary>
    private bool EditWithTom(Action<ITextDocument> edit)
    {
        ITextDocument doc = null;
        try { doc = RichEditTom.GetDocument(this); } catch (Exception) { /* TOM not available */ }
        if (doc == null) return false;

        int oldMask = SendMessage(Handle, EM_SETEVENTMASK, IntPtr.Zero, IntPtr.Zero); // stop change/selection notifications
        doc.Freeze(); // no repainting until we've finished
        bool grouped = TryBeginUndoGroup(doc);
        try
        {
            edit(doc);
        }
        finally
        {
            if (grouped) doc.EndEditCollection();
            doc.Unfreeze();
            SendMessage(Handle, EM_SETEVENTMASK, IntPtr.Zero, (IntPtr)oldMask);
            Marshal.ReleaseComObject(doc);
        }
        OnTextChanged(EventArgs.Empty); // one notification for the whole change
        return true;
    }

    /// <summary>
    /// Makes one edit per formatting run that contains changes. Replaced text takes on the
    /// formatting of the first character it replaces, so an edit must never cross a change of
    /// formatting; within one formatting run, everything from the first to the last changed
    /// character is replaced in one go (unchanged characters in between are rewritten as they were).
    /// Each edit costs time in proportion to the document's size, so fewer edits is what matters.
    /// </summary>
    private static void ReplaceWithinFormatRuns(ITextDocument doc, int start, string original, string replacement)
    {
        ITextRange range = doc.Range(0, 0);
        try
        {
            int n = original.Length;
            int i = 0;
            while (i < n)
            {
                if (original[i] == replacement[i]) { i++; continue; }

                // i is the first changed character; find the end of its formatting run
                range.SetRange(start + i, start + i);
                range.MoveEnd(RichEditTom.tomCharFormat, 1);
                int runEnd = Math.Min(Math.Max(range.End - start, i + 1), n);

                // last changed character in this formatting run
                int last = runEnd - 1;
                while (original[last] == replacement[last]) last--;

                range.SetRange(start + i, start + last + 1);
                range.Text = replacement.Substring(i, last + 1 - i);
                i = runEnd;
            }
        }
        finally
        {
            Marshal.ReleaseComObject(range);
        }
    }

    /// <summary>Groups all the edits into a single Undo step where the RichEdit version supports it.</summary>
    private static bool TryBeginUndoGroup(ITextDocument doc)
    {
        try { doc.BeginEditCollection(); return true; }
        catch (Exception) { return false; } // E_NOTIMPL in older RichEdit versions
    }

    /// <summary>Fallback when TOM is unavailable: replace one character at a time.</summary>
    private void ReplaceCharByChar(int start, string original, string replacement)
    {
        for (int i = 0; i < original.Length; i++)
        {
            if (original[i] != replacement[i])
            {
                Select(start + i, 1);
                SelectedText = replacement[i].ToString();
            }
        }
    }

    private const int EM_SETEVENTMASK = 0x0400 + 69;

    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    private void ToggleStyle(FontStyleFlags flag)
    {
        var f = this.SelectionFont;
        if (f == null) return;
        System.Drawing.FontStyle newStyle;// = f.Style;
        newStyle = FontStyle.Regular;
        switch (flag)
        {
            case FontStyleFlags.None:
            newStyle = FontStyle.Regular; // TODO Set default font
                break;
            case FontStyleFlags.Italics:
                newStyle = f.Style ^ FontStyle.Italic;
                break;
            case FontStyleFlags.Bold:
                newStyle = f.Style ^ FontStyle.Bold;
                break;
             case FontStyleFlags.Underline:
                newStyle = f.Style ^ FontStyle.Underline;
                break;
        }
        this.SelectionFont = new Font(f, newStyle);
    }
    private void HandlePaste(bool forcePlain)
    {
        bool isRtfMode = IsRtfModeGetter?.Invoke() ?? false;

        if (!forcePlain && isRtfMode)
        {
            // Normal paste (RTF allowed)
            Paste();
            return;
        }

        // Plain mode → force plain text
        if (Clipboard.ContainsText(TextDataFormat.Text))
        {
            SelectedText = Clipboard.GetText(TextDataFormat.Text);
            return;
        }

        // If clipboard only has RTF, convert to plain text
        if (Clipboard.ContainsText(TextDataFormat.Rtf))
        {
            string plain = Clipboard.GetText(TextDataFormat.Rtf);
            SelectedText = plain;
            return;
        }

        // Fallback
        Paste();
    }
    /// <summary>
    /// Keyboard case change (Ctrl+L, Ctrl+Shift+L, Ctrl+K, Ctrl+S), as in ClipX 1.
    /// With a selection it acts on the selection, like the toolstrip buttons. Without one it
    /// changes the character to the right of the caret and moves the caret past it, so holding
    /// the key down works along the text. (Toolstrip buttons act on the whole text instead.)
    /// </summary>
    private void KeyCase(CaseMode mode)
    {
        if (this.SelectionLength > 0)
        {
            ApplyCase(mode); // keeps formatting in RTF mode and restores the selection
            return;
        }

        int selstart = this.SelectionStart;
        if (selstart >= this.TextLength) return; // at end of text

        this.Select(selstart, 1);
        string original = this.SelectedText;

        // Initial caps depends on the surrounding word (capital at the start of a word, lower case
        // inside it), so it needs the text around the caret; the other modes need only the character
        // (and reading Text copies the whole document)
        string changed = (mode == CaseMode.InitCaps || mode == CaseMode.Title)
            ? CaseTransformer.Transform(this.Text, selstart, 1, mode, keepCapitals: false) // forced, as in ClipX 1
            : CaseTransformer.Transform(original, mode);
        if (changed != original)
            this.SelectedText = changed; // a one-character replacement keeps that character's formatting
        this.Select(selstart + 1, 0);
    }
}
