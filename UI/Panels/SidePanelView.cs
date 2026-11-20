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

        /// <summary>Called on the shown view when the editor switches between plain and rich text.</summary>
        protected internal virtual void OnModeChanged() { }

        protected void RequestClose() => CloseRequested?.Invoke(this, EventArgs.Empty);

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
            if (keyData == Keys.Escape)
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
