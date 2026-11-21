using System;
using System.Windows.Forms;
using ClipX2.TextEngine;
using ClipX2.UI;

namespace ClipX2
{
    /// <summary>How a tool was started: decides whether a tool with options shows them or uses the last ones.</summary>
    internal enum ToolTrigger
    {
        /// <summary>A click on its button: a tool with options opens them in the side panel.</summary>
        Click,
        /// <summary>A right-click: the tool runs at once with its last-used options (as ClipX 1).</summary>
        RightClick,
        /// <summary>A keyboard shortcut (F7, Shift+F3).</summary>
        Key,
        /// <summary>Ctrl+R, Repeat last tool: runs at once with the last-used options.</summary>
        Repeat,
        /// <summary>A PowerButton: runs at once with the PowerButton's own options (or the last used).</summary>
        PowerButton,
    }

    public partial class Form1
    {
        /// <summary>The last tool run, for Repeat last tool (Ctrl+R).</summary>
        private string _lastTool;

        /// <summary>The toolbars whose buttons are tools (found by their tags).</summary>
        private ToolStrip[] ToolStrips => new[] { toolStripStyles, toolStripStylesSmall, toolStripCase, toolStrip6, toolStrip5, toolStrip4, toolStrip7, toolStrip2, toolStripFavourites };

        /// <summary>Every tagged tool button goes to <see cref="RunTool"/>: a click, or a right-click for the last settings.</summary>
        private void WireToolButtons()
        {
            foreach (ToolStrip strip in ToolStrips)
                foreach (ToolStripItem item in strip.Items)
                    if (item.Tag is string tag && tag.Length > 0 && !(item is ToolStripControlHost) && !(item is ToolStripSplitButton)) // lists (font, size) and colour buttons have their own events
                        item.MouseUp += (s, e) =>
                        {
                            if (e.Button == MouseButtons.Left) RunTool(tag, ToolTrigger.Click);
                            else if (e.Button == MouseButtons.Right) RunTool(tag, ToolTrigger.RightClick);
                        };
        }

        /// <summary>
        /// The tool handler (ClipX 1's MainToolHandler): every tool, however started (button, key,
        /// Repeat, PowerButton), comes through here by its tag. It counts the use (usage.json),
        /// remembers the tool for Repeat, tidies up (another tool's options panel, Quick search's
        /// highlights, the status bar), then runs the tool.
        /// </summary>
        /// <param name="options">A PowerButton's own options for the tool (IndentOptions…); null for the last used.</param>
        internal void RunTool(string tag, ToolTrigger how, object options = null)
        {
            if (string.IsNullOrEmpty(tag)) return;
            if (tag == "clipxRPT") { RepeatLastTool(); return; }
            switch (tag) // the app's own actions (from "What do you want to do?" or a PowerButton)
            {
                case "clipxOPT": UsageStatistics.Record(tag); ShowOptions(OptionsPage.Display); return;
                case "clipxFIN": UsageStatistics.Record(tag); Done(); return;
                case "clipxMOD": UsageStatistics.Record(tag); toolStripLabelRtfMode_Click(this, EventArgs.Empty); return;
                case "clipxQSR": UsageStatistics.Record(tag); _quickSearch.Activate(); return;
            }
            if (tag == "clipxREF") // Refresh: the clipboard again (as at startup)
            {
                UsageStatistics.Record(tag);
                _quickSearch.Deactivate();
                RefreshFromClipboard();
                return;
            }

            UsageStatistics.Record(tag, fromPowerButton: how == ToolTrigger.PowerButton);
            _lastTool = tag;
            _quickSearch.Deactivate();     // Find all's highlights go
            CloseOtherOptions(tag, how);
            ShowStatus("");

            ClipXRichTextBox box = richTextBox1;
            AppSettings s = AppSettings.Current;
            // With options: a click shows them; anything else runs at once with the given or last-used ones
            bool showOptions = how == ToolTrigger.Click;
            T Use<T>(T saved) where T : class => options as T ?? saved;

            switch (tag)
            {
                // Change case
                case "ccUPP": ApplyCase(CaseMode.Upper); break;
                case "ccLOW": ApplyCase(CaseMode.Lower); break;
                case "ccREV": ApplyCase(CaseMode.Reverse); break;
                case "ccINC": ApplyCase(CaseMode.InitCaps); break;
                case "ccTIT": ApplyCase(CaseMode.Title); break;
                case "ccSEN": ApplyCase(CaseMode.Sentence); break;
                case "ccCAP": ApplyCase(CaseMode.SentenceCaps); break;
                case "ccCYC":
                    if (box.SelectionLength > 0) box.CycleCase();
                    else ShowStatus("Select some text to cycle its case");
                    break;

                // Edit
                case "edSTR": ShowStatus(FormatTools.StripEmail(box)); break;
                case "edUDC": ShowStatus(FormatTools.DecodeUrl(box)); break;
                case "edHDC": ShowStatus(FormatTools.DecodeHtml(box)); break;
                case "edUTF": ShowStatus(FormatTools.DecodeUtf8(box)); break;
                case "edMIM": ShowStatus(FormatTools.DecodeMime(box)); break;
                case "edPLA": ShowStatus(FormatTools.PlainCharacters(box)); break;
                case "edCRL": ShowStatus(FormatTools.CurlyQuotes(box)); break;
                case "edEOL": ShowStatus(FormatTools.RegulariseLineEnds(box)); break;

                // Format
                case "foTRM": ShowStatus(FormatTools.TrimWhiteSpace(box)); break;
                case "foTRA": ShowStatus(FormatTools.TrimTrailing(box)); break;
                case "foSSP": ShowStatus(FormatTools.SingleSpace(box)); break;
                case "foT2S": ShowStatus(FormatTools.TabsToSpaces(box)); break;
                case "foS2T": ShowStatus(FormatTools.SpacesToTabs(box)); break;
                case "foPAR": ShowStatus(FormatTools.Paragraph(box)); break;
                case "foIND": // Indent / Unindent: at once, the last of the two used (or a PowerButton's)
                    if (showOptions) ShowPanel<ClipX2.UI.Panels.IndentPanel>(p => p.Attach(box, ShowStatus));
                    else
                    {
                        IndentOptions o = Use(s.Indent);
                        ShowStatus(o.Unindent ? FormatTools.Unindent(box, o) : FormatTools.Indent(box, o));
                    }
                    break;
                case "foUNI": // ClipX 1's separate Unindent (PowerButtons)
                    ShowStatus(FormatTools.Unindent(box, Use(s.Indent)));
                    break;
                case "foWRP":
                    if (showOptions) ShowPanel<ClipX2.UI.Panels.WordWrapPanel>(p => p.Attach(box, () => _isRtfMode, ShowStatus));
                    else ShowStatus(FormatTools.WordWrap(box, _isRtfMode, Use(s.Wordwrap)));
                    break;
                case "foREM":
                    if (showOptions) ShowPanel<ClipX2.UI.Panels.RemoveLinesPanel>(p => p.Attach(box, ShowStatus));
                    else ShowStatus(FormatTools.RemoveExtraLines(box, Use(s.RemLines)));
                    break;

                // Lists
                case "liBUL":
                    if (showOptions) ShowPanel<ClipX2.UI.Panels.BulletListPanel>(p => p.Attach(box, () => _isRtfMode, ShowStatus));
                    else ShowStatus(ListTools.Bullets(box, _isRtfMode, Use(s.BulletList)));
                    break;
                case "liNUM":
                    if (showOptions) ShowPanel<ClipX2.UI.Panels.NumberListPanel>(p => p.Attach(box, () => _isRtfMode, ShowStatus));
                    else ShowStatus(ListTools.Numbers(box, _isRtfMode, Use(s.NumList)));
                    break;
                case "liREN":
                    if (showOptions) ShowPanel<ClipX2.UI.Panels.LineNumbersPanel>(p => p.Attach(box, () => _isRtfMode, ShowStatus));
                    else ShowStatus(FormatTools.LineNumbers(box, Use(s.NumLines)));
                    break;
                case "liRMB": ShowStatus(ListTools.RemoveBullets(box, _isRtfMode, s.BulletList)); break;
                case "liREM": ShowStatus(ListTools.RemoveNumbers(box, _isRtfMode, s.NumList)); break;
                case "liRLN": ShowStatus(FormatTools.RemoveLineNumbers(box)); break;

                // Sorting
                case "soALP": ShowStatus(SortTools.Sort(box, SortKind.Alphabetic)); break;
                case "soNUM": ShowStatus(SortTools.Sort(box, SortKind.Numeric)); break;
                case "soSMA": ShowStatus(SortTools.Sort(box, SortKind.Smart)); break;
                case "soREV": ShowStatus(SortTools.Reverse(box)); break;
                case "soNUP": ShowStatus(SortTools.Nudge(box, up: true)); break;
                case "soNUD": ShowStatus(SortTools.Nudge(box, up: false)); break;
                case "soKEY": ShowStatus(SortTools.SortItems(box)); break;

                // Language tools
                case "chPNC": CheckPunctuation(); break;
                case "chSPE":
                case "chGRA": SpellNext(); break;
                case "chSTAT": ShowPanel<ClipX2.UI.Panels.StatisticsPanel>(p => p.Attach(box, () => _isRtfMode)); break;

                default:
                    if (RunStyleTool(tag, options, how)) break; // the Styles tab (Form1.Styles.cs)
                    _lastTool = null;
                    ShowStatus($"'{ClipX2.TextEngine.Tools.ToolCatalogue.Find(tag)?.Name ?? tag}' isn't available yet");
                    break;
            }
        }

        /// <summary>Ctrl+R: the last tool again, at once, with its last-used options.</summary>
        private void RepeatLastTool()
        {
            if (_lastTool == null) { ShowStatus("No tool to repeat yet"); return; }
            UsageStatistics.Record("clipxRPT");
            RunTool(_lastTool, ToolTrigger.Repeat);
        }

        /// <summary>Shows a tool's side panel, connecting it to the editor the first time.</summary>
        private void ShowPanel<T>(Action<T> attach) where T : ClipX2.UI.Panels.SidePanelView, new()
        {
            T panel = _panels.Show<T>();
            if (!panel.IsAttached) attach(panel);
        }

        /// <summary>The side panel a tool opens on a click (null: it acts at once).</summary>
        private static Type PanelFor(string tag) => tag switch
        {
            "liBUL" => typeof(ClipX2.UI.Panels.BulletListPanel),
            "liNUM" => typeof(ClipX2.UI.Panels.NumberListPanel),
            "liREN" => typeof(ClipX2.UI.Panels.LineNumbersPanel),
            "foIND" => typeof(ClipX2.UI.Panels.IndentPanel),
            "foWRP" => typeof(ClipX2.UI.Panels.WordWrapPanel),
            "foREM" => typeof(ClipX2.UI.Panels.RemoveLinesPanel),
            "chSPE" => typeof(ClipX2.UI.Panels.SpellingPanel),
            "chSTAT" => typeof(ClipX2.UI.Panels.StatisticsPanel),
            _ => null,
        };

        /// <summary>
        /// A tool is starting: an open options panel closes, unless it is this tool's own (clicked
        /// again, or run at once with its last options), or this tool opens its own panel, which
        /// replaces it. The spelling panel isn't an options panel: it stays.
        /// </summary>
        private void CloseOtherOptions(string tag, ToolTrigger how)
        {
            ClipX2.UI.Panels.SidePanelView open = _panels.Current;
            if (open == null || open is ClipX2.UI.Panels.SpellingPanel) return;
            Type own = PanelFor(tag);
            if (own == open.GetType()) return;                       // the same tool again
            if (own != null && how == ToolTrigger.Click) return;     // its panel replaces this one
            _panels.Close();
        }
    }
}
