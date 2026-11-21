using System;
using System.Collections.Generic;
using System.Linq;

namespace ClipX2.TextEngine.Tools
{
    /// <summary>What a tool needs in order to run.</summary>
    public enum ToolKind
    {
        /// <summary>Text in, edits out: the engine runs it (<see cref="TextTools.Run"/>), with or without options.</summary>
        Text,
        /// <summary>Works with the user (spelling, search, statistics): the host program provides it.</summary>
        Interactive,
        /// <summary>Acts on the editor rather than the text (select all, copy, paste): PowerButton extras, from the host.</summary>
        Editor,
    }

    /// <summary>One tool: its tag (ClipX 1's token, never changed), name, group and what it needs.</summary>
    public sealed class ToolInfo
    {
        internal ToolInfo(string tag, string name, string group, ToolKind kind, Type options, string description, bool powerButtonOnly = false)
        {
            Tag = tag; Name = name; Group = group; Kind = kind; OptionsType = options; Description = description; PowerButtonOnly = powerButtonOnly;
        }

        /// <summary>ClipX 1's token, e.g. "ccUPP": the tool's permanent identity (buttons, PowerButtons, usage counts).</summary>
        public string Tag { get; }
        /// <summary>English name, e.g. "UPPER CASE". (ClipX shows its own translated names.)</summary>
        public string Name { get; }
        /// <summary>The group (tab) it belongs to: "Change Case", "Edit", "Format"…</summary>
        public string Group { get; }
        public ToolKind Kind { get; }
        /// <summary>The options class it takes (IndentOptions, WordWrapOptions…), or null if none.</summary>
        public Type OptionsType { get; }
        public bool HasOptions => OptionsType != null;
        /// <summary>English description (ClipX 1's).</summary>
        public string Description { get; }
        /// <summary>Only for PowerButtons (no button of its own).</summary>
        public bool PowerButtonOnly { get; }

        public override string ToString() => $"{Tag} ({Name})";
    }

    /// <summary>
    /// Every ClipX tool, by tag. The tags are ClipX 1's tokens and never change: ClipX's buttons,
    /// PowerButtons, usage statistics and programs using this engine all name tools by them.
    /// </summary>
    public static class ToolCatalogue
    {
        private const string Extras = "PowerButton Extras", Case = "Change Case", Edit = "Edit", Format = "Format",
            Lists = "Lists", Sorting = "Sorting", Language = "Language Tools", ClipX = "ClipX";

        private static readonly ToolInfo[] Tools =
        {
            new ToolInfo("pbCLR", "Clear selection", Extras, ToolKind.Editor, null, "Deselect all text", true),
            new ToolInfo("pbALL", "Select all", Extras, ToolKind.Editor, null, "Select all text", true),
            new ToolInfo("pbCPY", "Copy to clipboard", Extras, ToolKind.Editor, null, "Copy selected text to clipboard", true),
            new ToolInfo("pbCUT", "Cut to clipboard", Extras, ToolKind.Editor, null, "Cut selected text to clipboard", true),
            new ToolInfo("pbPST", "Paste from clipboard", Extras, ToolKind.Editor, null, "Paste from clipboard", true),
            new ToolInfo("pbINS", "Insert text", Extras, ToolKind.Editor, typeof(string), "Insert text at cursor position", true),
            new ToolInfo("pbDEL", "Delete selected text", Extras, ToolKind.Editor, null, "Delete the selected text (if any)", true),
            new ToolInfo("pbPBT", "PowerButton", Extras, ToolKind.Editor, typeof(string), "Include another PowerButton in this one", true),

            new ToolInfo("ccUPP", "UPPER CASE", Case, ToolKind.Text, null, "All capitals"),
            new ToolInfo("ccLOW", "lower case", Case, ToolKind.Text, null, "All small letters"),
            new ToolInfo("ccREV", "rEVERSE cASE", Case, ToolKind.Text, null, "Swops case"),
            new ToolInfo("ccINC", "Initial Caps", Case, ToolKind.Text, null, "Initial letter of each word capital"),
            new ToolInfo("ccTIT", "Title Case", Case, ToolKind.Text, null, "Proper case - initial letter of each main word capital"),
            new ToolInfo("ccSEN", "Sentence Case", Case, ToolKind.Text, null, "Changes text to lower case, initial letter of each sentence capital"),
            new ToolInfo("ccCAP", "Capitalise sentences", Case, ToolKind.Text, null, "Initial letter of each sentence capital, remaining letters unchanged"),
            new ToolInfo("ccCYC", "Cycle case", Case, ToolKind.Interactive, null, "Step the selection through Sentence case, Initial Caps, UPPER CASE and lower case (Shift+F3)"),

            new ToolInfo("edSTR", "Strip email characters", Edit, ToolKind.Text, null, "Removes indent character (usually >) from start of each line"),
            new ToolInfo("edPLA", "Plain characters", Edit, ToolKind.Text, null, "Replace special characters such as curly quotes with plain characters"),
            new ToolInfo("edCRL", "Curly quotes etc.", Edit, ToolKind.Text, null, "Replace plain quotes with curly ones ('66 and 99'), en- and em-dashes and three dots"),
            new ToolInfo("edUDC", "Decode URL characters", Edit, ToolKind.Text, null, "Replace characters encoded in URLs, e.g. %20"),
            new ToolInfo("edHDC", "Decode HTML characters", Edit, ToolKind.Text, null, "Decode HTML character entities, e.g. &#169;"),
            new ToolInfo("edUTF", "Decode UTF-8", Edit, ToolKind.Text, null, "Decode UTF-8 encoded characters, e.g. â€™"),
            new ToolInfo("edMIM", "Decode MIME", Edit, ToolKind.Text, null, "Decode quotable printed MIME 1.0 encoded characters, e.g. =30=20"),
            new ToolInfo("edEOL", "Regularise line ends", Edit, ToolKind.Text, null, "Replace LF and CR with standard line ending"),
            new ToolInfo("edSRC", "Search", Edit, ToolKind.Interactive, null, "Search"),
            new ToolInfo("edREP", "Replace", Edit, ToolKind.Interactive, null, "Search and replace"),

            new ToolInfo("foTRM", "Trim white space", Format, ToolKind.Text, null, "Remove white space from start and end of lines"),
            new ToolInfo("foTRA", "Trim trailing space", Format, ToolKind.Text, null, "Remove white space from end of lines only"),
            new ToolInfo("foSSP", "Single space", Format, ToolKind.Text, null, "Replace multiple spaces with single space"),
            new ToolInfo("foS2T", "Spaces to tab", Format, ToolKind.Text, null, "Convert multiple spaces to single tab character"),
            new ToolInfo("foT2S", "Tabs to spaces", Format, ToolKind.Text, null, "Replace tabs with spaces to form columns"),
            new ToolInfo("foIND", "Indent", Format, ToolKind.Text, typeof(IndentOptions), "Indent text one level"),
            new ToolInfo("foUNI", "Unindent", Format, ToolKind.Text, typeof(IndentOptions), "Remove one level of indenting"),
            new ToolInfo("foWRP", "Word wrap", Format, ToolKind.Text, typeof(WordWrapOptions), "Split paragraphs into lines with optional hanging indents and full justification"),
            new ToolInfo("foREM", "Remove extra lines", Format, ToolKind.Text, typeof(RemoveLinesOptions), "Remove blank lines; make multiple blank lines into single ones"),
            new ToolInfo("foPAR", "Paragraph", Format, ToolKind.Text, null, "Un-wordwrap: concatenate lines into single paragraphs; make multiple blank lines into single ones"),

            new ToolInfo("liBUL", "Bulleted", Lists, ToolKind.Text, typeof(BulletOptions), "Unordered list with configurable bullet points"),
            new ToolInfo("liNUM", "Numbered", Lists, ToolKind.Text, typeof(NumberOptions), "Ordered list with configurable numbering scheme"),
            new ToolInfo("liREN", "Line numbers", Lists, ToolKind.Text, typeof(LineNumberOptions), "Add line numbers"),
            new ToolInfo("liRMB", "Remove bullets", Lists, ToolKind.Text, null, "Remove bullets"),
            new ToolInfo("liREM", "Remove numbers", Lists, ToolKind.Text, null, "Remove numbering"),
            new ToolInfo("liRLN", "Remove line numbers", Lists, ToolKind.Text, null, "Remove line numbering"),

            new ToolInfo("soALP", "Alphabetic", Sorting, ToolKind.Text, null, "Sort paragraphs alphabetically"),
            new ToolInfo("soNUM", "Numeric", Sorting, ToolKind.Text, null, "Sort paragraphs numerically"),
            new ToolInfo("soREV", "Reverse", Sorting, ToolKind.Text, null, "Reverse order of selected paragraphs"),
            new ToolInfo("soNUP", "Nudge up", Sorting, ToolKind.Text, null, "Move current line or selection up one line"),
            new ToolInfo("soNUD", "Nudge down", Sorting, ToolKind.Text, null, "Move current line or selection down one line"),
            new ToolInfo("soKEY", "List sort", Sorting, ToolKind.Text, null, "Sort comma separated list on a line alphabetically"),

            new ToolInfo("chPNC", "Punctuation", Language, ToolKind.Text, typeof(PunctuationOptions), "Check spaces around punctuation, match brackets and quotes"),
            new ToolInfo("chSPE", "Spelling", Language, ToolKind.Interactive, null, "Spelling and grammar checker"),
            new ToolInfo("chGRA", "Spelling && Grammar", Language, ToolKind.Interactive, null, "Spelling and grammar checker"),
            new ToolInfo("chSTAT", "Statistics", Language, ToolKind.Interactive, null, "Document statistics: word count etc."),

            new ToolInfo("clipxRPT", "Repeat last tool", ClipX, ToolKind.Interactive, null, "Run the last tool used again, with its last settings (Ctrl+R)"),
        };

        private static readonly Dictionary<string, ToolInfo> ByTag = Tools.ToDictionary(t => t.Tag, StringComparer.Ordinal);

        /// <summary>All the tools, in ClipX 1's order (by group).</summary>
        public static IReadOnlyList<ToolInfo> All => Tools;

        /// <summary>The tool with this tag, or null.</summary>
        public static ToolInfo Find(string tag) => tag != null && ByTag.TryGetValue(tag, out ToolInfo t) ? t : null;

        /// <summary>Whether the engine can run this tool itself (<see cref="TextTools.Run"/>).</summary>
        public static bool CanRun(string tag) => TextTools.Supports(tag);
    }
}
