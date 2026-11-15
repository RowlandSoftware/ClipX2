using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WeCantSpell.Hunspell;

namespace ClipX2.TextEngine.Spelling
{
    /// <summary>
    /// A Hunspell dictionary (.aff + .dic), plus any words added at run time (the personal
    /// dictionary). Uses WeCantSpell.Hunspell, a managed port of Hunspell, so there is no native
    /// DLL to break (ClipX 1's NHunspell wrapper stopped working on later Windows).
    /// Results of <see cref="IsCorrect"/> are cached, as most texts repeat their words a lot.
    /// </summary>
    public sealed class SpellingDictionary
    {
        private readonly WordList _words;
        private readonly Dictionary<string, bool> _cache = new Dictionary<string, bool>(StringComparer.Ordinal);
        private readonly object _lock = new object();

        private SpellingDictionary(WordList words, string locale)
        {
            _words = words;
            Locale = locale;
        }

        /// <summary>The locale, e.g. "en_GB" (taken from the file name).</summary>
        public string Locale { get; }

        /// <summary>Loads e.g. en_GB.aff and en_GB.dic from <paramref name="folder"/>.</summary>
        public static SpellingDictionary Load(string folder, string locale)
        {
            string dic = Path.Combine(folder, locale + ".dic");
            string aff = Path.Combine(folder, locale + ".aff");
            if (!File.Exists(dic)) throw new FileNotFoundException("Dictionary not found", dic);
            if (!File.Exists(aff)) throw new FileNotFoundException("Affix file not found", aff);
            return new SpellingDictionary(WordList.CreateFromFiles(dic, aff), locale);
        }

        /// <summary>Loads in the background (a large dictionary takes a fraction of a second).</summary>
        public static Task<SpellingDictionary> LoadAsync(string folder, string locale, CancellationToken cancel = default) =>
            Task.Run(() => Load(folder, locale), cancel);

        /// <summary>The locales for which <paramref name="folder"/> has both an .aff and a .dic file.</summary>
        public static IReadOnlyList<string> AvailableLocales(string folder) =>
            Directory.Exists(folder)
                ? Directory.GetFiles(folder, "*.dic")
                    .Select(Path.GetFileNameWithoutExtension)
                    .Where(l => File.Exists(Path.Combine(folder, l + ".aff")))
                    .OrderBy(l => l, StringComparer.OrdinalIgnoreCase)
                    .ToList()
                : new List<string>();

        /// <summary>Adds a word for this run (personal dictionary, Add to dictionary).</summary>
        public void Add(string word)
        {
            word = Normalise(word?.Trim());
            if (string.IsNullOrEmpty(word)) return;
            lock (_lock)
            {
                _words.Add(word);
                _cache.Clear(); // "Add" can make other forms correct too
            }
        }

        public void AddRange(IEnumerable<string> words)
        {
            lock (_lock)
            {
                foreach (string w in words)
                {
                    string word = Normalise(w?.Trim());
                    if (!string.IsNullOrEmpty(word)) _words.Add(word);
                }
                _cache.Clear();
            }
        }

        /// <summary>
        /// True if the dictionary knows the word. Curly apostrophes count as straight ones. A
        /// hyphenated word is correct if the whole is known or every part is.
        /// </summary>
        public bool IsCorrect(string word)
        {
            if (string.IsNullOrEmpty(word)) return true;
            string w = Normalise(word);
            lock (_lock)
            {
                if (_cache.TryGetValue(w, out bool known)) return known;
                known = _words.Check(w);
                // Possessive of a known word, which many names lack in the dictionary (O'Neill's)
                if (!known && w.Length > 2 && w.EndsWith("'s", StringComparison.OrdinalIgnoreCase))
                    known = _words.Check(w.Substring(0, w.Length - 2));
                if (!known && w.IndexOf('-') > 0)
                    known = w.Split('-').All(part => part.Length == 0 || _words.Check(part));
                _cache[w] = known;
                return known;
            }
        }

        /// <summary>
        /// Suggestions, best first. If the word was written with a curly apostrophe, so are the
        /// suggestions. Can take tens of milliseconds, so ask only for the word being shown.
        /// </summary>
        public IReadOnlyList<string> Suggest(string word, int max = 10)
        {
            if (string.IsNullOrEmpty(word)) return Array.Empty<string>();
            bool curly = word.IndexOf('’') >= 0;
            string w = Normalise(word);
            var list = new List<string>();
            lock (_lock)
            {
                // Swapped letters first (teh → the): the commonest typing slip, which Hunspell
                // doesn't always rank highly for capitalised words
                for (int i = 0; i + 1 < w.Length; i++)
                {
                    if (w[i] == w[i + 1]) continue;
                    char[] c = w.ToCharArray();
                    (c[i], c[i + 1]) = (c[i + 1], c[i]);
                    string swapped = new string(c);
                    if (_words.Check(swapped)) list.Add(swapped);
                }
                foreach (string s in _words.Suggest(w))
                {
                    if (list.Count >= max) break;
                    list.Add(s);
                }
            }
            // The dictionary may give either apostrophe; use the one the word was written with
            for (int i = 0; i < list.Count; i++)
                list[i] = curly ? list[i].Replace('\'', '’') : list[i].Replace('’', '\'');
            return list.Distinct(StringComparer.Ordinal).Take(max).ToList();
        }

        private static string Normalise(string word) => word?.Replace('’', '\'').Replace('‘', '\'');
    }
}
