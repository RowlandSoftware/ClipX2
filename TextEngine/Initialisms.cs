using System;
using System.Collections.Generic;
using System.Linq;

namespace ClipX2.TextEngine
{
    /// <summary>
    /// Whether an initialism (BBC, NATO, UNESCO) is said as letters or as a word, by a rule rather
    /// than a dictionary: three letters or fewer are said as letters (BBC, USA, EU), unless they're
    /// known words (RAM, PIN, GIF); longer ones are said as a word when they split into syllables
    /// English allows (NATO, NA-SA, U-NES-CO, NAF-TA), each with a vowel, starting with
    /// consonants a word can start with and ending with ones a word can end with; otherwise as
    /// letters (HTML, ISBN, DVLA, OECD). A short list covers known exceptions.
    /// </summary>
    public static class Initialisms
    {
        /// <summary>Three letters or fewer, but said as a word.</summary>
        private static readonly HashSet<string> ShortWords = new HashSet<string>(StringComparer.Ordinal)
        {
            "RAM", "ROM", "PIN", "SIM", "GIF", "LAN", "WAN", "SAD", "SIDS",
        };

        /// <summary>Longer, and pronounceable, but said as letters.</summary>
        private static readonly HashSet<string> LongLetters = new HashSet<string>(StringComparer.Ordinal)
        {
            "USAF", "IEEE", "AAAS", "OSCE",
        };

        private static readonly HashSet<string> Onsets = new HashSet<string>(StringComparer.Ordinal)
        {
            "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z",
            "bl", "br", "ch", "cl", "cr", "dr", "dw", "fl", "fr", "gl", "gn", "gr", "kl", "kn", "kr", "ph", "pl", "pr", "ps",
            "qu", "sc", "sch", "scr", "sh", "shr", "sk", "sl", "sm", "sn", "sp", "sph", "spl", "spr", "squ", "st", "str",
            "sv", "sw", "th", "thr", "tr", "tw", "wh", "wr", "zh",
        };

        private static readonly HashSet<string> Codas = new HashSet<string>(StringComparer.Ordinal)
        {
            "b", "c", "d", "f", "g", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z",
            "bs", "ch", "ck", "cs", "ct", "ds", "ff", "fs", "ft", "gh", "gs", "ks", "ld", "lf", "lk", "ll", "lm", "lp", "ls", "lt",
            "mb", "mp", "ms", "nch", "nd", "nds", "ng", "ngs", "nk", "ns", "nt", "nts", "ps", "pt", "rb", "rc", "rd", "rf", "rg",
            "rk", "rl", "rm", "rn", "rp", "rs", "rt", "rth", "sh", "sk", "sp", "ss", "st", "th", "ts", "xt",
        };

        /// <summary>
        /// Whether a group of letters (capitals, from a heading; full stops already taken out) is
        /// said as letters: B-B-C. False for ones said as words (NATO), and for anything that
        /// isn't two or more capitals.
        /// </summary>
        public static bool SaidAsLetters(string letters)
        {
            if (letters == null || letters.Length < 2 || !letters.All(c => char.IsLetter(c) && char.IsUpper(c))) return false;
            if (ShortWords.Contains(letters)) return false;
            if (LongLetters.Contains(letters)) return true;
            if (letters.Length <= 3) return true;
            return !Pronounceable(letters.ToLowerInvariant());
        }

        /// <summary>Splits into syllables English allows (see the class).</summary>
        public static bool Pronounceable(string s)
        {
            // Alternating clusters: consonants, vowels, consonants… (y is a vowel except first)
            var clusters = new List<string>();
            var vowelFlags = new List<bool>();
            for (int i = 0; i < s.Length; i++)
            {
                bool v = IsVowel(s, i);
                if (vowelFlags.Count > 0 && vowelFlags[vowelFlags.Count - 1] == v) clusters[clusters.Count - 1] += s[i];
                else { clusters.Add(s[i].ToString()); vowelFlags.Add(v); }
            }
            if (!vowelFlags.Contains(true)) return false;
            for (int k = 0; k < clusters.Count; k++)
            {
                if (vowelFlags[k]) continue;
                string c = clusters[k];
                bool first = k == 0, last = k == clusters.Count - 1;
                if (first && !Onsets.Contains(c)) return false;
                else if (last && !Codas.Contains(c)) return false;
                else if (!first && !last && !Splits(c)) return false;
            }
            return true;
        }

        /// <summary>A cluster between vowels: the end of one syllable and the start of the next.</summary>
        private static bool Splits(string c)
        {
            for (int k = 0; k <= c.Length; k++)
            {
                string coda = c.Substring(0, k), onset = c.Substring(k);
                if ((coda.Length == 0 || Codas.Contains(coda)) && (onset.Length == 0 || Onsets.Contains(onset))) return true;
            }
            return false;
        }

        private static bool IsVowel(string s, int i)
        {
            char c = s[i];
            return "aeiou".IndexOf(c) >= 0 || (c == 'y' && i > 0);
        }
    }
}
