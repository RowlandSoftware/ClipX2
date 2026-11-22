using System;
using System.Collections.Generic;
using System.Text;
using ClipX2.TextEngine.Export;

namespace ClipX2.TextEngine.Import
{
    /// <summary>
    /// Reads a word processor's RTF (LibreOffice, Word) into a <see cref="FormattedDocument"/>,
    /// keeping each paragraph's style name and each run's character style and language.
    /// <para>
    /// Unlike the editor's own RTF reader, it follows the style sheet: LibreOffice writes only what
    /// each style sets itself, leaving its parents' settings (a heading's font from "Heading") to the
    /// reader. Numbers a paragraph style generates itself (an empty numbered "Separator") are left
    /// out rather than becoming typed text; real lists become live lists. Comments, bookmarks,
    /// fields' codes, pictures, headers and footers are left out.
    /// </para>
    /// </summary>
    public static class RtfReader
    {
        /// <summary>True if the RTF has a style sheet with named styles worth keeping (a word processor's).</summary>
        public static bool HasNamedStyles(string rtf)
        {
            if (string.IsNullOrEmpty(rtf)) return false;
            var span = RtfLexer.FindGroup(rtf, "stylesheet");
            if (span == null) return false;
            RtfStyleSheet sheet = new RtfStyleSheet();
            ReadStyleSheet(rtf, span.Value, sheet);
            int named = 0;
            foreach (RtfStyle s in sheet.Styles) if (s.Number != 0 || s.IsCharacter) named++;
            return named > 0;
        }

        public static FormattedDocument Read(string rtf)
        {
            var sheet = new RtfStyleSheet();
            var lists = new Dictionary<int, ListDefinition>(); // by \ls number
            ReadHeader(rtf, sheet);
            var fonts = RtfLexer.FindGroup(rtf, "fonttbl");
            if (fonts != null) ReadFontTable(rtf, fonts.Value, sheet);
            var colours = RtfLexer.FindGroup(rtf, "colortbl");
            if (colours != null) ReadColourTable(rtf, colours.Value, sheet);
            var styles = RtfLexer.FindGroup(rtf, "stylesheet");
            if (styles != null) ReadStyleSheet(rtf, styles.Value, sheet);
            if (sheet.Paragraph(0) == null)
            {
                var normal = new RtfStyle { Number = 0, Name = "Normal", Definition = "{\\s0 Normal;}" };
                sheet.Add(normal);
            }
            sheet.Resolve();
            ReadLists(rtf, lists);

            var doc = new Body(rtf, sheet, lists).Read();
            doc.Styles = sheet;
            return doc;
        }

        /**************** Tables ****************/

        private static void ReadHeader(string rtf, RtfStyleSheet sheet)
        {
            var lex = new RtfLexer(rtf);
            int depth = 0;
            while (lex.Next())
            {
                if (lex.Type == RtfLexer.Kind.Open) { if (++depth > 1) break; }
                else if (lex.Type == RtfLexer.Kind.Close) depth--;
                else if (lex.Type == RtfLexer.Kind.Word && depth == 1)
                {
                    if (lex.Word == "deff" && lex.Param.HasValue) sheet.DefaultFont = lex.Param;
                    else if (lex.Word == "deflang" && lex.Param > 0) sheet.DefaultLanguage = lex.Param.Value;
                }
                else if (lex.Type == RtfLexer.Kind.Text) break;
            }
        }

        private static void ReadFontTable(string rtf, (int Start, int End) span, RtfStyleSheet sheet)
        {
            sheet.FontTableRtf = rtf.Substring(span.Start, span.End - span.Start);
            var lex = new RtfLexer(rtf, span.Start, span.End);
            int depth = 0, skipBelow = int.MaxValue, current = -1, uskip = 0;
            bool groupStart = false;
            var name = new StringBuilder();
            while (lex.Next())
            {
                switch (lex.Type)
                {
                    case RtfLexer.Kind.Open: depth++; groupStart = true; continue;
                    case RtfLexer.Kind.Close: depth--; if (depth < skipBelow) skipBelow = int.MaxValue; groupStart = false; continue;
                    case RtfLexer.Kind.Symbol:
                        if (lex.Char == '*' && groupStart && depth < skipBelow) skipBelow = depth;
                        continue;
                    case RtfLexer.Kind.Word:
                        if (groupStart && (lex.Word == "falt" || lex.Word == "panose") && depth < skipBelow) skipBelow = depth;
                        groupStart = false;
                        if (depth >= skipBelow) continue;
                        if (lex.Word == "f" && lex.Param.HasValue) { current = lex.Param.Value; name.Clear(); }
                        else RtfLexer.AppendText(lex, name, ref uskip);
                        continue;
                }
                groupStart = false;
                if (depth >= skipBelow) continue;
                if (lex.Type == RtfLexer.Kind.Text && lex.Char == ';')
                {
                    if (current >= 0) sheet.Fonts[current] = name.ToString().Trim();
                    name.Clear();
                    continue;
                }
                RtfLexer.AppendText(lex, name, ref uskip);
            }
        }

        private static void ReadColourTable(string rtf, (int Start, int End) span, RtfStyleSheet sheet)
        {
            sheet.ColourTableRtf = rtf.Substring(span.Start, span.End - span.Start);
            var lex = new RtfLexer(rtf, span.Start, span.End);
            int r = -1, g = -1, b = -1;
            while (lex.Next())
            {
                if (lex.Type == RtfLexer.Kind.Word)
                {
                    if (lex.Word == "red") r = lex.Param ?? 0;
                    else if (lex.Word == "green") g = lex.Param ?? 0;
                    else if (lex.Word == "blue") b = lex.Param ?? 0;
                }
                else if (lex.Type == RtfLexer.Kind.Text && lex.Char == ';')
                {
                    sheet.Colours.Add(r < 0 && g < 0 && b < 0 ? (int?)null : (Math.Max(0, r) << 16) | (Math.Max(0, g) << 8) | Math.Max(0, b));
                    r = g = b = -1;
                }
            }
        }

        private static void ReadStyleSheet(string rtf, (int Start, int End) span, RtfStyleSheet sheet)
        {
            var lex = new RtfLexer(rtf, span.Start, span.End);
            int depth = 0, entryStart = -1, uskip = 0;
            RtfStyle style = null;
            bool skip = false;
            var name = new StringBuilder();
            while (lex.Next())
            {
                switch (lex.Type)
                {
                    case RtfLexer.Kind.Open:
                        depth++;
                        if (depth == 2) { entryStart = lex.Start; style = new RtfStyle(); skip = false; name.Clear(); }
                        break;
                    case RtfLexer.Kind.Close:
                        if (depth == 2 && style != null && !skip)
                        {
                            style.Name = name.ToString().Trim().TrimEnd(';').Trim();
                            style.Definition = rtf.Substring(entryStart, lex.Position - entryStart);
                            if (style.Name.Length > 0 && (style.IsCharacter ? sheet.Character(style.Number) : sheet.Paragraph(style.Number)) == null)
                                sheet.Add(style);
                            style = null;
                        }
                        depth--;
                        break;
                    case RtfLexer.Kind.Word:
                        if (depth != 2 || style == null) break;
                        switch (lex.Word)
                        {
                            case "s": style.Number = lex.Param ?? 0; break;
                            case "cs": style.Number = lex.Param ?? 0; style.IsCharacter = true; break;
                            case "ds": case "ts": case "tsrowd": skip = true; break;
                            case "sbasedon": if (lex.Param.HasValue && lex.Param != 222) style.BasedOn = lex.Param; break;
                            case "u": RtfLexer.AppendText(lex, name, ref uskip); break;
                            default: if (!style.Char.Apply(lex.Word, lex.Param)) style.Para.Apply(lex.Word, lex.Param); break;
                        }
                        break;
                    case RtfLexer.Kind.Text:
                    case RtfLexer.Kind.Hex:
                        if (depth == 2 && style != null) RtfLexer.AppendText(lex, name, ref uskip);
                        break;
                }
            }
        }

        /// <summary>One list level: its numbering (RTF \levelnfc), start and label text.</summary>
        private sealed class ListLevel
        {
            public int Nfc;
            public int Start = 1;
            public string Text = "";
        }

        private sealed class ListDefinition
        {
            public readonly List<ListLevel> Levels = new List<ListLevel>();
        }

        private static void ReadLists(string rtf, Dictionary<int, ListDefinition> lists)
        {
            var byId = new Dictionary<int, ListDefinition>();
            var table = RtfLexer.FindGroup(rtf, "listtable");
            if (table != null)
            {
                var lex = new RtfLexer(rtf, table.Value.Start, table.Value.End);
                ListDefinition list = null;
                ListLevel level = null;
                int depth = 0, textDepth = -1, uskip = 0;
                var text = new StringBuilder();
                while (lex.Next())
                {
                    switch (lex.Type)
                    {
                        case RtfLexer.Kind.Open: depth++; break;
                        case RtfLexer.Kind.Close:
                            if (depth == textDepth) { if (level != null) level.Text = text.ToString(); textDepth = -1; }
                            depth--;
                            break;
                        case RtfLexer.Kind.Word:
                            switch (lex.Word)
                            {
                                case "list": list = new ListDefinition(); level = null; break;
                                case "listlevel": if (list != null) { level = new ListLevel(); list.Levels.Add(level); } break;
                                case "levelnfc": case "levelnfcn": if (level != null) level.Nfc = lex.Param ?? 0; break;
                                case "levelstartat": if (level != null) level.Start = lex.Param ?? 1; break;
                                case "leveltext": textDepth = depth; text.Clear(); uskip = 0; break;
                                case "listid": if (list != null && lex.Param.HasValue && depth == 2) byId[lex.Param.Value] = list; break;
                                default: if (depth == textDepth) RtfLexer.AppendText(lex, text, ref uskip); break;
                            }
                            break;
                        default:
                            if (depth == textDepth) RtfLexer.AppendText(lex, text, ref uskip);
                            break;
                    }
                }
            }
            var overrides = RtfLexer.FindGroup(rtf, "listoverridetable");
            if (overrides == null) return;
            var o = new RtfLexer(rtf, overrides.Value.Start, overrides.Value.End);
            int? id = null;
            while (o.Next())
            {
                if (o.Type != RtfLexer.Kind.Word) continue;
                if (o.Word == "listoverride") id = null;
                else if (o.Word == "listid") id = o.Param;
                else if (o.Word == "ls" && o.Param.HasValue && id.HasValue && byId.TryGetValue(id.Value, out ListDefinition def))
                    lists[o.Param.Value] = def;
            }
        }

        /**************** The text ****************/

        private sealed class GroupState
        {
            public int ParaStyle;
            public RtfParaFormat Para = new RtfParaFormat();
            public int? List;
            public int ListLevel;
            public int? CharStyle;
            public RtfCharFormat Char = new RtfCharFormat();
            public int Uc = 1;
            public bool Skip;
            public bool GroupStart;
            public bool Star;

            public GroupState Clone()
            {
                var c = (GroupState)MemberwiseClone();
                c.Para = Para.Clone();
                c.Char = Char.Clone();
                c.GroupStart = true;
                c.Star = false;
                return c;
            }
        }

        /// <summary>Destinations whose contents aren't the document's text.</summary>
        private static readonly HashSet<string> Skipped = new HashSet<string>(StringComparer.Ordinal)
        {
            "fonttbl", "colortbl", "stylesheet", "listtable", "listoverridetable", "info", "pict", "object",
            "header", "headerl", "headerr", "headerf", "footer", "footerl", "footerr", "footerf", "footnote",
            "annotation", "atnid", "atnauthor", "atndate", "atnref", "atrfstart", "atrfend", "listtext", "pntext",
            "pntxta", "pntxtb", "pn", "xe", "tc", "bkmkstart", "bkmkend", "fldinst", "themedata",
            "colorschememapping", "latentstyles", "datastore", "rsidtbl", "generator", "pgdsctbl", "mmathPr",
            "xmlnstbl", "revtbl", "userprops", "ftnsep", "ftnsepc", "ftncn", "aftnsep", "aftnsepc", "aftncn",
            "shp", "shpinst", "nonshppict", "template", "docvar", "formfield", "datafield", "wgrffmtfilter",
            "filetbl", "pgptbl", "protusertbl", "fchars", "lchars", "operator", "company", "title", "author",
        };

        private sealed class Body
        {
            private readonly string _rtf;
            private readonly RtfStyleSheet _sheet;
            private readonly Dictionary<int, ListDefinition> _lists;
            private readonly FormattedDocument _doc = new FormattedDocument();
            private readonly Stack<GroupState> _stack = new Stack<GroupState>();
            private GroupState _state = new GroupState();

            private readonly List<(StringBuilder Text, RtfCharFormat Format, string Style)> _runs = new List<(StringBuilder, RtfCharFormat, string)>();
            private int _uskip;
            private int _version, _cachedVersion = -1;
            private RtfCharFormat _cached;
            private string _cachedStyle;
            private readonly Dictionary<int, RtfCharFormat> _paraChar = new Dictionary<int, RtfCharFormat>();

            public Body(string rtf, RtfStyleSheet sheet, Dictionary<int, ListDefinition> lists)
            {
                _rtf = rtf; _sheet = sheet; _lists = lists;
            }

            public FormattedDocument Read()
            {
                var lex = new RtfLexer(_rtf);
                int depth = 0;
                while (lex.Next())
                {
                    switch (lex.Type)
                    {
                        case RtfLexer.Kind.Open:
                            _stack.Push(_state);
                            _state = _state.Clone();
                            depth++; _version++; _uskip = 0;
                            break;
                        case RtfLexer.Kind.Close:
                            if (depth <= 1) { Finish(final: true); return _doc; } // the last paragraph, in the document's own state
                            if (_stack.Count > 0) _state = _stack.Pop();
                            depth--; _version++; _uskip = 0;
                            break;
                        case RtfLexer.Kind.Symbol:
                            if (lex.Char == '*' && _state.GroupStart) _state.Star = true;
                            break;
                        case RtfLexer.Kind.Word:
                            Word(lex.Word, lex.Param);
                            break;
                        case RtfLexer.Kind.Text:
                        case RtfLexer.Kind.Hex:
                            _state.GroupStart = false;
                            if (_state.Skip) break;
                            if (_uskip > 0) { _uskip--; break; }
                            Append(lex.Type == RtfLexer.Kind.Hex ? RtfLexer.FromAnsi(lex.Char) : lex.Char);
                            break;
                    }
                }
                Finish(final: true);
                return _doc;
            }

            private void Word(string word, int? param)
            {
                GroupState s = _state;
                if (s.GroupStart)
                {
                    s.GroupStart = false;
                    if (s.Star || Skipped.Contains(word)) { s.Skip = true; return; }
                }
                if (s.Skip) return;
                _version++;
                switch (word)
                {
                    case "par": Finish(final: false); return;
                    case "row": Finish(final: false); return;
                    case "pard": s.ParaStyle = 0; s.Para = new RtfParaFormat(); s.List = null; s.ListLevel = 0; return;
                    case "plain": s.Char = new RtfCharFormat(); s.CharStyle = null; return;
                    case "s": s.ParaStyle = param ?? 0; return;
                    case "cs": s.CharStyle = param; return;
                    case "ls": s.List = param; return;
                    case "ilvl": s.ListLevel = Math.Max(0, Math.Min(8, param ?? 0)); return;
                    case "uc": s.Uc = Math.Max(0, param ?? 1); return;
                    case "u":
                        if (param.HasValue) { int u = param.Value; Append((char)(u < 0 ? u + 65536 : u)); _uskip = s.Uc; }
                        return;
                    case "tab": case "cell": Append('\t'); return;
                    case "line": Append('\v'); return;
                    case "emdash": Append('—'); return;
                    case "endash": Append('–'); return;
                    case "emspace": Append(' '); return;
                    case "enspace": Append(' '); return;
                    case "qmspace": Append(' '); return;
                    case "lquote": Append('‘'); return;
                    case "rquote": Append('’'); return;
                    case "ldblquote": Append('“'); return;
                    case "rdblquote": Append('”'); return;
                    case "bullet": Append('•'); return;
                }
                if (!s.Char.Apply(word, param)) s.Para.Apply(word, param);
            }

            /// <summary>A paragraph style's character formatting, over the document's defaults.</summary>
            private RtfCharFormat ParagraphChar(int style)
            {
                if (_paraChar.TryGetValue(style, out RtfCharFormat f)) return f;
                RtfStyle s = _sheet.Paragraph(style) ?? _sheet.Paragraph(0);
                f = s?.ResolvedChar ?? _sheet.Defaults();
                _paraChar[style] = f;
                return f;
            }

            private void Append(char c)
            {
                if (_state.Skip) return;
                if (_cachedVersion != _version)
                {
                    RtfCharFormat f = ParagraphChar(_state.ParaStyle).Clone();
                    RtfStyle cs = _state.CharStyle.HasValue ? _sheet.Character(_state.CharStyle.Value) : null;
                    if (cs != null) f.Over(cs.ResolvedChar);
                    f.Over(_state.Char);
                    if (_cached == null || !_cached.SameAs(f) || _cachedStyle != cs?.Name) { _cached = f; _cachedStyle = cs?.Name; }
                    _cachedVersion = _version;
                }
                if (_cached.Hidden == true) return;
                if (_runs.Count > 0 && ReferenceEquals(_runs[_runs.Count - 1].Format, _cached) && _runs[_runs.Count - 1].Style == _cachedStyle)
                    _runs[_runs.Count - 1].Text.Append(c);
                else
                    _runs.Add((new StringBuilder().Append(c), _cached, _cachedStyle));
            }

            private bool _sawParagraph;

            private void Finish(bool final)
            {
                if (final && _runs.Count == 0 && _sawParagraph) return; // no text after the last \par
                _sawParagraph = true;

                GroupState s = _state;
                RtfStyle style = _sheet.Paragraph(s.ParaStyle) ?? _sheet.Paragraph(0);
                RtfParaFormat para = (style?.ResolvedPara ?? new RtfParaFormat()).Clone().Over(s.Para);

                var p = new FormattedParagraph
                {
                    StyleName = style?.Name,
                    Alignment = (ParagraphAlignment)Math.Max(0, Math.Min(3, para.Alignment ?? 0)),
                    LeftIndent = para.LeftIndent ?? 0,
                    FirstLineIndent = para.FirstLineIndent ?? 0,
                    SpaceBefore = para.SpaceBefore ?? 0,
                    SpaceAfter = para.SpaceAfter ?? 0,
                    LineSpacing = para.Spacing,
                    HeadingLevel = s.Para.OutlineLevel is int o && o >= 0 && o < 9 ? o + 1 : style?.HeadingLevel ?? 0,
                };

                bool empty = true;
                foreach (var run in _runs)
                {
                    string text = run.Text.ToString();
                    if (text.Trim().Length > 0) empty = false;
                    RtfCharFormat f = run.Format;
                    p.Runs.Add(new FormattedRun
                    {
                        Text = text,
                        Bold = f.Bold == true, Italic = f.Italic == true, Underline = f.Underline == true,
                        Strikethrough = f.Strikethrough == true, Superscript = f.Superscript == true,
                        Subscript = f.Subscript == true && f.Superscript != true,
                        FontName = _sheet.FontName(f.Font), FontSize = f.Size ?? 0,
                        Colour = _sheet.Rgb(f.Colour), Highlight = _sheet.Rgb(f.Highlight),
                        Language = f.Language ?? 0, CharacterStyle = run.Style,
                    });
                    _sheet.Count(run.Style);
                }
                _runs.Clear();
                _version++;

                // Lists. Numbering a style generates on a paragraph with no text (a Separator's I, II,
                // III) is shown as a number, the one it had, and noted with the style sheet: Done leaves
                // it to the style again, so the word processor goes on generating it
                if (s.List.HasValue && _lists.TryGetValue(s.List.Value, out ListDefinition list) && list.Levels.Count > 0)
                {
                    ListLevel level = list.Levels[Math.Min(s.ListLevel, list.Levels.Count - 1)];
                    ListKind kind = KindOf(level.Nfc);
                    int number = Count(s.List.Value, s.ListLevel, level.Start);
                    if (empty && kind != ListKind.None) _sheet.RecordStyleNumbered(p.StyleName);
                    if (kind != ListKind.None)
                    {
                        p.ListKind = kind;
                        p.ListLevel = empty ? 0 : s.ListLevel;
                        p.ListStart = empty ? number : Math.Max(1, level.Start);
                        p.ListNumberStyle = NumberStyleOf(level.Text, s.ListLevel);
                        if (kind == ListKind.Bullet) p.BulletCharacter = BulletOf(level.Text);
                        p.LeftIndent = 0; p.FirstLineIndent = 0; // the editor's list indents
                    }
                }

                _sheet.Count(p.StyleName);
                _doc.Paragraphs.Add(p);
            }

            private readonly Dictionary<(int List, int Level), int> _numbers = new Dictionary<(int, int), int>();

            /// <summary>The number a list paragraph has: one more than the last at its level (deeper levels start again).</summary>
            private int Count(int list, int level, int start)
            {
                int n = _numbers.TryGetValue((list, level), out int last) ? last + 1 : Math.Max(0, start);
                _numbers[(list, level)] = n;
                for (int deeper = level + 1; deeper < 9; deeper++) _numbers.Remove((list, deeper));
                return Math.Max(1, n);
            }

            private static ListKind KindOf(int nfc)
            {
                switch (nfc)
                {
                    case 0: case 22: return ListKind.Arabic;
                    case 1: return ListKind.UpperRoman;
                    case 2: return ListKind.LowerRoman;
                    case 3: return ListKind.UpperLetter;
                    case 4: return ListKind.LowerLetter;
                    case 23: return ListKind.Bullet;
                    default: return ListKind.None; // 255 = no number, and schemes the editor can't show
                }
            }

            /// <summary>From a level's text (its length, then the label with level placeholders \'00…\'08): 1. 1) (1) or 1.</summary>
            private static ListNumberStyle NumberStyleOf(string text, int level)
            {
                if (string.IsNullOrEmpty(text)) return ListNumberStyle.Period;
                string label = text.Substring(1);
                int at = label.LastIndexOf((char)level);
                if (at < 0) return ListNumberStyle.Plain;
                char after = at + 1 < label.Length ? label[at + 1] : '\0';
                char before = at > 0 ? label[at - 1] : '\0';
                if (before == '(') return ListNumberStyle.Parentheses;
                if (after == ')') return ListNumberStyle.RightParenthesis;
                if (after == '.') return ListNumberStyle.Period;
                return ListNumberStyle.Plain;
            }

            /// <summary>A bullet level's character: null for •. Word's Symbol and Wingdings bullets mapped.</summary>
            private static string BulletOf(string text)
            {
                char c = text != null && text.Length > 1 ? text[1] : '•';
                switch (c)
                {
                    case '•': case '·': case '': return null;
                    case 'o': case '': case '◦': return "◦";
                    case '§': case '': case '▪': case '■': return "▪";
                    default: return c < ' ' ? null : c.ToString();
                }
            }
        }
    }
}
