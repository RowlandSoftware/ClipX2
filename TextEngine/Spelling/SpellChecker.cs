using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ClipX2.TextEngine.Spelling
{
    public enum SpellingIssueKind
    {
        /// <summary>Not in the dictionary.</summary>
        Misspelt,
        /// <summary>The same word twice in a row ("the the"); the fix is to delete the second.</summary>
        RepeatedWord
    }

    /// <summary>One thing the spelling checker wants the user to look at.</summary>
    public sealed class SpellingIssue
    {
        internal SpellingIssue(int start, int length, string word, SpellingIssueKind kind)
        {
            Start = start;
            Length = length;
            Word = word;
            Kind = kind;
        }

        /// <summary>Position in the text that was checked.</summary>
        public int Start { get; }
        public int Length { get; }
        public int End => Start + Length;
        /// <summary>The word as it appears in the text.</summary>
        public string Word { get; }
        public SpellingIssueKind Kind { get; }

        public override string ToString() => $"{Kind} '{Word}' @{Start}";
    }

    /// <summary>What the checker skips. The defaults are those of most word processors.</summary>
    public sealed class SpellingOptions
    {
        /// <summary>Skip words in capitals (NATO, HTML).</summary>
        public bool IgnoreUppercase { get; set; } = true;
        /// <summary>Skip words with digits in or next to them (MP3, 3rd, x64).</summary>
        public bool IgnoreWordsWithNumbers { get; set; } = true;
        /// <summary>Skip web, e-mail and file names (www.x.org, me@x.com, file.txt, C:\temp).</summary>
        public bool IgnoreAddresses { get; set; } = true;
        /// <summary>Report "the the".</summary>
        public bool CheckRepeatedWords { get; set; } = true;
    }

    /// <summary>
    /// Spelling checker for one ClipX session: finds issues, gives suggestions, and turns the
    /// user's choices (change, change all, delete, ignore, ignore all, add to dictionary) into
    /// <see cref="TextEdit"/>s, so the editor can apply them in place and keep the formatting.
    /// Ignore All and Change All last for the life of this object (ClipX 1 kept them for one
    /// check); the personal dictionary is saved to disk.
    /// Paragraph is an entity: a repeated word is only reported within a paragraph.
    /// </summary>
    public sealed class SpellChecker
    {
        private readonly HashSet<string> _ignoreAll = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _changeAll = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<(int Start, string Word)> _ignoredOnce = new List<(int, string)>();
        // Corrections the user chose this session (kept across checks: each text is a new check)
        private readonly HashSet<string> _accepted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // Word counts of the last text suggestions were asked for (the same text, issue after issue)
        private string _countedText;
        private Dictionary<string, int> _textWords;

        public SpellChecker(SpellingDictionary dictionary, PersonalDictionary personal = null, SpellingOptions options = null)
        {
            Dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
            Personal = personal;
            Options = options ?? new SpellingOptions();
            if (personal != null) dictionary.AddRange(personal.Words);
        }

        public SpellingDictionary Dictionary { get; }
        public PersonalDictionary Personal { get; }
        public SpellingOptions Options { get; }

        /// <summary>Words set to Change All, with their replacements (for display or reuse).</summary>
        public IReadOnlyDictionary<string, string> ChangeAllList => _changeAll;

        /************ Finding ************/

        /// <summary>Every issue in the text, in order.</summary>
        public IReadOnlyList<SpellingIssue> FindIssues(string text) => FindIssues(text, 0, text?.Length ?? 0);

        /// <summary>
        /// Every issue in the range, in order. The range is widened to whole words. Issues the
        /// user has ignored are left out.
        /// </summary>
        public IReadOnlyList<SpellingIssue> FindIssues(string text, int start, int length)
        {
            var list = new List<SpellingIssue>();
            if (string.IsNullOrEmpty(text)) return list;
            Scan(text, start, length, issue => { list.Add(issue); return true; });
            return list;
        }

        /// <summary>
        /// The first issue at or after <paramref name="from"/> (F7, "next"). If none, and
        /// <paramref name="wrap"/> is set, the first one from the top. Null if there are none.
        /// A word the position is inside counts (the caret in a typo), but not one that ends
        /// there: searching from the end of an issue always moves on (the scan widens to whole
        /// words, so without this it would find the same word again, e.g. "clickety" in
        /// clickety-clack).
        /// </summary>
        public SpellingIssue FindNext(string text, int from, bool wrap = true)
        {
            if (string.IsNullOrEmpty(text)) return null;
            from = Math.Max(0, Math.Min(from, text.Length));
            SpellingIssue found = null;
            Scan(text, from, text.Length - from, issue =>
            {
                if (issue.End <= from) return true; // before the start: keep looking
                found = issue;
                return false;
            });
            if (found == null && wrap && from > 0)
                Scan(text, 0, from, issue => { found = issue; return false; });
            return found;
        }

        /// <summary>Suggestions for an issue, best first, in the dictionary's order (none for a repeated word).</summary>
        public IReadOnlyList<string> Suggest(SpellingIssue issue, int max = 10) =>
            issue == null || issue.Kind != SpellingIssueKind.Misspelt
                ? (IReadOnlyList<string>)Array.Empty<string>()
                : Dictionary.Suggest(issue.Word, max);

        /// <summary>
        /// Suggestions for an issue, best first, ordered with the help of the text and of the
        /// corrections accepted this session (see <see cref="SuggestionRanker"/>): words the user
        /// has written or chosen, and their other forms, come first if they are close to the typo.
        /// </summary>
        public IReadOnlyList<string> Suggest(string text, SpellingIssue issue, int max = 10)
        {
            if (issue == null || issue.Kind != SpellingIssueKind.Misspelt) return Array.Empty<string>();
            IReadOnlyList<string> fromDictionary = Dictionary.Suggest(issue.Word, max + 10); // room for promoting
            if (!string.Equals(text, _countedText, StringComparison.Ordinal))
            {
                _textWords = SuggestionRanker.CountWords(text);
                _countedText = text;
            }
            return SuggestionRanker.Rank(issue.Word, fromDictionary, _textWords, _accepted, IsCorrect, max, Frequencies);
        }

        /// <summary>How common words are in this language (for ordering suggestions), if a table is available; else null.</summary>
        public WordFrequencies Frequencies { get; set; }

        /// <summary>Corrections chosen this session (Change, Change All), which rank high next time.</summary>
        public IReadOnlyCollection<string> AcceptedCorrections => _accepted;

        private void Accept(string replacement)
        {
            if (string.IsNullOrWhiteSpace(replacement)) return;
            foreach (string w in replacement.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries))
                if (w.Length > 1) _accepted.Add(w);
        }

        /// <summary>True if the word passes (dictionary, personal dictionary, or Ignore All).</summary>
        public bool IsCorrect(string word) => _ignoreAll.Contains(word) || Dictionary.IsCorrect(word);

        /************ The user's choices ************/

        /// <summary>
        /// The edit that replaces the issue with <paramref name="replacement"/>, in the original
        /// word's case (teh → the, Teh → The, TEH → THE), or exactly as typed if
        /// <paramref name="matchCase"/> is false (ClipX 1's Smart replace off).
        /// </summary>
        public TextEdit Change(SpellingIssue issue, string replacement, bool matchCase = true)
        {
            Accept(matchCase ? CaseFreeForm(issue.Word, replacement) : replacement);
            return new TextEdit(issue.Start, issue.Length, matchCase ? MatchCase(issue.Word, replacement) : replacement);
        }

        /// <summary>
        /// Replaces every occurrence of the word (whole words, any case, including as a part of a
        /// hyphenated word) in the text, each in its own case unless <paramref name="matchCase"/>
        /// is false, and remembers the change. Edits are in text order.
        /// </summary>
        public IReadOnlyList<TextEdit> ChangeAll(string text, SpellingIssue issue, string replacement, bool matchCase = true)
        {
            // The suggestion comes in the flagged word's case (Safty → Safety); take that off
            // first, so the other occurrences get their own case (safty → safety)
            if (matchCase) replacement = CaseFreeForm(issue.Word, replacement);
            _changeAll[issue.Word] = replacement;
            Accept(replacement);
            var edits = new List<TextEdit>();
            bool matchParts = issue.Word.IndexOf('-') < 0;
            void Try(int s, int len)
            {
                if (len == issue.Length && string.Compare(text, s, issue.Word, 0, len, StringComparison.OrdinalIgnoreCase) == 0)
                    edits.Add(new TextEdit(s, len, matchCase ? MatchCase(text.Substring(s, len), replacement) : replacement));
            }
            ForEachWord(text, 0, text.Length, (s, len) =>
            {
                int hyphen = text.IndexOf('-', s, len);
                if (hyphen < 0 || !matchParts) { Try(s, len); return; }
                int ps = s;                                     // each part of parker-bowles
                for (int i = s; i <= s + len; i++)
                    if (i == s + len || text[i] == '-') { Try(ps, i - ps); ps = i + 1; }
            });
            return edits;
        }

        /// <summary>
        /// The edit that deletes the issue: a repeated word goes with the space before it; any
        /// other word goes with one space beside it, so no double space is left.
        /// </summary>
        public TextEdit Delete(string text, SpellingIssue issue)
        {
            int s = issue.Start, e = issue.End;
            if (s > 0 && IsSpace(text[s - 1]))
                while (s > 0 && IsSpace(text[s - 1])) s--;
            else
                while (e < text.Length && IsSpace(text[e])) e++;
            return new TextEdit(s, e - s, "");
        }

        /// <summary>Skip this one occurrence (it stays skipped as the text is edited; see <see cref="NotifyEdits"/>).</summary>
        public void IgnoreOnce(SpellingIssue issue) => _ignoredOnce.Add((issue.Start, issue.Word));

        /// <summary>Skip the word everywhere, for the rest of this session.</summary>
        public void IgnoreAll(string word) { if (!string.IsNullOrEmpty(word)) _ignoreAll.Add(word); }

        /// <summary>Adds the word to the personal dictionary (saved) and the live dictionary.</summary>
        public void AddToDictionary(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return;
            Personal?.Add(word);
            Dictionary.Add(word);
        }

        /// <summary>
        /// Call after applying edits (in any order) so that ignored-once positions follow their
        /// words. Edits are positions in the text before any of them was applied.
        /// </summary>
        public void NotifyEdits(IEnumerable<TextEdit> edits)
        {
            var sorted = new List<TextEdit>(edits);
            sorted.Sort((a, b) => b.Start.CompareTo(a.Start)); // last first, so earlier positions stay valid
            foreach (TextEdit edit in sorted)
            {
                int delta = edit.Replacement.Length - edit.Length;
                for (int i = _ignoredOnce.Count - 1; i >= 0; i--)
                {
                    var (start, word) = _ignoredOnce[i];
                    if (start >= edit.Start + edit.Length) _ignoredOnce[i] = (start + delta, word);
                    else if (start + word.Length > edit.Start) _ignoredOnce.RemoveAt(i); // the word itself was edited
                }
            }
        }

        /// <summary>Forgets Ignore once / Ignore All / Change All (a new check of a new text).</summary>
        public void Reset()
        {
            _ignoreAll.Clear();
            _changeAll.Clear();
            _ignoredOnce.Clear();
        }

        /// <summary>
        /// Undoes the case <paramref name="original"/> gave <paramref name="replacement"/>: if
        /// the word was capitalised (or in capitals) and so is the replacement, the lower-case
        /// form, provided the dictionary knows it (Safety → safety, but Parker stays Parker,
        /// NASA stays NASA). Otherwise the replacement unchanged.
        /// </summary>
        public string CaseFreeForm(string original, string replacement)
        {
            if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(replacement)) return replacement ?? "";
            CultureInfo culture = CultureInfo.CurrentCulture;
            bool AllKnown(string s) => s.Split(' ').All(w => w.Length == 0 || IsCorrect(w));

            if (IsAllCaps(original) && IsAllCaps(replacement))
            {
                string lower = replacement.ToLower(culture);
                if (AllKnown(lower)) return lower;
                string cap = char.ToUpper(lower[0], culture) + lower.Substring(1);
                return AllKnown(cap) ? cap : replacement;
            }
            if (char.IsUpper(original[0]) && char.IsUpper(replacement[0]))
            {
                string lower = char.ToLower(replacement[0], culture) + replacement.Substring(1);
                if (AllKnown(lower)) return lower;
            }
            return replacement;
        }

        private static bool IsAllCaps(string s)
        {
            int letters = 0;
            foreach (char c in s)
                if (char.IsLetter(c)) { letters++; if (!char.IsUpper(c)) return false; }
            return letters > 1;
        }

        /// <summary>Puts <paramref name="replacement"/> into the case pattern of <paramref name="original"/>.</summary>
        public static string MatchCase(string original, string replacement)
        {
            if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(replacement)) return replacement ?? "";
            int letters = 0, upper = 0;
            foreach (char c in original)
                if (char.IsLetter(c)) { letters++; if (char.IsUpper(c)) upper++; }
            if (letters > 1 && upper == letters) return replacement.ToUpper(CultureInfo.CurrentCulture);
            // A word with capitals of its own (iTunes, eBay) keeps them, even at a sentence start
            bool ownCapitals = false;
            for (int i = 1; i < replacement.Length; i++) if (char.IsUpper(replacement[i])) { ownCapitals = true; break; }
            if (ownCapitals) return replacement;
            if (char.IsUpper(original[0]) && char.IsLower(replacement[0]))
                return char.ToUpper(replacement[0], CultureInfo.CurrentCulture) + replacement.Substring(1);
            return replacement;
        }

        /************ Scanning ************/

        /// <summary>Calls <paramref name="report"/> for each issue in the range until it returns false.</summary>
        private void Scan(string text, int start, int length, Func<SpellingIssue, bool> report)
        {
            int end = Math.Min(text.Length, start + length);
            start = Math.Max(0, start);
            while (start > 0 && IsWordChar(text, start - 1)) start--;          // widen to whole words
            while (end < text.Length && IsWordChar(text, end)) end++;

            // the word before the range, for "the the" across the range start
            string previous = null;
            int previousEnd = -1;
            {
                int p = start;
                while (p > 0 && IsSpace(text[p - 1])) p--;
                int pe = p;
                while (p > 0 && IsWordChar(text, p - 1)) p--;
                if (pe > p) { previous = text.Substring(p, pe - p); previousEnd = pe; }
            }

            bool stop = false;
            ForEachChunk(text, start, end, (cs, ce) =>
            {
                if (stop) return;
                if (Options.IgnoreAddresses && IsAddress(text, cs, ce))
                {
                    previous = null;
                    return;
                }
                ForEachWord(text, cs, ce, (ws, wl) =>
                {
                    if (stop) return;
                    string word = text.Substring(ws, wl);

                    // Repeated word: only spaces/tabs between, same paragraph and line
                    bool repeated = Options.CheckRepeatedWords && previous != null &&
                        OnlySpacesBetween(text, previousEnd, ws) &&
                        string.Equals(previous, word, StringComparison.CurrentCultureIgnoreCase) &&
                        word.Any(char.IsLetter);
                    previous = word;
                    previousEnd = ws + wl;

                    if (repeated)
                    {
                        var issue = new SpellingIssue(ws, wl, word, SpellingIssueKind.RepeatedWord);
                        if (!IsIgnoredOnce(issue) && !report(issue)) stop = true;
                    }
                    else if (!Skip(text, ws, wl, word))
                    {
                        foreach (SpellingIssue issue in CheckWord(text, ws, word))
                            if (!IsIgnoredOnce(issue) && !report(issue)) { stop = true; break; }
                    }
                });
                // punctuation between words ("the. The") breaks a repetition
                if (ce > 0 && !char.IsLetterOrDigit(text[ce - 1])) previous = null;
            });
        }

        /// <summary>
        /// The issues in one word: none, the word, or, for a hyphenated word, each wrong part in
        /// turn (so Ignore on one part moves on to the next). A hyphenated word whose parts all
        /// pass (dictionary, personal dictionary or Ignore All) is not then flagged as a whole.
        /// </summary>
        private IEnumerable<SpellingIssue> CheckWord(string text, int start, string word)
        {
            if (IsCorrect(word)) yield break;

            // Leading elision written with an apostrophe: 'tis, 'twas (the apostrophe isn't part of the word)
            if (start > 0 && IsApostrophe(text[start - 1]) && IsCorrect("'" + word)) yield break;

            if (word.IndexOf('-') > 0)
            {
                int ps = 0;
                foreach (string part in word.Split('-'))
                {
                    if (part.Length > 0 && !IsCorrect(part))
                        yield return new SpellingIssue(start + ps, part.Length, part, SpellingIssueKind.Misspelt);
                    ps += part.Length + 1;
                }
                yield break;
            }
            yield return new SpellingIssue(start, word.Length, word, SpellingIssueKind.Misspelt);
        }

        private bool Skip(string text, int start, int length, string word)
        {
            if (Options.IgnoreWordsWithNumbers &&
                ((start > 0 && char.IsDigit(text[start - 1])) ||
                 (start + length < text.Length && char.IsDigit(text[start + length]))))
                return true;

            if (Options.IgnoreUppercase)
            {
                int letters = 0;
                bool lower = false;
                foreach (char c in word)
                    if (char.IsLetter(c)) { letters++; if (!char.IsUpper(c)) { lower = true; break; } }
                if (!lower && letters > 1) return true;
            }
            return false;
        }

        private bool IsIgnoredOnce(SpellingIssue issue)
        {
            foreach (var (start, word) in _ignoredOnce)
                if (start == issue.Start && word == issue.Word) return true;
            return false;
        }

        /// <summary>Runs of non-space characters.</summary>
        private static void ForEachChunk(string text, int start, int end, Action<int, int> chunk)
        {
            int i = start;
            while (i < end)
            {
                while (i < end && char.IsWhiteSpace(text[i])) i++;
                int s = i;
                while (i < end && !char.IsWhiteSpace(text[i])) i++;
                if (i > s) chunk(s, i);
            }
        }

        /// <summary>
        /// Words: letters, with apostrophes and hyphens between letters (don't, O'Neill,
        /// well-known). Trailing apostrophes (boys') and leading ones ('tis) are not part of it.
        /// </summary>
        private static void ForEachWord(string text, int start, int end, Action<int, int> word)
        {
            int i = start;
            while (i < end)
            {
                while (i < end && !char.IsLetter(text[i])) i++;
                int s = i;
                while (i < end && IsWordChar(text, i)) i++;
                if (i > s) word(s, i - s);
            }
        }

        /// <summary>A letter, or an apostrophe/hyphen with a letter on both sides.</summary>
        private static bool IsWordChar(string text, int i)
        {
            char c = text[i];
            if (char.IsLetter(c)) return true;
            if (c == '\'' || c == '\u2019' || c == '-')
                return i > 0 && i + 1 < text.Length && char.IsLetter(text[i - 1]) && char.IsLetter(text[i + 1]);
            return false;
        }

        private static bool IsApostrophe(char c) => c == '\'' || c == '\u2019';

        private static bool IsSpace(char c) => c == ' ' || c == '\t' || c == '\u00A0';

        private static bool OnlySpacesBetween(string text, int from, int to)
        {
            if (from < 0 || to <= from) return false;
            for (int i = from; i < to; i++) if (!IsSpace(text[i])) return false;
            return true;
        }

        /// <summary>
        /// A web, e-mail or file name: contains ://, @ or a backslash between characters, starts
        /// with www., has a dot between two letters (example.com, file.txt, e.g.), or two or more
        /// slashes (a/b/c; a single one is usually "and/or").
        /// </summary>
        private static bool IsAddress(string text, int start, int end)
        {
            // ignore surrounding brackets, quotes and sentence punctuation
            while (start < end && !char.IsLetterOrDigit(text[start]) && text[start] != '\\' && text[start] != '/') start++;
            while (end > start && !char.IsLetterOrDigit(text[end - 1]) && text[end - 1] != '/') end--;
            if (end - start < 3) return false;

            string chunk = text.Substring(start, end - start);
            if (chunk.IndexOf("://", StringComparison.Ordinal) >= 0) return true;
            if (chunk.StartsWith("www.", StringComparison.OrdinalIgnoreCase)) return true;

            int slashes = 0;
            for (int i = 0; i < chunk.Length; i++)
            {
                char c = chunk[i];
                bool inside = i > 0 && i < chunk.Length - 1;
                if ((c == '@' || c == '\\') && inside) return true;
                if (c == '.' && inside && char.IsLetter(chunk[i - 1]) && char.IsLetterOrDigit(chunk[i + 1])) return true;
                if (c == '/') slashes++;
            }
            return slashes >= 2;
        }
    }
}
