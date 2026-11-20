namespace ClipX2.UI.Panels
{
    partial class RemoveLinesPanel
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
            tableLayout = new System.Windows.Forms.TableLayoutPanel();
            labelTitle = new System.Windows.Forms.Label();
            checkBoxMultiple = new System.Windows.Forms.CheckBox();
            checkBoxSingle = new System.Windows.Forms.CheckBox();
            labelEffect = new System.Windows.Forms.Label();
            labelNote = new System.Windows.Forms.Label();
            buttonApply = new System.Windows.Forms.Button();
            tableLayout.SuspendLayout();
            SuspendLayout();
            //
            // tableLayout
            //
            tableLayout.ColumnCount = 2;
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.Controls.Add(labelTitle, 0, 0);
            tableLayout.Controls.Add(checkBoxMultiple, 0, 1);
            tableLayout.Controls.Add(checkBoxSingle, 0, 2);
            tableLayout.Controls.Add(labelEffect, 0, 3);
            tableLayout.Controls.Add(labelNote, 0, 4);
            tableLayout.Controls.Add(buttonApply, 1, 6);
            tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayout.Location = new System.Drawing.Point(0, 0);
            tableLayout.Name = "tableLayout";
            tableLayout.Padding = new System.Windows.Forms.Padding(2);
            tableLayout.RowCount = 7;
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
            labelTitle.Text = "Remove extra lines";
            //
            // checkBoxMultiple
            //
            checkBoxMultiple.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxMultiple, 2);
            checkBoxMultiple.Name = "checkBoxMultiple";
            checkBoxMultiple.TabIndex = 1;
            checkBoxMultiple.Text = "&Multiple blank lines to one";
            checkBoxMultiple.UseVisualStyleBackColor = true;
            //
            // checkBoxSingle
            //
            checkBoxSingle.AutoSize = true;
            tableLayout.SetColumnSpan(checkBoxSingle, 2);
            checkBoxSingle.Name = "checkBoxSingle";
            checkBoxSingle.TabIndex = 2;
            checkBoxSingle.Text = "Remove &single blank lines";
            checkBoxSingle.UseVisualStyleBackColor = true;
            //
            // labelEffect
            //
            labelEffect.AutoSize = true;
            tableLayout.SetColumnSpan(labelEffect, 2);
            labelEffect.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            labelEffect.MaximumSize = new System.Drawing.Size(233, 0);
            labelEffect.Name = "labelEffect";
            labelEffect.TabIndex = 3;
            labelEffect.Text = "Effect";
            //
            // labelNote
            //
            labelNote.AutoSize = true;
            tableLayout.SetColumnSpan(labelNote, 2);
            labelNote.ForeColor = System.Drawing.SystemColors.GrayText;
            labelNote.MaximumSize = new System.Drawing.Size(233, 0);
            labelNote.Name = "labelNote";
            labelNote.TabIndex = 4;
            labelNote.Text = "A blank line is empty or only white space. Blank lines that stay are emptied, and blank lines at the end of the text go.";
            //
            // buttonApply
            //
            buttonApply.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonApply.Name = "buttonApply";
            buttonApply.Size = new System.Drawing.Size(113, 30);
            buttonApply.TabIndex = 5;
            buttonApply.Text = "&Apply";
            buttonApply.UseVisualStyleBackColor = true;
            //
            // RemoveLinesPanel
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tableLayout);
            Name = "RemoveLinesPanel";
            Size = new System.Drawing.Size(243, 468);
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.CheckBox checkBoxMultiple;
        private System.Windows.Forms.CheckBox checkBoxSingle;
        private System.Windows.Forms.Label labelEffect;
        private System.Windows.Forms.Label labelNote;
        private System.Windows.Forms.Button buttonApply;
    }
}
