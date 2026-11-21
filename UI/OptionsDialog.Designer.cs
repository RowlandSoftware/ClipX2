namespace ClipX2.UI
{
    partial class OptionsDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tabControl = new System.Windows.Forms.TabControl();
            tabDisplay = new System.Windows.Forms.TabPage();
            labelFontIntro = new System.Windows.Forms.Label();
            radioProportional = new System.Windows.Forms.RadioButton();
            textBoxProportional = new System.Windows.Forms.TextBox();
            buttonProportional = new System.Windows.Forms.Button();
            radioFixed = new System.Windows.Forms.RadioButton();
            textBoxFixed = new System.Windows.Forms.TextBox();
            buttonFixed = new System.Windows.Forms.Button();
            labelFontTip = new System.Windows.Forms.Label();
            checkBoxWordWrap = new System.Windows.Forms.CheckBox();
            tabGeneral = new System.Windows.Forms.TabPage();
            labelSettingsIntro = new System.Windows.Forms.Label();
            linkSettingsFolder = new System.Windows.Forms.LinkLabel();
            buttonExport = new System.Windows.Forms.Button();
            buttonImport = new System.Windows.Forms.Button();
            labelExportNote = new System.Windows.Forms.Label();
            checkBoxHideFormattingWarning = new System.Windows.Forms.CheckBox();
            tabSpelling = new System.Windows.Forms.TabPage();
            labelDictionaries = new System.Windows.Forms.Label();
            labelFind = new System.Windows.Forms.Label();
            textBoxFindDictionary = new System.Windows.Forms.TextBox();
            groupBoxSpellingOptions = new System.Windows.Forms.GroupBox();
            listViewDictionaries = new System.Windows.Forms.ListView();
            columnName = new System.Windows.Forms.ColumnHeader();
            columnLanguage = new System.Windows.Forms.ColumnHeader();
            columnWhere = new System.Windows.Forms.ColumnHeader();
            columnLicence = new System.Windows.Forms.ColumnHeader();
            buttonShowAvailable = new System.Windows.Forms.Button();
            buttonDownload = new System.Windows.Forms.Button();
            buttonRemove = new System.Windows.Forms.Button();
            buttonSetDefault = new System.Windows.Forms.Button();
            buttonOpenFolder = new System.Windows.Forms.Button();
            labelDictionaryStatus = new System.Windows.Forms.Label();
            linkDictionarySource = new System.Windows.Forms.LinkLabel();
            checkBoxIgnoreUppercase = new System.Windows.Forms.CheckBox();
            checkBoxIgnoreNumbers = new System.Windows.Forms.CheckBox();
            checkBoxIgnoreAddresses = new System.Windows.Forms.CheckBox();
            checkBoxRepeated = new System.Windows.Forms.CheckBox();
            checkBoxSmartReplace = new System.Windows.Forms.CheckBox();
            checkBoxDefinitions = new System.Windows.Forms.CheckBox();
            checkBoxSimpleFirst = new System.Windows.Forms.CheckBox();
            checkBoxOxford = new System.Windows.Forms.CheckBox();
            tabGrammar = new System.Windows.Forms.TabPage();
            checkBoxGrammar = new System.Windows.Forms.CheckBox();
            groupBoxService = new System.Windows.Forms.GroupBox();
            radioFree = new System.Windows.Forms.RadioButton();
            radioPremium = new System.Windows.Forms.RadioButton();
            labelEmail = new System.Windows.Forms.Label();
            textBoxEmail = new System.Windows.Forms.TextBox();
            labelKey = new System.Windows.Forms.Label();
            textBoxKey = new System.Windows.Forms.TextBox();
            linkGetKey = new System.Windows.Forms.LinkLabel();
            radioOwnServer = new System.Windows.Forms.RadioButton();
            textBoxServerUrl = new System.Windows.Forms.TextBox();
            buttonTest = new System.Windows.Forms.Button();
            labelTestResult = new System.Windows.Forms.Label();
            checkBoxPicky = new System.Windows.Forms.CheckBox();
            checkBoxSecondOpinion = new System.Windows.Forms.CheckBox();
            labelIgnoredRules = new System.Windows.Forms.Label();
            listBoxIgnoredRules = new System.Windows.Forms.ListBox();
            buttonRuleOn = new System.Windows.Forms.Button();
            linkLanguageTool = new System.Windows.Forms.LinkLabel();
            buttonOK = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            buttonApply = new System.Windows.Forms.Button();
            fontDialog = new System.Windows.Forms.FontDialog();
            saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            openFileDialog = new System.Windows.Forms.OpenFileDialog();
            toolTip = new System.Windows.Forms.ToolTip(components);
            tabControl.SuspendLayout();
            tabDisplay.SuspendLayout();
            tabGeneral.SuspendLayout();
            tabSpelling.SuspendLayout();
            groupBoxSpellingOptions.SuspendLayout();
            tabGrammar.SuspendLayout();
            groupBoxService.SuspendLayout();
            SuspendLayout();
            //
            // tabControl
            //
            tabControl.Controls.Add(tabDisplay);
            tabControl.Controls.Add(tabGeneral);
            tabControl.Controls.Add(tabSpelling);
            tabControl.Controls.Add(tabGrammar);
            tabControl.Location = new System.Drawing.Point(10, 10);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(620, 500);
            tabControl.TabIndex = 0;
            //
            // tabDisplay
            //
            tabDisplay.Controls.Add(labelFontIntro);
            tabDisplay.Controls.Add(radioProportional);
            tabDisplay.Controls.Add(textBoxProportional);
            tabDisplay.Controls.Add(buttonProportional);
            tabDisplay.Controls.Add(radioFixed);
            tabDisplay.Controls.Add(textBoxFixed);
            tabDisplay.Controls.Add(buttonFixed);
            tabDisplay.Controls.Add(labelFontTip);
            tabDisplay.Controls.Add(checkBoxWordWrap);
            tabDisplay.Location = new System.Drawing.Point(4, 26);
            tabDisplay.Name = "tabDisplay";
            tabDisplay.Padding = new System.Windows.Forms.Padding(3);
            tabDisplay.Size = new System.Drawing.Size(612, 470);
            tabDisplay.TabIndex = 0;
            tabDisplay.Text = "Display";
            tabDisplay.UseVisualStyleBackColor = true;
            //
            // labelFontIntro
            //
            labelFontIntro.AutoSize = true;
            labelFontIntro.Location = new System.Drawing.Point(12, 14);
            labelFontIntro.Name = "labelFontIntro";
            labelFontIntro.Size = new System.Drawing.Size(330, 19);
            labelFontIntro.TabIndex = 0;
            labelFontIntro.Text = "Font for plain text (rich text keeps its own fonts):";
            //
            // radioProportional
            //
            radioProportional.Location = new System.Drawing.Point(16, 46);
            radioProportional.Name = "radioProportional";
            radioProportional.Size = new System.Drawing.Size(130, 24);
            radioProportional.TabIndex = 1;
            radioProportional.TabStop = true;
            radioProportional.Text = "&Proportional:";
            radioProportional.UseVisualStyleBackColor = true;
            //
            // textBoxProportional
            //
            textBoxProportional.Location = new System.Drawing.Point(150, 45);
            textBoxProportional.Name = "textBoxProportional";
            textBoxProportional.ReadOnly = true;
            textBoxProportional.Size = new System.Drawing.Size(330, 25);
            textBoxProportional.TabIndex = 2;
            textBoxProportional.TabStop = false;
            //
            // buttonProportional
            //
            buttonProportional.Location = new System.Drawing.Point(490, 43);
            buttonProportional.Name = "buttonProportional";
            buttonProportional.Size = new System.Drawing.Size(100, 30);
            buttonProportional.TabIndex = 3;
            buttonProportional.Text = "Choose…";
            buttonProportional.UseVisualStyleBackColor = true;
            //
            // radioFixed
            //
            radioFixed.Location = new System.Drawing.Point(16, 84);
            radioFixed.Name = "radioFixed";
            radioFixed.Size = new System.Drawing.Size(130, 24);
            radioFixed.TabIndex = 4;
            radioFixed.TabStop = true;
            radioFixed.Text = "&Fixed-width:";
            radioFixed.UseVisualStyleBackColor = true;
            //
            // textBoxFixed
            //
            textBoxFixed.Location = new System.Drawing.Point(150, 83);
            textBoxFixed.Name = "textBoxFixed";
            textBoxFixed.ReadOnly = true;
            textBoxFixed.Size = new System.Drawing.Size(330, 25);
            textBoxFixed.TabIndex = 5;
            textBoxFixed.TabStop = false;
            //
            // buttonFixed
            //
            buttonFixed.Location = new System.Drawing.Point(490, 81);
            buttonFixed.Name = "buttonFixed";
            buttonFixed.Size = new System.Drawing.Size(100, 30);
            buttonFixed.TabIndex = 6;
            buttonFixed.Text = "Choose…";
            buttonFixed.UseVisualStyleBackColor = true;
            //
            // labelFontTip
            //
            labelFontTip.AutoSize = true;
            labelFontTip.ForeColor = System.Drawing.SystemColors.GrayText;
            labelFontTip.Location = new System.Drawing.Point(16, 122);
            labelFontTip.Name = "labelFontTip";
            labelFontTip.Size = new System.Drawing.Size(370, 19);
            labelFontTip.TabIndex = 7;
            labelFontTip.Text = "Right-click the Options button to switch between them.";
            //
            // checkBoxWordWrap
            //
            checkBoxWordWrap.AutoSize = true;
            checkBoxWordWrap.Location = new System.Drawing.Point(16, 166);
            checkBoxWordWrap.Name = "checkBoxWordWrap";
            checkBoxWordWrap.Size = new System.Drawing.Size(200, 23);
            checkBoxWordWrap.TabIndex = 8;
            checkBoxWordWrap.Text = "&Word wrap (display only)";
            checkBoxWordWrap.UseVisualStyleBackColor = true;
            //
            // tabGeneral
            //
            tabGeneral.Controls.Add(labelSettingsIntro);
            tabGeneral.Controls.Add(linkSettingsFolder);
            tabGeneral.Controls.Add(buttonExport);
            tabGeneral.Controls.Add(buttonImport);
            tabGeneral.Controls.Add(labelExportNote);
            tabGeneral.Controls.Add(checkBoxHideFormattingWarning);
            tabGeneral.Location = new System.Drawing.Point(4, 26);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            tabGeneral.Size = new System.Drawing.Size(612, 470);
            tabGeneral.TabIndex = 1;
            tabGeneral.Text = "General";
            tabGeneral.UseVisualStyleBackColor = true;
            //
            // labelSettingsIntro
            //
            labelSettingsIntro.AutoSize = true;
            labelSettingsIntro.Location = new System.Drawing.Point(12, 14);
            labelSettingsIntro.Name = "labelSettingsIntro";
            labelSettingsIntro.Size = new System.Drawing.Size(320, 19);
            labelSettingsIntro.TabIndex = 0;
            labelSettingsIntro.Text = "Your settings and personal dictionary are kept in:";
            //
            // linkSettingsFolder
            //
            linkSettingsFolder.AutoSize = true;
            linkSettingsFolder.Location = new System.Drawing.Point(12, 38);
            linkSettingsFolder.Name = "linkSettingsFolder";
            linkSettingsFolder.Size = new System.Drawing.Size(100, 19);
            linkSettingsFolder.TabIndex = 1;
            linkSettingsFolder.TabStop = true;
            linkSettingsFolder.Text = "(folder)";
            //
            // buttonExport
            //
            buttonExport.Location = new System.Drawing.Point(16, 80);
            buttonExport.Name = "buttonExport";
            buttonExport.Size = new System.Drawing.Size(170, 30);
            buttonExport.TabIndex = 2;
            buttonExport.Text = "&Export settings…";
            buttonExport.UseVisualStyleBackColor = true;
            //
            // buttonImport
            //
            buttonImport.Location = new System.Drawing.Point(196, 80);
            buttonImport.Name = "buttonImport";
            buttonImport.Size = new System.Drawing.Size(170, 30);
            buttonImport.TabIndex = 3;
            buttonImport.Text = "&Import settings…";
            buttonImport.UseVisualStyleBackColor = true;
            //
            // labelExportNote
            //
            labelExportNote.ForeColor = System.Drawing.SystemColors.GrayText;
            labelExportNote.Location = new System.Drawing.Point(16, 122);
            labelExportNote.Name = "labelExportNote";
            labelExportNote.Size = new System.Drawing.Size(580, 80);
            labelExportNote.TabIndex = 4;
            labelExportNote.Text = "Export copies your settings to a file, to keep or to take to another computer. Import replaces your settings with those in such a file. A LanguageTool API key is stored so that only your Windows account can read it: after importing on another computer or account, enter it again.";
            // 
            // checkBoxHideFormattingWarning
            // 
            checkBoxHideFormattingWarning.AutoSize = true;
            checkBoxHideFormattingWarning.Location = new System.Drawing.Point(16, 215);
            checkBoxHideFormattingWarning.Name = "checkBoxHideFormattingWarning";
            checkBoxHideFormattingWarning.TabIndex = 5;
            checkBoxHideFormattingWarning.Text = "&Hide warning about loss of formatting";
            checkBoxHideFormattingWarning.UseVisualStyleBackColor = true;
            //
            // tabSpelling
            //
            tabSpelling.Controls.Add(labelDictionaries);
            tabSpelling.Controls.Add(listViewDictionaries);
            tabSpelling.Controls.Add(buttonShowAvailable);
            tabSpelling.Controls.Add(buttonDownload);
            tabSpelling.Controls.Add(buttonRemove);
            tabSpelling.Controls.Add(buttonSetDefault);
            tabSpelling.Controls.Add(buttonOpenFolder);
            tabSpelling.Controls.Add(labelDictionaryStatus);
            tabSpelling.Controls.Add(linkDictionarySource);
            tabSpelling.Controls.Add(labelFind);
            tabSpelling.Controls.Add(textBoxFindDictionary);
            tabSpelling.Controls.Add(groupBoxSpellingOptions);
            tabSpelling.Location = new System.Drawing.Point(4, 26);
            tabSpelling.Name = "tabSpelling";
            tabSpelling.Padding = new System.Windows.Forms.Padding(3);
            tabSpelling.Size = new System.Drawing.Size(612, 470);
            tabSpelling.TabIndex = 2;
            tabSpelling.Text = "Spelling";
            tabSpelling.UseVisualStyleBackColor = true;
            //
            // labelDictionaries
            //
            labelDictionaries.AutoSize = true;
            labelDictionaries.Location = new System.Drawing.Point(12, 10);
            labelDictionaries.Name = "labelDictionaries";
            labelDictionaries.Size = new System.Drawing.Size(90, 19);
            labelDictionaries.TabIndex = 0;
            labelDictionaries.Text = "Dictionaries:";
            //
            // listViewDictionaries
            //
            listViewDictionaries.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnName, columnLanguage, columnWhere, columnLicence });
            listViewDictionaries.FullRowSelect = true;
            listViewDictionaries.HideSelection = false;
            listViewDictionaries.Location = new System.Drawing.Point(12, 32);
            listViewDictionaries.MultiSelect = false;
            listViewDictionaries.Name = "listViewDictionaries";
            listViewDictionaries.Size = new System.Drawing.Size(586, 190);
            listViewDictionaries.TabIndex = 1;
            listViewDictionaries.UseCompatibleStateImageBehavior = false;
            listViewDictionaries.View = System.Windows.Forms.View.Details;
            //
            // columnName
            //
            columnName.Text = "Name";
            columnName.Width = 90;
            //
            // columnLanguage
            //
            columnLanguage.Text = "Language";
            columnLanguage.Width = 185;
            //
            // columnWhere
            //
            columnWhere.Text = "Installed";
            columnWhere.Width = 175;
            //
            // columnLicence
            //
            columnLicence.Text = "From";
            columnLicence.Width = 120;
            //
            // buttonShowAvailable
            //
            buttonShowAvailable.Location = new System.Drawing.Point(12, 228);
            buttonShowAvailable.Name = "buttonShowAvailable";
            buttonShowAvailable.Size = new System.Drawing.Size(150, 30);
            buttonShowAvailable.TabIndex = 2;
            buttonShowAvailable.Text = "&Show downloads";
            buttonShowAvailable.UseVisualStyleBackColor = true;
            //
            // buttonDownload
            //
            buttonDownload.Location = new System.Drawing.Point(168, 228);
            buttonDownload.Name = "buttonDownload";
            buttonDownload.Size = new System.Drawing.Size(92, 30);
            buttonDownload.TabIndex = 3;
            buttonDownload.Text = "&Download";
            buttonDownload.UseVisualStyleBackColor = true;
            //
            // buttonRemove
            //
            buttonRemove.Location = new System.Drawing.Point(266, 228);
            buttonRemove.Name = "buttonRemove";
            buttonRemove.Size = new System.Drawing.Size(84, 30);
            buttonRemove.TabIndex = 4;
            buttonRemove.Text = "&Remove";
            buttonRemove.UseVisualStyleBackColor = true;
            //
            // buttonSetDefault
            //
            buttonSetDefault.Location = new System.Drawing.Point(356, 228);
            buttonSetDefault.Name = "buttonSetDefault";
            buttonSetDefault.Size = new System.Drawing.Size(124, 30);
            buttonSetDefault.TabIndex = 5;
            buttonSetDefault.Text = "Set as defa&ult";
            buttonSetDefault.UseVisualStyleBackColor = true;
            //
            // buttonOpenFolder
            //
            buttonOpenFolder.Location = new System.Drawing.Point(486, 228);
            buttonOpenFolder.Name = "buttonOpenFolder";
            buttonOpenFolder.Size = new System.Drawing.Size(112, 30);
            buttonOpenFolder.TabIndex = 5;
            buttonOpenFolder.Text = "Open &folder";
            buttonOpenFolder.UseVisualStyleBackColor = true;
            //
            // labelDictionaryStatus
            //
            labelDictionaryStatus.AutoEllipsis = true;
            labelDictionaryStatus.Location = new System.Drawing.Point(12, 264);
            labelDictionaryStatus.Name = "labelDictionaryStatus";
            labelDictionaryStatus.Size = new System.Drawing.Size(586, 20);
            labelDictionaryStatus.TabIndex = 6;
            //
            // linkDictionarySource
            //
            linkDictionarySource.AutoSize = true;
            linkDictionarySource.Location = new System.Drawing.Point(12, 286);
            linkDictionarySource.Name = "linkDictionarySource";
            linkDictionarySource.Size = new System.Drawing.Size(400, 19);
            linkDictionarySource.TabIndex = 7;
            linkDictionarySource.TabStop = true;
            linkDictionarySource.Text = "Downloads come from LibreOffice's dictionaries; each has its own licence";
            //
            // labelFind
            //
            labelFind.AutoSize = true;
            labelFind.Location = new System.Drawing.Point(352, 9);
            labelFind.Name = "labelFind";
            labelFind.Size = new System.Drawing.Size(38, 19);
            labelFind.TabIndex = 16;
            labelFind.Text = "F&ind:";
            //
            // textBoxFindDictionary
            //
            textBoxFindDictionary.Location = new System.Drawing.Point(398, 5);
            textBoxFindDictionary.Name = "textBoxFindDictionary";
            textBoxFindDictionary.PlaceholderText = "language or name";
            textBoxFindDictionary.Size = new System.Drawing.Size(200, 25);
            textBoxFindDictionary.TabIndex = 17;
            //
            // groupBoxSpellingOptions
            //
            groupBoxSpellingOptions.Controls.Add(checkBoxIgnoreUppercase);
            groupBoxSpellingOptions.Controls.Add(checkBoxIgnoreNumbers);
            groupBoxSpellingOptions.Controls.Add(checkBoxIgnoreAddresses);
            groupBoxSpellingOptions.Controls.Add(checkBoxRepeated);
            groupBoxSpellingOptions.Controls.Add(checkBoxSmartReplace);
            groupBoxSpellingOptions.Controls.Add(checkBoxDefinitions);
            groupBoxSpellingOptions.Controls.Add(checkBoxSimpleFirst);
            groupBoxSpellingOptions.Controls.Add(checkBoxOxford);
            groupBoxSpellingOptions.Location = new System.Drawing.Point(12, 312);
            groupBoxSpellingOptions.Name = "groupBoxSpellingOptions";
            groupBoxSpellingOptions.Size = new System.Drawing.Size(586, 146);
            groupBoxSpellingOptions.TabIndex = 18;
            groupBoxSpellingOptions.TabStop = false;
            groupBoxSpellingOptions.Text = "Options";
            //
            // checkBoxIgnoreUppercase
            //
            checkBoxIgnoreUppercase.AutoSize = true;
            checkBoxIgnoreUppercase.Location = new System.Drawing.Point(12, 24);
            checkBoxIgnoreUppercase.Name = "checkBoxIgnoreUppercase";
            checkBoxIgnoreUppercase.Size = new System.Drawing.Size(220, 23);
            checkBoxIgnoreUppercase.TabIndex = 8;
            checkBoxIgnoreUppercase.Text = "Ignore words in &UPPER CASE";
            checkBoxIgnoreUppercase.UseVisualStyleBackColor = true;
            //
            // checkBoxIgnoreNumbers
            //
            checkBoxIgnoreNumbers.AutoSize = true;
            checkBoxIgnoreNumbers.Location = new System.Drawing.Point(12, 52);
            checkBoxIgnoreNumbers.Name = "checkBoxIgnoreNumbers";
            checkBoxIgnoreNumbers.Size = new System.Drawing.Size(210, 23);
            checkBoxIgnoreNumbers.TabIndex = 9;
            checkBoxIgnoreNumbers.Text = "Ignore words with &numbers";
            checkBoxIgnoreNumbers.UseVisualStyleBackColor = true;
            //
            // checkBoxIgnoreAddresses
            //
            checkBoxIgnoreAddresses.AutoSize = true;
            checkBoxIgnoreAddresses.Location = new System.Drawing.Point(12, 80);
            checkBoxIgnoreAddresses.Name = "checkBoxIgnoreAddresses";
            checkBoxIgnoreAddresses.Size = new System.Drawing.Size(250, 23);
            checkBoxIgnoreAddresses.TabIndex = 10;
            checkBoxIgnoreAddresses.Text = "Ignore &Internet and file addresses";
            checkBoxIgnoreAddresses.UseVisualStyleBackColor = true;
            //
            // checkBoxRepeated
            //
            checkBoxRepeated.AutoSize = true;
            checkBoxRepeated.Location = new System.Drawing.Point(12, 108);
            checkBoxRepeated.Name = "checkBoxRepeated";
            checkBoxRepeated.Size = new System.Drawing.Size(180, 23);
            checkBoxRepeated.TabIndex = 11;
            checkBoxRepeated.Text = "Check &repeated words";
            checkBoxRepeated.UseVisualStyleBackColor = true;
            //
            // checkBoxSmartReplace
            //
            checkBoxSmartReplace.AutoSize = true;
            checkBoxSmartReplace.Location = new System.Drawing.Point(288, 24);
            checkBoxSmartReplace.Name = "checkBoxSmartReplace";
            checkBoxSmartReplace.Size = new System.Drawing.Size(290, 23);
            checkBoxSmartReplace.TabIndex = 12;
            checkBoxSmartReplace.Text = "S&mart replace (keep the word's capitals)";
            checkBoxSmartReplace.UseVisualStyleBackColor = true;
            //
            // checkBoxDefinitions
            //
            checkBoxDefinitions.AutoSize = true;
            checkBoxDefinitions.Location = new System.Drawing.Point(288, 52);
            checkBoxDefinitions.Name = "checkBoxDefinitions";
            checkBoxDefinitions.Size = new System.Drawing.Size(290, 23);
            checkBoxDefinitions.TabIndex = 13;
            checkBoxDefinitions.Text = "Definitions on right-&click (Wiktionary)";
            checkBoxDefinitions.UseVisualStyleBackColor = true;
            //
            // checkBoxSimpleFirst
            //
            checkBoxSimpleFirst.AutoSize = true;
            checkBoxSimpleFirst.Location = new System.Drawing.Point(288, 80);
            checkBoxSimpleFirst.Name = "checkBoxSimpleFirst";
            checkBoxSimpleFirst.Size = new System.Drawing.Size(260, 23);
            checkBoxSimpleFirst.TabIndex = 14;
            checkBoxSimpleFirst.Text = "Simple English definitions &first";
            checkBoxSimpleFirst.UseVisualStyleBackColor = true;
            //
            // checkBoxOxford
            //
            checkBoxOxford.AutoSize = true;
            checkBoxOxford.Location = new System.Drawing.Point(288, 108);
            checkBoxOxford.Name = "checkBoxOxford";
            checkBoxOxford.Size = new System.Drawing.Size(240, 23);
            checkBoxOxford.TabIndex = 15;
            checkBoxOxford.Text = "Oxford spelling: -i&ze (realize)";
            checkBoxOxford.UseVisualStyleBackColor = true;
            //
            // tabGrammar
            //
            tabGrammar.Controls.Add(checkBoxGrammar);
            tabGrammar.Controls.Add(groupBoxService);
            tabGrammar.Controls.Add(buttonTest);
            tabGrammar.Controls.Add(labelTestResult);
            tabGrammar.Controls.Add(checkBoxPicky);
            tabGrammar.Controls.Add(checkBoxSecondOpinion);
            tabGrammar.Controls.Add(labelIgnoredRules);
            tabGrammar.Controls.Add(listBoxIgnoredRules);
            tabGrammar.Controls.Add(buttonRuleOn);
            tabGrammar.Controls.Add(linkLanguageTool);
            tabGrammar.Location = new System.Drawing.Point(4, 26);
            tabGrammar.Name = "tabGrammar";
            tabGrammar.Padding = new System.Windows.Forms.Padding(3);
            tabGrammar.Size = new System.Drawing.Size(612, 470);
            tabGrammar.TabIndex = 3;
            tabGrammar.Text = "Grammar";
            tabGrammar.UseVisualStyleBackColor = true;
            //
            // checkBoxGrammar
            //
            checkBoxGrammar.AutoSize = true;
            checkBoxGrammar.Location = new System.Drawing.Point(12, 12);
            checkBoxGrammar.Name = "checkBoxGrammar";
            checkBoxGrammar.Size = new System.Drawing.Size(480, 23);
            checkBoxGrammar.TabIndex = 0;
            checkBoxGrammar.Text = "Check &grammar with LanguageTool (the text is sent to the service below)";
            checkBoxGrammar.UseVisualStyleBackColor = true;
            //
            // groupBoxService
            //
            groupBoxService.Controls.Add(radioFree);
            groupBoxService.Controls.Add(radioPremium);
            groupBoxService.Controls.Add(labelEmail);
            groupBoxService.Controls.Add(textBoxEmail);
            groupBoxService.Controls.Add(labelKey);
            groupBoxService.Controls.Add(textBoxKey);
            groupBoxService.Controls.Add(linkGetKey);
            groupBoxService.Controls.Add(radioOwnServer);
            groupBoxService.Controls.Add(textBoxServerUrl);
            groupBoxService.Location = new System.Drawing.Point(12, 44);
            groupBoxService.Name = "groupBoxService";
            groupBoxService.Size = new System.Drawing.Size(586, 186);
            groupBoxService.TabIndex = 1;
            groupBoxService.TabStop = false;
            groupBoxService.Text = "Service";
            //
            // radioFree
            //
            radioFree.AutoSize = true;
            radioFree.Location = new System.Drawing.Point(14, 24);
            radioFree.Name = "radioFree";
            radioFree.Size = new System.Drawing.Size(380, 23);
            radioFree.TabIndex = 0;
            radioFree.TabStop = true;
            radioFree.Text = "&Free service (no account; limited text per minute)";
            radioFree.UseVisualStyleBackColor = true;
            //
            // radioPremium
            //
            radioPremium.AutoSize = true;
            radioPremium.Location = new System.Drawing.Point(14, 52);
            radioPremium.Name = "radioPremium";
            radioPremium.Size = new System.Drawing.Size(200, 23);
            radioPremium.TabIndex = 1;
            radioPremium.TabStop = true;
            radioPremium.Text = "LanguageTool &Premium";
            radioPremium.UseVisualStyleBackColor = true;
            //
            // labelEmail
            //
            labelEmail.AutoSize = true;
            labelEmail.Location = new System.Drawing.Point(40, 84);
            labelEmail.Name = "labelEmail";
            labelEmail.Size = new System.Drawing.Size(105, 19);
            labelEmail.TabIndex = 2;
            labelEmail.Text = "Account &email:";
            //
            // textBoxEmail
            //
            textBoxEmail.Location = new System.Drawing.Point(170, 81);
            textBoxEmail.Name = "textBoxEmail";
            textBoxEmail.Size = new System.Drawing.Size(300, 25);
            textBoxEmail.TabIndex = 3;
            //
            // labelKey
            //
            labelKey.AutoSize = true;
            labelKey.Location = new System.Drawing.Point(40, 116);
            labelKey.Name = "labelKey";
            labelKey.Size = new System.Drawing.Size(60, 19);
            labelKey.TabIndex = 4;
            labelKey.Text = "API &key:";
            //
            // textBoxKey
            //
            textBoxKey.Location = new System.Drawing.Point(170, 113);
            textBoxKey.Name = "textBoxKey";
            textBoxKey.Size = new System.Drawing.Size(300, 25);
            textBoxKey.TabIndex = 5;
            textBoxKey.UseSystemPasswordChar = true;
            //
            // linkGetKey
            //
            linkGetKey.AutoSize = true;
            linkGetKey.Location = new System.Drawing.Point(480, 116);
            linkGetKey.Name = "linkGetKey";
            linkGetKey.Size = new System.Drawing.Size(80, 19);
            linkGetKey.TabIndex = 6;
            linkGetKey.TabStop = true;
            linkGetKey.Text = "Get my key";
            //
            // radioOwnServer
            //
            radioOwnServer.AutoSize = true;
            radioOwnServer.Location = new System.Drawing.Point(14, 148);
            radioOwnServer.Name = "radioOwnServer";
            radioOwnServer.Size = new System.Drawing.Size(140, 23);
            radioOwnServer.TabIndex = 7;
            radioOwnServer.TabStop = true;
            radioOwnServer.Text = "&My own server:";
            radioOwnServer.UseVisualStyleBackColor = true;
            //
            // textBoxServerUrl
            //
            textBoxServerUrl.Location = new System.Drawing.Point(170, 147);
            textBoxServerUrl.Name = "textBoxServerUrl";
            textBoxServerUrl.Size = new System.Drawing.Size(300, 25);
            textBoxServerUrl.TabIndex = 8;
            //
            // buttonTest
            //
            buttonTest.Location = new System.Drawing.Point(12, 240);
            buttonTest.Name = "buttonTest";
            buttonTest.Size = new System.Drawing.Size(150, 30);
            buttonTest.TabIndex = 2;
            buttonTest.Text = "&Test connection";
            buttonTest.UseVisualStyleBackColor = true;
            //
            // labelTestResult
            //
            labelTestResult.Location = new System.Drawing.Point(170, 240);
            labelTestResult.Name = "labelTestResult";
            labelTestResult.Size = new System.Drawing.Size(428, 44);
            labelTestResult.TabIndex = 3;
            labelTestResult.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // checkBoxPicky
            //
            checkBoxPicky.AutoSize = true;
            checkBoxPicky.Location = new System.Drawing.Point(12, 292);
            checkBoxPicky.Name = "checkBoxPicky";
            checkBoxPicky.Size = new System.Drawing.Size(260, 23);
            checkBoxPicky.TabIndex = 4;
            checkBoxPicky.Text = "P&icky: stricter style rules too";
            checkBoxPicky.UseVisualStyleBackColor = true;
            //
            // checkBoxSecondOpinion
            //
            checkBoxSecondOpinion.AutoSize = true;
            checkBoxSecondOpinion.Location = new System.Drawing.Point(12, 320);
            checkBoxSecondOpinion.Name = "checkBoxSecondOpinion";
            checkBoxSecondOpinion.Size = new System.Drawing.Size(440, 23);
            checkBoxSecondOpinion.TabIndex = 5;
            checkBoxSecondOpinion.Text = "Second &opinion on spelling: skip words LanguageTool knows";
            checkBoxSecondOpinion.UseVisualStyleBackColor = true;
            //
            // labelIgnoredRules
            //
            labelIgnoredRules.AutoSize = true;
            labelIgnoredRules.Location = new System.Drawing.Point(12, 352);
            labelIgnoredRules.Name = "labelIgnoredRules";
            labelIgnoredRules.Size = new System.Drawing.Size(160, 19);
            labelIgnoredRules.TabIndex = 6;
            labelIgnoredRules.Text = "Rules you have ignored:";
            //
            // listBoxIgnoredRules
            //
            listBoxIgnoredRules.FormattingEnabled = true;
            listBoxIgnoredRules.IntegralHeight = false;
            listBoxIgnoredRules.Location = new System.Drawing.Point(12, 374);
            listBoxIgnoredRules.Name = "listBoxIgnoredRules";
            listBoxIgnoredRules.Size = new System.Drawing.Size(440, 84);
            listBoxIgnoredRules.TabIndex = 7;
            //
            // buttonRuleOn
            //
            buttonRuleOn.Location = new System.Drawing.Point(460, 374);
            buttonRuleOn.Name = "buttonRuleOn";
            buttonRuleOn.Size = new System.Drawing.Size(138, 30);
            buttonRuleOn.TabIndex = 8;
            buttonRuleOn.Text = "Switch &back on";
            buttonRuleOn.UseVisualStyleBackColor = true;
            //
            // linkLanguageTool
            //
            linkLanguageTool.AutoSize = true;
            linkLanguageTool.Location = new System.Drawing.Point(460, 436);
            linkLanguageTool.Name = "linkLanguageTool";
            linkLanguageTool.Size = new System.Drawing.Size(110, 19);
            linkLanguageTool.TabIndex = 9;
            linkLanguageTool.TabStop = true;
            linkLanguageTool.Text = "languagetool.org";
            //
            // buttonOK
            //
            buttonOK.Location = new System.Drawing.Point(370, 520);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(80, 30);
            buttonOK.TabIndex = 1;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            //
            // buttonCancel
            //
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(460, 520);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(80, 30);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // buttonApply
            //
            buttonApply.Location = new System.Drawing.Point(550, 520);
            buttonApply.Name = "buttonApply";
            buttonApply.Size = new System.Drawing.Size(80, 30);
            buttonApply.TabIndex = 3;
            buttonApply.Text = "&Apply";
            buttonApply.UseVisualStyleBackColor = true;
            //
            // fontDialog
            //
            fontDialog.FontMustExist = true;
            fontDialog.ShowEffects = false;
            //
            // saveFileDialog
            //
            saveFileDialog.DefaultExt = "json";
            saveFileDialog.FileName = "ClipX2 settings.json";
            saveFileDialog.Filter = "Settings files (*.json)|*.json|All files (*.*)|*.*";
            saveFileDialog.Title = "Export settings";
            //
            // openFileDialog
            //
            openFileDialog.DefaultExt = "json";
            openFileDialog.Filter = "Settings files (*.json)|*.json|All files (*.*)|*.*";
            openFileDialog.Title = "Import settings";
            //
            // OptionsDialog
            //
            AcceptButton = buttonOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(640, 560);
            Controls.Add(tabControl);
            Controls.Add(buttonOK);
            Controls.Add(buttonCancel);
            Controls.Add(buttonApply);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "OptionsDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Options";
            tabControl.ResumeLayout(false);
            tabDisplay.ResumeLayout(false);
            tabDisplay.PerformLayout();
            tabGeneral.ResumeLayout(false);
            tabGeneral.PerformLayout();
            tabSpelling.ResumeLayout(false);
            tabSpelling.PerformLayout();
            groupBoxSpellingOptions.ResumeLayout(false);
            groupBoxSpellingOptions.PerformLayout();
            tabGrammar.ResumeLayout(false);
            tabGrammar.PerformLayout();
            groupBoxService.ResumeLayout(false);
            groupBoxService.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabDisplay;
        private System.Windows.Forms.Label labelFontIntro;
        private System.Windows.Forms.RadioButton radioProportional;
        private System.Windows.Forms.TextBox textBoxProportional;
        private System.Windows.Forms.Button buttonProportional;
        private System.Windows.Forms.RadioButton radioFixed;
        private System.Windows.Forms.TextBox textBoxFixed;
        private System.Windows.Forms.Button buttonFixed;
        private System.Windows.Forms.Label labelFontTip;
        private System.Windows.Forms.CheckBox checkBoxWordWrap;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.Label labelSettingsIntro;
        private System.Windows.Forms.LinkLabel linkSettingsFolder;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.Label labelExportNote;
        private System.Windows.Forms.CheckBox checkBoxHideFormattingWarning;
        private System.Windows.Forms.TabPage tabSpelling;
        private System.Windows.Forms.Label labelDictionaries;
        private System.Windows.Forms.Label labelFind;
        private System.Windows.Forms.TextBox textBoxFindDictionary;
        private System.Windows.Forms.GroupBox groupBoxSpellingOptions;
        private System.Windows.Forms.ListView listViewDictionaries;
        private System.Windows.Forms.ColumnHeader columnName;
        private System.Windows.Forms.ColumnHeader columnLanguage;
        private System.Windows.Forms.ColumnHeader columnWhere;
        private System.Windows.Forms.ColumnHeader columnLicence;
        private System.Windows.Forms.Button buttonShowAvailable;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonSetDefault;
        private System.Windows.Forms.Button buttonOpenFolder;
        private System.Windows.Forms.Label labelDictionaryStatus;
        private System.Windows.Forms.LinkLabel linkDictionarySource;
        private System.Windows.Forms.CheckBox checkBoxIgnoreUppercase;
        private System.Windows.Forms.CheckBox checkBoxIgnoreNumbers;
        private System.Windows.Forms.CheckBox checkBoxIgnoreAddresses;
        private System.Windows.Forms.CheckBox checkBoxRepeated;
        private System.Windows.Forms.CheckBox checkBoxSmartReplace;
        private System.Windows.Forms.CheckBox checkBoxDefinitions;
        private System.Windows.Forms.CheckBox checkBoxSimpleFirst;
        private System.Windows.Forms.CheckBox checkBoxOxford;
        private System.Windows.Forms.TabPage tabGrammar;
        private System.Windows.Forms.CheckBox checkBoxGrammar;
        private System.Windows.Forms.GroupBox groupBoxService;
        private System.Windows.Forms.RadioButton radioFree;
        private System.Windows.Forms.RadioButton radioPremium;
        private System.Windows.Forms.Label labelEmail;
        private System.Windows.Forms.TextBox textBoxEmail;
        private System.Windows.Forms.Label labelKey;
        private System.Windows.Forms.TextBox textBoxKey;
        private System.Windows.Forms.LinkLabel linkGetKey;
        private System.Windows.Forms.RadioButton radioOwnServer;
        private System.Windows.Forms.TextBox textBoxServerUrl;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.Label labelTestResult;
        private System.Windows.Forms.CheckBox checkBoxPicky;
        private System.Windows.Forms.CheckBox checkBoxSecondOpinion;
        private System.Windows.Forms.Label labelIgnoredRules;
        private System.Windows.Forms.ListBox listBoxIgnoredRules;
        private System.Windows.Forms.Button buttonRuleOn;
        private System.Windows.Forms.LinkLabel linkLanguageTool;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.FontDialog fontDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
