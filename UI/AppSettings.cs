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

        public DisplaySettings Display { get; set; } = new DisplaySettings();

        /// <summary>
        /// Don't warn that formatting may be lost on switching to plain text (the warning's own
        /// "Do not show this warning again", and Options, General: "Hide warning about loss of formatting").
        /// </summary>
        public bool HideFormattingLossWarning { get; set; }

        /// <summary>
        /// Selecting with the mouse extends to whole words (Options, General). Off by default:
        /// the selection is exactly the characters dragged over.
        /// </summary>
        public bool WholeWordMouseSelection { get; set; }

        /// <summary>Quick search's recent searches, latest first (kept short).</summary>
        public System.Collections.Generic.List<string> QuickSearchHistory { get; set; } = new System.Collections.Generic.List<string>();

        /// <summary>Find and Replace: mode, Match case, Whole words, Smart replace, Special characters (ClipX 1's Search section).</summary>
        public ClipX2.TextEngine.SearchOptions Search { get; set; } = new ClipX2.TextEngine.SearchOptions();

        /// <summary>Find and Replace's recent find texts, latest first (apart from Quick search's).</summary>
        public System.Collections.Generic.List<string> FindHistory { get; set; } = new System.Collections.Generic.List<string>();

        /// <summary>Find and Replace's recent replacements, latest first.</summary>
        public System.Collections.Generic.List<string> ReplaceHistory { get; set; } = new System.Collections.Generic.List<string>();

        // List tools: the same section and key names as ClipX 1's settings (BulletList, NumList), so its
        // settings can be brought across directly; Style and FollowChar keep ClipX 1's numbering

        /// <summary>Bullets: Character, Tab, PadChars.</summary>
        public ClipX2.TextEngine.BulletOptions BulletList { get; set; } = new ClipX2.TextEngine.BulletOptions();

        /// <summary>Numbers: Style, StartNumber, Renumber, FollowChar, Tab, PadChars.</summary>
        public ClipX2.TextEngine.NumberOptions NumList { get; set; } = new ClipX2.TextEngine.NumberOptions();

        /// <summary>Line numbers: Start, Step, Every, IgnoreBlanks, Pad, RangeRight, Renumber.</summary>
        public ClipX2.TextEngine.LineNumberOptions NumLines { get; set; } = new ClipX2.TextEngine.LineNumberOptions();

        // Format tools: ClipX 1's names too (Indent, Wordwrap, RemLines)

        /// <summary>Indent and Unindent: Option ("Tab", "Spaces", "Text"), Spaces, Text.</summary>
        public ClipX2.TextEngine.IndentOptions Indent { get; set; } = new ClipX2.TextEngine.IndentOptions();

        /// <summary>Word wrap: Width, Hanging, Tab, Justify.</summary>
        public ClipX2.TextEngine.WordWrapOptions Wordwrap { get; set; } = new ClipX2.TextEngine.WordWrapOptions();

        /// <summary>Remove extra lines: Single, Multiple.</summary>
        public ClipX2.TextEngine.RemoveLinesOptions RemLines { get; set; } = new ClipX2.TextEngine.RemoveLinesOptions();

        /// <summary>The PowerButtons, in the order shown (Alt+1… run the first nine).</summary>
        public System.Collections.Generic.List<PowerButton> PowerButtons { get; set; } = new System.Collections.Generic.List<PowerButton>();

        /// <summary>Fonts the user has typed or chosen in the icon editor, offered after its own list.</summary>
        public System.Collections.Generic.List<string> IconEditorFonts { get; set; } = new System.Collections.Generic.List<string>();

        /// <summary>The PowerButton editor's tool list: the groups folded away (by heading); none at first, so all open.</summary>
        public System.Collections.Generic.List<string> PaletteCollapsedGroups { get; set; } = new System.Collections.Generic.List<string>();

        /// <summary>What the tool list's expand/collapse-all arrow does next: true, expand all; false (at first), collapse all.</summary>
        public bool PaletteToggleExpands { get; set; }

        /// <summary>How many columns of PowerButtons the panel on the right shows (widened by dragging its left edge).</summary>
        public int PowerButtonColumns { get; set; } = 1;

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
                s.Display ??= new DisplaySettings();
                s.BulletList ??= new ClipX2.TextEngine.BulletOptions();
                s.NumList ??= new ClipX2.TextEngine.NumberOptions();
                s.NumLines ??= new ClipX2.TextEngine.LineNumberOptions();
                s.QuickSearchHistory ??= new System.Collections.Generic.List<string>();
                s.Search ??= new ClipX2.TextEngine.SearchOptions();
                s.FindHistory ??= new System.Collections.Generic.List<string>();
                s.ReplaceHistory ??= new System.Collections.Generic.List<string>();
                s.Indent ??= new ClipX2.TextEngine.IndentOptions();
                s.Wordwrap ??= new ClipX2.TextEngine.WordWrapOptions();
                s.RemLines ??= new ClipX2.TextEngine.RemoveLinesOptions();
                s.PowerButtons ??= new System.Collections.Generic.List<PowerButton>();
                s.PowerButtons.RemoveAll(b => b == null);
                foreach (PowerButton b in s.PowerButtons)
                {
                    b.Name ??= "";
                    b.Icon ??= new ButtonIcon();
                    b.Tasks ??= new System.Collections.Generic.List<PowerTask>();
                    b.Tasks.RemoveAll(t => t == null);
                }
                s.PowerButtonColumns = Math.Max(1, Math.Min(6, s.PowerButtonColumns));
                s.IconEditorFonts ??= new System.Collections.Generic.List<string>();
                s.PaletteCollapsedGroups ??= new System.Collections.Generic.List<string>();
                s.Display.ProportionalFont ??= FontSpec.DefaultProportional;
                s.Display.FixedFont ??= FontSpec.DefaultFixed;
                s.Display.RichTextFont ??= FontSpec.DefaultRichText;
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

        /// <summary>
        /// Copies the settings (as saved now) to a file of the user's choosing: JSON, or with
        /// <paramref name="asPref"/> XML in ClipX 1's .pref format (<see cref="SettingsPref"/>).
        /// </summary>
        public static void Export(string path, bool asPref = false)
        {
            Current.Save();
            if (asPref) SettingsPref.Export(Current, path);
            else File.Copy(FilePath, path, overwrite: true);
        }

        /// <summary>The file holds XML (a .pref), whatever it is called; else it is taken as JSON.</summary>
        public static bool IsPrefFile(string path)
        {
            using var reader = new StreamReader(path, detectEncodingFromByteOrderMarks: true);
            int c;
            while ((c = reader.Read()) >= 0 && char.IsWhiteSpace((char)c)) { }
            return c == '<';
        }

        /// <summary>
        /// Reads settings from a file exported earlier and makes them the current ones (and
        /// saves them). Throws if the file isn't a settings file, leaving the current ones as they were.
        /// </summary>
        public static void Import(string path) => Import(path, out _);

        /// <summary>
        /// <see cref="Import(string)"/>, for JSON or a .pref (XML, ClipX 1's format: one exported
        /// by ClipX2 holds all the settings; one from ClipX 1 changes the settings it has, and
        /// <paramref name="fromClipX1"/> is true). A .pref that can't be read throws FormatException.
        /// </summary>
        public static void Import(string path, out bool fromClipX1)
        {
            fromClipX1 = false;
            if (IsPrefFile(path))
            {
                AppSettings s = SettingsPref.Import(path, Current, out fromClipX1);
                _current = s;
                s.Save();
                _current = Load(); // fills in anything the file lacks
                return;
            }
            _ = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), JsonOptions)
                ?? throw new JsonException("The file is empty.");
            Directory.CreateDirectory(Folder);
            File.Copy(path, FilePath, overwrite: true);
            _current = Load(); // fills in anything the file lacks
        }
    }

    /// <summary>A font, as kept in the settings (name, size in points, bold, italic).</summary>
    public sealed class FontSpec
    {
        public string Name { get; set; } = "Segoe UI";
        public float Size { get; set; } = 10.875F;
        public bool Bold { get; set; }
        public bool Italic { get; set; }

        public static FontSpec DefaultProportional => new FontSpec { Name = "Segoe UI", Size = 10.875F };
        public static FontSpec DefaultFixed => new FontSpec { Name = "Consolas", Size = 10.5F };
        public static FontSpec DefaultRichText => new FontSpec { Name = "Segoe UI", Size = 10.875F };

        public System.Drawing.Font ToFont()
        {
            var style = (Bold ? System.Drawing.FontStyle.Bold : 0) | (Italic ? System.Drawing.FontStyle.Italic : 0);
            try { return new System.Drawing.Font(Name, Size, style, System.Drawing.GraphicsUnit.Point); }
            catch (ArgumentException) { return new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, Size, style); }
        }

        public static FontSpec From(System.Drawing.Font font) => new FontSpec
        {
            Name = font.Name, Size = font.SizeInPoints, Bold = font.Bold, Italic = font.Italic,
        };

        /// <summary>"Segoe UI Bold 11 pt", as ClipX 1 described fonts.</summary>
        public override string ToString() =>
            Name + (Bold ? " Bold" : "") + (Italic ? " Italic" : "") + " " + Math.Round(Size, 1) + " pt";
    }

    /// <summary>How the text is shown (ClipX 1's Display page).</summary>
    public sealed class DisplaySettings
    {
        /// <summary>The font for plain text (rich text keeps its own fonts).</summary>
        public FontSpec ProportionalFont { get; set; } = FontSpec.DefaultProportional;
        /// <summary>The fixed-width font, for plain text laid out in columns.</summary>
        public FontSpec FixedFont { get; set; } = FontSpec.DefaultFixed;
        /// <summary>The font a new, empty rich text starts in (rich text brought in keeps its own fonts).</summary>
        public FontSpec RichTextFont { get; set; } = FontSpec.DefaultRichText;
        /// <summary>Show plain text in the fixed-width font (right-click on Options toggles it, as in ClipX 1).</summary>
        public bool UseFixedFont { get; set; }
        /// <summary>Wrap long lines in the window (display only: the text itself is unchanged).</summary>
        public bool WordWrap { get; set; } = true;

        public FontSpec CurrentFont => UseFixedFont ? FixedFont : ProportionalFont;
    }

    /// <summary>Spelling checker options (the side panel and the Options dialog).</summary>
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
        /// <summary>For Premium: the account email.</summary>
        public string Username { get; set; } = "";
        /// <summary>
        /// For Premium: the API key, encrypted so that only this Windows user can read it (see
        /// <see cref="SecretStore"/>). Use <see cref="ApiKey"/>.
        /// </summary>
        public string ApiKeyProtected { get; set; } = "";

        /// <summary>The API key in plain text (not saved as such).</summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string ApiKey
        {
            get => SecretStore.Unprotect(ApiKeyProtected);
            set => ApiKeyProtected = SecretStore.Protect(value);
        }
        /// <summary>Rules the user chose to ignore (LanguageTool rule ids).</summary>
        public System.Collections.Generic.List<string> DisabledRules { get; set; } = new System.Collections.Generic.List<string>();
        /// <summary>LanguageTool's stricter "picky" rules too.</summary>
        public bool Picky { get; set; }
        /// <summary>Use LanguageTool's spelling verdict as a second opinion: a word Hunspell flags is shown only if LanguageTool flags it too.</summary>
        public bool SecondOpinionOnSpelling { get; set; } = true;
    }
}
