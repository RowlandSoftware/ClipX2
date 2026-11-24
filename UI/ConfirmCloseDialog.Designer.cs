using System.Drawing;
using System.Windows.Forms;

namespace ClipX2.UI
{
    partial class ConfirmCloseDialog
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Dispose is in ConfirmCloseDialog.cs (it also frees the icon)

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _icon = new PictureBox();
            _message = new Label();
            _keep = new Button();
            _discard = new Button();
            _cancel = new Button();
            ((System.ComponentModel.ISupportInitialize)_icon).BeginInit();
            SuspendLayout();
            // 
            // _icon
            // 
            _icon.Location = new Point(32, 40);
            _icon.Name = "_icon";
            _icon.Size = new Size(64, 64);
            _icon.SizeMode = PictureBoxSizeMode.Zoom;
            _icon.TabIndex = 0;
            _icon.TabStop = false;
            // 
            // _message
            // 
            _message.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            _message.Location = new Point(128, 40);
            _message.Name = "_message";
            _message.Size = new Size(340, 256);
            _message.TabIndex = 1;
            _message.Text = "Save the changes?";
            // 
            // _keep
            // 
            _keep.DialogResult = DialogResult.OK;
            _keep.Font = new Font("Segoe UI", 10.125F, FontStyle.Regular, GraphicsUnit.Point, 0);
            _keep.Location = new Point(492, 32);
            _keep.Name = "_keep";
            _keep.Size = new Size(276, 72);
            _keep.TabIndex = 2;
            _keep.Text = "Save changes";
            _keep.UseVisualStyleBackColor = true;
            _keep.Click += Keep_Click;
            // 
            // _discard
            // 
            _discard.DialogResult = DialogResult.Continue;
            _discard.Font = new Font("Segoe UI", 10.125F);
            _discard.Location = new Point(492, 128);
            _discard.Name = "_discard";
            _discard.Size = new Size(276, 72);
            _discard.TabIndex = 3;
            _discard.Text = "Discard changes";
            _discard.UseVisualStyleBackColor = true;
            _discard.Click += Discard_Click;
            // 
            // _cancel
            // 
            _cancel.DialogResult = DialogResult.Cancel;
            _cancel.Font = new Font("Segoe UI", 10.125F);
            _cancel.Location = new Point(492, 232);
            _cancel.Name = "_cancel";
            _cancel.Size = new Size(276, 72);
            _cancel.TabIndex = 4;
            _cancel.Text = "Cancel";
            _cancel.UseVisualStyleBackColor = true;
            _cancel.Click += Cancel_Click;
            // 
            // ConfirmCloseDialog
            // 
            AcceptButton = _keep;
            AutoScaleDimensions = new SizeF(192F, 192F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = _cancel;
            ClientSize = new Size(800, 336);
            Controls.Add(_icon);
            Controls.Add(_message);
            Controls.Add(_keep);
            Controls.Add(_discard);
            Controls.Add(_cancel);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConfirmCloseDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Confirm close";
            ((System.ComponentModel.ISupportInitialize)_icon).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox _icon;
        private System.Windows.Forms.Label _message;
        private System.Windows.Forms.Button _keep;
        private System.Windows.Forms.Button _discard;
        private System.Windows.Forms.Button _cancel;
    }
}
