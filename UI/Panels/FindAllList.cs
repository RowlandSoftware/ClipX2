using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ClipX2.TextEngine;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Find All's list of matches (ClipX 1's list, without line numbers): each match in its
    /// context (bullets kept; indents and runs of spaces as one space, so more of the text shows). It lies over the Find and Replace panel
    /// until closed (its close cross, Esc, or a click in the text); closing it leaves the panel
    /// and the highlights as they are. A click on a match scrolls the text to it and shows it as
    /// the current one (a stronger highlight), without selecting it, so you can go down the list
    /// until you find the one you want.
    /// </summary>
    public partial class FindAllList : SidePanelView
    {
        /// <summary>Characters of context each side of a match (within its paragraph).</summary>
        private const int Context = 40;

        private ClipXRichTextBox _box;
        private readonly List<Item> _items = new List<Item>();
        private Font _bold;

        private sealed class Item
        {
            public int Start, Length;
            public string Before, Match, After;
        }

        public FindAllList()
        {
            InitializeComponent();
            Disposed += (s, e) => _bold?.Dispose();
            listMatches.DrawItem += DrawMatch;
            listMatches.SelectedIndexChanged += (s, e) => ShowInText(listMatches.SelectedIndex);
            listMatches.MouseDown += (s, e) =>
            {
                // A click on the match already chosen: show it again (the text may have been scrolled away)
                int i = listMatches.IndexFromPoint(e.Location);
                if (i >= 0 && i == listMatches.SelectedIndex) ShowInText(i);
            };
        }

        /// <summary>
        /// Fills the list: the matches in <paramref name="text"/>, with <paramref name="summary"/>
        /// ("12 found") above. <paramref name="listLabels"/> (rich text): each paragraph's live
        /// bullet or number, shown where the context starts at its paragraph's start, as a typed
        /// one is in plain text.
        /// </summary>
        internal void ShowMatches(ClipXRichTextBox box, string text, IReadOnlyList<SearchMatch> matches, string summary, string[] listLabels = null)
        {
            _box = box;
            _items.Clear();
            int paragraph = 0, counted = 0; // the paragraph number at position 'counted' (matches come in text order)
            foreach (SearchMatch m in matches)
            {
                int end = Math.Min(text.Length, m.Start + m.Length);
                int from = Math.Max(0, m.Start - Context), to = Math.Min(text.Length, end + Context);
                // Context within the paragraph only (a match may itself cross a paragraph break)
                for (int k = m.Start - 1; k >= from; k--) if (text[k] == '\n' || text[k] == '\r') { from = k + 1; break; }
                for (int k = end; k < to; k++) if (text[k] == '\n' || text[k] == '\r') { to = k; break; }
                string label = null;
                if (listLabels != null)
                {
                    for (; counted < m.Start && counted < text.Length; counted++) if (text[counted] == '\n' || text[counted] == '\r') paragraph++;
                    bool atStart = from == 0 || text[from - 1] == '\n' || text[from - 1] == '\r';
                    if (atStart && paragraph < listLabels.Length && !string.IsNullOrEmpty(listLabels[paragraph])) label = listLabels[paragraph] + " ";
                }
                _items.Add(new Item
                {
                    Start = m.Start, Length = m.Length,
                    Before = label + OneLine(text.Substring(from, m.Start - from)).TrimStart(), // no indent; a live bullet or number first
                    Match = OneLine(text.Substring(m.Start, end - m.Start)),
                    After = OneLine(text.Substring(end, to - end)).TrimEnd(),
                });
            }
            listMatches.BeginUpdate();
            listMatches.Items.Clear();
            for (int i = 0; i < _items.Count; i++) listMatches.Items.Add(i);
            listMatches.EndUpdate();
            labelCount.Text = summary;
            _box.CurrentMark = -1;
        }

        /// <summary>The match number <paramref name="index"/> scrolled into view and shown as the current one (not selected).</summary>
        private void ShowInText(int index)
        {
            if (_box == null || index < 0 || index >= _items.Count) return;
            _box.CurrentMark = index;
            _box.ScrollToShow(_items[index].Start);
        }

        /// <summary>
        /// On one line, with room for as much text as possible: a run of spaces and tabs (an
        /// indent, the tab after a bullet) as one space; ¶ for a break inside a match.
        /// </summary>
        private static string OneLine(string s)
        {
            var sb = new System.Text.StringBuilder(s.Length);
            foreach (char c in s)
            {
                if (c == '\r' || c == '\n' || c == '\v' || c == '\f')
                {
                    if (sb.Length == 0 || sb[sb.Length - 1] != '¶') sb.Append('¶');
                }
                else if (char.IsWhiteSpace(c) || char.IsControl(c))
                {
                    if (sb.Length == 0 || sb[sb.Length - 1] != ' ') sb.Append(' ');
                }
                else sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// One match, from the left: the text before it, the match in bold on the highlight
        /// colour, then the text after it. If the text before is too long to leave the match and
        /// some of what follows in view, its start is cut ("…").
        /// </summary>
        private void DrawMatch(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _items.Count) return;
            Item item = _items[e.Index];
            bool selected = (e.State & DrawItemState.Selected) != 0;
            e.DrawBackground();
            Graphics g = e.Graphics;
            _bold ??= new Font(listMatches.Font, FontStyle.Bold);
            Color ink = selected ? SystemColors.HighlightText : SystemColors.WindowText;
            Color faint = selected ? SystemColors.HighlightText : SystemColors.GrayText;
            const TextFormatFlags Flags = TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;
            Rectangle r = e.Bounds;
            int left = r.Left + 4, width = Math.Max(1, r.Width - 8);
            Size Measure(string s, Font f) => TextRenderer.MeasureText(g, s, f, new Size(int.MaxValue, r.Height), Flags);
            int matchW = Math.Min(Measure(item.Match, _bold).Width, width);

            // Before it: all of it if the match and a quarter of the width after it still fit;
            // otherwise its end, cut at the start with "…"
            string before = item.Before;
            int room = Math.Max(0, width - matchW - width / 4);
            if (before.Length > 0 && Measure(before, listMatches.Font).Width > room)
            {
                int cut = 0;
                while (cut < before.Length && Measure("…" + before.Substring(cut), listMatches.Font).Width > room) cut++;
                before = cut < before.Length ? "…" + before.Substring(cut) : "";
            }
            int beforeW = before.Length > 0 ? Measure(before, listMatches.Font).Width : 0;
            if (beforeW > 0) TextRenderer.DrawText(g, before, listMatches.Font, new Rectangle(left, r.Top, beforeW, r.Height), faint, Flags);

            // The match
            int matchX = left + beforeW + (beforeW > 0 ? 2 : 0);
            matchW = Math.Min(matchW, r.Right - 4 - matchX);
            var matchRect = new Rectangle(matchX, r.Top, matchW, r.Height);
            if (!selected)
                using (var marker = new SolidBrush(Color.FromArgb(255, 236, 150)))
                    g.FillRectangle(marker, new Rectangle(matchX - 1, r.Top + 3, matchW + 2, r.Height - 6));
            TextRenderer.DrawText(g, item.Match, _bold, matchRect, ink, Flags | TextFormatFlags.EndEllipsis);

            // After it
            var afterRect = Rectangle.FromLTRB(matchRect.Right + 2, r.Top, r.Right - 4, r.Bottom);
            if (afterRect.Width > 0)
                TextRenderer.DrawText(g, item.After, listMatches.Font, afterRect, faint, Flags | TextFormatFlags.EndEllipsis);
            e.DrawFocusRectangle();
        }

        /// <summary>The list has the keys while it is shown: Up and Down go through the matches.</summary>
        protected internal override void OnActivated()
        {
            if (listMatches.Items.Count > 0) listMatches.Focus();
        }
    }
}
