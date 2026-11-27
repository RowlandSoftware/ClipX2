namespace ClipX2.UI
{
    partial class PersonalDictionaryPage
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
            labelFind = new System.Windows.Forms.Label();
            textBoxFind = new System.Windows.Forms.TextBox();
            listBoxWords = new System.Windows.Forms.ListBox();
            labelCount = new System.Windows.Forms.Label();
            checkBoxKnownOnly = new System.Windows.Forms.CheckBox();
            labelWord = new System.Windows.Forms.Label();
            textBoxWord = new System.Windows.Forms.TextBox();
            labelCheck = new System.Windows.Forms.Label();
            checkBoxAnyway = new System.Windows.Forms.CheckBox();
            buttonAdd = new System.Windows.Forms.Button();
            buttonChange = new System.Windows.Forms.Button();
            buttonDelete = new System.Windows.Forms.Button();
            labelForms = new System.Windows.Forms.Label();
            checkedListBoxForms = new System.Windows.Forms.CheckedListBox();
            checkBoxAllForms = new System.Windows.Forms.CheckBox();
            labelLanguage = new System.Windows.Forms.Label();
            toolTip = new System.Windows.Forms.ToolTip(components);
            SuspendLayout();
            //
            // labelFind
            //
            labelFind.AutoSize = true;
            labelFind.Location = new System.Drawing.Point(8, 12);
            labelFind.Name = "labelFind";
            labelFind.TabIndex = 0;
            labelFind.Text = "&Find:";
            //
            // textBoxFind
            //
            textBoxFind.Location = new System.Drawing.Point(52, 9);
            textBoxFind.Name = "textBoxFind";
            textBoxFind.PlaceholderText = "Part of a word";
            textBoxFind.Size = new System.Drawing.Size(226, 25);
            textBoxFind.TabIndex = 1;
            toolTip.SetToolTip(textBoxFind, "Shows only the words containing this (capitals or not)");
            //
            // listBoxWords
            //
            listBoxWords.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            listBoxWords.IntegralHeight = false;
            listBoxWords.Location = new System.Drawing.Point(8, 40);
            listBoxWords.Name = "listBoxWords";
            listBoxWords.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            listBoxWords.Size = new System.Drawing.Size(270, 340);
            listBoxWords.TabIndex = 2;
            toolTip.SetToolTip(listBoxWords, "Your words. Select one to change it; Ctrl or Shift + click to select several to delete");
            //
            // labelCount
            //
            labelCount.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            labelCount.AutoSize = true;
            labelCount.ForeColor = System.Drawing.SystemColors.GrayText;
            labelCount.Location = new System.Drawing.Point(8, 384);
            labelCount.Name = "labelCount";
            labelCount.TabIndex = 3;
            labelCount.Text = "0 words";
            //
            // checkBoxKnownOnly
            //
            checkBoxKnownOnly.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkBoxKnownOnly.AutoSize = true;
            checkBoxKnownOnly.Location = new System.Drawing.Point(8, 406);
            checkBoxKnownOnly.Name = "checkBoxKnownOnly";
            checkBoxKnownOnly.TabIndex = 4;
            checkBoxKnownOnly.Text = "Only words the dictionary &has itself";
            toolTip.SetToolTip(checkBoxKnownOnly, "Shows only your words that the dictionary in use already accepts (a place name such as Brereton may be one): you may not need them there");
            checkBoxKnownOnly.UseVisualStyleBackColor = true;
            //
            // labelWord
            //
            labelWord.AutoSize = true;
            labelWord.Location = new System.Drawing.Point(300, 12);
            labelWord.Name = "labelWord";
            labelWord.TabIndex = 5;
            labelWord.Text = "&Word:";
            //
            // textBoxWord
            //
            textBoxWord.Location = new System.Drawing.Point(350, 9);
            textBoxWord.Name = "textBoxWord";
            textBoxWord.Size = new System.Drawing.Size(250, 25);
            textBoxWord.TabIndex = 6;
            toolTip.SetToolTip(textBoxWord, "A word to add, or the selected word to change");
            //
            // labelCheck
            //
            labelCheck.ForeColor = System.Drawing.Color.Navy;
            labelCheck.Location = new System.Drawing.Point(300, 40);
            labelCheck.Name = "labelCheck";
            labelCheck.Size = new System.Drawing.Size(300, 40);
            labelCheck.TabIndex = 7;
            //
            // checkBoxAnyway
            //
            checkBoxAnyway.Location = new System.Drawing.Point(300, 80);
            checkBoxAnyway.Name = "checkBoxAnyway";
            checkBoxAnyway.Size = new System.Drawing.Size(300, 42);
            checkBoxAnyway.TabIndex = 8;
            checkBoxAnyway.Text = "Add it a&nyway (for another language)";
            toolTip.SetToolTip(checkBoxAnyway, "Your words count in every language: add a word the dictionary has when you want it accepted in other languages too");
            checkBoxAnyway.UseVisualStyleBackColor = true;
            //
            // buttonAdd
            //
            buttonAdd.Location = new System.Drawing.Point(300, 126);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new System.Drawing.Size(94, 30);
            buttonAdd.TabIndex = 9;
            buttonAdd.Text = "A&dd";
            toolTip.SetToolTip(buttonAdd, "Adds the word, and the other forms ticked below (Enter)");
            buttonAdd.UseVisualStyleBackColor = true;
            //
            // buttonChange
            //
            buttonChange.Location = new System.Drawing.Point(403, 126);
            buttonChange.Name = "buttonChange";
            buttonChange.Size = new System.Drawing.Size(94, 30);
            buttonChange.TabIndex = 10;
            buttonChange.Text = "&Change";
            toolTip.SetToolTip(buttonChange, "Replaces the selected word with the one typed (a correction)");
            buttonChange.UseVisualStyleBackColor = true;
            //
            // buttonDelete
            //
            buttonDelete.Location = new System.Drawing.Point(506, 126);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new System.Drawing.Size(94, 30);
            buttonDelete.TabIndex = 11;
            buttonDelete.Text = "D&elete";
            toolTip.SetToolTip(buttonDelete, "Deletes the selected words (Delete key)");
            buttonDelete.UseVisualStyleBackColor = true;
            //
            // labelForms
            //
            labelForms.Location = new System.Drawing.Point(300, 170);
            labelForms.Name = "labelForms";
            labelForms.Size = new System.Drawing.Size(300, 20);
            labelForms.TabIndex = 12;
            labelForms.Text = "Other forms to add:";
            //
            // checkedListBoxForms
            //
            checkedListBoxForms.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkedListBoxForms.CheckOnClick = true;
            checkedListBoxForms.IntegralHeight = false;
            checkedListBoxForms.Location = new System.Drawing.Point(300, 192);
            checkedListBoxForms.Name = "checkedListBoxForms";
            checkedListBoxForms.Size = new System.Drawing.Size(300, 188);
            checkedListBoxForms.TabIndex = 13;
            toolTip.SetToolTip(checkedListBoxForms, "Made with the endings of a dictionary word that ends the same way. Ticked: the usual ones (-s, -ed, -ing); tick any others you want");
            //
            // checkBoxAllForms
            //
            checkBoxAllForms.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkBoxAllForms.AutoSize = true;
            checkBoxAllForms.Location = new System.Drawing.Point(300, 384);
            checkBoxAllForms.Name = "checkBoxAllForms";
            checkBoxAllForms.TabIndex = 14;
            checkBoxAllForms.Text = "A&ll forms";
            checkBoxAllForms.UseVisualStyleBackColor = true;
            //
            // labelLanguage
            //
            labelLanguage.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            labelLanguage.ForeColor = System.Drawing.SystemColors.GrayText;
            labelLanguage.Location = new System.Drawing.Point(300, 404);
            labelLanguage.Name = "labelLanguage";
            labelLanguage.Size = new System.Drawing.Size(300, 56);
            labelLanguage.TabIndex = 15;
            labelLanguage.Text = "Checked against the dictionary in use. Changes are saved at once.";
            //
            // PersonalDictionaryPage
            //
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            Controls.Add(labelFind);
            Controls.Add(textBoxFind);
            Controls.Add(listBoxWords);
            Controls.Add(labelCount);
            Controls.Add(checkBoxKnownOnly);
            Controls.Add(labelWord);
            Controls.Add(textBoxWord);
            Controls.Add(labelCheck);
            Controls.Add(checkBoxAnyway);
            Controls.Add(buttonAdd);
            Controls.Add(buttonChange);
            Controls.Add(buttonDelete);
            Controls.Add(labelForms);
            Controls.Add(checkedListBoxForms);
            Controls.Add(checkBoxAllForms);
            Controls.Add(labelLanguage);
            Name = "PersonalDictionaryPage";
            Size = new System.Drawing.Size(606, 464);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelFind;
        private System.Windows.Forms.TextBox textBoxFind;
        private System.Windows.Forms.ListBox listBoxWords;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.CheckBox checkBoxKnownOnly;
        private System.Windows.Forms.Label labelWord;
        private System.Windows.Forms.TextBox textBoxWord;
        private System.Windows.Forms.Label labelCheck;
        private System.Windows.Forms.CheckBox checkBoxAnyway;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonChange;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Label labelForms;
        private System.Windows.Forms.CheckedListBox checkedListBoxForms;
        private System.Windows.Forms.CheckBox checkBoxAllForms;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
