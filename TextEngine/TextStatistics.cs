using System;
using System.Text.RegularExpressions;

namespace ClipX2.TextEngine
{
    /// <summary>Counts and measures text (the basis of ClipX's Statistics tool and live word count).</summary>
    public static class TextStatistics
    {
        /// <summary>
        /// Counts the words in <paramref name="text"/>, using ClipX 1's rule: a word starts with a
        /// letter and continues through letters, apostrophes and hyphens, so "don't" and "re-enter"
        /// are one word each and a number on its own is not a word.
        /// </summary>
        public static int CountWords(string text) => CountWords(text, 0, text?.Length ?? 0);

        /// <summary>Counts the words in part of <paramref name="text"/>, e.g. the selection.</summary>
        public static int CountWords(string text, int start, int length)
        {
            if (string.IsNullOrEmpty(text)) return 0;

            int count = 0;
            bool inWord = false;
            for (int i = start; i < start + length; i++)
            {
                char c = text[i];
                if (inWord)
                    inWord = char.IsLetter(c) || c == '\'' || c == '’' || c == '-';
                else if (char.IsLetter(c))
                {
                    inWord = true;
                    count++;
                }
            }
            return count;
        }

        /************ Statistics (chSTAT) ************/

        /// <summary>
        /// A reading-ease formula for one language: Flesch's (English) or a version of it fitted
        /// to another language. Each gives roughly 0 (very hard) to 100 (very easy) from the words
        /// per sentence and syllables per word, so Flesch's descriptions fit them all.
        /// </summary>
        public sealed class ReadingEaseFormula
        {
            public string Name { get; }
            private readonly Func<double, double, double> _score; // (words per sentence, syllables per word)
            private ReadingEaseFormula(string name, Func<double, double, double> score) { Name = name; _score = score; }
            public double Score(double wordsPerSentence, double syllablesPerWord) => _score(wordsPerSentence, syllablesPerWord);

            /// <summary>The formula for a language ("en_GB", "de_DE_frami", "fr"…), or null if there is none.</summary>
            public static ReadingEaseFormula For(string language)
            {
                switch (LanguageCode(language))
                {
                    case "en": return new ReadingEaseFormula("Flesch Reading Ease", (s, w) => 206.835 - 1.015 * s - 84.6 * w);
                    case "de": return new ReadingEaseFormula("Flesch Reading Ease for German (Amstad)", (s, w) => 180 - s - 58.5 * w);
                    case "fr": return new ReadingEaseFormula("Flesch Reading Ease for French (Kandel and Moles)", (s, w) => 207 - 1.015 * s - 73.6 * w);
                    case "es": return new ReadingEaseFormula("Reading ease for Spanish (Fernández Huerta)", (s, w) => 206.84 - 1.02 * s - 60 * w);
                    case "it": return new ReadingEaseFormula("Reading ease for Italian (Flesch–Vacca)", (s, w) => 217 - 1.3 * s - 60 * w);
                    case "nl": return new ReadingEaseFormula("Flesch Reading Ease for Dutch (Douma)", (s, w) => 206.835 - 0.93 * s - 77 * w);
                    case "pt": return new ReadingEaseFormula("Flesch Reading Ease for Portuguese (Martins)", (s, w) => 248.835 - 1.015 * s - 84.6 * w);
                    case "ru": return new ReadingEaseFormula("Flesch Reading Ease for Russian (Oborneva)", (s, w) => 206.835 - 1.3 * s - 60.1 * w);
                    default: return null;
                }
            }
        }

        /// <summary>The language part of a locale name: "en" for en_GB, "de" for de_DE_frami.</summary>
        public static string LanguageCode(string language) =>
            (language ?? "").Split('_', '-', '.')[0].ToLowerInvariant();

        /// <summary>English: syllables by ClipX's own rules, and the English-only scores (grade, Fog).</summary>
        public static bool IsEnglish(string language) => LanguageCode(language) == "en";

        /// <summary>
        /// Whether readability in this language needs a hyphenation dictionary to count syllables:
        /// a language with a reading-ease formula, other than English (which has its own rules,
        /// better than English hyphenation patterns for this).
        /// </summary>
        public static bool NeedsHyphenation(string language) => !IsEnglish(language) && ReadingEaseFormula.For(language) != null;

        /// <summary>Counts for a stretch of text, and readability scores worked out from them.</summary>
        public sealed class Counts
        {
            /// <summary>The language the scores are for (the spelling language).</summary>
            public string Language { get; internal set; } = "en";
            /// <summary>Syllables were counted (English rules, or the language's hyphenation patterns).</summary>
            public bool SyllablesCounted { get; internal set; }
            /// <summary>Characters, not counting line breaks (as ClipX 1).</summary>
            public int Characters { get; internal set; }
            /// <summary>Characters other than spaces, tabs and line breaks (ClipX 1's "printable characters").</summary>
            public int NonSpaceCharacters { get; internal set; }
            public int Words { get; internal set; }
            public int Sentences { get; internal set; }
            /// <summary>Paragraphs with something in them (blank lines aren't counted).</summary>
            public int Paragraphs { get; internal set; }
            /// <summary>Estimated: English syllable rules, or the language's hyphenation patterns.</summary>
            public int Syllables { get; internal set; }
            /// <summary>Gunning's "complex" words: three or more syllables, not names, compounds or a three-syllable count made by -es, -ed, -ing.</summary>
            public int ComplexWords { get; internal set; }

            public double WordsPerSentence => Sentences == 0 ? 0 : (double)Words / Sentences;
            public double SyllablesPerWord => Words == 0 ? 0 : (double)Syllables / Words;

            /// <summary>Whether there is enough text for the readability scores to mean anything.</summary>
            public bool HasReadability => Words >= 10 && Sentences > 0;

            /// <summary>The language's reading-ease formula (null: none for this language).</summary>
            public ReadingEaseFormula Formula => ReadingEaseFormula.For(Language);

            /// <summary>Reading ease, about 0 (very hard) to 100 (very easy); null if it can't be worked out.</summary>
            public double? ReadingEase =>
                Formula != null && SyllablesCounted && HasReadability ? Formula.Score(WordsPerSentence, SyllablesPerWord) : (double?)null;

            /// <summary>Flesch–Kincaid grade (English only): years of schooling needed to comprehend.</summary>
            public double? FleschKincaidGrade =>
                IsEnglish(Language) && HasReadability ? 0.39 * WordsPerSentence + 11.8 * SyllablesPerWord - 15.59 : (double?)null;

            /// <summary>Gunning Fog index (English only): years of schooling needed; about 12 suits a wide audience.</summary>
            public double? FogIndex =>
                IsEnglish(Language) && HasReadability ? 0.4 * (WordsPerSentence + 100.0 * ComplexWords / Words) : (double?)null;

            /// <summary>Flesch's own description of a reading-ease score ("plain English", or "plain language" for another).</summary>
            public static string EaseDescription(double score, bool english = true) =>
                score >= 90 ? "very easy" : score >= 80 ? "easy" : score >= 70 ? "fairly easy"
                : score >= 60 ? (english ? "plain English" : "plain language") : score >= 50 ? "fairly difficult"
                : score >= 30 ? "difficult" : "very difficult";
        }

        /// <summary>
        /// Counts words, sentences, paragraphs, syllables and so on in part of the text. English
        /// syllables come from ClipX's rules; for another language, give
        /// <paramref name="syllableCounter"/> (a word's syllables, from its hyphenation patterns),
        /// or syllables aren't counted and there is no reading ease.
        /// </summary>
        public static Counts Measure(string text, int start, int length, string language = "en", Func<string, int> syllableCounter = null)
        {
            bool english = IsEnglish(language);
            if (english) syllableCounter = null; // ClipX's rules beat hyphenation patterns for English
            var c = new Counts { Language = language ?? "en", SyllablesCounted = english || syllableCounter != null };
            if (string.IsNullOrEmpty(text) || length <= 0) return c;
            int end = Math.Min(text.Length, start + length);

            bool paragraphHasText = false, sentenceOpen = false, sentenceStart = true;
            int i = start;
            while (i < end)
            {
                char ch = text[i];
                if (ch == '\n' || ch == '\r' || ch == '\v' || ch == '\f' || ch == (char)0x2028 || ch == (char)0x2029)
                {
                    // A paragraph ends: so does a sentence without a full stop (a heading, a list item)
                    if (paragraphHasText) c.Paragraphs++;
                    if (sentenceOpen) c.Sentences++;
                    paragraphHasText = sentenceOpen = false;
                    sentenceStart = true;
                    i++;
                    continue;
                }
                c.Characters++;
                if (!char.IsWhiteSpace(ch)) { c.NonSpaceCharacters++; paragraphHasText = true; }

                if (char.IsLetter(ch))
                {
                    // A word: letters, apostrophes and hyphens (as CountWords)
                    int w = i;
                    while (i < end && (char.IsLetter(text[i]) || text[i] == '\'' || text[i] == '’' || text[i] == '-')) i++;
                    string word = text.Substring(w, i - w);
                    c.Characters += word.Length - 1;
                    c.NonSpaceCharacters += word.Length - 1;
                    c.Words++;
                    if (english)
                    {
                        int syllables = Syllables(word);
                        c.Syllables += syllables;
                        if (IsComplex(word, syllables, sentenceStart)) c.ComplexWords++;
                    }
                    else if (syllableCounter != null) c.Syllables += SyllablesByParts(word, syllableCounter);
                    sentenceOpen = true;
                    sentenceStart = false;
                    continue;
                }
                if ((ch == '.' || ch == '!' || ch == '?' || ch == '…') && sentenceOpen && EndsSentence(text, i, end))
                {
                    c.Sentences++;
                    sentenceOpen = false;
                    sentenceStart = true;
                }
                i++;
            }
            if (paragraphHasText) c.Paragraphs++;
            if (sentenceOpen) c.Sentences++;
            return c;
        }

        /// <summary>
        /// A word's syllables from a counter (hyphenation patterns), part by part: a hyphenated
        /// compound's parts, and the parts either side of an apostrophe; an elided letter
        /// ("l'" in l'homme, "d'" in d'accord) adds nothing.
        /// </summary>
        private static int SyllablesByParts(string word, Func<string, int> counter)
        {
            int sum = 0;
            foreach (string part in word.Split('-', '\'', '’'))
                if (part.Length > 1) sum += Math.Max(1, counter(part));
            return Math.Max(1, sum);
        }

        /// <summary>Abbreviations whose full stop doesn't end a sentence ("Dr. Smith", "e.g. this").</summary>
        private static readonly string[] Abbreviations =
            { "mr", "mrs", "ms", "dr", "prof", "st", "rev", "e.g", "i.e", "eg", "ie", "cf", "vs", "no", "approx", "fig" };

        /// <summary>
        /// Whether the punctuation at <paramref name="i"/> ends a sentence: followed (after any more
        /// stops, quotes and brackets) by a space or the end, and not a decimal point, an initial
        /// ("J. Smith") or one of the common abbreviations.
        /// </summary>
        private static bool EndsSentence(string text, int i, int end)
        {
            int j = i + 1;
            while (j < end && (".!?…\"'”’)]".IndexOf(text[j]) >= 0)) j++;
            if (j < end && !char.IsWhiteSpace(text[j])) return false; // 3.5, e.g. (the inner stop), example.com
            if (text[i] != '.') return true;
            // The word before the stop
            int w = i;
            while (w > 0 && (char.IsLetter(text[w - 1]) || text[w - 1] == '.')) w--;
            string before = text.Substring(w, i - w).ToLowerInvariant();
            if (before.Length == 1 && char.IsUpper(text[w])) return false; // an initial
            return Array.IndexOf(Abbreviations, before) < 0;
        }

        /// <summary>
        /// English syllables in a word, estimated from its vowel groups: a silent final e, and -es
        /// and -ed that add no syllable, are allowed for. Right for most everyday words; long or
        /// borrowed words may be one out, which matters little over a whole text.
        /// </summary>
        public static int Syllables(string word)
        {
            string w = word.ToLowerInvariant().Replace("’", "'");
            int apostrophe = w.IndexOf('\'');
            if (apostrophe > 0) w = w.Substring(0, apostrophe) + (w.EndsWith("n't") ? "nt" : ""); // don't, it's, we'll
            if (w.Contains("-"))
            {
                int sum = 0; // a compound: its parts
                foreach (string part in w.Split('-')) if (part.Length > 0) sum += Syllables(part);
                return Math.Max(1, sum);
            }
            if (w.Length <= 3) return 1;

            int count = 0;
            bool inVowels = false;
            for (int k = 0; k < w.Length; k++)
            {
                bool vowel = "aeiouy".IndexOf(w[k]) >= 0 && !(w[k] == 'y' && k == 0);
                if (vowel && !inVowels) count++;
                inVowels = vowel;
            }
            // Vowel pairs that are two syllables: ar-e-a, i-de-a, po-em, qui-et, sci-ence, go-ing, vi-de-o, us-u-al, pris-m
            foreach (Regex split in SplitVowels) count += split.Matches(w).Count;
            // Silent endings: make, mak-es, ask-ed (but not little, rais-es, wait-ed)
            if (w.EndsWith("e") && !w.EndsWith("le") && !w.EndsWith("ee") && !w.EndsWith("ie")) count--;
            else if (w.EndsWith("le") && w.Length > 2 && "aeiouy".IndexOf(w[w.Length - 3]) >= 0) count--; // whale, mile
            else if (w.EndsWith("es") && !EndsWithAny(w, "ses", "zes", "xes", "ches", "shes", "ges", "ces")) count--;
            else if (w.EndsWith("ed") && !EndsWithAny(w, "ted", "ded")) count--;
            return Math.Max(1, count);
        }

        /// <summary>Vowel pairs said as two syllables, which the vowel-group count takes as one.</summary>
        private static readonly Regex[] SplitVowels =
        {
            new Regex("[^tcsgxl]ia", RegexOptions.Compiled),          // vi-a, In-di-a, tri-al (not -tial, -cial, brilliant)
            new Regex("[^tcsgxln]io", RegexOptions.Compiled),         // ra-di-o, vi-o-lin (not -tion, -sion, -gion, million, onion)
            new Regex("[aeiouy][^aeiouy]ea$", RegexOptions.Compiled), // ar-e-a, i-de-a, Kor-e-a (not sea, plea)
            new Regex("[aeiouy]ing$", RegexOptions.Compiled),         // go-ing, be-ing, see-ing
            new Regex("oe[^s]", RegexOptions.Compiled),               // po-em, po-et (not shoes)
            new Regex("ie(t|nc|nt)", RegexOptions.Compiled),          // qui-et, sci-ence, cli-ent
            new Regex("eo([^pu]|$)", RegexOptions.Compiled),             // vid-e-o, ge-o-graphy (not people, Europe)
            new Regex("[^gq]ua[^aeiouy]", RegexOptions.Compiled),     // us-u-al, act-u-al (not guard, quality)
            new Regex("[^aeiouy]ism$", RegexOptions.Compiled),        // pris-m, real-is-m
        };

        /// <summary>
        /// Gunning's complex word: three or more syllables, except names (capitalised, not starting
        /// a sentence), hyphenated compounds, and words that get there only by -es, -ed or -ing.
        /// </summary>
        private static bool IsComplex(string word, int syllables, bool sentenceStart)
        {
            if (syllables < 3) return false;
            if (word.Contains("-")) return false;
            if (char.IsUpper(word[0]) && !sentenceStart) return false;
            string w = word.ToLowerInvariant();
            if (w.EndsWith("ing") && syllables - 1 < 3) return false;
            if ((w.EndsWith("es") || w.EndsWith("ed")) && Syllables(w.Substring(0, w.Length - 2)) < 3) return false;
            return true;
        }

        private static bool EndsWithAny(string w, params string[] endings)
        {
            foreach (string e in endings) if (w.EndsWith(e)) return true;
            return false;
        }
    }
}
