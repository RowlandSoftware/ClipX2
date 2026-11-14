using System.Text;
using System.Windows.Forms;
using static ClipX2.TextEngine.CaseTransformer;

namespace ClipX2.TextEngine.Engines
{
    public static class SentenceCaseEngine
    {
        public static char ProcessChar(
            string text,
            int index,
            ref bool newSentence,
            bool lowerRest)
        {
            int inxdStart;

            char c = text[index];
            char prev = index > 0 ? text[index - 1] : '\0';
            char next = index < text.Length - 1 ? text[index + 1] : '\0';

            if (Utils.IsAbbrevAt(text, index))
            {
                newSentence = false;
                return c;
            }

            if (newSentence && char.IsLetter(c))
            {
                newSentence = false;
                return char.ToUpperInvariant(c);
            }

            char transformed = lowerRest
                ? char.ToLowerInvariant(c)
                : c;

            if (char.ToLowerInvariant(c) == 'i')
            {
                bool prevIsLetter = char.IsLetter(prev);
                bool nextIsLetter = char.IsLetter(next);
                bool nextIsApostrophe = next == '\'';
                bool nextIsPeriod = next == '.';

                if (!prevIsLetter &&
                    (!nextIsLetter || nextIsApostrophe) &&
                    !nextIsPeriod)
                {
                    transformed = 'I';
                }
            }

            string context = Utils.GetContext(text, index, out inxdStart);

            if (ClipXConstants.SentenceTerminators.Contains(c.ToString()) &&
                !char.IsDigit(next) &&
                !Utils.IsAbbrev(context))
            {
                newSentence = true;
            }

            if (c == '\n' || c == '\r')
                newSentence = true;

            return transformed;
        }

        public static string TransformPlain(string text, int selstart, int sellen, bool lowerRest)
        {
            var sb = new StringBuilder(text.Length);
            bool newSentence = true;

            for (int i = selstart; i < sellen; i++)
                sb.Append(ProcessChar(text, i, ref newSentence, lowerRest));

            return sb.ToString();
        }

        //some stuff
        public static void XdoCaseChange(RichTextBox rtb, Mode mode)
        {
            int start = rtb.SelectionStart;
            int length = rtb.SelectionLength;

            bool newWord = true;
            bool newSentence = true;

            rtb.SuspendLayout();

            for (int i = 0; i < length; i++)
            {
                rtb.Select(start + i, 1);
                string original = rtb.SelectedText;

                if (string.IsNullOrEmpty(original))
                    continue;

                char c = original[0];
                char transformed; // = c;

                switch (mode)
                {
                    case Mode.InitCaps:
                        if (char.IsLetter(c))
                        {
                            if (newWord)
                                transformed = char.ToUpperInvariant(c);
                            else
                                transformed = char.ToLowerInvariant(c);

                            newWord = false;
                        }
                        else
                        {
                            if (!c.Equals('\''))
                                newWord = true;
                        }
                        break;

                    case Mode.Sentence:
                    case Mode.SentenceCaps:
                        {
                            bool lowerRest = (mode == Mode.Sentence);

                            // Determine previous and next characters
                            char prev = i > 0 ? rtb.Text[start + i - 1] : '\0';
                            char next = (i < length - 1) ? rtb.Text[start + i + 1] : '\0';

                            if (char.IsLetter(c))
                            {
                                if (newSentence)
                                {
                                    transformed = char.ToUpperInvariant(c);
                                    newSentence = false;
                                }
                                else
                                {
                                    // Default behaviour for non-first letters
                                    transformed = lowerRest
                                        ? char.ToLowerInvariant(c)
                                        : c;

                                    // Special rule for "i" (AFTER lowercasing)
                                    if (transformed == 'i')
                                    {
                                        bool prevIsLetter = char.IsLetter(prev);
                                        bool nextIsLetter = char.IsLetter(next);
                                        bool nextIsApostrophe = next == '\'';
                                        bool nextIsPeriod = next == '.';

                                        if (!prevIsLetter &&
                                            (!nextIsLetter || nextIsApostrophe) &&
                                            !nextIsPeriod)
                                        {
                                            transformed = 'I';
                                        }
                                    }
                                }
                            }

                            // Sentence boundary detection
                            if (c == '.' || c == '!' || c == '?' || c == '\n' || c == '\r')
                                newSentence = true;

                            break;
                        }


                }

                rtb.Select(start, length);
                rtb.ResumeLayout();
            }
        }
    }
}
