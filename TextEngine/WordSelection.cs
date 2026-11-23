using System.Globalization;

namespace ClipX2.TextEngine
{
    /// <summary>
    /// Ctrl+W, select word. Word characters are letters and digits (and the accents combined with
    /// them); everything else that isn't white space is "other" (punctuation, symbols: "&lt;!--").
    /// With no selection: the word at the caret, the word just before it if the caret is at a word's
    /// end, else the run of other characters at the caret (never white space). With a selection: it
    /// grows to the right, over the white space and the word (or run of other characters) after it,
    /// or over the run of other characters, or the rest of the word, that it ends in front of.
    /// Paragraph and line breaks are never crossed (a paragraph is the unit of every text operation).
    /// </summary>
    public static class WordSelection
    {
        private enum Kind { Word, Other, Space, Break }

        /// <summary>
        /// The new selection in <paramref name="text"/> (which may be just the stretch of a longer
        /// text around the selection). Returns the selection unchanged if there's nothing to select.
        /// </summary>
        public static (int Start, int Length) Next(string text, int start, int length)
        {
            if (string.IsNullOrEmpty(text) || start < 0 || start > text.Length) return (start, length);
            if (start + length > text.Length) length = text.Length - start;

            if (length > 0)
            {
                int end = start + length;
                Kind next = KindAt(text, end);
                if (next == Kind.Space)
                {
                    int q = Skip(text, end, Kind.Space);
                    Kind after = KindAt(text, q);
                    if (after != Kind.Word && after != Kind.Other) return (start, length); // only spaces before the line's end
                    return (start, Skip(text, q, after) - start);
                }
                if (next == Kind.Word || next == Kind.Other) return (start, Skip(text, end, next) - start);
                return (start, length); // at the end of the paragraph
            }

            int p = start;
            Kind right = KindAt(text, p), left = KindAt(text, p - 1);
            if (right == Kind.Word) return Around(text, p, Kind.Word);
            if (left == Kind.Word) return Before(text, p, Kind.Word);
            if (right == Kind.Other) return Around(text, p, Kind.Other);
            if (left == Kind.Other) return Before(text, p, Kind.Other);
            if (right == Kind.Space)
            {
                // between spaces: the word (or other characters) after them, if on this line
                int q = Skip(text, p, Kind.Space);
                Kind after = KindAt(text, q);
                if (after == Kind.Word || after == Kind.Other) return (q, Skip(text, q, after) - q);
            }
            return (start, 0);
        }

        /// <summary>The run of <paramref name="kind"/> that position <paramref name="p"/> is in.</summary>
        private static (int, int) Around(string text, int p, Kind kind)
        {
            int from = Back(text, p, kind);
            return (from, Skip(text, p, kind) - from);
        }

        /// <summary>The run of <paramref name="kind"/> ending at <paramref name="p"/>.</summary>
        private static (int, int) Before(string text, int p, Kind kind)
        {
            int from = Back(text, p, kind);
            return (from, p - from);
        }

        /// <summary>The position after the run of <paramref name="kind"/> starting at <paramref name="i"/>.</summary>
        private static int Skip(string text, int i, Kind kind)
        {
            while (i < text.Length && KindAt(text, i) == kind) i++;
            return i;
        }

        /// <summary>The start of the run of <paramref name="kind"/> ending at <paramref name="i"/>.</summary>
        private static int Back(string text, int i, Kind kind)
        {
            while (i > 0 && KindAt(text, i - 1) == kind) i--;
            return i;
        }

        private static Kind KindAt(string text, int i)
        {
            if (i < 0 || i >= text.Length) return Kind.Break;
            char c = text[i];
            if (c == '\n' || c == '\r' || c == '\v' || c == '\f' || c == (char)0x2028 || c == (char)0x2029) return Kind.Break;
            if (char.IsWhiteSpace(c)) return Kind.Space;
            // the second half of a character outside the BMP: as its first half
            if (char.IsLowSurrogate(c) && i > 0 && char.IsHighSurrogate(text[i - 1])) i--;
            if (char.IsLetterOrDigit(text, i)) return Kind.Word;
            UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(text, i);
            if (cat == UnicodeCategory.NonSpacingMark || cat == UnicodeCategory.SpacingCombiningMark || cat == UnicodeCategory.EnclosingMark)
                return Kind.Word; // an accent combined with a letter
            return Kind.Other;
        }
    }
}
