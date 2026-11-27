using System;
using System.Linq;
using ClipX2.TextEngine;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Smart sort options, in the side panel: a style (NISO TR03-1999, BS ISO 999 as Hart's has
    /// it, Chicago) that sets the rule options together, the options themselves (word or letter
    /// by letter, stopping at a comma, text in brackets, Mc and St), how numbers compare, case
    /// and duplicates, with a sample sorted as the options say. Sort works on the selected lines
    /// (or the whole text) and keeps the options as the last used, which a right-click uses.
    /// </summary>
    public partial class SmartSortPanel : SidePanelView
    {
        private ClipXRichTextBox _box;
        private Action<string> _status;
        private bool _loading;
        private bool _oldHideSelection;

        /// <summary>The sample: lines that the styles put in different orders.</summary>
        private static readonly string[] SampleLines = { "Bath bun", "bathhouse", "Bath, Order of the", "BBC", "McKay", "Mackay", "NATO", "high (light-headed)", "High, J.", "item 10", "item 2" };

        /// <summary>The style list's entries, in order.</summary>
        private static readonly SmartSortPreset[] Presets =
        {
            SmartSortPreset.Custom, SmartSortPreset.NisoTr03, SmartSortPreset.BsIso999WordByWord, SmartSortPreset.BsIso999LetterByLetter,
            SmartSortPreset.ChicagoWordByWord, SmartSortPreset.ChicagoLetterByLetter,
        };

        /// <summary>The numbers list's entries, in order.</summary>
        private static readonly NumberOrder[] NumberOrders = { NumberOrder.Value, NumberOrder.Version, NumberOrder.DigitByDigit };

        public SmartSortPanel()
        {
            InitializeComponent();
            comboPreset.SelectedIndexChanged += (s, e) => PresetChosen();
            foreach (var c in new System.Windows.Forms.ButtonBase[] { radioWord, radioLetter, checkBoxComma, checkBoxBrackets, checkBoxMc, checkBoxSaint, checkBoxInitialisms, checkBoxCase, checkBoxDuplicates })
            {
                if (c is System.Windows.Forms.RadioButton r) r.CheckedChanged += (s, e) => OptionsChanged();
                else ((System.Windows.Forms.CheckBox)c).CheckedChanged += (s, e) => OptionsChanged();
            }
            comboNumbers.SelectedIndexChanged += (s, e) => OptionsChanged();
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
            ShowOptions(AppSettings.Current.SmartSort);
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
            ShowOptions(options as SmartSortOptions ?? new SmartSortOptions());
        }

        internal override object EditedOptions => Options();

        private void ShowOptions(SmartSortOptions o)
        {
            _loading = true;
            radioLetter.Checked = o.LetterByLetter;
            radioWord.Checked = !o.LetterByLetter;
            checkBoxComma.Checked = o.StopAtComma;
            checkBoxBrackets.Checked = o.IgnoreBracketed;
            checkBoxMc.Checked = o.McAsMac;
            checkBoxSaint.Checked = o.StAsSaint;
            checkBoxInitialisms.Checked = o.InitialismsAsLetters;
            comboNumbers.SelectedIndex = Math.Max(0, Array.IndexOf(NumberOrders, o.Numbers));
            checkBoxCase.Checked = o.CaseSensitive;
            checkBoxDuplicates.Checked = o.RemoveDuplicates;
            _loading = false;
            OptionsChanged();
        }

        private SmartSortOptions Options() => new SmartSortOptions
        {
            LetterByLetter = radioLetter.Checked,
            StopAtComma = checkBoxComma.Checked,
            IgnoreBracketed = checkBoxBrackets.Checked,
            McAsMac = checkBoxMc.Checked,
            StAsSaint = checkBoxSaint.Checked,
            InitialismsAsLetters = checkBoxInitialisms.Checked,
            Numbers = NumberOrders[Math.Max(0, comboNumbers.SelectedIndex)],
            CaseSensitive = checkBoxCase.Checked,
            RemoveDuplicates = checkBoxDuplicates.Checked,
        };

        /// <summary>A style chosen: its options shown (Custom leaves them as they are).</summary>
        private void PresetChosen()
        {
            if (_loading || comboPreset.SelectedIndex <= 0) return;
            SmartSortOptions o = Options();
            o.Apply(Presets[comboPreset.SelectedIndex]);
            ShowOptions(o);
        }

        /// <summary>The style the options match, and the sample sorted as they say.</summary>
        private void OptionsChanged()
        {
            if (_loading) return;
            SmartSortOptions o = Options();
            _loading = true;
            comboPreset.SelectedIndex = Array.IndexOf(Presets, o.Preset());
            _loading = false;
            var comparer = LineSorter.Comparer(SortKind.Smart, null, o);
            var lines = o.RemoveDuplicates ? SampleLines.Distinct(StringComparer.Ordinal) : SampleLines;
            labelSample.Text = string.Join(Environment.NewLine, lines.OrderBy(l => l, comparer));
        }

        private void Apply()
        {
            if (_box == null) return;
            SmartSortOptions o = Options();
            AppSettings.Current.SmartSort = o; // the last used: a right-click on the button uses them
            AppSettings.Current.Save();
            _status?.Invoke(SortTools.Sort(_box, SortKind.Smart, o));
            RaiseApplied();
        }
    }
}
