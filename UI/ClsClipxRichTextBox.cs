using System;
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

    private bool _mouseDown;
    private int _mouseDownCharIndex;
    public Func<bool> IsRtfModeGetter { get; set; } // delegate to query Form1

    /// <summary>
    /// If true, use normal RichTextBox word-selection behaviour.
    /// If false, dragging selects characters exactly (ClipX1 style).
    /// </summary>
    public bool PreciseSelection { get; set; } = true;

    /// <summary>
    /// Mirrors the built-in AutoWordSelection, but only applies
    /// when PreciseSelection is false.
    /// </summary>
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
                    UpperLower(e.Shift);
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
        if (!isRtfMode)
            SelectedText = transformed;
        else
            ReplaceKeepingFormat(selstart, selected, transformed);

        Select(selstart, sellength);
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

        ITextDocument doc = null;
        try { doc = RichEditTom.GetDocument(this); } catch (Exception) { /* TOM not available */ }

        if (doc == null)
        {
            ReplaceCharByChar(start, original, replacement);
            return;
        }

        int oldMask = SendMessage(Handle, EM_SETEVENTMASK, IntPtr.Zero, IntPtr.Zero); // stop change/selection notifications
        doc.Freeze(); // no repainting until we've finished
        bool grouped = TryBeginUndoGroup(doc);
        try
        {
            ReplaceWithinFormatRuns(doc, start, original, replacement);
        }
        finally
        {
            if (grouped) doc.EndEditCollection();
            doc.Unfreeze();
            SendMessage(Handle, EM_SETEVENTMASK, IntPtr.Zero, (IntPtr)oldMask);
            Marshal.ReleaseComObject(doc);
        }
        OnTextChanged(EventArgs.Empty); // one notification for the whole change
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
    private void UpperLower(bool shiftPressed)
    {
        if (this.SelectionLength > 0)
        {
            ApplyCase(shiftPressed ? CaseMode.Upper : CaseMode.Lower); // keeps formatting in RTF mode and restores the selection
        }
        else
        {
            int selstart = this.SelectionStart;
            if (selstart == this.TextLength) return; // at end of text

            this.SelectionLength = 1;
            this.SelectedText = shiftPressed ? this.SelectedText.ToUpper() : this.SelectedText.ToLower();
            this.Select(selstart+1, 0); // restore selection
        }
    }
}
