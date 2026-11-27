using System;
using System.Collections.Generic;
using System.Linq;
using ClipX2.TextEngine.Export;
using Line = ClipX2.TextEngine.ListFormatter.Line;

namespace ClipX2.TextEngine.Import
{
    /// <summary>
    /// Turns typed lists and typed indentation in a <see cref="FormattedDocument"/> into real
    /// structure, for plain text (and HTML such as Word's, whose list numbers are text) going into
    /// rich text: "1.", "a)", "(iv)" and bullets (• - * ◦ ▪ ·) at the start of paragraphs become
    /// list items, their levels from indentation (tabs, runs of spaces, or the paragraph's left
    /// indent), numbered lists keeping their style, punctuation and start; leading tabs and spaces
    /// on other paragraphs become paragraph indents. Legal numbering (1.1.) stays typed.
    /// </summary>
    public static class TypedListRecognition
    {
        /// <summary>One level of indentation in rich text, in points: half an inch.</summary>
        public const float LevelStep = 36f;

        /// <summary>Spaces making one level of indentation in typed text.</summary>
        public const int SpacesPerLevel = 4;

        public static void Apply(FormattedDocument doc, BulletOptions bullets = null, NumberOptions numbers = null)
        {
            bullets = bullets ?? new BulletOptions();
            numbers = numbers ?? new NumberOptions();
            var paragraphs = doc.Paragraphs;
            int count = paragraphs.Count;
            if (count == 0) return;

            // The paragraphs as lines of one text (no-break spaces as spaces, for recognition only)
            string[] texts = paragraphs.Select(p => p.ListKind == ListKind.None ? string.Concat(p.Runs.Select(r => r.Text)) : "").ToArray();
            string joined = string.Join("\n", texts).Replace(' ', ' ');
            if (texts.Any(t => t.IndexOf('\n') >= 0 || t.IndexOf('\r') >= 0)) return; // a paragraph with a line break in it: leave the document as it is
            // One line a paragraph, an empty last one included (LinesIn would drop it, as the end of
            // a selection, and the text ending with a line break would then leave every list typed)
            var lines = new List<Line>(count);
            for (int i = 0, at = 0; i < count; at += texts[i].Length + 1, i++) lines.Add(new Line(at, at + texts[i].Length));

            bool Blank(int i) => paragraphs[i].ListKind == ListKind.None && joined.Substring(lines[i].Start, lines[i].Length).Trim().Length == 0;
            int Indent(int i) => ListFormatter.IndentLength(joined, lines[i]);
            bool Numbered(int i) => paragraphs[i].ListKind == ListKind.None && !Blank(i)
                && ListFormatter.NumberLength(joined, lines[i].Start + Indent(i), lines[i].End) > 0;
            bool Bulleted(int i) => paragraphs[i].ListKind == ListKind.None && !Blank(i)
                && ListFormatter.BulletLength(joined, lines[i].Start + Indent(i), lines[i].End) > 0;
            int Level(int i) => ListFormatter.Level(joined, lines[i], true, SpacesPerLevel)
                + (int)Math.Round(Math.Max(0, paragraphs[i].LeftIndent) / LevelStep);

            var handled = new bool[count];
            for (int i = 0; i < count; i++)
            {
                if (handled[i]) continue;
                bool numbered = Numbered(i), bulleted = !numbered && Bulleted(i);
                if (!numbered && !bulleted) continue;
                Func<int, bool> member = numbered ? (Func<int, bool>)Numbered : Bulleted;

                // The list: paragraphs of the same kind, with blank ones between them
                int end = i;
                for (int j = i + 1; j < count; j++) { if (member(j)) end = j; else if (!Blank(j)) break; }
                for (int j = i; j <= end; j++) handled[j] = true;
                int baseLevel = Enumerable.Range(i, end - i + 1).Where(j => !Blank(j)).Min(Level);

                if (numbered)
                {
                    NumberOptions o = ListFormatter.RecogniseList(joined, lines, i, end, numbers);
                    if (o == null || o.Style == NumberStyle.Legal) continue; // typed it stays
                    if (!Counts(o.Style, i, end)) continue; // "N. E. Zenith Co.", "A. Smith": not a list
                    ListNumberStyle style = ToNumberStyle(o.FollowChar);
                    int previousLevel = 0;
                    for (int j = i; j <= end; j++)
                    {
                        FormattedParagraph p = paragraphs[j];
                        if (Blank(j))
                        {
                            // A blank line inside a numbered list: an item without a number, so the numbering runs on
                            int before = previousLevel, after = NextLevel(j, end);
                            int level = Math.Min(before, after);
                            MakeItem(p, ToListKind(ListFormatter.StyleForLevel(o.Style, level)), ListNumberStyle.NoNumber, level, level == 0 ? o.StartNumber : 1, null);
                            continue;
                        }
                        int lvl = Math.Max(0, Level(j) - baseLevel);
                        previousLevel = lvl;
                        RemovePrefix(p, Indent(j) + ListFormatter.NumberLength(joined, lines[j].Start + Indent(j), lines[j].End));
                        MakeItem(p, ToListKind(ListFormatter.StyleForLevel(o.Style, lvl)), style, lvl, lvl == 0 ? o.StartNumber : 1, null);
                    }
                }
                else
                {
                    string first = joined.Substring(lines[i].Start + Indent(i), 1);
                    if (first == "·" || first == "*" || first == "-") first = "•"; // stand-ins for the usual bullet
                    for (int j = i; j <= end; j++)
                    {
                        if (Blank(j)) continue; // blank lines stay blank lines between bullets
                        int lvl = Math.Max(0, Level(j) - baseLevel);
                        RemovePrefix(paragraphs[j], Indent(j) + ListFormatter.BulletLength(joined, lines[j].Start + Indent(j), lines[j].End));
                        string bullet = ListFormatter.BulletForLevel(first, lvl);
                        MakeItem(paragraphs[j], ListKind.Bullet, ListNumberStyle.RightParenthesis, lvl, 1, bullet == "•" ? null : bullet);
                    }
                }

                // Letters and Roman numerals only make a list when at least two of its top-level
                // items count on (a, b; iii, iv): a line on its own that starts with an initial
                // ("N. E. Zenith Co.") or a run of them (names) stays as typed. Digits always do.
                bool Counts(NumberStyle style, int first, int last)
                {
                    if (style == NumberStyle.Arabic) return true;
                    var levels = Enumerable.Range(first, last - first + 1).Where(j => !Blank(j)).Select(j => (j, Level(j))).ToList();
                    int top = levels.Min(x => x.Item2);
                    var values = levels.Where(x => x.Item2 == top).Select(x => ListFormatter.MarkerValue(Marker(x.j), style)).ToList();
                    if (values.Count < 2) return false;
                    for (int k = 1; k < values.Count; k++) if (values[k] != values[k - 1] + 1) return false;
                    return true;
                }

                string Marker(int j)
                {
                    int at = lines[j].Start + Indent(j);
                    return joined.Substring(at, ListFormatter.NumberLength(joined, at, lines[j].End));
                }

                int NextLevel(int j, int last)
                {
                    for (int k = j + 1; k <= last; k++) if (!Blank(k)) return Math.Max(0, Level(k) - baseLevel);
                    return 0;
                }
            }

            // Other paragraphs: leading tabs (and runs of spaces) become a paragraph indent
            for (int i = 0; i < count; i++)
            {
                if (handled[i] || paragraphs[i].ListKind != ListKind.None || Blank(i)) continue;
                int levels = 0, consumed = 0, spaces = 0;
                for (int k = lines[i].Start; k < lines[i].End; k++)
                {
                    char c = joined[k];
                    if (c == '\t') { levels++; spaces = 0; consumed = k - lines[i].Start + 1; }
                    else if (c == ' ') { if (++spaces == SpacesPerLevel) { levels++; spaces = 0; consumed = k - lines[i].Start + 1; } }
                    else break;
                }
                if (levels == 0) continue;
                RemovePrefix(paragraphs[i], consumed);
                paragraphs[i].LeftIndent += levels * LevelStep;
            }
        }

        private static void MakeItem(FormattedParagraph p, ListKind kind, ListNumberStyle style, int level, int start, string bullet)
        {
            p.ListKind = kind;
            p.ListNumberStyle = style;
            p.ListLevel = Math.Min(level, 8);
            p.ListStart = Math.Max(1, start);
            p.BulletCharacter = bullet;
            p.LeftIndent = 0;
            p.FirstLineIndent = 0;
        }

        /// <summary>Takes the first <paramref name="length"/> characters off a paragraph's runs.</summary>
        private static void RemovePrefix(FormattedParagraph p, int length)
        {
            while (length > 0 && p.Runs.Count > 0)
            {
                FormattedRun r = p.Runs[0];
                if (r.Text.Length <= length) { length -= r.Text.Length; p.Runs.RemoveAt(0); }
                else { r.Text = r.Text.Substring(length); length = 0; }
            }
        }

        public static ListKind ToListKind(NumberStyle style)
        {
            switch (style)
            {
                case NumberStyle.Letters: return ListKind.LowerLetter;
                case NumberStyle.LettersCap: return ListKind.UpperLetter;
                case NumberStyle.Roman: return ListKind.LowerRoman;
                case NumberStyle.RomanCap: return ListKind.UpperRoman;
                default: return ListKind.Arabic;
            }
        }

        public static ListNumberStyle ToNumberStyle(NumberPunctuation p)
        {
            switch (p)
            {
                case NumberPunctuation.Period: return ListNumberStyle.Period;
                case NumberPunctuation.TwoBrackets: return ListNumberStyle.Parentheses;
                case NumberPunctuation.None: return ListNumberStyle.Plain;
                default: return ListNumberStyle.RightParenthesis;
            }
        }
    }

    /// <summary>Plain text into a <see cref="FormattedDocument"/>: a paragraph a line, typed lists and indentation recognised.</summary>
    public static class PlainTextReader
    {
        public static FormattedDocument Read(string text, BulletOptions bullets = null, NumberOptions numbers = null)
        {
            var doc = new FormattedDocument();
            text = (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n');
            foreach (string line in text.Split('\n'))
            {
                var p = new FormattedParagraph();
                if (line.Length > 0) p.Runs.Add(new FormattedRun { Text = line });
                doc.Paragraphs.Add(p);
            }
            TypedListRecognition.Apply(doc, bullets, numbers);
            return doc;
        }
    }
}
