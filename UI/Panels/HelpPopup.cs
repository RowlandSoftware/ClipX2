using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Help for a side panel, floating over the editor just to the right of the side panel, as the
    /// spelling panel's definitions box does (the panel's Help button shows and hides it). Built
    /// in code: a header with a close button, and a read-only text area.
    ///
    /// The text is simple lines: "# Heading" is a heading; "term\tmeaning" puts the term in a
    /// fixed-width bold column with the meaning beside it (wrapped lines stay in the meaning's
    /// column); anything else is an ordinary paragraph; an empty line is a gap.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Code")]
    internal sealed class HelpPopup : UserControl
    {
        private const int TermWidth = 64; // pixels for the term column
        private readonly Label _title;
        private readonly RichTextBox _body;

        public HelpPopup()
        {
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = SystemColors.Info;
            ForeColor = SystemColors.InfoText;
            Padding = new Padding(6, 2, 2, 4);

            _body = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = SystemColors.Info,
                ForeColor = SystemColors.InfoText,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                DetectUrls = false,
                TabStop = false,
            };
            var header = new Panel { Dock = DockStyle.Top, Height = 28 };
            _title = new Label
            {
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font(Font.FontFamily, 11F, FontStyle.Bold),
            };
            var close = new Button
            {
                Dock = DockStyle.Right,
                Width = 26,
                Text = "×",
                FlatStyle = FlatStyle.Flat,
                TabStop = false,
                Cursor = Cursors.Hand,
            };
            close.FlatAppearance.BorderSize = 0;
            close.Click += (s, e) => { Hide(); Closed?.Invoke(this, EventArgs.Empty); };
            header.Controls.Add(_title);
            header.Controls.Add(close);
            Controls.Add(_body);
            Controls.Add(header);
        }

        /// <summary>The user closed it (its × button).</summary>
        public event EventHandler Closed;

        /// <summary>Shows the help (see the class summary for the layout of <paramref name="text"/>).</summary>
        public void ShowHelp(string title, string text)
        {
            _title.Text = title;
            RichTextBox rtb = _body;
            rtb.SuspendLayout();
            rtb.Clear();
            Font normal = rtb.Font;
            using (var heading = new Font(normal, FontStyle.Bold))
            using (var term = new Font("Consolas", normal.SizeInPoints, FontStyle.Bold))
            {
                bool first = true;
                foreach (string raw in (text ?? "").Replace("\r\n", "\n").Split('\n'))
                {
                    string line = raw.TrimEnd();
                    string nl = first ? "" : "\n";
                    first = false;
                    rtb.SelectionStart = rtb.TextLength;
                    rtb.SelectionTabs = new[] { TermWidth };
                    if (line.StartsWith("# ", StringComparison.Ordinal))
                    {
                        Append(nl + line.Substring(2), heading, 0, 0);
                    }
                    else if (line.IndexOf('\t') > 0)
                    {
                        int tab = line.IndexOf('\t');
                        Append(nl + line.Substring(0, tab), term, 0, TermWidth);
                        Append("\t" + line.Substring(tab + 1), normal, 0, TermWidth);
                    }
                    else Append(nl + line, normal, 0, 0);
                }
            }
            rtb.SelectionStart = 0;
            rtb.ScrollToCaret();
            rtb.ResumeLayout();
            Visible = true;
        }

        private void Append(string text, Font font, int indent, int hanging)
        {
            RichTextBox rtb = _body;
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionFont = font;
            rtb.SelectionColor = ForeColor;
            rtb.SelectionIndent = indent;
            rtb.SelectionHangingIndent = hanging;
            rtb.SelectionTabs = new[] { TermWidth };
            rtb.AppendText(text);
        }

        /// <summary>
        /// Floats on <paramref name="form"/> just to the right of <paramref name="side"/> (the side
        /// panel), from <paramref name="top"/> (form coordinates) to the side panel's bottom.
        /// </summary>
        public void Place(Form form, Control side, int top, int width = 330)
        {
            if (form == null || side == null) return;
            Point right = form.PointToClient(side.PointToScreen(new Point(side.ClientSize.Width, 0)));
            int bottom = form.PointToClient(side.PointToScreen(new Point(0, side.ClientSize.Height))).Y;
            SetBounds(right.X + 2, top, width, Math.Max(120, bottom - top));
        }
    }
}
