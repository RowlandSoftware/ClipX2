using ClipX2.TextEngine.Engines;
using System;

namespace ClipX2.TextEngine
{
    /// <summary>The letter-case conversions the engine can perform.</summary>
    public enum CaseMode
    {
        /// <summary>ALL CAPITALS.</summary>
        Upper,
        /// <summary>all lower case.</summary>
        Lower,
        /// <summary>sWAP tHE cASE of every letter.</summary>
        Reverse,
        /// <summary>Capitalise the first letter of each sentence and lower-case the rest.</summary>
        Sentence,
        /// <summary>Capitalise the first letter of each sentence, leaving other letters unchanged.</summary>
        SentenceCaps,
        /// <summary>Capitalise The First Letter Of Every Word.</summary>
        InitCaps,
        /// <summary>Capitalise Words as in a Title, Leaving Minor Words in Lower Case.</summary>
        Title
    }

    /// <summary>
    /// Public entry point for letter-case conversion. Works on plain strings only, so it can be
    /// used by any program (WinForms, WPF, console, web) without a dependency on a UI framework.
    /// </summary>
    public static class CaseTransformer
    {
        /// <summary>Converts the whole of <paramref name="text"/>.</summary>
        /// <returns>The converted text. It is always the same length as the input.</returns>
        public static string Transform(string text, CaseMode mode)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            return Transform(text, 0, text.Length, mode);
        }

        /// <summary>
        /// Converts part of <paramref name="text"/>, for example the user's selection.
        /// The text around the range is used as context (e.g. whether the range starts a sentence).
        /// </summary>
        /// <param name="text">The full text.</param>
        /// <param name="start">Index of the first character to convert.</param>
        /// <param name="length">Number of characters to convert.</param>
        /// <param name="mode">The conversion to apply.</param>
        /// <returns>
        /// The converted range only (not the full text). It is always <paramref name="length"/>
        /// characters long, so it can be written back character-for-character, which lets callers
        /// preserve formatting such as RTF.
        /// </returns>
        public static string Transform(string text, int start, int length, CaseMode mode)
            => Transform(text, start, length, mode, keepCapitals: true);

        /// <summary>
        /// As <see cref="Transform(string, int, int, CaseMode)"/>, with control over whether
        /// <see cref="CaseMode.InitCaps"/> and <see cref="CaseMode.Title"/> keep existing capitals.
        /// </summary>
        /// <param name="text">The full text.</param>
        /// <param name="start">Index of the first character to convert.</param>
        /// <param name="length">Number of characters to convert.</param>
        /// <param name="mode">The conversion to apply.</param>
        /// <returns>The converted range only, always <paramref name="length"/> characters long.</returns>
        /// <param name="keepCapitals">
        /// True (the default) leaves all-caps words (NASA) and mixed-case names (OmniFocus) as they
        /// are. False forces every word to a capital followed by lower case, as when the user steps
        /// through text with the keyboard. Name rules (iPhone, McDonald, O'Neill) still apply.
        /// Ignored by the other modes.
        /// </param>
        public static string Transform(string text, int start, int length, CaseMode mode, bool keepCapitals)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (start < 0 || start > text.Length) throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0 || start + length > text.Length) throw new ArgumentOutOfRangeException(nameof(length));

            string seltext = text.Substring(start, length);
            switch (mode)
            {
                case CaseMode.Sentence:
                    return SentenceCaseEngine.TransformPlain(text, start, length, true);

                case CaseMode.SentenceCaps:
                    return SentenceCaseEngine.TransformPlain(text, start, length, false);

                case CaseMode.InitCaps:
                case CaseMode.Title:
                {
                    // Whether a letter is capitalised depends on its word and its paragraph (start of
                    // a word, first or last word of a title, shouted text), so convert the whole
                    // paragraphs the range touches and return just the part asked for. Otherwise a
                    // selection starting mid-word, or a single character, would look like a word start.
                    int from = ParagraphStart(text, start);
                    int to = ParagraphEnd(text, start + length);
                    string paragraphs = InitCapsEngine.TransformPlain(text, from, to - from, mode == CaseMode.Title, keepCapitals);
                    return paragraphs.Substring(start - from, length);
                }

                case CaseMode.Upper:
                    return seltext.ToUpperInvariant();

                case CaseMode.Lower:
                    return seltext.ToLowerInvariant();

                case CaseMode.Reverse:
                    return ReverseCase(seltext);

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

        /// <summary>Start of the paragraph containing <paramref name="pos"/>.</summary>
        private static int ParagraphStart(string text, int pos)
        {
            while (pos > 0 && text[pos - 1] != '\n' && text[pos - 1] != '\r') pos--;
            return pos;
        }

        /// <summary>End of the paragraph containing <paramref name="pos"/> (the index of its line break, or the text length).</summary>
        private static int ParagraphEnd(string text, int pos)
        {
            while (pos < text.Length && text[pos] != '\n' && text[pos] != '\r') pos++;
            return pos;
        }

        private static string ReverseCase(string s)
        {
            var chars = s.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                chars[i] = char.IsUpper(c)
                    ? char.ToLowerInvariant(c)
                    : char.IsLower(c)
                        ? char.ToUpperInvariant(c)
                        : c;
            }
            return new string(chars);
        }
    }
}
