using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;

namespace ClipX2.UI
{
    /// <summary>
    /// The fonts offered for an icon's text: symbol fonts that make good icons (ClipX 1's
    /// Wingdings and Webdings; Windows' own icon fonts) and a few plain ones for letters, then
    /// any the user has typed or chosen in the Font dialog (kept in the settings). Only fonts
    /// installed on this computer are offered.
    /// </summary>
    internal static class IconFonts
    {
        /// <summary>The fonts offered to start with, in this order.</summary>
        public static readonly string[] Defaults =
        {
            "Wingdings", "Wingdings 2", "Wingdings 3", "Webdings",          // ClipX 1's
            "Segoe Fluent Icons", "Segoe MDL2 Assets",                       // Windows 11's and 10's icons
            "Segoe UI Symbol", "Segoe UI Emoji",
            "Segoe UI", "Arial", "Arial Narrow", "Georgia", "Consolas",      // for letters
        };

        private static HashSet<string> _installed;

        /// <summary>The installed font families (read once).</summary>
        private static HashSet<string> Installed
        {
            get
            {
                if (_installed != null) return _installed;
                using var fonts = new InstalledFontCollection();
                return _installed = new HashSet<string>(fonts.Families.Select(f => f.Name), StringComparer.OrdinalIgnoreCase);
            }
        }

        public static bool IsInstalled(string name) => !string.IsNullOrWhiteSpace(name) && Installed.Contains(name.Trim());

        /// <summary>The installed name as Windows spells it ("wingdings" → "Wingdings").</summary>
        public static string Canonical(string name) =>
            Installed.FirstOrDefault(n => string.Equals(n, name?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? name?.Trim();

        /// <summary>The list: the defaults, then the user's own, those installed.</summary>
        public static List<string> ForList() =>
            Defaults.Concat(AppSettings.Current.IconEditorFonts)
                .Where(IsInstalled).Select(Canonical)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        /// <summary>
        /// Adds a font the user typed or chose to their list (if installed and not there yet);
        /// true if it was added.
        /// </summary>
        public static bool Remember(string name)
        {
            if (!IsInstalled(name)) return false;
            name = Canonical(name);
            if (ForList().Contains(name, StringComparer.OrdinalIgnoreCase)) return false;
            AppSettings.Current.IconEditorFonts.Add(name);
            AppSettings.Current.Save();
            return true;
        }

        /************ The characters a font has ************/

        [DllImport("gdi32.dll")] private static extern uint GetFontUnicodeRanges(IntPtr hdc, IntPtr glyphSet);
        [DllImport("gdi32.dll")] private static extern uint GetFontData(IntPtr hdc, uint table, uint offset, byte[] buffer, uint length);
        [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);
        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr obj);

        /// <summary>
        /// Every character the font has a glyph for, as code points (in order), less controls and
        /// spaces. Icon fonts keep theirs in the Private Use Area (Segoe Fluent Icons from U+E700);
        /// symbol fonts (Wingdings) at U+F020–U+F0FF; emoji fonts most of theirs beyond U+FFFF
        /// (U+1F600 😀), which Windows' coverage call doesn't report, so those are read from the
        /// font's own character map.
        /// </summary>
        public static List<int> CharactersOf(string fontName)
        {
            var result = new List<int>();
            using var font = new Font(fontName, 16f, GraphicsUnit.Pixel);
            using var bmp = new Bitmap(1, 1);
            using Graphics g = Graphics.FromImage(bmp);
            IntPtr hdc = g.GetHdc();
            IntPtr hfont = font.ToHfont();
            IntPtr old = SelectObject(hdc, hfont);
            try
            {
                AddBasicPlane(hdc, result);
                AddBeyondBasicPlane(hdc, result);
            }
            finally
            {
                SelectObject(hdc, old);
                DeleteObject(hfont);
                g.ReleaseHdc(hdc);
            }
            // A symbol font (Wingdings) reports each symbol more than once: at U+F021–U+F0FF, at
            // 0x21–0xFF, and some at the Windows-1252 letters (Œ, „…). Keep only the U+F0xx ones,
            // as ClipX 1's icons are read
            if (result.Count(c => c >= 0xF020 && c <= 0xF0FF) >= 100)
                result.RemoveAll(c => c <= 0xF020 || c > 0xF0FF); // U+F020 is its space
            return result;
        }

        /// <summary>U+0000–U+FFFF, from Windows' GetFontUnicodeRanges.</summary>
        private static void AddBasicPlane(IntPtr hdc, List<int> result)
        {
            uint size = GetFontUnicodeRanges(hdc, IntPtr.Zero);
            if (size == 0) return;
            IntPtr buffer = Marshal.AllocHGlobal((int)size);
            try
            {
                if (GetFontUnicodeRanges(hdc, buffer) == 0) return;
                // GLYPHSET: cbThis, flAccel, cGlyphsSupported, cRanges (4 bytes each), then WCRANGEs (WCHAR low, USHORT count)
                int ranges = Marshal.ReadInt32(buffer, 12);
                for (int r = 0; r < ranges; r++)
                {
                    int low = (ushort)Marshal.ReadInt16(buffer, 16 + r * 4);
                    int count = (ushort)Marshal.ReadInt16(buffer, 16 + r * 4 + 2);
                    for (int c = low; c < low + count && c <= 0xFFFF; c++)
                    {
                        char ch = (char)c;
                        if (char.IsControl(ch) || char.IsWhiteSpace(ch) || char.IsSurrogate(ch) || ch == '\uFEFF' || ch == '\uFFFF') continue;
                        result.Add(c);
                    }
                }
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }

        /// <summary>
        /// Beyond U+FFFF: from the font's 'cmap' table, its format 12 subtable (groups of
        /// consecutive code points), which fonts with such characters have.
        /// </summary>
        private static void AddBeyondBasicPlane(IntPtr hdc, List<int> result)
        {
            const uint Cmap = 0x70616D63; // "cmap", as GetFontData wants the tag
            uint size = GetFontData(hdc, Cmap, 0, null, 0);
            if (size == 0 || size == 0xFFFFFFFF) return;
            var d = new byte[size];
            if (GetFontData(hdc, Cmap, 0, d, size) != size) return;
            int U16(long o) => o + 2 <= d.Length ? (d[o] << 8) | d[o + 1] : 0;
            long U32(long o) => o + 4 <= d.Length ? ((long)d[o] << 24) | ((long)d[o + 1] << 16) | ((long)d[o + 2] << 8) | d[o + 3] : 0;
            int tables = U16(2);
            for (int t = 0; t < tables; t++)
            {
                int platform = U16(4 + t * 8), encoding = U16(6 + t * 8);
                long offset = U32(8 + t * 8);
                bool full = (platform == 3 && encoding == 10) || (platform == 0 && (encoding == 4 || encoding == 6));
                if (!full || U16(offset) != 12) continue;
                long groups = U32(offset + 12);
                var seen = new HashSet<int>(result);
                for (long gi = 0; gi < groups; gi++)
                {
                    long at = offset + 16 + gi * 12;
                    long start = U32(at), end = U32(at + 4);
                    for (long c = Math.Max(start, 0x10000); c <= end && c <= 0x10FFFF; c++)
                        if (seen.Add((int)c)) result.Add((int)c);
                }
                return; // one full table is enough
            }
        }
    }
}
