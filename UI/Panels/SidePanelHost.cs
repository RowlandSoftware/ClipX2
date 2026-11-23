using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Manages the side panel (Form1.panel1). Shows one <see cref="SidePanelView"/> at a time,
    /// creating each the first time it is wanted and keeping it for reuse (so a tool remembers
    /// its settings for the session). When no tool is open, it shows the <see cref="HelpView"/>,
    /// which describes the toolstrip button under the mouse, in place of tooltips; while a tool is
    /// open, the description goes in the status bar instead, so the tool keeps the whole panel.
    /// Descriptions are the buttons' ToolTipText, so they are edited in the designer as before.
    /// </summary>
    internal sealed class SidePanelHost
    {
        private readonly Panel _container;
        private readonly ToolStripLabel _statusHint;
        private readonly HelpView _help;
        private readonly Dictionary<Type, SidePanelView> _views = new Dictionary<Type, SidePanelView>();
        private SidePanelView _current;

        /// <param name="container">The side panel.</param>
        /// <param name="statusHint">Where descriptions go while a tool is open (may be null).</param>
        public SidePanelHost(Panel container, ToolStripLabel statusHint)
        {
            _container = container;
            _statusHint = statusHint;
            _help = new HelpView { Dock = DockStyle.Fill };
            container.Controls.Add(_help);
        }

        /// <summary>A view has been hidden: closed, or replaced by another (a PowerButton waiting on it stops).</summary>
        public event EventHandler<SidePanelView> ViewHidden;

        /// <summary>The open tool view, or null if the help view is showing.</summary>
        public SidePanelView Current => _current;

        /// <summary>Shows the view of type <typeparamref name="T"/>, creating it the first time.</summary>
        public T Show<T>() where T : SidePanelView, new()
        {
            if (!_views.TryGetValue(typeof(T), out SidePanelView view))
            {
                view = new T { Dock = DockStyle.Fill, Visible = false };
                view.CloseRequested += (s, e) => { if (_current == s && (ConfirmClose?.Invoke(view) ?? true)) Close(); };
                _views[typeof(T)] = view;
                _container.Controls.Add(view);
            }

            if (_current != view)
            {
                SidePanelView hidden = _current;
                if (_current != null) { _current.Visible = false; _current.OnDeactivated(); }
                _current = view;
                _help.Visible = false;
                view.Visible = true;
                view.OnActivated();
                if (hidden != null) ViewHidden?.Invoke(this, hidden);
            }
            return (T)view;
        }

        /// <summary>Closes the open view and goes back to the help view.</summary>
        public void Close()
        {
            if (_current == null) return;
            SidePanelView closing = _current;
            _current = null;
            closing.Visible = false;
            closing.OnDeactivated();
            _help.Visible = true;
            if (_statusHint != null) _statusHint.Text = "";
            ViewHidden?.Invoke(this, closing);
        }

        /// <summary>
        /// Asked when the user closes a view (its close cross, Esc): false keeps it open (a
        /// PowerButton waiting on it asks whether to stop). Not asked when a tool closes it.
        /// </summary>
        public Func<SidePanelView, bool> ConfirmClose { get; set; }

        /// <summary>Describes something under the mouse that isn't a toolstrip button (a PowerButton); null to clear.</summary>
        public void Describe(string title, string text)
        {
            if (_current == null) _help.ShowDescription(title, text);
            else if (_statusHint != null) _statusHint.Text = text ?? "";
        }

        /// <summary>
        /// Makes every toolstrip under <paramref name="root"/> describe its buttons here instead
        /// of in tooltips. Items without ToolTipText keep quiet.
        /// </summary>
        public void AttachHoverHelp(Control root)
        {
            foreach (Control c in root.Controls)
            {
                if (c is ToolStrip strip)
                {
                    strip.ShowItemToolTips = false;
                    foreach (ToolStripItem item in strip.Items) AttachHoverHelp(item);
                }
                if (c.HasChildren) AttachHoverHelp(c);
            }
        }

        private void AttachHoverHelp(ToolStripItem item)
        {
            if (item == _statusHint) return;
            item.MouseEnter += (s, e) => ShowDescription(item);
            item.MouseLeave += (s, e) => ShowDescription(null);
            if (item is ToolStripDropDownItem dropDown)
                foreach (ToolStripItem sub in dropDown.DropDownItems) AttachHoverHelp(sub);
        }

        private void ShowDescription(ToolStripItem item)
        {
            string title = item == null ? null : (string.IsNullOrEmpty(item.Text) ? item.AccessibleName : item.Text);
            string text = item?.ToolTipText;
            if (_current == null)
                _help.ShowDescription(title, text);
            else if (_statusHint != null)
                _statusHint.Text = text ?? "";
        }
    }
}
