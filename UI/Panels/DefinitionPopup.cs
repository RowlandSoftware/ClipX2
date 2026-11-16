using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Definitions of a word, floating over the editor beside the side panel (the spelling
    /// panel shows it when the user clicks a suggestion or "Change to", for quick comparisons).
    /// Built in code: it is a header with a close button, a read-only text area and the credit
    /// Wiktionary's licence asks for.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Code")] // built in code: open as code, not in the designer
    internal sealed class DefinitionPopup : UserControl
    {
        private readonly Label _title;
        private readonly RichTextBox _body;
        private readonly LinkLabel _credit;
        private readonly LinkLabel _more;

        /// <summary>The word shown (or being looked up).</summary>
        public string Word { get; private set; }

        public DefinitionPopup()
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
            close.Click += (s, e) => Hide();
            header.Controls.Add(_title);
            header.Controls.Add(close);
            var credit = _credit = new LinkLabel
            {
                Dock = DockStyle.Bottom,
                Height = 18,
                Text = "From Wiktionary (CC BY-SA)",
                LinkColor = SystemColors.GrayText,
                ActiveLinkColor = SystemColors.HotTrack,
                LinkBehavior = LinkBehavior.HoverUnderline,
                ForeColor = SystemColors.GrayText,
                Font = new Font(Font.FontFamily, 8F),
                TextAlign = ContentAlignment.BottomRight,
            };
            credit.LinkClicked += (s, e) => OpenLink(credit.Tag as string);
            _more = new LinkLabel
            {
                Dock = DockStyle.Bottom,
                Height = 20,
                Text = "More about this rule",
                TextAlign = ContentAlignment.MiddleLeft,
                LinkBehavior = LinkBehavior.HoverUnderline,
                Visible = false,
            };
            _more.LinkClicked += (s, e) => OpenLink(_more.Tag as string);
            SetCredit("From Wiktionary (CC BY-SA)", "https://en.wiktionary.org");
            // Docking goes from the last added: header and credit first, then the body fills the rest
            Controls.Add(_body);
            Controls.Add(_more);
            Controls.Add(credit);
            Controls.Add(header);
        }

        /// <summary>The credit line at the bottom, linked to the source's site.</summary>
        private void SetCredit(string text, string url)
        {
            _credit.Text = text;
            _credit.Tag = url;
        }

        /// <summary>Opens a page in the user's browser (the user clicked a link).</summary>
        private static void OpenLink(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true }); }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception || ex is InvalidOperationException) { /* no browser */ }
        }

        /// <summary>
        /// A grammar checker's explanation of the issue shown: the message, the rule's own
        /// description (if it adds anything), a link to more about the rule, and the credit.
        /// </summary>
        public void ShowExplanation(string title, string message, string ruleDescription, string moreUrl,
            string credit, string creditUrl)
        {
            Word = null;
            _title.Text = title;
            SetCredit(credit, creditUrl);
            _more.Tag = moreUrl;
            _more.Visible = !string.IsNullOrEmpty(moreUrl);
            _body.Clear();
            Append(_body, message + "\n", _body.Font, ForeColor, 0);
            if (!string.IsNullOrEmpty(ruleDescription) && ruleDescription != message)
                using (var italic = new Font(_body.Font, FontStyle.Italic))
                    Append(_body, "\n" + ruleDescription + "\n", italic, SystemColors.GrayText, 0);
            _body.SelectionStart = 0;
            Visible = true;
        }

        /// <summary>A message instead of definitions ("Looking up…", "No entry…").</summary>
        public void ShowMessage(string word, string message)
        {
            Word = word;
            _title.Text = word;
            _more.Visible = false;
            SetCredit("From Wiktionary (CC BY-SA)", "https://en.wiktionary.org"); // a look-up (until the result says which)
            _body.Clear();
            _body.SelectionColor = SystemColors.GrayText;
            _body.AppendText(message);
            Visible = true;
        }

        /// <summary>The definitions, by part of speech, numbered, each with its examples (if any) under it.</summary>
        public void ShowResult(LookupResult result)
        {
            SetCredit($"From {result.Source} (CC BY-SA)",
                result.Source == WiktionaryLookup.SimpleName ? "https://simple.wiktionary.org" : "https://en.wiktionary.org");
            _more.Visible = false;
            if (result.Senses.Count == 0)
            {
                ShowMessage(result.Word, result.Message);
                return;
            }
            Word = result.Word;
            _title.Text = result.Word;
            var rtb = _body;
            rtb.SuspendLayout();
            rtb.Clear();
            Font normal = rtb.Font;
            using (var heading = new Font(normal, FontStyle.Bold | FontStyle.Italic))
            using (var example = new Font(normal, FontStyle.Italic))
            {
                bool first = true;
                foreach (WordSense sense in result.Senses)
                {
                    Append(rtb, (first ? "" : "\n") + sense.PartOfSpeech + "\n", heading, ForeColor, 0);
                    first = false;
                    int n = 1;
                    foreach (WordDefinition definition in sense.Definitions)
                    {
                        Append(rtb, $"{n++}. {definition.Text}\n", normal, ForeColor, 0);
                        foreach (string text in definition.Examples)
                            Append(rtb, text + "\n", example, SystemColors.GrayText, 16);
                    }
                }
            }
            rtb.SelectionStart = 0;
            rtb.ScrollToCaret();
            rtb.ResumeLayout();
            Visible = true;
        }

        private static void Append(RichTextBox rtb, string text, Font font, Color color, int indent)
        {
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionFont = font;
            rtb.SelectionColor = color;
            rtb.SelectionIndent = indent;
            rtb.AppendText(text);
        }
    }
}
