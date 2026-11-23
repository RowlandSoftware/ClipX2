using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ClipX2.TextEngine.Import;

namespace ClipX2.TextEngine.Export
{
    /// <summary>
    /// HTML for text that came from a word processor with named styles. Each paragraph and run names
    /// its style with a class, and a style sheet for the HTML's head defines each class the way Word's
    /// own HTML does (mso-style-name gives the real name). Word's ordinary paste, which takes HTML
    /// before RTF, then applies those styles (creating any the document lacks), as it does with its
    /// own HTML. Inline formatting only says how a paragraph or run differs from its style.
    /// Browsers read the classes' formatting from the same style sheet. (Word's default for pasting
    /// from other programs, Merge Formatting, drops all styles; Keep Source Formatting keeps them,
    /// but only if there's no h1–h6 anywhere in the fragment, so headings are p's of their style.)
    /// </summary>
    public static partial class DocumentExport
    {
        /// <summary>True if the document's paragraphs name styles from <paramref name="sheet"/>.</summary>
        private static bool IsStyled(FormattedDocument doc, RtfStyleSheet sheet) =>
            sheet != null && doc.Paragraphs.Any(p => p.StyleName != null && sheet.ParagraphNamed(p.StyleName) != null);

        /// <summary>
        /// The &lt;style&gt; element for the HTML's head: every paragraph and character style the
        /// document uses, and the styles they're based on. Empty if the text has no named styles.
        /// </summary>
        public static string HtmlStyleSheet(FormattedDocument doc, RtfStyleSheet sheet)
        {
            if (doc == null || !IsStyled(doc, sheet)) return "";
            var paragraphStyles = new List<RtfStyle>();
            var characterStyles = new List<RtfStyle>();
            void Add(RtfStyle s, List<RtfStyle> list, int depth = 0)
            {
                // parents first, so each is defined before a style based on it
                if (s == null || list.Contains(s) || depth > 20) return;
                Add(Parent(s, sheet), list, depth + 1);
                list.Add(s);
            }
            foreach (FormattedParagraph p in doc.Paragraphs)
            {
                Add(sheet.ParagraphNamed(p.StyleName), paragraphStyles);
                foreach (FormattedRun r in p.Runs)
                    if (r.CharacterStyle != null) Add(sheet.CharacterNamed(r.CharacterStyle), characterStyles);
            }

            var sb = new StringBuilder("<style>\r\n");
            foreach (RtfStyle s in paragraphStyles)
            {
                string c = CssClass(s.Name);
                sb.Append("p.").Append(c).Append(", li.").Append(c).Append(", div.").Append(c).Append("\r\n\t{");
                AppendStyleNames(sb, s, sheet);
                RtfParaFormat para = s.ResolvedPara ?? new RtfParaFormat();
                sb.Append("margin-top:").Append(Pt(para.SpaceBefore ?? 0)).Append(';');
                sb.Append("margin-right:0pt;");
                sb.Append("margin-bottom:").Append(Pt(para.SpaceAfter ?? 0)).Append(';');
                sb.Append("margin-left:").Append(Pt(para.LeftIndent ?? 0)).Append(';');
                sb.Append("text-indent:").Append(Pt(para.FirstLineIndent ?? 0)).Append(';');
                sb.Append("text-align:").Append(CssAlignment(para.Alignment ?? 0)).Append(';');
                sb.Append(CssLineHeight(para.Spacing));
                AppendCharCss(sb, s.ResolvedChar ?? new RtfCharFormat(), sheet, true);
                sb.Append("}\r\n");
            }
            // Heading levels the source has no style for: the word processor's own "heading N", by name only
            var levels = new SortedSet<int>(doc.Paragraphs
                .Where(p => { RtfStyle s = sheet.ParagraphNamed(p.StyleName); return s != null && IsBuiltInHeading(p, s); })
                .Select(p => p.HeadingLevel));
            foreach (int level in levels)
                sb.Append("p.").Append(CssClass("heading " + level)).Append("\r\n\t{mso-style-name:")
                  .Append(CssString("heading " + level)).Append(";}\r\n");
            foreach (RtfStyle s in characterStyles)
            {
                sb.Append("span.").Append(CssClass(s.Name)).Append("\r\n\t{");
                AppendStyleNames(sb, s, sheet);
                AppendCharCss(sb, s.ResolvedChar ?? new RtfCharFormat(), sheet, false);
                sb.Append("}\r\n");
            }
            sb.Append("</style>");
            return sb.ToString();
        }

        private static RtfStyle Parent(RtfStyle s, RtfStyleSheet sheet)
        {
            if (!s.BasedOn.HasValue || s.BasedOn == s.Number) return null;
            RtfStyle parent = s.IsCharacter ? sheet.Character(s.BasedOn.Value) : sheet.Paragraph(s.BasedOn.Value);
            return parent == s ? null : parent;
        }

        private static void AppendStyleNames(StringBuilder sb, RtfStyle s, RtfStyleSheet sheet)
        {
            sb.Append("mso-style-name:").Append(CssString(s.Name)).Append(';');
            RtfStyle parent = Parent(s, sheet);
            sb.Append("mso-style-parent:").Append(CssString(parent?.Name ?? "")).Append(';');
        }

        /// <summary>
        /// A style's character formatting as CSS. <paramref name="complete"/> (paragraph styles):
        /// everything, so the browser's defaults don't creep in; otherwise only what the style sets.
        /// </summary>
        private static void AppendCharCss(StringBuilder sb, RtfCharFormat ch, RtfStyleSheet sheet, bool complete)
        {
            string font = sheet.FontName(ch.Font ?? (complete ? sheet.DefaultFont : null));
            if (font != null) sb.Append("font-family:").Append(CssFontName(font)).Append(';');
            if (ch.Size.HasValue || complete) sb.Append("font-size:").Append(Pt(ch.Size ?? 12f)).Append(';');
            if (ch.Bold.HasValue || complete) sb.Append("font-weight:").Append(ch.Bold == true ? "bold" : "normal").Append(';');
            if (ch.Italic.HasValue || complete) sb.Append("font-style:").Append(ch.Italic == true ? "italic" : "normal").Append(';');
            if (ch.Underline == true || ch.Strikethrough == true)
                sb.Append("text-decoration:").Append(ch.Underline == true ? "underline" : "").Append(ch.Underline == true && ch.Strikethrough == true ? " " : "")
                  .Append(ch.Strikethrough == true ? "line-through" : "").Append(';');
            if (ch.Superscript == true) sb.Append("vertical-align:super;");
            else if (ch.Subscript == true) sb.Append("vertical-align:sub;");
            int? colour = sheet.Rgb(ch.Colour);
            if (colour.HasValue) sb.Append("color:").Append(Hex(colour.Value)).Append(';');
            int? highlight = sheet.Rgb(ch.Highlight);
            if (highlight.HasValue) sb.Append("background:").Append(Hex(highlight.Value)).Append(';');
            string language = CssLanguage(ch.Language ?? (complete && sheet.DefaultLanguage > 0 ? sheet.DefaultLanguage : 0));
            if (language != null) sb.Append("mso-ansi-language:").Append(language).Append(';');
        }

        /// <summary>A paragraph's inline style in styled HTML: only how it differs from its style.</summary>
        private static string StyledParagraphStyle(FormattedParagraph p, RtfStyle style, bool indents)
        {
            RtfParaFormat para = style.ResolvedPara ?? new RtfParaFormat();
            var sb = new StringBuilder("white-space:pre-wrap;");
            if ((int)p.Alignment != (para.Alignment ?? 0)) sb.Append("text-align:").Append(CssAlignment((int)p.Alignment)).Append(';');
            if (!StyleMatcher.Near(p.SpaceBefore, para.SpaceBefore ?? 0)) sb.Append("margin-top:").Append(Pt(p.SpaceBefore)).Append(';');
            if (!StyleMatcher.Near(p.SpaceAfter, para.SpaceAfter ?? 0)) sb.Append("margin-bottom:").Append(Pt(p.SpaceAfter)).Append(';');
            if (indents)
            {
                if (!StyleMatcher.Near(p.LeftIndent, para.LeftIndent ?? 0)) sb.Append("margin-left:").Append(Pt(p.LeftIndent)).Append(';');
                if (!StyleMatcher.Near(p.FirstLineIndent, para.FirstLineIndent ?? 0)) sb.Append("text-indent:").Append(Pt(p.FirstLineIndent)).Append(';');
            }
            if (!p.LineSpacing.SameAs(para.Spacing)) sb.Append(CssLineHeight(p.LineSpacing));
            return sb.ToString();
        }

        /// <summary>A run in styled HTML: its character style as a class, and only what neither style explains.</summary>
        private static void AppendStyledRun(StringBuilder sb, FormattedRun run, RtfStyle paragraphStyle, RtfStyleSheet sheet)
        {
            RtfCharFormat effective = (paragraphStyle.ResolvedChar ?? new RtfCharFormat()).Clone();
            RtfStyle cs = run.CharacterStyle != null ? sheet.CharacterNamed(run.CharacterStyle) : null;
            if (cs?.ResolvedChar != null) effective.Over(cs.ResolvedChar);

            var style = new StringBuilder();
            if (!string.IsNullOrEmpty(run.FontName) && !StyleMatcher.SameFont(run.FontName, sheet.FontName(effective.Font ?? sheet.DefaultFont)))
                style.Append("font-family:").Append(CssFontName(run.FontName)).Append(';');
            if (run.FontSize > 0 && Math.Abs(run.FontSize - (effective.Size ?? 12f)) > 0.3f) style.Append("font-size:").Append(Pt(run.FontSize)).Append(';');
            if (!StyleMatcher.SameColour(run.Colour, sheet.Rgb(effective.Colour)))
                style.Append("color:").Append(run.Colour.HasValue ? Hex(run.Colour.Value) : "black").Append(';');
            if (run.Highlight.HasValue && run.Highlight != sheet.Rgb(effective.Highlight))
                style.Append("background-color:").Append(Hex(run.Highlight.Value)).Append(';');
            // switched off where the style has it on
            if (!run.Bold && effective.Bold == true) style.Append("font-weight:normal;");
            if (!run.Italic && effective.Italic == true) style.Append("font-style:normal;");

            var closing = new Stack<string>();
            if (cs != null || style.Length > 0)
            {
                sb.Append("<span");
                if (cs != null) sb.Append(" class=").Append(CssClass(cs.Name));
                if (style.Length > 0) sb.Append(" style=\"").Append(style).Append('"');
                sb.Append('>');
                closing.Push("span");
            }
            void Open(bool on, string tag) { if (on) { sb.Append('<').Append(tag).Append('>'); closing.Push(tag); } }
            Open(run.Bold && effective.Bold != true, "b");
            Open(run.Italic && effective.Italic != true, "i");
            Open(run.Underline && effective.Underline != true, "u");
            Open(run.Strikethrough && effective.Strikethrough != true, "s");
            Open(run.Superscript && effective.Superscript != true, "sup");
            Open(run.Subscript && !run.Superscript && effective.Subscript != true, "sub");

            sb.Append(Escape(run.Text).Replace("\v", "<br>"));
            while (closing.Count > 0) sb.Append("</").Append(closing.Pop()).Append('>');
        }

        /// <summary>A heading at a level the source has no style for (the matched style isn't at that level).</summary>
        private static bool IsBuiltInHeading(FormattedParagraph p, RtfStyle style) =>
            p.HeadingLevel >= 1 && p.HeadingLevel <= 9 && style.HeadingLevel != p.HeadingLevel;

        /// <summary>A style name as a CSS class: its letters and digits (as Word does: "Body Text First Para" → BodyTextFirstPara).</summary>
        private static string CssClass(string name)
        {
            var sb = new StringBuilder();
            foreach (char c in name ?? "") if (char.IsLetterOrDigit(c) && c < 0x80) sb.Append(c);
            if (sb.Length == 0 || char.IsDigit(sb[0])) sb.Insert(0, "s");
            return sb.ToString();
        }

        private static string CssString(string s) => "\"" + (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

        private static string CssAlignment(int alignment)
        {
            switch (alignment)
            {
                case 1: return "center";
                case 2: return "right";
                case 3: return "justify";
                default: return "left";
            }
        }

        private static string CssLineHeight(LineSpacing spacing)
        {
            LineSpacing s = LineSpacing.Normalise(spacing);
            switch (s.Rule)
            {
                case LineSpacingRule.Multiple: return "line-height:" + (s.Value * 100).ToString("0.#", CultureInfo.InvariantCulture) + "%;";
                case LineSpacingRule.AtLeast: return "line-height:" + Pt(s.Value) + ";mso-line-height-rule:at-least;";
                case LineSpacingRule.Exactly: return "line-height:" + Pt(s.Value) + ";mso-line-height-rule:exactly;";
                default: return "line-height:normal;";
            }
        }

        /// <summary>A Windows language ID as Word's HTML gives it (EN-GB, FR); null if unknown.</summary>
        private static string CssLanguage(int lcid)
        {
            if (lcid <= 0 || lcid == 0x400 || lcid == 0x3FF) return null; // \lang1024: no language
            try { return CultureInfo.GetCultureInfo(lcid).Name.ToUpperInvariant(); }
            catch (Exception) { return null; }
        }
    }
}
