using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ClipX2.UI
{
    /// <summary>
    /// How often each tool is used (ClipX 1's ClipX-Stats), in a file of its own,
    /// %AppData%\ClipX2\usage.json, not in settings.json. Only tool tags, counts and dates:
    /// never any text. Counted by the tool handler (<c>Form1.RunTool</c>) for every tool however
    /// started; PowerButton runs are counted separately as well, as ClipX 1 did.
    ///
    /// "Usage" is since <see cref="Since"/> (the last upload, when uploading is added: it will ask
    /// first); "Total" is since <see cref="FirstUse"/>.
    /// </summary>
    internal sealed class UsageStatistics
    {
        public DateTime FirstUse { get; set; } = DateTime.Now;
        public DateTime Since { get; set; } = DateTime.Now;
        public Dictionary<string, int> Usage { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> Total { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> PowerButtonUsage { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> PowerButtonTotal { get; set; } = new Dictionary<string, int>();

        public static string FilePath { get; } = Path.Combine(AppSettings.Folder, "usage.json");

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = true };

        private static UsageStatistics _current;

        /// <summary>The statistics, loaded on first use.</summary>
        public static UsageStatistics Current => _current ??= Load();

        /// <summary>Counts one use of a tool, and saves.</summary>
        public static void Record(string tag, bool fromPowerButton = false)
        {
            if (string.IsNullOrEmpty(tag)) return;
            UsageStatistics s = Current;
            Add(s.Usage, tag);
            Add(s.Total, tag);
            if (fromPowerButton)
            {
                Add(s.PowerButtonUsage, tag);
                Add(s.PowerButtonTotal, tag);
            }
            s.Save();
        }

        private static void Add(Dictionary<string, int> counts, string tag) =>
            counts[tag] = counts.TryGetValue(tag, out int n) ? n + 1 : 1;

        /// <summary>After an upload: the "since" counts start again (the totals carry on).</summary>
        public void StartNewPeriod()
        {
            Usage.Clear();
            PowerButtonUsage.Clear();
            Since = DateTime.Now;
            Save();
        }

        private static UsageStatistics Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    UsageStatistics s = JsonSerializer.Deserialize<UsageStatistics>(File.ReadAllText(FilePath), JsonOptions);
                    if (s != null)
                    {
                        s.Usage ??= new Dictionary<string, int>();
                        s.Total ??= new Dictionary<string, int>();
                        s.PowerButtonUsage ??= new Dictionary<string, int>();
                        s.PowerButtonTotal ??= new Dictionary<string, int>();
                        return s;
                    }
                }
            }
            catch (Exception ex) when (ex is IOException || ex is JsonException || ex is UnauthorizedAccessException)
            {
                // Unreadable: start again (keeping the bad file aside)
                try { File.Copy(FilePath, FilePath + ".bad", overwrite: true); } catch (Exception) { }
            }
            return new UsageStatistics();
        }

        /// <summary>Saves; a failure to save loses only these counts, so it is ignored.</summary>
        public void Save()
        {
            try
            {
                Directory.CreateDirectory(AppSettings.Folder);
                string temp = FilePath + ".tmp";
                File.WriteAllText(temp, JsonSerializer.Serialize(this, JsonOptions));
                File.Move(temp, FilePath, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { }
        }
    }
}
