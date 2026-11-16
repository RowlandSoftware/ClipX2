using System.Windows.Forms;
using ClipX2.TextEngine;

namespace ClipX2.UI
{
    /// <summary>
    /// Gives a plain TextBox (e.g. in a side panel) some of the editor's keyboard tools:
    /// Ctrl+L lower case, Ctrl+Shift+L upper case, Ctrl+S reverse case, Ctrl+T transpose. They
    /// behave as in the main editor: with a selection, the case keys act on it; otherwise on the
    /// character right of the caret, then move past it. Ctrl+T swaps the characters either side of the caret (with a selection, its first
    /// character and the one before) and moves one place right.
    /// </summary>
    internal static class TextBoxKeys
    {
        public static void Attach(TextBox box) => box.KeyDown += (s, e) =>
        {
            if (!e.Control || e.Alt) return;
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

        private static void ChangeCase(TextBox box, CaseMode mode)
        {
            int start = box.SelectionStart, length = box.SelectionLength;
            if (length > 0)
            {
                box.SelectedText = CaseTransformer.Transform(box.SelectedText, mode);
                box.Select(start, length);
                return;
            }
            if (start >= box.TextLength) return;
            int n = char.IsHighSurrogate(box.Text[start]) && start + 1 < box.TextLength ? 2 : 1;
            box.Select(start, n);
            box.SelectedText = CaseTransformer.Transform(box.SelectedText, mode);
            box.Select(start + n, 0);
        }

        private static void Transpose(TextBox box)
        {
            string text = box.Text;
            int pos = box.SelectionStart;
            if (pos <= 0 || pos >= text.Length) return;
            int rightLen = char.IsHighSurrogate(text[pos]) && pos + 1 < text.Length && char.IsLowSurrogate(text[pos + 1]) ? 2 : 1;
            int leftLen = char.IsLowSurrogate(text[pos - 1]) && pos >= 2 && char.IsHighSurrogate(text[pos - 2]) ? 2 : 1;
            if (text[pos] == '\n' || text[pos] == '\r' || text[pos - 1] == '\n' || text[pos - 1] == '\r') return;
            int leftStart = pos - leftLen;
            box.Select(leftStart, leftLen + rightLen);
            box.SelectedText = text.Substring(pos, rightLen) + text.Substring(leftStart, leftLen); // SelectedText keeps Ctrl+Z working
            box.Select(pos + rightLen, 0);
        }
    }
}
