namespace ClipX2.UI.Panels
{
    partial class BulletListPanel
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tableLayout = new System.Windows.Forms.TableLayoutPanel();
            labelTitle = new System.Windows.Forms.Label();
            labelCharacter = new System.Windows.Forms.Label();
            comboCharacter = new System.Windows.Forms.ComboBox();
            radioTab = new System.Windows.Forms.RadioButton();
            radioPad = new System.Windows.Forms.RadioButton();
            numericPad = new System.Windows.Forms.NumericUpDown();
            labelSample = new System.Windows.Forms.Label();
            textBoxSample = new System.Windows.Forms.TextBox();
            labelNote = new System.Windows.Forms.Label();
            buttonApply = new System.Windows.Forms.Button();
            buttonClose = new System.Windows.Forms.Button();
            toolTip = new System.Windows.Forms.ToolTip(components);
            tableLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericPad).BeginInit();
            SuspendLayout();
            //
            // tableLayout
            //
            tableLayout.ColumnCount = 2;
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.Controls.Add(labelTitle, 0, 0);
            tableLayout.Controls.Add(labelCharacter, 0, 1);
            tableLayout.Controls.Add(comboCharacter, 1, 1);
            tableLayout.Controls.Add(radioTab, 0, 2);
            tableLayout.Controls.Add(radioPad, 0, 3);
            tableLayout.Controls.Add(numericPad, 1, 3);
            tableLayout.Controls.Add(labelSample, 0, 4);
            tableLayout.Controls.Add(textBoxSample, 0, 5);
            tableLayout.Controls.Add(labelNote, 0, 6);
            tableLayout.Controls.Add(buttonApply, 0, 8);
            tableLayout.Controls.Add(buttonClose, 1, 8);
            tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayout.Location = new System.Drawing.Point(0, 0);
            tableLayout.Name = "tableLayout";
            tableLayout.Padding = new System.Windows.Forms.Padding(2);
            tableLayout.RowCount = 9;
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.Size = new System.Drawing.Size(243, 468);
            tableLayout.TabIndex = 0;
            //
            // labelTitle
            //
            labelTitle.AutoSize = true;
            tableLayout.SetColumnSpan(labelTitle, 2);
            labelTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            labelTitle.Location = new System.Drawing.Point(5, 2);
            labelTitle.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new System.Drawing.Size(80, 19);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "Bullet list";
            //
            // labelCharacter
            //
            labelCharacter.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelCharacter.AutoSize = true;
            labelCharacter.Location = new System.Drawing.Point(5, 33);
            labelCharacter.Name = "labelCharacter";
            labelCharacter.Size = new System.Drawing.Size(65, 19);
            labelCharacter.TabIndex = 1;
            labelCharacter.Text = "&Bullet:";
            //
            // comboCharacter
            //
            comboCharacter.Dock = System.Windows.Forms.DockStyle.Fill;
            comboCharacter.Font = new System.Drawing.Font("Segoe UI Symbol", 10F);
            comboCharacter.Items.AddRange(new object[] { "•", "◦", "▪", "‣", "–", "-", "*", "·", "►", "✓", "→" });
            comboCharacter.Location = new System.Drawing.Point(124, 29);
            comboCharacter.MaxLength = 3;
            comboCharacter.Name = "comboCharacter";
            comboCharacter.Size = new System.Drawing.Size(114, 25);
            comboCharacter.TabIndex = 2;
            toolTip.SetToolTip(comboCharacter, "Choose a bullet, or type your own. In rich text, • makes a live list; others are typed in.");
            //
            // radioTab
            //
            radioTab.AutoSize = true;
            tableLayout.SetColumnSpan(radioTab, 2);
            radioTab.Location = new System.Drawing.Point(5, 60);
            radioTab.Name = "radioTab";
            radioTab.Size = new System.Drawing.Size(150, 23);
            radioTab.TabIndex = 3;
            radioTab.TabStop = true;
            radioTab.Text = "&Tab after the bullet";
            radioTab.UseVisualStyleBackColor = true;
            //
            // radioPad
            //
            radioPad.Anchor = System.Windows.Forms.AnchorStyles.Left;
            radioPad.AutoSize = true;
            radioPad.Location = new System.Drawing.Point(5, 91);
            radioPad.Name = "radioPad";
            radioPad.Size = new System.Drawing.Size(110, 23);
            radioPad.TabIndex = 4;
            radioPad.TabStop = true;
            radioPad.Text = "&Pad to";
            toolTip.SetToolTip(radioPad, "Spaces after the bullet, so that it and they fill this many characters");
            radioPad.UseVisualStyleBackColor = true;
            //
            // numericPad
            //
            numericPad.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numericPad.Location = new System.Drawing.Point(124, 90);
            numericPad.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            numericPad.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            numericPad.Name = "numericPad";
            numericPad.Size = new System.Drawing.Size(60, 25);
            numericPad.TabIndex = 5;
            numericPad.Value = new decimal(new int[] { 5, 0, 0, 0 });
            toolTip.SetToolTip(numericPad, "The bullet and its spaces fill this many characters; it is also one level of indentation");
            //
            // labelSample
            //
            labelSample.AutoSize = true;
            tableLayout.SetColumnSpan(labelSample, 2);
            labelSample.Location = new System.Drawing.Point(5, 124);
            labelSample.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            labelSample.Name = "labelSample";
            labelSample.Size = new System.Drawing.Size(56, 19);
            labelSample.TabIndex = 6;
            labelSample.Text = "Sample:";
            //
            // textBoxSample
            //
            tableLayout.SetColumnSpan(textBoxSample, 2);
            textBoxSample.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxSample.Font = new System.Drawing.Font("Consolas", 9.75F);
            textBoxSample.Location = new System.Drawing.Point(5, 146);
            textBoxSample.Multiline = true;
            textBoxSample.Name = "textBoxSample";
            textBoxSample.ReadOnly = true;
            textBoxSample.Size = new System.Drawing.Size(233, 104);
            textBoxSample.TabIndex = 7;
            textBoxSample.TabStop = false;
            textBoxSample.WordWrap = false;
            //
            // labelNote
            //
            labelNote.AutoSize = true;
            tableLayout.SetColumnSpan(labelNote, 2);
            labelNote.ForeColor = System.Drawing.SystemColors.GrayText;
            labelNote.Location = new System.Drawing.Point(5, 256);
            labelNote.MaximumSize = new System.Drawing.Size(233, 0);
            labelNote.Name = "labelNote";
            labelNote.Size = new System.Drawing.Size(40, 19);
            labelNote.TabIndex = 8;
            labelNote.Text = "Note";
            //
            // buttonApply
            //
            buttonApply.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonApply.Location = new System.Drawing.Point(5, 433);
            buttonApply.Name = "buttonApply";
            buttonApply.Size = new System.Drawing.Size(113, 30);
            buttonApply.TabIndex = 9;
            buttonApply.Text = "&Apply";
            buttonApply.UseVisualStyleBackColor = true;
            //
            // buttonClose
            //
            buttonClose.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonClose.Location = new System.Drawing.Point(124, 433);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(114, 30);
            buttonClose.TabIndex = 10;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            //
            // BulletListPanel
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tableLayout);
            Name = "BulletListPanel";
            Size = new System.Drawing.Size(243, 468);
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericPad).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelCharacter;
        private System.Windows.Forms.ComboBox comboCharacter;
        private System.Windows.Forms.RadioButton radioTab;
        private System.Windows.Forms.RadioButton radioPad;
        private System.Windows.Forms.NumericUpDown numericPad;
        private System.Windows.Forms.Label labelSample;
        private System.Windows.Forms.TextBox textBoxSample;
        private System.Windows.Forms.Label labelNote;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
