namespace ClipX2.UI.Panels
{
    partial class AlphaSortPanel
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
            radioWord = new System.Windows.Forms.RadioButton();
            radioLetter = new System.Windows.Forms.RadioButton();
            panelLetterRule = new System.Windows.Forms.FlowLayoutPanel();
            radioStrict = new System.Windows.Forms.RadioButton();
            radioChicago = new System.Windows.Forms.RadioButton();
            checkBoxCase = new System.Windows.Forms.CheckBox();
            checkBoxDuplicates = new System.Windows.Forms.CheckBox();
            labelSampleTitle = new System.Windows.Forms.Label();
            labelSample = new System.Windows.Forms.Label();
            labelNote = new System.Windows.Forms.Label();
            buttonApply = new System.Windows.Forms.Button();
            toolTip = new System.Windows.Forms.ToolTip(components);
            tableLayout.SuspendLayout();
            panelLetterRule.SuspendLayout();
            SuspendLayout();
            //
            // tableLayout
            //
            tableLayout.ColumnCount = 2;
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.Controls.Add(labelTitle, 0, 0);
            tableLayout.Controls.Add(radioWord, 0, 1);
            tableLayout.Controls.Add(radioLetter, 0, 2);
            tableLayout.Controls.Add(panelLetterRule, 0, 3);
            tableLayout.Controls.Add(checkBoxCase, 0, 4);
            tableLayout.Controls.Add(checkBoxDuplicates, 0, 5);
            tableLayout.Controls.Add(labelSampleTitle, 0, 6);
            tableLayout.Controls.Add(labelSample, 0, 7);
            tableLayout.Controls.Add(labelNote, 0, 8);
            tableLayout.Controls.Add(buttonApply, 1, 10);
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
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
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
            labelTitle.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            labelTitle.Name = "labelTitle";
            labelTitle.TabIndex = 0;
            labelTitle.Text = "Alphabetic sort";
            //
            // radioWord
            //
            radioWord.AutoSize = true;
            tableLayout.SetColumnSpan(radioWord, 2);
            radioWord.Name = "radioWord";
            radioWord.TabIndex = 1;
            radioWord.TabStop = true;
            radioWord.Text = "&Word by word";
            toolTip.SetToolTip(radioWord, "A space comes before any letter: New York before Newark (BS ISO 999 for general indexes; NISO TR03)");
            radioWord.UseVisualStyleBackColor = true;
            //
            // radioLetter
            //
            radioLetter.AutoSize = true;
            tableLayout.SetColumnSpan(radioLetter, 2);
            radioLetter.Name = "radioLetter";
            radioLetter.TabIndex = 2;
            radioLetter.Text = "&Letter by letter";
            toolTip.SetToolTip(radioLetter, "Spaces, hyphens and punctuation ignored: Newark before New York (encyclopedias, gazetteers)");
            radioLetter.UseVisualStyleBackColor = true;
            //
            // panelLetterRule
            //
            panelLetterRule.AutoSize = true;
            tableLayout.SetColumnSpan(panelLetterRule, 2);
            panelLetterRule.Controls.Add(radioStrict);
            panelLetterRule.Controls.Add(radioChicago);
            panelLetterRule.Margin = new System.Windows.Forms.Padding(20, 0, 3, 0);
            panelLetterRule.Name = "panelLetterRule";
            panelLetterRule.TabIndex = 3;
            panelLetterRule.WrapContents = false;
            //
            // radioStrict
            //
            radioStrict.AutoSize = true;
            radioStrict.Name = "radioStrict";
            radioStrict.TabIndex = 0;
            radioStrict.TabStop = true;
            radioStrict.Text = "S&trict";
            toolTip.SetToolTip(radioStrict, "Every space and punctuation mark ignored, without exception: New, Thomas after newsprint (NISO TR03)");
            radioStrict.UseVisualStyleBackColor = true;
            //
            // radioChicago
            //
            radioChicago.AutoSize = true;
            radioChicago.Name = "radioChicago";
            radioChicago.TabIndex = 1;
            radioChicago.Text = "C&hicago";
            toolTip.SetToolTip(radioChicago, "Chicago Manual of Style: letters count up to the first parenthesis or comma, then start again. A word alone first, then followed by a parenthesis, a comma, a number, letters: New, Thomas before Newark");
            radioChicago.UseVisualStyleBackColor = true;
            //
            // checkBoxCase
            //
            checkBoxCase.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxCase, 2);
            checkBoxCase.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            checkBoxCase.Name = "checkBoxCase";
            checkBoxCase.TabIndex = 4;
            checkBoxCase.Text = "&Case sensitive";
            toolTip.SetToolTip(checkBoxCase, "Capitals before small letters, as two alphabets: Zebra before apple. Off: case is ignored (Apple before apple only when otherwise the same)");
            checkBoxCase.UseVisualStyleBackColor = true;
            //
            // checkBoxDuplicates
            //
            checkBoxDuplicates.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxDuplicates, 2);
            checkBoxDuplicates.Name = "checkBoxDuplicates";
            checkBoxDuplicates.TabIndex = 5;
            checkBoxDuplicates.Text = "Delete &duplicates";
            toolTip.SetToolTip(checkBoxDuplicates, "A line the same as one already there goes (indents and runs of spaces aside; capitals count): the first stays");
            checkBoxDuplicates.UseVisualStyleBackColor = true;
            //
            // labelSampleTitle
            //
            labelSampleTitle.AutoSize = true;
            tableLayout.SetColumnSpan(labelSampleTitle, 2);
            labelSampleTitle.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            labelSampleTitle.Name = "labelSampleTitle";
            labelSampleTitle.TabIndex = 6;
            labelSampleTitle.Text = "Sample:";
            //
            // labelSample
            //
            labelSample.AutoSize = true;
            labelSample.BackColor = System.Drawing.SystemColors.Window;
            labelSample.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            tableLayout.SetColumnSpan(labelSample, 2);
            labelSample.Dock = System.Windows.Forms.DockStyle.Fill;
            labelSample.Name = "labelSample";
            labelSample.Padding = new System.Windows.Forms.Padding(4);
            labelSample.TabIndex = 7;
            labelSample.Text = "Sample";
            //
            // labelNote
            //
            labelNote.AutoSize = true;
            tableLayout.SetColumnSpan(labelNote, 2);
            labelNote.ForeColor = System.Drawing.SystemColors.GrayText;
            labelNote.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            labelNote.MaximumSize = new System.Drawing.Size(233, 0);
            labelNote.Name = "labelNote";
            labelNote.TabIndex = 8;
            labelNote.Text = "Sorts the selected lines, or the whole text. Each line moves whole, with its formatting; indents and runs of spaces don't count; blank lines go.";
            //
            // buttonApply
            //
            buttonApply.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonApply.Name = "buttonApply";
            buttonApply.Size = new System.Drawing.Size(113, 30);
            buttonApply.TabIndex = 9;
            buttonApply.Text = "&Sort";
            buttonApply.UseVisualStyleBackColor = true;
            //
            // AlphaSortPanel
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tableLayout);
            Name = "AlphaSortPanel";
            Size = new System.Drawing.Size(243, 468);
            panelLetterRule.ResumeLayout(false);
            panelLetterRule.PerformLayout();
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.RadioButton radioWord;
        private System.Windows.Forms.RadioButton radioLetter;
        private System.Windows.Forms.FlowLayoutPanel panelLetterRule;
        private System.Windows.Forms.RadioButton radioStrict;
        private System.Windows.Forms.RadioButton radioChicago;
        private System.Windows.Forms.CheckBox checkBoxCase;
        private System.Windows.Forms.CheckBox checkBoxDuplicates;
        private System.Windows.Forms.Label labelSampleTitle;
        private System.Windows.Forms.Label labelSample;
        private System.Windows.Forms.Label labelNote;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
