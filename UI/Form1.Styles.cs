using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ClipX2.UI;

namespace ClipX2
{
    /// <summary>
    /// The Styles tab: font, size, bold, italic (rich text only) and increase / decrease indent
    /// (both: paragraph indents in rich text, typed indentation in plain text). The controls show
    /// the selection's formatting as it changes; the tools themselves go through RunTool.
    /// </summary>
    public partial class Form1
    {
        /// <summary>One indent step in rich text: half an inch, as Word.</summary>
        private const float IndentStepPoints = 36f;

        private bool _showingStyles;     // filling the controls: not a user's choice
        private string _lastFontName;    // for Repeat (Ctrl+R)
        private float? _lastFontSize;

        private void InitStyles()
        {
            ComboBox fonts = toolStripComboBoxFont.ComboBox;
            fonts.BeginUpdate();
            foreach (FontFamily family in FontFamily.Families.OrderBy(f => f.Name, StringComparer.CurrentCultureIgnoreCase))
                if (family.IsStyleAvailable(FontStyle.Regular)) fonts.Items.Add(family.Name);
            fonts.EndUpdate();

            // A choice from the list applies at once (the item chosen: the box's text hasn't changed
            // yet when the list says so); a typed name or size, on Enter
            fonts.SelectionChangeCommitted += (s, e) => ApplyFontChoice(fonts.SelectedItem as string);
            ComboBox sizes = toolStripComboBoxSize.ComboBox;
            sizes.SelectionChangeCommitted += (s, e) => ApplySizeChoice(sizes.SelectedItem as string);
            toolStripComboBoxFont.KeyDown += (s, e) => StyleComboKey(e, () => ApplyFontChoice(null));
            toolStripComboBoxSize.KeyDown += (s, e) => StyleComboKey(e, () => ApplySizeChoice(null));

            // Font and size on top, the heading level under the font (two rows, as the small buttons)
            // Three columns: the font list spans the first two, the size list the third; underneath,
            // the label in the first and the heading list spanning the other two, out to the size
            // list's right edge
            toolStripStyles.LayoutStyle = ToolStripLayoutStyle.Table;
            if (toolStripStyles.LayoutSettings is TableLayoutSettings rows)
            {
                rows.ColumnCount = 3; rows.RowCount = 2;
                rows.SetColumnSpan(toolStripComboBoxFont, 2);
                rows.SetColumnSpan(toolStripComboBoxHeading, 2);
            }
            toolStripStyles.AutoSize = true;
            toolStripStyles.Padding = new Padding(0, 0, 0, 2);
            foreach (ToolStripItem item in toolStripStyles.Items) item.Margin = new Padding(4, 1, 2, 1);
            SizeHeadingList();
            ComboBox headings = toolStripComboBoxHeading.ComboBox;
            headings.SelectionChangeCommitted += (s, e) =>
            {
                if (!_showingStyles && headings.SelectedIndex >= 0) RunTool("stHDG", ToolTrigger.Click, headings.SelectedIndex);
                richTextBox1.Focus();
            };

            // They show the formatting where the caret is: after any move, and on coming back to the text
            richTextBox1.SelectionSettled += (s, e) => ShowStyles();
            richTextBox1.GotFocus += (s, e) => ShowStyles();
            Shown += (s, e) => ShowStyles();

            InitSmallStyles();
        }

        /// <summary>
        /// The label as wide as its text (whatever the language), the heading list the rest of the
        /// way to the size list's right edge; at least 75 wide, so "Heading 6" fits even beside a
        /// long translation.
        /// </summary>
        private void SizeHeadingList()
        {
            ToolStripLabel label = toolStripLabelHeading;
            label.AutoSize = false;
            label.TextAlign = ContentAlignment.MiddleLeft;
            // As wide as its text, as tall as the list beside it, the text centred on the list's
            label.Size = new Size(label.GetPreferredSize(Size.Empty).Width, toolStripComboBoxHeading.Height);
            label.Margin = toolStripComboBoxHeading.Margin;
            int row = toolStripComboBoxFont.Width + toolStripComboBoxFont.Margin.Horizontal
                    + toolStripComboBoxSize.Width + toolStripComboBoxSize.Margin.Horizontal;
            int rest = row - label.Width - label.Margin.Horizontal - toolStripComboBoxHeading.Margin.Horizontal;
            toolStripComboBoxHeading.AutoSize = false;
            toolStripComboBoxHeading.Size = new Size(Math.Max(75, rest), toolStripComboBoxHeading.Height);
        }

        /************ The small buttons: two rows beside the font lists ************/

        private int _lastHeadingLevel = 1;        // for Repeat
        private int _lastTextColour = 0xC00000;   // dark red (Word's first standard colour); for the button and Repeat
        private int _lastHighlight = 0xFFFF00;    // yellow
        private const int Automatic = -1;          // a palette's "Automatic" / "No highlight"

        private void InitSmallStyles()
        {
            // Two rows of small buttons (a table layout: the designer shows them in one row)
            ToolStrip small = toolStripStylesSmall;
            small.LayoutStyle = ToolStripLayoutStyle.Table;
            if (small.LayoutSettings is TableLayoutSettings grid) { grid.ColumnCount = 8; grid.RowCount = 2; }
            small.AutoSize = true;
            // A space before Highlight, so it sits under Font colour (the bottom row has one button
            // fewer): the wide colour buttons share one column, which keeps the toolbar narrow
            if (!small.Items.ContainsKey("styleSpacer"))
                small.Items.Insert(small.Items.IndexOf(toolStripSplitButtonStHIL), new ToolStripLabel { Name = "styleSpacer", AutoSize = false, Size = new Size(22, 22) });
            // Buttons 22 square (a highlight border round 20), a pixel apart, and room under the bottom
            // row so its border isn't drawn over by the toolbar's edge: 2 × 24 + 2 = 50, inside the 58 tab page
            small.Padding = new Padding(0, 0, 0, 2);
            foreach (ToolStripItem item in small.Items)
            {
                item.AutoSize = false;
                item.Size = new Size(item is ToolStripSplitButton ? 34 : 22, 22);
                item.Margin = new Padding(1);
            }
            void Place()
            {
                toolStripStyles.Location = new Point(0, Math.Max(0, (tabStyles.ClientSize.Height - toolStripStyles.Height) / 2));
                small.Location = new Point(toolStripStyles.Right + 2, Math.Max(0, (tabStyles.ClientSize.Height - small.Height) / 2));
            }
            tabStyles.Layout += (s, e) => Place();
            small.SizeChanged += (s, e) => Place();
            Place();

            // Drawn icons for alignment (lines), and colour bars under A and ab
            SetIcon(toolStripButtonStALL, AlignIcon(0));
            SetIcon(toolStripButtonStALC, AlignIcon(1));
            SetIcon(toolStripButtonStALR, AlignIcon(2));
            SetIcon(toolStripButtonStALJ, AlignIcon(3));
            ShowColourIcons();

            // The colour buttons: click for the last colour, the arrow for a palette
            FillPalette(toolStripSplitButtonStCOL, "Automatic",
                (0x000000, "Black"), (0xC00000, "Dark red"), (0xFF0000, "Red"), (0xFFC000, "Orange"), (0xFFFF00, "Yellow"),
                (0x92D050, "Light green"), (0x00B050, "Green"), (0x00B0F0, "Light blue"), (0x0070C0, "Blue"),
                (0x002060, "Dark blue"), (0x7030A0, "Purple"), (0x808080, "Grey"));
            FillPalette(toolStripSplitButtonStHIL, "No highlight",
                (0xFFFF00, "Yellow"), (0x00FF00, "Bright green"), (0x00FFFF, "Turquoise"), (0xFF00FF, "Pink"),
                (0x0000FF, "Blue"), (0xFF0000, "Red"), (0x008080, "Teal"), (0xC0C0C0, "Grey"));
            toolStripSplitButtonStCOL.ButtonClick += (s, e) => RunTool("stCOL", ToolTrigger.Click, _lastTextColour);
            toolStripSplitButtonStHIL.ButtonClick += (s, e) => RunTool("stHIL", ToolTrigger.Click, _lastHighlight);
        }

        private static void SetIcon(ToolStripItem item, Image image)
        {
            item.Image = image;
            item.DisplayStyle = ToolStripItemDisplayStyle.Image;
            item.ImageScaling = ToolStripItemImageScaling.None;
        }

        /// <summary>Lines showing an alignment: 0 left, 1 centre, 2 right, 3 justify.</summary>
        private static Bitmap AlignIcon(int align)
        {
            var bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            using (var pen = new Pen(SystemColors.ControlText, 1.6f))
            {
                int[] lengths = { 14, 9, 12, 7 };
                for (int i = 0; i < 4; i++)
                {
                    int length = align == 3 ? 14 : lengths[i];
                    int x = align == 0 || align == 3 ? 1 : align == 1 ? (16 - length) / 2 : 15 - length;
                    int y = 3 + i * 3;
                    g.DrawLine(pen, x, y, x + length, y);
                }
            }
            return bmp;
        }

        /// <summary>The colour buttons' icons: A (font colour) or ab (highlight) over a bar of the last colour.</summary>
        private void ShowColourIcons()
        {
            SetIcon(toolStripSplitButtonStCOL, ColourIcon("A", _lastTextColour, false));
            SetIcon(toolStripSplitButtonStHIL, ColourIcon("ab", _lastHighlight, true));
        }

        private static Bitmap ColourIcon(string letters, int rgb, bool highlight)
        {
            var bmp = new Bitmap(18, 18);
            using (Graphics g = Graphics.FromImage(bmp))
            using (var font = new Font("Segoe UI", highlight ? 7.5f : 9f, FontStyle.Bold, GraphicsUnit.Point))
            using (var bar = new SolidBrush(Color.FromArgb(rgb >> 16 & 0xFF, rgb >> 8 & 0xFF, rgb & 0xFF)))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                var size = g.MeasureString(letters, font);
                g.DrawString(letters, font, SystemBrushes.ControlText, (18 - size.Width) / 2, -1);
                g.FillRectangle(bar, 1, 13, 16, 4);
            }
            return bmp;
        }

        /// <summary>A colour button's palette: the default (automatic or none), the colours with swatches, More colours….</summary>
        private void FillPalette(ToolStripSplitButton button, string none, params (int Rgb, string Name)[] colours)
        {
            string tag = (string)button.Tag;
            button.DropDownItems.Add(new ToolStripMenuItem(none, null, (s, e) => RunTool(tag, ToolTrigger.Click, Automatic)));
            button.DropDownItems.Add(new ToolStripSeparator());
            foreach (var (rgb, name) in colours)
            {
                var swatch = new Bitmap(16, 16);
                using (Graphics g = Graphics.FromImage(swatch))
                {
                    using (var brush = new SolidBrush(Color.FromArgb(rgb >> 16 & 0xFF, rgb >> 8 & 0xFF, rgb & 0xFF))) g.FillRectangle(brush, 1, 1, 14, 14);
                    g.DrawRectangle(SystemPens.ControlDark, 1, 1, 13, 13);
                }
                int chosen = rgb;
                button.DropDownItems.Add(new ToolStripMenuItem(name, swatch, (s, e) => RunTool(tag, ToolTrigger.Click, chosen)));
            }
            button.DropDownItems.Add(new ToolStripSeparator());
            button.DropDownItems.Add(new ToolStripMenuItem("More colours…", null, (s, e) =>
            {
                using var dialog = new ColorDialog { FullOpen = true, AnyColor = true };
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    RunTool(tag, ToolTrigger.Click, dialog.Color.R << 16 | dialog.Color.G << 8 | dialog.Color.B);
            }));
        }

        /// <summary>
        /// Whether Tab (Shift+Tab) indents (outdents) rather than typing a tab: when the selection
        /// covers more than one line, or the caret (the selection's start) is at the start of a
        /// line or in its leading spaces and tabs. In a live list item, that changes its level.
        /// </summary>
        private bool TabIndents()
        {
            string text = richTextBox1.Text;
            int start = Math.Min(richTextBox1.SelectionStart, text.Length);
            int end = Math.Min(start + richTextBox1.SelectionLength, text.Length);
            if (text.IndexOf('\n', start, end - start) >= 0 && end - 1 > start) return true; // several lines
            int lineStart = start == 0 ? 0 : text.LastIndexOf('\n', start - 1) + 1;
            for (int i = lineStart; i < start; i++)
                if (text[i] != ' ' && text[i] != '\t') return false;
            return true;
        }

        private void StyleComboKey(KeyEventArgs e, Action apply)
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; apply(); }
            else if (e.KeyCode == Keys.Escape) { e.SuppressKeyPress = true; ShowStyles(); richTextBox1.Focus(); }
        }

        private void ApplyFontChoice(string chosen)
        {
            if (_showingStyles) return;
            string typed = (chosen ?? toolStripComboBoxFont.Text).Trim();
            string name = toolStripComboBoxFont.ComboBox.Items.Cast<string>()
                .FirstOrDefault(n => string.Equals(n, typed, StringComparison.CurrentCultureIgnoreCase));
            if (name == null) { ShowStatus($"There is no font called '{typed}'"); return; }
            RunTool("stFNT", ToolTrigger.Click, name);
            richTextBox1.Focus();
        }

        private void ApplySizeChoice(string chosen)
        {
            if (_showingStyles) return;
            string typed = (chosen ?? toolStripComboBoxSize.Text).Trim();
            if (!float.TryParse(typed, NumberStyles.Float, CultureInfo.CurrentCulture, out float size) || size < 1 || size > 1638)
            {
                ShowStatus("A font size is a number of points, from 1 to 1638");
                return;
            }
            RunTool("stSIZ", ToolTrigger.Click, size);
            richTextBox1.Focus();
        }

        /// <summary>
        /// The controls show the selection's formatting (blank where mixed); the rich-text-only
        /// ones are off in plain text.
        /// </summary>
        private void ShowStyles()
        {
            bool rich = _isRtfMode;
            toolStripComboBoxFont.Enabled = toolStripComboBoxSize.Enabled = toolStripComboBoxHeading.Enabled = rich;
            // The small buttons: all rich text only, except the indents
            foreach (ToolStripItem item in toolStripStylesSmall.Items)
                if (item != toolStripButtonStIIN && item != toolStripButtonStDIN) item.Enabled = rich;
            ToolStripButton[] states = { toolStripButtonStBLD, toolStripButtonStITA, toolStripButtonStUND, toolStripButtonStSTR,
                toolStripButtonStSUP, toolStripButtonStSUB, toolStripButtonStALL, toolStripButtonStALC, toolStripButtonStALR, toolStripButtonStALJ };
            if (!rich || toolStripComboBoxFont.Focused || toolStripComboBoxSize.Focused) // don't overwrite typing
            {
                if (!rich)
                {
                    foreach (ToolStripButton b in states) b.Checked = false;
                    _showingStyles = true; toolStripComboBoxFont.Text = toolStripComboBoxSize.Text = ""; toolStripComboBoxHeading.SelectedIndex = -1; _showingStyles = false;
                }
                return;
            }
            var format = richTextBox1.SelectionFormat();
            var effects = richTextBox1.SelectionEffects();
            int align = richTextBox1.ParagraphAlignment();
            _showingStyles = true;
            try
            {
                toolStripComboBoxFont.Text = format.Name ?? "";
                toolStripComboBoxSize.Text = format.Size is float size ? size.ToString("0.#", CultureInfo.CurrentCulture) : "";
                toolStripButtonStBLD.Checked = format.Bold == true;
                toolStripButtonStITA.Checked = format.Italic == true;
                toolStripButtonStUND.Checked = effects.Underline == true;
                toolStripButtonStSTR.Checked = effects.Strike == true;
                toolStripButtonStSUP.Checked = effects.Super == true;
                toolStripButtonStSUB.Checked = effects.Sub == true;
                toolStripButtonStALL.Checked = align == 0;
                toolStripButtonStALC.Checked = align == 1;
                toolStripButtonStALR.Checked = align == 2;
                toolStripButtonStALJ.Checked = align == 3;
                int heading = richTextBox1.HeadingLevelAt(); // blank if the paragraphs differ
                toolStripComboBoxHeading.SelectedIndex = heading >= 0 && heading < toolStripComboBoxHeading.Items.Count ? heading : -1;
            }
            finally { _showingStyles = false; }
        }

        /// <summary>The Styles tools, from RunTool. Returns false if the tag isn't one of them.</summary>
        private bool RunStyleTool(string tag, object options, ToolTrigger how = ToolTrigger.Click)
        {
            ClipXRichTextBox box = richTextBox1;
            bool richOnly = tag is "stFNT" or "stSIZ" or "stBLD" or "stITA" or "stUND" or "stSTR" or "stSUP" or "stSUB"
                or "stCLR" or "stALL" or "stALC" or "stALR" or "stALJ" or "stCOL" or "stHIL" or "stHDG";
            if (richOnly && !_isRtfMode)
            {
                ShowStatus("Fonts and styles are for rich text: switch to rich text (bottom left) to use them");
                return true;
            }
            switch (tag)
            {
                case "stBLD": box.ToggleFontStyle(FontStyle.Bold); break;
                case "stITA": box.ToggleFontStyle(FontStyle.Italic); break;
                case "stUND": box.ToggleFontStyle(FontStyle.Underline); break;
                case "stSTR": box.ToggleFontStyle(FontStyle.Strikeout); break;
                case "stHDG": // heading level (0 = Normal): the tag Word and HTML read, and the look
                    int level = options is int chosenLevel ? chosenLevel : _lastHeadingLevel;
                    box.SetHeadingLevel(level, RichBaseFont());
                    _lastHeadingLevel = level;
                    ShowStatus(level == 0 ? "Normal paragraph" : $"Heading {level}: a heading in Word and on web pages");
                    break;
                case "stSUP": box.ToggleScript(superscript: true); break;
                case "stSUB": box.ToggleScript(superscript: false); break;
                case "stCLR":
                    if (box.SelectionLength == 0 && how == ToolTrigger.Key)
                    {
                        // Ctrl+M at the caret (keys act where you are): plain for what is typed next
                        box.ClearTypingFormat(RichBaseFont());
                        ShowStatus("Formatting cleared for what you type next");
                        break;
                    }
                    box.ClearCharacterFormatting(RichBaseFont()); // the button: the selection, or the whole text
                    ShowStatus(box.SelectionLength > 0 ? "Formatting cleared from the selection" : "Formatting cleared from the whole text");
                    break;
                case "stALL": box.SetParagraphAlignment(HorizontalAlignment.Left); break;
                case "stALC": box.SetParagraphAlignment(HorizontalAlignment.Center); break;
                case "stALR": box.SetParagraphAlignment(HorizontalAlignment.Right); break;
                case "stALJ": box.SetParagraphAlignment(null, justify: true); break;
                case "stCOL":
                case "stHIL":
                    // A palette choice, or the last colour (the button itself, Repeat)
                    bool text = tag == "stCOL";
                    int colour = options is int chosen ? chosen : text ? _lastTextColour : _lastHighlight;
                    if (colour == Automatic)
                    {
                        if (text) box.SetTextColour(null); else box.SetHighlight(null);
                        break;
                    }
                    if (text) { box.SetTextColour(colour); _lastTextColour = colour; }
                    else { box.SetHighlight(colour); _lastHighlight = colour; }
                    ShowColourIcons();
                    break;
                case "stFNT":
                    string name = options as string ?? _lastFontName;
                    if (name == null) { ShowStatus("Choose a font from the list"); break; }
                    box.SetFontName(name);
                    _lastFontName = name;
                    break;
                case "stSIZ":
                    float? size = options is float f ? f : _lastFontSize;
                    if (size == null) { ShowStatus("Choose a size from the list"); break; }
                    box.SetFontSize(size.Value);
                    _lastFontSize = size;
                    break;
                case "stIIN":
                case "stDIN":
                    bool increase = tag == "stIIN";
                    if (_isRtfMode)
                    {
                        // List items go down / up a level (bullet and number style follow); other paragraphs move
                        var (moved, anyItems) = ListTools.ChangeListLevel(box, increase ? 1 : -1, AppSettings.Current.BulletList);
                        bool indented = box.ChangeParagraphIndent(increase ? IndentStepPoints : -IndentStepPoints, skipListItems: true);
                        ShowStatus(moved > 0 ? (increase ? "List level down" : "List level up")
                            : indented ? (increase ? "Indent increased" : "Indent reduced")
                            : anyItems ? (increase ? "Already the deepest level" : "Already the top level")
                            : (increase ? "Can't indent further" : "Already at the margin"));
                    }
                    else
                    {
                        // Plain text: typed indentation, a tab or spaces as the Indent tool's
                        // settings say (its "text" option, such as "> ", isn't an indent: a tab then)
                        var saved = AppSettings.Current.Indent;
                        var o = saved.Option == "Text" ? new TextEngine.IndentOptions { Option = "Tab" } : saved;
                        // With typed lists following their new levels (bullets • ◦ ▪, numbers 1 → a → i
                        // renumbered), all as one Undo step
                        ShowStatus(FormatTools.IndentHere(box, o, increase, AppSettings.Current.BulletList, AppSettings.Current.NumList));
                    }
                    break;
                default:
                    return false;
            }
            ShowStyles();
            return true;
        }
    }
}
