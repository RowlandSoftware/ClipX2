using System.Linq;

namespace ClipX2.TextEngine.Engines
{
    internal static class InitCapsEngine
    {
        public static string TransformPlain(string text, int selstart, int sellen, bool titleCase)
        {
            char[] buffer = text.ToCharArray(selstart, sellen);
            string prevWord = string.Empty;

            // Collect the tokens first so we know which words are first and last in a title.
            // (While walking the words one at a time, there's no way to tell that a word is the last.)
            var tokens = Utils.GetWords(text, selstart, sellen).ToList();

            // A line break, colon or sentence end starts a new title (or subtitle), so each line
            // of a multi-line selection, and the part after "Title: ", gets its own first and last word.
            // startsTitle[i] is true when token i is the first token of a title.
            var startsTitle = new bool[tokens.Count + 1];
            for (int i = 0; i < tokens.Count; i++)
            {
                if (i == 0) { startsTitle[i] = true; continue; }

                int gapStart = tokens[i - 1].Start + tokens[i - 1].Length;

                // A full stop straight after an abbreviation (v. vs. Mr. Dr.) is not a break
                if (gapStart < text.Length && text[gapStart] == '.' &&
                    Utils.IsInList(Utils.GetTokenText(text, tokens[i - 1]), ClipXConstants.ABBREVS_WITH_PERIOD))
                    gapStart++;

                startsTitle[i] = ContainsTitleBreak(text, gapStart, tokens[i].Start);
            }
            startsTitle[tokens.Count] = true; // end of selection ends the last title

            // If a title (line) has no lower-case letters at all, it is 'shouted' text, so convert it
            // properly rather than keeping every word as an all-caps acronym.
            // shouted[i] is true when token i belongs to such a title.
            var shouted = new bool[tokens.Count];
            for (int first = 0; first < tokens.Count;)
            {
                int last = first;
                while (!startsTitle[last + 1]) last++;

                bool anyLower = false;
                for (int i = first; i <= last && !anyLower; i++)
                    anyLower = Utils.GetTokenText(text, tokens[i]).Any(char.IsLower);

                for (int i = first; i <= last; i++)
                    shouted[i] = !anyLower;

                first = last + 1;
            }

            for (int idx = 0; idx < tokens.Count; idx++)
            {
                Token token = tokens[idx];
                string original = Utils.GetTokenText(text, token);
                string word = original.ToLowerInvariant(); // for list look-ups

                if (token.Type != TokenType.Word || word.Length == 0)
                {
                    prevWord = word;
                    continue; // only handle words
                }

                // Possessive "'s" is ignored when looking words up in lists (AT&T's, iPhone's)
                string baseWord = Utils.StripPossessive(word);

                // All-caps words of 2+ letters (NASA, BBC, GCSE, AT&T's) are left as they are, as in ClipX 1
                string baseOriginal = original.Substring(0, baseWord.Length);
                if (!shouted[idx] && baseOriginal.Count(char.IsLetter) > 1 && !baseOriginal.Any(char.IsLower))
                {
                    prevWord = word;
                    continue;
                }

                char[] temp = word.ToCharArray();

                // Cap up first letter. In title case, minor words stay lower case
                // unless they are the first or last word of the title.
                bool capitalise = !titleCase
                    || startsTitle[idx]
                    || startsTitle[idx + 1]
                    || !Utils.IsInList(word, ClipXConstants.WORDS_NOT_TITLE_CASE);

                if (capitalise && char.IsLetter(temp[0]))
                    temp[0] = char.ToUpperInvariant(temp[0]);

                // Hyphenated word exceptions: re-enter, co-operate, anti-war
                if (token.Flags.HasFlag(TokenFlags.PrecededByHyphen) && Utils.IsHyphenatedPrefix(prevWord))
                    temp[0] = char.ToLowerInvariant(temp[0]);

                // 'Apple' words (iTunes, iPod, eBay, pH &c), including plurals and possessives
                if (Utils.IsSecondCharCapWord(word))
                {
                    temp[0] = char.ToLowerInvariant(temp[0]);
                    temp[1] = char.ToUpperInvariant(temp[1]);
                    if (word.StartsWith("ios")) temp[2] = 'S';
                }

                // O'Neill rule
                if (word.Length > 2)
                {
                    if (!word.Equals("o'clock") && (word.StartsWith("o'") || word.StartsWith("d'") || word.StartsWith("mc")))
                    {
                        temp[2] = char.ToUpperInvariant(temp[2]);
                    }
                }

                // Q&A, AT&T etc. (only the base word, so AT&T's keeps a lower-case 's)
                if (Utils.IsInList(baseWord, ClipXConstants.WORDS_WITH_AMPERSAND))
                {
                    for (int i = 0; i < baseWord.Length; i++)
                        temp[i] = char.ToUpperInvariant(temp[i]);
                }

                // Write transformed word back into buffer ('token.Start' is an absolute position in text)
                int relative = token.Start - selstart;

                // If the token ends outside the selection, stop at the end of the selection
                for (int i = 0; i < temp.Length && relative + i < buffer.Length; i++)
                    buffer[relative + i] = temp[i];

                prevWord = word;
            }

            return new string(buffer);
        }

        /// <summary>True if text[from..to) contains a line break, colon or sentence-ending punctuation.</summary>
        private static bool ContainsTitleBreak(string text, int from, int to)
        {
            for (int i = from; i < to; i++)
            {
                char c = text[i];
                if (c == '\n' || c == '\r' || c == ':' || ClipXConstants.SentenceTerminators.IndexOf(c) >= 0)
                    return true;
            }
            return false;
        }
    }
}
