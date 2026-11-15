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
            tableLayout = new System.Windows.Forms.TableLayoutPanel();
            labelTitle = new System.Windows.Forms.Label();
            comboLanguage = new System.Windows.Forms.ComboBox();
            labelKind = new System.Windows.Forms.Label();
            richTextBoxContext = new System.Windows.Forms.RichTextBox();
            labelChangeTo = new System.Windows.Forms.Label();
            textBoxChangeTo = new System.Windows.Forms.TextBox();
            listBoxSuggestions = new System.Windows.Forms.ListBox();
            buttonIgnore = new System.Windows.Forms.Button();
            buttonIgnoreAll = new System.Windows.Forms.Button();
            buttonChange = new System.Windows.Forms.Button();
            buttonChangeAll = new System.Windows.Forms.Button();
            buttonAdd = new System.Windows.Forms.Button();
            buttonDelete = new System.Windows.Forms.Button();
            buttonClose = new System.Windows.Forms.Button();
            checkBoxSmartReplace = new System.Windows.Forms.CheckBox();
            tableLayout.SuspendLayout();
            SuspendLayout();
            //
            // tableLayout
            //
            tableLayout.ColumnCount = 2;
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.Controls.Add(labelTitle, 0, 0);
            tableLayout.Controls.Add(comboLanguage, 1, 0);
            tableLayout.Controls.Add(labelKind, 0, 1);
            tableLayout.Controls.Add(richTextBoxContext, 0, 2);
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
            tableLayout.Controls.Add(buttonClose, 1, 9);
            tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayout.Location = new System.Drawing.Point(0, 0);
            tableLayout.Name = "tableLayout";
            tableLayout.Padding = new System.Windows.Forms.Padding(2);
            tableLayout.RowCount = 10;
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                              // 0 title + language
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                              // 1 Not in dictionary: / messages
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));    // 2 context
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                              // 3 Change to: + Smart replace
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                              // 4 change-to box
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));    // 5 suggestions
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                              // 6 Ignore / Ignore All
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                              // 7 Change / Change All
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                              // 8 Add to Dictionary
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());                                              // 9 Delete / Close
            tableLayout.Size = new System.Drawing.Size(243, 430);
            tableLayout.TabIndex = 0;
            //
            // labelTitle
            //
            labelTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelTitle.AutoSize = true;
            labelTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            labelTitle.Name = "labelTitle";
            labelTitle.TabIndex = 0;
            labelTitle.Text = "Spelling";
            //
            // comboLanguage
            //
            comboLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
            comboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboLanguage.Name = "comboLanguage";
            comboLanguage.TabIndex = 12;
            //
            // labelKind
            //
            labelKind.AutoSize = true;
            tableLayout.SetColumnSpan(labelKind, 2);
            labelKind.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            labelKind.MaximumSize = new System.Drawing.Size(233, 0);
            labelKind.Name = "labelKind";
            labelKind.TabIndex = 1;
            labelKind.Text = "Not in dictionary:";
            //
            // richTextBoxContext
            //
            richTextBoxContext.BackColor = System.Drawing.SystemColors.Window;
            tableLayout.SetColumnSpan(richTextBoxContext, 2);
            richTextBoxContext.Dock = System.Windows.Forms.DockStyle.Fill;
            richTextBoxContext.Name = "richTextBoxContext";
            richTextBoxContext.ReadOnly = true;
            richTextBoxContext.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            richTextBoxContext.TabIndex = 2;
            richTextBoxContext.TabStop = false;
            richTextBoxContext.Text = "";
            //
            // labelChangeTo
            //
            labelChangeTo.AutoSize = true;
            tableLayout.SetColumnSpan(labelChangeTo, 1);
            labelChangeTo.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            labelChangeTo.Name = "labelChangeTo";
            labelChangeTo.TabIndex = 3;
            labelChangeTo.Text = "C&hange to:";
            //
            // textBoxChangeTo
            //
            tableLayout.SetColumnSpan(textBoxChangeTo, 2);
            textBoxChangeTo.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxChangeTo.Name = "textBoxChangeTo";
            textBoxChangeTo.TabIndex = 4;
            //
            // listBoxSuggestions
            //
            tableLayout.SetColumnSpan(listBoxSuggestions, 2);
            listBoxSuggestions.Dock = System.Windows.Forms.DockStyle.Fill;
            listBoxSuggestions.IntegralHeight = false;
            listBoxSuggestions.Name = "listBoxSuggestions";
            listBoxSuggestions.TabIndex = 6;
            //
            // buttonIgnore
            //
            buttonIgnore.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonIgnore.Name = "buttonIgnore";
            buttonIgnore.Size = new System.Drawing.Size(112, 30);
            buttonIgnore.TabIndex = 6;
            buttonIgnore.Text = "&Ignore";
            buttonIgnore.UseVisualStyleBackColor = true;
            //
            // buttonIgnoreAll
            //
            buttonIgnoreAll.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonIgnoreAll.Name = "buttonIgnoreAll";
            buttonIgnoreAll.Size = new System.Drawing.Size(112, 30);
            buttonIgnoreAll.TabIndex = 7;
            buttonIgnoreAll.Text = "I&gnore All";
            buttonIgnoreAll.UseVisualStyleBackColor = true;
            //
            // buttonChange
            //
            buttonChange.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonChange.Name = "buttonChange";
            buttonChange.Size = new System.Drawing.Size(112, 30);
            buttonChange.TabIndex = 8;
            buttonChange.Text = "&Change";
            buttonChange.UseVisualStyleBackColor = true;
            //
            // buttonChangeAll
            //
            buttonChangeAll.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonChangeAll.Name = "buttonChangeAll";
            buttonChangeAll.Size = new System.Drawing.Size(112, 30);
            buttonChangeAll.TabIndex = 9;
            buttonChangeAll.Text = "Change A&ll";
            buttonChangeAll.UseVisualStyleBackColor = true;
            //
            // buttonAdd
            //
            tableLayout.SetColumnSpan(buttonAdd, 2);
            buttonAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new System.Drawing.Size(112, 30);
            buttonAdd.TabIndex = 10;
            buttonAdd.Text = "&Add to Dictionary"; // the personal dictionary, saved at once
            buttonAdd.UseVisualStyleBackColor = true;
            //
            // buttonDelete
            //
            buttonDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new System.Drawing.Size(112, 30);
            buttonDelete.TabIndex = 11;
            buttonDelete.Text = "D&elete"; // Alt+D is Done
            buttonDelete.UseVisualStyleBackColor = true;
            //
            // buttonClose
            //
            buttonClose.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(112, 30);
            buttonClose.TabIndex = 13;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            //
            // checkBoxSmartReplace
            //
            checkBoxSmartReplace.Anchor = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
            checkBoxSmartReplace.AutoSize = true;
            checkBoxSmartReplace.Checked = true;
            checkBoxSmartReplace.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxSmartReplace.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            checkBoxSmartReplace.Name = "checkBoxSmartReplace";
            checkBoxSmartReplace.TabIndex = 5;
            checkBoxSmartReplace.Text = "&Smart replace";
            checkBoxSmartReplace.UseVisualStyleBackColor = true;
            //
            // SpellingPanel
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
        private System.Windows.Forms.RichTextBox richTextBoxContext;
        private System.Windows.Forms.Label labelChangeTo;
        private System.Windows.Forms.TextBox textBoxChangeTo;
        private System.Windows.Forms.ListBox listBoxSuggestions;
        private System.Windows.Forms.Button buttonIgnore;
        private System.Windows.Forms.Button buttonIgnoreAll;
        private System.Windows.Forms.Button buttonChange;
        private System.Windows.Forms.Button buttonChangeAll;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.CheckBox checkBoxSmartReplace;
    }
}
