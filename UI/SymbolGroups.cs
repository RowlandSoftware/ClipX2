using System.Collections.Generic;
using System.Linq;

namespace ClipX2.UI
{
    /// <summary>
    /// The character picker's groups, in plain words: Arrows, Shapes, Symbols and Icons, Emoji.
    /// Letters, numbers, punctuation and other scripts aren't offered: they can be typed. Each
    /// group gathers a few Unicode blocks (Symbols and Icons: Miscellaneous Symbols, Dingbats,
    /// Miscellaneous Technical, currency, maths, the Private Use Area where icon fonts and symbol
    /// fonts keep theirs…; Emoji: the pictographs beyond U+FFFF).
    /// </summary>
    internal static class SymbolGroups
    {
        public const string Arrows = "Arrows", Shapes = "Shapes", Symbols = "Symbols and Icons", Emoji = "Emoji";

        /// <summary>The groups, in the order offered.</summary>
        public static readonly string[] Order = { Arrows, Shapes, Symbols, Emoji };

        private static readonly (int Start, int End, string Group)[] Ranges =
        {
            (0x20A0, 0x20CF, Symbols),   // Currency Symbols
            (0x2100, 0x214F, Symbols),   // Letterlike Symbols (℃ № ™)
            (0x2190, 0x21FF, Arrows),    // Arrows
            (0x2200, 0x22FF, Symbols),   // Mathematical Operators
            (0x2300, 0x23FF, Symbols),   // Miscellaneous Technical (⌚ ⌨)
            (0x2400, 0x245F, Symbols),   // Control Pictures, OCR
            (0x2460, 0x24FF, Symbols),   // Enclosed Alphanumerics (① Ⓐ)
            (0x2500, 0x25FF, Shapes),    // Box Drawing, Block Elements, Geometric Shapes
            (0x2600, 0x26FF, Symbols),   // Miscellaneous Symbols (☀ ☂ ♠)
            (0x2700, 0x27BF, Symbols),   // Dingbats (✂ ✓ ✈)
            (0x27C0, 0x27EF, Symbols),   // Miscellaneous Mathematical Symbols-A
            (0x27F0, 0x27FF, Arrows),    // Supplemental Arrows-A
            (0x2900, 0x297F, Arrows),    // Supplemental Arrows-B
            (0x2980, 0x2AFF, Symbols),   // more mathematical symbols
            (0x2B00, 0x2B4F, Arrows),    // Miscellaneous Symbols and Arrows: the arrows
            (0x2B50, 0x2B5F, Shapes),    //   its stars and circles
            (0x2B60, 0x2BB9, Arrows),    //   more arrows
            (0x2BBA, 0x2BFF, Shapes),    //   more shapes
            (0xE000, 0xF8FF, Symbols),   // Private Use Area: icon fonts, symbol fonts (Wingdings)
            (0x1F000, 0x1F0FF, Symbols), // Mahjong, Domino, Playing Cards
            (0x1F100, 0x1F1E5, Symbols), // Enclosed Alphanumeric Supplement (not the regional indicators, flag halves)
            (0x1F300, 0x1F3FA, Emoji),   // Miscellaneous Symbols and Pictographs (not the skin-tone modifiers)
            (0x1F400, 0x1F5FF, Emoji),
            (0x1F600, 0x1F64F, Emoji),   // Emoticons
            (0x1F650, 0x1F67F, Symbols), // Ornamental Dingbats
            (0x1F680, 0x1F6FF, Emoji),   // Transport and Map Symbols
            (0x1F700, 0x1F77F, Symbols), // Alchemical Symbols
            (0x1F780, 0x1F7FF, Shapes),  // Geometric Shapes Extended
            (0x1F800, 0x1F8FF, Arrows),  // Supplemental Arrows-C
            (0x1F900, 0x1F9FF, Emoji),   // Supplemental Symbols and Pictographs
            (0x1FA00, 0x1FA6F, Symbols), // Chess Symbols
            (0x1FA70, 0x1FAFF, Emoji),   // Symbols and Pictographs Extended-A
        };

        /// <summary>The group a character is in, or null (letters, punctuation, other scripts: not offered).</summary>
        public static string GroupOf(int codePoint)
        {
            foreach (var (start, end, group) in Ranges)
                if (codePoint >= start && codePoint <= end) return group;
            return null;
        }

        /// <summary>A font's characters by group, in the order offered; groups it has none of left out.</summary>
        public static List<(string Name, List<int> Chars)> GroupsOf(IEnumerable<int> chars)
        {
            var byGroup = chars.GroupBy(GroupOf).Where(g => g.Key != null).ToDictionary(g => g.Key, g => g.ToList());
            return Order.Where(byGroup.ContainsKey).Select(name => (name, byGroup[name])).ToList();
        }
    }
}
