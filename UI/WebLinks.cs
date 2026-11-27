using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ClipX2.UI
{
    /// <summary>
    /// Opens web pages (the About box's web site, Help pages, links in panels) in the user's
    /// default browser. Only http and https addresses are opened. If Windows can't open one, the
    /// user is told, with the address so it can be copied.
    /// </summary>
    internal static class WebLinks
    {
        /// <summary>Opens <paramref name="url"/> in the default browser. False if it couldn't be.</summary>
        public static bool Open(string url, IWin32Window owner = null)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                return false;
            try
            {
                Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
                return true;
            }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception || ex is InvalidOperationException)
            {
                MessageBox.Show(owner, $"Your web browser couldn't be opened ({ex.Message}).\n\nThe address is:\n{uri.AbsoluteUri}",
                    "ClipX", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }
    }
}
