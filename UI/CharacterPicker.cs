using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace ClipX2.UI
{
    /// <summary>
    /// A font's symbols, one group at a time (the icon editor's "…"): Arrows, Shapes, Symbols and
    /// Icons, Emoji — those the font has, picked in a short list on the right. Letters, numbers
    /// and punctuation aren't offered (they can be typed). Click a character to see it large with
    /// its code; double-click it, Enter or OK to use it.
    /// </summary>
    internal sealed class CharacterPicker : Form
    {
        private const int Cell = 40, Columns = 16, Rows = 12, Side = 220;

        private readonly List<(string Name, List<int> Chars)> _groups;
        private List<int> _chars = new List<int>(); // the group shown
        private readonly Font _font;
        private readonly Grid _grid;
        private readonly Panel _scroller = new StayPanel { AutoScroll = true, BorderStyle = BorderStyle.FixedSingle };
        private readonly Label _code = new Label { AutoSize = false, TextAlign = ContentAlignment.TopLeft };
        private readonly PictureBox _large = new PictureBox { BorderStyle = BorderStyle.FixedSingle };
        private readonly ListBox _groupList = new ListBox { IntegralHeight = false };
        private readonly Label _count = new Label { AutoSize = false, ForeColor = SystemColors.GrayText, TextAlign = ContentAlignment.MiddleLeft };
        private int _hover = -1, _selected = -1;

        /// <summary>The character chosen (after OK), as a string (two UTF-16 units for an emoji beyond U+FFFF).</summary>
        public string Chosen { get; private set; }

        public CharacterPicker(string fontName, string current)
        {
            Text = "Symbols in " + fontName;
            Font = new Font("Segoe UI", 10F);
            AutoScaleMode = AutoScaleMode.Font;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            KeyPreview = true;

            _groups = SymbolGroups.GroupsOf(IconFonts.CharactersOf(fontName));
            _font = new Font(fontName, 24f, GraphicsUnit.Pixel);
            _grid = new Grid(this);

            int width = Columns * Cell + SystemInformation.VerticalScrollBarWidth + 2;
            ClientSize = new Size(12 + width + 12 + Side + 12, 12 + Rows * Cell + 2 + 56);
            _scroller.Bounds = new Rectangle(12, 12, width, Rows * Cell + 2);
            _scroller.Controls.Add(_grid);
            Controls.Add(_scroller);

            // Right: the character large, its code, the groups
            int right = _scroller.Right + 12;
            _large.Bounds = new Rectangle(right, 12, Side, 120);
            _code.Bounds = new Rectangle(right, 138, Side, 44);
            var groupLabel = new Label { Text = "&Show:", AutoSize = true, Location = new Point(right, 186) };
            foreach (var (name, chars) in _groups) _groupList.Items.Add($"{name} ({chars.Count:N0})");
            _groupList.Font = Font; // first: its line height follows the font
            _groupList.Bounds = new Rectangle(right, 208, Side, Math.Max(1, _groups.Count) * _groupList.ItemHeight + SystemInformation.BorderSize.Height * 2 + 4); // every group, no scrolling
            _groupList.SelectedIndexChanged += (s, e) => { if (!_settingGroup) ShowGroup(_groupList.SelectedIndex, keep: -1); };
            Controls.AddRange(new Control[] { _large, _code, groupLabel, _groupList });

            _count.Bounds = new Rectangle(12, ClientSize.Height - 44, 460, 32);
            var ok = new Button { Text = "OK", Bounds = new Rectangle(ClientSize.Width - 212, ClientSize.Height - 44, 96, 32) };
            ok.Click += (s, e) => Choose(_selected);
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Bounds = new Rectangle(ClientSize.Width - 108, ClientSize.Height - 44, 96, 32) };
            Controls.AddRange(new Control[] { _count, ok, cancel });
            AcceptButton = ok;
            CancelButton = cancel;

            if (_groups.Count == 0)
            {
                _count.Text = "This font has no arrows, shapes, symbols or emoji: type its letters into Characters.";
                return;
            }
            // Open on the current character's group (selected), else the first the font has
            int currentCode = string.IsNullOrEmpty(current) ? -1 : char.ConvertToUtf32(current, 0);
            int group = _groups.FindIndex(g => g.Chars.Contains(currentCode));
            ShowGroup(Math.Max(0, group), keep: group >= 0 ? _groups[group].Chars.IndexOf(currentCode) : -1);
        }

        /// <summary>Shows one group in the grid, from its top (the character <paramref name="keep"/> selected and in view, if any).</summary>
        private void ShowGroup(int group, int keep)
        {
            if (group < 0 || group >= _groups.Count) return;
            if (_groupList.SelectedIndex != group)
            {
                _settingGroup = true; // not a click in the list
                _groupList.SelectedIndex = group;
                _settingGroup = false;
            }
            _chars = _groups[group].Chars;
            _selected = keep;
            _hover = -1;
            _grid.Size = new Size(Columns * Cell, Math.Max(1, (_chars.Count + Columns - 1) / Columns) * Cell);
            _scroller.AutoScrollPosition = new Point(0, keep >= 0 ? Math.Max(0, keep / Columns - 4) * Cell : 0);
            _grid.Invalidate();
            ShowDetails(_selected);
            _count.Text = $"{_groups[group].Name}: {_chars.Count:N0}. Double-click one to use it.";
        }

        private bool _opening = true;
        private bool _settingGroup;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_opening && _selected >= 0) _scroller.AutoScrollPosition = new Point(0, Math.Max(0, _selected / Columns - 4) * Cell);
            _opening = false;
            _grid.Focus();
        }

        private void Choose(int index)
        {
            if (index < 0 || index >= _chars.Count) return;
            Chosen = char.ConvertFromUtf32(_chars[index]);
            DialogResult = DialogResult.OK;
        }

        /// <summary>Selects a character (a click, the arrow keys): shown large, with its code.</summary>
        private void SelectChar(int index)
        {
            if (index < 0 || index >= _chars.Count) return;
            _selected = index;
            ShowDetails(index);
            _grid.Invalidate();
        }

        /// <summary>The character large, with its code.</summary>
        private void ShowDetails(int index)
        {
            _large.Image?.Dispose();
            _large.Image = null;
            if (index < 0 || index >= _chars.Count) { _code.Text = ""; return; }
            string ch = char.ConvertFromUtf32(_chars[index]);
            var b = new Bitmap(_large.Width, _large.Height);
            using (Graphics g = Graphics.FromImage(b))
            using (var big = new Font(_font.FontFamily, 88f, _font.Style, GraphicsUnit.Pixel))
            using (var centre = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.Clear(Color.White);
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.DrawString(ch, big, Brushes.Black, new RectangleF(0, 0, b.Width, b.Height), centre);
            }
            _large.Image = b;
            _code.Text = $"U+{_chars[index]:X4}";
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_groupList.Focused) return base.ProcessCmdKey(ref msg, keyData); // the list's own arrows
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
        /// gets the focus; the grid is one tall control, so that moved the grid on a click. This
        /// one stays where it is: the picker does its own scrolling.
        /// </summary>
        private sealed class StayPanel : Panel
        {
            protected override Point ScrollToControl(Control activeControl) => DisplayRectangle.Location;
        }

        /// <summary>The grid itself, drawing only the rows in view (a group can have thousands of characters).</summary>
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
                List<int> chars = _picker._chars;
                int first = Math.Max(0, e.ClipRectangle.Top / Cell) * Columns;
                int last = Math.Min(chars.Count - 1, (e.ClipRectangle.Bottom / Cell + 1) * Columns - 1);
                using var line = new Pen(Color.FromArgb(225, 225, 225));
                using var centre = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                for (int i = first; i <= last; i++)
                {
                    Rectangle r = CellBounds(i);
                    if (i == _picker._selected) g.FillRectangle(SystemBrushes.Highlight, r);
                    else if (i == _picker._hover) using (var hot = new SolidBrush(Color.FromArgb(229, 241, 251))) g.FillRectangle(hot, r);
                    g.DrawRectangle(line, r.X, r.Y, r.Width - 1, r.Height - 1);
                    g.DrawString(char.ConvertFromUtf32(chars[i]), _picker._font, i == _picker._selected ? SystemBrushes.HighlightText : Brushes.Black, r, centre);
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
