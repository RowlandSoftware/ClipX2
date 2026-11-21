using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClipX2.TextEngine.Tools;

namespace ClipX2
{
    /// <summary>
    /// "What do you want to do?" (the box on the right-hand tab): as the user types, the tools
    /// that best match appear in a list under the box (<see cref="ToolFinder"/>: tool names,
    /// captions, tooltips and synonyms). Up/Down move through it, Enter (or a click) runs the tool
    /// through RunTool, as its button would; Esc closes the list. Alt+Q goes to the box.
    /// </summary>
    public partial class Form1
    {
        private sealed class Suggestion
        {
            public string Tag;          // null: "nothing found"
            public string Name;
            public string Description;
            public override string ToString() => Name;
        }

        private ListBox _suggestions;
        private List<ToolInfo> _searchable;
        private Dictionary<string, ToolStripItem> _buttons; // tag → its button, for what the user sees

        /// <summary>The app's own actions, searchable though not on a tool tab.</summary>
        private static readonly string[] AppActions = { "clipxOPT", "clipxFIN", "clipxMOD", "clipxQSR", "clipxREF", "clipxRPT", "ccCYC" };

        private void InitFind()
        {
            // What can be found: the tools on the toolbars, and the app's actions
            _buttons = new Dictionary<string, ToolStripItem>(StringComparer.Ordinal);
            foreach (ToolStrip strip in ToolStrips.Concat(new[] { toolStrip3, toolStrip1 }))
                foreach (ToolStripItem item in strip.Items)
                    if (item.Tag is string tag && tag.Length > 0 && !_buttons.ContainsKey(tag)) _buttons[tag] = item;
            _searchable = ToolCatalogue.All.Where(t => !t.PowerButtonOnly && (_buttons.ContainsKey(t.Tag) || AppActions.Contains(t.Tag))).ToList();

            _suggestions = new ListBox
            {
                Visible = false,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 36,
                IntegralHeight = false,
                TabStop = false,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f),
            };
            Controls.Add(_suggestions);
            _suggestions.DrawItem += DrawSuggestion;
            _suggestions.MouseMove += (s, e) =>
            {
                int i = _suggestions.IndexFromPoint(e.Location);
                if (i >= 0 && i != _suggestions.SelectedIndex) _suggestions.SelectedIndex = i;
            };
            _suggestions.MouseClick += (s, e) => RunSuggestion(_suggestions.IndexFromPoint(e.Location));

            textBoxWhatToDo.TextChanged += (s, e) => ShowSuggestions();
            textBoxWhatToDo.KeyDown += WhatToDoKeyDown;
            textBoxWhatToDo.Leave += (s, e) => BeginInvoke((Action)(() =>
            {
                if (!_suggestions.Focused && !textBoxWhatToDo.Focused) _suggestions.Visible = false;
            }));
        }

        /// <summary>The tools matching what has been typed, in the list under the box.</summary>
        private void ShowSuggestions()
        {
            string query = textBoxWhatToDo.Text.Trim();
            if (query.Length == 0) { _suggestions.Visible = false; return; }

            List<ToolMatch> found = ToolFinder.Find(query, _searchable, 8, ShownAs);
            _suggestions.BeginUpdate();
            _suggestions.Items.Clear();
            foreach (ToolMatch m in found)
            {
                var (name, description) = ShownAs(m.Tool);
                _suggestions.Items.Add(new Suggestion { Tag = m.Tool.Tag, Name = name, Description = description });
            }
            if (found.Count == 0)
                _suggestions.Items.Add(new Suggestion { Name = "No tools found", Description = "Try other words: what do you want to happen to the text?" });
            _suggestions.EndUpdate();
            _suggestions.SelectedIndex = found.Count > 0 ? 0 : -1;

            // Under the box, kept inside the window
            Point under = PointToClient(textBoxWhatToDo.PointToScreen(new Point(0, textBoxWhatToDo.Height + 1)));
            int width = Math.Max(textBoxWhatToDo.Width, 380);
            int x = Math.Max(4, Math.Min(under.X, ClientSize.Width - width - 4));
            _suggestions.SetBounds(x, under.Y, width, _suggestions.Items.Count * _suggestions.ItemHeight + 2);
            _suggestions.Visible = true;
            _suggestions.BringToFront();
        }

        /// <summary>What the user sees for a tool: its button's caption and tooltip, else the catalogue's.</summary>
        private (string Name, string Description) ShownAs(ToolInfo tool)
        {
            if (_buttons.TryGetValue(tool.Tag, out ToolStripItem item))
            {
                string name = string.IsNullOrWhiteSpace(item.Text) || item.Text.Length <= 2 ? item.AccessibleName ?? tool.Name : item.Text;
                return (name, string.IsNullOrWhiteSpace(item.ToolTipText) ? tool.Description : item.ToolTipText);
            }
            return (tool.Name, tool.Description);
        }

        private void DrawSuggestion(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var s = (Suggestion)_suggestions.Items[e.Index];
            bool selected = (e.State & DrawItemState.Selected) != 0 && s.Tag != null;
            using (var back = new SolidBrush(selected ? SystemColors.Highlight : SystemColors.Window)) e.Graphics.FillRectangle(back, e.Bounds);
            Color fore = selected ? SystemColors.HighlightText : SystemColors.WindowText;
            Color grey = selected ? SystemColors.HighlightText : SystemColors.GrayText;
            using (var bold = new Font(_suggestions.Font, s.Tag != null ? FontStyle.Bold : FontStyle.Italic))
            {
                var line1 = new Rectangle(e.Bounds.X + 6, e.Bounds.Y + 2, e.Bounds.Width - 12, 16);
                var line2 = new Rectangle(e.Bounds.X + 6, e.Bounds.Y + 18, e.Bounds.Width - 12, 16);
                TextRenderer.DrawText(e.Graphics, s.Name, bold, line1, fore, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(e.Graphics, s.Description, _suggestions.Font, line2, grey, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }

        private void WhatToDoKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                case Keys.Up:
                    if (!_suggestions.Visible || _suggestions.Items.Count == 0) return;
                    int i = _suggestions.SelectedIndex + (e.KeyCode == Keys.Down ? 1 : -1);
                    _suggestions.SelectedIndex = Math.Max(0, Math.Min(_suggestions.Items.Count - 1, i));
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Enter:
                    e.SuppressKeyPress = true;
                    if (_suggestions.Visible) RunSuggestion(Math.Max(0, _suggestions.SelectedIndex));
                    break;
                case Keys.Escape:
                    e.SuppressKeyPress = true;
                    if (_suggestions.Visible) _suggestions.Visible = false;
                    else { textBoxWhatToDo.Clear(); richTextBox1.Focus(); }
                    break;
            }
        }

        /// <summary>Runs the suggestion as its button would, and goes back to the text (or to the tool's panel).</summary>
        private void RunSuggestion(int index)
        {
            if (index < 0 || index >= _suggestions.Items.Count) return;
            var s = (Suggestion)_suggestions.Items[index];
            if (s.Tag == null) return; // "No tools found"
            _suggestions.Visible = false;
            textBoxWhatToDo.Clear();
            richTextBox1.Focus();
            RunTool(s.Tag, ToolTrigger.Click);
        }
    }
}
