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
    /// back). The first button named is the default (Enter); Esc is Cancel. The layout is in
    /// ConfirmCloseDialog.Designer.cs; the title, question and button names are set here.
    /// </summary>
    internal sealed partial class ConfirmCloseDialog : Form
    {
        private CloseChoice _choice = CloseChoice.Cancel;

        /// <summary>For the designer.</summary>
        public ConfirmCloseDialog()
        {
            InitializeComponent();
        }

        private ConfirmCloseDialog(string title, string message, string keep, string discard) : this()
        {
            Text = title;
            _message.Text = message;
            _keep.Text = keep;
            _discard.Text = discard;
            // Windows' question icon, at the screen's scaling
            _icon.Image = SystemIcons.GetStockIcon(StockIconId.Help, _icon.Width).ToBitmap();
        }

        private void Keep_Click(object sender, EventArgs e) => Choose(CloseChoice.Keep);
        private void Discard_Click(object sender, EventArgs e) => Choose(CloseChoice.Discard);
        private void Cancel_Click(object sender, EventArgs e) => Choose(CloseChoice.Cancel);

        private void Choose(CloseChoice choice)
        {
            _choice = choice;
            DialogResult = choice == CloseChoice.Cancel ? DialogResult.Cancel : DialogResult.OK;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                _icon.Image?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
