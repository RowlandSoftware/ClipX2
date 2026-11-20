namespace ClipX2.UI.Panels
{
    partial class StatisticsPanel
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
            listViewStats = new System.Windows.Forms.ListView();
            columnName = new System.Windows.Forms.ColumnHeader();
            columnValue = new System.Windows.Forms.ColumnHeader();
            tableLayout.SuspendLayout();
            SuspendLayout();
            //
            // tableLayout
            //
            tableLayout.ColumnCount = 1;
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayout.Controls.Add(labelTitle, 0, 0);
            tableLayout.Controls.Add(listViewStats, 0, 1);
            tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayout.Location = new System.Drawing.Point(0, 0);
            tableLayout.Name = "tableLayout";
            tableLayout.Padding = new System.Windows.Forms.Padding(2);
            tableLayout.RowCount = 2;
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayout.Size = new System.Drawing.Size(243, 520);
            tableLayout.TabIndex = 0;
            //
            // labelTitle
            //
            labelTitle.AutoSize = true;
            labelTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            labelTitle.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            labelTitle.Name = "labelTitle";
            labelTitle.TabIndex = 0;
            labelTitle.Text = "Statistics";
            //
            // listViewStats
            //
            listViewStats.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnName, columnValue });
            listViewStats.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewStats.FullRowSelect = true;
            listViewStats.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            listViewStats.MultiSelect = false;
            listViewStats.Name = "listViewStats";
            listViewStats.ShowItemToolTips = true;
            listViewStats.TabIndex = 1;
            listViewStats.UseCompatibleStateImageBehavior = false;
            listViewStats.View = System.Windows.Forms.View.Details;
            //
            // columnName
            //
            columnName.Text = "";
            columnName.Width = 130;
            //
            // columnValue
            //
            columnValue.Text = "";
            columnValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnValue.Width = 85;
            //
            // StatisticsPanel
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tableLayout);
            Name = "StatisticsPanel";
            Size = new System.Drawing.Size(243, 520);
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.ListView listViewStats;
        private System.Windows.Forms.ColumnHeader columnName;
        private System.Windows.Forms.ColumnHeader columnValue;
    }
}
