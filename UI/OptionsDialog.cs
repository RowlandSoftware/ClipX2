using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClipX2.TextEngine.Grammar;

namespace ClipX2.UI
{
    /// <summary>The Options dialog's pages, to open it at one.</summary>
    internal enum OptionsPage { Display, General, Spelling, Grammar }

    /// <summary>
    /// ClipX's options (ClipX 1's dlgPreferences, without its Extensions and Updates pages):
    /// Display (fonts for plain text, word wrap), General (export and import of the settings),
    /// Spelling (dictionaries, with downloads; the checker's options) and Grammar (LanguageTool:
    /// free service, Premium key or own server; ignored rules).
    ///
    /// Changes take effect on OK or Apply (<see cref="Applied"/>); Cancel drops them. Downloading
    /// or removing a dictionary happens at once (it is not something Cancel could undo).
    /// </summary>
    internal partial class OptionsDialog : Form
    {
        private FontSpec _proportional, _fixed, _rich;
        private readonly List<string> _ignoredRules = new List<string>();
        private CancellationTokenSource _work;
        private bool _grammarWasOn;  // grammar checking was on when the dialog opened (or at the last Apply)

        /// <summary>The settings were applied (OK, Apply, or an import): the main window should use them.</summary>
        public event EventHandler Applied;

        /// <summary>Dictionaries were downloaded or removed: the language list (and the one in use) may have changed.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool DictionariesChanged { get; private set; }

        /// <summary>The spelling language in use (for testing LanguageTool in it).</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string CurrentLanguage { get; set; } = "en_GB";

        public OptionsDialog(OptionsPage page = OptionsPage.Display)
        {
            InitializeComponent();
            tabControl.SelectedIndex = (int)page;

            buttonOK.Click += (s, e) => { if (Apply()) { DialogResult = DialogResult.OK; Close(); } };
            buttonApply.Click += (s, e) => Apply();
            FormClosed += (s, e) => _work?.Cancel();

            // Display
            buttonProportional.Click += (s, e) => ChooseFont(fixedPitch: false);
            buttonFixed.Click += (s, e) => ChooseFont(fixedPitch: true);
            buttonRichFont.Click += (s, e) => ChooseRichFont();

            // General
            linkSettingsFolder.Text = AppSettings.Folder;
            linkSettingsFolder.LinkClicked += (s, e) => OpenFolder(AppSettings.Folder);
            buttonExport.Click += (s, e) => ExportSettings();
            buttonImport.Click += (s, e) => ImportSettings();

            // Spelling
            listViewDictionaries.SelectedIndexChanged += (s, e) => UpdateDictionaryButtons();
            textBoxFindDictionary.TextChanged += (s, e) => FillDictionaries(SelectedDictionary?.Name);
            buttonShowAvailable.Click += async (s, e) => await ShowAvailable();
            buttonDownload.Click += async (s, e) => await Download();
            buttonRemove.Click += (s, e) => RemoveDictionary();
            buttonSetDefault.Click += (s, e) => SetDefaultLanguage();
            listViewDictionaries.DoubleClick += (s, e) => SetDefaultLanguage();
            buttonOpenFolder.Click += (s, e) => OpenFolder(DictionaryCatalogue.UserFolder);
            linkDictionarySource.LinkClicked += (s, e) => OpenLink(DictionaryCatalogue.SourceUrl);
            toolTip.SetToolTip(linkDictionarySource, DictionaryCatalogue.SourceName);
            toolTip.SetToolTip(listViewDictionaries, "Supplied: comes with ClipX. Yours: downloaded or added by you (used instead of a supplied one of the same name).");

            // Grammar
            radioFree.CheckedChanged += (s, e) => UpdateGrammarControls();
            radioPremium.CheckedChanged += (s, e) => UpdateGrammarControls();
            radioOwnServer.CheckedChanged += (s, e) => UpdateGrammarControls();
            checkBoxGrammar.CheckedChanged += (s, e) => UpdateGrammarControls();
            linkGetKey.LinkClicked += (s, e) => OpenLink("https://languagetool.org/editor/settings/access-tokens");
            linkLanguageTool.LinkClicked += (s, e) => OpenLink(LanguageToolChecker.CreditUrl);
            toolTip.SetToolTip(linkGetKey, "Opens your LanguageTool account's page of API keys (sign in first)");
            buttonTest.Click += async (s, e) => await TestConnection();
            listBoxIgnoredRules.SelectedIndexChanged += (s, e) => buttonRuleOn.Enabled = listBoxIgnoredRules.SelectedIndex >= 0;
            buttonRuleOn.Click += (s, e) => SwitchRuleBackOn();

            LoadFromSettings();
        }

        /************ Settings in and out ************/

        private void LoadFromSettings()
        {
            AppSettings all = AppSettings.Current;

            DisplaySettings d = all.Display;
            _proportional = Copy(d.ProportionalFont);
            _fixed = Copy(d.FixedFont);
            textBoxProportional.Text = _proportional.ToString();
            textBoxFixed.Text = _fixed.ToString();
            _rich = Copy(d.RichTextFont);
            textBoxRichFont.Text = _rich.ToString();
            radioFixed.Checked = d.UseFixedFont;
            radioProportional.Checked = !d.UseFixedFont;
            checkBoxWordWrap.Checked = d.WordWrap;
            checkBoxHideFormattingWarning.Checked = AppSettings.Current.HideFormattingLossWarning;

            SpellingSettings sp = all.Spelling;
            checkBoxIgnoreUppercase.Checked = sp.IgnoreUppercase;
            checkBoxIgnoreNumbers.Checked = sp.IgnoreWordsWithNumbers;
            checkBoxIgnoreAddresses.Checked = sp.IgnoreAddresses;
            checkBoxRepeated.Checked = sp.CheckRepeatedWords;
            checkBoxSmartReplace.Checked = sp.SmartReplace;
            checkBoxDefinitions.Checked = sp.LookUpDefinitions;
            checkBoxSimpleFirst.Checked = sp.SimpleDefinitionsFirst;
            checkBoxOxford.Checked = sp.OxfordIze;
            _defaultLanguage = sp.Language;
            FillDictionaries(null);

            GrammarSettings g = all.Grammar;
            checkBoxGrammar.Checked = _grammarWasOn = g.Enabled;
            Enum.TryParse(g.Service, true, out LanguageToolService service);
            radioFree.Checked = service == LanguageToolService.Free;
            radioPremium.Checked = service == LanguageToolService.Premium;
            radioOwnServer.Checked = service == LanguageToolService.OwnServer;
            textBoxEmail.Text = g.Username;
            textBoxKey.Text = g.ApiKey;
            textBoxServerUrl.Text = g.ServerUrl;
            checkBoxPicky.Checked = g.Picky;
            checkBoxSecondOpinion.Checked = g.SecondOpinionOnSpelling;
            _ignoredRules.Clear();
            _ignoredRules.AddRange(g.DisabledRules);
            FillIgnoredRules();
            labelTestResult.Text = "";
            UpdateGrammarControls();
        }

        private static FontSpec Copy(FontSpec f) => new FontSpec { Name = f.Name, Size = f.Size, Bold = f.Bold, Italic = f.Italic };

        /// <summary>Writes the dialog's values to the settings and saves them. False if something needs correcting first.</summary>
        private bool Apply()
        {
            if (checkBoxGrammar.Checked && radioPremium.Checked
                && (textBoxEmail.Text.Trim().Length == 0 || textBoxKey.Text.Trim().Length == 0))
            {
                tabControl.SelectedTab = tabGrammar;
                MessageBox.Show(this, "For LanguageTool Premium, enter your account email and API key (or choose the free service).",
                    "Options", MessageBoxButtons.OK, MessageBoxIcon.Information);
                (textBoxEmail.Text.Trim().Length == 0 ? textBoxEmail : textBoxKey).Focus();
                return false;
            }

            AppSettings all = AppSettings.Current;

            DisplaySettings d = all.Display;
            d.ProportionalFont = Copy(_proportional);
            d.FixedFont = Copy(_fixed);
            d.RichTextFont = Copy(_rich);
            d.UseFixedFont = radioFixed.Checked;
            d.WordWrap = checkBoxWordWrap.Checked;
            all.HideFormattingLossWarning = checkBoxHideFormattingWarning.Checked;

            SpellingSettings sp = all.Spelling;
            sp.IgnoreUppercase = checkBoxIgnoreUppercase.Checked;
            sp.IgnoreWordsWithNumbers = checkBoxIgnoreNumbers.Checked;
            sp.IgnoreAddresses = checkBoxIgnoreAddresses.Checked;
            sp.CheckRepeatedWords = checkBoxRepeated.Checked;
            if (!string.IsNullOrEmpty(_defaultLanguage)) sp.Language = _defaultLanguage;
            sp.SmartReplace = checkBoxSmartReplace.Checked;
            sp.LookUpDefinitions = checkBoxDefinitions.Checked;
            sp.SimpleDefinitionsFirst = checkBoxSimpleFirst.Checked;
            sp.OxfordIze = checkBoxOxford.Checked;

            GrammarSettings g = all.Grammar;
            g.Enabled = checkBoxGrammar.Checked;
            // Ticked here, beside "the text is sent": that is the user's agreement, so no need to
            // ask again. (Merely leaving it ticked, as it is for a new user, is not: they are asked at the first check.)
            if (checkBoxGrammar.Checked && !_grammarWasOn) g.ConsentGiven = true;
            _grammarWasOn = checkBoxGrammar.Checked;
            g.Service = SelectedService().ToString();
            g.Username = textBoxEmail.Text.Trim();
            g.ApiKey = textBoxKey.Text.Trim();
            g.ServerUrl = textBoxServerUrl.Text.Trim();
            g.Picky = checkBoxPicky.Checked;
            g.SecondOpinionOnSpelling = checkBoxSecondOpinion.Checked;
            g.DisabledRules.Clear(); // the same list object: the grammar checker sees the change
            g.DisabledRules.AddRange(_ignoredRules);

            all.Save();
            Applied?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /************ Display ************/

        private void ChooseFont(bool fixedPitch)
        {
            FontSpec spec = fixedPitch ? _fixed : _proportional;
            using (System.Drawing.Font current = spec.ToFont())
            {
                fontDialog.Font = current;
                fontDialog.FixedPitchOnly = fixedPitch;
                if (fontDialog.ShowDialog(this) != DialogResult.OK) return;
            }
            FontSpec chosen = FontSpec.From(fontDialog.Font);
            if (fixedPitch) { _fixed = chosen; textBoxFixed.Text = chosen.ToString(); radioFixed.Checked = true; }
            else { _proportional = chosen; textBoxProportional.Text = chosen.ToString(); radioProportional.Checked = true; }
        }

        /// <summary>The font a new, empty rich text starts in.</summary>
        private void ChooseRichFont()
        {
            using (System.Drawing.Font current = _rich.ToFont())
            {
                fontDialog.Font = current;
                fontDialog.FixedPitchOnly = false;
                if (fontDialog.ShowDialog(this) != DialogResult.OK) return;
            }
            _rich = FontSpec.From(fontDialog.Font);
            textBoxRichFont.Text = _rich.ToString();
        }

        /************ General ************/

        private void ExportSettings()
        {
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (saveFileDialog.ShowDialog(this) != DialogResult.OK) return;
            // The file type chosen says which: JSON, or XML in ClipX 1's .pref format
            bool asPref = saveFileDialog.FilterIndex == 2;
            string path = saveFileDialog.FileName;
            string wanted = asPref ? ".pref" : ".json";
            if (!string.Equals(Path.GetExtension(path), wanted, StringComparison.OrdinalIgnoreCase))
                path = Path.ChangeExtension(path, wanted);
            try
            {
                if (!Apply()) return; // export what is on screen
                AppSettings.Export(path, asPref);
                MessageBox.Show(this, "Settings exported to\n" + path, "Export settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Xml.XmlException)
            {
                MessageBox.Show(this, "The settings could not be exported: " + ex.Message, "Export settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ImportSettings()
        {
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog(this) != DialogResult.OK) return;
            if (MessageBox.Show(this, "Replace your settings with those in\n" + openFileDialog.FileName + "?",
                    "Import settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            bool fromClipX1;
            try
            {
                AppSettings.Import(openFileDialog.FileName, out fromClipX1);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Text.Json.JsonException || ex is FormatException)
            {
                MessageBox.Show(this, "That file could not be imported (it may not be a ClipX settings file): " + ex.Message,
                    "Import settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            LoadFromSettings();
            Applied?.Invoke(this, EventArgs.Empty);
            string keyNote = AppSettings.Current.Grammar.ApiKeyProtected.Length > 0 && AppSettings.Current.Grammar.ApiKey.Length == 0
                ? "\n\nThe LanguageTool API key in it can't be read on this account: enter it again on the Grammar page." : "";
            string clipX1Note = fromClipX1
                ? "\n\nFrom ClipX 1: its tool settings, find options, fonts and PowerButtons; your other settings are as they were." : "";
            MessageBox.Show(this, "Settings imported." + clipX1Note + keyNote, "Import settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /************ Spelling: dictionaries ************/

        private IReadOnlyList<DictionaryEntry> _available; // once fetched

        /// <summary>
        /// The default spelling language: the one a check starts in. It is also the last one
        /// chosen in the spelling panel (picking one there makes it the default, as ClipX 1's
        /// tools remember their last settings); here it can be set directly. Saved on OK or Apply.
        /// </summary>
        private string _defaultLanguage;

        private bool IsDefault(string name) => string.Equals(name, _defaultLanguage, StringComparison.OrdinalIgnoreCase);

        private void SetDefaultLanguage()
        {
            DictionaryRow r = SelectedDictionary;
            if (r == null || r.Installed == null) return;
            _defaultLanguage = r.Name;
            labelDictionaryStatus.Text = $"{r.Description} will be the default (on OK or Apply).";
            FillDictionaries(r.Name);
        }

        /// <summary>A line in the list: an installed dictionary, one to download, or both (a newer version is out).</summary>
        private sealed class DictionaryRow
        {
            public DictionaryEntry Installed;   // null if not installed
            public DictionaryEntry Remote;      // null if not (yet) known to be downloadable
            public bool Newer;                  // installed, and LibreOffice's current one differs (usually newer)
            public bool LacksHyphenation;       // installed, but not the hyphenation file its readability needs (LibreOffice has one)
            public string Name => (Installed ?? Remote).Name;
            public string Description => (Installed ?? Remote).Description;
            public bool CanDownload => Remote != null && (Installed == null || Newer || LacksHyphenation);
            public bool CanRemove => Installed != null && Installed.Source == DictionarySource.User;
        }

        /// <summary>Installed dictionaries, then (once fetched) those to download; only those matching Find.</summary>
        private void FillDictionaries(string select)
        {
            string find = textBoxFindDictionary.Text.Trim();
            // Word starts: "fr" finds fr, br_FR and French, not Afrikaans
            bool StartsWord(string text) => text.Split(new[] { ' ', '_', '-', '(', ')', '–' }, StringSplitOptions.RemoveEmptyEntries)
                .Any(w => w.StartsWith(find, StringComparison.OrdinalIgnoreCase)) || text.StartsWith(find, StringComparison.OrdinalIgnoreCase);
            bool Matches(DictionaryRow r) => find.Length == 0
                || StartsWord(r.Name)
                || StartsWord(r.Description);

            var rows = new List<DictionaryRow>();
            foreach (DictionaryEntry i in DictionaryCatalogue.Installed())
            {
                DictionaryEntry remote = _available?.FirstOrDefault(a => string.Equals(a.Name, i.Name, StringComparison.OrdinalIgnoreCase));
                rows.Add(new DictionaryRow
                {
                    Installed = i, Remote = remote,
                    Newer = remote != null && DictionaryCatalogue.DiffersFromRemote(i.Name, remote),
                    LacksHyphenation = DictionaryCatalogue.LacksHyphenation(i.Name, remote),
                });
            }
            if (_available != null)
                foreach (DictionaryEntry a in _available)
                    if (!rows.Any(r => string.Equals(r.Name, a.Name, StringComparison.OrdinalIgnoreCase)))
                        rows.Add(new DictionaryRow { Remote = a });

            listViewDictionaries.BeginUpdate();
            listViewDictionaries.Items.Clear();
            // "From" says nothing until the downloads are listed: hidden till then
            bool showFrom = _available != null;
            if ((columnLicence.Width > 0) != showFrom)
            {
                columnLicence.Width = showFrom ? 120 : 0;
                columnLanguage.Width = showFrom ? 185 : 305;
            }
            foreach (DictionaryRow r in rows.Where(Matches))
            {
                bool isDefault = r.Installed != null && IsDefault(r.Name);
                string where = r.Installed == null ? "No"
                    : (r.Installed.Source == DictionarySource.User ? "Yours" : "Supplied")
                      + (isDefault ? " · default" : "")
                      + (r.Newer ? " – LibreOffice's differs" : r.LacksHyphenation ? " – hyphenation available" : "");
                string from = r.Remote != null ? "LibreOffice (" + r.Remote.RemoteFolder + ")" : "";
                var item = new ListViewItem(new[] { r.Name, r.Description, where, from }) { Tag = r };
                if (r.Installed == null) item.ForeColor = System.Drawing.SystemColors.GrayText;
                else if (r.Newer || r.LacksHyphenation) item.ForeColor = System.Drawing.Color.DarkGreen;
                if (isDefault) item.Font = new System.Drawing.Font(listViewDictionaries.Font, System.Drawing.FontStyle.Bold);
                listViewDictionaries.Items.Add(item);
            }
            listViewDictionaries.EndUpdate();
            if (select != null)
                foreach (ListViewItem item in listViewDictionaries.Items)
                    if (((DictionaryRow)item.Tag).Name == select)
                    {
                        item.Selected = true;
                        item.EnsureVisible();
                        break;
                    }
            UpdateDictionaryButtons();
        }

        private DictionaryRow SelectedDictionary =>
            listViewDictionaries.SelectedItems.Count > 0 ? (DictionaryRow)listViewDictionaries.SelectedItems[0].Tag : null;

        private void UpdateDictionaryButtons()
        {
            DictionaryRow r = SelectedDictionary;
            bool busy = _work != null;
            buttonDownload.Enabled = !busy && r != null && r.CanDownload;
            buttonDownload.Text = r != null && (r.Newer || r.LacksHyphenation) ? "&Update" : "&Download";
            buttonRemove.Enabled = !busy && r != null && r.CanRemove;
            buttonSetDefault.Enabled = r != null && r.Installed != null && !IsDefault(r.Name);
            buttonShowAvailable.Enabled = !busy && _available == null;
        }

        private async Task ShowAvailable()
        {
            labelDictionaryStatus.Text = "Fetching the list of dictionaries…";
            _work = new CancellationTokenSource();
            UpdateDictionaryButtons();
            try
            {
                _available = await DictionaryCatalogue.AvailableAsync(_work.Token);
                labelDictionaryStatus.Text = $"{_available.Count} dictionaries in LibreOffice's collection (grey: not installed; green: yours differs).";
            }
            catch (Exception ex) when (ex is System.Net.Http.HttpRequestException || ex is TaskCanceledException || ex is InvalidDataException || ex is System.Text.Json.JsonException)
            {
                labelDictionaryStatus.Text = "The list could not be fetched: " + ex.Message;
            }
            finally
            {
                _work = null;
            }
            if (IsDisposed) return;
            FillDictionaries(null);
        }

        private async Task Download()
        {
            DictionaryRow r = SelectedDictionary;
            if (r == null || !r.CanDownload) return;
            DictionaryEntry e = r.Remote;
            string question =
                r.Installed != null && r.Installed.Source == DictionarySource.User
                    ? $"Download LibreOffice's current {e.Description} ({e.Name}), replacing yours?"
                : r.Installed != null
                    ? $"Download LibreOffice's current {e.Description} ({e.Name})?\n\nIt will be used instead of the {e.Name} supplied with ClipX. To go back, remove it."
                    : $"Download {e.Description} ({e.Name}) from LibreOffice's dictionaries?";
            question += "\n\nIts licence and credits are saved beside it, as " + e.Name + ".licence.txt.";
            if (e.RemoteHyph.Length > 0)
                question += "\n\nIts hyphenation patterns come too (" + e.Name + HyphenationFiles.Extension + ")"
                    + (ClipX2.TextEngine.TextStatistics.NeedsHyphenation(e.Name) ? ", for the readability scores in Statistics." : ".");
            if (MessageBox.Show(this, question, "Download dictionary", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _work = new CancellationTokenSource();
            UpdateDictionaryButtons();
            var progress = new Progress<string>(text => { if (!IsDisposed) labelDictionaryStatus.Text = text; });
            try
            {
                await DictionaryCatalogue.DownloadAsync(e, progress, _work.Token);
                DictionariesChanged = true;
            }
            catch (Exception ex) when (ex is System.Net.Http.HttpRequestException || ex is TaskCanceledException || ex is IOException || ex is UnauthorizedAccessException)
            {
                if (!IsDisposed) labelDictionaryStatus.Text = "The download failed: " + ex.Message;
            }
            finally
            {
                _work = null;
            }
            if (IsDisposed) return;
            FillDictionaries(e.Name);
        }

        private void RemoveDictionary()
        {
            DictionaryRow r = SelectedDictionary;
            if (r == null || !r.CanRemove) return;
            bool supplied = File.Exists(Path.Combine(DictionaryCatalogue.ProgramFolder, r.Name + ".dic"));
            string question = $"Remove {r.Description} ({r.Name}) from your dictionaries?" +
                (supplied ? $"\n\nThe {r.Name} supplied with ClipX will be used again." : "\n\nYou can download it again later.");
            if (MessageBox.Show(this, question, "Remove dictionary", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            try
            {
                DictionaryCatalogue.Remove(r.Name);
                DictionariesChanged = true;
                labelDictionaryStatus.Text = $"{r.Name} removed.";
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                labelDictionaryStatus.Text = "It could not be removed: " + ex.Message;
            }
            FillDictionaries(r.Name);
        }

        /************ Grammar ************/

        private LanguageToolService SelectedService() =>
            radioPremium.Checked ? LanguageToolService.Premium
            : radioOwnServer.Checked ? LanguageToolService.OwnServer
            : LanguageToolService.Free;

        private void UpdateGrammarControls()
        {
            bool on = checkBoxGrammar.Checked;
            groupBoxService.Enabled = buttonTest.Enabled = checkBoxPicky.Enabled = checkBoxSecondOpinion.Enabled = on;
            labelEmail.Enabled = textBoxEmail.Enabled = labelKey.Enabled = textBoxKey.Enabled = linkGetKey.Enabled = radioPremium.Checked;
            textBoxServerUrl.Enabled = radioOwnServer.Checked;
        }

        private void FillIgnoredRules()
        {
            listBoxIgnoredRules.Items.Clear();
            foreach (string rule in _ignoredRules) listBoxIgnoredRules.Items.Add(rule);
            if (_ignoredRules.Count == 0) listBoxIgnoredRules.Items.Add("(none)");
            listBoxIgnoredRules.Enabled = _ignoredRules.Count > 0;
            buttonRuleOn.Enabled = false;
        }

        private void SwitchRuleBackOn()
        {
            if (listBoxIgnoredRules.SelectedItem is string rule && _ignoredRules.Remove(rule)) FillIgnoredRules();
        }

        /// <summary>A short check with what is on the page (not yet applied): does the service answer, and take the key?</summary>
        private async Task TestConnection()
        {
            buttonTest.Enabled = false;
            labelTestResult.ForeColor = System.Drawing.SystemColors.ControlText;
            labelTestResult.Text = "Testing…";
            var checker = new LanguageToolChecker(new LanguageToolOptions
            {
                Service = SelectedService(),
                Username = textBoxEmail.Text.Trim(),
                ApiKey = textBoxKey.Text.Trim(),
                ServerUrl = textBoxServerUrl.Text.Trim(),
                Language = LanguageToolChecker.LanguageCode(CurrentLanguage),
            });
            GrammarCheckResult result;
            try { result = await checker.TestConnectionAsync(); }
            catch (Exception ex) { result = new GrammarCheckResult(null, GrammarCheckError.NoConnection, ex.Message); }
            if (IsDisposed) return;
            buttonTest.Enabled = checkBoxGrammar.Checked;
            if (result.Complete)
            {
                labelTestResult.ForeColor = System.Drawing.Color.DarkGreen;
                labelTestResult.Text = SelectedService() == LanguageToolService.Premium
                    ? "Connected: the Premium key was accepted."
                    : "Connected.";
            }
            else
            {
                labelTestResult.ForeColor = System.Drawing.Color.Firebrick;
                labelTestResult.Text = result.ErrorMessage;
            }
        }

        /************ Helpers ************/

        private void OpenFolder(string folder)
        {
            try
            {
                Directory.CreateDirectory(folder);
                Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.ComponentModel.Win32Exception)
            {
                MessageBox.Show(this, "The folder could not be opened: " + ex.Message, "Options");
            }
        }

        private static void OpenLink(string url)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception || ex is InvalidOperationException) { /* no browser */ }
        }
    }
}
