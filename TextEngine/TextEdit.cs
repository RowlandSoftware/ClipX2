namespace ClipX2.TextEngine
{
    /// <summary>
    /// One change to a text: replace <see cref="Length"/> characters at <see cref="Start"/> with
    /// <see cref="Replacement"/> (Length 0 = insert, empty Replacement = delete). Tools that change
    /// the length of the text return lists of these, so an editor can apply them in place and keep
    /// the formatting of everything it doesn't touch.
    /// </summary>
    public readonly struct TextEdit
    {
        public TextEdit(int start, int length, string replacement)
        {
            Start = start;
            Length = length;
            Replacement = replacement ?? "";
        }

        /// <summary>Position in the original text.</summary>
        public int Start { get; }
        public int Length { get; }
        public string Replacement { get; }

        public override string ToString() => $"@{Start} -{Length} +\"{Replacement}\"";
    }
}
