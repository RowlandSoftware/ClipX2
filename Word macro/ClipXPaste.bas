Attribute VB_Name = "ClipXPaste"
' ClipX: paste back into Word with the text's own styles.
'
' Word's Ctrl+V takes ClipX's HTML and, with its "Pasting from other programs" setting (Merge
' Formatting, or Keep Source Formatting when the document already has the styles), turns every
' paragraph into Normal plus direct formatting. This macro pastes ClipX's RTF instead, as Paste
' Special > Formatted Text (RTF) does, but from VBA, which isn't subject to that setting:
'   - paragraphs and words get their styles back (Body Text First Para, Emphasis, French...);
'   - styles the document already has are used as they are, never redefined;
'   - styles it lacks are created; Word's own headings stay Word's Heading 1, 2, 3...;
'   - only real edits (underline, an added bold) stay direct formatting.
' Without RTF on the clipboard (text copied from somewhere else) it does an ordinary paste.
'
' Installing (once):
'   1. In Word, Alt+F11 opens the VBA editor.
'   2. In the Project pane, click Normal; then File > Import File... and choose this file.
'      (Or Insert > Module and paste in everything below the Attribute line.)
'   3. Close the editor. The macro is in Normal.dotm, so every document has it.
'   4. A button: File > Options > Quick Access Toolbar, "Choose commands from: Macros",
'      Normal.ClipXPaste.PasteWithStyles, Add, then Modify to give it an icon and a name.
'      A key: File > Options > Customize Ribbon > Keyboard shortcuts: Customize..., category
'      Macros, PasteWithStyles, and press the keys (e.g. Ctrl+Shift+V).

Option Explicit

Public Sub PasteWithStyles()
    On Error GoTo NoRtf
    Selection.PasteSpecial DataType:=wdPasteRTF
    Exit Sub
NoRtf:
    On Error GoTo 0
    Selection.Paste
End Sub
