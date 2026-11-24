using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using ClipX2.TextEngine;
using ClipX2.TextEngine.Tools;
using ClipX2.UI.Panels;

namespace ClipX2.UI
{
    /// <summary>
    /// Changes a PowerButton (ClipX 1's Edit PowerButton dialogue, made visual): its name and icon
    /// at the top; the tools on the left, dragged onto the timeline of tasks (or double-clicked);
    /// the selected task's options below the timeline: the tool's own side panel, showing the
    /// task's options, and whether the task uses them, the tool's last-used options, or asks when
    /// the button runs. "Delete PowerButton" deletes the whole button.
    /// </summary>
    internal sealed class PowerButtonEditor : Form
    {
        private readonly PowerButton _button;
        private readonly IReadOnlyList<PowerButton> _all;
        private readonly Func<string, Image> _toolIcon;
        private readonly Action<PowerButton> _try;

        private readonly TextBox _name = new TextBox();
        private readonly PictureBox _iconPreview = new PictureBox();
        private readonly Label _keyLabel = new Label();
        private readonly ListView _palette = new ListView();
        private readonly TaskTimeline _timeline;
        private readonly Label _taskTitle = new Label();
        private readonly Label _taskDescription = new Label();
        private readonly RadioButton _useOwn = new RadioButton { Text = "&These options" };
        private readonly RadioButton _useLast = new RadioButton { Text = "&Last-used options" };
        private readonly RadioButton _useAsk = new RadioButton { Text = "As&k when run" };
        private readonly FlowLayoutPanel _useRow = new FlowLayoutPanel();
        private readonly Panel _optionsHost = new Panel();
        private readonly Label _message = new Label();
        private readonly ToolTip _tips = new ToolTip();

        private PowerTask _current;          // the task whose options are showing
        private Func<object> _readOptions;   // reads them back from the controls showing them
        private bool _loading;

        // Unsaved changes: the name (against the one it came with), or anything else the user did
        private bool _dirty;
        private string _originalName;
        private string _shownOptions;        // the options as first shown (JSON), to tell whether they were changed
        private bool _cancelClicked;         // Cancel discards without asking; the close icon and Esc ask

        /// <summary>The button's place (1, 2…): Alt+n runs it.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden), System.ComponentModel.Browsable(false)]
        public int ButtonNumber
        {
            set
            {
                Text = $"PowerButton {value}";
                _keyLabel.Text = value <= 9 ? $"Alt+{value} runs it" : "";
            }
        }

        /// <summary>"Delete PowerButton" was confirmed: the caller deletes the button.</summary>
        public bool DeleteRequested { get; private set; }

        /// <param name="button">The button to change (a copy: Cancel leaves the original alone).</param>
        /// <param name="all">All the PowerButtons (to include one in another, and check for loops).</param>
        /// <param name="toolIcon">A tool's toolbar icon by tag, or null.</param>
        /// <param name="tryOnText">Runs a button on the text now ("Try on the text").</param>
        public PowerButtonEditor(PowerButton button, IReadOnlyList<PowerButton> all, Func<string, Image> toolIcon, Action<PowerButton> tryOnText)
        {
            _button = button;
            _all = all;
            _toolIcon = toolIcon;
            _try = tryOnText;
            _timeline = new TaskTimeline(tag => _toolIcon(tag)) { MakeTask = NewTask };

            Text = "PowerButton";
            Font = new Font("Segoe UI", 10F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1060, 760);
            MinimumSize = new Size(860, 600);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MinimizeBox = false;
            KeyPreview = true;

            BuildLayout();
            FillPalette();

            _name.Text = button.Name;
            _originalName = button.Name.Trim();
            ShowIconPreview();
            _timeline.Tasks = button.Tasks; // edited in place (the button is a copy)
            _timeline.SelectionChanged += (s, e) => ShowTask(_timeline.SelectedTask);
            _timeline.TasksChanged += (s, e) => { _message.Text = ""; _dirty = true; };
            ShowTask(_timeline.SelectedTask);
        }

        /************ Layout ************/

        private void BuildLayout()
        {
            SuspendLayout();

            // Top: name, icon, key, Delete
            // Each panel is the form's width before its right-anchored buttons are placed (anchoring keeps that distance)
            var top = new Panel { Dock = DockStyle.Top, Height = 64, Width = ClientSize.Width, Padding = new Padding(10, 10, 10, 6) };
            var nameLabel = new Label { Text = "&Name:", AutoSize = true, Location = new Point(12, 22) };
            _name.SetBounds(70, 18, 320, 26);
            _name.MaxLength = 80;
            _iconPreview.SetBounds(408, 8, 46, 46);
            _iconPreview.Cursor = Cursors.Hand;
            _iconPreview.Click += (s, e) => EditIcon();
            _tips.SetToolTip(_iconPreview, "The button's icon: click to change it");
            var iconButton = new Button { Text = "&Icon…", AutoSize = true, Location = new Point(462, 16) };
            iconButton.Click += (s, e) => EditIcon();
            _keyLabel.AutoSize = true;
            _keyLabel.ForeColor = SystemColors.GrayText;
            _keyLabel.Location = new Point(560, 22);
            var delete = new Button { Text = "&Delete PowerButton", AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            delete.Location = new Point(ClientSize.Width - 190, 16);
            delete.Click += (s, e) => DeleteButton();
            delete.Image = TrashImage();
            delete.ImageAlign = ContentAlignment.MiddleLeft;
            delete.TextImageRelation = TextImageRelation.ImageBeforeText;
            delete.Padding = new Padding(6, 0, 4, 0);
            _tips.SetToolTip(delete, "Delete this whole PowerButton");
            top.Controls.AddRange(new Control[] { nameLabel, _name, _iconPreview, iconButton, _keyLabel, delete });

            // Bottom: Try, message, OK, Cancel
            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 52, Width = ClientSize.Width };
            var tryButton = new Button { Text = "T&ry on the text", AutoSize = true, Location = new Point(12, 12) };
            tryButton.Click += (s, e) => TryOnText();
            _tips.SetToolTip(tryButton, "Run the button as it is now on ClipX's text (Ctrl+Z undoes it). Tasks that ask use the last-used options.");
            _message.AutoSize = false;
            _message.SetBounds(160, 8, ClientSize.Width - 160 - 220, 38);
            _message.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _message.ForeColor = Color.Firebrick;
            _message.TextAlign = ContentAlignment.MiddleLeft;
            var ok = new Button { Text = "OK", Size = new Size(96, 32), Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(ClientSize.Width - 212, 10) };
            var cancel = new Button { Text = "Cancel", Size = new Size(96, 32), Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(ClientSize.Width - 108, 10), DialogResult = DialogResult.Cancel };
            ok.Click += (s, e) => Ok();
            cancel.Click += (s, e) => _cancelClicked = true;
            AcceptButton = null; // Enter belongs to the controls (the palette adds a tool)
            CancelButton = null; // Esc closes as the close icon does: it asks if there are changes
            bottom.Controls.AddRange(new Control[] { tryButton, _message, ok, cancel });

            // Left: the tools
            var left = new Panel { Dock = DockStyle.Left, Width = 300, Padding = new Padding(10, 0, 6, 6) };
            var paletteLabel = new Label { Text = "Tools: drag one onto the timeline, or double-click it", Dock = DockStyle.Top, Height = 40, ForeColor = SystemColors.GrayText };
            _palette.Dock = DockStyle.Fill;
            left.Controls.Add(_palette);
            left.Controls.Add(paletteLabel);

            // Right: the timeline, then the selected task's options
            var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6, 0, 10, 6) };
            var timelineLabel = new Label
            {
                Text = "Tasks, in the order they run (drag to move; Delete or drag off to remove)",
                Dock = DockStyle.Top, Height = 24, ForeColor = SystemColors.GrayText,
            };
            _timeline.Dock = DockStyle.Top;
            _timeline.Height = 132;

            var taskHeader = new Panel { Dock = DockStyle.Top, Height = 92, Padding = new Padding(0, 8, 0, 0) };
            _taskTitle.SetBounds(0, 10, 600, 24);
            _taskTitle.Font = new Font(Font.FontFamily, 11F, FontStyle.Bold);
            _taskTitle.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _taskDescription.SetBounds(0, 34, 640, 22);
            _taskDescription.ForeColor = SystemColors.GrayText;
            _taskDescription.AutoEllipsis = true;
            _taskDescription.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _useRow.SetBounds(0, 58, 700, 30);
            _useRow.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _useRow.WrapContents = false;
            foreach (RadioButton r in new[] { _useOwn, _useLast, _useAsk })
            {
                r.AutoSize = true;
                r.Margin = new Padding(0, 2, 18, 0);
                r.CheckedChanged += (s, e) => { if (((RadioButton)s).Checked) UseChanged(); };
                _useRow.Controls.Add(r);
            }
            _tips.SetToolTip(_useOwn, "Run the tool with the options below, kept with this PowerButton");
            _tips.SetToolTip(_useLast, "Run the tool with the options it was last used with (as a right-click on its button)");
            _tips.SetToolTip(_useAsk, "Stop at this task and show the tool's options: its Apply carries the PowerButton on");
            taskHeader.Controls.AddRange(new Control[] { _taskTitle, _taskDescription, _useRow });

            _optionsHost.Dock = DockStyle.Fill;
            _optionsHost.AutoScroll = true;
            _optionsHost.BorderStyle = BorderStyle.None;

            right.Controls.Add(_optionsHost);
            right.Controls.Add(taskHeader);
            right.Controls.Add(_timeline);
            right.Controls.Add(timelineLabel);

            Controls.Add(right);
            Controls.Add(left);
            Controls.Add(bottom);
            Controls.Add(top);
            ResumeLayout(true);
        }

        /// <summary>The tools, by group, as tiles with their toolbar icons.</summary>
        private void FillPalette()
        {
            var images = new ImageList { ImageSize = new Size(32, 32), ColorDepth = ColorDepth.Depth32Bit };
            _palette.View = View.Tile;
            _palette.TileSize = new Size(250, 40);
            _palette.LargeImageList = images;
            _palette.ShowItemToolTips = true;
            _palette.MultiSelect = false;
            _palette.HideSelection = false;
            _palette.FullRowSelect = true;

            IEnumerable<ToolInfo> tools = ToolCatalogue.All.Where(InPalette);
            // The text tools first (what most buttons are made of), then styles, the PowerButton extras, ClipX's own
            int Rank(string g) => g == "Styles" ? 1 : g == "PowerButton Extras" ? 2 : g == "ClipX" ? 3 : 0;
            var groups = tools.Select(t => t.Group).Distinct().OrderBy(Rank).ToList(); // a stable sort: the catalogue's order within each
            foreach (string group in groups)
            {
                var g = new ListViewGroup(group, group);
                _palette.Groups.Add(g);
                foreach (ToolInfo t in tools.Where(x => x.Group == group))
                {
                    images.Images.Add(t.Tag, PaletteImage(t));
                    _palette.Items.Add(new ListViewItem(t.Name.Replace("&&", "&"), t.Tag, g) { Tag = t.Tag, ToolTipText = t.Description });
                }
            }
            _palette.ItemDrag += (s, e) =>
            {
                if (e.Item is ListViewItem item) _palette.DoDragDrop(new DataObject(DataFormats.StringFormat, (string)item.Tag), DragDropEffects.Copy);
            };
            _palette.ItemActivate += (s, e) => // double-click or Enter: to the end of the timeline
            {
                if (_palette.SelectedItems.Count == 0) return;
                PowerTask t = NewTask((string)_palette.SelectedItems[0].Tag);
                if (t != null) _timeline.Append(t);
            };
        }

        /// <summary>The tools a PowerButton can use (not the ones about ClipX itself, or that only show something).</summary>
        private bool InPalette(ToolInfo t) => t.Tag switch
        {
            "clipxOPT" or "clipxQSR" or "clipxREF" or "clipxRPT" => false,
            "chSTAT" or "chGRA" or "ccCYC" => false,
            "pbPBT" => _all.Any(b => b.Id != _button.Id),
            _ => true,
        };

        private Image PaletteImage(ToolInfo t)
        {
            var b = new Bitmap(32, 32);
            using Graphics g = Graphics.FromImage(b);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            Image icon = _toolIcon(t.Tag);
            if (icon != null) g.DrawImage(icon, new Rectangle(0, 0, 32, 32));
            else TaskTimeline.DrawLetters(g, new Rectangle(1, 1, 30, 30), t.Name.Replace("&&", "&"));
            return b;
        }

        /************ Name and icon ************/

        private void ShowIconPreview()
        {
            var b = new Bitmap(_iconPreview.Width, _iconPreview.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.Clear(BackColor);
                using Bitmap icon = IconPainter.Render(_button.Icon, 32);
                int o = (b.Width - 32) / 2;
                g.DrawImage(icon, o, o, 32, 32);
                g.DrawRectangle(SystemPens.ControlDark, 0, 0, b.Width - 1, b.Height - 1);
            }
            _iconPreview.Image?.Dispose();
            _iconPreview.Image = b;
        }

        private void EditIcon()
        {
            using var editor = new IconEditor(_button.Icon.Clone());
            if (editor.ShowDialog(this) != DialogResult.OK) return;
            _button.Icon = editor.Result;
            _dirty = true;
            ShowIconPreview();
        }

        private void DeleteButton()
        {
            string name = _name.Text.Trim().Length > 0 ? _name.Text.Trim() : "this PowerButton";
            if (MessageBox.Show(this, $"Delete “{name}” and all its tasks?", "Delete PowerButton",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.OK) return;
            DeleteRequested = true;
            DialogResult = DialogResult.Abort;
        }

        /// <summary>A waste bin, 18 pixels, for "Delete PowerButton" (greyed with the button when it is).</summary>
        private static Bitmap TrashImage()
        {
            var b = new Bitmap(18, 18, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using Graphics g = Graphics.FromImage(b);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Color ink = Color.FromArgb(60, 60, 60);
            using var pen = new Pen(ink, 1.5f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round, LineJoin = System.Drawing.Drawing2D.LineJoin.Round };
            // the bin: tapering a little to the bottom, with three ribs
            PointF[] body = { new PointF(4, 5.5f), new PointF(14, 5.5f), new PointF(13, 16.5f), new PointF(5, 16.5f) };
            using (var fill = new SolidBrush(Color.FromArgb(225, 225, 225))) g.FillPolygon(fill, body);
            g.DrawPolygon(pen, body);
            g.DrawLine(pen, 7, 8.5f, 7.3f, 14);
            g.DrawLine(pen, 9, 8.5f, 9, 14);
            g.DrawLine(pen, 11, 8.5f, 10.7f, 14);
            // the lid and its handle
            g.DrawLine(pen, 2.5f, 3.8f, 15.5f, 3.8f);
            g.DrawLines(pen, new[] { new PointF(7, 3.8f), new PointF(7, 1.8f), new PointF(11, 1.8f), new PointF(11, 3.8f) });
            return b;
        }

        /************ Tasks ************/

        /// <summary>A new task for a tool dropped in: its own options, starting from the tool's last-used ones.</summary>
        private PowerTask NewTask(string tag)
        {
            if (ToolCatalogue.Find(tag) == null) return null;
            var t = new PowerTask(tag, TaskOptions.Own, DefaultOptions(tag));
            return t;
        }

        /// <summary>A copy of the tool's last-used options (what the task starts with), or a sensible value.</summary>
        private object DefaultOptions(string tag)
        {
            AppSettings s = AppSettings.Current;
            switch (tag)
            {
                case "foIND":
                case "foUNI":
                    IndentOptions i = PowerTaskTypes.CopyOf(s.Indent);
                    i.Unindent = tag == "foUNI";
                    return i;
                case "foWRP": return PowerTaskTypes.CopyOf(s.Wordwrap);
                case "foREM": return PowerTaskTypes.CopyOf(s.RemLines);
                case "liBUL": return PowerTaskTypes.CopyOf(s.BulletList);
                case "liNUM": return PowerTaskTypes.CopyOf(s.NumList);
                case "liREN": return PowerTaskTypes.CopyOf(s.NumLines);
                case "edSRC": return FindReplaceRunner.LastUsed(search: true);
                case "edREP": return FindReplaceRunner.LastUsed(search: false);
                case "pbINS": return "";
                case "pbPBT": return null; // chosen in the options
                case "stFNT": return "Segoe UI";
                case "stSIZ": return 11f;
                case "stHDG": return 1;
                case "stCOL": return 0xC00000;
                case "stHIL": return 0xFFFF00;
                default: return null;
            }
        }

        /// <summary>Shows a task's options below the timeline (keeping those of the task shown before).</summary>
        private void ShowTask(PowerTask t)
        {
            SaveOptions();
            _current = t;
            _readOptions = null;
            foreach (Control c in _optionsHost.Controls.Cast<Control>().ToList()) { _optionsHost.Controls.Remove(c); c.Dispose(); }

            _loading = true;
            if (t == null)
            {
                _taskTitle.Text = _timeline.Tasks.Count == 0 ? "No tasks yet" : "";
                _taskDescription.Text = _timeline.Tasks.Count == 0 ? "Drag tools from the list on the left onto the timeline above." : "Click a task to see its options.";
                _useRow.Visible = false;
                _loading = false;
                return;
            }
            int n = _timeline.Tasks.IndexOf(t) + 1;
            _taskTitle.Text = $"Task {n}: {t.Name}";
            _taskDescription.Text = t.Tool?.Description ?? "This tool isn't in this version of ClipX: the PowerButton will stop here. Delete the task, or keep it for later.";
            _tips.SetToolTip(_taskDescription, _taskDescription.Text);

            bool options = PowerTaskTypes.HasOptions(t.Tag);
            _useRow.Visible = options && !PowerTaskTypes.MustBeOwn(t.Tag);
            _useAsk.Enabled = PowerTaskTypes.CanAsk(t.Tag);
            if (!_useAsk.Enabled && t.Use == TaskOptions.Ask) t.Use = TaskOptions.LastUsed;
            if (PowerTaskTypes.MustBeOwn(t.Tag)) t.Use = TaskOptions.Own;
            _useOwn.Checked = t.Use == TaskOptions.Own;
            _useLast.Checked = t.Use == TaskOptions.LastUsed;
            _useAsk.Checked = t.Use == TaskOptions.Ask;

            if (!options)
            {
                _optionsHost.Controls.Add(new Label
                {
                    Text = t.Tool == null ? "" : $"{t.Name} has no options: it just runs.",
                    AutoSize = true, Location = new Point(0, 8), ForeColor = SystemColors.GrayText,
                });
            }
            else ShowOptionControls(t);
            _loading = false;
            _shownOptions = OptionsJson();
            EnableOptions();
        }

        /// <summary>The controls for a task's options: the tool's side panel where it has one, else a few simple ones.</summary>
        private void ShowOptionControls(PowerTask t)
        {
            SidePanelView panel = PanelFor(t.Tag);
            if (panel != null)
            {
                panel.Location = new Point(0, 0);
                panel.Size = new Size(250, panel.Height);
                panel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                _optionsHost.Controls.Add(panel);
                panel.EditOptions(t.Options ?? DefaultOptions(t.Tag));
                if (t.Tag == "edSRC" || t.Tag == "edREP")
                {
                    var actions = FindActionControls(t, out Func<FindAction> action, out Func<bool> backwards);
                    actions.Location = new Point(panel.Right + 16, 8);
                    _optionsHost.Controls.Add(actions);
                    _readOptions = () =>
                    {
                        var f = (FindReplaceTask)panel.EditedOptions;
                        f.Action = action();
                        f.Backwards = backwards();
                        return f;
                    };
                }
                else if (t.Tag == "foUNI" || t.Tag == "foIND")
                    _readOptions = () => { var o = (IndentOptions)panel.EditedOptions; o.Unindent = t.Tag == "foUNI"; return o; };
                else _readOptions = () => panel.EditedOptions;
                return;
            }

            var label = new Label { AutoSize = true, Location = new Point(0, 8) };
            _optionsHost.Controls.Add(label);
            switch (t.Tag)
            {
                case "pbINS":
                    label.Text = "Text to insert at the cursor (or in place of the selection):";
                    var text = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, AcceptsReturn = true, Bounds = new Rectangle(0, 32, 420, 120), Text = t.Options as string ?? "" };
                    _optionsHost.Controls.Add(text);
                    _readOptions = () => text.Text;
                    break;
                case "pbPBT":
                    label.Text = "The PowerButton to run here (all its tasks):";
                    var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Bounds = new Rectangle(0, 32, 320, 28) };
                    var others = _all.Where(b => b.Id != _button.Id).ToList();
                    foreach (PowerButton b in others) combo.Items.Add(b.Name);
                    int at = t.Options is Guid id ? others.FindIndex(b => b.Id == id) : -1;
                    combo.SelectedIndex = at;
                    _optionsHost.Controls.Add(combo);
                    _readOptions = () => combo.SelectedIndex >= 0 ? others[combo.SelectedIndex].Id : (object)null;
                    break;
                case "stFNT":
                    label.Text = "Font:";
                    var fonts = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown, Bounds = new Rectangle(0, 32, 300, 28), AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems };
                    using (var installed = new InstalledFontCollection())
                        foreach (FontFamily f in installed.Families) fonts.Items.Add(f.Name);
                    fonts.Text = t.Options as string ?? "Segoe UI";
                    _optionsHost.Controls.Add(fonts);
                    _readOptions = () => fonts.Text.Trim().Length > 0 ? fonts.Text.Trim() : null;
                    break;
                case "stSIZ":
                    label.Text = "Size, in points:";
                    var size = new NumericUpDown { Minimum = 1, Maximum = 400, DecimalPlaces = 1, Increment = 0.5M, Bounds = new Rectangle(0, 32, 100, 28), Value = (decimal)Math.Max(1f, Math.Min(400f, t.Options is float f0 ? f0 : 11f)) };
                    _optionsHost.Controls.Add(size);
                    _readOptions = () => (float)size.Value;
                    break;
                case "stHDG":
                    label.Text = "Paragraph style:";
                    var heading = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Bounds = new Rectangle(0, 32, 200, 28) };
                    heading.Items.Add("Normal");
                    for (int h = 1; h <= 6; h++) heading.Items.Add("Heading " + h);
                    heading.SelectedIndex = t.Options is int lvl && lvl >= 0 && lvl <= 6 ? lvl : 1;
                    _optionsHost.Controls.Add(heading);
                    _readOptions = () => heading.SelectedIndex;
                    break;
                case "stCOL":
                case "stHIL":
                    bool textColour = t.Tag == "stCOL";
                    label.Text = textColour ? "Text colour:" : "Highlight:";
                    int colour = t.Options is int c ? c : textColour ? 0xC00000 : 0xFFFF00;
                    var swatch = new Button { Bounds = new Rectangle(0, 32, 120, 30), FlatStyle = FlatStyle.Flat, Text = "" };
                    var auto = new CheckBox { Text = textColour ? "Automatic" : "No highlight", AutoSize = true, Location = new Point(132, 36), Checked = colour < 0 };
                    void ShowSwatch() => swatch.BackColor = auto.Checked ? SystemColors.Control : Color.FromArgb(colour | unchecked((int)0xFF000000));
                    swatch.Click += (s, e) =>
                    {
                        using var dialog = new ColorDialog { FullOpen = true, Color = Color.FromArgb(Math.Max(0, colour) | unchecked((int)0xFF000000)) };
                        if (dialog.ShowDialog(this) != DialogResult.OK) return;
                        colour = dialog.Color.ToArgb() & 0xFFFFFF;
                        auto.Checked = false;
                        ShowSwatch();
                    };
                    auto.CheckedChanged += (s, e) => ShowSwatch();
                    ShowSwatch();
                    _optionsHost.Controls.Add(swatch);
                    _optionsHost.Controls.Add(auto);
                    _readOptions = () => auto.Checked ? -1 : colour;
                    break;
                default:
                    label.Text = "";
                    break;
            }
        }

        private static SidePanelView PanelFor(string tag) => tag switch
        {
            "foIND" or "foUNI" => new IndentPanel(),
            "foWRP" => new WordWrapPanel(),
            "foREM" => new RemoveLinesPanel(),
            "liBUL" => new BulletListPanel(),
            "liNUM" => new NumberListPanel(),
            "liREN" => new LineNumbersPanel(),
            "edSRC" or "edREP" => new FindReplacePanel(),
            _ => null,
        };

        /// <summary>Find and Replace: what the task does when the button runs (the panel's buttons, as choices).</summary>
        private Control FindActionControls(PowerTask t, out Func<FindAction> action, out Func<bool> backwards)
        {
            var o = t.Options as FindReplaceTask ?? new FindReplaceTask { Action = t.Tag == "edSRC" ? FindAction.FindNext : FindAction.ReplaceAll };
            var box = new GroupBox { Text = "When the PowerButton runs", Size = new Size(290, 190) };
            var choices = new (FindAction Action, string Text)[]
            {
                (FindAction.ReplaceAll, "Replace &all"),
                (FindAction.ReplaceNext, "Replace the &next one"),
                (FindAction.FindNext, "&Find the next one (select it)"),
                (FindAction.FindAll, "Find all (&highlight them)"),
            };
            var radios = new List<(RadioButton Radio, FindAction Action)>();
            int y = 26;
            foreach (var (a, text) in choices)
            {
                var r = new RadioButton { Text = text, AutoSize = true, Location = new Point(12, y), Checked = o.Action == a };
                box.Controls.Add(r);
                radios.Add((r, a));
                y += 28;
            }
            var back = new CheckBox { Text = "&Backwards (the one before the cursor)", AutoSize = true, Location = new Point(12, y + 6), Checked = o.Backwards };
            box.Controls.Add(back);
            void Enable() => back.Enabled = radios.Any(r => r.Radio.Checked && (r.Action == FindAction.ReplaceNext || r.Action == FindAction.FindNext));
            foreach (var (r, _) in radios) r.CheckedChanged += (s, e) => Enable();
            Enable();
            var note = new Label
            {
                Text = "From start: from the top of the text (else from the cursor). In selection: Replace all and Find all keep to the selection.",
                Location = new Point(0, box.Bottom + 8), Size = new Size(290, 64), ForeColor = SystemColors.GrayText,
            };
            var holder = new Panel { Size = new Size(292, box.Height + 80) };
            holder.Controls.Add(box);
            holder.Controls.Add(note);
            action = () => radios.FirstOrDefault(r => r.Radio.Checked).Action;
            backwards = () => back.Checked && back.Enabled;
            return holder;
        }

        private void UseChanged()
        {
            if (_loading || _current == null) return;
            _current.Use = _useAsk.Checked ? TaskOptions.Ask : _useLast.Checked ? TaskOptions.LastUsed : TaskOptions.Own;
            _dirty = true;
            EnableOptions();
            _timeline.RefreshCards();
        }

        /// <summary>The options can be changed only when the task uses its own.</summary>
        private void EnableOptions()
        {
            bool own = _current != null && _current.Use == TaskOptions.Own;
            foreach (Control c in _optionsHost.Controls) c.Enabled = own;
        }

        /// <summary>Keeps what the options controls show in the task they belong to.</summary>
        private void SaveOptions()
        {
            if (_current == null || _readOptions == null) return;
            try
            {
                _current.Options = _readOptions();
                string now = OptionsJson();
                if (now != _shownOptions) { _dirty = true; _shownOptions = now; }
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is NullReferenceException) { /* the controls are gone */ }
        }

        /// <summary>The options the controls show, as JSON (null if none), to compare.</summary>
        private string OptionsJson()
        {
            if (_readOptions == null) return null;
            try
            {
                object o = _readOptions();
                return o == null ? null : System.Text.Json.JsonSerializer.Serialize(o, o.GetType());
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is NullReferenceException || ex is NotSupportedException) { return null; }
        }

        /// <summary>Unsaved changes: anything changed since the editor opened.</summary>
        internal bool IsDirty
        {
            get
            {
                SaveOptions();
                return _dirty || _name.Text.Trim() != _originalName;
            }
        }

        /************ Try, OK ************/

        private void TryOnText()
        {
            SaveOptions();
            _button.Name = _name.Text.Trim().Length > 0 ? _name.Text.Trim() : _button.Name;
            if (!CheckIncludes()) return;
            _message.ForeColor = SystemColors.ControlText;
            _message.Text = "Run on the text: Ctrl+Z in ClipX's window undoes it.";
            _try?.Invoke(_button);
        }

        private void Ok()
        {
            if (Accept()) DialogResult = DialogResult.OK;
        }

        /// <summary>Checks the button, and keeps the name: true if it can be saved (else why not, in the message).</summary>
        private bool Accept()
        {
            SaveOptions();
            _message.ForeColor = Color.Firebrick;
            string name = _name.Text.Trim();
            if (name.Length == 0)
            {
                _message.Text = "Give the PowerButton a name.";
                _name.Focus();
                return false;
            }
            _button.Name = name;
            if (!CheckIncludes()) return false;
            PowerTask emptyFind = _button.Tasks.FirstOrDefault(t => t.Use == TaskOptions.Own && t.Options is FindReplaceTask f && string.IsNullOrEmpty(f.Find));
            if (emptyFind != null)
            {
                _timeline.SelectedIndex = _button.Tasks.IndexOf(emptyFind);
                _message.Text = $"Task {_button.Tasks.IndexOf(emptyFind) + 1}: type what to find.";
                return false;
            }
            return true;
        }

        /// <summary>Esc: closes as the close icon does (asking, if there are changes); in a drop-down list, closes that.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape && !(ActiveControl is ComboBox { DroppedDown: true }))
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// The close icon (or Esc) with unsaved changes: Save changes, Discard changes, or Cancel
        /// (back to the editor), as ClipX 1's "Confirm close". OK, Cancel and Delete don't ask.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.Cancel || e.CloseReason != CloseReason.UserClosing || DialogResult != DialogResult.Cancel || _cancelClicked) return;
            if (!IsDirty) return;
            string name = _name.Text.Trim().Length > 0 ? $"“{_name.Text.Trim()}”" : "this PowerButton";
            switch (ConfirmCloseDialog.Ask(this, Text, $"Save the changes to {name}?", "Save changes"))
            {
                case CloseChoice.Keep:
                    if (Accept()) DialogResult = DialogResult.OK;
                    else { e.Cancel = true; DialogResult = DialogResult.None; } // why not, in the message
                    break;
                case CloseChoice.Discard:
                    break;
                default:
                    e.Cancel = true;
                    DialogResult = DialogResult.None;
                    break;
            }
        }

        /// <summary>Included PowerButtons: each chosen, and none including this one again (ClipX 1's circular reference check).</summary>
        private bool CheckIncludes()
        {
            _message.ForeColor = Color.Firebrick;
            PowerTask unset = _button.Tasks.FirstOrDefault(t => t.Tag == "pbPBT" && !(t.Options is Guid));
            if (unset != null)
            {
                _timeline.SelectedIndex = _button.Tasks.IndexOf(unset);
                _message.Text = $"Task {_button.Tasks.IndexOf(unset) + 1}: choose the PowerButton to include.";
                return false;
            }
            List<PowerButton> loop = PowerButtonStore.FindLoop(_button, _all);
            if (loop != null)
            {
                _message.Text = "That would go round for ever: " + string.Join(" includes ", loop.Select(b => $"“{b.Name}”")) + ".";
                return false;
            }
            return true;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_button.Tasks.Count == 0) _palette.Focus();
            else _timeline.Focus();
        }
    }
}
