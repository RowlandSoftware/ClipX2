using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ClipX2.TextEngine.Export
{
    /// <summary>Turns a <see cref="FormattedDocument"/> into HTML or plain text for the clipboard.</summary>
    public static class DocumentExport
    {
        /// <summary>
        /// Plain text, one line per paragraph. List numbers and bullets are written out as plain
        /// characters ("1. ", "• "), as in ClipX 1, with a tab per nesting level.
        /// </summary>
        public static string ToPlainText(FormattedDocument doc, string newLine = "\r\n")
        {
            var labels = GetListLabels(doc);
            var sb = new StringBuilder();
            for (int i = 0; i < doc.Paragraphs.Count; i++)
            {
                if (i > 0) sb.Append(newLine);
                FormattedParagraph p = doc.Paragraphs[i];

                if (p.ListKind != ListKind.None)
                {
                    sb.Append('\t', p.ListLevel);
                    if (labels[i].Label.Length > 0) sb.Append(labels[i].Label).Append(' ');
                }
                foreach (FormattedRun run in p.Runs)
                    sb.Append(run.Text.Replace("\v", newLine));
            }
            return sb.ToString();
        }

        /// <summary>
        /// An HTML fragment (no &lt;html&gt; or &lt;body&gt;): paragraphs, lists (&lt;ol&gt;/&lt;ul&gt;,
        /// so they renumber when edited after pasting) and inline formatting. Spacing is preserved
        /// (white-space: pre-wrap), since ClipX text often lines things up with spaces.
        /// </summary>
        public static string ToHtml(FormattedDocument doc)
        {
            var labels = GetListLabels(doc);
            var sb = new StringBuilder();
            var open = new Stack<(int Level, string Tag)>(); // open lists, innermost on top

            // The document's main font goes on a wrapper once; runs only state a font that differs
            var (mainFont, mainSize) = MainFont(doc);
            sb.Append("<div style=\"");
            if (mainFont != null) sb.Append("font-family:").Append(CssFontName(mainFont)).Append(';');
            if (mainSize > 0) sb.Append("font-size:").Append(Pt(mainSize)).Append(';');
            sb.Append("\">");

            for (int i = 0; i < doc.Paragraphs.Count; i++)
            {
                FormattedParagraph p = doc.Paragraphs[i];

                if (p.ListKind == ListKind.None)
                {
                    while (open.Count > 0) sb.Append("</li></").Append(open.Pop().Tag).Append('>');
                    sb.Append("<p style=\"").Append(ParagraphStyle(p, true)).Append("\">");
                    WriteRuns(sb, p);
                    sb.Append("</p>");
                    continue;
                }

                // Close lists nested deeper than this item
                while (open.Count > 0 && open.Peek().Level > p.ListLevel)
                    sb.Append("</li></").Append(open.Pop().Tag).Append('>');

                if (open.Count > 0 && open.Peek().Level == p.ListLevel)
                {
                    if (labels[i].StartsList)
                        sb.Append("</li></").Append(open.Pop().Tag).Append('>');
                    else
                        sb.Append("</li>"); // next item of the same list
                }

                if (open.Count == 0 || open.Peek().Level < p.ListLevel)
                {
                    string tag = p.ListKind == ListKind.Bullet ? "ul" : "ol";
                    sb.Append('<').Append(tag).Append(" style=\"margin-top:0;margin-bottom:0\"");
                    if (tag == "ol")
                    {
                        sb.Append(" type=\"").Append(HtmlListType(p.ListKind)).Append('"');
                        if (labels[i].Number != 1) sb.Append(" start=\"").Append(labels[i].Number).Append('"');
                    }
                    sb.Append('>');
                    open.Push((p.ListLevel, tag));
                }

                sb.Append("<li style=\"").Append(ParagraphStyle(p, false));
                if (p.ListNumberStyle == ListNumberStyle.NoNumber) sb.Append("list-style-type:none;");
                sb.Append("\">");
                WriteRuns(sb, p);
            }
            while (open.Count > 0) sb.Append("</li></").Append(open.Pop().Tag).Append('>');

            sb.Append("</div>");
            return sb.ToString();

            void WriteRuns(StringBuilder s, FormattedParagraph para) => AppendRuns(s, para, mainFont, mainSize);
        }

        /// <summary>The font name and size covering the most characters.</summary>
        private static (string Name, float Size) MainFont(FormattedDocument doc)
        {
            var names = new Dictionary<string, int>();
            var sizes = new Dictionary<float, int>();
            foreach (FormattedParagraph p in doc.Paragraphs)
                foreach (FormattedRun r in p.Runs)
                {
                    if (!string.IsNullOrEmpty(r.FontName))
                        names[r.FontName] = (names.TryGetValue(r.FontName, out int n) ? n : 0) + r.Text.Length;
                    if (r.FontSize > 0)
                        sizes[r.FontSize] = (sizes.TryGetValue(r.FontSize, out int s) ? s : 0) + r.Text.Length;
                }

            string name = null; float size = 0; int best = -1;
            foreach (var kv in names) if (kv.Value > best) { best = kv.Value; name = kv.Key; }
            best = -1;
            foreach (var kv in sizes) if (kv.Value > best) { best = kv.Value; size = kv.Key; }
            return (name, size);
        }

        private static string CssFontName(string name) => "'" + Escape(name).Replace("'", "\\'") + "'";

        private static string ParagraphStyle(FormattedParagraph p, bool indents)
        {
            var sb = new StringBuilder("margin-top:0;margin-bottom:0;white-space:pre-wrap;");
            switch (p.Alignment)
            {
                case ParagraphAlignment.Centre: sb.Append("text-align:center;"); break;
                case ParagraphAlignment.Right: sb.Append("text-align:right;"); break;
                case ParagraphAlignment.Justify: sb.Append("text-align:justify;"); break;
            }
            if (indents)
            {
                if (p.LeftIndent != 0) sb.Append("margin-left:").Append(Pt(p.LeftIndent)).Append(';');
                if (p.FirstLineIndent != 0) sb.Append("text-indent:").Append(Pt(p.FirstLineIndent)).Append(';');
            }
            return sb.ToString();
        }

        private static void AppendRuns(StringBuilder sb, FormattedParagraph p, string mainFont, float mainSize)
        {
            if (p.Runs.TrueForAll(r => r.Text.Length == 0))
            {
                sb.Append("<br>"); // keep empty lines
                return;
            }

            foreach (FormattedRun run in p.Runs)
            {
                if (run.Text.Length == 0) continue;

                var style = new StringBuilder();
                if (!string.IsNullOrEmpty(run.FontName) && run.FontName != mainFont)
                    style.Append("font-family:").Append(CssFontName(run.FontName)).Append(';');
                if (run.FontSize > 0 && run.FontSize != mainSize) style.Append("font-size:").Append(Pt(run.FontSize)).Append(';');
                if (run.Colour.HasValue) style.Append("color:").Append(Hex(run.Colour.Value)).Append(';');
                if (run.Highlight.HasValue) style.Append("background-color:").Append(Hex(run.Highlight.Value)).Append(';');

                var closing = new Stack<string>();
                void Open(bool on, string tag) { if (on) { sb.Append('<').Append(tag).Append('>'); closing.Push(tag); } }

                if (style.Length > 0) { sb.Append("<span style=\"").Append(style).Append("\">"); closing.Push("span"); }
                Open(run.Bold, "b");
                Open(run.Italic, "i");
                Open(run.Underline, "u");
                Open(run.Strikethrough, "s");
                Open(run.Superscript, "sup");
                Open(run.Subscript && !run.Superscript, "sub");

                sb.Append(Escape(run.Text).Replace("\v", "<br>"));

                while (closing.Count > 0) sb.Append("</").Append(closing.Pop()).Append('>');
            }
        }

        private static string HtmlListType(ListKind kind)
        {
            switch (kind)
            {
                case ListKind.LowerLetter: return "a";
                case ListKind.UpperLetter: return "A";
                case ListKind.LowerRoman: return "i";
                case ListKind.UpperRoman: return "I";
                default: return "1";
            }
        }

        private static string Escape(string s) =>
            s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

        private static string Pt(float points) => points.ToString("0.##", CultureInfo.InvariantCulture) + "pt";

        private static string Hex(int rgb) => "#" + (rgb & 0xFFFFFF).ToString("x6", CultureInfo.InvariantCulture);

        /**************** List numbering ****************/

        internal struct ListLabel
        {
            public string Label;     // "3.", "c)", "•", "" for an unnumbered item, null if not a list item
            public int Number;       // this item's number
            public bool StartsList;  // first item of a new list at its level
        }

        /// <summary>
        /// Works out each list item's number, as the editor displays it: consecutive items of the
        /// same kind and level form one list, counting on from its start number; a paragraph that
        /// isn't a list item ends all lists; a deeper level restarts each time it is entered.
        /// </summary>
        internal static ListLabel[] GetListLabels(FormattedDocument doc)
        {
            var result = new ListLabel[doc.Paragraphs.Count];
            var levels = new List<(FormattedParagraph First, int Count)>(); // the current list at each level

            for (int i = 0; i < doc.Paragraphs.Count; i++)
            {
                FormattedParagraph p = doc.Paragraphs[i];
                if (p.ListKind == ListKind.None)
                {
                    levels.Clear();
                    continue;
                }

                int level = p.ListLevel < 0 ? 0 : p.ListLevel;
                if (levels.Count > level + 1) levels.RemoveRange(level + 1, levels.Count - level - 1);

                bool continues = levels.Count == level + 1 && levels[level].First != null &&
                    levels[level].First.ListKind == p.ListKind &&
                    levels[level].First.ListStart == p.ListStart;

                int number;
                if (continues)
                {
                    number = levels[level].Count + (p.ListNumberStyle == ListNumberStyle.NoNumber ? 0 : 1);
                    levels[level] = (levels[level].First, number);
                }
                else
                {
                    number = p.ListStart;
                    while (levels.Count <= level) levels.Add((null, 0));
                    levels[level] = (p, number);
                }

                result[i] = new ListLabel
                {
                    Label = p.ListNumberStyle == ListNumberStyle.NoNumber ? "" : FormatLabel(p, number),
                    Number = number,
                    StartsList = !continues
                };
            }
            return result;
        }

        private static string FormatLabel(FormattedParagraph p, int number)
        {
            if (p.ListKind == ListKind.Bullet) return "•";

            string n;
            switch (p.ListKind)
            {
                case ListKind.LowerLetter: n = ToLetters(number).ToLowerInvariant(); break;
                case ListKind.UpperLetter: n = ToLetters(number); break;
                case ListKind.LowerRoman: n = ToRoman(number).ToLowerInvariant(); break;
                case ListKind.UpperRoman: n = ToRoman(number); break;
                default: n = number.ToString(CultureInfo.InvariantCulture); break;
            }

            switch (p.ListNumberStyle)
            {
                case ListNumberStyle.Parentheses: return "(" + n + ")";
                case ListNumberStyle.Period: return n + ".";
                case ListNumberStyle.Plain: return n;
                default: return n + ")";
            }
        }

        /// <summary>1 = A, 26 = Z, 27 = AA ...</summary>
        internal static string ToLetters(int number)
        {
            if (number < 1) return number.ToString(CultureInfo.InvariantCulture);
            var sb = new StringBuilder();
            while (number > 0)
            {
                number--;
                sb.Insert(0, (char)('A' + number % 26));
                number /= 26;
            }
            return sb.ToString();
        }

        internal static string ToRoman(int number)
        {
            if (number < 1 || number > 3999) return number.ToString(CultureInfo.InvariantCulture);
            int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
            string[] numerals = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
            var sb = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
                while (number >= values[i]) { sb.Append(numerals[i]); number -= values[i]; }
            return sb.ToString();
        }
    }
}
