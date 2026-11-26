namespace ClipX2.UI.Panels
{
    partial class SpellingPanel
    {
        /// <summary>Required designer variable.</summary>
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
            comboLanguage = new System.Windows.Forms.ComboBox();
            labelKind = new System.Windows.Forms.Label();
            labelChangeTo = new System.Windows.Forms.Label();
            textBoxChangeTo = new System.Windows.Forms.TextBox();
            listBoxSuggestions = new System.Windows.Forms.ListBox();
            buttonIgnore = new System.Windows.Forms.Button();
            buttonIgnoreAll = new System.Windows.Forms.Button();
            buttonChange = new System.Windows.Forms.Button();
            buttonChangeAll = new System.Windows.Forms.Button();
            buttonAdd = new System.Windows.Forms.Button();
            buttonDelete = new System.Windows.Forms.Button();
            checkBoxSmartReplace = new System.Windows.Forms.CheckBox();
            checkBoxGrammar = new System.Windows.Forms.CheckBox();
            labelWord = new System.Windows.Forms.Label();
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
            tableLayout.Controls.Add(comboLanguage, 0, 1);
            tableLayout.Controls.Add(checkBoxGrammar, 1, 1);
            tableLayout.Controls.Add(labelKind, 0, 2);
            tableLayout.Controls.Add(labelChangeTo, 0, 3);
            tableLayout.Controls.Add(textBoxChangeTo, 0, 4);
            tableLayout.Controls.Add(listBoxSuggestions, 0, 5);
            tableLayout.Controls.Add(buttonIgnore, 0, 6);
            tableLayout.Controls.Add(buttonIgnoreAll, 1, 6);
            tableLayout.Controls.Add(buttonChange, 0, 7);
            tableLayout.Controls.Add(buttonChangeAll, 1, 7);
            tableLayout.Controls.Add(buttonAdd, 0, 8);
            tableLayout.Controls.Add(buttonDelete, 0, 9);
            tableLayout.Controls.Add(checkBoxSmartReplace, 1, 3);
            tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayout.Location = new System.Drawing.Point(0, 0);
            tableLayout.Name = "tableLayout";
            tableLayout.Padding = new System.Windows.Forms.Padding(2);
            tableLayout.RowCount = 10;
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.Size = new System.Drawing.Size(243, 430);
            tableLayout.TabIndex = 0;
            // 
            // labelTitle
            // 
            labelTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelTitle.AutoSize = true;
            labelTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            labelTitle.Location = new System.Drawing.Point(5, 8);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new System.Drawing.Size(63, 19);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "Spelling";
            // 
            // comboLanguage
            // 
            comboLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
            comboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboLanguage.Location = new System.Drawing.Point(124, 5);
            comboLanguage.Name = "comboLanguage";
            comboLanguage.Size = new System.Drawing.Size(114, 25);
            comboLanguage.TabIndex = 12;
            // 
            // labelKind
            // 
            labelKind.AutoSize = true;
            tableLayout.SetColumnSpan(labelKind, 2);
            labelKind.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            labelKind.ForeColor = System.Drawing.Color.Navy;
            labelKind.Location = new System.Drawing.Point(5, 39);
            labelKind.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            labelKind.MaximumSize = new System.Drawing.Size(233, 0);
            labelKind.Name = "labelKind";
            labelKind.Size = new System.Drawing.Size(114, 19);
            labelKind.TabIndex = 1;
            labelKind.Text = "Not in dictionary:";
            // 
            // labelChangeTo
            // 
            labelChangeTo.AutoSize = true;
            labelChangeTo.Location = new System.Drawing.Point(5, 64);
            labelChangeTo.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            labelChangeTo.Name = "labelChangeTo";
            labelChangeTo.Size = new System.Drawing.Size(76, 19);
            labelChangeTo.TabIndex = 3;
            labelChangeTo.Text = "C&hange to:";
            // 
            // textBoxChangeTo
            // 
            tableLayout.SetColumnSpan(textBoxChangeTo, 2);
            textBoxChangeTo.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxChangeTo.Location = new System.Drawing.Point(5, 87);
            textBoxChangeTo.Name = "textBoxChangeTo";
            textBoxChangeTo.Size = new System.Drawing.Size(233, 25);
            textBoxChangeTo.TabIndex = 4;
            toolTip.SetToolTip(textBoxChangeTo, "Right-click to see dictionary definition");
            // 
            // listBoxSuggestions
            // 
            tableLayout.SetColumnSpan(listBoxSuggestions, 2);
            listBoxSuggestions.Dock = System.Windows.Forms.DockStyle.Fill;
            listBoxSuggestions.IntegralHeight = false;
            listBoxSuggestions.Location = new System.Drawing.Point(5, 118);
            listBoxSuggestions.Name = "listBoxSuggestions";
            listBoxSuggestions.Size = new System.Drawing.Size(233, 162);
            listBoxSuggestions.TabIndex = 6;
            toolTip.SetToolTip(listBoxSuggestions, "Right-click to see dictionary definition");
            // 
            // buttonIgnore
            // 
            buttonIgnore.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonIgnore.Location = new System.Drawing.Point(5, 286);
            buttonIgnore.Name = "buttonIgnore";
            buttonIgnore.Size = new System.Drawing.Size(113, 30);
            buttonIgnore.TabIndex = 6;
            buttonIgnore.Text = "&Ignore";
            buttonIgnore.UseVisualStyleBackColor = true;
            // 
            // buttonIgnoreAll
            // 
            buttonIgnoreAll.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonIgnoreAll.Location = new System.Drawing.Point(124, 286);
            buttonIgnoreAll.Name = "buttonIgnoreAll";
            buttonIgnoreAll.Size = new System.Drawing.Size(114, 30);
            buttonIgnoreAll.TabIndex = 7;
            buttonIgnoreAll.Text = "I&gnore All";
            buttonIgnoreAll.UseVisualStyleBackColor = true;
            // 
            // buttonChange
            // 
            buttonChange.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonChange.Location = new System.Drawing.Point(5, 322);
            buttonChange.Name = "buttonChange";
            buttonChange.Size = new System.Drawing.Size(113, 30);
            buttonChange.TabIndex = 8;
            buttonChange.Text = "&Change";
            buttonChange.UseVisualStyleBackColor = true;
            // 
            // buttonChangeAll
            // 
            buttonChangeAll.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonChangeAll.Location = new System.Drawing.Point(124, 322);
            buttonChangeAll.Name = "buttonChangeAll";
            buttonChangeAll.Size = new System.Drawing.Size(114, 30);
            buttonChangeAll.TabIndex = 9;
            buttonChangeAll.Text = "Change A&ll";
            buttonChangeAll.UseVisualStyleBackColor = true;
            // 
            // buttonAdd
            // 
            tableLayout.SetColumnSpan(buttonAdd, 2);
            buttonAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonAdd.Location = new System.Drawing.Point(5, 358);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new System.Drawing.Size(233, 30);
            buttonAdd.TabIndex = 10;
            buttonAdd.Text = "&Add to Dictionary";
            buttonAdd.UseVisualStyleBackColor = true;
            // 
            // buttonDelete
            // 
            buttonDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonDelete.Location = new System.Drawing.Point(5, 394);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new System.Drawing.Size(113, 31);
            buttonDelete.TabIndex = 11;
            buttonDelete.Text = "D&elete";
            buttonDelete.UseVisualStyleBackColor = true;
            // 
            // checkBoxSmartReplace
            // 
            checkBoxSmartReplace.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            checkBoxSmartReplace.AutoSize = true;
            checkBoxSmartReplace.Checked = true;
            checkBoxSmartReplace.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxSmartReplace.Location = new System.Drawing.Point(127, 61);
            checkBoxSmartReplace.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            checkBoxSmartReplace.Name = "checkBoxSmartReplace";
            checkBoxSmartReplace.Size = new System.Drawing.Size(111, 23);
            checkBoxSmartReplace.TabIndex = 5;
            checkBoxSmartReplace.Text = "&Smart replace";
            checkBoxSmartReplace.UseVisualStyleBackColor = true;
            //
            // checkBoxGrammar
            //
            checkBoxGrammar.AutoSize = true;
            checkBoxGrammar.Location = new System.Drawing.Point(127, 33);
            checkBoxGrammar.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            checkBoxGrammar.Name = "checkBoxGrammar";
            checkBoxGrammar.Size = new System.Drawing.Size(85, 23);
            checkBoxGrammar.TabIndex = 2;
            checkBoxGrammar.Text = "Gra&mmar";
            toolTip.SetToolTip(checkBoxGrammar, "Check grammar too, with LanguageTool (the text is sent to its server)");
            checkBoxGrammar.UseVisualStyleBackColor = true;
            // 
            // labelWord
            // 
            labelWord.AutoEllipsis = true;
            labelWord.BackColor = System.Drawing.SystemColors.Window;
            labelWord.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            labelWord.Cursor = System.Windows.Forms.Cursors.Hand;
            labelWord.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            labelWord.Location = new System.Drawing.Point(122, 36);
            labelWord.Name = "labelWord";
            labelWord.Size = new System.Drawing.Size(116, 25);
            labelWord.TabIndex = 15;
            labelWord.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            toolTip.SetToolTip(labelWord, "Click to edit in Change to box");
            labelWord.Visible = false;
            // 
            // SpellingPanel
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(labelWord);
            Controls.Add(tableLayout);
            Name = "SpellingPanel";
            Size = new System.Drawing.Size(243, 430);
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.ComboBox comboLanguage;
        private System.Windows.Forms.Label labelKind;
        private System.Windows.Forms.Label labelWord;
        private System.Windows.Forms.Label labelChangeTo;
        private System.Windows.Forms.TextBox textBoxChangeTo;
        private System.Windows.Forms.ListBox listBoxSuggestions;
        private System.Windows.Forms.Button buttonIgnore;
        private System.Windows.Forms.Button buttonIgnoreAll;
        private System.Windows.Forms.Button buttonChange;
        private System.Windows.Forms.Button buttonChangeAll;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.CheckBox checkBoxSmartReplace;
        private System.Windows.Forms.CheckBox checkBoxGrammar;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
