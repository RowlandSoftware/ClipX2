using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using ClipX2.TextEngine.Export;

namespace ClipX2.TextEngine.Import
{
    /// <summary>
    /// HTML (from the clipboard: browsers, Word, Outlook, Google Docs) into a
    /// <see cref="FormattedDocument"/>, keeping what rich text can show: paragraphs and line
    /// breaks; bold, italic, underline, strikethrough, superscript, subscript; fonts, sizes and
    /// colours; headings (bold, larger); lists (nested, numbered in their style); alignment and
    /// left indents; tables as lines with a tab between cells. Scripts, styles and comments are
    /// left out. Lists typed as text (Word's "1." and "·") are recognised afterwards.
    /// Tolerant rather than strict: untidy HTML still gives its text.
    /// </summary>
    public static class HtmlReader
    {
        private static readonly Regex Token = new Regex(
            @"<!--.*?-->|<!\[.*?\]>|<![^>]*>|<\?[^>]*>|<(/?)([a-zA-Z][\w:.-]*)((?:[^>""']|""[^""]*""|'[^']*')*)>",
            RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex Attribute = new Regex(@"([\w:-]+)\s*(?:=\s*(?:""([^""]*)""|'([^']*)'|([^\s>]+)))?", RegexOptions.Compiled);

        private static readonly HashSet<string> Skipped = new HashSet<string> { "script", "style", "head", "title", "xml", "noscript", "template" };
        private static readonly HashSet<string> Void = new HashSet<string> { "br", "img", "hr", "meta", "link", "input", "col", "wbr", "area", "base", "source" };
        private static readonly HashSet<string> Blocks = new HashSet<string>
        {
            "p", "div", "h1", "h2", "h3", "h4", "h5", "h6", "li", "tr", "blockquote", "pre", "ul", "ol", "table",
            "section", "article", "header", "footer", "dd", "dt", "dl", "address", "figure", "figcaption", "main", "nav", "aside", "hr", "body",
        };

        /// <summary>Formatting as it applies to a stretch of text.</summary>
        private sealed class Style
        {
            public string Tag;
            public bool? Bold, Italic, Underline, Strike, Super, Sub;
            public string Font; public float Size; public int? Colour, Highlight;
            public bool Pre;
        }

        private sealed class ListState { public bool Ordered; public ListKind Kind; public int Start; public string Bullet; }

        private static readonly Regex KnownTag = new Regex(
            @"<(/?)(html|head|body|p|div|span|ol|ul|li|table|thead|tbody|tr|td|th|h[1-6]|br|hr|a|b|i|u|s|strong|em|font|img|pre|code|blockquote|section|article|header|footer|nav|style|script|meta|link|title|sup|sub|dl|dt|dd)\b[^<>]*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Whether plain text is HTML code (copied from a text editor, so the clipboard doesn't say
        /// it is HTML): a doctype or &lt;html&gt;, or at least two HTML tags, one of them a
        /// closing tag or a list, table or paragraph; and tags not lost in a lot of other text.
        /// "a &lt; b &gt; c" or a lone "&lt;b&gt;" in prose isn't taken as HTML.
        /// </summary>
        public static bool LooksLikeHtml(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            string t = text.TrimStart();
            if (t.StartsWith("<!doctype html", StringComparison.OrdinalIgnoreCase) || t.StartsWith("<html", StringComparison.OrdinalIgnoreCase)) return true;
            MatchCollection tags = KnownTag.Matches(text);
            if (tags.Count < 2) return false;
            bool structural = tags.Cast<Match>().Any(m => m.Groups[1].Value == "/"
                || Regex.IsMatch(m.Groups[2].Value, "^(p|div|ol|ul|li|table|tr|td|h[1-6]|br|body)$", RegexOptions.IgnoreCase));
            int tagChars = tags.Cast<Match>().Sum(m => m.Length);
            // Markup should be a fair part of the text, or it starts with a tag
            return structural && (t.StartsWith("<", StringComparison.Ordinal) || tagChars * 20 >= text.Length);
        }

        public static FormattedDocument Read(string html, BulletOptions bullets = null, NumberOptions numbers = null)
        {
            var doc = new FormattedDocument();
            var styles = new List<Style> { new Style { Tag = "" } };
            var lists = new List<ListState>();
            FormattedParagraph current = null;
            bool cellStarted = false;           // in a table row: a tab before the next cell
            int skipping = 0; string skipTag = null;
            float blockIndent = 0;              // blockquote / margin-left on the containing block
            var indentStack = new List<(string Tag, float Indent)>();
            ParagraphAlignment align = ParagraphAlignment.Left;

            html = html ?? "";
            int pos = 0;
            foreach (Match m in Token.Matches(html))
            {
                if (m.Index > pos && skipping == 0) AddText(html.Substring(pos, m.Index - pos));
                pos = m.Index + m.Length;
                if (!m.Groups[2].Success) continue; // comment, <!DOCTYPE>, <![if …]>
                bool closing = m.Groups[1].Value == "/";
                string tag = m.Groups[2].Value.ToLowerInvariant();
                if (tag.Contains(":")) { if (tag == "o:p") continue; } // Word's <o:p>, <v:…>: no formatting of their own
                Dictionary<string, string> attrs = closing ? null : Attributes(m.Groups[3].Value);
                bool selfClosing = !closing && m.Groups[3].Value.TrimEnd().EndsWith("/", StringComparison.Ordinal);

                if (skipping > 0)
                {
                    if (tag == skipTag) skipping += closing ? -1 : (selfClosing ? 0 : 1);
                    continue;
                }
                if (Skipped.Contains(tag) && !closing && !selfClosing) { skipping = 1; skipTag = tag; continue; }

                if (closing) Close(tag);
                else
                {
                    Open(tag, attrs);
                    if (selfClosing && !Void.Contains(tag)) Close(tag);
                }
            }
            if (pos < html.Length && skipping == 0) AddText(html.Substring(pos));
            EndParagraph();

            // Trim blank paragraphs at the start and end (wrappers, spacing)
            while (doc.Paragraphs.Count > 0 && IsEmpty(doc.Paragraphs[0])) doc.Paragraphs.RemoveAt(0);
            while (doc.Paragraphs.Count > 0 && IsEmpty(doc.Paragraphs[doc.Paragraphs.Count - 1])) doc.Paragraphs.RemoveAt(doc.Paragraphs.Count - 1);

            TypedListRecognition.Apply(doc, bullets, numbers);
            return doc;

            /************ Tags ************/

            void Open(string tag, Dictionary<string, string> a)
            {
                if (tag == "br") { Current().Runs.Add(NewRun("\v")); return; }
                if (tag == "img") { string alt = Get(a, "alt"); if (!string.IsNullOrEmpty(alt)) AddText("[" + alt + "]"); return; }
                if (tag == "hr") { EndParagraph(); return; }
                if (tag == "td" || tag == "th")
                {
                    if (cellStarted) Current().Runs.Add(NewRun("\t"));
                    cellStarted = true;
                }

                if (Blocks.Contains(tag))
                {
                    EndParagraph();
                    if (tag == "tr") cellStarted = false;
                    if (tag == "ul" || tag == "ol")
                    {
                        var list = new ListState { Ordered = tag == "ol", Start = 1 };
                        if (list.Ordered)
                        {
                            list.Kind = KindFromType(Get(a, "type") ?? CssValue(a, "list-style-type"));
                            if (int.TryParse(Get(a, "start"), out int start)) list.Start = start;
                        }
                        else list.Bullet = BulletFromType(CssValue(a, "list-style-type"), lists.Count(l => !l.Ordered));
                        lists.Add(list);
                    }
                    // Alignment and a left indent from the block (blockquote: half an inch)
                    string alignValue = (CssValue(a, "text-align") ?? Get(a, "align") ?? "").ToLowerInvariant();
                    if (alignValue.Length > 0)
                        align = alignValue == "center" ? ParagraphAlignment.Centre : alignValue == "right" ? ParagraphAlignment.Right
                            : alignValue == "justify" ? ParagraphAlignment.Justify : ParagraphAlignment.Left;
                    float indent = tag == "blockquote" ? TypedListRecognition.LevelStep : Points(CssValue(a, "margin-left"), 0) + Points(CssValue(a, "padding-left"), 0);
                    if (tag != "ul" && tag != "ol" && tag != "li" && indent > 0 && indent < 1000)
                    {
                        indentStack.Add((tag, indent));
                        blockIndent += indent;
                    }
                    else indentStack.Add((tag, 0));
                }

                // Inline (and block) formatting
                var s = new Style { Tag = tag };
                switch (tag)
                {
                    case "b": case "strong": s.Bold = true; break;
                    case "i": case "em": case "cite": case "var": case "dfn": s.Italic = true; break;
                    case "u": case "ins": s.Underline = true; break;
                    case "s": case "strike": case "del": s.Strike = true; break;
                    case "sup": s.Super = true; break;
                    case "sub": s.Sub = true; break;
                    case "th": s.Bold = true; break;
                    case "code": case "kbd": case "samp": case "tt": s.Font = "Consolas"; break;
                    case "pre": s.Font = "Consolas"; s.Pre = true; break;
                    case "h1": s.Bold = true; s.Size = 24; break;
                    case "h2": s.Bold = true; s.Size = 18; break;
                    case "h3": s.Bold = true; s.Size = 14; break;
                    case "h4": s.Bold = true; s.Size = 12; break;
                    case "h5": s.Bold = true; s.Size = 10; break;
                    case "h6": s.Bold = true; s.Size = 9; break;
                    case "font":
                        s.Font = FirstFont(Get(a, "face"));
                        s.Colour = Colour(Get(a, "color"));
                        if (int.TryParse(Get(a, "size"), out int fs)) s.Size = new[] { 0f, 7.5f, 10, 12, 13.5f, 18, 24, 36 }[Math.Max(1, Math.Min(7, fs))];
                        break;
                }
                ApplyCss(s, Get(a, "style"));
                if (!Void.Contains(tag)) styles.Add(s);
                else if (Blocks.Contains(tag)) indentStack.RemoveAt(indentStack.Count - 1);
            }

            void Close(string tag)
            {
                // The innermost open element of this name, and everything opened inside it
                int at = styles.FindLastIndex(x => x.Tag == tag);
                if (at > 0) styles.RemoveRange(at, styles.Count - at);
                if (Blocks.Contains(tag))
                {
                    EndParagraph();
                    if (tag == "ul" || tag == "ol") { if (lists.Count > 0) lists.RemoveAt(lists.Count - 1); }
                    if (tag == "tr") cellStarted = false;
                    int k = indentStack.FindLastIndex(x => x.Tag == tag);
                    if (k >= 0)
                    {
                        for (int j = indentStack.Count - 1; j >= k; j--) blockIndent -= indentStack[j].Indent;
                        indentStack.RemoveRange(k, indentStack.Count - k);
                    }
                    if (tag == "p" || tag == "div" || tag.StartsWith("h", StringComparison.Ordinal)) align = ParagraphAlignment.Left;
                }
            }

            /************ Text ************/

            FormattedParagraph Current()
            {
                if (current != null) return current;
                current = new FormattedParagraph { Alignment = align, LeftIndent = blockIndent };
                // A heading: its level (and the look, from the h1–h6 style above)
                Style heading = styles.LastOrDefault(x => x.Tag.Length == 2 && x.Tag[0] == 'h' && x.Tag[1] >= '1' && x.Tag[1] <= '6');
                if (heading != null) current.HeadingLevel = heading.Tag[1] - '0';
                // A list item: its list's kind and level
                if (lists.Count > 0 && styles.Exists(x => x.Tag == "li"))
                {
                    ListState list = lists[lists.Count - 1];
                    current.ListLevel = Math.Min(lists.Count - 1, 8);
                    current.LeftIndent = 0;
                    if (list.Ordered)
                    {
                        current.ListKind = list.Kind;
                        current.ListNumberStyle = ListNumberStyle.Period;
                        current.ListStart = list.Start;
                    }
                    else
                    {
                        current.ListKind = ListKind.Bullet;
                        current.BulletCharacter = list.Bullet;
                    }
                }
                return current;
            }

            void AddText(string raw)
            {
                bool pre = styles.Exists(x => x.Pre);
                string text = WebUtility.HtmlDecode(raw);
                if (!pre)
                {
                    text = Regex.Replace(text, @"[ \t\r\n\f]+", " ");
                    if (current == null || current.Runs.Count == 0 || current.Runs[current.Runs.Count - 1].Text.EndsWith("\v", StringComparison.Ordinal))
                        text = text.TrimStart(' ');
                    if (text.Length == 0) return;
                    FormattedRun last = current?.Runs.LastOrDefault();
                    if (text == " " && (last == null || last.Text.EndsWith(" ", StringComparison.Ordinal))) return;
                    Current().Runs.Add(NewRun(text));
                }
                else
                {
                    // Preformatted: line breaks are paragraphs
                    string[] parts = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
                    for (int k = 0; k < parts.Length; k++)
                    {
                        if (k > 0) { EndParagraph(force: true); }
                        if (parts[k].Length > 0) Current().Runs.Add(NewRun(parts[k]));
                    }
                }
            }

            FormattedRun NewRun(string text)
            {
                var r = new FormattedRun { Text = text };
                foreach (Style s in styles)
                {
                    if (s.Bold.HasValue) r.Bold = s.Bold.Value;
                    if (s.Italic.HasValue) r.Italic = s.Italic.Value;
                    if (s.Underline.HasValue) r.Underline = s.Underline.Value;
                    if (s.Strike.HasValue) r.Strikethrough = s.Strike.Value;
                    if (s.Super.HasValue) r.Superscript = s.Super.Value;
                    if (s.Sub.HasValue) r.Subscript = s.Sub.Value;
                    if (s.Font != null) r.FontName = s.Font;
                    if (s.Size > 0) r.FontSize = s.Size;
                    if (s.Colour.HasValue) r.Colour = s.Colour;
                    if (s.Highlight.HasValue) r.Highlight = s.Highlight;
                }
                return r;
            }

            void EndParagraph(bool force = false)
            {
                if (current == null)
                {
                    if (force) doc.Paragraphs.Add(new FormattedParagraph { Alignment = align, LeftIndent = blockIndent });
                    return;
                }
                FormattedRun lastRun = current.Runs.LastOrDefault();
                if (lastRun != null) lastRun.Text = lastRun.Text.TrimEnd(' ');
                // A paragraph of only a no-break space (Word's and browsers' empty line) is an empty line
                if (current.Runs.All(r => r.Text.Trim(' ', ' ').Length == 0)) current.Runs.Clear();
                MergeRuns(current);
                doc.Paragraphs.Add(current);
                current = null;
            }
        }

        private static bool IsEmpty(FormattedParagraph p) => p.ListKind == ListKind.None && p.Runs.All(r => r.Text.Trim().Length == 0);

        /// <summary>Neighbouring runs with the same formatting become one.</summary>
        private static void MergeRuns(FormattedParagraph p)
        {
            for (int i = p.Runs.Count - 1; i > 0; i--)
            {
                FormattedRun a = p.Runs[i - 1], b = p.Runs[i];
                if (a.Bold == b.Bold && a.Italic == b.Italic && a.Underline == b.Underline && a.Strikethrough == b.Strikethrough
                    && a.Superscript == b.Superscript && a.Subscript == b.Subscript && a.FontName == b.FontName
                    && a.FontSize == b.FontSize && a.Colour == b.Colour && a.Highlight == b.Highlight)
                {
                    a.Text += b.Text;
                    p.Runs.RemoveAt(i);
                }
            }
        }

        /************ Attributes and CSS ************/

        private static Dictionary<string, string> Attributes(string text)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match m in Attribute.Matches(text))
            {
                string value = m.Groups[2].Success ? m.Groups[2].Value : m.Groups[3].Success ? m.Groups[3].Value : m.Groups[4].Value;
                result[m.Groups[1].Value] = WebUtility.HtmlDecode(value);
            }
            return result;
        }

        private static string Get(Dictionary<string, string> a, string name) => a != null && a.TryGetValue(name, out string v) ? v : null;

        private static string CssValue(Dictionary<string, string> a, string property)
        {
            string style = Get(a, "style");
            if (string.IsNullOrEmpty(style)) return null;
            foreach (string declaration in style.Split(';'))
            {
                int colon = declaration.IndexOf(':');
                if (colon > 0 && declaration.Substring(0, colon).Trim().Equals(property, StringComparison.OrdinalIgnoreCase))
                    return declaration.Substring(colon + 1).Trim();
            }
            return null;
        }

        private static void ApplyCss(Style s, string style)
        {
            if (string.IsNullOrEmpty(style)) return;
            foreach (string declaration in style.Split(';'))
            {
                int colon = declaration.IndexOf(':');
                if (colon <= 0) continue;
                string name = declaration.Substring(0, colon).Trim().ToLowerInvariant();
                string value = declaration.Substring(colon + 1).Trim().ToLowerInvariant().Replace("!important", "").Trim();
                switch (name)
                {
                    case "font-weight": s.Bold = value == "bold" || value == "bolder" || (int.TryParse(value, out int w) && w >= 600); break;
                    case "font-style": s.Italic = value == "italic" || value == "oblique"; break;
                    case "text-decoration":
                    case "text-decoration-line":
                        if (value.Contains("underline")) s.Underline = true;
                        if (value.Contains("line-through")) s.Strike = true;
                        if (value == "none") { s.Underline = false; s.Strike = false; }
                        break;
                    case "vertical-align": if (value == "super") s.Super = true; else if (value == "sub") s.Sub = true; break;
                    case "font-family": s.Font = FirstFont(declaration.Substring(colon + 1)); break;
                    case "font-size": float size = Points(value, 0); if (size > 0 && size < 200) s.Size = size; break;
                    case "color": s.Colour = Colour(value) ?? s.Colour; break;
                    case "background-color": case "background": s.Highlight = Colour(value) ?? s.Highlight; break;
                }
            }
        }

        /// <summary>The first font in a CSS font-family list that isn't generic; null if none.</summary>
        private static string FirstFont(string family)
        {
            if (string.IsNullOrEmpty(family)) return null;
            foreach (string f in family.Split(','))
            {
                string name = f.Trim().Trim('"', '\'').Replace("!important", "").Trim();
                if (name.Length == 0) continue;
                if (new[] { "serif", "sans-serif", "monospace", "cursive", "fantasy", "system-ui", "inherit", "initial" }.Contains(name.ToLowerInvariant())) continue;
                return name;
            }
            return null;
        }

        /// <summary>A CSS length in points: pt, px (at 96 dpi), in, cm, mm, em (12 pt); else the fallback.</summary>
        private static float Points(string value, float fallback)
        {
            if (string.IsNullOrEmpty(value)) return fallback;
            Match m = Regex.Match(value.Trim().ToLowerInvariant(), @"^(-?[\d.]+)\s*(pt|px|in|cm|mm|em|rem|%)?");
            if (!m.Success || !float.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float n)) return fallback;
            switch (m.Groups[2].Value)
            {
                case "px": return n * 0.75f;
                case "in": return n * 72;
                case "cm": return n * 28.35f;
                case "mm": return n * 2.835f;
                case "em": case "rem": return n * 12;
                case "%": return fallback;
                default: return n; // pt, or unitless
            }
        }

        private static readonly Dictionary<string, int> Named = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["black"] = 0x000000, ["white"] = 0xFFFFFF, ["red"] = 0xFF0000, ["green"] = 0x008000, ["blue"] = 0x0000FF,
            ["yellow"] = 0xFFFF00, ["orange"] = 0xFFA500, ["purple"] = 0x800080, ["grey"] = 0x808080, ["gray"] = 0x808080,
            ["silver"] = 0xC0C0C0, ["maroon"] = 0x800000, ["navy"] = 0x000080, ["teal"] = 0x008080, ["olive"] = 0x808000,
            ["lime"] = 0x00FF00, ["aqua"] = 0x00FFFF, ["fuchsia"] = 0xFF00FF, ["cyan"] = 0x00FFFF, ["magenta"] = 0xFF00FF,
        };

        /// <summary>A CSS or HTML colour as 0xRRGGBB; null for none, transparent or unknown. Black text counts as none (the default).</summary>
        private static int? Colour(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            value = value.Trim().ToLowerInvariant();
            if (value.StartsWith("#", StringComparison.Ordinal))
            {
                string hex = value.Substring(1);
                if (hex.Length == 3) hex = string.Concat(hex.Select(c => new string(c, 2)));
                return hex.Length == 6 && int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int rgb) ? rgb : (int?)null;
            }
            Match m = Regex.Match(value, @"^rgba?\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(?:,\s*([\d.]+)\s*)?\)");
            if (m.Success)
            {
                if (m.Groups[4].Success && float.TryParse(m.Groups[4].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float alpha) && alpha < 0.1f) return null;
                return (Clamp(m.Groups[1].Value) << 16) | (Clamp(m.Groups[2].Value) << 8) | Clamp(m.Groups[3].Value);
            }
            return Named.TryGetValue(value, out int named) ? named : (int?)null;

            int Clamp(string s) => Math.Max(0, Math.Min(255, int.Parse(s, CultureInfo.InvariantCulture)));
        }

        private static ListKind KindFromType(string type)
        {
            switch (type)
            {
                case "a": case "lower-alpha": case "lower-latin": return ListKind.LowerLetter;
                case "A": case "upper-alpha": case "upper-latin": return ListKind.UpperLetter;
                case "i": case "lower-roman": return ListKind.LowerRoman;
                case "I": case "upper-roman": return ListKind.UpperRoman;
                default: return ListKind.Arabic;
            }
        }

        /// <summary>A bullet list's bullet: as its CSS says, else by nesting (• ◦ ▪, as browsers show them).</summary>
        private static string BulletFromType(string type, int depth)
        {
            switch ((type ?? "").ToLowerInvariant())
            {
                case "disc": return null;
                case "circle": return "◦";
                case "square": return "▪";
                default:
                    string b = ListFormatter.BulletForLevel("•", depth);
                    return b == "•" ? null : b;
            }
        }
    }
}
