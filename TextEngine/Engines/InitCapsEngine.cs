using System;
using System.Linq;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace ClipX2.TextEngine.Engines
{
    public static class InitCapsEngine
    {
        public static string TransformPlain(string text, int selstart, int sellen, bool titleCase)
        {
            string seltext = text.Substring(selstart, sellen);
            char[] buffer = seltext.ToCharArray();
            string prevWord = string.Empty;
            bool flgFirstWord = true;
            bool flgLastWord = false;

            foreach (Token token in Utils.GetWords(text, selstart, sellen))
            {
                string word = Utils.GetTokenText(text, token).ToLower();

                if (token.Type != TokenType.Word)
                {
                    prevWord = word;
                    continue; // only handle words
                }

                int start = token.Start;
                char[] temp = word.ToCharArray();

                // Cap up first letter
                if (char.IsLetter(temp[0]))
                {
                    if (!(titleCase && Utils.IsInList(word, ClipXConstants.WORDS_NOT_TITLE_CASE)))
                        temp[0] = char.ToUpperInvariant(temp[0]);
                }

                // First word?
                if (char.IsLetter(temp[0]))
                {
                    if (!(titleCase && flgFirstWord ))
                    {
                        temp[0] = char.ToUpperInvariant(temp[0]);
                        flgFirstWord = false;
                    }
                }
                
                // Last word?
                if (char.IsLetter(temp[0]))
                {
                    if (!(titleCase && flgLastWord))
                    {
                        temp[0] = char.ToUpperInvariant(temp[0]);
                        flgLastWord = false;
                        flgFirstWord = true; // because it might be
                    }
                }

                // Hyphenated word exceptions
                if (token.Flags.HasFlag(TokenFlags.PrecededByHyphen))
                {
                    if (Utils.IsHyphenatedPrefix(prevWord)) temp[0] = char.ToLowerInvariant(temp[0]);
                }

                // 'Apple' words (iTunes, iPod, eBay &c)
                if (Utils.IsInListStartsWith(word, ClipXConstants.WORDS_SECOND_CHAR_CAP))
                {
                    temp[0] = char.ToLowerInvariant(temp[0]);
                    temp[1] = char.ToUpperInvariant(temp[1]);
                    if (word == "ios") temp[2] = 'S';
                }

                // O'Neill rule
                if (word.Length > 2)
                {
                    if (!word.Equals("o'clock") && (word.StartsWith("o'") || word.StartsWith("d'") || word.StartsWith("mc")))
                    {
                        temp[2] = char.ToUpperInvariant(temp[2]);
                    }
                }
                // Q&A etc.
                if (Utils.IsInList(word, ClipXConstants.WORDS_WITH_AMPERSAND))
                {
                    for (int i = 0; i < temp.Length; i++)
                    {
                        temp[i] = char.ToUpperInvariant(temp[i]);
                    }
                }

                // Write transformed word back into buffer
                int relative = start - selstart; //'start' here is an absolute address from token.start
                // 1) Token starts before selection → skip or handle character-wise
                if (relative < 0)
                    continue;

                // 2) Token ends after selection → clamp
                //if (relative + word.Length > buffer.Length)
                //    continue;

                // 3) Token must be non-empty
                if (word.Length == 0)
                    continue;

                // if token ends outside buffer, truncate the loop. E.g. a filename is detected by lookahead that wasn't fully selected
                for (int i = 0; i < word.Length && relative + i < buffer.Length; i++)
                    buffer[relative + i] = temp[i];    

                prevWord = word;
            }

            return new string(buffer);
        }
    }
}
