using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClipX2.TextEngine.Tools
{
    /// <summary>
    /// How often each tool has been run through the engine (<see cref="TextTools.Run"/>), for this
    /// run of the program. Only tool tags and counts: never any text.
    ///
    /// The engine never sends anything anywhere. A program using it may report the counts (with
    /// its users' agreement) to help show which tools people use: <see cref="Report"/> gives them
    /// in a simple form, and <see cref="ToolUsed"/> tells the program as each tool runs. ClipX
    /// keeps its own statistics (it counts button presses, including tools the engine doesn't run).
    /// </summary>
    public static class ToolUsage
    {
        private static readonly object Gate = new object();
        private static readonly Dictionary<string, int> Counts = new Dictionary<string, int>(StringComparer.Ordinal);

        /// <summary>Set false to stop counting.</summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>Raised (on the caller's thread) each time a tool runs, with its tag.</summary>
        public static event EventHandler<string> ToolUsed;

        internal static void Record(string tag)
        {
            if (!Enabled || tag == null) return;
            lock (Gate) Counts[tag] = Counts.TryGetValue(tag, out int n) ? n + 1 : 1;
            ToolUsed?.Invoke(null, tag);
        }

        /// <summary>The counts so far (a copy), by tag.</summary>
        public static IReadOnlyDictionary<string, int> Snapshot()
        {
            lock (Gate) return new Dictionary<string, int>(Counts, StringComparer.Ordinal);
        }

        /// <summary>Starts counting again from nothing (after the counts have been reported, say).</summary>
        public static void Reset()
        {
            lock (Gate) Counts.Clear();
        }

        /// <summary>
        /// The counts as plain text, one "tag count" a line, sorted by tag, after a first line naming
        /// the engine and its version: easy to log, show to the user or send on.
        /// </summary>
        public static string Report()
        {
            var sb = new StringBuilder();
            sb.Append("ClipX2.TextEngine ").Append(typeof(ToolUsage).Assembly.GetName().Version).Append('\n');
            foreach (KeyValuePair<string, int> c in Snapshot().OrderBy(c => c.Key, StringComparer.Ordinal))
                sb.Append(c.Key).Append(' ').Append(c.Value).Append('\n');
            return sb.ToString();
        }
    }
}
