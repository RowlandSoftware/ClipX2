using System;
using ClipX2.TextEngine;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Bullets' options (ClipX 1's Bullet List Style dialogue), in the side panel: the bullet,
    /// then a tab or padding to a width, with a sample. Apply bullets the selected lines (or the
    /// whole text) and keeps the options as the last used, which a right-click on the Bullets
    /// button applies straight away.
    /// </summary>
    public partial class BulletListPanel : SidePanelView
    {
        private ClipXRichTextBox _box;
        private Func<bool> _isRichText;
        private Action<string> _status;
        private bool _loading;
        private bool _oldHideSelection;

        public BulletListPanel()
        {
            InitializeComponent();
            comboCharacter.TextChanged += (s, e) => OptionsChanged();
            radioTab.CheckedChanged += (s, e) => OptionsChanged();
            numericPad.ValueChanged += (s, e) => OptionsChanged();
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
            Load(AppSettings.Current.BulletList);
            comboCharacter.Focus();
        }

        protected internal override void OnDeactivated()
        {
            if (_box != null) _box.HideSelection = _oldHideSelection;
        }

        private void Load(BulletOptions o)
        {
            _loading = true;
            comboCharacter.Text = o.Character;
            radioTab.Checked = o.Tab;
            radioPad.Checked = !o.Tab;
            numericPad.Value = Math.Max(numericPad.Minimum, Math.Min(numericPad.Maximum, o.PadChars));
            _loading = false;
            OptionsChanged();
        }

        private BulletOptions Options() => new BulletOptions
        {
            Character = comboCharacter.Text.Length > 0 ? comboCharacter.Text : "•",
            Tab = radioTab.Checked,
            PadChars = (int)numericPad.Value,
        };

        private void OptionsChanged()
        {
            if (_loading) return;
            numericPad.Enabled = radioPad.Checked;
            BulletOptions o = Options();
            textBoxSample.Text = Sample(o);
            bool rich = _isRichText?.Invoke() ?? false;
            labelNote.Text = !rich
                ? $"Indent a line by {(o.Tab ? "a tab" : o.PadChars + " spaces")} to put it a level down."
                : ListTools.BulletsAreLive(o)
                    ? $"Rich text: a live list. Indent a line by {(o.Tab ? "a tab" : o.PadChars + " spaces")} to put it a level down."
                    : "Rich text: live lists use •; this bullet is typed in, as in plain text.";
        }

        /// <summary>Three items, one a level down, bulleted with the options.</summary>
        private static string Sample(BulletOptions o)
        {
            string text = "First item\r\nSecond item\r\n" + ListFormatter.Indent(1, o.Tab, o.PadChars) + "Second-level item\r\nThird item";
            return ListFormatter.Apply(text, ListFormatter.BulletEdits(text, 0, text.Length, o));
        }

        private void Apply()
        {
            if (_box == null) return;
            BulletOptions o = Options();
            AppSettings.Current.BulletList = o; // the last used: a right-click on the button applies them
            AppSettings.Current.Save();
            _status?.Invoke(ListTools.Bullets(_box, _isRichText(), o));
        }
    }
}
