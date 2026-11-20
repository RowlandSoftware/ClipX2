using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClipX2.TextEngine;
using ClipX2.TextEngine.Grammar;
using ClipX2.TextEngine.Spelling;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Spelling and grammar checker in the side panel (ClipX 1's dlgSpelling). Works on the
    /// editor's text directly: each Change is one Undo step in the editor and keeps the formatting.
    /// Checks from the caret to the end, then from the top to where it started; with a selection,
    /// only the selection. If the user edits the text meanwhile, it picks up from the caret.
    ///
    /// Spelling is Hunspell's (with the personal dictionary, Ignore All…), at once. Grammar is
    /// LanguageTool's, which arrives a paragraph or so at a time from the caret: if the next
    /// spelling issue is in a part not yet checked for grammar, the panel waits for it (briefly:
    /// the part at the caret is asked for first). LanguageTool is also a second opinion on
    /// spelling: a word Hunspell flags is skipped if LanguageTool knows it (Bertie, waggon).
    /// </summary>
    public partial class SpellingPanel : SidePanelView
    {
        private ClipXRichTextBox _box;
        private Func<string, Task<SpellChecker>> _loader;
        private SpellChecker _spell;
        private PanelIssue _issue;

        private int _startPos;        // where the check began (moves with edits before it)
        private bool _canWrap;        // started part-way through the whole text, so go round to the top at the end
        private bool _secondPass;     // gone round: now checking from the top to where it started
        private int _limitEnd = -1;   // end of the selection being checked, -1 for the whole text
        private int _found;           // issues shown in this check
        private bool _oldHideSelection;
        private int _findTicket;      // a newer search makes an older (waiting) one give up
        private bool _checking;       // a check is under way (an issue shown, or waiting for grammar results)
        private int _searchPos;       // where the current search is looking from
        private readonly string _ignoreAllText;

        /// <summary>The issue being shown: a spelling one (Hunspell) or a grammar one (LanguageTool).</summary>
        private sealed class PanelIssue
        {
            public SpellingIssue Spelling;
            public GrammarItem Grammar;
            /// <summary>For a spelling issue, LanguageTool's comment if it adds something ("'color' is American English.").</summary>
            public string Note = "";

            public bool IsGrammar => Grammar != null;
            public int Start => Spelling?.Start ?? Grammar.Start;
            public int Length => Spelling?.Length ?? Grammar.Length;
            public int End => Start + Length;
            public string Word => Spelling?.Word ?? Grammar.Issue.Text;
        }

        public SpellingPanel()
        {
            InitializeComponent();
            _ignoreAllText = buttonIgnoreAll.Text;

            buttonIgnore.Click += (s, e) => Act(Ignore);
            buttonIgnoreAll.Click += (s, e) => Act(IgnoreAll);
            buttonChange.Click += (s, e) => Act(Change);
            buttonChangeAll.Click += (s, e) => Act(ChangeAll);
            buttonAdd.Click += (s, e) => Act(AddToDictionary);
            buttonDelete.Click += (s, e) => Act(Delete);

            listBoxSuggestions.SelectedIndexChanged += (s, e) =>
            {
                if (listBoxSuggestions.SelectedItem is string w) textBoxChangeTo.Text = w;
            };
            listBoxSuggestions.DoubleClick += (s, e) => { if (listBoxSuggestions.SelectedItem != null) Act(Change); };
            comboLanguage.DrawMode = DrawMode.OwnerDrawFixed;
            comboLanguage.DrawItem += DrawLanguageItem;
            // The wheel over the closed list would switch the language at once: only when it is open
            comboLanguage.MouseWheel += (s, e) => { if (!comboLanguage.DroppedDown && e is HandledMouseEventArgs h) h.Handled = true; };
            comboLanguage.SelectionChangeCommitted += async (s, e) => await ChangeLanguage();

            // The word in question, over the right half of labelKind; a click copies it to "Change to"
            // (for when the typo is closer to the right word than any suggestion)
            tableLayout.Layout += (s, e) => PlaceWordLabel();
            labelWord.Click += (s, e) => CopyWordToChangeTo();
            TextBoxKeys.Attach(textBoxChangeTo); // Ctrl+L, Ctrl+Shift+L, Ctrl+S, Ctrl+T, as in the editor

            // Definitions (Wiktionary) beside the panel: a right-click on a suggestion or on
            // "Change to" opens them (the menu key or Shift+F10 too); once open, they follow the
            // selected suggestion and what is typed
            listBoxSuggestions.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    int index = listBoxSuggestions.IndexFromPoint(e.Location);
                    if (index >= 0) listBoxSuggestions.SelectedIndex = index;
                    LookUpSuggestion();
                }
                else if (DefinitionsOpen) LookUpSuggestion();
            };
            listBoxSuggestions.KeyUp += (s, e) =>
            {
                if (e.KeyCode == Keys.Apps || (e.KeyCode == Keys.F10 && e.Shift)) { LookUpSuggestion(); return; }
                if (DefinitionsOpen && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.PageUp ||
                    e.KeyCode == Keys.PageDown || e.KeyCode == Keys.Home || e.KeyCode == Keys.End))
                    LookUpSuggestion();
            };
            // The text box's own menu (cut, copy, paste) gives way to the look-up; the keys still work
            var lookUpMenu = new ContextMenuStrip();
            lookUpMenu.Opening += (s, e) => { e.Cancel = true; LookUp(textBoxChangeTo.Text); };
            textBoxChangeTo.ContextMenuStrip = lookUpMenu;
            textBoxChangeTo.TextChanged += (s, e) =>
            {
                if (!textBoxChangeTo.Focused || !DefinitionsOpen) return;
                _typingPause.Stop(); // typing: look up when the user pauses
                _typingPause.Start();
            };
            _typingPause.Tick += (s, e) => { _typingPause.Stop(); LookUp(textBoxChangeTo.Text); };
            checkBoxSmartReplace.CheckedChanged += (s, e) =>
            {
                if (_settings == null || _settings.SmartReplace == checkBoxSmartReplace.Checked) return;
                _settings.SmartReplace = checkBoxSmartReplace.Checked;
                AppSettings.Current.Save();
            };
            checkBoxGrammar.CheckedChanged += (s, e) => GrammarBoxChanged();

            ShowFinished(null);
        }

        private Func<IReadOnlyList<string>> _languages;
        private SpellingSettings _settings;
        private Action _moreLanguages;

        /// <summary>Last item in the language list; picking it calls the "more languages" action.</summary>
        private const string MoreLanguagesItem = "More languages…";

        /// <summary>
        /// Connects the panel to the editor, the dictionary loader, the list of installed
        /// languages (read afresh each time the panel opens), the spelling settings (the language
        /// and Smart replace are kept there) and what "More languages…" does. Once, after creation.
        /// </summary>
        internal void Attach(ClipXRichTextBox box, Func<string, Task<SpellChecker>> loader,
            Func<IReadOnlyList<string>> languages, SpellingSettings settings, Action moreLanguages)
        {
            _box = box;
            _loader = loader;
            _languages = languages;
            _settings = settings;
            _moreLanguages = moreLanguages;
            checkBoxSmartReplace.Checked = settings.SmartReplace;
            OnActivated(); // the host activated the panel before it was attached
        }

        internal bool IsAttached => _box != null;

        /// <summary>
        /// The editor's selection must stay visible while focus is in the panel. The language
        /// list is refreshed, in case dictionaries have been added.
        /// </summary>
        protected internal override void OnActivated()
        {
            if (_box == null) return;
            _oldHideSelection = _box.HideSelection;
            _box.HideSelection = false;
            FillLanguages(_spell?.Dictionary.Locale ?? _settings.Language);
            _settings = AppSettings.Current.Spelling; // a new object after Import settings
            checkBoxSmartReplace.Checked = _settings.SmartReplace; // may have been changed in Options
            ShowGrammarSetting();
        }

        /// <summary>
        /// A dictionary in the language list: shown by name, in full in the list ("English
        /// (United Kingdom)") and short in the narrow box ("English (GB)"); its file name
        /// (en_GB) in the tooltip.
        /// </summary>
        private sealed class LanguageItem
        {
            public string Locale;
            public string Full;
            public string Short;
            public override string ToString() => Full;
        }

        private static string LocaleOf(object item) => (item as LanguageItem)?.Locale ?? item as string;

        private void FillLanguages(string selected)
        {
            comboLanguage.BeginUpdate();
            comboLanguage.Items.Clear();
            var items = _languages()
                .Select(l => new LanguageItem { Locale = l, Full = DictionaryCatalogue.DescribeLocale(l), Short = DictionaryCatalogue.ShortLocale(l) })
                .OrderBy(i => i.Full, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            foreach (LanguageItem item in items) comboLanguage.Items.Add(item);
            comboLanguage.Items.Add(MoreLanguagesItem);
            // The list is as wide as its longest name, though the box is narrow
            int widest = comboLanguage.Width;
            foreach (object item in comboLanguage.Items)
                widest = Math.Max(widest, TextRenderer.MeasureText(item.ToString(), comboLanguage.Font).Width + SystemInformation.VerticalScrollBarWidth + 8);
            comboLanguage.DropDownWidth = widest;
            SelectLanguage(selected);
            comboLanguage.EndUpdate();
        }

        private void SelectLanguage(string locale)
        {
            foreach (object item in comboLanguage.Items)
                if (string.Equals(LocaleOf(item), locale, StringComparison.OrdinalIgnoreCase))
                {
                    comboLanguage.SelectedItem = item;
                    toolTip.SetToolTip(comboLanguage, item is LanguageItem li ? $"{li.Full} ({li.Locale})" : "");
                    return;
                }
        }

        /// <summary>The full name in the list, the short one in the box itself.</summary>
        private void DrawLanguageItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0)
            {
                object item = comboLanguage.Items[e.Index];
                bool inBox = (e.State & DrawItemState.ComboBoxEdit) != 0;
                string text = item is LanguageItem li ? (inBox ? li.Short : li.Full) : item.ToString();
                TextRenderer.DrawText(e.Graphics, text, e.Font, e.Bounds, e.ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
            e.DrawFocusRectangle();
        }

        protected internal override void OnDeactivated()
        {
            if (_box == null) return;
            _findTicket++;   // a search waiting for grammar gives up
            CancelGrammar(); // and no more of the text is sent
            _checking = false;
            _box.HideSelection = _oldHideSelection;
            _box.ClearMarker();
            HideDefinitions();
            _issue = null;
            _box.Focus();
        }

        /// <summary>
        /// Starts a check: the selection if there is one, else from the caret round to the caret.
        /// If an issue is already selected (F7 again), moves past it.
        /// </summary>
        internal async Task StartCheck(Task<SpellChecker> checker)
        {
            labelKind.Text = checker.IsCompleted ? "" : "Loading dictionary…";
            try { _spell = await checker; }
            catch (Exception ex)
            {
                ShowFinished("The dictionary could not be loaded: " + ex.Message);
                return;
            }
            SelectLanguage(_spell.Dictionary.Locale); // may differ from the setting if that dictionary has gone

            int selStart = _box.SelectionStart, selLen = _box.SelectionLength;
            if (_issue != null && _box.HasMarker && selStart == _issue.Start && selLen == 0)
            {
                FindFrom(_issue.End); // F7 on the issue being shown: skip it, same check
                return;
            }

            _found = 0;
            _checking = true;
            if (selLen > 0)
            {
                _limitEnd = selStart + selLen;  // check the selection only
                _startPos = selStart;
                _canWrap = false;
                _secondPass = false;
            }
            else
            {
                _limitEnd = -1;
                _startPos = selStart;
                _canWrap = _startPos > 0;
                _secondPass = false;
            }
            StartGrammar(selStart, _limitEnd);
            FindFrom(selStart);
        }

        /************ Finding ************/

        /// <summary>
        /// Shows the next issue at or after <paramref name="pos"/>, or that the check is done.
        /// Waits (showing "Checking grammar…") if grammar results for the part in question are
        /// still on their way.
        /// </summary>
        private async void FindFrom(int pos)
        {
            int ticket = ++_findTicket;
            while (true)
            {
                string text = _box.Text;
                int stop = _limitEnd >= 0 ? _limitEnd : (_secondPass ? _startPos : text.Length);
                _searchPos = pos;
                PanelIssue issue = NextIssue(text, pos, stop, out bool wait);
                if (wait)
                {
                    // Say so, and if no answer comes for a while, say why it may be
                    Task signal = _grammarSignal.Task;
                    ShowWaiting(false);
                    if (await Task.WhenAny(signal, Task.Delay(SlowAnswer)) != signal)
                    {
                        if (ticket != _findTicket) return;
                        ShowWaiting(true);
                        await signal;
                    }
                    if (ticket != _findTicket) return; // another search (or Close) since
                    continue;
                }
                if (issue == null && _canWrap && !_secondPass)
                {
                    _secondPass = true;  // go round to the top, up to where we started
                    pos = 0;
                    continue;
                }
                if (issue == null) ShowFinished(FinishedMessage());
                else ShowIssue(issue, text);
                return;
            }
        }

        /// <summary>
        /// The first issue in [pos, stop): spelling or grammar, whichever comes first. Null if
        /// none, or if grammar results that could matter haven't arrived yet (<paramref name="wait"/>).
        /// </summary>
        private PanelIssue NextIssue(string text, int pos, int stop, out bool wait)
        {
            wait = false;
            while (true)
            {
                SpellingIssue sp = pos < stop ? _spell.FindNext(text, pos, wrap: false) : null;
                if (sp != null && sp.Start >= stop) sp = null;
                GrammarItem g = NextGrammar(text, pos, stop);
                bool spellingFirst = sp != null && (g == null || sp.Start <= g.Start);

                // Grammar results needed up to the issue (or the end), to be sure none comes
                // earlier, and to have LanguageTool's view of a misspelt word
                int needed = spellingFirst ? sp.End : g?.Start ?? stop;
                if (GrammarPending && !GrammarCovers(text, pos, needed)) { wait = true; return null; }

                if (sp == null && g == null) return null;
                if (!spellingFirst) return new PanelIssue { Grammar = g };

                GrammarItem opinion = GrammarSpellingAt(sp.Start, sp.End);
                if (sp.Kind == SpellingIssueKind.Misspelt && SecondOpinion && opinion == null && GrammarCovers(text, sp.Start, sp.End))
                {
                    pos = Math.Max(sp.End, pos + 1); // LanguageTool knows the word (Bertie, waggon): not shown; always move on
                    continue;
                }
                return new PanelIssue { Spelling = sp, Note = SpellingNote(opinion) };
            }
        }

        /// <summary>How long to wait for LanguageTool before saying it may be busy.</summary>
        private static readonly TimeSpan SlowAnswer = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Waiting for LanguageTool: "Contacting LanguageTool…" until it has answered once,
        /// then "Checking grammar…". If it is <paramref name="slow"/>, why, and what to do.
        /// </summary>
        private void ShowWaiting(bool slow)
        {
            _issue = null;
            _box.ClearMarker();
            labelWord.Visible = false;
            HideDefinitions();
            bool answered = _run != null && _run.Checked.Count > 0;
            labelKind.Text = (answered ? "Checking grammar…" : "Contacting LanguageTool…")
                + (slow ? "\nNo answer yet: it may be busy, or its free limits reached. Wait, or Close to stop." : "");
            textBoxChangeTo.Text = "";
            listBoxSuggestions.Items.Clear();
            SetButtons(false, false, false);
        }

        private void ShowIssue(PanelIssue issue, string text)
        {
            _issue = issue;
            _found++;
            bool repeated = issue.Spelling?.Kind == SpellingIssueKind.RepeatedWord;

            // Caret to the word, scrolled into view, and the word highlighted (painted over the
            // text, not formatting: see ClipXRichTextBox.SetMarker)
            _box.Select(issue.Start, 0);
            _box.ScrollToCaret();
            _box.SetMarker(issue.Start, issue.Length);
            HideDefinitions(); // they were for the last word's suggestions

            IReadOnlyList<string> suggestions;
            if (issue.IsGrammar)
            {
                GrammarIssue g = issue.Grammar.Issue;
                labelKind.Text = GrammarKind(g) + ":" + (g.ShortMessage.Length > 0 && g.ShortMessage != g.Message ? "\n" + g.ShortMessage : "");
                suggestions = g.Replacements;
                buttonIgnoreAll.Text = "Ignore &Rule";
            }
            else
            {
                labelKind.Text = (repeated ? "Repeated word:" : "Not in dictionary:") + (issue.Note.Length > 0 ? "\n" + issue.Note : "");
                suggestions = _spell.Suggest(_box.Text, issue.Spelling, 10);
                buttonIgnoreAll.Text = _ignoreAllText;
            }
            labelWord.Text = issue.Word;
            labelWord.Visible = true;

            listBoxSuggestions.BeginUpdate();
            listBoxSuggestions.Items.Clear();
            foreach (string s in suggestions) listBoxSuggestions.Items.Add(s);
            listBoxSuggestions.EndUpdate();

            // Repeated word: empty "Change to" means Change deletes it, as in ClipX 1
            textBoxChangeTo.Text = repeated ? "" : suggestions.Count > 0 ? suggestions[0] : issue.Word;
            if (!repeated && suggestions.Count > 0) listBoxSuggestions.SelectedIndex = 0;
            else if (!repeated) listBoxSuggestions.Items.Add("(no suggestions)");
            listBoxSuggestions.Enabled = suggestions.Count > 0;

            SetButtons(true, repeated, issue.IsGrammar);
            if (issue.IsGrammar) ShowExplanation(issue.Grammar.Issue);

            textBoxChangeTo.Focus();
            textBoxChangeTo.SelectAll();
        }

        private void ShowFinished(string message)
        {
            _issue = null;
            _checking = false;
            _box?.ClearMarker();
            labelWord.Visible = false;
            HideDefinitions();
            labelKind.Text = "";
            textBoxChangeTo.Text = "";
            listBoxSuggestions.Items.Clear();
            buttonIgnoreAll.Text = _ignoreAllText;
            SetButtons(false, false, false);
            labelKind.Text = message ?? "";
            if (message != null) FocusCloseButton();
        }

        private string FinishedMessage()
        {
            bool grammar = _run != null && _run.Error.Length == 0;
            string message = _found == 0
                ? (grammar ? "No spelling or grammar errors found." : "No spelling errors found.")
                : (grammar ? "Spelling and grammar check complete." : "Spelling check complete.");
            if (_run != null && _run.Error.Length > 0) message += "\nGrammar not fully checked: " + _run.Error;
            return message;
        }

        private void SetButtons(bool on, bool repeated, bool grammar)
        {
            buttonIgnore.Enabled = buttonIgnoreAll.Enabled = buttonChange.Enabled = buttonDelete.Enabled = on;
            buttonChangeAll.Enabled = buttonAdd.Enabled = on && !repeated && !grammar;
            textBoxChangeTo.Enabled = on;
        }

        /************ The user's choices ************/

        /// <summary>
        /// Runs an action on the current issue, first checking it is still there (the user may
        /// have edited the text); if not, carries on from the caret instead.
        /// </summary>
        private void Act(Action action)
        {
            if (_issue == null || _spell == null) return;
            string text = _box.Text;
            if (_issue.End > text.Length || string.CompareOrdinal(text, _issue.Start, _issue.Word, 0, _issue.Length) != 0)
            {
                FindFrom(_box.SelectionStart);
                return;
            }
            action();
        }

        private void Ignore()
        {
            if (_issue.IsGrammar)
            {
                _run?.Items.Remove(_issue.Grammar);
                FindFrom(_issue.Start);
                return;
            }
            _spell.IgnoreOnce(_issue.Spelling);
            FindFrom(_issue.End);
        }

        /// <summary>Spelling: ignore the word everywhere. Grammar: ignore the rule from now on (saved).</summary>
        private void IgnoreAll()
        {
            if (_issue.IsGrammar)
            {
                List<string> rules = AppSettings.Current.Grammar.DisabledRules;
                string rule = _issue.Grammar.Issue.RuleId;
                if (rule.Length > 0 && !rules.Contains(rule))
                {
                    rules.Add(rule);
                    AppSettings.Current.Save();
                }
                FindFrom(_issue.Start);
                return;
            }
            _spell.IgnoreAll(_issue.Word);
            FindFrom(_issue.Start);
        }

        /// <summary>
        /// Adds the flagged word to the personal dictionary and moves on. But if the user has
        /// typed their own word in "Change to" (itunes → iTunes), adds that instead and stays on
        /// the flagged word, so they can then Change or Change All.
        /// </summary>
        private void AddToDictionary()
        {
            if (_issue.IsGrammar) return;
            string typed = textBoxChangeTo.Text.Trim();
            bool edited = typed.Length > 0 && typed != _issue.Word && !listBoxSuggestions.Items.Contains(typed);
            if (!edited)
            {
                _spell.AddToDictionary(_issue.Word);
                FindFrom(_issue.Start);
                return;
            }

            foreach (string word in typed.Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
                if (!_spell.IsCorrect(word)) _spell.AddToDictionary(word);
            labelKind.Text = $"Not in dictionary:\n('{typed}' added to dictionary)";
            buttonChange.Focus();
        }

        /// <summary>
        /// Keeps the word label over labelKind, from the middle of the panel to its right-hand
        /// edge, level with the first line of labelKind. Its height is fixed (designer).
        /// labelKind wraps within the panel (grammar messages can be long).
        /// </summary>
        private void PlaceWordLabel()
        {
            int left = tableLayout.Left + tableLayout.Width / 2;
            int right = tableLayout.Right - tableLayout.Padding.Right - labelKind.Margin.Right;
            int top = tableLayout.Top + labelKind.Top + (labelKind.Font.Height + 4 - labelWord.Height) / 2;
            labelWord.SetBounds(left, Math.Max(0, top), Math.Max(10, right - left), labelWord.Height);

            int width = Math.Max(50, tableLayout.ClientSize.Width - tableLayout.Padding.Horizontal - labelKind.Margin.Horizontal);
            if (labelKind.MaximumSize.Width != width) labelKind.MaximumSize = new Size(width, 0);
        }

        /************ Definitions ************/

        private DefinitionPopup _definitions;
        private CancellationTokenSource _lookupCancel;
        private readonly System.Windows.Forms.Timer _typingPause = new System.Windows.Forms.Timer { Interval = 600 };

        /// <summary>The definitions box is showing (so it follows the selection and typing).</summary>
        private bool DefinitionsOpen => _definitions != null && _definitions.Visible && _definitions.Word != null;

        private void LookUpSuggestion()
        {
            if (listBoxSuggestions.Enabled && listBoxSuggestions.SelectedItem is string word) LookUp(word);
        }

        /// <summary>
        /// Shows the word's definitions beside the panel (Wiktionary, for the spelling language).
        /// A newer request cancels an older one, so a quick run of clicks shows only the last.
        /// Not for grammar issues: the box shows the rule's explanation then.
        /// </summary>
        private async void LookUp(string word)
        {
            word = word?.Trim() ?? "";
            if (word.Length == 0 || _issue == null || _issue.IsGrammar || !AppSettings.Current.Spelling.LookUpDefinitions) return;
            if (_definitions != null && _definitions.Visible && _definitions.Word == word) return;

            if (!EnsureDefinitions()) return;
            _lookupCancel?.Cancel();
            var cancel = _lookupCancel = new CancellationTokenSource();
            _definitions.ShowMessage(word, "Looking up…");
            PlaceDefinitions();
            _definitions.BringToFront();

            string language = (_spell?.Dictionary.Locale ?? "en").Split('_', '-')[0];
            try
            {
                LookupResult result = await WiktionaryLookup.LookUpAsync(word, language,
                    AppSettings.Current.Spelling.SimpleDefinitionsFirst, cancel.Token);
                if (!cancel.IsCancellationRequested && _definitions.Visible) _definitions.ShowResult(result);
            }
            catch (OperationCanceledException) { /* superseded */ }
        }

        /// <summary>A grammar issue's explanation in the box beside the panel, with LanguageTool's credit.</summary>
        private void ShowExplanation(GrammarIssue g)
        {
            if (!EnsureDefinitions()) return;
            _definitions.ShowExplanation(GrammarKind(g), g.Message, g.RuleDescription, g.MoreInfoUrl,
                LanguageToolChecker.Credit, LanguageToolChecker.CreditUrl);
            PlaceDefinitions();
            _definitions.BringToFront();
        }

        /// <summary>The definitions box floats on the form, over the editor, so it is the form's child.</summary>
        private bool EnsureDefinitions()
        {
            if (_definitions != null) return true;
            Form form = FindForm();
            if (form == null) return false;
            _definitions = new DefinitionPopup { Visible = false };
            form.Controls.Add(_definitions);
            form.Resize += (s, e) => PlaceDefinitions();
            if (Parent != null) Parent.SizeChanged += (s, e) => PlaceDefinitions(); // the side panel resized (splitter)
            return true;
        }

        /// <summary>
        /// Just to the right of the side panel, from the level of "Change to" down to the bottom
        /// of the side panel.
        /// </summary>
        private void PlaceDefinitions()
        {
            if (_definitions == null || Parent == null) return;
            Form form = _definitions.Parent as Form;
            if (form == null) return;
            Control side = Parent;
            Point right = form.PointToClient(side.PointToScreen(new Point(side.ClientSize.Width, 0)));
            int top = form.PointToClient(textBoxChangeTo.PointToScreen(Point.Empty)).Y;
            int bottom = form.PointToClient(side.PointToScreen(new Point(0, side.ClientSize.Height))).Y;
            _definitions.SetBounds(right.X + 2, top, 330, Math.Max(120, bottom - top));
        }

        private void HideDefinitions()
        {
            _typingPause.Stop();
            _lookupCancel?.Cancel();
            _definitions?.Hide();
        }

        /// <summary>Puts the word itself in "Change to", ready to edit (Ctrl+T, Ctrl+S work there).</summary>
        private void CopyWordToChangeTo()
        {
            if (_issue == null) return;
            listBoxSuggestions.ClearSelected();
            textBoxChangeTo.Text = _issue.Word;
            textBoxChangeTo.Focus();
            textBoxChangeTo.Select(textBoxChangeTo.TextLength, 0);
        }

        /// <summary>
        /// Replaces the word. Carries on from the start of the replacement, so if what the user
        /// typed is itself wrong, it is shown next (ClipX 1 re-checked in the same way).
        /// </summary>
        private void Change()
        {
            string replacement = textBoxChangeTo.Text;
            if (replacement.Length == 0) { Delete(); return; }
            TextEdit edit = _issue.IsGrammar
                ? new TextEdit(_issue.Start, _issue.Length, replacement) // LanguageTool's replacements come in the right case
                : _spell.Change(_issue.Spelling, replacement, checkBoxSmartReplace.Checked);
            Apply(new[] { edit });
            FindFrom(_issue.Start);
        }

        private void ChangeAll()
        {
            if (_issue.IsGrammar) return;
            string replacement = textBoxChangeTo.Text;
            if (replacement.Length == 0) return;
            if (!AllCorrect(replacement))
            {
                if (MessageBox.Show(this, $"'{replacement}' may be misspelt.\n\nChange every '{_issue.Word}' to it?",
                        "Change All", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                _spell.IgnoreAll(replacement); // accepted, so don't flag the changed words
            }

            IReadOnlyList<TextEdit> edits = _spell.ChangeAll(_box.Text, _issue.Spelling, replacement, checkBoxSmartReplace.Checked);
            if (edits.Count == 0) edits = new[] { _spell.Change(_issue.Spelling, replacement, checkBoxSmartReplace.Checked) }; // never leave it stuck on the word
            // Carry on after the replacement: the user has already said yes to it, so even if
            // it fails the check it isn't shown again
            int next = MapPosition(_issue.Start, edits);
            foreach (TextEdit e in edits)
                if (e.Start == _issue.Start) { next += e.Replacement.Length; break; }
            Apply(edits);
            FindFrom(next);
        }

        private void Delete()
        {
            TextEdit edit = _issue.IsGrammar ? DeleteRange(_box.Text, _issue.Start, _issue.End) : _spell.Delete(_box.Text, _issue.Spelling);
            Apply(new[] { edit });
            FindFrom(edit.Start);
        }

        /// <summary>Deletes the range with one space beside it, so no double space is left (as for a word).</summary>
        private static TextEdit DeleteRange(string text, int s, int e)
        {
            bool IsSpace(char c) => c == ' ' || c == '\t' || c == (char)0x00A0;
            if (s > 0 && IsSpace(text[s - 1]))
                while (s > 0 && IsSpace(text[s - 1])) s--;
            else
                while (e < text.Length && IsSpace(text[e])) e++;
            return new TextEdit(s, e - s, "");
        }

        private bool AllCorrect(string replacement)
        {
            foreach (SpellingIssue i in _spell.FindIssues(replacement))
                if (i.Kind == SpellingIssueKind.Misspelt) return false;
            return true;
        }

        /// <summary>Applies edits in the editor and keeps the check's own positions in step.</summary>
        private void Apply(IReadOnlyList<TextEdit> edits)
        {
            _box.ApplyEdits(edits);
            _spell.NotifyEdits(edits);
            ShiftGrammar(edits);
            int start = _startPos, limit = _limitEnd;
            foreach (TextEdit e in edits)
            {
                int delta = e.Replacement.Length - e.Length;
                if (e.Start < start) _startPos += delta;
                if (limit >= 0 && e.Start < limit) _limitEnd += delta;
            }
        }

        /// <summary>Where <paramref name="pos"/> (before the edits) is after them.</summary>
        private static int MapPosition(int pos, IReadOnlyList<TextEdit> edits)
        {
            int result = pos;
            foreach (TextEdit e in edits)
                if (e.Start < pos) result += e.Replacement.Length - e.Length;
            return result;
        }

        private async Task ChangeLanguage()
        {
            string locale = LocaleOf(comboLanguage.SelectedItem);
            if (_loader == null || locale == null) return;
            if (locale == MoreLanguagesItem)
            {
                SelectLanguage(_spell?.Dictionary.Locale ?? _settings.Language); // put the list back
                _moreLanguages?.Invoke();
                FillLanguages(_spell?.Dictionary.Locale ?? _settings.Language);  // in case files were added meanwhile
                return;
            }
            if (_spell != null && _spell.Dictionary.Locale == locale) return;
            // A check under way (even one waiting for grammar results) goes on in the new language
            bool checking = _checking;
            int from = _issue?.Start ?? (checking ? _searchPos : _box.SelectionStart);
            string kind = labelKind.Text;
            labelKind.Text = "Loading dictionary…";
            try { _spell = await _loader(locale); }
            catch (Exception ex) { ShowFinished("The dictionary could not be loaded: " + ex.Message); return; }
            _settings.Language = locale;
            AppSettings.Current.Save();
            labelKind.Text = checking ? kind : "";
            if (checking)
            {
                StartGrammar(from, _limitEnd); // in the new language (the old request is cancelled)
                FindFrom(from);
            }
            else CancelGrammar(); // results kept from the old language would be no use
        }

        /// <summary>Enter in "Change to" (or on the list) = Change.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter && (textBoxChangeTo.Focused || listBoxSuggestions.Focused) && _issue != null)
            {
                Act(Change);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /************ Grammar (LanguageTool) ************/

        /// <summary>A grammar issue, at its position in the editor's text now.</summary>
        private sealed class GrammarItem
        {
            public int Start, Length;
            public GrammarIssue Issue;
            public int End => Start + Length;
        }

        /// <summary>One grammar check of the text: what has arrived so far, in today's positions.</summary>
        private sealed class GrammarRun
        {
            public readonly CancellationTokenSource Cancel = new CancellationTokenSource();
            /// <summary>Edits made since the text was sent, to place results that arrive later.</summary>
            public readonly List<IReadOnlyList<TextEdit>> Edits = new List<IReadOnlyList<TextEdit>>();
            public readonly List<GrammarItem> Items = new List<GrammarItem>();             // by position
            public readonly List<(int Start, int End)> Checked = new List<(int, int)>();   // parts done
            public bool Done;
            public string Error = "";
        }

        private LanguageToolChecker _grammarChecker;
        private string _grammarKey;
        private GrammarRun _run;
        private TaskCompletionSource<bool> _grammarSignal = new TaskCompletionSource<bool>();
        private SynchronizationContext _ui;

        private bool GrammarPending => _run != null && !_run.Done;
        private bool SecondOpinion => _run != null && AppSettings.Current.Grammar.SecondOpinionOnSpelling;

        /// <summary>Wakes a search waiting for grammar results.</summary>
        private void SignalGrammar()
        {
            TaskCompletionSource<bool> signal = _grammarSignal;
            _grammarSignal = new TaskCompletionSource<bool>();
            signal.TrySetResult(true);
        }

        /// <summary>
        /// Sends the text (from the caret; the selection only, if <paramref name="until"/> ≥ 0)
        /// to LanguageTool, if grammar checking is on and the user has agreed. Results come in a
        /// part at a time; FindFrom waits for them where it needs them.
        /// </summary>
        private void StartGrammar(int from, int until)
        {
            CancelGrammar();
            GrammarSettings settings = AppSettings.Current.Grammar;
            if (!settings.Enabled || !AskConsent(settings)) return;

            LanguageToolChecker checker = GrammarChecker(settings);
            var run = _run = new GrammarRun();
            _ui = SynchronizationContext.Current;
            Task<GrammarCheckResult> task = checker.CheckAsync(_box.Text, from,
                chunk => _ui?.Post(_ => AddChunk(run, chunk), null), run.Cancel.Token, until);
            FinishGrammar(run, task);
        }

        private async void FinishGrammar(GrammarRun run, Task<GrammarCheckResult> task)
        {
            try
            {
                GrammarCheckResult result = await task; // back on the UI thread, after the parts posted before it
                if (!result.Complete) run.Error = result.ErrorMessage;
            }
            catch (OperationCanceledException) { /* closed, or a new check */ }
            catch (Exception ex) { run.Error = ex.Message; }
            run.Done = true;
            if (run == _run) SignalGrammar();
        }

        private void CancelGrammar()
        {
            if (_run != null)
            {
                _run.Cancel.Cancel();
                _run.Done = true;
            }
            _run = null;
            SignalGrammar();
        }

        /// <summary>A part of the text has been checked: its issues, placed in the text as it is now.</summary>
        private void AddChunk(GrammarRun run, GrammarChunk chunk)
        {
            if (run != _run) return;
            int Map(int pos) { foreach (IReadOnlyList<TextEdit> edits in run.Edits) pos = MapPosition(pos, edits); return pos; }
            foreach (GrammarIssue issue in chunk.Issues)
            {
                int start = Map(issue.Start), end = Map(issue.End);
                if (end - start != issue.Length) continue; // edited since: no longer stands
                var item = new GrammarItem { Start = start, Length = issue.Length, Issue = issue };
                int at = run.Items.FindIndex(i => i.Start > start);
                run.Items.Insert(at < 0 ? run.Items.Count : at, item);
            }
            run.Checked.Add((Map(chunk.Start), Map(chunk.End)));
            SignalGrammar();
        }

        /// <summary>The text has been edited (by the panel): grammar positions follow; issues the edit touched go.</summary>
        private void ShiftGrammar(IReadOnlyList<TextEdit> edits)
        {
            if (_run == null) return;
            _run.Edits.Add(edits);
            for (int i = _run.Items.Count - 1; i >= 0; i--)
            {
                GrammarItem item = _run.Items[i];
                bool touched = false;
                foreach (TextEdit e in edits)
                    if (e.Start < item.End && e.Start + Math.Max(e.Length, 1) > item.Start) { touched = true; break; }
                if (touched) { _run.Items.RemoveAt(i); continue; }
                int start = MapPosition(item.Start, edits);
                item.Start = start;
            }
            for (int i = 0; i < _run.Checked.Count; i++)
                _run.Checked[i] = (MapPosition(_run.Checked[i].Start, edits), MapPosition(_run.Checked[i].End, edits));
        }

        /// <summary>True if grammar results are in for all of [a, b) (gaps of spaces and line breaks don't count).</summary>
        private bool GrammarCovers(string text, int a, int b)
        {
            if (_run == null) return true;
            int pos = a;
            while (pos < b && pos < text.Length)
            {
                int next = -1;
                foreach (var (start, end) in _run.Checked)
                    if (start <= pos && pos < end && end > next) next = end;
                if (next > pos) { pos = next; continue; }
                if (char.IsWhiteSpace(text[pos])) { pos++; continue; }
                return false;
            }
            return true;
        }

        /// <summary>The first grammar issue to show in [pos, stop), skipping ignored rules and LanguageTool's own spelling verdicts.</summary>
        private GrammarItem NextGrammar(string text, int pos, int stop)
        {
            if (_run == null) return null;
            bool repeatsByHunspell = _spell.Options.CheckRepeatedWords;
            foreach (GrammarItem item in _run.Items)
            {
                if (item.Start < pos) continue;
                if (item.Start >= stop) break;
                GrammarIssue g = item.Issue;
                if (g.IsSpelling || IsRuleOff(g.RuleId)) continue;
                if (repeatsByHunspell && g.RuleId.IndexOf("WORD_REPEAT", StringComparison.Ordinal) >= 0) continue; // "the the": already a repeated word
                if (item.End > text.Length || string.CompareOrdinal(text, item.Start, g.Text, 0, item.Length) != 0) continue; // changed since
                return item;
            }
            return null;
        }

        /// <summary>LanguageTool's "Would you like to use the Oxford spelling 'realized'?" (realise → realize).</summary>
        private const string OxfordSpellingRule = "OXFORD_SPELLING_Z_NOT_S";

        /// <summary>
        /// True if a rule is not to be shown: one the user chose to ignore, or the Oxford
        /// -ize suggestion when the user writes -ise (the Spelling setting OxfordIze).
        /// </summary>
        private static bool IsRuleOff(string ruleId) =>
            AppSettings.Current.Grammar.DisabledRules.Contains(ruleId)
            || (ruleId == OxfordSpellingRule && !AppSettings.Current.Spelling.OxfordIze);

        /// <summary>LanguageTool's own spelling verdict on [start, end), if it has one.</summary>
        private GrammarItem GrammarSpellingAt(int start, int end)
        {
            if (_run == null) return null;
            foreach (GrammarItem item in _run.Items)
            {
                if (item.Start >= end) break;
                if (item.Issue.IsSpelling && item.End > start) return item;
            }
            return null;
        }

        /// <summary>What LanguageTool adds about a misspelt word ("'color' is American English."), if anything.</summary>
        private static string SpellingNote(GrammarItem opinion)
        {
            if (opinion == null) return "";
            string m = opinion.Issue.Message.Trim();
            const string generic = "Possible spelling mistake";
            if (m.StartsWith(generic, StringComparison.OrdinalIgnoreCase))
            {
                m = m.Substring(generic.Length).TrimStart('.', ' ');
                if (m.StartsWith("found", StringComparison.OrdinalIgnoreCase)) m = "";
            }
            return m.Trim();
        }

        /// <summary>A short heading for a grammar issue, from its category.</summary>
        private static string GrammarKind(GrammarIssue g)
        {
            switch (g.CategoryId)
            {
                case "TYPOS": return "Possible typo";
                case "CONFUSED_WORDS": return "Confused word";
                case "PUNCTUATION": return "Punctuation";
                case "TYPOGRAPHY": return "Typography";
                case "CASING": return "Capitals";
                case "STYLE":
                case "REDUNDANCY":
                case "PLAIN_ENGLISH":
                case "REPETITIONS_STYLE": return "Style";
                case "COLLOCATIONS": return "Wording";
                case "SEMANTICS": return "Meaning";
                default: return "Grammar";
            }
        }

        /// <summary>The checker for the current language and settings (kept, with its results, while they stay the same).</summary>
        private LanguageToolChecker GrammarChecker(GrammarSettings settings)
        {
            if (!Enum.TryParse(settings.Service, true, out LanguageToolService service)) service = LanguageToolService.Free;
            string apiKey = service == LanguageToolService.Premium ? settings.ApiKey : "";
            string language = LanguageToolChecker.LanguageCode(_spell.Dictionary.Locale);
            string key = $"{service}|{settings.ServerUrl}|{settings.Username}|{apiKey}|{language}|{settings.Picky}";
            if (_grammarChecker == null || _grammarKey != key)
            {
                _grammarChecker = new LanguageToolChecker(new LanguageToolOptions
                {
                    Service = service,
                    ServerUrl = settings.ServerUrl,
                    Username = settings.Username,
                    ApiKey = apiKey,
                    Language = language,
                    DisabledRules = settings.DisabledRules, // the settings' own list: rules ignored later are sent from then on
                    Picky = settings.Picky,
                });
                _grammarKey = key;
            }
            return _grammarChecker;
        }

        /// <summary>
        /// Before any text goes to LanguageTool's server, the user is told and asked, once. No
        /// means spelling only (until changed in Options). Not asked for the user's own server.
        /// </summary>
        private bool AskConsent(GrammarSettings settings)
        {
            if (settings.ConsentGiven || string.Equals(settings.Service, nameof(LanguageToolService.OwnServer), StringComparison.OrdinalIgnoreCase))
                return true;
            DialogResult answer = MessageBox.Show(this,
                "ClipX can check grammar as well as spelling, using LanguageTool (languagetool.org).\n\n" +
                "To do that, it sends the text being checked to LanguageTool's server. Nothing is sent unless you agree.\n\n" +
                "Check grammar too?\n\n" +
                "Yes: spelling and grammar (you won't be asked again).\n" +
                "No: spelling only.",
                "Grammar checking", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer == DialogResult.Yes) settings.ConsentGiven = true;
            else settings.Enabled = false;
            AppSettings.Current.Save();
            ShowGrammarSetting();
            return settings.ConsentGiven;
        }

        private bool _showingGrammarSetting;

        /// <summary>The Grammar box shows the setting (Options can change it too).</summary>
        private void ShowGrammarSetting()
        {
            _showingGrammarSetting = true;
            checkBoxGrammar.Checked = AppSettings.Current.Grammar.Enabled;
            _showingGrammarSetting = false;
        }

        /// <summary>
        /// The Grammar box: switches grammar checking on or off (the same setting as in Options,
        /// so it is remembered). Ticked, after the user's agreement if not yet given, a check
        /// under way carries on with grammar from where it is; unticked, grammar stops, and a
        /// grammar issue on screen gives way to the next spelling one.
        /// </summary>
        private void GrammarBoxChanged()
        {
            if (_showingGrammarSetting) return;
            GrammarSettings settings = AppSettings.Current.Grammar;
            settings.Enabled = checkBoxGrammar.Checked;
            if (settings.Enabled && !AskConsent(settings)) return; // No: AskConsent has switched it off again
            AppSettings.Current.Save();
            if (!_checking || _spell == null)
            {
                if (!settings.Enabled) CancelGrammar();
                return;
            }
            int from = _issue?.Start ?? _searchPos;
            if (settings.Enabled) StartGrammar(from, _limitEnd);
            else CancelGrammar();
            FindFrom(from);
        }
    }
}
