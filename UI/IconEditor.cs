using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace ClipX2.UI
{
    /// <summary>
    /// The PowerButton icon editor: a 64-pixel icon (shown at 32 on the button), drawn on a
    /// magnified canvas. Pencil, eraser and fill paint pixels; line, rectangle, ellipse and text
    /// make a layer that can still be moved (drag it, the arrow buttons or the arrow keys) and
    /// resized (the + and − buttons or keys) until it is committed to the bitmap (Enter, or by
    /// drawing something else). Text keeps ClipX 1's way of making icons: one to three
    /// characters from a font, often a symbol font such as Wingdings. An uncommitted text layer is
    /// kept with the icon, so it can be adjusted again later. Images can be imported or pasted;
    /// Ctrl+Z / Ctrl+Y undo and redo.
    /// </summary>
    internal sealed class IconEditor : Form
    {
        private const int N = ButtonIcon.Size;   // icon pixels
        private const int Zoom = 8;

        private enum Tool { Pencil, Eraser, Fill, Line, Rectangle, Ellipse, Text }
        private enum ShapeKind { Line, Rectangle, Ellipse }

        /// <summary>A shape not yet committed: its kind, the two points that define it, colour, width, filled.</summary>
        private sealed class Shape
        {
            public ShapeKind Kind;
            public PointF A, B;
            public Color Colour;
            public float Width;
            public bool Filled;
            public Shape Clone() => (Shape)MemberwiseClone();
        }

        /// <summary>One state for undo: the bitmap and the layer.</summary>
        private sealed class Snapshot
        {
            public Bitmap Bitmap;
            public IconText Text;
            public Shape Shape;
        }

        private Bitmap _bitmap;
        private IconText _text;      // the text layer (uncommitted), or null
        private Shape _shape;        // a shape layer (uncommitted), or null
        private Tool _tool = Tool.Pencil;
        private Color _colour = Color.White;
        private readonly Stack<Snapshot> _undo = new Stack<Snapshot>(), _redo = new Stack<Snapshot>();

        private readonly Canvas _canvas;
        private readonly PictureBox _preview = new PictureBox();
        private readonly Panel _swatch = new Panel();
        private readonly NumericUpDown _width = new NumericUpDown { Minimum = 1, Maximum = 16, Value = 2 };
        private readonly CheckBox _filled = new CheckBox { Text = "&Filled", AutoSize = true };
        private readonly CheckBox _grid = new CheckBox { Text = "&Grid", AutoSize = true, Checked = true };
        private readonly Dictionary<Tool, RadioButton> _toolButtons = new Dictionary<Tool, RadioButton>();
        private readonly GroupBox _textBox = new GroupBox { Text = "Text && Symbols" };
        private readonly TextBox _characters = new TextBox { MaxLength = 4 };
        private readonly ComboBox _font = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown };
        private readonly CheckBox _bold = new CheckBox { Text = "B&old", AutoSize = true };
        private readonly CheckBox _italic = new CheckBox { Text = "Ita&lic", AutoSize = true };
        private readonly GroupBox _layerBox = new GroupBox { Text = "Adjust before accepting" };
        private readonly GroupBox _lineBox = new GroupBox { Text = "Lines and shapes" };
        private Button _undoButton, _redoButton;
        private readonly Label _status = new Label();   // the pointer's position, under the canvas
        private readonly Label _message = new Label();  // why something didn't happen, at the bottom left
        private readonly ToolTip _tips = new ToolTip();
        private bool _loadingText;

        // While a layer waits for Accept or Cancel, the other tools are greyed out
        private readonly List<Control> _otherTools = new List<Control>();
        private Button _commit, _drop;
        private bool _syncingTool;

        // Unsaved changes: something was done (and the icon isn't as it came)
        private readonly ButtonIcon _original;
        private bool _dirty;
        private bool _cancelClicked; // Cancel discards without asking; the close icon and Esc ask

        /// <summary>The icon as edited (after OK).</summary>
        public ButtonIcon Result { get; private set; }

        public IconEditor(ButtonIcon icon)
        {
            _original = icon.Clone();
            _bitmap = IconPainter.BitmapOf(icon);
            _text = icon.Text?.Clone();
            if (_text != null) _colour = Color.FromArgb(_text.Colour);

            Text = "PowerButton icon";
            Font = new Font("Segoe UI", 10F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1006, 616);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            KeyPreview = true;

            _canvas = new Canvas(this) { Location = new Point(196, 12), Size = new Size(N * Zoom + 2, N * Zoom + 2) };
            BuildLayout();
            LoadTextControls();
            SelectTool(_text != null ? Tool.Text : Tool.Pencil);
            Changed();
        }

        /************ Layout ************/

        private void BuildLayout()
        {
            SuspendLayout();
            // Tools, down the left
            int y = 12;
            var tools = new (Tool Tool, string Text, string Tip)[]
            {
                (Tool.Pencil, "&Pencil", "Draw pixels (width as set below)"),
                (Tool.Eraser, "&Eraser", "Rub out to transparent"),
                (Tool.Fill, "F&ill", "Fill an area of one colour"),
                (Tool.Line, "Li&ne", "A straight line: drag; adjust, then Enter"),
                (Tool.Rectangle, "&Rectangle", "Drag; adjust, then Enter"),
                (Tool.Ellipse, "Elli&pse", "Drag; adjust, then Enter"),
                (Tool.Text, "&Text && Symbols", "Characters or symbols from a font (symbol fonts make good icons); adjust, then Enter"),
            };
            foreach (var (tool, text, tip) in tools)
            {
                var r = new RadioButton
                {
                    Text = text, Appearance = Appearance.Button, Bounds = new Rectangle(12, y, 170, 32),
                    Image = ToolImage(tool), ImageAlign = ContentAlignment.MiddleLeft,
                    TextImageRelation = TextImageRelation.ImageBeforeText, TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 0, 0),
                };
                r.CheckedChanged += (s, e) => { if (r.Checked) SelectTool(tool); };
                _tips.SetToolTip(r, tip);
                _toolButtons[tool] = r;
                Controls.Add(r);
                y += 36;
            }
            // Pictures from elsewhere, under the drawing tools
            y += 2;
            var import = new Button { Text = "I&mport image…", Bounds = new Rectangle(12, y, 170, 30) };
            import.Click += (s, e) => ImportImage();
            _tips.SetToolTip(import, "A picture from a file (PNG, ICO, BMP, GIF, JPEG, TIFF), fitted to the icon");
            var paste = new Button { Text = "Paste ima&ge", Bounds = new Rectangle(12, y + 34, 170, 30) };
            paste.Click += (s, e) => PasteImage();
            _tips.SetToolTip(paste, "Paste a picture from the clipboard (Ctrl+V)");
            Controls.Add(import);
            Controls.Add(paste);
            _otherTools.Add(import);
            _otherTools.Add(paste);
            y += 76;

            // Colours, with Set Background under them: the palette's bottom level with the canvas's,
            // Set Background beside Grid (the canvas is placed before the layout is built)
            y = _canvas.Bottom - (4 * 26 + 22) - 32;
            Controls.Add(new Label { Text = "Colour:", AutoSize = true, Location = new Point(12, y + 2) });
            _swatch.Bounds = new Rectangle(80, y, 102, 26);
            _swatch.BorderStyle = BorderStyle.FixedSingle;
            _swatch.Cursor = Cursors.Hand;
            _swatch.Click += (s, e) => MoreColours();
            _tips.SetToolTip(_swatch, "Click for more colours");
            Controls.Add(_swatch);
            y += 32;
            Color[] palette =
            {
                Color.White, Color.Black, Color.FromArgb(128, 128, 128), Color.FromArgb(192, 192, 192),
                Color.FromArgb(1, 99, 49), Color.FromArgb(0, 158, 96), Color.FromArgb(31, 47, 143), Color.FromArgb(0, 120, 215),
                Color.FromArgb(0, 183, 195), Color.FromArgb(107, 105, 214), Color.FromArgb(135, 100, 184), Color.FromArgb(194, 57, 179),
                Color.FromArgb(232, 17, 35), Color.FromArgb(202, 80, 16), Color.FromArgb(255, 140, 0), Color.FromArgb(255, 185, 0),
                Color.FromArgb(255, 241, 118), Color.FromArgb(142, 86, 46), Color.FromArgb(73, 130, 5), Color.FromArgb(16, 124, 16),
            };
            for (int i = 0; i < palette.Length; i++)
            {
                Color c = palette[i];
                var p = new Panel { BackColor = c, BorderStyle = BorderStyle.FixedSingle, Bounds = new Rectangle(12 + (i % 4) * 43, y + (i / 4) * 26, 39, 22), Cursor = Cursors.Hand };
                p.Click += (s, e) => SetColour(c);
                Controls.Add(p);
            }
            var background = new Button { Text = "Set &Background", Bounds = new Rectangle(12, _canvas.Bottom + 3, 170, 30) };
            background.Click += (s, e) => FillBackground();
            _tips.SetToolTip(background, "Fill behind everything with the colour, as a rounded square");
            Controls.Add(background);
            _otherTools.Add(background);

            // The canvas, with Grid and the pointer's position under it
            Controls.Add(_canvas);
            int under = _canvas.Bottom + 6;
            var sizeLabel = new Label
            {
                Text = $"{N} × {N} px", AutoSize = false, ForeColor = SystemColors.GrayText,
                TextAlign = ContentAlignment.MiddleLeft, Bounds = new Rectangle(_canvas.Left, under, 140, 24),
            };
            _tips.SetToolTip(sizeLabel, "Drawn at 64 × 64 and shown at 32 × 32 on the button: the extra detail keeps it sharp when Windows scales the display up (150 %, 200 %)");
            Controls.Add(sizeLabel);
            _grid.AutoSize = true;
            Controls.Add(_grid);
            _grid.Location = new Point(_canvas.Left + (_canvas.Width - _grid.PreferredSize.Width) / 2, under + 2);
            _grid.CheckedChanged += (s, e) => _canvas.Invalidate();
            _status.SetBounds(_canvas.Right - 140, under, 140, 24);
            _status.ForeColor = SystemColors.GrayText;
            _status.TextAlign = ContentAlignment.MiddleRight;
            Controls.Add(_status);

            // Right: the previews with Clear all, the layer's adjustments, text
            int x = _canvas.Right + 16;
            const int RightWidth = 262; // the right-hand groups' width
            Controls.Add(new Label { Text = "On the button:", AutoSize = true, Location = new Point(x, 12) });
            _preview.Bounds = new Rectangle(x, 34, 120, 60);
            Controls.Add(_preview);
            var clear = new Button { Text = "Clea&r all", Bounds = new Rectangle(x + RightWidth - 108, 49, 108, 30) };
            clear.Click += (s, e) => ClearAll();
            _tips.SetToolTip(clear, "Start again with an empty icon (Ctrl+Z brings it back)");
            Controls.Add(clear);
            _otherTools.Add(clear);

            // The layer: nudge, resize, accept or cancel
            // The three groups spaced evenly between the previews and the canvas's bottom, Text's bottom level with it
            const int LayerHeight = 130, LineHeight = 66, TextHeight = 168;
            int textTop = _canvas.Bottom - TextHeight;
            int gap = (textTop - _preview.Bottom - LayerHeight - LineHeight) / 3;
            _layerBox.Bounds = new Rectangle(x, _preview.Bottom + gap, RightWidth, LayerHeight);
            Button Small(string text, int bx, int by, string tip, Action click)
            {
                var b = new Button { Text = text, Bounds = new Rectangle(bx, by, 34, 30) };
                b.Click += (s, e) => click();
                _tips.SetToolTip(b, tip);
                _layerBox.Controls.Add(b);
                return b;
            }
            Small("▲", 48, 24, "Up a pixel (↑)", () => Nudge(0, -1));
            Small("◀", 12, 56, "Left a pixel (←)", () => Nudge(-1, 0));
            Small("▶", 84, 56, "Right a pixel (→)", () => Nudge(1, 0));
            Small("▼", 48, 88, "Down a pixel (↓)", () => Nudge(0, 1));
            Small("+", 136, 24, "Bigger (+)", () => ResizeLayer(1));
            Small("−", 136, 56, "Smaller (−)", () => ResizeLayer(-1));
            Small("◎", 136, 88, "Centre it", CentreLayer);
            var commit = _commit = new Button { Text = "&Accept", Bounds = new Rectangle(188, 24, 62, 46) };
            commit.Click += (s, e) => Commit();
            _tips.SetToolTip(commit, "Draw it into the icon (Enter)");
            var discard = _drop = new Button { Text = "&Cancel", Bounds = new Rectangle(188, 72, 62, 46) };
            discard.Click += (s, e) => Discard();
            _tips.SetToolTip(discard, "Take it away (Esc)");
            _layerBox.Controls.Add(commit);
            _layerBox.Controls.Add(discard);
            Controls.Add(_layerBox);

            // Lines and shapes: line width, filled (only for the tools that draw lines)
            _lineBox.Bounds = new Rectangle(x, _layerBox.Bottom + gap, RightWidth, LineHeight);
            _lineBox.Controls.Add(new Label { Text = "Line &width:", AutoSize = true, Location = new Point(10, 30) });
            _width.Bounds = new Rectangle(92, 26, 56, 28);
            _width.ValueChanged += (s, e) => { if (_shape != null) { _shape.Width = (float)_width.Value; Changed(); } };
            _lineBox.Controls.Add(_width);
            _filled.Location = new Point(172, 29);
            _filled.CheckedChanged += (s, e) => { if (_shape != null && _shape.Kind != ShapeKind.Line) { _shape.Filled = _filled.Checked; Changed(); } };
            _lineBox.Controls.Add(_filled);
            Controls.Add(_lineBox);

            // Text: characters, font (only with the Text tool)
            _textBox.Bounds = new Rectangle(x, textTop, RightWidth, TextHeight);
            _textBox.Controls.Add(new Label { Text = "C&haracters:", AutoSize = true, Location = new Point(10, 28) });
            _characters.Bounds = new Rectangle(100, 24, 102, 28);
            _characters.TextChanged += (s, e) => TextSettingsChanged(recentre: true);
            TextBoxKeys.Attach(_characters); // Ctrl+L, Ctrl+Shift+L, Ctrl+S, Ctrl+T, as in the editor
            var pick = new Button { Text = "…", Bounds = new Rectangle(208, 23, 42, 30) };
            pick.Click += (s, e) => PickCharacter();
            _tips.SetToolTip(pick, "Choose a character from the font");
            _textBox.Controls.Add(_characters);
            _textBox.Controls.Add(pick);
            _textBox.Controls.Add(new Label { Text = "F&ont:", AutoSize = true, Location = new Point(10, 66) });
            _font.Bounds = new Rectangle(60, 62, 190, 28);
            foreach (string f in IconFonts.ForList()) _font.Items.Add(f); // installed ones: symbol fonts, letters, the user's own
            _font.TextChanged += (s, e) => TextSettingsChanged(recentre: true);
            _font.Leave += (s, e) => RememberFont(_font.Text);
            _textBox.Controls.Add(_font);
            _bold.Location = new Point(60, 98);
            _italic.Location = new Point(130, 98);
            _bold.CheckedChanged += (s, e) => TextSettingsChanged(recentre: false);
            _italic.CheckedChanged += (s, e) => TextSettingsChanged(recentre: false);
            _textBox.Controls.Add(_bold);
            _textBox.Controls.Add(_italic);
            var fontDialog = new Button { Text = "Font…", Bounds = new Rectangle(60, 128, 90, 30) };
            fontDialog.Click += (s, e) => ChooseFont();
            _textBox.Controls.Add(fontDialog);
            Controls.Add(_textBox);

            // Bottom row: a message at the left; < Undo, Redo >, a little apart from OK and Cancel
            int bottom = ClientSize.Height - 44;
            _message.SetBounds(12, bottom, ClientSize.Width - 444 - 24, 32);
            _message.ForeColor = Color.Firebrick;
            _message.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(_message);
            _undoButton = new Button { Text = "< &Undo", Bounds = new Rectangle(ClientSize.Width - 444, bottom, 96, 32) };
            _undoButton.Click += (s, e) => Undo();
            _redoButton = new Button { Text = "Re&do >", Bounds = new Rectangle(ClientSize.Width - 342, bottom, 96, 32) };
            _redoButton.Click += (s, e) => Redo();
            _tips.SetToolTip(_undoButton, "Ctrl+Z");
            _tips.SetToolTip(_redoButton, "Ctrl+Y");
            Controls.Add(_undoButton);
            Controls.Add(_redoButton);

            var ok = new Button { Text = "OK", Bounds = new Rectangle(ClientSize.Width - 212, bottom, 96, 32) };
            ok.Click += (s, e) => Ok();
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Bounds = new Rectangle(ClientSize.Width - 108, bottom, 96, 32) };
            cancel.Click += (s, e) => _cancelClicked = true;
            Controls.Add(ok);
            Controls.Add(cancel);
            CancelButton = null; // Esc drops the layer first (handled in ProcessCmdKey)
            ResumeLayout(true);
        }

        /// <summary>A small picture of a tool for its button (20 pixels; greyed with the button when it is).</summary>
        private static Bitmap ToolImage(Tool tool)
        {
            const int S = 20;
            var b = new Bitmap(S, S, PixelFormat.Format32bppArgb);
            using Graphics g = Graphics.FromImage(b);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color ink = Color.FromArgb(50, 50, 50);
            using var pen = new Pen(ink, 1.6f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
            using var inkBrush = new SolidBrush(ink);
            using var thick = new Pen(ink, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            switch (tool)
            {
                case Tool.Pencil:
                {
                    // a pencil from top right to bottom left: yellow body, wood tip, dark point
                    PointF[] body = { new PointF(13, 2.5f), new PointF(17.5f, 7), new PointF(8, 16.5f), new PointF(3.5f, 12) };
                    using (var yellow = new SolidBrush(Color.FromArgb(255, 190, 40))) g.FillPolygon(yellow, body);
                    PointF[] tip = { new PointF(3.5f, 12), new PointF(8, 16.5f), new PointF(2, 18) };
                    using (var wood = new SolidBrush(Color.FromArgb(240, 210, 160))) g.FillPolygon(wood, tip);
                    g.FillPolygon(inkBrush, new[] { new PointF(2.6f, 15.6f), new PointF(4.4f, 17.4f), new PointF(2, 18) });
                    g.DrawPolygon(pen, new[] { new PointF(13, 2.5f), new PointF(17.5f, 7), new PointF(8, 16.5f), new PointF(2, 18), new PointF(3.5f, 12) });
                    break;
                }
                case Tool.Eraser:
                {
                    // a tilted block: pink end, blue sleeve
                    var state = g.Save();
                    g.TranslateTransform(10, 10);
                    g.RotateTransform(-40);
                    using (var pink = new SolidBrush(Color.FromArgb(240, 120, 140))) g.FillRectangle(pink, -8, -4, 7, 8);
                    using (var blue = new SolidBrush(Color.FromArgb(70, 120, 200))) g.FillRectangle(blue, -1, -4, 9, 8);
                    g.DrawRectangle(pen, -8, -4, 16, 8);
                    g.Restore(state);
                    break;
                }
                case Tool.Fill:
                {
                    // a tipped bucket, paint pouring from it
                    var state = g.Save();
                    g.TranslateTransform(8, 10);
                    g.RotateTransform(-35);
                    using (var grey = new SolidBrush(Color.FromArgb(215, 215, 215))) g.FillRectangle(grey, -5, -5, 10, 10);
                    g.DrawRectangle(pen, -5, -5, 10, 10);
                    g.DrawArc(pen, -4, -10, 8, 8, 180, 180);
                    g.Restore(state);
                    using (var paint = new SolidBrush(Color.FromArgb(0, 120, 215)))
                    {
                        g.FillEllipse(paint, 13.5f, 11, 4.5f, 6);
                        g.FillPolygon(paint, new[] { new PointF(14, 13), new PointF(17.5f, 13), new PointF(15.7f, 8.5f) });
                    }
                    break;
                }
                case Tool.Line:
                    g.DrawLine(thick, 3, 17, 17, 3);
                    break;
                case Tool.Rectangle:
                    g.DrawRectangle(thick, 2.5f, 4.5f, 15, 11);
                    break;
                case Tool.Ellipse:
                    g.DrawEllipse(thick, 2, 4, 16, 12);
                    break;
                case Tool.Text:
                    using (var font = new Font("Georgia", 19f, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (var centre = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        g.DrawString("A", font, inkBrush, new RectangleF(0, 2.5f, S, S), centre);
                    }
                    break;
            }
            return b;
        }

        /************ Tools and colour ************/

        private void SelectTool(Tool tool)
        {
            if (_syncingTool) return;
            _tool = tool;
            if (!_toolButtons[tool].Checked) _toolButtons[tool].Checked = true;
            if (tool == Tool.Text && _text == null)
            {
                if (_shape != null) Commit();
                Snap();
                _text = new IconText
                {
                    Characters = _characters.Text.Length > 0 ? _characters.Text : "A",
                    FontName = _font.Text.Length > 0 ? _font.Text : "Segoe UI",
                    Bold = _bold.Checked, Italic = _italic.Checked,
                    Size = 40,
                    Colour = _colour.ToArgb(),
                };
                IconPainter.Centre(_text);
                LoadTextControls();
            }
            _canvas.Cursor = tool == Tool.Text ? Cursors.SizeAll : tool == Tool.Fill ? Cursors.Hand : Cursors.Cross;
            Changed();
        }

        private void SetColour(Color c)
        {
            _colour = c;
            // The layer being adjusted takes the colour too
            if (_text != null && (_tool == Tool.Text || _shape == null)) { Snap(); _text.Colour = c.ToArgb(); }
            else if (_shape != null) { Snap(); _shape.Colour = c; }
            Changed();
        }

        private void MoreColours()
        {
            using var dialog = new ColorDialog { FullOpen = true, Color = _colour };
            if (dialog.ShowDialog(this) == DialogResult.OK) SetColour(dialog.Color);
        }

        /************ Text ************/

        private void LoadTextControls()
        {
            _loadingText = true;
            IconText t = _text ?? new IconText();
            _characters.Text = t.Characters;
            _font.Text = t.FontName;
            _bold.Checked = t.Bold;
            _italic.Checked = t.Italic;
            _characters.Font = new Font(PickFamily(t.FontName), 14F);
            _loadingText = false;
        }

        private static FontFamily PickFamily(string name)
        {
            try { return new FontFamily(name); }
            catch (ArgumentException) { return FontFamily.GenericSansSerif; }
        }

        /// <summary>The characters or font changed: the text layer follows (re-centred on new characters, as ClipX 1).</summary>
        private void TextSettingsChanged(bool recentre)
        {
            if (_loadingText) return;
            if (_text == null) { SelectTool(Tool.Text); return; }
            if (_characters.Text.Length == 0) return;
            Snap(merge: true);
            _text.Characters = _characters.Text;
            _text.FontName = _font.Text.Trim().Length > 0 ? _font.Text.Trim() : "Segoe UI";
            _text.Bold = _bold.Checked;
            _text.Italic = _italic.Checked;
            try { _characters.Font = new Font(PickFamily(_text.FontName), 14F); } catch (ArgumentException) { }
            if (recentre) IconPainter.Centre(_text);
            Changed();
            ShowFontInstalled(); // after Changed, which clears the message
        }

        private void ChooseFont()
        {
            IconText t = _text ?? new IconText();
            using var dialog = new FontDialog { ShowEffects = false, FontMustExist = true };
            try { dialog.Font = new Font(t.FontName, 16F, (t.Bold ? FontStyle.Bold : 0) | (t.Italic ? FontStyle.Italic : 0)); } catch (ArgumentException) { }
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            _loadingText = true;
            _font.Text = dialog.Font.Name;
            _bold.Checked = dialog.Font.Bold;
            _italic.Checked = dialog.Font.Italic;
            _loadingText = false;
            RememberFont(dialog.Font.Name);
            TextSettingsChanged(recentre: true);
        }

        /// <summary>A font typed or chosen in the Font dialog joins the list (kept in the settings), if installed.</summary>
        private void RememberFont(string name)
        {
            if (!IconFonts.Remember(name)) return;
            string canonical = IconFonts.Canonical(name);
            if (!_font.Items.Contains(canonical)) _font.Items.Add(canonical);
        }

        /// <summary>A font name that isn't installed shows in red, with why (the text is drawn in a plain font instead).</summary>
        private void ShowFontInstalled()
        {
            string name = _font.Text.Trim();
            bool missing = name.Length > 0 && !IconFonts.IsInstalled(name);
            _font.ForeColor = missing ? Color.Firebrick : SystemColors.WindowText;
            if (missing) _message.Text = $"“{name}” isn't installed on this computer: the text is drawn in a plain font instead";
        }

        /// <summary>Every character the font has, in a scrolling grid (<see cref="CharacterPicker"/>): click one to use it.</summary>
        private void PickCharacter()
        {
            string fontName = _font.Text.Trim().Length > 0 ? _font.Text.Trim() : "Segoe UI";
            if (!IconFonts.IsInstalled(fontName)) { _message.Text = $"“{fontName}” isn't installed: choose another font first"; return; }
            using var picker = new CharacterPicker(IconFonts.Canonical(fontName), _characters.Text);
            if (picker.ShowDialog(this) != DialogResult.OK || picker.Chosen == null) return;
            if (_text == null) SelectTool(Tool.Text);
            _characters.Text = picker.Chosen;
        }

        /************ The layer: nudge, resize, commit ************/

        private void Nudge(int dx, int dy)
        {
            if (_text == null && _shape == null) { _message.Text = "Nothing to move: draw a line, rectangle, ellipse or text first"; return; }
            Snap(merge: true);
            if (_shape != null) { _shape.A = new PointF(_shape.A.X + dx, _shape.A.Y + dy); _shape.B = new PointF(_shape.B.X + dx, _shape.B.Y + dy); }
            else { _text.X += dx; _text.Y += dy; }
            Changed();
        }

        private void ResizeLayer(int by)
        {
            if (_text == null && _shape == null) { _message.Text = "Nothing to resize: draw a line, rectangle, ellipse or text first"; return; }
            Snap(merge: true);
            if (_shape != null)
            {
                // Grows from its centre, a pixel each side
                float sx = Math.Sign(_shape.B.X - _shape.A.X), sy = Math.Sign(_shape.B.Y - _shape.A.Y);
                if (_shape.Kind == ShapeKind.Line)
                {
                    double len = Math.Sqrt(Math.Pow(_shape.B.X - _shape.A.X, 2) + Math.Pow(_shape.B.Y - _shape.A.Y, 2));
                    if (len < 1 && by < 0) return;
                    float ux = len == 0 ? 1 : (float)((_shape.B.X - _shape.A.X) / len), uy = len == 0 ? 0 : (float)((_shape.B.Y - _shape.A.Y) / len);
                    _shape.A = new PointF(_shape.A.X - ux * by, _shape.A.Y - uy * by);
                    _shape.B = new PointF(_shape.B.X + ux * by, _shape.B.Y + uy * by);
                }
                else
                {
                    if (sx == 0) sx = 1;
                    if (sy == 0) sy = 1;
                    if (by < 0 && (Math.Abs(_shape.B.X - _shape.A.X) <= 2 || Math.Abs(_shape.B.Y - _shape.A.Y) <= 2)) return;
                    _shape.A = new PointF(_shape.A.X - sx * by, _shape.A.Y - sy * by);
                    _shape.B = new PointF(_shape.B.X + sx * by, _shape.B.Y + sy * by);
                }
            }
            else
            {
                // Grows about its centre
                SizeF before = IconPainter.Measure(_text);
                float size = Math.Max(4, _text.Size + by * 2);
                if (size == _text.Size) return;
                _text.Size = size;
                SizeF after = IconPainter.Measure(_text);
                _text.X -= (after.Width - before.Width) / 2;
                _text.Y -= (after.Height - before.Height) / 2;
            }
            Changed();
        }

        private void CentreLayer()
        {
            if (_text == null && _shape == null) return;
            Snap();
            if (_shape != null)
            {
                RectangleF r = ShapeBounds(_shape);
                float dx = (N - r.Width) / 2 - r.Left, dy = (N - r.Height) / 2 - r.Top;
                _shape.A = new PointF(_shape.A.X + dx, _shape.A.Y + dy);
                _shape.B = new PointF(_shape.B.X + dx, _shape.B.Y + dy);
            }
            else IconPainter.Centre(_text);
            Changed();
        }

        /// <summary>Draws the layer into the bitmap (the shape first if both).</summary>
        private void Commit()
        {
            if (_text == null && _shape == null) return;
            Snap();
            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                if (_shape != null) DrawShape(g, _shape, 1f);
                else IconPainter.DrawText(g, _text, 1f);
            }
            if (_shape != null) _shape = null; else _text = null;
            Changed();
        }

        private void Discard()
        {
            if (_text == null && _shape == null) return;
            Snap();
            if (_shape != null) _shape = null; else _text = null;
            Changed();
        }

        private static RectangleF ShapeBounds(Shape s) =>
            RectangleF.FromLTRB(Math.Min(s.A.X, s.B.X), Math.Min(s.A.Y, s.B.Y), Math.Max(s.A.X, s.B.X), Math.Max(s.A.Y, s.B.Y));

        private static void DrawShape(Graphics g, Shape s, float scale)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            using var pen = new Pen(s.Colour, s.Width * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            using var brush = new SolidBrush(s.Colour);
            RectangleF r = ShapeBounds(s);
            r = new RectangleF(r.X * scale, r.Y * scale, r.Width * scale, r.Height * scale);
            switch (s.Kind)
            {
                case ShapeKind.Line: g.DrawLine(pen, s.A.X * scale, s.A.Y * scale, s.B.X * scale, s.B.Y * scale); break;
                case ShapeKind.Rectangle:
                    if (s.Filled) g.FillRectangle(brush, r); else g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
                    break;
                case ShapeKind.Ellipse:
                    if (s.Filled) g.FillEllipse(brush, r); else g.DrawEllipse(pen, r);
                    break;
            }
        }

        /************ Background, images ************/

        private void FillBackground()
        {
            Commit();
            Snap();
            var b = new Bitmap(N, N, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(_colour);
                using GraphicsPath path = IconPainter.RoundedSquare(new RectangleF(0, 0, N, N), N / 8f);
                g.FillPath(brush, path);
                g.DrawImage(_bitmap, 0, 0, N, N);
            }
            _bitmap.Dispose();
            _bitmap = b;
            Changed();
        }

        private void ClearAll()
        {
            Snap();
            _bitmap.Dispose();
            _bitmap = new Bitmap(N, N, PixelFormat.Format32bppArgb);
            _text = null;
            _shape = null;
            Changed();
        }

        private void ImportImage()
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Import an image for the icon",
                Filter = "Images|*.png;*.ico;*.bmp;*.gif;*.jpg;*.jpeg;*.tif;*.tiff|All files|*.*",
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            Bitmap image = LoadImageFile(dialog.FileName, out string why);
            if (image == null)
            {
                MessageBox.Show(this, "That file couldn't be read as an image.\r\n\r\n" + why, "Import image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (image) PutImage(image);
        }

        /// <summary>
        /// A picture from a file (an .ico at the icon's size), or null and why not. Anything GDI+
        /// can't read as a picture (a text file, a damaged image) comes back null: GDI+ says so in
        /// several ways (OutOfMemoryException, ExternalException, ArgumentException…), so every
        /// error but the fatal ones is caught.
        /// </summary>
        internal static Bitmap LoadImageFile(string path, out string why)
        {
            why = null;
            try
            {
                if (path.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
                    using (var ico = new System.Drawing.Icon(path, N, N)) return ico.ToBitmap();
                using (var loaded = Image.FromFile(path)) return new Bitmap(loaded);
            }
            catch (Exception ex) when (!(ex is StackOverflowException || ex is AccessViolationException))
            {
                why = ex is OutOfMemoryException || ex is System.Runtime.InteropServices.ExternalException || ex is ArgumentException
                    ? "It isn't a picture in a format Windows can read (PNG, ICO, BMP, GIF, JPEG, TIFF)."
                    : ex.Message;
                return null;
            }
        }

        private void PasteImage()
        {
            Image image = null;
            try { if (Clipboard.ContainsImage()) image = Clipboard.GetImage(); }
            catch (Exception ex) when (!(ex is StackOverflowException || ex is AccessViolationException)) { image = null; } // an odd format on the clipboard
            if (image == null) { _message.Text = "There's no picture on the clipboard"; return; }
            using (image) PutImage(image);
        }

        /// <summary>An image over the icon, scaled to fit and centred (its proportions kept).</summary>
        private void PutImage(Image image)
        {
            Commit();
            Snap();
            float scale = Math.Min((float)N / image.Width, (float)N / image.Height);
            float w = image.Width * scale, h = image.Height * scale;
            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(image, (N - w) / 2, (N - h) / 2, w, h);
            }
            Changed();
        }

        /************ Painting with the mouse ************/

        private Point _last;
        private bool _drawing, _movingLayer;
        private PointF _moveFrom;

        private static Point ToIcon(Point canvas) => new Point(Math.Max(-1, Math.Min(N, (canvas.X - 1) / Zoom)), Math.Max(-1, Math.Min(N, (canvas.Y - 1) / Zoom)));
        private static PointF ToIconF(Point canvas) => new PointF((canvas.X - 1) / (float)Zoom, (canvas.Y - 1) / (float)Zoom);

        private RectangleF LayerBounds()
        {
            if (_shape != null) { RectangleF r = ShapeBounds(_shape); r.Inflate(_shape.Width / 2 + 1, _shape.Width / 2 + 1); return r; }
            if (_text != null) { SizeF s = IconPainter.Measure(_text); return new RectangleF(_text.X, _text.Y, s.Width, s.Height); }
            return RectangleF.Empty;
        }

        private void CanvasDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            PointF pf = ToIconF(e.Location);
            // On the layer (or anywhere, with the text tool): drag moves it
            if ((_text != null || _shape != null) && (LayerBounds().Contains(pf) || (_tool == Tool.Text && _text != null)))
            {
                Snap();
                _movingLayer = true;
                _moveFrom = pf;
                return;
            }
            Point p = ToIcon(e.Location);
            switch (_tool)
            {
                case Tool.Pencil:
                case Tool.Eraser:
                    Commit();
                    Snap();
                    _drawing = true;
                    _last = p;
                    PaintLine(p, p);
                    break;
                case Tool.Fill:
                    Commit();
                    Snap();
                    FloodFill(p.X, p.Y, _colour);
                    Changed();
                    break;
                case Tool.Line:
                case Tool.Rectangle:
                case Tool.Ellipse:
                    Commit();
                    Snap();
                    var point = new PointF((float)Math.Round(pf.X), (float)Math.Round(pf.Y));
                    _shape = new Shape
                    {
                        Kind = _tool == Tool.Line ? ShapeKind.Line : _tool == Tool.Rectangle ? ShapeKind.Rectangle : ShapeKind.Ellipse,
                        A = point, B = point, Colour = _colour, Width = (float)_width.Value, Filled = _filled.Checked && _tool != Tool.Line,
                    };
                    _drawing = true;
                    Changed();
                    break;
            }
        }

        private void CanvasMove(MouseEventArgs e)
        {
            Point p = ToIcon(e.Location);
            _status.Text = p.X >= 0 && p.X < N && p.Y >= 0 && p.Y < N ? $"x {p.X}, y {p.Y}" : "";
            if (_movingLayer)
            {
                PointF pf = ToIconF(e.Location);
                float dx = (float)Math.Round(pf.X - _moveFrom.X), dy = (float)Math.Round(pf.Y - _moveFrom.Y);
                if (dx == 0 && dy == 0) return;
                _moveFrom = new PointF(_moveFrom.X + dx, _moveFrom.Y + dy);
                if (_shape != null) { _shape.A = new PointF(_shape.A.X + dx, _shape.A.Y + dy); _shape.B = new PointF(_shape.B.X + dx, _shape.B.Y + dy); }
                else if (_text != null) { _text.X += dx; _text.Y += dy; }
                Changed();
                return;
            }
            if (!_drawing) return;
            if (_shape != null)
            {
                PointF pf = ToIconF(e.Location);
                _shape.B = new PointF((float)Math.Round(pf.X), (float)Math.Round(pf.Y));
                Changed();
                return;
            }
            if (p == _last) return;
            PaintLine(_last, p);
            _last = p;
        }

        private void CanvasUp(MouseEventArgs e)
        {
            _drawing = false;
            _movingLayer = false;
            if (_shape != null && _shape.A == _shape.B) { _shape = null; Changed(); } // a click, not a drag
        }

        /// <summary>Pencil or eraser from one pixel to another, crisp (no smoothing).</summary>
        private void PaintLine(Point from, Point to)
        {
            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                bool erase = _tool == Tool.Eraser;
                g.CompositingMode = erase ? CompositingMode.SourceCopy : CompositingMode.SourceOver;
                Color c = erase ? Color.Transparent : _colour;
                int w = (int)_width.Value;
                using var brush = new SolidBrush(c);
                // Squares along the line: exact pixels at any width
                int steps = Math.Max(Math.Abs(to.X - from.X), Math.Abs(to.Y - from.Y));
                for (int i = 0; i <= steps; i++)
                {
                    float t = steps == 0 ? 0 : i / (float)steps;
                    int x = (int)Math.Round(from.X + (to.X - from.X) * t), y = (int)Math.Round(from.Y + (to.Y - from.Y) * t);
                    g.FillRectangle(brush, x - (w - 1) / 2, y - (w - 1) / 2, w, w);
                }
            }
            Changed();
        }

        private void FloodFill(int x, int y, Color colour)
        {
            if (x < 0 || y < 0 || x >= N || y >= N) return;
            int target = _bitmap.GetPixel(x, y).ToArgb(), with = colour.ToArgb();
            if (target == with) return;
            var stack = new Stack<Point>();
            stack.Push(new Point(x, y));
            while (stack.Count > 0)
            {
                Point p = stack.Pop();
                if (p.X < 0 || p.Y < 0 || p.X >= N || p.Y >= N) continue;
                if (_bitmap.GetPixel(p.X, p.Y).ToArgb() != target) continue;
                _bitmap.SetPixel(p.X, p.Y, colour);
                stack.Push(new Point(p.X + 1, p.Y));
                stack.Push(new Point(p.X - 1, p.Y));
                stack.Push(new Point(p.X, p.Y + 1));
                stack.Push(new Point(p.X, p.Y - 1));
            }
        }

        /************ Undo ************/

        private DateTime _lastSnap;

        /// <summary>Remembers the state before a change. <paramref name="merge"/>: quick repeats (nudges, typing) make one Undo step.</summary>
        private void Snap(bool merge = false)
        {
            if (merge && _undo.Count > 0 && (DateTime.Now - _lastSnap).TotalMilliseconds < 800) { _lastSnap = DateTime.Now; return; }
            _undo.Push(Current());
            _redo.Clear();
            _lastSnap = DateTime.Now;
            _dirty = true;
            UpdateUndoButtons();
        }

        private Snapshot Current() => new Snapshot { Bitmap = new Bitmap(_bitmap), Text = _text?.Clone(), Shape = _shape?.Clone() };

        private void Restore(Snapshot s)
        {
            _bitmap.Dispose();
            _bitmap = s.Bitmap;
            _text = s.Text;
            _shape = s.Shape;
            LoadTextControls();
            Changed();
        }

        /// <summary>Undo and Redo greyed out when there is nothing to undo or redo.</summary>
        private void UpdateUndoButtons()
        {
            if (_undoButton == null) return;
            _undoButton.Enabled = _undo.Count > 0;
            _redoButton.Enabled = _redo.Count > 0;
        }

        private void Undo()
        {
            if (_undo.Count == 0) return;
            _redo.Push(Current());
            Restore(_undo.Pop());
        }

        private void Redo()
        {
            if (_redo.Count == 0) return;
            _undo.Push(Current());
            Restore(_redo.Pop());
        }

        /************ Showing ************/

        /// <summary>The icon as it stands: the bitmap with the layer on it (for the canvas and preview).</summary>
        private Bitmap Composed()
        {
            var b = new Bitmap(_bitmap);
            using Graphics g = Graphics.FromImage(b);
            if (_text != null) IconPainter.DrawText(g, _text, 1f);
            if (_shape != null) DrawShape(g, _shape, 1f);
            return b;
        }

        private void Changed()
        {
            _message.Text = ""; // something happened: an earlier "nothing to…" no longer applies
            bool layer = _text != null || _shape != null;
            foreach (Control c in _layerBox.Controls) c.Enabled = layer;
            LockTools();
            _layerBox.Text = _shape != null ? $"Adjust the {_shape.Kind.ToString().ToLowerInvariant()}, then accept"
                : _text != null ? "Adjust the text, then accept" : "Adjust before accepting";
            UpdateUndoButtons();
            _swatch.BackColor = _colour;
            _canvas.Invalidate();
            ShowPreview();
        }

        /// <summary>The tool the layer belongs to (null if there is no layer).</summary>
        private Tool? LayerTool() =>
            _shape != null ? (_shape.Kind == ShapeKind.Line ? Tool.Line : _shape.Kind == ShapeKind.Rectangle ? Tool.Rectangle : Tool.Ellipse)
            : _text != null ? Tool.Text : (Tool?)null;

        private static readonly Color LockedBack = Color.FromArgb(210, 210, 210);
        private static readonly Color WaitingBack = Color.FromArgb(255, 160, 40); // as a PowerButton's "Go"

        /// <summary>
        /// While a layer (text or a shape) waits for Accept or Cancel, only its own tool, the colours
        /// and the adjustments work: the other tools are greyed out, so nothing else takes the
        /// layer by surprise (a colour meant for a rectangle colouring the text). Accept and Cancel
        /// show in orange.
        /// </summary>
        private void LockTools()
        {
            Tool? own = LayerTool();
            bool locked = own != null;
            if (locked && _tool != own.Value && !_syncingTool)
            {
                _syncingTool = true; // an undo brought back a layer of another tool: that tool
                _tool = own.Value;
                _toolButtons[own.Value].Checked = true;
                _syncingTool = false;
            }
            foreach (var pair in _toolButtons)
                Lock(pair.Value, locked && pair.Key != own);
            foreach (Control c in _otherTools) Lock(c, locked);
            // The settings that go with the tool chosen: text for Text, line width and Filled for
            // the tools that draw lines (Filled only for rectangles and ellipses)
            _textBox.Enabled = _tool == Tool.Text;
            _lineBox.Enabled = _tool != Tool.Fill && _tool != Tool.Text;
            _filled.Enabled = _tool == Tool.Rectangle || _tool == Tool.Ellipse;
            foreach (Button b in new[] { _commit, _drop })
            {
                b.BackColor = locked ? WaitingBack : SystemColors.Control;
                b.UseVisualStyleBackColor = !locked; // after BackColor, which turns it off
            }
        }

        private static void Lock(Control c, bool locked)
        {
            c.Enabled = !locked;
            if (c is ButtonBase b)
            {
                b.BackColor = locked ? LockedBack : SystemColors.Control;
                b.UseVisualStyleBackColor = !locked; // after BackColor, which turns it off
            }
        }

        /// <summary>The previews' tile when the icon's edge matches the dialog's grey: a little darker.</summary>
        private static readonly Color PreviewContrast = Color.FromArgb(196, 196, 196);

        /// <summary>
        /// Most of the icon's outer edge (two pixels deep), as seen over <paramref name="back"/>,
        /// is close to <paramref name="back"/> itself: the icon would have no visible edge on it.
        /// </summary>
        private static bool EdgeMatches(Bitmap icon, Color back)
        {
            int close = 0, total = 0;
            for (int y = 0; y < icon.Height; y++)
                for (int x = 0; x < icon.Width; x++)
                {
                    if (x > 1 && x < icon.Width - 2 && y > 1 && y < icon.Height - 2) continue;
                    Color p = icon.GetPixel(x, y);
                    float a = p.A / 255f;
                    float r = p.R * a + back.R * (1 - a), gr = p.G * a + back.G * (1 - a), bl = p.B * a + back.B * (1 - a);
                    float d = (r - back.R) * (r - back.R) + (gr - back.G) * (gr - back.G) + (bl - back.B) * (bl - back.B);
                    if (d < 40 * 40) close++;
                    total++;
                }
            return close * 2 > total;
        }

        private void ShowPreview()
        {
            var b = new Bitmap(_preview.Width, _preview.Height);
            using (Graphics g = Graphics.FromImage(b))
            using (Bitmap icon = Composed())
            {
                g.Clear(SystemColors.Control);
                // Each preview on a tile with a thin border; the tile turns a darker grey when the
                // icon's edge would vanish against the dialog (background cleared, or near its grey)
                Color tile = EdgeMatches(icon, SystemColors.Control) ? PreviewContrast : SystemColors.Control;
                using (var brush = new SolidBrush(tile))
                using (var border = new Pen(SystemColors.ControlDark))
                    foreach (Rectangle r in new[] { new Rectangle(2, 14, 40, 40), new Rectangle(52, 1, 56, 56) })
                    {
                        g.FillRectangle(brush, r);
                        g.DrawRectangle(border, r.X, r.Y, r.Width - 1, r.Height - 1);
                    }
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(icon, 6, 18, 32, 32);   // as on the button
                g.DrawImage(icon, 56, 5, 48, 48);   // at 150 % scaling
            }
            _preview.Image?.Dispose();
            _preview.Image = b;
        }

        private sealed class Canvas : Control
        {
            private readonly IconEditor _editor;

            public Canvas(IconEditor editor)
            {
                _editor = editor;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                // Transparency shows as a light chequerboard
                using (var light = new SolidBrush(Color.White))
                using (var dark = new SolidBrush(Color.FromArgb(225, 225, 225)))
                    for (int y = 0; y < N; y += 4)
                        for (int x = 0; x < N; x += 4)
                            g.FillRectangle(((x + y) / 4) % 2 == 0 ? light : dark, 1 + x * Zoom, 1 + y * Zoom, 4 * Zoom, 4 * Zoom);
                using (Bitmap icon = _editor.Composed())
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(icon, new Rectangle(1, 1, N * Zoom, N * Zoom));
                }
                g.PixelOffsetMode = PixelOffsetMode.Default;
                if (_editor._grid.Checked)
                    using (var pen = new Pen(Color.FromArgb(40, 0, 0, 0)))
                        for (int i = 1; i < N; i++)
                        {
                            g.DrawLine(pen, 1 + i * Zoom, 1, 1 + i * Zoom, N * Zoom);
                            g.DrawLine(pen, 1, 1 + i * Zoom, N * Zoom, 1 + i * Zoom);
                        }
                // The button's 32-pixel size: every other line a little darker
                // The layer being adjusted: a dashed box round it
                RectangleF r = _editor.LayerBounds();
                if (!r.IsEmpty)
                    using (var pen = new Pen(SystemColors.Highlight) { DashStyle = DashStyle.Dash })
                        g.DrawRectangle(pen, 1 + r.X * Zoom, 1 + r.Y * Zoom, r.Width * Zoom, r.Height * Zoom);
                ControlPaint.DrawBorder(g, ClientRectangle, SystemColors.ControlDark, ButtonBorderStyle.Solid);
            }

            protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); Focus(); _editor.CanvasDown(e); }
            protected override void OnMouseMove(MouseEventArgs e) { base.OnMouseMove(e); _editor.CanvasMove(e); }
            protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); _editor.CanvasUp(e); }
        }

        /************ Keys, OK ************/

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool typing = ActiveControl is TextBox || ActiveControl is ComboBox || ActiveControl is NumericUpDown;
            bool layer = _text != null || _shape != null;
            switch (keyData)
            {
                case Keys.Control | Keys.Z: Undo(); return true;
                case Keys.Control | Keys.Y: Redo(); return true;
                case Keys.Control | Keys.V when !typing: PasteImage(); return true;
                case Keys.Escape:
                    if (layer) { Discard(); return true; } // the layer's Cancel (Ctrl+Z brings it back)
                    Close(); // as the close icon: asks, if there are changes
                    return true;
                case Keys.Enter when layer && !(ActiveControl is Button):
                    Commit();
                    return true;
            }
            if (!typing && layer)
            {
                switch (keyData)
                {
                    case Keys.Up: Nudge(0, -1); return true;
                    case Keys.Down: Nudge(0, 1); return true;
                    case Keys.Left: Nudge(-1, 0); return true;
                    case Keys.Right: Nudge(1, 0); return true;
                    case Keys.Oemplus:
                    case Keys.Add:
                    case Keys.Shift | Keys.Oemplus: ResizeLayer(1); return true;
                    case Keys.OemMinus:
                    case Keys.Subtract: ResizeLayer(-1); return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>OK: shapes are committed; text not committed stays a layer of the icon (adjustable next time).</summary>
        private void Ok()
        {
            if (_text != null) RememberFont(_text.FontName); // typed, and OK pressed without leaving the box
            if (_shape != null) Commit();
            Result = new ButtonIcon { Png = IconPainter.ToPng(_bitmap), Text = _text?.Clone() };
            DialogResult = DialogResult.OK;
        }

        /// <summary>Unsaved changes: something was done, and the icon isn't as it came.</summary>
        internal bool IsDirty
        {
            get
            {
                if (!_dirty) return false;
                if (_shape != null) return true;
                string text = _text == null ? null : System.Text.Json.JsonSerializer.Serialize(_text);
                string was = _original.Text == null ? null : System.Text.Json.JsonSerializer.Serialize(_original.Text);
                if (text != was) return true;
                using Bitmap original = IconPainter.BitmapOf(_original);
                return IconPainter.ToPng(_bitmap) != IconPainter.ToPng(original);
            }
        }

        /// <summary>
        /// The close icon (or Esc) with unsaved changes: Save changes, Discard changes, or Cancel
        /// (back to the icon), as ClipX 1's "Confirm close". OK and Cancel don't ask.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.Cancel || e.CloseReason != CloseReason.UserClosing || DialogResult != DialogResult.Cancel || _cancelClicked) return;
            if (!IsDirty) return;
            switch (ConfirmCloseDialog.Ask(this, Text, "Save the changes to the icon?", "Save changes"))
            {
                case CloseChoice.Keep:
                    Ok();
                    break;
                case CloseChoice.Discard:
                    break;
                default:
                    e.Cancel = true;
                    DialogResult = DialogResult.None;
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bitmap?.Dispose();
                foreach (Snapshot s in _undo.Concat(_redo)) s.Bitmap.Dispose();
                _preview.Image?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
