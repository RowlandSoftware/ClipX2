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

        /// <summary>
        /// The named styles the text came with (from a word processor's RTF), or null. Kept so
        /// that Done can name them again: the word processor then applies its own styles.
        /// </summary>
        public ClipX2.TextEngine.Import.RtfStyleSheet Styles { get; set; }
    }

    /// <summary>One paragraph (ClipX's "line"), without its paragraph mark.</summary>
    public sealed class FormattedParagraph
    {
        public List<FormattedRun> Runs { get; } = new List<FormattedRun>();

        public ListKind ListKind { get; set; }
        public ListNumberStyle ListNumberStyle { get; set; }

        /// <summary>A bullet item's bullet if not the usual • (◦ and ▪ on deeper levels); null for •.</summary>
        public string BulletCharacter { get; set; }

        /// <summary>The number the list starts at (for lettered and Roman lists too: 3 = c / iii).</summary>
        public int ListStart { get; set; } = 1;

        /// <summary>Nesting level of a list item, 0 for the outermost.</summary>
        public int ListLevel { get; set; }

        /// <summary>A heading's level (1 = Heading 1 … 6), 0 for an ordinary paragraph: Word's heading styles, HTML's h1–h6.</summary>
        public int HeadingLevel { get; set; }

        public ParagraphAlignment Alignment { get; set; }

        /// <summary>Left indent in points.</summary>
        public float LeftIndent { get; set; }

        /// <summary>First-line indent in points, relative to <see cref="LeftIndent"/> (negative for a hanging indent).</summary>
        public float FirstLineIndent { get; set; }

        /// <summary>Space above and below the paragraph, in points.</summary>
        public float SpaceBefore { get; set; }
        public float SpaceAfter { get; set; }

        /// <summary>Line spacing (default single).</summary>
        public LineSpacing LineSpacing { get; set; } = LineSpacing.Single;

        /// <summary>The named paragraph style (a word processor's, e.g. "Body Text"), or null.</summary>
        public string StyleName { get; set; }
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

        /// <summary>Language as a Windows LCID (2057 = en-GB, 1036 = fr-FR), or 0 if not known.</summary>
        public int Language { get; set; }

        /// <summary>The named character style (e.g. "Emphasis"), or null.</summary>
        public string CharacterStyle { get; set; }
    }

    public enum ListKind { None, Bullet, Arabic, LowerLetter, UpperLetter, LowerRoman, UpperRoman }

    /// <summary>How a list number is punctuated: 1) (1) 1. 1, or an unnumbered item continuing the list.</summary>
    public enum ListNumberStyle { RightParenthesis, Parentheses, Period, Plain, NoNumber }

    public enum ParagraphAlignment { Left, Centre, Right, Justify }

    /// <summary>How line spacing is given (the editor's TOM values).</summary>
    public enum LineSpacingRule { Single = 0, AtLeast = 3, Exactly = 4, Multiple = 5 }

    /// <summary>
    /// A paragraph's line spacing: single, a multiple of single (Value in lines, e.g. 1.25), or at
    /// least / exactly Value points.
    /// </summary>
    public struct LineSpacing
    {
        public LineSpacingRule Rule;
        public float Value;

        public LineSpacing(LineSpacingRule rule, float value) { Rule = rule; Value = value; }

        public static LineSpacing Single => new LineSpacing(LineSpacingRule.Single, 1f);

        /// <summary>From RTF's \sl (twips; 0 = single; negative = exactly) and \slmult.</summary>
        public static LineSpacing FromRtf(int sl, bool multiple)
        {
            if (sl == 0) return Single;
            if (multiple) return Normalise(new LineSpacing(LineSpacingRule.Multiple, sl / 240f));
            return sl < 0 ? new LineSpacing(LineSpacingRule.Exactly, -sl / 20f) : new LineSpacing(LineSpacingRule.AtLeast, sl / 20f);
        }

        /// <summary>The RTF control words for it.</summary>
        public string ToRtf()
        {
            switch (Normalise(this).Rule)
            {
                case LineSpacingRule.Multiple: return @"\sl" + (int)System.Math.Round(Value * 240) + @"\slmult1";
                case LineSpacingRule.AtLeast: return @"\sl" + (int)System.Math.Round(Value * 20) + @"\slmult0";
                case LineSpacingRule.Exactly: return @"\sl-" + (int)System.Math.Round(Value * 20) + @"\slmult0";
                default: return @"\sl240\slmult1";
            }
        }

        /// <summary>A multiple of 1 is single spacing.</summary>
        public static LineSpacing Normalise(LineSpacing s) =>
            s.Rule == LineSpacingRule.Multiple && System.Math.Abs(s.Value - 1f) < 0.01f ? Single
            : s.Rule == LineSpacingRule.Single ? Single : s;

        public bool SameAs(LineSpacing other)
        {
            LineSpacing a = Normalise(this), b = Normalise(other);
            return a.Rule == b.Rule && System.Math.Abs(a.Value - b.Value) < (a.Rule == LineSpacingRule.Multiple ? 0.02f : 0.6f);
        }
    }
}
