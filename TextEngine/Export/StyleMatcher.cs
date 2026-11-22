using System;
using System.Collections.Generic;
using System.Linq;
using ClipX2.TextEngine.Import;

namespace ClipX2.TextEngine.Export
{
    /// <summary>
    /// Names each paragraph's and run's style from how it looks: the style sheet the text came with
    /// says what each style looks like, so whatever has been done to the text in between (sorted,
    /// joined, retyped, italics added) each paragraph gets the style it now looks like, and each run
    /// the character style that explains how it differs from its paragraph (italic and French: the
    /// "French" style rather than "Emphasis"). Whatever no style explains stays direct formatting.
    /// </summary>
    public static class StyleMatcher
    {
        private static readonly string[] Preferred = { "Emphasis", "Strong", "Strong Emphasis" };

        public static void Apply(FormattedDocument doc, RtfStyleSheet sheet)
        {
            if (doc == null || sheet == null) return;
            List<RtfStyle> paragraphStyles = sheet.Styles.Where(s => !s.IsCharacter).ToList();
            List<RtfStyle> characterStyles = sheet.Styles.Where(s => s.IsCharacter && s.ResolvedChar != null && !s.ResolvedChar.IsEmpty).ToList();
            if (paragraphStyles.Count == 0) return;

            foreach (FormattedParagraph p in doc.Paragraphs)
            {
                FormattedRun main = p.Runs.Where(r => r.Text.Trim().Length > 0).OrderByDescending(r => r.Text.Length).FirstOrDefault();

                // The heading level first (it is what the paragraph is: a heading set in the editor
                // stays a heading at that level), then the look. "Heading N" wins a close call.
                List<RtfStyle> candidates = paragraphStyles.Where(s => s.HeadingLevel == p.HeadingLevel).ToList();
                if (candidates.Count == 0) candidates = paragraphStyles;

                RtfStyle best = null;
                float bestDistance = float.MaxValue;
                foreach (RtfStyle s in candidates)
                {
                    float d = Distance(p, main, s, sheet);
                    if (p.HeadingLevel > 0 && !IsHeadingNamed(s, p.HeadingLevel)) d += HeadingNameBonus;
                    if (best == null || d < bestDistance - 0.001f
                        || (Math.Abs(d - bestDistance) < 0.001f && Better(s, best, sheet)))
                    {
                        best = s;
                        bestDistance = d;
                    }
                }
                p.StyleName = best.Name;
                foreach (FormattedRun r in p.Runs)
                    r.CharacterStyle = CharacterStyleFor(r, best.ResolvedChar, characterStyles, sheet);
            }
        }

        /// <summary>The likelier of two equally good styles: the one the source used more, then the lower number.</summary>
        /// <summary>How much nearer in look another style at the same level must be to win over "Heading N".</summary>
        private const float HeadingNameBonus = 2f;

        private static bool IsHeadingNamed(RtfStyle s, int level) =>
            s.Name != null && string.Equals(s.Name.Trim(), "heading " + level, StringComparison.OrdinalIgnoreCase);

        private static bool Better(RtfStyle a, RtfStyle b, RtfStyleSheet sheet)
        {
            int ua = sheet.UsageOf(a.Name), ub = sheet.UsageOf(b.Name);
            if (ua != ub) return ua > ub;
            int pa = Array.IndexOf(Preferred, a.Name), pb = Array.IndexOf(Preferred, b.Name);
            if (pa != pb) return pa >= 0 && (pb < 0 || pa < pb);
            return a.Number < b.Number;
        }

        private static float Distance(FormattedParagraph p, FormattedRun main, RtfStyle s, RtfStyleSheet sheet)
        {
            RtfParaFormat para = s.ResolvedPara;
            RtfCharFormat ch = s.ResolvedChar;
            float d = 0;
            if ((int)p.Alignment != (para.Alignment ?? 0)) d += 3;
            if (p.HeadingLevel != s.HeadingLevel) d += 4;
            if (p.ListKind == ListKind.None)
            {
                if (!Near(p.FirstLineIndent, para.FirstLineIndent ?? 0)) d += 2;
                if (!Near(p.LeftIndent, para.LeftIndent ?? 0)) d += 2;
            }
            if (!Near(p.SpaceBefore, para.SpaceBefore ?? 0)) d += 1;
            if (!Near(p.SpaceAfter, para.SpaceAfter ?? 0)) d += 1;
            if (!p.LineSpacing.SameAs(para.Spacing)) d += 1;
            if (main != null)
            {
                if (!SameFont(main.FontName, sheet.FontName(ch.Font))) d += 2;
                if (main.FontSize > 0 && Math.Abs(main.FontSize - (ch.Size ?? 12f)) > 0.3f) d += 2;
                if (main.Bold != (ch.Bold ?? false)) d += 1;
                if (main.Italic != (ch.Italic ?? false)) d += 1;
                if (!SameColour(main.Colour, sheet.Rgb(ch.Colour))) d += 1;
            }
            return d;
        }

        /// <summary>The character style that best explains how a run differs from its paragraph's style; null if none.</summary>
        private static string CharacterStyleFor(FormattedRun run, RtfCharFormat paragraph, List<RtfStyle> styles, RtfStyleSheet sheet)
        {
            if (run.Text.Length == 0) return null;
            RtfStyle best = null;
            int bestCovered = 0;
            foreach (RtfStyle s in styles)
            {
                if (!Explains(s.ResolvedChar, paragraph, run, sheet, out int covered) || covered == 0) continue;
                if (best == null || covered > bestCovered || (covered == bestCovered && Better(s, best, sheet)))
                {
                    best = s;
                    bestCovered = covered;
                }
            }
            return best?.Name;
        }

        /// <summary>
        /// True if nothing the style sets contradicts the run; <paramref name="covered"/>: how many
        /// of the run's differences from its paragraph style the style accounts for.
        /// </summary>
        internal static bool Explains(RtfCharFormat style, RtfCharFormat paragraph, FormattedRun run, RtfStyleSheet sheet, out int covered)
        {
            int n = 0;
            bool ok = true;
            void Flag(bool? set, bool actual, bool? under)
            {
                if (!set.HasValue || !ok) return;
                if (set.Value != actual) ok = false;
                else if ((under ?? false) != set.Value) n++;
            }
            Flag(style.Bold, run.Bold, paragraph.Bold);
            Flag(style.Italic, run.Italic, paragraph.Italic);
            Flag(style.Underline, run.Underline, paragraph.Underline);
            Flag(style.Strikethrough, run.Strikethrough, paragraph.Strikethrough);
            Flag(style.Superscript, run.Superscript, paragraph.Superscript);
            Flag(style.Subscript, run.Subscript, paragraph.Subscript);
            if (style.Hidden == true) ok = false;
            if (ok && style.Font.HasValue)
            {
                string name = sheet.FontName(style.Font);
                if (!SameFont(name, run.FontName)) ok = false;
                else if (!SameFont(name, sheet.FontName(paragraph.Font))) n++;
            }
            if (ok && style.Size.HasValue)
            {
                if (Math.Abs(style.Size.Value - run.FontSize) > 0.3f) ok = false;
                else if (Math.Abs(style.Size.Value - (paragraph.Size ?? 12f)) > 0.3f) n++;
            }
            if (ok && style.Colour.HasValue)
            {
                int? rgb = sheet.Rgb(style.Colour);
                if (!SameColour(rgb, run.Colour)) ok = false;
                else if (!SameColour(rgb, sheet.Rgb(paragraph.Colour))) n++;
            }
            if (ok && style.Highlight.HasValue)
            {
                int? rgb = sheet.Rgb(style.Highlight);
                if (rgb != run.Highlight) ok = false;
                else if (rgb != sheet.Rgb(paragraph.Highlight)) n++;
            }
            if (ok && style.Language.HasValue)
            {
                if (style.Language.Value != run.Language) ok = false;
                else if (style.Language != paragraph.Language) n++;
            }
            covered = n;
            return ok;
        }

        internal static bool Near(float a, float b) => Math.Abs(a - b) < 0.6f;

        internal static bool SameFont(string a, string b) =>
            string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b) || string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        /// <summary>Automatic and black count as the same (LibreOffice's \cf0, the editor's black).</summary>
        internal static bool SameColour(int? a, int? b) => (a ?? 0) == (b ?? 0);
    }
}
