using System;
using System.Windows.Forms;
using ClipX2.TextEngine;

namespace ClipX2.UI
{
    /// <summary>
    /// Gives a plain TextBox or an editable ComboBox (e.g. in a side panel) some of the editor's
    /// keyboard tools: Ctrl+L lower case, Ctrl+Shift+L upper case, Ctrl+S reverse case, Ctrl+T
    /// transpose. They behave as in the main editor: with a selection, the case keys act on it;
    /// otherwise on the character right of the caret, then move past it. Ctrl+T swaps the
    /// characters either side of the caret (with a selection, its first character and the one
    /// before) and moves one place right.
    /// </summary>
    internal static class TextBoxKeys
    {
        public static void Attach(TextBox box) => Attach(box, new Box(
            () => box.Text, () => box.SelectionStart, () => box.SelectionLength,
            () => box.SelectedText, t => box.SelectedText = t, (s, l) => box.Select(s, l)));

        /// <summary>For an editable ComboBox (DropDown style), such as Find and Replace's.</summary>
        public static void Attach(ComboBox box) => Attach(box, new Box(
            () => box.Text, () => box.SelectionStart, () => box.SelectionLength,
            () => box.SelectedText, t => box.SelectedText = t, (s, l) => box.Select(s, l)));

        private sealed record Box(Func<string> Text, Func<int> Start, Func<int> Length,
            Func<string> Selected, Action<string> Replace, Action<int, int> Select);

        private static void Attach(Control control, Box box) => control.KeyDown += (s, e) =>
        {
            if (!e.Control || e.Alt) return;
            if (control is ComboBox { DropDownStyle: ComboBoxStyle.DropDownList }) return;
            switch (e.KeyCode)
            {
                case Keys.L: ChangeCase(box, e.Shift ? CaseMode.Upper : CaseMode.Lower); break;
                case Keys.S when !e.Shift: ChangeCase(box, CaseMode.Reverse); break;
                case Keys.T when !e.Shift: Transpose(box); break;
                default: return;
            }
            e.Handled = true;
            e.SuppressKeyPress = true;
        };

        private static void ChangeCase(Box box, CaseMode mode)
        {
            string text = box.Text();
            int start = box.Start(), length = box.Length();
            if (length > 0)
            {
                box.Replace(CaseTransformer.Transform(box.Selected(), mode));
                box.Select(start, length);
                return;
            }
            if (start >= text.Length) return;
            int n = char.IsHighSurrogate(text[start]) && start + 1 < text.Length ? 2 : 1;
            box.Select(start, n);
            box.Replace(CaseTransformer.Transform(box.Selected(), mode));
            box.Select(start + n, 0);
        }

        private static void Transpose(Box box)
        {
            string text = box.Text();
            int pos = box.Start();
            if (pos <= 0 || pos >= text.Length) return;
            int rightLen = char.IsHighSurrogate(text[pos]) && pos + 1 < text.Length && char.IsLowSurrogate(text[pos + 1]) ? 2 : 1;
            int leftLen = char.IsLowSurrogate(text[pos - 1]) && pos >= 2 && char.IsHighSurrogate(text[pos - 2]) ? 2 : 1;
            if (text[pos] == '\n' || text[pos] == '\r' || text[pos - 1] == '\n' || text[pos - 1] == '\r') return;
            int leftStart = pos - leftLen;
            box.Select(leftStart, leftLen + rightLen);
            box.Replace(text.Substring(pos, rightLen) + text.Substring(leftStart, leftLen)); // replacing the selection keeps Ctrl+Z working
            box.Select(pos + rightLen, 0);
        }
    }
}
