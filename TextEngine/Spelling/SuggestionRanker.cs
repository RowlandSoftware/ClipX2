using System;
using System.Collections.Generic;
using System.Linq;

namespace ClipX2.TextEngine.Spelling
{
    /// <summary>
    /// Puts the dictionary's suggestions in a better order using what the user has written:
    /// words in the text (the more often, the stronger), corrections accepted earlier in the
    /// session, and other forms of either (adversary → adversaries, adversary's). Such words
    /// are also added when the dictionary didn't suggest them, if they are close enough to the
    /// typo and the dictionary accepts them.
    ///
    /// The rest keep the dictionary's order. A word from the text moves above one of those
    /// only if it is about as close to the typo: its closeness, less a bonus for the evidence,
    /// must be no worse than the other's. So "the" for "teh" is not pushed down by "tech"
    /// merely because the text is about technology.
    ///
    /// Closeness is edit distance in which swapping two neighbouring letters counts as one
    /// edit (optimal string alignment), ignoring case and apostrophe style.
    /// </summary>
    public static class SuggestionRanker
    {
        // Bonuses, in edits: how much nearer the evidence makes a word count
        private const double AcceptedBonus = 1.0;     // accepted as a correction this session
        private const double AcceptedFormBonus = 0.8; // another form of one
        private const double TextFormPenalty = 0.1;   // another form of a word in the text: a little less than the word itself

        /// <summary>A word in the text: 0.5 if used once, rising with use, at most 0.9.</summary>
        private static double TextBonus(int count) => Math.Min(0.9, 0.5 + 0.1 * Math.Log(count));

        /// <param name="typo">The flagged word.</param>
        /// <param name="suggestions">The dictionary's suggestions, best first.</param>
        /// <param name="textWords">Words in the text with how often each occurs (see <see cref="CountWords"/>).</param>
        /// <param name="accepted">Corrections accepted this session.</param>
        /// <param name="isCorrect">The dictionary's verdict, for words not among its suggestions.</param>
        /// <param name="max">How many to return.</param>
        public static IReadOnlyList<string> Rank(string typo, IReadOnlyList<string> suggestions,
            IReadOnlyDictionary<string, int> textWords, IEnumerable<string> accepted, Func<string, bool> isCorrect, int max)
        {
            string target = Key(typo);
            if (target.Length == 0) return suggestions.Take(max).ToList();
            int limit = MaxDistance(target.Length);

            // Evidence for each word (by key): the best bonus, and the form to show
            var bonus = new Dictionary<string, double>(StringComparer.Ordinal);
            var shown = new Dictionary<string, string>(StringComparer.Ordinal);
            void Note(string key, string form, double b)
            {
                if (key == target) return; // the typo itself, if it occurs elsewhere
                if (!bonus.TryGetValue(key, out double old) || b > old) { bonus[key] = b; shown[key] = form; }
            }
            void NoteFamily(string word, double own, double family)
            {
                string key = Key(word);
                // A name (only ever written with a capital) has no other forms but the possessive
                IEnumerable<string> forms = word.Length > 0 && char.IsUpper(word[0]) ? new[] { key, key + "'s" } : Forms(key);
                foreach (string form in forms)
                {
                    if (Math.Abs(form.Length - target.Length) > limit + 1) continue;
                    Note(form, form == key ? word : form, form == key ? own : family);
                }
            }
            if (textWords != null)
                foreach (KeyValuePair<string, int> w in textWords)
                {
                    double b = TextBonus(w.Value);
                    NoteFamily(w.Key, b, Math.Max(0.4, b - TextFormPenalty));
                }
            if (accepted != null)
                foreach (string a in accepted) NoteFamily(a, AcceptedBonus, AcceptedFormBonus);

            // The dictionary's own suggestions, with any evidence they have
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var plain = new List<(string Word, int Distance)>();      // no evidence: dictionary order
            var backed = new List<(string Word, double Score, int Order)>();
            int order = 0;
            foreach (string s in suggestions)
            {
                string key = Key(s);
                if (!seen.Add(key)) continue;
                int d = Distance(target, key);
                if (bonus.TryGetValue(key, out double b)) backed.Add((s, d - b, order));
                else plain.Add((s, d));
                order++;
            }
            // Words the dictionary didn't suggest: close enough, and correct
            foreach (KeyValuePair<string, double> e in bonus)
            {
                if (seen.Contains(e.Key)) continue;
                int d = Distance(target, e.Key, limit);
                if (d > limit) continue;
                string form = shown[e.Key];
                if (!isCorrect(form)) continue;
                seen.Add(e.Key);
                backed.Add((SpellChecker.MatchCase(typo, form), d - e.Value, order++));
            }
            backed.Sort((a, b) => a.Score != b.Score ? a.Score.CompareTo(b.Score) : a.Order.CompareTo(b.Order));

            // Merge: a backed word goes before a plain one if it is at least as close, after its bonus
            var result = new List<string>();
            int i = 0;
            foreach (var p in plain)
            {
                while (i < backed.Count && backed[i].Score <= p.Distance) result.Add(backed[i++].Word);
                result.Add(p.Word);
            }
            while (i < backed.Count) result.Add(backed[i++].Word);

            bool curly = typo.IndexOf('’') >= 0;
            return result.Select(w => curly ? w.Replace('\'', '’') : w.Replace('’', '\''))
                         .Distinct(StringComparer.Ordinal).Take(max).ToList();
        }

        /// <summary>
        /// How often each word occurs in the text (case and apostrophe style ignored; the
        /// lower-case form is kept if the word ever appears in lower case, else the first form
        /// seen, so Parker stays Parker). Hyphenated words count both whole and as their parts.
        /// </summary>
        public static Dictionary<string, int> CountWords(string text)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);   // by key
            var forms = new Dictionary<string, string>(StringComparer.Ordinal);
            void Add(string word)
            {
                string key = Key(word);
                if (key.Length < 2) return;
                counts.TryGetValue(key, out int n);
                counts[key] = n + 1;
                string form = word.Replace('’', '\'');
                if (!forms.TryGetValue(key, out string old) || (old != key && form == key)) forms[key] = form;
            }
            if (!string.IsNullOrEmpty(text))
            {
                int i = 0;
                while (i < text.Length)
                {
                    while (i < text.Length && !char.IsLetter(text[i])) i++;
                    int s = i;
                    while (i < text.Length && IsWordChar(text, i)) i++;
                    if (i == s) continue;
                    string word = text.Substring(s, i - s);
                    Add(word);
                    if (word.IndexOf('-') >= 0) foreach (string part in word.Split('-')) Add(part);
                }
            }
            var result = new Dictionary<string, int>(counts.Count, StringComparer.Ordinal);
            foreach (KeyValuePair<string, int> c in counts) result[forms[c.Key]] = c.Value;
            return result;
        }

        private static bool IsWordChar(string text, int i)
        {
            char c = text[i];
            if (char.IsLetter(c)) return true;
            if (c == '\'' || c == '’' || c == '-')
                return i > 0 && i + 1 < text.Length && char.IsLetter(text[i - 1]) && char.IsLetter(text[i + 1]);
            return false;
        }

        /// <summary>Lower case, straight apostrophes: the form words are compared in.</summary>
        private static string Key(string word) =>
            (word ?? "").Trim().Replace('’', '\'').Replace('‘', '\'').ToLowerInvariant();

        /// <summary>How far a word not suggested by the dictionary may be from the typo.</summary>
        private static int MaxDistance(int length) => length <= 4 ? 1 : length <= 7 ? 2 : 3;

        /************ Other forms of a word ************/

        /// <summary>
        /// The word and its likely other forms (English endings: plural, possessive, -ed, -ing,
        /// -ly, -er, -est), found by taking an ending off (not -er, -est, -ly) and putting endings on. Many are not
        /// words; the dictionary decides. Doubled consonants (stop → stopped) are not covered.
        /// </summary>
        internal static IEnumerable<string> Forms(string key)
        {
            var bases = new HashSet<string>(StringComparer.Ordinal) { key };
            void Base(string b) { if (b.Length >= 2) bases.Add(b); }
            if (key.EndsWith("'s")) Base(key.Substring(0, key.Length - 2));
            if (key.EndsWith("s'")) Base(key.Substring(0, key.Length - 2));
            if (key.EndsWith("ies")) Base(key.Substring(0, key.Length - 3) + "y");
            if (key.EndsWith("ied")) Base(key.Substring(0, key.Length - 3) + "y");
            if (key.EndsWith("es")) Base(key.Substring(0, key.Length - 2));
            if (key.EndsWith("s") && !key.EndsWith("ss")) Base(key.Substring(0, key.Length - 1));
            if (key.EndsWith("ed")) { Base(key.Substring(0, key.Length - 2)); Base(key.Substring(0, key.Length - 1)); }
            if (key.EndsWith("ing")) { Base(key.Substring(0, key.Length - 3)); Base(key.Substring(0, key.Length - 3) + "e"); }
            // Not taken off: -er, -est, -ly (Parker is not a form of park, nor early of ear)

            var forms = new HashSet<string>(StringComparer.Ordinal);
            foreach (string b in bases)
            {
                forms.Add(b);
                forms.Add(b + "s"); forms.Add(b + "es"); forms.Add(b + "s'");
                if (!b.EndsWith("s")) forms.Add(b + "'s"); // not adversaries's
                forms.Add(b + "ly"); forms.Add(b + "er"); forms.Add(b + "est");
                if (b.EndsWith("e"))
                {
                    string stem = b.Substring(0, b.Length - 1);
                    forms.Add(b + "d"); forms.Add(stem + "ing"); forms.Add(b + "r"); forms.Add(b + "st");
                }
                else { forms.Add(b + "ed"); forms.Add(b + "ing"); }
                if (b.Length > 2 && b.EndsWith("y") && !IsVowel(b[b.Length - 2]))
                {
                    string stem = b.Substring(0, b.Length - 1);
                    forms.Add(stem + "ies"); forms.Add(stem + "ied"); forms.Add(stem + "ier"); forms.Add(stem + "iest"); forms.Add(stem + "ily");
                }
            }
            return forms;
        }

        private static bool IsVowel(char c) => "aeiou".IndexOf(c) >= 0;

        /************ Distance ************/

        /// <summary>
        /// Edits from one to the other: a letter added, dropped or changed, or two neighbouring
        /// letters swapped, each counting one. Stops early (returning more than
        /// <paramref name="limit"/>) once the answer must exceed it.
        /// </summary>
        internal static int Distance(string a, string b, int limit = int.MaxValue)
        {
            if (Math.Abs(a.Length - b.Length) > limit) return limit + 1;
            int n = a.Length, m = b.Length;
            var prev2 = new int[m + 1];
            var prev = new int[m + 1];
            var cur = new int[m + 1];
            for (int j = 0; j <= m; j++) prev[j] = j;
            for (int i = 1; i <= n; i++)
            {
                cur[0] = i;
                int rowMin = cur[0];
                for (int j = 1; j <= m; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    int v = Math.Min(Math.Min(prev[j] + 1, cur[j - 1] + 1), prev[j - 1] + cost);
                    if (i > 1 && j > 1 && a[i - 1] == b[j - 2] && a[i - 2] == b[j - 1])
                        v = Math.Min(v, prev2[j - 2] + 1);
                    cur[j] = v;
                    if (v < rowMin) rowMin = v;
                }
                if (rowMin > limit) return limit + 1;
                var t = prev2; prev2 = prev; prev = cur; cur = t;
            }
            return prev[m];
        }
    }
}
