using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ClipX2.TextEngine
{
    /// <summary>The Smart sort's presets: sets of its options that follow a standard or guide.</summary>
    public enum SmartSortPreset
    {
        /// <summary>Options that match no preset.</summary>
        Custom,
        /// <summary>NISO TR03-1999: word by word; commas and brackets ignored, their words counting as any others.</summary>
        NisoTr03,
        /// <summary>BS ISO 999 as Hart's has it, word by word: a comma ends the first part; text in brackets ignored; Mc as Mac.</summary>
        BsIso999WordByWord,
        /// <summary>BS ISO 999 as Hart's has it, letter by letter: the same, spaces and punctuation ignored.</summary>
        BsIso999LetterByLetter,
        /// <summary>Chicago Manual of Style, word by word: a parenthesis, then a comma, ends the first part.</summary>
        ChicagoWordByWord,
        /// <summary>Chicago Manual of Style, letter by letter (15.67): the same, spaces and other punctuation ignored.</summary>
        ChicagoLetterByLetter,
    }

    /// <summary>
    /// Smart sort options (soSMA): the order of characters is always NISO TR03-1999's (spaces,
    /// then symbols, then numbers, then letters; numbers anywhere in a line compare as numbers);
    /// these choose how words, punctuation and names count. A preset sets the rule options
    /// together; case and duplicates are separate.
    /// </summary>
    public sealed class SmartSortOptions
    {
        /// <summary>Letter by letter: spaces, hyphens and punctuation ignored. Off: word by word (nothing before something).</summary>
        public bool LetterByLetter { get; set; }

        /// <summary>
        /// A comma ends a heading's first part (inverted headings: High, J. as High, then J.): the
        /// first part alone comes first, then followed by a comma, then by more words. Unless text
        /// in brackets is ignored, an opening parenthesis does the same, before the comma (Chicago).
        /// Off: commas and brackets are ignored, as NISO TR03.
        /// </summary>
        public bool StopAtComma { get; set; }

        /// <summary>Text in (round) or [square] brackets doesn't count, except to break a tie: high (light-headed) as high.</summary>
        public bool IgnoreBracketed { get; set; }

        /// <summary>Mc counts as Mac (McDonald with MacDonald).</summary>
        public bool McAsMac { get; set; }

        /// <summary>St, St. and Ste before a name count as Saint and Sainte (St Albans with Saint Albans).</summary>
        public bool StAsSaint { get; set; }

        /// <summary>
        /// Initialisms said as letters (BBC, A.B.C.) sort as separate letters, before the words
        /// that start with the same letter: BBC before Bach (Hart's; TR03's figures). Ones said as
        /// words (NATO, UNESCO) sort as words. See <see cref="Initialisms"/>.
        /// </summary>
        public bool InitialismsAsLetters { get; set; }

        /// <summary>How numbers compare, as the Numeric sort's choice.</summary>
        public NumberOrder Numbers { get; set; } = NumberOrder.Value;

        /// <summary>Capitals before small letters, as two alphabets. Off: case equal (capitals first only in a tie).</summary>
        public bool CaseSensitive { get; set; }

        /// <summary>Lines the same as one already there (indentation and runs of spaces aside) go; the first stays.</summary>
        public bool RemoveDuplicates { get; set; }

        /// <summary>The options a preset sets (Custom: NISO TR03's). Case, duplicates and numbers are left as they are.</summary>
        public void Apply(SmartSortPreset preset)
        {
            bool letter = preset == SmartSortPreset.BsIso999LetterByLetter || preset == SmartSortPreset.ChicagoLetterByLetter;
            bool bs = preset == SmartSortPreset.BsIso999WordByWord || preset == SmartSortPreset.BsIso999LetterByLetter;
            bool chicago = preset == SmartSortPreset.ChicagoWordByWord || preset == SmartSortPreset.ChicagoLetterByLetter;
            LetterByLetter = letter;
            StopAtComma = bs || chicago;
            IgnoreBracketed = bs;
            McAsMac = bs;
            StAsSaint = false;
            InitialismsAsLetters = bs;
        }

        /// <summary>The preset these options follow (Custom if none).</summary>
        public SmartSortPreset Preset()
        {
            foreach (SmartSortPreset p in new[] { SmartSortPreset.NisoTr03, SmartSortPreset.BsIso999WordByWord, SmartSortPreset.BsIso999LetterByLetter,
                                                  SmartSortPreset.ChicagoWordByWord, SmartSortPreset.ChicagoLetterByLetter })
            {
                var o = new SmartSortOptions();
                o.Apply(p);
                if (o.LetterByLetter == LetterByLetter && o.StopAtComma == StopAtComma && o.IgnoreBracketed == IgnoreBracketed
                    && o.McAsMac == McAsMac && o.StAsSaint == StAsSaint && o.InitialismsAsLetters == InitialismsAsLetters) return p;
            }
            return SmartSortPreset.Custom;
        }
    }

    /// <summary>
    /// The Smart sort's order (NISO TR03-1999, with the options of <see cref="SmartSortOptions"/>).
    /// Each line is read as a row of units: spaces (a hyphen, dash or slash counts as one; runs
    /// as one), symbols (a run of them as one; all equal), numbers (by value, or as the options
    /// say) and words; full stops, commas, colons, semicolons, brackets, apostrophes, quotation
    /// marks, ! and ? are ignored. Units compare in that order: the end of a line first (nothing
    /// before something), then a parenthesis or comma that ends the first part (when they do),
    /// then a space, a symbol, a number, a word. Words compare in the language's alphabet, accents
    /// aside. Ties: text in brackets (when ignored), then capitals first, accents, the text itself.
    /// </summary>
    public sealed class SmartComparer : IComparer<string>
    {
        private const int Paren = 1, Comma = 2, Space = 3, Symbol = 4, Number = 5, Letters = 6;

        private readonly struct Unit
        {
            public readonly int Kind;
            public readonly string Text;
            public Unit(int kind, string text) { Kind = kind; Text = text; }
        }

        private readonly CompareInfo _c;
        private readonly SmartSortOptions _o;
        private readonly Dictionary<string, Unit[]> _keys = new Dictionary<string, Unit[]>(StringComparer.Ordinal);
        private readonly Dictionary<string, Unit[]> _fullKeys = new Dictionary<string, Unit[]>(StringComparer.Ordinal);

        public SmartComparer(SmartSortOptions options = null, CompareInfo compare = null)
        {
            _o = options ?? new SmartSortOptions();
            _c = compare ?? CultureInfo.CurrentCulture.CompareInfo;
        }

        public int Compare(string a, string b)
        {
            a ??= ""; b ??= "";
            int r = Compare(Key(a, _keys, _o.IgnoreBracketed), Key(b, _keys, _o.IgnoreBracketed));
            if (r != 0) return r;
            if (_o.IgnoreBracketed)
            {
                r = Compare(Key(a, _fullKeys, false), Key(b, _fullKeys, false)); // the bracketed text breaks the tie
                if (r != 0) return r;
            }
            string na = LineSorter.AlphaComparer.Normalise(a), nb = LineSorter.AlphaComparer.Normalise(b);
            if (!_o.CaseSensitive)
            {
                r = CapitalsFirst(na, nb); // Apple before apple
                if (r != 0) return r;
            }
            r = _c.Compare(na, nb, CompareOptions.None); // accents, punctuation
            return r != 0 ? r : string.CompareOrdinal(a, b);
        }

        private int Compare(Unit[] x, Unit[] y)
        {
            for (int i = 0; i < Math.Min(x.Length, y.Length); i++)
            {
                Unit u = x[i], v = y[i];
                if (u.Kind != v.Kind) return u.Kind.CompareTo(v.Kind);
                int r = u.Kind switch
                {
                    Number => NumberOrdering.Compare(u.Text, v.Text, _o.Numbers),
                    Letters => CompareWords(u.Text, v.Text),
                    _ => 0, // spaces, symbols, breaks: all alike
                };
                if (r != 0) return r;
            }
            return x.Length.CompareTo(y.Length); // nothing before something
        }

        private int CompareWords(string x, string y)
        {
            if (!_o.CaseSensitive) return _c.Compare(x, y, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
            for (int i = 0; i < Math.Min(x.Length, y.Length); i++)
            {
                char p = x[i], q = y[i];
                if (p == q) continue;
                bool up = char.IsUpper(p), uq = char.IsUpper(q);
                if (up != uq) return up ? -1 : 1; // capitals as an alphabet before the small letters
                int r = _c.Compare(p.ToString(), q.ToString(), CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
                if (r != 0) return r;
            }
            return x.Length.CompareTo(y.Length);
        }

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

        /************ Reading a line into units ************/

        private static readonly Regex Bracketed = new Regex(@"\([^()]*\)|\[[^\[\]]*\]", RegexOptions.Compiled);
        private static readonly Regex Mc = new Regex(@"\bMc(?=\p{L})", RegexOptions.Compiled);
        /// <summary>Two to seven capitals, as a whole word, with or without full stops: BBC, A.B.C., NATO.</summary>
        private static readonly Regex Initialism = new Regex(@"\G(?:\p{Lu}\.?){2,7}(?![\p{L}\p{N}])", RegexOptions.Compiled);

        private static readonly Regex Saint = new Regex(@"\b(St|Ste)\.?(?=\s+\p{Lu})", RegexOptions.Compiled);

        private Unit[] Key(string s, Dictionary<string, Unit[]> cache, bool dropBracketed)
        {
            if (cache.TryGetValue(s, out Unit[] key)) return key;
            string t = s;
            if (dropBracketed) t = Bracketed.Replace(t, " ").Trim();
            if (_o.McAsMac) t = Mc.Replace(t, "Mac");
            if (_o.StAsSaint) t = Saint.Replace(t, m => m.Groups[1].Value == "Ste" ? "Sainte" : "Saint");
            key = Read(t).ToArray();
            cache[s] = key;
            return key;
        }

        /// <summary>Punctuation that doesn't count (NISO TR03 3.3), and isn't a space either.</summary>
        private const string Ignored = ".,;:()[]<>{}'\"!?‘’‚“”„«»‹›…";

        private static bool IsSpaceLike(char ch) =>
            char.IsWhiteSpace(ch) || ch == '-' || ch == '/' || (ch >= '‐' && ch <= '―') || ch == '−' || ch == '⁄';

        private List<Unit> Read(string s)
        {
            var units = new List<Unit>();
            var word = new StringBuilder();
            bool inWord = false; // the last unit is a word still open (ignored punctuation doesn't close it)

            void Close() { if (inWord) { units.Add(new Unit(Letters, word.ToString())); word.Clear(); inWord = false; } }
            void Add(int kind, string text = "")
            {
                Close();
                if (units.Count > 0)
                {
                    int last = units[units.Count - 1].Kind;
                    if (kind == Space && (last == Space || last == Paren || last == Comma)) return; // runs as one; none after a break
                    if (kind == Symbol && last == Symbol) return; // a run of symbols as one
                    if ((kind == Paren || kind == Comma) && last == Space) units.RemoveAt(units.Count - 1);
                }
                units.Add(new Unit(kind, text));
            }

            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                bool startOfWord = i == 0 || IsSpaceLike(s[i - 1]);

                // A number: digits; a point and digits (.300); a sign before digits, starting a word
                bool sign = (ch == '-' || ch == '+' || ch == '−') && startOfWord && _o.Numbers == NumberOrder.Value
                            && i + 1 < s.Length && (char.IsDigit(s[i + 1]) || (s[i + 1] == '.' && i + 2 < s.Length && char.IsDigit(s[i + 2])));
                bool point = ch == '.' && startOfWord && i + 1 < s.Length && char.IsDigit(s[i + 1]);
                if (char.IsDigit(ch) || sign || point)
                {
                    int len = NumberOrdering.Read(s, i, _o.Numbers);
                    if (len > 0)
                    {
                        Add(Number, s.Substring(i, len));
                        i += len - 1;
                        continue;
                    }
                }
                // An initialism said as letters (BBC, A.B.C.): each letter a word of its own
                if (_o.InitialismsAsLetters && char.IsUpper(ch) && (i == 0 || !char.IsLetterOrDigit(s[i - 1])))
                {
                    var m = Initialism.Match(s, i);
                    string letters = m.Success ? m.Value.Replace(".", "") : "";
                    if (letters.Length > 0 && Initialisms.SaidAsLetters(letters))
                    {
                        Close();
                        for (int k = 0; k < letters.Length; k++)
                        {
                            if (k > 0) units.Add(new Unit(Space, "")); // a space even letter by letter: B B C
                            units.Add(new Unit(Letters, letters[k].ToString()));
                        }
                        i += m.Length - 1;
                        continue;
                    }
                }
                if (char.IsLetter(ch) || (inWord && (char.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)))
                {
                    inWord = true;
                    word.Append(ch);
                    continue;
                }
                if (IsSpaceLike(ch))
                {
                    if (_o.LetterByLetter) continue; // across spaces: the words run on
                    Add(Space);
                    continue;
                }
                if (_o.StopAtComma && ch == ',' && !(i > 0 && i + 1 < s.Length && char.IsDigit(s[i - 1]) && char.IsDigit(s[i + 1])))
                {
                    Add(Comma);
                    continue;
                }
                if (_o.StopAtComma && !_o.IgnoreBracketed && ch == '(')
                {
                    Add(Paren);
                    continue;
                }
                if (Ignored.IndexOf(ch) >= 0) continue; // words run on across it: O'Brien as OBrien
                if (char.IsControl(ch)) continue;
                Add(Symbol);
            }
            Close();
            while (units.Count > 0 && units[units.Count - 1].Kind == Space) units.RemoveAt(units.Count - 1);
            return units;
        }
    }
}
