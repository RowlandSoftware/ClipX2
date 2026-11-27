namespace ClipX2.UI.Panels
{
    partial class SmartSortPanel
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
            labelPreset = new System.Windows.Forms.Label();
            comboPreset = new System.Windows.Forms.ComboBox();
            radioWord = new System.Windows.Forms.RadioButton();
            radioLetter = new System.Windows.Forms.RadioButton();
            checkBoxComma = new System.Windows.Forms.CheckBox();
            checkBoxBrackets = new System.Windows.Forms.CheckBox();
            checkBoxMc = new System.Windows.Forms.CheckBox();
            checkBoxSaint = new System.Windows.Forms.CheckBox();
            checkBoxInitialisms = new System.Windows.Forms.CheckBox();
            labelNumbers = new System.Windows.Forms.Label();
            comboNumbers = new System.Windows.Forms.ComboBox();
            checkBoxCase = new System.Windows.Forms.CheckBox();
            checkBoxDuplicates = new System.Windows.Forms.CheckBox();
            labelSampleTitle = new System.Windows.Forms.Label();
            labelSample = new System.Windows.Forms.Label();
            buttonApply = new System.Windows.Forms.Button();
            toolTip = new System.Windows.Forms.ToolTip(components);
            tableLayout.SuspendLayout();
            SuspendLayout();
            //
            // tableLayout
            //
            tableLayout.ColumnCount = 2;
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.Controls.Add(labelTitle, 0, 0);
            tableLayout.Controls.Add(labelPreset, 0, 1);
            tableLayout.Controls.Add(comboPreset, 0, 2);
            tableLayout.Controls.Add(radioWord, 0, 3);
            tableLayout.Controls.Add(radioLetter, 0, 4);
            tableLayout.Controls.Add(checkBoxComma, 0, 5);
            tableLayout.Controls.Add(checkBoxBrackets, 0, 6);
            tableLayout.Controls.Add(checkBoxMc, 0, 7);
            tableLayout.Controls.Add(checkBoxSaint, 0, 8);
            tableLayout.Controls.Add(checkBoxInitialisms, 0, 9);
            tableLayout.Controls.Add(labelNumbers, 0, 10);
            tableLayout.Controls.Add(comboNumbers, 1, 10);
            tableLayout.Controls.Add(checkBoxCase, 0, 11);
            tableLayout.Controls.Add(checkBoxDuplicates, 0, 12);
            tableLayout.Controls.Add(labelSampleTitle, 0, 13);
            tableLayout.Controls.Add(labelSample, 0, 14);
            tableLayout.Controls.Add(buttonApply, 1, 15);
            tableLayout.AutoSize = true;
            tableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableLayout.Dock = System.Windows.Forms.DockStyle.Top;
            tableLayout.Location = new System.Drawing.Point(0, 0);
            tableLayout.Name = "tableLayout";
            tableLayout.Padding = new System.Windows.Forms.Padding(2);
            tableLayout.RowCount = 16;
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.Size = new System.Drawing.Size(243, 640);
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
            labelTitle.Text = "Smart sort";
            //
            // labelPreset
            //
            labelPreset.AutoSize = true;
            tableLayout.SetColumnSpan(labelPreset, 2);
            labelPreset.Name = "labelPreset";
            labelPreset.TabIndex = 1;
            labelPreset.Text = "St&yle:";
            //
            // comboPreset
            //
            tableLayout.SetColumnSpan(comboPreset, 2);
            comboPreset.Dock = System.Windows.Forms.DockStyle.Fill;
            comboPreset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboPreset.Items.AddRange(new object[] { "Custom", "NISO TR03-1999", "BS ISO 999 word by word (Hart's)", "BS ISO 999 letter by letter (Hart's)", "Chicago word by word", "Chicago letter by letter" });
            comboPreset.Name = "comboPreset";
            comboPreset.TabIndex = 2;
            toolTip.SetToolTip(comboPreset, "Sets the options below as a standard or guide has them; changing them makes it Custom");
            //
            // radioWord
            //
            radioWord.AutoSize = true;
            tableLayout.SetColumnSpan(radioWord, 2);
            radioWord.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            radioWord.Name = "radioWord";
            radioWord.TabIndex = 3;
            radioWord.TabStop = true;
            radioWord.Text = "&Word by word";
            toolTip.SetToolTip(radioWord, "Nothing before something: a space (or hyphen, dash, slash) before any letter: New York before Newark");
            radioWord.UseVisualStyleBackColor = true;
            //
            // radioLetter
            //
            radioLetter.AutoSize = true;
            tableLayout.SetColumnSpan(radioLetter, 2);
            radioLetter.Name = "radioLetter";
            radioLetter.TabIndex = 4;
            radioLetter.Text = "&Letter by letter";
            toolTip.SetToolTip(radioLetter, "Spaces, hyphens and punctuation ignored: Newark before New York");
            radioLetter.UseVisualStyleBackColor = true;
            //
            // checkBoxComma
            //
            checkBoxComma.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxComma, 2);
            checkBoxComma.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            checkBoxComma.Name = "checkBoxComma";
            checkBoxComma.TabIndex = 5;
            checkBoxComma.Text = "Stop at a &comma";
            toolTip.SetToolTip(checkBoxComma, "A comma ends the first part (inverted headings): Bath; Bath, Order of the; Bath bun. Unless text in brackets is ignored, an opening parenthesis does too, before the comma (Chicago). Off: commas ignored (NISO)");
            checkBoxComma.UseVisualStyleBackColor = true;
            //
            // checkBoxBrackets
            //
            checkBoxBrackets.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxBrackets, 2);
            checkBoxBrackets.Name = "checkBoxBrackets";
            checkBoxBrackets.TabIndex = 6;
            checkBoxBrackets.Text = "&Ignore text in brackets";
            toolTip.SetToolTip(checkBoxBrackets, "Text in ( ) or [ ] doesn't count, except to break a tie: high (light-headed) as high (Hart's). Off: its words count as any others (NISO)");
            checkBoxBrackets.UseVisualStyleBackColor = true;
            //
            // checkBoxMc
            //
            checkBoxMc.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxMc, 2);
            checkBoxMc.Name = "checkBoxMc";
            checkBoxMc.TabIndex = 7;
            checkBoxMc.Text = "&Mc as Mac";
            toolTip.SetToolTip(checkBoxMc, "McDonald sorts as MacDonald (Hart's)");
            checkBoxMc.UseVisualStyleBackColor = true;
            //
            // checkBoxSaint
            //
            checkBoxSaint.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxSaint, 2);
            checkBoxSaint.Name = "checkBoxSaint";
            checkBoxSaint.TabIndex = 8;
            checkBoxSaint.Text = "S&t as Saint";
            toolTip.SetToolTip(checkBoxSaint, "St, St. and Ste before a name sort as Saint and Sainte: St Ives with Saint Albans. NISO: as written");
            checkBoxSaint.UseVisualStyleBackColor = true;
            //
            // checkBoxInitialisms
            //
            checkBoxInitialisms.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxInitialisms, 2);
            checkBoxInitialisms.Name = "checkBoxInitialisms";
            checkBoxInitialisms.TabIndex = 9;
            checkBoxInitialisms.Text = "Initi&alisms said as letters first";
            toolTip.SetToolTip(checkBoxInitialisms, "BBC, A.B.C. sort as separate letters, before words starting with the same letter: BBC before Bach (Hart's). Ones said as words (NATO, NASA, UNESCO) sort as words. ClipX judges by whether the letters can be said as syllables");
            checkBoxInitialisms.UseVisualStyleBackColor = true;
            //
            // labelNumbers
            //
            labelNumbers.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelNumbers.AutoSize = true;
            labelNumbers.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            labelNumbers.Name = "labelNumbers";
            labelNumbers.TabIndex = 10;
            labelNumbers.Text = "&Numbers:";
            //
            // comboNumbers
            //
            comboNumbers.Dock = System.Windows.Forms.DockStyle.Fill;
            comboNumbers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboNumbers.Items.AddRange(new object[] { "By value", "Version numbers", "Digit by digit" });
            comboNumbers.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            comboNumbers.Name = "comboNumbers";
            comboNumbers.TabIndex = 11;
            toolTip.SetToolTip(comboNumbers, "By value: 2 before 10, decimals too (Dewey). Version numbers: 1.5 before 1.10. Digit by digit: 10 before 2");
            //
            // checkBoxCase
            //
            checkBoxCase.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxCase, 2);
            checkBoxCase.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            checkBoxCase.Name = "checkBoxCase";
            checkBoxCase.TabIndex = 12;
            checkBoxCase.Text = "Case sensiti&ve";
            toolTip.SetToolTip(checkBoxCase, "Capitals before small letters, as two alphabets: Zebra before apple. Off: case is equal (Apple before apple only when otherwise the same)");
            checkBoxCase.UseVisualStyleBackColor = true;
            //
            // checkBoxDuplicates
            //
            checkBoxDuplicates.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxDuplicates, 2);
            checkBoxDuplicates.Name = "checkBoxDuplicates";
            checkBoxDuplicates.TabIndex = 13;
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
            labelSampleTitle.TabIndex = 14;
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
            labelSample.TabIndex = 15;
            labelSample.Text = "Sample";
            //
            // buttonApply
            //
            buttonApply.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonApply.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            buttonApply.Name = "buttonApply";
            buttonApply.Size = new System.Drawing.Size(113, 30);
            buttonApply.TabIndex = 16;
            buttonApply.Text = "&Sort";
            buttonApply.UseVisualStyleBackColor = true;
            //
            // SmartSortPanel
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoScroll = true;
            Controls.Add(tableLayout);
            Name = "SmartSortPanel";
            Size = new System.Drawing.Size(243, 640);
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelPreset;
        private System.Windows.Forms.ComboBox comboPreset;
        private System.Windows.Forms.RadioButton radioWord;
        private System.Windows.Forms.RadioButton radioLetter;
        private System.Windows.Forms.CheckBox checkBoxComma;
        private System.Windows.Forms.CheckBox checkBoxBrackets;
        private System.Windows.Forms.CheckBox checkBoxMc;
        private System.Windows.Forms.CheckBox checkBoxSaint;
        private System.Windows.Forms.CheckBox checkBoxInitialisms;
        private System.Windows.Forms.Label labelNumbers;
        private System.Windows.Forms.ComboBox comboNumbers;
        private System.Windows.Forms.CheckBox checkBoxCase;
        private System.Windows.Forms.CheckBox checkBoxDuplicates;
        private System.Windows.Forms.Label labelSampleTitle;
        private System.Windows.Forms.Label labelSample;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
