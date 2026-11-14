using System;
using System.Drawing;
using System.Windows.Forms;

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
        bool isRtfMode = IsRtfModeGetter?.Invoke() ?? false;

        if (this.SelectionLength > 0)
        {
            int selstart = this.SelectionStart;
            int sellen = this.SelectionLength;

            if (!isRtfMode)
            {
                this.SelectedText = shiftPressed ? this.SelectedText.ToUpper() : this.SelectedText.ToLower();
            }
            else
            {
                int end = selstart + sellen;
                for (int i = selstart; i < end; i++)
                {
                    this.SelectionStart = i;
                    this.SelectionLength = 1;

                    char c = this.Text[i];
                    char newC = shiftPressed ? char.ToUpper(c) : char.ToLower(c);
                    if (char.IsLetter(newC)) this.SelectedText = newC.ToString();
                }                
            }
            this.Select(selstart, sellen); // restore selection
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
