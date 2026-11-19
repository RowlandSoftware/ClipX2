using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ClipX2.TextEngine;
using Line = ClipX2.TextEngine.ListFormatter.Line;

namespace ClipX2.UI
{
    /// <summary>
    /// The list tools (Bullets, Numbers) on the editor: the selection widened to whole lines, or
    /// the whole text if nothing is selected. One Undo step each.
    ///
    /// Plain text: bullets and numbers are typed in (ClipX 1; see <see cref="ListFormatter"/>).
    ///
    /// Rich text: a live list (the editor's own, kept as a real list when pasted into Word,
    /// Outlook, LibreOffice): it renumbers itself as items are added or moved, and has levels,
    /// taken from each line's indentation (a tab, or one pad width of spaces, per level), or from
    /// the list a paragraph is already in. The indentation (and an old bullet, or with Renumber an
    /// old number) is taken off; the list's own indents replace it. Numbers go down the levels as
    /// 1 → a → i → I → A (starting from the style chosen). What the editor can't do as a live
    /// list is typed in as in plain text: Legal numbering (1.1.) and bullets other than •.
    /// </summary>
    internal static class ListTools
    {
        // TOM ITextPara.ListType: the kind in the low word, the punctuation in the high word
        private const int tomListNone = 0, tomListBullet = 1;
        private const int tomListNumberAsArabic = 2, tomListNumberAsLCLetter = 3, tomListNumberAsUCLetter = 4;
        private const int tomListNumberAsLCRoman = 5, tomListNumberAsUCRoman = 6;
        private const int tomListParentheses = 0x10000, tomListPeriod = 0x20000, tomListPlain = 0x30000; // 0 = "1)"
        private const int tomListNoNumber = 0x40000; // an item that shows no number and doesn't count
        private const int tomParagraph = 4;

        // List indents (points): each level 0.5 inch further in, the number hanging 0.25 inch
        private const float LevelStep = 36f, Hang = 18f;

        /// <summary>The bullet the editor's live lists use; others are typed in.</summary>
        public const string LiveBullet = "•";

        /// <summary>True if bullets with these options become a live list in rich text.</summary>
        public static bool BulletsAreLive(BulletOptions o) => o.Character == LiveBullet;

        /// <summary>True if numbers with these options become a live list in rich text.</summary>
        public static bool NumbersAreLive(NumberOptions o) => o.Style != NumberStyle.Legal;

        /// <summary>Bullets the lines. Returns a short message for the status bar.</summary>
        public static string Bullets(ClipXRichTextBox box, bool richText, BulletOptions o)
        {
            if (richText && BulletsAreLive(o))
                return Live(box, o.Tab, o.PadChars, (text, at, end) => ListFormatter.BulletLength(text, at, end),
                    level => tomListBullet, 1, "bullet")
                    ? "Bulleted (a live list)" : TypedBullets(box, richText, o);
            return TypedBullets(box, richText, o);
        }

        /// <summary>Numbers the lines. Returns a short message for the status bar.</summary>
        public static string Numbers(ClipXRichTextBox box, bool richText, NumberOptions o)
        {
            if (richText && NumbersAreLive(o))
            {
                int punctuation = o.FollowChar switch
                {
                    NumberPunctuation.None => tomListPlain,
                    NumberPunctuation.Period => tomListPeriod,
                    NumberPunctuation.TwoBrackets => tomListParentheses,
                    _ => 0, // "1)"
                };
                Func<string, int, int, int> strip = o.Renumber
                    ? (text, at, end) => ListFormatter.NumberLength(text, at, end)
                    : (Func<string, int, int, int>)((text, at, end) => 0);
                if (Live(box, o.Tab, o.PadChars, strip, level => NumberKind(o.Style, level) | punctuation, Math.Max(1, o.StartNumber), "number"))
                    return "Numbered (a live list)";
            }
            var (from, to) = Scope(box);
            List<TextEdit> edits = ListFormatter.NumberEdits(box.Text, from, to, o);
            Typed(box, richText, edits, from, to);
            return "Numbered";
        }

        /// <summary>Takes the bullets off the lines (typed or live); indentation is kept or restored. Returns a status message.</summary>
        public static string RemoveBullets(ClipXRichTextBox box, bool richText, BulletOptions o) =>
            Remove(box, richText, bullets: true, o.Tab, o.PadChars,
                (text, from, to) => ListFormatter.RemoveBulletEdits(text, from, to)) ? "Bullets removed" : "No bullets to remove";

        /// <summary>Takes the numbers off the lines (typed or live); indentation is kept or restored. Returns a status message.</summary>
        public static string RemoveNumbers(ClipXRichTextBox box, bool richText, NumberOptions o) =>
            Remove(box, richText, bullets: false, o.Tab, o.PadChars,
                (text, from, to) => ListFormatter.RemoveNumberEdits(text, from, to)) ? "Numbers removed" : "No numbers to remove";

        /// <summary>
        /// Removes typed markers (<paramref name="typedEdits"/>) and, in rich text, takes live-list
        /// paragraphs of the kind out of the list, putting back the indentation their level stood
        /// for (a tab, or a pad width of spaces, per level, as Bullets and Numbers read it), so the
        /// text is as it was before the list was made. False if there was nothing to remove.
        /// </summary>
        private static bool Remove(ClipXRichTextBox box, bool richText, bool bullets, bool tab, int padChars,
            Func<string, int, int, List<TextEdit>> typedEdits)
        {
            string text = box.Text;
            var (from, to) = Scope(box);
            bool hadSelection = box.SelectionLength > 0;
            List<Line> lines = ListFormatter.LinesIn(text, from, to);
            List<TextEdit> edits = typedEdits(text, from, to);

            if (richText)
            {
                bool changed = false;
                bool ok = box.EditDocument(doc =>
                {
                    // Which lines are live items of this kind, and at what level
                    var live = new List<(Line Line, int Level)>();
                    foreach (Line line in lines)
                    {
                        int kind = -1, level = 0;
                        SetParagraph(doc, line.Start, p => { kind = (int)p.ListType & 0xFFFF; level = Math.Max(0, (int)p.ListLevelIndex); });
                        if (kind == tomListNone || (kind == tomListBullet) != bullets) continue;
                        live.Add((line, level));
                    }

                    // A live item's text isn't a typed marker; give it back its indentation instead
                    edits.RemoveAll(e => live.Exists(l => e.Start >= l.Line.Start && e.Start <= l.Line.End));
                    foreach (var (line, level) in live)
                        if (level > 0 && line.Length > 0) edits.Add(new TextEdit(line.Start, 0, ListFormatter.Indent(level, tab, padChars)));
                    edits.Sort((a, b) => a.Start.CompareTo(b.Start));

                    ApplyEdits(doc, edits);
                    foreach (var (line, _) in live)
                        SetParagraph(doc, NewPosition(line.Start, edits), p =>
                        {
                            p.ListType = tomListNone;
                            p.SetIndents(0f, 0f, 0f);
                        });
                    changed = edits.Count > 0 || live.Count > 0;
                });
                if (ok)
                {
                    Reselect(box, hadSelection, lines, edits);
                    return changed;
                }
            }

            if (edits.Count == 0) return false;
            box.ApplyEdits(edits);
            Reselect(box, hadSelection, lines, edits);
            return true;
        }

        private static string TypedBullets(ClipXRichTextBox box, bool richText, BulletOptions o)
        {
            var (from, to) = Scope(box);
            Typed(box, richText, ListFormatter.BulletEdits(box.Text, from, to, o), from, to);
            return "Bulleted";
        }

        /// <summary>The selection, or the whole text if nothing is selected.</summary>
        private static (int From, int To) Scope(ClipXRichTextBox box) =>
            box.SelectionLength > 0 ? (box.SelectionStart, box.SelectionStart + box.SelectionLength) : (0, box.TextLength);

        /// <summary>
        /// Numbers down the levels: from the style chosen, through 1 → a → i → I → A and round
        /// again (so Arabic gives 1, a, i…; capital Roman gives I, A, 1, a, i…).
        /// </summary>
        private static int NumberKind(NumberStyle style, int level)
        {
            int[] cascade = { tomListNumberAsUCRoman, tomListNumberAsUCLetter, tomListNumberAsArabic, tomListNumberAsLCLetter, tomListNumberAsLCRoman };
            int first = style switch
            {
                NumberStyle.RomanCap => 0,
                NumberStyle.LettersCap => 1,
                NumberStyle.Letters => 3,
                NumberStyle.Roman => 4,
                _ => 2,
            };
            return cascade[(first + level) % cascade.Length];
        }

        /************ Typed lists ************/

        /// <summary>
        /// Applies typed bullets or numbers; in rich text, the lines first stop being a live list
        /// (so they don't get two numbers). Keeps the lines selected if they were.
        /// </summary>
        private static void Typed(ClipXRichTextBox box, bool richText, List<TextEdit> edits, int from, int to)
        {
            bool hadSelection = box.SelectionLength > 0;
            List<Line> lines = ListFormatter.LinesIn(box.Text, from, to);
            if (!richText || !box.EditDocument(doc =>
                {
                    ApplyEdits(doc, edits);
                    foreach (Line line in lines)
                        SetParagraph(doc, NewPosition(line.Start, edits), p => { if ((int)p.ListType != tomListNone) p.ListType = tomListNone; });
                }))
                box.ApplyEdits(edits);
            Reselect(box, hadSelection, lines, edits);
        }

        /************ Live lists ************/

        /// <summary>
        /// Makes the lines a live list: takes off each line's indentation and old marker
        /// (<paramref name="markerLength"/>), then sets each non-blank paragraph's list kind
        /// (<paramref name="kindForLevel"/>), level and indents. False if TOM isn't available.
        /// </summary>
        private static bool Live(ClipXRichTextBox box, bool tab, int padChars, Func<string, int, int, int> markerLength,
            Func<int, int> kindForLevel, int start, string what)
        {
            string text = box.Text;
            var (from, to) = Scope(box);
            bool hadSelection = box.SelectionLength > 0;
            List<Line> lines = ListFormatter.LinesIn(text, from, to);
            var levels = new int[lines.Count];
            var edits = new List<TextEdit>();
            var blank = new bool[lines.Count];

            return box.EditDocument(doc =>
            {
                // Levels first, from indentation or the list the paragraph is already in
                for (int i = 0; i < lines.Count; i++)
                {
                    Line line = lines[i];
                    int indent = ListFormatter.IndentLength(text, line);
                    blank[i] = indent == line.Length;
                    if (blank[i]) continue;
                    int level = ListFormatter.Level(text, line, tab, padChars);
                    if (indent == 0) level = Math.Max(level, ExistingLevel(doc, line.Start));
                    levels[i] = level;
                    int old = markerLength(text, line.Start + indent, line.End);
                    if (indent + old > 0) edits.Add(new TextEdit(line.Start, indent + old, ""));
                }
                ApplyEdits(doc, edits);

                // A blank line between numbered items would end the list (the editor restarts the
                // numbering after any paragraph that isn't an item), so it becomes an item without a
                // number, at the shallower of the levels either side: the numbering runs on past it
                var itemKind = new int[lines.Count];
                for (int i = 0; i < lines.Count; i++)
                {
                    if (!blank[i]) { itemKind[i] = kindForLevel(levels[i]); continue; }
                    int before = i - 1, after = i + 1;
                    while (before >= 0 && blank[before]) before--;
                    while (after < lines.Count && blank[after]) after++;
                    if (before < 0 || after >= lines.Count || (kindForLevel(0) & 0xFFFF) == tomListBullet) continue;
                    levels[i] = Math.Min(levels[before], levels[after]);
                    itemKind[i] = (kindForLevel(levels[i]) & 0xFFFF) | tomListNoNumber;
                }

                for (int i = 0; i < lines.Count; i++)
                {
                    if (itemKind[i] == 0) continue;
                    int level = levels[i], kind = itemKind[i];
                    SetParagraph(doc, NewPosition(lines[i].Start, edits), p =>
                    {
                        p.ListType = kind;
                        p.ListLevelIndex = level;
                        p.ListStart = level == 0 ? start : 1;
                        p.ListTab = Hang;
                        p.SetIndents(-Hang, LevelStep * (level + 1), 0f);
                    });
                }
            }) && Reselect(box, hadSelection, lines, edits);
        }

        /// <summary>The level of the live list a paragraph is already in (0 if none).</summary>
        private static int ExistingLevel(ITextDocument doc, int position)
        {
            int level = 0;
            SetParagraph(doc, position, p =>
            {
                if ((int)p.ListType != tomListNone) level = Math.Max(0, (int)p.ListLevelIndex);
            });
            return level;
        }

        /************ TOM helpers ************/

        /// <summary>Applies text edits (in text order, positions in the original) from last to first.</summary>
        private static void ApplyEdits(ITextDocument doc, List<TextEdit> edits)
        {
            ITextRange range = doc.Range(0, 0);
            try
            {
                for (int k = edits.Count - 1; k >= 0; k--)
                {
                    range.SetRange(edits[k].Start, edits[k].Start + edits[k].Length);
                    range.Text = edits[k].Replacement;
                }
            }
            finally { Marshal.ReleaseComObject(range); }
        }

        /// <summary>Runs <paramref name="change"/> on the paragraph format (ITextPara) of the paragraph at <paramref name="position"/>.</summary>
        private static void SetParagraph(ITextDocument doc, int position, Action<dynamic> change)
        {
            ITextRange range = doc.Range(position, position);
            object para = null;
            try
            {
                range.Expand(tomParagraph);
                para = range.Para;
                change(para);
            }
            finally
            {
                if (para != null) Marshal.ReleaseComObject(para);
                Marshal.ReleaseComObject(range);
            }
        }

        /// <summary>Where a line start is once the edits are made (edits at the line start itself don't move it).</summary>
        private static int NewPosition(int position, List<TextEdit> edits)
        {
            int result = position;
            foreach (TextEdit e in edits)
                if (e.Start < position) result += e.Replacement.Length - e.Length;
            return result;
        }

        /// <summary>Selects the lines again (if they were selected), from the start of the first to the end of the last.</summary>
        private static bool Reselect(ClipXRichTextBox box, bool hadSelection, List<Line> lines, List<TextEdit> edits)
        {
            if (lines.Count == 0) return true;
            int start = lines[0].Start;
            int end = lines[lines.Count - 1].End + edits.Sum(e => e.Replacement.Length - e.Length);
            if (hadSelection) box.Select(start, Math.Max(0, end - start));
            else box.Select(Math.Min(box.SelectionStart, box.TextLength), 0);
            return true;
        }
    }
}
