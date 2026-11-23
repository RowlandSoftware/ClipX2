using System.Collections.Generic;

namespace ClipX2.TextEngine.Export
{
    /// <summary>
    /// The names LibreOffice Writer gives its own styles in RTF, for the RTF ClipX writes for Writer.
    /// Writer reads an RTF style by its RTF name (its own export's: "Body Text", "Strong", "heading 1");
    /// one named by Writer's internal or displayed name instead ("Text body", as a Word document that
    /// began in Writer has it) clashes with the style Writer already has and comes in as a copy,
    /// "Text body (WW)". So these names are translated in the Writer version.
    /// Taken from Writer 26.8: every built-in style used once and exported as RTF.
    /// </summary>
    public static class WriterStyleNames
    {
        private static readonly Dictionary<string, string> Paragraph = new Dictionary<string, string>
        {
            { "Quotations", "Block Quotation" },
            { "Text body", "Body Text" },
            { "First line indent", "Body Text First Indent" },
            { "First Line Indent", "Body Text First Indent" },
            { "Text body indent", "Body Text Indent" },
            { "Body Text, Indented", "Body Text Indent" },
            { "Appendix", "Closing" },
            { "Footer left", "Footer Left" },
            { "Footer right", "Footer Right" },
            { "Frame contents", "Frame Contents" },
            { "Hanging indent", "Hanging Indent" },
            { "Header left", "Header Left" },
            { "Header right", "Header Right" },
            { "List 1", "List Bullet" },
            { "List 2", "List Bullet 2" },
            { "List 3", "List Bullet 3" },
            { "List 4", "List Bullet 4" },
            { "List 5", "List Bullet 5" },
            { "List 1 Cont.", "List Continue" },
            { "List 2 Cont.", "List Continue 2" },
            { "List 3 Cont.", "List Continue 3" },
            { "List 4 Cont.", "List Continue 4" },
            { "List 5 Cont.", "List Continue 5" },
            { "Numbering 1", "List Number" },
            { "Numbering 2", "List Number 2" },
            { "Numbering 3", "List Number 3" },
            { "Numbering 4", "List Number 4" },
            { "Numbering 5", "List Number 5" },
            { "Standard", "Normal" },
            { "Default Paragraph Style", "Normal" },
            { "Object index 1", "Object Index 1" },
            { "Object index heading", "Object Index Heading" },
            { "Complimentary Close", "Salutation" },
            { "Contents Heading", "TOC Heading" },
            { "Table index 1", "Table Index 1" },
            { "Table index heading", "Table Index Heading" },
            { "Marginalia", "annotation text" },
            { "Caption", "caption" },
            { "Endnote", "endnote text" },
            { "Addressee", "envelope address" },
            { "Sender", "envelope return" },
            { "Footer", "footer" },
            { "Footnote", "footnote text" },
            { "Header", "header" },
            { "Heading 1", "heading 1" },
            { "Heading 2", "heading 2" },
            { "Heading 3", "heading 3" },
            { "Heading 4", "heading 4" },
            { "Heading 5", "heading 5" },
            { "Heading 6", "heading 6" },
            { "Heading 7", "heading 7" },
            { "Heading 8", "heading 8" },
            { "Heading 9", "heading 9" },
            { "Index 1", "index 1" },
            { "Index 2", "index 2" },
            { "Index 3", "index 3" },
            { "Index Heading", "index heading" },
            { "Bibliography Heading", "table of authorities" },
            { "Figure Index 1", "table of figures" },
            { "Contents 1", "toc 1" },
            { "Contents 2", "toc 2" },
            { "Contents 3", "toc 3" },
            { "Contents 4", "toc 4" },
            { "Contents 5", "toc 5" },
            { "Contents 6", "toc 6" },
            { "Contents 7", "toc 7" },
            { "Contents 8", "toc 8" },
            { "Contents 9", "toc 9" },
        };

        private static readonly Dictionary<string, string> Character = new Dictionary<string, string>
        {
            { "Bullet Symbols", "Bullets" },
            { "Caption characters", "Caption Characters" },
            { "Endnote Symbol", "Endnote Characters" },
            { "Visited Internet Link", "FollowedHyperlink" },
            { "Footnote Symbol", "Footnote Characters" },
            { "Internet link", "Hyperlink" },
            { "Internet Link", "Hyperlink" },
            { "Main index entry", "Main Index Entry" },
            { "Citation", "Quotation" },
            { "Strong Emphasis", "Strong" },
            { "Endnote anchor", "endnote reference" },
            { "Endnote Anchor", "endnote reference" },
            { "Footnote anchor", "footnote reference" },
            { "Footnote Anchor", "footnote reference" },
            { "Line numbering", "line number" },
            { "Line Numbering", "line number" },
            { "Page Number", "page number" },
        };

        /// <summary>The name Writer's RTF gives the style named <paramref name="name"/>; null if it's the same.</summary>
        public static string RtfName(string name, bool character)
        {
            if (name == null) return null;
            return (character ? Character : Paragraph).TryGetValue(name.Trim(), out string rtf) ? rtf : null;
        }
    }
}
