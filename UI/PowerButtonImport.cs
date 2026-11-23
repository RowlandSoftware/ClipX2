using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using ClipX2.TextEngine;

namespace ClipX2.UI
{
    /// <summary>
    /// Brings ClipX 1's PowerButtons across: from the registry, where ClipX 1 kept them
    /// (HKCU\Software\VB and VBA Program Settings\ClipX\PowerButtons\Button n), or from a
    /// preferences file it exported (UTF-16 XML of the same keys, e.g. ClipX1.pref).
    ///
    /// Each button: Name, FontName, FontSize, Bold, Italic, Text (1–3 characters), X, Y (twips in
    /// ClipX 1's 32×30-pixel round button); subkeys 1, 2… one per action: Token (the tool's tag),
    /// DlgOption (0 Prompt, 1 Custom, 2 Current settings) and Details ("Key=Value, Key=Value", the
    /// Custom options). The icon becomes ClipX 1's green square with the characters on it, still
    /// adjustable in the icon editor.
    /// </summary>
    public static class PowerButtonImport
    {
        /// <summary>ClipX 1's button background (the colour of its round button picture).</summary>
        public static readonly int ClipX1Green = unchecked((int)0xFF016331);

        private const string RegistryPath = @"Software\VB and VBA Program Settings\ClipX\PowerButtons";

        /// <summary>A key: its values and subkeys (the registry and the .pref file have the same shape).</summary>
        private sealed class Key
        {
            public readonly Dictionary<string, string> Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            public readonly List<(string Name, Key Key)> Subkeys = new List<(string, Key)>();
            public string Value(string name, string fallback = "") => Values.TryGetValue(name, out string v) ? v ?? fallback : fallback;
        }

        /// <summary>ClipX 1 has PowerButtons in the registry (it was used on this computer).</summary>
        public static bool RegistryHasButtons()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RegistryPath);
                return key != null && key.GetSubKeyNames().Length > 0;
            }
            catch (Exception ex) when (ex is System.Security.SecurityException || ex is UnauthorizedAccessException || ex is System.IO.IOException) { return false; }
        }

        /// <summary>ClipX 1's PowerButtons from the registry, in button order (none if there are none).</summary>
        public static List<PowerButton> FromRegistry()
        {
            using var root = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RegistryPath);
            if (root == null) return new List<PowerButton>();
            return Convert(ReadRegistry(root));
        }

        private static Key ReadRegistry(Microsoft.Win32.RegistryKey reg)
        {
            var key = new Key();
            foreach (string name in reg.GetValueNames()) key.Values[name] = reg.GetValue(name)?.ToString() ?? "";
            foreach (string name in reg.GetSubKeyNames())
                using (var sub = reg.OpenSubKey(name))
                    if (sub != null) key.Subkeys.Add((name, ReadRegistry(sub)));
            return key;
        }

        /// <summary>ClipX 1's PowerButtons from an exported preferences file. Throws if it isn't one.</summary>
        public static List<PowerButton> FromPrefFile(string path)
        {
            var doc = new XmlDocument();
            doc.Load(path); // the declaration says UTF-16
            XmlElement node = doc.SelectSingleNode("/Preferences/Key[@name='PowerButtons']") as XmlElement;
            if (node == null)
            {
                if (doc.DocumentElement?.Name != "Preferences") throw new FormatException("This isn't a ClipX 1 preferences file.");
                return new List<PowerButton>();
            }
            return Convert(ReadXml(node));
        }

        private static Key ReadXml(XmlElement element)
        {
            var key = new Key();
            foreach (XmlNode child in element.ChildNodes)
            {
                if (!(child is XmlElement e)) continue;
                string name = e.GetAttribute("name");
                if (e.Name == "Value") key.Values[name] = e.InnerText;
                else if (e.Name == "Key") key.Subkeys.Add((name, ReadXml(e)));
            }
            return key;
        }

        /// <summary>The "Button n" keys, in n order, made into PowerButtons (buttons without actions are skipped).</summary>
        private static List<PowerButton> Convert(Key powerButtons)
        {
            var slots = new List<(int Slot, Key Key)>();
            foreach (var (name, key) in powerButtons.Subkeys)
            {
                Match m = Regex.Match(name, @"^Button (\d+)$", RegexOptions.IgnoreCase);
                if (m.Success) slots.Add((int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture), key));
            }
            slots.Sort((a, b) => a.Slot.CompareTo(b.Slot));

            // First the buttons, so an included PowerButton (by slot number) can be found by its new Id
            var bySlot = new Dictionary<int, PowerButton>();
            var made = new List<(PowerButton Button, Key Key)>();
            foreach (var (slot, key) in slots)
            {
                if (!ActionKeys(key).Any()) continue; // never defined
                var b = new PowerButton { Name = key.Value("Name").Trim(), Icon = IconFrom(key) };
                if (b.Name.Length == 0 || b.Name == "<Enter name here>") b.Name = "PowerButton " + slot;
                bySlot[slot] = b;
                made.Add((b, key));
            }
            foreach (var (b, key) in made)
                foreach (Key action in ActionKeys(key))
                    b.Tasks.Add(TaskFrom(action, bySlot));
            return made.Select(m => m.Button).ToList();
        }

        /// <summary>The action subkeys (1, 2, …) in number order.</summary>
        private static IEnumerable<Key> ActionKeys(Key button) =>
            button.Subkeys
                .Where(s => int.TryParse(s.Name, out _) && s.Key.Value("Token").Length > 0)
                .OrderBy(s => int.Parse(s.Name, CultureInfo.InvariantCulture))
                .Select(s => s.Key);

        /************ The icon ************/

        /// <summary>
        /// ClipX 1's icon: the characters at (X, Y) twips (15 a pixel) in a 32×30 picture, the
        /// font's size in points (at 96 dpi), in white: here on a 64-pixel square, so twice that.
        /// </summary>
        private static ButtonIcon IconFrom(Key key)
        {
            string text = key.Value("Text");
            string font = key.Value("FontName");
            float scale = ButtonIcon.Size / 32f;
            var icon = new ButtonIcon { Png = IconPainter.SolidPng(ClipX1Green) };
            if (text.Length == 0) return icon;
            if (font.Length == 0) font = "Arial";
            float points = (float)Number(key.Value("FontSize"), 16);
            if (points <= 0) points = 16;
            icon.Text = new IconText
            {
                Characters = IsSymbolFont(font) ? ToSymbolCharacters(text) : text,
                FontName = font,
                Size = points * 96f / 72f * scale,
                Bold = Bool(key.Value("Bold")),
                Italic = Bool(key.Value("Italic")),
                Colour = unchecked((int)0xFFFFFFFF),
                X = (float)Number(key.Value("X"), 0) / 15f * scale,
                // ClipX 1's picture was 30 pixels high, centred on a 32-pixel button
                Y = ((float)Number(key.Value("Y"), 0) / 15f + 1) * scale,
            };
            return icon;
        }

        private static readonly string[] SymbolFonts =
            { "Wingdings", "Wingdings 2", "Wingdings 3", "Webdings", "Symbol", "Marlett", "MT Extra", "Bookshelf Symbol 7", "MS Outlook", "ZapfDingbats" };

        /// <summary>A font whose characters are symbols at codes 0x20–0xFF (not Unicode letters).</summary>
        public static bool IsSymbolFont(string font) => SymbolFonts.Any(f => string.Equals(f, font, StringComparison.OrdinalIgnoreCase));

        // Windows-1252's characters for 0x80–0x9F (0 where it has none: those came through unchanged)
        private static readonly char[] Cp1252High =
        {
            '€', '\0', '‚', 'ƒ', '„', '…', '†', '‡', 'ˆ', '‰', 'Š', '‹', 'Œ', '\0', 'Ž', '\0',
            '\0', '‘', '’', '“', '”', '•', '–', '—', '˜', '™', 'š', '›', 'œ', '\0', 'ž', 'Ÿ',
        };

        /// <summary>
        /// ClipX 1 (VB6, ANSI) stored a symbol font's character codes as Windows-1252 text: "š" is
        /// code 0x9A. A symbol font's glyph for code c is at U+F000 + c.
        /// </summary>
        public static string ToSymbolCharacters(string text)
        {
            var chars = new char[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                int code = c;
                if (c > 0xFF)
                {
                    int k = Array.IndexOf(Cp1252High, c);
                    code = k >= 0 ? 0x80 + k : -1;
                }
                chars[i] = code >= 0x20 && code <= 0xFF ? (char)(0xF000 + code) : c;
            }
            return new string(chars);
        }

        /************ The actions ************/

        private static PowerTask TaskFrom(Key action, Dictionary<int, PowerButton> bySlot)
        {
            string tag = action.Value("Token").Trim();
            var p = Details(action.Value("Details"));
            int dlg = (int)Number(action.Value("DlgOption"), 1);
            var task = new PowerTask(tag, dlg == 0 ? TaskOptions.Ask : dlg == 2 ? TaskOptions.LastUsed : TaskOptions.Own);
            if (!PowerTaskTypes.HasOptions(tag)) { task.Use = TaskOptions.Own; return task; }
            if (task.Use == TaskOptions.Own) task.Options = OptionsFrom(tag, p, bySlot);
            if (task.Options == null && task.Use == TaskOptions.Own) task.Use = TaskOptions.LastUsed; // nothing kept: as a right-click
            if (PowerTaskTypes.MustBeOwn(tag)) task.Use = TaskOptions.Own;
            return task;
        }

        /// <summary>
        /// "Key=Value, Key=Value": split only where ", " is followed by a name and "=", since values
        /// may hold commas themselves (GroupingSep=, or Replace=$1;,).
        /// </summary>
        internal static Dictionary<string, string> Details(string details)
        {
            var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(details)) return d;
            foreach (string part in Regex.Split(details, @", (?=[A-Za-z]+=)"))
            {
                int eq = part.IndexOf('=');
                if (eq <= 0) continue;
                d[part.Substring(0, eq).Trim()] = part.Substring(eq + 1);
            }
            return d;
        }

        private static object OptionsFrom(string tag, Dictionary<string, string> p, Dictionary<int, PowerButton> bySlot)
        {
            if (p.Count == 0) return null;
            string S(string k, string fallback = "") => p.TryGetValue(k, out string v) ? v : fallback;
            int I(string k, int fallback) => (int)Number(S(k), fallback);
            bool B(string k, bool fallback = false) => p.ContainsKey(k) ? Bool(S(k)) : fallback;

            switch (tag)
            {
                case "edSRC":
                case "edREP":
                    int anchor = I("Anchor", 4);
                    string grouping = S("GroupingSep", S("DigitGrpSep", ","));
                    string direction = S("Direction", "D").Trim().ToUpperInvariant();
                    return new FindReplaceTask
                    {
                        Find = S("Find"),
                        Replace = S("Replace"),
                        Search = new SearchOptions
                        {
                            Mode = I("Mode", 1) == 3 ? SearchMode.Regex : I("Mode", 1) == 2 ? SearchMode.Wildcard : SearchMode.Simple,
                            MatchCase = B("MatchCase"),
                            WholeWords = B("WholeWords"),
                            SmartReplace = B("SmartReplace"),
                            SpecialCharacters = B("SpecialChars"),
                            // ClipX 1: 0 start of word, 1 end of word, 2 start of line, 3 end of line, 4 anywhere
                            Position = anchor switch
                            {
                                0 => WildcardPosition.StartOfWord,
                                1 => WildcardPosition.EndOfWord,
                                2 => WildcardPosition.StartOfLine,
                                3 => WildcardPosition.EndOfLine,
                                _ => WildcardPosition.Anywhere,
                            },
                            DecimalSeparator = S("DecimalSep", "."),
                            ThousandsSeparator = grouping,
                        },
                        Action = tag == "edSRC" ? FindAction.FindNext : B("ReplaceAll", true) ? FindAction.ReplaceAll : FindAction.ReplaceNext,
                        FromStart = direction == "S",
                        Backwards = direction == "U",
                    };
                case "foIND":
                case "foUNI":
                    int radio = I("RadioButton", 0); // ClipX 1's option buttons: 0 Tab, 1 Spaces, 2 This text
                    return new IndentOptions
                    {
                        Option = radio == 1 ? "Spaces" : radio == 2 ? "Text" : "Tab",
                        Spaces = I("Spaces", 4),
                        Text = S("Text"),
                        Unindent = tag == "foUNI",
                    };
                case "foWRP":
                    return new WordWrapOptions { Width = I("Width", 50), Hanging = B("Hanging"), Tab = B("Tab", true), Justify = B("Justify") };
                case "foREM":
                    return new RemoveLinesOptions { Single = B("Single"), Multiple = B("Multiple", true) };
                case "liBUL":
                    return new BulletOptions { Character = S("Bullet", S("Character", "•")), Tab = B("Tab", true), PadChars = I("Spaces", I("PadChars", 5)) };
                case "liNUM":
                    return new NumberOptions
                    {
                        StartNumber = I("Start", 1),
                        Style = (NumberStyle)I("Style", 0),
                        FollowChar = (NumberPunctuation)I("FollowChar", 1),
                        Tab = B("Tab", true),
                        PadChars = I("Spaces", 4),
                        Renumber = B("Renumber", true),
                    };
                case "liREN":
                    return new LineNumberOptions
                    {
                        Start = I("Start", 1),
                        Step = I("Step", 1),
                        Every = I("Every", 1),
                        IgnoreBlanks = S("IgnoreBlanks") == "0", // ClipX 1 ran it with IgnoreBlanks = "0" (its check box's meaning was the reverse)
                        Renumber = B("Renumber", true),
                        Pad = B("Pad", true),
                        RangeRight = B("RangeRight", true),
                    };
                case "pbINS":
                    return S("Text");
                case "pbPBT":
                    return bySlot.TryGetValue(I("PowerButton", 0), out PowerButton inner) ? inner.Id : (object)null;
                default:
                    return null;
            }
        }

        /// <summary>A VB number as saved: always with a full stop (ClipX 1 used Val, locale-independent).</summary>
        private static double Number(string s, double fallback) =>
            double.TryParse((s ?? "").Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double d) ? d : fallback;

        /// <summary>VB's True / False, or 1 / 0 (-1 for True too).</summary>
        private static bool Bool(string s)
        {
            s = (s ?? "").Trim();
            return s.Equals("True", StringComparison.OrdinalIgnoreCase) || s == "1" || s == "-1";
        }
    }
}
