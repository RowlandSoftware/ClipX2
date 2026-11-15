using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Base for anything shown in the side panel (a tool's options or dialogue, replacing ClipX 1's
    /// dialog boxes). Design each one as a UserControl that inherits from this; the
    /// <see cref="SidePanelHost"/> shows one at a time, filling the panel.
    /// </summary>
    public class SidePanelView : UserControl
    {
        /// <summary>Raised when the view wants to close (its Close button, Esc).</summary>
        public event EventHandler CloseRequested;

        /// <summary>Called by the host when the view is shown.</summary>
        protected internal virtual void OnActivated() { }

        /// <summary>Called by the host when the view is hidden (closed, or another view shown).</summary>
        protected internal virtual void OnDeactivated() { }

        protected void RequestClose() => CloseRequested?.Invoke(this, EventArgs.Empty);

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
