using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClipX2.TextEngine.Tools
{
    /// <summary>A tool that matches a "What do you want to do?" query, and how well.</summary>
    public sealed class ToolMatch
    {
        internal ToolMatch(ToolInfo tool, double score) { Tool = tool; Score = score; }
        public ToolInfo Tool { get; }
        public double Score { get; }
        public override string ToString() => $"{Tool.Name} ({Score:0.0})";
    }

    /// <summary>
    /// "What do you want to do?": finds tools from words a user types, which are often not the
    /// tools' own names (they are searching because they couldn't find the tool). Each tool's
    /// name, its synonyms (<see cref="ToolKeywords"/>: other words for it, American spellings,
    /// things people describe it by), its description and its group are searched. A word matches
    /// whole, as the start of a word ("bul" finds Bullets), as a plural or other ending, inside a
    /// word, or with a typo; filler words ("how do I…") are ignored. Results are best first.
    /// English for now: synonyms in other languages go with the translations.
    /// </summary>
    public static class ToolFinder
    {
        private static readonly HashSet<string> Filler = new HashSet<string>(StringComparer.Ordinal)
        {
            "i", "to", "the", "a", "an", "my", "me", "how", "do", "can", "want", "wanna", "please", "of", "in", "on", "for",
            "with", "all", "it", "this", "that", "some", "is", "are", "be", "into", "from", "so", "and", "or", "text",
            "selected", "selection", "would", "like", "need", "should", "you", "what", "where", "which", "get",
            "make", "turn", "set", "put", "use", "using", "have", "has", "there", "them", "these", "those", "words", "word's",
        };

        /// <summary>Ways of saying "remove", made "remove" before matching ("get rid of formatting" = "remove formatting").</summary>
        private static readonly (string From, string To)[] Removal =
        {
            ("get rid of", "remove"), ("take out", "remove"), ("take off", "remove"), ("take away", "remove"),
            ("delete", "remove"), ("erase", "remove"), ("strip", "remove"), ("eliminate", "remove"), ("lose", "remove"), ("kill", "remove"),
        };

        /// <summary>Words that ask for something to go: without one, the Remove… tools rank lower.</summary>
        private static readonly HashSet<string> RemovalWords = new HashSet<string>(StringComparer.Ordinal) { "remove", "no", "without", "off", "clear", "undo" };

        private const double NameWeight = 10, KeywordWeight = 8, DescriptionWeight = 3, GroupWeight = 2;

        /// <summary>The tools best matching <paramref name="query"/>, best first, at most <paramref name="max"/>.</summary>
        /// <param name="query">What the user typed.</param>
        /// <param name="tools">The tools to search.</param>
        /// <param name="max">At most this many results.</param>
        /// <param name="shown">
        /// What the program shows for a tool, if not the catalogue's English: its button's caption
        /// and tooltip (searched as name and description too, so users find what they see).
        /// </param>
        public static List<ToolMatch> Find(string query, IEnumerable<ToolInfo> tools, int max = 8,
            Func<ToolInfo, (string Name, string Description)> shown = null)
        {
            var results = new List<ToolMatch>();
            string q = " " + Normalise(query) + " ";
            foreach (var (from, to) in Removal) q = q.Replace(" " + from + " ", " " + to + " ");
            q = q.Trim();
            List<string> words = Words(q).Where(w => !Filler.Contains(w)).ToList();
            bool askingToRemove = words.Any(RemovalWords.Contains);
            if (words.Count == 0) words = Words(q).ToList(); // only filler words: use them after all
            if (words.Count == 0) return results;

            foreach (ToolInfo tool in tools)
            {
                var own = shown?.Invoke(tool) ?? (null, null);
                string name = Normalise(tool.Name);
                string[] keywords = ToolKeywords.For(tool.Tag).Select(Normalise)
                    .Concat(string.IsNullOrEmpty(own.Name) ? Array.Empty<string>() : new[] { Normalise(own.Name) }).ToArray();
                string description = Normalise(tool.Description + " " + own.Description);
                string group = Normalise(tool.Group);

                double total = 0;
                int matched = 0;
                foreach (string w in words)
                {
                    double best = Math.Max(
                        Math.Max(NameWeight * Best(w, Words(name)), KeywordWeight * keywords.Select(k => Best(w, Words(k))).DefaultIfEmpty(0).Max()),
                        Math.Max(DescriptionWeight * Best(w, Words(description)), GroupWeight * Best(w, Words(group))));
                    if (best > 0) matched++;
                    total += best;
                }
                if (matched == 0) continue;

                // Every word should count: a tool matching only some of them ranks well below
                total *= Math.Pow((double)matched / words.Count, 2);
                // The whole query as a phrase: exactly a name or synonym ("smart quotes", "capitals")
                // ranks above one merely containing it ("remove smart quotes")
                string phrase = string.Join(" ", words);
                if (phrase.Length >= 3)
                {
                    if (name == phrase || keywords.Contains(phrase)) total += 15;
                    else if (name.Contains(phrase) || keywords.Any(k => k.Contains(phrase))) total += 6;
                }
                // Remove… tools only near the top when something is to go ("bullets" → Bulleted first)
                if (!askingToRemove && name.StartsWith("remove", StringComparison.Ordinal)) total *= 0.6;
                if (total >= 2.5) results.Add(new ToolMatch(tool, total));
            }
            return results.OrderByDescending(r => r.Score).ThenBy(r => r.Tool.Name, StringComparer.CurrentCulture).Take(max).ToList();
        }

        /// <summary>How well a typed word matches the best of some words: 1 whole … 0 not at all.</summary>
        private static double Best(string typed, IEnumerable<string> words)
        {
            double best = 0;
            foreach (string w in words)
            {
                double s = Match(typed, w);
                if (s > best) best = s;
                if (best >= 1) break;
            }
            return best;
        }

        private static double Match(string typed, string word)
        {
            if (typed == word) return 1;
            if (word.Length >= 4 && typed.StartsWith(word, StringComparison.Ordinal) && typed.Length - word.Length <= 3) return 0.9; // bullets, numbering
            if (typed.Length >= 2 && word.StartsWith(typed, StringComparison.Ordinal)) return typed.Length >= 4 ? 0.85 : 0.7;  // "bul", "capit"
            if (typed.Length >= 4 && word.Contains(typed)) return 0.5;
            if (typed.Length >= 4 && word.Length >= 4)
            {
                int allowed = typed.Length >= 8 ? 2 : 1;
                if (Math.Abs(typed.Length - word.Length) <= allowed && Distance(typed, word, allowed) <= allowed) return 0.6; // a typo
            }
            return 0;
        }

        /// <summary>Edit distance (with swapped neighbours as one edit), stopping once past <paramref name="limit"/>.</summary>
        private static int Distance(string a, string b, int limit)
        {
            var d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;
            for (int i = 1; i <= a.Length; i++)
            {
                int rowBest = int.MaxValue;
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    int v = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                    if (i > 1 && j > 1 && a[i - 1] == b[j - 2] && a[i - 2] == b[j - 1]) v = Math.Min(v, d[i - 2, j - 2] + 1);
                    d[i, j] = v;
                    rowBest = Math.Min(rowBest, v);
                }
                if (rowBest > limit) return limit + 1;
            }
            return d[a.Length, b.Length];
        }

        /// <summary>Lower case, curly quotes straight, American "-ize" and the like left for the synonyms.</summary>
        private static string Normalise(string s) =>
            (s ?? "").ToLowerInvariant().Replace('’', '\'').Replace('–', '-').Replace('—', '-');

        /// <summary>The words of a string: letters, digits and the symbols people search with (%20, &amp;amp;, =20, x², >).</summary>
        private static IEnumerable<string> Words(string s) =>
            Regex.Matches(s, @"[\p{L}\p{N}%&;=#²₂>'·•]+").Cast<Match>().Select(m => m.Value.Trim('\'')).Where(w => w.Length > 0);
    }
}
