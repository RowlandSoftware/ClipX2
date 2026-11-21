using System;
using System.Collections.Generic;
using System.Linq;
using ClipX2.TextEngine;
using Line = ClipX2.TextEngine.ListFormatter.Line;

namespace ClipX2.UI
{
    /// <summary>
    /// The Sorting tools on the editor (see <see cref="LineSorter"/>): lines move whole, each with
    /// its own formatting, as one Undo step. The selected lines, or the whole text; Nudge moves the
    /// line with the caret (or the selected lines). Each returns a message for the status bar.
    /// </summary>
    internal static class SortTools
    {
        public static string Sort(ClipXRichTextBox box, SortKind kind)
        {
            string text = box.Text;
            (List<Line> lines, bool hadSelection) = Scope(box, text);
            if (lines.Count(l => l.Length > 0 && text.Substring(l.Start, l.Length).Trim().Length > 0) < 2)
                return "Select two or more lines to sort (or none, to sort the whole text)";
            int[] order = LineSorter.Order(text, lines, kind);
            if (order == null) return "Already in order";
            Reorder(box, lines, order, hadSelection);
            int blanks = lines.Count - order.Length;
            return $"{order.Length:N0} lines sorted" + (blanks == 0 ? "" : blanks == 1 ? "; 1 blank line removed" : $"; {blanks:N0} blank lines removed");
        }

        public static string Reverse(ClipXRichTextBox box)
        {
            string text = box.Text;
            (List<Line> lines, bool hadSelection) = Scope(box, text);
            int[] order = LineSorter.ReverseOrder(lines);
            if (order == null) return "Select more than one line";
            Reorder(box, lines, order, hadSelection);
            return "Line order reversed";
        }

        /// <summary>Nudge up / down (Alt+Up, Alt+Down): the line(s) swap with the one above / below; the selection goes with them.</summary>
        public static string Nudge(ClipXRichTextBox box, bool up)
        {
            int start = box.SelectionStart, length = box.SelectionLength;
            var n = LineSorter.Nudge(box.Text, start, start + length, up);
            if (n == null) return up ? "Already at the top" : "Already at the bottom";
            using (box.BeginBulkEdit())
            {
                box.ReorderLines(n.Value.Block, n.Value.Order);
                box.Select(start + n.Value.Shift, length);
            }
            return "";
        }

        /// <summary>List sort: the comma separated items on each line, alphabetically.</summary>
        public static string SortItems(ClipXRichTextBox box)
        {
            string text = box.Text;
            bool hadSelection = box.SelectionLength > 0;
            int from = hadSelection ? box.SelectionStart : 0, to = hadSelection ? from + box.SelectionLength : 0;
            List<TextEdit> edits = LineSorter.SortItems(text, from, to);
            if (edits.Count == 0)
                return text.IndexOf(',') < 0 ? "No lists to sort (items separated by commas)" : "Already in order";
            using (box.BeginBulkEdit())
            {
                box.ApplyEdits(edits);
                if (!hadSelection) box.Select(0, 0);
            }
            return edits.Count == 1 ? "1 list sorted" : $"{edits.Count:N0} lists sorted";
        }

        private static (List<Line> Lines, bool HadSelection) Scope(ClipXRichTextBox box, string text)
        {
            bool hadSelection = box.SelectionLength > 0;
            int from = hadSelection ? box.SelectionStart : 0;
            int to = hadSelection ? from + box.SelectionLength : 0;
            return (LineSorter.Scope(text, from, to), hadSelection);
        }

        /// <summary>Moves the lines and selects the result (or puts the caret at the start, for the whole text).</summary>
        private static void Reorder(ClipXRichTextBox box, List<Line> lines, int[] order, bool hadSelection)
        {
            int start = lines[0].Start;
            int newLength = order.Sum(i => lines[i].Length) + order.Length - 1;
            using (box.BeginBulkEdit())
            {
                box.ReorderLines(lines, order);
                if (hadSelection) box.Select(start, Math.Max(0, newLength));
                else box.Select(0, 0);
            }
        }
    }
}
