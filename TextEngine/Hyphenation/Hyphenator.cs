using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ClipX2.TextEngine.Hyphenation
{
    /// <summary>
    /// Hyphenation by Liang's patterns (as TeX, LibreOffice and libhyphen), from a LibreOffice
    /// hyphenation dictionary (hyph_en_GB.dic and so on): the first line names the encoding, then
    /// optional settings (LEFTHYPHENMIN…, NEXTLEVEL between the compound and ordinary levels), then
    /// one pattern a line, such as ".ab3ol" or "1tion": letters with numbers between them, an odd
    /// number allowing a break there, the highest number at each place winning; "." marks the
    /// start or end of the word. Both levels are used together here.
    ///
    /// Used to count syllables for the readability scores: in most languages the places a word
    /// may be hyphenated are its syllable boundaries. The minimum lengths at the word's ends (two
    /// letters, three) are for typesetting, so they are ignored when counting syllables.
    /// </summary>
    public sealed class Hyphenator
    {
        private readonly Dictionary<string, byte[]> _patterns = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        private int _longest;

        /// <summary>The fewest letters to leave before the first break / after the last, when hyphenating for print.</summary>
        public int LeftMin { get; private set; } = 2;
        public int RightMin { get; private set; } = 3;

        public int PatternCount => _patterns.Count;

        /// <summary>Reads a LibreOffice hyphenation dictionary.</summary>
        public static Hyphenator Load(string path) => Load(File.ReadAllBytes(path));

        public static Hyphenator Load(byte[] bytes)
        {
            // The first line is the encoding: UTF-8 nearly always; ISO8859-x in some old ones
            int firstEnd = Array.IndexOf(bytes, (byte)'\n');
            string first = Encoding.ASCII.GetString(bytes, 0, firstEnd < 0 ? bytes.Length : firstEnd).Trim();
            Encoding encoding = EncodingFor(first);
            string text = encoding.GetString(bytes, firstEnd < 0 ? bytes.Length : firstEnd + 1, firstEnd < 0 ? 0 : bytes.Length - firstEnd - 1);

            var h = new Hyphenator();
            foreach (string raw in text.Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line[0] == '%' || line[0] == '#') continue;
                if (line.StartsWith("LEFTHYPHENMIN", StringComparison.Ordinal)) { h.LeftMin = Number(line, 2); continue; }
                if (line.StartsWith("RIGHTHYPHENMIN", StringComparison.Ordinal)) { h.RightMin = Number(line, 3); continue; }
                if (line.StartsWith("COMPOUND", StringComparison.Ordinal) || line == "NEXTLEVEL"
                    || line.StartsWith("NOHYPHEN", StringComparison.Ordinal)) continue;
                int slash = line.IndexOf('/'); // non-standard breaks (Hungarian "ssz/sz=sz"): the plain pattern only
                if (slash >= 0) line = line.Substring(0, slash);
                h.Add(line);
            }
            return h;
        }

        private static Encoding EncodingFor(string name)
        {
            if (name.Equals("UTF-8", StringComparison.OrdinalIgnoreCase)) return new UTF8Encoding(false);
            try
            {
                // ISO8859-1 → ISO-8859-1, KOI8-R, microsoft-cp1251 → windows-1251
                string n = name.Replace("ISO8859", "ISO-8859").Replace("microsoft-cp", "windows-");
                return Encoding.GetEncoding(n);
            }
            catch (ArgumentException) { return new UTF8Encoding(false); }
        }

        private static int Number(string line, int fallback)
        {
            string[] parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 && int.TryParse(parts[1], out int n) ? n : fallback;
        }

        /// <summary>Adds a pattern such as "a1b2c": letters and the numbers between them.</summary>
        private void Add(string pattern)
        {
            var letters = new StringBuilder();
            var values = new List<byte> { 0 };
            foreach (char c in pattern)
            {
                if (c >= '0' && c <= '9') values[values.Count - 1] = (byte)(c - '0');
                else { letters.Append(char.ToLowerInvariant(c)); values.Add(0); }
            }
            if (letters.Length == 0) return;
            string key = letters.ToString();
            byte[] v = values.ToArray();
            if (_patterns.TryGetValue(key, out byte[] old)) // the same letters at two levels: the higher number at each place
                for (int i = 0; i < v.Length && i < old.Length; i++) v[i] = Math.Max(v[i], old[i]);
            _patterns[key] = v;
            _longest = Math.Max(_longest, key.Length);
        }

        /// <summary>
        /// The places a word may break: index i means between letters i-1 and i. With
        /// <paramref name="forPrint"/>, LeftMin and RightMin are kept to; otherwise every break the
        /// patterns allow (for counting syllables).
        /// </summary>
        public List<int> Breaks(string word, bool forPrint = false)
        {
            var breaks = new List<int>();
            if (string.IsNullOrEmpty(word) || _patterns.Count == 0) return breaks;
            string w = "." + word.ToLowerInvariant() + ".";
            var points = new byte[w.Length + 1];
            for (int start = 0; start < w.Length; start++)
                for (int len = 1; len <= _longest && start + len <= w.Length; len++)
                    if (_patterns.TryGetValue(w.Substring(start, len), out byte[] values))
                        for (int k = 0; k < values.Length; k++)
                            if (values[k] > points[start + k]) points[start + k] = values[k];

            // points[j] is before character j of w; character j of w is letter j-1 of the word
            int left = forPrint ? Math.Max(1, LeftMin) : 1, right = forPrint ? Math.Max(1, RightMin) : 1;
            for (int i = left; i <= word.Length - right; i++)
                if (points[i + 1] % 2 == 1) breaks.Add(i);
            return breaks;
        }

        /// <summary>The word with hyphens where it may break (for checking): "hy-phen-ation".</summary>
        public string Hyphenate(string word, bool forPrint = false, string hyphen = "-")
        {
            var sb = new StringBuilder(word);
            List<int> breaks = Breaks(word, forPrint);
            for (int i = breaks.Count - 1; i >= 0; i--) sb.Insert(breaks[i], hyphen);
            return sb.ToString();
        }

        /// <summary>
        /// Syllables: the pieces between the breaks the patterns allow, counting only pieces with
        /// a vowel (ignoring the minimum lengths at the ends lets the patterns split off lone
        /// consonants, as in "In-for-ma-ti-o-n"; those belong to the syllable beside them). In a
        /// script whose vowels aren't known here, every piece counts.
        /// </summary>
        public int Syllables(string word)
        {
            List<int> breaks = Breaks(word);
            // A word starting vowel, consonant, vowel opens with a one-letter syllable (U-ni-, a-gua,
            // e-co-), which hyphenation never splits off
            int opening = word.Length >= 3 && (breaks.Count == 0 || breaks[0] > 1)
                && HasVowel(word, 0, 1) && !HasVowel(word, 1, 2) && HasVowel(word, 2, 3) ? 1 : 0;
            if (breaks.Count == 0) return 1 + opening;
            int count = 0, from = 0;
            bool anyVowel = false;
            foreach (int at in breaks.Concat(new[] { word.Length }))
            {
                if (HasVowel(word, from, at)) { count++; anyVowel = true; }
                from = at;
            }
            return anyVowel ? Math.Max(1, count) + opening : breaks.Count + 1;
        }

        private const string Vowels =
            "aeiouyàáâãäåæèéêëìíîïòóôõöøùúûüýÿœāăąēĕėęěīĭįıōŏőūŭůűųȧ" + // Latin
            "аеёиоуыэюяіїєў" +                                           // Cyrillic
            "αεηιουωάέήίόύώϊϋ";                                          // Greek

        private static bool HasVowel(string word, int from, int to)
        {
            for (int i = from; i < to; i++)
                if (Vowels.IndexOf(char.ToLowerInvariant(word[i])) >= 0) return true;
            return false;
        }
    }
}
