using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClipX2.TextEngine;
using ClipX2.UI;
using ClipX2.UI.Panels;

namespace ClipX2
{
    /// <summary>
    /// PowerButtons (ClipX 1's): the column of buttons on the right (panel2), Alt+1…Alt+9, the
    /// editor, and running a button's tasks one after another through <see cref="RunTool"/>.
    /// A run is one Undo step, except that an "Ask" task pauses it: the tool's side panel opens
    /// with a note, and its Apply carries the run on (closing the panel stops it).
    /// </summary>
    public partial class Form1
    {
        /// <summary>The "tag" Repeat (Ctrl+R) remembers for a whole PowerButton: this, then its Id.</summary>
        private const string PowerButtonTagPrefix = "powerbutton:";

        private PowerButtonBar _powerBar;

        private static List<PowerButton> PowerButtons => AppSettings.Current.PowerButtons;

        private void InitPowerButtons()
        {
            _powerBar = new PowerButtonBar { Dock = DockStyle.Fill };
            panel2.Controls.Add(_powerBar);
            _powerBar.SetButtons(PowerButtons);
            _powerBar.RunRequested += (s, b) => RunPowerButton(b);
            _powerBar.EditRequested += (s, b) => EditPowerButton(b);
            _powerBar.NewRequested += (s, e) => EditPowerButton(null);
            _powerBar.Reordered += (s, e) => AppSettings.Current.Save();
            _powerBar.ColumnsWanted += (s, n) => SetPowerButtonColumns(n, save: true);
            _powerBar.Hovered += (s, b) => DescribePowerButton(b);
            _powerBar.HoverEnded += (s, e) => _panels.Describe(null, null);
            _powerBar.MenuRequested += (s, at) => PowerButtonMenu(at);
            _panels.ViewHidden += (s, view) => PanelHiddenDuringRun(view);
            _panels.ConfirmClose = ConfirmPanelClose;
            SetPowerButtonColumns(AppSettings.Current.PowerButtonColumns, save: false);
        }

        /// <summary>Widens or narrows the PowerButton panel (its left edge moves; the text box gives or takes the room).</summary>
        private void SetPowerButtonColumns(int columns, bool save)
        {
            columns = Math.Max(1, Math.Min(PowerButtonBar.MaxColumns, columns));
            int width = PowerButtonBar.WidthFor(columns);
            int delta = width - panel2.Width;
            if (delta > 0 && richTextBox1.Width - delta < 200) return; // keep room for the text
            SuspendLayout();
            panel2.SetBounds(panel2.Left - delta, panel2.Top, width, panel2.Height);
            richTextBox1.Width -= delta;
            _powerBar.Columns = columns;
            ResumeLayout(true);
            if (save && AppSettings.Current.PowerButtonColumns != columns)
            {
                AppSettings.Current.PowerButtonColumns = columns;
                AppSettings.Current.Save();
            }
        }

        /// <summary>The hover help for a PowerButton: its name, key and what it does.</summary>
        private void DescribePowerButton(PowerButton b)
        {
            if (b == null)
            {
                _panels.Describe("New PowerButton", "Make a button that runs several tools one after another, each with its own options."
                    + "\r\n\r\nRight-click beside the buttons to bring in ClipX 1's PowerButtons.");
                return;
            }
            int n = PowerButtons.IndexOf(b) + 1;
            string key = n >= 1 && n <= 9 ? $" (Alt+{n})" : "";
            string tasks = b.Tasks.Count == 0 ? "Not set up yet."
                : string.Join(" → ", b.Tasks.Select(t => t.Name));
            _panels.Describe(b.Name + key, tasks + "\r\n\r\nClick to run it; right-click to change it; drag to move it.");
        }

        /// <summary>Alt+1…Alt+9 run the first nine PowerButtons; Shift+Alt+1…9 change them. True if the key was one of these.</summary>
        private bool PowerButtonKey(Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            Keys mods = keyData & Keys.Modifiers;
            if (key < Keys.D1 || key > Keys.D9 || (mods != Keys.Alt && mods != (Keys.Alt | Keys.Shift))) return false;
            int index = key - Keys.D1;
            if (index >= PowerButtons.Count)
            {
                if (mods == (Keys.Alt | Keys.Shift)) EditPowerButton(null);
                else ShowStatus($"There is no PowerButton {index + 1} yet: click + on the right to make one");
                return true;
            }
            if (mods == Keys.Alt) RunPowerButton(PowerButtons[index]);
            else EditPowerButton(PowerButtons[index]);
            return true;
        }

        /// <summary>Right-click beside the buttons: new, ClipX 1's PowerButtons, columns.</summary>
        private void PowerButtonMenu(Point at)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("New PowerButton…", null, (s, e) => EditPowerButton(null));
            menu.Items.Add(new ToolStripSeparator());
            var fromRegistry = new ToolStripMenuItem("Import ClipX 1's PowerButtons from this computer", null, (s, e) => ImportClipX1(fromFile: false))
            {
                Enabled = PowerButtonImport.RegistryHasButtons(),
            };
            menu.Items.Add(fromRegistry);
            menu.Items.Add("Import ClipX 1's PowerButtons from a preferences file…", null, (s, e) => ImportClipX1(fromFile: true));
            menu.Items.Add(new ToolStripSeparator());
            int columns = AppSettings.Current.PowerButtonColumns;
            menu.Items.Add("More columns", null, (s, e) => SetPowerButtonColumns(columns + 1, save: true)).Enabled = columns < PowerButtonBar.MaxColumns;
            menu.Items.Add("Fewer columns", null, (s, e) => SetPowerButtonColumns(columns - 1, save: true)).Enabled = columns > 1;
            menu.Closed += (s, e) => BeginInvoke((Action)menu.Dispose);
            menu.Show(_powerBar, at);
        }

        /// <summary>Brings ClipX 1's PowerButtons in after the ones already here (from its registry entries, or an exported .pref file).</summary>
        private void ImportClipX1(bool fromFile)
        {
            List<PowerButton> found;
            try
            {
                if (fromFile)
                {
                    using var dialog = new OpenFileDialog
                    {
                        Title = "ClipX 1 preferences file",
                        Filter = "ClipX 1 preferences (*.pref;*.xml)|*.pref;*.xml|All files|*.*",
                    };
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    found = PowerButtonImport.FromPrefFile(dialog.FileName);
                }
                else found = PowerButtonImport.FromRegistry();
            }
            catch (Exception ex) when (ex is System.Xml.XmlException || ex is FormatException || ex is System.IO.IOException || ex is UnauthorizedAccessException || ex is System.Security.SecurityException)
            {
                MessageBox.Show(this, "ClipX 1's PowerButtons couldn't be read.\r\n\r\n" + ex.Message, "Import PowerButtons", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (found.Count == 0)
            {
                MessageBox.Show(this, "No PowerButtons were found there.", "Import PowerButtons", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string names = string.Join("\r\n", found.Select(b => "• " + b.Name));
            if (MessageBox.Show(this, $"Add these {found.Count} PowerButtons after the ones you have?\r\n\r\n{names}", "Import PowerButtons",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;
            PowerButtons.AddRange(found);
            AppSettings.Current.Save();
            _powerBar.SetButtons(PowerButtons);
            List<string> missing = found.SelectMany(b => b.Tasks).Where(t => t.Tool == null).Select(t => t.Tag).Distinct().ToList();
            ShowStatus($"{found.Count} PowerButtons added" + (missing.Count > 0 ? $" (tools not in ClipX2 yet: {string.Join(", ", missing)})" : ""));
        }

        /************ The editor ************/

        /// <summary>Opens the editor on a PowerButton (null: a new one); saves what it returns.</summary>
        private void EditPowerButton(PowerButton b)
        {
            if (_run != null) StopRun("stopped, to change PowerButtons");
            bool isNew = b == null;
            PowerButton editing = isNew ? NewPowerButton() : b.Clone();
            using var editor = new PowerButtonEditor(editing, PowerButtons, ToolIcon, TryPowerButton)
            {
                ButtonNumber = isNew ? PowerButtons.Count + 1 : PowerButtons.IndexOf(b) + 1,
            };
            DialogResult result = editor.ShowDialog(this);
            if (editor.DeleteRequested && !isNew)
            {
                PowerButtons.Remove(b);
                // Tasks including it would do nothing now: they go too
                foreach (PowerButton other in PowerButtons)
                    other.Tasks.RemoveAll(t => t.Tag == "pbPBT" && t.Options is Guid id && id == b.Id);
            }
            else if (result == DialogResult.OK)
            {
                if (isNew) PowerButtons.Add(editing);
                else PowerButtons[PowerButtons.IndexOf(b)] = editing;
            }
            else return;
            AppSettings.Current.Save();
            _powerBar.SetButtons(PowerButtons);
        }

        /// <summary>A new button: a name, and an icon with its number on ClipX 1's green.</summary>
        private static PowerButton NewPowerButton()
        {
            int n = PowerButtons.Count + 1;
            var b = new PowerButton { Name = "PowerButton " + n };
            b.Icon.Png = IconPainter.SolidPng(PowerButtonImport.ClipX1Green);
            b.Icon.Text = new IconText { Characters = n.ToString(), FontName = "Segoe UI", Size = 40, Bold = true };
            IconPainter.Centre(b.Icon.Text);
            return b;
        }

        /// <summary>A tool's icon, as on its toolbar button (null if it has no button).</summary>
        private Image ToolIcon(string tag)
        {
            foreach (ToolStrip strip in ToolStrips)
                foreach (ToolStripItem item in strip.Items)
                    if (item.Tag is string t && t == tag && item.Image != null) return item.Image;
            return null;
        }

        /// <summary>The editor's "Try on the text": runs the button being edited at once (Ask tasks use the last-used options).</summary>
        private void TryPowerButton(PowerButton b) => StartRun(b, canAsk: false);

        /************ Running ************/

        /// <summary>A PowerButton being run: its tasks (included PowerButtons expanded) and how far it has got.</summary>
        private sealed class PowerRun
        {
            public PowerButton Button;
            public List<PowerTask> Tasks;
            public int Next;
            public bool CanAsk;
            /// <summary>The side panel the run is waiting on (an Ask task, or spelling), or null.</summary>
            public SidePanelView WaitingOn;
            /// <summary>Waiting for the panel to close (spelling), not for its Apply.</summary>
            public bool UntilClosed;
            public string WaitingTag;
            public IDisposable Undo, Bulk;
        }

        private PowerRun _run;

        /// <summary>Runs a PowerButton by its Id (Repeat, or an included PowerButton).</summary>
        private void RunPowerButton(string id)
        {
            PowerButton b = Guid.TryParse(id, out Guid g) ? PowerButtons.FirstOrDefault(p => p.Id == g) : null;
            if (b == null) { ShowStatus("That PowerButton has been deleted"); return; }
            RunPowerButton(b);
        }

        private void RunPowerButton(PowerButton b) => StartRun(b, canAsk: true);

        private void StartRun(PowerButton b, bool canAsk)
        {
            if (_run != null) StopRun(null);
            if (b.Tasks.Count == 0)
            {
                ShowStatus($"“{b.Name}” isn't set up yet: right-click it to choose its tools");
                return;
            }
            var tasks = new List<PowerTask>();
            string problem = Expand(b, tasks, new List<Guid>());
            if (problem != null) { ShowStatus($"“{b.Name}”: {problem}"); return; }
            _run = new PowerRun { Button = b, Tasks = tasks, CanAsk = canAsk };
            ContinueRun();
        }

        /// <summary>The tasks, with included PowerButtons' tasks in their place; why not, if one includes itself or is missing.</summary>
        private static string Expand(PowerButton b, List<PowerTask> into, List<Guid> within)
        {
            if (within.Contains(b.Id)) return $"it includes itself (through “{b.Name}”)";
            if (within.Count > 10) return "PowerButtons are included too deeply";
            within.Add(b.Id);
            foreach (PowerTask t in b.Tasks)
            {
                if (t.Tag != "pbPBT") { into.Add(t); continue; }
                PowerButton inner = t.Options is Guid id ? PowerButtons.FirstOrDefault(p => p.Id == id) : null;
                if (inner == null) return "a PowerButton it includes has been deleted";
                string problem = Expand(inner, into, within);
                if (problem != null) return problem;
            }
            within.RemoveAt(within.Count - 1);
            return null;
        }

        /// <summary>Runs tasks until the end, or until one waits on a side panel.</summary>
        private void ContinueRun()
        {
            PowerRun run = _run;
            if (run == null) return;
            ClipXRichTextBox box = richTextBox1;
            run.Bulk ??= box.BeginBulkEdit();
            run.Undo ??= box.BeginUndoGroup();
            while (run.Next < run.Tasks.Count)
            {
                PowerTask task = run.Tasks[run.Next++];
                string why = WhyNot(task);
                if (why != null) { StopRun($"stopped at {task.Name} (task {run.Next} of {run.Tasks.Count}): {why}"); return; }

                if (task.Use == TaskOptions.Ask && run.CanAsk && PowerTaskTypes.CanAsk(task.Tag))
                {
                    Wait(run, task, untilClosed: false);
                    return;
                }
                try
                {
                    object options = task.Use == TaskOptions.Own && task.Options != null
                        ? PowerTaskTypes.CopyOf(task.Options, task.Options.GetType()) : null;
                    RunTool(task.Tag, ToolTrigger.PowerButton, options);
                }
                catch (Exception ex) when (ex is SearchPatternException || ex is NotSupportedException || ex is ArgumentException || ex is InvalidOperationException)
                {
                    StopRun($"stopped at {task.Name} (task {run.Next} of {run.Tasks.Count}): {ex.Message}");
                    return;
                }
                if (_run != run) return; // stopped meanwhile
                // Spelling works with the user: carry on when its panel closes
                if (run.CanAsk && (task.Tag == "chSPE" || task.Tag == "chGRA") && _panels.Current is SpellingPanel)
                {
                    Wait(run, task, untilClosed: true);
                    return;
                }
            }
            string last = toolStripLabelStatus.Text;
            EndSegment(run);
            _run = null;
            _lastTool = PowerButtonTagPrefix + run.Button.Id; // Ctrl+R repeats the whole button, as ClipX 1
            ShowStatus($"“{run.Button.Name}” done" + (string.IsNullOrEmpty(last) ? "" : ": " + last));
        }

        /// <summary>Why a task can't run now (null if it can).</summary>
        private string WhyNot(PowerTask task)
        {
            if (task.Tool == null) return $"ClipX2 doesn't have the tool “{task.Tag}”";
            if (PowerTaskTypes.RichTextOnly(task.Tag) && !_isRtfMode) return "it is for rich text, and the text is plain";
            return null;
        }

        /// <summary>Pauses the run on a side panel: an Ask task's options (Apply carries on), or spelling (closing carries on).</summary>
        private void Wait(PowerRun run, PowerTask task, bool untilClosed)
        {
            EndSegment(run); // what has been done so far is one Undo step
            if (!untilClosed)
                RunTool(task.Tag == "foUNI" ? "foIND" : task.Tag, ToolTrigger.Click); // its panel, with the last-used options
            SidePanelView view = _panels.Current;
            if (view == null) { ContinueRun(); return; }
            run.WaitingOn = view;
            run.UntilClosed = untilClosed;
            run.WaitingTag = task.Tag;
            view.Applied -= RunPanelApplied;
            view.Applied += RunPanelApplied;
            if (untilClosed)
                ShowStatus($"“{run.Button.Name}” carries on when you close the {task.Name} panel");
            else
            {
                view.ShowGo(task.Tag); // its Apply reads "Go", in orange
                ShowStatus($"“{run.Button.Name}” is waiting: choose the {task.Name} options, then Go");
            }
        }

        /// <summary>
        /// The user is closing a side panel (its close cross, Esc): if a PowerButton waits on it for
        /// an "Ask" task, closing stops the run, so ask first. False keeps the panel open.
        /// </summary>
        private bool ConfirmPanelClose(SidePanelView view)
        {
            PowerRun run = _run;
            if (run == null || run.WaitingOn != view || run.UntilClosed) return true;
            int n = PowerButtons.IndexOf(run.Button) + 1;
            string which = n > 0 ? $"PowerButton {n} (“{run.Button.Name}”)" : $"PowerButton “{run.Button.Name}”";
            return MessageBox.Show(this, $"Stop {which} running?", "PowerButton",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>The panel a run waits on has applied: an Ask task done, so on with the rest.</summary>
        private void RunPanelApplied(object sender, string action)
        {
            PowerRun run = _run;
            if (run == null || run.WaitingOn != sender || run.UntilClosed) return;
            bool done = run.WaitingTag switch
            {
                "edREP" => action == "ReplaceAll",
                "edSRC" => action == "Find", // Next (Go), or Previous
                _ => true,
            };
            if (!done) return;
            SidePanelView view = run.WaitingOn;
            StopWaiting(run);
            if (_panels.Current == view) _panels.Close(); // its work is done
            BeginInvoke((Action)ContinueRun); // after the panel's own Apply has finished
        }

        /// <summary>A side panel has closed or been replaced: if the run was waiting on it, spelling carries on, an Ask stops.</summary>
        private void PanelHiddenDuringRun(SidePanelView view)
        {
            PowerRun run = _run;
            if (run == null || run.WaitingOn != view) return;
            bool carryOn = run.UntilClosed;
            StopWaiting(run);
            if (carryOn) BeginInvoke((Action)ContinueRun);
            else StopRun($"stopped at task {run.Next} of {run.Tasks.Count} (its panel was closed)");
        }

        private void StopWaiting(PowerRun run)
        {
            if (run.WaitingOn == null) return;
            run.WaitingOn.Applied -= RunPanelApplied;
            run.WaitingOn.HideGo();
            run.WaitingOn = null;
        }

        /// <summary>Stops the run (what was done stays, as one Undo step); the message, if any, in the status bar.</summary>
        private void StopRun(string message)
        {
            PowerRun run = _run;
            if (run == null) return;
            _run = null;
            StopWaiting(run);
            EndSegment(run);
            if (message != null) ShowStatus($"“{run.Button.Name}” {message}");
        }

        private static void EndSegment(PowerRun run)
        {
            run.Undo?.Dispose();
            run.Undo = null;
            run.Bulk?.Dispose();
            run.Bulk = null;
        }
    }
}
