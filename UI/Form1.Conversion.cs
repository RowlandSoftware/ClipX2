using System;
using System.Drawing;
using System.Windows.Forms;
using ClipX2.TextEngine.Export;
using ClipX2.TextEngine.Import;
using ClipX2.UI;

namespace ClipX2
{
    /// <summary>
    /// Plain text ⇄ rich text: at startup (whatever the clipboard holds, in the mode chosen), on
    /// switching mode, and (in ClipboardExport) on Done. Everything goes through a
    /// <see cref="FormattedDocument"/>: plain text and HTML are read into one (typed lists and
    /// indents recognised), rich text is read from the editor; it is then written into the editor
    /// as rich text (live lists, paragraph indents) or out as plain text (typed lists, tabs).
    /// </summary>
    public partial class Form1
    {
        /// <summary>The font plain text takes on becoming rich text (fonts from RTF or HTML are kept).</summary>
        internal static Font RichBaseFont() => AppSettings.Current.Display.ProportionalFont.ToFont();

        /// <summary>
        /// The HTML on the clipboard: its HTML format (the fragment copied), or, when there is only
        /// plain text, that text if it is HTML code (copied from a text editor). Null if neither.
        /// </summary>
        private string ClipboardHtml(bool hasRtf, bool hasText)
        {
            if (Clipboard.ContainsData(DataFormats.Html))
                return ExtractHtmlFragment(Clipboard.GetText(TextDataFormat.Html));
            if (!hasRtf && hasText)
            {
                string text = Clipboard.GetText(TextDataFormat.UnicodeText);
                if (HtmlReader.LooksLikeHtml(text)) return text;
            }
            return null;
        }

        /// <summary>
        /// Startup: loads the clipboard in the mode chosen, converting if that format isn't there
        /// (plain text copied to format it or make lists; rich text copied to strip it).
        /// Rich: RTF as it is; else HTML (<paramref name="html"/>: the clipboard's, or plain text
        /// that is HTML code), keeping its formatting; else plain text with its lists and indents
        /// made live. Plain: HTML converted (lists typed: browsers leave list numbers out of their
        /// own plain text); else the clipboard's own text; else RTF converted.
        /// </summary>
        private static void LoadClipboard(ClipXRichTextBox target, bool rich, string html)
        {
            bool hasRtf = Clipboard.ContainsData(DataFormats.Rtf);
            bool hasText = Clipboard.ContainsText(TextDataFormat.UnicodeText);
            AppSettings s = AppSettings.Current;
            if (rich)
            {
                if (hasRtf) target.Rtf = Clipboard.GetText(TextDataFormat.Rtf);
                else if (html != null) RichEditWriter.Write(target, HtmlReader.Read(html, s.BulletList, s.NumList), RichBaseFont());
                else if (hasText) RichEditWriter.Write(target, PlainTextReader.Read(Clipboard.GetText(TextDataFormat.UnicodeText), s.BulletList, s.NumList), RichBaseFont());
            }
            else
            {
                if (html != null) SetPlainText(target, DocumentExport.ToPlainText(HtmlReader.Read(html, s.BulletList, s.NumList), "\n"));
                else if (hasText) SetPlainText(target, Clipboard.GetText(TextDataFormat.UnicodeText));
                else if (hasRtf)
                {
                    target.Rtf = Clipboard.GetText(TextDataFormat.Rtf);
                    SetPlainText(target, DocumentExport.ToPlainText(RichEditReader.Read(target), "\n"));
                }
            }
        }

        /// <summary>
        /// What the clipboard holds, into <paramref name="target"/>: asks which format (when there
        /// is anything to choose from) and loads it, converting if need be. Returns rich (true) or
        /// plain (false) text, or null if the clipboard is empty or the user cancelled. Used at
        /// startup and by Refresh.
        /// </summary>
        private bool? LoadFromClipboard(ClipXRichTextBox target)
        {
            bool hasRtf = Clipboard.ContainsData(DataFormats.Rtf);
            bool hasText = Clipboard.ContainsText(TextDataFormat.UnicodeText);
            string html = ClipboardHtml(hasRtf, hasText); // the clipboard's HTML, or plain text that is HTML code
            if (html == null && !hasRtf && !hasText) return null;

            using var dlg = new FormatChoiceDialog(html != null, hasRtf, hasText);
            if (dlg.ShowDialog(this) != DialogResult.OK) return null;
            if (dlg.Selected == FormatChoiceDialog.Choice.Html)
            {
                SetPlainText(target, html); // the HTML itself, as code
                return false;
            }
            bool rich = dlg.Selected == FormatChoiceDialog.Choice.Rtf;
            LoadClipboard(target, rich, html);
            return rich;
        }

        /// <summary>
        /// Refresh (ClipX 1's cmdRefresh): the clipboard again, as at startup, replacing the text
        /// in one Undo step (Ctrl+Z gets back what was there). Built in a hidden editor first, so
        /// nothing changes if the user cancels.
        /// </summary>
        private void RefreshFromClipboard()
        {
            bool? rich;
            string rtf;
            using (var temp = new ClipXRichTextBox())
            {
                temp.CreateControl();
                IntPtr unused = temp.Handle; // TOM needs the window
                rich = LoadFromClipboard(temp);
                if (rich == null)
                {
                    if (!Clipboard.ContainsText() && !Clipboard.ContainsData(DataFormats.Rtf) && !Clipboard.ContainsData(DataFormats.Html))
                        ShowStatus("The clipboard is empty");
                    return;
                }
                rtf = temp.Rtf;
            }

            using (richTextBox1.BeginBulkEdit())
            {
                richTextBox1.SelectAll();
                richTextBox1.SelectedRtf = rtf; // undoable, unlike setting the text
                richTextBox1.Select(0, 0);
            }
            _isRtfMode = rich.Value;
            if (!_isRtfMode) RichEditWriter.ClearFormatting(richTextBox1); // no list or indent left on the last paragraph
            toolStripLabelRtfMode.Text = _isRtfMode ? "Rich text" : "Plain text";
            ApplyDisplaySettings();
            _panels.Current?.OnModeChanged();
            ShowStyles();
            ShowStatus("Text retrieved from clipboard");
        }

        /// <summary>Plain text into the editor, with no formatting left over from before.</summary>
        private void SetPlainText(string text) => SetPlainText(richTextBox1, text);

        private static void SetPlainText(ClipXRichTextBox target, string text)
        {
            using (target.BeginBulkEdit())
            {
                RichEditWriter.ClearFormatting(target);
                target.Text = text.Replace("\r\n", "\n");
            }
        }

        /// <summary>
        /// Switches between plain and rich text. To plain: live lists become typed ones, paragraph
        /// indents tabs; other formatting goes (after a warning, unless turned off). To rich: typed
        /// lists become live, leading tabs paragraph indents. Returns false if the user cancelled.
        /// </summary>
        private bool SwitchMode()
        {
            AppSettings s = AppSettings.Current;
            if (_isRtfMode)
            {
                if (richTextBox1.TextLength > 0 && !s.HideFormattingLossWarning && !ConfirmFormattingLoss()) return false;
                string plain = DocumentExport.ToPlainText(RichEditReader.Read(richTextBox1), "\n");
                _isRtfMode = false;
                SetPlainText(plain);
            }
            else
            {
                FormattedDocument doc = PlainTextReader.Read(richTextBox1.Text, s.BulletList, s.NumList);
                _isRtfMode = true;
                RichEditWriter.Write(richTextBox1, doc, RichBaseFont());
            }
            return true;
        }

        /// <summary>
        /// "Formatting may be lost" before switching to plain text, with "Do not show this warning
        /// again" (also in Options, General). False if the user chose Cancel.
        /// </summary>
        private bool ConfirmFormattingLoss()
        {
            var page = new TaskDialogPage
            {
                Caption = "Switch to plain text",
                Heading = "Formatting may be lost",
                Text = "Plain text has no fonts, sizes, bold, italic or colours. Lists are kept as typed bullets and numbers, and indents as tabs.",
                Icon = TaskDialogIcon.Warning,
                Verification = new TaskDialogVerificationCheckBox("Do not show this warning again"),
                Buttons = { TaskDialogButton.OK, TaskDialogButton.Cancel },
                DefaultButton = TaskDialogButton.OK,
            };
            TaskDialogButton result = TaskDialog.ShowDialog(this, page);
            if (page.Verification.Checked && result == TaskDialogButton.OK)
            {
                AppSettings.Current.HideFormattingLossWarning = true;
                AppSettings.Current.Save();
            }
            return result == TaskDialogButton.OK;
        }
    }
}
