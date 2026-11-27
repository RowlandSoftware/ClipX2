using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using ClipX2.TextEngine;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Find and Replace (ClipX 1's Search and Replace dialogue, edREP), in the side panel; Ctrl+H or Alt+F.
    /// Quick search (Ctrl+F, the bottom bar) stays apart, with its own history.
    ///
    /// Three modes, as ClipX 1's three tabs, as option buttons: Simple, Wildcard and Regex.
    /// Every mode goes through one <see cref="TextFinder"/>, so the buttons work the same in all
    /// three. In Regex mode the status line says while typing what is wrong with the expression.
    /// Wildcard mode shows its options (Find at, how # numbers are written); in Simple mode Special
    /// chars shows buttons that put \t \n \x \u \\ in the Find or Replace box. Help floats beside the
    /// side panel, as the spelling panel's definitions do, and follows the mode.
    ///
    /// Previous / Next (Shift+F3 / F3, Shift+Enter / Enter in the Find box) select the match
    /// before or after the cursor; at the end of the text it says so (with a beep) and the next
    /// search that way goes round from the other end. From start makes the next search start at
    /// the start (Previous: the end), then unticks itself. Find All highlights every match and
    /// counts them; Replace replaces the selected match and goes on to the next (the first press,
    /// with no match selected, only finds); Replace All replaces every match as one Undo step,
    /// keeping the formatting. With In selection (ticked when the panel opens on a selection of
    /// more than one line), Find All and Replace All keep to the selection; finding a match
    /// unticks it, since the selection is then the match.
    /// </summary>
    public partial class FindReplacePanel : SidePanelView
    {
        private const int HistoryLimit = 15;

        private ClipXRichTextBox _box;
        private bool _loading;
        private bool _oldHideSelection;
        private bool _highlighting;      // the box's markers are Find All's
        private bool? _wrapForward;      // reached the end (true) or start (false): the next step goes round
        private string _wrapKey;
        private ComboBox _lastBox;       // Find or Replace: where the special-character buttons type
        private HelpPopup _help;
        private Action<string> _status;  // the window's status bar

        /// <summary>The Find at drop-down, in <see cref="WildcardPosition"/>'s order.</summary>
        private static readonly string[] PositionChoices = { "Anywhere", "Start of line", "End of line", "Start of word", "End of word" };

        /// <summary>The # numbers drop-down: a number as it is written, and its decimal point and thousands separator.</summary>
        private static readonly (string Shown, string Decimal, string Thousands)[] NumberChoices =
        {
            ("1,234.5", ".", ","), ("1.234,5", ",", "."), ("1 234,5", ",", " "), ("1 234.5", ".", " "),
            ("1'234.5", ".", "'"), ("1234.5", ".", ""), ("1234,5", ",", ""),
        };

        public FindReplacePanel()
        {
            InitializeComponent();
            radioSimple.CheckedChanged += (s, e) => ModeChanged();
            radioWildcard.CheckedChanged += (s, e) => ModeChanged();
            radioRegex.CheckedChanged += (s, e) => ModeChanged();
            foreach (CheckBox c in new[] { checkBoxMatchCase, checkBoxWholeWords })
                c.CheckedChanged += (s, e) => SearchChanged(refresh: true);
            checkBoxSpecialChars.CheckedChanged += (s, e) => { ShowSpecialButtons(); SearchChanged(refresh: true); };
            comboPosition.Items.AddRange(PositionChoices);
            comboNumbers.Items.AddRange(NumberChoices.Select(n => (object)n.Shown).ToArray());
            comboPosition.SelectedIndexChanged += (s, e) => SearchChanged(refresh: true);
            comboNumbers.SelectedIndexChanged += (s, e) => SearchChanged(refresh: true);
            comboFind.TextChanged += (s, e) => { SearchChanged(refresh: false); UpdateButtons(); CheckPattern(); };
            comboFind.KeyDown += FindKeyDown;
            comboReplace.KeyDown += ReplaceKeyDown;
            TextBoxKeys.Attach(comboFind);    // Ctrl+L, Ctrl+Shift+L, Ctrl+S, Ctrl+T, as in the editor
            TextBoxKeys.Attach(comboReplace);
            _lastBox = comboFind;
            comboFind.Enter += (s, e) => _lastBox = comboFind;
            comboReplace.Enter += (s, e) => _lastBox = comboReplace;
            // A combo box forgets its cursor once it loses the focus (to a special-character button):
            // remember where it was after each key and click, for InsertSpecial
            foreach (ComboBox combo in new[] { comboFind, comboReplace })
            {
                ComboBox c = combo;
                c.KeyUp += (s, e) => KeepCursor(c);
                c.MouseUp += (s, e) => KeepCursor(c);
            }
            buttonTab.Click += (s, e) => InsertSpecial("\\t");
            buttonNewLine.Click += (s, e) => InsertSpecial("\\n");
            buttonHex.Click += (s, e) => InsertSpecial("\\x");
            buttonUnicode.Click += (s, e) => InsertSpecial("\\u");
            buttonBackslash.Click += (s, e) => InsertSpecial("\\\\");
            buttonSwap.Click += (s, e) => Swap();
            buttonPrevious.Click += (s, e) => Safely(() => Step(forward: false));
            buttonNext.Click += (s, e) => Safely(() => Step(forward: true));
            buttonFindAll.Click += (s, e) => Safely(FindAll);
            buttonReplace.Click += (s, e) => Safely(Replace);
            buttonReplaceAll.Click += (s, e) => Safely(ReplaceAll);
            buttonHelp.Click += (s, e) => ToggleHelp();
        }

        /// <summary>
        /// Runs a search, reporting a search that can't be done (one that took too long, say)
        /// in the status line rather than failing.
        /// </summary>
        private void Safely(Action search)
        {
            try { search(); }
            catch (SearchPatternException ex) { Report(ex.Message, beep: true); }
        }

        /// <summary>
        /// Connects the panel to the editor; once, after creation. <paramref name="status"/> shows
        /// the panel's messages in the window's status bar too, where a small window can't hide them.
        /// </summary>
        internal void Attach(ClipXRichTextBox box, Action<string> status = null)
        {
            _box = box;
            _status = status;
            // A click in the text, or a change to it, closes Find All's list (its positions would be stale)
            _box.MouseDown += (s, e) => HideList();
            _box.TextChanged += (s, e) => HideList();
            _loading = true;
            foreach (string term in AppSettings.Current.FindHistory.Take(HistoryLimit)) comboFind.Items.Add(term);
            foreach (string term in AppSettings.Current.ReplaceHistory.Take(HistoryLimit)) comboReplace.Items.Add(term);
            LoadOptions(AppSettings.Current.Search);
            if (comboFind.Items.Count > 0) comboFind.Text = (string)comboFind.Items[0];
            if (comboReplace.Items.Count > 0) comboReplace.Text = (string)comboReplace.Items[0];
            _loading = false;
            ModeChanged();
            ShowSpecialButtons();
            OnActivated(); // the host activated the panel before it was attached
        }

        /// <summary>The PowerButton editor: a Find and Replace task's texts and options (the editor chooses what it does).</summary>
        internal override Button GoButton(string tag) => tag == "edREP" ? buttonReplaceAll : buttonNext;

        internal override void EditOptions(object options)
        {
            FindReplaceTask t = options as FindReplaceTask ?? new FindReplaceTask();
            EnterEditMode(buttonPrevious, buttonNext, buttonFindAll, buttonReplace, buttonReplaceAll);
            _loading = true;
            if (comboFind.Items.Count == 0)
            {
                foreach (string term in AppSettings.Current.FindHistory.Take(HistoryLimit)) comboFind.Items.Add(term);
                foreach (string term in AppSettings.Current.ReplaceHistory.Take(HistoryLimit)) comboReplace.Items.Add(term);
            }
            LoadOptions(t.Search ?? new SearchOptions());
            comboFind.Text = t.Find ?? "";
            comboReplace.Text = t.Replace ?? "";
            checkBoxFromStart.Checked = t.FromStart;
            checkBoxInSelection.Checked = t.InSelection;
            _loading = false;
            ModeChanged();
            ShowSpecialButtons();
        }

        internal override object EditedOptions => new FindReplaceTask
        {
            Find = Term,
            Replace = comboReplace.Text,
            Search = Options(),
            FromStart = checkBoxFromStart.Checked,
            InSelection = checkBoxInSelection.Checked,
        };

        private void LoadOptions(SearchOptions o)
        {
            radioSimple.Checked = o.Mode == SearchMode.Simple;
            radioWildcard.Checked = o.Mode == SearchMode.Wildcard;
            radioRegex.Checked = o.Mode == SearchMode.Regex;
            checkBoxMatchCase.Checked = o.MatchCase;
            checkBoxWholeWords.Checked = o.WholeWords;
            checkBoxSmartReplace.Checked = o.SmartReplace;
            checkBoxSpecialChars.Checked = o.SpecialCharacters;
            int position = (int)o.Position;
            comboPosition.SelectedIndex = position >= 0 && position < PositionChoices.Length ? position : 0;
            // The separators kept (or the computer's own): the way of writing numbers with them, else
            // the first with that decimal point (a no-break space counts as a space)
            string dec = o.DecimalSeparator == "," ? "," : ".";
            string thousands = o.ThousandsSeparator ?? "";
            if (thousands.Length > 0 && thousands.Trim().Length == 0) thousands = " ";
            int n = Array.FindIndex(NumberChoices, c => c.Decimal == dec && c.Thousands == thousands);
            if (n < 0) n = Array.FindIndex(NumberChoices, c => c.Decimal == dec);
            comboNumbers.SelectedIndex = n;
        }

        internal override bool IsAttached => _box != null;

        protected internal override void OnActivated()
        {
            if (_box == null) return;
            _oldHideSelection = _box.HideSelection;
            _box.HideSelection = false; // the match found shows while the focus is here
        }

        protected internal override void OnDeactivated()
        {
            if (_box == null) return;
            ClearHighlights();
            _help?.Hide();
            buttonHelp.Text = "&Help ▶";
            _box.HideSelection = _oldHideSelection;
        }

        /// <summary>
        /// Ctrl+H (or the tool started another way), whether or not the panel was open: a
        /// selection within a line goes into the Find box; a selection of more than one line ticks
        /// In selection. Then to the Find box, its text selected for typing over.
        /// </summary>
        internal void Begin()
        {
            if (_box == null) return;
            Report("");
            if (_box.SelectionLength > 0)
            {
                string selected = _box.SelectedText;
                bool lines = selected.IndexOf('\n') >= 0 || selected.IndexOf('\r') >= 0;
                checkBoxInSelection.Checked = lines;
                if (!lines) comboFind.Text = AsTyped(selected);
            }
            else checkBoxInSelection.Checked = false;
            comboFind.Focus();
            comboFind.SelectAll();
        }

        /// <summary>The selected text as the Find box needs it to find it as it is, in the current mode and options.</summary>
        private string AsTyped(string text)
        {
            if (Mode == SearchMode.Regex) return System.Text.RegularExpressions.Regex.Escape(text); // \. \( \t …
            if (Mode == SearchMode.Wildcard)
            {
                text = TextFinder.EscapeWildcards(text);                       // \ \? \* …
                return checkBoxSpecialChars.Checked ? text.Replace("\t", "\\t") : text;
            }
            return checkBoxSpecialChars.Checked ? text.Replace("\\", "\\\\").Replace("\t", "\\t") : text;
        }

        /// <summary>F3 / Shift+F3 while the panel is open: the next or previous match.</summary>
        internal void FindNext(bool forward) => Safely(() => Step(forward));

        /************ The options ************/

        private SearchMode Mode => radioRegex.Checked ? SearchMode.Regex : radioWildcard.Checked ? SearchMode.Wildcard : SearchMode.Simple;

        private SearchOptions Options() => new SearchOptions
        {
            Mode = Mode,
            MatchCase = checkBoxMatchCase.Checked,
            WholeWords = checkBoxWholeWords.Checked,
            SmartReplace = checkBoxSmartReplace.Checked,
            SpecialCharacters = checkBoxSpecialChars.Checked,
            Position = comboPosition.SelectedIndex > 0 ? (WildcardPosition)comboPosition.SelectedIndex : WildcardPosition.Anywhere,
            DecimalSeparator = Numbers.Decimal,
            ThousandsSeparator = Numbers.Thousands,
        };

        private (string Shown, string Decimal, string Thousands) Numbers =>
            NumberChoices[Math.Max(0, comboNumbers.SelectedIndex)];

        private string Term => comboFind.Text;

        /// <summary>What a search depends on: when it changes, going round from the other end starts again.</summary>
        private string WrapKey
        {
            get
            {
                SearchOptions o = Options();
                return $"{o.Mode}|{o.MatchCase}|{o.WholeWords}|{o.SpecialCharacters}|{o.Position}|{o.DecimalSeparator}|{o.ThousandsSeparator}|{Term}";
            }
        }

        private void ModeChanged()
        {
            if (_loading) return;
            SearchMode mode = Mode;
            groupWildcard.Visible = mode == SearchMode.Wildcard;
            checkBoxSpecialChars.Enabled = mode != SearchMode.Regex; // a regular expression has its own \t \n \x \u
            ShowSpecialButtons();
            if (_help != null && _help.Visible) ShowHelp();
            Report("");
            CheckPattern();
            SearchChanged(refresh: true);
            UpdateButtons();
        }

        /// <summary>Regex mode: what is wrong with the expression typed so far, in the status line (cleared once it is right).</summary>
        private void CheckPattern()
        {
            if (_loading || Mode != SearchMode.Regex) return;
            string problem = TextFinder.CheckRegex(Term);
            if (problem != null) Report(problem);
            else if (labelStatus.Text.StartsWith("Not a usable regular expression", StringComparison.Ordinal)) Report("");
        }

        private void SearchChanged(bool refresh)
        {
            if (_loading) return;
            _wrapForward = null;
            if (!_highlighting) return;
            if (refresh) Safely(Highlight); else ClearHighlights(); // retyping the term doesn't search every keystroke
        }

        private void UpdateButtons()
        {
            bool can = Term.Length > 0;
            buttonPrevious.Enabled = buttonNext.Enabled = buttonFindAll.Enabled = buttonReplace.Enabled = buttonReplaceAll.Enabled = can;
        }

        /// <summary>The finder for what is in the boxes, or null (and why, in the status line) if it can't be made.</summary>
        private TextFinder Finder()
        {
            if (_box == null || Term.Length == 0) return null;
            try
            {
                TextFinder f = TextFinder.Create(Term, comboReplace.Text, Options());
                return f.IsEmpty ? null : f;
            }
            catch (Exception ex) when (ex is SearchPatternException || ex is NotSupportedException)
            {
                Report(ex.Message, beep: true);
                return null;
            }
        }

        /// <summary>The options are the last used (a later session starts with them); the texts go to the top of their histories.</summary>
        private void Keep(bool replacing)
        {
            AppSettings s = AppSettings.Current;
            s.Search = Options();
            s.FindHistory = Remember(comboFind, Term);
            if (replacing) s.ReplaceHistory = Remember(comboReplace, comboReplace.Text);
            s.Save();
        }

        /************ Finding ************/

        /// <summary>Selects the next match after the cursor (or the previous one before it).</summary>
        private bool Step(bool forward, string before = null)
        {
            TextFinder f = Finder();
            if (f == null) return false;
            ClearHighlights();
            Keep(replacing: before != null);
            string text = _box.Text;
            int start = _box.SelectionStart, length = _box.SelectionLength;
            // Already on a match (the last one found): carry on past it
            int from = forward ? (f.IsMatchAt(text, start, length) ? start + length : start) : start;
            string key = WrapKey;
            if (checkBoxFromStart.Checked)
            {
                from = forward ? 0 : text.Length;
                checkBoxFromStart.Checked = false; // once: then on from there
            }
            // Told "Reached end/start of text" last time: this time go round from the other end
            else if (_wrapForward == forward && _wrapKey == key) from = forward ? 0 : text.Length;
            _wrapForward = null;

            SearchMatch? found = f.Find(text, from, forward);
            if (found == null)
            {
                bool anywhere = f.Find(text, 0, true) != null;
                if (anywhere) { _wrapForward = forward; _wrapKey = key; }
                Report(before + (!anywhere ? (before == null ? "Not found" : "No more matches")
                    : forward ? "Reached end of text: Next goes round to the start"
                    : "Reached start of text: Previous goes round to the end"), beep: true);
                return false;
            }
            checkBoxInSelection.Checked = false; // the selection is now the match
            _box.Select(found.Value.Start, found.Value.Length);
            _box.ScrollToCaret();
            Report(before ?? "");
            if (before == null) RaiseApplied("Find");
            return true;
        }

        /// <summary>
        /// Find All: every match (in the selection with In selection) highlighted and counted; the
        /// selection stays. The matches are listed too, with their context, over this panel.
        /// </summary>
        private void FindAll()
        {
            if (Finder() == null) return;
            Keep(replacing: false);
            Highlight(list: true);
            RaiseApplied("FindAll");
        }

        /// <summary>
        /// Every match highlighted as the spelling panel marks a word; the cursor stays put. With
        /// <paramref name="list"/>, or while the list is shown (an option changed), the list too.
        /// </summary>
        private void Highlight() => Highlight(list: false);

        private void Highlight(bool list)
        {
            TextFinder f = Finder();
            if (f == null) { ClearHighlights(); return; }
            var (start, end) = Range();
            string text = _box.Text;
            List<SearchMatch> found = f.FindAll(text, start, end);
            if (found.Count == 0)
            {
                ClearHighlights();
                Report(checkBoxInSelection.Checked ? "Not found in the selection" : "Not found", beep: true);
                return;
            }
            _box.SetMarkers(found.Select(m => (m.Start, m.Length)));
            _highlighting = true;
            string summary = (found.Count == 1 ? "1 found" : $"{found.Count:N0} found") + (checkBoxInSelection.Checked ? " in the selection" : "");
            Report(summary);
            if (list || ListShown) ShowList(text, found, summary);
        }

        private void ClearHighlights()
        {
            HideList(); // its matches go with the highlights
            if (!_highlighting) return;
            _highlighting = false;
            _box.ClearMarker();
        }

        /************ Find All's list ************/

        private FindAllList _list;

        private bool ListShown => _list != null && _list.Visible;

        /// <summary>The list of matches, over this panel (created the first time).</summary>
        private void ShowList(string text, List<SearchMatch> found, string summary)
        {
            if (_list == null)
            {
                _list = new FindAllList { Dock = DockStyle.Fill, Visible = false };
                _list.CloseRequested += (s, e) => HideList();
                Controls.Add(_list);
            }
            _list.ShowMatches(_box, text, found, $"“{Term}”: {summary}", LiveListLabels(text));
            _list.BringToFront();
            _list.Visible = true;
            _list.OnActivated();
        }

        /// <summary>
        /// Rich text: each paragraph's live bullet or number ("•", "3.", "c)"), which isn't in the
        /// text itself, so the list can show it as plain text shows a typed one; null if none
        /// (plain text, or the paragraphs can't be matched up).
        /// </summary>
        private string[] LiveListLabels(string text)
        {
            try
            {
                var doc = RichEditReader.Read(_box);
                int paragraphs = 1;
                foreach (char c in text) if (c == '\n' || c == '\r') paragraphs++;
                if (doc.Paragraphs.Count != paragraphs || doc.Paragraphs.TrueForAll(p => p.ListKind == TextEngine.Export.ListKind.None)) return null;
                return TextEngine.Export.DocumentExport.ListLabels(doc);
            }
            catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException || ex is InvalidOperationException) { return null; }
        }

        /// <summary>Closes the list: the highlights (and this panel) stay as they are.</summary>
        private void HideList()
        {
            if (!ListShown) return;
            bool hadFocus = _list.ContainsFocus;
            _list.Visible = false;
            _box.CurrentMark = -1;
            if (hadFocus) comboFind.Focus();
        }

        /// <summary>Where Find All and Replace All work: the selection with In selection, else the whole text (end -1).</summary>
        private (int Start, int End) Range() =>
            checkBoxInSelection.Checked && _box.SelectionLength > 0
                ? (_box.SelectionStart, _box.SelectionStart + _box.SelectionLength)
                : (0, -1);

        /************ Replacing ************/

        /// <summary>
        /// The selected match replaced, then on to the next. With no match selected, only finds
        /// (so the first press shows what will be replaced).
        /// </summary>
        private void Replace()
        {
            TextFinder f = Finder();
            if (f == null) return;
            string text = _box.Text;
            int start = _box.SelectionStart, length = _box.SelectionLength;
            if (!f.IsMatchAt(text, start, length)) { Step(forward: true); return; }

            string replacement = f.ReplacementFor(text, new SearchMatch(start, length));
            _box.ApplyEdits(new[] { new TextEdit(start, length, replacement) });
            _box.Select(start + replacement.Length, 0); // on from after it: a replacement containing the term isn't found again
            Step(forward: true, before: "Replaced 1. ");
            RaiseApplied("Replace");
        }

        /// <summary>Every match replaced, as one Undo step, keeping the formatting; the count in the status line.</summary>
        private void ReplaceAll()
        {
            TextFinder f = Finder();
            if (f == null) return;
            ClearHighlights();
            Keep(replacing: true);
            var (start, end) = Range();
            bool inSelection = end >= 0;
            int selStart = _box.SelectionStart, selLength = _box.SelectionLength;
            List<TextEdit> edits = f.ReplaceAll(_box.Text, start, end);
            if (edits.Count == 0)
            {
                Report(inSelection ? "Not found in the selection" : "Not found", beep: true);
                return;
            }
            _box.ApplyEdits(edits);

            // The selection (or cursor) where it was, allowing for the replacements before it
            int Moved(int position) => position + edits.Where(e => e.Start + e.Length <= position).Sum(e => e.Replacement.Length - e.Length);
            if (inSelection) _box.Select(start, Moved(end) - start);
            else _box.Select(Moved(selStart), 0);
            Report(edits.Count == 1 ? "Replaced 1" : $"Replaced {edits.Count:N0}");
            RaiseApplied("ReplaceAll");
        }

        /// <summary>
        /// The special-character buttons, while Special chars is ticked, in Simple mode only (Regex
        /// has its own codes; Wildcard mode's options take the room, and its help lists the codes).
        /// </summary>
        private void ShowSpecialButtons() =>
            groupSpecial.Visible = checkBoxSpecialChars.Checked && Mode == SearchMode.Simple;

        /// <summary>
        /// Puts a special-character code in the box last typed in (Find or Replace), in place of
        /// its selection; the cursor after it, ready for the digits of \x or \u.
        /// </summary>
        private void InsertSpecial(string code)
        {
            ComboBox box = _lastBox ?? comboFind;
            string text = box.Text;
            int start, length;
            if (box.Focused) { start = box.SelectionStart; length = box.SelectionLength; }
            else if (_keptCursor.Box == box && _keptCursor.Text == text) { start = _keptCursor.Start; length = _keptCursor.Length; }
            else { start = text.Length; length = 0; } // not known: at the end
            if (start > text.Length) start = text.Length;
            box.Text = text.Substring(0, start) + code + text.Substring(Math.Min(text.Length, start + length));
            box.Focus();
            box.Select(start + code.Length, 0);
            _keptCursor = (box, box.Text, start + code.Length, 0); // a second button goes after this one
        }

        /// <summary>
        /// Ctrl+Alt+Shift+R (hidden, as in ClipX 1), Wildcard mode: the same search in Regex mode,
        /// the regular expression ClipX made in the Find box (and the replacement, as Regex mode
        /// reads it, in the Replace box), to learn from or to go on with. Whole words is unticked:
        /// it is already in the expression.
        /// </summary>
        private void ShowAsRegex()
        {
            if (Mode != SearchMode.Wildcard)
            {
                Report("Ctrl+Alt+Shift+R shows a wildcard search as a regular expression: in Wildcard mode", beep: true);
                return;
            }
            (string find, string replace) converted;
            try { converted = TextFinder.WildcardAsRegex(Term, comboReplace.Text, Options()); }
            catch (SearchPatternException ex) { Report(ex.Message, beep: true); return; }
            bool replaceChanged = converted.replace != comboReplace.Text;
            radioRegex.Checked = true;
            checkBoxWholeWords.Checked = false;
            comboFind.Text = converted.find;
            if (replaceChanged) comboReplace.Text = converted.replace;
            comboFind.Focus();
            comboFind.SelectionStart = comboFind.Text.Length;
            Report("The wildcard search as a regular expression (Regex mode)");
        }

        /// <summary>Where the cursor was in the Find or Replace box when it lost the focus (with its text then, to know it still applies).</summary>
        private (ComboBox Box, string Text, int Start, int Length) _keptCursor;

        private void KeepCursor(ComboBox box) => _keptCursor = (box, box.Text, box.SelectionStart, box.SelectionLength);

        private void Swap()
        {
            string find = comboFind.Text;
            comboFind.Text = comboReplace.Text;
            comboReplace.Text = find;
            comboFind.Focus();
        }

        /************ Keys, history, messages ************/

        /// <summary>
        /// Esc closes an open drop-down list first; otherwise the panel (SidePanelView). Alt+F in
        /// the panel is From start's key (the window would take it as Find and Replace again).
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Alt | Keys.Shift | Keys.R))
            {
                ShowAsRegex();
                return true;
            }
            if (keyData == (Keys.Alt | Keys.F))
            {
                checkBoxFromStart.Focus();
                checkBoxFromStart.Checked = !checkBoxFromStart.Checked;
                return true;
            }
            if (keyData == Keys.Escape && (comboFind.DroppedDown || comboReplace.DroppedDown))
            {
                comboFind.DroppedDown = comboReplace.DroppedDown = false;
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void FindKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true; // no beep
            if (comboFind.DroppedDown) comboFind.DroppedDown = false;
            Step(forward: !e.Shift);
        }

        private void ReplaceKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            if (comboReplace.DroppedDown) comboReplace.DroppedDown = false;
            Replace();
        }

        /// <summary>
        /// The text to the top of its drop-down (moved there if already in it), kept to
        /// <see cref="HistoryLimit"/>; the list, for the settings.
        /// </summary>
        private static List<string> Remember(ComboBox combo, string term)
        {
            int index = combo.Items.IndexOf(term);
            if (index != 0 && term.Length > 0)
            {
                int caret = combo.SelectionStart;
                if (index > 0) combo.Items.RemoveAt(index);
                combo.Items.Insert(0, term);
                while (combo.Items.Count > HistoryLimit) combo.Items.RemoveAt(combo.Items.Count - 1);
                if (combo.Text != term) combo.Text = term; // removing the item may have cleared it
                combo.SelectionStart = Math.Min(caret, term.Length);
            }
            return combo.Items.Cast<string>().ToList();
        }

        private void Report(string message, bool beep = false)
        {
            labelStatus.Text = message;
            _status?.Invoke(message);
            if (beep) SystemSounds.Beep.Play();
        }

        /************ Help ************/

        private void ToggleHelp()
        {
            if (_help != null && _help.Visible) { _help.Hide(); buttonHelp.Text = "&Help ▶"; }
            else ShowHelp();
        }

        /// <summary>The help for the mode, floating just to the right of the side panel (as the spelling panel's definitions).</summary>
        private void ShowHelp()
        {
            Form form = FindForm();
            if (form == null || Parent == null) return;
            if (_help == null)
            {
                _help = new HelpPopup { Visible = false };
                _help.Closed += (s, e) => buttonHelp.Text = "&Help ▶";
                form.Controls.Add(_help);
                form.Resize += (s, e) => PlaceHelp();
                Parent.SizeChanged += (s, e) => PlaceHelp(); // the side panel resized (splitter)
            }
            SearchMode mode = Mode;
            _help.ShowHelp(mode == SearchMode.Simple ? "Simple search" : mode == SearchMode.Wildcard ? "Wildcards" : "Regular expressions", HelpText(mode));
            PlaceHelp();
            _help.BringToFront();
            buttonHelp.Text = "&Help ◀";
        }

        private void PlaceHelp()
        {
            if (_help == null || Parent == null) return;
            Form form = _help.Parent as Form;
            if (form == null) return;
            // In the PowerButton editor the panel sits in a wide options area: float the help just
            // right of the panel itself (over "When the PowerButton runs"), not past the area's edge
            Control side = IsEditingOptions ? this : Parent;
            _help.Place(form, side, form.PointToClient(side.PointToScreen(System.Drawing.Point.Empty)).Y);
        }

        /// <summary>
        /// The help for a mode, in <see cref="HelpPopup"/>'s lines: "# heading", "term\tmeaning",
        /// paragraphs. Wildcards: ClipX 1's wildcard help sheet (WildCardHelp2), brought up to date.
        /// </summary>
        private static string HelpText(SearchMode mode)
        {
            switch (mode)
            {
                case SearchMode.Wildcard:
                    return string.Join("\n", new[]
                    {
                        "Everything you type is found as it is, except these:",
                        "",
                        "# Wildcards",
                        "? or .\tany character",
                        "*\tnone or more characters",
                        "+\tone or more characters",
                        "&\tany letter",
                        "!\tany word character (letter, digit or _)",
                        "%\tany digit",
                        "~\twhite space (space, tab)",
                        "#\ta number: 12, -3, 1,234.50, .5, 2.5e-3 (separators below)",
                        "@\ta word: cat, isn't, well-known",
                        "[…]\tany one of these: [aeiou], [a-z], [0-9.,]",
                        "[^…]\tany character except these: [^ ] (not a space)",
                        "",
                        "# Combinations",
                        "&*  &+\tnone or more, one or more letters",
                        "Also\t!*  !+  %*  %+  ~*  ~+  ?*  ?+  […]*  […]+",
                        "",
                        "# Not greedy",
                        "*, + and the combinations match as little as they can, so <*> finds <b> and </b> separately, not all of <b>bold</b>.",
                        "The exception is the very last thing you type: there they match as much as they can, so c%+ finds all of c123, and ~+ a whole run of spaces (replace it with one space to single-space the text).",
                        "None of them runs on over a paragraph break; \\n (Special chars) finds those.",
                        "",
                        "# Replacement",
                        "$&\tthe whole match",
                        "$1\twhat the first wildcard matched",
                        "$2\tthe second, and so on: $3 … $9, ${10}",
                        "$$\ta $ sign",
                        "\\U \\L\tcapitals, small letters from here on: \\U$1 makes the first wildcard's text capitals",
                        "\\E\tthe case as found again, from here on",
                        "Every wildcard (with its * or +) is a group, numbered from the left. Find @, @ and replace with $2 $1: Smith, John becomes John Smith. Find <*> and replace with nothing to remove tags. Find @ and replace with \\U$1 to put every word in capitals.",
                        "",
                        "# To find a wildcard character",
                        "\\?  \\*  \\.\ta backslash before it: e\\.g\\. finds e.g.",
                        "\\\\\ta backslash",
                        "With Special chars: \\t tab, \\n new line, \\x41 and \\u00E9 a character by its code. Without it, a backslash before anything but a wildcard character is found as itself: \\t is a backslash and a t.",
                        "",
                        "# Options",
                        "Find at\tthe match must be at the start or end of a line or a word",
                        "Whole words\tthe match isn't part of a longer word",
                        "Match case\tcapitals and small letters as typed (&, [a-z] too)",
                        "# numbers like\thow the numbers # finds are written: the decimal point and thousands separator (a space there is any space, as in 1 234,5)",
                        "",
                        "# Learning regular expressions",
                        "Ctrl+Alt+Shift+R turns this search into the same search in Regex mode: the regular expression ClipX makes from your wildcards goes in the Find box.",
                    });
                case SearchMode.Regex:
                    return string.Join("\n", new[]
                    {
                        "A .NET regular expression. Paragraphs count as lines.",
                        "",
                        "# Characters",
                        ".\tany character except a paragraph break",
                        "\\d \\w \\s\ta digit, a word character, white space (\\s takes paragraph breaks too)",
                        "\\D \\W \\S\tanything but those",
                        "\\p{L} \\p{Lu}\ta letter, a capital letter, in any language",
                        "[abc] [^abc]\tone of these; any but these. [a-z] a range",
                        "\\. \\( \\\\\ta character with a meaning, as itself",
                        "",
                        "# How many",
                        "*\tnone or more (as many as possible)",
                        "+\tone or more",
                        "?\tnone or one",
                        "{2} {2,5} {2,}\texactly 2, 2 to 5, 2 or more",
                        "*? +? {2,5}?\tthe same, as few as possible",
                        "",
                        "# Where",
                        "^ $\tthe start, the end of a line (each paragraph)",
                        "\\b\ta word boundary",
                        "\\A \\z\tthe start, the end of the whole text",
                        "(?=…) (?!…)\tfollowed by …; not followed by …",
                        "(?<=…) (?<!…)\tpreceded by …; not preceded by …",
                        "",
                        "# Groups",
                        "(…)\ta group: $1, $2 … in the replacement",
                        "(?<name>…)\ta named group: ${name}",
                        "(?:…)\ta group without a number",
                        "a|b\ta or b",
                        "\\1\tgroup 1's text again: \\b(\\w+) \\1\\b finds doubled words",
                        "",
                        "# Replacement",
                        "$1 ${name}\twhat the group matched",
                        "\\1 … \\9\tthe same as $1 … $9 (as Notepad++ and sed write them); \\0 the whole match",
                        "$&\tthe whole match",
                        "$$\ta $ sign",
                        "\\t \\n \\\\\ta tab, a new line, a backslash (\\xHH and \\uHHHH too)",
                        "\\U \\L\tcapitals, small letters from here on: \\U$1",
                        "\\E\tthe case as found again: \\U$1\\E $2 makes only group 1 capitals",
                        "(?<first>\\w)(?<rest>\\w*) replaced with \\U${first}\\L${rest} puts every word in title case. With \\U \\L \\E in it, Smart replace leaves the case alone.",
                        "Replace All replaces empty matches too: find ^ and replace with \"> \" to quote every line; find $ to add to the end of every line.",
                        "",
                        "# Options",
                        "Match case\twhen off, case is ignored everywhere, so [A-Z] and \\p{Lu} match small letters too; (?-i) in the expression turns it back on",
                        "Whole words\tthe match isn't part of a longer word",
                        "",
                        "# Compared with ClipX 1 (VBA)",
                        "New: look-behind (?<=…), named groups, \\p{…} classes (é and ß are letters), options in the expression (?i) (?s) (?x), atomic groups (?>…), \\A \\z.",
                        "^ and $ always work at every line.",
                        "\\t and \\n work in the replacement.",
                        "A search that takes over 3 seconds is stopped, with a message, rather than hanging ClipX.",
                    });
                default:
                    return string.Join("\n", new[]
                    {
                        "Finds the text exactly as typed.",
                        "",
                        "# Options",
                        "Match case\tonly in the same capitals and small letters",
                        "Whole words\t\"cat\" finds cat, not scatter",
                        "Smart replace\tthe replacement takes the case of what it replaces: cat → dog, Cat → Dog, CAT → DOG",
                        "Special chars\tin both boxes, \\t a tab, \\n a new line (a paragraph break), \\x41 or \\u00E9 the character with that hexadecimal code, \\\\ a backslash. The buttons put them in. Replace \\n with a space to join lines",
                        "From start\tthe next search starts at the start of the text (Previous: at the end)",
                        "In selection\tFind All and Replace All keep to the selected text. Ticked by itself when the panel opens on more than one line",
                        "",
                        "# Keys",
                        "Enter\tnext match (Shift+Enter: previous); in the Replace box, Replace",
                        "F3\tnext match while the panel is open (Shift+F3: previous)",
                        "",
                        "Replace replaces the selected match and goes on to the next. Replace All is one step: Ctrl+Z undoes it.",
                    });
            }
        }
    }
}
