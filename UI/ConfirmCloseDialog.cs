using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClipX2.UI
{
    /// <summary>What the user chose in a <see cref="ConfirmCloseDialog"/>.</summary>
    internal enum CloseChoice { Keep, Discard, Cancel }

    /// <summary>
    /// ClipX 1's "Confirm close" (dlgConfirmClose): a question and three wide buttons, one above
    /// the other: keep the changes (Copy &amp; Close, Save changes), discard them, or Cancel (go
    /// back). The first button named is the default (Enter); Esc is Cancel.
    /// </summary>
    internal sealed class ConfirmCloseDialog : Form
    {
        private CloseChoice _choice = CloseChoice.Cancel;

        private ConfirmCloseDialog(string title, string message, string keep, string discard)
        {
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Font;
            Font = SystemFonts.MessageBoxFont;
            ClientSize = new Size(400, 168);

            var icon = new PictureBox
            {
                Image = SystemIcons.Question.ToBitmap(),
                SizeMode = PictureBoxSizeMode.AutoSize,
                Location = new Point(16, 20),
            };
            var label = new Label
            {
                Text = message,
                AutoSize = false,
                Location = new Point(64, 20),
                Size = new Size(170, 128),
            };
            Button Make(string text, CloseChoice choice, int top)
            {
                var b = new Button
                {
                    Text = text,
                    Font = new Font(Font, FontStyle.Bold),
                    Location = new Point(246, top),
                    Size = new Size(138, 36),
                };
                b.Click += (s, e) => { _choice = choice; DialogResult = choice == CloseChoice.Cancel ? DialogResult.Cancel : DialogResult.OK; };
                return b;
            }
            Button keepButton = Make(keep, CloseChoice.Keep, 16);
            Button discardButton = Make(discard, CloseChoice.Discard, 64);
            Button cancel = Make("Cancel", CloseChoice.Cancel, 116);
            Controls.AddRange(new Control[] { icon, label, keepButton, discardButton, cancel });
            AcceptButton = keepButton;
            CancelButton = cancel;
        }

        /// <summary>
        /// Asks; Keep, Discard or Cancel. Keep is at the top and the default, in every window
        /// (ClipX 1's main window had Discard first: changed for consistency).
        /// </summary>
        public static CloseChoice Ask(IWin32Window owner, string title, string message, string keep, string discard = "Discard changes")
        {
            if (TestAnswer != null) return TestAnswer(message);
            using var d = new ConfirmCloseDialog(title, message, keep, discard);
            d.ShowDialog(owner);
            return d._choice;
        }

        /// <summary>For tests: the answer, given the message, without showing the dialog.</summary>
        internal static Func<string, CloseChoice> TestAnswer { get; set; }
    }
}
