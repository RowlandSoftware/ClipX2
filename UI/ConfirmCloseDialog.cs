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

        private ConfirmCloseDialog(string title, string message, string keep, string discard, bool discardFirst)
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
            Button keepButton = Make(keep, CloseChoice.Keep, discardFirst ? 64 : 16);
            Button discardButton = Make(discard, CloseChoice.Discard, discardFirst ? 16 : 64);
            Button cancel = Make("Cancel", CloseChoice.Cancel, 116);
            Controls.AddRange(new Control[] { icon, label, discardFirst ? discardButton : keepButton, discardFirst ? keepButton : discardButton, cancel });
            AcceptButton = discardFirst ? discardButton : keepButton;
            CancelButton = cancel;
        }

        /// <summary>
        /// Asks; Keep, Discard or Cancel. <paramref name="discardFirst"/> puts Discard at the top,
        /// as the default (ClipX 1's main window).
        /// </summary>
        public static CloseChoice Ask(IWin32Window owner, string title, string message, string keep, string discard = "Discard changes", bool discardFirst = false)
        {
            if (TestAnswer != null) return TestAnswer(message);
            using var d = new ConfirmCloseDialog(title, message, keep, discard, discardFirst);
            d.ShowDialog(owner);
            return d._choice;
        }

        /// <summary>For tests: the answer, given the message, without showing the dialog.</summary>
        internal static Func<string, CloseChoice> TestAnswer { get; set; }
    }
}
