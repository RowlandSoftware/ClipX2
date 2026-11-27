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
        /// soSMA: NISO TR03-1999's order (spaces, symbols, numbers, letters; numbers anywhere by
        /// value: item 2 before item 10), with presets for BS ISO 999 (Hart's) and Chicago; see
        /// <see cref="SmartSortOptions"/>.
        /// </summary>
        Smart,
    }

    /// <summary>Alphabetic sort options (soALP).</summary>
    public sealed class AlphaSortOptions
    {
        /// <summary>Capitals before small letters, as two alphabets: Zebra before apple. Off: case ignored (Apple before apple only when otherwise the same).</summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// Letter by letter: spaces, hyphens and other punctuation ignored (Newark before New York).
        /// Off: word by word, the space before any letter (New York before Newark), as BS ISO 999
        /// for general indexes and NISO TR03-1999.
        /// </summary>
        public bool LetterByLetter { get; set; }

        /// <summary>Which letter-by-letter rule (only when <see cref="LetterByLetter"/>).</summary>
        public LetterByLetterRule LetterRule { get; set; } = LetterByLetterRule.Strict;

        /// <summary>Lines the same as one already there (indentation and runs of spaces aside) go; the first stays.</summary>
        public bool RemoveDuplicates { get; set; }
    }

    /// <summary>The letter-by-letter interpretations of NISO TR03-1999 (Figure 2).</summary>
    public enum LetterByLetterRule
    {
        /// <summary>Strict: every space and punctuation mark ignored, without exception (New, Thomas after newsprint).</summary>
        Strict,
        /// <summary>
        /// Chicago Manual of Style (17th and 18th eds., 15.67): letters count up to the first
        /// parenthesis or comma, then start again after it. A word alone comes first, then the
        /// word followed by a parenthesis, then by a comma, then by a number, then by letters
        /// (New, Thomas before Newark; newton (unit) before Newton, Isaac before Newton's rings).
        /// </summary>
        Chicago,
    }

    /// <summary>How numbers compare (the Numeric sort; also the number parts of other sorts).</summary>
    public enum NumberOrder
    {
        /// <summary>By value, decimals too (8 before 10; 1.10 before 1.5; Dewey 004.16 before 004.6). Sign and thousands commas read.</summary>
        Value,
        /// <summary>Version numbers: each part between the points by value (1.5 before 1.10, 1.2 before 1.2.1).</summary>
        Version,
        /// <summary>Digit by digit, as text (lexical): 10 before 8; codes whose digits aren't amounts.</summary>
        DigitByDigit,
    }

    /// <summary>Numeric sort options (soNUM).</summary>
    public sealed class NumberSortOptions
    {
        public NumberOrder Order { get; set; } = NumberOrder.Value;
    }

    /// <summary>
    /// The Sorting tools: sort, reverse or nudge whole lines (paragraphs), and sort the comma
    /// separated items within a line. Each line is an entity: it moves whole. The selection is
    /// widened to whole lines; with none, the whole text (nudging: the line with the caret).
    /// <see cref="Order(string, IReadOnlyList{Line}, IComparer{string})"/> gives the new order of the lines, so an editor can move them with their
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
        public static int[] Order(string text, IReadOnlyList<Line> lines, SortKind kind, CultureInfo culture = null) =>
            Order(text, lines, Comparer(kind, culture));

        /// <summary>The new order with a comparer of the caller's (a sort's options).</summary>
        public static int[] Order(string text, IReadOnlyList<Line> lines, IComparer<string> comparer) =>
            Order(text, lines, comparer, removeDuplicates: false, out _);

        /// <summary>
        /// The new order; with <paramref name="removeDuplicates"/>, a line the same as one already
        /// kept is left out too (the same text, indentation and runs of spaces aside; the first
        /// of them, in the text's order, stays). <paramref name="duplicates"/>: how many went.
        /// </summary>
        public static int[] Order(string text, IReadOnlyList<Line> lines, IComparer<string> comparer, bool removeDuplicates, out int duplicates)
        {
            IEnumerable<int> kept = Enumerable.Range(0, lines.Count).Where(i => !IsBlank(text, lines[i]));
            duplicates = 0;
            if (removeDuplicates)
            {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                var unique = new List<int>();
                foreach (int i in kept)
                {
                    if (seen.Add(AlphaComparer.Normalise(Key(text, lines[i])))) unique.Add(i);
                    else duplicates++;
                }
                kept = unique;
            }
            int[] order = kept.OrderBy(i => Key(text, lines[i]), comparer).ToArray(); // OrderBy is stable
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

        /// <summary>The comparer for a kind of sort, with its options (null: the defaults).</summary>
        public static IComparer<string> Comparer(SortKind kind, CultureInfo culture = null, object options = null)
        {
            CompareInfo compare = (culture ?? CultureInfo.CurrentCulture).CompareInfo;
            switch (kind)
            {
                case SortKind.Numeric: return new NumericComparer(options as NumberSortOptions, compare);
                case SortKind.Smart: return new SmartComparer(options as SmartSortOptions, compare);
                default: return new AlphaComparer(options as AlphaSortOptions, compare);
            }
        }

        /// <summary>
        /// Alphabetical, in the language's own order (é with e), runs of spaces counting as one.
        /// Word by word (the space before any letter) or letter by letter (spaces and punctuation
        /// ignored, strictly or as Chicago has it); case ignored (capitals first only when otherwise the same) or case-sensitive
        /// (capitals before small letters, as two alphabets). For the Alphabetic sort, and the
        /// letters of others (Smart sort).
        /// </summary>
        public sealed class AlphaComparer : IComparer<string>
        {
            private readonly CompareInfo _c;
            private readonly bool _caseSensitive, _letterByLetter, _chicago;

            public AlphaComparer(AlphaSortOptions options = null, CompareInfo compare = null)
            {
                _c = compare ?? CultureInfo.CurrentCulture.CompareInfo;
                _caseSensitive = options?.CaseSensitive ?? false;
                _letterByLetter = options?.LetterByLetter ?? false;
                _chicago = _letterByLetter && options.LetterRule == LetterByLetterRule.Chicago;
            }

            public int Compare(string a, string b)
            {
                string oa = a ?? "", ob = b ?? "";
                a = Normalise(oa); b = Normalise(ob);
                if (!_letterByLetter) { a = WordByWord(a); b = WordByWord(b); }
                int r = _chicago ? ChicagoPrimary(a, b) : Primary(a, b);
                if (r != 0) return r;
                // Ties: case last of all (Apple before apple), then accents, then the original text.
                r = _caseSensitive ? _c.Compare(a, b, CompareOptions.None) : CapitalsFirst(a, b);
                return r != 0 ? r : string.CompareOrdinal(oa, ob);
            }

            /// <summary>The main comparison: letters (and digits), case ignored or as two alphabets; no tie-breaks.</summary>
            private int Primary(string a, string b)
            {
                if (!_caseSensitive)
                    return _c.Compare(a, b, CompareOptions.IgnoreCase | (_letterByLetter ? CompareOptions.IgnoreSymbols : CompareOptions.None));
                if (_letterByLetter) { a = LettersAndDigits(a); b = LettersAndDigits(b); }
                return CapitalsAsAlphabet(a, b);
            }

            /// <summary>
            /// Chicago letter by letter: the letters up to the first parenthesis or comma compare
            /// first; when they are the same, nothing more comes first, then a parenthesis, then a
            /// comma; then what follows compares the same way.
            /// </summary>
            private int ChicagoPrimary(string a, string b)
            {
                while (true)
                {
                    int ma = ChicagoBreak(a, out string headA, out string restA);
                    int mb = ChicagoBreak(b, out string headB, out string restB);
                    int r = Primary(headA, headB);
                    if (r != 0) return r;
                    r = ma.CompareTo(mb);
                    if (r != 0 || ma == 0) return r;
                    a = restA; b = restB;
                }
            }

            /// <summary>
            /// Splits at the first parenthesis or comma (not a comma between digits, as in 1,250):
            /// 0 none, 1 a parenthesis, 2 a comma.
            /// </summary>
            private static int ChicagoBreak(string s, out string head, out string rest)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    char ch = s[i];
                    bool paren = ch == '(';
                    bool comma = ch == ',' && !(i > 0 && i + 1 < s.Length && char.IsDigit(s[i - 1]) && char.IsDigit(s[i + 1]));
                    if (!paren && !comma) continue;
                    head = s.Substring(0, i);
                    rest = s.Substring(i + 1);
                    return paren ? 1 : 2;
                }
                head = s; rest = "";
                return 0;
            }

            /// <summary>Runs of spaces and tabs as one space; none at the ends.</summary>
            public static string Normalise(string s)
            {
                var sb = new System.Text.StringBuilder(s.Length);
                foreach (char ch in s)
                {
                    if (ch == ' ' || ch == '\t' || ch == ' ') { if (sb.Length > 0 && sb[sb.Length - 1] != ' ') sb.Append(' '); }
                    else sb.Append(ch);
                }
                return sb.ToString().TrimEnd(' ');
            }

            /// <summary>
            /// Word by word as NISO TR03-1999 (and Hart's) have it: a hyphen, dash or slash counts as
            /// a space (client/server and client-side as client server, client side); full stops,
            /// commas, semicolons, colons, brackets, apostrophes, quotation marks, ! and ? are
            /// ignored (O'Brien as OBrien), except a point or comma between digits (1.5, 1,250).
            /// </summary>
            public static string WordByWord(string s)
            {
                var sb = new System.Text.StringBuilder(s.Length);
                for (int i = 0; i < s.Length; i++)
                {
                    char ch = s[i];
                    if (ch == '-' || ch == '/' || (ch >= '‐' && ch <= '―') || ch == '−') { sb.Append(' '); continue; } // hyphens, dashes, minus
                    bool betweenDigits = i > 0 && i + 1 < s.Length && char.IsDigit(s[i - 1]) && char.IsDigit(s[i + 1]);
                    if ((ch == '.' || ch == ',') && betweenDigits) { sb.Append(ch); continue; }
                    if (".,;:()[]<>{}'\"!?‘’“”".IndexOf(ch) >= 0) continue; // and curly quotes
                    sb.Append(ch);
                }
                return Normalise(sb.ToString());
            }

            private static string LettersAndDigits(string s) => new string(s.Where(char.IsLetterOrDigit).ToArray());

            /// <summary>The first place where two otherwise equal texts differ in case: the capital first.</summary>
            private static int CapitalsFirst(string a, string b)
            {
                for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
                {
                    char x = a[i], y = b[i];
                    if (x == y || char.ToLowerInvariant(x) != char.ToLowerInvariant(y)) continue;
                    if (char.IsUpper(x) != char.IsUpper(y)) return char.IsUpper(x) ? -1 : 1;
                }
                return 0;
            }

            /// <summary>
            /// Case-sensitive: character by character, a capital before any small letter (Zebra
            /// before apple); letters of the same case in the language's order, accents aside;
            /// anything else (spaces, digits, punctuation) in the language's order, before letters.
            /// </summary>
            private int CapitalsAsAlphabet(string a, string b)
            {
                int n = Math.Min(a.Length, b.Length);
                for (int i = 0; i < n; i++)
                {
                    char x = a[i], y = b[i];
                    if (x == y) continue;
                    if (char.IsLetter(x) && char.IsLetter(y))
                    {
                        bool ux = char.IsUpper(x), uy = char.IsUpper(y);
                        if (ux != uy) return ux ? -1 : 1;
                    }
                    int r = _c.Compare(x.ToString(), y.ToString(), CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
                    if (r != 0) return r;
                }
                return a.Length.CompareTo(b.Length);
            }
        }

        /// <summary>
        /// Numeric sort: by the number each line starts with, in the chosen <see cref="NumberOrder"/>;
        /// equal numbers, and lines without one (after the rest), alphabetically.
        /// </summary>
        private sealed class NumericComparer : IComparer<string>
        {
            private readonly NumberOrder _order;
            private readonly AlphaComparer _alpha;
            public NumericComparer(NumberSortOptions options, CompareInfo c)
            {
                _order = options?.Order ?? NumberOrder.Value;
                _alpha = new AlphaComparer(null, c);
            }
            public int Compare(string a, string b)
            {
                int la = NumberOrdering.Read(a, 0, _order), lb = NumberOrdering.Read(b, 0, _order);
                if ((la > 0) != (lb > 0)) return la > 0 ? -1 : 1;
                if (la > 0)
                {
                    int r = NumberOrdering.Compare(a.Substring(0, la), b.Substring(0, lb), _order);
                    if (r != 0) return r;
                }
                return _alpha.Compare(a, b);
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

        public static List<TextEdit> Sort(string text, int from, int to, SortKind kind, object options = null)
        {
            List<Line> lines = Scope(text, from, to);
            return Reorder(text, lines, Order(text, lines, Comparer(kind, null, options), RemovesDuplicates(options), out _));
        }

        /// <summary>Whether a sort's options ask for duplicate lines to go.</summary>
        public static bool RemovesDuplicates(object options) =>
            options is AlphaSortOptions { RemoveDuplicates: true } || options is SmartSortOptions { RemoveDuplicates: true };

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

    /// <summary>
    /// Numbers in text, read and compared in a <see cref="NumberOrder"/>: the Numeric sort's,
    /// and for the numbers inside lines in other sorts (Smart sort).
    /// </summary>
    public static class NumberOrdering
    {
        /// <summary>
        /// The length of the number at <paramref name="at"/> as <paramref name="order"/> reads it,
        /// 0 if there is none. Value: a sign, digits with thousands commas (1,250), a decimal
        /// point (-3.5). Version: digits and the points between them (1.10.2). Digit by digit:
        /// digits, with points between them (004.16).
        /// </summary>
        public static int Read(string s, int at, NumberOrder order)
        {
            int i = at;
            if (order == NumberOrder.Value && i < s.Length && (s[i] == '-' || s[i] == '+' || s[i] == '−') && i + 1 < s.Length && (char.IsDigit(s[i + 1]) || s[i + 1] == '.')) i++;
            int start = i;
            bool point = false, any = false;
            for (; i < s.Length; i++)
            {
                char ch = s[i];
                if (ch >= '0' && ch <= '9') { any = true; continue; }
                bool digitNext = i + 1 < s.Length && s[i + 1] >= '0' && s[i + 1] <= '9';
                if (order == NumberOrder.Value && ch == ',' && !point && any && IsThousands(s, i)) continue;
                if (ch == '.' && digitNext && (order != NumberOrder.Value || !point)) { point = true; continue; }
                break;
            }
            return any ? i - at : 0;
        }

        private static bool IsThousands(string s, int comma)
        {
            for (int k = comma + 1; k <= comma + 3; k++) if (k >= s.Length || !char.IsDigit(s[k])) return false;
            return comma + 4 >= s.Length || !char.IsDigit(s[comma + 4]);
        }

        /// <summary>Compares two numbers as <see cref="Read"/> found them.</summary>
        public static int Compare(string x, string y, NumberOrder order)
        {
            switch (order)
            {
                case NumberOrder.Version:
                    string[] px = x.Split('.'), py = y.Split('.');
                    for (int k = 0; k < Math.Min(px.Length, py.Length); k++)
                    {
                        int r = CompareDigits(px[k], py[k]);
                        if (r != 0) return r;
                    }
                    return px.Length.CompareTo(py.Length); // 1.2 before 1.2.1
                case NumberOrder.DigitByDigit:
                    return string.CompareOrdinal(x, y);
                default:
                    return Value(x).CompareTo(Value(y));
            }
        }

        /// <summary>Whole numbers of any length by value: leading zeros aside, the longer is larger, then digit by digit.</summary>
        public static int CompareDigits(string x, string y)
        {
            x = x.TrimStart('0'); y = y.TrimStart('0');
            return x.Length != y.Length ? x.Length.CompareTo(y.Length) : string.CompareOrdinal(x, y);
        }

        /// <summary>A number as Read found it (Value): its value; very long ones approximately.</summary>
        public static double Value(string s)
        {
            string t = s.Replace(",", "").Replace('−', '-');
            return double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : 0;
        }
    }
}
