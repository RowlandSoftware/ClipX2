using System;
using ClipX2.TextEngine;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Indent and Unindent options (ClipX 1's Indent dialogue), in the side panel: a tab, a number
    /// of spaces, or some text (e.g. "> " to quote), with Indent and Unindent buttons. Either keeps
    /// the options as the last used, which a right-click on the Indent or Unindent button applies.
    /// </summary>
    public partial class IndentPanel : SidePanelView
    {
        private ClipXRichTextBox _box;
        private Action<string> _status;
        private bool _loading;
        private bool _oldHideSelection;

        public IndentPanel()
        {
            InitializeComponent();
            radioTab.CheckedChanged += (s, e) => OptionsChanged();
            radioSpaces.CheckedChanged += (s, e) => OptionsChanged();
            radioText.CheckedChanged += (s, e) => OptionsChanged();
            numericSpaces.ValueChanged += (s, e) => OptionsChanged();
            comboText.TextChanged += (s, e) => { if (!_loading && comboText.Focused) radioText.Checked = true; OptionsChanged(); };
            buttonIndent.Click += (s, e) => Apply(indent: true);
            buttonUnindent.Click += (s, e) => Apply(indent: false);
        }

        /// <summary>Connects the panel to the editor; once, after creation.</summary>
        internal void Attach(ClipXRichTextBox box, Action<string> status)
        {
            _box = box;
            _status = status;
            OnActivated(); // the host activated the panel before it was attached
        }

        internal bool IsAttached => _box != null;

        protected internal override void OnActivated()
        {
            if (_box == null) return;
            _oldHideSelection = _box.HideSelection;
            _box.HideSelection = false; // the selection is what the buttons work on
            Load(AppSettings.Current.Indent);
        }

        protected internal override void OnDeactivated()
        {
            if (_box != null) _box.HideSelection = _oldHideSelection;
        }

        private void Load(IndentOptions o)
        {
            _loading = true;
            radioSpaces.Checked = o.Option == "Spaces";
            radioText.Checked = o.Option == "Text";
            radioTab.Checked = !radioSpaces.Checked && !radioText.Checked;
            numericSpaces.Value = Math.Max(numericSpaces.Minimum, Math.Min(numericSpaces.Maximum, o.Spaces));
            comboText.Text = o.Text ?? "";
            _loading = false;
            OptionsChanged();
        }

        private IndentOptions Options() => new IndentOptions
        {
            Option = radioSpaces.Checked ? "Spaces" : radioText.Checked ? "Text" : "Tab",
            Spaces = (int)numericSpaces.Value,
            Text = comboText.Text,
        };

        private void OptionsChanged()
        {
            if (_loading) return;
            numericSpaces.Enabled = radioSpaces.Checked;
            IndentOptions o = Options();
            bool any = o.IndentText.Length > 0;
            buttonIndent.Enabled = buttonUnindent.Enabled = any;
            string text = "First line\r\nSecond line\r\n\r\nAfter a blank line";
            textBoxSample.Text = ListFormatter.Apply(text, LineFormatter.Indent(text, 0, text.Length, o)).Replace("\t", "→   ");
        }

        private void Apply(bool indent)
        {
            if (_box == null) return;
            IndentOptions o = Options();
            o.Unindent = !indent;           // a right-click on the button repeats this one
            AppSettings.Current.Indent = o; // with these settings
            AppSettings.Current.Save();
            _status?.Invoke(indent ? FormatTools.Indent(_box, o) : FormatTools.Unindent(_box, o));
        }
    }
}
