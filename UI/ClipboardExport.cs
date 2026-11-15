using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ClipX2.TextEngine.Export;

namespace ClipX2.UI
{
    /// <summary>
    /// Builds what the Done button puts on the clipboard. In rich-text mode: RTF (for Word,
    /// LibreOffice, Outlook), HTML (for browsers and web apps such as Gmail and Google Docs, which
    /// don't read RTF) and plain text (for everything else). In plain-text mode: plain text only.
    /// The application you paste into chooses the richest format it understands.
    /// </summary>
    public static class ClipboardExport
    {
        public static DataObject Create(RichTextBox rtb, bool richText)
        {
            var data = new DataObject();
            if (!richText)
            {
                data.SetData(DataFormats.UnicodeText, ToCrLf(rtb.Text));
                return data;
            }

            FormattedDocument doc = RichEditReader.Read(rtb);
            data.SetData(DataFormats.Rtf, rtb.Rtf);
            data.SetData(DataFormats.Html, ToClipboardHtml(DocumentExport.ToHtml(doc)));
            data.SetData(DataFormats.UnicodeText, DocumentExport.ToPlainText(doc));
            return data;
        }

        private static string ToCrLf(string text) => text.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "\r\n");

        /// <summary>
        /// Wraps an HTML fragment in the Windows "HTML Format" (CF_HTML) envelope: a header giving
        /// the byte offsets (in UTF-8) of the document and of the fragment between the
        /// StartFragment/EndFragment comments.
        /// </summary>
        public static string ToClipboardHtml(string fragment)
        {
            const string header =
                "Version:0.9\r\nStartHTML:{0:D10}\r\nEndHTML:{1:D10}\r\nStartFragment:{2:D10}\r\nEndFragment:{3:D10}\r\n";
            const string before = "<html><head><meta charset=\"utf-8\"></head><body>\r\n<!--StartFragment-->";
            const string after = "<!--EndFragment-->\r\n</body></html>";

            int headerLength = string.Format(header, 0, 0, 0, 0).Length; // the header is ASCII
            int startHtml = headerLength;
            int startFragment = startHtml + Encoding.UTF8.GetByteCount(before);
            int endFragment = startFragment + Encoding.UTF8.GetByteCount(fragment);
            int endHtml = endFragment + Encoding.UTF8.GetByteCount(after);

            return string.Format(header, startHtml, endHtml, startFragment, endFragment) + before + fragment + after;
        }
    }

    /// <summary>Reads a RichTextBox's paragraphs and formatting runs into a <see cref="FormattedDocument"/>, via TOM.</summary>
    public static class RichEditReader
    {
        private const int tomParagraph = 4;
        private const int tomTrue = -1;
        private const int tomUndefined = -9999999;
        private const int tomAutoColor = -9999997;

        public static FormattedDocument Read(RichTextBox rtb)
        {
            ITextDocument tom = null;
            try { tom = RichEditTom.GetDocument(rtb); } catch (Exception) { /* TOM not available */ }
            if (tom == null) return ReadPlain(rtb.Text);

            var result = new FormattedDocument();
            ITextRange para = tom.Range(0, 0);
            ITextRange run = tom.Range(0, 0);
            try
            {
                int storyLength = para.StoryLength;
                int pos = 0;
                while (pos < storyLength)
                {
                    para.SetRange(pos, pos);
                    para.Expand(tomParagraph);
                    int paraEnd = Math.Max(para.End, pos + 1);

                    var p = new FormattedParagraph();
                    ReadParagraphFormat(para, p);

                    // Runs, up to (not including) the paragraph mark
                    int contentEnd = paraEnd - 1;
                    int runPos = pos;
                    while (runPos < contentEnd)
                    {
                        run.SetRange(runPos, runPos);
                        run.MoveEnd(RichEditTom.tomCharFormat, 1);
                        int runEnd = Math.Min(Math.Max(run.End, runPos + 1), contentEnd);
                        run.SetRange(runPos, runEnd);

                        FormattedRun r = ReadRun(run);
                        if (r != null) p.Runs.Add(r);
                        runPos = runEnd;
                    }

                    result.Paragraphs.Add(p);
                    pos = paraEnd;
                }
            }
            finally
            {
                Marshal.ReleaseComObject(run);
                Marshal.ReleaseComObject(para);
                Marshal.ReleaseComObject(tom);
            }
            return result;
        }

        private static void ReadParagraphFormat(ITextRange para, FormattedParagraph p)
        {
            object pf = para.Para;
            try
            {
                dynamic d = pf;
                int listType = d.ListType;
                if (listType != tomUndefined)
                {
                    p.ListKind = ToListKind(listType & 0xFFFF);
                    p.ListNumberStyle = ToNumberStyle(listType & 0xF0000);
                }
                if (p.ListKind != ListKind.None)
                {
                    int start = d.ListStart;
                    p.ListStart = start > 0 ? start : 1;
                    int level = d.ListLevelIndex; // 0 = outermost (written to RTF as \ilvl)
                    p.ListLevel = Math.Max(0, level);
                }

                switch ((int)d.Alignment)
                {
                    case 1: p.Alignment = ParagraphAlignment.Centre; break;
                    case 2: p.Alignment = ParagraphAlignment.Right; break;
                    case 3: p.Alignment = ParagraphAlignment.Justify; break;
                }
                float left = d.LeftIndent, first = d.FirstLineIndent;
                if (left > tomUndefined) p.LeftIndent = left;
                if (first > tomUndefined) p.FirstLineIndent = first;
            }
            catch (Exception) { /* keep whatever was read */ }
            finally
            {
                Marshal.ReleaseComObject(pf);
            }
        }

        private static FormattedRun ReadRun(ITextRange range)
        {
            string text = range.Text ?? "";
            text = text.Replace("￼", ""); // embedded objects (pictures) can't be exported as text
            if (text.Length == 0) return null;

            var r = new FormattedRun { Text = text };
            object font = range.Font;
            try
            {
                dynamic f = font;
                if ((int)f.Hidden == tomTrue) return null;

                r.Bold = (int)f.Bold == tomTrue;
                r.Italic = (int)f.Italic == tomTrue;
                int underline = f.Underline;
                r.Underline = underline != 0 && underline != tomUndefined;
                r.Strikethrough = (int)f.StrikeThrough == tomTrue;
                r.Superscript = (int)f.Superscript == tomTrue;
                r.Subscript = (int)f.Subscript == tomTrue;
                r.FontName = f.Name;
                float size = f.Size;
                if (size > 0) r.FontSize = size;
                r.Colour = ToRgb(f.ForeColor);
                r.Highlight = ToRgb(f.BackColor);
            }
            catch (Exception) { /* keep whatever was read */ }
            finally
            {
                Marshal.ReleaseComObject(font);
            }
            return r;
        }

        /// <summary>COLORREF (0x00BBGGRR) to 0xRRGGBB; null for automatic or undefined.</summary>
        private static int? ToRgb(int colorref)
        {
            if (colorref == tomAutoColor || colorref == tomUndefined || colorref < 0) return null;
            return ((colorref & 0xFF) << 16) | (colorref & 0xFF00) | ((colorref >> 16) & 0xFF);
        }

        private static ListKind ToListKind(int type)
        {
            switch (type)
            {
                case 0: return ListKind.None;
                case 1: return ListKind.Bullet;
                case 3: return ListKind.LowerLetter;
                case 4: return ListKind.UpperLetter;
                case 5: return ListKind.LowerRoman;
                case 6: return ListKind.UpperRoman;
                default: return ListKind.Arabic; // 2 = arabic; other numbering schemes fall back to 1, 2, 3
            }
        }

        private static ListNumberStyle ToNumberStyle(int style)
        {
            switch (style)
            {
                case 0x10000: return ListNumberStyle.Parentheses;
                case 0x20000: return ListNumberStyle.Period;
                case 0x30000: return ListNumberStyle.Plain;
                case 0x40000: return ListNumberStyle.NoNumber;
                default: return ListNumberStyle.RightParenthesis;
            }
        }

        private static FormattedDocument ReadPlain(string text)
        {
            var doc = new FormattedDocument();
            foreach (string line in text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
            {
                var p = new FormattedParagraph();
                p.Runs.Add(new FormattedRun { Text = line });
                doc.Paragraphs.Add(p);
            }
            return doc;
        }
    }
}
