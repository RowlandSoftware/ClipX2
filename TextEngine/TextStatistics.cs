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
    }
}
