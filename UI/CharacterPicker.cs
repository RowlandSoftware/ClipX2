using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Windows.Forms;

namespace ClipX2.UI
{
    /// <summary>
    /// Every character a font has, in a scrolling grid (the icon editor's "…"): click one to see
    /// it large with its code (U+E700…, as Character Map, which can't show the icon fonts'
    /// Private Use Area), double-click it, Enter or OK to use it. A jump list on the right names
    /// the Unicode blocks the font has ("Arrows", "Dingbats"…): click one to go to it; the block
    /// at the top of the grid, or of the character chosen, is the one highlighted.
    /// </summary>
    internal sealed class CharacterPicker : Form
    {
        private const int Cell = 40, Columns = 16, Rows = 12, Side = 220;

        private readonly List<char> _chars;
        private readonly List<(string Name, int First)> _groups;
        private readonly Font _font;
        private readonly Grid _grid;
        private readonly Panel _scroller = new StayPanel { AutoScroll = true, BorderStyle = BorderStyle.FixedSingle };
        private readonly Label _code = new Label { AutoSize = false, TextAlign = ContentAlignment.TopLeft };
        private readonly PictureBox _large = new PictureBox { BorderStyle = BorderStyle.FixedSingle };
        private readonly ListBox _jump = new ListBox { IntegralHeight = false, HorizontalScrollbar = true }; // long block names scroll sideways
        private int _hover = -1, _selected = -1;
        private bool _showingGroup; // the list's selection is being set to follow the grid, not clicked

        /// <summary>The character chosen (after OK).</summary>
        public string Chosen { get; private set; }

        public CharacterPicker(string fontName, string current)
        {
            Text = "Characters in " + fontName;
            Font = new Font("Segoe UI", 10F);
            AutoScaleMode = AutoScaleMode.Font;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            KeyPreview = true;

            _chars = IconFonts.CharactersOf(fontName);
            _groups = UnicodeBlocks.GroupsOf(_chars);
            _font = new Font(fontName, 24f, GraphicsUnit.Pixel);
            _grid = new Grid(this);

            int width = Columns * Cell + SystemInformation.VerticalScrollBarWidth + 2;
            ClientSize = new Size(12 + width + 12 + Side + 12, 12 + Rows * Cell + 2 + 56);
            _scroller.Bounds = new Rectangle(12, 12, width, Rows * Cell + 2);
            _grid.Size = new Size(Columns * Cell, Math.Max(1, (_chars.Count + Columns - 1) / Columns) * Cell);
            _scroller.Controls.Add(_grid);
            Controls.Add(_scroller);
            // The jump list follows the scrolling (the bar, the wheel, the keys)
            _scroller.Scroll += (s, e) => ShowGroupInView();
            _scroller.MouseWheel += (s, e) => BeginInvoke((Action)ShowGroupInView);
            _grid.MouseWheel += (s, e) => BeginInvoke((Action)ShowGroupInView);

            // Right: the character large, its code, the jump list
            int right = _scroller.Right + 12;
            _large.Bounds = new Rectangle(right, 12, Side, 120);
            _code.Bounds = new Rectangle(right, 138, Side, 44);
            var jumpLabel = new Label { Text = "&Jump to:", AutoSize = true, Location = new Point(right, 186) };
            _jump.Bounds = new Rectangle(right, 208, Side, _scroller.Bottom - 208);
            foreach (var (name, _) in _groups) _jump.Items.Add(name);
            _jump.SelectedIndexChanged += (s, e) => { if (!_showingGroup) JumpTo(_jump.SelectedIndex); };
            Controls.AddRange(new Control[] { _large, _code, jumpLabel, _jump });

            var count = new Label
            {
                Text = _chars.Count == 0 ? "This font has no characters ClipX can show."
                    : $"{_chars.Count:N0} characters" + (_groups.Count > 1 ? $" in {_groups.Count} groups" : "") + ": double-click one to use it",
                AutoSize = false, ForeColor = SystemColors.GrayText, TextAlign = ContentAlignment.MiddleLeft,
                Bounds = new Rectangle(12, ClientSize.Height - 44, 460, 32),
            };
            var ok = new Button { Text = "OK", Bounds = new Rectangle(ClientSize.Width - 212, ClientSize.Height - 44, 96, 32) };
            ok.Click += (s, e) => Choose(_selected);
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Bounds = new Rectangle(ClientSize.Width - 108, ClientSize.Height - 44, 96, 32) };
            Controls.AddRange(new Control[] { count, ok, cancel });
            AcceptButton = ok;
            CancelButton = cancel;

            // The current character selected and in view
            if (!string.IsNullOrEmpty(current)) _selected = _chars.IndexOf(current[0]);
            ShowDetails(_selected);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_selected >= 0) ScrollToRow(Math.Max(0, _selected / Columns - 4));
            ShowGroupOf(_selected >= 0 ? _selected : 0);
            _grid.Focus();
        }

        private void Choose(int index)
        {
            if (index < 0 || index >= _chars.Count) return;
            Chosen = _chars[index].ToString();
            DialogResult = DialogResult.OK;
        }

        /// <summary>Selects a character (a click, the arrow keys): shown large, its group highlighted.</summary>
        private void SelectChar(int index)
        {
            if (index < 0 || index >= _chars.Count) return;
            _selected = index;
            ShowDetails(index);
            ShowGroupOf(index);
            _grid.Invalidate();
        }

        /************ The jump list ************/

        /// <summary>A group clicked in the list: its first character selected, its row at the top of the grid.</summary>
        private void JumpTo(int group)
        {
            if (group < 0 || group >= _groups.Count) return;
            _grid.Focus(); // first: focus mustn't move the grid afterwards (see StayPanel)
            SelectChar(_groups[group].First);
            ScrollToRow(_groups[group].First / Columns);
        }

        private void ScrollToRow(int row) => _scroller.AutoScrollPosition = new Point(0, row * Cell);

        /// <summary>The group of the first character in view (the grid scrolled).</summary>
        private void ShowGroupInView()
        {
            int row = (-_scroller.AutoScrollPosition.Y + Cell / 2) / Cell;
            ShowGroupOf(Math.Min(_chars.Count - 1, row * Columns));
        }

        /// <summary>Highlights the group a character is in, without jumping.</summary>
        private void ShowGroupOf(int index)
        {
            if (index < 0 || _groups.Count == 0) return;
            int g = _groups.Count - 1;
            while (g > 0 && _groups[g].First > index) g--;
            if (_jump.SelectedIndex == g) return;
            _showingGroup = true;
            _jump.SelectedIndex = g;
            _showingGroup = false;
        }

        /************ Details ************/

        /// <summary>The character large, with its code, its block, and whether it is an icon (private use).</summary>
        private void ShowDetails(int index)
        {
            _large.Image?.Dispose();
            _large.Image = null;
            if (index < 0 || index >= _chars.Count) { _code.Text = ""; return; }
            char ch = _chars[index];
            var b = new Bitmap(_large.Width, _large.Height);
            using (Graphics g = Graphics.FromImage(b))
            using (var big = new Font(_font.FontFamily, 88f, _font.Style, GraphicsUnit.Pixel))
            using (var centre = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.Clear(Color.White);
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.DrawString(ch.ToString(), big, Brushes.Black, new RectangleF(0, 0, b.Width, b.Height), centre);
            }
            _large.Image = b;
            bool privateUse = CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.PrivateUse;
            _code.Text = $"U+{(int)ch:X4}\n" + (privateUse ? "Private use (an icon)" : UnicodeBlocks.NameOf(ch));
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_jump.Focused) return base.ProcessCmdKey(ref msg, keyData); // the list's own arrows
            int move = keyData switch { Keys.Left => -1, Keys.Right => 1, Keys.Up => -Columns, Keys.Down => Columns, _ => 0 };
            if (move != 0 && _chars.Count > 0)
            {
                SelectChar(Math.Max(0, Math.Min(_chars.Count - 1, _selected < 0 ? 0 : _selected + move)));
                Rectangle cell = _grid.CellBounds(_selected);
                int top = -_scroller.AutoScrollPosition.Y;
                if (cell.Top < top) _scroller.AutoScrollPosition = new Point(0, cell.Top);
                else if (cell.Bottom > top + _scroller.ClientSize.Height) _scroller.AutoScrollPosition = new Point(0, cell.Bottom - _scroller.ClientSize.Height);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _font.Dispose(); _large.Image?.Dispose(); }
            base.Dispose(disposing);
        }

        /// <summary>
        /// The panel the grid scrolls in. A scrolling panel normally scrolls to show a control that
        /// gets the focus; the grid is one tall control, so that undid every jump and moved the grid
        /// on a click. This one stays where it is: the picker does its own scrolling.
        /// </summary>
        private sealed class StayPanel : Panel
        {
            protected override Point ScrollToControl(Control activeControl) => DisplayRectangle.Location;
        }

        /// <summary>The grid itself, drawing only the rows in view (fonts can have thousands of characters).</summary>
        private sealed class Grid : Control
        {
            private readonly CharacterPicker _picker;

            public Grid(CharacterPicker picker)
            {
                _picker = picker;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.Selectable, true);
                BackColor = Color.White;
            }

            public Rectangle CellBounds(int index) => new Rectangle((index % Columns) * Cell, (index / Columns) * Cell, Cell, Cell);

            private int IndexAt(Point p)
            {
                int i = (p.Y / Cell) * Columns + p.X / Cell;
                return p.X >= 0 && p.X < Columns * Cell && p.Y >= 0 && i < _picker._chars.Count ? i : -1;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                int first = Math.Max(0, e.ClipRectangle.Top / Cell) * Columns;
                int last = Math.Min(_picker._chars.Count - 1, (e.ClipRectangle.Bottom / Cell + 1) * Columns - 1);
                using var line = new Pen(Color.FromArgb(225, 225, 225));
                using var groupLine = new Pen(Color.FromArgb(120, 150, 200), 2f); // where a group starts
                using var centre = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                var starts = new HashSet<int>();
                foreach (var (_, f) in _picker._groups) if (f > 0) starts.Add(f);
                for (int i = first; i <= last; i++)
                {
                    Rectangle r = CellBounds(i);
                    if (i == _picker._selected) g.FillRectangle(SystemBrushes.Highlight, r);
                    else if (i == _picker._hover) using (var hot = new SolidBrush(Color.FromArgb(229, 241, 251))) g.FillRectangle(hot, r);
                    g.DrawRectangle(line, r.X, r.Y, r.Width - 1, r.Height - 1);
                    if (starts.Contains(i)) g.DrawLine(groupLine, r.X + 1, r.Y + 2, r.X + 1, r.Bottom - 2);
                    g.DrawString(_picker._chars[i].ToString(), _picker._font, i == _picker._selected ? SystemBrushes.HighlightText : Brushes.Black, r, centre);
                }
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                int i = IndexAt(e.Location);
                if (i == _picker._hover) return;
                _picker._hover = i;
                _picker.ShowDetails(i >= 0 ? i : _picker._selected);
                Invalidate();
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                _picker._hover = -1;
                _picker.ShowDetails(_picker._selected);
                Invalidate();
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                Focus();
                int i = IndexAt(e.Location);
                if (i >= 0) _picker.SelectChar(i);
            }

            protected override void OnMouseDoubleClick(MouseEventArgs e)
            {
                base.OnMouseDoubleClick(e);
                int i = IndexAt(e.Location);
                if (i >= 0) _picker.Choose(i);
            }
        }
    }
}
