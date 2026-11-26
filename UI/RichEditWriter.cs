using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ClipX2.TextEngine.Export;

namespace ClipX2.UI
{
    /// <summary>
    /// Writes a <see cref="FormattedDocument"/> into the editor as rich text (the reverse of
    /// <see cref="RichEditReader"/>): the text, then each paragraph's list (a live list: items
    /// renumber themselves, levels nest) or indent and alignment, then each run's formatting,
    /// through TOM. Used for plain text and HTML going into rich text (at startup and on switching
    /// to rich text) and for Done in plain-text mode (RTF made from the recognised lists).
    /// </summary>
    internal static class RichEditWriter
    {
        private const int tomParagraph = 4, tomTrue = -1, tomFalse = 0;
        private const int tomListNoNumber = 0x40000;
        private const float LevelStep = 36f, Hang = 18f; // as ListTools' live lists

        public static void Write(ClipXRichTextBox box, FormattedDocument doc, Font baseFont)
        {
            string text = string.Join("\n", doc.Paragraphs.Select(p => string.Concat(p.Runs.Select(r => r.Text))));
            using (box.BeginBulkEdit())
            {
                ClearFormatting(box);
                box.Text = text;
                box.SelectAll();
                if (baseFont != null) box.SelectionFont = baseFont;
                box.Select(0, 0);

                box.EditDocument(tom =>
                {
                    int pos = 0;
                    ITextRange range = tom.Range(0, 0);
                    try
                    {
                        foreach (FormattedParagraph p in doc.Paragraphs)
                        {
                            int length = p.Runs.Sum(r => r.Text.Length);
                            range.SetRange(pos, pos);
                            range.Expand(tomParagraph);
                            WriteParagraph(range, p);

                            int at = pos;
                            foreach (FormattedRun run in p.Runs)
                            {
                                if (run.Text.Length > 0 && HasFormatting(run))
                                {
                                    range.SetRange(at, at + run.Text.Length);
                                    WriteRun(range, run);
                                }
                                at += run.Text.Length;
                            }
                            pos += length + 1;
                        }
                    }
                    finally { Marshal.ReleaseComObject(range); }
                });
                box.Select(0, 0);
            }
        }

        /// <summary>
        /// After the editor has read RTF itself: bullets on deeper levels get their level's bullet
        /// (◦, ▪ … as ClipX's own lists, demoting, and typed lists made live have them). The
        /// editor's RTF reader drops a level's bullet character, even from RTF it wrote itself, and
        /// shows • on every level. A bullet character it did keep is left alone.
        /// <paramref name="read"/>: the editor reading the RTF (a paste), done first, so that and
        /// this are one Undo step.
        /// </summary>
        public static void BulletsByLevel(ClipXRichTextBox box, Action read = null)
        {
            box.EditDocument(tom =>
            {
                read?.Invoke();
                string text = box.Text;
                if (text.Length == 0) return;
                foreach (var line in ClipX2.TextEngine.ListFormatter.LinesIn(text, 0, text.Length))
                {
                    ITextRange range = tom.Range(line.Start, line.Start);
                    object para = range.Para;
                    try
                    {
                        dynamic d = para;
                        int type = d.ListType;
                        int level = d.ListLevelIndex;
                        if ((type & 0xFFFF) != 1 || level <= 0) continue; // not a plain • bullet on a deeper level
                        string bullet = ClipX2.TextEngine.ListFormatter.BulletForLevel("•", level);
                        if (bullet == "•") continue;
                        d.ListType = (type & ~0xFFFF) | char.ConvertToUtf32(bullet, 0);
                        range.Para = para;
                    }
                    catch (Exception) { /* leave this paragraph as it is */ }
                    finally
                    {
                        Marshal.ReleaseComObject(para);
                        Marshal.ReleaseComObject(range);
                    }
                }
            });
        }

        /// <summary>
        /// Takes all paragraph formatting (lists, indents, alignment) and character formatting off,
        /// so the editor can take plain text without any of it lingering.
        /// </summary>
        public static void ClearFormatting(ClipXRichTextBox box)
        {
            box.EditDocument(tom =>
            {
                ITextRange all = tom.Range(0, box.TextLength + 1);
                object para = all.Para;
                try
                {
                    dynamic p = para;
                    p.ListType = 0;
                    p.SetIndents(0f, 0f, 0f);
                    p.Alignment = 0;
                    all.Para = para;
                }
                catch (Exception) { /* not available: the text is still replaced */ }
                finally
                {
                    Marshal.ReleaseComObject(para);
                    Marshal.ReleaseComObject(all);
                }
            });
        }

        private static void WriteParagraph(ITextRange range, FormattedParagraph p)
        {
            object para = range.Para;
            try
            {
                dynamic d = para;
                if (p.HeadingLevel >= 1 && p.HeadingLevel <= 9) d.Style = -1 - p.HeadingLevel; // tomStyleHeading1 = -2 …
                d.Alignment = p.Alignment switch
                {
                    ParagraphAlignment.Centre => 1,
                    ParagraphAlignment.Right => 2,
                    ParagraphAlignment.Justify => 3,
                    _ => 0,
                };
                if (p.ListKind != ListKind.None)
                {
                    int kind = p.ListKind switch
                    {
                        ListKind.Bullet => string.IsNullOrEmpty(p.BulletCharacter) || p.BulletCharacter == "•" ? 1 : char.ConvertToUtf32(p.BulletCharacter, 0),
                        ListKind.LowerLetter => 3,
                        ListKind.UpperLetter => 4,
                        ListKind.LowerRoman => 5,
                        ListKind.UpperRoman => 6,
                        _ => 2,
                    };
                    int punctuation = p.ListNumberStyle switch
                    {
                        ListNumberStyle.Parentheses => 0x10000,
                        ListNumberStyle.Period => 0x20000,
                        ListNumberStyle.Plain => 0x30000,
                        ListNumberStyle.NoNumber => tomListNoNumber,
                        _ => 0,
                    };
                    int level = Math.Max(0, Math.Min(8, p.ListLevel));
                    d.ListType = kind | (p.ListKind == ListKind.Bullet ? 0 : punctuation);
                    d.ListLevelIndex = level;
                    d.ListStart = Math.Max(1, p.ListStart);
                    d.ListTab = Hang;
                    d.SetIndents(-Hang, LevelStep * (level + 1) + Math.Max(0, p.LeftIndent), 0f);
                }
                else if (p.LeftIndent != 0 || p.FirstLineIndent != 0)
                    d.SetIndents(p.FirstLineIndent, Math.Max(0, p.LeftIndent), 0f);
                if (p.StyleName != null)
                {
                    // A word processor's spacing exactly (not the editor's heading spacing)
                    d.SpaceBefore = p.SpaceBefore;
                    d.SpaceAfter = p.SpaceAfter;
                    LineSpacing ls = LineSpacing.Normalise(p.LineSpacing);
                    d.SetLineSpacing((int)ls.Rule, ls.Value);
                }
                else
                {
                    if (p.SpaceBefore > 0) d.SpaceBefore = p.SpaceBefore;
                    if (p.SpaceAfter > 0) d.SpaceAfter = p.SpaceAfter;
                }
                range.Para = para;
            }
            catch (Exception) { /* a paragraph format the editor can't take: the text is still there */ }
            finally { Marshal.ReleaseComObject(para); }
        }

        private static bool HasFormatting(FormattedRun r) =>
            r.Bold || r.Italic || r.Underline || r.Strikethrough || r.Superscript || r.Subscript
            || !string.IsNullOrEmpty(r.FontName) || r.FontSize > 0 || r.Colour.HasValue || r.Highlight.HasValue || r.Language > 0;

        private static void WriteRun(ITextRange range, FormattedRun r)
        {
            object font = range.Font;
            try
            {
                dynamic f = font;
                if (r.Bold) f.Bold = tomTrue;
                if (r.Italic) f.Italic = tomTrue;
                if (r.Underline) f.Underline = 1; // single
                if (r.Strikethrough) f.StrikeThrough = tomTrue;
                if (r.Superscript) f.Superscript = tomTrue;
                else if (r.Subscript) f.Subscript = tomTrue;
                if (!string.IsNullOrEmpty(r.FontName)) f.Name = r.FontName;
                if (r.FontSize > 0) f.Size = r.FontSize;
                if (r.Colour.HasValue) f.ForeColor = ToColorRef(r.Colour.Value);
                if (r.Highlight.HasValue) f.BackColor = ToColorRef(r.Highlight.Value);
                if (r.Language > 0) f.LanguageID = r.Language;
                range.Font = font;
            }
            catch (Exception) { /* keep the text, without this formatting */ }
            finally { Marshal.ReleaseComObject(font); }
        }

        /// <summary>0xRRGGBB to the editor's COLORREF (0x00BBGGRR).</summary>
        private static int ToColorRef(int rgb) => ((rgb & 0xFF) << 16) | (rgb & 0xFF00) | ((rgb >> 16) & 0xFF);
    }
}
