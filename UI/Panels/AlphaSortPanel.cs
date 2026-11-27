using System;
using System.Linq;
using ClipX2.TextEngine;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Alphabetic sort options, in the side panel: word by word or letter by letter (strictly or
    /// as Chicago has it), case ignored
    /// or not, with a sample sorted as the options say. Sort works on the selected lines (or the
    /// whole text) and keeps the options as the last used, which a right-click on the button uses.
    /// </summary>
    public partial class AlphaSortPanel : SidePanelView
    {
        private ClipXRichTextBox _box;
        private Action<string> _status;
        private bool _loading;
        private bool _oldHideSelection;

        /// <summary>The sample: lines that word by word, letter by letter and case put in different orders.</summary>
        private static readonly string[] SampleLines = { "Newark", "apple", "New York", "New, Thomas", "newt", "Zebra", "new-born", "Apple", "apple" };

        public AlphaSortPanel()
        {
            InitializeComponent();
            checkBoxCase.CheckedChanged += (s, e) => OptionsChanged();
            checkBoxDuplicates.CheckedChanged += (s, e) => OptionsChanged();
            radioWord.CheckedChanged += (s, e) => OptionsChanged();
            radioLetter.CheckedChanged += (s, e) => OptionsChanged();
            radioStrict.CheckedChanged += (s, e) => OptionsChanged();
            radioChicago.CheckedChanged += (s, e) => OptionsChanged();
            buttonApply.Click += (s, e) => Apply();
        }

        /// <summary>Connects the panel to the editor; once, after creation.</summary>
        internal void Attach(ClipXRichTextBox box, Action<string> status)
        {
            _box = box;
            _status = status;
            OnActivated(); // the host activated the panel before it was attached
        }

        internal override bool IsAttached => _box != null;

        protected internal override void OnActivated()
        {
            if (_box == null) return;
            _oldHideSelection = _box.HideSelection;
            _box.HideSelection = false;
            ShowOptions(AppSettings.Current.AlphaSort);
            buttonApply.Focus();
        }

        protected internal override void OnDeactivated()
        {
            if (_box != null) _box.HideSelection = _oldHideSelection;
        }

        internal override System.Windows.Forms.Button GoButton(string tag) => buttonApply;

        internal override void EditOptions(object options)
        {
            EnterEditMode(buttonApply);
            ShowOptions(options as AlphaSortOptions ?? new AlphaSortOptions());
        }

        internal override object EditedOptions => Options();

        private void ShowOptions(AlphaSortOptions o)
        {
            _loading = true;
            checkBoxCase.Checked = o.CaseSensitive;
            checkBoxDuplicates.Checked = o.RemoveDuplicates;
            radioLetter.Checked = o.LetterByLetter;
            radioWord.Checked = !o.LetterByLetter;
            radioChicago.Checked = o.LetterRule == LetterByLetterRule.Chicago;
            radioStrict.Checked = !radioChicago.Checked;
            _loading = false;
            OptionsChanged();
        }

        private AlphaSortOptions Options() => new AlphaSortOptions
        {
            CaseSensitive = checkBoxCase.Checked,
            LetterByLetter = radioLetter.Checked,
            RemoveDuplicates = checkBoxDuplicates.Checked,
            LetterRule = radioChicago.Checked ? LetterByLetterRule.Chicago : LetterByLetterRule.Strict,
        };

        /// <summary>The sample, sorted as the options say.</summary>
        private void OptionsChanged()
        {
            if (_loading) return;
            panelLetterRule.Enabled = radioLetter.Checked; // the rule is letter by letter's
            AlphaSortOptions o = Options();
            var comparer = LineSorter.Comparer(SortKind.Alphabetic, null, o);
            var lines = o.RemoveDuplicates ? SampleLines.Distinct(StringComparer.Ordinal) : SampleLines;
            labelSample.Text = string.Join(Environment.NewLine, lines.OrderBy(l => l, comparer));
        }

        private void Apply()
        {
            if (_box == null) return;
            AlphaSortOptions o = Options();
            AppSettings.Current.AlphaSort = o; // the last used: a right-click on the button uses them
            AppSettings.Current.Save();
            _status?.Invoke(SortTools.Sort(_box, SortKind.Alphabetic, o));
            RaiseApplied();
        }
    }
}
