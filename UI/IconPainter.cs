using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace ClipX2.UI
{
    /// <summary>Draws PowerButton icons (<see cref="ButtonIcon"/>): the bitmap, then its text layer.</summary>
    public static class IconPainter
    {
        /// <summary>
        /// ClipX 2's navy (#1f2f8f, as its toolbar icons): behind a new PowerButton's number, and
        /// behind the characters of a button imported from ClipX 1 (whose buttons were green, 0x016331).
        /// </summary>
        public static readonly int DefaultBackground = unchecked((int)0xFF1F2F8F);

        /// <summary>The icon's bitmap at full size (<see cref="ButtonIcon.Size"/>), or a transparent one.</summary>
        public static Bitmap BitmapOf(ButtonIcon icon)
        {
            Bitmap b = FromPng(icon?.Png);
            if (b != null && b.Width == ButtonIcon.Size && b.Height == ButtonIcon.Size) return b;
            var full = new Bitmap(ButtonIcon.Size, ButtonIcon.Size, PixelFormat.Format32bppArgb);
            if (b != null)
            {
                using (Graphics g = Graphics.FromImage(full))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(b, new Rectangle(0, 0, ButtonIcon.Size, ButtonIcon.Size));
                }
                b.Dispose();
            }
            return full;
        }

        /// <summary>The whole icon (bitmap and text layer) at full size.</summary>
        public static Bitmap Compose(ButtonIcon icon)
        {
            Bitmap b = BitmapOf(icon);
            if (icon?.Text != null)
                using (Graphics g = Graphics.FromImage(b)) DrawText(g, icon.Text, 1f);
            return b;
        }

        /// <summary>The whole icon at <paramref name="size"/> pixels square (32 on the panel), smoothly scaled.</summary>
        public static Bitmap Render(ButtonIcon icon, int size)
        {
            using Bitmap full = Compose(icon);
            var b = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(full, new Rectangle(0, 0, size, size));
            }
            return b;
        }

        /// <summary>Draws the text layer, its coordinates multiplied by <paramref name="scale"/>.</summary>
        public static void DrawText(Graphics g, IconText t, float scale)
        {
            if (t == null || string.IsNullOrEmpty(t.Characters)) return;
            using Font font = FontOf(t, scale);
            using var brush = new SolidBrush(Color.FromArgb(t.Colour));
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.DrawString(t.Characters, font, brush, t.X * scale, t.Y * scale, StringFormat.GenericTypographic);
        }

        /// <summary>The text's font, its size in pixels (times <paramref name="scale"/>).</summary>
        public static Font FontOf(IconText t, float scale)
        {
            var style = (t.Bold ? FontStyle.Bold : 0) | (t.Italic ? FontStyle.Italic : 0);
            float size = Math.Max(1f, t.Size * scale);
            try { return new Font(t.FontName, size, style, GraphicsUnit.Pixel); }
            catch (ArgumentException)
            {
                try { return new Font(t.FontName, size, FontStyle.Regular, GraphicsUnit.Pixel); }
                catch (ArgumentException) { return new Font(FontFamily.GenericSansSerif, size, style, GraphicsUnit.Pixel); }
            }
        }

        /// <summary>The size of the text's box, in icon pixels.</summary>
        public static SizeF Measure(IconText t)
        {
            using var bmp = new Bitmap(1, 1);
            using Graphics g = Graphics.FromImage(bmp);
            using Font font = FontOf(t, 1f);
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            return g.MeasureString(t.Characters ?? "", font, PointF.Empty, StringFormat.GenericTypographic);
        }

        /// <summary>
        /// Puts the text in the middle of the icon, both ways (ClipX 1 centred newly typed
        /// characters). Centres the ink, the characters as drawn: the text's box has the font's
        /// room above and below (ascent, descent), which left it high or low.
        /// </summary>
        public static void Centre(IconText t)
        {
            SizeF s = Measure(t);
            t.X = (ButtonIcon.Size - s.Width) / 2f;
            t.Y = (ButtonIcon.Size - s.Height) / 2f;
            RectangleF ink = InkBounds(t);
            if (ink.IsEmpty) return; // nothing drawn (a space): the box is centred
            t.X += (ButtonIcon.Size - ink.Width) / 2f - ink.Left;
            t.Y += (ButtonIcon.Size - ink.Height) / 2f - ink.Top;
        }

        /// <summary>Where the text's characters are actually drawn, in icon pixels (to a quarter pixel); empty if nowhere.</summary>
        public static RectangleF InkBounds(IconText t)
        {
            if (t == null || string.IsNullOrEmpty(t.Characters)) return RectangleF.Empty;
            const int Scale = 4;
            int margin = ButtonIcon.Size * Scale; // room for text partly off the icon
            int side = 3 * margin;
            using var b = new Bitmap(side, side, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.TranslateTransform(margin, margin);
                IconText solid = t.Clone();
                solid.Colour = unchecked((int)0xFF000000); // opaque, whatever its colour
                DrawText(g, solid, Scale);
            }
            BitmapData data = b.LockBits(new Rectangle(0, 0, side, side), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                int stride = data.Stride;
                byte[] pixels = new byte[stride * side];
                System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                int left = side, top = side, right = -1, bottom = -1;
                for (int y = 0; y < side; y++)
                    for (int x = 0; x < side; x++)
                        if (pixels[y * stride + x * 4 + 3] > 64)
                        {
                            if (x < left) left = x;
                            if (x > right) right = x;
                            if (y < top) top = y;
                            bottom = y;
                        }
                if (right < 0) return RectangleF.Empty;
                return RectangleF.FromLTRB((left - margin) / (float)Scale, (top - margin) / (float)Scale,
                    (right + 1 - margin) / (float)Scale, (bottom + 1 - margin) / (float)Scale);
            }
            finally { b.UnlockBits(data); }
        }

        /// <summary>A rounded square filled with <paramref name="argb"/>, as PNG (base64).</summary>
        public static string SolidPng(int argb)
        {
            using var b = new Bitmap(ButtonIcon.Size, ButtonIcon.Size, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(Color.FromArgb(argb));
                using GraphicsPath path = RoundedSquare(new RectangleF(0, 0, ButtonIcon.Size, ButtonIcon.Size), ButtonIcon.Size / 8f);
                g.FillPath(brush, path);
            }
            return ToPng(b);
        }

        public static GraphicsPath RoundedSquare(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2;
            path.AddArc(r.Left, r.Top, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static string ToPng(Bitmap b)
        {
            using var ms = new MemoryStream();
            b.Save(ms, ImageFormat.Png);
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>The bitmap a base64 PNG holds, or null if none (or unreadable).</summary>
        public static Bitmap FromPng(string png)
        {
            if (string.IsNullOrEmpty(png)) return null;
            try
            {
                using var ms = new MemoryStream(Convert.FromBase64String(png));
                using var loaded = new Bitmap(ms);
                return new Bitmap(loaded); // independent of the stream
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentException || ex is System.Runtime.InteropServices.ExternalException) { return null; }
        }
    }
}
