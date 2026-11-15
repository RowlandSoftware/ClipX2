using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ClipX2.TextEngine.Spelling
{
    /// <summary>
    /// The user's own words: a UTF-8 text file, one word per line (the same format as ClipX 1's
    /// persdict.dat, so that file can be used as it is).
    /// </summary>
    public sealed class PersonalDictionary
    {
        private readonly SortedSet<string> _words = new SortedSet<string>(StringComparer.Ordinal);

        public PersonalDictionary(string path)
        {
            Path = path;
            if (File.Exists(path))
                foreach (string line in File.ReadAllLines(path, Encoding.UTF8))
                {
                    string w = line.Trim();
                    if (w.Length > 0) _words.Add(w);
                }
        }

        public string Path { get; }
        public IReadOnlyCollection<string> Words => _words;

        /// <summary>Adds a word and saves the file. False if it was already there.</summary>
        public bool Add(string word)
        {
            word = word?.Trim();
            if (string.IsNullOrEmpty(word) || !_words.Add(word)) return false;
            Save();
            return true;
        }

        /// <summary>Removes a word and saves the file (for a future "edit dictionary" list).</summary>
        public bool Remove(string word)
        {
            if (!_words.Remove(word)) return false;
            Save();
            return true;
        }

        private void Save()
        {
            string dir = System.IO.Path.GetDirectoryName(Path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllLines(Path, _words.ToArray(), new UTF8Encoding(false));
        }
    }
}
