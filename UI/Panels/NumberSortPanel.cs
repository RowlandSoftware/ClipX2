using System;
using System.Linq;
using ClipX2.TextEngine;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Numeric sort options, in the side panel: how the numbers lines start with compare (by
    /// value, as version numbers, digit by digit), with a sample sorted as the choice says. Sort
    /// works on the selected lines (or the whole text) and keeps the choice as the last used,
    /// which a right-click on the button uses.
    /// </summary>
    public partial class NumberSortPanel : SidePanelView
    {
        private ClipXRichTextBox _box;
        private Action<string> _status;
        private bool _loading;
        private bool _oldHideSelection;

        /// <summary>The sample: numbers that the three orders put differently.</summary>
        private static readonly string[] SampleLines = { "10 green bottles", "8 blue", "1.10", "1.5", "1.2.1", "004.6", "004.16", "No number" };

        public NumberSortPanel()
        {
            InitializeComponent();
            radioValue.CheckedChanged += (s, e) => OptionsChanged();
            radioVersion.CheckedChanged += (s, e) => OptionsChanged();
            radioDigits.CheckedChanged += (s, e) => OptionsChanged();
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
            ShowOptions(AppSettings.Current.NumberSort);
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
            ShowOptions(options as NumberSortOptions ?? new NumberSortOptions());
        }

        internal override object EditedOptions => Options();

        private void ShowOptions(NumberSortOptions o)
        {
            _loading = true;
            radioValue.Checked = o.Order == NumberOrder.Value;
            radioVersion.Checked = o.Order == NumberOrder.Version;
            radioDigits.Checked = o.Order == NumberOrder.DigitByDigit;
            _loading = false;
            OptionsChanged();
        }

        private NumberSortOptions Options() => new NumberSortOptions
        {
            Order = radioVersion.Checked ? NumberOrder.Version : radioDigits.Checked ? NumberOrder.DigitByDigit : NumberOrder.Value,
        };

        /// <summary>The sample, sorted as the choice says.</summary>
        private void OptionsChanged()
        {
            if (_loading) return;
            var comparer = LineSorter.Comparer(SortKind.Numeric, null, Options());
            labelSample.Text = string.Join(Environment.NewLine, SampleLines.OrderBy(l => l, comparer));
        }

        private void Apply()
        {
            if (_box == null) return;
            NumberSortOptions o = Options();
            AppSettings.Current.NumberSort = o; // the last used: a right-click on the button uses it
            AppSettings.Current.Save();
            _status?.Invoke(SortTools.Sort(_box, SortKind.Numeric, o));
            RaiseApplied();
        }
    }
}
