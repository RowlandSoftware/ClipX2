using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ClipX2.TextEngine.Grammar
{
    /// <summary>Which LanguageTool to ask.</summary>
    public enum LanguageToolService
    {
        /// <summary>The free public service (no account; limited requests and text per minute).</summary>
        Free,
        /// <summary>A Premium account: higher limits and extra rules; needs the account email and an API key.</summary>
        Premium,
        /// <summary>A LanguageTool server the user runs (e.g. http://localhost:8081): private, no limits.</summary>
        OwnServer,
    }

    /// <summary>How to reach LanguageTool and what to ask it.</summary>
    public sealed class LanguageToolOptions
    {
        public LanguageToolService Service { get; set; } = LanguageToolService.Free;
        /// <summary>For <see cref="LanguageToolService.OwnServer"/>: its address, e.g. http://localhost:8081.</summary>
        public string ServerUrl { get; set; } = "http://localhost:8081";
        /// <summary>For Premium: the account's email address.</summary>
        public string Username { get; set; } = "";
        /// <summary>For Premium: the API key from the account's settings page.</summary>
        public string ApiKey { get; set; } = "";
        /// <summary>LanguageTool's language code, e.g. en-GB (see <see cref="LanguageToolChecker.LanguageCode"/>).</summary>
        public string Language { get; set; } = "en-GB";
        /// <summary>Rules the user has chosen to ignore (rule ids).</summary>
        public IList<string> DisabledRules { get; set; } = new List<string>();
        /// <summary>Also the stricter rules LanguageTool keeps for its "picky" level.</summary>
        public bool Picky { get; set; }
    }

    /// <summary>A part of the text that has been checked, with its issues (positions in the whole text).</summary>
    public sealed class GrammarChunk
    {
        internal GrammarChunk(int start, int end, IReadOnlyList<GrammarIssue> issues)
        {
            Start = start; End = end; Issues = issues;
        }
        public int Start { get; }
        public int End { get; }
        public IReadOnlyList<GrammarIssue> Issues { get; }
    }

    /// <summary>
    /// Grammar (and spelling, style, typography) checking by LanguageTool, through its HTTP API:
    /// the free public service, a Premium account, or the user's own server; they all take the
    /// same request.
    ///
    /// The text goes a paragraph at a time ("a paragraph is an entity"): paragraphs are grouped
    /// into requests under the service's size limit, starting with the one holding the caret,
    /// so the first results come quickly while the rest follows. Each paragraph's results are
    /// kept, so checking again sends only paragraphs that have changed. For the free service
    /// (and Premium) the requests are paced to stay inside the per-minute limits, rather than
    /// being refused.
    ///
    /// LanguageTool asks that it be credited with a visible link: see <see cref="Credit"/>.
    /// The text checked goes to the server: the user must be told so (except for their own server).
    /// </summary>
    public sealed class LanguageToolChecker
    {
        public const string Credit = "Grammar checking by LanguageTool";
        public const string CreditUrl = "https://languagetool.org";

        private const string FreeEndpoint = "https://api.languagetool.org/v2/check";
        private const string PremiumEndpoint = "https://api.languagetoolplus.com/v2/check";
        private const int MaxReplacements = 10;
        private const int FirstRequestBytes = 2000;

        private static readonly HttpClient Http = CreateClient();

        private static HttpClient CreateClient()
        {
            var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) }; // then "could not be reached", and spelling carries on
            string version = typeof(LanguageToolChecker).Assembly.GetName().Version?.ToString(2) ?? "2";
            http.DefaultRequestHeaders.UserAgent.ParseAdd($"ClipX2/{version} (desktop text utility; grammar check)");
            return http;
        }

        private readonly LanguageToolOptions _options;
        private readonly Limits _limits;
        private readonly SemaphoreSlim _oneAtATime = new SemaphoreSlim(1, 1);
        private readonly Queue<(DateTime Time, int Bytes)> _sent = new Queue<(DateTime, int)>();
        // Each segment's issues (positions within the segment), by its text
        private readonly Dictionary<string, List<GrammarIssue>> _cache = new Dictionary<string, List<GrammarIssue>>(StringComparer.Ordinal);
        private const int CacheLimit = 20000;
        private string _language;

        public LanguageToolChecker(LanguageToolOptions options)
        {
            _options = options ?? new LanguageToolOptions();
            _language = string.IsNullOrWhiteSpace(_options.Language) ? "en-GB" : _options.Language.Trim();
            _limits = Limits.For(_options.Service);
        }

        public LanguageToolOptions Options => _options;

        /// <summary>The language code actually used (may be shortened if the server didn't know the variety).</summary>
        public string Language => _language;

        /// <summary>
        /// A Hunspell dictionary name as a LanguageTool code: en_GB → en-GB, fr → fr; anything
        /// after the country is dropped (en_GB-ize → en-GB, de_DE_frami → de-DE).
        /// </summary>
        public static string LanguageCode(string locale)
        {
            if (string.IsNullOrWhiteSpace(locale)) return "en-GB";
            string[] parts = locale.Trim().Split('_', '-');
            string language = parts[0].ToLowerInvariant();
            if (parts.Length > 1 && parts[1].Length == 2 && parts[1].All(char.IsLetter))
                return language + "-" + parts[1].ToUpperInvariant();
            return language;
        }

        /// <summary>Forgets the results kept for paragraphs (e.g. after changing the ignored rules).</summary>
        public void ClearCache() { lock (_cache) _cache.Clear(); }

        /************ Checking ************/

        /// <summary>
        /// Checks the whole text, starting with the paragraph at <paramref name="from"/> and going
        /// on to the end, then from the top. <paramref name="onChecked"/> (if given) hears about
        /// each part as soon as it is done, so the first issues can be shown before the rest
        /// arrive; it is called on whatever thread the work is on. If the check stops short (no
        /// connection, limits, key refused), the result has what was found so far and why.
        /// With <paramref name="until"/> (≥ 0), only the paragraphs from <paramref name="from"/>
        /// up to that position are checked (a selection), and it doesn't go round to the top.
        /// </summary>
        public async Task<GrammarCheckResult> CheckAsync(string text, int from = 0,
            Action<GrammarChunk> onChecked = null, CancellationToken cancel = default, int until = -1)
        {
            var all = new List<GrammarIssue>();
            if (string.IsNullOrEmpty(text)) return new GrammarCheckResult(all);

            List<Segment> segments = Segments(text, _limits.MaxRequestBytes);
            int first = Math.Max(0, segments.FindIndex(s => s.End >= Math.Max(0, Math.Min(from, text.Length))));
            var order = until >= 0
                ? Enumerable.Range(first, segments.Count - first).TakeWhile(i => i == first || segments[i].Start < until).ToList()
                : Enumerable.Range(first, segments.Count - first).Concat(Enumerable.Range(0, first)).ToList();
            var done = new bool[segments.Count];
            int requests = 0;

            foreach (int index in order)
            {
                if (done[index]) continue;
                Segment seg = segments[index];

                // Already known: no request
                List<GrammarIssue> known;
                lock (_cache) _cache.TryGetValue(CacheKey(seg.Text), out known);
                if (seg.Blank || known != null)
                {
                    done[index] = true;
                    var issues = Place(known, seg, text);
                    all.AddRange(issues);
                    onChecked?.Invoke(new GrammarChunk(seg.Start, seg.End, issues));
                    continue;
                }

                // A request: this segment and the ones after it not yet known, up to the size limit;
                // the first request is kept small (the paragraph at the caret), so the first issue comes quickly
                int size = requests == 0 ? Math.Min(FirstRequestBytes, _limits.MaxRequestBytes) : _limits.MaxRequestBytes;
                requests++;
                int last = index, bytes = seg.Bytes;
                while (last + 1 < segments.Count && !done[last + 1])
                {
                    Segment next = segments[last + 1];
                    bool nextKnown;
                    lock (_cache) nextKnown = next.Blank || _cache.ContainsKey(CacheKey(next.Text));
                    if (nextKnown || bytes + 1 + next.Bytes > size) break;
                    bytes += 1 + next.Bytes;
                    last++;
                }
                int start = seg.Start, end = segments[last].End;
                Response response = await RequestAsync(text.Substring(start, end - start), bytes, cancel).ConfigureAwait(false);
                if (response.Error != GrammarCheckError.None)
                {
                    all.Sort((a, b) => a.Start.CompareTo(b.Start));
                    return new GrammarCheckResult(all, response.Error, response.Message);
                }

                // Share the issues out to their segments, and keep them
                var chunkIssues = new List<GrammarIssue>();
                for (int i = index; i <= last; i++)
                {
                    Segment s = segments[i];
                    var own = new List<GrammarIssue>();
                    foreach (GrammarIssue issue in response.Issues)
                    {
                        int at = start + issue.Start;
                        bool inside = (at >= s.Start || i == index) && (i == last || at < segments[i + 1].Start);
                        if (inside) own.Add(issue.Moved(at - s.Start));
                    }
                    Remember(s.Text, own);
                    done[i] = true;
                    chunkIssues.AddRange(Place(own, s, text));
                }
                all.AddRange(chunkIssues);
                onChecked?.Invoke(new GrammarChunk(start, end, chunkIssues));
            }

            all.Sort((a, b) => a.Start.CompareTo(b.Start));
            return new GrammarCheckResult(all);
        }

        /// <summary>
        /// A short check, to see whether the service answers and (for Premium) the key is
        /// accepted. Nothing is kept.
        /// </summary>
        public async Task<GrammarCheckResult> TestConnectionAsync(CancellationToken cancel = default)
        {
            const string sample = "This is a test.";
            Response r = await RequestAsync(sample, sample.Length, cancel).ConfigureAwait(false);
            return new GrammarCheckResult(r.Issues, r.Error, r.Message);
        }

        private string CacheKey(string segmentText) => segmentText; // one checker = one language and set of options

        private void Remember(string segmentText, List<GrammarIssue> issues)
        {
            lock (_cache)
            {
                if (_cache.Count >= CacheLimit) _cache.Clear();
                _cache[CacheKey(segmentText)] = issues;
            }
        }

        /// <summary>A segment's issues placed in the whole text.</summary>
        private static List<GrammarIssue> Place(List<GrammarIssue> issues, Segment seg, string text)
        {
            var placed = new List<GrammarIssue>();
            if (issues == null) return placed;
            foreach (GrammarIssue i in issues)
            {
                GrammarIssue p = i.Moved(seg.Start + i.Start);
                int len = Math.Min(p.Length, Math.Max(0, text.Length - p.Start));
                p.Text = p.Start < text.Length ? text.Substring(p.Start, len) : "";
                placed.Add(p);
            }
            return placed;
        }

        /************ Paragraphs ************/

        private sealed class Segment
        {
            public int Start, End, Bytes;
            public string Text;
            public bool Blank;
        }

        /// <summary>
        /// The text as paragraphs (line breaks not included), each cut at sentence ends if it is
        /// too long for one request on its own.
        /// </summary>
        private static List<Segment> Segments(string text, int maxBytes)
        {
            var list = new List<Segment>();
            int p = 0;
            while (p <= text.Length)
            {
                int nl = text.IndexOf('\n', p);
                int end = nl < 0 ? text.Length : nl;
                int contentEnd = end > p && text[end - 1] == '\r' ? end - 1 : end;
                foreach (var (s, e) in Cut(text, p, contentEnd, maxBytes))
                {
                    string t = text.Substring(s, e - s);
                    list.Add(new Segment { Start = s, End = e, Text = t, Bytes = Encoding.UTF8.GetByteCount(t), Blank = string.IsNullOrWhiteSpace(t) });
                }
                if (nl < 0) break;
                p = nl + 1;
            }
            return list;
        }

        /// <summary>A paragraph in pieces of at most <paramref name="maxBytes"/>: at a sentence end if possible, else a space.</summary>
        private static IEnumerable<(int, int)> Cut(string text, int start, int end, int maxBytes)
        {
            int maxChars = maxBytes / 3; // safe even if every character took three bytes
            if (Encoding.UTF8.GetByteCount(text.Substring(start, end - start)) <= maxBytes) { yield return (start, end); yield break; }
            int s = start;
            while (end - s > maxChars)
            {
                int limit = s + maxChars, cut = -1;
                for (int i = limit - 1; i > s + maxChars / 2 && cut < 0; i--)
                    if ((text[i - 1] == '.' || text[i - 1] == '!' || text[i - 1] == '?') && text[i] == ' ') cut = i + 1;
                if (cut < 0) { int sp = text.LastIndexOf(' ', limit - 1, maxChars / 2); cut = sp > s ? sp + 1 : limit; }
                yield return (s, cut);
                s = cut;
            }
            if (s < end) yield return (s, end);
        }

        /************ Requests ************/

        private sealed class Response
        {
            public List<GrammarIssue> Issues = new List<GrammarIssue>();
            public GrammarCheckError Error;
            public string Message = "";
        }

        private string Endpoint
        {
            get
            {
                switch (_options.Service)
                {
                    case LanguageToolService.Premium: return PremiumEndpoint;
                    case LanguageToolService.OwnServer:
                        string url = (_options.ServerUrl ?? "").Trim().TrimEnd('/');
                        if (url.Length == 0) url = "http://localhost:8081";
                        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                            url = "http://" + url;
                        if (url.EndsWith("/v2/check", StringComparison.OrdinalIgnoreCase)) return url;
                        return url.EndsWith("/v2", StringComparison.OrdinalIgnoreCase) ? url + "/check" : url + "/v2/check";
                    default: return FreeEndpoint;
                }
            }
        }

        private async Task<Response> RequestAsync(string chunk, int bytes, CancellationToken cancel)
        {
            await _oneAtATime.WaitAsync(cancel).ConfigureAwait(false);
            try
            {
                for (int attempt = 0; ; attempt++)
                {
                    await WaitTurnAsync(bytes, cancel).ConfigureAwait(false);
                    var fields = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("text", chunk),
                        new KeyValuePair<string, string>("language", _language),
                    };
                    if (_options.Service == LanguageToolService.Premium)
                    {
                        fields.Add(new KeyValuePair<string, string>("username", (_options.Username ?? "").Trim()));
                        fields.Add(new KeyValuePair<string, string>("apiKey", (_options.ApiKey ?? "").Trim()));
                    }
                    if (_options.DisabledRules != null && _options.DisabledRules.Count > 0)
                        fields.Add(new KeyValuePair<string, string>("disabledRules", string.Join(",", _options.DisabledRules)));
                    if (_options.Picky) fields.Add(new KeyValuePair<string, string>("level", "picky"));

                    HttpResponseMessage http;
                    try
                    {
                        http = await Http.PostAsync(Endpoint, new FormUrlEncodedContent(fields), cancel).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (cancel.IsCancellationRequested) { throw; }
                    catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException)
                    {
                        return Fail(GrammarCheckError.NoConnection, ServerName + " could not be reached.");
                    }

                    using (http)
                    {
                        string body = await http.Content.ReadAsStringAsync().ConfigureAwait(false);
                        int status = (int)http.StatusCode;
                        if (http.IsSuccessStatusCode) return Parse(body);

                        // Not a known variety (e.g. fr-FR): try the plain language once (fr)
                        if (status == 400 && attempt == 0 && _language.IndexOf('-') > 0 && body.IndexOf("language", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            _language = _language.Substring(0, _language.IndexOf('-'));
                            continue;
                        }
                        if (status == 400 && body.IndexOf("language", StringComparison.OrdinalIgnoreCase) >= 0)
                            return Fail(GrammarCheckError.LanguageNotSupported, $"{ServerName} can't check this language ({_options.Language}).");
                        if (status == 429 || status == 503 || (status == 403 && _options.Service == LanguageToolService.Free))
                            return Fail(GrammarCheckError.Busy, ServerName + " is busy, or its free limits have been reached. Try again in a minute.");
                        if (status == 401 || status == 403)
                            return Fail(GrammarCheckError.NotAuthorised, "LanguageTool didn't accept the account email or API key.");
                        return Fail(GrammarCheckError.NoConnection, $"{ServerName} gave an error ({status}).");
                    }
                }
            }
            finally
            {
                _oneAtATime.Release();
            }
        }

        private string ServerName => _options.Service == LanguageToolService.OwnServer ? "The LanguageTool server" : "LanguageTool";

        private static Response Fail(GrammarCheckError error, string message) => new Response { Error = error, Message = message };

        /// <summary>
        /// { "matches": [ { "message", "shortMessage", "offset", "length", "replacements": [ { "value" } ],
        ///   "rule": { "id", "description", "issueType", "urls": [ { "value" } ], "category": { "id", "name" } } } ] }
        /// </summary>
        private static Response Parse(string json)
        {
            var response = new Response();
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    if (!doc.RootElement.TryGetProperty("matches", out JsonElement matches) || matches.ValueKind != JsonValueKind.Array)
                        return response;
                    foreach (JsonElement m in matches.EnumerateArray())
                    {
                        var issue = new GrammarIssue(Int(m, "offset"), Int(m, "length"), Str(m, "message"))
                        {
                            ShortMessage = Str(m, "shortMessage"),
                        };
                        if (m.TryGetProperty("replacements", out JsonElement reps) && reps.ValueKind == JsonValueKind.Array)
                            issue.Replacements = reps.EnumerateArray().Select(r => Str(r, "value"))
                                                     .Where(v => v.Length > 0).Take(MaxReplacements).ToList();
                        if (m.TryGetProperty("rule", out JsonElement rule) && rule.ValueKind == JsonValueKind.Object)
                        {
                            issue.RuleId = Str(rule, "id");
                            issue.RuleDescription = Str(rule, "description");
                            issue.IssueType = Str(rule, "issueType");
                            if (rule.TryGetProperty("category", out JsonElement cat) && cat.ValueKind == JsonValueKind.Object)
                            {
                                issue.CategoryId = Str(cat, "id");
                                issue.CategoryName = Str(cat, "name");
                            }
                            if (rule.TryGetProperty("urls", out JsonElement urls) && urls.ValueKind == JsonValueKind.Array)
                                issue.MoreInfoUrl = urls.EnumerateArray().Select(u => Str(u, "value")).FirstOrDefault(u => u.Length > 0) ?? "";
                        }
                        response.Issues.Add(issue);
                    }
                }
            }
            catch (JsonException)
            {
                return Fail(GrammarCheckError.NoConnection, "LanguageTool's reply could not be read.");
            }
            return response;
        }

        private static string Str(JsonElement e, string name) =>
            e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out JsonElement v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";

        private static int Int(JsonElement e, string name) =>
            e.TryGetProperty(name, out JsonElement v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out int n) ? n : 0;

        /************ Pacing ************/

        /// <summary>The service's limits (per minute, and per request).</summary>
        private sealed class Limits
        {
            public int RequestsPerMinute, BytesPerMinute, MaxRequestBytes;

            public static Limits For(LanguageToolService service)
            {
                switch (service)
                {
                    // The published limits, with a margin: the free service may also slow heavy users down
                    case LanguageToolService.Premium:   return new Limits { RequestsPerMinute = 75, BytesPerMinute = 280000, MaxRequestBytes = 55000 };
                    case LanguageToolService.OwnServer: return new Limits { RequestsPerMinute = int.MaxValue, BytesPerMinute = int.MaxValue, MaxRequestBytes = 50000 };
                    default:                            return new Limits { RequestsPerMinute = 18, BytesPerMinute = 70000, MaxRequestBytes = 18000 };
                }
            }
        }

        /// <summary>Waits, if need be, until sending <paramref name="bytes"/> more keeps within the per-minute limits.</summary>
        private async Task WaitTurnAsync(int bytes, CancellationToken cancel)
        {
            if (_limits.RequestsPerMinute == int.MaxValue) return;
            while (true)
            {
                DateTime now = DateTime.UtcNow;
                while (_sent.Count > 0 && now - _sent.Peek().Time >= TimeSpan.FromMinutes(1)) _sent.Dequeue();
                int used = _sent.Sum(s => s.Bytes);
                if (_sent.Count < _limits.RequestsPerMinute && (used + bytes <= _limits.BytesPerMinute || _sent.Count == 0))
                {
                    _sent.Enqueue((now, bytes));
                    return;
                }
                TimeSpan wait = _sent.Peek().Time.AddMinutes(1) - now;
                await Task.Delay(wait > TimeSpan.FromMilliseconds(200) ? wait : TimeSpan.FromMilliseconds(200), cancel).ConfigureAwait(false);
            }
        }
    }
}
