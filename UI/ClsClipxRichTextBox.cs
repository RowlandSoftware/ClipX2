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

    /************ Marker: a highlight painted over the text, not part of it ************/

    // Used by the spelling panel to show the word in question. It is drawn after RichEdit has
    // painted, so it never touches the text or its formatting: nothing goes into Undo or into
    // what Done exports, and there is nothing to remove before a replacement. Any change to the
    // text clears it (its position would be stale).

    // Quick search's Find all marks every match the same way: several markers, in text order.
    private readonly List<(int Start, int Length)> _marks = new List<(int Start, int Length)>();

    /// <summary>The marker's colour; translucent, so the text shows through (a highlighter).</summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color MarkerColor { get; set; } = Color.FromArgb(120, 255, 210, 0);

    /// <summary>Highlights the given characters until <see cref="ClearMarker"/> or the text changes.</summary>
    public void SetMarker(int start, int length) => SetMarkers(new[] { (start, length) });

    /// <summary>Highlights several stretches of text (in text order), replacing any marker.</summary>
    public void SetMarkers(IEnumerable<(int Start, int Length)> marks)
    {
        _marks.Clear();
        foreach (var m in marks)
            if (m.Start >= 0 && m.Length > 0) _marks.Add(m);
        Invalidate();
    }

    public void ClearMarker()
    {
        if (_marks.Count == 0) return;
        _marks.Clear();
        Invalidate();
    }

    [Browsable(false)]
    public bool HasMarker => _marks.Count > 0;

    private const int WM_PAINT = 0x000F;

    private const int WM_HSCROLL = 0x0114, WM_VSCROLL = 0x0115, WM_MOUSEWHEEL = 0x020A, WM_MOUSEHWHEEL = 0x020E;

    protected override void OnVScroll(EventArgs e) { base.OnVScroll(e); if (HasMarker) Invalidate(); }
    protected override void OnHScroll(EventArgs e) { base.OnHScroll(e); if (HasMarker) Invalidate(); }

    protected override void WndProc(ref Message m)
    {
        if (HasMarker && (m.Msg == WM_VSCROLL || m.Msg == WM_HSCROLL || m.Msg == WM_MOUSEWHEEL || m.Msg == WM_MOUSEHWHEEL))
        {
            base.WndProc(ref m);
            Invalidate(); // see the note below on scrolling
            return;
        }
        if (m.Msg != WM_PAINT || !HasMarker)
        {
            base.WndProc(ref m);
            return;
        }
        // Paint over only what RichEdit repaints, so the translucent colour is never laid twice
        IntPtr updateRgn = CreateRectRgn(0, 0, 0, 0);
        try
        {
            bool any = GetUpdateRgn(Handle, updateRgn, false) > 1; // > NULLREGION
            base.WndProc(ref m);
            if (any) PaintMarker(updateRgn);
        }
        finally { DeleteObject(updateRgn); }

        // Scrolling shifts the pixels already on screen, marker included, and repaints only the
        // strip uncovered, leaving stray copies of the marker. So after any scroll (bar, wheel,
        // smooth scrolling, keys, ScrollToCaret), repaint the whole box once.
        Point scroll = GetScrollPos();
        if (scroll != _markerScroll)
        {
            _markerScroll = scroll;
            Invalidate();
        }
    }

    private Point _markerScroll = new Point(int.MinValue, int.MinValue);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X, Y; }

    private const int EM_GETSCROLLPOS = 0x0400 + 221;

    [DllImport("user32.dll", EntryPoint = "SendMessage")]
    private static extern IntPtr SendMessagePoint(IntPtr hWnd, int msg, IntPtr wParam, ref POINT lParam);

    private Point GetScrollPos()
    {
        var p = new POINT();
        SendMessagePoint(Handle, EM_GETSCROLLPOS, IntPtr.Zero, ref p);
        return new Point(p.X, p.Y);
    }

    private void PaintMarker(IntPtr updateRgn)
    {
        List<Rectangle> rects = MarkerRectangles();
        if (rects.Count == 0) return;
        using (Graphics g = CreateGraphics())
        using (Region clip = Region.FromHrgn(updateRgn))
        using (var brush = new SolidBrush(MarkerColor))
        {
            g.Clip = clip;
            foreach (Rectangle r in rects) g.FillRectangle(brush, r);
        }
    }

    /// <summary>The marker's boxes in client coordinates, one per line it is on (usually one).</summary>
    private List<Rectangle> MarkerRectangles()
    {
        var rects = new List<Rectangle>();
        ITextDocument doc = null;
        try { doc = RichEditTom.GetDocument(this); } catch (Exception) { /* TOM not available */ }
        if (doc == null) return rects;
        try
        {
            // Only the characters on screen (Find all may mark thousands): from the first visible
            // character to the end of the last visible line
            int length = TextLength;
            int firstVisible = GetCharIndexFromPosition(Point.Empty);
            int lastLine = GetLineFromCharIndex(GetCharIndexFromPosition(new Point(ClientSize.Width - 1, ClientSize.Height - 1)));
            int afterVisible = GetFirstCharIndexFromLine(lastLine + 1);
            if (afterVisible < 0) afterVisible = length;
            foreach (var mark in _marks)
            {
                int end = Math.Min(Math.Min(mark.Start + mark.Length, length), afterVisible);
                for (int k = Math.Max(mark.Start, firstVisible); k < end; k++)
                {
                    ITextRange ch = doc.Range(k, k + 1);
                    try
                    {
                        // Only points actually on screen (S_OK); otherwise the coordinates are junk
                        if (ch.GetPoint(RichEditTom.tomStart | RichEditTom.TA_TOP | RichEditTom.TA_LEFT, out int x1, out int y1) != 0) continue;
                        if (ch.GetPoint(RichEditTom.tomEnd | RichEditTom.TA_BOTTOM | RichEditTom.TA_LEFT, out int x2, out int y2) != 0) continue;
                        Point a = PointToClient(new Point(x1, y1)), b = PointToClient(new Point(x2, y2));
                        if (b.X <= a.X || b.Y <= a.Y) continue; // end wrapped to the next line: skip this character
                        var r = Rectangle.FromLTRB(a.X, a.Y, b.X, b.Y);
                        if (r.Width > ClientSize.Width / 2 || !ClientRectangle.IntersectsWith(r)) continue; // not plausible / not visible
                        int last = rects.Count - 1;
                        if (last >= 0 && rects[last].Top == r.Top && rects[last].Bottom == r.Bottom && Math.Abs(rects[last].Right - r.Left) <= 1)
                            rects[last] = Rectangle.Union(rects[last], r); // same line: one box
                        else
                            rects.Add(r);
                    }
                    catch (COMException) { /* off screen */ }
                    finally { Marshal.ReleaseComObject(ch); }
                }
            }
        }
        finally { Marshal.ReleaseComObject(doc); }
        return rects;
    }

    [DllImport("gdi32.dll")] private static extern IntPtr CreateRectRgn(int left, int top, int right, int bottom);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);
    [DllImport("user32.dll")] private static extern int GetUpdateRgn(IntPtr hWnd, IntPtr hRgn, bool bErase);

    protected override void OnTextChanged(EventArgs e)
    {
        ClearMarker(); // its position would be stale
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
                case Keys.T:
                    if (e.Shift || e.Alt) break;
                    Transpose();
                    goto Handle; // jump to shared
            }
        }
        else if (e.KeyCode == Keys.F3 && e.Shift && !e.Alt && SelectionLength > 0)
        {
            CycleCase(); // replaces RichEdit's own Shift+F3, which is misled by a part-selected first word
            goto Handle;
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

        ReplaceSelection(SelectedText, CaseTransformer.Transform(Text, selstart, sellength, mode));
    }

    /// <summary>
    /// Shift+F3: steps the selection through Sentence case → Init Caps → ALL CAPS → lower case. The current case is
    /// judged from the whole words in the selection: if it starts partway through a word, that
    /// fragment can't show Init Caps (its first letter is mid-word, so stays lower case), so it is
    /// judged from the next word on. Otherwise every press would see "lower" and repeat Init Caps.
    /// </summary>
    public void CycleCase()
    {
        int selstart = SelectionStart, sellength = SelectionLength;
        if (sellength == 0) return;

        string text = Text;
        string selected = text.Substring(selstart, sellength);
        int skip = PartWordLength(text, selstart, sellength);

        bool withinWord = skip == 0 && selstart > 0 && IsWordChar(text[selstart - 1]) && IsWordChar(text[selstart]);

        // The current state is the first in the cycle that the selection already matches
        // (forced Init Caps, so a stray capital doesn't stop the cycle)
        CaseCycleState[] cycle = SentenceCycle;
        var results = new string[cycle.Length];
        int current = -1;
        for (int i = 0; i < cycle.Length; i++)
        {
            results[i] = cycle[i] == CaseCycleState.SentenceCase
                ? SentenceCase(text, selstart, sellength, skip, withinWord)
                : CaseTransformer.Transform(text, selstart, sellength, ToMode(cycle[i]), keepCapitals: false);
            if (current < 0 && string.CompareOrdinal(results[i], skip, selected, skip, sellength - skip) == 0) current = i;
        }

        // The next state that changes something (one word: Sentence case and Init Caps are the
        // same); none matched (mixed case): Sentence case first
        for (int step = 1; step <= cycle.Length; step++)
        {
            string result = results[(current + step + cycle.Length) % cycle.Length];
            if (result != selected) { ReplaceSelection(selected, result); return; }
        }
    }

    /// <summary>
    /// Sentence case for Shift+F3: a capital on the selection's first whole word, whether or not it
    /// starts a sentence, and at sentence starts within it; everything else lower case. A part-word
    /// at the start stays lower case.
    /// </summary>
    private static string SentenceCase(string text, int start, int length, int skip, bool withinWord)
    {
        string lower = text.Substring(start, length).ToLowerInvariant();
        if (withinWord) return lower;
        char[] chars = CaseTransformer.Transform(text, start, length, CaseMode.Sentence).ToCharArray();
        for (int i = 0; i < skip; i++) chars[i] = lower[i];
        for (int i = skip; i < chars.Length; i++)
            if (char.IsLetter(chars[i])) { chars[i] = char.ToUpperInvariant(chars[i]); break; }
        return new string(chars);
    }

    private static CaseMode ToMode(CaseCycleState state) => state switch
    {
        CaseCycleState.SentenceCase => CaseMode.Sentence,
        CaseCycleState.InitCaps => CaseMode.InitCaps,
        CaseCycleState.AllCaps => CaseMode.Upper,
        _ => CaseMode.Lower,
    };

    /// <summary>
    /// How much of the selection is the tail of a word begun before it: up to the first character
    /// that isn't part of a word. 0 if the selection starts at a word start, or lies within a single
    /// word (then there is nothing else to judge by).
    /// </summary>
    private static int PartWordLength(string text, int start, int length)
    {
        if (start == 0 || !IsWordChar(text[start - 1]) || !IsWordChar(text[start])) return 0;
        int i = start;
        while (i < start + length && IsWordChar(text[i])) i++;
        return i < start + length ? i - start : 0;
    }

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '\'' || c == '’';

    /// <summary>Writes a same-length case change over the selection (keeping formatting in RTF mode) and reselects it.</summary>
    private void ReplaceSelection(string selected, string transformed)
    {
        int selstart = SelectionStart, sellength = SelectionLength;
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
    /// Ctrl+T (ClipX 1's doKBTranspose): swaps the characters either side of the caret and moves
    /// the caret one place right (ab|cd → acb|d), so repeated presses carry a character along.
    /// With a selection, its first character is the right-hand one, and the selection is
    /// cancelled. Each character keeps its own formatting, except that a letter swapped with a
    /// space takes the formatting of the word it joins; one Undo step. Does nothing at the
    /// start or end of the text, or next to a paragraph break (swapping those would merge or
    /// split paragraphs). A surrogate pair (e.g. an emoji) counts as one character.
    /// </summary>
    public void Transpose()
    {
        int pos = SelectionStart;
        int length = TextLength;
        if (pos <= 0 || pos >= length) return;

        // Read only the few characters needed (the text may be long): the pair, plus the
        // neighbours either side, for the formatting rule below
        int from = Math.Max(0, pos - 3), to = Math.Min(length, pos + 3);
        string near = GetTextRange(from, to);
        if (near == null) return;
        int p = pos - from;                  // the caret within 'near'
        if (p <= 0 || p >= near.Length) return;

        int rightLen = char.IsHighSurrogate(near[p]) && p + 1 < near.Length && char.IsLowSurrogate(near[p + 1]) ? 2 : 1;
        int leftLen = char.IsLowSurrogate(near[p - 1]) && p >= 2 && char.IsHighSurrogate(near[p - 2]) ? 2 : 1;
        if (IsParagraphBreak(near[p]) || IsParagraphBreak(near[p - 1])) return;
        int leftStart = pos - leftLen;

        // Formatting belongs to words: if a letter swaps with a space, it joins the word on its
        // far side, so it takes that word's formatting (thea *pple* → the *apple*;
        // *atet* he → *ate* the). The space keeps its own. -1: no change.
        bool rightIsWord = char.IsLetterOrDigit(near[p]), leftIsWord = char.IsLetterOrDigit(near[p - 1]);
        bool rightIsSpace = char.IsWhiteSpace(near[p]), leftIsSpace = char.IsWhiteSpace(near[p - 1]);
        int adoptAt = -1, adoptLen = 0, adoptFrom = -1;  // document positions after the swap
        string adoptText = null;
        if (rightIsWord && leftIsSpace)                  // the letter moves left, onto the end of the word before
        {
            int n = leftStart - 1 - from;
            if (n >= 0 && char.IsLetterOrDigit(near[n])) { adoptAt = leftStart; adoptLen = rightLen; adoptFrom = leftStart - 1; adoptText = near.Substring(p, rightLen); }
        }
        else if (leftIsWord && rightIsSpace)             // the letter moves right, onto the start of the word after
        {
            int n = pos + rightLen - from;
            if (n < near.Length && char.IsLetterOrDigit(near[n])) { adoptAt = leftStart + rightLen; adoptLen = leftLen; adoptFrom = pos + rightLen; adoptText = near.Substring(p - leftLen, leftLen); }
        }

        bool done = EditWithTom(doc =>
        {
            ITextRange right = doc.Range(pos, pos + rightLen);
            ITextRange target = doc.Range(leftStart, leftStart);
            try
            {
                target.FormattedText = right.FormattedText;            // a copy, with its formatting, before the left character
                ITextRange old = doc.Range(pos + rightLen, pos + 2 * rightLen); // the original, now shifted right
                try { old.Text = ""; }
                finally { Marshal.ReleaseComObject(old); }

                if (adoptAt >= 0)
                {
                    // Put a copy of the neighbour (with its formatting) in front of the letter,
                    // then replace copy + letter with the letter: replaced text takes the
                    // formatting of the first character it replaces
                    ITextRange word = doc.Range(adoptFrom, adoptFrom + 1);
                    ITextRange at = doc.Range(adoptAt, adoptAt);
                    try
                    {
                        at.FormattedText = word.FormattedText;
                        ITextRange both = doc.Range(adoptAt, adoptAt + 1 + adoptLen);
                        try { both.Text = adoptText; }
                        finally { Marshal.ReleaseComObject(both); }
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(at);
                        Marshal.ReleaseComObject(word);
                    }
                }
            }
            finally
            {
                Marshal.ReleaseComObject(target);
                Marshal.ReleaseComObject(right);
            }
        });

        if (!done) // TOM not available: plain swap (the moved character takes its neighbour's formatting)
        {
            Select(pos, rightLen);
            string moved = SelectedText;
            SelectedText = "";
            Select(leftStart, 0);
            SelectedText = moved;
        }
        Select(pos + rightLen, 0);
    }

    private static bool IsParagraphBreak(char c) => c == '\n' || c == '\r' || c == '\v' || c == '\f';

    /// <summary>The text from <paramref name="start"/> to <paramref name="end"/>, without fetching the whole text.</summary>
    private string GetTextRange(int start, int end)
    {
        ITextDocument doc = null;
        try { doc = RichEditTom.GetDocument(this); } catch (Exception) { /* TOM not available */ }
        if (doc == null)
        {
            string all = Text;
            return end <= all.Length ? all.Substring(start, end - start) : null;
        }
        ITextRange range = doc.Range(start, end);
        try { return range.Text?.Replace('\r', '\n'); }
        finally
        {
            Marshal.ReleaseComObject(range);
            Marshal.ReleaseComObject(doc);
        }
    }

    /// <summary>
    /// Runs a set of TOM edits with repainting and change notifications off, as one Undo step,
    /// then raises TextChanged once. Returns false (doing nothing) if TOM isn't available.
    /// </summary>
    internal bool EditDocument(Action<ITextDocument> edit) => EditWithTom(edit); // for tools outside the control (lists)

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
