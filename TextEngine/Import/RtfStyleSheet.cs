using System;
using System.Collections.Generic;
using System.Text;
using ClipX2.TextEngine.Export;

namespace ClipX2.TextEngine.Import
{
    /// <summary>Character formatting as RTF sets it. Null: not set (taken from the style or the default).</summary>
    public sealed class RtfCharFormat
    {
        public bool? Bold, Italic, Underline, Strikethrough, Superscript, Subscript, Hidden;
        /// <summary>Font table index.</summary>
        public int? Font;
        /// <summary>Points.</summary>
        public float? Size;
        /// <summary>Colour table index (0 = automatic).</summary>
        public int? Colour;
        /// <summary>Colour table index of the highlight (0 = none).</summary>
        public int? Highlight;
        /// <summary>Windows LCID.</summary>
        public int? Language;

        public RtfCharFormat Clone() => (RtfCharFormat)MemberwiseClone();

        /// <summary>This, with whatever <paramref name="top"/> sets on top of it.</summary>
        public RtfCharFormat Over(RtfCharFormat top)
        {
            if (top == null) return this;
            Bold = top.Bold ?? Bold; Italic = top.Italic ?? Italic; Underline = top.Underline ?? Underline;
            Strikethrough = top.Strikethrough ?? Strikethrough; Superscript = top.Superscript ?? Superscript;
            Subscript = top.Subscript ?? Subscript; Hidden = top.Hidden ?? Hidden; Font = top.Font ?? Font;
            Size = top.Size ?? Size; Colour = top.Colour ?? Colour; Highlight = top.Highlight ?? Highlight;
            Language = top.Language ?? Language;
            return this;
        }

        public bool SameAs(RtfCharFormat o) =>
            o != null && Bold == o.Bold && Italic == o.Italic && Underline == o.Underline && Strikethrough == o.Strikethrough
            && Superscript == o.Superscript && Subscript == o.Subscript && Hidden == o.Hidden && Font == o.Font
            && Size == o.Size && Colour == o.Colour && Highlight == o.Highlight && Language == o.Language;

        public bool IsEmpty => !(Bold.HasValue || Italic.HasValue || Underline.HasValue || Strikethrough.HasValue
            || Superscript.HasValue || Subscript.HasValue || Hidden.HasValue || Font.HasValue || Size.HasValue
            || Colour.HasValue || Highlight.HasValue || Language.HasValue);

        /// <summary>Applies a character-formatting control word; false if it isn't one.</summary>
        internal bool Apply(string word, int? param)
        {
            bool on = param != 0;
            switch (word)
            {
                case "b": Bold = on; return true;
                case "i": Italic = on; return true;
                case "ul": case "uld": case "uldb": case "uldash": case "uldashd": case "uldashdd": case "ulw":
                case "ulwave": case "ulth": case "ulthd": case "ulthdash": case "ulhwave": case "ululdbwave":
                case "ulthldash": case "ulldash": case "ulthdashd": case "ulthdashdd":
                    Underline = on; return true;
                case "ulnone": Underline = false; return true;
                case "strike": case "striked": Strikethrough = on; return true;
                case "super": Superscript = on; if (on) Subscript = false; return true;
                case "sub": Subscript = on; if (on) Superscript = false; return true;
                case "nosupersub": Superscript = false; Subscript = false; return true;
                case "v": Hidden = on; return true;
                case "f": if (param.HasValue) Font = param; return true;
                case "fs": if (param > 0) Size = param.Value / 2f; return true;
                case "cf": Colour = param ?? 0; return true;
                case "highlight": case "cb": case "chcbpat": Highlight = param ?? 0; return true;
                case "lang": if (param.HasValue) Language = param; return true;
                default: return false;
            }
        }
    }

    /// <summary>Paragraph formatting as RTF sets it. Null: not set.</summary>
    public sealed class RtfParaFormat
    {
        /// <summary>0 left, 1 centre, 2 right, 3 justified.</summary>
        public int? Alignment;
        /// <summary>Points.</summary>
        public float? LeftIndent, FirstLineIndent, SpaceBefore, SpaceAfter;
        /// <summary>0-based (0 = Heading 1).</summary>
        public int? OutlineLevel;
        /// <summary>Line spacing as RTF gives it: \sl (twips; 0 = single) and \slmult (1 = a multiple of single).</summary>
        public int? Sl;
        public bool? SlMult;

        /// <summary>The line spacing as a rule and value: see <see cref="LineSpacing"/>.</summary>
        public LineSpacing Spacing => LineSpacing.FromRtf(Sl ?? 0, SlMult ?? false);

        public RtfParaFormat Clone() => (RtfParaFormat)MemberwiseClone();

        public RtfParaFormat Over(RtfParaFormat top)
        {
            if (top == null) return this;
            Alignment = top.Alignment ?? Alignment; LeftIndent = top.LeftIndent ?? LeftIndent;
            FirstLineIndent = top.FirstLineIndent ?? FirstLineIndent; SpaceBefore = top.SpaceBefore ?? SpaceBefore;
            SpaceAfter = top.SpaceAfter ?? SpaceAfter; OutlineLevel = top.OutlineLevel ?? OutlineLevel;
            Sl = top.Sl ?? Sl; SlMult = top.SlMult ?? SlMult;
            return this;
        }

        internal bool Apply(string word, int? param)
        {
            switch (word)
            {
                case "ql": Alignment = 0; return true;
                case "qc": Alignment = 1; return true;
                case "qr": Alignment = 2; return true;
                case "qj": case "qd": Alignment = 3; return true;
                case "li": LeftIndent = (param ?? 0) / 20f; return true;
                case "fi": FirstLineIndent = (param ?? 0) / 20f; return true;
                case "sb": SpaceBefore = (param ?? 0) / 20f; return true;
                case "sa": SpaceAfter = (param ?? 0) / 20f; return true;
                case "outlinelevel": OutlineLevel = param; return true;
                case "sl": Sl = param ?? 0; return true;
                case "slmult": SlMult = (param ?? 1) != 0; return true;
                default: return false;
            }
        }
    }

    /// <summary>One named style from an RTF style sheet.</summary>
    public sealed class RtfStyle
    {
        public int Number { get; internal set; }
        public string Name { get; internal set; }
        public bool IsCharacter { get; internal set; }
        public int? BasedOn { get; internal set; }

        /// <summary>What the style itself sets (its parents' settings not included).</summary>
        public RtfParaFormat Para { get; } = new RtfParaFormat();
        public RtfCharFormat Char { get; } = new RtfCharFormat();

        /// <summary>Its definition as it came, e.g. <c>{\s60\sbasedon74 … Body Text First Para;}</c>.</summary>
        public string Definition { get; internal set; }

        /// <summary>Everything it sets, its parents' settings included (paragraph styles: over the document's defaults).</summary>
        public RtfParaFormat ResolvedPara { get; internal set; }
        public RtfCharFormat ResolvedChar { get; internal set; }

        /// <summary>
        /// Its formatting as RTF control words, its parents' first (e.g. <c>\sl300\slmult1\f4\fs24\sb119</c>):
        /// written again after each <c>\sN</c> / <c>\csN</c>, as RTF expects. A reader such as Writer's
        /// takes whatever isn't repeated as direct formatting that overrides the style.
        /// </summary>
        public string Words { get; internal set; }

        /// <summary>Heading level (1 = Heading 1), 0 for none: its outline level, or "heading N", or its parent's.</summary>
        public int HeadingLevel { get; internal set; }

        public override string ToString() => (IsCharacter ? "cs" : "s") + Number + " " + Name;
    }

    /// <summary>
    /// The styles, fonts and colours an RTF document came with (a word processor's: LibreOffice,
    /// Word). Kept with the text, so that Done can name the styles again: pasted back, the word
    /// processor then applies its own styles instead of formatting each paragraph directly.
    /// </summary>
    public sealed class RtfStyleSheet
    {
        private readonly Dictionary<int, RtfStyle> _para = new Dictionary<int, RtfStyle>();
        private readonly Dictionary<int, RtfStyle> _char = new Dictionary<int, RtfStyle>();

        public List<RtfStyle> Styles { get; } = new List<RtfStyle>();
        public Dictionary<int, string> Fonts { get; } = new Dictionary<int, string>();
        /// <summary>The colour table: 0xRRGGBB, or null for automatic.</summary>
        public List<int?> Colours { get; } = new List<int?>();
        public int? DefaultFont { get; internal set; }
        public int DefaultLanguage { get; internal set; }

        /// <summary>The source's tables as they came, to write back (fonts and colours with any new ones added).</summary>
        public string FontTableRtf { get; internal set; }
        public string ColourTableRtf { get; internal set; }

        /// <summary>
        /// Styles whose paragraphs with no text the source numbered (a Separator numbered I, II, III
        /// by its style): the editor shows the number, and Done leaves it to the style again.
        /// </summary>
        private readonly HashSet<string> _styleNumbered = new HashSet<string>(StringComparer.Ordinal);

        internal void RecordStyleNumbered(string style)
        {
            if (style != null) _styleNumbered.Add(style);
        }

        public bool IsStyleNumbered(string style) => style != null && _styleNumbered.Contains(style);

        /// <summary>How many paragraphs / runs used each style in the source: the likelier style wins a tie.</summary>
        public Dictionary<string, int> Usage { get; } = new Dictionary<string, int>(StringComparer.Ordinal);

        public RtfStyle Paragraph(int number) => _para.TryGetValue(number, out RtfStyle s) ? s : null;
        public RtfStyle Character(int number) => _char.TryGetValue(number, out RtfStyle s) ? s : null;
        public RtfStyle ParagraphNamed(string name) => Find(name, false);
        public RtfStyle CharacterNamed(string name) => Find(name, true);

        private RtfStyle Find(string name, bool character)
        {
            if (name == null) return null;
            foreach (RtfStyle s in Styles) if (s.IsCharacter == character && s.Name == name) return s;
            return null;
        }

        public string FontName(int? index) => index.HasValue && Fonts.TryGetValue(index.Value, out string n) ? n : null;

        public int? Rgb(int? index) => index.HasValue && index.Value > 0 && index.Value < Colours.Count ? Colours[index.Value] : null;

        internal void Add(RtfStyle style)
        {
            Styles.Add(style);
            (style.IsCharacter ? _char : _para)[style.Number] = style;
        }

        internal void Count(string name)
        {
            if (name == null) return;
            Usage.TryGetValue(name, out int n);
            Usage[name] = n + 1;
        }

        public int UsageOf(string name) => name != null && Usage.TryGetValue(name, out int n) ? n : 0;

        /// <summary>The document's own default character formatting (before any style).</summary>
        internal RtfCharFormat Defaults() => new RtfCharFormat
        {
            Bold = false, Italic = false, Underline = false, Strikethrough = false, Superscript = false,
            Subscript = false, Hidden = false, Font = DefaultFont, Size = 12f, Colour = 0, Highlight = 0,
            Language = DefaultLanguage > 0 ? DefaultLanguage : (int?)null,
        };

        /// <summary>Works out every style's full formatting and heading level, following its parents.</summary>
        internal void Resolve()
        {
            foreach (RtfStyle s in Styles)
            {
                var chain = new List<RtfStyle>();
                for (RtfStyle at = s; at != null && !chain.Contains(at) && chain.Count < 20;
                     at = at.BasedOn.HasValue ? (s.IsCharacter ? Character(at.BasedOn.Value) : Paragraph(at.BasedOn.Value)) : null)
                    chain.Add(at);
                chain.Reverse(); // root first

                var para = new RtfParaFormat { Alignment = 0, LeftIndent = 0, FirstLineIndent = 0, SpaceBefore = 0, SpaceAfter = 0, Sl = 0, SlMult = false };
                RtfCharFormat ch = s.IsCharacter ? new RtfCharFormat() : Defaults();
                int heading = 0;
                var words = new StringBuilder();
                foreach (RtfStyle at in chain)
                {
                    words.Append(OwnWords(at.Definition));
                    para.Over(at.Para);
                    ch.Over(at.Char);
                    int level = at.Para.OutlineLevel is int o && o >= 0 && o < 9 ? o + 1 : NamedHeading(at.Name);
                    if (level > 0) heading = level;
                }
                s.ResolvedPara = para;
                s.ResolvedChar = ch;
                s.HeadingLevel = s.IsCharacter ? 0 : heading;
                s.Words = words.ToString();
            }
        }

        /// <summary>Style sheet words that aren't formatting: the style's number, parent, next style, flags.</summary>
        private static readonly HashSet<string> NotFormatting = new HashSet<string>(StringComparer.Ordinal)
        {
            "s", "cs", "ds", "ts", "tsrowd", "sbasedon", "snext", "slink", "sautoupd", "additive", "shidden", "ssemihidden",
            "sunhideused", "sqformat", "spriority", "slocked", "styrsid", "spersonal", "scompose", "sreply", "keycode",
            "ls", "ilvl", "listid",
        };

        /// <summary>The formatting words of a style definition (<c>{\s60\sbasedon74\sb119\cf0 Name;}</c> → <c>\sb119\cf0</c>).</summary>
        internal static string OwnWords(string definition)
        {
            Split(definition, out _, out string words, out _);
            return words;
        }

        /// <summary>
        /// The style's definition with everything it inherits written into it (its <see cref="Words"/>),
        /// its parent still named. A reader such as Writer's compares each paragraph with its style's
        /// own definition only: with the parents' settings in it too, a paragraph that repeats them has
        /// no direct formatting; and a style created from it (in another document) looks the same.
        /// </summary>
        public string FlatDefinition(RtfStyle style)
        {
            Split(style.Definition, out string header, out _, out string rest);
            if (rest.Length == 0) return style.Definition;
            return "{" + header + style.Words + " " + rest;
        }

        /// <summary>
        /// Splits <c>{\*\cs15\snext15\i French;}</c> into the words that aren't formatting (<c>\*\cs15\snext15</c>),
        /// the formatting words (<c>\i</c>) and the rest (<c>French;}</c>). List references are left out.
        /// </summary>
        private static void Split(string definition, out string header, out string words, out string rest)
        {
            header = words = rest = "";
            if (string.IsNullOrEmpty(definition)) return;
            var h = new StringBuilder();
            var sb = new StringBuilder();
            int i = definition[0] == '{' ? 1 : 0;
            while (i < definition.Length)
            {
                char c = definition[i];
                if (c == ' ' || c == '\r' || c == '\n') { i++; continue; }
                if (c == '{')
                {
                    // a group inside the definition (e.g. a keycode): left out
                    int depth = 0;
                    for (; i < definition.Length; i++)
                    {
                        if (definition[i] == '\\') { i++; continue; }
                        if (definition[i] == '{') depth++;
                        else if (definition[i] == '}' && --depth == 0) { i++; break; }
                    }
                    continue;
                }
                if (c != '\\' || i + 1 >= definition.Length) break; // the name
                char n = definition[i + 1];
                if (!char.IsLetter(n))
                {
                    if (n == '*') { h.Append("\\*"); i += 2; continue; }
                    break; // a control symbol or escaped character: part of the name
                }
                int start = i++;
                while (i < definition.Length && char.IsLetter(definition[i])) i++;
                string word = definition.Substring(start + 1, i - start - 1);
                if (i < definition.Length && (definition[i] == '-' || char.IsDigit(definition[i])))
                {
                    i++;
                    while (i < definition.Length && char.IsDigit(definition[i])) i++;
                }
                string token = definition.Substring(start, i - start);
                if (i < definition.Length && definition[i] == ' ') i++; // the delimiter
                if (word == "ls" || word == "ilvl" || word == "listid") continue; // the source's lists aren't written
                (NotFormatting.Contains(word) ? h : sb).Append(token);
            }
            header = h.ToString();
            words = sb.ToString();
            rest = i < definition.Length ? definition.Substring(i) : "";
        }

        /// <summary>"heading 3" / "Heading 3" → 3.</summary>
        private static int NamedHeading(string name)
        {
            if (name == null || !name.StartsWith("heading ", StringComparison.OrdinalIgnoreCase)) return 0;
            return int.TryParse(name.Substring(8).Trim(), out int n) && n >= 1 && n <= 9 ? n : 0;
        }
    }

    /// <summary>Splits RTF into groups, control words, control symbols and text.</summary>
    internal sealed class RtfLexer
    {
        public enum Kind { End, Open, Close, Word, Symbol, Text, Hex }

        private readonly string _s;
        private readonly int _end;
        private int _i;

        public Kind Type;
        public string Word;
        public int? Param;
        public char Char;
        /// <summary>Where the current token starts.</summary>
        public int Start;
        public int Position => _i;

        public RtfLexer(string s, int start = 0, int end = -1)
        {
            _s = s ?? "";
            _i = start;
            _end = end < 0 ? _s.Length : Math.Min(end, _s.Length);
        }

        public bool Next()
        {
            while (_i < _end && (_s[_i] == '\r' || _s[_i] == '\n')) _i++;
            Start = _i;
            Word = null; Param = null;
            if (_i >= _end) { Type = Kind.End; return false; }
            char c = _s[_i++];
            if (c == '{') { Type = Kind.Open; return true; }
            if (c == '}') { Type = Kind.Close; return true; }
            if (c != '\\') { Type = Kind.Text; Char = c; return true; }
            if (_i >= _end) { Type = Kind.End; return false; }

            c = _s[_i];
            if (IsLetter(c))
            {
                int w = _i;
                while (_i < _end && IsLetter(_s[_i]) && _i - w < 32) _i++;
                Word = _s.Substring(w, _i - w);
                bool negative = _i < _end && _s[_i] == '-' && _i + 1 < _end && char.IsDigit(_s[_i + 1]);
                if (negative) _i++;
                if (_i < _end && char.IsDigit(_s[_i]))
                {
                    long n = 0;
                    while (_i < _end && char.IsDigit(_s[_i])) { if (n < int.MaxValue) n = n * 10 + (_s[_i] - '0'); _i++; }
                    if (n > int.MaxValue) n = int.MaxValue;
                    Param = (int)(negative ? -n : n);
                }
                if (_i < _end && _s[_i] == ' ') _i++; // the delimiter
                Type = Kind.Word;
                return true;
            }

            _i++;
            switch (c)
            {
                case '\'':
                    int hex = 0, digits = 0;
                    while (digits < 2 && _i < _end && Uri.IsHexDigit(_s[_i])) { hex = hex * 16 + Convert.ToInt32(_s[_i].ToString(), 16); _i++; digits++; }
                    Type = Kind.Hex; Char = (char)hex; return true;
                case '\\': case '{': case '}': Type = Kind.Text; Char = c; return true;
                case '~': Type = Kind.Text; Char = ' '; return true;
                case '_': Type = Kind.Text; Char = '‑'; return true;
                case '\r': case '\n': Type = Kind.Word; Word = "par"; return true;
                default: Type = Kind.Symbol; Char = c; return true; // \* \- \: \|
            }
        }

        private static bool IsLetter(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');

        /// <summary>Windows-1252 bytes 0x80–0x9F (the rest are Latin-1).</summary>
        private static readonly string Cp1252High =
            "€\u0081‚ƒ„…†‡ˆ‰Š‹Œ\u008DŽ\u008F" +
            "\u0090‘’“”•–—˜™š›œ\u009DžŸ";

        public static char FromAnsi(int b) => b >= 0x80 && b <= 0x9F ? Cp1252High[b - 0x80] : (char)b;

        /// <summary>The span of the first group that starts with <paramref name="word"/> (after any \*), braces included; or null.</summary>
        public static (int Start, int End)? FindGroup(string rtf, string word)
        {
            var lex = new RtfLexer(rtf);
            var opens = new Stack<int>();
            bool groupStart = false;
            while (lex.Next())
            {
                switch (lex.Type)
                {
                    case Kind.Open: opens.Push(lex.Start); groupStart = true; break;
                    case Kind.Close: if (opens.Count > 0) opens.Pop(); groupStart = false; break;
                    case Kind.Symbol: if (lex.Char != '*') groupStart = false; break;
                    case Kind.Word:
                        if (groupStart && lex.Word == word && opens.Count > 0)
                        {
                            int start = opens.Peek(), depth = 1;
                            while (depth > 0 && lex.Next())
                            {
                                if (lex.Type == Kind.Open) depth++;
                                else if (lex.Type == Kind.Close) depth--;
                            }
                            return (start, lex.Position);
                        }
                        groupStart = false;
                        break;
                    default: groupStart = false; break;
                }
            }
            return null;
        }

        /// <summary>Text with its \'hh and \u escapes decoded (for names in tables).</summary>
        public static void AppendText(RtfLexer lex, StringBuilder sb, ref int skip)
        {
            if (lex.Type == Kind.Word && lex.Word == "u" && lex.Param.HasValue)
            {
                int u = lex.Param.Value;
                sb.Append((char)(u < 0 ? u + 65536 : u));
                skip = 1;
                return;
            }
            if (lex.Type != Kind.Text && lex.Type != Kind.Hex) return;
            if (skip > 0) { skip--; return; }
            sb.Append(lex.Type == Kind.Hex ? FromAnsi(lex.Char) : lex.Char);
        }
    }
}
