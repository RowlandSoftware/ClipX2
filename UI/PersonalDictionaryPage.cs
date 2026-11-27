using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClipX2.TextEngine.Spelling;

namespace ClipX2.UI
{
    /// <summary>
    /// The Options dialog's Personal dictionary page: your words (persdict.dat), with a search,
    /// and Add, Change and Delete. A word the dictionary in use already has isn't added unless you
    /// say so (your words count in every language, so you may want it for checking in another).
    /// A new word comes with its other forms, made with the endings of a dictionary word that ends
    /// the same way (geocache, like cache: geocaches, geocached, geocaching). Changes are saved
    /// at once, as the spelling panel's Add to dictionary is.
    /// </summary>
    public partial class PersonalDictionaryPage : UserControl
    {
        private PersonalDictionary _personal;
        private SpellingDictionary _dictionary;  // the dictionary in use, without your words
        private WordForms _forms;
        private string _language;
        private bool _filling;

        public PersonalDictionaryPage()
        {
            InitializeComponent();
            textBoxFind.TextChanged += (s, e) => Fill();
            checkBoxKnownOnly.CheckedChanged += (s, e) => Fill();
            listBoxWords.SelectedIndexChanged += (s, e) => WordSelected();
            listBoxWords.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) { DeleteWords(); e.Handled = true; } };
            textBoxWord.TextChanged += (s, e) => CheckWord();
            textBoxWord.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.Handled = e.SuppressKeyPress = true; // not the dialog's OK
                if (buttonAdd.Enabled) AddWord();
            };
            checkBoxAnyway.CheckedChanged += (s, e) => UpdateButtons();
            checkedListBoxForms.ItemCheck += (s, e) => BeginInvoke(new Action(UpdateButtons)); // after the tick changes
            buttonAdd.Click += (s, e) => AddWord();
            buttonChange.Click += (s, e) => ChangeWord();
            buttonDelete.Click += (s, e) => DeleteWords();
            checkBoxAllForms.CheckedChanged += (s, e) =>
            {
                if (_filling) return;
                for (int i = 0; i < checkedListBoxForms.Items.Count; i++) checkedListBoxForms.SetItemChecked(i, checkBoxAllForms.Checked);
            };
            UpdateButtons();
        }

        /// <summary>Words were added, changed or deleted: the spelling checker should read them again.</summary>
        public bool Changed { get; private set; }

        /// <summary>Whether <see cref="Open"/> has been called.</summary>
        public bool IsOpen => _personal != null;

        /// <summary>Reads the words, and (in the background) the dictionary in use, to check against.</summary>
        public async void Open(string personalPath, string language)
        {
            _personal = new PersonalDictionary(personalPath);
            _language = language;
            Fill();
            string name = DictionaryCatalogue.DescribeLocale(language);
            labelLanguage.Text = $"Loading the {name} dictionary…";
            checkBoxKnownOnly.Enabled = false;
            string folder = DictionaryCatalogue.FolderOf(language);
            try
            {
                if (folder == null) throw new FileNotFoundException(language);
                var loaded = await Task.Run(() => (SpellingDictionary.Load(folder, language),
                    WordForms.Load(Path.Combine(folder, language + ".aff"), Path.Combine(folder, language + ".dic"))));
                if (IsDisposed) return;
                (_dictionary, _forms) = loaded;
                labelLanguage.Text = $"Checked against the {name} dictionary. Changes are saved at once.";
                checkBoxKnownOnly.Enabled = true;
                CheckWord();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is InvalidDataException)
            {
                if (!IsDisposed) labelLanguage.Text = $"The {name} dictionary couldn't be read, so new words aren't checked against it. Changes are saved at once.";
            }
        }

        /************ The list ************/

        private void Fill()
        {
            if (_personal == null) return;
            string find = textBoxFind.Text.Trim();
            var selected = listBoxWords.SelectedItems.Cast<string>().ToList();
            IEnumerable<string> words = _personal.Words
                .OrderBy(w => w, StringComparer.CurrentCultureIgnoreCase).ThenBy(w => w, StringComparer.Ordinal);
            if (find.Length > 0) words = words.Where(w => w.IndexOf(find, StringComparison.CurrentCultureIgnoreCase) >= 0);
            if (checkBoxKnownOnly.Checked && _dictionary != null) words = words.Where(_dictionary.IsCorrect);
            var list = words.ToList();
            _filling = true;
            listBoxWords.BeginUpdate();
            listBoxWords.Items.Clear();
            listBoxWords.Items.AddRange(list.ToArray());
            foreach (string w in selected)
            {
                int i = listBoxWords.Items.IndexOf(w);
                if (i >= 0) listBoxWords.SetSelected(i, true);
            }
            listBoxWords.EndUpdate();
            _filling = false;
            int all = _personal.Words.Count;
            labelCount.Text = list.Count == all ? $"{all:N0} words" : $"{list.Count:N0} of {all:N0} words";
            UpdateButtons();
        }

        private void Select(string word)
        {
            _filling = true;
            listBoxWords.ClearSelected();
            int i = listBoxWords.Items.IndexOf(word);
            if (i >= 0) { listBoxWords.SetSelected(i, true); listBoxWords.TopIndex = Math.Max(0, i - 3); }
            _filling = false;
            UpdateButtons();
        }

        private void WordSelected()
        {
            if (_filling) return;
            if (listBoxWords.SelectedItems.Count == 1) textBoxWord.Text = (string)listBoxWords.SelectedItem;
            UpdateButtons();
        }

        /************ Checking a word ************/

        private string Word => textBoxWord.Text.Trim();

        /// <summary>The dictionary in use has the word (your words aside).</summary>
        private bool Known(string word) => _dictionary != null && _dictionary.IsCorrect(word);

        private void CheckWord()
        {
            string word = Word;
            string name = DictionaryCatalogue.DescribeLocale(_language ?? "");
            bool known = word.Length > 0 && Known(word);
            if (word.Length == 0) labelCheck.Text = "";
            else if (word.IndexOf(' ') >= 0) labelCheck.Text = "One word at a time (no spaces).";
            else if (_personal != null && _personal.Contains(word)) labelCheck.Text = $"“{word}” is in your dictionary.";
            else if (known) labelCheck.Text = $"The {name} dictionary already has “{word}”.";
            else labelCheck.Text = "";
            checkBoxAnyway.Visible = known;
            if (!known) checkBoxAnyway.Checked = false;
            ShowForms(word);
            UpdateButtons();
        }

        /// <summary>The word's other forms, those neither dictionary has; the usual endings ticked.</summary>
        private void ShowForms(string word)
        {
            _filling = true;
            checkedListBoxForms.BeginUpdate();
            checkedListBoxForms.Items.Clear();
            string model = null;
            if (_forms != null && word.Length > 1 && word.IndexOf(' ') < 0)
            {
                var (like, forms) = _forms.Suggest(word, f => Known(f) || _personal.Contains(f));
                model = like;
                bool english = (_language ?? "").StartsWith("en", StringComparison.OrdinalIgnoreCase);
                foreach (string f in forms.OrderBy(f => IsUsualEnding(word, f) ? 0 : 1).ThenBy(f => f, StringComparer.CurrentCulture))
                    checkedListBoxForms.Items.Add(f, english && IsUsualEnding(word, f));
            }
            checkedListBoxForms.EndUpdate();
            labelForms.Text = checkedListBoxForms.Items.Count == 0 ? "Other forms to add:"
                : string.Equals(model, word, StringComparison.Ordinal) ? "Other forms to add:"
                : $"Other forms to add (endings as “{model}”):";
            checkBoxAllForms.Checked = checkedListBoxForms.Items.Count > 0 && checkedListBoxForms.CheckedItems.Count == checkedListBoxForms.Items.Count;
            checkBoxAllForms.Enabled = checkedListBoxForms.Items.Count > 0;
            _filling = false;
        }

        /// <summary>English plurals, verb forms and the like: -s, -es, -ies, -ed, -d, -ied, -ing (with a doubled consonant, or a dropped e).</summary>
        private static bool IsUsualEnding(string word, string form)
        {
            // Not for a word that is already such a form: frobnicating, frobnicated, pulls
            if (word.EndsWith("ing", StringComparison.Ordinal) || word.EndsWith("ed", StringComparison.Ordinal) || word.EndsWith("s", StringComparison.Ordinal)) return false;
            string w = word, last = word.Substring(word.Length - 1), stem = word.Substring(0, word.Length - 1);
            var usual = new List<string> { w + "s", w + "es", w + "ed", w + "d", w + "ing", w + last + "ed", w + last + "ing" };
            if (last == "y") usual.AddRange(new[] { stem + "ies", stem + "ied" });
            if (last == "e") usual.Add(stem + "ing");
            return usual.Contains(form, StringComparer.Ordinal);
        }

        private void UpdateButtons()
        {
            string word = Word;
            bool valid = _personal != null && word.Length > 0 && word.IndexOf(' ') < 0;
            bool allowed = valid && (!Known(word) || checkBoxAnyway.Checked);
            int selected = listBoxWords.SelectedItems.Count;
            // A word you have: Add adds the forms ticked for it
            buttonAdd.Enabled = allowed && (!_personal.Contains(word) || checkedListBoxForms.CheckedItems.Count > 0);
            buttonChange.Enabled = allowed && selected == 1 && !_personal.Contains(word);
            buttonDelete.Enabled = selected > 0;
        }

        /************ Add, change, delete ************/

        private void AddWord()
        {
            string word = Word;
            var forms = checkedListBoxForms.CheckedItems.Cast<string>().ToList();
            bool had = _personal.Contains(word);
            int added = _personal.AddRange(new[] { word }.Concat(forms));
            if (added == 0) return;
            Changed = true;
            textBoxFind.Text = "";
            Fill();
            Select(word);
            labelCheck.Text = had ? (forms.Count == 1 ? $"Added “{forms[0]}”." : $"Added {forms.Count} forms of “{word}”.")
                : forms.Count == 0 ? $"Added “{word}”."
                : forms.Count == 1 ? $"Added “{word}” and “{forms[0]}”."
                : $"Added “{word}” and {forms.Count} other forms.";
            ShowForms("");
            checkBoxAnyway.Visible = false;
            UpdateButtons();
        }

        private void ChangeWord()
        {
            if (listBoxWords.SelectedItems.Count != 1) return;
            string old = (string)listBoxWords.SelectedItem, word = Word;
            if (!_personal.Change(old, word)) return;
            Changed = true;
            Fill();
            Select(word);
            labelCheck.Text = $"Changed “{old}” to “{word}”.";
        }

        private void DeleteWords()
        {
            var words = listBoxWords.SelectedItems.Cast<string>().ToList();
            if (words.Count == 0) return;
            if (words.Count > 1 && MessageBox.Show(this, $"Delete these {words.Count} words from your dictionary?", "Personal dictionary",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;
            int index = listBoxWords.SelectedIndex;
            _personal.RemoveRange(words);
            Changed = true;
            Fill();
            if (listBoxWords.Items.Count > 0) listBoxWords.SetSelected(Math.Min(index, listBoxWords.Items.Count - 1), true);
            labelCheck.Text = words.Count == 1 ? $"Deleted “{words[0]}”." : $"Deleted {words.Count} words.";
        }
    }
}
