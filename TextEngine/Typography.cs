using System;
using System.Collections.Generic;
using System.Linq;

namespace ClipX2.TextEngine
{
    /// <summary>
    /// ClipX 1's character tools: Plain characters (edPLA), Curly quotes (edCRL) and Regularise
    /// line ends (edEOL). Each returns edits for [<c>from</c>, <c>to</c>), one per character or
    /// sequence changed, so the rest of the text and its formatting are untouched; characters just
    /// outside the range are read for context.
    /// </summary>
    public static class Typography
    {
        /************ Plain characters ************/

        /// <summary>Each special character and its plain replacement ("" = removed).</summary>
        private static readonly Dictionary<char, string> Plain = new Dictionary<char, string>
        {
            // ClipX 1's: quotes, ellipsis, acute accent used as an apostrophe, dashes
            ['‚'] = "'", ['‘'] = "'", ['’'] = "'", ['‛'] = "'", ['´'] = "'",
            ['“'] = "\"", ['”'] = "\"", ['„'] = "\"", ['‟'] = "\"",
            ['…'] = "...",
            // En and em dashes the reverse of Curly quotes (-- → –, --- → —), so the two round-trip
            ['–'] = "--", ['—'] = "---", ['―'] = "---", ['‒'] = "-", ['‐'] = "-", ['‑'] = "-", ['−'] = "-",
            // Primes (feet and inches, minutes and seconds)
            ['′'] = "'", ['″'] = "\"",
            // Hard and special-width spaces
            [(char)0x00A0] = " ", [(char)0x2002] = " ", [(char)0x2003] = " ", [(char)0x2004] = " ", [(char)0x2005] = " ", [(char)0x2006] = " ",
            [(char)0x2007] = " ", [(char)0x2008] = " ", [(char)0x2009] = " ", [(char)0x200A] = " ", [(char)0x202F] = " ", [(char)0x205F] = " ", [(char)0x3000] = " ",
            // Invisible: soft hyphen, zero-width space and joiners, byte-order mark
            [(char)0x00AD] = "", [(char)0x200B] = "", [(char)0x200C] = "", [(char)0x200D] = "", [(char)0x2060] = "", [(char)0xFEFF] = "",
        };

        /// <summary>
        /// Plain characters (edPLA): curly quotes → straight, en dash → --, em dash → --- (the reverse of
        /// Curly quotes), other dashes → hyphens,
        /// … → ..., primes → ' and ", hard and special spaces → spaces; soft hyphens and zero-width
        /// characters go. For pasting where only plain ASCII punctuation will do.
        /// </summary>
        public static List<TextEdit> PlainCharacters(string text, int from, int to)
        {
            var edits = new List<TextEdit>();
            for (int i = from; i < to; i++)
                if (Plain.TryGetValue(text[i], out string plain)) edits.Add(new TextEdit(i, 1, plain));
            return edits;
        }

        /************ Curly quotes ************/

        /// <summary>Contraction endings after an apostrophe: don't, I've, I'm, he's, we'll, I'd, we're.</summary>
        private static readonly string[] ContractionEndings = { "t", "ve", "nt", "m", "s", "ll", "d", "re" };

        /// <summary>Words that start with an apostrophe: 'tis, 'twas, 'em, 'cause, 'til, rock 'n' roll.</summary>
        private static readonly string[] LeadingElisions = { "tis", "twas", "em", "cause", "til", "n", "round", "bout", "cos" };

        /// <summary>The result of <see cref="CurlyQuotes"/>: the edits, and paragraphs whose double quotes don't pair up.</summary>
        public sealed class CurlyResult
        {
            public List<TextEdit> Edits { get; } = new List<TextEdit>();
            /// <summary>Start of each paragraph with more opening than closing double quotes, or the reverse.</summary>
            public List<int> Unmatched { get; } = new List<int>();
        }

        /// <summary>
        /// Curly quotes etc. (edCRL): straight quotes become curly, each turned by what is either
        /// side of it: after a space, the start of a line, a bracket or a dash it opens (‘ “),
        /// otherwise it closes (’ ”). An apostrophe is ’ wherever it is (don't, the boys' toys,
        /// 'tis, '90s). A straight quote just after a number outside a quotation is feet or inches
        /// (6'2"), so it stays straight. Also --- → em dash, -- → en dash (not a command-line
        /// --option), ... → ….
        /// </summary>
        public static CurlyResult CurlyQuotes(string text, int from, int to)
        {
            var result = new CurlyResult();
            var edits = result.Edits;
            int paragraph = from, opened = 0, closed = 0;

            for (int i = from; i < to; i++)
            {
                char c = text[i];
                char prev = i > 0 ? text[i - 1] : '\0';
                char next = i + 1 < text.Length ? text[i + 1] : '\0';

                if (c == '\n' || c == '\r')
                {
                    if (opened != closed) result.Unmatched.Add(paragraph);
                    paragraph = i + 1; opened = closed = 0;
                    continue;
                }

                if (c == '\'')
                {
                    if (char.IsDigit(prev) && !(next == 's' && !char.IsLetter(At(text, i + 2)))) continue; // 6'2": feet (but 1990's)
                    edits.Add(new TextEdit(i, 1, Opens(prev) && !IsLeadingElision(text, i) ? "‘" : "’"));
                }
                else if (c == '"')
                {
                    if (char.IsDigit(prev) && opened == closed) continue; // 12": inches
                    bool opens = Opens(prev) && !(IsSpace(prev) && (IsSpace(next) || next == '\0') && opened > closed);
                    if (opens) opened++; else closed++;
                    edits.Add(new TextEdit(i, 1, opens ? "“" : "”"));
                }
                else if (c == '“') opened++;
                else if (c == '”') closed++;
                else if (c == '-' && next == '-')
                {
                    int n = 2;
                    while (i + n < to && text[i + n] == '-') n++;
                    if (n > 3) { i += n - 1; continue; }                                   // ---- a rule: leave it
                    if (n == 2 && (prev == '\0' || IsSpace(prev) || IsBreak(prev)) && char.IsLetter(At(text, i + 2))) { i++; continue; } // --option
                    if (i + n > to) break;
                    edits.Add(new TextEdit(i, n, n == 3 ? "—" : "–"));
                    i += n - 1;
                }
                else if (c == '.' && next == '.' && i + 2 < to && text[i + 2] == '.')
                {
                    int n = 3;
                    while (i + n < to && text[i + n] == '.') n++;
                    if (n == 3) edits.Add(new TextEdit(i, 3, "…")); // longer runs (dot leaders) are left alone
                    i += n - 1;
                }
            }
            if (opened != closed) result.Unmatched.Add(paragraph);
            return result;
        }

        private static char At(string text, int i) => i >= 0 && i < text.Length ? text[i] : '\0';
        private static bool IsSpace(char c) => c == ' ' || c == '\t' || c == (char)0x00A0;
        private static bool IsBreak(char c) => c == '\n' || c == '\r' || c == '\v';

        /// <summary>A quote after this opens: the start, a space or line break, an opening bracket or quote, a dash or slash.</summary>
        private static bool Opens(char prev) =>
            prev == '\0' || IsSpace(prev) || IsBreak(prev) || "([{<“‘«‹—–-/".IndexOf(prev) >= 0;

        /// <summary>'tis, 'twas, rock 'n' roll, '90s: an apostrophe at the start of a word.</summary>
        private static bool IsLeadingElision(string text, int i)
        {
            if (char.IsDigit(At(text, i + 1)) && char.IsDigit(At(text, i + 2))) return true; // '90s, '24
            int w = i + 1;
            while (w < text.Length && char.IsLetter(text[w])) w++;
            string word = text.Substring(i + 1, w - i - 1).ToLowerInvariant();
            if (word == "n") return At(text, w) == '\''; // 'n' needs its closing apostrophe
            return LeadingElisions.Contains(word) && !char.IsLetter(At(text, w));
        }

        /************ Line ends ************/

        /// <summary>
        /// Regularise line ends (edEOL): every kind of line break becomes an ordinary one
        /// (<paramref name="newLine"/>): soft line breaks (Shift+Enter, \v), Unicode line and
        /// paragraph separators, NEL, form feeds, and a lone CR or LF where the other is used.
        /// </summary>
        public static List<TextEdit> RegulariseLineEnds(string text, int from, int to, string newLine = "\n")
        {
            var edits = new List<TextEdit>();
            for (int i = from; i < to; i++)
            {
                char c = text[i];
                if (c == '\r' && At(text, i + 1) == '\n' && i + 1 < to)
                {
                    if (newLine != "\r\n") edits.Add(new TextEdit(i, 2, newLine));
                    i++;
                }
                else if (c == '\r' || c == '\n')
                {
                    if (newLine != c.ToString()) edits.Add(new TextEdit(i, 1, newLine));
                }
                else if (c == '\v' || c == '\f' || c == (char)0x2028 || c == (char)0x2029 || c == '\u0085')
                    edits.Add(new TextEdit(i, 1, newLine));
            }
            return edits;
        }
    }
}
