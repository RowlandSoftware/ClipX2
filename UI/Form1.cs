using ClipX2.TextEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipX2
{
    public partial class Form1 : Form
    {
        private bool _isRtfMode;
        public Form1()
        {
            InitializeComponent();
            _isRtfMode = false;
            // when load settings (example)
            //richTextBox1.PreciseSelection = userSettings.PreciseSelection;
            //richTextBox1.AutoWordSelectionEnabled = userSettings.AutoWordSelection;
            richTextBox1.IsRtfModeGetter = () => _isRtfMode;
            richTextBox1.TextSettled += (s, e) => UpdateWordCount();
            richTextBox1.SelectionSettled += (s, e) => ShowWordCount();

        }
        private void On_FormLoad(object sender, EventArgs e)
        {
            bool hasHtml = Clipboard.ContainsData(DataFormats.Html);
            bool hasRtf = Clipboard.ContainsData(DataFormats.Rtf);
            bool hasText = Clipboard.ContainsText(TextDataFormat.UnicodeText);

            using var dlg = new FormatChoiceDialog(hasHtml, hasRtf || hasText, hasRtf || hasText);

            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                _isRtfMode = false;
                return;
            }

            switch (dlg.Selected)
            {
                case FormatChoiceDialog.Choice.Html:
                    string rawHtml = Clipboard.GetData(DataFormats.Html) as string;
                    richTextBox1.Text = ExtractHtmlFragment(rawHtml);
                    _isRtfMode = false;
                    break;

                case FormatChoiceDialog.Choice.Rtf:
                    if (!hasRtf)
                    {
                        richTextBox1.Text = Clipboard.GetText(TextDataFormat.UnicodeText);
                    }
                    else
                    {
                        string rtf = Clipboard.GetData(DataFormats.Rtf) as string;
                        richTextBox1.Rtf = rtf;
                    }
                    _isRtfMode = true;
                    break;

                case FormatChoiceDialog.Choice.Text:
                    richTextBox1.Text = Clipboard.GetText(TextDataFormat.UnicodeText);
                    _isRtfMode = false;
                    break;
            }
            toolStripLabelRtfMode.Text = _isRtfMode ? toolStripLabelRtfMode.Text = "Rich text" : "Plain text";
        }
        private int _totalWords; // kept so a selection change needn't recount the whole text

        /// <summary>Recounts the whole text; called when editing pauses or a tool finishes.</summary>
        private void UpdateWordCount()
        {
            _totalWords = TextStatistics.CountWords(richTextBox1.Text);
            ShowWordCount();
        }

        /// <summary>
        /// Shows "n words", or "Selected: n of m words" when text is selected. Called when the
        /// selection settles; counting the selection only reads the selected text.
        /// </summary>
        private void ShowWordCount()
        {
            string total = _totalWords == 1 ? "1 word" : $"{_totalWords:N0} words";
            if (richTextBox1.SelectionLength == 0)
                toolStripLabelWordCount.Text = total;
            else
                toolStripLabelWordCount.Text = $"Selected: {TextStatistics.CountWords(richTextBox1.SelectedText):N0} of {total}";
        }

        private static string ExtractHtmlFragment(string htmlClipboardData)
        {
            if (string.IsNullOrEmpty(htmlClipboardData))
                return string.Empty;

            const string startTag = "<!--StartFragment-->";
            const string endTag = "<!--EndFragment-->";

            int start = htmlClipboardData.IndexOf(startTag);
            int end = htmlClipboardData.IndexOf(endTag);

            if (start < 0 || end < 0)
                return string.Empty;

            start += startTag.Length;
            return htmlClipboardData.Substring(start, end - start).Trim();
        }
        
        /*********** Toolbar clicks ****************/

        private void toolStripButtonUpperCase_Click(object sender, EventArgs e)
        {
            ApplyCase(CaseMode.Upper);

        }
        private void toolStripButtonLowerCase_Click(object sender, EventArgs e)
        {
            ApplyCase(CaseMode.Lower);
        }

        private void toolStripButtonReverseCase_Click(object sender, EventArgs e)
        {
            ApplyCase(CaseMode.Reverse);
        }
        private void toolStripButtonInitCaps_Click(object sender, EventArgs e)
        {
            ApplyCase(CaseMode.InitCaps);
        }
        private void toolStripButtonTitleCase_Click(object sender, EventArgs e)
        {
            ApplyCase(CaseMode.Title);
        }
        private void toolStripButtonSentenceCase_Click(object sender, EventArgs e)
        {
            ApplyCase(CaseMode.Sentence);
        }
        private void toolStripButtonCapitaliseSentences_Click(object sender, EventArgs e)
        {
            ApplyCase(CaseMode.SentenceCaps);
        }
        private void ApplyCase(CaseMode mode)
        {
            int selStart = richTextBox1.SelectionStart;
            int selLen = richTextBox1.SelectionLength;

            if (selLen == 0)
                richTextBox1.SelectAll();

            richTextBox1.ApplyCase(mode);

            richTextBox1.Select(selStart, selLen); // restore selection
        }

        private void toolStripLabelRtfMode_Click(object sender, EventArgs e)
        {
            _isRtfMode = !_isRtfMode;
            if (!_isRtfMode)
            {
                richTextBox1.Text = richTextBox1.Text;
            }
            toolStripLabelRtfMode.Text = _isRtfMode ? toolStripLabelRtfMode.Text = "Rich text" : "Plain text";
        }
    }
}
