using System.Text;

namespace ClipX2.TextEngine.Engines
{
    internal static class SentenceCaseEngine
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
            var sb = new StringBuilder(sellen);
            bool newSentence = true;

            for (int i = selstart; i < selstart + sellen; i++)
                sb.Append(ProcessChar(text, i, ref newSentence, lowerRest));

            return sb.ToString();
        }

    }
}
