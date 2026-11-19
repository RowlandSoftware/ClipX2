using System;
using ClipX2.TextEngine;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Remove extra lines options (ClipX 1's Remove Lines dialogue), in the side panel. Apply works
    /// on the selected lines (or the whole text) and keeps the options as the last used, which a
    /// right-click on the button applies.
    /// </summary>
    public partial class RemoveLinesPanel : SidePanelView
    {
        private ClipXRichTextBox _box;
        private Action<string> _status;
        private bool _loading;
        private bool _oldHideSelection;

        public RemoveLinesPanel()
        {
            InitializeComponent();
            checkBoxMultiple.CheckedChanged += (s, e) => OptionsChanged();
            checkBoxSingle.CheckedChanged += (s, e) => OptionsChanged();
            buttonApply.Click += (s, e) => Apply();
            buttonClose.Click += (s, e) => RequestClose();
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
            _box.HideSelection = false;
            RemoveLinesOptions o = AppSettings.Current.RemLines;
            _loading = true;
            checkBoxMultiple.Checked = o.Multiple;
            checkBoxSingle.Checked = o.Single;
            _loading = false;
            OptionsChanged();
            checkBoxMultiple.Focus();
        }

        protected internal override void OnDeactivated()
        {
            if (_box != null) _box.HideSelection = _oldHideSelection;
        }

        private RemoveLinesOptions Options() => new RemoveLinesOptions
        {
            Multiple = checkBoxMultiple.Checked,
            Single = checkBoxSingle.Checked,
        };

        private void OptionsChanged()
        {
            if (_loading) return;
            RemoveLinesOptions o = Options();
            buttonApply.Enabled = o.Single || o.Multiple;
            labelEffect.Text = o.Single && o.Multiple ? "All blank lines will go."
                : o.Single ? "Lone blank lines will go; runs of them stay."
                : o.Multiple ? "Runs of blank lines will become one."
                : "Tick at least one.";
        }

        private void Apply()
        {
            if (_box == null) return;
            RemoveLinesOptions o = Options();
            AppSettings.Current.RemLines = o; // the last used: a right-click on the button applies them
            AppSettings.Current.Save();
            _status?.Invoke(FormatTools.RemoveExtraLines(_box, o));
        }
    }
}
