using System;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using ClipX2.TextEngine;

namespace ClipX2.UI
{
    /// <summary>
    /// Quick search in the bottom bar. Ctrl+F goes to the box; Enter finds the text from the
    /// cursor downwards (Shift+Enter upwards) and selects it. ▲ ▼ (and F3, Shift+F3 while Quick
    /// search is active) find the previous and next match. At the end of the text it says so
    /// (with a beep) and stays put; the next search in the same direction carries on from the
    /// other end. The Find all button highlights every match instead, leaving the cursor where
    /// it is; the highlights go as soon as anything else is done. Esc goes back to the text: the
    /// last match stays selected, or the highlights go. Recent searches are kept in the
    /// drop-down, latest first, and in the settings for next time.
    /// </summary>
    internal sealed class QuickSearch
    {
        private const int HistoryLimit = 15;

        private readonly ClipXRichTextBox _box;
        private readonly ToolStripComboBox _combo;
        private readonly ToolStripCheckBox _matchCase, _wholeWords;
        private readonly Action<string> _status;
        private bool _highlighting;      // the box's markers are Find all's
        private bool? _wrapForward;      // reached the end (true) or start (false): the next step goes round
        private string _wrapTerm;

        public QuickSearch(ClipXRichTextBox box, ToolStripComboBox combo,
            ToolStripCheckBox matchCase, ToolStripCheckBox wholeWords, ToolStripButton findAll,
            ToolStripButton previous, ToolStripButton next, Action<string> status)
        {
            _box = box;
            _combo = combo;
            _matchCase = matchCase;
            _wholeWords = wholeWords;
            _status = status;

            foreach (string term in AppSettings.Current.QuickSearchHistory.Take(HistoryLimit))
                _combo.Items.Add(term);

            _combo.KeyDown += ComboKeyDown;
            _combo.Enter += (s, e) => IsActive = true;
            previous.Click += (s, e) => { IsActive = true; Step(forward: false); };
            next.Click += (s, e) => { IsActive = true; Step(forward: true); };
            findAll.Click += (s, e) => FindAll();
            _matchCase.CheckedChanged += (s, e) => OptionsChanged();
            _wholeWords.CheckedChanged += (s, e) => OptionsChanged();
            // A click in the text ends the search: highlights go, and F3 / Shift+F3 are the text's again
            _box.MouseDown += (s, e) => Deactivate();
        }

        /// <summary>
        /// Quick search is in use (from Ctrl+F, the box, a search or an arrow until Esc, a click in
        /// the text or another tool): F3 and Shift+F3 find the next and previous match.
        /// </summary>
        public bool IsActive { get; private set; }

        private string Term => _combo.Text;

        private StringComparison Comparison => _matchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        /// <summary>Ctrl+F: to the search box, its text selected for typing over.</summary>
        public void Activate()
        {
            IsActive = true;
            _combo.Focus();
            _combo.SelectAll();
        }

        /// <summary>Another tool was used, or the text clicked: Find all's highlights go, and the search is no longer active.</summary>
        public void Deactivate()
        {
            ClearHighlights();
            IsActive = false;
        }

        /// <summary>F3 / Shift+F3 (and ▲ ▼): the next or previous match.</summary>
        public void FindNext(bool forward) => Step(forward);

        private void OptionsChanged()
        {
            _wrapForward = null;
            if (_highlighting) Highlight();
        }

        private void ComboKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // no beep
                if (_combo.DroppedDown) _combo.DroppedDown = false;
                if (Term.Length == 0) return;
                IsActive = true;
                Step(forward: !e.Shift);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (_combo.DroppedDown) return; // closes the list first
                e.SuppressKeyPress = true;
                Deactivate();   // after Find all, the highlights go and the cursor is where it was
                _status("");
                _box.Focus();   // otherwise the last match stays selected
            }
        }

        /// <summary>Selects the next match after the cursor (or the previous one before it).</summary>
        private void Step(bool forward)
        {
            string term = Term;
            if (term.Length == 0) return;
            ClearHighlights(); // anything after Find all ends its highlights
            Remember(term);
            string text = _box.Text;
            int start = _box.SelectionStart, length = _box.SelectionLength;
            // Already on a match (the last one found): carry on past it
            bool onMatch = length == term.Length && start + length <= text.Length
                && string.Compare(text, start, term, 0, length, Comparison) == 0;
            int from = forward ? (onMatch ? start + length : start) : start;
            // Told "Reached end/start of text" last time: this time go round from the other end
            if (_wrapForward == forward && _wrapTerm == term) from = forward ? 0 : text.Length;
            _wrapForward = null;

            int found = TextSearch.Find(text, term, from, forward, _matchCase.Checked, _wholeWords.Checked);
            if (found < 0)
            {
                bool anywhere = TextSearch.Find(text, term, 0, true, _matchCase.Checked, _wholeWords.Checked) >= 0;
                if (anywhere) { _wrapForward = forward; _wrapTerm = term; }
                _status(!anywhere ? "Not found" : forward ? "Reached end of text" : "Reached start of text");
                SystemSounds.Beep.Play();
                return;
            }
            _box.Select(found, term.Length);
            _box.ScrollToCaret();
            _status("");
        }

        /// <summary>The Find all button: the search remembered, every match highlighted.</summary>
        private void FindAll()
        {
            if (Term.Length == 0) return;
            IsActive = true;
            Remember(Term);
            Highlight();
        }

        /// <summary>Every match highlighted as the spelling panel marks a word; the cursor stays put.</summary>
        private void Highlight()
        {
            string term = Term;
            if (term.Length == 0) { ClearHighlights(); return; }
            var found = TextSearch.FindAll(_box.Text, term, _matchCase.Checked, _wholeWords.Checked);
            if (found.Count == 0)
            {
                ClearHighlights();
                _status("Not found");
                SystemSounds.Beep.Play();
                return;
            }
            _box.SetMarkers(found.Select(i => (i, term.Length)));
            _highlighting = true;
            _status(found.Count == 1 ? "1 found" : $"{found.Count:N0} found");
        }

        private void ClearHighlights()
        {
            if (!_highlighting) return;
            _highlighting = false;
            _box.ClearMarker();
        }

        /// <summary>
        /// The term to the top of the drop-down (moved there if already in it), and saved in the
        /// settings so the list is there next time.
        /// </summary>
        private void Remember(string term)
        {
            int index = -1;
            for (int i = 0; i < _combo.Items.Count; i++)
                if ((string)_combo.Items[i] == term) { index = i; break; }
            if (index == 0) return;
            if (index > 0) _combo.Items.RemoveAt(index);
            _combo.Items.Insert(0, term);
            while (_combo.Items.Count > HistoryLimit) _combo.Items.RemoveAt(_combo.Items.Count - 1);
            if (_combo.Text != term) _combo.Text = term; // removing the item may have cleared it
            _combo.SelectionStart = term.Length;

            AppSettings.Current.QuickSearchHistory = _combo.Items.Cast<string>().ToList();
            AppSettings.Current.Save();
        }
    }
}
