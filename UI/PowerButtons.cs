using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClipX2.TextEngine;
using ClipX2.TextEngine.Tools;

namespace ClipX2.UI
{
    /// <summary>
    /// A PowerButton (ClipX 1's): a named button, with its own icon, that runs several tools one
    /// after another, each with its own options, the last-used ones, or asking. Kept in the
    /// settings (<see cref="AppSettings.PowerButtons"/>), in the order shown; Alt+1…Alt+9 run the
    /// first nine.
    /// </summary>
    public sealed class PowerButton
    {
        /// <summary>Permanent identity: a PowerButton included in another (pbPBT) is found by it, so re-ordering doesn't break it.</summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public ButtonIcon Icon { get; set; } = new ButtonIcon();
        public List<PowerTask> Tasks { get; set; } = new List<PowerTask>();

        /// <summary>A copy to edit (the editor's Cancel leaves the original as it was); the same Id.</summary>
        public PowerButton Clone() => PowerButtonStore.Copy(this);
    }

    /// <summary>Where a task's options come from (ClipX 1's DlgOption).</summary>
    public enum TaskOptions
    {
        /// <summary>The task's own options, kept with it (ClipX 1's Custom settings).</summary>
        Own,
        /// <summary>The tool's last-used options, as a right-click on its button (ClipX 1's Use current settings).</summary>
        LastUsed,
        /// <summary>The run stops at this task and shows its options in the side panel; its Apply carries on (ClipX 1's Prompt for settings).</summary>
        Ask,
    }

    /// <summary>One step of a PowerButton: a tool (by tag) and its options.</summary>
    [JsonConverter(typeof(PowerTaskConverter))]
    public sealed class PowerTask
    {
        public string Tag { get; set; } = "";
        public TaskOptions Use { get; set; } = TaskOptions.Own;
        /// <summary>The options object (<see cref="PowerTaskTypes.OptionsType"/> for the tag), or null for none / not set.</summary>
        public object Options { get; set; }

        public PowerTask() { }
        public PowerTask(string tag, TaskOptions use = TaskOptions.Own, object options = null) { Tag = tag; Use = use; Options = options; }

        /// <summary>The tool, or null if this version of ClipX doesn't know the tag (a ClipX 1 tool not ported).</summary>
        [JsonIgnore] public ToolInfo Tool => ToolCatalogue.Find(Tag);

        /// <summary>The tool's name, or the tag in brackets if it isn't known.</summary>
        [JsonIgnore] public string Name => Tool?.Name.Replace("&&", "&") ?? $"(unknown tool {Tag})";
    }

    /// <summary>
    /// A PowerButton's icon: a bitmap (PNG, <see cref="Size"/> pixels square, shown at 32), and on
    /// top of it at most one layer not yet committed to it: characters in a font (ClipX 1's icon),
    /// which can still be moved and resized in the icon editor. Buttons from ClipX 1 come in as a
    /// coloured square with their characters as that layer.
    /// </summary>
    public sealed class ButtonIcon
    {
        /// <summary>The icon is drawn at 64 pixels and shown at 32 (sharp at 150 % and 200 % scaling).</summary>
        public const int Size = 64;

        /// <summary>The bitmap, as base64 PNG; null or empty for a blank icon.</summary>
        public string Png { get; set; }
        /// <summary>Characters drawn over the bitmap (not yet committed to it), or null.</summary>
        public IconText Text { get; set; }

        public ButtonIcon Clone() => new ButtonIcon { Png = Png, Text = Text?.Clone() };
    }

    /// <summary>Characters in a font, on an icon: position and size in icon pixels (of <see cref="ButtonIcon.Size"/>).</summary>
    public sealed class IconText
    {
        public string Characters { get; set; } = "A";
        public string FontName { get; set; } = "Segoe UI";
        /// <summary>The font's em height, in icon pixels.</summary>
        public float Size { get; set; } = 40;
        public bool Bold { get; set; } = true;
        public bool Italic { get; set; }
        /// <summary>ARGB.</summary>
        public int Colour { get; set; } = unchecked((int)0xFFFFFFFF);
        /// <summary>Where the text's box starts (its top left), in icon pixels.</summary>
        public float X { get; set; }
        public float Y { get; set; }

        public IconText Clone() => (IconText)MemberwiseClone();
    }

    /// <summary>
    /// What Find and Replace does in a PowerButton (ClipX 1's edSRC / edREP Details): the texts and
    /// the options, and which of the panel's buttons it is.
    /// </summary>
    public sealed class FindReplaceTask
    {
        public string Find { get; set; } = "";
        public string Replace { get; set; } = "";
        public SearchOptions Search { get; set; } = new SearchOptions();
        public FindAction Action { get; set; } = FindAction.ReplaceAll;
        /// <summary>Find next / Replace: from the start of the text (ClipX 1's Direction S), not from the cursor.</summary>
        public bool FromStart { get; set; }
        /// <summary>Find next / Replace: backwards (ClipX 1's Direction U).</summary>
        public bool Backwards { get; set; }
        /// <summary>Replace all / Find all: only in the selection (if there is one).</summary>
        public bool InSelection { get; set; }

        public FindReplaceTask Clone()
        {
            var c = (FindReplaceTask)MemberwiseClone();
            c.Search = PowerTaskTypes.CopyOf(Search);
            return c;
        }
    }

    public enum FindAction
    {
        /// <summary>Every match replaced.</summary>
        ReplaceAll,
        /// <summary>The next match replaced (ClipX 1's Replace with Replace All off).</summary>
        ReplaceNext,
        /// <summary>The next match selected (ClipX 1's Search: the following tasks work on it).</summary>
        FindNext,
        /// <summary>Every match highlighted.</summary>
        FindAll,
    }

    /// <summary>Which options object each tool takes in a PowerButton.</summary>
    public static class PowerTaskTypes
    {
        /// <summary>The options type for a tag in a PowerButton, or null if the tool has none.</summary>
        public static Type OptionsType(string tag) => tag switch
        {
            "edSRC" or "edREP" => typeof(FindReplaceTask),
            "pbINS" => typeof(string),      // the text inserted
            "pbPBT" => typeof(Guid),        // the PowerButton included
            "stFNT" => typeof(string),      // font name
            "stSIZ" => typeof(float),       // points
            "stHDG" => typeof(int),         // 0 = Normal, 1–6
            "stCOL" or "stHIL" => typeof(int), // RGB, or -1 for automatic / none
            "chPNC" => null,                // its options aren't shown anywhere yet
            _ => ToolCatalogue.Find(tag)?.OptionsType,
        };

        /// <summary>The tool has options (so Own / Last used / Ask apply).</summary>
        public static bool HasOptions(string tag) => OptionsType(tag) != null;

        /// <summary>Only Own makes sense: the tool has no last-used options to fall back on (Insert text, include a PowerButton).</summary>
        public static bool MustBeOwn(string tag) => tag is "pbINS" or "pbPBT";

        /// <summary>The tool has a side panel to ask with (an "Ask" task shows it; its Apply carries on).</summary>
        public static bool CanAsk(string tag) => tag is "foIND" or "foUNI" or "foWRP" or "foREM" or "liBUL" or "liNUM" or "liREN" or "edSRC" or "edREP";

        /// <summary>Works only on rich text (fonts and styles): a PowerButton stops there in plain-text mode.</summary>
        public static bool RichTextOnly(string tag) => tag is "stFNT" or "stSIZ" or "stBLD" or "stITA" or "stUND" or "stSTR" or "stSUP" or "stSUB"
            or "stCLR" or "stALL" or "stALC" or "stALR" or "stALJ" or "stCOL" or "stHIL" or "stHDG";

        /// <summary>A copy of an options object (through JSON: they are plain settings classes).</summary>
        public static T CopyOf<T>(T o) => o == null ? default : (T)JsonSerializer.Deserialize(JsonSerializer.Serialize(o, o.GetType()), o.GetType());

        public static object CopyOf(object o, Type type) => o == null ? null : JsonSerializer.Deserialize(JsonSerializer.Serialize(o, type), type);
    }

    /// <summary>
    /// Writes a task as {"Tag":…, "Use":…, "Options":{…}}, the options in the shape of the tag's
    /// options type, and reads it back into that type (so the settings file stays readable and
    /// each tool's options are its ordinary settings class).
    /// </summary>
    internal sealed class PowerTaskConverter : JsonConverter<PowerTask>
    {
        public override PowerTask Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            JsonElement root = doc.RootElement;
            var task = new PowerTask();
            if (root.TryGetProperty("Tag", out JsonElement tag)) task.Tag = tag.GetString() ?? "";
            if (root.TryGetProperty("Use", out JsonElement use))
                task.Use = use.ValueKind == JsonValueKind.Number ? (TaskOptions)use.GetInt32()
                    : Enum.TryParse(use.GetString(), out TaskOptions u) ? u : TaskOptions.Own;
            Type type = PowerTaskTypes.OptionsType(task.Tag);
            if (type != null && root.TryGetProperty("Options", out JsonElement o) && o.ValueKind != JsonValueKind.Null)
            {
                try { task.Options = o.Deserialize(type, options); }
                catch (Exception ex) when (ex is JsonException || ex is NotSupportedException || ex is FormatException) { task.Options = null; } // a changed type: the defaults
            }
            return task;
        }

        public override void Write(Utf8JsonWriter writer, PowerTask value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Tag", value.Tag);
            writer.WriteString("Use", value.Use.ToString());
            writer.WritePropertyName("Options");
            if (value.Options == null) writer.WriteNullValue();
            else JsonSerializer.Serialize(writer, value.Options, value.Options.GetType(), options);
            writer.WriteEndObject();
        }
    }

    /// <summary>Copies, lookups and checks on the list of PowerButtons.</summary>
    public static class PowerButtonStore
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
        };

        internal static PowerButton Copy(PowerButton b) =>
            JsonSerializer.Deserialize<PowerButton>(JsonSerializer.Serialize(b, Json), Json);

        public static string Serialize(IEnumerable<PowerButton> buttons) => JsonSerializer.Serialize(buttons.ToList(), Json);

        public static List<PowerButton> Deserialize(string json) =>
            JsonSerializer.Deserialize<List<PowerButton>>(json, Json) ?? new List<PowerButton>();

        /// <summary>
        /// The chain of PowerButtons <paramref name="button"/> would include itself through, if
        /// <paramref name="buttons"/> stood as they are with <paramref name="button"/> as given;
        /// null if there is no loop. (ClipX 1's isCircular.)
        /// </summary>
        public static List<PowerButton> FindLoop(PowerButton button, IReadOnlyList<PowerButton> buttons)
        {
            PowerButton ById(Guid id) => id == button.Id ? button : buttons.FirstOrDefault(b => b.Id == id);
            var path = new List<PowerButton>();
            var done = new HashSet<Guid>();
            bool Visit(PowerButton b)
            {
                path.Add(b);
                foreach (PowerTask t in b.Tasks)
                {
                    if (t.Tag != "pbPBT" || !(t.Options is Guid id)) continue;
                    if (id == button.Id) { path.Add(button); return true; }
                    PowerButton inner = ById(id);
                    if (inner == null || done.Contains(id) || path.Contains(inner)) continue;
                    if (Visit(inner)) return true;
                }
                done.Add(b.Id);
                path.RemoveAt(path.Count - 1);
                return false;
            }
            return Visit(button) ? path : null;
        }
    }
}
