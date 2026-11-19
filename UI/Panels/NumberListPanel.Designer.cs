namespace ClipX2.UI.Panels
{
    partial class NumberListPanel
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
            labelStyle = new System.Windows.Forms.Label();
            comboStyle = new System.Windows.Forms.ComboBox();
            labelPunctuation = new System.Windows.Forms.Label();
            comboPunctuation = new System.Windows.Forms.ComboBox();
            labelStart = new System.Windows.Forms.Label();
            numericStart = new System.Windows.Forms.NumericUpDown();
            radioTab = new System.Windows.Forms.RadioButton();
            radioPad = new System.Windows.Forms.RadioButton();
            numericPad = new System.Windows.Forms.NumericUpDown();
            checkBoxRenumber = new System.Windows.Forms.CheckBox();
            labelSample = new System.Windows.Forms.Label();
            textBoxSample = new System.Windows.Forms.TextBox();
            labelNote = new System.Windows.Forms.Label();
            buttonApply = new System.Windows.Forms.Button();
            buttonClose = new System.Windows.Forms.Button();
            toolTip = new System.Windows.Forms.ToolTip(components);
            tableLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericStart).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericPad).BeginInit();
            SuspendLayout();
            //
            // tableLayout
            //
            tableLayout.ColumnCount = 2;
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.Controls.Add(labelTitle, 0, 0);
            tableLayout.Controls.Add(labelStyle, 0, 1);
            tableLayout.Controls.Add(comboStyle, 1, 1);
            tableLayout.Controls.Add(labelPunctuation, 0, 2);
            tableLayout.Controls.Add(comboPunctuation, 1, 2);
            tableLayout.Controls.Add(labelStart, 0, 3);
            tableLayout.Controls.Add(numericStart, 1, 3);
            tableLayout.Controls.Add(radioTab, 0, 4);
            tableLayout.Controls.Add(radioPad, 0, 5);
            tableLayout.Controls.Add(numericPad, 1, 5);
            tableLayout.Controls.Add(checkBoxRenumber, 0, 6);
            tableLayout.Controls.Add(labelSample, 0, 7);
            tableLayout.Controls.Add(textBoxSample, 0, 8);
            tableLayout.Controls.Add(labelNote, 0, 9);
            tableLayout.Controls.Add(buttonApply, 0, 11);
            tableLayout.Controls.Add(buttonClose, 1, 11);
            tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayout.Location = new System.Drawing.Point(0, 0);
            tableLayout.Name = "tableLayout";
            tableLayout.Padding = new System.Windows.Forms.Padding(2);
            tableLayout.RowCount = 12;
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
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
            labelTitle.Size = new System.Drawing.Size(110, 19);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "Numbered list";
            //
            // labelStyle
            //
            labelStyle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelStyle.AutoSize = true;
            labelStyle.Location = new System.Drawing.Point(5, 33);
            labelStyle.Name = "labelStyle";
            labelStyle.Size = new System.Drawing.Size(40, 19);
            labelStyle.TabIndex = 1;
            labelStyle.Text = "&Style:";
            //
            // comboStyle
            //
            comboStyle.Dock = System.Windows.Forms.DockStyle.Fill;
            comboStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboStyle.DropDownWidth = 150;
            comboStyle.Items.AddRange(new object[] { "1, 2, 3", "a, b, c", "A, B, C", "i, ii, iii", "I, II, III", "1.1, 1.2 (legal)" });
            comboStyle.Location = new System.Drawing.Point(124, 29);
            comboStyle.Name = "comboStyle";
            comboStyle.Size = new System.Drawing.Size(114, 25);
            comboStyle.TabIndex = 2;
            toolTip.SetToolTip(comboStyle, "Legal numbers by indentation: 1., 1.1., 1.1.1.");
            //
            // labelPunctuation
            //
            labelPunctuation.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelPunctuation.AutoSize = true;
            labelPunctuation.Location = new System.Drawing.Point(5, 64);
            labelPunctuation.Name = "labelPunctuation";
            labelPunctuation.Size = new System.Drawing.Size(85, 19);
            labelPunctuation.TabIndex = 3;
            labelPunctuation.Text = "P&unctuation:";
            //
            // comboPunctuation
            //
            comboPunctuation.Dock = System.Windows.Forms.DockStyle.Fill;
            comboPunctuation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboPunctuation.Items.AddRange(new object[] { "1", "1.", "1)", "(1)" });
            comboPunctuation.Location = new System.Drawing.Point(124, 60);
            comboPunctuation.Name = "comboPunctuation";
            comboPunctuation.Size = new System.Drawing.Size(114, 25);
            comboPunctuation.TabIndex = 4;
            //
            // labelStart
            //
            labelStart.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelStart.AutoSize = true;
            labelStart.Location = new System.Drawing.Point(5, 95);
            labelStart.Name = "labelStart";
            labelStart.Size = new System.Drawing.Size(60, 19);
            labelStart.TabIndex = 5;
            labelStart.Text = "St&art at:";
            //
            // numericStart
            //
            numericStart.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numericStart.Location = new System.Drawing.Point(124, 91);
            numericStart.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numericStart.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericStart.Name = "numericStart";
            numericStart.Size = new System.Drawing.Size(70, 25);
            numericStart.TabIndex = 6;
            numericStart.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // radioTab
            //
            radioTab.AutoSize = true;
            tableLayout.SetColumnSpan(radioTab, 2);
            radioTab.Location = new System.Drawing.Point(5, 122);
            radioTab.Name = "radioTab";
            radioTab.Size = new System.Drawing.Size(160, 23);
            radioTab.TabIndex = 7;
            radioTab.TabStop = true;
            radioTab.Text = "&Tab after the number";
            radioTab.UseVisualStyleBackColor = true;
            //
            // radioPad
            //
            radioPad.Anchor = System.Windows.Forms.AnchorStyles.Left;
            radioPad.AutoSize = true;
            radioPad.Location = new System.Drawing.Point(5, 153);
            radioPad.Name = "radioPad";
            radioPad.Size = new System.Drawing.Size(110, 23);
            radioPad.TabIndex = 8;
            radioPad.TabStop = true;
            radioPad.Text = "&Pad to";
            toolTip.SetToolTip(radioPad, "Spaces after the number, so that it and they fill this many characters");
            radioPad.UseVisualStyleBackColor = true;
            //
            // numericPad
            //
            numericPad.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numericPad.Location = new System.Drawing.Point(124, 152);
            numericPad.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            numericPad.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            numericPad.Name = "numericPad";
            numericPad.Size = new System.Drawing.Size(60, 25);
            numericPad.TabIndex = 9;
            numericPad.Value = new decimal(new int[] { 4, 0, 0, 0 });
            toolTip.SetToolTip(numericPad, "The number and its spaces fill this many characters (\"1.\" and 2 spaces = 4); it is also one level of indentation");
            //
            // checkBoxRenumber
            //
            checkBoxRenumber.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxRenumber, 2);
            checkBoxRenumber.Location = new System.Drawing.Point(5, 184);
            checkBoxRenumber.Name = "checkBoxRenumber";
            checkBoxRenumber.Size = new System.Drawing.Size(200, 23);
            checkBoxRenumber.TabIndex = 10;
            checkBoxRenumber.Text = "&Renumber (replace numbers)";
            checkBoxRenumber.UseVisualStyleBackColor = true;
            //
            // labelSample
            //
            labelSample.AutoSize = true;
            tableLayout.SetColumnSpan(labelSample, 2);
            labelSample.Location = new System.Drawing.Point(5, 216);
            labelSample.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            labelSample.Name = "labelSample";
            labelSample.Size = new System.Drawing.Size(56, 19);
            labelSample.TabIndex = 11;
            labelSample.Text = "Sample:";
            //
            // textBoxSample
            //
            tableLayout.SetColumnSpan(textBoxSample, 2);
            textBoxSample.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxSample.Font = new System.Drawing.Font("Consolas", 9.75F);
            textBoxSample.Location = new System.Drawing.Point(5, 238);
            textBoxSample.Multiline = true;
            textBoxSample.Name = "textBoxSample";
            textBoxSample.ReadOnly = true;
            textBoxSample.Size = new System.Drawing.Size(233, 114);
            textBoxSample.TabIndex = 12;
            textBoxSample.TabStop = false;
            textBoxSample.WordWrap = false;
            //
            // labelNote
            //
            labelNote.AutoSize = true;
            tableLayout.SetColumnSpan(labelNote, 2);
            labelNote.ForeColor = System.Drawing.SystemColors.GrayText;
            labelNote.Location = new System.Drawing.Point(5, 358);
            labelNote.MaximumSize = new System.Drawing.Size(233, 0);
            labelNote.Name = "labelNote";
            labelNote.Size = new System.Drawing.Size(40, 19);
            labelNote.TabIndex = 13;
            labelNote.Text = "Note";
            //
            // buttonApply
            //
            buttonApply.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonApply.Location = new System.Drawing.Point(5, 433);
            buttonApply.Name = "buttonApply";
            buttonApply.Size = new System.Drawing.Size(113, 30);
            buttonApply.TabIndex = 14;
            buttonApply.Text = "&Apply";
            buttonApply.UseVisualStyleBackColor = true;
            //
            // buttonClose
            //
            buttonClose.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonClose.Location = new System.Drawing.Point(124, 433);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(114, 30);
            buttonClose.TabIndex = 15;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            //
            // NumberListPanel
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tableLayout);
            Name = "NumberListPanel";
            Size = new System.Drawing.Size(243, 468);
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericStart).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericPad).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelStyle;
        private System.Windows.Forms.ComboBox comboStyle;
        private System.Windows.Forms.Label labelPunctuation;
        private System.Windows.Forms.ComboBox comboPunctuation;
        private System.Windows.Forms.Label labelStart;
        private System.Windows.Forms.NumericUpDown numericStart;
        private System.Windows.Forms.RadioButton radioTab;
        private System.Windows.Forms.RadioButton radioPad;
        private System.Windows.Forms.NumericUpDown numericPad;
        private System.Windows.Forms.CheckBox checkBoxRenumber;
        private System.Windows.Forms.Label labelSample;
        private System.Windows.Forms.TextBox textBoxSample;
        private System.Windows.Forms.Label labelNote;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
