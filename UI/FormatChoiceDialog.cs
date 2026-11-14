using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipX2
{
    public partial class FormatChoiceDialog : Form
    {
        public enum Choice { Html, Rtf, Text, None }
        public Choice Selected { get; private set; } = Choice.None;

        public FormatChoiceDialog(bool hasHtml, bool hasRtf, bool hasText)
        {
            InitializeComponent();

            btnHtml.FlatAppearance.BorderSize = 1;
            btnHtml.FlatAppearance.BorderColor = SystemColors.ControlDark;
            btnHtml.Enabled = hasHtml;

            btnRtf.FlatAppearance.BorderSize = 1;
            btnRtf.FlatAppearance.BorderColor = SystemColors.ControlDark;
            btnRtf.Enabled = hasRtf;

            btnText.FlatAppearance.BorderSize = 1;
            btnText.FlatAppearance.BorderColor = SystemColors.ControlDark;
            btnText.Enabled = hasText;                        
        }
        private void btnHtml_Click(object sender, EventArgs e)
        {
            Selected = Choice.Html;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnRtf_Click(object sender, EventArgs e)
        {
            Selected = Choice.Rtf;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnText_Click(object sender, EventArgs e)
        {
            Selected = Choice.Text;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
