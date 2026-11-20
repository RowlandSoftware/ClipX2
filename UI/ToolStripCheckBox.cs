using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ClipX2.UI
{
    /// <summary>
    /// A check box on a toolstrip (the quick search options in the bottom bar). Shows in the
    /// designer's "Add ToolStripItem" list for toolstrips and status strips.
    /// </summary>
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
    [DefaultEvent(nameof(CheckedChanged))]
    public class ToolStripCheckBox : ToolStripControlHost
    {
        public ToolStripCheckBox() : base(new CheckBox { AutoSize = true, BackColor = Color.Transparent, Padding = new Padding(2, 0, 0, 0) })
        {
            CheckBox.CheckedChanged += (s, e) => CheckedChanged?.Invoke(this, e);
        }

        [Browsable(false)]
        public CheckBox CheckBox => (CheckBox)Control;

        [DefaultValue(false)]
        public bool Checked
        {
            get => CheckBox.Checked;
            set => CheckBox.Checked = value;
        }

        public event EventHandler CheckedChanged;
    }
}
