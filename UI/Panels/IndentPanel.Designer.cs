namespace ClipX2.UI.Panels
{
    partial class IndentPanel
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
            labelWith = new System.Windows.Forms.Label();
            radioTab = new System.Windows.Forms.RadioButton();
            radioSpaces = new System.Windows.Forms.RadioButton();
            numericSpaces = new System.Windows.Forms.NumericUpDown();
            radioText = new System.Windows.Forms.RadioButton();
            comboText = new System.Windows.Forms.ComboBox();
            labelSample = new System.Windows.Forms.Label();
            textBoxSample = new System.Windows.Forms.TextBox();
            labelNote = new System.Windows.Forms.Label();
            buttonIndent = new System.Windows.Forms.Button();
            buttonUnindent = new System.Windows.Forms.Button();
            toolTip = new System.Windows.Forms.ToolTip(components);
            tableLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericSpaces).BeginInit();
            SuspendLayout();
            //
            // tableLayout
            //
            tableLayout.ColumnCount = 2;
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.Controls.Add(labelTitle, 0, 0);
            tableLayout.Controls.Add(labelWith, 0, 1);
            tableLayout.Controls.Add(radioTab, 0, 2);
            tableLayout.Controls.Add(radioSpaces, 0, 3);
            tableLayout.Controls.Add(numericSpaces, 1, 3);
            tableLayout.Controls.Add(radioText, 0, 4);
            tableLayout.Controls.Add(comboText, 1, 4);
            tableLayout.Controls.Add(labelSample, 0, 5);
            tableLayout.Controls.Add(textBoxSample, 0, 6);
            tableLayout.Controls.Add(labelNote, 0, 7);
            tableLayout.Controls.Add(buttonIndent, 0, 9);
            tableLayout.Controls.Add(buttonUnindent, 1, 9);
            tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayout.Location = new System.Drawing.Point(0, 0);
            tableLayout.Name = "tableLayout";
            tableLayout.Padding = new System.Windows.Forms.Padding(2);
            tableLayout.RowCount = 11;
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.Size = new System.Drawing.Size(243, 468);
            tableLayout.TabIndex = 0;
            //
            // labelTitle
            //
            labelTitle.AutoSize = true;
            tableLayout.SetColumnSpan(labelTitle, 2);
            labelTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            labelTitle.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            labelTitle.Name = "labelTitle";
            labelTitle.TabIndex = 0;
            labelTitle.Text = "Indent / Unindent";
            //
            // labelWith
            //
            labelWith.AutoSize = true;
            tableLayout.SetColumnSpan(labelWith, 2);
            labelWith.Name = "labelWith";
            labelWith.TabIndex = 1;
            labelWith.Text = "One level of indent is:";
            //
            // radioTab
            //
            radioTab.AutoSize = true;
            tableLayout.SetColumnSpan(radioTab, 2);
            radioTab.Name = "radioTab";
            radioTab.TabIndex = 2;
            radioTab.TabStop = true;
            radioTab.Text = "&Tab";
            radioTab.UseVisualStyleBackColor = true;
            //
            // radioSpaces
            //
            radioSpaces.Anchor = System.Windows.Forms.AnchorStyles.Left;
            radioSpaces.AutoSize = true;
            radioSpaces.Name = "radioSpaces";
            radioSpaces.TabIndex = 3;
            radioSpaces.Text = "&Spaces";
            radioSpaces.UseVisualStyleBackColor = true;
            //
            // numericSpaces
            //
            numericSpaces.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numericSpaces.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            numericSpaces.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericSpaces.Name = "numericSpaces";
            numericSpaces.Size = new System.Drawing.Size(60, 25);
            numericSpaces.TabIndex = 4;
            numericSpaces.Value = new decimal(new int[] { 4, 0, 0, 0 });
            //
            // radioText
            //
            radioText.Anchor = System.Windows.Forms.AnchorStyles.Left;
            radioText.AutoSize = true;
            radioText.Name = "radioText";
            radioText.TabIndex = 5;
            radioText.Text = "This te&xt:";
            toolTip.SetToolTip(radioText, "Put this text in front of each line, e.g. \"> \" to quote an email");
            radioText.UseVisualStyleBackColor = true;
            //
            // comboText
            //
            comboText.Dock = System.Windows.Forms.DockStyle.Fill;
            comboText.Font = new System.Drawing.Font("Consolas", 9.75F);
            comboText.Items.AddRange(new object[] { "> ", "| ", "# ", "// ", "-- " });
            comboText.Name = "comboText";
            comboText.TabIndex = 6;
            toolTip.SetToolTip(comboText, "Spaces count: \"> \" is a > and a space");
            //
            // labelSample
            //
            labelSample.AutoSize = true;
            tableLayout.SetColumnSpan(labelSample, 2);
            labelSample.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            labelSample.Name = "labelSample";
            labelSample.TabIndex = 7;
            labelSample.Text = "Sample:";
            //
            // textBoxSample
            //
            tableLayout.SetColumnSpan(textBoxSample, 2);
            textBoxSample.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxSample.Font = new System.Drawing.Font("Consolas", 9.75F);
            textBoxSample.Multiline = true;
            textBoxSample.Name = "textBoxSample";
            textBoxSample.ReadOnly = true;
            textBoxSample.TabIndex = 8;
            textBoxSample.TabStop = false;
            textBoxSample.WordWrap = false;
            //
            // labelNote
            //
            labelNote.AutoSize = true;
            tableLayout.SetColumnSpan(labelNote, 2);
            labelNote.ForeColor = System.Drawing.SystemColors.GrayText;
            labelNote.MaximumSize = new System.Drawing.Size(233, 0);
            labelNote.Name = "labelNote";
            labelNote.TabIndex = 9;
            labelNote.Text = "Works on the selected lines, or all the text. Blank lines aren't indented. Unindent takes one level off the lines that start with it.";
            //
            // buttonIndent
            //
            buttonIndent.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonIndent.Name = "buttonIndent";
            buttonIndent.Size = new System.Drawing.Size(113, 30);
            buttonIndent.TabIndex = 10;
            buttonIndent.Text = "&Indent";
            buttonIndent.UseVisualStyleBackColor = true;
            //
            // buttonUnindent
            //
            buttonUnindent.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonUnindent.Name = "buttonUnindent";
            buttonUnindent.Size = new System.Drawing.Size(114, 30);
            buttonUnindent.TabIndex = 11;
            buttonUnindent.Text = "&Unindent";
            buttonUnindent.UseVisualStyleBackColor = true;
            //
            // IndentPanel
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tableLayout);
            Name = "IndentPanel";
            Size = new System.Drawing.Size(243, 468);
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericSpaces).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelWith;
        private System.Windows.Forms.RadioButton radioTab;
        private System.Windows.Forms.RadioButton radioSpaces;
        private System.Windows.Forms.NumericUpDown numericSpaces;
        private System.Windows.Forms.RadioButton radioText;
        private System.Windows.Forms.ComboBox comboText;
        private System.Windows.Forms.Label labelSample;
        private System.Windows.Forms.TextBox textBoxSample;
        private System.Windows.Forms.Label labelNote;
        private System.Windows.Forms.Button buttonIndent;
        private System.Windows.Forms.Button buttonUnindent;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
