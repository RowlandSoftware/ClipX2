using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClipX2.UI
{
    /// <summary>
    /// Minimal interop for the RichEdit Text Object Model (TOM, tom.h). TOM lets us edit ranges of
    /// text directly, without moving the selection, and step through the text one formatting run
    /// at a time. Only the start of each interface is declared: COM calls go by position in the
    /// vtable, so members must stay in tom.h order and nothing after the last one may be called.
    /// </summary>
    internal static class RichEditTom
    {
        // Units for Move/MoveEnd/Expand
        public const int tomCharacter = 1;
        public const int tomCharFormat = 13;

        private const int WM_USER = 0x0400;
        private const int EM_GETOLEINTERFACE = WM_USER + 60;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, out IntPtr lParam);

        /// <summary>Returns the TOM document for a RichTextBox, or null if it is not available.</summary>
        public static ITextDocument GetDocument(RichTextBox rtb)
        {
            if (SendMessage(rtb.Handle, EM_GETOLEINTERFACE, IntPtr.Zero, out IntPtr unk) == IntPtr.Zero || unk == IntPtr.Zero)
                return null;
            try
            {
                // The cast does a COM QueryInterface from IRichEditOle to ITextDocument
                return Marshal.GetObjectForIUnknown(unk) as ITextDocument;
            }
            finally
            {
                Marshal.Release(unk);
            }
        }
    }

    [ComImport, Guid("8CC497C0-A1DF-11CE-8098-00AA0047BE5D"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    internal interface ITextDocument
    {
        string Name { [return: MarshalAs(UnmanagedType.BStr)] get; }
        ITextRange Selection { get; }
        int StoryCount { get; }
        object StoryRanges { [return: MarshalAs(UnmanagedType.Interface)] get; }
        int Saved { get; set; }
        float DefaultTabStop { get; set; }
        void New();
        void Open([MarshalAs(UnmanagedType.Struct)] ref object pVar, int Flags, int CodePage);
        void Save([MarshalAs(UnmanagedType.Struct)] ref object pVar, int Flags, int CodePage);
        int Freeze();
        int Unfreeze();
        void BeginEditCollection();
        void EndEditCollection();
        int Undo(int Count);
        int Redo(int Count);
        ITextRange Range(int cpActive, int cpAnchor);
    }

    [ComImport, Guid("8CC497C2-A1DF-11CE-8098-00AA0047BE5D"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    internal interface ITextRange
    {
        string Text { [return: MarshalAs(UnmanagedType.BStr)] get; [param: MarshalAs(UnmanagedType.BStr)] set; }
        int Char { get; set; }
        ITextRange Duplicate { get; }
        ITextRange FormattedText { get; set; }
        int Start { get; set; }
        int End { get; set; }
        object Font { [return: MarshalAs(UnmanagedType.Interface)] get; [param: MarshalAs(UnmanagedType.Interface)] set; }
        object Para { [return: MarshalAs(UnmanagedType.Interface)] get; [param: MarshalAs(UnmanagedType.Interface)] set; }
        int StoryLength { get; }
        int StoryType { get; }
        void Collapse(int bStart);
        int Expand(int Unit);
        int GetIndex(int Unit);
        void SetIndex(int Unit, int Index, int Extend);
        void SetRange(int cpAnchor, int cpActive);
        int InRange(ITextRange pRange);
        int InStory(ITextRange pRange);
        int IsEqual(ITextRange pRange);
        void Select();
        int StartOf(int Unit, int Extend);
        int EndOf(int Unit, int Extend);
        int Move(int Unit, int Count);
        int MoveStart(int Unit, int Count);
        int MoveEnd(int Unit, int Count);
    }
}
