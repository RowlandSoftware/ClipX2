using System.Collections.Generic;

namespace ClipX2.TextEngine.Export
{
    /// <summary>
    /// A simple, editor-independent model of formatted text: paragraphs made of runs of characters
    /// that share the same formatting. A UI fills it from its editor (e.g. a RichTextBox), and
    /// <see cref="DocumentExport"/> turns it into HTML or plain text.
    /// </summary>
    public sealed class FormattedDocument
    {
        public List<FormattedParagraph> Paragraphs { get; } = new List<FormattedParagraph>();
    }

    /// <summary>One paragraph (ClipX's "line"), without its paragraph mark.</summary>
    public sealed class FormattedParagraph
    {
        public List<FormattedRun> Runs { get; } = new List<FormattedRun>();

        public ListKind ListKind { get; set; }
        public ListNumberStyle ListNumberStyle { get; set; }

        /// <summary>The number the list starts at (for lettered and Roman lists too: 3 = c / iii).</summary>
        public int ListStart { get; set; } = 1;

        /// <summary>Nesting level of a list item, 0 for the outermost.</summary>
        public int ListLevel { get; set; }

        public ParagraphAlignment Alignment { get; set; }

        /// <summary>Left indent in points.</summary>
        public float LeftIndent { get; set; }

        /// <summary>First-line indent in points, relative to <see cref="LeftIndent"/> (negative for a hanging indent).</summary>
        public float FirstLineIndent { get; set; }
    }

    /// <summary>Characters sharing the same formatting. A soft line break inside a paragraph is '\v'.</summary>
    public sealed class FormattedRun
    {
        public string Text { get; set; } = "";
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public bool Strikethrough { get; set; }
        public bool Superscript { get; set; }
        public bool Subscript { get; set; }

        /// <summary>Font name, or null for the default.</summary>
        public string FontName { get; set; }

        /// <summary>Font size in points, or 0 for the default.</summary>
        public float FontSize { get; set; }

        /// <summary>Text colour as 0xRRGGBB, or null for automatic.</summary>
        public int? Colour { get; set; }

        /// <summary>Highlight (background) colour as 0xRRGGBB, or null for none.</summary>
        public int? Highlight { get; set; }
    }

    public enum ListKind { None, Bullet, Arabic, LowerLetter, UpperLetter, LowerRoman, UpperRoman }

    /// <summary>How a list number is punctuated: 1) (1) 1. 1, or an unnumbered item continuing the list.</summary>
    public enum ListNumberStyle { RightParenthesis, Parentheses, Period, Plain, NoNumber }

    public enum ParagraphAlignment { Left, Centre, Right, Justify }
}
