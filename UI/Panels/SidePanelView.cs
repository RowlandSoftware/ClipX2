using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Base for anything shown in the side panel (a tool's options or dialogue, replacing ClipX 1's
    /// dialog boxes). Design each one as a UserControl that inherits from this; the
    /// <see cref="SidePanelHost"/> shows one at a time, filling the panel. Every view gets a close
    /// button (a red cross) at its top right, level with its title, so it needs no Close button of
    /// its own; keep the top-right corner of the layout free for it.
    /// </summary>
    public class SidePanelView : UserControl
    {
        private const int CloseSize = 24, CloseImageSize = 20, CloseMargin = 2;

        private static Image _closeImage;

        private readonly Button _closeButton;
        private readonly ToolTip _toolTip = new ToolTip();

        public SidePanelView()
        {
            _closeButton = new Button
            {
                Name = "buttonCloseCross",
                AccessibleName = "Close",
                Size = new Size(CloseSize, CloseSize),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Image = CloseImage(),
                ImageAlign = ContentAlignment.MiddleCenter,
                TabStop = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            _closeButton.FlatAppearance.BorderSize = 0;
            _closeButton.FlatAppearance.MouseOverBackColor = SystemColors.ControlLight;
            _closeButton.FlatAppearance.MouseDownBackColor = SystemColors.ControlDark;
            _closeButton.Click += (s, e) => RequestClose();
            _toolTip.SetToolTip(_closeButton, "Close (Esc)");
            Controls.Add(_closeButton); // first, so it stays in front of the view's own layout
            PlaceCloseButton();
        }

        /// <summary>Raised when the view wants to close (the close button, Esc).</summary>
        public event EventHandler CloseRequested;

        /// <summary>Called by the host when the view is shown.</summary>
        protected internal virtual void OnActivated() { }

        /// <summary>Called by the host when the view is hidden (closed, or another view shown).</summary>
        protected internal virtual void OnDeactivated() { }

        /// <summary>Connected to the editor (each view's Attach): false until then.</summary>
        internal virtual bool IsAttached => true;

        /// <summary>Called on the shown view when the editor switches between plain and rich text.</summary>
        protected internal virtual void OnModeChanged() { }

        protected void RequestClose() => CloseRequested?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raised after the view has done its work on the text (its Apply; Find and Replace says
        /// which button): a PowerButton waiting on this view (an "Ask" task) then carries on.
        /// </summary>
        public event EventHandler<string> Applied;

        /// <summary>Call after applying to the text; <paramref name="action"/> names the button (for views with several).</summary>
        protected void RaiseApplied(string action = "Apply") => Applied?.Invoke(this, action);

        /************ A PowerButton waiting on this view (an "Ask" task) ************/

        /// <summary>The button that carries on a PowerButton's <paramref name="tag"/> task (its Apply); null if none.</summary>
        internal virtual Button GoButton(string tag) => null;

        /// <summary>The orange of the Go button while a PowerButton waits on the view.</summary>
        internal static readonly Color GoColour = Color.FromArgb(255, 160, 40);

        private Button _go;

        /// <summary>A PowerButton is waiting on this view (an "Ask" task): its Apply works as the PowerButton's tasks do.</summary>
        internal bool WaitedOnByPowerButton => _go != null;
        private string _goText;
        private Color _goBack;
        private bool _goVisualStyle;

        /// <summary>
        /// While a PowerButton waits on the view, its Apply button for <paramref name="tag"/> reads
        /// "Go", in orange, and has the focus (Enter presses it). False if the view has none.
        /// </summary>
        internal bool ShowGo(string tag)
        {
            HideGo();
            Button b = GoButton(tag);
            if (b == null) return false;
            _go = b;
            _goText = b.Text;
            _goBack = b.BackColor;
            _goVisualStyle = b.UseVisualStyleBackColor;
            b.Text = "Go";
            b.UseVisualStyleBackColor = false;
            b.BackColor = GoColour;
            if (b.CanFocus) b.Focus();
            return true;
        }

        /// <summary>The Go button back to its own text and colour.</summary>
        internal void HideGo()
        {
            if (_go == null) return;
            _go.Text = _goText;
            _go.BackColor = _goBack;
            _go.UseVisualStyleBackColor = _goVisualStyle;
            _go = null;
        }

        /************ The PowerButton editor: options to change, not to apply ************/

        /// <summary>
        /// The PowerButton editor shows a tool's view to set a task's own options: the view shows
        /// <paramref name="options"/> (its options class; null for the defaults) and hides the
        /// buttons that act on the text, and its close cross. Views that can't do this do nothing.
        /// </summary>
        internal virtual void EditOptions(object options) { }

        /// <summary>The options as set in the view (in <see cref="EditOptions"/> mode); null if the view has none.</summary>
        internal virtual object EditedOptions => null;

        /// <summary>The view is in the PowerButton editor (<see cref="EditOptions"/>).</summary>
        internal bool IsEditingOptions { get; private set; }

        /// <summary>For <see cref="EditOptions"/>: hides the buttons that act on the text, and the close cross.</summary>
        protected void EnterEditMode(params Control[] actions)
        {
            IsEditingOptions = true;
            foreach (Control c in actions) c.Visible = false;
            _closeButton.Visible = false;
        }

        /// <summary>Puts the focus on the close button (when the view's work is done: Enter or Space then closes it).</summary>
        protected void FocusCloseButton() => _closeButton.Focus();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _closeButton.BringToFront();
            PlaceCloseButton();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            PlaceCloseButton();
        }

        private void PlaceCloseButton() =>
            _closeButton.Location = new Point(Math.Max(0, ClientSize.Width - CloseSize - CloseMargin), CloseMargin);

        /// <summary>The Close resource (32×32), drawn smaller to sit level with a panel title.</summary>
        private static Image CloseImage()
        {
            if (_closeImage != null) return _closeImage;
            Image source = Properties.Resources.Close;
            var small = new Bitmap(CloseImageSize, CloseImageSize);
            using (Graphics g = Graphics.FromImage(small))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(source, new Rectangle(0, 0, CloseImageSize, CloseImageSize));
            }
            return _closeImage = small;
        }

        /// <summary>Esc closes the view while focus is in it.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape && !IsEditingOptions) // in the PowerButton editor, Esc is the editor's
            {
                RequestClose();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsActive => Visible && Parent != null;
    }
}
