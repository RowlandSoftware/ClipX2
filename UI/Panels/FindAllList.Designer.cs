namespace ClipX2.UI.Panels
{
    partial class FindAllList
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
            labelCount = new System.Windows.Forms.Label();
            listMatches = new System.Windows.Forms.ListBox();
            labelNote = new System.Windows.Forms.Label();
            tableLayout.SuspendLayout();
            SuspendLayout();
            //
            // tableLayout
            //
            tableLayout.ColumnCount = 1;
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayout.Controls.Add(labelTitle, 0, 0);
            tableLayout.Controls.Add(labelCount, 0, 1);
            tableLayout.Controls.Add(listMatches, 0, 2);
            tableLayout.Controls.Add(labelNote, 0, 3);
            tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayout.Location = new System.Drawing.Point(0, 0);
            tableLayout.Name = "tableLayout";
            tableLayout.Padding = new System.Windows.Forms.Padding(2);
            tableLayout.RowCount = 4;
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.Size = new System.Drawing.Size(243, 560);
            tableLayout.TabIndex = 0;
            //
            // labelTitle
            //
            labelTitle.AutoSize = true;
            labelTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            labelTitle.Location = new System.Drawing.Point(5, 2);
            labelTitle.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new System.Drawing.Size(67, 19);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "Find All";
            //
            // labelCount
            //
            labelCount.AutoSize = true;
            labelCount.Location = new System.Drawing.Point(5, 27);
            labelCount.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            labelCount.MaximumSize = new System.Drawing.Size(233, 0);
            labelCount.Name = "labelCount";
            labelCount.Size = new System.Drawing.Size(0, 17);
            labelCount.TabIndex = 1;
            //
            // listMatches
            //
            listMatches.Dock = System.Windows.Forms.DockStyle.Fill;
            listMatches.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            listMatches.IntegralHeight = false;
            listMatches.ItemHeight = 24;
            listMatches.Location = new System.Drawing.Point(5, 53);
            listMatches.Name = "listMatches";
            listMatches.Size = new System.Drawing.Size(233, 440);
            listMatches.TabIndex = 2;
            //
            // labelNote
            //
            labelNote.AutoSize = true;
            labelNote.ForeColor = System.Drawing.SystemColors.GrayText;
            labelNote.Location = new System.Drawing.Point(5, 502);
            labelNote.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            labelNote.MaximumSize = new System.Drawing.Size(233, 0);
            labelNote.Name = "labelNote";
            labelNote.Size = new System.Drawing.Size(229, 17);
            labelNote.TabIndex = 3;
            labelNote.Text = "Click a match to see it in the text.";
            //
            // FindAllList
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tableLayout);
            Name = "FindAllList";
            Size = new System.Drawing.Size(243, 560);
            Controls.SetChildIndex(tableLayout, 0);
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.ListBox listMatches;
        private System.Windows.Forms.Label labelNote;
    }
}
