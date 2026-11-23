using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ClipX2.UI
{
    /// <summary>
    /// The PowerButtons down the right of the window (Form1.panel2): 32-pixel icons in a column, or
    /// in several once the panel is widened (drag its left edge). Click runs a button, right-click
    /// changes it, drag moves it; the "+" at the end makes a new one. The order is the settings'
    /// order, which Alt+1…Alt+9 follow.
    /// </summary>
    internal sealed class PowerButtonBar : Panel
    {
        public const int IconSize = 32;
        private const int Cell = IconSize + 6;       // a tile: the icon and its hover frame
        private const int Pitch = Cell + 4;          // from one tile to the next
        private const int TopMargin = 6;
        private const int Grip = 5;                  // the left edge that widens the panel
        /// <summary>The panel's width for one column (the designer's); each further column adds <see cref="Pitch"/>.</summary>
        public const int OneColumnWidth = 82;
        public const int MaxColumns = 6;

        public static int WidthFor(int columns) => OneColumnWidth + (Math.Max(1, columns) - 1) * Pitch;

        private readonly List<Tile> _tiles = new List<Tile>();
        private readonly Tile _newTile;
        private IList<PowerButton> _buttons = new List<PowerButton>();
        private int _columns = 1;

        public event EventHandler<PowerButton> RunRequested;
        public event EventHandler<PowerButton> EditRequested;
        public event EventHandler NewRequested;
        /// <summary>A button was dragged to a new place (the list has been re-ordered).</summary>
        public event EventHandler Reordered;
        /// <summary>The left edge is being dragged: the number of columns wanted.</summary>
        public event EventHandler<int> ColumnsWanted;
        /// <summary>The mouse is over a button (null: the "+"; or left them: see <see cref="HoverEnded"/>).</summary>
        public event EventHandler<PowerButton> Hovered;
        public event EventHandler HoverEnded;
        /// <summary>A right-click on the panel itself (not a button): where, for a menu.</summary>
        public event EventHandler<Point> MenuRequested;

        public PowerButtonBar()
        {
            DoubleBuffered = true;
            AutoScroll = true;
            _newTile = new Tile(this, null);
            Controls.Add(_newTile);
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden), System.ComponentModel.Browsable(false)]
        public int Columns
        {
            get => _columns;
            set { _columns = Math.Max(1, Math.Min(MaxColumns, value)); LayoutTiles(); }
        }

        /// <summary>Shows these buttons (the settings' list, re-ordered in place by dragging).</summary>
        public void SetButtons(IList<PowerButton> buttons)
        {
            _buttons = buttons;
            SuspendLayout();
            foreach (Tile t in _tiles) { Controls.Remove(t); t.Dispose(); }
            _tiles.Clear();
            foreach (PowerButton b in buttons)
            {
                var t = new Tile(this, b);
                _tiles.Add(t);
                Controls.Add(t);
            }
            ResumeLayout(false);
            LayoutTiles();
        }

        /// <summary>Redraws a button's icon (after editing).</summary>
        public void RefreshIcons()
        {
            foreach (Tile t in _tiles) t.RefreshIcon();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutTiles();
        }

        private void LayoutTiles()
        {
            int columns = _columns;
            int used = columns * Pitch - (Pitch - Cell);
            int left = Math.Max(Grip, (ClientSize.Width - used) / 2);
            var all = new List<Tile>(_tiles) { _newTile };
            for (int i = 0; i < all.Count; i++)
            {
                Point p = new Point(left + (i % columns) * Pitch, TopMargin + (i / columns) * Pitch);
                all[i].Bounds = new Rectangle(p.X + AutoScrollPosition.X, p.Y + AutoScrollPosition.Y, Cell, Cell);
            }
            Invalidate();
        }

        /// <summary>The index a tile dropped at <paramref name="client"/> would take (0 … count).</summary>
        private int DropIndex(Point client)
        {
            int columns = _columns;
            int used = columns * Pitch - (Pitch - Cell);
            int left = Math.Max(Grip, (ClientSize.Width - used) / 2);
            int x = client.X - AutoScrollPosition.X - left, y = client.Y - AutoScrollPosition.Y - TopMargin;
            int row = Math.Max(0, (int)Math.Floor(y / (double)Pitch));
            int col = Math.Max(0, Math.Min(columns - 1, (int)Math.Floor((x + Pitch / 2.0) / Pitch)));
            int index = row * columns + col;
            if (x > (columns - 1) * Pitch + Cell / 2) index = row * columns + columns; // past the last column: end of the row
            return Math.Max(0, Math.Min(_tiles.Count, index));
        }

        private int _markerIndex = -1;

        private void ShowMarker(int index)
        {
            if (_markerIndex == index) return;
            _markerIndex = index;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // The grip: a faint line down the left edge
            using (var pen = new Pen(SystemColors.ControlDark) { DashStyle = DashStyle.Dot })
                e.Graphics.DrawLine(pen, 1, 0, 1, ClientSize.Height);
            if (_markerIndex < 0) return;
            // Where a dragged button would go: a bar before that place
            Rectangle r;
            if (_markerIndex < _tiles.Count) r = _tiles[_markerIndex].Bounds;
            else
            {
                Rectangle last = _tiles.Count > 0 ? _tiles[_tiles.Count - 1].Bounds : _newTile.Bounds;
                r = new Rectangle(last.Right + 4, last.Top, last.Width, last.Height); // after the last button
            }
            using var brush = new SolidBrush(SystemColors.Highlight);
            e.Graphics.FillRectangle(brush, r.Left - 4, r.Top, 3, r.Height);
        }

        /************ The left edge: widening ************/

        private bool _sizing;
        private int _sizingStartX, _sizingStartColumns;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_sizing)
            {
                int dx = _sizingStartX - Cursor.Position.X; // left widens
                int wanted = _sizingStartColumns + (int)Math.Round(dx / (double)Pitch);
                wanted = Math.Max(1, Math.Min(MaxColumns, wanted));
                if (wanted != _columns) ColumnsWanted?.Invoke(this, wanted);
                return;
            }
            Cursor = e.X < Grip ? Cursors.SizeWE : Cursors.Default;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && e.X < Grip)
            {
                _sizing = true;
                _sizingStartX = Cursor.Position.X;
                _sizingStartColumns = _columns;
                Capture = true;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _sizing = false;
            Capture = false;
            if (e.Button == MouseButtons.Right) MenuRequested?.Invoke(this, e.Location);
        }

        /************ One button ************/

        private sealed class Tile : Control
        {
            private readonly PowerButtonBar _bar;
            public readonly PowerButton Button;   // null: the "+"
            private Bitmap _icon;
            private bool _hover, _pressed, _dragging;
            private Point _downAt;

            public Tile(PowerButtonBar bar, PowerButton button)
            {
                _bar = bar;
                Button = button;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
                AccessibleRole = AccessibleRole.PushButton;
                AccessibleName = button == null ? "New PowerButton" : button.Name;
                RefreshIcon();
            }

            public void RefreshIcon()
            {
                _icon?.Dispose();
                _icon = Button == null ? null : IconPainter.Render(Button.Icon, IconSize);
                AccessibleName = Button == null ? "New PowerButton" : Button.Name;
                Invalidate();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing) _icon?.Dispose();
                base.Dispose(disposing);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var frame = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
                if (_hover || _pressed)
                {
                    using GraphicsPath path = IconPainter.RoundedSquare(frame, 4);
                    using var fill = new SolidBrush(_pressed ? SystemColors.ControlDark : SystemColors.ControlLight);
                    using var pen = new Pen(SystemColors.Highlight);
                    g.FillPath(fill, path);
                    g.DrawPath(pen, path);
                }
                int o = (Width - IconSize) / 2;
                if (_icon != null)
                {
                    g.DrawImage(_icon, o, o, IconSize, IconSize);
                    if (Button.Tasks.Count == 0) // not set up yet
                        using (var veil = new SolidBrush(Color.FromArgb(140, SystemColors.Control))) g.FillRectangle(veil, o, o, IconSize, IconSize);
                }
                else
                {
                    // "+": a dashed square with a plus
                    using GraphicsPath path = IconPainter.RoundedSquare(new RectangleF(o + 0.5f, o + 0.5f, IconSize - 1, IconSize - 1), 5);
                    using var pen = new Pen(SystemColors.ControlDarkDark) { DashStyle = DashStyle.Dash };
                    g.DrawPath(pen, path);
                    using var plus = new Pen(SystemColors.ControlDarkDark, 3);
                    int c = Width / 2;
                    g.DrawLine(plus, c - 7, c, c + 7, c);
                    g.DrawLine(plus, c, c - 7, c, c + 7);
                }
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                base.OnMouseEnter(e);
                _hover = true;
                Invalidate();
                _bar.Hovered?.Invoke(_bar, Button);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                _hover = false;
                Invalidate();
                _bar.HoverEnded?.Invoke(_bar, EventArgs.Empty);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;
                _pressed = true;
                _downAt = e.Location;
                Invalidate();
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                if (!_pressed || Button == null || e.Button != MouseButtons.Left) return;
                if (!_dragging && (Math.Abs(e.X - _downAt.X) > 4 || Math.Abs(e.Y - _downAt.Y) > 4))
                {
                    _dragging = true;
                    Cursor = Cursors.SizeAll;
                }
                if (_dragging) _bar.ShowMarker(_bar.DropIndex(_bar.PointToClient(PointToScreen(e.Location))));
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                bool wasPressed = _pressed, wasDragging = _dragging;
                _pressed = _dragging = false;
                Cursor = Cursors.Default;
                Invalidate();
                if (wasDragging)
                {
                    int to = _bar.DropIndex(_bar.PointToClient(PointToScreen(e.Location)));
                    _bar.ShowMarker(-1);
                    _bar.MoveButton(Button, to);
                    return;
                }
                if (!wasPressed || !ClientRectangle.Contains(e.Location)) return;
                if (Button == null) { _bar.NewRequested?.Invoke(_bar, EventArgs.Empty); return; }
                if (e.Button == MouseButtons.Left) _bar.RunRequested?.Invoke(_bar, Button);
                else if (e.Button == MouseButtons.Right) _bar.EditRequested?.Invoke(_bar, Button);
            }
        }

        /// <summary>Moves a button to a place in the list (the index as before the move), then shows the new order.</summary>
        private void MoveButton(PowerButton button, int to)
        {
            int from = _buttons.IndexOf(button);
            if (from < 0) return;
            if (to > from) to--;
            if (to == from) return;
            _buttons.RemoveAt(from);
            _buttons.Insert(to, button);
            SetButtons(_buttons);
            Reordered?.Invoke(this, EventArgs.Empty);
        }
    }
}
