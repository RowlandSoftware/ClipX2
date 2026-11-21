using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Line = ClipX2.TextEngine.ListFormatter.Line;

namespace ClipX2.TextEngine
{
    /// <summary>How lines are compared when sorting.</summary>
    public enum SortKind
    {
        /// <summary>soALP: alphabetical, ignoring case and leading spaces, in the language's own order (é with e).</summary>
        Alphabetic,
        /// <summary>soNUM: by the number each line starts with (lines without one after, alphabetically).</summary>
        Numeric,
        /// <summary>
        /// soSMA: "smart" (natural) order: numbers inside lines compare as numbers (item 2 before
        /// item 10); lines starting with a symbol, then a number, then a letter.
        /// TODO (Andrew): check against the Oracle Style Guide.
        /// </summary>
        Smart,
    }

    /// <summary>
    /// The Sorting tools: sort, reverse or nudge whole lines (paragraphs), and sort the comma
    /// separated items within a line. Each line is an entity: it moves whole. The selection is
    /// widened to whole lines; with none, the whole text (nudging: the line with the caret).
    /// <see cref="Order"/> gives the new order of the lines, so an editor can move them with their
    /// formatting; the Edits methods give the plain-text result as one edit.
    /// </summary>
    public static class LineSorter
    {
        /************ The order ************/

        /// <summary>
        /// The new order of <paramref name="lines"/>: indexes into it, first to last. Blank lines
        /// are left out (as ClipX 1: they go). Equal lines keep their order. Null if the lines are
        /// already in order and there are no blank ones to remove.
        /// </summary>
        public static int[] Order(string text, IReadOnlyList<Line> lines, SortKind kind, CultureInfo culture = null)
        {
            IComparer<string> comparer = Comparer(kind, culture);
            int[] order = Enumerable.Range(0, lines.Count)
                .Where(i => !IsBlank(text, lines[i]))
                .OrderBy(i => Key(text, lines[i]), comparer) // OrderBy is stable
                .ToArray();
            return order.Length == lines.Count && order.Select((v, i) => v == i).All(b => b) ? null : order;
        }

        /// <summary>The lines in reverse order (blank ones too: they are part of the order).</summary>
        public static int[] ReverseOrder(IReadOnlyList<Line> lines) =>
            lines.Count < 2 ? null : Enumerable.Range(0, lines.Count).Reverse().ToArray();

        /// <summary>A line's sort key: its text after the indentation.</summary>
        private static string Key(string text, Line line)
        {
            int i = line.Start;
            while (i < line.End && char.IsWhiteSpace(text[i])) i++;
            return text.Substring(i, line.End - i);
        }

        private static bool IsBlank(string text, Line line)
        {
            for (int i = line.Start; i < line.End; i++) if (!char.IsWhiteSpace(text[i])) return false;
            return true;
        }

        /// <summary>The comparer for a kind of sort.</summary>
        public static IComparer<string> Comparer(SortKind kind, CultureInfo culture = null)
        {
            CompareInfo compare = (culture ?? CultureInfo.CurrentCulture).CompareInfo;
            switch (kind)
            {
                case SortKind.Numeric: return new NumericComparer(compare);
                case SortKind.Smart: return new SmartComparer(compare);
                default: return new AlphaComparer(compare);
            }
        }

        private sealed class AlphaComparer : IComparer<string>
        {
            private readonly CompareInfo _c;
            public AlphaComparer(CompareInfo c) { _c = c; }
            public int Compare(string a, string b) => _c.Compare(a, b, CompareOptions.IgnoreCase);
        }

        /// <summary>By the number at the start (sign, decimal point, thousands commas); lines without one after the rest, alphabetically.</summary>
        private sealed class NumericComparer : IComparer<string>
        {
            private readonly CompareInfo _c;
            public NumericComparer(CompareInfo c) { _c = c; }
            public int Compare(string a, string b)
            {
                bool hasA = LeadingNumber(a, out double x), hasB = LeadingNumber(b, out double y);
                if (hasA != hasB) return hasA ? -1 : 1;
                if (hasA && x != y) return x.CompareTo(y);
                return _c.Compare(a, b, CompareOptions.IgnoreCase);
            }
        }

        /// <summary>The number a line starts with: "-3.5", "1,250", "42."; false if none.</summary>
        public static bool LeadingNumber(string s, out double value)
        {
            value = 0;
            int i = 0;
            bool negative = false;
            if (i < s.Length && (s[i] == '-' || s[i] == '+' || s[i] == (char)0x2212)) { negative = s[i] != '+'; i++; }
            var digits = new System.Text.StringBuilder();
            bool point = false;
            for (; i < s.Length; i++)
            {
                char ch = s[i];
                bool thousands = ch == ',' && !point && digits.Length > 0
                    && IsDigits(s, i + 1, 3) && (i + 4 >= s.Length || !char.IsDigit(s[i + 4])); // 1,250
                if (ch >= '0' && ch <= '9') digits.Append(ch);
                else if (thousands) continue;
                else if (ch == '.' && !point && i + 1 < s.Length && char.IsDigit(s[i + 1])) { point = true; digits.Append('.'); }
                else break;
            }
            if (digits.Length == 0) return false;
            value = double.Parse(digits.ToString(), CultureInfo.InvariantCulture) * (negative ? -1 : 1);
            return true;
        }

        private static bool IsDigits(string s, int at, int count)
        {
            if (at + count > s.Length) return false;
            for (int k = at; k < at + count; k++) if (!char.IsDigit(s[k])) return false;
            return true;
        }

        /// <summary>
        /// Natural order: lines starting with a symbol, then with a number, then with a letter;
        /// within that, runs of digits compare by value (2 before 10; 007 after 7 when otherwise
        /// equal) and the rest alphabetically, ignoring case.
        /// </summary>
        public sealed class SmartComparer : IComparer<string>
        {
            private readonly CompareInfo _c;
            public SmartComparer(CompareInfo compare = null) { _c = compare ?? CultureInfo.CurrentCulture.CompareInfo; }

            private static int Group(string s) =>
                s.Length == 0 ? 0 : char.IsDigit(s[0]) ? 1 : char.IsLetter(s[0]) ? 2 : 0;

            public int Compare(string a, string b)
            {
                int ga = Group(a), gb = Group(b);
                if (ga != gb) return ga.CompareTo(gb);
                int i = 0, j = 0;
                while (i < a.Length && j < b.Length)
                {
                    bool da = char.IsDigit(a[i]), db = char.IsDigit(b[j]);
                    int ei = i, ej = j;
                    while (ei < a.Length && char.IsDigit(a[ei]) == da) ei++;
                    while (ej < b.Length && char.IsDigit(b[ej]) == db) ej++;
                    int result;
                    if (da && db)
                    {
                        // By value: ignore leading zeros, then the longer is bigger, then digit by digit
                        string na = a.Substring(i, ei - i).TrimStart('0'), nb = b.Substring(j, ej - j).TrimStart('0');
                        result = na.Length != nb.Length ? na.Length.CompareTo(nb.Length) : string.CompareOrdinal(na, nb);
                    }
                    else if (da != db) result = da ? -1 : 1; // a number before text at the same place
                    else result = _c.Compare(a.Substring(i, ei - i), b.Substring(j, ej - j), CompareOptions.IgnoreCase);
                    if (result != 0) return result;
                    i = ei; j = ej;
                }
                int left = (a.Length - i).CompareTo(b.Length - j); // the shorter first
                if (left != 0) return left;
                int whole = _c.Compare(a, b, CompareOptions.IgnoreCase);  // equal so far: 007 against 7
                return whole != 0 ? whole : string.CompareOrdinal(a, b);
            }
        }

        /************ Nudging ************/

        /// <summary>
        /// Nudge up / down (soNUP, soNUD): the lines touching [from, to) swap places with the line
        /// above / below. Returns the block of lines affected and its new order (null if already at
        /// the top / bottom), and how far the selection moves.
        /// </summary>
        public static (List<Line> Block, int[] Order, int Shift)? Nudge(string text, int from, int to, bool up)
        {
            List<Line> all = ListFormatter.LinesIn(text, 0, text.Length);
            List<Line> chosen = ListFormatter.LinesIn(text, from, to);
            if (chosen.Count == 0) return null;
            int first = all.FindIndex(l => l.Start == chosen[0].Start);
            int last = first + chosen.Count - 1;
            if (first < 0) return null;
            if (up)
            {
                if (first == 0) return null;
                var block = all.GetRange(first - 1, chosen.Count + 1);
                int[] order = Enumerable.Range(1, chosen.Count).Concat(new[] { 0 }).ToArray();
                return (block, order, -(all[first - 1].Length + 1));
            }
            else
            {
                if (last >= all.Count - 1) return null;
                var block = all.GetRange(first, chosen.Count + 1);
                int[] order = new[] { chosen.Count }.Concat(Enumerable.Range(0, chosen.Count)).ToArray();
                return (block, order, all[last + 1].Length + 1);
            }
        }

        /************ Plain-text results (the engine's TextTools, PowerButtons) ************/

        /// <summary>The block of lines rewritten in a new order, as one edit (none if the order is null).</summary>
        public static List<TextEdit> Reorder(string text, IReadOnlyList<Line> lines, int[] order)
        {
            var edits = new List<TextEdit>();
            if (order == null || lines.Count == 0) return edits;
            int start = lines[0].Start, end = lines[lines.Count - 1].End;
            string newBlock = string.Join("\n", order.Select(i => text.Substring(lines[i].Start, lines[i].Length)));
            if (newBlock != text.Substring(start, end - start)) edits.Add(new TextEdit(start, end - start, newBlock));
            return edits;
        }

        /// <summary>The lines to sort or reverse: the whole lines of [from, to), or of the whole text if from = to.</summary>
        public static List<Line> Scope(string text, int from, int to) =>
            ListFormatter.LinesIn(text, from == to ? 0 : from, from == to ? text.Length : to);

        public static List<TextEdit> Sort(string text, int from, int to, SortKind kind)
        {
            List<Line> lines = Scope(text, from, to);
            return Reorder(text, lines, Order(text, lines, kind));
        }

        public static List<TextEdit> Reverse(string text, int from, int to)
        {
            List<Line> lines = Scope(text, from, to);
            return Reorder(text, lines, ReverseOrder(lines));
        }

        public static List<TextEdit> NudgeEdits(string text, int from, int to, bool up)
        {
            var n = Nudge(text, from, to, up);
            return n == null ? new List<TextEdit>() : Reorder(text, n.Value.Block, n.Value.Order);
        }

        /************ Items within a line (soKEY) ************/

        /// <summary>
        /// List sort (soKEY): on each line with commas, the items between them sorted
        /// alphabetically and rejoined with ", " (as ClipX 1); the line's indentation is kept.
        /// Lines without a comma are left alone.
        /// </summary>
        public static List<TextEdit> SortItems(string text, int from, int to, CultureInfo culture = null)
        {
            var edits = new List<TextEdit>();
            IComparer<string> comparer = Comparer(SortKind.Alphabetic, culture);
            foreach (Line line in Scope(text, from, to))
            {
                int start = line.Start;
                while (start < line.End && char.IsWhiteSpace(text[start])) start++;
                string content = text.Substring(start, line.End - start);
                if (content.IndexOf(',') < 0) continue;
                string sorted = string.Join(", ", content.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).OrderBy(s => s, comparer));
                if (sorted != content) edits.Add(new TextEdit(start, line.End - start, sorted));
            }
            return edits;
        }
    }
}
