using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ClipX2.TextEngine.Spelling
{
    /// <summary>
    /// How common words are, for ordering spelling suggestions: a small bonus for frequent
    /// words, worth well under one edit, so frequency breaks ties and near-ties between
    /// suggestions but never puts a common word above a clearly closer one.
    ///
    /// The list is read from a file beside the dictionary, named after it: en_GB.al (for English,
    /// from Adam Kilgarriff's BNC lemma list: about 6,300 lemmas). A
    /// suggestion that is itself a lemma (adversary) gets its lemma's bonus; another form of
    /// one (adversaries, adversary's) gets a little less, the lemma being found by taking
    /// ordinary English endings off (see <see cref="SuggestionRanker"/>). Irregular forms
    /// (went, children) aren't linked to their lemmas and get nothing; nor do words not in the
    /// table, which keep the dictionary's order.
    /// </summary>
    public sealed class WordFrequencies
    {
        // Bonus, in edits: from Smallest (the least common word in the table) to Largest ("the")
        private const double Smallest = 0.15;
        private const double Largest = 0.45;
        private const double FormShare = 0.8; // another form of a lemma: less common than the lemma as a whole

        private readonly Dictionary<string, long> _counts;
        private readonly double _logMin, _logMax;

        private WordFrequencies(Dictionary<string, long> counts)
        {
            _counts = counts;
            long min = counts.Count > 0 ? counts.Values.Min() : 1, max = counts.Count > 0 ? counts.Values.Max() : 1;
            _logMin = Math.Log(Math.Max(1, min));
            _logMax = Math.Log(Math.Max(min + 1, max));
        }

        /// <summary>How many words (lemmas) the table has.</summary>
        public int Count => _counts.Count;

        /// <summary>
        /// Reads a frequency list (UTF-8): one word and its frequency per line, separated by
        /// spaces or a tab, e.g. "abandon 4249"; in any order. A word listed more than once
        /// (about, as adverb and as preposition) gets the total. Lines that don't fit are
        /// skipped, so a heading or comment does no harm.
        /// </summary>
        public static WordFrequencies Load(string path)
        {
            var counts = new Dictionary<string, long>(StringComparer.Ordinal);
            foreach (string line in File.ReadLines(path))
            {
                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2 || !long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out long frequency))
                    continue;
                string lemma = parts[0].ToLowerInvariant();
                if (lemma.Length == 0 || !lemma.All(c => char.IsLetter(c) || c == '\'' || c == '-')) continue;
                counts.TryGetValue(lemma, out long n);
                counts[lemma] = n + frequency;
            }
            return new WordFrequencies(counts);
        }

        /// <summary>
        /// The bonus for a suggestion (in edits): from about 0.15 for the least common word in
        /// the table to 0.45 for the commonest; a little less for another form of a lemma; 0 if
        /// not in the table.
        /// </summary>
        public double Bonus(string word)
        {
            if (string.IsNullOrEmpty(word) || word.IndexOf(' ') >= 0) return 0; // "T eh": not one word
            string key = word.Trim().Replace('’', '\'').ToLowerInvariant();
            if (_counts.TryGetValue(key, out long n)) return Scale(n);

            // Another form: the lemma is among the bases ending-stripping gives (Forms includes them)
            long best = 0;
            foreach (string form in SuggestionRanker.Forms(key))
                if (form.Length < key.Length && _counts.TryGetValue(form, out long m) && m > best) best = m;
            return best > 0 ? Scale(best) * FormShare : 0;
        }

        private double Scale(long count)
        {
            double t = (_logMax - _logMin) > 0 ? (Math.Log(Math.Max(1, count)) - _logMin) / (_logMax - _logMin) : 1;
            return Smallest + (Largest - Smallest) * Math.Max(0, Math.Min(1, t));
        }
    }
}
