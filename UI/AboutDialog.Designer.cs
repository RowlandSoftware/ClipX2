namespace ClipX2.UI
{
    partial class AboutDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            labelRS = new System.Windows.Forms.Label();
            labelTitle = new System.Windows.Forms.Label();
            labelTagline = new System.Windows.Forms.Label();
            pictureBox = new System.Windows.Forms.PictureBox();
            labelVersion = new System.Windows.Forms.Label();
            labelCopyright = new System.Windows.Forms.Label();
            linkWebsite = new System.Windows.Forms.LinkLabel();
            buttonOK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            SuspendLayout();
            // 
            // labelRS
            // 
            labelRS.AutoSize = true;
            labelRS.Font = new System.Drawing.Font("Segoe UI Variable Display", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelRS.ForeColor = System.Drawing.Color.FromArgb(200, 32, 32);
            labelRS.Location = new System.Drawing.Point(135, 18);
            labelRS.Name = "labelRS";
            labelRS.Size = new System.Drawing.Size(26, 19);
            labelRS.TabIndex = 0;
            labelRS.Text = "RS";
            // 
            // labelTitle
            // 
            labelTitle.Font = new System.Drawing.Font("Bell MT", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelTitle.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);
            labelTitle.Location = new System.Drawing.Point(12, 18);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new System.Drawing.Size(496, 66);
            labelTitle.TabIndex = 1;
            labelTitle.Text = "ClipX 2";
            labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelTagline
            // 
            labelTagline.Font = new System.Drawing.Font("Rockwell Nova", 13F);
            labelTagline.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70);
            labelTagline.Location = new System.Drawing.Point(12, 84);
            labelTagline.Name = "labelTagline";
            labelTagline.Size = new System.Drawing.Size(496, 28);
            labelTagline.TabIndex = 2;
            labelTagline.Text = "The Clipboard Text Toolbox";
            labelTagline.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox
            // 
            pictureBox.Image = Properties.Resources.About;
            pictureBox.Location = new System.Drawing.Point(20, 122);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new System.Drawing.Size(480, 320);
            pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBox.TabIndex = 3;
            pictureBox.TabStop = false;
            // 
            // labelVersion
            // 
            labelVersion.ForeColor = System.Drawing.SystemColors.GrayText;
            labelVersion.Location = new System.Drawing.Point(12, 452);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new System.Drawing.Size(496, 20);
            labelVersion.TabIndex = 4;
            labelVersion.Text = "Version";
            labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelCopyright
            // 
            labelCopyright.Location = new System.Drawing.Point(12, 474);
            labelCopyright.Name = "labelCopyright";
            labelCopyright.Size = new System.Drawing.Size(496, 20);
            labelCopyright.TabIndex = 5;
            labelCopyright.Text = "Copyright © RowlandSoftware 2026";
            labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkWebsite
            // 
            linkWebsite.Location = new System.Drawing.Point(12, 496);
            linkWebsite.Name = "linkWebsite";
            linkWebsite.Size = new System.Drawing.Size(496, 20);
            linkWebsite.TabIndex = 6;
            linkWebsite.TabStop = true;
            linkWebsite.Text = "https://rowlandsoftware.com/";
            linkWebsite.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            linkWebsite.LinkClicked += linkWebsite_LinkClicked;
            // 
            // buttonOK
            // 
            buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOK.Location = new System.Drawing.Point(213, 530);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(94, 30);
            buttonOK.TabIndex = 7;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            // 
            // AboutDialog
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            CancelButton = buttonOK;
            ClientSize = new System.Drawing.Size(520, 574);
            Controls.Add(labelRS);
            Controls.Add(labelTitle);
            Controls.Add(labelTagline);
            Controls.Add(pictureBox);
            Controls.Add(labelVersion);
            Controls.Add(labelCopyright);
            Controls.Add(linkWebsite);
            Controls.Add(buttonOK);
            Font = new System.Drawing.Font("Segoe UI", 9.75F);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "About ClipX";
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelRS;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelTagline;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.LinkLabel linkWebsite;
        private System.Windows.Forms.Button buttonOK;
    }
}
