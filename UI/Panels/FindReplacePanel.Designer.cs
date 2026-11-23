namespace ClipX2.UI.Panels
{
    partial class FindReplacePanel
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
            flowModes = new System.Windows.Forms.FlowLayoutPanel();
            radioSimple = new System.Windows.Forms.RadioButton();
            radioWildcard = new System.Windows.Forms.RadioButton();
            radioRegex = new System.Windows.Forms.RadioButton();
            labelFind = new System.Windows.Forms.Label();
            comboFind = new System.Windows.Forms.ComboBox();
            labelReplace = new System.Windows.Forms.Label();
            buttonSwap = new System.Windows.Forms.Button();
            comboReplace = new System.Windows.Forms.ComboBox();
            checkBoxMatchCase = new System.Windows.Forms.CheckBox();
            checkBoxWholeWords = new System.Windows.Forms.CheckBox();
            checkBoxSmartReplace = new System.Windows.Forms.CheckBox();
            checkBoxSpecialChars = new System.Windows.Forms.CheckBox();
            checkBoxFromStart = new System.Windows.Forms.CheckBox();
            checkBoxInSelection = new System.Windows.Forms.CheckBox();
            groupSpecial = new System.Windows.Forms.GroupBox();
            tableSpecial = new System.Windows.Forms.TableLayoutPanel();
            buttonTab = new System.Windows.Forms.Button();
            buttonNewLine = new System.Windows.Forms.Button();
            buttonBackslash = new System.Windows.Forms.Button();
            buttonHex = new System.Windows.Forms.Button();
            buttonUnicode = new System.Windows.Forms.Button();
            groupWildcard = new System.Windows.Forms.GroupBox();
            tableWildcard = new System.Windows.Forms.TableLayoutPanel();
            labelPosition = new System.Windows.Forms.Label();
            comboPosition = new System.Windows.Forms.ComboBox();
            labelNumbers = new System.Windows.Forms.Label();
            comboNumbers = new System.Windows.Forms.ComboBox();
            buttonPrevious = new System.Windows.Forms.Button();
            buttonNext = new System.Windows.Forms.Button();
            buttonFindAll = new System.Windows.Forms.Button();
            buttonReplace = new System.Windows.Forms.Button();
            buttonReplaceAll = new System.Windows.Forms.Button();
            buttonHelp = new System.Windows.Forms.Button();
            labelStatus = new System.Windows.Forms.Label();
            toolTip = new System.Windows.Forms.ToolTip(components);
            tableLayout.SuspendLayout();
            flowModes.SuspendLayout();
            groupSpecial.SuspendLayout();
            tableSpecial.SuspendLayout();
            groupWildcard.SuspendLayout();
            tableWildcard.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayout
            // 
            tableLayout.ColumnCount = 2;
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayout.Controls.Add(labelTitle, 0, 0);
            tableLayout.Controls.Add(flowModes, 0, 1);
            tableLayout.Controls.Add(labelFind, 0, 2);
            tableLayout.Controls.Add(comboFind, 0, 3);
            tableLayout.Controls.Add(labelReplace, 0, 4);
            tableLayout.Controls.Add(buttonSwap, 1, 4);
            tableLayout.Controls.Add(comboReplace, 0, 5);
            tableLayout.Controls.Add(checkBoxMatchCase, 0, 6);
            tableLayout.Controls.Add(checkBoxWholeWords, 1, 6);
            tableLayout.Controls.Add(checkBoxSmartReplace, 0, 7);
            tableLayout.Controls.Add(checkBoxSpecialChars, 1, 7);
            tableLayout.Controls.Add(checkBoxFromStart, 0, 8);
            tableLayout.Controls.Add(checkBoxInSelection, 1, 8);
            tableLayout.Controls.Add(groupSpecial, 0, 9);
            tableLayout.Controls.Add(groupWildcard, 0, 10);
            tableLayout.Controls.Add(buttonPrevious, 0, 11);
            tableLayout.Controls.Add(buttonNext, 1, 11);
            tableLayout.Controls.Add(buttonFindAll, 0, 12);
            tableLayout.Controls.Add(buttonReplace, 1, 12);
            tableLayout.Controls.Add(buttonReplaceAll, 0, 13);
            tableLayout.Controls.Add(buttonHelp, 1, 13);
            tableLayout.Controls.Add(labelStatus, 0, 14);
            tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayout.Location = new System.Drawing.Point(0, 0);
            tableLayout.Name = "tableLayout";
            tableLayout.Padding = new System.Windows.Forms.Padding(2);
            tableLayout.RowCount = 16;
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayout.Size = new System.Drawing.Size(243, 560);
            tableLayout.TabIndex = 0;
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            tableLayout.SetColumnSpan(labelTitle, 2);
            labelTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            labelTitle.Location = new System.Drawing.Point(5, 2);
            labelTitle.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new System.Drawing.Size(110, 19);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "Find && Replace";
            // 
            // flowModes
            // 
            flowModes.AutoSize = true;
            tableLayout.SetColumnSpan(flowModes, 2);
            flowModes.Controls.Add(radioSimple);
            flowModes.Controls.Add(radioWildcard);
            flowModes.Controls.Add(radioRegex);
            flowModes.Dock = System.Windows.Forms.DockStyle.Fill;
            flowModes.Location = new System.Drawing.Point(2, 27);
            flowModes.Margin = new System.Windows.Forms.Padding(0);
            flowModes.Name = "flowModes";
            flowModes.Size = new System.Drawing.Size(239, 29);
            flowModes.TabIndex = 1;
            flowModes.WrapContents = false;
            // 
            // radioSimple
            // 
            radioSimple.AutoSize = true;
            radioSimple.Checked = true;
            radioSimple.Location = new System.Drawing.Point(3, 3);
            radioSimple.Name = "radioSimple";
            radioSimple.Size = new System.Drawing.Size(67, 23);
            radioSimple.TabIndex = 0;
            radioSimple.TabStop = true;
            radioSimple.Text = "Si&mple";
            radioSimple.UseVisualStyleBackColor = true;
            // 
            // radioWildcard
            // 
            radioWildcard.AutoSize = true;
            radioWildcard.Location = new System.Drawing.Point(76, 3);
            radioWildcard.Name = "radioWildcard";
            radioWildcard.Size = new System.Drawing.Size(80, 23);
            radioWildcard.TabIndex = 1;
            radioWildcard.Text = "&Wildcard";
            radioWildcard.UseVisualStyleBackColor = true;
            // 
            // radioRegex
            // 
            radioRegex.AutoSize = true;
            radioRegex.Location = new System.Drawing.Point(162, 3);
            radioRegex.Name = "radioRegex";
            radioRegex.Size = new System.Drawing.Size(63, 23);
            radioRegex.TabIndex = 2;
            radioRegex.Text = "Re&gEx";
            radioRegex.UseVisualStyleBackColor = true;
            // 
            // labelFind
            // 
            labelFind.AutoSize = true;
            tableLayout.SetColumnSpan(labelFind, 2);
            labelFind.Location = new System.Drawing.Point(5, 62);
            labelFind.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            labelFind.Name = "labelFind";
            labelFind.Size = new System.Drawing.Size(38, 19);
            labelFind.TabIndex = 2;
            labelFind.Text = "F&ind:";
            // 
            // comboFind
            // 
            tableLayout.SetColumnSpan(comboFind, 2);
            comboFind.Dock = System.Windows.Forms.DockStyle.Fill;
            comboFind.Location = new System.Drawing.Point(5, 84);
            comboFind.Name = "comboFind";
            comboFind.Size = new System.Drawing.Size(233, 25);
            comboFind.TabIndex = 3;
            // 
            // labelReplace
            // 
            labelReplace.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            labelReplace.AutoSize = true;
            labelReplace.Location = new System.Drawing.Point(5, 120);
            labelReplace.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            labelReplace.Name = "labelReplace";
            labelReplace.Size = new System.Drawing.Size(88, 19);
            labelReplace.TabIndex = 4;
            labelReplace.Text = "Replace wi&th:";
            // 
            // buttonSwap
            // 
            buttonSwap.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonSwap.Location = new System.Drawing.Point(210, 115);
            buttonSwap.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            buttonSwap.Name = "buttonSwap";
            buttonSwap.Size = new System.Drawing.Size(28, 24);
            buttonSwap.TabIndex = 6;
            buttonSwap.Text = "⇅";
            toolTip.SetToolTip(buttonSwap, "Swap the find and replace texts");
            buttonSwap.UseVisualStyleBackColor = true;
            // 
            // comboReplace
            // 
            tableLayout.SetColumnSpan(comboReplace, 2);
            comboReplace.Dock = System.Windows.Forms.DockStyle.Fill;
            comboReplace.Location = new System.Drawing.Point(5, 142);
            comboReplace.Name = "comboReplace";
            comboReplace.Size = new System.Drawing.Size(233, 25);
            comboReplace.TabIndex = 5;
            // 
            // checkBoxMatchCase
            // 
            checkBoxMatchCase.AutoSize = true;
            checkBoxMatchCase.Location = new System.Drawing.Point(5, 176);
            checkBoxMatchCase.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            checkBoxMatchCase.Name = "checkBoxMatchCase";
            checkBoxMatchCase.Size = new System.Drawing.Size(97, 23);
            checkBoxMatchCase.TabIndex = 7;
            checkBoxMatchCase.Text = "Match &case";
            toolTip.SetToolTip(checkBoxMatchCase, "Find only text in the same capitals and small letters as typed");
            checkBoxMatchCase.UseVisualStyleBackColor = true;
            // 
            // checkBoxWholeWords
            // 
            checkBoxWholeWords.AutoSize = true;
            checkBoxWholeWords.Location = new System.Drawing.Point(124, 176);
            checkBoxWholeWords.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            checkBoxWholeWords.Name = "checkBoxWholeWords";
            checkBoxWholeWords.Size = new System.Drawing.Size(108, 23);
            checkBoxWholeWords.TabIndex = 8;
            checkBoxWholeWords.Text = "Wh&ole words";
            toolTip.SetToolTip(checkBoxWholeWords, "Find only whole words: \"cat\" but not \"scatter\"");
            checkBoxWholeWords.UseVisualStyleBackColor = true;
            // 
            // checkBoxSmartReplace
            // 
            checkBoxSmartReplace.AutoSize = true;
            checkBoxSmartReplace.Location = new System.Drawing.Point(5, 202);
            checkBoxSmartReplace.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            checkBoxSmartReplace.Name = "checkBoxSmartReplace";
            checkBoxSmartReplace.Size = new System.Drawing.Size(111, 23);
            checkBoxSmartReplace.TabIndex = 9;
            checkBoxSmartReplace.Text = "&Smart replace";
            toolTip.SetToolTip(checkBoxSmartReplace, "The replacement takes the case of what it replaces: cat → dog, Cat → Dog, CAT → DOG");
            checkBoxSmartReplace.UseVisualStyleBackColor = true;
            // 
            // checkBoxSpecialChars
            // 
            checkBoxSpecialChars.AutoSize = true;
            checkBoxSpecialChars.Location = new System.Drawing.Point(124, 202);
            checkBoxSpecialChars.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            checkBoxSpecialChars.Name = "checkBoxSpecialChars";
            checkBoxSpecialChars.Size = new System.Drawing.Size(105, 23);
            checkBoxSpecialChars.TabIndex = 10;
            checkBoxSpecialChars.Text = "Sp&ecial chars";
            toolTip.SetToolTip(checkBoxSpecialChars, "\\t tab, \\n new line, \\x41 or \\u00E9 a character by its code, \\\\ a backslash");
            checkBoxSpecialChars.UseVisualStyleBackColor = true;
            // 
            // checkBoxFromStart
            // 
            checkBoxFromStart.AutoSize = true;
            checkBoxFromStart.Location = new System.Drawing.Point(5, 228);
            checkBoxFromStart.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            checkBoxFromStart.Name = "checkBoxFromStart";
            checkBoxFromStart.Size = new System.Drawing.Size(92, 23);
            checkBoxFromStart.TabIndex = 11;
            checkBoxFromStart.Text = "&From start";
            toolTip.SetToolTip(checkBoxFromStart, "The next search starts at the start of the text (Previous: at the end), then carries on from there");
            checkBoxFromStart.UseVisualStyleBackColor = true;
            // 
            // checkBoxInSelection
            // 
            checkBoxInSelection.AutoSize = true;
            checkBoxInSelection.Location = new System.Drawing.Point(124, 228);
            checkBoxInSelection.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            checkBoxInSelection.Name = "checkBoxInSelection";
            checkBoxInSelection.Size = new System.Drawing.Size(97, 23);
            checkBoxInSelection.TabIndex = 12;
            checkBoxInSelection.Text = "In selection";
            toolTip.SetToolTip(checkBoxInSelection, "Find All and Replace All work only on the selected text");
            checkBoxInSelection.UseVisualStyleBackColor = true;
            // 
            // groupSpecial
            // 
            groupSpecial.AutoSize = true;
            groupSpecial.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableLayout.SetColumnSpan(groupSpecial, 2);
            groupSpecial.Controls.Add(tableSpecial);
            groupSpecial.Dock = System.Windows.Forms.DockStyle.Fill;
            groupSpecial.Location = new System.Drawing.Point(5, 254);
            groupSpecial.Name = "groupSpecial";
            groupSpecial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            groupSpecial.Size = new System.Drawing.Size(233, 79);
            groupSpecial.TabIndex = 13;
            groupSpecial.TabStop = false;
            groupSpecial.Text = "Special characters";
            groupSpecial.Visible = false;
            // 
            // tableSpecial
            // 
            tableSpecial.AutoSize = true;
            tableSpecial.ColumnCount = 3;
            tableSpecial.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            tableSpecial.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            tableSpecial.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            tableSpecial.Controls.Add(buttonTab, 0, 0);
            tableSpecial.Controls.Add(buttonNewLine, 1, 0);
            tableSpecial.Controls.Add(buttonBackslash, 2, 0);
            tableSpecial.Controls.Add(buttonHex, 0, 1);
            tableSpecial.Controls.Add(buttonUnicode, 1, 1);
            tableSpecial.Dock = System.Windows.Forms.DockStyle.Fill;
            tableSpecial.Location = new System.Drawing.Point(3, 18);
            tableSpecial.Name = "tableSpecial";
            tableSpecial.RowCount = 2;
            tableSpecial.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            tableSpecial.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            tableSpecial.Size = new System.Drawing.Size(227, 58);
            tableSpecial.TabIndex = 0;
            // 
            // buttonTab
            // 
            buttonTab.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonTab.Location = new System.Drawing.Point(1, 1);
            buttonTab.Margin = new System.Windows.Forms.Padding(1);
            buttonTab.Name = "buttonTab";
            buttonTab.Size = new System.Drawing.Size(66, 27);
            buttonTab.TabIndex = 0;
            buttonTab.Text = "Tab  \\t";
            toolTip.SetToolTip(buttonTab, "Put \\t (a tab) in the Find or Replace box, where the cursor was");
            buttonTab.UseVisualStyleBackColor = true;
            // 
            // buttonNewLine
            // 
            buttonNewLine.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonNewLine.Location = new System.Drawing.Point(69, 1);
            buttonNewLine.Margin = new System.Windows.Forms.Padding(1);
            buttonNewLine.Name = "buttonNewLine";
            buttonNewLine.Size = new System.Drawing.Size(88, 27);
            buttonNewLine.TabIndex = 1;
            buttonNewLine.Text = "New line  \\n";
            toolTip.SetToolTip(buttonNewLine, "Put \\n (a new line: a paragraph break) in the Find or Replace box");
            buttonNewLine.UseVisualStyleBackColor = true;
            // 
            // buttonBackslash
            // 
            buttonBackslash.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonBackslash.Location = new System.Drawing.Point(159, 1);
            buttonBackslash.Margin = new System.Windows.Forms.Padding(1);
            buttonBackslash.Name = "buttonBackslash";
            buttonBackslash.Size = new System.Drawing.Size(67, 27);
            buttonBackslash.TabIndex = 2;
            buttonBackslash.Text = "\\\\";
            toolTip.SetToolTip(buttonBackslash, "Put \\\\ (a backslash itself) in the box");
            buttonBackslash.UseVisualStyleBackColor = true;
            // 
            // buttonHex
            // 
            buttonHex.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonHex.Location = new System.Drawing.Point(1, 30);
            buttonHex.Margin = new System.Windows.Forms.Padding(1);
            buttonHex.Name = "buttonHex";
            buttonHex.Size = new System.Drawing.Size(66, 27);
            buttonHex.TabIndex = 3;
            buttonHex.Text = "Hex  \\x..";
            toolTip.SetToolTip(buttonHex, "Put \\x in the box: type two hexadecimal digits after it, as \\x41 for A");
            buttonHex.UseVisualStyleBackColor = true;
            // 
            // buttonUnicode
            // 
            tableSpecial.SetColumnSpan(buttonUnicode, 2);
            buttonUnicode.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonUnicode.Location = new System.Drawing.Point(69, 30);
            buttonUnicode.Margin = new System.Windows.Forms.Padding(1);
            buttonUnicode.Name = "buttonUnicode";
            buttonUnicode.Size = new System.Drawing.Size(157, 27);
            buttonUnicode.TabIndex = 4;
            buttonUnicode.Text = "Unicode  \\u....";
            toolTip.SetToolTip(buttonUnicode, "Put \\u in the box: type four hexadecimal digits after it, as \\u00E9 for é");
            buttonUnicode.UseVisualStyleBackColor = true;
            // 
            // groupWildcard
            // 
            groupWildcard.AutoSize = true;
            groupWildcard.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableLayout.SetColumnSpan(groupWildcard, 2);
            groupWildcard.Controls.Add(tableWildcard);
            groupWildcard.Dock = System.Windows.Forms.DockStyle.Fill;
            groupWildcard.Location = new System.Drawing.Point(5, 339);
            groupWildcard.Name = "groupWildcard";
            groupWildcard.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            groupWildcard.Size = new System.Drawing.Size(233, 79);
            groupWildcard.TabIndex = 14;
            groupWildcard.TabStop = false;
            groupWildcard.Text = "Wildcard options";
            groupWildcard.Visible = false;
            // 
            // tableWildcard
            // 
            tableWildcard.AutoSize = true;
            tableWildcard.ColumnCount = 2;
            tableWildcard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableWildcard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableWildcard.Controls.Add(labelPosition, 0, 0);
            tableWildcard.Controls.Add(comboPosition, 1, 0);
            tableWildcard.Controls.Add(labelNumbers, 0, 1);
            tableWildcard.Controls.Add(comboNumbers, 1, 1);
            tableWildcard.Dock = System.Windows.Forms.DockStyle.Fill;
            tableWildcard.Location = new System.Drawing.Point(3, 18);
            tableWildcard.Name = "tableWildcard";
            tableWildcard.RowCount = 2;
            tableWildcard.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableWildcard.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableWildcard.Size = new System.Drawing.Size(227, 58);
            tableWildcard.TabIndex = 0;
            // 
            // labelPosition
            // 
            labelPosition.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelPosition.AutoSize = true;
            labelPosition.Location = new System.Drawing.Point(3, 5);
            labelPosition.Name = "labelPosition";
            labelPosition.Size = new System.Drawing.Size(54, 19);
            labelPosition.TabIndex = 0;
            labelPosition.Text = "Fin&d at:";
            // 
            // comboPosition
            // 
            comboPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            comboPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboPosition.Location = new System.Drawing.Point(111, 2);
            comboPosition.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            comboPosition.Name = "comboPosition";
            comboPosition.Size = new System.Drawing.Size(113, 25);
            comboPosition.TabIndex = 1;
            toolTip.SetToolTip(comboPosition, "Where the match must be: anywhere, or at the start or end of a line or a word");
            // 
            // labelNumbers
            // 
            labelNumbers.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelNumbers.AutoSize = true;
            labelNumbers.Location = new System.Drawing.Point(3, 34);
            labelNumbers.Name = "labelNumbers";
            labelNumbers.Size = new System.Drawing.Size(102, 19);
            labelNumbers.TabIndex = 2;
            labelNumbers.Text = "# n&umbers like:";
            // 
            // comboNumbers
            // 
            comboNumbers.Dock = System.Windows.Forms.DockStyle.Fill;
            comboNumbers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboNumbers.Location = new System.Drawing.Point(111, 31);
            comboNumbers.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            comboNumbers.Name = "comboNumbers";
            comboNumbers.Size = new System.Drawing.Size(113, 25);
            comboNumbers.TabIndex = 3;
            toolTip.SetToolTip(comboNumbers, "The decimal point and thousands separator of the numbers # finds");
            // 
            // buttonPrevious
            // 
            buttonPrevious.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonPrevious.Location = new System.Drawing.Point(5, 429);
            buttonPrevious.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            buttonPrevious.Name = "buttonPrevious";
            buttonPrevious.Size = new System.Drawing.Size(113, 30);
            buttonPrevious.TabIndex = 15;
            buttonPrevious.Text = "◀ &Previous";
            toolTip.SetToolTip(buttonPrevious, "The previous match (Shift+F3, Shift+Enter)");
            buttonPrevious.UseVisualStyleBackColor = true;
            // 
            // buttonNext
            // 
            buttonNext.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonNext.Location = new System.Drawing.Point(124, 429);
            buttonNext.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            buttonNext.Name = "buttonNext";
            buttonNext.Size = new System.Drawing.Size(114, 30);
            buttonNext.TabIndex = 14;
            buttonNext.Text = "&Next ▶";
            toolTip.SetToolTip(buttonNext, "The next match (F3, Enter)");
            buttonNext.UseVisualStyleBackColor = true;
            // 
            // buttonFindAll
            // 
            buttonFindAll.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonFindAll.Location = new System.Drawing.Point(5, 465);
            buttonFindAll.Name = "buttonFindAll";
            buttonFindAll.Size = new System.Drawing.Size(113, 30);
            buttonFindAll.TabIndex = 16;
            buttonFindAll.Text = "Find &All";
            toolTip.SetToolTip(buttonFindAll, "Highlight every match and count them");
            buttonFindAll.UseVisualStyleBackColor = true;
            // 
            // buttonReplace
            // 
            buttonReplace.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonReplace.Location = new System.Drawing.Point(124, 465);
            buttonReplace.Name = "buttonReplace";
            buttonReplace.Size = new System.Drawing.Size(114, 30);
            buttonReplace.TabIndex = 17;
            buttonReplace.Text = "&Replace";
            toolTip.SetToolTip(buttonReplace, "Replace the selected match and go to the next (the first press only finds)");
            buttonReplace.UseVisualStyleBackColor = true;
            // 
            // buttonReplaceAll
            // 
            buttonReplaceAll.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonReplaceAll.Location = new System.Drawing.Point(5, 501);
            buttonReplaceAll.Name = "buttonReplaceAll";
            buttonReplaceAll.Size = new System.Drawing.Size(113, 30);
            buttonReplaceAll.TabIndex = 18;
            buttonReplaceAll.Text = "Replace A&ll";
            toolTip.SetToolTip(buttonReplaceAll, "Replace every match (in the selection with In selection); one Undo");
            buttonReplaceAll.UseVisualStyleBackColor = true;
            // 
            // buttonHelp
            // 
            buttonHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonHelp.Location = new System.Drawing.Point(124, 501);
            buttonHelp.Name = "buttonHelp";
            buttonHelp.Size = new System.Drawing.Size(114, 30);
            buttonHelp.TabIndex = 19;
            buttonHelp.Text = "&Help ▶";
            toolTip.SetToolTip(buttonHelp, "Show or hide help on this kind of search, beside the panel");
            buttonHelp.UseVisualStyleBackColor = true;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            tableLayout.SetColumnSpan(labelStatus, 2);
            labelStatus.Location = new System.Drawing.Point(5, 540);
            labelStatus.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            labelStatus.MaximumSize = new System.Drawing.Size(233, 0);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new System.Drawing.Size(0, 19);
            labelStatus.TabIndex = 20;
            // 
            // FindReplacePanel
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tableLayout);
            Name = "FindReplacePanel";
            Size = new System.Drawing.Size(243, 560);
            Controls.SetChildIndex(tableLayout, 0);
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            flowModes.ResumeLayout(false);
            flowModes.PerformLayout();
            groupSpecial.ResumeLayout(false);
            groupSpecial.PerformLayout();
            tableSpecial.ResumeLayout(false);
            groupWildcard.ResumeLayout(false);
            groupWildcard.PerformLayout();
            tableWildcard.ResumeLayout(false);
            tableWildcard.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.FlowLayoutPanel flowModes;
        private System.Windows.Forms.RadioButton radioSimple;
        private System.Windows.Forms.RadioButton radioWildcard;
        private System.Windows.Forms.RadioButton radioRegex;
        private System.Windows.Forms.Label labelFind;
        private System.Windows.Forms.ComboBox comboFind;
        private System.Windows.Forms.Label labelReplace;
        private System.Windows.Forms.Button buttonSwap;
        private System.Windows.Forms.ComboBox comboReplace;
        private System.Windows.Forms.CheckBox checkBoxMatchCase;
        private System.Windows.Forms.CheckBox checkBoxWholeWords;
        private System.Windows.Forms.CheckBox checkBoxSmartReplace;
        private System.Windows.Forms.CheckBox checkBoxSpecialChars;
        private System.Windows.Forms.CheckBox checkBoxFromStart;
        private System.Windows.Forms.CheckBox checkBoxInSelection;
        private System.Windows.Forms.GroupBox groupWildcard;
        private System.Windows.Forms.TableLayoutPanel tableWildcard;
        private System.Windows.Forms.Label labelPosition;
        private System.Windows.Forms.ComboBox comboPosition;
        private System.Windows.Forms.Label labelNumbers;
        private System.Windows.Forms.ComboBox comboNumbers;
        private System.Windows.Forms.GroupBox groupSpecial;
        private System.Windows.Forms.TableLayoutPanel tableSpecial;
        private System.Windows.Forms.Button buttonTab;
        private System.Windows.Forms.Button buttonNewLine;
        private System.Windows.Forms.Button buttonHex;
        private System.Windows.Forms.Button buttonUnicode;
        private System.Windows.Forms.Button buttonBackslash;
        private System.Windows.Forms.Button buttonPrevious;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.Button buttonFindAll;
        private System.Windows.Forms.Button buttonReplace;
        private System.Windows.Forms.Button buttonReplaceAll;
        private System.Windows.Forms.Button buttonHelp;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
