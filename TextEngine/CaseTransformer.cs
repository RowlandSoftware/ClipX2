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
                    return InitCapsEngine.TransformPlain(text, start, length, false);

                case CaseMode.Title:
                    return InitCapsEngine.TransformPlain(text, start, length, true);

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
