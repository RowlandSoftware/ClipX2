using Svg;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

public static class IconManager
{
    private static readonly ConcurrentDictionary<string, Bitmap> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets an icon by name (without extension), scaled for DPI.
    /// </summary>
    public static Bitmap Get(string name, float dpiScale, int logicalSize, Color? tint = null)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string path = Path.Combine(baseDir, "icons", name + ".svg");
        if (!File.Exists(path))
            throw new FileNotFoundException("Icon not found", path);

        int px = (int)(logicalSize * dpiScale);
        string key = $"{path}|{px}|{tint?.ToArgb()}";

        return _cache.GetOrAdd(key, _ =>
        {
            Bitmap bmp = RenderSvg(path, px);

            if (tint.HasValue)
                bmp = ApplyTint(bmp, tint.Value);

            return bmp;
        });
    }
    private static Bitmap RenderSvg(string path, int px)
    {
        var svg = SvgDocument.Open(path);
        if (svg == null)
            throw new InvalidOperationException($"SVG failed to load: {path}");

        // Ensure viewBox exists
        if (svg.ViewBox.Width <= 0 || svg.ViewBox.Height <= 0)
            throw new InvalidOperationException("SVG has no valid viewBox.");

        var bmp = new Bitmap(px, px);
        bmp.SetResolution(96, 96);

        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        // Compute scale factor from viewBox to target pixel size
        float scaleX = px / svg.ViewBox.Width;
        float scaleY = px / svg.ViewBox.Height;
        float scale = Math.Min(scaleX, scaleY);

        // Apply scaling
        g.ScaleTransform(scale, scale);

        // Draw the SVG at scaled size
        svg.Draw(g);

        return bmp;
    }
    public static void SetActive(this ToolStripButton btn, bool active)
    {
        btn.Checked = active;
        btn.BackColor = active ? Color.LightSteelBlue : Color.Transparent;
    }
    private static Bitmap ApplyTint(Bitmap source, Color tint)
    {
        Bitmap result = new(source.Width, source.Height);

        using var g = Graphics.FromImage(result);
        g.DrawImage(source, 0, 0);

        using var brush = new SolidBrush(Color.FromArgb(120, tint));
        g.FillRectangle(brush, 0, 0, source.Width, source.Height);

        return result;
    }
}
