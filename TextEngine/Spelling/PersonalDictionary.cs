using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ClipX2.TextEngine.Spelling
{
    /// <summary>
    /// The user's own words: a UTF-8 text file, one word per line (the same format as ClipX 1's
    /// persdict.dat, so that file can be used as it is). Every change is saved at once.
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

        public bool Contains(string word) => word != null && _words.Contains(word.Trim());

        /// <summary>Adds a word and saves the file. False if it was already there.</summary>
        public bool Add(string word)
        {
            word = word?.Trim();
            if (string.IsNullOrEmpty(word) || !_words.Add(word)) return false;
            Save();
            return true;
        }

        /// <summary>Adds words (a word and its other forms) and saves once. How many were new.</summary>
        public int AddRange(IEnumerable<string> words)
        {
            int added = 0;
            foreach (string w in words)
            {
                string word = w?.Trim();
                if (!string.IsNullOrEmpty(word) && _words.Add(word)) added++;
            }
            if (added > 0) Save();
            return added;
        }

        /// <summary>Removes a word and saves the file.</summary>
        public bool Remove(string word)
        {
            if (!_words.Remove(word)) return false;
            Save();
            return true;
        }

        /// <summary>Removes words and saves once. How many went.</summary>
        public int RemoveRange(IEnumerable<string> words)
        {
            int removed = words.Count(w => _words.Remove(w));
            if (removed > 0) Save();
            return removed;
        }

        /// <summary>Replaces a word with another (a correction) and saves. False if the old one isn't there or the new one is empty.</summary>
        public bool Change(string oldWord, string newWord)
        {
            newWord = newWord?.Trim();
            if (string.IsNullOrEmpty(newWord) || !_words.Contains(oldWord)) return false;
            _words.Remove(oldWord);
            _words.Add(newWord);
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
