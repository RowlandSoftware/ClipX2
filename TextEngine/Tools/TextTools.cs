using System;
using System.Collections.Generic;
using System.Linq;

namespace ClipX2.TextEngine.Tools
{
    /// <summary>What a tool did: the edits to make (in text order, not overlapping) and any notes.</summary>
    public sealed class ToolResult
    {
        internal ToolResult(string tag, List<TextEdit> edits, IReadOnlyList<string> notes = null)
        {
            Tag = tag;
            Edits = edits ?? new List<TextEdit>();
            Notes = notes ?? Array.Empty<string>();
        }

        public string Tag { get; }
        /// <summary>The changes, in text order and not overlapping: apply them to keep formatting, or use <see cref="ApplyTo"/>.</summary>
        public IReadOnlyList<TextEdit> Edits { get; }
        /// <summary>Anything to tell the user that isn't an edit (punctuation problems, unpaired quotes), in English.</summary>
        public IReadOnlyList<string> Notes { get; }
        public bool Changed => Edits.Count > 0;

        /// <summary>The text with the edits made.</summary>
        public string ApplyTo(string text) => ListFormatter.Apply(text, Edits);
    }

    /// <summary>
    /// Runs a text tool by its tag (see <see cref="ToolCatalogue"/>): the same tools as ClipX's
    /// buttons, for any program using the engine, and for PowerButtons. Each works on
    /// [<c>from</c>, <c>to</c>) of the text (most widen that to whole lines) and returns edits, so
    /// a host can apply them in place and keep its formatting. Options are the tool's own class
    /// (IndentOptions…); null means its defaults. Every run is counted in <see cref="ToolUsage"/>.
    /// </summary>
    public static class TextTools
    {
        private static readonly Dictionary<string, Func<string, int, int, object, ToolResult>> Runners =
            new Dictionary<string, Func<string, int, int, object, ToolResult>>(StringComparer.Ordinal)
            {
                // Change case: the same length, so only changed stretches become edits
                ["ccUPP"] = (t, f, e, o) => Case("ccUPP", t, f, e, CaseMode.Upper),
                ["ccLOW"] = (t, f, e, o) => Case("ccLOW", t, f, e, CaseMode.Lower),
                ["ccREV"] = (t, f, e, o) => Case("ccREV", t, f, e, CaseMode.Reverse),
                ["ccINC"] = (t, f, e, o) => Case("ccINC", t, f, e, CaseMode.InitCaps),
                ["ccTIT"] = (t, f, e, o) => Case("ccTIT", t, f, e, CaseMode.Title),
                ["ccSEN"] = (t, f, e, o) => Case("ccSEN", t, f, e, CaseMode.Sentence),
                ["ccCAP"] = (t, f, e, o) => Case("ccCAP", t, f, e, CaseMode.SentenceCaps),

                // Edit
                ["edSTR"] = (t, f, e, o) => Edits("edSTR", LineFormatter.StripQuoteMarks(t, f, e)),
                ["edPLA"] = (t, f, e, o) => Edits("edPLA", Typography.PlainCharacters(t, f, e)),
                ["edCRL"] = (t, f, e, o) => Curly(t, f, e),
                ["edUDC"] = (t, f, e, o) => Edits("edUDC", TextDecoder.Url(t, f, e)),
                ["edHDC"] = (t, f, e, o) => Edits("edHDC", TextDecoder.Html(t, f, e)),
                ["edUTF"] = (t, f, e, o) => Edits("edUTF", TextDecoder.Utf8(t, f, e)),
                ["edMIM"] = (t, f, e, o) => Edits("edMIM", TextDecoder.Mime(t, f, e)),
                ["edEOL"] = (t, f, e, o) => Edits("edEOL", Typography.RegulariseLineEnds(t, f, e)),

                // Format (whole lines)
                ["foTRM"] = (t, f, e, o) => Edits("foTRM", LineFormatter.Trim(t, f, e, leading: true)),
                ["foTRA"] = (t, f, e, o) => Edits("foTRA", LineFormatter.Trim(t, f, e, leading: false)),
                ["foSSP"] = (t, f, e, o) => Edits("foSSP", LineFormatter.SingleSpace(t, f, e)),
                ["foS2T"] = (t, f, e, o) => Edits("foS2T", LineFormatter.SpacesToTabs(t, f, e)),
                ["foT2S"] = (t, f, e, o) => Edits("foT2S", LineFormatter.TabsToSpaces(t, f, e)),
                ["foIND"] = (t, f, e, o) => Edits("foIND", LineFormatter.Indent(t, f, e, Options<IndentOptions>(o))),
                ["foUNI"] = (t, f, e, o) => Edits("foUNI", LineFormatter.Unindent(t, f, e, Options<IndentOptions>(o))),
                ["foWRP"] = (t, f, e, o) => Edits("foWRP", LineFormatter.WordWrap(t, f, e, Options<WordWrapOptions>(o))),
                ["foREM"] = (t, f, e, o) => Edits("foREM", LineFormatter.RemoveExtraLines(t, f, e, Options<RemoveLinesOptions>(o))),
                ["foPAR"] = (t, f, e, o) => Edits("foPAR", LineFormatter.Paragraph(t, f, e)),

                // Lists (typed, as plain text)
                ["liBUL"] = (t, f, e, o) => Edits("liBUL", ListFormatter.BulletEdits(t, f, e, Options<BulletOptions>(o))),
                ["liNUM"] = (t, f, e, o) => Edits("liNUM", ListFormatter.NumberEdits(t, f, e, Options<NumberOptions>(o))),
                ["liREN"] = (t, f, e, o) => Edits("liREN", LineNumbers.Add(t, f, e, Options<LineNumberOptions>(o))),
                ["liRMB"] = (t, f, e, o) => Edits("liRMB", ListFormatter.RemoveBulletEdits(t, f, e)),
                ["liREM"] = (t, f, e, o) => Edits("liREM", ListFormatter.RemoveNumberEdits(t, f, e)),
                ["liRLN"] = (t, f, e, o) => Edits("liRLN", LineNumbers.Remove(t, f, e)),

                // Language tools
                ["chPNC"] = (t, f, e, o) => Punctuation(t, f, e, o as PunctuationOptions),
            };

        /// <summary>Whether the engine runs this tool (a text tool that has been written).</summary>
        public static bool Supports(string tag) => tag != null && Runners.ContainsKey(tag);

        /// <summary>The tags the engine runs.</summary>
        public static IEnumerable<string> SupportedTags => Runners.Keys;

        /// <summary>
        /// Runs a tool on [<paramref name="from"/>, <paramref name="to"/>) of the text (from = to = 0
        /// with an empty selection is fine: pass 0 and text.Length for the whole text).
        /// </summary>
        /// <exception cref="NotSupportedException">The engine doesn't run this tool (unknown, interactive, or not yet written).</exception>
        public static ToolResult Run(string tag, string text, int from, int to, object options = null)
        {
            if (!Supports(tag))
                throw new NotSupportedException(ToolCatalogue.Find(tag) is ToolInfo t
                    ? $"{t.Name} ({tag}) isn't a text tool the engine runs: the program provides it."
                    : $"There is no tool '{tag}'.");
            text = text ?? "";
            from = Math.Max(0, Math.Min(from, text.Length));
            to = Math.Max(from, Math.Min(to, text.Length));
            ToolResult result = Runners[tag](text, from, to, options);
            ToolUsage.Record(tag);
            return result;
        }

        /// <summary>Runs a tool on the whole text and returns the new text.</summary>
        public static string Run(string tag, string text, object options = null) =>
            Run(tag, text, 0, text?.Length ?? 0, options).ApplyTo(text ?? "");

        private static T Options<T>(object options) where T : new() => options is T t ? t : new T();

        private static ToolResult Edits(string tag, List<TextEdit> edits) => new ToolResult(tag, edits);

        /// <summary>Case changes keep the length: an edit for each stretch that changed.</summary>
        private static ToolResult Case(string tag, string text, int from, int to, CaseMode mode)
        {
            string changed = CaseTransformer.Transform(text, from, to - from, mode);
            var edits = new List<TextEdit>();
            int i = 0;
            while (i < changed.Length)
            {
                if (changed[i] == text[from + i]) { i++; continue; }
                int start = i;
                while (i < changed.Length && changed[i] != text[from + i]) i++;
                edits.Add(new TextEdit(from + start, i - start, changed.Substring(start, i - start)));
            }
            return new ToolResult(tag, edits);
        }

        private static ToolResult Curly(string text, int from, int to)
        {
            Typography.CurlyResult r = Typography.CurlyQuotes(text, from, to);
            var notes = r.Unmatched.Count == 0 ? null
                : new[] { r.Unmatched.Count == 1 ? "A paragraph has unpaired double quotes (fine if a quotation runs on)"
                                                 : $"{r.Unmatched.Count} paragraphs have unpaired double quotes (fine if a quotation runs on)" };
            return new ToolResult("edCRL", r.Edits, notes);
        }

        private static ToolResult Punctuation(string text, int from, int to, PunctuationOptions options)
        {
            PunctuationCheckResult r = PunctuationChecker.Check(text, from, to - from, options);
            var notes = r.Problems.Select(p => p.ToString()).ToList();
            return new ToolResult("chPNC", r.Edits.ToList(), notes);
        }
    }
}
