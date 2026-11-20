using System;
using System.Collections.Generic;

namespace ClipX2.TextEngine
{
    /// <summary>
    /// Plain text search (Quick search): forwards or backwards from a position, or every match;
    /// optionally matching case and whole words only.
    /// </summary>
    public static class TextSearch
    {
        /// <summary>
        /// The start of the first match at or after <paramref name="from"/> (forwards), or the last
        /// match ending at or before it (backwards); -1 if none.
        /// </summary>
        public static int Find(string text, string term, int from, bool forward, bool matchCase, bool wholeWords)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(term)) return -1;
            StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            from = Math.Max(0, Math.Min(from, text.Length));
            if (forward)
            {
                for (int i = from; i <= text.Length - term.Length; i++)
                {
                    i = text.IndexOf(term, i, comparison);
                    if (i < 0) break;
                    if (!wholeWords || IsWholeWord(text, i, term.Length)) return i;
                }
            }
            else
            {
                // The match must end at or before 'from'; each try ends one character earlier
                for (int last = from - 1; last >= term.Length - 1; )
                {
                    int i = text.LastIndexOf(term, last, last + 1, comparison);
                    if (i < 0) break;
                    if (!wholeWords || IsWholeWord(text, i, term.Length)) return i;
                    last = i + term.Length - 2;
                }
            }
            return -1;
        }

        /// <summary>Every match, in text order, not overlapping.</summary>
        public static List<int> FindAll(string text, string term, bool matchCase, bool wholeWords)
        {
            var found = new List<int>();
            int i = 0;
            while ((i = Find(text, term, i, true, matchCase, wholeWords)) >= 0)
            {
                found.Add(i);
                i += term.Length;
            }
            return found;
        }

        /// <summary>
        /// Whether a match stands as a whole word: no letter or digit runs on into it at either
        /// end. A term starting or ending with punctuation or a space needs no boundary there.
        /// </summary>
        public static bool IsWholeWord(string text, int start, int length)
        {
            int end = start + length;
            if (length == 0) return false;
            if (IsWordChar(text[start]) && start > 0 && IsWordChar(text[start - 1])) return false;
            if (IsWordChar(text[end - 1]) && end < text.Length && IsWordChar(text[end])) return false;
            return true;
        }

        private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';
    }
}
