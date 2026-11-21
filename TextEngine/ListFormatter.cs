using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClipX2.TextEngine
{
    /// <summary>Number styles, in ClipX 1's order (numStyles), so its settings carry over.</summary>
    public enum NumberStyle
    {
        /// <summary>1, 2, 3</summary>
        Arabic = 0,
        /// <summary>a, b, c</summary>
        Letters = 1,
        /// <summary>A, B, C</summary>
        LettersCap = 2,
        /// <summary>i, ii, iii</summary>
        Roman = 3,
        /// <summary>I, II, III</summary>
        RomanCap = 4,
        /// <summary>1., 1.1., 1.1.1. by indentation (hierarchical)</summary>
        Legal = 5,
    }

    /// <summary>What goes round a number, in ClipX 1's order (numPunctuation).</summary>
    public enum NumberPunctuation
    {
        /// <summary>1</summary>
        None = 0,
        /// <summary>1.</summary>
        Period = 1,
        /// <summary>1)</summary>
        Bracket = 2,
        /// <summary>(1)</summary>
        TwoBrackets = 3,
    }

    /// <summary>Bullet list options (ClipX 1's BulletList settings: same names).</summary>
    public sealed class BulletOptions
    {
        /// <summary>The bullet.</summary>
        public string Character { get; set; } = "•";
        /// <summary>A tab after the bullet (and a tab per level of indentation); else spaces.</summary>
        public bool Tab { get; set; } = true;
        /// <summary>
        /// With spaces: the width the bullet is padded to (5 = the bullet and 4 spaces), which is
        /// also one level of indentation (a line indented by 5 spaces is second level).
        /// </summary>
        public int PadChars { get; set; } = 5;
    }

    /// <summary>Numbered list options (ClipX 1's NumList settings: same names).</summary>
    public sealed class NumberOptions
    {
        public NumberStyle Style { get; set; } = NumberStyle.Arabic;
        public int StartNumber { get; set; } = 1;
        /// <summary>Take off existing numbers first (else lines already numbered get a second number).</summary>
        public bool Renumber { get; set; } = true;
        public NumberPunctuation FollowChar { get; set; } = NumberPunctuation.Period;
        /// <summary>A tab after the number (and a tab per level of indentation); else spaces.</summary>
        public bool Tab { get; set; } = true;
        /// <summary>With spaces: the width the number is padded to ("1." + 2 spaces = 4); also one level of indentation.</summary>
        public int PadChars { get; set; } = 4;
    }

    /// <summary>
    /// Bullets and numbers typed into the text (ClipX 1's lists; plain-text mode, and rich text
    /// where a live list can't do the job). Works line by line on whole lines: the edits replace
    /// only the start of each line (its indentation, and an old bullet or number), so the rest of
    /// the line, and its formatting in rich text, is untouched. Blank lines are left alone and
    /// don't interrupt the numbering.
    ///
    /// Levels come from indentation: a tab, or (with spaces) one pad width of spaces, per level.
    /// Bullets keep each line's level. Numbers: Legal style numbers by level (1., 1.1., 1.1.1.),
    /// restarting at each new sub-list; the other styles number only the lines that aren't
    /// indented and move indented lines along by one level, as ClipX 1 did.
    /// </summary>
    public static class ListFormatter
    {
        /// <summary>Characters taken for bullets when re-bulleting or removing bullets (followed by a space or tab).</summary>
        public const string BulletChars = "•·*-–—◦▪▫■□○●►▸‣⁃✓✔→>";

        /************ The lines to work on ************/

        /// <summary>A line of the text: where it starts and ends (excluding the line break).</summary>
        public readonly struct Line
        {
            public Line(int start, int end) { Start = start; End = end; }
            public int Start { get; }
            public int End { get; }
            public int Length => End - Start;
        }

        /// <summary>
        /// The whole lines touching [<paramref name="from"/>, <paramref name="to"/>): a selection
        /// widened to whole lines. A selection ending at the very start of a line doesn't take that line.
        /// </summary>
        public static List<Line> LinesIn(string text, int from, int to)
        {
            var lines = new List<Line>();
            if (text == null) return lines;
            from = Math.Max(0, Math.Min(from, text.Length));
            to = Math.Max(from, Math.Min(to, text.Length));
            if (to > from && to > 0 && IsBreak(text[to - 1])) to--; // selection ending just after a line break
            int start = from;
            while (start > 0 && !IsBreak(text[start - 1])) start--;
            while (true)
            {
                int end = start;
                while (end < text.Length && !IsBreak(text[end])) end++;
                lines.Add(new Line(start, end));
                if (end >= to || end >= text.Length) break;
                start = end + (text[end] == '\r' && end + 1 < text.Length && text[end + 1] == '\n' ? 2 : 1);
            }
            return lines;
        }

        private static bool IsBreak(char c) => c == '\n' || c == '\r';

        /************ Indentation and levels ************/

        /// <summary>Length of the leading spaces and tabs.</summary>
        public static int IndentLength(string text, Line line)
        {
            int i = line.Start;
            while (i < line.End && (text[i] == ' ' || text[i] == '\t')) i++;
            return i - line.Start;
        }

        /// <summary>
        /// The level of a line from its indentation (ClipX 1's CountIndents): leading tabs, or
        /// leading runs of <paramref name="padChars"/> spaces.
        /// </summary>
        public static int Level(string text, Line line, bool tab, int padChars)
        {
            // Either way of indenting counts, mixed too (a tab typed into text indented with
            // spaces): each tab is a level, and so is each run of pad-width spaces
            int level = 0, i = line.Start, spaces = 0;
            int width = Math.Max(1, padChars);
            for (; i < line.End; i++)
            {
                if (text[i] == '\t') { level++; spaces = 0; }
                else if (text[i] == ' ') { if (++spaces == width) { level++; spaces = 0; } }
                else break;
            }
            return level;
        }

        /// <summary>
        /// The bullet for a level: the chosen one at the top, then ◦ and ▪ (as Word), round again.
        /// If the chosen one is itself ◦ or ▪, the others follow it in the same order.
        /// </summary>
        public static string BulletForLevel(string first, int level)
        {
            string[] cascade = { string.IsNullOrEmpty(first) ? "•" : first, "◦", "▪" };
            if (cascade[0] == "◦") cascade = new[] { "◦", "▪", "•" };
            else if (cascade[0] == "▪") cascade = new[] { "▪", "•", "◦" };
            return cascade[Math.Max(0, level) % cascade.Length];
        }

        /// <summary>The indentation for a level: tabs, or pad widths of spaces.</summary>
        public static string Indent(int level, bool tab, int padChars) =>
            level <= 0 ? "" : tab ? new string('\t', level) : new string(' ', Math.Max(1, padChars) * level);

        /// <summary>A bullet or number followed by a tab, or padded with spaces to the pad width (always at least one space).</summary>
        public static string Pad(string marker, bool tab, int padChars)
        {
            if (tab) return marker + "\t";
            int spaces = Math.Max(1, Math.Max(1, padChars) - marker.Length);
            return marker + new string(' ', spaces);
        }

        /************ Bullets ************/

        /// <summary>
        /// Bullets the lines: each keeps its level (indentation), any old bullet is replaced, and
        /// the text follows the bullet after a tab or the padding.
        /// </summary>
        public static List<TextEdit> BulletEdits(string text, int from, int to, BulletOptions options)
        {
            var edits = new List<TextEdit>();
            string bullet = string.IsNullOrEmpty(options.Character) ? "•" : options.Character;
            foreach (Line line in LinesIn(text, from, to))
            {
                if (IsBlank(text, line)) continue;
                int level = Level(text, line, options.Tab, options.PadChars);
                int indent = IndentLength(text, line);
                int old = BulletLength(text, line.Start + indent, line.End);
                string prefix = Indent(level, options.Tab, options.PadChars) + Pad(BulletForLevel(bullet, level), options.Tab, options.PadChars);
                AddEdit(edits, text, line.Start, indent + old, prefix);
            }
            return edits;
        }

        /// <summary>
        /// After lines have been indented or unindented: each typed bullet becomes the one for its
        /// new level (• ◦ ▪), if it is one of that cascade (a bullet of the user's own is left alone).
        /// </summary>
        public static List<TextEdit> RebulletEdits(string text, int from, int to, BulletOptions options)
        {
            var edits = new List<TextEdit>();
            string first = string.IsNullOrEmpty(options.Character) ? "•" : options.Character;
            string cascade = BulletForLevel(first, 0) + BulletForLevel(first, 1) + BulletForLevel(first, 2);
            foreach (Line line in LinesIn(text, from, to))
            {
                int indent = IndentLength(text, line);
                int at = line.Start + indent;
                if (BulletLength(text, at, line.End) == 0 || cascade.IndexOf(text[at]) < 0) continue;
                string wanted = BulletForLevel(first, Level(text, line, options.Tab, options.PadChars));
                if (text[at].ToString() != wanted) edits.Add(new TextEdit(at, 1, wanted));
            }
            return edits;
        }

        /// <summary>Takes the bullets off the lines (keeping their indentation).</summary>
        public static List<TextEdit> RemoveBulletEdits(string text, int from, int to)
        {
            var edits = new List<TextEdit>();
            foreach (Line line in LinesIn(text, from, to))
            {
                int indent = IndentLength(text, line);
                int old = BulletLength(text, line.Start + indent, line.End);
                if (old > 0) edits.Add(new TextEdit(line.Start + indent, old, ""));
            }
            return edits;
        }

        /// <summary>Length of a bullet and the spaces or tabs after it at <paramref name="at"/> (0 if none).</summary>
        public static int BulletLength(string text, int at, int end)
        {
            if (at >= end || BulletChars.IndexOf(text[at]) < 0) return 0;
            int i = at + 1;
            if (i >= end || (text[i] != ' ' && text[i] != '\t')) return 0; // "-5" or "*emphasis*" is text
            while (i < end && (text[i] == ' ' || text[i] == '\t')) i++;
            return i - at;
        }

        /************ Numbers ************/

        /// <summary>Numbers the lines (see the class notes for levels).</summary>
        public static List<TextEdit> NumberEdits(string text, int from, int to, NumberOptions options)
        {
            var edits = new List<TextEdit>();
            var counts = new List<int> { options.StartNumber - 1 };
            int current = 0;   // Legal: the level being numbered
            int previous = 0;  // hierarchical: the last line's level
            // Levels count from the list's own shallowest line (a list that is all indented starts at the top)
            int baseLevel = LinesIn(text, from, to).Where(l => !IsBlank(text, l))
                .Select(l => Level(text, l, options.Tab, options.PadChars)).DefaultIfEmpty(0).Min();
            foreach (Line line in LinesIn(text, from, to))
            {
                if (IsBlank(text, line)) continue;
                int indent = IndentLength(text, line);
                int old = options.Renumber ? NumberLength(text, line.Start + indent, line.End) : 0;

                if (options.Style == NumberStyle.Legal)
                {
                    int level = Math.Max(0, Level(text, line, options.Tab, options.PadChars) - baseLevel);
                    while (current < level) { current++; if (counts.Count <= current) counts.Add(0); else counts[current] = 0; } // a new sub-list
                    current = level;
                    counts[current]++;
                    var label = new StringBuilder();
                    for (int j = 0; j <= current; j++) label.Append(counts[j].ToString(CultureInfo.InvariantCulture)).Append('.');
                    AddEdit(edits, text, line.Start + indent, old, Pad(label.ToString(), options.Tab, options.PadChars)); // the indentation is kept
                }
                else
                {
                    // Hierarchical: each level numbered in the next style down (1 → a → i → I → A),
                    // a sub-list starting again at 1 under each item; the indentation is kept
                    int level = Math.Max(0, Level(text, line, options.Tab, options.PadChars) - baseLevel);
                    while (counts.Count <= level) counts.Add(0);
                    if (level > previous) for (int l = previous + 1; l <= level; l++) counts[l] = 0;
                    previous = level;
                    counts[level]++;
                    string label = FormatNumber(counts[level], StyleForLevel(options.Style, level), options.FollowChar);
                    AddEdit(edits, text, line.Start + indent, old, Pad(label, options.Tab, options.PadChars));
                }
            }
            edits.Sort((a, b) => a.Start.CompareTo(b.Start));
            return edits;
        }

        /// <summary>
        /// The style for a level of a hierarchical list: from the chosen style, through
        /// 1 → a → i → I → A and round again (Arabic gives 1, a, i…; capital Roman I, A, 1, a, i…).
        /// The same cascade as the editor's live lists.
        /// </summary>
        public static NumberStyle StyleForLevel(NumberStyle style, int level)
        {
            NumberStyle[] cascade = { NumberStyle.RomanCap, NumberStyle.LettersCap, NumberStyle.Arabic, NumberStyle.Letters, NumberStyle.Roman };
            int first = Array.IndexOf(cascade, style);
            if (first < 0) return style; // Legal
            return cascade[(first + Math.Max(0, level)) % cascade.Length];
        }

        /************ Typed lists: recognising and renumbering ************/

        /// <summary>
        /// Renumbers the typed numbered lists that the lines [from, to) are in, after lines have
        /// been indented or outdented (Tab, Increase indent): each list is recognised (consecutive
        /// numbered lines, blank lines between allowed), its style, punctuation, start number and
        /// padding read from its first top-level number, and numbered again level by level (a line
        /// indented under "2." becomes "a."). Lines that aren't in a numbered list are left alone.
        /// </summary>
        public static List<TextEdit> RenumberEdits(string text, int from, int to, NumberOptions defaults)
        {
            var edits = new List<TextEdit>();
            List<Line> all = LinesIn(text, 0, text.Length);
            List<Line> touched = LinesIn(text, from, to);
            if (touched.Count == 0) return edits;
            int firstTouched = all.FindIndex(l => l.Start == touched[0].Start);
            if (firstTouched < 0) return edits;
            int lastTouched = firstTouched + touched.Count - 1;
            bool Numbered(int i) => !IsBlank(text, all[i]) && NumberLength(text, all[i].Start + IndentLength(text, all[i]), all[i].End) > 0;

            int done = -1;
            for (int seed = firstTouched; seed <= lastTouched; seed++)
            {
                if (seed <= done || !Numbered(seed)) continue;
                // The list around the seed: numbered lines, with blank lines between them
                int start = seed, end = seed;
                for (int i = seed - 1; i >= 0; i--) { if (Numbered(i)) start = i; else if (!IsBlank(text, all[i])) break; }
                for (int i = seed + 1; i < all.Count; i++) { if (Numbered(i)) end = i; else if (!IsBlank(text, all[i])) break; }
                done = end;

                NumberOptions o = RecogniseList(text, all, start, end, defaults);
                if (o == null) continue;
                edits.AddRange(NumberEdits(text, all[start].Start, all[end].End, o));
            }
            edits.Sort((a, b) => a.Start.CompareTo(b.Start));
            return edits;
        }

        /// <summary>
        /// A typed list's numbering, read from its first top-level number: style (1, a, A, i, I or
        /// legal 1.1.), punctuation, start, and a tab or spaces after it. Null if unreadable.
        /// </summary>
        internal static NumberOptions RecogniseList(string text, List<Line> all, int start, int end, NumberOptions defaults)
        {
            int topLevel = int.MaxValue, top = -1;
            for (int i = start; i <= end; i++)
            {
                if (IsBlank(text, all[i])) continue;
                int level = Level(text, all[i], defaults.Tab, defaults.PadChars);
                if (level < topLevel) { topLevel = level; top = i; }
            }
            if (top < 0) return null;
            Line line = all[top];
            int at = line.Start + IndentLength(text, line);
            int length = NumberLength(text, at, line.End);
            string marker = text.Substring(at, length);
            string label = marker.TrimEnd(' ', '\t');
            var o = new NumberOptions
            {
                Renumber = true,
                Tab = marker.EndsWith("\t", StringComparison.Ordinal),
                PadChars = marker.EndsWith("\t", StringComparison.Ordinal) ? defaults.PadChars : Math.Max(label.Length + 1, length),
            };

            // Legal (1.1.) if any of its numbers is: the first may be just "1."
            for (int i = start; i <= end; i++)
            {
                if (IsBlank(text, all[i])) continue;
                int a = all[i].Start + IndentLength(text, all[i]);
                string m = text.Substring(a, NumberLength(text, a, all[i].End)).TrimEnd(' ', '\t');
                if (Regex.IsMatch(m, @"^\d+(\.\d+)+\.?$")) { o.Style = NumberStyle.Legal; o.StartNumber = 1; return o; }
            }

            string core = label;
            if (core.StartsWith("(", StringComparison.Ordinal) && core.EndsWith(")", StringComparison.Ordinal)) { o.FollowChar = NumberPunctuation.TwoBrackets; core = core.Substring(1, core.Length - 2); }
            else if (core.EndsWith(")", StringComparison.Ordinal)) { o.FollowChar = NumberPunctuation.Bracket; core = core.Substring(0, core.Length - 1); }
            else if (core.EndsWith(".", StringComparison.Ordinal)) { o.FollowChar = NumberPunctuation.Period; core = core.Substring(0, core.Length - 1); }
            else o.FollowChar = NumberPunctuation.None;
            if (core.Length == 0) return null;

            if (core.All(char.IsDigit)) { o.Style = NumberStyle.Arabic; o.StartNumber = int.Parse(core, CultureInfo.InvariantCulture); }
            else if (IsRomanStart(core))
            {
                o.Style = char.IsUpper(core[0]) ? NumberStyle.RomanCap : NumberStyle.Roman;
                o.StartNumber = FromRoman(core.ToUpperInvariant());
            }
            else if (core.Length == 1 && char.IsLetter(core[0]))
            {
                o.Style = char.IsUpper(core[0]) ? NumberStyle.LettersCap : NumberStyle.Letters;
                o.StartNumber = char.ToUpperInvariant(core[0]) - 'A' + 1;
            }
            else return null;
            return o;
        }

        /// <summary>A Roman numeral rather than a letter: "i"/"I", or several Roman letters ("ii", "iv"); a lone c, d, l, m, v or x is taken as a letter.</summary>
        private static bool IsRomanStart(string core)
        {
            if (core.Equals("i", StringComparison.OrdinalIgnoreCase)) return true;
            if (core.Length < 2) return false;
            bool lower = core.All(c => "ivxlcdm".IndexOf(c) >= 0), upper = core.All(c => "IVXLCDM".IndexOf(c) >= 0);
            return (lower || upper) && ToRoman(FromRoman(core.ToUpperInvariant())) == core.ToUpperInvariant();
        }

        private static int FromRoman(string roman)
        {
            int total = 0;
            for (int i = 0; i < roman.Length; i++)
            {
                int v = RomanValue(roman[i]), next = i + 1 < roman.Length ? RomanValue(roman[i + 1]) : 0;
                total += v < next ? -v : v;
            }
            return Math.Max(1, total);
        }

        private static int RomanValue(char c) => c switch
        {
            'I' => 1, 'V' => 5, 'X' => 10, 'L' => 50, 'C' => 100, 'D' => 500, 'M' => 1000, _ => 0,
        };

        /// <summary>
        /// Takes the numbers off the lines, undoing <see cref="NumberEdits"/>: indentation is kept
        /// (so Legal levels stay as they were), and in a list numbered only at the top level
        /// (1, a, i…), the unnumbered indented lines that numbering moved along by one level are
        /// moved back by the same amount (a tab, or the width of the list's numbers).
        /// </summary>
        public static List<TextEdit> RemoveNumberEdits(string text, int from, int to)
        {
            var edits = new List<TextEdit>();
            var run = new List<(Line Line, int Indent, int Number)>(); // a list: numbered lines, with lines between
            foreach (Line line in LinesIn(text, from, to))
            {
                if (IsBlank(text, line)) continue; // doesn't interrupt a list
                int indent = IndentLength(text, line);
                int number = NumberLength(text, line.Start + indent, line.End);
                if (number == 0 && indent == 0) { EndRun(); continue; } // ordinary text ends a list
                if (number > 0 && indent == 0 && run.Count > 0 && run.TrueForAll(r => r.Number == 0)) run.Clear(); // indented text before the list isn't part of it
                run.Add((line, indent, number));
            }
            EndRun();
            edits.Sort((a, b) => a.Start.CompareTo(b.Start));
            return edits;

            void EndRun()
            {
                // Numbered only at the top level: indented lines were moved along by numbering
                bool topOnly = run.Exists(r => r.Number > 0) && !run.Exists(r => r.Number > 0 && r.Indent > 0);
                bool tab = false; int width = int.MaxValue;
                foreach (var r in run)
                    if (r.Number > 0)
                    {
                        string marker = text.Substring(r.Line.Start + r.Indent, r.Number);
                        if (marker.EndsWith("\t", StringComparison.Ordinal)) tab = true;
                        else width = Math.Min(width, marker.Length);
                    }

                bool seenNumber = false;
                foreach (var r in run)
                {
                    if (r.Number > 0)
                    {
                        seenNumber = true;
                        edits.Add(new TextEdit(r.Line.Start + r.Indent, r.Number, ""));
                    }
                    else if (topOnly && seenNumber)
                    {
                        // Back one level: the tab, or the spaces, that numbering added
                        int shift = tab
                            ? (text[r.Line.Start] == '\t' ? 1 : 0)
                            : CountSpaces(text, r.Line.Start, Math.Min(r.Indent, width == int.MaxValue ? 0 : width));
                        if (shift > 0) edits.Add(new TextEdit(r.Line.Start, shift, ""));
                    }
                }
                run.Clear();
            }
        }

        private static int CountSpaces(string text, int at, int max)
        {
            int n = 0;
            while (n < max && text[at + n] == ' ') n++;
            return n;
        }

        // A number at the start of a line, with what follows it:
        //   1.1 / 1.1. / 2.1.3.  (legal; may run straight into the text, as ClipX 1 padded)
        //   1. / 1) / (1) / 1 + tab or two spaces
        //   a. / a) / (a) / iv. / (IV) and the like, followed by a space or tab
        private static readonly Regex Number = new Regex(
            @"\G(?:\d+(?:\.\d+)+\.?[ \t]*" +
            @"|\(?\d+[.)][ \t]+|\(\d+\)[ \t]+|\d+(?:\t|[ ]{2,})[ \t]*" +
            @"|\(?(?:[a-zA-Z]|[ivxlcdm]{1,7}|[IVXLCDM]{1,7})[.)][ \t]+|\((?:[a-zA-Z]|[ivxlcdm]{1,7}|[IVXLCDM]{1,7})\)[ \t]+" +
            @"|(?:[a-zA-Z]|[ivxlcdm]{1,7}|[IVXLCDM]{1,7})(?:\t|[ ]{2,})[ \t]*)", // a, iv with no punctuation: a tab or two spaces after, as for digits
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>Length of a list number and the spaces or tabs after it at <paramref name="at"/> (0 if none).</summary>
        public static int NumberLength(string text, int at, int end)
        {
            if (at >= end) return 0;
            Match m = Number.Match(text.Substring(0, end), at);
            return m.Success && m.Index == at ? m.Length : 0;
        }

        /// <summary>A number in a style, with its punctuation: 3 → "3.", "c)", "(iii)"…</summary>
        public static string FormatNumber(int n, NumberStyle style, NumberPunctuation punctuation)
        {
            string number;
            switch (style)
            {
                case NumberStyle.Letters: number = ToLetters(n).ToLowerInvariant(); break;
                case NumberStyle.LettersCap: number = ToLetters(n); break;
                case NumberStyle.Roman: number = ToRoman(n).ToLowerInvariant(); break;
                case NumberStyle.RomanCap: number = ToRoman(n); break;
                default: number = n.ToString(CultureInfo.InvariantCulture); break;
            }
            switch (punctuation)
            {
                case NumberPunctuation.Period: return number + ".";
                case NumberPunctuation.Bracket: return number + ")";
                case NumberPunctuation.TwoBrackets: return "(" + number + ")";
                default: return number;
            }
        }

        /// <summary>1 → A, 26 → Z, 27 → AA (as spreadsheet columns).</summary>
        public static string ToLetters(int n)
        {
            if (n <= 0) return n.ToString(CultureInfo.InvariantCulture);
            var sb = new StringBuilder();
            while (n > 0) { n--; sb.Insert(0, (char)('A' + n % 26)); n /= 26; }
            return sb.ToString();
        }

        /// <summary>1 → I, 4 → IV, 1999 → MCMXCIX (numbers outside 1–3999 as digits).</summary>
        public static string ToRoman(int n)
        {
            if (n <= 0 || n >= 4000) return n.ToString(CultureInfo.InvariantCulture);
            int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
            string[] numerals = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
            var sb = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
                while (n >= values[i]) { sb.Append(numerals[i]); n -= values[i]; }
            return sb.ToString();
        }

        /************ Helpers ************/

        private static bool IsBlank(string text, Line line)
        {
            for (int i = line.Start; i < line.End; i++)
                if (!char.IsWhiteSpace(text[i])) return false;
            return true;
        }

        /// <summary>Adds an edit unless it would change nothing.</summary>
        private static void AddEdit(List<TextEdit> edits, string text, int start, int length, string replacement)
        {
            if (length == replacement.Length && string.CompareOrdinal(text, start, replacement, 0, length) == 0) return;
            edits.Add(new TextEdit(start, length, replacement));
        }

        /// <summary>The text with the edits applied (edits in text order, positions in the original).</summary>
        public static string Apply(string text, IReadOnlyList<TextEdit> edits)
        {
            var sb = new StringBuilder(text);
            for (int k = edits.Count - 1; k >= 0; k--)
            {
                TextEdit e = edits[k];
                sb.Remove(e.Start, e.Length).Insert(e.Start, e.Replacement);
            }
            return sb.ToString();
        }
    }
}
