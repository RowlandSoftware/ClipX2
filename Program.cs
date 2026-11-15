using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipX2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Visual styles, text rendering, default font and DPI mode (set in ClipX2.csproj)
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}
