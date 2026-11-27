namespace ClipX2.UI.Panels
{
    partial class NumberSortPanel
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
            radioValue = new System.Windows.Forms.RadioButton();
            radioVersion = new System.Windows.Forms.RadioButton();
            radioDigits = new System.Windows.Forms.RadioButton();
            labelSampleTitle = new System.Windows.Forms.Label();
            labelSample = new System.Windows.Forms.Label();
            labelNote = new System.Windows.Forms.Label();
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
            tableLayout.Controls.Add(radioValue, 0, 1);
            tableLayout.Controls.Add(radioVersion, 0, 2);
            tableLayout.Controls.Add(radioDigits, 0, 3);
            tableLayout.Controls.Add(labelSampleTitle, 0, 4);
            tableLayout.Controls.Add(labelSample, 0, 5);
            tableLayout.Controls.Add(labelNote, 0, 6);
            tableLayout.Controls.Add(buttonApply, 1, 8);
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
            labelTitle.Text = "Numeric sort";
            //
            // radioValue
            //
            radioValue.AutoSize = true;
            tableLayout.SetColumnSpan(radioValue, 2);
            radioValue.Name = "radioValue";
            radioValue.TabIndex = 1;
            radioValue.TabStop = true;
            radioValue.Text = "By &value";
            toolTip.SetToolTip(radioValue, "8 before 10; decimals too: 1.10 before 1.5, and Dewey numbers (004.16 before 004.6). Signs and thousands commas are read");
            radioValue.UseVisualStyleBackColor = true;
            //
            // radioVersion
            //
            radioVersion.AutoSize = true;
            tableLayout.SetColumnSpan(radioVersion, 2);
            radioVersion.Name = "radioVersion";
            radioVersion.TabIndex = 2;
            radioVersion.Text = "As v&ersion numbers";
            toolTip.SetToolTip(radioVersion, "Each part between the points by value: 1.5 before 1.10, and 1.2 before 1.2.1");
            radioVersion.UseVisualStyleBackColor = true;
            //
            // radioDigits
            //
            radioDigits.AutoSize = true;
            tableLayout.SetColumnSpan(radioDigits, 2);
            radioDigits.Name = "radioDigits";
            radioDigits.TabIndex = 3;
            radioDigits.Text = "&Digit by digit";
            toolTip.SetToolTip(radioDigits, "As text, one digit at a time (lexical order): 10 before 8. For codes whose digits aren't amounts");
            radioDigits.UseVisualStyleBackColor = true;
            //
            // labelSampleTitle
            //
            labelSampleTitle.AutoSize = true;
            tableLayout.SetColumnSpan(labelSampleTitle, 2);
            labelSampleTitle.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            labelSampleTitle.Name = "labelSampleTitle";
            labelSampleTitle.TabIndex = 4;
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
            labelSample.TabIndex = 5;
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
            labelNote.TabIndex = 6;
            labelNote.Text = "By the number each line starts with; lines without one go after, alphabetically, as do equal numbers. Sorts the selected lines, or the whole text; blank lines go.";
            //
            // buttonApply
            //
            buttonApply.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonApply.Name = "buttonApply";
            buttonApply.Size = new System.Drawing.Size(113, 30);
            buttonApply.TabIndex = 7;
            buttonApply.Text = "&Sort";
            buttonApply.UseVisualStyleBackColor = true;
            //
            // NumberSortPanel
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tableLayout);
            Name = "NumberSortPanel";
            Size = new System.Drawing.Size(243, 468);
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.RadioButton radioValue;
        private System.Windows.Forms.RadioButton radioVersion;
        private System.Windows.Forms.RadioButton radioDigits;
        private System.Windows.Forms.Label labelSampleTitle;
        private System.Windows.Forms.Label labelSample;
        private System.Windows.Forms.Label labelNote;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
