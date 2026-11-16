using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ClipX2.UI
{
    /// <summary>One definition, in plain text, with any example sentences.</summary>
    internal sealed class WordDefinition
    {
        public string Text { get; set; }
        public List<string> Examples { get; } = new List<string>();
    }

    /// <summary>One part of speech of a word, with its definitions.</summary>
    internal sealed class WordSense
    {
        public string PartOfSpeech { get; set; }
        public List<WordDefinition> Definitions { get; } = new List<WordDefinition>();
    }

    /// <summary>The outcome of a look-up: senses found, or a message saying why not.</summary>
    internal sealed class LookupResult
    {
        public string Word { get; set; }
        /// <summary>Where the definitions came from, for the credit: "Wiktionary", "Simple English Wiktionary".</summary>
        public string Source { get; set; } = WiktionaryLookup.FullName;
        public List<WordSense> Senses { get; } = new List<WordSense>();
        /// <summary>Set when there are no senses: not found, no entry in this language, or no connection.</summary>
        public string Message { get; set; }
        /// <summary>No connection, or the server is busy: worth asking again later, so not cached.</summary>
        public bool Transient { get; set; }
    }

    /// <summary>
    /// Definitions from Wiktionary (CC BY-SA, so credited wherever shown).
    ///
    /// For English, the Simple English Wiktionary is tried first if wanted: its definitions are
    /// written in plain English for children and learners ("When you receive something, you get
    /// something that was sent or given to you"), with example sentences, but it has far fewer
    /// entries. It has no definitions API, so its pages' wiki markup is read and simplified.
    ///
    /// Otherwise the English Wiktionary's REST API, which has entries for words in many
    /// languages (with definitions written in English); the entry for the spelling language is
    /// used.
    ///
    /// Only the word asked about is sent, and only when the user asks. Results are cached for
    /// the session.
    /// </summary>
    internal static class WiktionaryLookup
    {
        public const string FullName = "Wiktionary";
        public const string SimpleName = "Simple English Wiktionary";

        private const string Endpoint = "https://en.wiktionary.org/api/rest_v1/page/definition/";
        private const string SimpleEndpoint =
            "https://simple.wiktionary.org/w/api.php?action=query&prop=revisions&rvprop=content&rvslots=main&format=json&formatversion=2&redirects=1&titles=";

        private static readonly HttpClient Http = CreateClient();
        private static readonly ConcurrentDictionary<string, LookupResult> Cache = new ConcurrentDictionary<string, LookupResult>();

        private static HttpClient CreateClient()
        {
            var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            // Wikimedia asks for a descriptive User-Agent
            string version = typeof(WiktionaryLookup).Assembly.GetName().Version?.ToString(2) ?? "2";
            http.DefaultRequestHeaders.UserAgent.ParseAdd($"ClipX2/{version} (desktop text utility; spelling definitions)");
            return http;
        }

        /// <param name="word">The word (as written; if not found, its lower-case and solid forms are tried).</param>
        /// <param name="language">Language code: "en", "fr"… (the first part of en_GB etc.).</param>
        /// <param name="simpleFirst">For English, try the Simple English Wiktionary first.</param>
        public static async Task<LookupResult> LookUpAsync(string word, string language, bool simpleFirst, CancellationToken cancel)
        {
            word = word?.Trim() ?? "";
            bool simple = simpleFirst && language == "en";
            string key = (simple ? "simple|" : "") + language + "|" + word;
            if (Cache.TryGetValue(key, out LookupResult cached)) return cached;

            List<string> forms = Forms(word);
            LookupResult result = null;
            if (simple)
            {
                // Not there, or busy or unreachable: the full Wiktionary (another server) instead
                result = await FetchSimpleAsync(forms, cancel).ConfigureAwait(false);
                if (result.Senses.Count == 0) result = null;
            }
            if (result == null)
                foreach (string form in forms)
                {
                    LookupResult next = await FetchAsync(form, language, cancel).ConfigureAwait(false);
                    if (result == null) result = next; // the first answer's message is the one shown if nothing is found
                    if (next.Senses.Count > 0 || next.Transient) { result = next; break; }
                }

            if (!result.Transient) Cache[key] = result; // don't remember a failed connection
            return result;
        }

        /// <summary>
        /// The forms to try, in order: the word as written; then lower case; then, for "school
        /// friend" or "school-friend", solid ("schoolfriend"), since Hunspell may suggest only the
        /// open forms of a closed compound.
        /// </summary>
        private static List<string> Forms(string word)
        {
            var forms = new List<string> { word };
            void Add(string form) { if (form != null && !forms.Contains(form)) forms.Add(form); }
            Add(word.ToLowerInvariant());
            string solid = SolidForm(word);
            if (solid != null) { Add(solid); Add(solid.ToLowerInvariant()); }
            return forms;
        }

        /// <summary>"school friend" / "school-friend" → "schoolfriend"; null if there is nothing to join.</summary>
        private static string SolidForm(string word)
        {
            string solid = word.Replace(" ", "").Replace("-", "");
            return solid.Length > 0 && solid != word ? solid : null;
        }

        private const string NoConnection = "Wiktionary could not be reached.";
        private const string Busy = "Wiktionary is busy. Try again in a moment.";

        /// <summary>
        /// The successful (or 404) response, or null with result.Message set: no connection,
        /// timed out, too many requests (429), or another server error.
        /// </summary>
        private static async Task<HttpResponseMessage> GetAsync(string url, LookupResult result, CancellationToken cancel)
        {
            HttpResponseMessage response;
            try
            {
                response = await Http.GetAsync(url, cancel).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancel.IsCancellationRequested) { throw; }
            catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException) // no network, or timed out
            {
                Fail(result, NoConnection);
                return null;
            }
            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound) return response;
            Fail(result, response.StatusCode == (HttpStatusCode)429 || response.StatusCode == HttpStatusCode.ServiceUnavailable
                ? Busy : NoConnection);
            response.Dispose();
            return null;
        }

        private static void Fail(LookupResult result, string message)
        {
            result.Message = message;
            result.Transient = true;
        }

        /************ The full Wiktionary: REST definitions API ************/

        private static async Task<LookupResult> FetchAsync(string word, string language, CancellationToken cancel)
        {
            var result = new LookupResult { Word = word };
            string url = Endpoint + Uri.EscapeDataString(word.Replace(' ', '_'));
            HttpResponseMessage response = await GetAsync(url, result, cancel).ConfigureAwait(false);
            if (response == null) return result;

            using (response)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    result.Message = $"Wiktionary has no entry for '{word}'.";
                    return result;
                }
                string json = await response.Content.ReadAsStringAsync(cancel).ConfigureAwait(false);
                Parse(json, language, result);
            }
            return result;
        }

        /// <summary>{ "en": [ { "partOfSpeech": "Verb", "definitions": [ { "definition": "&lt;html&gt;" } ] } ], "fr": [...] }</summary>
        private static void Parse(string json, string language, LookupResult result)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty(language, out JsonElement entries) || entries.ValueKind != JsonValueKind.Array)
                {
                    // Each language's entries carry its name ("French"); list them
                    var names = new List<string>();
                    foreach (JsonProperty other in doc.RootElement.EnumerateObject())
                        if (other.Value.ValueKind == JsonValueKind.Array)
                            foreach (JsonElement e in other.Value.EnumerateArray())
                            {
                                string name = e.TryGetProperty("language", out JsonElement l) ? l.GetString() : other.Name;
                                if (!string.IsNullOrEmpty(name) && !names.Contains(name)) names.Add(name);
                                break;
                            }
                    result.Message = names.Count == 0
                        ? $"Wiktionary has '{result.Word}', but not as a word in this language."
                        : $"Wiktionary has '{result.Word}' only in {JoinNames(names)}.";
                    return;
                }
                foreach (JsonElement entry in entries.EnumerateArray())
                {
                    var sense = new WordSense
                    {
                        PartOfSpeech = entry.TryGetProperty("partOfSpeech", out JsonElement pos) ? pos.GetString() : ""
                    };
                    if (entry.TryGetProperty("definitions", out JsonElement defs) && defs.ValueKind == JsonValueKind.Array)
                        foreach (JsonElement def in defs.EnumerateArray())
                        {
                            string text = def.TryGetProperty("definition", out JsonElement d) ? ToPlainText(d.GetString()) : "";
                            if (text.Length > 0) sense.Definitions.Add(new WordDefinition { Text = text });
                        }
                    if (sense.Definitions.Count > 0) result.Senses.Add(sense);
                }
                if (result.Senses.Count == 0) result.Message = $"Wiktionary has no definitions for '{result.Word}'.";
            }
            catch (JsonException)
            {
                Fail(result, NoConnection);
            }
        }

        /// <summary>"French"; "French and Spanish"; "French, Spanish and 3 other languages".</summary>
        private static string JoinNames(List<string> names)
        {
            if (names.Count == 1) return names[0];
            if (names.Count <= 3) return string.Join(", ", names.GetRange(0, names.Count - 1)) + " and " + names[names.Count - 1];
            return $"{names[0]}, {names[1]} and {names.Count - 2} other languages";
        }

        /************ Simple English Wiktionary: the page's wiki markup ************/

        /// <summary>
        /// All the forms in one request (Wikimedia limits how often it may be asked), then the
        /// first form, in order, whose page has definitions.
        /// </summary>
        private static async Task<LookupResult> FetchSimpleAsync(List<string> forms, CancellationToken cancel)
        {
            var result = new LookupResult { Word = forms[0], Source = SimpleName };
            string url = SimpleEndpoint + Uri.EscapeDataString(string.Join("|", forms));
            HttpResponseMessage response = await GetAsync(url, result, cancel).ConfigureAwait(false);
            if (response == null) return result;

            using (response)
            {
                string json = await response.Content.ReadAsStringAsync(cancel).ConfigureAwait(false);
                try
                {
                    // { "query": { "normalized": [ {from, to} ], "redirects": [ {from, to} ],
                    //   "pages": [ { "title", "missing": true } or { "title", "revisions": [ { "slots": { "main": { "content" } } } ] } ] } }
                    using JsonDocument doc = JsonDocument.Parse(json);
                    if (!doc.RootElement.TryGetProperty("query", out JsonElement query)) return NotInSimple(result);
                    var renames = new Dictionary<string, string>(StringComparer.Ordinal);
                    foreach (string list in new[] { "normalized", "redirects" })
                        if (query.TryGetProperty(list, out JsonElement items) && items.ValueKind == JsonValueKind.Array)
                            foreach (JsonElement item in items.EnumerateArray())
                                renames[item.GetProperty("from").GetString()] = item.GetProperty("to").GetString();
                    var pages = new Dictionary<string, string>(StringComparer.Ordinal);
                    if (query.TryGetProperty("pages", out JsonElement pageList) && pageList.ValueKind == JsonValueKind.Array)
                        foreach (JsonElement page in pageList.EnumerateArray())
                            if (page.TryGetProperty("revisions", out JsonElement revs) && revs.GetArrayLength() > 0
                                && revs[0].TryGetProperty("slots", out JsonElement slots)
                                && slots.TryGetProperty("main", out JsonElement main)
                                && main.TryGetProperty("content", out JsonElement content))
                                pages[page.GetProperty("title").GetString()] = content.GetString();

                    foreach (string form in forms)
                    {
                        string title = form;
                        for (int hops = 0; hops < 3 && renames.TryGetValue(title, out string to); hops++) title = to; // normalised, then redirected
                        if (!pages.TryGetValue(title, out string wikitext)) continue;
                        var found = new LookupResult { Word = title, Source = SimpleName };
                        ParseWikitext(wikitext, found);
                        if (found.Senses.Count > 0) return found;
                    }
                }
                catch (Exception ex) when (ex is JsonException || ex is InvalidOperationException || ex is KeyNotFoundException)
                {
                    Fail(result, NoConnection);
                    return result;
                }
            }
            return NotInSimple(result);
        }

        private static LookupResult NotInSimple(LookupResult result)
        {
            result.Message = $"{SimpleName} has no entry for '{result.Word}'.";
            return result;
        }

        private static readonly Regex Heading = new Regex(@"^(={2,6})\s*(.*?)\s*\1\s*$", RegexOptions.Compiled);

        /// <summary>
        /// == Verb ==
        /// # {{transitive}} When you '''receive''' something, you [[get]] something…{{syn|obtain}}
        /// #: ''I '''receive''' [[letters]] in the [[mail]] every day.''
        /// Headings with numbered lines under them are parts of speech; "#:" lines are examples.
        /// </summary>
        private static void ParseWikitext(string wikitext, LookupResult result)
        {
            if (string.IsNullOrEmpty(wikitext)) return;
            string heading = "";
            WordSense sense = null;
            foreach (string raw in wikitext.Split('\n'))
            {
                string line = raw.TrimEnd('\r');
                Match h = Heading.Match(line);
                if (h.Success)
                {
                    heading = WikiToPlainText(h.Groups[2].Value);
                    sense = null;
                    continue;
                }
                if (!line.StartsWith("#")) continue;

                int marks = 0;
                while (marks < line.Length && line[marks] == '#') marks++;
                char kind = marks < line.Length ? line[marks] : ' ';
                if (kind == '*') continue; // quotations
                string text = WikiToPlainText(line.Substring(kind == ':' ? marks + 1 : marks));
                if (text.Length == 0) continue;

                if (kind == ':')
                {
                    if (sense != null && sense.Definitions.Count > 0)
                        sense.Definitions[sense.Definitions.Count - 1].Examples.Add(text);
                    continue;
                }
                if (sense == null)
                {
                    sense = new WordSense { PartOfSpeech = heading };
                    result.Senses.Add(sense);
                }
                sense.Definitions.Add(new WordDefinition { Text = text });
            }
        }

        private static readonly Regex Template = new Regex(@"\{\{([^{}]*)\}\}", RegexOptions.Compiled);
        private static readonly Regex FileLink = new Regex(@"\[\[(?:File|Image|Category):[^\[\]]*\]\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex Link = new Regex(@"\[\[(?:[^\[\]|]*\|)?([^\[\]|]*)\]\]", RegexOptions.Compiled);
        private static readonly Regex ExternalLink = new Regex(@"\[https?://\S+\s*([^\]]*)\]", RegexOptions.Compiled);
        private static readonly Regex Label = new Regex(@"^[a-z][a-z ]*$", RegexOptions.Compiled);

        /// <summary>
        /// Wiki markup as plain text: [[link|text]] → text; '''bold''' and ''italic'' marks
        /// dropped; templates dropped, except bare labels such as {{transitive}} → (transitive).
        /// </summary>
        private static string WikiToPlainText(string wiki)
        {
            string text = wiki;
            for (int guard = 0; guard < 10 && text.Contains("{{"); guard++) // innermost first, for nested templates
                text = Template.Replace(text, m =>
                {
                    string inner = m.Groups[1].Value.Trim();
                    return Label.IsMatch(inner) ? "(" + inner + ") " : "";
                });
            text = FileLink.Replace(text, "");
            text = Link.Replace(text, "$1");
            text = ExternalLink.Replace(text, "$1");
            text = text.Replace("'''", "").Replace("''", "");
            return ToPlainText(text); // <ref>s and any HTML
        }

        /************ HTML ************/

        // Removed with their contents: style/script, footnotes, and nested lists (sub-senses,
        // which the API also lists as definitions of their own)
        private static readonly Regex Blocks = new Regex(@"<(style|script|ref|ol|ul|dl)\b[^>]*>.*?</\1\s*>",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private static readonly Regex Tags = new Regex("<[^>]*>", RegexOptions.Compiled);
        private static readonly Regex Spaces = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>Wiktionary's HTML fragment as plain text (links, labels and markup removed).</summary>
        private static string ToPlainText(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            string text = WebUtility.HtmlDecode(Tags.Replace(Blocks.Replace(html, ""), ""));
            return Spaces.Replace(text, " ").Trim();
        }
    }
}
