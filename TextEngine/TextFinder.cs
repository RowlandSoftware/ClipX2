using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClipX2.TextEngine
{
    /// <summary>Find and Replace's three kinds of search (ClipX 1's three tabs).</summary>
    public enum SearchMode
    {
        /// <summary>The text as typed (optionally with \t \n \x.. \u.... for special characters).</summary>
        Simple,
        /// <summary>Wildcards (? * + &amp; ! % ~ # @ [..]), turned into a regular expression (<see cref="TextFinder.WildcardToRegex"/>).</summary>
        Wildcard,
        /// <summary>A .NET regular expression, as typed (multiline; \t \n \x \u work in the replacement).</summary>
        Regex,
    }

    /// <summary>Wildcard search: where a match must be (ClipX 1's Position options).</summary>
    public enum WildcardPosition
    {
        Anywhere,
        StartOfLine,
        EndOfLine,
        StartOfWord,
        EndOfWord,
    }

    /// <summary>Find and Replace's options: the last used are kept in the settings (Search).</summary>
    public sealed class SearchOptions
    {
        public SearchMode Mode { get; set; } = SearchMode.Simple;
        public bool MatchCase { get; set; }
        public bool WholeWords { get; set; }
        /// <summary>The replacement takes each match's case (teh → the, Teh → The, TEH → THE), as the spelling panel's.</summary>
        public bool SmartReplace { get; set; }
        /// <summary>\t, \n, \xHH, \uHHHH and \\ in the find and replace texts stand for those characters (ClipX 1's "Allow special characters").</summary>
        public bool SpecialCharacters { get; set; }

        /// <summary>Wildcard search: where a match must be.</summary>
        public WildcardPosition Position { get; set; } = WildcardPosition.Anywhere;
        /// <summary>Wildcard search, # (a number): the decimal separator. The current culture's by default.</summary>
        public string DecimalSeparator { get; set; } = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        /// <summary>
        /// Wildcard search, # (a number): the thousands separator; "" for none. Any space (a
        /// space, no-break space or narrow no-break space) stands for all three.
        /// </summary>
        public string ThousandsSeparator { get; set; } = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
    }

    /// <summary>One match: where it starts in the text and how long it is.</summary>
    public readonly struct SearchMatch
    {
        public SearchMatch(int start, int length) { Start = start; Length = length; }
        public int Start { get; }
        public int Length { get; }
        public int End => Start + Length;
        public override string ToString() => $"@{Start} +{Length}";
    }

    /// <summary>The find or replace text can't be used (a bad \x.. code, a [ without its ], a search too slow). The message says why.</summary>
    public sealed class SearchPatternException : Exception
    {
        public SearchPatternException(string message) : base(message) { }
    }

    /// <summary>
    /// Find and Replace's engine, the same for every mode: made once from what the user typed and
    /// the options (<see cref="Create"/>), it finds the next or previous match, every match in a
    /// range, the replacement for a match, and the edits for Replace All. Each mode only says how
    /// to find a match and what replaces it; the rest is shared. Positions are in the text given,
    /// which for the editor is its Text (paragraphs end in \n).
    /// </summary>
    public abstract class TextFinder
    {
        protected TextFinder(SearchOptions options) { Options = options; }

        public SearchOptions Options { get; }

        /// <summary>The finder for the given texts and options.</summary>
        /// <exception cref="SearchPatternException">The find or replace text can't be used.</exception>
        public static TextFinder Create(string find, string replace, SearchOptions options)
        {
            options = options ?? new SearchOptions();
            switch (options.Mode)
            {
                case SearchMode.Simple: return new SimpleFinder(find, replace, options);
                case SearchMode.Wildcard:
                    // The replacement's \t \n \xHH \uHHHH \\ only with Special characters
                    return new RegexFinder(string.IsNullOrEmpty(find) ? "" : WildcardToRegex(find, options),
                        replace, options, emptyMatches: false, unescape: options.SpecialCharacters);
                default:
                    // The user's own regular expression; Whole words around it as for wildcards. In the
                    // replacement, \t \n \xHH \uHHHH \\ always work (a .NET replacement has no escapes of its own)
                    string pattern = string.IsNullOrEmpty(find) ? "" : options.WholeWords ? WholeWordsAround(find) : find;
                    return new RegexFinder(pattern, replace, options, emptyMatches: true, unescape: true);
            }
        }

        /// <summary>
        /// Whole words round a pattern: a word boundary only where the match begins or ends with a
        /// word character (as Simple search), so "~-~" or ", " can still be found as whole words.
        /// </summary>
        private static string WholeWordsAround(string pattern) =>
            $"(?:{NotAfterWord}|(?!{WordChar}))(?:{pattern})(?:(?<!{WordChar})|{NotBeforeWord})";

        /// <summary>
        /// Checks a regular expression as typed (Regex mode): null if it can be used, else what is
        /// wrong with it, for the status line while typing.
        /// </summary>
        public static string CheckRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return null;
            try { _ = new Regex(pattern, RegexOptions.Multiline | RegexOptions.CultureInvariant); return null; }
            catch (ArgumentException ex) { return RegexFinder.Explain(ex); }
        }

        /// <summary>Nothing to look for (an empty find text).</summary>
        public abstract bool IsEmpty { get; }

        /// <summary>
        /// The first match starting at or after <paramref name="from"/> (forwards), or the last
        /// ending at or before it (backwards), inside [<paramref name="rangeStart"/>,
        /// <paramref name="rangeEnd"/>) (rangeEnd -1: to the end of the text); null if none.
        /// Never an empty match.
        /// </summary>
        public abstract SearchMatch? Find(string text, int from, bool forward, int rangeStart = 0, int rangeEnd = -1);

        /// <summary>Whether exactly these characters are a match (the selection is the last match found).</summary>
        public abstract bool IsMatchAt(string text, int start, int length);

        /// <summary>What replaces the match (in its case, with Smart replace).</summary>
        public abstract string ReplacementFor(string text, SearchMatch match);

        /// <summary>Every match in the range, in text order, not overlapping.</summary>
        public List<SearchMatch> FindAll(string text, int rangeStart = 0, int rangeEnd = -1)
        {
            var found = new List<SearchMatch>();
            if (string.IsNullOrEmpty(text) || IsEmpty) return found;
            int end = rangeEnd < 0 ? text.Length : Math.Min(rangeEnd, text.Length);
            int from = Math.Max(0, rangeStart);
            while (from < end)
            {
                SearchMatch? m = Find(text, from, true, rangeStart, end);
                if (m == null) break;
                found.Add(m.Value);
                from = m.Value.End;
            }
            return found;
        }

        /// <summary>Replace All: the edits for every match in the range, in text order (for the editor's ApplyEdits).</summary>
        public virtual List<TextEdit> ReplaceAll(string text, int rangeStart = 0, int rangeEnd = -1)
        {
            var edits = new List<TextEdit>();
            foreach (SearchMatch m in FindAll(text, rangeStart, rangeEnd))
                edits.Add(new TextEdit(m.Start, m.Length, ReplacementFor(text, m)));
            return edits;
        }

        /// <summary>The range made sensible for the text: (start, end), end -1 meaning the end of the text.</summary>
        protected static (int Start, int End) Clamp(string text, int rangeStart, int rangeEnd)
        {
            int end = rangeEnd < 0 ? text.Length : Math.Min(rangeEnd, text.Length);
            int start = Math.Max(0, Math.Min(rangeStart, end));
            return (start, end);
        }

        /// <summary>Smart replace: the replacement in the case of what it replaces (as the spelling panel's Smart replace).</summary>
        protected string Smart(string found, string replacement) =>
            Options.SmartReplace ? Spelling.SpellChecker.MatchCase(found, replacement) : replacement;

        /************ Special characters (ClipX 1's "Allow special characters") ************/

        /// <summary>
        /// Turns \t (tab), \n (new line), \xHH and \uHHHH (the character with that hexadecimal
        /// code) and \\ (a backslash) into the characters; any other backslash stays as it is.
        /// </summary>
        /// <exception cref="SearchPatternException">\x or \u without enough hexadecimal digits.</exception>
        public static string Unescape(string s)
        {
            if (string.IsNullOrEmpty(s) || s.IndexOf('\\') < 0) return s ?? "";
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '\\' && TrySpecial(s, ref i, out char special)) sb.Append(special);
                else if (c == '\\' && i + 1 < s.Length && s[i + 1] == '\\') { sb.Append('\\'); i++; }
                else sb.Append(c); // a backslash on its own
            }
            return sb.ToString();
        }

        /// <summary>
        /// At a backslash: \t, \n, \xHH or \uHHHH gives the character, and <paramref name="i"/>
        /// moves to the code's last character. False for anything else (\\ included).
        /// </summary>
        private static bool TrySpecial(string s, ref int i, out char special)
        {
            special = '\0';
            if (i + 1 >= s.Length) return false;
            char next = s[i + 1];
            switch (next)
            {
                case 't': special = '\t'; i++; return true;
                case 'n': special = '\n'; i++; return true;
                case 'x':
                case 'X':
                case 'u': // not \U: in a replacement that is "capitals from here"
                    int digits = next == 'x' || next == 'X' ? 2 : 4;
                    string hex = i + 2 + digits <= s.Length ? s.Substring(i + 2, digits) : s.Substring(i + 2);
                    if (hex.Length < digits || !int.TryParse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int code))
                        throw new SearchPatternException($"\\{next} needs {digits} hexadecimal digits (0–9, A–F) after it, as \\{next}{(digits == 2 ? "09" : "00E9")}.");
                    special = (char)code;
                    i += 1 + digits;
                    return true;
                default: return false;
            }
        }

        /************ Wildcards ************/

        /// <summary>The characters with a meaning in a wildcard search (a backslash before one makes it plain).</summary>
        public const string WildcardCharacters = "?.*+&!%~#@[]\\";

        // Word characters, as Whole words and TextSearch count them: letters (with their accents), digits, _
        private const string WordChar = @"[\p{L}\p{M}\p{Nd}_]";
        private const string NotAfterWord = @"(?<![\p{L}\p{M}\p{Nd}_])";
        private const string NotBeforeWord = @"(?![\p{L}\p{M}\p{Nd}_])";
        // White space within a line: ~ never runs on over a paragraph break (\n finds those)
        private const string Space = @"[^\S\n\r]";

        /// <summary>The text as a wildcard search for it: every wildcard character with a backslash before it.</summary>
        public static string EscapeWildcards(string text)
        {
            if (string.IsNullOrEmpty(text)) return text ?? "";
            var sb = new StringBuilder(text.Length + 8);
            foreach (char c in text)
            {
                if (WildcardCharacters.IndexOf(c) >= 0) sb.Append('\\');
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Turns a wildcard search into a .NET regular expression (ClipX 1's translateWildcardsToRegExp,
        /// with its bugs fixed). Each wildcard, with a * or + after it, becomes a group, so $1, $2…
        /// in the replacement are what the first, second… wildcard matched; * and + match as little
        /// as they can (not greedy), except on the last wildcard of all, which takes as much as it
        /// can (c%+ finds c123, ~+ a whole run of spaces): matching as little as possible there
        /// would only ever match nothing (*) or one character (+). Everything else is found as typed.
        /// <list type="bullet">
        /// <item>? or . any character (not a paragraph break); * none or more, + one or more</item>
        /// <item>&amp; a letter, ! a word character (letter, digit, _), % a digit, ~ white space (not a paragraph break): each with * or + after it for none or more, one or more</item>
        /// <item># a number (sign, thousands separators, decimals, exponent, as the options say), @ a word (with ' ’ or - inside: isn't, well-known)</item>
        /// <item>[…] any one of the characters (ranges a-z), [^…] any but them; * or + after it too</item>
        /// <item>\ before a wildcard character finds it (\? \* \[ \\); with Special characters, \t \n \xHH \uHHHH; any other backslash is found as itself (\t without Special characters is a backslash and a t)</item>
        /// </list>
        /// Whole words and the Position option go round the whole.
        /// </summary>
        /// <exception cref="SearchPatternException">A [ without its ], or a bad \x.. code.</exception>
        public static string WildcardToRegex(string wildcards, SearchOptions options)
        {
            options = options ?? new SearchOptions();
            var sb = new StringBuilder();
            string s = wildcards ?? "";
            int lazyAt = -1; // the last token's lazy '?' in sb (-1: the last token isn't a * or +)
            for (int i = 0; i < s.Length; i++)
            {
                int tokenLazy = -1;
                char c = s[i];
                char next = i + 1 < s.Length ? s[i + 1] : '\0';
                // A * or + after a wildcard: how many (a lazy quantifier); i moves past it
                string Many()
                {
                    if (next != '*' && next != '+') return "";
                    i++;
                    tokenLazy = sb.Length + 1; // the quantifier goes at sb.Length, its ? after it
                    return next == '*' ? "*?" : "+?";
                }
                switch (c)
                {
                    case '\\':
                        if (i == s.Length - 1) { sb.Append(@"\\"); break; }  // a backslash at the end
                        if (options.SpecialCharacters && TrySpecial(s, ref i, out char special)) { sb.Append(Regex.Escape(special.ToString())); break; }
                        if (WildcardCharacters.IndexOf(next) >= 0)
                        {
                            sb.Append(Regex.Escape(next.ToString()));        // \? \* \\ … : that character itself
                            i++;
                        }
                        else sb.Append(@"\\");                               // \t without Special chars: a backslash, then t as typed
                        break;
                    case '?':
                    case '.':
                        sb.Append("(.").Append(Many()).Append(')');
                        break;
                    case '*': tokenLazy = sb.Length + 3; sb.Append("(.*?)"); break;
                    case '+': tokenLazy = sb.Length + 3; sb.Append("(.+?)"); break;
                    case '&': sb.Append(@"(\p{L}").Append(Many()).Append(')'); break;
                    case '!': sb.Append('(').Append(WordChar).Append(Many()).Append(')'); break;
                    case '%': sb.Append(@"(\d").Append(Many()).Append(')'); break;
                    case '~': sb.Append('(').Append(Space).Append(Many()).Append(')'); break;
                    case '#': sb.Append('(').Append(NumberPattern(options)).Append(')'); break;
                    case '@': sb.Append('(').Append(WordPattern).Append(')'); break;
                    case '[':
                        int close = ClassEnd(s, i);
                        if (close < 0) throw new SearchPatternException("A [ needs a ] after it: [aeiou] is any one of those letters. To find a [ itself, type \\[.");
                        string inside = s.Substring(i + 1, close - i - 1);
                        i = close;
                        next = i + 1 < s.Length ? s[i + 1] : '\0';
                        sb.Append("([").Append(ClassContent(inside)).Append(']').Append(Many()).Append(')');
                        break;
                    default:
                        sb.Append(Regex.Escape(c.ToString()));
                        break;
                }
                lazyAt = tokenLazy;
            }
            if (lazyAt >= 0) sb.Remove(lazyAt, 1); // the last wildcard of all: greedy

            string pattern = sb.ToString();
            if (pattern.Length == 0) return "";
            if (options.WholeWords) pattern = WholeWordsAround(pattern);
            switch (options.Position)
            {
                case WildcardPosition.StartOfLine: pattern = "^(?:" + pattern + ")"; break;
                case WildcardPosition.EndOfLine: pattern = "(?:" + pattern + ")$"; break;
                case WildcardPosition.StartOfWord: pattern = NotAfterWord + "(?=" + WordChar + ")(?:" + pattern + ")"; break;
                case WildcardPosition.EndOfWord: pattern = "(?:" + pattern + ")(?<=" + WordChar + ")" + NotBeforeWord; break;
            }
            return pattern;
        }

        /// <summary>
        /// A wildcard search as the same search in Regex mode (ClipX 1's hidden Ctrl+Alt+Shift+R, to
        /// learn from): the find text as <see cref="WildcardToRegex"/> makes it (Whole words and the
        /// position included, so Whole words should then be off), and the replacement as Regex
        /// mode reads it: unchanged with Special characters; without, each backslash doubled (Regex
        /// mode always reads \t \n \\), except before the case changes \U \L \E.
        /// </summary>
        public static (string Find, string Replace) WildcardAsRegex(string find, string replace, SearchOptions options)
        {
            options = options ?? new SearchOptions();
            string pattern = string.IsNullOrEmpty(find) ? "" : WildcardToRegex(find, options);
            string r = replace ?? "";
            if (!options.SpecialCharacters && r.IndexOf('\\') >= 0)
            {
                var sb = new StringBuilder(r.Length + 4);
                for (int i = 0; i < r.Length; i++)
                {
                    char c = r[i], next = i + 1 < r.Length ? r[i + 1] : '\0';
                    sb.Append(c);
                    if (c == '\\' && next != 'U' && next != 'L' && next != 'E') sb.Append('\\');
                }
                r = sb.ToString();
            }
            return (pattern, r);
        }

        /// <summary>Where a [ … ] ends: the first ] after it, a ] straight after [ or [^ being one of the characters; -1 if none.</summary>
        private static int ClassEnd(string s, int open)
        {
            int i = open + 1;
            if (i < s.Length && s[i] == '^') i++;
            if (i < s.Length && s[i] == ']') i++;
            for (; i < s.Length; i++)
            {
                if (s[i] == '\\') { i++; continue; } // \] is a ] among the characters
                if (s[i] == ']') return i;
            }
            return -1;
        }

        /// <summary>The characters of a [ … ]: as typed, except that [ and a leading ] are made plain, and \t \n \x \u work as in regular expressions.</summary>
        private static string ClassContent(string inside)
        {
            var sb = new StringBuilder();
            for (int k = 0; k < inside.Length; k++)
            {
                char c = inside[k];
                if (c == '\\' && k + 1 < inside.Length) { sb.Append(c).Append(inside[k + 1]); k++; }
                else if (c == '[' || (c == ']' && (k == 0 || (k == 1 && inside[0] == '^')))) sb.Append('\\').Append(c);
                else sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>@: a word, beginning with a letter, with ' ’ or - inside (isn't, well-known), standing on its own.</summary>
        private const string WordPattern = NotAfterWord + @"\p{L}" + WordChar + @"*(?:['’-]" + WordChar + "+)*" + NotBeforeWord;

        /// <summary>
        /// #: a number, with an optional sign; digits, in thousands groups with the thousands
        /// separator (12,345) or not (12345); optional decimals (12.5, .5); an optional exponent
        /// (1.5e-3). Not part of a longer run of digits.
        /// </summary>
        private static string NumberPattern(SearchOptions o)
        {
            string dec = string.IsNullOrEmpty(o.DecimalSeparator) ? "." : o.DecimalSeparator;
            string thou = o.ThousandsSeparator ?? "";
            string decRx = Regex.Escape(dec);
            string thouRx = thou.Length == 0 || thou == dec ? null
                : thou.Trim().Length == 0 ? "[   ]" : Regex.Escape(thou);
            string integer = thouRx == null ? @"\d+" : $@"(?:\d{{1,3}}(?:{thouRx}\d{{3}})+|\d+)";
            return $@"(?<![\d{decRx}])[+-]?(?:{integer}(?:{decRx}\d+)?|{decRx}\d+)(?:[eE][+-]?\d+)?(?!\d)";
        }
    }

    /// <summary>
    /// The replacement of a regular-expression search (Wildcard or Regex mode): .NET's
    /// substitutions ($1, ${name}, $&amp;, $$), with case changes as Notepad++ and Perl have:
    /// \U capitals from here, \L small letters from here, \E back to as found. So \U$1\E makes
    /// group 1 capitals, and \L$&amp; the whole match small letters. With case changes in it, Smart
    /// replace leaves the case alone: the replacement says what case it wants.
    /// </summary>
    internal sealed class RegexReplacement
    {
        private enum CaseChange { None, Upper, Lower }

        private readonly List<(string Template, CaseChange Case)> _parts = new List<(string, CaseChange)>();

        /// <param name="replace">As typed.</param>
        /// <param name="unescape">\t \n \xHH \uHHHH \\ are the characters (see <see cref="TextFinder.Unescape"/>).</param>
        /// <param name="backReferences">
        /// \0 to \9 are the groups, as $0 to $9 (Regex mode: the other common way of writing them, as
        /// Notepad++ and sed; not in Wildcard mode, which keeps to one way, $1).
        /// </param>
        public RegexReplacement(string replace, bool unescape, bool backReferences = false)
        {
            string s = replace ?? "";
            var part = new StringBuilder();
            CaseChange current = CaseChange.None;
            void Flush()
            {
                if (part.Length > 0) _parts.Add((unescape ? TextFinder.Unescape(part.ToString()) : part.ToString(), current));
                part.Clear();
            }
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i], next = i + 1 < s.Length ? s[i + 1] : '\0';
                if (c == '\\' && next == '\\') { part.Append(c).Append(next); i++; } // \\U is a backslash and a U
                else if (c == '\\' && backReferences && next >= '0' && next <= '9') { part.Append("${").Append(next).Append('}'); i++; }
                else if (c == '\\' && (next == 'U' || next == 'L' || next == 'E'))
                {
                    Flush();
                    current = next == 'U' ? CaseChange.Upper : next == 'L' ? CaseChange.Lower : CaseChange.None;
                    HasCaseChanges = true;
                    i++;
                }
                else part.Append(c);
            }
            Flush();
        }

        /// <summary>\U, \L or \E in it: Smart replace doesn't apply.</summary>
        public bool HasCaseChanges { get; }

        /// <summary>The replacement as typed (unescaped), for a match that can't be had.</summary>
        public string Plain => string.Concat(_parts.Select(p => p.Template));

        /// <summary>The replacement for <paramref name="m"/>.</summary>
        public string For(Match m)
        {
            var sb = new StringBuilder();
            foreach (var (template, change) in _parts)
            {
                string result;
                try { result = m.Result(template); }
                catch (ArgumentException) { result = template; } // a $ that isn't a substitution: as typed
                CultureInfo culture = CultureInfo.CurrentCulture;
                sb.Append(change == CaseChange.Upper ? culture.TextInfo.ToUpper(result)
                    : change == CaseChange.Lower ? culture.TextInfo.ToLower(result)
                    : result);
            }
            return sb.ToString();
        }
    }

    /// <summary>Simple search: the text as typed, through <see cref="TextSearch"/> (as Quick search).</summary>
    internal sealed class SimpleFinder : TextFinder
    {
        private readonly string _find, _replace;

        public SimpleFinder(string find, string replace, SearchOptions options) : base(options)
        {
            _find = options.SpecialCharacters ? Unescape(find) : find ?? "";
            _replace = options.SpecialCharacters ? Unescape(replace) : replace ?? "";
        }

        public override bool IsEmpty => _find.Length == 0;

        private StringComparison Comparison => Options.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        public override SearchMatch? Find(string text, int from, bool forward, int rangeStart = 0, int rangeEnd = -1)
        {
            if (string.IsNullOrEmpty(text) || IsEmpty) return null;
            var (start, end) = Clamp(text, rangeStart, rangeEnd);
            from = Math.Max(start, Math.Min(from, end));
            if (forward)
            {
                int i = TextSearch.Find(text, _find, from, true, Options.MatchCase, Options.WholeWords);
                return i >= 0 && i + _find.Length <= end ? new SearchMatch(i, _find.Length) : (SearchMatch?)null;
            }
            else
            {
                int i = TextSearch.Find(text, _find, from, false, Options.MatchCase, Options.WholeWords);
                return i >= start ? new SearchMatch(i, _find.Length) : (SearchMatch?)null;
            }
        }

        public override bool IsMatchAt(string text, int start, int length) =>
            !IsEmpty && length == _find.Length && start >= 0 && start + length <= text.Length
            && string.Compare(text, start, _find, 0, length, Comparison) == 0
            && (!Options.WholeWords || TextSearch.IsWholeWord(text, start, length));

        public override string ReplacementFor(string text, SearchMatch match) =>
            Smart(text.Substring(match.Start, match.Length), _replace);
    }

    /// <summary>
    /// A search by regular expression: Wildcard mode's translation, or Regex mode's own pattern.
    /// Multiline (^ and $ at each line), case-insensitive unless Match case, with a time limit so
    /// a pathological pattern can't hang ClipX. Previous, Next and Find All skip empty matches
    /// (there is nothing to select or highlight); in Regex mode Replace All includes them, so ^
    /// or $ can add text at the start or end of every line.
    /// </summary>
    internal sealed class RegexFinder : TextFinder
    {
        private static readonly TimeSpan Limit = TimeSpan.FromSeconds(3);
        private const string TooLong = "The search took too long and was stopped. Try fewer * and + in a row, or something more definite between them.";
        private readonly Regex _regex;
        private readonly RegexReplacement _replace;
        private readonly bool _emptyMatches;

        /// <param name="pattern">The regular expression (Wildcard mode's translation, or Regex mode's own).</param>
        /// <param name="replace">The replacement, with $1… for groups.</param>
        /// <param name="options">The search options (Match case…).</param>
        /// <param name="emptyMatches">Replace All includes empty matches (Regex mode: ^ or $ adds text to every line).</param>
        /// <param name="unescape">Special characters in the replacement are the characters (see <see cref="TextFinder.Unescape"/>).</param>
        public RegexFinder(string pattern, string replace, SearchOptions options, bool emptyMatches, bool unescape) : base(options)
        {
            Pattern = pattern ?? "";
            _replace = new RegexReplacement(replace, unescape, backReferences: options.Mode == SearchMode.Regex);
            _emptyMatches = emptyMatches;
            if (Pattern.Length == 0) return;
            RegexOptions o = RegexOptions.Multiline | RegexOptions.CultureInvariant;
            if (!options.MatchCase) o |= RegexOptions.IgnoreCase;
            try { _regex = new Regex(Pattern, o, Limit); }
            catch (ArgumentException ex) { throw new SearchPatternException(Explain(ex)); }
        }

        /// <summary>.NET's complaint about a pattern, in a sentence for the status line.</summary>
        internal static string Explain(ArgumentException ex)
        {
            // .NET: "Invalid pattern '(a' at offset 2. Not enough )'s." – the reason is the useful part
            string message = ex.Message;
            int offset = message.IndexOf(" at offset ", StringComparison.Ordinal);
            if (offset >= 0)
            {
                int stop = message.IndexOf(". ", offset, StringComparison.Ordinal);
                string reason = stop >= 0 ? message.Substring(stop + 2).Trim() : "";
                string at = new string(message.Substring(offset + 11).TakeWhile(char.IsDigit).ToArray());
                if (reason.Length > 0) return $"Not a usable regular expression: {reason.TrimEnd('.')} (at character {at}).";
            }
            return "Not a usable regular expression: " + message;
        }

        /// <summary>
        /// Regex mode keeps empty matches (^ $ \b), so Replace All can insert at every line start
        /// or end; Wildcard mode, as Find All, doesn't.
        /// </summary>
        public override List<TextEdit> ReplaceAll(string text, int rangeStart = 0, int rangeEnd = -1)
        {
            if (!_emptyMatches) return base.ReplaceAll(text, rangeStart, rangeEnd);
            var edits = new List<TextEdit>();
            if (text == null || IsEmpty) return edits;
            var (start, end) = Clamp(text, rangeStart, rangeEnd);
            try
            {
                for (Match m = _regex.Match(text, start); m.Success && m.Index <= end; m = m.NextMatch())
                {
                    if (m.Index + m.Length > end) continue; // runs past the selection
                    // An empty match just after a selection ending in a paragraph break (^ there) is the
                    // next line's, not the selection's
                    if (m.Length == 0 && rangeEnd >= 0 && m.Index == end && end > start && text[end - 1] == '\n') continue;
                    edits.Add(new TextEdit(m.Index, m.Length, Replacement(m)));
                }
            }
            catch (RegexMatchTimeoutException) { throw new SearchPatternException(TooLong); }
            return edits;
        }

        /// <summary>The regular expression searched for.</summary>
        public string Pattern { get; }

        public override bool IsEmpty => _regex == null;

        /// <summary>The first non-empty match starting at or after <paramref name="from"/>, ending by <paramref name="end"/>.</summary>
        private Match Next(string text, int from, int end)
        {
            try
            {
                for (Match m = _regex.Match(text, from); m.Success && m.Index < end; )
                {
                    if (m.Length > 0 && m.Index + m.Length <= end) return m;
                    // empty, or running past the range: try from the next character
                    if (m.Index + 1 > text.Length) break;
                    m = _regex.Match(text, m.Index + 1);
                }
                return null;
            }
            catch (RegexMatchTimeoutException)
            {
                throw new SearchPatternException(TooLong);
            }
        }

        public override SearchMatch? Find(string text, int from, bool forward, int rangeStart = 0, int rangeEnd = -1)
        {
            if (string.IsNullOrEmpty(text) || IsEmpty) return null;
            var (start, end) = Clamp(text, rangeStart, rangeEnd);
            from = Math.Max(start, Math.Min(from, end));
            if (forward)
            {
                Match m = Next(text, from, end);
                return m == null ? (SearchMatch?)null : new SearchMatch(m.Index, m.Length);
            }
            // Backwards: the last of the matches (as Find All finds them) ending by 'from'
            SearchMatch? last = null;
            for (Match m = Next(text, start, from); m != null; m = Next(text, m.Index + m.Length, from))
                last = new SearchMatch(m.Index, m.Length);
            return last;
        }

        public override bool IsMatchAt(string text, int start, int length)
        {
            if (IsEmpty || length <= 0 || start < 0 || start + length > text.Length) return false;
            Match m = Next(text, start, text.Length);
            return m != null && m.Index == start && m.Length == length;
        }

        public override string ReplacementFor(string text, SearchMatch match)
        {
            Match m = Next(text, match.Start, text.Length);
            if (m == null || m.Index != match.Start) return _replace.Plain;
            return Replacement(m);
        }

        /// <summary>What replaces <paramref name="m"/>: Smart replace unless the replacement has its own case changes.</summary>
        private string Replacement(Match m)
        {
            string result = _replace.For(m);
            return _replace.HasCaseChanges ? result : Smart(m.Value, result);
        }
    }
}
