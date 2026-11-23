using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ClipX2.TextEngine.Import;

namespace ClipX2.TextEngine.Export
{
    /// <summary>
    /// RTF that names its styles: the source's style sheet (and font and colour tables) as they
    /// came, each paragraph and run naming its style (<see cref="StyleMatcher"/> chooses them), and
    /// direct formatting only where the text differs from its style. Pasted into the word processor
    /// the text came from, its own styles apply (they survive "clear direct formatting").
    /// </summary>
    public static class RtfExport
    {
        /// <param name="forWriter">
        /// The version for LibreOffice Writer: styles named as Writer's RTF names them
        /// (<see cref="WriterStyleNames"/>), and no paragraph mark after the last paragraph, which
        /// Writer would paste as an extra empty paragraph.
        /// </param>
        public static string ToRtf(FormattedDocument doc, RtfStyleSheet sheet, bool forWriter = false)
        {
            var fonts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var f in sheet.Fonts) if (!fonts.ContainsKey(f.Value)) fonts[f.Value] = f.Key;
            var newFonts = new List<(int Index, string Name)>();
            int nextFont = sheet.Fonts.Count == 0 ? 0 : sheet.Fonts.Keys.Max() + 1;

            int baseColours = sheet.ColourTableRtf != null ? sheet.Colours.Count : 1;
            var newColours = new List<int>();

            int FontIndex(string name)
            {
                if (fonts.TryGetValue(name, out int i)) return i;
                i = nextFont++;
                fonts[name] = i;
                newFonts.Add((i, name));
                return i;
            }

            int ColourIndex(int? rgb)
            {
                if (rgb == null) return 0;
                for (int i = 1; i < sheet.Colours.Count && sheet.ColourTableRtf != null; i++)
                    if (sheet.Colours[i] == rgb) return i;
                int at = newColours.IndexOf(rgb.Value);
                if (at < 0) { newColours.Add(rgb.Value); at = newColours.Count - 1; }
                return baseColours + at;
            }

            var lists = ListBlocks(doc, sheet);
            var body = new StringBuilder();
            RtfStyle normal = sheet.Paragraph(0);
            for (int pi = 0; pi < doc.Paragraphs.Count; pi++)
            {
                FormattedParagraph p = doc.Paragraphs[pi];
                RtfStyle style = sheet.ParagraphNamed(p.StyleName) ?? normal;
                RtfParaFormat sp = style?.ResolvedPara ?? new RtfParaFormat();
                // The style's own settings repeated after it: what isn't, a reader takes as direct formatting
                body.Append("\\pard\\plain \\s").Append(style?.Number ?? 0).Append(style?.Words);

                if ((int)p.Alignment != (sp.Alignment ?? 0))
                    body.Append(p.Alignment == ParagraphAlignment.Centre ? "\\qc" : p.Alignment == ParagraphAlignment.Right ? "\\qr"
                        : p.Alignment == ParagraphAlignment.Justify ? "\\qj" : "\\ql");
                if (lists.TryGetValue(pi, out int list))
                {
                    body.Append("\\ls").Append(list).Append("\\ilvl").Append(Math.Max(0, Math.Min(8, p.ListLevel)));
                    body.Append("\\fi").Append(Twips(p.FirstLineIndent)).Append("\\li").Append(Twips(p.LeftIndent)).Append("\\lin").Append(Twips(p.LeftIndent));
                }
                else if (IsStyleNumbered(p, sheet) && p.ListKind != ListKind.None)
                {
                    // A Separator's I, II, III (a number in the editor): no list and the style's indents,
                    // so the word processor's own style numbers it, carrying on the document's numbering
                }
                else
                {
                    if (!StyleMatcher.Near(p.FirstLineIndent, sp.FirstLineIndent ?? 0)) body.Append("\\fi").Append(Twips(p.FirstLineIndent));
                    if (!StyleMatcher.Near(p.LeftIndent, sp.LeftIndent ?? 0)) body.Append("\\li").Append(Twips(p.LeftIndent)).Append("\\lin").Append(Twips(p.LeftIndent));
                }
                if (!StyleMatcher.Near(p.SpaceBefore, sp.SpaceBefore ?? 0)) body.Append("\\sb").Append(Twips(p.SpaceBefore));
                if (!StyleMatcher.Near(p.SpaceAfter, sp.SpaceAfter ?? 0)) body.Append("\\sa").Append(Twips(p.SpaceAfter));
                if (!p.LineSpacing.SameAs(sp.Spacing)) body.Append(p.LineSpacing.ToRtf());
                if (style != null && p.HeadingLevel != style.HeadingLevel)
                    body.Append("\\outlinelevel").Append(p.HeadingLevel > 0 ? p.HeadingLevel - 1 : 9);
                body.Append(' ');

                RtfCharFormat paragraphChar = style?.ResolvedChar ?? sheet.Defaults();
                foreach (FormattedRun r in p.Runs)
                {
                    if (r.Text.Length == 0) continue;
                    RtfStyle cs = sheet.CharacterNamed(r.CharacterStyle);
                    RtfCharFormat under = paragraphChar.Clone();
                    if (cs != null) under.Over(cs.ResolvedChar);

                    body.Append('{');
                    if (cs != null) body.Append("\\cs").Append(cs.Number).Append(cs.Words);
                    Flag(body, r.Bold, under.Bold, "\\b", "\\b0");
                    Flag(body, r.Italic, under.Italic, "\\i", "\\i0");
                    Flag(body, r.Underline, under.Underline, "\\ul", "\\ulnone");
                    Flag(body, r.Strikethrough, under.Strikethrough, "\\strike", "\\strike0");
                    if (r.Superscript != (under.Superscript ?? false) || r.Subscript != (under.Subscript ?? false))
                        body.Append(r.Superscript ? "\\super" : r.Subscript ? "\\sub" : "\\nosupersub");
                    if (!string.IsNullOrEmpty(r.FontName) && !StyleMatcher.SameFont(r.FontName, sheet.FontName(under.Font)))
                        body.Append("\\f").Append(FontIndex(r.FontName));
                    if (r.FontSize > 0 && Math.Abs(r.FontSize - (under.Size ?? 12f)) > 0.05f)
                        body.Append("\\fs").Append((int)Math.Round(r.FontSize * 2));
                    if (!StyleMatcher.SameColour(r.Colour, sheet.Rgb(under.Colour)))
                        body.Append("\\cf").Append(ColourIndex(r.Colour));
                    if (r.Highlight != sheet.Rgb(under.Highlight))
                        body.Append("\\highlight").Append(ColourIndex(r.Highlight));
                    if (r.Language > 0 && r.Language != under.Language)
                        body.Append("\\lang").Append(r.Language);
                    if (body[body.Length - 1] != '{') body.Append(' '); // ends the last control word
                    AppendText(body, r.Text);
                    body.Append('}');
                }
                if (pi < doc.Paragraphs.Count - 1 || (sheet.EndsWithParagraphMark && !forWriter)) body.Append("\\par").Append("\r\n");
            }

            var rtf = new StringBuilder("{\\rtf1\\ansi\\ansicpg1252");
            if (sheet.DefaultFont.HasValue) rtf.Append("\\deff").Append(sheet.DefaultFont.Value);
            if (sheet.DefaultLanguage > 0) rtf.Append("\\deflang").Append(sheet.DefaultLanguage);
            rtf.Append("\r\n");

            string extraFonts = string.Concat(newFonts.Select(f => "{\\f" + f.Index + "\\fnil " + Escape(f.Name) + ";}"));
            rtf.Append(sheet.FontTableRtf != null ? InsertBeforeClose(sheet.FontTableRtf, extraFonts) : "{\\fonttbl" + extraFonts + "}").Append("\r\n");

            string extraColours = string.Concat(newColours.Select(c => "\\red" + ((c >> 16) & 0xFF) + "\\green" + ((c >> 8) & 0xFF) + "\\blue" + (c & 0xFF) + ";"));
            rtf.Append(sheet.ColourTableRtf != null ? InsertBeforeClose(sheet.ColourTableRtf, extraColours) : "{\\colortbl;" + extraColours + "}").Append("\r\n");

            rtf.Append("{\\stylesheet");
            foreach (RtfStyle s in ParentsFirst(sheet))
                rtf.Append("\r\n").Append(forWriter ? WriterNamed(sheet.FlatDefinition(s), s, sheet) : sheet.FlatDefinition(s));
            rtf.Append("}\r\n");

            AppendListTable(rtf, doc, lists);
            rtf.Append(body).Append("}");
            return rtf.ToString();
        }

        /// <summary>
        /// A style definition with the style's name as Writer's RTF gives it ("Text body" → "Body Text"),
        /// unless the source also has a style of that name.
        /// </summary>
        private static string WriterNamed(string definition, RtfStyle style, RtfStyleSheet sheet)
        {
            string rtfName = WriterStyleNames.RtfName(style.Name, style.IsCharacter);
            string ending = style.Name + ";}";
            if (rtfName == null || !definition.EndsWith(ending, StringComparison.Ordinal)) return definition;
            if (sheet.Styles.Any(s => s != style && s.IsCharacter == style.IsCharacter && string.Equals(s.Name, rtfName, StringComparison.OrdinalIgnoreCase)))
                return definition;
            return definition.Substring(0, definition.Length - ending.Length) + rtfName + ";}";
        }

        /// <summary>The styles with each parent before its children (a reader may not look ahead for a parent).</summary>
        private static List<RtfStyle> ParentsFirst(RtfStyleSheet sheet)
        {
            var result = new List<RtfStyle>();
            var placed = new HashSet<RtfStyle>();
            void Place(RtfStyle s, int depth)
            {
                if (s == null || placed.Contains(s) || depth > 20) return;
                if (s.BasedOn.HasValue) Place(s.IsCharacter ? sheet.Character(s.BasedOn.Value) : sheet.Paragraph(s.BasedOn.Value), depth + 1);
                if (placed.Add(s)) result.Add(s);
            }
            foreach (RtfStyle s in sheet.Styles) Place(s, 0);
            return result;
        }

        private static void Flag(StringBuilder sb, bool value, bool? under, string on, string off)
        {
            if (value != (under ?? false)) sb.Append(value ? on : off);
        }

        private static int Twips(float points) => (int)Math.Round(points * 20);

        private static string InsertBeforeClose(string group, string extra)
        {
            if (extra.Length == 0) return group;
            int close = group.LastIndexOf('}');
            return close < 0 ? group + extra : group.Substring(0, close) + extra + group.Substring(close);
        }

        private static string Escape(string text)
        {
            var sb = new StringBuilder();
            AppendText(sb, text);
            return sb.ToString();
        }

        private static void AppendText(StringBuilder sb, string text)
        {
            foreach (char c in text)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '{': sb.Append("\\{"); break;
                    case '}': sb.Append("\\}"); break;
                    case '\t': sb.Append("\\tab "); break;
                    case '\v': sb.Append("\\line "); break;
                    case '\u00A0': sb.Append("\\~"); break;
                    case '\r': case '\n': break;
                    default:
                        if (c < 0x80) sb.Append(c);
                        else sb.Append("\\u").Append(((short)c).ToString(CultureInfo.InvariantCulture)).Append('?');
                        break;
                }
            }
        }

        /**************** Lists ****************/

        /// <summary>Each list paragraph's list number (\ls): consecutive list paragraphs are one list.</summary>
        private static Dictionary<int, int> ListBlocks(FormattedDocument doc, RtfStyleSheet sheet)
        {
            var result = new Dictionary<int, int>();
            int list = 0;
            bool inList = false;
            for (int i = 0; i < doc.Paragraphs.Count; i++)
            {
                bool item = doc.Paragraphs[i].ListKind != ListKind.None && !IsStyleNumbered(doc.Paragraphs[i], sheet);
                if (item && !inList) list++;
                inList = item;
                if (item) result[i] = list;
            }
            return result;
        }

        /// <summary>
        /// True for a paragraph whose number its style generated in the source (a Separator's I, II,
        /// III, shown in the editor as a number on a paragraph with no text): not a list made here.
        /// </summary>
        private static bool IsStyleNumbered(FormattedParagraph p, RtfStyleSheet sheet) =>
            sheet.IsStyleNumbered(p.StyleName) && p.Runs.All(r => r.Text.Trim().Length == 0);

        private static void AppendListTable(StringBuilder output, FormattedDocument doc, Dictionary<int, int> lists)
        {
            if (lists.Count == 0) return;
            int count = lists.Values.Max();
            var rtf = new StringBuilder();
            for (int list = 1; list <= count; list++)
            {
                var items = lists.Where(e => e.Value == list).Select(e => doc.Paragraphs[e.Key]).ToList();
                rtf.Append("\r\n{\\list\\listtemplateid").Append(list).Append("\\listhybrid");
                for (int level = 0; level < 9; level++)
                {
                    FormattedParagraph first = items.FirstOrDefault(p => p.ListLevel == level);
                    ListKind kind = first?.ListKind ?? ListKind.Arabic;
                    int nfc = kind == ListKind.Bullet ? 23 : kind == ListKind.UpperRoman ? 1 : kind == ListKind.LowerRoman ? 2
                        : kind == ListKind.UpperLetter ? 3 : kind == ListKind.LowerLetter ? 4 : 0;
                    rtf.Append("{\\listlevel\\levelnfc").Append(nfc).Append("\\levelnfcn").Append(nfc)
                       .Append("\\leveljc0\\leveljcn0\\levelfollow0\\levelstartat").Append(Math.Max(1, first?.ListStart ?? 1));
                    if (kind == ListKind.Bullet)
                    {
                        string bullet = string.IsNullOrEmpty(first?.BulletCharacter) ? "•" : first.BulletCharacter;
                        rtf.Append("{\\leveltext\\'01\\u").Append((short)bullet[0]).Append("?;}{\\levelnumbers;}");
                    }
                    else
                    {
                        string placeholder = "\\'0" + level;
                        ListNumberStyle style = first?.ListNumberStyle ?? ListNumberStyle.Period;
                        string label, numbers;
                        switch (style)
                        {
                            case ListNumberStyle.Parentheses: label = "\\'03(" + placeholder + ")"; numbers = "\\'02"; break;
                            case ListNumberStyle.RightParenthesis: label = "\\'02" + placeholder + ")"; numbers = "\\'01"; break;
                            case ListNumberStyle.Plain: case ListNumberStyle.NoNumber: label = "\\'01" + placeholder; numbers = "\\'01"; break;
                            default: label = "\\'02" + placeholder + "."; numbers = "\\'01"; break;
                        }
                        rtf.Append("{\\leveltext").Append(label).Append(";}{\\levelnumbers").Append(numbers).Append(";}");
                    }
                    rtf.Append("\\fi-360\\li").Append(720 * (level + 1)).Append('}');
                }
                rtf.Append("\\listid").Append(list).Append('}');
            }
            output.Append("{\\*\\listtable").Append(rtf).Append("}\r\n{\\*\\listoverridetable");
            for (int list = 1; list <= count; list++)
                output.Append("{\\listoverride\\listid").Append(list).Append("\\listoverridecount0\\ls").Append(list).Append('}');
            output.Append("}\r\n");
        }
    }
}
