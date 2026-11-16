using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClipX2.UI
{
    /// <summary>
    /// The user's settings (ClipX 1 kept them in the registry). One JSON file,
    /// settings.json, in the per-user application-data folder: %AppData%\ClipX2 on Windows,
    /// ~/.config/ClipX2 on Linux and macOS. Nothing here depends on Windows Forms, so it can move
    /// with the engine if ClipX goes cross-platform.
    ///
    /// Each tool has its own section, holding its last-used options (so a right-click on a tool
    /// can run it with them, skipping its options panel). New settings need only a property with
    /// a default: a file written by an older version just lacks it, and the default is used;
    /// properties the file has but this version doesn't know are ignored.
    /// </summary>
    public sealed class AppSettings
    {
        /// <summary>Format version, for converting old files if ever needed.</summary>
        public int Version { get; set; } = 1;

        public SpellingSettings Spelling { get; set; } = new SpellingSettings();

        public GrammarSettings Grammar { get; set; } = new GrammarSettings();

        /************ Loading and saving ************/

        /// <summary>The per-user folder for settings, the personal dictionary and downloaded dictionaries.</summary>
        public static string Folder { get; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClipX2");

        public static string FilePath { get; } = Path.Combine(Folder, "settings.json");

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,                                   // readable and editable by hand
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        };

        private static AppSettings _current;

        /// <summary>The settings for this run, loaded on first use.</summary>
        public static AppSettings Current => _current ??= Load();

        /// <summary>
        /// Reads the settings file. If it is missing, the defaults; if it can't be read, the
        /// defaults too, and the bad file is kept as settings.json.bad rather than overwritten.
        /// </summary>
        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new AppSettings();
                AppSettings s = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath), JsonOptions);
                if (s == null) return new AppSettings();
                s.Spelling ??= new SpellingSettings();
                s.Grammar ??= new GrammarSettings();
                s.Grammar.DisabledRules ??= new System.Collections.Generic.List<string>();
                return s;
            }
            catch (Exception ex) when (ex is JsonException || ex is IOException || ex is UnauthorizedAccessException)
            {
                try { File.Copy(FilePath, FilePath + ".bad", true); } catch (Exception) { /* keep going with defaults */ }
                return new AppSettings();
            }
        }

        /// <summary>
        /// Writes the settings (to a temporary file first, then swaps it in, so a crash mid-write
        /// can't leave a half-written file). Failure is not fatal: the settings just aren't kept.
        /// </summary>
        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Folder);
                string temp = FilePath + ".tmp";
                File.WriteAllText(temp, JsonSerializer.Serialize(this, JsonOptions));
                File.Move(temp, FilePath, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                System.Diagnostics.Debug.WriteLine("Settings not saved: " + ex.Message);
            }
        }
    }

    /// <summary>Spelling checker options (the side panel now; the Options dialog later).</summary>
    public sealed class SpellingSettings
    {
        /// <summary>Hunspell dictionary name, e.g. en_GB.</summary>
        public string Language { get; set; } = "en_GB";
        public bool SmartReplace { get; set; } = true;
        public bool IgnoreUppercase { get; set; } = true;
        public bool IgnoreWordsWithNumbers { get; set; } = true;
        public bool IgnoreAddresses { get; set; } = true;
        public bool CheckRepeatedWords { get; set; } = true;
        /// <summary>Show Wiktionary definitions when a suggestion or "Change to" is clicked (needs the internet).</summary>
        public bool LookUpDefinitions { get; set; } = true;
        /// <summary>For English, plain definitions from the Simple English Wiktionary first (for children and learners).</summary>
        public bool SimpleDefinitionsFirst { get; set; } = true;
        /// <summary>
        /// Oxford spelling, -ize (realize, organize). Off (the default): -ise, and LanguageTool's
        /// "Would you like to use the Oxford spelling…?" is not shown. Hunspell's side is the
        /// dictionary chosen: the en_GB installed is -ise only; an -ize one (e.g. en_GB-ize) can
        /// be added and picked in the language list.
        /// </summary>
        public bool OxfordIze { get; set; }
    }

    /// <summary>Grammar checking by LanguageTool (see ClipX2.TextEngine.Grammar.LanguageToolChecker).</summary>
    public sealed class GrammarSettings
    {
        /// <summary>Check grammar along with spelling (on by default; the user is asked once before any text is sent).</summary>
        public bool Enabled { get; set; } = true;
        /// <summary>The user has been told that the text goes to LanguageTool's server, and agreed.</summary>
        public bool ConsentGiven { get; set; }
        /// <summary>Free, Premium or OwnServer.</summary>
        public string Service { get; set; } = "Free";
        /// <summary>For OwnServer.</summary>
        public string ServerUrl { get; set; } = "http://localhost:8081";
        /// <summary>For Premium: the account email (the API key is stored separately, encrypted: still to do).</summary>
        public string Username { get; set; } = "";
        /// <summary>Rules the user chose to ignore (LanguageTool rule ids).</summary>
        public System.Collections.Generic.List<string> DisabledRules { get; set; } = new System.Collections.Generic.List<string>();
        /// <summary>LanguageTool's stricter "picky" rules too.</summary>
        public bool Picky { get; set; }
        /// <summary>Use LanguageTool's spelling verdict as a second opinion: a word Hunspell flags is shown only if LanguageTool flags it too.</summary>
        public bool SecondOpinionOnSpelling { get; set; } = true;
    }
}
