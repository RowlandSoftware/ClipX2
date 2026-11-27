using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ClipX2.UI
{
    /// <summary>
    /// About ClipX (the clpAbout button): the name, large, in Segoe UI Variable Display (friendly,
    /// plain, elegant; Windows 11's own), a small RS at its top left, the tagline in Rockwell (a
    /// tool's slab serif), the picture, the version, the copyright and the web site.
    /// </summary>
    public partial class AboutDialog : Form
    {
        private const string Website = "https://rowlandsoftware.com/";

        public AboutDialog()
        {
            InitializeComponent();

            // The fonts, or the nearest ones installed (Rockwell comes with Office, not Windows)
            labelTitle.Font = FirstInstalled(labelTitle.Font, "Segoe UI Variable Display Semib", "Segoe UI Variable Display", "Segoe UI Semibold", "Segoe UI"); // GDI cuts the name short
            labelRS.Font = FirstInstalled(labelRS.Font, "Segoe UI Variable Display Semib", "Segoe UI Semibold", "Segoe UI");
            labelTagline.Font = FirstInstalled(labelTagline.Font, "Rockwell Nova", "Rockwell", "Segoe UI");

            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            labelVersion.Text = v == null ? "" : $"Version {v.Major}.{v.Minor}.{v.Build}";
            linkWebsite.LinkClicked += (s, e) => { if (WebLinks.Open(Website, this)) linkWebsite.LinkVisited = true; };
            Load += (s, e) => PlaceRS();
        }

        /// <summary>
        /// The name centred, just as wide as its text (so it doesn't cover the RS), and the RS at
        /// its top left.
        /// </summary>
        private void PlaceRS()
        {
            int top = labelTitle.Top, height = labelTitle.Height;
            labelTitle.AutoSize = true;
            labelTitle.Location = new Point((ClientSize.Width - labelTitle.Width) / 2, top + (height - labelTitle.Height) / 2);
            labelRS.Location = new Point(labelTitle.Left - labelRS.Width + 4, labelTitle.Top + labelTitle.Height / 5);
        }

        /// <summary>The first of the families that is installed, in the font's size and style; the font itself if none is.</summary>
        private static Font FirstInstalled(Font font, params string[] families)
        {
            using var installed = new InstalledFontCollection();
            foreach (string name in families)
            {
                FontFamily family = installed.Families.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
                if (family == null) continue;
                FontStyle style = family.IsStyleAvailable(font.Style) ? font.Style : FontStyle.Regular;
                if (name.IndexOf("Semib", StringComparison.Ordinal) >= 0) style &= ~FontStyle.Bold; // the weight is in the name
                if (!family.IsStyleAvailable(style)) continue;
                return new Font(family, font.Size, style, font.Unit);
            }
            return font;
        }

        private void linkWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }
    }
}
