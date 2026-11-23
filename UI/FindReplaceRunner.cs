using System;
using System.Collections.Generic;
using System.Linq;
using ClipX2.TextEngine;

namespace ClipX2.UI
{
    /// <summary>
    /// Find and Replace without its panel: a PowerButton's edSRC / edREP task (ClipX 1's
    /// doPBSearch). The same engine as the panel (<see cref="TextFinder"/>); Replace All is one
    /// Undo step and keeps the formatting.
    /// </summary>
    internal static class FindReplaceRunner
    {
        /// <summary>
        /// Does what the task says to the text in <paramref name="box"/>. Returns the status
        /// message, and whether it found anything (a PowerButton carries on either way, as ClipX 1).
        /// Throws <see cref="SearchPatternException"/> if the find text can't be used.
        /// </summary>
        public static (string Message, bool Found) Run(ClipXRichTextBox box, FindReplaceTask task)
        {
            if (string.IsNullOrEmpty(task.Find)) return ("Nothing to find", false);
            TextFinder f = TextFinder.Create(task.Find, task.Replace ?? "", task.Search ?? new SearchOptions());
            if (f.IsEmpty) return ("Nothing to find", false);
            string text = box.Text;
            bool inSelection = task.InSelection && box.SelectionLength > 0;
            int start = inSelection ? box.SelectionStart : 0;
            int end = inSelection ? box.SelectionStart + box.SelectionLength : -1;
            string where = inSelection ? " in the selection" : "";

            switch (task.Action)
            {
                case FindAction.ReplaceAll:
                {
                    List<TextEdit> edits = f.ReplaceAll(text, start, end);
                    if (edits.Count == 0) return ("Not found" + where, false);
                    int selStart = box.SelectionStart;
                    box.ApplyEdits(edits);
                    int Moved(int position) => position + edits.Where(e => e.Start + e.Length <= position).Sum(e => e.Replacement.Length - e.Length);
                    if (inSelection) box.Select(start, Moved(end) - start);
                    else box.Select(Moved(selStart), 0);
                    return ((edits.Count == 1 ? "Replaced 1" : $"Replaced {edits.Count:N0}") + where, true);
                }
                case FindAction.FindAll:
                {
                    List<SearchMatch> found = f.FindAll(text, start, end);
                    if (found.Count == 0) return ("Not found" + where, false);
                    box.SetMarkers(found.Select(m => (m.Start, m.Length)));
                    return ((found.Count == 1 ? "1 found" : $"{found.Count:N0} found") + where, true);
                }
                default: // FindNext, ReplaceNext
                {
                    bool forward = !task.Backwards;
                    int from = task.FromStart ? (forward ? 0 : text.Length)
                        : forward ? box.SelectionStart + box.SelectionLength : box.SelectionStart;
                    SearchMatch? m = f.Find(text, from, forward);
                    if (m == null) return ("Not found", false);
                    if (task.Action == FindAction.FindNext)
                    {
                        box.Select(m.Value.Start, m.Value.Length);
                        box.ScrollToCaret();
                        return ("Found", true);
                    }
                    string replacement = f.ReplacementFor(text, m.Value);
                    box.ApplyEdits(new[] { new TextEdit(m.Value.Start, m.Value.Length, replacement) });
                    box.Select(m.Value.Start + replacement.Length, 0);
                    return ("Replaced 1", true);
                }
            }
        }

        /// <summary>The last Find and Replace used in the panel, as a Replace All task (a PowerButton's "Last-used options").</summary>
        public static FindReplaceTask LastUsed(bool search)
        {
            AppSettings s = AppSettings.Current;
            return new FindReplaceTask
            {
                Find = s.FindHistory.FirstOrDefault() ?? "",
                Replace = s.ReplaceHistory.FirstOrDefault() ?? "",
                Search = PowerTaskTypes.CopyOf(s.Search),
                Action = search ? FindAction.FindNext : FindAction.ReplaceAll,
            };
        }
    }
}
