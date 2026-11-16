using System;
using System.Collections.Generic;

namespace ClipX2.TextEngine.Grammar
{
    /// <summary>One thing a grammar checker found: where, what, and what it suggests instead.</summary>
    public sealed class GrammarIssue
    {
        public GrammarIssue(int start, int length, string message)
        {
            Start = start;
            Length = length;
            Message = message ?? "";
        }

        /// <summary>Position in the text checked (UTF-16 characters, as in .NET strings).</summary>
        public int Start { get; internal set; }
        public int Length { get; }
        public int End => Start + Length;

        /// <summary>The text flagged (filled in from the text checked).</summary>
        public string Text { get; internal set; } = "";

        /// <summary>The explanation, e.g. "'color' is American English."</summary>
        public string Message { get; }
        /// <summary>A shorter one, if the rule has it (e.g. "Spelling mistake"); may be empty.</summary>
        public string ShortMessage { get; internal set; } = "";

        /// <summary>Suggested replacements, best first (may be empty).</summary>
        public IReadOnlyList<string> Replacements { get; internal set; } = Array.Empty<string>();

        /// <summary>The rule's id, e.g. MORFOLOGIK_RULE_EN_GB, CONFUSION_RULE_FORM_FROM (for "ignore this rule").</summary>
        public string RuleId { get; internal set; } = "";
        public string RuleDescription { get; internal set; } = "";
        /// <summary>The rule's category: id (TYPOS, GRAMMAR, STYLE, PUNCTUATION…) and its name.</summary>
        public string CategoryId { get; internal set; } = "";
        public string CategoryName { get; internal set; } = "";
        /// <summary>The kind of issue: misspelling, grammar, style, typographical, duplication…</summary>
        public string IssueType { get; internal set; } = "";
        /// <summary>A page explaining the rule, if there is one.</summary>
        public string MoreInfoUrl { get; internal set; } = "";

        /// <summary>
        /// The checker's own spelling verdict (a word not in its dictionary), as opposed to a
        /// grammar or style point. Confusions such as form/from are not spelling, though they
        /// are in the TYPOS category, nor "Their → There" (CONFUSED_WORDS), though LanguageTool
        /// calls it a misspelling. Its spelling rules are MORFOLOGIK_RULE_EN_GB, HUNSPELL_RULE,
        /// GERMAN_SPELLER_RULE, FR_SPELLING_RULE and the like.
        /// </summary>
        public bool IsSpelling =>
            IssueType == "misspelling" && CategoryId == "TYPOS"
            && (RuleId.IndexOf("MORFOLOGIK", StringComparison.Ordinal) >= 0 || RuleId.IndexOf("SPELL", StringComparison.Ordinal) >= 0
                || RuleId.IndexOf("HUNSPELL", StringComparison.Ordinal) >= 0);

        public override string ToString() => $"{RuleId} '{Text}' @{Start}: {Message}";

        /// <summary>A copy at another position.</summary>
        internal GrammarIssue Moved(int start) => new GrammarIssue(start, Length, Message)
        {
            Text = Text, ShortMessage = ShortMessage, Replacements = Replacements, RuleId = RuleId,
            RuleDescription = RuleDescription, CategoryId = CategoryId, CategoryName = CategoryName,
            IssueType = IssueType, MoreInfoUrl = MoreInfoUrl,
        };
    }

    /// <summary>Why a check stopped short (or None).</summary>
    public enum GrammarCheckError
    {
        None,
        /// <summary>No network, the server didn't answer, or it gave an error.</summary>
        NoConnection,
        /// <summary>Too many requests: the free service's limits, or the server is busy. Try later.</summary>
        Busy,
        /// <summary>The account email or API key was not accepted (Premium).</summary>
        NotAuthorised,
        /// <summary>The server doesn't know the language.</summary>
        LanguageNotSupported,
    }

    /// <summary>The issues found, and, if the check stopped short, why.</summary>
    public sealed class GrammarCheckResult
    {
        public GrammarCheckResult(IReadOnlyList<GrammarIssue> issues, GrammarCheckError error = GrammarCheckError.None, string errorMessage = "")
        {
            Issues = issues ?? Array.Empty<GrammarIssue>();
            Error = error;
            ErrorMessage = errorMessage ?? "";
        }

        /// <summary>Issues in text order. If <see cref="Error"/> is set, only for the part checked.</summary>
        public IReadOnlyList<GrammarIssue> Issues { get; }
        public GrammarCheckError Error { get; }
        /// <summary>A sentence for the user saying what went wrong (empty if nothing did).</summary>
        public string ErrorMessage { get; }
        public bool Complete => Error == GrammarCheckError.None;
    }
}
