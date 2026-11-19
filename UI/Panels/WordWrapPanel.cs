using System;
using ClipX2.TextEngine;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Word wrap options (ClipX 1's Word Wrap dialogue), in the side panel: page width, hanging
    /// indent (a tab or eight spaces) and justification, with a sample. Apply wraps the selected
    /// lines (or the whole text) and keeps the options as the last used, which a right-click on
    /// the Word wrap button applies. Plain text only.
    /// </summary>
    public partial class WordWrapPanel : SidePanelView
    {
        private ClipXRichTextBox _box;
        private Func<bool> _isRichText;
        private Action<string> _status;
        private bool _loading;
        private bool _oldHideSelection;

        private const string SampleText =
            "ClipX tidies text copied from one program before you paste it into another, a line at a time or all at once.";

        public WordWrapPanel()
        {
            InitializeComponent();
            numericWidth.ValueChanged += (s, e) => OptionsChanged();
            checkBoxHanging.CheckedChanged += (s, e) => OptionsChanged();
            radioTab.CheckedChanged += (s, e) => OptionsChanged();
            checkBoxJustify.CheckedChanged += (s, e) => OptionsChanged();
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

        protected internal override void OnActivated()
        {
            if (_box == null) return;
            _oldHideSelection = _box.HideSelection;
            _box.HideSelection = false;
            Load(AppSettings.Current.Wordwrap);
            numericWidth.Focus();
        }

        protected internal override void OnDeactivated()
        {
            if (_box != null) _box.HideSelection = _oldHideSelection;
        }

        private void Load(WordWrapOptions o)
        {
            _loading = true;
            numericWidth.Value = Math.Max(numericWidth.Minimum, Math.Min(numericWidth.Maximum, o.Width));
            checkBoxHanging.Checked = o.Hanging;
            radioTab.Checked = o.Tab;
            radioSpaces.Checked = !o.Tab;
            checkBoxJustify.Checked = o.Justify;
            _loading = false;
            OptionsChanged();
        }

        private WordWrapOptions Options() => new WordWrapOptions
        {
            Width = (int)numericWidth.Value,
            Hanging = checkBoxHanging.Checked,
            Tab = radioTab.Checked,
            Justify = checkBoxJustify.Checked,
        };

        private void OptionsChanged()
        {
            if (_loading) return;
            radioTab.Enabled = radioSpaces.Enabled = checkBoxHanging.Checked;
            WordWrapOptions o = Options();
            // The sample at a third of the width, so it shows a few lines in the panel
            var sample = new WordWrapOptions { Width = Math.Max(12, o.Width / 3), Hanging = o.Hanging, Tab = o.Tab, Justify = o.Justify };
            string wrapped = ListFormatter.Apply(SampleText, LineFormatter.WordWrap(SampleText, 0, SampleText.Length, sample, "\r\n"));
            textBoxSample.Text = wrapped.Replace("\t", "        ");
            labelSampleNote.Text = $"Sample (at {sample.Width} characters):";
            bool rich = _isRichText?.Invoke() ?? false;
            buttonApply.Enabled = !rich;
            labelNote.Text = rich
                ? "Plain text only: rich text wraps to the window."
                : "Each line is a paragraph: use Paragraph first to rejoin text that is already wrapped.";
        }

        private void Apply()
        {
            if (_box == null) return;
            WordWrapOptions o = Options();
            AppSettings.Current.Wordwrap = o; // the last used: a right-click on the button applies them
            AppSettings.Current.Save();
            _status?.Invoke(FormatTools.WordWrap(_box, _isRichText(), o));
        }
    }
}
