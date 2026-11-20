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
            toolStripButtonCheckPunctuation.Click += (s, e) => CheckPunctuation();
            toolStripButtonSpellcheck.Click += (s, e) => SpellNext();

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

            // Any tool button closes an open options panel, unless it is that panel's own tool, or
            // it opens its own panel (which replaces it). First, so it runs before the tools do.
            foreach (ToolStrip strip in new[] { toolStripCase, toolStrip4, toolStrip2, toolStrip5, toolStrip6 })
                foreach (ToolStripItem item in strip.Items)
                    if (item.Tag is string toolTag) item.MouseUp += (s, e) =>
                    {
                        if (e.Button is MouseButtons.Left or MouseButtons.Right) _quickSearch.Deactivate(); // Find all's highlights go
                        CloseOtherOptions(toolTag, e);
                    };

            // Lists: a click shows the options in the side panel; a right-click applies the last used
            _statusHint = statusHint;
            toolStripButtonLiBUL.MouseUp += (s, e) => ListButton(e, bullets: true);
            toolStripButtonLiNum.MouseUp += (s, e) => ListButton(e, bullets: false);
            // Format: found by their tags (ClipX 1's tool names)
            foreach (ToolStripItem item in toolStrip5.Items)
                if (item.Tag is string tag) item.MouseUp += (s, e) => FormatButton(tag, e);

            // Edit: decoders (no options: a click or right-click decodes at once)
            foreach (ToolStripItem item in toolStrip6.Items)
                if (item.Tag is string tag) item.MouseUp += (s, e) => EditButton(tag, e);

            // Remove: no options, so a click or right-click removes at once
            toolStripButtonLiRMB.MouseUp += (s, e) => { if (e.Button is MouseButtons.Left or MouseButtons.Right) ShowStatus(ListTools.RemoveBullets(richTextBox1, _isRtfMode, AppSettings.Current.BulletList)); };
            toolStripButtonLiREM.MouseUp += (s, e) => { if (e.Button is MouseButtons.Left or MouseButtons.Right) ShowStatus(ListTools.RemoveNumbers(richTextBox1, _isRtfMode, AppSettings.Current.NumList)); };

            // Statistics: in the side panel (click or right-click); found by its tag
            foreach (ToolStripItem item in toolStrip2.Items)
                if (item.Tag as string == "chSTAT")
                    item.MouseUp += (s, e) => { if (e.Button is MouseButtons.Left or MouseButtons.Right) ShowStatistics(); };

            // Line numbers: options in the side panel, right-click = last used; Remove acts at once
            toolStripButtonLiRen.MouseUp += (s, e) => LineNumbersButton(e);
            toolStripButtonLiRLN.MouseUp += (s, e) => { if (e.Button is MouseButtons.Left or MouseButtons.Right) ShowStatus(FormatTools.RemoveLineNumbers(richTextBox1)); };
        }

        /// <summary>Show statistics: the figures in the side panel, kept up to date while it is open.</summary>
        private void ShowStatistics()
        {
            var panel = _panels.Show<ClipX2.UI.Panels.StatisticsPanel>();
            if (!panel.IsAttached) panel.Attach(richTextBox1, () => _isRtfMode);
        }

        /// <summary>Line numbers: a click opens its options in the side panel; a right-click numbers at once with the last used.</summary>
        private void LineNumbersButton(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) ShowStatus(FormatTools.LineNumbers(richTextBox1, AppSettings.Current.NumLines));
            else if (e.Button == MouseButtons.Left)
            {
                var panel = _panels.Show<ClipX2.UI.Panels.LineNumbersPanel>();
                if (!panel.IsAttached) panel.Attach(richTextBox1, () => _isRtfMode, ShowStatus);
            }
        }

        private readonly ToolStripLabel _statusHint;
        private readonly QuickSearch _quickSearch;

        /*********** Lists ****************/

        /// <summary>
        /// Bullets / Numbers button: a click opens its options in the side panel (Apply there);
        /// a right-click bullets or numbers the lines at once with the last-used options (ClipX 1).
        /// </summary>
        private void ListButton(MouseEventArgs e, bool bullets)
        {
            if (e.Button == MouseButtons.Right)
            {
                string done = bullets
                    ? ListTools.Bullets(richTextBox1, _isRtfMode, AppSettings.Current.BulletList)
                    : ListTools.Numbers(richTextBox1, _isRtfMode, AppSettings.Current.NumList);
                ShowStatus(done);
                return;
            }
            if (e.Button != MouseButtons.Left) return;
            if (bullets)
            {
                var panel = _panels.Show<ClipX2.UI.Panels.BulletListPanel>();
                if (!panel.IsAttached) panel.Attach(richTextBox1, () => _isRtfMode, ShowStatus);
            }
            else
            {
                var panel = _panels.Show<ClipX2.UI.Panels.NumberListPanel>();
                if (!panel.IsAttached) panel.Attach(richTextBox1, () => _isRtfMode, ShowStatus);
            }
        }

        /*********** Options panels ****************/

        /// <summary>The side panel a tool opens on a click (null: it acts at once).</summary>
        private static Type PanelFor(string tag) => tag switch
        {
            "liBUL" => typeof(ClipX2.UI.Panels.BulletListPanel),
            "liNUM" => typeof(ClipX2.UI.Panels.NumberListPanel),
            "liREN" => typeof(ClipX2.UI.Panels.LineNumbersPanel),
            "foIND" => typeof(ClipX2.UI.Panels.IndentPanel),
            "foWRP" => typeof(ClipX2.UI.Panels.WordWrapPanel),
            "foREM" => typeof(ClipX2.UI.Panels.RemoveLinesPanel),
            "chSPE" => typeof(ClipX2.UI.Panels.SpellingPanel),
            "chSTAT" => typeof(ClipX2.UI.Panels.StatisticsPanel),
            _ => null,
        };

        /// <summary>
        /// A tool button was clicked: an open options panel closes, unless the click is on that
        /// panel's own tool (again, or a right-click applying its last options), or opens another
        /// panel, which replaces it. The spelling panel isn't an options panel: it stays.
        /// </summary>
        private void CloseOtherOptions(string tag, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;
            ClipX2.UI.Panels.SidePanelView open = _panels.Current;
            if (open == null || open is ClipX2.UI.Panels.SpellingPanel) return;
            Type own = PanelFor(tag);
            if (own == open.GetType()) return;                     // the same tool again
            if (own != null && e.Button == MouseButtons.Left) return; // its panel replaces this one
            _panels.Close();
        }

        /*********** Edit ****************/

        /// <summary>An Edit button: acts at once on a click or right-click.</summary>
        private void EditButton(string tag, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;
            var box = richTextBox1;
            switch (tag)
            {
                case "edUDC": ShowStatus(FormatTools.DecodeUrl(box)); break;
                case "edHDC": ShowStatus(FormatTools.DecodeHtml(box)); break;
                case "edUTF": ShowStatus(FormatTools.DecodeUtf8(box)); break;
                case "edMIM": ShowStatus(FormatTools.DecodeMime(box)); break;
                case "edPLA": ShowStatus(FormatTools.PlainCharacters(box)); break;
                case "edCRL": ShowStatus(FormatTools.CurlyQuotes(box)); break;
                case "edEOL": ShowStatus(FormatTools.RegulariseLineEnds(box)); break;
            }
        }

        /*********** Format ****************/

        /// <summary>
        /// A Format button. Those with options (Indent, Unindent, Word wrap, Remove extra lines)
        /// open them in the side panel on a click, and apply the last used on a right-click; the
        /// rest act at once on either.
        /// </summary>
        private void FormatButton(string tag, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;
            bool right = e.Button == MouseButtons.Right;
            var box = richTextBox1;
            AppSettings s = AppSettings.Current;
            switch (tag)
            {
                case "foTRM": ShowStatus(FormatTools.TrimWhiteSpace(box)); break;
                case "foTRA": ShowStatus(FormatTools.TrimTrailing(box)); break;
                case "foSSP": ShowStatus(FormatTools.SingleSpace(box)); break;
                case "foT2S": ShowStatus(FormatTools.TabsToSpaces(box)); break;
                case "foS2T": ShowStatus(FormatTools.SpacesToTabs(box)); break;
                case "foPAR": ShowStatus(FormatTools.Paragraph(box)); break;

                case "foIND": // Indent / Unindent: a right-click repeats the last of the two, with the last settings
                    if (right) ShowStatus(s.Indent.Unindent ? FormatTools.Unindent(box, s.Indent) : FormatTools.Indent(box, s.Indent));
                    else
                    {
                        var panel = _panels.Show<ClipX2.UI.Panels.IndentPanel>();
                        if (!panel.IsAttached) panel.Attach(box, ShowStatus);
                    }
                    break;
                case "foWRP":
                    if (right) ShowStatus(FormatTools.WordWrap(box, _isRtfMode, s.Wordwrap));
                    else
                    {
                        var panel = _panels.Show<ClipX2.UI.Panels.WordWrapPanel>();
                        if (!panel.IsAttached) panel.Attach(box, () => _isRtfMode, ShowStatus);
                    }
                    break;
                case "foREM":
                    if (right) ShowStatus(FormatTools.RemoveExtraLines(box, s.RemLines));
                    else
                    {
                        var panel = _panels.Show<ClipX2.UI.Panels.RemoveLinesPanel>();
                        if (!panel.IsAttached) panel.Attach(box, ShowStatus);
                    }
                    break;
            }
        }

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
                case Keys.F7:                             // next spelling issue (as Word)
                    _quickSearch.Deactivate();
                    SpellNext();
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
            _isRtfMode = !_isRtfMode;
            if (!_isRtfMode)
            {
                // Live list numbers and bullets become typed text (else they would vanish)
                richTextBox1.Text = ClipX2.TextEngine.Export.DocumentExport.ToPlainText(RichEditReader.Read(richTextBox1), "\n");
            }
            toolStripLabelRtfMode.Text = _isRtfMode ? toolStripLabelRtfMode.Text = "Rich text" : "Plain text";
            ApplyDisplaySettings();
            _panels.Current?.OnModeChanged(); // an open panel shows what applies in the new mode
        }
    }
}
