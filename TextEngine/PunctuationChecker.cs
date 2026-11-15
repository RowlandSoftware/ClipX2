using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClipX2.TextEngine
{
    /// <summary>A problem found, or a place where the checker had to guess, with its position in the text.</summary>
    public sealed class PunctuationNote
    {
        internal PunctuationNote(int position, int line, string message)
        {
            Position = position;
            Line = line;
            Message = message;
        }

        /// <summary>Position in the original text.</summary>
        public int Position { get; }

        /// <summary>Paragraph ("line") number, counting from 1 at the start of the whole text.</summary>
        public int Line { get; }

        public string Message { get; }

        public override string ToString() => $"Line {Line}: {Message}";
    }

    public sealed class PunctuationCheckResult
    {
        internal PunctuationCheckResult(int start, int length, string text, IReadOnlyList<TextEdit> edits,
            IReadOnlyList<PunctuationNote> problems, IReadOnlyList<PunctuationNote> guesses)
        {
            Start = start;
            Length = length;
            Text = text;
            Edits = edits;
            Problems = problems;
            Guesses = guesses;
        }

        /// <summary>Start of the range checked (the selection widened to whole paragraphs).</summary>
        public int Start { get; }

        /// <summary>Length of the range checked, in the original text.</summary>
        public int Length { get; }

        /// <summary>The corrected text of the range checked.</summary>
        public string Text { get; }

        /// <summary>The corrections as edits to the original text, in order. Applying them from last to first is simplest.</summary>
        public IReadOnlyList<TextEdit> Edits { get; }

        /// <summary>Things the user should look at: unmatched quotes and brackets.</summary>
        public IReadOnlyList<PunctuationNote> Problems { get; }

        /// <summary>
        /// Places where the checker couldn't be sure and made a decision, e.g. whether a straight
        /// quote opens or closes. (A future version may ask the user, as a spelling checker does.)
        /// </summary>
        public IReadOnlyList<PunctuationNote> Guesses { get; }
    }

    public sealed class PunctuationOptions
    {
        /// <summary>
        /// Also treat word-like internet domains (.it, .is, .in, .me, .so, .to, .uk ...) as web names,
        /// so no space is put after a full stop before them. Off by default, as in ClipX 1, because
        /// "end.So" is more often a missing space than a web address.
        /// </summary>
        public bool IncludeWordLikeDomains { get; set; }

        /// <summary>Extra file extensions to recognise (without the dot).</summary>
        public IList<string> ExtraFileExtensions { get; } = new List<string>();
    }

    /// <summary>
    /// ClipX's Check Punctuation tool (ported from ClipX 1): puts single spaces after punctuation
    /// and closing brackets and quotes, removes spaces inside brackets and quotes and before
    /// punctuation, and reports unmatched quotes and brackets. File names, web addresses, numbers
    /// (3.14, 1,000, 12:30) and abbreviations (e.g., i.e.) are left alone. Straight and curly
    /// quotes are both understood. It only ever adds or removes spaces.
    /// </summary>
    public static class PunctuationChecker
    {
        public static PunctuationCheckResult Check(string text, PunctuationOptions options = null)
            => Check(text, 0, text?.Length ?? 0, options);

        /// <summary>
        /// Checks the paragraphs touched by the range <paramref name="start"/>..+<paramref name="length"/>
        /// (whole paragraphs, since each paragraph is checked as a unit).
        /// </summary>
        public static PunctuationCheckResult Check(string text, int start, int length, PunctuationOptions options = null)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (start < 0 || start > text.Length) throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0 || start + length > text.Length) throw new ArgumentOutOfRangeException(nameof(length));

            var run = new Run(text, options ?? new PunctuationOptions());

            int from = start;
            while (from > 0 && !IsLineBreak(text[from - 1])) from--;
            int to = start + length;
            while (to < text.Length && !IsLineBreak(text[to])) to++;

            var result = new StringBuilder(to - from + 64);
            int p = from;
            while (true)
            {
                int end = p;
                while (end < to && !IsLineBreak(text[end])) end++;
                run.CheckParagraph(p, end, result);
                if (end >= to) break;

                // copy the line break (\r\n, \r or \n)
                result.Append(text[end]);
                if (text[end] == '\r' && end + 1 < to && text[end + 1] == '\n') { result.Append('\n'); end++; }
                p = end + 1;
            }

            string checkedText = result.ToString();
            return new PunctuationCheckResult(from, to - from, checkedText,
                Diff(text, from, to, checkedText), run.Problems, run.Guesses);
        }

        /// <summary>
        /// The checker only adds and removes spaces, so the edits can be found by walking the
        /// original and the result side by side.
        /// </summary>
        private static List<TextEdit> Diff(string original, int from, int to, string result)
        {
            var edits = new List<TextEdit>();
            int i = from, j = 0;
            while (i < to || j < result.Length)
            {
                if (i < to && j < result.Length && original[i] == result[j]) { i++; j++; continue; }

                if (j < result.Length && result[j] == ' ' && (i >= to || original[i] != ' '))
                {
                    int count = 0;
                    while (j < result.Length && result[j] == ' ' && (i >= to || original[i] != result[j])) { j++; count++; }
                    edits.Add(new TextEdit(i, 0, new string(' ', count)));
                }
                else if (i < to && original[i] == ' ')
                {
                    int s = i;
                    while (i < to && original[i] == ' ' && (j >= result.Length || result[j] != ' ')) i++;
                    edits.Add(new TextEdit(s, i - s, ""));
                }
                else
                {
                    // Not just spaces (shouldn't happen): replace the rest in one go
                    edits.Add(new TextEdit(i, to - i, result.Substring(j)));
                    break;
                }
            }
            return edits;
        }

        /********************************************************************************/

        private const string Punctuation = ".,?!;:";
        private const string BracketsOpen = "([{";
        private const string BracketsClose = ")]}";
        private const string StopChars = ".?!";

        private const char DoubleStraight = '"', DoubleOpen = '“', DoubleClose = '”';
        private const char SingleStraight = '\'', SingleOpen = '‘', SingleClose = '’';

        private static bool IsLineBreak(char c) => c == '\n' || c == '\r';
        private static bool IsWhite(char c) => c == ' ' || c == '\t' || IsLineBreak(c);
        private static bool IsDoubleQuote(char c) => c == DoubleStraight || c == DoubleOpen || c == DoubleClose;
        private static bool IsSingleQuote(char c) => c == SingleStraight || c == SingleOpen || c == SingleClose;
        private static bool IsQuote(char c) => IsDoubleQuote(c) || IsSingleQuote(c);

        /// <summary>Where a file name or web address ends (ClipX 1's INVALID_FILENAME_CHARS, plus curly quotes).</summary>
        private static bool EndsFilename(char c) => IsWhite(c) || c == '"' || c == '*' || c == '|' || c == DoubleOpen || c == DoubleClose;

        /// <summary>Contraction endings after an apostrophe: don't, I've, I'm, he's, we'll, I'd, we're.</summary>
        private static readonly string[] ContractionEndings = { "t", "ve", "nt", "m", "s", "ll", "d", "re" };

        /// <summary>Words that start with an apostrophe: 'tis, 'twas, 'em, 'cause, 'til, rock 'n' roll.</summary>
        private static readonly string[] LeadingElisions = { "tis", "twas", "em", "cause", "til", "n", "round", "bout" };

        /// <summary>
        /// Common file extensions. (ClipX 1 asked the Windows registry; a built-in list keeps the engine
        /// portable.) Single letters and ordinary words (key, pages, go) are left out on purpose.
        /// </summary>
        private const string FileExtensions =
            ",txt,doc,docx,docm,dot,dotx,xls,xlsx,xlsm,ppt,pptx,pdf,rtf,odt,ods,odp,odg,csv,tsv,htm,html,xhtml,xml,json,js,ts,css," +
            "cs,vb,vbs,bas,cls,frm,vbp,exe,dll,bat,cmd,ps1,py,java,cpp,hpp,zip,rar,7z,gz,tgz,tar,jpg,jpeg,png,gif,bmp,svg,svgz,tif,tiff,ico," +
            "webp,heic,mp3,mp4,m4a,aac,wav,avi,mov,mkv,flac,ogg,wma,wmv,webm,mpg,mpeg,ini,cfg,conf,log,md,sql,db,msi,iso,php,asp,aspx," +
            "jsp,sh,yml,yaml,eml,pst,epub,psd,eps,ttf,otf,woff,woff2,sln,slnx,csproj,vbproj,resx,config,dat,bak,tmp,lnk,reg,sys,inf,cab," +
            "apk,dmg,pkg,deb,rpm,jar,rs,rb,pl,lua,swift,kt,ipynb,xaml,ics,vcf,torrent,";

        // ClipX 1's INTERNET_TLD_CODES_SHORT and INTERNET_TLD_CODES_WORDS
        private const string DomainsShort =
            ",ac,ad,ae,aero,af,ag,ai,al,ao,aq,ar,arpa,au,aw,az,ba,bb,bd,bf,bg,bh,bi,biz,bj,bl,bm,bn,bo,bq,br,bs,bt,bv,bw,bz,ca,cc,cd,cf,cg," +
            "ch,ci,ck,cl,cm,cn,co,com,cr,cu,cv,cw,cx,cy,cz,de,dj,dk,dm,dz,ec,edu,ee,eg,eh,er,es,et,eu,fi,fj,fk,fm,fo,fr,ga,gb,gd,ge,gf,gg," +
            "gh,gi,gl,gm,gn,gp,gq,gr,gs,gt,gu,gw,gy,hk,hm,hn,hr,ht,hu,id,ie,il,im,int,io,iq,ir,je,jm,jo,jp,ke,kg,kh,ki,km,kn,kp,kr,kw,ky," +
            "kz,la,lb,lc,li,lk,lr,ls,lt,lu,lv,ly,mc,md,mf,mg,mh,mil,mk,ml,mm,mn,mo,mobi,mp,mq,mr,ms,mt,mu,mv,mw,mx,my,mz,na,nc,ne,net,nf," +
            "ng,ni,nl,np,nr,nu,nz,om,org,pa,pe,pf,pg,ph,pk,pl,pm,pn,pr,pt,pw,py,qa,re,ro,rs,ru,rw,sa,sb,sc,sd,se,sg,sh,si,sj,sk,sl,sm,sn," +
            "sr,ss,st,su,sv,sx,sy,sz,tc,td,tf,tg,th,tj,tk,tl,tm,tn,tp,tr,tt,tv,tw,tz,ua,ug,um,uy,uz,va,vc,ve,vg,vi,vn,vu,wf,ws,xxx,yt,za,zm,zw,";
        private const string DomainsWords =
            ",am,an,as,asia,at,ax,be,by,cat,coop,do,gov,in,info,is,it,jobs,ma,me,museum,name,no,post,pro,ps,so,tel,to,travel,uk,us,ye,";

        /// <summary>The state of one check.</summary>
        private sealed class Run
        {
            private readonly string _text;
            private readonly PunctuationOptions _options;
            private readonly string _extraExtensions;
            private readonly int[] _lineStarts;

            public readonly List<PunctuationNote> Problems = new List<PunctuationNote>();
            public readonly List<PunctuationNote> Guesses = new List<PunctuationNote>();

            public Run(string text, PunctuationOptions options)
            {
                _text = text;
                _options = options;
                _extraExtensions = "," + string.Join(",", options.ExtraFileExtensions.Select(e => e.TrimStart('.').ToLowerInvariant())) + ",";

                var starts = new List<int> { 0 };
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                    if (IsLineBreak(text[i])) starts.Add(i + 1);
                }
                _lineStarts = starts.ToArray();
            }

            private int LineOf(int position)
            {
                int index = Array.BinarySearch(_lineStarts, position);
                return (index >= 0 ? index : ~index - 1) + 1;
            }

            private void Problem(int position, string message) => Problems.Add(new PunctuationNote(position, LineOf(position), message));
            private void Guess(int position, string message) => Guesses.Add(new PunctuationNote(position, LineOf(position), message));

            /// <summary>A few characters either side of a position, for messages.</summary>
            private string Snippet(int position)
            {
                // the word containing the position, with one word either side, within the line
                int a = position, b = Math.Min(_text.Length, position + 1);
                for (int words = 0; words < 2; words++)
                {
                    while (a > 0 && _text[a - 1] == ' ') a--;
                    while (a > 0 && !IsWhite(_text[a - 1])) a--;
                    while (b < _text.Length && _text[b] == ' ') b++;
                    while (b < _text.Length && !IsWhite(_text[b])) b++;
                }
                string s = _text.Substring(a, b - a).Trim();
                if (s.Length > 60) s = s.Substring(Math.Max(0, position - a - 30), Math.Min(60, s.Length - Math.Max(0, position - a - 30)));
                return "‘" + s + "’";
            }

            /// <summary>Checks text[from..to), one paragraph, appending the result.</summary>
            public void CheckParagraph(int from, int to, StringBuilder output)
            {
                string s = _text;
                var r = new StringBuilder(); // this paragraph's result
                bool inDouble = false, inSingle = false;
                int doubleOpenedAt = -1, singleOpenedAt = -1;
                var brackets = new[] { new Stack<int>(), new Stack<int>(), new Stack<int>() };

                for (int i = from; i < to; i++)
                {
                    char c = s[i];
                    char next = i + 1 < to ? s[i + 1] : '\0';
                    char prev = i > from ? s[i - 1] : '\0';
                    string ch = c.ToString(); // may get a space added

                    // Count brackets (list numbering such as "1)" or "(iii)" at the start of a line doesn't count)
                    CountBracket(c, i, from, brackets);

                    // Full stop or colon in a file name or web address?
                    bool isFilename = false;
                    if (c == '.' || c == ':')
                    {
                        if (c == ':' && next == '-')
                            isFilename = true; // ":-" is the writer's choice; keep it together
                        else
                            isFilename = IsFilename(Context(i, from, to), i);
                    }

                    // Abbreviation such as e.g., i.e., N.A.T.O., or an ellipsis?
                    bool isAbbrev = c == '.' && IsAbbrev(Context(i, from, to));

                    // A space after punctuation, none before it
                    if (Punctuation.IndexOf(c) >= 0)
                    {
                        if (!(isFilename && c == '.')) TrimEnd(r);
                        if (next != '\0' && !IsWhite(next)
                            && !(char.IsDigit(prev) && char.IsDigit(next))          // 3.14, 1,000, 12:30
                            && !isFilename
                            && !(isAbbrev && c == '.')
                            && Punctuation.IndexOf(next) < 0 && BracketsClose.IndexOf(next) < 0
                            && !IsClosingQuote(next, inDouble, inSingle, i + 1, from, to))
                        {
                            ch += " ";
                        }
                    }

                    // Opening bracket: no space after it; a space before it, except product(s), Close() and colo(u)r
                    if (BracketsOpen.IndexOf(c) >= 0)
                    {
                        while (i + 1 < to && s[i + 1] == ' ') i++;

                        // (decided from the output so far, so a space already added after punctuation isn't doubled)
                        char last = Last(r);
                        if (last != '\0' && !IsWhite(last) && !IsQuote(last) && BracketsOpen.IndexOf(last) < 0)
                        {
                            string context = TrimStopChars(Context(i, from, to)).ToLowerInvariant();
                            bool plural = context.EndsWith("(s)") || context.EndsWith("(es)");
                            bool empty = context.EndsWith("()");
                            if (!plural && !empty && !IsInWordBrackets(i, to))
                            {
                                r.Append(' ');
                                if (char.IsLetterOrDigit(last))
                                    Guess(i, $"Added a space before the bracket in {Snippet(i)}. (Could be code, e.g. a function call.)");
                            }
                        }
                    }

                    // Closing bracket: no space before it; a space after it
                    if (BracketsClose.IndexOf(c) >= 0)
                    {
                        TrimEnd(r);
                        if (next != '\0' && !IsWhite(next) && Punctuation.IndexOf(next) < 0 && !IsQuote(next)
                            && BracketsClose.IndexOf(next) < 0 && !IsInWordBracketsClose(i, from, to))
                        {
                            ch += " ";
                        }
                    }

                    // Single quotes (and apostrophes)
                    if (IsSingleQuote(c) && !IsApostrophe(i, from, to, inSingle))
                    {
                        bool opening = c == SingleOpen || (c == SingleStraight && !inSingle);
                        if (opening)
                        {
                            char last = Last(r);
                            if (last != '\0' && !IsWhite(last) && BracketsOpen.IndexOf(last) < 0 && !IsDoubleQuote(last))
                            {
                                r.Append(' ');
                                if (c == SingleStraight)
                                    Guess(i, $"Took the straight quote in {Snippet(i)} as an opening quote and added a space before it.");
                            }
                            inSingle = true;
                            singleOpenedAt = i;
                            while (i + 1 < to && s[i + 1] == ' ') i++;
                        }
                        else
                        {
                            if (next != '\0' && !IsWhite(next) && !IsDoubleQuote(next) && BracketsClose.IndexOf(next) < 0 && Punctuation.IndexOf(next) < 0)
                                ch += " ";
                            if (Last(r) == ' ')
                            {
                                TrimEnd(r);
                                if (c == SingleStraight)
                                    Guess(i, $"Took the straight quote in {Snippet(i)} as a closing quote and removed the space before it.");
                            }
                            inSingle = false;
                        }
                    }

                    // Double quotes
                    if (IsDoubleQuote(c) && !IsInchMark(i, from, inDouble))
                    {
                        bool opening = c == DoubleOpen || (c == DoubleStraight && !inDouble);
                        if (opening)
                        {
                            char last = Last(r);
                            if (last != '\0' && !IsWhite(last) && BracketsOpen.IndexOf(last) < 0 && !IsSingleQuote(last))
                            {
                                r.Append(' ');
                                if (c == DoubleStraight)
                                    Guess(i, $"Took the straight quote in {Snippet(i)} as an opening quote and added a space before it.");
                            }
                            if (inDouble && c == DoubleOpen)
                                Problem(doubleOpenedAt, $"Opening quote in {Snippet(doubleOpenedAt)} seems to have no closing quote.");
                            inDouble = true;
                            doubleOpenedAt = i;
                            while (i + 1 < to && s[i + 1] == ' ') i++;
                        }
                        else
                        {
                            if (next != '\0' && !IsWhite(next) && !IsSingleQuote(next) && BracketsClose.IndexOf(next) < 0 && Punctuation.IndexOf(next) < 0)
                                ch += " ";
                            if (Last(r) == ' ')
                            {
                                TrimEnd(r);
                                if (c == DoubleStraight)
                                    Guess(i, $"Took the straight quote in {Snippet(i)} as a closing quote and removed the space before it.");
                            }
                            if (!inDouble)
                                Problem(i, $"Closing quote in {Snippet(i)} has no opening quote.");
                            inDouble = false;
                        }
                    }

                    r.Append(ch);

                    // Copy the rest of a file name or web address as it is (so '?' in a URL doesn't get a space)
                    if (isFilename)
                    {
                        while (i + 1 < to && !EndsFilename(s[i + 1]) && !(inSingle && IsSingleQuote(s[i + 1])))
                        {
                            i++;
                            r.Append(s[i]);
                            CountBracket(s[i], i, from, brackets);
                        }
                    }
                }

                // Report what's left open at the end of the paragraph
                if (inDouble)
                    Problem(doubleOpenedAt, $"Opening quote in {Snippet(doubleOpenedAt)} has no closing quote in this paragraph" +
                        " (fine if the quotation continues into the next paragraph).");
                if (inSingle)
                    Problem(singleOpenedAt, $"Opening single quote in {Snippet(singleOpenedAt)} has no closing quote in this paragraph.");
                for (int b = 0; b < 3; b++)
                    foreach (int pos in brackets[b].Reverse())
                        Problem(pos, $"Unmatched opening bracket ‘{BracketsOpen[b]}’ in {Snippet(pos)}.");

                output.Append(r);
            }

            private void CountBracket(char c, int i, int from, Stack<int>[] brackets)
            {
                int open = BracketsOpen.IndexOf(c), close = BracketsClose.IndexOf(c);
                if (open < 0 && close < 0) return;
                if (IsListNumber(i, from)) return;

                if (open >= 0)
                    brackets[open].Push(i);
                else if (brackets[close].Count > 0)
                    brackets[close].Pop();
                else
                    Problem(i, $"Unmatched closing bracket ‘{c}’ in {Snippet(i)}.");
            }

            /// <summary>A bracket in list numbering at the start of a line: 1) a) iii) (2) (b)</summary>
            private bool IsListNumber(int i, int from)
            {
                int a = from;
                while (a < i && (_text[a] == ' ' || _text[a] == '\t')) a++;
                int end = i;
                if (_text[i] == '(')
                {
                    if (i != a) return false;
                    end = i + 1;
                    while (end < _text.Length && char.IsLetterOrDigit(_text[end])) end++;
                    if (end >= _text.Length || _text[end] != ')') return false;
                    a = i + 1;
                }
                else if (_text[i] == ')')
                {
                    if (a < i && _text[a] == '(') a++;
                }
                else return false;

                string token = _text.Substring(a, end - a);
                return token.Length > 0 && token.Length <= 4 &&
                       (token.All(char.IsDigit) || (token.Length == 1 && char.IsLetter(token[0])) ||
                        token.ToLowerInvariant().All(ch => "ivxlc".IndexOf(ch) >= 0));
            }

            /// <summary>colo(u)r, vapo(u)rware: brackets of letters joined to letters on both sides.</summary>
            private bool IsInWordBrackets(int open, int to)
            {
                int len = Utils.InWordBracketLength(_text, open);
                int after = open + len;
                return len > 0 && after < to && char.IsLetter(_text[after]);
            }

            private bool IsInWordBracketsClose(int close, int from, int to)
            {
                int open = close - 1;
                while (open >= from && char.IsLetter(_text[open])) open--;
                return open >= from && _text[open] == '(' && IsInWordBrackets(open, to);
            }

            /// <summary>Is the quote at <paramref name="i"/> a closing one (so no space goes before it)?</summary>
            private bool IsClosingQuote(char c, bool inDouble, bool inSingle, int i, int from, int to)
            {
                if (c == DoubleClose || c == SingleClose) return true;
                if (c == DoubleStraight) return inDouble;
                if (c == SingleStraight) return inSingle || IsApostrophe(i, from, to, inSingle, record: false);
                return false; // opening quotes “ ‘ need a space before them
            }

            /// <summary>
            /// Is the ' or ’ at <paramref name="i"/> an apostrophe (don't, O'Neill, the boys' toys, '90s)
            /// rather than a quotation mark?
            /// </summary>
            private bool IsApostrophe(int i, int from, int to, bool inSingle, bool record = true)
            {
                char c = _text[i];
                if (c == SingleOpen) return false;

                char prev = i > from ? _text[i - 1] : '\0';
                char next = i + 1 < to ? _text[i + 1] : '\0';

                // Letters on both sides: don't, O'Neill, rock'n'roll
                if (char.IsLetterOrDigit(prev) && char.IsLetter(next)) return true;

                // Contraction endings
                int w = i + 1;
                while (w < to && char.IsLetter(_text[w])) w++;
                string after = _text.Substring(i + 1, w - i - 1).ToLowerInvariant();
                if (char.IsLetter(prev) && ContractionEndings.Contains(after)) return true;

                // Feet (6'2") - a prime, not a quote
                if (char.IsDigit(prev) && !inSingle)
                {
                    if (record) Guess(i, $"Took the ' in {Snippet(i)} as feet (a prime mark), not a quote.");
                    return true;
                }

                // Leading elisions and decades: 'tis, 'twas, rock 'n' roll, '90s
                if (!char.IsLetterOrDigit(prev) && !inSingle)
                {
                    if (LeadingElisions.Contains(after)) return true;
                    if (char.IsDigit(next)) return true;
                }

                // Plural possessive: the boys' toys. Inside single quotes it could equally be the closing quote.
                if (prev == 's' && i - 2 >= from && char.IsLetter(_text[i - 2]) && !char.IsLetter(next))
                {
                    if (!inSingle) return true;
                    if (record) Guess(i, $"Took the ' after an s in {Snippet(i)} as a closing quote, not a plural possessive (as in the boys' toys).");
                    return false;
                }

                return false;
            }

            /// <summary>12" - a straight double quote straight after a number, outside a quotation, is inches.</summary>
            private bool IsInchMark(int i, int from, bool inDouble)
            {
                if (_text[i] != DoubleStraight || inDouble || i == from || !char.IsDigit(_text[i - 1])) return false;
                Guess(i, $"Took the \" in {Snippet(i)} as inches (a double prime), not a quote.");
                return true;
            }

            /// <summary>The 'word' (up to whitespace either side) containing position i. ClipX 1's getContext.</summary>
            private string Context(int i, int from, int to)
            {
                int a = i;
                while (a > from && !IsWhite(_text[a - 1])) a--;
                int b = i + 1;
                while (b < to && !IsWhite(_text[b])) b++;
                return _text.Substring(a, b - a);
            }

            /// <summary>ClipX 1's IsFilename: a path, web address, e-mail address or name.ext.</summary>
            private bool IsFilename(string word, int position)
            {
                string t = TrimQuotes(TrimPunctuationAndBrackets(word)).Replace('\\', '/');

                if (t.Contains("//") || t.Contains(":/")) return true;
                if (t.Count(ch => ch == '/') > 1) return true;

                // Code such as Application.Close() or list.Add(x)
                int dotAt = t.IndexOf('.');
                if (dotAt > 0 && t.IndexOf('(', dotAt) > dotAt + 1) return true;

                if (t.Count(ch => ch == '.') > 1 || t.Contains("@"))   // also e.g., i.e., a.m.
                {
                    // ...but "file.txt.Then" or "me@x.org.Done" may be a missing space after a full stop
                    int last = t.LastIndexOf('.');
                    if (last > 0 && last + 1 < t.Length && char.IsUpper(t[last + 1]) && char.IsLower(t[last - 1]) && !IsAbbrev(word))
                        Guess(position, $"Took {Snippet(position)} as a file, web or e-mail name, so no space was added after the full stop.");
                    return true;
                }

                string ext = Extension(t);
                if (ext.Length == 0) return false;
                string list = "," + ext.ToLowerInvariant() + ",";
                bool known = (!ext.Equals("i", StringComparison.OrdinalIgnoreCase) &&
                              (FileExtensions.Contains(list) || _extraExtensions.Contains(list)))
                             || DomainsShort.Contains(list)
                             || (_options.IncludeWordLikeDomains && DomainsWords.Contains(list));

                if (known && _text[position] == '.')
                    Guess(position, $"Took {Snippet(position)} as a file or web name, so no space was added after the full stop.");
                return known;
            }

            private static string Extension(string name)
            {
                int dot = name.LastIndexOf('.');
                if (dot < 0) return "";
                string ext = name.Substring(dot + 1);
                int bad = 0;
                while (bad < ext.Length && !EndsFilename(ext[bad]) && BracketsOpen.IndexOf(ext[bad]) < 0 && BracketsClose.IndexOf(ext[bad]) < 0) bad++;
                return ext.Substring(0, bad);
            }

            /// <summary>Abbreviations with full stops (e.g. i.e. N.A.T.O. U.S.A) and ellipses.</summary>
            private static bool IsAbbrev(string word)
            {
                string t = TrimQuotes(word.Trim('(', ')', '[', ']', '{', '}'));
                if (t.EndsWith("...") || t.EndsWith("…") || t.Contains("..")) return true;
                t = t.TrimEnd(',', ';', ':', '?', '!');

                // letter-dot pairs, optionally ending in a letter without its dot: e.g. N.A.T.O. U.S.A
                if (t.Length < 2) return false;
                for (int k = 0; k < t.Length; k += 2)
                {
                    if (!char.IsLetter(t[k])) return false;
                    if (k + 1 < t.Length && t[k + 1] != '.') return false;
                }
                return t.Count(ch => ch == '.') >= 1 && t.Length >= 3;
            }

            private static string TrimPunctuationAndBrackets(string t) => t.TrimEnd((Punctuation + BracketsClose).ToCharArray());
            private static string TrimQuotes(string t) => t.Trim('"', '\'', DoubleOpen, DoubleClose, SingleOpen, SingleClose);
            private static string TrimStopChars(string t) => t.TrimEnd((StopChars + "\"'" + DoubleClose + SingleClose).ToCharArray());

            private static char Last(StringBuilder sb) => sb.Length > 0 ? sb[sb.Length - 1] : '\0';

            private static void TrimEnd(StringBuilder sb)
            {
                int n = sb.Length;
                while (n > 0 && sb[n - 1] == ' ') n--;
                sb.Length = n;
            }
        }
    }
}
