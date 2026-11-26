using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ClipX2.UI;

namespace ClipX2
{
    /// <summary>
    /// Undo and Redo (the ↶ ↷ buttons between Refresh and Finish; Ctrl+Z, Ctrl+Y anywhere but
    /// another text field). Two layers:
    ///
    /// The text box's own history: typing (as insertions and deletions) and every tool (one
    /// step each: a tool's edits go in as one), in the order they happened, up to
    /// <see cref="ClipXRichTextBox.UndoLimit"/> steps. Each step is the change only, so long
    /// texts cost little.
    ///
    /// Switches between plain and rich text (and a Refresh that changes mode): the text is
    /// replaced, which the text box can't keep in its history, and the mode is ClipX's, not the
    /// text box's. So at each switch ClipX keeps the text as it was (with its formatting, the
    /// selection and the mode) and the text box starts a new history. When Undo has gone back to
    /// the switch, the next Undo puts that back, mode and all; Redo goes forward over it again.
    /// The finer history from before a switch isn't kept: Undo goes back to just before it, then
    /// to just before the switch before that. The text as loaded is the bottom.
    /// </summary>
    public partial class Form1
    {
        /// <summary>The text, its formatting, selection and mode at a switch.</summary>
        private sealed class ModeSnapshot
        {
            public bool Rich;
            public string Rtf;   // rich text
            public string Text;  // plain text
            public int Start, Length;
            public TextEngine.Import.RtfStyleSheet SourceStyles;
        }

        /// <summary>Switches kept to undo (each a copy of the whole text).</summary>
        private const int SwitchesKept = 20;

        private readonly List<ModeSnapshot> _beforeSwitches = new List<ModeSnapshot>(); // Undo: the text before each switch, last on top
        private readonly List<ModeSnapshot> _afterSwitches = new List<ModeSnapshot>();  // Redo: the text after a switch undone
        private bool _undoing; // the text is changing because of Undo or Redo, not an edit

        private void InitUndo()
        {
            toolStripButtonUndo.Click += (s, e) => UndoText();
            toolStripButtonRedo.Click += (s, e) => RedoText();
            // A new edit: what was undone across a switch can't be redone any more (as within the text box)
            richTextBox1.TextChanged += (s, e) => { if (!_undoing) _afterSwitches.Clear(); };
            Application.Idle += UpdateUndoButtons;
            FormClosed += (s, e) => Application.Idle -= UpdateUndoButtons;
        }

        private bool CanUndoText => richTextBox1.CanUndo || _beforeSwitches.Count > 0;
        private bool CanRedoText => richTextBox1.CanRedo || _afterSwitches.Count > 0;

        /// <summary>The buttons greyed out when there is nothing to undo or redo (checked when the window is idle).</summary>
        private void UpdateUndoButtons(object sender, EventArgs e)
        {
            if (IsDisposed) return;
            bool undo = CanUndoText, redo = CanRedoText;
            if (toolStripButtonUndo.Enabled != undo) toolStripButtonUndo.Enabled = undo;
            if (toolStripButtonRedo.Enabled != redo) toolStripButtonRedo.Enabled = redo;
        }

        private void UndoText()
        {
            _undoing = true;
            try
            {
                if (richTextBox1.CanUndo) richTextBox1.Undo();
                else if (_beforeSwitches.Count > 0)
                {
                    ModeSnapshot before = Pop(_beforeSwitches);
                    _afterSwitches.Add(TakeSnapshot());
                    Restore(before);
                    ShowStatus(before.Rich ? "Undone: back to rich text" : "Undone: back to plain text");
                }
                else ShowStatus("Nothing to undo");
            }
            finally { _undoing = false; }
            UpdateUndoButtons(this, EventArgs.Empty);
        }

        private void RedoText()
        {
            _undoing = true;
            try
            {
                if (richTextBox1.CanRedo) richTextBox1.Redo();
                else if (_afterSwitches.Count > 0)
                {
                    ModeSnapshot after = Pop(_afterSwitches);
                    Keep(TakeSnapshot());
                    Restore(after);
                    ShowStatus(after.Rich ? "Redone: rich text again" : "Redone: plain text again");
                }
                else ShowStatus("Nothing to redo");
            }
            finally { _undoing = false; }
            UpdateUndoButtons(this, EventArgs.Empty);
        }

        /// <summary>
        /// Before the text is replaced by a switch of mode: what to keep for Undo. Call
        /// <see cref="SwitchDone"/> with it once the switch has been made.
        /// </summary>
        private ModeSnapshot BeforeSwitch() => TakeSnapshot();

        /// <summary>The switch has been made: its "before" goes on the Undo list, and the text box starts a new history.</summary>
        private void SwitchDone(ModeSnapshot before)
        {
            Keep(before);
            _afterSwitches.Clear();
            richTextBox1.ClearUndo();
        }

        private void Keep(ModeSnapshot s)
        {
            _beforeSwitches.Add(s);
            if (_beforeSwitches.Count > SwitchesKept) _beforeSwitches.RemoveAt(0); // the oldest goes
        }

        private static ModeSnapshot Pop(List<ModeSnapshot> list)
        {
            ModeSnapshot s = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return s;
        }

        private ModeSnapshot TakeSnapshot() => new ModeSnapshot
        {
            Rich = _isRtfMode,
            Rtf = _isRtfMode ? richTextBox1.Rtf : null,
            Text = _isRtfMode ? null : richTextBox1.Text,
            Start = richTextBox1.SelectionStart,
            Length = richTextBox1.SelectionLength,
            SourceStyles = richTextBox1.SourceStyles,
        };

        /// <summary>Puts a kept text back, in its mode, with its selection; the text box starts a new history.</summary>
        private void Restore(ModeSnapshot s)
        {
            using (richTextBox1.BeginBulkEdit())
            {
                _isRtfMode = s.Rich;
                if (s.Rich)
                {
                    richTextBox1.Rtf = s.Rtf;
                    RichEditWriter.BulletsByLevel(richTextBox1); // the editor's RTF reader drops ◦ and ▪
                }
                else SetPlainText(s.Text);
                richTextBox1.SourceStyles = s.SourceStyles;
                int start = Math.Min(s.Start, richTextBox1.TextLength);
                richTextBox1.Select(start, Math.Max(0, Math.Min(s.Length, richTextBox1.TextLength - start)));
            }
            richTextBox1.ClearUndo();
            toolStripLabelRtfMode.Text = _isRtfMode ? "Rich text" : "Plain text";
            ApplyDisplaySettings();
            _panels.Current?.OnModeChanged();
            ShowStyles();
            richTextBox1.ScrollToCaret();
        }

        /// <summary>
        /// Ctrl+Z, Ctrl+Y (or Ctrl+Shift+Z) for the text, wherever the focus is, except in another
        /// text field (the Find box, "What do you want to do?"…), which keeps its own Undo.
        /// </summary>
        private bool UndoKey(Keys keyData)
        {
            bool undo = keyData == (Keys.Control | Keys.Z);
            bool redo = keyData == (Keys.Control | Keys.Y) || keyData == (Keys.Control | Keys.Shift | Keys.Z);
            if (!undo && !redo) return false;
            Control focused = Control.FromChildHandle(GetFocus());
            bool otherTextField = focused != null && focused != richTextBox1
                && (focused is TextBoxBase || (focused is ComboBox c && c.DropDownStyle != ComboBoxStyle.DropDownList) || focused is UpDownBase);
            if (otherTextField) return false;
            if (undo) UndoText(); else RedoText();
            return true;
        }

        [DllImport("user32.dll")] private static extern IntPtr GetFocus();
    }
}
