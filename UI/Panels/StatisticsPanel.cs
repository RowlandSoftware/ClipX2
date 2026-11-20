using System;
using System.Text;
using System.Windows.Forms;
using ClipX2.TextEngine;

namespace ClipX2.UI.Panels
{
    /// <summary>
    /// Show statistics (chSTAT; ClipX 1's Statistics dialogue), in the side panel: the selection's
    /// figures first (only when there is a selection), then the whole text's. They follow the
    /// selection and the text while the panel is open. Readability scores are for English.
    /// </summary>
    public partial class StatisticsPanel : SidePanelView
    {
        private ClipXRichTextBox _box;
        private Func<bool> _isRichText;
        private bool _listening;

        public StatisticsPanel()
        {
            InitializeComponent();
            listViewStats.Resize += (s, e) => FitColumns();
        }

        /// <summary>Connects the panel to the editor; once, after creation.</summary>
        internal void Attach(ClipXRichTextBox box, Func<bool> isRichText)
        {
            _box = box;
            _isRichText = isRichText;
            OnActivated(); // the host activated the panel before it was attached
        }

        internal bool IsAttached => _box != null;

        /// <summary>Plain text / rich text switched: the size is worked out differently.</summary>
        protected internal override void OnModeChanged() => UpdateFigures();

        protected internal override void OnActivated()
        {
            if (_box == null) return;
            if (!_listening)
            {
                _box.TextSettled += Changed;
                _box.SelectionSettled += Changed;
                _listening = true;
            }
            UpdateFigures();
        }

        protected internal override void OnDeactivated()
        {
            if (_box == null || !_listening) return;
            _box.TextSettled -= Changed;
            _box.SelectionSettled -= Changed;
            _listening = false;
        }

        private void Changed(object sender, EventArgs e) => UpdateFigures();

        private void FitColumns()
        {
            int width = listViewStats.ClientSize.Width;
            columnValue.Width = Math.Max(60, width * 45 / 100);
            columnName.Width = Math.Max(60, width - columnValue.Width - 1);
        }

        /// <summary>Works out the figures and fills the list (selection first, then the whole text).</summary>
        internal void UpdateFigures()
        {
            if (_box == null) return;
            string text = _box.Text;
            bool rich = _isRichText?.Invoke() ?? false;
            int start = _box.SelectionStart, length = _box.SelectionLength;

            // The text's language is taken to be the spelling language. Other than English,
            // syllables come from the language's hyphenation patterns (downloaded with its dictionary).
            string language = AppSettings.Current.Spelling.Language ?? "en_GB";
            Func<string, int> syllables = null;
            if (TextStatistics.NeedsHyphenation(language))
            {
                var hyphenator = HyphenationFiles.For(language);
                if (hyphenator != null) syllables = hyphenator.Syllables;
            }

            listViewStats.BeginUpdate();
            try
            {
                listViewStats.Items.Clear();
                if (length > 0)
                {
                    var selection = TextStatistics.Measure(text, start, length, language, syllables);
                    long size = rich ? _box.SelectedRtf.Length : Utf8Size(text, start, length);
                    AddGroup("Selection", selection, size, rich);
                }
                var whole = TextStatistics.Measure(text, 0, text.Length, language, syllables);
                AddGroup("Whole text", whole, rich ? _box.Rtf.Length : Utf8Size(text, 0, text.Length), rich);
            }
            finally { listViewStats.EndUpdate(); }
            FitColumns();
        }

        private void AddGroup(string title, TextStatistics.Counts c, long size, bool rich)
        {
            // A heading row (bold), after a gap if something is above it
            if (listViewStats.Items.Count > 0) listViewStats.Items.Add(new ListViewItem(new[] { "", "" }));
            listViewStats.Items.Add(new ListViewItem(new[] { title, "" })
            {
                Font = new System.Drawing.Font(listViewStats.Font, System.Drawing.FontStyle.Bold),
                UseItemStyleForSubItems = true,
            });
            void Row(string name, string value, string tip = null)
            {
                var item = new ListViewItem(new[] { "  " + name, value }) { ToolTipText = tip ?? "" };
                listViewStats.Items.Add(item);
            }
            Row("Words", $"{c.Words:N0}");
            Row("Characters", $"{c.Characters:N0}", "Not counting line breaks");
            Row("Without spaces", $"{c.NonSpaceCharacters:N0}", "Characters other than spaces, tabs and line breaks");
            Row("Sentences", $"{c.Sentences:N0}", "A paragraph without a full stop (a heading, a list item) counts as one");
            Row("Paragraphs", $"{c.Paragraphs:N0}", "Blank lines aren't counted");
            Row("Words per sentence", c.Sentences == 0 ? "–" : $"{c.WordsPerSentence:0.0}");
            Row(rich ? "Size (RTF)" : "Size (UTF-8)", Bytes(size),
                rich ? "As rich text (RTF), formatting included" : "As a plain text file (UTF-8, Windows line ends)");
            // Readability: why there is none, if there isn't
            string languageName = DictionaryCatalogue.ShortLocale(TextStatistics.LanguageCode(c.Language));
            if (c.Formula == null)
            {
                Row("Readability", "–", $"No readability formula for {languageName}: there are for English, German, French, Spanish, Italian, Dutch, Portuguese and Russian");
                return;
            }
            if (!c.SyllablesCounted)
            {
                Row("Readability", "–", $"Needs {languageName}'s hyphenation patterns to count syllables: in Options, Spelling, update (or download) the {c.Language} dictionary");
                return;
            }
            if (!c.HasReadability)
            {
                Row("Readability", "–", "Needs at least 10 words and a sentence");
                return;
            }
            // Explanations in the tooltips (no room for a note); a warning when the text is short
            bool english = TextStatistics.IsEnglish(c.Language);
            string rough = c.Words < 100 ? " (rough: under 100 words)" : "";
            string from = english ? "" : $" Syllables counted with {languageName}'s hyphenation patterns.";
            double ease = Math.Max(0, Math.Min(100, c.ReadingEase.Value)); // very short sentences can go past 100
            Row("Reading ease", $"{ease:0}", $"{c.Formula.Name}: 0 (very hard) to 100 (very easy), higher is easier; 60–70 is plain {(english ? "English" : "language")}." + from + rough);
            Row("Level", TextStatistics.Counts.EaseDescription(ease, english), "Flesch's description of the reading-ease score" + rough);
            if (c.FleschKincaidGrade is double grade)
                Row("Grade", $"{Math.Max(0, grade):0.0}", "Flesch–Kincaid grade: years of schooling needed to comprehend" + rough);
            if (c.FogIndex is double fog)
                Row("Fog index", $"{fog:0.0}", "Gunning Fog index: years of schooling needed to comprehend; about 12 or less suits a wide audience" + rough);
        }

        /// <summary>Bytes as a plain text file: UTF-8, line breaks as CR LF.</summary>
        private static long Utf8Size(string text, int start, int length)
        {
            string part = text.Substring(start, Math.Min(length, text.Length - start));
            int breaks = 0;
            foreach (char ch in part) if (ch == '\n') breaks++;
            return Encoding.UTF8.GetByteCount(part) + breaks; // each \n becomes \r\n
        }

        private static string Bytes(long n) =>
            n < 10_000 ? $"{n:N0} bytes" : n < 10_000_000 ? $"{n / 1024.0:N0} KB" : $"{n / 1048576.0:N1} MB";
    }
}
