using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ClipX2.TextEngine
{
    /// <summary>
    /// ClipX 1's Decode tools: URL escapes (%20), HTML entities (&amp;#169;, &amp;copy;), UTF-8 read
    /// as Windows-1252 (â€™ for ’), and MIME quoted-printable (=E2=80=99, and encoded words such as
    /// =?utf-8?Q?...?=). Each returns edits for [<c>from</c>, <c>to</c>), one per encoded
    /// sequence, so the rest of the text, and its formatting, is untouched. Escaped bytes are read
    /// as UTF-8 where they form it (as they do today), else as Windows-1252. Escapes for control
    /// characters (other than tab and line breaks) are left as they are.
    /// </summary>
    public static class TextDecoder
    {
        /************ URL ************/

        private static readonly Regex UrlRun = new Regex(@"(?:%[0-9A-Fa-f]{2})+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>Decode URL (edUDC): %20 → space, %C3%A9 → é. A + is left alone (it is only a space in query strings).</summary>
        public static List<TextEdit> Url(string text, int from, int to) =>
            Matches(text, from, to, UrlRun, m =>
            {
                var bytes = new byte[m.Length / 3];
                for (int i = 0; i < bytes.Length; i++)
                    bytes[i] = byte.Parse(m.Value.Substring(i * 3 + 1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                return DecodeBytes(bytes);
            });

        /************ HTML ************/

        private static readonly Regex Entity = new Regex(@"&(?:#[0-9]{1,7}|#[xX][0-9A-Fa-f]{1,6}|[A-Za-z][A-Za-z0-9]{1,31});", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>Decode HTML (edHDC): &amp;#169; &amp;#xA9; &amp;copy; → ©, and every other named entity (&amp;nbsp; → a hard space).</summary>
        public static List<TextEdit> Html(string text, int from, int to) =>
            Matches(text, from, to, Entity, m =>
            {
                string decoded = WebUtility.HtmlDecode(m.Value);
                if (decoded == m.Value) return null; // not an entity
                return decoded.Length == 1 && IsControl(decoded[0]) ? null : decoded;
            });

        /************ UTF-8 shown as Windows-1252 ************/

        /// <summary>
        /// Decode UTF-8 (edUTF): text whose UTF-8 bytes were read one by one as Windows-1252 (or
        /// Latin-1), e.g. â€™ → ’, Ã© → é, Â (before a hard space) → nothing. Only complete, valid
        /// UTF-8 sequences are decoded, so ordinary accented text is left alone.
        /// </summary>
        public static List<TextEdit> Utf8(string text, int from, int to)
        {
            var edits = new List<TextEdit>();
            int i = from;
            while (i < to)
            {
                int lead = ByteOf(text[i]);
                int extra = lead >= 0xC2 && lead <= 0xDF ? 1 : lead >= 0xE0 && lead <= 0xEF ? 2 : lead >= 0xF0 && lead <= 0xF4 ? 3 : 0;
                if (extra == 0 || i + extra >= to) { i++; continue; } // not a lead byte, or the sequence would run past the scope

                var bytes = new byte[extra + 1];
                bytes[0] = (byte)lead;
                bool ok = true;
                for (int k = 1; k <= extra && ok; k++)
                {
                    int b = ByteOf(text[i + k]);
                    ok = b >= 0x80 && b <= 0xBF;
                    bytes[k] = (byte)Math.Max(0, b);
                }
                string decoded = ok ? StrictUtf8(bytes) : null;
                if (decoded == null) { i++; continue; }
                edits.Add(new TextEdit(i, extra + 1, decoded));
                i += extra + 1;
            }
            return edits;
        }

        /************ MIME ************/

        // An encoded word (RFC 2047), a soft line break, or a run of =XX
        private static readonly Regex MimeCode = new Regex(
            @"=\?(?<cs>[^?\s]+)\?(?<enc>[QqBb])\?(?<data>[^?\s]*)\?=|=\r?\n|(?:=[0-9A-Fa-f]{2})+",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Decode MIME (edMIM), quoted-printable: =E2=80=99 → ’, =20 → space; "=" at the end of a
        /// line (a soft line break) joins it to the next; encoded words in headers
        /// (=?utf-8?Q?Caf=C3=A9?=, =?utf-8?B?Q2Fmw6k=?=) are decoded too.
        /// </summary>
        public static List<TextEdit> Mime(string text, int from, int to) =>
            Matches(text, from, to, MimeCode, m =>
            {
                if (m.Value[1] == '\r' || m.Value[1] == '\n') return ""; // soft line break
                if (m.Groups["enc"].Success) return EncodedWord(m.Groups["cs"].Value, m.Groups["enc"].Value, m.Groups["data"].Value);
                var bytes = new byte[m.Length / 3];
                for (int i = 0; i < bytes.Length; i++)
                    bytes[i] = byte.Parse(m.Value.Substring(i * 3 + 1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                return DecodeBytes(bytes);
            });

        private static string EncodedWord(string charset, string encoding, string data)
        {
            byte[] bytes;
            if (encoding == "B" || encoding == "b")
            {
                try { bytes = Convert.FromBase64String(data); }
                catch (FormatException) { return null; }
            }
            else
            {
                var list = new List<byte>();
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == '_') list.Add(0x20);
                    else if (data[i] == '=' && i + 2 < data.Length &&
                             byte.TryParse(data.Substring(i + 1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
                    { list.Add(b); i += 2; }
                    else list.Add((byte)data[i]);
                }
                bytes = list.ToArray();
            }
            string cs = charset.ToLowerInvariant();
            if (cs == "utf-8" || cs == "utf8") return StrictUtf8(bytes) ?? Windows1252(bytes);
            if (cs == "us-ascii" || cs == "iso-8859-1" || cs == "latin1" || cs == "windows-1252") return Windows1252(bytes);
            try { return Encoding.GetEncoding(charset).GetString(bytes); }
            catch (ArgumentException) { return null; } // a character set this system doesn't have: leave it
        }

        /************ Helpers ************/

        private static List<TextEdit> Matches(string text, int from, int to, Regex regex, Func<Match, string> decode)
        {
            var edits = new List<TextEdit>();
            for (Match m = regex.Match(text, from, to - from); m.Success; m = m.NextMatch())
            {
                string decoded = decode(m);
                if (decoded != null && decoded != m.Value) edits.Add(new TextEdit(m.Index, m.Length, decoded));
            }
            return edits;
        }

        private static bool IsControl(char c) => c < 0x20 && c != '\t' && c != '\n' && c != '\r' || c == 0x7F;

        /// <summary>UTF-8 if the bytes are valid UTF-8, else Windows-1252; control characters (except tab and line breaks) make it null.</summary>
        private static string DecodeBytes(byte[] bytes)
        {
            string s = StrictUtf8(bytes) ?? Windows1252(bytes);
            foreach (char c in s) if (IsControl(c)) return null;
            return s;
        }

        private static readonly UTF8Encoding Utf8Strict = new UTF8Encoding(false, true);

        private static string StrictUtf8(byte[] bytes)
        {
            try { return Utf8Strict.GetString(bytes); }
            catch (DecoderFallbackException) { return null; }
        }

        // Windows-1252's characters for bytes 0x80–0x9F (the rest are as Latin-1); \0 = unassigned
        private const string Cp1252High = "€\0‚ƒ„…†‡ˆ‰Š‹Œ\0Ž\0\0‘’“”•–—˜™š›œ\0žŸ";

        private static string Windows1252(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length);
            foreach (byte b in bytes)
                sb.Append(b >= 0x80 && b <= 0x9F && Cp1252High[b - 0x80] != '\0' ? Cp1252High[b - 0x80] : (char)b);
            return sb.ToString();
        }

        /// <summary>The Windows-1252 (or Latin-1) byte a character stands for, or -1 if it isn't one above 0x7F.</summary>
        private static int ByteOf(char c)
        {
            if (c >= 0x80 && c <= 0xFF) return c; // Latin-1, including C1 controls left by a Latin-1 decoder
            int k = Cp1252High.IndexOf(c);
            return c != '\0' && k >= 0 ? 0x80 + k : -1;
        }
    }
}
