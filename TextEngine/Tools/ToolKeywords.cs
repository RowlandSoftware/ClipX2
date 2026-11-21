using System;
using System.Collections.Generic;

namespace ClipX2.TextEngine.Tools
{
    /// <summary>
    /// Other words for each tool, for <see cref="ToolFinder"/>: what people call the thing they
    /// want when they don't know the tool's name; American spellings; examples of what the tool
    /// deals with (%20, â€™). English; other languages' go with the translations.
    /// </summary>
    public static class ToolKeywords
    {
        private static readonly Dictionary<string, string[]> Words = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            // Change case
            ["ccUPP"] = new[] { "capitals", "caps", "all caps", "uppercase", "capitalise everything", "capitalize", "shout", "block capitals" },
            ["ccLOW"] = new[] { "lowercase", "small letters", "no capitals", "remove capitals", "decapitalise", "uncapitalize", "lower" },
            ["ccREV"] = new[] { "swap case", "invert case", "toggle case", "caps lock", "wrong case", "flip case" },
            ["ccINC"] = new[] { "initial capitals", "capitalise each word", "capitalize every word", "first letter of each word", "proper case", "word caps" },
            ["ccTIT"] = new[] { "title case", "headline case", "capitalise title", "book title", "heading capitals" },
            ["ccSEN"] = new[] { "sentence case", "normal case", "fix capitals", "ordinary case", "first letter capital" },
            ["ccCAP"] = new[] { "capitalise sentences", "capitalize sentences", "start of sentence", "first letter of sentence" },
            ["ccCYC"] = new[] { "change case", "cycle case", "shift f3", "case" },

            // Edit
            ["edSTR"] = new[] { "email", "quote marks", "quoted", ">", "reply", "strip email", "forwarded", "chevrons", "remove >" },
            ["edPLA"] = new[] { "plain characters", "straight quotes", "dumb quotes", "remove smart quotes", "ascii", "simple characters", "plain quotes", "hyphens" },
            ["edCRL"] = new[] { "smart quotes", "curly quotes", "typographic", "dashes", "em dash", "en dash", "ellipsis", "proper quotes", "typography", "speech marks", "66 and 99", "66 & 99" },
            ["edUDC"] = new[] { "url", "link", "web address", "%20", "percent", "decode link", "encoded url" },
            ["edHDC"] = new[] { "html entities", "&amp;", "&nbsp;", "entities", "decode html", "html codes" },
            ["edUTF"] = new[] { "utf-8", "utf8", "mojibake", "garbled", "â€™", "encoding", "weird characters", "strange characters", "corrupt characters" },
            ["edMIM"] = new[] { "mime", "quoted printable", "=20", "email encoding", "=3d" },
            ["edEOL"] = new[] { "line endings", "line breaks", "crlf", "carriage return", "soft returns", "shift enter", "newlines", "new lines"},

            // Format
            ["foTRM"] = new[] { "trim", "leading spaces", "trailing spaces", "whitespace", "spaces at start", "spaces at end", "remove spaces" },
            ["foTRA"] = new[] { "trailing spaces", "end of line spaces", "spaces at end", "trailing whitespace" },
            ["foSSP"] = new[] { "double spaces", "extra spaces", "multiple spaces", "single space", "two spaces", "too many spaces" },
            ["foS2T"] = new[] { "spaces to tabs", "columns", "tab separated", "convert spaces" },
            ["foT2S"] = new[] { "tabs to spaces", "expand tabs", "remove tabs", "convert tabs" },
            ["foIND"] = new[] { "indent", "unindent", "outdent", "margin", "move right", "move left", "prefix", "quote text", "add >", "tab in" },
            ["foWRP"] = new[] { "wrap", "line length", "hard wrap", "column width", "justify", "fixed width", "line width", "characters per line", "break lines" },
            ["foREM"] = new[] { "blank lines", "empty lines", "remove lines", "double spacing", "gaps", "delete empty lines", "extra lines" },
            ["foPAR"] = new[] { "unwrap", "join lines", "merge lines", "paragraphs", "remove line breaks", "reflow", "hard returns", "broken lines", "pdf text" },

            // Lists
            ["liBUL"] = new[] { "bullets", "bullet points", "unordered list", "dot points", "list", "dots" },
            ["liNUM"] = new[] { "numbering", "numbered list", "ordered list", "1 2 3", "outline", "numbers", "list", "letters" },
            ["liREN"] = new[] { "line numbers", "number lines", "count lines", "numbered lines", "code lines" },
            ["liRMB"] = new[] { "remove bullets", "delete bullets", "unbullet", "no bullets" },
            ["liREM"] = new[] { "remove numbers", "delete numbering", "unnumber", "no numbers", "remove numbering" },
            ["liRLN"] = new[] { "remove line numbers", "delete line numbers", "strip line numbers" },

            // Sorting
            ["soSMA"] = new[] { "natural sort", "smart sort", "sort numbers in text", "item 2 before item 10", "sort" },
            ["soALP"] = new[] { "sort", "alphabetical", "alphabetise", "alphabetize", "a-z", "order", "arrange", "sort lines" },
            ["soNUM"] = new[] { "numeric sort", "sort numbers", "number order", "sort by number" },
            ["soREV"] = new[] { "reverse", "flip order", "backwards", "upside down", "invert order", "reverse lines" },
            ["soNUP"] = new[] { "move line up", "nudge up", "swap lines", "move up", "alt up" },
            ["soNUD"] = new[] { "move line down", "nudge down", "swap lines", "move down", "alt down" },
            ["soKEY"] = new[] { "sort list", "comma separated", "sort items", "keywords", "sort words", "tags" },

            // Language tools
            ["chPNC"] = new[] { "punctuation", "spaces before punctuation", "brackets", "unmatched quotes", "check punctuation", "proofread", "braces", "parentheses" },
            ["chSPE"] = new[] { "spelling", "spell check", "spellcheck", "typos", "grammar", "proofread", "check spelling", "misspelt", "misspelled", "f7" },
            ["chGRA"] = new[] { "grammar", "spelling and grammar", "proofread" },
            ["chSTAT"] = new[] { "word count", "count words", "statistics", "stats", "characters", "readability", "length", "how long", "sentences", "fog index", "reading level" },

            // Styles
            ["stFNT"] = new[] { "font", "typeface", "change font" },
            ["stSIZ"] = new[] { "font size", "text size", "bigger", "smaller", "larger", "points" },
            ["stHDG"] = new[] { "heading", "title", "h1", "h2", "h3", "headline", "subheading" },
            ["stBLD"] = new[] { "bold", "strong", "ctrl b", "heavy" },
            ["stITA"] = new[] { "italic", "italics", "emphasis", "slanted", "ctrl i" },
            ["stUND"] = new[] { "underline", "underlined", "ctrl u" },
            ["stSTR"] = new[] { "strikethrough", "strike through", "cross out", "line through", "struck" },
            ["stSUP"] = new[] { "superscript", "power", "exponent", "squared", "x²", "raised" },
            ["stSUB"] = new[] { "subscript", "chemical formula", "h₂o", "lowered" },
            ["stCLR"] = new[] { "clear formatting", "remove formatting", "strip formatting", "plain", "reset formatting", "ctrl m", "unformat" },
            ["stALL"] = new[] { "align left", "left aligned", "left justify", "alignment", "range left", "left range", "ragged right" },
            ["stALC"] = new[] { "centre", "center", "centred", "centered", "middle", "alignment" },
            ["stALR"] = new[] { "align right", "right aligned", "right justify", "alignment", "range right", "right range", "ragged left"},
            ["stALJ"] = new[] { "justify", "justified", "both edges", "straight edges", "alignment", "flush" },
            ["stCOL"] = new[] { "colour", "color", "text colour", "font color", "red", "blue", "coloured text" },
            ["stHIL"] = new[] { "highlight", "highlighter", "marker", "background colour", "background color", "yellow" },
            ["stIIN"] = new[] { "indent", "increase indent", "move right", "tab", "list level down", "demote" },
            ["stDIN"] = new[] { "outdent", "unindent", "decrease indent", "move left", "shift tab", "list level up", "promote" },

            // ClipX itself
            ["clipxREF"] = new[] { "start again", "reload", "refresh", "clipboard again", "undo everything", "reset", "paste again", "start over" },
            ["clipxRPT"] = new[] { "repeat", "again", "do it again", "last tool", "ctrl r" },
            ["clipxOPT"] = new[] { "settings", "options", "preferences", "configure", "setup", "dictionaries", "languages", "fixed width font" },
            ["clipxFIN"] = new[] { "done", "finish", "copy", "paste back", "exit", "close", "put on clipboard", "alt d" },
            ["clipxMOD"] = new[] { "plain text", "rich text", "formatted", "switch mode", "remove all formatting", "convert", "unformatted" },
            ["clipxQSR"] = new[] { "find", "search", "look for", "ctrl f", "where is", "highlight matches" },
        };

        /// <summary>The other words for a tool (none if it has none).</summary>
        public static IReadOnlyList<string> For(string tag) =>
            tag != null && Words.TryGetValue(tag, out string[] words) ? words : Array.Empty<string>();
    }
}
