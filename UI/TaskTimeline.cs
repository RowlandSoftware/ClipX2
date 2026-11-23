using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ClipX2.UI
{
    /// <summary>
    /// The PowerButton editor's timeline: the button's tasks as cards, left to right in the order
    /// they run, joined by arrows. Tools are dragged in from the palette (data: the tool's tag) and
    /// dropped where the marker shows; cards are dragged to re-order them, or out of the timeline
    /// to take them away (Delete does that too). Clicking a card selects it.
    /// </summary>
    internal sealed class TaskTimeline : ScrollableControl
    {
        private const int CardWidth = 104, CardHeight = 92, Gap = 26, Margin0 = 10;

        private List<PowerTask> _tasks = new List<PowerTask>();
        private int _selected = -1;
        private int _marker = -1;          // where a drop would go (0 … count), or -1
        private int _hover = -1;
        private int _pressed = -1;
        private Point _downAt;
        private readonly Func<string, Image> _icon;

        /// <summary>The selected task changed (index, or -1).</summary>
        public event EventHandler SelectionChanged;
        /// <summary>Tasks added, removed or moved.</summary>
        public event EventHandler TasksChanged;
        /// <summary>A tool dropped in: make its task (the timeline inserts it).</summary>
        public Func<string, PowerTask> MakeTask;

        public TaskTimeline(Func<string, Image> icon)
        {
            _icon = icon;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            AllowDrop = true;
            AutoScroll = true;
            BackColor = SystemColors.Window;
            TabStop = true;
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden), System.ComponentModel.Browsable(false)]
        public List<PowerTask> Tasks
        {
            get => _tasks;
            set { _tasks = value ?? new List<PowerTask>(); _selected = _tasks.Count > 0 ? 0 : -1; UpdateScroll(); Invalidate(); SelectionChanged?.Invoke(this, EventArgs.Empty); }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden), System.ComponentModel.Browsable(false)]
        public int SelectedIndex
        {
            get => _selected;
            set
            {
                int v = value < 0 || value >= _tasks.Count ? -1 : value;
                if (v == _selected) return;
                _selected = v;
                if (v >= 0) ScrollToCard(v);
                Invalidate();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public PowerTask SelectedTask => _selected >= 0 ? _tasks[_selected] : null;

        /// <summary>A card changed (its options): redraw.</summary>
        public void RefreshCards() => Invalidate();

        public void RemoveSelected()
        {
            if (_selected < 0) return;
            _tasks.RemoveAt(_selected);
            int next = Math.Min(_selected, _tasks.Count - 1);
            _selected = -2; // force the change event
            UpdateScroll();
            TasksChanged?.Invoke(this, EventArgs.Empty);
            SelectedIndex = next;
        }

        private Rectangle CardRect(int i) =>
            new Rectangle(Margin0 + i * (CardWidth + Gap) + AutoScrollPosition.X, (ClientSize.Height - CardHeight) / 2, CardWidth, CardHeight);

        private int CardAt(Point p)
        {
            for (int i = 0; i < _tasks.Count; i++) if (CardRect(i).Contains(p)) return i;
            return -1;
        }

        /// <summary>Where a drop at <paramref name="p"/> goes: before the card whose middle is right of it.</summary>
        private int DropIndex(Point p)
        {
            int x = p.X - AutoScrollPosition.X - Margin0 + Gap / 2;
            int i = (int)Math.Floor((x + CardWidth / 2.0) / (CardWidth + Gap));
            return Math.Max(0, Math.Min(_tasks.Count, i));
        }

        private void UpdateScroll()
        {
            AutoScrollMinSize = new Size(Margin0 * 2 + Math.Max(1, _tasks.Count + 1) * (CardWidth + Gap), 0);
        }

        private void ScrollToCard(int i)
        {
            Rectangle r = CardRect(i);
            if (r.Left < 0) AutoScrollPosition = new Point(-AutoScrollPosition.X + r.Left - Margin0, 0);
            else if (r.Right > ClientSize.Width) AutoScrollPosition = new Point(-AutoScrollPosition.X + r.Right - ClientSize.Width + Margin0, 0);
        }

        protected override void OnScroll(ScrollEventArgs se) { base.OnScroll(se); Invalidate(); }

        /************ Drawing ************/

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            if (_tasks.Count == 0)
            {
                TextRenderer.DrawText(g, "Drag tools here from the list on the left, in the order they are to run.",
                    Font, new Rectangle(Margin0, 0, ClientSize.Width - 2 * Margin0, ClientSize.Height), SystemColors.GrayText,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            }
            for (int i = 0; i < _tasks.Count; i++)
            {
                Rectangle r = CardRect(i);
                if (i > 0) DrawArrow(g, CardRect(i - 1).Right + 4, r.Left - 4, r.Top + r.Height / 2);
                DrawCard(g, r, i);
            }
            if (_marker >= 0)
            {
                int x = _marker < _tasks.Count ? CardRect(_marker).Left - Gap / 2 : (_tasks.Count == 0 ? Margin0 : CardRect(_tasks.Count - 1).Right + Gap / 2);
                int top = (ClientSize.Height - CardHeight) / 2 - 4;
                using var brush = new SolidBrush(SystemColors.Highlight);
                g.FillRectangle(brush, x - 2, top, 4, CardHeight + 8);
            }
            ControlPaint.DrawBorder(g, ClientRectangle, SystemColors.ControlDark, ButtonBorderStyle.Solid);
            if (Focused && _selected >= 0)
            {
                Rectangle f = CardRect(_selected);
                f.Inflate(3, 3);
                ControlPaint.DrawFocusRectangle(g, f);
            }
        }

        private void DrawArrow(Graphics g, int x1, int x2, int y)
        {
            using var pen = new Pen(SystemColors.ControlDark, 2) { EndCap = LineCap.ArrowAnchor };
            g.DrawLine(pen, x1, y, x2, y);
        }

        private void DrawCard(Graphics g, Rectangle r, int i)
        {
            PowerTask t = _tasks[i];
            bool selected = i == _selected, unknown = t.Tool == null;
            using (GraphicsPath path = IconPainter.RoundedSquare(r, 6))
            {
                Color fill = selected ? Color.FromArgb(220, 232, 252) : i == _hover ? SystemColors.ControlLight : SystemColors.Control;
                using var brush = new SolidBrush(fill);
                using var pen = new Pen(selected ? SystemColors.Highlight : SystemColors.ControlDark, selected ? 2 : 1);
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }
            // The number, top left
            TextRenderer.DrawText(g, (i + 1).ToString(), Font, new Rectangle(r.Left + 4, r.Top + 2, 24, 18), SystemColors.GrayText, TextFormatFlags.Left);
            // Where its options come from, top right
            string use = !PowerTaskTypes.HasOptions(t.Tag) ? "" : t.Use == TaskOptions.Ask ? "Ask" : t.Use == TaskOptions.LastUsed ? "Last" : "Own";
            if (use.Length > 0)
                TextRenderer.DrawText(g, use, Font, new Rectangle(r.Right - 44, r.Top + 2, 40, 18),
                    t.Use == TaskOptions.Ask ? Color.DarkOrange : SystemColors.GrayText, TextFormatFlags.Right);
            // The tool's icon, then its name
            Image icon = _icon(t.Tag);
            var iconRect = new Rectangle(r.Left + (r.Width - 32) / 2, r.Top + 10, 32, 32);
            if (icon != null) g.DrawImage(icon, iconRect);
            else DrawLetters(g, iconRect, t);
            TextRenderer.DrawText(g, t.Name, Font, new Rectangle(r.Left + 3, r.Top + 46, r.Width - 6, r.Height - 48),
                unknown ? Color.Firebrick : SystemColors.ControlText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        /// <summary>A tool without a toolbar icon: its initials in a circle.</summary>
        internal static void DrawLetters(Graphics g, Rectangle r, PowerTask t) => DrawLetters(g, r, t.Name);

        internal static void DrawLetters(Graphics g, Rectangle r, string name)
        {
            string letters = "";
            foreach (string w in name.Split(new[] { ' ', '(' }, StringSplitOptions.RemoveEmptyEntries))
                if (letters.Length < 2 && char.IsLetterOrDigit(w[0])) letters += char.ToUpperInvariant(w[0]);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(Color.FromArgb(90, 110, 140));
            g.FillEllipse(brush, r);
            using var font = new Font("Segoe UI", r.Height * 0.36f, FontStyle.Bold, GraphicsUnit.Pixel);
            TextRenderer.DrawText(g, letters, font, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        /************ Mouse and keys ************/

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int i = CardAt(e.Location);
            _pressed = i;
            _downAt = e.Location;
            if (i >= 0) SelectedIndex = i;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int h = CardAt(e.Location);
            if (h != _hover) { _hover = h; Invalidate(); }
            if (_pressed >= 0 && e.Button == MouseButtons.Left &&
                (Math.Abs(e.X - _downAt.X) > SystemInformation.DragSize.Width || Math.Abs(e.Y - _downAt.Y) > SystemInformation.DragSize.Height))
            {
                PowerTask dragged = _tasks[_pressed];
                _pressed = -1;
                DragDropEffects done = DoDragDrop(new DataObject(typeof(PowerTask).FullName, dragged), DragDropEffects.Move);
                _marker = -1;
                Invalidate();
                // Dropped outside the timeline: the task goes
                if (done == DragDropEffects.None && !ClientRectangle.Contains(PointToClient(Cursor.Position)) && _tasks.Contains(dragged))
                {
                    SelectedIndex = _tasks.IndexOf(dragged);
                    RemoveSelected();
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); _pressed = -1; }

        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hover = -1; Invalidate(); }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        protected override bool IsInputKey(Keys keyData) =>
            keyData is Keys.Left or Keys.Right or Keys.Home or Keys.End || base.IsInputKey(keyData);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Delete: RemoveSelected(); e.Handled = true; break;
                case Keys.Left when e.Control: MoveSelected(-1); e.Handled = true; break;
                case Keys.Right when e.Control: MoveSelected(+1); e.Handled = true; break;
                case Keys.Left: if (_selected > 0) SelectedIndex = _selected - 1; e.Handled = true; break;
                case Keys.Right: if (_selected < _tasks.Count - 1) SelectedIndex = _selected + 1; e.Handled = true; break;
                case Keys.Home: SelectedIndex = 0; e.Handled = true; break;
                case Keys.End: SelectedIndex = _tasks.Count - 1; e.Handled = true; break;
            }
        }

        /// <summary>Ctrl+Left / Ctrl+Right: the selected task one place earlier or later.</summary>
        public void MoveSelected(int by)
        {
            int to = _selected + by;
            if (_selected < 0 || to < 0 || to >= _tasks.Count) return;
            PowerTask t = _tasks[_selected];
            _tasks.RemoveAt(_selected);
            _tasks.Insert(to, t);
            _selected = to;
            ScrollToCard(to);
            Invalidate();
            TasksChanged?.Invoke(this, EventArgs.Empty);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /************ Dropping ************/

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            bool tool = e.Data.GetDataPresent(DataFormats.StringFormat), task = e.Data.GetDataPresent(typeof(PowerTask).FullName);
            if (!tool && !task) { e.Effect = DragDropEffects.None; return; }
            e.Effect = task ? DragDropEffects.Move : DragDropEffects.Copy;
            Point p = PointToClient(new Point(e.X, e.Y));
            int m = DropIndex(p);
            // Auto-scroll near the ends
            if (p.X < 20) AutoScrollPosition = new Point(-AutoScrollPosition.X - 20, 0);
            else if (p.X > ClientSize.Width - 20) AutoScrollPosition = new Point(-AutoScrollPosition.X + 20, 0);
            if (m != _marker) { _marker = m; Invalidate(); }
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);
            _marker = -1;
            Invalidate();
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            int to = DropIndex(PointToClient(new Point(e.X, e.Y)));
            _marker = -1;
            if (e.Data.GetData(typeof(PowerTask).FullName) is PowerTask moved)
            {
                int from = _tasks.IndexOf(moved);
                if (from < 0) return;
                if (to > from) to--;
                _tasks.RemoveAt(from);
                _tasks.Insert(to, moved);
            }
            else if (e.Data.GetData(DataFormats.StringFormat) is string tag && MakeTask != null)
            {
                PowerTask t = MakeTask(tag);
                if (t == null) return;
                _tasks.Insert(to, t);
            }
            else return;
            UpdateScroll();
            _selected = -2;
            TasksChanged?.Invoke(this, EventArgs.Empty);
            SelectedIndex = to;
            Focus();
        }

        /// <summary>Adds a task at the end (double-click or Enter in the palette).</summary>
        public void Append(PowerTask t)
        {
            _tasks.Add(t);
            UpdateScroll();
            _selected = -2;
            TasksChanged?.Invoke(this, EventArgs.Empty);
            SelectedIndex = _tasks.Count - 1;
        }
    }
}
