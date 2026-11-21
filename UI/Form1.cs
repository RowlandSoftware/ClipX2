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
            toolStripButtonFinish.Click += (s, e) => Done();
            toolStripButtonRefresh.Click += (s, e) => RunTool("clipxREF", ToolTrigger.Click); // start again from the clipboard

            // The side panel: tool views, or button descriptions on hover (in place of tooltips)
            // Status messages (as ClipX 1's labStatus) and descriptions while a panel is open go in
            // the bottom bar's status label, which fills the space between the search and Finish
            var statusHint = toolStripLabelStatus;
            toolStrip3.LayoutCompleted += (s, e) => FitStatusLabel();
            _quickSearch = new QuickSearch(richTextBox1, toolStripComboBox1,
                toolStripCheckBoxMatchCase, toolStripCheckBoxWholeWords, toolStripButtonFindAll,
                toolStripButtonFindPrevious, toolStripButtonFindNext, ShowStatus);
            _panels = new ClipX2.UI.Panels.SidePanelHost(panel1, statusHint);
            _panels.AttachHoverHelp(this);

            _spellLoading = LoadSpellChecker(ResolveLanguage(AppSettings.Current.Spelling.Language)); // in the background, so the first F7 is instant

            toolStripButtonOptions.MouseUp += OptionsButtonMouseUp;
            ApplyDisplaySettings();

            _statusHint = statusHint;

            // Every tool button, found by its tag (ClipX 1's token), goes to the one tool handler, RunTool
            WireToolButtons();
            InitStyles();
            InitFind();   // "What do you want to do?"
        }

        private readonly ToolStripLabel _statusHint;
        private readonly QuickSearch _quickSearch;

        /// <summary>
        /// Widens the status label to fill the bottom bar between the left-hand items (search) and
        /// the right-hand ones (Finish), so a long message doesn't push items into the overflow.
        /// </summary>
        private void FitStatusLabel()
        {
            ToolStripLabel label = toolStripLabelStatus;
            int used = 0;
            foreach (ToolStripItem item in toolStrip3.Items)
                if (item != label && item.Available) used += item.Width + item.Margin.Horizontal;
            int width = Math.Max(0, toolStrip3.DisplayRectangle.Width - used - label.Margin.Horizontal - 1);
            if (label.Width != width) label.Width = width; // lays out again, then unchanged
        }

        /// <summary>A short message in the status bar (as ClipX 1's labStatus).</summary>
        private void ShowStatus(string message)
        {
            if (_statusHint != null) _statusHint.Text = message ?? "";
        }

        private readonly ClipX2.UI.Panels.SidePanelHost _panels;
        private void On_FormLoad(object sender, EventArgs e)
        {
            bool empty = !Clipboard.ContainsText(TextDataFormat.UnicodeText) && !Clipboard.ContainsData(DataFormats.Rtf) && !Clipboard.ContainsData(DataFormats.Html);
            if (empty)
            {
                _isRtfMode = true;
            }
            else
            {
                // Plain or rich, whatever the clipboard holds: it is converted if need be (Form1.Conversion.cs)
                bool? rich = LoadFromClipboard(richTextBox1);
                if (rich == null)
                {
                    _isRtfMode = false;
                    return;
                }
                _isRtfMode = rich.Value;
                richTextBox1.ClearUndo(); // the first Undo shouldn't empty the text
            }
            toolStripLabelRtfMode.Text = _isRtfMode ? toolStripLabelRtfMode.Text = "Rich text" : "Plain text";
            ApplyDisplaySettings(); // plain text in the chosen font
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
                // Tools by key go through the tool handler, like the buttons
                case Keys.F7:                             // next spelling issue (as Word)
                    RunTool("chSPE", ToolTrigger.Key);
                    return true;
                case Keys.Alt | Keys.Up when richTextBox1.Focused:   // Nudge up (as ClipX 1); elsewhere Alt+Up/Down open drop-down lists
                    RunTool("soNUP", ToolTrigger.Key);
                    return true;
                case Keys.Alt | Keys.Down when richTextBox1.Focused: // Nudge down
                    RunTool("soNUD", ToolTrigger.Key);
                    return true;
                // Tab / Shift+Tab at the start of a line (or over several lines) indent / outdent; else a tab is typed
                case Keys.Tab when richTextBox1.Focused && TabIndents():
                    RunTool("stIIN", ToolTrigger.Key);
                    return true;
                case Keys.Shift | Keys.Tab when richTextBox1.Focused && TabIndents():
                    RunTool("stDIN", ToolTrigger.Key);
                    return true;
                // Bold, italic, underline (rich text) through the tool handler too, when typing in the text
                case Keys.Control | Keys.B when richTextBox1.Focused:
                    RunTool("stBLD", ToolTrigger.Key);
                    return true;
                case Keys.Control | Keys.I when richTextBox1.Focused:
                    RunTool("stITA", ToolTrigger.Key);
                    return true;
                case Keys.Control | Keys.U when richTextBox1.Focused:
                    RunTool("stUND", ToolTrigger.Key);
                    return true;
                case Keys.Control | Keys.M when richTextBox1.Focused: // Clear formatting, as the T/ button
                    RunTool("stCLR", ToolTrigger.Key);
                    return true;
                case Keys.Control | Keys.R:               // Repeat last tool (RichEdit's own Ctrl+R, right-align, isn't used)
                    RunTool("clipxRPT", ToolTrigger.Key);
                    return true;
                case Keys.Shift | Keys.F3 when !_quickSearch.IsActive && richTextBox1.Focused && richTextBox1.SelectionLength > 0:
                    RunTool("ccCYC", ToolTrigger.Key);    // cycle case
                    return true;
                case Keys.Alt | Keys.Q:                   // "What do you want to do?" (as Office's Tell me)
                    textBoxWhatToDo.Focus();
                    textBoxWhatToDo.SelectAll();
                    return true;
                case Keys.Control | Keys.F:               // Quick search
                    _quickSearch.Activate();
                    return true;
                // Next / previous match while Quick search is active (Search and Replace will do the
                // same when it is active); otherwise Shift+F3 goes on to the text box: cycle case
                case Keys.F3 when _quickSearch.IsActive:
                    _quickSearch.FindNext(forward: true);
                    return true;
                case Keys.Shift | Keys.F3 when _quickSearch.IsActive:
                    _quickSearch.FindNext(forward: false);
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

        private static string ProgramDictionaries => DictionaryCatalogue.ProgramFolder;

        private Task<SpellChecker> _spellLoading;
        private PersonalDictionary _personal;

        /// <summary>Every dictionary in either folder, by name (en_GB, fr_FR…).</summary>
        private static IReadOnlyList<string> SpellingLanguages() => DictionaryCatalogue.InstalledNames();

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
            string folder = DictionaryCatalogue.FolderOf(locale) ?? ProgramDictionaries;
            SpellingSettings s = AppSettings.Current.Spelling;
            var options = new SpellingOptions
            {
                IgnoreUppercase = s.IgnoreUppercase,
                IgnoreWordsWithNumbers = s.IgnoreWordsWithNumbers,
                IgnoreAddresses = s.IgnoreAddresses,
                CheckRepeatedWords = s.CheckRepeatedWords,
            };
            return new SpellChecker(SpellingDictionary.Load(folder, locale), _personal, options)
            {
                Frequencies = FrequenciesFor(locale),
            };
        });

        private static readonly Dictionary<string, WordFrequencies> _frequencies = new Dictionary<string, WordFrequencies>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// How common words are, for ordering suggestions: the frequency list beside the
        /// dictionary, named after it (en_GB.al), in the user's dictionaries folder or the
        /// program's; failing that, one for the same language (en_GB.al serves en_US too).
        /// Null if there is none: suggestions are then ordered without it. Each list is read once.
        /// </summary>
        private static WordFrequencies FrequenciesFor(string locale)
        {
            lock (_frequencies)
            {
                if (_frequencies.TryGetValue(locale, out WordFrequencies known)) return known;
                string language = locale.Split('_', '-')[0];
                string path = DictionaryCatalogue.Folders
                    .Select(f => System.IO.Path.Combine(f, locale + ".al"))
                    .FirstOrDefault(System.IO.File.Exists)
                    ?? DictionaryCatalogue.Folders
                        .Where(System.IO.Directory.Exists)
                        .SelectMany(f => System.IO.Directory.EnumerateFiles(f, "*.al"))
                        .FirstOrDefault(p =>
                        {
                            string name = System.IO.Path.GetFileNameWithoutExtension(p);
                            return string.Equals(name, language, StringComparison.OrdinalIgnoreCase)
                                || name.StartsWith(language + "_", StringComparison.OrdinalIgnoreCase)
                                || name.StartsWith(language + "-", StringComparison.OrdinalIgnoreCase);
                        });
                WordFrequencies list = null;
                if (path != null)
                    try { list = WordFrequencies.Load(path); }
                    catch (Exception ex) when (ex is System.IO.IOException || ex is UnauthorizedAccessException) { /* ranking without it */ }
                _frequencies[locale] = list;
                return list;
            }
        }

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

        /// <summary>"More languages…" in the spelling panel: the Options dialog's Spelling page, where dictionaries are downloaded.</summary>
        private void MoreSpellingLanguages() => ShowOptions(OptionsPage.Spelling);

        /*********** Options ****************/

        /// <summary>
        /// The Options button, as in ClipX 1: a click opens the Options dialog; a right-click
        /// switches plain text between the proportional and the fixed-width font.
        /// </summary>
        private void OptionsButtonMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DisplaySettings d = AppSettings.Current.Display;
                d.UseFixedFont = !d.UseFixedFont;
                AppSettings.Current.Save();
                ApplyDisplaySettings();
            }
            else if (e.Button == MouseButtons.Left)
            {
                ShowOptions(OptionsPage.Display);
            }
        }

        private void ShowOptions(OptionsPage page)
        {
            string language = ResolveLanguage(AppSettings.Current.Spelling.Language);
            using var dialog = new OptionsDialog(page) { CurrentLanguage = language };
            dialog.Applied += (s, e) =>
            {
                ApplyDisplaySettings();
                // The spelling options (or, after an import, the language) may have changed
                _spellLoading = LoadSpellChecker(ResolveLanguage(AppSettings.Current.Spelling.Language));
            };
            dialog.ShowDialog(this);
            if (dialog.DictionariesChanged) // a dictionary downloaded or removed: perhaps the one in use
                _spellLoading = LoadSpellChecker(ResolveLanguage(AppSettings.Current.Spelling.Language));
        }

        /// <summary>
        /// Word wrap, and the font for plain text (in rich text each piece of text has its own
        /// font, so the setting applies only when the text is plain).
        /// </summary>
        private void ApplyDisplaySettings()
        {
            DisplaySettings d = AppSettings.Current.Display;
            if (richTextBox1.WordWrap != d.WordWrap) richTextBox1.WordWrap = d.WordWrap;
            if (!_isRtfMode || richTextBox1.TextLength == 0)
            {
                Font font = d.CurrentFont.ToFont();
                if (!font.Equals(richTextBox1.Font)) richTextBox1.Font = font;
                else font.Dispose();
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
            if (!SwitchMode()) return; // lists, indents converted both ways (Form1.Conversion.cs)
            toolStripLabelRtfMode.Text = _isRtfMode ? toolStripLabelRtfMode.Text = "Rich text" : "Plain text";
            ApplyDisplaySettings();
            _panels.Current?.OnModeChanged(); // an open panel shows what applies in the new mode
            ShowStyles();                     // fonts and styles: rich text only
        }
    }
}
