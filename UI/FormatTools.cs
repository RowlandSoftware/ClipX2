using System;
using System.Collections.Generic;
using System.Linq;
using ClipX2.TextEngine;

namespace ClipX2.UI
{
    /// <summary>
    /// The Format tools on the editor (see <see cref="LineFormatter"/>): the selected lines, or the
    /// whole text if nothing is selected. Edits go in place (formatting kept in rich text), as one
    /// Undo step, and the lines stay selected. Each returns a short message for the status bar.
    /// </summary>
    internal static class FormatTools
    {
        public static string TrimWhiteSpace(ClipXRichTextBox box) =>
            Removed(Run(box, (t, f, e) => LineFormatter.Trim(t, f, e, leading: true)));

        public static string TrimTrailing(ClipXRichTextBox box) =>
            Removed(Run(box, (t, f, e) => LineFormatter.Trim(t, f, e, leading: false)));

        public static string SingleSpace(ClipXRichTextBox box) =>
            Removed(Run(box, LineFormatter.SingleSpace));

        public static string Indent(ClipXRichTextBox box, IndentOptions o) =>
            Run(box, (t, f, e) => LineFormatter.Indent(t, f, e, o)).Edits > 0 ? "Indent increased" : "Nothing to indent";

        public static string Unindent(ClipXRichTextBox box, IndentOptions o) =>
            Run(box, (t, f, e) => LineFormatter.Unindent(t, f, e, o)).Edits > 0 ? "Indent reduced" : "No line starts with that indent";

        public static string TabsToSpaces(ClipXRichTextBox box) =>
            Run(box, LineFormatter.TabsToSpaces).Edits > 0 ? "Tabs converted to spaces" : "No tabs";

        public static string SpacesToTabs(ClipXRichTextBox box) =>
            Run(box, LineFormatter.SpacesToTabs).Edits > 0 ? "Spaces converted to tabs" : "No runs of spaces";

        public static string RemoveExtraLines(ClipXRichTextBox box, RemoveLinesOptions o) =>
            Removed(Run(box, (t, f, e) => LineFormatter.RemoveExtraLines(t, f, e, o)));

        public static string Paragraph(ClipXRichTextBox box) =>
            Removed(Run(box, LineFormatter.Paragraph));

        /// <summary>Plain text only (as ClipX 1): rich text wraps to the window.</summary>
        public static string WordWrap(ClipXRichTextBox box, bool richText, WordWrapOptions o)
        {
            if (richText) return "Word wrap is for plain text (rich text wraps to the window)";
            return Run(box, (t, f, e) => LineFormatter.WordWrap(t, f, e, o)).Edits > 0
                ? $"Wrapped at {o.Width} characters" : "Nothing to wrap";
        }

        /// <summary>Applies a tool's edits to the scope and reselects the lines.</summary>
        private static (int Edits, int Delta) Run(ClipXRichTextBox box, Func<string, int, int, List<TextEdit>> make)
        {
            string text = box.Text;
            bool hadSelection = box.SelectionLength > 0;
            int from = hadSelection ? box.SelectionStart : 0;
            int to = hadSelection ? from + box.SelectionLength : text.Length;
            List<ListFormatter.Line> lines = ListFormatter.LinesIn(text, from, to);
            List<TextEdit> edits = make(text, from, to);
            if (edits.Count == 0) return (0, 0);

            int delta = edits.Sum(e => e.Replacement.Length - e.Length);
            using (box.BeginBulkEdit())
            {
                box.ApplyEdits(edits);
                if (hadSelection && lines.Count > 0)
                {
                    int start = Math.Min(lines[0].Start, box.TextLength);
                    int end = Math.Min(box.TextLength, lines[lines.Count - 1].End + delta);
                    box.Select(start, Math.Max(0, end - start));
                }
                else box.Select(0, 0);
            }
            return (edits.Count, delta);
        }

        /// <summary>"n characters removed", as ClipX 1.</summary>
        private static string Removed((int Edits, int Delta) result)
        {
            if (result.Edits == 0) return "Nothing to change";
            int removed = -result.Delta;
            return removed == 1 ? "1 character removed"
                : removed >= 0 ? $"{removed:N0} characters removed"
                : $"{-removed:N0} characters added";
        }
    }
}
