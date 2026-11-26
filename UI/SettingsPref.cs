using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;

namespace ClipX2.UI
{
    /// <summary>
    /// Settings as XML in ClipX 1's .pref format (modImportExport: its registry key written out):
    /// &lt;Preferences&gt; holding &lt;Key name="…"&gt; sections and &lt;Value name="…"&gt;
    /// entries, strings in CDATA, numbers and True/False bare; UTF-16 with tabs.
    ///
    /// Export writes all of ClipX2's settings that way (a section per settings class, the same
    /// names as ClipX 1's where they share them: BulletList, NumList, NumLines, Indent, Wordwrap,
    /// RemLines), lists as keys of entries named 1, 2, 3…, plus ClipX 1's Preferences section
    /// (the fonts), so ClipX 1 can take those. Import reads a file exported here back exactly,
    /// or a ClipX 1 file: its tool settings, find options, fonts and PowerButtons.
    /// </summary>
    internal static class SettingsPref
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        /************ Export ************/

        public static void Export(AppSettings settings, string path) => Write(settings, path, powerButtonsOnly: false);

        /// <summary>
        /// Just the PowerButtons (and the settings' Version, which marks a ClipX 2 file): Import
        /// then replaces only the PowerButtons, and the PowerButton panel's import can add them.
        /// </summary>
        public static void ExportPowerButtons(AppSettings settings, string path) => Write(settings, path, powerButtonsOnly: true);

        private static readonly string[] PowerButtonsOnlyEntries = { nameof(AppSettings.Version), nameof(AppSettings.PowerButtons) };

        private static void Write(AppSettings settings, string path, bool powerButtonsOnly)
        {
            using JsonDocument doc = JsonDocument.Parse(JsonSerializer.Serialize(settings));
            var xmlSettings = new XmlWriterSettings
            {
                Encoding = Encoding.Unicode, // UTF-16, as ClipX 1
                Indent = true,
                IndentChars = "\t",
                NewLineChars = "\r\n",
            };
            using (XmlWriter w = XmlWriter.Create(path, xmlSettings))
            {
                w.WriteStartDocument();
                w.WriteStartElement("Preferences");
                foreach (JsonProperty p in doc.RootElement.EnumerateObject())
                    if (!powerButtonsOnly || PowerButtonsOnlyEntries.Contains(p.Name))
                        WriteEntry(w, p.Name, p.Value);
                if (!powerButtonsOnly) WriteClipX1Preferences(w, settings.Display);
                w.WriteEndElement();
                w.WriteEndDocument();
            }
        }

        private static void WriteEntry(XmlWriter w, string name, JsonElement e)
        {
            switch (e.ValueKind)
            {
                case JsonValueKind.Object:
                    w.WriteStartElement("Key");
                    w.WriteAttributeString("name", name);
                    foreach (JsonProperty p in e.EnumerateObject()) WriteEntry(w, p.Name, p.Value);
                    w.WriteEndElement();
                    break;
                case JsonValueKind.Array:
                    if (e.GetArrayLength() == 0) return; // an empty list is the default anyway
                    w.WriteStartElement("Key");
                    w.WriteAttributeString("name", name);
                    int i = 1;
                    foreach (JsonElement item in e.EnumerateArray()) WriteEntry(w, (i++).ToString(CultureInfo.InvariantCulture), item);
                    w.WriteEndElement();
                    break;
                case JsonValueKind.String:
                    WriteString(w, name, e.GetString());
                    break;
                case JsonValueKind.Number:
                    WriteBare(w, name, e.GetRawText());
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    WriteBare(w, name, e.GetBoolean() ? "True" : "False");
                    break;
                default: // null: an empty entry
                    w.WriteStartElement("Value");
                    w.WriteAttributeString("name", name);
                    w.WriteEndElement();
                    break;
            }
        }

        private static void WriteString(XmlWriter w, string name, string text)
        {
            w.WriteStartElement("Value");
            w.WriteAttributeString("name", name);
            // CDATA can't hold "]]>": split it across two sections
            string[] parts = (text ?? "").Split(new[] { "]]>" }, StringSplitOptions.None);
            for (int i = 0; i < parts.Length; i++)
                w.WriteCData((i > 0 ? ">" : "") + parts[i] + (i < parts.Length - 1 ? "]]" : ""));
            w.WriteEndElement();
        }

        private static void WriteBare(XmlWriter w, string name, string text)
        {
            w.WriteStartElement("Value");
            w.WriteAttributeString("name", name);
            w.WriteString(text);
            w.WriteEndElement();
        }

        /// <summary>ClipX 1's own Preferences section: the two plain-text fonts and which is used.</summary>
        private static void WriteClipX1Preferences(XmlWriter w, DisplaySettings d)
        {
            w.WriteStartElement("Key");
            w.WriteAttributeString("name", "Preferences");
            foreach (var (prefix, f) in new[] { ("Prop", d.ProportionalFont), ("Fixed", d.FixedFont) })
            {
                WriteString(w, prefix + "_FontName", f.Name);
                WriteBare(w, prefix + "_FontBold", f.Bold ? "True" : "False");
                WriteBare(w, prefix + "_FontItalic", f.Italic ? "True" : "False");
                WriteBare(w, prefix + "_FontSize", f.Size.ToString(CultureInfo.InvariantCulture));
            }
            WriteBare(w, "FontUseProp", d.UseFixedFont ? "0" : "1");
            WriteBare(w, "Wordwrap", d.WordWrap ? "1" : "0");
            w.WriteEndElement();
        }

        /************ Import ************/

        /// <summary>
        /// The settings in a .pref file: one exported by ClipX2 (all its settings), or by ClipX 1
        /// (its settings put onto a copy of <paramref name="current"/>). Throws
        /// <see cref="FormatException"/> if the file isn't a ClipX preference file.
        /// </summary>
        public static AppSettings Import(string path, AppSettings current, out bool fromClipX1)
        {
            XElement root = Root(path);
            fromClipX1 = IsClipX1(root);
            if (!fromClipX1)
            {
                AppSettings read = FromClipX2(root);
                if (!IsPowerButtonsOnly(root)) return read;
                AppSettings s2 = Copy(current); // a PowerButtons file: only the PowerButtons change
                s2.PowerButtons = read.PowerButtons;
                return s2;
            }

            AppSettings s = Copy(current);
            FromClipX1(root, s, path);
            return s;
        }

        /// <summary>True if the file is ClipX 2's PowerButtons export (<see cref="ExportPowerButtons"/>); false for anything else, readable or not.</summary>
        public static bool IsPowerButtonsFile(string path)
        {
            try { XElement root = Root(path); return !IsClipX1(root) && IsPowerButtonsOnly(root); }
            catch (Exception ex) when (ex is FormatException || ex is IOException || ex is UnauthorizedAccessException) { return false; }
        }

        /// <summary>
        /// The PowerButtons in a .pref file, ClipX 2's (a settings or PowerButtons export) or
        /// ClipX 1's: for the PowerButton panel's import, which adds them to those there.
        /// </summary>
        public static List<PowerButton> ReadPowerButtons(string path)
        {
            XElement root = Root(path);
            return IsClipX1(root) ? PowerButtonImport.FromPrefFile(path) : FromClipX2(root).PowerButtons ?? new List<PowerButton>();
        }

        private static XElement Root(string path)
        {
            XDocument doc;
            try { doc = XDocument.Load(path); }
            catch (XmlException ex) { throw new FormatException("It isn't XML: " + ex.Message, ex); }
            XElement root = doc.Root;
            if (root == null || root.Name.LocalName != "Preferences")
                throw new FormatException("It isn't a ClipX preference file (no <Preferences>).");
            return root;
        }

        /// <summary>ClipX 2 writes the settings' Version at the top; ClipX 1 has none.</summary>
        private static bool IsClipX1(XElement root) => !root.Elements("Value").Any(v => (string)v.Attribute("name") == nameof(AppSettings.Version));

        /// <summary>Nothing at the top but Version and PowerButtons.</summary>
        private static bool IsPowerButtonsOnly(XElement root) =>
            root.Elements().Any(e => (string)e.Attribute("name") == nameof(AppSettings.PowerButtons))
            && root.Elements().All(e => PowerButtonsOnlyEntries.Contains((string)e.Attribute("name")));

        private static AppSettings FromClipX2(XElement root)
        {
            JsonNode tree = ReadKey(root);
            try
            {
                return JsonSerializer.Deserialize<AppSettings>(tree.ToJsonString(), Json)
                    ?? throw new FormatException("The file is empty.");
            }
            catch (JsonException ex) { throw new FormatException("Its settings couldn't be read: " + ex.Message, ex); }
        }

        private static AppSettings Copy(AppSettings s) => JsonSerializer.Deserialize<AppSettings>(JsonSerializer.Serialize(s), Json);

        /// <summary>A key as JSON: an array if its entries are named 1, 2, 3…, else an object.</summary>
        private static JsonNode ReadKey(XElement key)
        {
            List<XElement> entries = key.Elements().Where(e => e.Name.LocalName == "Key" || e.Name.LocalName == "Value").ToList();
            bool list = entries.Count > 0 && entries.Select((e, i) => (string)e.Attribute("name") == (i + 1).ToString(CultureInfo.InvariantCulture)).All(b => b);
            if (list)
            {
                var array = new JsonArray();
                foreach (XElement e in entries) array.Add(ReadEntry(e));
                return array;
            }
            var obj = new JsonObject();
            foreach (XElement e in entries)
            {
                string name = (string)e.Attribute("name");
                if (string.IsNullOrEmpty(name) || obj.ContainsKey(name)) continue;
                obj[name] = ReadEntry(e);
            }
            return obj;
        }

        private static JsonNode ReadEntry(XElement e) => e.Name.LocalName == "Key" ? ReadKey(e) : ReadValue(e);

        /// <summary>CDATA: a string; empty: null; else True/False, a number, or (anything else) a string.</summary>
        private static JsonNode ReadValue(XElement v)
        {
            if (v.Nodes().OfType<XCData>().Any()) return JsonValue.Create(v.Value);
            if (!v.Nodes().Any()) return null;
            string text = v.Value.Trim();
            if (text.Equals("True", StringComparison.OrdinalIgnoreCase)) return JsonValue.Create(true);
            if (text.Equals("False", StringComparison.OrdinalIgnoreCase)) return JsonValue.Create(false);
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                try { return JsonNode.Parse(text); } // as written: 3 stays an integer
                catch (JsonException) { }
            }
            return JsonValue.Create(v.Value);
        }

        /************ ClipX 1's settings ************/

        private static void FromClipX1(XElement root, AppSettings s, string path)
        {
            XElement Section(string name) => root.Elements("Key").FirstOrDefault(k => (string)k.Attribute("name") == name);
            Dictionary<string, string> Values(XElement key) => key == null ? new Dictionary<string, string>()
                : key.Elements("Value").Where(v => v.Attribute("name") != null)
                    .GroupBy(v => (string)v.Attribute("name")).ToDictionary(g => g.Key, g => g.First().Value);

            // The tools' last-used options: the same section and value names as ClipX2's
            Fill(s.BulletList, Values(Section("BulletList")));
            Fill(s.NumList, Values(Section("NumList")));
            Fill(s.NumLines, Values(Section("NumLines")));
            Fill(s.Indent, Values(Section("Indent")));
            Fill(s.Wordwrap, Values(Section("Wordwrap")));
            Fill(s.RemLines, Values(Section("RemLines")));

            // Find and Replace: some names differ
            Dictionary<string, string> search = Values(Section("Search"));
            var renamed = new Dictionary<string, string>();
            foreach (var (from, to) in new[] { ("WholeWords", "WholeWords"), ("MatchCase", "MatchCase"), ("SmartReplace", "SmartReplace"),
                ("AllowSpecial", "SpecialCharacters"), ("GroupingSep", "ThousandsSeparator"), ("DecimalSep", "DecimalSeparator") })
                if (search.TryGetValue(from, out string value)) renamed[to] = value;
            Fill(s.Search, renamed);
            if (search.TryGetValue("Anchor", out string anchor) && int.TryParse(anchor, out int a))
                s.Search.Position = a switch
                {
                    0 => ClipX2.TextEngine.WildcardPosition.StartOfWord,
                    1 => ClipX2.TextEngine.WildcardPosition.EndOfWord,
                    2 => ClipX2.TextEngine.WildcardPosition.StartOfLine,
                    3 => ClipX2.TextEngine.WildcardPosition.EndOfLine,
                    _ => ClipX2.TextEngine.WildcardPosition.Anywhere,
                };

            // Fonts (Preferences)
            Dictionary<string, string> prefs = Values(Section("Preferences"));
            FontSpec Font(string prefix, FontSpec was)
            {
                if (!prefs.TryGetValue(prefix + "_FontName", out string name) || name.Trim().Length == 0) return was;
                return new FontSpec
                {
                    Name = name.Trim(),
                    Size = prefs.TryGetValue(prefix + "_FontSize", out string size) && TryNumber(size, out double pt) && pt > 0 ? (float)pt : was.Size,
                    Bold = prefs.TryGetValue(prefix + "_FontBold", out string bold) && TryBool(bold, out bool b) && b,
                    Italic = prefs.TryGetValue(prefix + "_FontItalic", out string italic) && TryBool(italic, out bool it) && it,
                };
            }
            s.Display.ProportionalFont = Font("Prop", s.Display.ProportionalFont);
            s.Display.FixedFont = Font("Fixed", s.Display.FixedFont);
            if (prefs.TryGetValue("FontUseProp", out string useProp) && TryBool(useProp, out bool prop)) s.Display.UseFixedFont = !prop;
            if (prefs.TryGetValue("Wordwrap", out string wrap) && TryBool(wrap, out bool wraps)) s.Display.WordWrap = wraps;

            // PowerButtons (as "Import ClipX 1's PowerButtons" reads them)
            XElement buttons = Section("PowerButtons");
            if (buttons != null && buttons.Elements("Key").Any())
            {
                List<PowerButton> found = PowerButtonImport.FromPrefFile(path);
                if (found.Count > 0) s.PowerButtons = found;
            }
        }

        /// <summary>Sets the properties of <paramref name="target"/> named in <paramref name="values"/> (ClipX 1's registry strings).</summary>
        private static void Fill(object target, Dictionary<string, string> values)
        {
            foreach (KeyValuePair<string, string> kv in values)
            {
                PropertyInfo p = target.GetType().GetProperty(kv.Key, BindingFlags.Public | BindingFlags.Instance);
                if (p == null || !p.CanWrite) continue;
                string text = kv.Value ?? "";
                Type t = p.PropertyType;
                object value = null;
                if (t == typeof(string)) value = text;
                else if (t == typeof(bool)) { if (TryBool(text, out bool b)) value = b; }
                else if (t == typeof(int)) { if (TryNumber(text, out double n)) value = (int)Math.Round(n); }
                else if (t == typeof(float)) { if (TryNumber(text, out double n)) value = (float)n; }
                else if (t == typeof(double)) { if (TryNumber(text, out double n)) value = n; }
                else if (t.IsEnum)
                {
                    if (int.TryParse(text.Trim(), out int i) && Enum.IsDefined(t, i)) value = Enum.ToObject(t, i);
                    else if (Enum.TryParse(t, text.Trim(), true, out object named)) value = named;
                }
                if (value != null) p.SetValue(target, value);
            }
        }

        /// <summary>VB's ways of writing a Boolean: True/False, -1/1/0.</summary>
        private static bool TryBool(string text, out bool value)
        {
            text = (text ?? "").Trim();
            if (bool.TryParse(text, out value)) return true;
            if (int.TryParse(text, out int i)) { value = i != 0; return true; }
            return false;
        }

        /// <summary>A number, as written in any culture ClipX 1 ran in (11.25 or 11,25).</summary>
        private static bool TryNumber(string text, out double value) =>
            double.TryParse((text ?? "").Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            || double.TryParse((text ?? "").Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out value);
    }
}
