namespace ClipX2.UI.Panels
{
    partial class LineNumbersPanel
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
            labelStart = new System.Windows.Forms.Label();
            numericStart = new System.Windows.Forms.NumericUpDown();
            labelStep = new System.Windows.Forms.Label();
            numericStep = new System.Windows.Forms.NumericUpDown();
            labelEvery = new System.Windows.Forms.Label();
            numericEvery = new System.Windows.Forms.NumericUpDown();
            checkBoxIgnoreBlanks = new System.Windows.Forms.CheckBox();
            checkBoxPad = new System.Windows.Forms.CheckBox();
            radioRight = new System.Windows.Forms.RadioButton();
            radioLeft = new System.Windows.Forms.RadioButton();
            checkBoxRenumber = new System.Windows.Forms.CheckBox();
            labelSampleNote = new System.Windows.Forms.Label();
            textBoxSample = new System.Windows.Forms.TextBox();
            labelNote = new System.Windows.Forms.Label();
            buttonApply = new System.Windows.Forms.Button();
            toolTip = new System.Windows.Forms.ToolTip(components);
            tableLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericStart).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericStep).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericEvery).BeginInit();
            SuspendLayout();
            //
            // tableLayout
            //
            tableLayout.ColumnCount = 2;
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.Controls.Add(labelTitle, 0, 0);
            tableLayout.Controls.Add(labelStart, 0, 1);
            tableLayout.Controls.Add(numericStart, 1, 1);
            tableLayout.Controls.Add(labelStep, 0, 2);
            tableLayout.Controls.Add(numericStep, 1, 2);
            tableLayout.Controls.Add(labelEvery, 0, 3);
            tableLayout.Controls.Add(numericEvery, 1, 3);
            tableLayout.Controls.Add(checkBoxIgnoreBlanks, 0, 4);
            tableLayout.Controls.Add(checkBoxPad, 0, 5);
            tableLayout.Controls.Add(radioRight, 0, 6);
            tableLayout.Controls.Add(radioLeft, 1, 6);
            tableLayout.Controls.Add(checkBoxRenumber, 0, 7);
            tableLayout.Controls.Add(labelSampleNote, 0, 8);
            tableLayout.Controls.Add(textBoxSample, 0, 9);
            tableLayout.Controls.Add(labelNote, 0, 10);
            tableLayout.Controls.Add(buttonApply, 1, 12);
            tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayout.Location = new System.Drawing.Point(0, 0);
            tableLayout.Name = "tableLayout";
            tableLayout.Padding = new System.Windows.Forms.Padding(2);
            tableLayout.RowCount = 13;
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 205F));
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.Size = new System.Drawing.Size(243, 520);
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
            labelTitle.Text = "Line numbers";
            //
            // labelStart
            //
            labelStart.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelStart.AutoSize = true;
            labelStart.Name = "labelStart";
            labelStart.TabIndex = 1;
            labelStart.Text = "&Start at:";
            //
            // numericStart
            //
            numericStart.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numericStart.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            numericStart.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numericStart.Name = "numericStart";
            numericStart.Size = new System.Drawing.Size(80, 25);
            numericStart.TabIndex = 2;
            toolTip.SetToolTip(numericStart, "The first line's number");
            numericStart.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // labelStep
            //
            labelStep.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelStep.AutoSize = true;
            labelStep.Name = "labelStep";
            labelStep.TabIndex = 3;
            labelStep.Text = "&Increment:";
            //
            // numericStep
            //
            numericStep.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numericStep.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numericStep.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericStep.Name = "numericStep";
            numericStep.Size = new System.Drawing.Size(80, 25);
            numericStep.TabIndex = 4;
            toolTip.SetToolTip(numericStep, "How much each number goes up by (10 gives 10, 20, 30…)");
            numericStep.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // labelEvery
            //
            labelEvery.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelEvery.AutoSize = true;
            labelEvery.Name = "labelEvery";
            labelEvery.TabIndex = 5;
            labelEvery.Text = "Show &every:";
            //
            // numericEvery
            //
            numericEvery.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numericEvery.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numericEvery.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericEvery.Name = "numericEvery";
            numericEvery.Size = new System.Drawing.Size(80, 25);
            numericEvery.TabIndex = 6;
            toolTip.SetToolTip(numericEvery, "1: a number on every line; 5: on lines 5, 10, 15… (all lines are still counted)");
            numericEvery.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // checkBoxIgnoreBlanks
            //
            checkBoxIgnoreBlanks.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxIgnoreBlanks, 2);
            checkBoxIgnoreBlanks.Name = "checkBoxIgnoreBlanks";
            checkBoxIgnoreBlanks.TabIndex = 7;
            checkBoxIgnoreBlanks.Text = "Skip &blank lines";
            toolTip.SetToolTip(checkBoxIgnoreBlanks, "Blank lines get no number and aren't counted");
            checkBoxIgnoreBlanks.UseVisualStyleBackColor = true;
            //
            // checkBoxPad
            //
            checkBoxPad.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxPad, 2);
            checkBoxPad.Name = "checkBoxPad";
            checkBoxPad.TabIndex = 8;
            checkBoxPad.Text = "&Pad numbers to the same width";
            toolTip.SetToolTip(checkBoxPad, "Spaces make the numbers all as wide as the longest, so the text lines up");
            checkBoxPad.UseVisualStyleBackColor = true;
            //
            // radioRight
            //
            radioRight.AutoSize = true;
            radioRight.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
            radioRight.Name = "radioRight";
            radioRight.TabIndex = 9;
            radioRight.TabStop = true;
            radioRight.Text = "Align &right";
            radioRight.UseVisualStyleBackColor = true;
            //
            // radioLeft
            //
            radioLeft.AutoSize = true;
            radioLeft.Name = "radioLeft";
            radioLeft.TabIndex = 10;
            radioLeft.Text = "Align &left";
            radioLeft.UseVisualStyleBackColor = true;
            //
            // checkBoxRenumber
            //
            checkBoxRenumber.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxRenumber, 2);
            checkBoxRenumber.Name = "checkBoxRenumber";
            checkBoxRenumber.TabIndex = 11;
            checkBoxRenumber.Text = "Re&number numbered lines";
            toolTip.SetToolTip(checkBoxRenumber, "Lines that already have line numbers lose them first, rather than getting a second number");
            checkBoxRenumber.UseVisualStyleBackColor = true;
            //
            // labelSampleNote
            //
            labelSampleNote.AutoSize = true;
            tableLayout.SetColumnSpan(labelSampleNote, 2);
            labelSampleNote.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            labelSampleNote.Name = "labelSampleNote";
            labelSampleNote.TabIndex = 12;
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
            textBoxSample.TabIndex = 13;
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
            labelNote.TabIndex = 14;
            labelNote.Text = "Note";
            //
            // buttonApply
            //
            buttonApply.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonApply.Name = "buttonApply";
            buttonApply.Size = new System.Drawing.Size(113, 30);
            buttonApply.TabIndex = 15;
            buttonApply.Text = "&Apply";
            buttonApply.UseVisualStyleBackColor = true;
            //
            // LineNumbersPanel
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tableLayout);
            Name = "LineNumbersPanel";
            Size = new System.Drawing.Size(243, 520);
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericStart).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericStep).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericEvery).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelStart;
        private System.Windows.Forms.NumericUpDown numericStart;
        private System.Windows.Forms.Label labelStep;
        private System.Windows.Forms.NumericUpDown numericStep;
        private System.Windows.Forms.Label labelEvery;
        private System.Windows.Forms.NumericUpDown numericEvery;
        private System.Windows.Forms.CheckBox checkBoxIgnoreBlanks;
        private System.Windows.Forms.CheckBox checkBoxPad;
        private System.Windows.Forms.RadioButton radioRight;
        private System.Windows.Forms.RadioButton radioLeft;
        private System.Windows.Forms.CheckBox checkBoxRenumber;
        private System.Windows.Forms.Label labelSampleNote;
        private System.Windows.Forms.TextBox textBoxSample;
        private System.Windows.Forms.Label labelNote;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
