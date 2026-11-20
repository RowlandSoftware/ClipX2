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

        /************ Lists: line numbers ************/

        public static string LineNumbers(ClipXRichTextBox box, LineNumberOptions o) =>
            Run(box, (t, f, e) => TextEngine.LineNumbers.Add(t, f, e, o)).Edits > 0 ? "Lines numbered" : "No lines to number";

        public static string RemoveLineNumbers(ClipXRichTextBox box) =>
            Run(box, TextEngine.LineNumbers.Remove).Edits > 0 ? "Line numbers removed" : "No line numbers found";

        /************ Edit group: decoding ************/

        public static string DecodeUrl(ClipXRichTextBox box) => Decode(box, TextDecoder.Url, "URL escape");
        public static string DecodeHtml(ClipXRichTextBox box) => Decode(box, TextDecoder.Html, "HTML entity", "HTML entities");
        public static string DecodeUtf8(ClipXRichTextBox box) => Decode(box, TextDecoder.Utf8, "UTF-8 character");
        public static string DecodeMime(ClipXRichTextBox box) => Decode(box, TextDecoder.Mime, "MIME code");

        private static string Decode(ClipXRichTextBox box, Func<string, int, int, List<TextEdit>> make, string one, string many = null)
        {
            int n = InPlace(box, make);
            return n == 0 ? $"Nothing to decode: no {many ?? one + "s"} found"
                : n == 1 ? $"Decoded 1 {one}" : $"Decoded {n:N0} {many ?? one + "s"}";
        }

        /************ Edit group: characters ************/

        public static string PlainCharacters(ClipXRichTextBox box)
        {
            int n = InPlace(box, Typography.PlainCharacters);
            return n == 0 ? "No special characters" : n == 1 ? "1 character made plain" : $"{n:N0} characters made plain";
        }

        /// <summary>Curly quotes; mentions paragraphs whose double quotes don't pair up (as ClipX 1 did).</summary>
        public static string CurlyQuotes(ClipXRichTextBox box)
        {
            Typography.CurlyResult result = null;
            int n = InPlace(box, (t, f, e) => (result = Typography.CurlyQuotes(t, f, e)).Edits);
            string done = n == 0 ? "No straight quotes, -- or ..." : n == 1 ? "1 change" : $"{n:N0} changes";
            int odd = result?.Unmatched.Count ?? 0;
            return odd == 0 ? done
                : done + (odd == 1 ? "; a paragraph has" : $"; {odd} paragraphs have") + " unpaired double quotes (fine if a quotation runs on)";
        }

        public static string RegulariseLineEnds(ClipXRichTextBox box)
        {
            int n = InPlace(box, (t, f, e) => Typography.RegulariseLineEnds(t, f, e));
            return n == 0 ? "Line ends are already regular" : n == 1 ? "1 line end regularised" : $"{n:N0} line ends regularised";
        }

        /// <summary>
        /// Applies edits to the selection exactly as selected (characters can sit anywhere in a
        /// line), or the whole text; what was selected stays selected. One Undo step, formatting
        /// kept. Returns the number of edits.
        /// </summary>
        private static int InPlace(ClipXRichTextBox box, Func<string, int, int, List<TextEdit>> make)
        {
            bool hadSelection = box.SelectionLength > 0;
            int from = hadSelection ? box.SelectionStart : 0;
            int to = hadSelection ? from + box.SelectionLength : box.TextLength;
            List<TextEdit> edits = make(box.Text, from, to);
            if (edits.Count == 0) return 0;
            int delta = edits.Sum(e => e.Replacement.Length - e.Length);
            using (box.BeginBulkEdit())
            {
                box.ApplyEdits(edits);
                if (hadSelection) box.Select(from, Math.Max(0, to + delta - from));
                else box.Select(0, 0);
            }
            return edits.Count;
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
