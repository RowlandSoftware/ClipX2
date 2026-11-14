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
    public enum TokenType
    {
        Word,               // letters/digits with optional internal apostrophes
        DottedAbbrev,       // dot-separated letter sequences (i.e., e.g., U.S.A., Ph.D.)
        Acronym,            // Common acronyms written all caps: NASA, GCSE, USA
        Filename,           // word + dot + extension (report.doc, README.TXT)
        Number,             // digits with optional punctuation (1,234.56)
        Punctuation,        // .,?!;: etc.
        Whitespace,         // spaces, tabs, newlines
        Other               // symbols, emojis, anything not above
    }
    [Flags]
    public enum TokenFlags
    {
        None = 0,
        PrecededByHyphen = 1 << 0,      // token follows a hyphen
        PrecededByApostrophe = 1 << 1,  // token follows an apostrophe (rare but useful)
        AllCaps = 1 << 2,               // token is entirely uppercase
        FirstInSentence = 1 << 3,       // token begins a sentence
        ContainsDigits = 1 << 4,        // token includes one or more digits
        Abbreviation = 1 << 5           // 'Word abbrev' e.g. Q&A
    }

    public readonly struct Token
    {
        public int Start { get; }
        public int Length { get; }
        public TokenType Type { get; }
        public TokenFlags Flags { get; }
        public Token(int start, int length, TokenType type, TokenFlags flags)
        {
            Start = start;
            Length = length;
            Type = type;
            Flags = flags;
        }
    }
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

        }
        private void On_FormLoad(object sender, EventArgs e)
        {
            bool hasHtml = Clipboard.ContainsData(DataFormats.Html);
            bool hasRtf = Clipboard.ContainsData(DataFormats.Rtf);
            bool hasText = Clipboard.ContainsText(TextDataFormat.UnicodeText);

            using var dlg = new FormatChoiceDialog(hasHtml, hasRtf, hasText);

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
                    string rtf = Clipboard.GetData(DataFormats.Rtf) as string;
                    richTextBox1.Rtf = rtf;
                    _isRtfMode = true;
                    break;

                case FormatChoiceDialog.Choice.Text:
                    richTextBox1.Text = Clipboard.GetText(TextDataFormat.UnicodeText);
                    _isRtfMode = false;
                    break;
            }
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
            ApplyCase(CaseTransformer.Mode.Upper);

        }
        private void toolStripButtonLowerCase_Click(object sender, EventArgs e)
        {
            ApplyCase(CaseTransformer.Mode.Lower);
        }

        private void toolStripButtonReverseCase_Click(object sender, EventArgs e)
        {
            ApplyCase(CaseTransformer.Mode.Reverse);
        }
        private void toolStripButtonInitCaps_Click(object sender, EventArgs e)
        {
            ApplyCase(CaseTransformer.Mode.InitCaps);
        }
        private void toolStripButtonTitleCase_Click(object sender, EventArgs e)
        {
            ApplyCase(CaseTransformer.Mode.Title);
        }
        private void toolStripButtonSentenceCase_Click(object sender, EventArgs e)
        {
            ApplyCase(CaseTransformer.Mode.Sentence);
        }
        private void toolStripButtonCapitaliseSentences_Click(object sender, EventArgs e)
        {
            ApplyCase(CaseTransformer.Mode.SentenceCaps);
        }
        private void ApplyCase(CaseTransformer.Mode mode)
        {
            int selStart = richTextBox1.SelectionStart;
            int selLen = richTextBox1.SelectionLength;

            if (selLen == 0)
                richTextBox1.SelectAll();

            CaseTransformer.Apply(richTextBox1, mode, _isRtfMode);

            richTextBox1.Select(selStart, selLen); // restore selection
        }        
    }
}
