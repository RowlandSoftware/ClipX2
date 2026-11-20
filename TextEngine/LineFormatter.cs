using System;
using System.Collections.Generic;
using System.Text;
using Line = ClipX2.TextEngine.ListFormatter.Line;

namespace ClipX2.TextEngine
{
    /// <summary>Indent / Unindent options (ClipX 1's Indent settings: same names).</summary>
    public sealed class IndentOptions
    {
        /// <summary>"Tab", "Spaces" or "Text": what one level of indentation is.</summary>
        public string Option { get; set; } = "Tab";
        public int Spaces { get; set; } = 4;
        /// <summary>With "Text": the text put in front of each line (e.g. "> " to quote).</summary>
        public string Text { get; set; } = "";
        /// <summary>The last used was Unindent (not Indent): a right-click on the Indent / Unindent button repeats it.</summary>
        public bool Unindent { get; set; }

        /// <summary>One level of indentation.</summary>
        public string IndentText =>
            Option == "Spaces" ? new string(' ', Math.Max(1, Spaces)) :
            Option == "Text" ? Text ?? "" : "\t";
    }

    /// <summary>Word wrap options (ClipX 1's Wordwrap settings: same names).</summary>
    public sealed class WordWrapOptions
    {
        /// <summary>The longest line, in characters (a tab reaches the next multiple of 8).</summary>
        public int Width { get; set; } = 50;
        /// <summary>Lines after a paragraph's first go one step further in.</summary>
        public bool Hanging { get; set; }
        /// <summary>The hanging step is a tab (else eight spaces).</summary>
        public bool Tab { get; set; } = true;
        /// <summary>Spaces added between words so lines fill the width (not a paragraph's last line).</summary>
        public bool Justify { get; set; }
    }

    /// <summary>Remove extra lines options (ClipX 1's RemLines settings: same names).</summary>
    public sealed class RemoveLinesOptions
    {
        /// <summary>Take out single blank lines.</summary>
        public bool Single { get; set; }
        /// <summary>Reduce several blank lines in a row to one.</summary>
        public bool Multiple { get; set; } = true;
    }

    /// <summary>
    /// The Format tools (ClipX 1's): trimming, spacing, indenting, tabs, blank lines, joining
    /// lines into paragraphs and word wrap. Each works on whole lines (the selection widened, as
    /// <see cref="ListFormatter.LinesIn"/>) and returns edits in text order, not overlapping, that
    /// change only what they must, so in rich text the rest keeps its formatting. A blank line is
    /// one that is empty or only white space.
    /// </summary>
    public static class LineFormatter
    {
        /// <summary>Width of the columns a tab reaches (ClipX 1's).</summary>
        public const int TabWidth = 8;

        private static bool IsSpace(char c) => c == ' ' || c == '\t' || c == (char)0x00A0;

        private static bool IsBlank(string text, Line line)
        {
            for (int i = line.Start; i < line.End; i++)
                if (!IsSpace(text[i])) return false;
            return true;
        }

        private static int Leading(string text, Line line)
        {
            int i = line.Start;
            while (i < line.End && IsSpace(text[i])) i++;
            return i - line.Start;
        }

        private static int TrimmedEnd(string text, Line line)
        {
            int k = line.End;
            while (k > line.Start && IsSpace(text[k - 1])) k--;
            return k;
        }

        /// <summary>Where the line after this one starts (its End if it is the last).</summary>
        private static int NextStart(string text, Line line)
        {
            int e = line.End;
            if (e >= text.Length) return e;
            return e + (text[e] == '\r' && e + 1 < text.Length && text[e + 1] == '\n' ? 2 : 1);
        }

        private static List<TextEdit> Sorted(List<TextEdit> edits)
        {
            edits.Sort((a, b) => a.Start.CompareTo(b.Start));
            return edits;
        }

        /************ Trim, single space ************/

        /// <summary>Trim trailing space (foTRA), or with <paramref name="leading"/> white space at both ends (foTRM).</summary>
        public static List<TextEdit> Trim(string text, int from, int to, bool leading)
        {
            var edits = new List<TextEdit>();
            foreach (Line line in ListFormatter.LinesIn(text, from, to))
            {
                int end = TrimmedEnd(text, line);
                int lead = leading ? Math.Min(Leading(text, line), end - line.Start) : 0;
                if (lead > 0) edits.Add(new TextEdit(line.Start, lead, ""));
                if (end < line.End) edits.Add(new TextEdit(end, line.End - end, ""));
            }
            return edits;
        }

        /// <summary>
        /// Single space (foSSP): after each line's indentation, a run of spaces becomes one space,
        /// and spaces next to a tab go (the tab stays: it lines up columns). Hard spaces in a run
        /// count as spaces. Indentation is kept.
        /// </summary>
        public static List<TextEdit> SingleSpace(string text, int from, int to)
        {
            var edits = new List<TextEdit>();
            foreach (Line line in ListFormatter.LinesIn(text, from, to))
            {
                int i = line.Start + Leading(text, line);
                while (i < line.End)
                {
                    if (!IsSpace(text[i])) { i++; continue; }
                    int start = i;
                    var tabs = new StringBuilder();
                    while (i < line.End && IsSpace(text[i])) { if (text[i] == '\t') tabs.Append('\t'); i++; }
                    string run = text.Substring(start, i - start);
                    string replacement = tabs.Length > 0 ? tabs.ToString() : run.Length == 1 ? run : " ";
                    if (replacement != run) edits.Add(new TextEdit(start, run.Length, replacement));
                }
            }
            return edits;
        }

        /************ Indent ************/

        /// <summary>Indent (foIND): one level in front of each line that isn't blank.</summary>
        public static List<TextEdit> Indent(string text, int from, int to, IndentOptions options)
        {
            var edits = new List<TextEdit>();
            string indent = options.IndentText;
            if (indent.Length == 0) return edits;
            foreach (Line line in ListFormatter.LinesIn(text, from, to))
                if (!IsBlank(text, line)) edits.Add(new TextEdit(line.Start, 0, indent));
            return edits;
        }

        /// <summary>
        /// Unindent (foUNI): one level off each line that starts with it. With spaces, a line
        /// indented by fewer loses what it has.
        /// </summary>
        public static List<TextEdit> Unindent(string text, int from, int to, IndentOptions options)
        {
            var edits = new List<TextEdit>();
            string indent = options.IndentText;
            if (indent.Length == 0) return edits;
            bool spaces = options.Option == "Spaces";
            foreach (Line line in ListFormatter.LinesIn(text, from, to))
            {
                int n = 0;
                if (spaces)
                    while (n < indent.Length && line.Start + n < line.End && text[line.Start + n] == ' ') n++;
                else if (line.Length >= indent.Length && string.CompareOrdinal(text, line.Start, indent, 0, indent.Length) == 0)
                    n = indent.Length;
                if (n > 0) edits.Add(new TextEdit(line.Start, n, ""));
            }
            return edits;
        }

        /************ Tabs and spaces ************/

        /// <summary>
        /// Tabs to spaces (foT2S): each tab becomes the spaces that reach the same column (every
        /// <see cref="TabWidth"/> characters from the start of the line), so columns still line up
        /// in a fixed-width font.
        /// </summary>
        public static List<TextEdit> TabsToSpaces(string text, int from, int to)
        {
            var edits = new List<TextEdit>();
            foreach (Line line in ListFormatter.LinesIn(text, from, to))
            {
                int column = 0;
                for (int i = line.Start; i < line.End; i++)
                {
                    if (text[i] != '\t') { column++; continue; }
                    int spaces = TabWidth - column % TabWidth;
                    edits.Add(new TextEdit(i, 1, new string(' ', spaces)));
                    column += spaces;
                }
            }
            return edits;
        }

        /// <summary>Spaces to tabs (foS2T): each run of two or more spaces becomes one tab (columns aren't lined up).</summary>
        public static List<TextEdit> SpacesToTabs(string text, int from, int to)
        {
            var edits = new List<TextEdit>();
            foreach (Line line in ListFormatter.LinesIn(text, from, to))
            {
                int i = line.Start;
                while (i < line.End)
                {
                    if (text[i] != ' ') { i++; continue; }
                    int start = i;
                    while (i < line.End && text[i] == ' ') i++;
                    if (i - start >= 2) edits.Add(new TextEdit(start, i - start, "\t"));
                }
            }
            return edits;
        }

        /************ Blank lines, paragraphs ************/

        /// <summary>
        /// Remove extra lines (foREM): with Multiple, blank lines in a row become one; with Single,
        /// a lone blank line goes; with both, all blank lines go. Blank lines that stay are emptied
        /// of white space; blank lines at the very end of the text go (as ClipX 1).
        /// </summary>
        public static List<TextEdit> RemoveExtraLines(string text, int from, int to, RemoveLinesOptions options) =>
            Sorted(BlankLineEdits(text, ListFormatter.LinesIn(text, from, to), options.Single, options.Multiple, trimEnd: true));

        private static List<TextEdit> BlankLineEdits(string text, List<Line> lines, bool single, bool multiple, bool trimEnd)
        {
            var edits = new List<TextEdit>();
            int i = 0;
            while (i < lines.Count)
            {
                if (!IsBlank(text, lines[i])) { i++; continue; }
                int first = i;
                while (i < lines.Count && IsBlank(text, lines[i])) i++;
                int n = i - first, last = i - 1;
                bool atEnd = trimEnd && OnlyWhiteSpaceFrom(text, lines[last].End); // nothing but blank lines follow

                int keep = atEnd ? 0 : single && multiple ? 0 : n == 1 ? (single ? 0 : 1) : (multiple ? 1 : n);
                for (int k = first; k < first + keep; k++)
                    if (lines[k].Length > 0) edits.Add(new TextEdit(lines[k].Start, lines[k].Length, "")); // empty it

                if (keep == n) continue;
                if (atEnd)
                {
                    // Nothing follows: take the line break before the run too (unless the text is all blank)
                    if (first == 0 && lines[0].Start == 0) { if (text.Length > 0) edits.Add(new TextEdit(0, text.Length, "")); continue; }
                    int start = first > 0 ? lines[first - 1].End : PreviousEnd(text, lines[first].Start);
                    edits.Add(new TextEdit(start, text.Length - start, ""));
                }
                else
                {
                    int start = lines[first + keep].Start;
                    edits.Add(new TextEdit(start, NextStart(text, lines[last]) - start, ""));
                }
            }
            return edits;
        }

        private static bool OnlyWhiteSpaceFrom(string text, int at)
        {
            for (int i = at; i < text.Length; i++)
                if (!IsSpace(text[i]) && text[i] != '\r' && text[i] != '\n') return false;
            return true;
        }

        /// <summary>The end of the line before the one starting at <paramref name="lineStart"/>.</summary>
        private static int PreviousEnd(string text, int lineStart)
        {
            int p = lineStart;
            if (p > 0 && text[p - 1] == '\n') p--;
            if (p > 0 && text[p - 1] == '\r') p--;
            return p;
        }

        /// <summary>
        /// Paragraph (foPAR), un-wordwrap: joins each line to the one before with a space, making
        /// paragraphs again. A line stays on its own if the one before is blank, or it is blank,
        /// indented, or starts with a bullet or a number (a list). Trailing spaces go; blank lines
        /// in a row become one.
        /// </summary>
        public static List<TextEdit> Paragraph(string text, int from, int to)
        {
            List<Line> lines = ListFormatter.LinesIn(text, from, to);
            var edits = BlankLineEdits(text, lines, single: false, multiple: true, trimEnd: false);
            for (int i = 0; i < lines.Count; i++)
            {
                Line line = lines[i];
                if (IsBlank(text, line)) continue; // emptied above if need be
                int end = TrimmedEnd(text, line);
                bool join = i + 1 < lines.Count && !IsBlank(text, lines[i + 1]) && !StartsOwnLine(text, lines[i + 1]);
                if (join)
                    edits.Add(new TextEdit(end, lines[i + 1].Start - end, " "));
                else if (end < line.End)
                    edits.Add(new TextEdit(end, line.End - end, ""));
            }
            return Sorted(edits);
        }

        /// <summary>Indented, or starting with a bullet or a list number.</summary>
        private static bool StartsOwnLine(string text, Line line) =>
            IsSpace(text[line.Start]) ||
            ListFormatter.BulletLength(text, line.Start, line.End) > 0 ||
            ListFormatter.NumberLength(text, line.Start, line.End) > 0;

        /************ Word wrap ************/

        /// <summary>
        /// Word wrap (foWRP): splits each line (a paragraph) into lines no wider than the width,
        /// breaking between words; a longer word gets a line of its own. Lines after the first keep
        /// the paragraph's indentation, plus a tab or eight spaces with Hanging. Justify spreads
        /// spaces so lines fill the width, except a paragraph's last line and left of a tab.
        /// <paramref name="newLine"/> is the line break to insert.
        /// </summary>
        public static List<TextEdit> WordWrap(string text, int from, int to, WordWrapOptions options, string newLine = "\n")
        {
            var edits = new List<TextEdit>();
            int width = Math.Max(1, options.Width);
            foreach (Line line in ListFormatter.LinesIn(text, from, to))
            {
                if (IsBlank(text, line)) continue;
                int lead = Leading(text, line);
                string indent = text.Substring(line.Start, lead);
                string next = indent + (options.Hanging ? (options.Tab ? "\t" : "        ") : "");
                List<string> wrapped = Wrap(text.Substring(line.Start + lead, TrimmedEnd(text, line) - line.Start - lead), indent, next, width);
                if (options.Justify)
                    for (int k = 0; k < wrapped.Count - 1; k++) wrapped[k] = Justify(wrapped[k], width);
                string result = string.Join(newLine, wrapped);
                if (result != text.Substring(line.Start, line.Length)) edits.Add(new TextEdit(line.Start, line.Length, result));
            }
            return edits;
        }

        private static List<string> Wrap(string content, string firstIndent, string nextIndent, int width)
        {
            var lines = new List<string>();
            var current = new StringBuilder(firstIndent);
            bool hasWord = false;
            foreach (string token in Tokens(content))
            {
                if (token == "\t") { current.Append('\t'); continue; } // a tab joins without a space
                bool space = hasWord && current[current.Length - 1] != '\t';
                int w = Columns(current.ToString() + (space ? " " : "") + token);
                if (hasWord && w > width)
                {
                    lines.Add(current.ToString().TrimEnd(' '));
                    current.Clear().Append(nextIndent).Append(token);
                }
                else
                {
                    if (space) current.Append(' ');
                    current.Append(token);
                }
                hasWord = true;
            }
            lines.Add(current.ToString().TrimEnd(' '));
            return lines;
        }

        /// <summary>Words (split at spaces) and tabs, in order.</summary>
        private static IEnumerable<string> Tokens(string content)
        {
            var word = new StringBuilder();
            foreach (char c in content)
            {
                if (c == ' ' || c == '\t')
                {
                    if (word.Length > 0) { yield return word.ToString(); word.Clear(); }
                    if (c == '\t') yield return "\t";
                }
                else word.Append(c);
            }
            if (word.Length > 0) yield return word.ToString();
        }

        /// <summary>Display width, a tab reaching the next multiple of <see cref="TabWidth"/>.</summary>
        private static int Columns(string s)
        {
            int column = 0;
            foreach (char c in s) column = c == '\t' ? (column / TabWidth + 1) * TabWidth : column + 1;
            return column;
        }

        /// <summary>Spreads extra spaces between the words after the line's indentation and last tab.</summary>
        private static string Justify(string line, int width)
        {
            int fixedPart = line.LastIndexOf('\t') + 1;
            while (fixedPart < line.Length && line[fixedPart] == ' ') fixedPart++;
            string head = line.Substring(0, fixedPart);
            string[] words = line.Substring(fixedPart).Split(' ');
            int gaps = words.Length - 1;
            int shortfall = width - Columns(line);
            if (gaps < 1 || shortfall <= 0) return line;
            var sb = new StringBuilder(head);
            for (int g = 0; g < words.Length; g++)
            {
                sb.Append(words[g]);
                if (g == gaps) break;
                // Spread the spares evenly (not all at the left), so there are no rivers of white
                int extra = shortfall * (g + 1) / gaps - shortfall * g / gaps;
                sb.Append(' ', 1 + extra);
            }
            return sb.ToString();
        }
    }
}
