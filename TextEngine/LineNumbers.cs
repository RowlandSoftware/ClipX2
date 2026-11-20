using System;
using System.Collections.Generic;
using Line = ClipX2.TextEngine.ListFormatter.Line;

namespace ClipX2.TextEngine
{
    /// <summary>Line numbers options (ClipX 1's NumLines settings: same names).</summary>
    public sealed class LineNumberOptions
    {
        /// <summary>The first line's number.</summary>
        public int Start { get; set; } = 1;
        /// <summary>The increment from one line to the next.</summary>
        public int Step { get; set; } = 1;
        /// <summary>Show the number on every nth line only (the others are still counted, and get spaces).</summary>
        public int Every { get; set; } = 1;
        /// <summary>Blank lines get no number and aren't counted.</summary>
        public bool IgnoreBlanks { get; set; } = true;
        /// <summary>All numbers the same width (padded with spaces), so the text lines up.</summary>
        public bool Pad { get; set; } = true;
        /// <summary>With Pad: numbers right-aligned (as 9, 10); otherwise left-aligned.</summary>
        public bool RangeRight { get; set; } = true;
        /// <summary>Lines already numbered lose their old numbers first.</summary>
        public bool Renumber { get; set; } = true;
    }

    /// <summary>
    /// Line numbers (liREN) and Remove line numbers (liRLN): a number and a space at the start of
    /// each line (paragraph). Removing puts the text back where it was, indentation included, so
    /// numbering and then removing gives the original text.
    /// </summary>
    public static class LineNumbers
    {
        /// <summary>Numbers the lines touching [from, to): edits in text order.</summary>
        public static List<TextEdit> Add(string text, int from, int to, LineNumberOptions o)
        {
            var edits = new List<TextEdit>();
            text = text ?? "";
            List<Line> lines = ListFormatter.LinesIn(text, from, to);
            int step = Math.Max(1, o.Step), every = Math.Max(1, o.Every);

            // The old numbers (if renumbering), and which lines take a number
            int[] old = o.Renumber ? Prefixes(text, lines) : new int[lines.Count];
            var counted = new bool[lines.Count];
            int count = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                counted[i] = !o.IgnoreBlanks || !IsBlank(text, lines[i].Start + old[i], lines[i].End);
                if (counted[i]) count++;
            }
            if (count == 0) return edits;

            long last = o.Start + (long)(count - 1) * step;
            int width = Math.Max(o.Start.ToString().Length, last.ToString().Length);
            long number = o.Start;
            int n = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                Line line = lines[i];
                bool blank = IsBlank(text, line.Start + old[i], line.End);
                string prefix = "";
                if (counted[i])
                {
                    n++;
                    string digits = number.ToString();
                    string field = !o.Pad ? digits
                        : o.RangeRight ? digits.PadLeft(width) : digits.PadRight(width);
                    if (n % every != 0) field = new string(' ', field.Length); // counted, not shown
                    // A blank line: the number alone (no trailing space)
                    prefix = blank ? field.TrimEnd() : field + " ";
                    number += step;
                }
                // A blank line keeps its spaces only if it isn't numbered
                int remove = old[i] + (blank && prefix.Length > 0 ? line.End - (line.Start + old[i]) : 0);
                if (remove > 0 || prefix.Length > 0)
                    edits.Add(new TextEdit(line.Start, remove, prefix));
            }
            return edits;
        }

        /// <summary>Takes the line numbers off the lines touching [from, to): edits in text order.</summary>
        public static List<TextEdit> Remove(string text, int from, int to)
        {
            var edits = new List<TextEdit>();
            text = text ?? "";
            List<Line> lines = ListFormatter.LinesIn(text, from, to);
            int[] prefixes = Prefixes(text, lines);
            for (int i = 0; i < lines.Count; i++)
                if (prefixes[i] > 0) edits.Add(new TextEdit(lines[i].Start, prefixes[i], ""));
            return edits;
        }

        /// <summary>
        /// The length of each line's line number and its separator (0 if none). The numbers' layout
        /// is worked out from the numbered lines together: padded on the right (all the same width,
        /// spaces before the shorter ones), padded on the left (spaces after), or unpadded. Lines
        /// between the numbers (numbering every nth line) lose the same width of spaces, and no
        /// more, so indentation after the number is kept.
        /// </summary>
        private static int[] Prefixes(string text, List<Line> lines)
        {
            var result = new int[lines.Count];
            var field = new int[lines.Count]; // spaces + digits, 0 if not numbered
            int widest = 0, numbered = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                field[i] = NumberField(text, lines[i]);
                if (field[i] > 0) { numbered++; widest = Math.Max(widest, field[i]); }
            }
            if (numbered == 0) return result;

            // Right-padded (or all the same width): every field is the widest. Left-padded: each
            // shorter number is followed by enough spaces to make it up.
            bool sameWidth = true, leftPadded = true;
            for (int i = 0; i < lines.Count; i++)
            {
                if (field[i] == 0) continue;
                if (field[i] != widest) sameWidth = false;
                int pos = lines[i].Start + field[i];
                if (pos < lines[i].End && Spaces(text, pos, lines[i].End) < widest - field[i] + 1) leftPadded = false;
            }
            // Left-padding could be indentation after unpadded numbers: when padded, the lines
            // between numbers (every nth line) start with the full width of spaces
            if (leftPadded && !sameWidth)
                for (int i = 0; i < lines.Count; i++)
                    if (field[i] == 0 && !IsBlank(text, lines[i].Start, lines[i].End)
                        && Spaces(text, lines[i].Start, lines[i].End) < widest + 1) { leftPadded = false; break; }
            bool padded = sameWidth || leftPadded;

            for (int i = 0; i < lines.Count; i++)
            {
                Line line = lines[i];
                if (field[i] > 0)
                {
                    int pos = line.Start + field[i];
                    if (pos == line.End) { result[i] = field[i]; continue; }       // a numbered blank line
                    if (text[pos] == '\t') { result[i] = field[i] + 1; continue; } // number, tab
                    int separator = padded ? widest - field[i] + 1 : 1;
                    result[i] = field[i] + Math.Min(separator, Spaces(text, pos, line.End));
                }
                else if (!IsBlank(text, line.Start, line.End))
                {
                    // Between numbers: the width of the number field there, in spaces
                    int width = padded ? widest + 1 : NearestField(field, i) + 1;
                    result[i] = Math.Min(width, Spaces(text, line.Start, line.End));
                }
            }
            return result;
        }

        /// <summary>Spaces and digits at the start of a line followed by a space, tab or the line end: its length, or 0.</summary>
        private static int NumberField(string text, Line line)
        {
            int pos = line.Start;
            while (pos < line.End && text[pos] == ' ') pos++;
            int digits = pos;
            while (pos < line.End && text[pos] >= '0' && text[pos] <= '9') pos++;
            if (pos == digits) return 0;
            if (pos < line.End && text[pos] != ' ' && text[pos] != '\t') return 0;
            return pos - line.Start;
        }

        /// <summary>The number field of the nearest numbered line before (or else after) line i.</summary>
        private static int NearestField(int[] field, int i)
        {
            for (int j = i - 1; j >= 0; j--) if (field[j] > 0) return field[j];
            for (int j = i + 1; j < field.Length; j++) if (field[j] > 0) return field[j];
            return 0;
        }

        private static int Spaces(string text, int from, int to)
        {
            int n = 0;
            while (from + n < to && text[from + n] == ' ') n++;
            return n;
        }

        private static bool IsBlank(string text, int from, int to)
        {
            for (int i = from; i < to; i++)
                if (text[i] != ' ' && text[i] != '\t') return false;
            return true;
        }
    }
}
