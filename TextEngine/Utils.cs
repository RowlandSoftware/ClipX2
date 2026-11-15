using System;
using System.Collections.Generic;
using System.Linq;

namespace ClipX2.TextEngine
{
    internal static class Utils
    {
        public static IEnumerable<Token> GetWords(string text, int selstart, int sellen)
        {
            int i = selstart;

            while (i < selstart + sellen)
            {
                char c = text[i];

                // Find next word start: skip non-word characters
                if (!char.IsLetterOrDigit(c))
                {
                    i++;
                    continue;
                }

                int start = i;

                // 1) DottedAbbrev?
                if (IsDottedAbbrev(text, start, out int abbrLen))
                {
                    var flags = ComputeFlags(text, start, abbrLen);
                    yield return new Token(start, abbrLen, TokenType.DottedAbbrev, flags);
                    i = start + abbrLen;
                    continue;
                }

                // 2) Filename?
                if (IsFilename(text, start, out int fileLen))
                {
                    var flags = ComputeFlags(text, start, fileLen);
                    yield return new Token(start, fileLen, TokenType.Filename, flags);
                    i = start + fileLen;
                    continue;
                }

                // 3) Normal word
                i++;
                while (i < text.Length)
                {
                    char ch = text[i];

                    if (char.IsLetterOrDigit(ch))
                    {
                        i++;
                        continue;
                    }

                    if ((ch == '\'' || ch == '’' || ch == '&') &&
                        i + 1 < text.Length &&
                        char.IsLetterOrDigit(text[i + 1]))
                    {
                        i++;
                        continue;
                    }

                    // colo(u)r, vapo(u)rware, book(s): the brackets are part of the word
                    int bracketLen = InWordBracketLength(text, i);
                    if (bracketLen > 0)
                    {
                        i += bracketLen;
                        continue;
                    }

                    break;
                }

                int length = i - start;
                var wordFlags = ComputeFlags(text, start, length);
                yield return new Token(start, length, TokenType.Word, wordFlags);
            }
        }
        static TokenFlags ComputeFlags(string text, int start, int length)
        {
            TokenFlags flags = TokenFlags.None;

            // Preceded by hyphen
            if (start > 0 && text[start - 1] == '-')
                flags |= TokenFlags.PrecededByHyphen;

            // Preceded by apostrophe
            if (start > 0 && (text[start - 1] == '\'' || text[start - 1] == '’'))
                flags |= TokenFlags.PrecededByApostrophe;

            // All caps? NB TRUE if NO letters
            bool allCaps = true;
            for (int j = start; j < start + length; j++)
            {
                char c = text[j];
                if (char.IsLetter(c) && !char.IsUpper(c))
                {
                    allCaps = false;
                    break;
                }
            }
            if (allCaps)
                flags |= TokenFlags.AllCaps;

            // Contains digits?
            for (int j = start; j < start + length; j++)
            {
                if (char.IsDigit(text[j]))
                {
                    flags |= TokenFlags.ContainsDigits;
                    break;
                }
            }

            // Contains &?
            if (text.IndexOf('&', start, length) >= 0)
                flags |= TokenFlags.Abbreviation;

            // First in sentence?
            int k = start - 1;
            while (k >= 0 && char.IsWhiteSpace(text[k]))
                k--;

            if (k < 0 || text[k] == '.' || text[k] == '!' || text[k] == '?')
                flags |= TokenFlags.FirstInSentence;

            return flags;
        }

        public static string GetTokenText(string text, Token token)
            => text.Substring(token.Start, token.Length);

        public static string TrimQuotes(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            const string quotes = ClipXConstants.Quotes;

            while (text.Length > 0 && quotes.IndexOf(text[0]) >= 0)
                text = text.Substring(1);

            while (text.Length > 0 && quotes.IndexOf(text[text.Length - 1]) >= 0)
                text = text.Substring(0, text.Length - 1);

            return text;
        }

        public static string TrimBrackets(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            const string open = ClipXConstants.BracketsOpen;
            const string close = ClipXConstants.BracketsClose;

            while (text.Length > 0 && open.IndexOf(text[0]) >= 0)
                text = text.Substring(1);

            while (text.Length > 0 && close.IndexOf(text[text.Length - 1]) >= 0)
                text = text.Substring(0, text.Length - 1);

            return text;
        }
        public static string TrimPunc_Brackets(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            const string charsToTrim = ClipXConstants.PUNCTUATION + ClipXConstants.BracketsClose;

            while (text.Length > 0 && charsToTrim.IndexOf(text[text.Length - 1]) >= 0)
                text = text.Substring(0, text.Length - 1);

            return text;
        }
        public static string GetContext(string text, int pos, out int startIndex)
        {
            startIndex = pos;

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Move left
            while (startIndex > 0 && !char.IsWhiteSpace(text[startIndex - 1]))
                startIndex--;

            int end = pos;

            // Move right
            while (end < text.Length - 1 && !char.IsWhiteSpace(text[end + 1]))
                end++;

            return text.Substring(startIndex, end - startIndex + 1);
        }
        public static bool IsAbbrevAt(string text, int index)
        {
            if (index + 3 >= text.Length)
                return false;

            return char.IsLetter(text[index]) &&
                   text[index + 1] == '.' &&
                   char.IsLetter(text[index + 2]) &&
                   text[index + 3] == '.';
        }

        public static bool IsAbbrev(string token)
        {
            token = TrimBrackets(token);

            if (token.EndsWith("...") || token.EndsWith("…"))
                return true;

            if (token.Length % 2 != 0)
                return false;

            for (int i = 0; i < token.Length; i += 2)
            {
                if (!char.IsLetter(token[i]) || token[i + 1] != '.')
                    return false;
            }
            return true;
        }
        public static bool IsInList(string searchFor, string list)
        {
            if (string.IsNullOrEmpty(searchFor)) return false;
            return list.IndexOf("," + searchFor + ",", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        /// <summary>Removes a trailing possessive 's (straight or curly apostrophe): "AT&amp;T's" → "AT&amp;T".</summary>
        public static string StripPossessive(string word)
        {
            if (word != null && word.Length > 2 &&
                (word.EndsWith("'s", StringComparison.OrdinalIgnoreCase) || word.EndsWith("’s", StringComparison.OrdinalIgnoreCase)))
                return word.Substring(0, word.Length - 2);
            return word;
        }
        /// <summary>
        /// True for words whose second letter is a capital (iPhone, eBay, pH), including plurals
        /// (iPhones) and possessives (iPhone's). Whole words only, so "phone" does not match "ph".
        /// </summary>
        /// <summary>
        /// True for a deliberately mixed-case word: starts with a capital and has another capital
        /// after a lower-case letter (OmniFocus, JavaScript, McDonald). An accidental Caps Lock word
        /// such as "tHE" starts lower case, so it is not kept.
        /// </summary>
        public static bool IsMixedCase(string word)
        {
            if (string.IsNullOrEmpty(word) || !char.IsUpper(word[0])) return false;
            for (int i = 2; i < word.Length; i++)
                if (char.IsUpper(word[i]) && char.IsLower(word[i - 1])) return true;
            return false;
        }

        /// <summary>
        /// Length of a bracketed run of letters inside a word, e.g. the "(u)" in "colo(u)r" or the
        /// "(s)" in "book(s)", starting at text[i] == '('; 0 if there isn't one.
        /// </summary>
        public static int InWordBracketLength(string text, int i)
        {
            if (i <= 0 || text[i] != '(' || !char.IsLetter(text[i - 1])) return 0;
            int j = i + 1;
            while (j < text.Length && char.IsLetter(text[j])) j++;
            if (j == i + 1 || j >= text.Length || text[j] != ')') return 0;
            return j + 1 - i;
        }

        public static bool IsSecondCharCapWord(string word)
        {
            if (string.IsNullOrEmpty(word)) return false;

            word = StripPossessive(word);

            if (IsInList(word, ClipXConstants.WORDS_SECOND_CHAR_CAP))
                return true;

            return word.Length > 1
                && word.EndsWith("s", StringComparison.OrdinalIgnoreCase)
                && IsInList(word.Substring(0, word.Length - 1), ClipXConstants.WORDS_SECOND_CHAR_CAP);
        }
        public static bool IsHyphenatedPrefix(string word)
        {
            return IsInList(word, ClipXConstants.HYPHENATED_PREFIXES);
        }
        public static bool IsElision(string text, int aposIndex)
        {
            // Safety
            if (aposIndex < 0 || aposIndex >= text.Length - 1)
                return false;

            char before = aposIndex > 0 ? text[aposIndex - 1] : ' ';
            char after = text[aposIndex + 1];

            // 1. If both sides are letters → internal apostrophe (O'Neill, rock'n'roll)
            if (char.IsLetter(before) && char.IsLetter(after))
                return true;

            // 2. Known contraction endings
            string[] endings = [ "t", "ve", "nt", "m", "s", "ll", "d", "re" ];

            foreach (var end in endings)
            {
                if (string.Compare(text, aposIndex + 1, end, 0, end.Length, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }

            return false;
        }
        public static bool IsDottedAbbrev(string text, int start, out int length)
        {
            int i = start;
            int n = text.Length;

            // First character must be a letter
            if (i >= n || !char.IsLetter(text[i]))
            {
                length = 0;
                return false;
            }
            i++;

            // Expect: . + letter, repeated
            while (i + 1 < n && text[i] == '.' && char.IsLetter(text[i + 1]))
            {
                i += 2;
            }

            // Valid DottedAbbrev only if at least one dot-letter pair was consumed
            length = i - start;
            return length > 2;   // e.g. "i.e." is length 4
        }
        // Rather crude identification. Accepted if contains slash ("and/or") or dot. Check for abbrevs *first*
        public static bool IsFilename(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            // Remove trailing punctuation/brackets/quotes
            text = TrimPunc_Brackets(text);
            text = TrimQuotes(text);

            // Reject anything containing whitespace
            if (text.Any(char.IsWhiteSpace))
                return false;

            // Normalise slashes
            text = text.Replace("\\", "/");

            // Accept email addresses
            if (text.Contains("@"))
                return true;

            // URL schemes
            if (text.Contains("://"))
                return true;

            // If it contains a slash, treat as filename/path/URL
            if (text.Contains("/"))
                return true;

            // If it contains a dot, treat as filename/domain
            if (text.Contains("."))
                return true;

            return false;
        }
        public static bool IsFilename(string text, int start, out int length)
        {
            int i = start;
            int n = text.Length;

            // Look  ahead
            while (i < n && (char.IsLetterOrDigit(text[i]) || "_-@.".Contains(text[i])))
                i++;

            length = i - start;

            return IsFilename(text.Substring(start, length));
        }
        public static string GetPrevWord(string text, int startIndex, bool splitAtHyphen = false)
        {
            if (string.IsNullOrEmpty(text) || startIndex <= 0)
                return string.Empty;

            int i = startIndex;
            var sb = new System.Text.StringBuilder();

            while (i > 0)
            {
                char c = text[i];
                char prev = i > 0 ? text[i - 1] : ' ';

                bool isLetter = char.IsLetter(c);

                bool isHyphenPart =
                    (c == '-') &&
                    !splitAtHyphen &&
                    char.IsLetter(prev);

                bool isApostrophePart =
                    (c == '\'' || c == '’') &&
                    char.IsLetter(prev);

                if (isLetter || isHyphenPart || isApostrophePart)
                {
                    sb.Insert(0, c);
                    i--;
                }
                else
                {
                    break;
                }
            }

            return sb.ToString();
        }

    }
}
