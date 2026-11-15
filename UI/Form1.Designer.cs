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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            panel1 = new System.Windows.Forms.Panel();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabStyles = new System.Windows.Forms.TabPage();
            tabCase = new System.Windows.Forms.TabPage();
            toolStripCase = new System.Windows.Forms.ToolStrip();
            toolStripButtonUpperCase = new System.Windows.Forms.ToolStripButton();
            toolStripButtonLowerCase = new System.Windows.Forms.ToolStripButton();
            toolStripButtonReverseCase = new System.Windows.Forms.ToolStripButton();
            toolStripButtonInitCaps = new System.Windows.Forms.ToolStripButton();
            toolStripButtonTitleCase = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSentenceCase = new System.Windows.Forms.ToolStripButton();
            toolStripButtonCapitaliseSentences = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            tabEdit = new System.Windows.Forms.TabPage();
            tabFormat = new System.Windows.Forms.TabPage();
            tabLists = new System.Windows.Forms.TabPage();
            tabSorting = new System.Windows.Forms.TabPage();
            tabLangTools = new System.Windows.Forms.TabPage();
            toolStrip2 = new System.Windows.Forms.ToolStrip();
            toolStripButtonSpellcheck = new System.Windows.Forms.ToolStripButton();
            toolStripButtonCheckPunctuation = new System.Windows.Forms.ToolStripButton();
            toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            toolStripButton6 = new System.Windows.Forms.ToolStripButton();
            toolStripButton7 = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            tabControl2 = new System.Windows.Forms.TabControl();
            tabPage0 = new System.Windows.Forms.TabPage();
            toolStripFavourites = new System.Windows.Forms.ToolStrip();
            panel2 = new System.Windows.Forms.Panel();
            toolStrip3 = new System.Windows.Forms.ToolStrip();
            toolStripLabelRtfMode = new System.Windows.Forms.ToolStripLabel();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabelWordCount = new System.Windows.Forms.ToolStripLabel();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonDone = new System.Windows.Forms.ToolStripButton();
            tabControl3 = new System.Windows.Forms.TabControl();
            tabPage1 = new System.Windows.Forms.TabPage();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            richTextBox1 = new ClipXRichTextBox();
            tabControl1.SuspendLayout();
            tabCase.SuspendLayout();
            toolStripCase.SuspendLayout();
            tabLangTools.SuspendLayout();
            toolStrip2.SuspendLayout();
            tabControl2.SuspendLayout();
            tabPage0.SuspendLayout();
            toolStrip3.SuspendLayout();
            tabControl3.SuspendLayout();
            tabPage1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            panel1.Location = new System.Drawing.Point(2, 122);
            panel1.Margin = new System.Windows.Forms.Padding(4);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(243, 430);
            panel1.TabIndex = 1;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabStyles);
            tabControl1.Controls.Add(tabCase);
            tabControl1.Controls.Add(tabEdit);
            tabControl1.Controls.Add(tabFormat);
            tabControl1.Controls.Add(tabLists);
            tabControl1.Controls.Add(tabSorting);
            tabControl1.Controls.Add(tabLangTools);
            tabControl1.Font = new System.Drawing.Font("Segoe UI", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            tabControl1.Location = new System.Drawing.Point(245, 0);
            tabControl1.Margin = new System.Windows.Forms.Padding(4);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(691, 119);
            tabControl1.TabIndex = 2;
            // 
            // tabStyles
            // 
            tabStyles.Location = new System.Drawing.Point(4, 26);
            tabStyles.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            tabStyles.Name = "tabStyles";
            tabStyles.Size = new System.Drawing.Size(683, 89);
            tabStyles.TabIndex = 6;
            tabStyles.Text = "Styles";
            tabStyles.UseVisualStyleBackColor = true;
            // 
            // tabCase
            // 
            tabCase.Controls.Add(toolStripCase);
            tabCase.Location = new System.Drawing.Point(4, 26);
            tabCase.Margin = new System.Windows.Forms.Padding(4);
            tabCase.Name = "tabCase";
            tabCase.Padding = new System.Windows.Forms.Padding(4);
            tabCase.Size = new System.Drawing.Size(683, 89);
            tabCase.TabIndex = 0;
            tabCase.Text = "Case";
            tabCase.UseVisualStyleBackColor = true;
            // 
            // toolStripCase
            // 
            toolStripCase.AccessibleName = "Lower";
            toolStripCase.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripCase.ImageScalingSize = new System.Drawing.Size(36, 36);
            toolStripCase.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButtonUpperCase, toolStripButtonLowerCase, toolStripButtonReverseCase, toolStripButtonInitCaps, toolStripButtonTitleCase, toolStripButtonSentenceCase, toolStripButtonCapitaliseSentences, toolStripSeparator, toolStripSeparator1 });
            toolStripCase.Location = new System.Drawing.Point(4, 4);
            toolStripCase.Name = "toolStripCase";
            toolStripCase.Padding = new System.Windows.Forms.Padding(0);
            toolStripCase.Size = new System.Drawing.Size(675, 55);
            toolStripCase.Stretch = true;
            toolStripCase.TabIndex = 0;
            toolStripCase.Text = "Case";
            // 
            // toolStripButtonUpperCase
            // 
            toolStripButtonUpperCase.AccessibleName = "Upper case";
            toolStripButtonUpperCase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonUpperCase.Image = Properties.Resources.Cap1;
            toolStripButtonUpperCase.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            toolStripButtonUpperCase.ImageTransparentColor = System.Drawing.Color.Black;
            toolStripButtonUpperCase.Name = "toolStripButtonUpperCase";
            toolStripButtonUpperCase.Size = new System.Drawing.Size(52, 52);
            toolStripButtonUpperCase.Text = "Upper case";
            toolStripButtonUpperCase.ToolTipText = "Upper Case - all letters to capitals";
            toolStripButtonUpperCase.Click += toolStripButtonUpperCase_Click;
            // 
            // toolStripButtonLowerCase
            // 
            toolStripButtonLowerCase.AccessibleName = "Lower case";
            toolStripButtonLowerCase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonLowerCase.Image = Properties.Resources.lower1;
            toolStripButtonLowerCase.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            toolStripButtonLowerCase.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonLowerCase.Name = "toolStripButtonLowerCase";
            toolStripButtonLowerCase.Size = new System.Drawing.Size(52, 52);
            toolStripButtonLowerCase.Text = "Lower case";
            toolStripButtonLowerCase.ToolTipText = "To lower case";
            toolStripButtonLowerCase.Click += toolStripButtonLowerCase_Click;
            // 
            // toolStripButtonReverseCase
            // 
            toolStripButtonReverseCase.AccessibleName = "Reverse case";
            toolStripButtonReverseCase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonReverseCase.Image = Properties.Resources.rev2;
            toolStripButtonReverseCase.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            toolStripButtonReverseCase.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonReverseCase.Name = "toolStripButtonReverseCase";
            toolStripButtonReverseCase.Size = new System.Drawing.Size(52, 52);
            toolStripButtonReverseCase.Text = "Reverse case";
            toolStripButtonReverseCase.ToolTipText = "Swop the letter case";
            toolStripButtonReverseCase.Click += toolStripButtonReverseCase_Click;
            // 
            // toolStripButtonInitCaps
            // 
            toolStripButtonInitCaps.AccessibleName = "Initial Caps";
            toolStripButtonInitCaps.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonInitCaps.Image = Properties.Resources.InitCaps1;
            toolStripButtonInitCaps.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            toolStripButtonInitCaps.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonInitCaps.Name = "toolStripButtonInitCaps";
            toolStripButtonInitCaps.Size = new System.Drawing.Size(52, 52);
            toolStripButtonInitCaps.Text = "Initial Caps";
            toolStripButtonInitCaps.ToolTipText = "Initial letter of each word to capitals";
            toolStripButtonInitCaps.Click += toolStripButtonInitCaps_Click;
            // 
            // toolStripButtonTitleCase
            // 
            toolStripButtonTitleCase.AccessibleName = "Title case";
            toolStripButtonTitleCase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonTitleCase.Image = Properties.Resources.Title1;
            toolStripButtonTitleCase.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            toolStripButtonTitleCase.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonTitleCase.Name = "toolStripButtonTitleCase";
            toolStripButtonTitleCase.Size = new System.Drawing.Size(52, 52);
            toolStripButtonTitleCase.Text = "Title case";
            toolStripButtonTitleCase.ToolTipText = "Capitalise initial letter of words according to title rules";
            toolStripButtonTitleCase.Click += toolStripButtonTitleCase_Click;
            // 
            // toolStripButtonSentenceCase
            // 
            toolStripButtonSentenceCase.AccessibleName = "Sentence case";
            toolStripButtonSentenceCase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonSentenceCase.Image = Properties.Resources.Sentence1;
            toolStripButtonSentenceCase.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            toolStripButtonSentenceCase.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonSentenceCase.Name = "toolStripButtonSentenceCase";
            toolStripButtonSentenceCase.Size = new System.Drawing.Size(52, 52);
            toolStripButtonSentenceCase.Text = "Sentence case";
            toolStripButtonSentenceCase.ToolTipText = "Initial letter of each sentence to capitals, all others lower case";
            toolStripButtonSentenceCase.Click += toolStripButtonSentenceCase_Click;
            // 
            // toolStripButtonCapitaliseSentences
            // 
            toolStripButtonCapitaliseSentences.AccessibleName = "Capitalise sentences";
            toolStripButtonCapitaliseSentences.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonCapitaliseSentences.Image = Properties.Resources.Sentence2;
            toolStripButtonCapitaliseSentences.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            toolStripButtonCapitaliseSentences.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonCapitaliseSentences.Name = "toolStripButtonCapitaliseSentences";
            toolStripButtonCapitaliseSentences.Size = new System.Drawing.Size(52, 52);
            toolStripButtonCapitaliseSentences.Text = "Capitalise sentences";
            toolStripButtonCapitaliseSentences.ToolTipText = "Initial letter of each sentence to capitals, all others left alone";
            toolStripButtonCapitaliseSentences.Click += toolStripButtonCapitaliseSentences_Click;
            // 
            // toolStripSeparator
            // 
            toolStripSeparator.Name = "toolStripSeparator";
            toolStripSeparator.Size = new System.Drawing.Size(6, 55);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 55);
            // 
            // tabEdit
            // 
            tabEdit.Location = new System.Drawing.Point(4, 26);
            tabEdit.Margin = new System.Windows.Forms.Padding(4);
            tabEdit.Name = "tabEdit";
            tabEdit.Padding = new System.Windows.Forms.Padding(4);
            tabEdit.Size = new System.Drawing.Size(683, 89);
            tabEdit.TabIndex = 1;
            tabEdit.Text = "Edit";
            tabEdit.UseVisualStyleBackColor = true;
            // 
            // tabFormat
            // 
            tabFormat.Location = new System.Drawing.Point(4, 26);
            tabFormat.Margin = new System.Windows.Forms.Padding(4);
            tabFormat.Name = "tabFormat";
            tabFormat.Size = new System.Drawing.Size(683, 89);
            tabFormat.TabIndex = 2;
            tabFormat.Text = "Format";
            tabFormat.UseVisualStyleBackColor = true;
            // 
            // tabLists
            // 
            tabLists.Location = new System.Drawing.Point(4, 26);
            tabLists.Margin = new System.Windows.Forms.Padding(4);
            tabLists.Name = "tabLists";
            tabLists.Size = new System.Drawing.Size(683, 89);
            tabLists.TabIndex = 3;
            tabLists.Text = "Lists";
            tabLists.UseVisualStyleBackColor = true;
            // 
            // tabSorting
            // 
            tabSorting.Location = new System.Drawing.Point(4, 26);
            tabSorting.Margin = new System.Windows.Forms.Padding(4);
            tabSorting.Name = "tabSorting";
            tabSorting.Size = new System.Drawing.Size(683, 89);
            tabSorting.TabIndex = 4;
            tabSorting.Text = "Sorting";
            tabSorting.UseVisualStyleBackColor = true;
            // 
            // tabLangTools
            // 
            tabLangTools.Controls.Add(toolStrip2);
            tabLangTools.Location = new System.Drawing.Point(4, 26);
            tabLangTools.Margin = new System.Windows.Forms.Padding(4);
            tabLangTools.Name = "tabLangTools";
            tabLangTools.Size = new System.Drawing.Size(683, 89);
            tabLangTools.TabIndex = 5;
            tabLangTools.Text = "Language Tools";
            tabLangTools.UseVisualStyleBackColor = true;
            // 
            // toolStrip2
            // 
            toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip2.ImageScalingSize = new System.Drawing.Size(36, 36);
            toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButtonSpellcheck, toolStripButtonCheckPunctuation, toolStripButton3, toolStripButton4, toolStripButton5, toolStripButton6, toolStripButton7, toolStripSeparator4, toolStripSeparator5 });
            toolStrip2.Location = new System.Drawing.Point(0, 0);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Padding = new System.Windows.Forms.Padding(0);
            toolStrip2.Size = new System.Drawing.Size(683, 55);
            toolStrip2.Stretch = true;
            toolStrip2.TabIndex = 1;
            toolStrip2.Text = "Case";
            // 
            // toolStripButtonSpellcheck
            // 
            toolStripButtonSpellcheck.AccessibleName = "Check Spelling";
            toolStripButtonSpellcheck.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonSpellcheck.Image = Properties.Resources.CheckSpelling;
            toolStripButtonSpellcheck.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            toolStripButtonSpellcheck.ImageTransparentColor = System.Drawing.Color.Black;
            toolStripButtonSpellcheck.Name = "toolStripButtonSpellcheck";
            toolStripButtonSpellcheck.Size = new System.Drawing.Size(52, 52);
            toolStripButtonSpellcheck.Text = "Check Spelling";
            toolStripButtonSpellcheck.ToolTipText = "Check for spelling errors and repeated words";
            // 
            // toolStripButtonCheckPunctuation
            // 
            toolStripButtonCheckPunctuation.AccessibleName = "Check Punctuation";
            toolStripButtonCheckPunctuation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonCheckPunctuation.Image = Properties.Resources.CheckPunc;
            toolStripButtonCheckPunctuation.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            toolStripButtonCheckPunctuation.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonCheckPunctuation.Name = "toolStripButtonCheckPunctuation";
            toolStripButtonCheckPunctuation.Size = new System.Drawing.Size(52, 52);
            toolStripButtonCheckPunctuation.Text = "Check Punctuation";
            toolStripButtonCheckPunctuation.ToolTipText = "Check for spacing errors around punctuation and brackets, and umatched brackets and speech marks";
            // 
            // toolStripButton3
            // 
            toolStripButton3.AccessibleName = "Reverse case";
            toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton3.Image = Properties.Resources.rev2;
            toolStripButton3.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new System.Drawing.Size(52, 52);
            toolStripButton3.Text = "toolStripButton2";
            toolStripButton3.ToolTipText = "Reverse case";
            // 
            // toolStripButton4
            // 
            toolStripButton4.AccessibleName = "Initial Caps";
            toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton4.Image = Properties.Resources.InitCaps1;
            toolStripButton4.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Size = new System.Drawing.Size(52, 52);
            toolStripButton4.Text = "toolStripButton1";
            toolStripButton4.ToolTipText = "Initial Caps";
            // 
            // toolStripButton5
            // 
            toolStripButton5.AccessibleName = "Title case";
            toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton5.Image = Properties.Resources.Title1;
            toolStripButton5.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton5.Name = "toolStripButton5";
            toolStripButton5.Size = new System.Drawing.Size(52, 52);
            toolStripButton5.Text = "toolStripButton1";
            toolStripButton5.ToolTipText = "Title case";
            // 
            // toolStripButton6
            // 
            toolStripButton6.AccessibleName = "Sentence case";
            toolStripButton6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton6.Image = Properties.Resources.Sentence1;
            toolStripButton6.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            toolStripButton6.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton6.Name = "toolStripButton6";
            toolStripButton6.Size = new System.Drawing.Size(52, 52);
            toolStripButton6.Text = "toolStripButton1";
            toolStripButton6.ToolTipText = "Sentence case";
            // 
            // toolStripButton7
            // 
            toolStripButton7.AccessibleName = "Capitalise sentences";
            toolStripButton7.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton7.Image = Properties.Resources.Sentence2;
            toolStripButton7.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            toolStripButton7.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton7.Name = "toolStripButton7";
            toolStripButton7.Size = new System.Drawing.Size(52, 52);
            toolStripButton7.Text = "toolStripButton1";
            toolStripButton7.ToolTipText = "Capitalise sentences";
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 55);
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(6, 55);
            // 
            // tabControl2
            // 
            tabControl2.Controls.Add(tabPage0);
            tabControl2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            tabControl2.Location = new System.Drawing.Point(2, 0);
            tabControl2.Margin = new System.Windows.Forms.Padding(4);
            tabControl2.Name = "tabControl2";
            tabControl2.SelectedIndex = 0;
            tabControl2.Size = new System.Drawing.Size(247, 114);
            tabControl2.TabIndex = 3;
            // 
            // tabPage0
            // 
            tabPage0.Controls.Add(toolStripFavourites);
            tabPage0.Font = new System.Drawing.Font("Segoe UI", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            tabPage0.Location = new System.Drawing.Point(4, 26);
            tabPage0.Margin = new System.Windows.Forms.Padding(4);
            tabPage0.Name = "tabPage0";
            tabPage0.Padding = new System.Windows.Forms.Padding(4);
            tabPage0.Size = new System.Drawing.Size(239, 84);
            tabPage0.TabIndex = 0;
            tabPage0.Text = "Favourites";
            tabPage0.UseVisualStyleBackColor = true;
            // 
            // toolStripFavourites
            // 
            toolStripFavourites.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripFavourites.ImageScalingSize = new System.Drawing.Size(32, 32);
            toolStripFavourites.Location = new System.Drawing.Point(4, 4);
            toolStripFavourites.Name = "toolStripFavourites";
            toolStripFavourites.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            toolStripFavourites.Size = new System.Drawing.Size(231, 25);
            toolStripFavourites.TabIndex = 0;
            toolStripFavourites.Text = "toolStrip2";
            // 
            // panel2
            // 
            panel2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            panel2.Location = new System.Drawing.Point(1015, 122);
            panel2.Margin = new System.Windows.Forms.Padding(4);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(82, 430);
            panel2.TabIndex = 4;
            // 
            // toolStrip3
            // 
            toolStrip3.Dock = System.Windows.Forms.DockStyle.Bottom;
            toolStrip3.ImageScalingSize = new System.Drawing.Size(32, 32);
            toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelRtfMode, toolStripSeparator2, toolStripLabelWordCount, toolStripSeparator3, toolStripButtonDone });
            toolStrip3.Location = new System.Drawing.Point(0, 549);
            toolStrip3.Name = "toolStrip3";
            toolStrip3.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            toolStrip3.Size = new System.Drawing.Size(1097, 39);
            toolStrip3.TabIndex = 5;
            toolStrip3.Text = "toolStrip3";
            // 
            // toolStripLabelRtfMode
            // 
            toolStripLabelRtfMode.Font = new System.Drawing.Font("Segoe UI", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            toolStripLabelRtfMode.Name = "toolStripLabelRtfMode";
            toolStripLabelRtfMode.Size = new System.Drawing.Size(101, 36);
            toolStripLabelRtfMode.Text = "toolStripLabel1";
            toolStripLabelRtfMode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            toolStripLabelRtfMode.Click += toolStripLabelRtfMode_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 39);
            // 
            // toolStripLabelWordCount
            // 
            toolStripLabelWordCount.Font = new System.Drawing.Font("Segoe UI", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            toolStripLabelWordCount.Name = "toolStripLabelWordCount";
            toolStripLabelWordCount.Size = new System.Drawing.Size(81, 36);
            toolStripLabelWordCount.Text = "Word count";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 39);
            // 
            // toolStripButtonDone
            // 
            toolStripButtonDone.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonDone.Image = (System.Drawing.Image)resources.GetObject("toolStripButtonDone.Image");
            toolStripButtonDone.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonDone.Name = "toolStripButtonDone";
            toolStripButtonDone.Size = new System.Drawing.Size(36, 36);
            toolStripButtonDone.Text = "toolStripButtonDone";
            // 
            // tabControl3
            // 
            tabControl3.Controls.Add(tabPage1);
            tabControl3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            tabControl3.Location = new System.Drawing.Point(937, 0);
            tabControl3.Margin = new System.Windows.Forms.Padding(4);
            tabControl3.Name = "tabControl3";
            tabControl3.SelectedIndex = 0;
            tabControl3.Size = new System.Drawing.Size(160, 114);
            tabControl3.TabIndex = 6;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(toolStrip1);
            tabPage1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            tabPage1.Location = new System.Drawing.Point(4, 26);
            tabPage1.Margin = new System.Windows.Forms.Padding(4);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new System.Windows.Forms.Padding(4);
            tabPage1.Size = new System.Drawing.Size(152, 84);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Favourites";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            toolStrip1.Location = new System.Drawing.Point(4, 4);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            toolStrip1.Size = new System.Drawing.Size(144, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip2";
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            richTextBox1.EnableAutoDragDrop = true;
            richTextBox1.Font = new System.Drawing.Font("Segoe UI", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            richTextBox1.HideSelection = false;
            richTextBox1.Location = new System.Drawing.Point(245, 122);
            richTextBox1.Margin = new System.Windows.Forms.Padding(4);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new System.Drawing.Size(769, 432);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1097, 588);
            Controls.Add(tabControl3);
            Controls.Add(toolStrip3);
            Controls.Add(panel2);
            Controls.Add(tabControl2);
            Controls.Add(tabControl1);
            Controls.Add(panel1);
            Controls.Add(richTextBox1);
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(4);
            Name = "Form1";
            Text = "Clipboard Text Toolbox 2";
            Load += On_FormLoad;
            tabControl1.ResumeLayout(false);
            tabCase.ResumeLayout(false);
            tabCase.PerformLayout();
            toolStripCase.ResumeLayout(false);
            toolStripCase.PerformLayout();
            tabLangTools.ResumeLayout(false);
            tabLangTools.PerformLayout();
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            tabControl2.ResumeLayout(false);
            tabPage0.ResumeLayout(false);
            tabPage0.PerformLayout();
            toolStrip3.ResumeLayout(false);
            toolStrip3.PerformLayout();
            tabControl3.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

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
        private System.Windows.Forms.ToolStripButton toolStripButtonDone;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton toolStripButtonSpellcheck;
        private System.Windows.Forms.ToolStripButton toolStripButtonCheckPunctuation;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ToolStripButton toolStripButton6;
        private System.Windows.Forms.ToolStripButton toolStripButton7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
    }
}

