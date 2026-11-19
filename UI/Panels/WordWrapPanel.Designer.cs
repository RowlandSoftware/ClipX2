namespace ClipX2.UI.Panels
{
    partial class WordWrapPanel
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
            labelWidth = new System.Windows.Forms.Label();
            numericWidth = new System.Windows.Forms.NumericUpDown();
            checkBoxHanging = new System.Windows.Forms.CheckBox();
            radioTab = new System.Windows.Forms.RadioButton();
            radioSpaces = new System.Windows.Forms.RadioButton();
            checkBoxJustify = new System.Windows.Forms.CheckBox();
            labelSampleNote = new System.Windows.Forms.Label();
            textBoxSample = new System.Windows.Forms.TextBox();
            labelNote = new System.Windows.Forms.Label();
            buttonApply = new System.Windows.Forms.Button();
            buttonClose = new System.Windows.Forms.Button();
            toolTip = new System.Windows.Forms.ToolTip(components);
            tableLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericWidth).BeginInit();
            SuspendLayout();
            //
            // tableLayout
            //
            tableLayout.ColumnCount = 2;
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.Controls.Add(labelTitle, 0, 0);
            tableLayout.Controls.Add(labelWidth, 0, 1);
            tableLayout.Controls.Add(numericWidth, 1, 1);
            tableLayout.Controls.Add(checkBoxHanging, 0, 2);
            tableLayout.Controls.Add(radioTab, 0, 3);
            tableLayout.Controls.Add(radioSpaces, 1, 3);
            tableLayout.Controls.Add(checkBoxJustify, 0, 4);
            tableLayout.Controls.Add(labelSampleNote, 0, 5);
            tableLayout.Controls.Add(textBoxSample, 0, 6);
            tableLayout.Controls.Add(labelNote, 0, 7);
            tableLayout.Controls.Add(buttonApply, 0, 9);
            tableLayout.Controls.Add(buttonClose, 1, 9);
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
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
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
            labelTitle.Text = "Word wrap";
            //
            // labelWidth
            //
            labelWidth.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelWidth.AutoSize = true;
            labelWidth.Name = "labelWidth";
            labelWidth.TabIndex = 1;
            labelWidth.Text = "Page &width:";
            //
            // numericWidth
            //
            numericWidth.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numericWidth.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numericWidth.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numericWidth.Name = "numericWidth";
            numericWidth.Size = new System.Drawing.Size(70, 25);
            numericWidth.TabIndex = 2;
            toolTip.SetToolTip(numericWidth, "The longest line, in characters");
            numericWidth.Value = new decimal(new int[] { 50, 0, 0, 0 });
            //
            // checkBoxHanging
            //
            checkBoxHanging.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxHanging, 2);
            checkBoxHanging.Name = "checkBoxHanging";
            checkBoxHanging.TabIndex = 3;
            checkBoxHanging.Text = "&Hanging indent";
            toolTip.SetToolTip(checkBoxHanging, "Lines after the first of each paragraph go further in");
            checkBoxHanging.UseVisualStyleBackColor = true;
            //
            // radioTab
            //
            radioTab.AutoSize = true;
            radioTab.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
            radioTab.Name = "radioTab";
            radioTab.TabIndex = 4;
            radioTab.TabStop = true;
            radioTab.Text = "&Tab";
            radioTab.UseVisualStyleBackColor = true;
            //
            // radioSpaces
            //
            radioSpaces.AutoSize = true;
            radioSpaces.Name = "radioSpaces";
            radioSpaces.TabIndex = 5;
            radioSpaces.Text = "Eight &spaces";
            radioSpaces.UseVisualStyleBackColor = true;
            //
            // checkBoxJustify
            //
            checkBoxJustify.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxJustify, 2);
            checkBoxJustify.Name = "checkBoxJustify";
            checkBoxJustify.TabIndex = 6;
            checkBoxJustify.Text = "&Justify (no ragged right margin)";
            checkBoxJustify.UseVisualStyleBackColor = true;
            //
            // labelSampleNote
            //
            labelSampleNote.AutoSize = true;
            tableLayout.SetColumnSpan(labelSampleNote, 2);
            labelSampleNote.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            labelSampleNote.Name = "labelSampleNote";
            labelSampleNote.TabIndex = 7;
            labelSampleNote.Text = "Sample:";
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
            labelNote.Text = "Note";
            //
            // buttonApply
            //
            buttonApply.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonApply.Name = "buttonApply";
            buttonApply.Size = new System.Drawing.Size(113, 30);
            buttonApply.TabIndex = 10;
            buttonApply.Text = "&Apply";
            buttonApply.UseVisualStyleBackColor = true;
            //
            // buttonClose
            //
            buttonClose.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(114, 30);
            buttonClose.TabIndex = 11;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            //
            // WordWrapPanel
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tableLayout);
            Name = "WordWrapPanel";
            Size = new System.Drawing.Size(243, 468);
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericWidth).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.NumericUpDown numericWidth;
        private System.Windows.Forms.CheckBox checkBoxHanging;
        private System.Windows.Forms.RadioButton radioTab;
        private System.Windows.Forms.RadioButton radioSpaces;
        private System.Windows.Forms.CheckBox checkBoxJustify;
        private System.Windows.Forms.Label labelSampleNote;
        private System.Windows.Forms.TextBox textBoxSample;
        private System.Windows.Forms.Label labelNote;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
