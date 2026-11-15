using System;

namespace ClipX2.TextEngine
{
    internal enum TokenType
    {
        Word,               // letters/digits with optional internal apostrophes
        DottedAbbrev,       // dot-separated letter sequences (i.e., e.g., U.S.A., Ph.D.)
        Acronym,            // Common acronyms written all caps: NASA, GCSE, USA
        Filename,           // word + dot + extension (report.doc, README.TXT)
        Number,             // digits with optional punctuation (1,234.56)
        Punctuation,        // .,?!;: etc.
        Whitespace,         // spaces, tabs, newlines
        Other               // symbols, emojis, anything not above
    }
    [Flags]
    internal enum TokenFlags
    {
        None = 0,
        PrecededByHyphen = 1 << 0,      // token follows a hyphen
        PrecededByApostrophe = 1 << 1,  // token follows an apostrophe (rare but useful)
        AllCaps = 1 << 2,               // token is entirely uppercase
        FirstInSentence = 1 << 3,       // token begins a sentence
        ContainsDigits = 1 << 4,        // token includes one or more digits
        Abbreviation = 1 << 5           // 'Word abbrev' e.g. Q&A
    }

    internal readonly struct Token
    {
        public int Start { get; }
        public int Length { get; }
        public TokenType Type { get; }
        public TokenFlags Flags { get; }
        public Token(int start, int length, TokenType type, TokenFlags flags)
        {
            Start = start;
            Length = length;
            Type = type;
            Flags = flags;
        }
    }
}
