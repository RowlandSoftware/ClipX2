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
    /// list is typed in as in plain text: Legal numbering (1.1.). Bullets go • ◦ ▪ down the levels.
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

        /// <summary>
        /// True if bullets with these options become a live list in rich text: any single
        /// character (RichEdit shows a list type above 32 as that bullet), so each level can have its own.
        /// </summary>
        public static bool BulletsAreLive(BulletOptions o) =>
            !string.IsNullOrEmpty(o.Character) && o.Character.Length == 1 && o.Character[0] > ' ';

        /// <summary>True if numbers with these options become a live list in rich text.</summary>
        public static bool NumbersAreLive(NumberOptions o) => o.Style != NumberStyle.Legal;

        /// <summary>Bullets the lines. Returns a short message for the status bar.</summary>
        public static string Bullets(ClipXRichTextBox box, bool richText, BulletOptions o)
        {
            if (richText && BulletsAreLive(o))
                return Live(box, o.Tab, o.PadChars, (text, at, end) => ListFormatter.BulletLength(text, at, end),
                    level => BulletKind(o.Character, level), 1, "bullet")
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
                        if (kind == tomListNone || IsBulletKind(kind) != bullets) continue;
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
                    if (indent == 0) level = Math.Max(level, ExistingLevel(doc, line.Start)); // already in a list, or indented as a paragraph
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
                    if (before < 0 || after >= lines.Count || IsBulletKind(kindForLevel(0) & 0xFFFF)) continue;
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

        /// <summary>
        /// The level a paragraph already has: its live list's level, or else its paragraph indent
        /// (each half inch a level, as Increase indent sets it). 0 if neither.
        /// </summary>
        private static int ExistingLevel(ITextDocument doc, int position)
        {
            int level = 0;
            SetParagraph(doc, position, p =>
            {
                if ((int)p.ListType != tomListNone) level = Math.Max(0, (int)p.ListLevelIndex);
                else
                {
                    float left = p.LeftIndent;
                    if (left > 0 && left < 10000) level = (int)Math.Round(left / LevelStep);
                }
            });
            return Math.Min(level, MaxLevel);
        }

        /// <summary>The editor's lists have nine levels (0–8).</summary>
        private const int MaxLevel = 8;

        /// <summary>A bullet list type: the usual • (tomListBullet), or a bullet given by its character (above 32).</summary>
        private static bool IsBulletKind(int kind) => kind == tomListBullet || kind > 32;

        /// <summary>
        /// The live list type for a bullet at a level: • (tomListBullet) or the level's own
        /// character (◦, ▪…; RichEdit takes a list type above 32 as the bullet's character).
        /// </summary>
        private static int BulletKind(string first, int level)
        {
            string bullet = ListFormatter.BulletForLevel(first, level);
            return bullet == LiveBullet ? tomListBullet : char.ConvertToUtf32(bullet, 0);
        }

        /// <summary>
        /// Increase / decrease indent on live list items (rich text): each item in the selection
        /// goes down / up a level, taking that level's bullet (• ◦ ▪) or number style (1 → a → i,
        /// punctuation kept) and indents, so the list stays a hierarchy. Paragraphs that aren't
        /// list items are left for the caller. Returns how many items moved, and whether any list
        /// items were in the selection at all.
        /// </summary>
        public static (int Moved, bool AnyItems) ChangeListLevel(ClipXRichTextBox box, int delta, BulletOptions bullets)
        {
            int selStart = box.SelectionStart, selLength = box.SelectionLength;
            string text = box.Text;
            List<Line> all = ListFormatter.LinesIn(text, 0, text.Length);
            List<Line> lines = ListFormatter.LinesIn(text, selStart, selStart + selLength);
            int moved = 0;
            bool any = false;
            int[] cascade = { tomListNumberAsUCRoman, tomListNumberAsUCLetter, tomListNumberAsArabic, tomListNumberAsLCLetter, tomListNumberAsLCRoman };
            using (box.BeginBulkEdit())
            {
                box.EditDocument(doc =>
                {
                    // The start number of the list's items at a level, near a line: the editor takes
                    // an item with a different start as the beginning of a new list, so an item moving
                    // to a level must take that level's start (above it first, then below), else a
                    // sub-level starts at 1 and the top level keeps its own
                    int StartAt(int index, int level, int own)
                    {
                        foreach (int step in new[] { -1, 1 })
                            for (int i = index + step; i >= 0 && i < all.Count; i += step)
                            {
                                int type = 0, lvl = 0, start = 0;
                                SetParagraph(doc, all[i].Start, p => { type = p.ListType; lvl = p.ListLevelIndex; start = p.ListStart; });
                                if ((type & 0xFFFF) == tomListNone) break; // the list ends
                                if (lvl == level) return start;
                                if (lvl < level && level > 0) break;       // a sub-list starts again under each item
                            }
                        return level > 0 ? 1 : own;
                    }

                    foreach (Line line in lines)
                    {
                        int index = all.FindIndex(l => l.Start == line.Start);
                        SetParagraph(doc, line.Start, p =>
                        {
                            int type = p.ListType;
                            int kind = type & 0xFFFF, flags = type & ~0xFFFF;
                            if (kind == tomListNone) return;
                            any = true;
                            int level = Math.Max(0, (int)p.ListLevelIndex);
                            int newLevel = Math.Max(0, Math.Min(MaxLevel, level + delta));
                            if (newLevel == level) return;

                            int newKind;
                            if (IsBulletKind(kind)) newKind = BulletKind(bullets.Character, newLevel);
                            else
                            {
                                int at = Array.IndexOf(cascade, kind);
                                newKind = at < 0 ? kind : cascade[((at + newLevel - level) % cascade.Length + cascade.Length) % cascade.Length];
                            }
                            int newStart = StartAt(index, newLevel, (int)p.ListStart);
                            p.ListType = newKind | flags;
                            p.ListLevelIndex = newLevel;
                            p.ListStart = newStart;
                            p.ListTab = Hang;
                            p.SetIndents(-Hang, LevelStep * (newLevel + 1), 0f);
                            moved++;
                        });
                    }
                });
                box.Select(selStart, selLength);
            }
            return (moved, any);
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
