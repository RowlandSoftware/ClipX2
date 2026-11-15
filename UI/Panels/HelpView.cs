using System.Drawing;
using System.Windows.Forms;

namespace ClipX2.UI.Panels
{
    /// <summary>The side panel when no tool is open: describes the button under the mouse.</summary>
    internal sealed class HelpView : UserControl
    {
        private const string Idle = "Point to a button to see what it does.";
        private readonly Label _title;
        private readonly Label _text;

        public HelpView()
        {
            Padding = new Padding(6);
            _text = new Label { Dock = DockStyle.Fill, AutoSize = false, Text = Idle, ForeColor = SystemColors.GrayText };
            _title = new Label { Dock = DockStyle.Top, AutoSize = false, Height = 28 };
            Controls.Add(_text);
            Controls.Add(_title);
        }

        protected override void OnFontChanged(System.EventArgs e)
        {
            base.OnFontChanged(e);
            _title.Font = new Font(Font, FontStyle.Bold);
        }

        protected override void OnHandleCreated(System.EventArgs e)
        {
            base.OnHandleCreated(e);
            _title.Font = new Font(Font, FontStyle.Bold);
        }

        public void ShowDescription(string title, string text)
        {
            bool none = string.IsNullOrEmpty(text) && string.IsNullOrEmpty(title);
            _title.Text = none ? "" : title;
            _text.Text = none ? Idle : text ?? "";
            _text.ForeColor = none ? SystemColors.GrayText : SystemColors.ControlText;
        }
    }
}
