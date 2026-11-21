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

            // The HTML itself, as code: only when there is some
            btnHtml.FlatAppearance.BorderSize = 1;
            btnHtml.FlatAppearance.BorderColor = SystemColors.ControlDark;
            btnHtml.Enabled = hasHtml;

            // Formatted or plain text, whatever the clipboard holds: what isn't there is converted
            // from what is (HTML keeps its formatting; plain text's lists and indents become
            // real ones), so both are always offered
            btnRtf.FlatAppearance.BorderSize = 1;
            btnRtf.FlatAppearance.BorderColor = SystemColors.ControlDark;
            if (!hasRtf) btnRtf.Text += hasHtml ? "\n(rendered from the HTML)" : hasText ? "\n(lists and indents made from the plain text)" : "";

            btnText.FlatAppearance.BorderSize = 1;
            btnText.FlatAppearance.BorderColor = SystemColors.ControlDark;
            if (!hasText) btnText.Text += "\n(formatting removed)";
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
