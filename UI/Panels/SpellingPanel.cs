using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClipX2.TextEngine;
using ClipX2.TextEngine.Spelling;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Spelling checker in the side panel (ClipX 1's dlgSpelling). Works on the editor's text
    /// directly: each Change is one Undo step in the editor and keeps the formatting.
    /// Checks from the caret to the end, then from the top to where it started; with a selection,
    /// only the selection. If the user edits the text meanwhile, it picks up from the caret.
    /// </summary>
    public partial class SpellingPanel : SidePanelView
    {
        private ClipXRichTextBox _box;
        private Func<string, Task<SpellChecker>> _loader;
        private SpellChecker _spell;
        private SpellingIssue _issue;

        private int _startPos;        // where the check began (moves with edits before it)
        private bool _canWrap;        // started part-way through the whole text, so go round to the top at the end
        private bool _secondPass;     // gone round: now checking from the top to where it started
        private int _limitEnd = -1;   // end of the selection being checked, -1 for the whole text
        private int _found;           // issues shown in this check
        private bool _oldHideSelection;

        public SpellingPanel()
        {
            InitializeComponent();

            buttonIgnore.Click += (s, e) => Act(Ignore);
            buttonIgnoreAll.Click += (s, e) => Act(IgnoreAll);
            buttonChange.Click += (s, e) => Act(Change);
            buttonChangeAll.Click += (s, e) => Act(ChangeAll);
            buttonAdd.Click += (s, e) => Act(AddToDictionary);
            buttonDelete.Click += (s, e) => Act(Delete);
            buttonClose.Click += (s, e) => RequestClose();

            listBoxSuggestions.SelectedIndexChanged += (s, e) =>
            {
                if (listBoxSuggestions.SelectedItem is string w) textBoxChangeTo.Text = w;
            };
            listBoxSuggestions.DoubleClick += (s, e) => { if (listBoxSuggestions.SelectedItem != null) Act(Change); };
            comboLanguage.SelectionChangeCommitted += async (s, e) => await ChangeLanguage();

            ShowFinished(null);
        }

        /// <summary>Connects the panel to the editor and the dictionary loader (once, after creation).</summary>
        internal void Attach(ClipXRichTextBox box, Func<string, Task<SpellChecker>> loader,
            IReadOnlyList<string> locales, string locale)
        {
            _box = box;
            _loader = loader;
            comboLanguage.Items.Clear();
            foreach (string l in locales) comboLanguage.Items.Add(l);
            comboLanguage.SelectedItem = locale;
        }

        internal bool IsAttached => _box != null;

        /// <summary>The editor's selection must stay visible while focus is in the panel.</summary>
        protected internal override void OnActivated()
        {
            if (_box == null) return;
            _oldHideSelection = _box.HideSelection;
            _box.HideSelection = false;
        }

        protected internal override void OnDeactivated()
        {
            if (_box == null) return;
            _box.HideSelection = _oldHideSelection;
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

            int selStart = _box.SelectionStart, selLen = _box.SelectionLength;
            if (_issue != null && selStart == _issue.Start && selLen == _issue.Length)
            {
                FindFrom(_issue.End); // F7 on the issue being shown: skip it, same check
                return;
            }

            _found = 0;
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
            FindFrom(selStart);
        }

        /************ Finding ************/

        private void FindFrom(int pos)
        {
            string text = _box.Text;
            int stop = _limitEnd >= 0 ? _limitEnd : (_secondPass ? _startPos : text.Length);
            SpellingIssue issue = pos < stop ? _spell.FindNext(text, pos, wrap: false) : null;
            if (issue != null && issue.Start >= stop) issue = null;

            if (issue == null && _canWrap && !_secondPass)
            {
                _secondPass = true;  // go round to the top, up to where we started
                issue = _spell.FindNext(text, 0, wrap: false);
                if (issue != null && issue.Start >= _startPos) issue = null;
            }

            if (issue == null)
                ShowFinished(_found == 0 ? "No spelling errors found." : "Spelling check complete.");
            else
                ShowIssue(issue, text);
        }

        private void ShowIssue(SpellingIssue issue, string text)
        {
            _issue = issue;
            _found++;
            bool repeated = issue.Kind == SpellingIssueKind.RepeatedWord;

            _box.Select(issue.Start, issue.Length);
            _box.ScrollToCaret();

            labelKind.Text = repeated ? "Repeated word:" : "Not in dictionary:";
            ShowContext(text, issue);

            listBoxSuggestions.BeginUpdate();
            listBoxSuggestions.Items.Clear();
            IReadOnlyList<string> suggestions = _spell.Suggest(issue, 10);
            foreach (string s in suggestions) listBoxSuggestions.Items.Add(s);
            listBoxSuggestions.EndUpdate();

            // Repeated word: empty "Change to" means Change deletes it, as in ClipX 1
            textBoxChangeTo.Text = repeated ? "" : suggestions.Count > 0 ? suggestions[0] : issue.Word;
            if (!repeated && suggestions.Count > 0) listBoxSuggestions.SelectedIndex = 0;
            else if (!repeated) listBoxSuggestions.Items.Add("(no suggestions)");
            listBoxSuggestions.Enabled = suggestions.Count > 0;

            SetButtons(true, repeated);

            textBoxChangeTo.Focus();
            textBoxChangeTo.SelectAll();
        }

        /// <summary>The words around the issue, with the issue in red, cut at the paragraph.</summary>
        private void ShowContext(string text, SpellingIssue issue)
        {
            const int span = 90;
            int from = issue.Start, to = issue.End;
            while (from > 0 && issue.Start - from < span && text[from - 1] != '\n') from--;
            while (to < text.Length && to - issue.End < span && text[to] != '\n') to++;
            // don't start or end mid-word
            if (from > 0 && text[from - 1] != '\n') while (from < issue.Start && !char.IsWhiteSpace(text[from - 1])) from++;
            if (to < text.Length && text[to] != '\n') while (to > issue.End && !char.IsWhiteSpace(text[to])) to--;

            string before = (from > 0 && text[from - 1] != '\n' ? "…" : "") + text.Substring(from, issue.Start - from);
            string after = text.Substring(issue.End, to - issue.End) + (to < text.Length && text[to] != '\n' ? "…" : "");

            var rtb = richTextBoxContext;
            rtb.Clear();
            rtb.SelectionColor = SystemColors.WindowText;
            rtb.AppendText(before);
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionColor = Color.Red;
            rtb.SelectionFont = new Font(rtb.Font, FontStyle.Bold);
            rtb.AppendText(issue.Word);
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionColor = SystemColors.WindowText;
            rtb.SelectionFont = rtb.Font;
            rtb.AppendText(after);
            rtb.SelectionStart = before.Length;
            rtb.ScrollToCaret();
        }

        private void ShowFinished(string message)
        {
            _issue = null;
            labelKind.Text = "";
            richTextBoxContext.Clear();
            textBoxChangeTo.Text = "";
            listBoxSuggestions.Items.Clear();
            SetButtons(false, false);
            labelKind.Text = message ?? "";
            if (message != null) buttonClose.Focus();
        }

        private void SetButtons(bool on, bool repeated)
        {
            buttonIgnore.Enabled = buttonIgnoreAll.Enabled = buttonChange.Enabled = buttonDelete.Enabled = on;
            buttonChangeAll.Enabled = buttonAdd.Enabled = on && !repeated;
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
            _spell.IgnoreOnce(_issue);
            FindFrom(_issue.End);
        }

        private void IgnoreAll()
        {
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
            labelKind.Text = (_issue.Kind == SpellingIssueKind.RepeatedWord ? "Repeated word:" : "Not in dictionary:")
                + $"   ('{typed}' added)";
            buttonChange.Focus();
        }

        /// <summary>
        /// Replaces the word. Carries on from the start of the replacement, so if what the user
        /// typed is itself wrong, it is shown next (ClipX 1 re-checked in the same way).
        /// </summary>
        private void Change()
        {
            string replacement = textBoxChangeTo.Text;
            if (replacement.Length == 0) { Delete(); return; }
            TextEdit edit = _spell.Change(_issue, replacement, checkBoxSmartReplace.Checked);
            Apply(new[] { edit });
            FindFrom(_issue.Start);
        }

        private void ChangeAll()
        {
            string replacement = textBoxChangeTo.Text;
            if (replacement.Length == 0) return;
            if (!AllCorrect(replacement))
            {
                if (MessageBox.Show(this, $"'{replacement}' may be misspelt.\n\nChange every '{_issue.Word}' to it?",
                        "Change All", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                _spell.IgnoreAll(replacement); // accepted, so don't flag the changed words
            }

            IReadOnlyList<TextEdit> edits = _spell.ChangeAll(_box.Text, _issue, replacement, checkBoxSmartReplace.Checked);
            if (edits.Count == 0) edits = new[] { _spell.Change(_issue, replacement, checkBoxSmartReplace.Checked) }; // never leave it stuck on the word
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
            TextEdit edit = _spell.Delete(_box.Text, _issue);
            Apply(new[] { edit });
            FindFrom(edit.Start);
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
            if (_loader == null || !(comboLanguage.SelectedItem is string locale)) return;
            if (_spell != null && _spell.Dictionary.Locale == locale) return;
            int from = _issue?.Start ?? _box.SelectionStart;
            labelKind.Text = "Loading dictionary…";
            try { _spell = await _loader(locale); }
            catch (Exception ex) { ShowFinished("The dictionary could not be loaded: " + ex.Message); return; }
            labelKind.Text = "";
            if (_issue != null) FindFrom(from);
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
    }
}
