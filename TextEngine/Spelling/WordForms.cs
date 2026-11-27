using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClipX2.TextEngine.Spelling
{
    /// <summary>
    /// Other forms of a word, from a Hunspell dictionary's affix rules: for a word the dictionary
    /// has, its own endings (pull: pulls, pulled, pulling); for a new word, the endings of the
    /// dictionary word that ends most like it (geocache, like cache: geocaches, geocached,
    /// geocaching). Suffixes only; possessives ('s) left out, as the checker accepts them anyway.
    /// </summary>
    public sealed class WordForms
    {
        private sealed class Rule
        {
            public string Strip, Add;
            public Regex Condition;
        }

        private readonly Dictionary<string, List<Rule>> _suffixes = new Dictionary<string, List<Rule>>(StringComparer.Ordinal);
        private readonly List<(string Word, string[] Flags)> _roots = new List<(string, string[])>();

        private WordForms() { }

        /// <summary>Reads a dictionary's .aff and .dic files (a second or so for a large one).</summary>
        public static WordForms Load(string affPath, string dicPath)
        {
            var forms = new WordForms();
            Encoding encoding = EncodingOf(affPath);
            string flagType = "";
            var aliases = new List<string>();
            List<Rule> current = null;
            int remaining = 0;
            foreach (string raw in File.ReadLines(affPath, encoding))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line[0] == '#') continue;
                string[] f = Regex.Split(line, @"\s+");
                if (f[0] == "FLAG" && f.Length > 1) flagType = f[1];
                else if (f[0] == "AF" && f.Length > 1 && !int.TryParse(f[1], out _)) aliases.Add(f[1]);
                else if (f[0] == "SFX" && f.Length >= 4 && remaining == 0 && (f[2] == "Y" || f[2] == "N") && int.TryParse(f[3], out int count))
                {
                    current = new List<Rule>();
                    forms._suffixes[f[1]] = current;
                    remaining = count;
                }
                else if (f[0] == "SFX" && f.Length >= 4 && remaining > 0 && current != null)
                {
                    remaining--;
                    string strip = f[2] == "0" ? "" : f[2];
                    string add = f[3].Split('/')[0];
                    if (add == "0") add = "";
                    string condition = f.Length > 4 ? f[4] : ".";
                    try { current.Add(new Rule { Strip = strip, Add = add, Condition = new Regex("(?:" + condition + ")$", RegexOptions.CultureInvariant) }); }
                    catch (ArgumentException) { /* an unusual condition: that rule is skipped */ }
                }
            }

            bool first = true;
            foreach (string raw in File.ReadLines(dicPath, encoding))
            {
                if (first) { first = false; if (int.TryParse(raw.Trim(), out _)) continue; }
                string line = raw.TrimEnd();
                if (line.Length == 0 || line[0] == '\t' || line[0] == '#') continue;
                int end = line.IndexOfAny(new[] { '\t', ' ' });
                string entry = end < 0 ? line : line.Substring(0, end);
                int slash = IndexOfSlash(entry);
                string word = (slash < 0 ? entry : entry.Substring(0, slash)).Replace("\\/", "/");
                string flags = slash < 0 ? "" : entry.Substring(slash + 1);
                if (aliases.Count > 0 && int.TryParse(flags, out int alias) && alias >= 1 && alias <= aliases.Count) flags = aliases[alias - 1];
                string[] list = SplitFlags(flags, flagType).Where(forms._suffixes.ContainsKey).ToArray();
                if (word.Length > 0) forms._roots.Add((word, list));
            }
            return forms;
        }

        /// <summary>
        /// The other forms of <paramref name="word"/>, and the dictionary word whose endings they
        /// follow (the word itself if the dictionary has it). Forms <paramref name="known"/> says
        /// the dictionary already accepts are left out. Empty if no word ends like it.
        /// </summary>
        public (string Model, IReadOnlyList<string> Forms) Suggest(string word, Func<string, bool> known = null)
        {
            word = word?.Trim();
            if (string.IsNullOrEmpty(word) || word.IndexOf(' ') >= 0) return (null, Array.Empty<string>());
            bool lower = char.IsLower(word[0]);

            string bestModel = null;
            List<string> bestForms = null;
            int bestLength = 1, bestScore = -1;
            foreach (var (root, flags) in _roots)
            {
                if (flags.Length == 0) continue;
                int common = CommonEnding(root, word);
                if (common < Math.Min(3, word.Length) || common < bestLength) continue;
                bool exact = string.Equals(root, word, StringComparison.Ordinal);
                int score = (exact ? 1000 : 0) + (char.IsLower(root[0]) == lower ? 100 : 0) + flags.Length;
                if (common == bestLength && score <= bestScore) continue;
                List<string> forms = Apply(word, flags);
                if (forms.Count == 0) continue;
                bestLength = common; bestScore = score; bestModel = root; bestForms = forms;
            }
            if (bestForms == null) return (null, Array.Empty<string>());
            var result = bestForms
                .Where(f => f != word && f.IndexOf('\'') < 0 && f.IndexOf('’') < 0)
                .Where(f => known == null || !known(f))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            return (bestModel, result);
        }

        private List<string> Apply(string word, string[] flags)
        {
            var forms = new List<string>();
            foreach (string flag in flags)
                foreach (Rule rule in _suffixes[flag])
                {
                    if (!word.EndsWith(rule.Strip, StringComparison.Ordinal) || !rule.Condition.IsMatch(word)) continue;
                    string stem = word.Substring(0, word.Length - rule.Strip.Length);
                    if (stem.Length == 0) continue;
                    forms.Add(stem + rule.Add);
                }
            return forms;
        }

        private static int CommonEnding(string a, string b)
        {
            int n = 0;
            while (n < a.Length && n < b.Length && char.ToLowerInvariant(a[a.Length - 1 - n]) == char.ToLowerInvariant(b[b.Length - 1 - n])) n++;
            return n;
        }

        private static int IndexOfSlash(string entry)
        {
            for (int i = 1; i < entry.Length; i++)
                if (entry[i] == '/' && entry[i - 1] != '\\') return i;
            return -1;
        }

        private static IEnumerable<string> SplitFlags(string flags, string type)
        {
            if (flags.Length == 0) return Array.Empty<string>();
            switch (type)
            {
                case "long": return Enumerable.Range(0, flags.Length / 2).Select(i => flags.Substring(i * 2, 2));
                case "num": return flags.Split(',');
                default: return flags.Select(c => c.ToString());
            }
        }

        /// <summary>The affix file's SET (UTF-8 or an ISO 8859 code page); Latin-1 if it says nothing.</summary>
        private static Encoding EncodingOf(string affPath)
        {
            foreach (string line in File.ReadLines(affPath, Encoding.GetEncoding("ISO-8859-1")))
            {
                string t = line.Trim().TrimStart('﻿', 'ï', '»', '¿');
                if (!t.StartsWith("SET ", StringComparison.Ordinal)) continue;
                string name = t.Substring(4).Trim();
                if (name.Equals("UTF-8", StringComparison.OrdinalIgnoreCase)) return new UTF8Encoding(false);
                try { return Encoding.GetEncoding(name); } catch (ArgumentException) { break; }
            }
            return Encoding.GetEncoding("ISO-8859-1");
        }
    }
}
