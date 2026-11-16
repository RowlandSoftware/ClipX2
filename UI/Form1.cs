using ClipX2.TextEngine;
using System;
using ClipX2.TextEngine.Spelling;
using ClipX2.UI;
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
            toolStripButtonDone.Click += (s, e) => Done();
            toolStripButtonCheckPunctuation.Click += (s, e) => CheckPunctuation();
            toolStripButtonSpellcheck.Click += (s, e) => SpellNext();

            // The side panel: tool views, or button descriptions on hover (in place of tooltips)
            var statusHint = new ToolStripLabel { Name = "toolStripLabelHint" };
            toolStrip3.Items.Add(statusHint);
            _panels = new ClipX2.UI.Panels.SidePanelHost(panel1, statusHint);
            _panels.AttachHoverHelp(this);

            _spellLoading = LoadSpellChecker(ResolveLanguage(AppSettings.Current.Spelling.Language)); // in the background, so the first F7 is instant
        }

        private readonly ClipX2.UI.Panels.SidePanelHost _panels;
        private void On_FormLoad(object sender, EventArgs e)
        {
            bool hasHtml = Clipboard.ContainsData(DataFormats.Html);
            bool hasRtf = Clipboard.ContainsData(DataFormats.Rtf);
            bool hasText = Clipboard.ContainsText(TextDataFormat.UnicodeText);

            using var dlg = new FormatChoiceDialog(hasHtml, hasRtf || hasText, hasRtf || hasText);

            if (!hasHtml && !hasRtf && !hasText)
            {
                _isRtfMode = true;
            }
            else
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    _isRtfMode = false;
                    return;
                }

                switch (dlg.Selected)
                {
                    case FormatChoiceDialog.Choice.Html:
                        string rawHtml = Clipboard.GetText(TextDataFormat.Html);
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
                            string rtf = Clipboard.GetText(TextDataFormat.Rtf);
                            richTextBox1.Rtf = rtf;
                        }
                        _isRtfMode = true;
                        break;

                    case FormatChoiceDialog.Choice.Text:
                        richTextBox1.Text = Clipboard.GetText(TextDataFormat.UnicodeText);
                        _isRtfMode = false;
                        break;
                }
            }
            toolStripLabelRtfMode.Text = _isRtfMode ? toolStripLabelRtfMode.Text = "Rich text" : "Plain text";
        }
        /// <summary>
        /// Done: puts the text on the clipboard, ready to paste back into the application it came
        /// from, and closes ClipX. In rich-text mode it goes on as RTF, HTML and plain text.
        /// </summary>
        private void Done()
        {
            var data = ClipX2.UI.ClipboardExport.Create(richTextBox1, _isRtfMode);
            // copy: true so the data stays on the clipboard after ClipX closes; retry if another
            // program has the clipboard open
            Clipboard.SetDataObject(data, true, 10, 100);
            Close();
        }

        /// <summary>Form-wide shortcuts, handled before the text box sees them.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Alt | Keys.D:                   // Done (as ClipX 1)
                case Keys.Control | Keys.Shift | Keys.C:  // Done (ClipX 1's alternative)
                    Done();
                    return true;
                case Keys.F7:                             // next spelling issue (as Word)
                    SpellNext();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
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
        /// <summary>
        /// Check Punctuation (ClipX 1's doCheckPunc): the selected paragraphs, or the whole text.
        /// Corrections are applied in place (formatting kept, one Undo step); then any problems,
        /// and the places where the checker had to guess, are listed.
        /// </summary>
        private void CheckPunctuation()
        {
            var box = richTextBox1;
            bool hadSelection = box.SelectionLength > 0;
            string text = box.Text;
            PunctuationCheckResult result = hadSelection
                ? PunctuationChecker.Check(text, box.SelectionStart, box.SelectionLength)
                : PunctuationChecker.Check(text);

            box.ApplyEdits(result.Edits);
            if (hadSelection) box.Select(result.Start, result.Text.Length); // the whole paragraphs checked

            if (result.Problems.Count == 0 && result.Guesses.Count == 0) return;

            const int maxListed = 12;
            var sb = new StringBuilder();
            sb.AppendLine(result.Edits.Count == 1 ? "1 correction made." : $"{result.Edits.Count} corrections made.");
            void List(string heading, IReadOnlyList<PunctuationNote> notes)
            {
                if (notes.Count == 0) return;
                sb.AppendLine().AppendLine(heading);
                foreach (PunctuationNote note in notes.Take(maxListed)) sb.AppendLine("• " + note);
                if (notes.Count > maxListed) sb.AppendLine($"…and {notes.Count - maxListed} more.");
            }
            List("Please check:", result.Problems);
            List("Places where ClipX had to guess:", result.Guesses);

            MessageBox.Show(this, sb.ToString(), "Check punctuation",
                MessageBoxButtons.OK, result.Problems.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        /*********** Spelling ****************/

        /// <summary>Dictionaries shipped with the program (may be read-only, under Program Files).</summary>
        private static readonly string ProgramDictionaries = System.IO.Path.Combine(AppContext.BaseDirectory, "Dictionaries");
        /// <summary>The user's own (and, later, downloaded) dictionaries; these win over shipped ones of the same name.</summary>
        private static readonly string UserDictionaries = System.IO.Path.Combine(AppSettings.Folder, "Dictionaries");
        private static readonly string[] DictionaryFolders = { UserDictionaries, ProgramDictionaries };

        private Task<SpellChecker> _spellLoading;
        private PersonalDictionary _personal;

        /// <summary>Every dictionary in either folder, by name (en_GB, fr_FR…).</summary>
        private static IReadOnlyList<string> SpellingLanguages() =>
            DictionaryFolders.SelectMany(SpellingDictionary.AvailableLocales)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(l => l, StringComparer.OrdinalIgnoreCase)
                .ToList();

        /// <summary>The language to use: the one asked for if present, else en_GB, else any.</summary>
        private static string ResolveLanguage(string wanted)
        {
            IReadOnlyList<string> all = SpellingLanguages();
            return all.FirstOrDefault(l => string.Equals(l, wanted, StringComparison.OrdinalIgnoreCase))
                ?? all.FirstOrDefault(l => l == "en_GB")
                ?? all.FirstOrDefault()
                ?? wanted;
        }

        /// <summary>
        /// Loads a Hunspell dictionary (the user's folder first, then the program's), with the
        /// personal dictionary from %AppData%\ClipX2 (the first time, that is copied from the
        /// persdict.dat shipped in Dictionaries, i.e. ClipX 1's word list), and the spelling
        /// options from the settings.
        /// </summary>
        private Task<SpellChecker> LoadSpellChecker(string locale) => Task.Run(() =>
        {
            if (_personal == null)
            {
                string personalPath = System.IO.Path.Combine(AppSettings.Folder, "persdict.dat");
                string seed = System.IO.Path.Combine(ProgramDictionaries, "persdict.dat");
                if (!System.IO.File.Exists(personalPath) && System.IO.File.Exists(seed))
                {
                    System.IO.Directory.CreateDirectory(AppSettings.Folder);
                    System.IO.File.Copy(seed, personalPath);
                }
                _personal = new PersonalDictionary(personalPath);
            }
            string folder = DictionaryFolders.FirstOrDefault(f =>
                System.IO.File.Exists(System.IO.Path.Combine(f, locale + ".dic"))) ?? ProgramDictionaries;
            SpellingSettings s = AppSettings.Current.Spelling;
            var options = new SpellingOptions
            {
                IgnoreUppercase = s.IgnoreUppercase,
                IgnoreWordsWithNumbers = s.IgnoreWordsWithNumbers,
                IgnoreAddresses = s.IgnoreAddresses,
                CheckRepeatedWords = s.CheckRepeatedWords,
            };
            return new SpellChecker(SpellingDictionary.Load(folder, locale), _personal, options);
        });

        /// <summary>
        /// F7 / Spelling button: opens the spelling panel and checks from the caret (or the
        /// selection). Pressed again while a word is shown, it skips that word.
        /// </summary>
        private async void SpellNext()
        {
            var panel = _panels.Show<ClipX2.UI.Panels.SpellingPanel>();
            if (!panel.IsAttached)
                panel.Attach(richTextBox1,
                    locale => _spellLoading = LoadSpellChecker(locale),
                    SpellingLanguages, AppSettings.Current.Spelling, MoreSpellingLanguages);
            await panel.StartCheck(_spellLoading);
        }

        /// <summary>
        /// Placeholder until the Options dialog can download dictionaries: says where to put
        /// Hunspell files by hand, and offers to open that folder.
        /// </summary>
        private void MoreSpellingLanguages()
        {
            string message =
                "Downloading dictionaries will come with the Options dialog.\n\n" +
                "For now, you can add a Hunspell dictionary yourself: put its two files (for example " +
                "fr_FR.aff and fr_FR.dic) in\n\n" + UserDictionaries + "\n\n" +
                "LibreOffice's dictionaries are suitable. Open the folder now?";
            if (MessageBox.Show(this, message, "More languages", MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes)
                return;
            try
            {
                System.IO.Directory.CreateDirectory(UserDictionaries);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(UserDictionaries) { UseShellExecute = true });
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is UnauthorizedAccessException || ex is System.ComponentModel.Win32Exception)
            {
                MessageBox.Show(this, "The folder could not be opened: " + ex.Message, "More languages");
            }
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
