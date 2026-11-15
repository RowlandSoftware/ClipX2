namespace ClipX2
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabStyles = new System.Windows.Forms.TabPage();
            this.tabCase = new System.Windows.Forms.TabPage();
            this.toolStripCase = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonUpperCase = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonLowerCase = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonReverseCase = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonInitCaps = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonTitleCase = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSentenceCase = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonCapitaliseSentences = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tabEdit = new System.Windows.Forms.TabPage();
            this.tabFormat = new System.Windows.Forms.TabPage();
            this.tabLists = new System.Windows.Forms.TabPage();
            this.tabSorting = new System.Windows.Forms.TabPage();
            this.tabLangTools = new System.Windows.Forms.TabPage();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage0 = new System.Windows.Forms.TabPage();
            this.toolStripFavourites = new System.Windows.Forms.ToolStrip();
            this.panel2 = new System.Windows.Forms.Panel();
            this.toolStrip3 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabelRtfMode = new System.Windows.Forms.ToolStripLabel();
            this.tabControl3 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelWordCount = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.richTextBox1 = new ClipXRichTextBox();
            this.tabControl1.SuspendLayout();
            this.tabCase.SuspendLayout();
            this.toolStripCase.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage0.SuspendLayout();
            this.toolStrip3.SuspendLayout();
            this.tabControl3.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.Location = new System.Drawing.Point(4, 179);
            this.panel1.Margin = new System.Windows.Forms.Padding(6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(415, 633);
            this.panel1.TabIndex = 1;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabStyles);
            this.tabControl1.Controls.Add(this.tabCase);
            this.tabControl1.Controls.Add(this.tabEdit);
            this.tabControl1.Controls.Add(this.tabFormat);
            this.tabControl1.Controls.Add(this.tabLists);
            this.tabControl1.Controls.Add(this.tabSorting);
            this.tabControl1.Controls.Add(this.tabLangTools);
            this.tabControl1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.tabControl1.Location = new System.Drawing.Point(419, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(6);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1183, 175);
            this.tabControl1.TabIndex = 2;
            // 
            // tabStyles
            // 
            this.tabStyles.Location = new System.Drawing.Point(8, 50);
            this.tabStyles.Margin = new System.Windows.Forms.Padding(4);
            this.tabStyles.Name = "tabStyles";
            this.tabStyles.Size = new System.Drawing.Size(1167, 117);
            this.tabStyles.TabIndex = 6;
            this.tabStyles.Text = "Styles";
            this.tabStyles.UseVisualStyleBackColor = true;
            // 
            // tabCase
            // 
            this.tabCase.Controls.Add(this.toolStripCase);
            this.tabCase.Location = new System.Drawing.Point(8, 50);
            this.tabCase.Margin = new System.Windows.Forms.Padding(6);
            this.tabCase.Name = "tabCase";
            this.tabCase.Padding = new System.Windows.Forms.Padding(6);
            this.tabCase.Size = new System.Drawing.Size(1167, 117);
            this.tabCase.TabIndex = 0;
            this.tabCase.Text = "Case";
            this.tabCase.UseVisualStyleBackColor = true;
            // 
            // toolStripCase
            // 
            this.toolStripCase.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripCase.ImageScalingSize = new System.Drawing.Size(36, 36);
            this.toolStripCase.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonUpperCase,
            this.toolStripButtonLowerCase,
            this.toolStripButtonReverseCase,
            this.toolStripButtonInitCaps,
            this.toolStripButtonTitleCase,
            this.toolStripButtonSentenceCase,
            this.toolStripButtonCapitaliseSentences,
            this.toolStripSeparator,
            this.toolStripSeparator1});
            this.toolStripCase.Location = new System.Drawing.Point(6, 6);
            this.toolStripCase.Name = "toolStripCase";
            this.toolStripCase.Padding = new System.Windows.Forms.Padding(0);
            this.toolStripCase.Size = new System.Drawing.Size(1155, 58);
            this.toolStripCase.Stretch = true;
            this.toolStripCase.TabIndex = 0;
            this.toolStripCase.Text = "Case";
            // 
            // toolStripButtonUpperCase
            // 
            this.toolStripButtonUpperCase.AccessibleName = "Upper case";
            this.toolStripButtonUpperCase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonUpperCase.Image = global::ClipX2.Properties.Resources.Cap1;
            this.toolStripButtonUpperCase.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonUpperCase.ImageTransparentColor = System.Drawing.Color.Black;
            this.toolStripButtonUpperCase.Name = "toolStripButtonUpperCase";
            this.toolStripButtonUpperCase.Size = new System.Drawing.Size(52, 52);
            this.toolStripButtonUpperCase.Text = "Upper case";
            this.toolStripButtonUpperCase.ToolTipText = "Upper Case";
            this.toolStripButtonUpperCase.Click += new System.EventHandler(this.toolStripButtonUpperCase_Click);
            // 
            // toolStripButtonLowerCase
            // 
            this.toolStripButtonLowerCase.AccessibleName = "Lower case";
            this.toolStripButtonLowerCase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonLowerCase.Image = global::ClipX2.Properties.Resources.lower1;
            this.toolStripButtonLowerCase.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonLowerCase.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonLowerCase.Name = "toolStripButtonLowerCase";
            this.toolStripButtonLowerCase.Size = new System.Drawing.Size(52, 52);
            this.toolStripButtonLowerCase.Text = "Lower case";
            this.toolStripButtonLowerCase.Click += new System.EventHandler(this.toolStripButtonLowerCase_Click);
            // 
            // toolStripButtonReverseCase
            // 
            this.toolStripButtonReverseCase.AccessibleName = "Reverse case";
            this.toolStripButtonReverseCase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonReverseCase.Image = global::ClipX2.Properties.Resources.rev2;
            this.toolStripButtonReverseCase.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonReverseCase.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonReverseCase.Name = "toolStripButtonReverseCase";
            this.toolStripButtonReverseCase.Size = new System.Drawing.Size(52, 52);
            this.toolStripButtonReverseCase.Text = "toolStripButton2";
            this.toolStripButtonReverseCase.ToolTipText = "Reverse case";
            this.toolStripButtonReverseCase.Click += new System.EventHandler(this.toolStripButtonReverseCase_Click);
            // 
            // toolStripButtonInitCaps
            // 
            this.toolStripButtonInitCaps.AccessibleName = "Initial Caps";
            this.toolStripButtonInitCaps.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonInitCaps.Image = global::ClipX2.Properties.Resources.InitCaps1;
            this.toolStripButtonInitCaps.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonInitCaps.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonInitCaps.Name = "toolStripButtonInitCaps";
            this.toolStripButtonInitCaps.Size = new System.Drawing.Size(52, 52);
            this.toolStripButtonInitCaps.Text = "toolStripButton1";
            this.toolStripButtonInitCaps.ToolTipText = "Initial Caps";
            this.toolStripButtonInitCaps.Click += new System.EventHandler(this.toolStripButtonInitCaps_Click);
            // 
            // toolStripButtonTitleCase
            // 
            this.toolStripButtonTitleCase.AccessibleName = "Title case";
            this.toolStripButtonTitleCase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonTitleCase.Image = global::ClipX2.Properties.Resources.Title1;
            this.toolStripButtonTitleCase.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonTitleCase.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonTitleCase.Name = "toolStripButtonTitleCase";
            this.toolStripButtonTitleCase.Size = new System.Drawing.Size(52, 52);
            this.toolStripButtonTitleCase.Text = "toolStripButton1";
            this.toolStripButtonTitleCase.ToolTipText = "Title case";
            this.toolStripButtonTitleCase.Click += new System.EventHandler(this.toolStripButtonTitleCase_Click);
            // 
            // toolStripButtonSentenceCase
            // 
            this.toolStripButtonSentenceCase.AccessibleName = "Sentence case";
            this.toolStripButtonSentenceCase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSentenceCase.Image = global::ClipX2.Properties.Resources.Sentence1;
            this.toolStripButtonSentenceCase.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonSentenceCase.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSentenceCase.Name = "toolStripButtonSentenceCase";
            this.toolStripButtonSentenceCase.Size = new System.Drawing.Size(52, 52);
            this.toolStripButtonSentenceCase.Text = "toolStripButton1";
            this.toolStripButtonSentenceCase.ToolTipText = "Sentence case";
            this.toolStripButtonSentenceCase.Click += new System.EventHandler(this.toolStripButtonSentenceCase_Click);
            // 
            // toolStripButtonCapitaliseSentences
            // 
            this.toolStripButtonCapitaliseSentences.AccessibleName = "Capitalise sentences";
            this.toolStripButtonCapitaliseSentences.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonCapitaliseSentences.Image = global::ClipX2.Properties.Resources.Sentence2;
            this.toolStripButtonCapitaliseSentences.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonCapitaliseSentences.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonCapitaliseSentences.Name = "toolStripButtonCapitaliseSentences";
            this.toolStripButtonCapitaliseSentences.Size = new System.Drawing.Size(52, 52);
            this.toolStripButtonCapitaliseSentences.Text = "toolStripButton1";
            this.toolStripButtonCapitaliseSentences.ToolTipText = "Capitalise sentences";
            this.toolStripButtonCapitaliseSentences.Click += new System.EventHandler(this.toolStripButtonCapitaliseSentences_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(6, 58);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 58);
            // 
            // tabEdit
            // 
            this.tabEdit.Location = new System.Drawing.Point(8, 50);
            this.tabEdit.Margin = new System.Windows.Forms.Padding(6);
            this.tabEdit.Name = "tabEdit";
            this.tabEdit.Padding = new System.Windows.Forms.Padding(6);
            this.tabEdit.Size = new System.Drawing.Size(1167, 117);
            this.tabEdit.TabIndex = 1;
            this.tabEdit.Text = "Edit";
            this.tabEdit.UseVisualStyleBackColor = true;
            // 
            // tabFormat
            // 
            this.tabFormat.Location = new System.Drawing.Point(8, 50);
            this.tabFormat.Margin = new System.Windows.Forms.Padding(6);
            this.tabFormat.Name = "tabFormat";
            this.tabFormat.Size = new System.Drawing.Size(1167, 117);
            this.tabFormat.TabIndex = 2;
            this.tabFormat.Text = "Format";
            this.tabFormat.UseVisualStyleBackColor = true;
            // 
            // tabLists
            // 
            this.tabLists.Location = new System.Drawing.Point(8, 50);
            this.tabLists.Margin = new System.Windows.Forms.Padding(6);
            this.tabLists.Name = "tabLists";
            this.tabLists.Size = new System.Drawing.Size(1167, 117);
            this.tabLists.TabIndex = 3;
            this.tabLists.Text = "Lists";
            this.tabLists.UseVisualStyleBackColor = true;
            // 
            // tabSorting
            // 
            this.tabSorting.Location = new System.Drawing.Point(8, 50);
            this.tabSorting.Margin = new System.Windows.Forms.Padding(6);
            this.tabSorting.Name = "tabSorting";
            this.tabSorting.Size = new System.Drawing.Size(1167, 117);
            this.tabSorting.TabIndex = 4;
            this.tabSorting.Text = "Sorting";
            this.tabSorting.UseVisualStyleBackColor = true;
            // 
            // tabLangTools
            // 
            this.tabLangTools.Location = new System.Drawing.Point(8, 50);
            this.tabLangTools.Margin = new System.Windows.Forms.Padding(6);
            this.tabLangTools.Name = "tabLangTools";
            this.tabLangTools.Size = new System.Drawing.Size(1167, 117);
            this.tabLangTools.TabIndex = 5;
            this.tabLangTools.Text = "Language Tools";
            this.tabLangTools.UseVisualStyleBackColor = true;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage0);
            this.tabControl2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl2.Location = new System.Drawing.Point(4, 0);
            this.tabControl2.Margin = new System.Windows.Forms.Padding(6);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(423, 167);
            this.tabControl2.TabIndex = 3;
            // 
            // tabPage0
            // 
            this.tabPage0.Controls.Add(this.toolStripFavourites);
            this.tabPage0.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabPage0.Location = new System.Drawing.Point(8, 50);
            this.tabPage0.Margin = new System.Windows.Forms.Padding(6);
            this.tabPage0.Name = "tabPage0";
            this.tabPage0.Padding = new System.Windows.Forms.Padding(6);
            this.tabPage0.Size = new System.Drawing.Size(407, 109);
            this.tabPage0.TabIndex = 0;
            this.tabPage0.Text = "Favourites";
            this.tabPage0.UseVisualStyleBackColor = true;
            // 
            // toolStripFavourites
            // 
            this.toolStripFavourites.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripFavourites.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStripFavourites.Location = new System.Drawing.Point(6, 6);
            this.toolStripFavourites.Name = "toolStripFavourites";
            this.toolStripFavourites.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStripFavourites.Size = new System.Drawing.Size(395, 50);
            this.toolStripFavourites.TabIndex = 0;
            this.toolStripFavourites.Text = "toolStrip2";
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Location = new System.Drawing.Point(1739, 179);
            this.panel2.Margin = new System.Windows.Forms.Padding(6);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(141, 632);
            this.panel2.TabIndex = 4;
            // 
            // toolStrip3
            // 
            this.toolStrip3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip3.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelRtfMode,
            this.toolStripSeparator2,
            this.toolStripLabelWordCount,
            this.toolStripSeparator3});
            this.toolStrip3.Location = new System.Drawing.Point(0, 815);
            this.toolStrip3.Name = "toolStrip3";
            this.toolStrip3.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStrip3.Size = new System.Drawing.Size(1880, 50);
            this.toolStrip3.TabIndex = 5;
            this.toolStrip3.Text = "toolStrip3";
            // 
            // toolStripLabelRtfMode
            // 
            this.toolStripLabelRtfMode.Name = "toolStripLabelRtfMode";
            this.toolStripLabelRtfMode.Size = new System.Drawing.Size(173, 44);
            this.toolStripLabelRtfMode.Text = "toolStripLabel1";
            this.toolStripLabelRtfMode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolStripLabelRtfMode.Click += new System.EventHandler(this.toolStripLabelRtfMode_Click);
            // 
            // tabControl3
            // 
            this.tabControl3.Controls.Add(this.tabPage1);
            this.tabControl3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl3.Location = new System.Drawing.Point(1606, 0);
            this.tabControl3.Margin = new System.Windows.Forms.Padding(6);
            this.tabControl3.Name = "tabControl3";
            this.tabControl3.SelectedIndex = 0;
            this.tabControl3.Size = new System.Drawing.Size(274, 167);
            this.tabControl3.TabIndex = 6;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.toolStrip1);
            this.tabPage1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabPage1.Location = new System.Drawing.Point(8, 50);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(6);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(6);
            this.tabPage1.Size = new System.Drawing.Size(258, 109);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Favourites";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Location = new System.Drawing.Point(6, 6);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStrip1.Size = new System.Drawing.Size(246, 50);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip2";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 50);
            // 
            // toolStripLabelWordCount
            // 
            this.toolStripLabelWordCount.Name = "toolStripLabelWordCount";
            this.toolStripLabelWordCount.Size = new System.Drawing.Size(139, 44);
            this.toolStripLabelWordCount.Text = "Word count";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 50);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.AutoWordSelectionEnabled = false;
            this.richTextBox1.EnableAutoDragDrop = true;
            this.richTextBox1.HideSelection = false;
            this.richTextBox1.IsRtfModeGetter = null;
            this.richTextBox1.Location = new System.Drawing.Point(419, 179);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(6);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.PreciseSelection = true;
            this.richTextBox1.Size = new System.Drawing.Size(1317, 633);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1880, 865);
            this.Controls.Add(this.tabControl3);
            this.Controls.Add(this.toolStrip3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.tabControl2);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.richTextBox1);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "Form1";
            this.Text = "Clipboard Text Toolbox 2";
            this.Load += new System.EventHandler(this.On_FormLoad);
            this.tabControl1.ResumeLayout(false);
            this.tabCase.ResumeLayout(false);
            this.tabCase.PerformLayout();
            this.toolStripCase.ResumeLayout(false);
            this.toolStripCase.PerformLayout();
            this.tabControl2.ResumeLayout(false);
            this.tabPage0.ResumeLayout(false);
            this.tabPage0.PerformLayout();
            this.toolStrip3.ResumeLayout(false);
            this.toolStrip3.PerformLayout();
            this.tabControl3.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        //System.Windows.Forms.RichTextBox richTextBox1;
        private ClipXRichTextBox richTextBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabCase;
        private System.Windows.Forms.ToolStrip toolStripCase;
        private System.Windows.Forms.TabPage tabEdit;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage0;
        private System.Windows.Forms.ToolStrip toolStripFavourites;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ToolStrip toolStrip3;
        private System.Windows.Forms.TabPage tabFormat;
        private System.Windows.Forms.ToolStripButton toolStripButtonUpperCase;
        private System.Windows.Forms.TabPage tabLists;
        private System.Windows.Forms.TabPage tabSorting;
        private System.Windows.Forms.TabPage tabLangTools;
        private System.Windows.Forms.ToolStripButton toolStripButtonLowerCase;
        private System.Windows.Forms.ToolStripLabel toolStripLabelRtfMode;
        private System.Windows.Forms.TabPage tabStyles;
        private System.Windows.Forms.ToolStripButton toolStripButtonReverseCase;
        private System.Windows.Forms.ToolStripButton toolStripButtonInitCaps;
        private System.Windows.Forms.ToolStripButton toolStripButtonTitleCase;
        private System.Windows.Forms.ToolStripButton toolStripButtonSentenceCase;
        private System.Windows.Forms.ToolStripButton toolStripButtonCapitaliseSentences;
        private System.Windows.Forms.TabControl tabControl3;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabelWordCount;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    }
}

