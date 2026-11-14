using ClipX2.TextEngine.Engines;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace ClipX2.TextEngine
{
    public static class CaseTransformer
    {
        public enum Mode
        {
            Upper,
            Lower,
            Reverse,
            Sentence,
            SentenceCaps,
            InitCaps,
            Title
        }
        public static void Apply(RichTextBox rtb, Mode mode, bool isRtfMode)
        {
            string selected = rtb.SelectedText;
            string transformed = TransformPlain(rtb.Text, rtb.SelectionStart, rtb.SelectionLength, mode);

            if (selected != transformed)
            {
                if (isRtfMode)
                {
                    UpdateRtf(rtb, selected, transformed);
                }
                else
                {
                    rtb.SelectedText = transformed;
                }
            }
        }
        public static string TransformPlain(string text, int selstart, int sellen, Mode mode)
        {
            string seltext = text.Substring(selstart, sellen);
            switch (mode)
            {
                case Mode.Sentence:
                    return SentenceCaseEngine.TransformPlain(text, selstart, sellen, true);

                case Mode.SentenceCaps:
                    return SentenceCaseEngine.TransformPlain(text, selstart, sellen, false);

                case Mode.InitCaps:
                    return InitCapsEngine.TransformPlain(text, selstart, sellen, false);

                case Mode.Title:
                    return InitCapsEngine.TransformPlain(text, selstart, sellen, true);

                case Mode.Upper:
                    return seltext.ToUpperInvariant();

                case Mode.Lower:
                    return seltext.ToLowerInvariant();

                case Mode.Reverse:
                    return ReverseCase(seltext);

                default:
                    return seltext;
            }
        }
        private static char ReverseCase(char c) // char overload
        {
            if (char.IsUpper(c)) return char.ToLowerInvariant(c);
            if (char.IsLower(c)) return char.ToUpperInvariant(c);
            return c;
        }
        private static string ReverseCase(string s)
        {
            var chars = s.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                chars[i] = char.IsUpper(c)
                    ? char.ToLowerInvariant(c)
                    : char.IsLower(c)
                        ? char.ToUpperInvariant(c)
                        : c;
            }
            return new string(chars);
        }
        private static void UpdateRtf(RichTextBox rtb, string selected, string transformed)
        {
            int selstart = rtb.SelectionStart;
            int sellength = rtb.SelectionLength;
            rtb.SuspendLayout();
            
            for (int i = 0; i < selected.Length; i++)
            {
                if (!selected[i].Equals(transformed[i]))
                {
                    rtb.Select(selstart + i, 1);
                    rtb.SelectedText = transformed[i].ToString(); // replacing one char at a time preserves formatting, but slow
                }
            }
            rtb.SelectionStart = selstart;
            rtb.SelectionLength = sellength;
            rtb.ResumeLayout();
        }
    }
}
