using System;
using ClipX2.TextEngine;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Numbers' options (ClipX 1's Number List Style dialogue), in the side panel: style,
    /// punctuation, start, a tab or padding to a width, Renumber, with a sample. Apply numbers the
    /// selected lines (or the whole text) and keeps the options as the last used, which a
    /// right-click on the Numbers button applies straight away.
    /// </summary>
    public partial class NumberListPanel : SidePanelView
    {
        private ClipXRichTextBox _box;
        private Func<bool> _isRichText;
        private Action<string> _status;
        private bool _loading;
        private bool _oldHideSelection;

        public NumberListPanel()
        {
            InitializeComponent();
            comboStyle.SelectedIndexChanged += (s, e) => OptionsChanged();
            comboPunctuation.SelectedIndexChanged += (s, e) => OptionsChanged();
            numericStart.ValueChanged += (s, e) => OptionsChanged();
            radioTab.CheckedChanged += (s, e) => OptionsChanged();
            numericPad.ValueChanged += (s, e) => OptionsChanged();
            checkBoxRenumber.CheckedChanged += (s, e) => OptionsChanged();
            buttonApply.Click += (s, e) => Apply();
            buttonClose.Click += (s, e) => RequestClose();
        }

        /// <summary>Connects the panel to the editor; once, after creation.</summary>
        internal void Attach(ClipXRichTextBox box, Func<bool> isRichText, Action<string> status)
        {
            _box = box;
            _isRichText = isRichText;
            _status = status;
            OnActivated(); // the host activated the panel before it was attached
        }

        internal bool IsAttached => _box != null;

        /// <summary>Shows the last-used options; keeps the editor's selection visible (it is what Apply works on).</summary>
        protected internal override void OnActivated()
        {
            if (_box == null) return;
            _oldHideSelection = _box.HideSelection;
            _box.HideSelection = false;
            Load(AppSettings.Current.NumList);
            comboStyle.Focus();
        }

        protected internal override void OnDeactivated()
        {
            if (_box != null) _box.HideSelection = _oldHideSelection;
        }

        private void Load(NumberOptions o)
        {
            _loading = true;
            comboStyle.SelectedIndex = Math.Max(0, Math.Min(comboStyle.Items.Count - 1, (int)o.Style));
            comboPunctuation.SelectedIndex = Math.Max(0, Math.Min(comboPunctuation.Items.Count - 1, (int)o.FollowChar));
            numericStart.Value = Math.Max(numericStart.Minimum, Math.Min(numericStart.Maximum, o.StartNumber));
            radioTab.Checked = o.Tab;
            radioPad.Checked = !o.Tab;
            numericPad.Value = Math.Max(numericPad.Minimum, Math.Min(numericPad.Maximum, o.PadChars));
            checkBoxRenumber.Checked = o.Renumber;
            _loading = false;
            OptionsChanged();
        }

        private NumberOptions Options() => new NumberOptions
        {
            Style = (NumberStyle)Math.Max(0, comboStyle.SelectedIndex),
            FollowChar = (NumberPunctuation)Math.Max(0, comboPunctuation.SelectedIndex),
            StartNumber = (int)numericStart.Value,
            Tab = radioTab.Checked,
            PadChars = (int)numericPad.Value,
            Renumber = checkBoxRenumber.Checked,
        };

        private void OptionsChanged()
        {
            if (_loading) return;
            NumberOptions o = Options();
            bool legal = o.Style == NumberStyle.Legal;
            numericPad.Enabled = radioPad.Checked;
            comboPunctuation.Enabled = !legal; // legal numbers are always 1.2.
            bool rich = _isRichText?.Invoke() ?? false;
            textBoxSample.Text = rich && ListTools.NumbersAreLive(o) ? LiveSample(o) : Sample(o);
            string indent = o.Tab ? "a tab" : o.PadChars + " spaces";
            labelNote.Text = !rich
                ? legal
                    ? $"Indent a line by {indent} per level: 1., 1.1., 1.1.1."
                    : $"Lines indented by {indent} or more are moved along, not numbered."
                : legal
                    ? $"Rich text: legal numbers are typed in (live lists can't do 1.1). Indent by {indent} per level."
                    : $"Rich text: a live list. Indent a line by {indent} per level; levels go 1 → a → i.";
        }

        /// <summary>Five items, two a level down, numbered with the options.</summary>
        private static string Sample(NumberOptions o)
        {
            string sub = ListFormatter.Indent(1, o.Tab, o.PadChars);
            string text = "First item\r\nSecond item\r\n" + sub + "Sub-item\r\n" + sub + "Sub-item\r\nThird item";
            return ListFormatter.Apply(text, ListFormatter.NumberEdits(text, 0, text.Length, o));
        }

        /// <summary>How a live list will look: the sub-items numbered in the next style down (1 → a → i → I → A).</summary>
        private static string LiveSample(NumberOptions o)
        {
            NumberStyle[] cascade = { NumberStyle.RomanCap, NumberStyle.LettersCap, NumberStyle.Arabic, NumberStyle.Letters, NumberStyle.Roman };
            NumberStyle sub = cascade[(Array.IndexOf(cascade, o.Style) + 1) % cascade.Length];
            string N(int n, NumberStyle s) => ListFormatter.FormatNumber(n, s, o.FollowChar) + "\t";
            int first = o.StartNumber;
            return N(first, o.Style) + "First item\r\n" + N(first + 1, o.Style) + "Second item\r\n" +
                   "\t" + N(1, sub) + "Sub-item\r\n\t" + N(2, sub) + "Sub-item\r\n" + N(first + 2, o.Style) + "Third item";
        }

        private void Apply()
        {
            if (_box == null) return;
            NumberOptions o = Options();
            AppSettings.Current.NumList = o; // the last used: a right-click on the button applies them
            AppSettings.Current.Save();
            _status?.Invoke(ListTools.Numbers(_box, _isRichText(), o));
        }
    }
}
