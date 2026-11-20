using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClipX2.TextEngine.Hyphenation;

namespace ClipX2.UI
{
    /// <summary>
    /// The hyphenation dictionaries downloaded with the spelling dictionaries (name.hyph.dic, in
    /// the same folders), loaded when first needed and kept. Statistics uses them to count
    /// syllables for readability in languages other than English.
    /// </summary>
    internal static class HyphenationFiles
    {
        /// <summary>Saved beside the spelling dictionary: de_DE_frami.hyph.dic.</summary>
        public const string Extension = ".hyph.dic";

        private static readonly Dictionary<string, Hyphenator> Loaded = new Dictionary<string, Hyphenator>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> PathsFound = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The hyphenation file for a language: name.hyph.dic for that dictionary, else one for
        /// another dictionary of the same language (de_AT using de_DE's); null if none.
        /// </summary>
        public static string PathFor(string language)
        {
            if (string.IsNullOrEmpty(language)) return null;
            if (PathsFound.TryGetValue(language, out string known)) return known;
            string found = null;
            foreach (string folder in DictionaryCatalogue.Folders)
            {
                string own = Path.Combine(folder, language + Extension);
                if (File.Exists(own)) { found = own; break; }
            }
            if (found == null)
            {
                string code = ClipX2.TextEngine.TextStatistics.LanguageCode(language);
                found = DictionaryCatalogue.Folders.Where(Directory.Exists)
                    .SelectMany(f => Directory.GetFiles(f, "*" + Extension))
                    .FirstOrDefault(p => ClipX2.TextEngine.TextStatistics.LanguageCode(Path.GetFileName(p)) == code);
            }
            PathsFound[language] = found;
            return found;
        }

        /// <summary>The hyphenator for a language, or null if there is no hyphenation file (or it can't be read).</summary>
        public static Hyphenator For(string language)
        {
            string path = PathFor(language);
            if (path == null) return null;
            if (Loaded.TryGetValue(path, out Hyphenator h)) return h;
            try { h = Hyphenator.Load(path); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { h = null; }
            Loaded[path] = h;
            return h;
        }

        /// <summary>After a download or removal: look again next time.</summary>
        public static void Reset()
        {
            Loaded.Clear();
            PathsFound.Clear();
        }
    }
}
