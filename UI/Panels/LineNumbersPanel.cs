using System;
using ClipX2.TextEngine;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Line numbers options (ClipX 1's Line Numbering dialogue), in the side panel. Apply numbers
    /// the selected lines (or the whole text) and keeps the options as the last used, which a
    /// right-click on the button applies.
    /// </summary>
    public partial class LineNumbersPanel : SidePanelView
    {
        private ClipXRichTextBox _box;
        private Func<bool> _isRichText;
        private Action<string> _status;
        private bool _loading;
        private bool _oldHideSelection;

        // Enough lines to reach two-digit numbers, with a blank line and an indented one
        private const string SampleText =
            "ClipX tidies text\ncopied from one\nprogram before you\npaste it into\nanother.\n\n"
            + "It works a line at\na time or on the\nwhole text:\n    one Undo step\nputs it back.";

        public LineNumbersPanel()
        {
            InitializeComponent();
            numericStart.ValueChanged += (s, e) => OptionsChanged();
            numericStep.ValueChanged += (s, e) => OptionsChanged();
            numericEvery.ValueChanged += (s, e) => OptionsChanged();
            checkBoxIgnoreBlanks.CheckedChanged += (s, e) => OptionsChanged();
            checkBoxPad.CheckedChanged += (s, e) => OptionsChanged();
            radioRight.CheckedChanged += (s, e) => OptionsChanged();
            checkBoxRenumber.CheckedChanged += (s, e) => OptionsChanged();
            buttonApply.Click += (s, e) => Apply();
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

        /// <summary>Plain text / rich text switched: the note (and what Apply can do) depend on it.</summary>
        protected internal override void OnModeChanged() => OptionsChanged();

        protected internal override void OnActivated()
        {
            if (_box == null) return;
            _oldHideSelection = _box.HideSelection;
            _box.HideSelection = false;
            Load(AppSettings.Current.NumLines);
            numericStart.Focus();
        }

        protected internal override void OnDeactivated()
        {
            if (_box != null) _box.HideSelection = _oldHideSelection;
        }

        private void Load(LineNumberOptions o)
        {
            _loading = true;
            numericStart.Value = Clamp(numericStart, o.Start);
            numericStep.Value = Clamp(numericStep, o.Step);
            numericEvery.Value = Clamp(numericEvery, o.Every);
            checkBoxIgnoreBlanks.Checked = o.IgnoreBlanks;
            checkBoxPad.Checked = o.Pad;
            radioRight.Checked = o.RangeRight;
            radioLeft.Checked = !o.RangeRight;
            checkBoxRenumber.Checked = o.Renumber;
            _loading = false;
            OptionsChanged();
        }

        private static decimal Clamp(System.Windows.Forms.NumericUpDown n, int value) =>
            Math.Max(n.Minimum, Math.Min(n.Maximum, value));

        private LineNumberOptions Options() => new LineNumberOptions
        {
            Start = (int)numericStart.Value,
            Step = (int)numericStep.Value,
            Every = (int)numericEvery.Value,
            IgnoreBlanks = checkBoxIgnoreBlanks.Checked,
            Pad = checkBoxPad.Checked,
            RangeRight = radioRight.Checked,
            Renumber = checkBoxRenumber.Checked,
        };

        private void OptionsChanged()
        {
            if (_loading) return;
            radioRight.Enabled = radioLeft.Enabled = checkBoxPad.Checked;
            LineNumberOptions o = Options();
            string numbered = ListFormatter.Apply(SampleText, LineNumbers.Add(SampleText, 0, SampleText.Length, o));
            textBoxSample.Text = numbered.Replace("\n", "\r\n");
            bool rich = _isRichText?.Invoke() ?? false;
            labelNote.Text = rich
                ? "Each paragraph is a line. In rich text the numbers line up only in a fixed-width font."
                : "Each paragraph is a line. Remove line numbers takes them off again, indentation kept.";
        }

        private void Apply()
        {
            if (_box == null) return;
            LineNumberOptions o = Options();
            AppSettings.Current.NumLines = o; // the last used: a right-click on the button applies them
            AppSettings.Current.Save();
            _status?.Invoke(FormatTools.LineNumbers(_box, o));
        }
    }
}
