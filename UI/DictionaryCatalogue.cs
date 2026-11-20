using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ClipX2.TextEngine.Spelling;

namespace ClipX2.UI
{
    /// <summary>Where a spelling dictionary is, or would come from.</summary>
    internal enum DictionarySource
    {
        /// <summary>Supplied with ClipX (the program's Dictionaries folder).</summary>
        Supplied,
        /// <summary>Downloaded or added by the user (their Dictionaries folder): wins over a supplied one of the same name.</summary>
        User,
        /// <summary>Not installed: can be downloaded.</summary>
        Available,
    }

    /// <summary>A Hunspell dictionary: installed, or available to download (or both, if a newer version is out).</summary>
    internal sealed class DictionaryEntry
    {
        /// <summary>The file name without extension, as used in the language list: en_GB, fr, de_DE_frami.</summary>
        public string Name { get; set; }
        /// <summary>"English (United Kingdom)".</summary>
        public string Description { get; set; }
        public DictionarySource Source { get; set; }

        /// <summary>For one that can be downloaded: the .dic, .aff and README (licence) in the collection.</summary>
        public string RemoteDic { get; set; } = "";
        public string RemoteAff { get; set; } = "";
        public string RemoteReadme { get; set; } = "";
        /// <summary>The collection's folder (en, fr_FR…), shown as where it comes from.</summary>
        public string RemoteFolder { get; set; } = "";
        /// <summary>The git hash of the collection's .dic, to tell whether an installed copy is the same.</summary>
        public string RemoteDicHash { get; set; } = "";
        /// <summary>The language's hyphenation dictionary in the collection (hyph_de_DE.dic…), and its README; "" if none.</summary>
        public string RemoteHyph { get; set; } = "";
        public string RemoteHyphReadme { get; set; } = "";
    }

    /// <summary>
    /// The spelling dictionaries: those supplied with ClipX, the user's own (downloaded or added
    /// by hand), and those that can be downloaded.
    ///
    /// Downloads come from LibreOffice's dictionaries (git.libreoffice.org/dictionaries), through
    /// its read-only mirror on GitHub: the same Hunspell dictionaries LibreOffice installs, each
    /// language with its own maintainers. The list is one request for the repository's file tree
    /// (GitHub allows 60 an hour without an account: plenty, as it is fetched once a run, on
    /// request). Each dictionary is its .aff and .dic, saved under LibreOffice's names (en_GB,
    /// fr, de_DE_frami…), and its folder's README, saved beside them as name.licence.txt.
    /// The language's hyphenation dictionary comes too, where LibreOffice has one (saved as
    /// name.hyph.dic, its README as name.hyph.licence.txt): Statistics counts syllables with it
    /// for readability in languages other than English. Thesaurus files are left out.
    ///
    /// The tree gives each file's git hash, so an installed dictionary can be compared with
    /// LibreOffice's current one without downloading it: if they differ, a newer one is offered.
    /// </summary>
    internal static class DictionaryCatalogue
    {
        /// <summary>Supplied with ClipX.</summary>
        public static string ProgramFolder { get; } = Path.Combine(AppContext.BaseDirectory, "Dictionaries");
        /// <summary>The user's own (downloaded or added by hand); these win over supplied ones of the same name.</summary>
        public static string UserFolder { get; } = Path.Combine(AppSettings.Folder, "Dictionaries");
        /// <summary>Where to look, the user's folder first.</summary>
        public static IReadOnlyList<string> Folders { get; } = new[] { UserFolder, ProgramFolder };

        private const string TreeUrl = "https://api.github.com/repos/LibreOffice/dictionaries/git/trees/master?recursive=1";
        private const string RawBase = "https://raw.githubusercontent.com/LibreOffice/dictionaries/master/";
        public const string SourceName = "LibreOffice's dictionaries (git.libreoffice.org/dictionaries, mirrored on GitHub)";
        public const string SourceUrl = "https://github.com/LibreOffice/dictionaries";

        private static readonly HttpClient Http = CreateClient();
        private static IReadOnlyList<DictionaryEntry> _available;

        private static HttpClient CreateClient()
        {
            var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            string version = typeof(DictionaryCatalogue).Assembly.GetName().Version?.ToString(2) ?? "2";
            http.DefaultRequestHeaders.UserAgent.ParseAdd($"ClipX2/{version} (desktop text utility; dictionary download)");
            return http;
        }

        /// <summary>Installed dictionary names (both folders), for the language list.</summary>
        public static IReadOnlyList<string> InstalledNames() =>
            Folders.SelectMany(SpellingDictionary.AvailableLocales)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(l => l, StringComparer.OrdinalIgnoreCase)
                .ToList();

        /// <summary>The folder holding a dictionary (the user's if both have it), or null.</summary>
        public static string FolderOf(string name) =>
            Folders.FirstOrDefault(f => File.Exists(Path.Combine(f, name + ".dic")));

        /// <summary>The installed dictionaries, with where each is.</summary>
        public static List<DictionaryEntry> Installed() =>
            InstalledNames().Select(name => new DictionaryEntry
            {
                Name = name,
                Description = DescribeLocale(name),
                Source = File.Exists(Path.Combine(UserFolder, name + ".dic")) ? DictionarySource.User : DictionarySource.Supplied,
            }).ToList();

        /// <summary>
        /// "English (United Kingdom)" for en_GB, from Windows' own names, with any extra part of
        /// the name after a dash (de_DE_frami → German (Germany) – frami); the name itself if unknown.
        /// </summary>
        public static string DescribeLocale(string name)
        {
            string[] parts = name.Split('_', '-');
            bool hasRegion = parts.Length > 1 && parts[1].Length == 2 && parts[1].All(char.IsLetter);
            string tag = hasRegion ? parts[0] + "-" + parts[1] : parts[0];
            int used = hasRegion ? 2 : 1;
            try
            {
                var culture = System.Globalization.CultureInfo.GetCultureInfo(tag);
                if (culture.EnglishName.StartsWith("Unknown", StringComparison.Ordinal)) return name;
                string extra = parts.Length > used ? " – " + string.Join(" ", parts.Skip(used)) : "";
                return culture.EnglishName + extra;
            }
            catch (System.Globalization.CultureNotFoundException)
            {
                return name;
            }
        }

        /// <summary>
        /// A short form for a narrow space: "English (GB)" for en_GB, "French" for fr,
        /// "German (DE) frami" for de_DE_frami; the name itself if unknown.
        /// </summary>
        public static string ShortLocale(string name)
        {
            string[] parts = name.Split('_', '-');
            bool hasRegion = parts.Length > 1 && parts[1].Length == 2 && parts[1].All(char.IsLetter);
            int used = hasRegion ? 2 : 1;
            try
            {
                var culture = System.Globalization.CultureInfo.GetCultureInfo(parts[0]);
                if (culture.EnglishName.StartsWith("Unknown", StringComparison.Ordinal)) return name;
                return culture.EnglishName
                    + (hasRegion ? " (" + parts[1].ToUpperInvariant() + ")" : "")
                    + (parts.Length > used ? " " + string.Join(" ", parts.Skip(used)) : "");
            }
            catch (System.Globalization.CultureNotFoundException)
            {
                return name;
            }
        }

        /// <summary>
        /// True if the installed copy of a dictionary differs from the collection's current one
        /// (a newer version, or simply another one). Compares git hashes: no download needed.
        /// </summary>
        public static bool DiffersFromRemote(string name, DictionaryEntry remote)
        {
            string folder = FolderOf(name);
            if (folder == null || remote == null || remote.RemoteDicHash.Length == 0) return false;
            try { return !string.Equals(GitBlobHash(Path.Combine(folder, name + ".dic")), remote.RemoteDicHash, StringComparison.OrdinalIgnoreCase); }
            catch (IOException) { return false; }
        }

        /// <summary>The git hash of a file: SHA-1 of "blob {length}\0" and its bytes.</summary>
        private static string GitBlobHash(string path)
        {
            byte[] content = File.ReadAllBytes(path);
            byte[] header = Encoding.ASCII.GetBytes($"blob {content.Length}\0");
            using var sha = SHA1.Create();
            sha.TransformBlock(header, 0, header.Length, null, 0);
            sha.TransformFinalBlock(content, 0, content.Length);
            return Convert.ToHexString(sha.Hash).ToLowerInvariant();
        }

        /// <summary>The dictionaries that can be downloaded (fetched once per run).</summary>
        public static async Task<IReadOnlyList<DictionaryEntry>> AvailableAsync(CancellationToken cancel = default)
        {
            if (_available != null) return _available;
            using var request = new HttpRequestMessage(HttpMethod.Get, TreeUrl);
            request.Headers.Accept.ParseAdd("application/vnd.github+json");
            using HttpResponseMessage response = await Http.SendAsync(request, cancel).ConfigureAwait(false);
            if ((int)response.StatusCode == 403 || (int)response.StatusCode == 429)
                throw new InvalidDataException("GitHub's limit on requests has been reached; try again in an hour.");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync(cancel).ConfigureAwait(false);

            // path → hash, for the files (blobs)
            var files = new Dictionary<string, string>(StringComparer.Ordinal);
            using (JsonDocument doc = JsonDocument.Parse(json))
                foreach (JsonElement item in doc.RootElement.GetProperty("tree").EnumerateArray())
                    if (item.GetProperty("type").GetString() == "blob")
                        files[item.GetProperty("path").GetString()] = item.GetProperty("sha").GetString();

            var list = new List<DictionaryEntry>();
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> f in files.OrderBy(f => f.Key, StringComparer.Ordinal))
            {
                string path = f.Key;
                if (!path.EndsWith(".dic", StringComparison.OrdinalIgnoreCase)) continue;
                string fileName = Path.GetFileName(path);
                if (fileName.StartsWith("hyph_", StringComparison.OrdinalIgnoreCase) ||
                    fileName.StartsWith("th_", StringComparison.OrdinalIgnoreCase)) continue; // hyphenation, thesaurus
                string name = Path.GetFileNameWithoutExtension(fileName);
                string aff = path.Substring(0, path.Length - 4) + ".aff";
                if (!files.ContainsKey(aff) || !names.Add(name)) continue;
                string top = path.Substring(0, path.IndexOf('/') < 0 ? 0 : path.IndexOf('/'));
                list.Add(new DictionaryEntry
                {
                    Name = name,
                    Description = DescribeLocale(name),
                    Source = DictionarySource.Available,
                    RemoteDic = path,
                    RemoteAff = aff,
                    RemoteReadme = FindReadme(files.Keys, path, top, name),
                    RemoteFolder = top,
                    RemoteDicHash = f.Value,
                    RemoteHyph = FindHyphenation(files.Keys, path, top, name),
                });
                DictionaryEntry added = list[list.Count - 1];
                if (added.RemoteHyph.Length > 0) added.RemoteHyphReadme = FindHyphenationReadme(files.Keys, added.RemoteHyph);
            }
            if (list.Count == 0) throw new InvalidDataException("The list of dictionaries could not be read.");
            _available = list.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase).ToList();
            return _available;
        }

        /// <summary>
        /// The README (with the licence and credits) for a dictionary: in the .dic's own folder,
        /// then its language folder; preferably one for the spelling dictionary (README_en_GB.txt,
        /// README_dict_fr.txt, README_hunspell_es.txt), never one for hyphenation, the thesaurus or
        /// the Lightproof grammar checker; else any README; else a licence file.
        /// </summary>
        private static string FindReadme(IEnumerable<string> paths, string dicPath, string top, string name)
        {
            string own = dicPath.Contains('/') ? dicPath.Substring(0, dicPath.LastIndexOf('/')) : "";
            var all = paths.ToList();
            foreach (string folder in new[] { own, top }.Where(f => f.Length > 0).Distinct())
            {
                var inFolder = all.Where(p => p.StartsWith(folder + "/", StringComparison.Ordinal) && p.IndexOf('/', folder.Length + 1) < 0).ToList();
                bool Has(string p, string part) => Path.GetFileName(p).IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0;
                bool IsReadme(string p) => Path.GetFileName(p).StartsWith("README", StringComparison.OrdinalIgnoreCase)
                    && !Has(p, "hyph") && !Has(p, "_th_") && !Has(p, "thes") && !Has(p, "lightproof");
                string found = inFolder.FirstOrDefault(p => IsReadme(p) && (Has(p, name) || Has(p, "dict") || Has(p, "hunspell")))
                    ?? inFolder.FirstOrDefault(IsReadme)
                    ?? inFolder.FirstOrDefault(p => Path.GetFileName(p).StartsWith("LICEN", StringComparison.OrdinalIgnoreCase));
                if (found != null) return found;
            }
            return "";
        }

        /// <summary>
        /// The hyphenation dictionary for a spelling dictionary: in its own folder, then its
        /// language folder; hyph_{name}.dic, else one for its language and region (de_DE_frami →
        /// hyph_de_DE.dic), else one for its language (fr → hyph_fr.dic, or any hyph_fr_*.dic).
        /// </summary>
        private static string FindHyphenation(IEnumerable<string> paths, string dicPath, string top, string name)
        {
            string own = dicPath.Contains('/') ? dicPath.Substring(0, dicPath.LastIndexOf('/')) : "";
            string[] parts = name.Split('_', '-');
            var wanted = new List<string> { "hyph_" + name + ".dic" };
            if (parts.Length > 1) wanted.Add("hyph_" + parts[0] + "_" + parts[1] + ".dic");
            wanted.Add("hyph_" + parts[0] + ".dic");
            var all = paths.ToList();
            foreach (string folder in new[] { own, top }.Where(f => f.Length > 0).Distinct())
            {
                var inFolder = all.Where(p => p.StartsWith(folder + "/", StringComparison.Ordinal) && p.IndexOf('/', folder.Length + 1) < 0).ToList();
                foreach (string file in wanted)
                {
                    string found = inFolder.FirstOrDefault(p => Path.GetFileName(p).Equals(file, StringComparison.OrdinalIgnoreCase));
                    if (found != null) return found;
                }
                string any = inFolder.FirstOrDefault(p => Path.GetFileName(p).StartsWith("hyph_" + parts[0] + "_", StringComparison.OrdinalIgnoreCase)
                                                        && p.EndsWith(".dic", StringComparison.OrdinalIgnoreCase));
                if (any != null) return any;
            }
            return "";
        }

        /// <summary>The README for a hyphenation dictionary (its licence and credits): README_hyph_….txt in its folder.</summary>
        private static string FindHyphenationReadme(IEnumerable<string> paths, string hyphPath)
        {
            string folder = hyphPath.Contains('/') ? hyphPath.Substring(0, hyphPath.LastIndexOf('/')) : "";
            string stem = Path.GetFileNameWithoutExtension(hyphPath); // hyph_de_DE
            var inFolder = paths.Where(p => p.StartsWith(folder + "/", StringComparison.Ordinal) && p.IndexOf('/', folder.Length + 1) < 0
                                         && Path.GetFileName(p).StartsWith("README", StringComparison.OrdinalIgnoreCase)
                                         && Path.GetFileName(p).IndexOf("hyph", StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            return inFolder.FirstOrDefault(p => Path.GetFileName(p).IndexOf(stem, StringComparison.OrdinalIgnoreCase) >= 0)
                ?? inFolder.FirstOrDefault() ?? "";
        }

        /// <summary>
        /// True if LibreOffice has a hyphenation dictionary for an installed dictionary's language
        /// that ClipX would use (readability in a language other than English) and it isn't installed.
        /// </summary>
        public static bool LacksHyphenation(string name, DictionaryEntry remote) =>
            remote != null && remote.RemoteHyph.Length > 0
            && ClipX2.TextEngine.TextStatistics.NeedsHyphenation(name)
            && HyphenationFiles.PathFor(name) == null;

        /// <summary>
        /// Downloads a dictionary into the user's folder (replacing any copy there). The files go
        /// in under temporary names and are renamed only when all have arrived, so a failed
        /// download leaves nothing half done.
        /// </summary>
        public static async Task DownloadAsync(DictionaryEntry entry, IProgress<string> progress, CancellationToken cancel = default)
        {
            Directory.CreateDirectory(UserFolder);
            var files = new List<(string Remote, string Local)>
            {
                (entry.RemoteAff, entry.Name + ".aff"),
                (entry.RemoteDic, entry.Name + ".dic"),
            };
            if (entry.RemoteReadme.Length > 0) files.Add((entry.RemoteReadme, entry.Name + ".licence.txt"));
            if (entry.RemoteHyph.Length > 0)
            {
                files.Add((entry.RemoteHyph, entry.Name + HyphenationFiles.Extension));
                if (entry.RemoteHyphReadme.Length > 0) files.Add((entry.RemoteHyphReadme, entry.Name + ".hyph.licence.txt"));
            }
            var done = new List<(string Temp, string Final)>();
            try
            {
                foreach (var (remote, local) in files)
                {
                    progress?.Report($"Downloading {entry.Description}: {local}…");
                    string url = RawBase + string.Join("/", remote.Split('/').Select(Uri.EscapeDataString));
                    byte[] bytes = await Http.GetByteArrayAsync(url, cancel).ConfigureAwait(false); // as is: the .aff says its own encoding
                    string temp = Path.Combine(UserFolder, local + ".download");
                    await File.WriteAllBytesAsync(temp, bytes, cancel).ConfigureAwait(false);
                    done.Add((temp, Path.Combine(UserFolder, local)));
                }
                foreach (var (temp, final) in done) File.Move(temp, final, overwrite: true);
                HyphenationFiles.Reset();
                progress?.Report($"{entry.Description} installed as {entry.Name}"
                    + (entry.RemoteHyph.Length > 0 ? ", with its hyphenation patterns." : "."));
            }
            finally
            {
                foreach (var (temp, _) in done)
                    try { if (File.Exists(temp)) File.Delete(temp); } catch (IOException) { /* left behind; harmless */ }
            }
        }

        /// <summary>Removes a dictionary from the user's folder (supplied ones can't be removed).</summary>
        public static void Remove(string name)
        {
            foreach (string ext in new[] { ".aff", ".dic", ".licence.txt", HyphenationFiles.Extension, ".hyph.licence.txt" })
            {
                string path = Path.Combine(UserFolder, name + ext);
                if (File.Exists(path)) File.Delete(path);
            }
            HyphenationFiles.Reset();
        }
    }
}
