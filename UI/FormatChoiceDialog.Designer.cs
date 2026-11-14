namespace ClipX2
{
    partial class FormatChoiceDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnHtml = new System.Windows.Forms.Button();
            this.btnRtf = new System.Windows.Forms.Button();
            this.btnText = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnHtml
            // 
            this.btnHtml.AccessibleDescription = "Includes tags and markup";
            this.btnHtml.Font = new System.Drawing.Font("Segoe UI", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHtml.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnHtml.Location = new System.Drawing.Point(57, 44);
            this.btnHtml.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnHtml.Name = "btnHtml";
            this.btnHtml.Padding = new System.Windows.Forms.Padding(8, 10, 8, 10);
            this.btnHtml.Size = new System.Drawing.Size(521, 95);
            this.btnHtml.TabIndex = 3;
            this.btnHtml.Text = "<HTML> Code";
            this.btnHtml.UseVisualStyleBackColor = true;
            this.btnHtml.Click += new System.EventHandler(this.btnHtml_Click);
            // 
            // btnRtf
            // 
            this.btnRtf.AccessibleDescription = "Preserves formatting";
            this.btnRtf.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRtf.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRtf.Location = new System.Drawing.Point(57, 160);
            this.btnRtf.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRtf.Name = "btnRtf";
            this.btnRtf.Padding = new System.Windows.Forms.Padding(8, 10, 8, 10);
            this.btnRtf.Size = new System.Drawing.Size(521, 95);
            this.btnRtf.TabIndex = 4;
            this.btnRtf.Text = "Formatted text";
            this.btnRtf.UseVisualStyleBackColor = true;
            this.btnRtf.Click += new System.EventHandler(this.btnRtf_Click);
            // 
            // btnText
            // 
            this.btnText.AccessibleDescription = "Unicode text, no formatting";
            this.btnText.Font = new System.Drawing.Font("Courier New", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnText.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnText.Location = new System.Drawing.Point(57, 276);
            this.btnText.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnText.Name = "btnText";
            this.btnText.Padding = new System.Windows.Forms.Padding(8, 10, 8, 10);
            this.btnText.Size = new System.Drawing.Size(521, 95);
            this.btnText.TabIndex = 5;
            this.btnText.Text = "Plain text";
            this.btnText.UseVisualStyleBackColor = true;
            this.btnText.Click += new System.EventHandler(this.btnText_Click);
            // 
            // FormatChoiceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(634, 416);
            this.Controls.Add(this.btnText);
            this.Controls.Add(this.btnRtf);
            this.Controls.Add(this.btnHtml);
            this.Font = new System.Drawing.Font("Segoe UI", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormatChoiceDialog";
            this.Text = "Choose Clipboard Format";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnHtml;
        private System.Windows.Forms.Button btnRtf;
        private System.Windows.Forms.Button btnText;
    }
}