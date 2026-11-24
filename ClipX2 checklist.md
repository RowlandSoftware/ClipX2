# ClipX2: project checklist

Items for later that cover the whole program, not one tool. Tool-specific lists are separate
(e.g. `Spelling UI checklist.md`).

## Text for translation

- [ ] Move every tool's name (now the button's Text) and description (now its ToolTipText) into
  string resource files, one per language (`Strings.resx`, `Strings.fr.resx`, `Strings.de.resx`…:
  separate files are easiest to hand to translators). Look them up by the tool's Tag (ClipX 1's
  token, e.g. `ccUPP`) and the current language: `ccUPP.Name`, `ccUPP.Description`. The hover help
  (`SidePanelHost`) and PowerButtons then read from there. Do this once the tooltip texts are
  written; until then keep entering them in the designer
- [ ] Other UI text (panel labels, messages) to the same files
- [ ] Tags so far: ccUPP ccLOW ccREV ccINC ccTIT ccSEN ccCAP chSPE chPNC (from ClipX 1's
  `MainToolHandler`). Untagged: toolStripButton3–7 (placeholders), Done

## Tools and options

- [ ] Tools with options show them in the side panel; **right-click on a tool skips the options and
  runs it with the last-used settings** (as ClipX 1)
- [ ] Each tool's last-used options are kept in its own section of `AppSettings`
- [x] PowerButtons (customisable buttons combining several tools, by tag): see PowerButtons below

## Settings

- [x] `UI/AppSettings.cs`: a JSON file, `settings.json`, in the per-user application-data folder
  (`%AppData%\ClipX2`; `~/.config/ClipX2` on Linux/macOS). Replaces ClipX 1's registry use. Kept
  so far: spelling language, Smart replace (and the spelling Ignore… options, not yet shown in the UI)
- [ ] Settings/Options dialog
- [ ] Download dictionaries (placeholder now: "More languages…" in the spelling panel explains how to
  add Hunspell files by hand to `%AppData%\ClipX2\Dictionaries`, and opens that folder)
- [x] The empty VS `Properties\Settings.settings` is unused; remove it when convenient

## International

- [ ] After ClipX 1 parity: make Check Punctuation's rules settings per language (French space before
  ! ? ; :, national quotation marks, Spanish ¡ ¿ pairing, etc.)
  
## Conversion
  
- [x] Startup: Plain or Formatted always offered (the HTML-as-code choice hidden); what isn't on the clipboard is converted from what is: formatted from RTF, else HTML (formatting, headings, lists, tables, indents kept), else plain text (typed lists made live, leading tabs paragraph indents); plain from the clipboard's own text, else HTML or RTF converted (lists typed)
- [x] Toggle: rich → plain keeps lists (typed) and indents (tabs), with a warning ("Do not show this warning again"; Options, General: Hide warning about loss of formatting); plain → rich makes typed lists live, tabs paragraph indents
- [x] Refresh (clipxREF; ClipX 1's cmdRefresh): ↻ left of Finish; the clipboard again as at startup (format dialog, conversions), one Undo step; "Text retrieved from clipboard"
- [x] Plain-only clipboard text that is HTML code (a text editor) is taken as HTML: HTML (code) shows it, Formatted and Plain render it. Plain from HTML is converted (list numbers typed)
- [x] Done: rich mode, plain text with typed lists and tabs for indents; plain mode, RTF (live lists) and HTML (real lists) made from the recognised plain text as well as the text
- [x] Engine: Import/PlainTextReader, Import/HtmlReader, Import/TypedListRecognition (Word's typed "1." and "·" too); UI: RichEditWriter (document into the editor)
- [ ] Rich text's font for converted plain text: the proportional display font (Options, Display); a separate setting?

### Word-processor round trip (styles)

- [x] Writer test (22 Nov 2026): RTF naming styles only (\s, \cs, no fonts) pasted into Sample.odt with Ctrl+V → Writer's own paragraph and character styles applied (French's language too), no duplicates, survives Ctrl+M
- [x] Import: RTF with named styles (LibreOffice, Word) read by ClipX (Import/RtfReader, RtfStyleSheet), not RichEdit: styles resolved through \sbasedon (inherited fonts: title in Malgun Gothic, not Garamond), style-generated numbers on empty paragraphs (Separator) left out, real lists live, spacing and language kept; comments, bookmarks, field codes, pictures, headers/footers left out. Other RTF as before
- [x] Style sheet kept with the text (ClipXRichTextBox.SourceStyles); cleared on switching to plain text and on loading other text
- [x] Done: each paragraph and run given the style it now looks like (Export/StyleMatcher: italics added in ClipX → Emphasis; italic + French → French); RTF (Export/RtfExport) with the source's style sheet, font and colour tables, \s / \cs, direct formatting only where the text differs from its style; HTML with &lt;em&gt; / &lt;strong&gt; for Emphasis / Strong
- [x] Andrew's first test: styles came back, but most text was overridden by direct formatting (Garamond 12, 0 spacing, 100% line spacing) until Ctrl+M; bold-italic became Emphasis + direct bold; Body Text First Para a child of Default in a new document; Clear formatting in ClipX gave Segoe UI
- [x] Fixed: Writer takes anything an RTF paragraph doesn't repeat from its style as direct formatting, and dedupes only against the style's own definition. Now each style's definition is flattened (its parents' settings written in, parent still named, parents first) and each paragraph / run repeats it after \s / \cs, as Writer's own RTF does. Checked with headless LibreOffice (pasting into a copy of the document, and opening as a new one): direct formatting left only for real edits (underline, strike, bold over Emphasis) and line spacing (Writer always makes that direct, even from its own clipboard; the value is the style's, so Ctrl+M changes nothing visible)
- [x] Line spacing read, shown in ClipX (125%) and written back
- [x] Style-generated numbering (Separator's I, II, III on an empty paragraph): shown in ClipX as a live number (the number it had), and on Finish left to the style again (no \ls), so the word processor numbers it and carries on the document's numbering. Checked pasting into a copy of Sample.odt: numbered by RomanOnly, outline level 2, no direct list. In a new document the number is lost: Writer's RTF doesn't say that a style is numbered
- [x] Clear formatting (button and Ctrl+M) on text from a word processor: back to each paragraph's style (Garamond 12), not the editor's font
- [x] Andrew's second test (Sample.odt): styles kept, a bold-italic character style used for bold + italic and survived Ctrl+M; only underline and strike (direct) went. Separator showed no number in ClipX (fixed since)
- [x] Heading level set in ClipX → on Finish, a style with that outline level (the level first, then the look; "Heading N" wins unless another style at that level looks nearer by 2+). Was the nearest look: Heading 4 became Subhead, or Body Text + Strong. Checked: a Body Text paragraph set to Heading 4 → Heading 4 in Sample.odt's copy; other paragraphs' styles unchanged
- [ ] ClipX shows its own Heading N look, not the source's: show the source style's look when a level is set?
- [ ] Separator's outline level: Sample.odt gives it outline level 2, but Writer's RTF doesn't say so (no \outlinelevel), so ClipX shows "Normal"
- [ ] Style box shows only heading levels (the Separator shows "Normal"): show the source's style names instead?
- [x] Word (22 Nov 2026, The Picnic.docx, tested with hidden Word: copy, then paste into a copy of the document): all styles, Separator's I (outline level 2), Emphasis / Strong Emphasis / French read. Word writes complete style definitions (left out = off, not inherited): recognised by \styrsid / \spriority, no inheritance then (Body Text First Para no indent, Scene no space after). Word's last paragraph ends with \par, so Finish does too (without it Word merged the last paragraph into the one pasted into: Normal). Pasted back, ClipX's RTF had less direct formatting than Word's own: only real edits (underline, strike, bold over Emphasis)
- [x] User's tests (22 Nov 2026): Word → ClipX → Writer: styles used; in a new document Writer creates the missing ones ("Strong Emphasis (WW)", Body Text First Para under "Text Body (WW)"); in Sample.odt as good as from Writer. Writer → ClipX → Word, and Word → ClipX → Word: styles used only when the document already has them; in a new document everything is Normal + direct formatting. Lists fine. (Later found: Word's ordinary paste takes the HTML, not the RTF; and the user's Paste Special → RTF, unlike hidden Word's, set Normal everywhere: probably the "Pasting from other programs: Merge Formatting" option. Clear All Formatting also sets Normal, so it can't be used to check.)
- [x] HTML names the styles (23 Nov 2026, Export/DocumentExport.Styles): when the text came with named styles, a style sheet in the HTML's head defines each style used (and its parents) as Word's own HTML does (p.BodyTextFirstPara {mso-style-name:"Body Text First Para"; mso-style-parent:…}, span.French for character styles), paragraphs and runs name them with a class, and inline formatting only says how they differ. Word's ordinary paste (Keep Source Formatting) then applies the styles, creating those the document lacks. hN only for "heading N" styles (Word makes any hN its Heading N), so the Separator is a p; a heading level with no style of its own is a plain hN (Word's built-in Heading N). Tested in hidden Word, Word's and Writer's text, new document and The Picnic: styles applied; direct formatting only real edits, fonts on Heading 1/3 in a new document (Word doesn't redefine its own headings), "Kate"'s shading. User confirmed the hand-made test in Word; Writer takes ClipX's RTF, so this doesn't affect it
- [x] Word's HTML paste rules (23 Nov 2026, hidden Word with the ribbon's Paste and PasteAndFormat, which behave like the user's Word; COM's plain Paste doesn't): Merge Formatting (the user's default for other programs) drops all styles. Keep Source Formatting keeps them only if (a) there's no h1–h6 anywhere (so headings are now p class=heading1; a level with no style of its own is p class=heading4 named "heading 4"), and (b) no pasted style already exists in the document, built-in ones included ("Body Text" fails in a new document, everything fails in The Picnic). One failure drops the styles of every paragraph. Use Destination Styles (PasteAndFormat 19) works in every case: Word's and Writer's text, new document and The Picnic, direct formatting only for real edits
- [x] Word's paste pop-up for ClipX's HTML offers only Keep Source Formatting, Merge Formatting and Keep Text Only (no Use Destination Styles). Decision (user, 23 Nov 2026): keep the source's names as they are ("heading 1" in lower case, as Word's RTF gives it). "Heading 1" would clash with Word's own (Heading 3–9 exist hidden in every document), and one clash makes Keep Source drop every style. Never overwrite existing styles; duplicates are the lesser evil
- [x] Pasting back into the source document (its styles already exist): Keep Source gives Normal + direct formatting. Word macro (Word macro/ClipXPaste.bas, PasteWithStyles, install notes in the file): Selection.PasteSpecial wdPasteRTF from VBA, which, unlike the window's Paste Special, isn't subject to the paste setting. Hidden Word, ClipX's full clipboard: real Heading 1/3, every style applied, The Picnic 432 styles before and after (Use Destination Styles with the HTML added 3 duplicate "heading N")
- [x] User tried the macro in Word (23 Nov 2026): works
- [x] Word → ClipX → Writer made "Text body (WW)" and "Strong Emphasis (WW)": Writer reads RTF styles by its RTF names ("Body Text", "Strong"), and "Text body" (its internal name, kept by a Word document that began in Writer) clashes. Now ClipX also puts a Writer version of its RTF on the clipboard as "Richtext Format" (LibreOffice's own RTF format, which Writer takes first and Word ignores): names translated (Export/WriterStyleNames, taken from Writer 26.8's own RTF export of every built-in style), no final \par. Headless Writer, ClipX's real clipboard: new document and Sample.odt use Body Text, Strong, no (WW) styles, Body Text First Para based on Body Text, no extra paragraph; Writer → ClipX → Writer unchanged; Word (macro) unchanged. User confirmed in Writer: Body Text used
- [ ] Test the styled HTML from the app in Word: new document and The Picnic
- [ ] Help / tips for Word users: keep Word open until ClipX has read the text (closing Word drops its RTF from the clipboard); paste with Keep Source Formatting (the option can be made the default); styles come through even in a new document
- [ ] Word: character shading (\chcbpat, e.g. "Kate") comes back as a highlight
- [ ] Heading level with no style at that level in the source (Word's RTF has only the styles used): Heading 4 became Text body + Strong Emphasis + outline level. Name Word's / Writer's built-in "heading N" anyway? (The HTML now does: a plain hN.) Do the same in the RTF?
- [ ] Word RTF with tables or pictures: ClipX's reader turns tables into tabs and leaves pictures out (RichEdit kept them). Use RichEdit's reader when there are tables/pictures?
- [ ] Comments (Writer's annotations) and bookmarks: lost; carry them?
- [ ] Does the code for export to RTF & HTML do specific fixes e.g. to 'Body Text' because of testing with The Picnic?

## Grammar (LanguageTool)

- [x] Engine client `TextEngine/Grammar/LanguageToolChecker`: free service, Premium (email + API key) or own server, same request; a paragraph at a time from the caret, results kept per paragraph (re-check sends only changed ones), paced to the free limits; errors as messages (no connection, busy, key refused, language not supported). Settings: `GrammarSettings` in settings.json
- [x] First use: ask once before any text is sent ("Grammar checking sends your text to LanguageTool's server") — `ConsentGiven`
- [x] Spelling panel: grammar issues in the same pass, in text order; LanguageTool's message in place of "Not in dictionary"; Ignore rule (saved to `DisabledRules`); rule explanation in the definitions box; credit "Grammar checking by LanguageTool" linked to languagetool.org
- [x] Second opinion on spelling (`SecondOpinionOnSpelling`): show a Hunspell-flagged word only if LanguageTool flags it too (`GrammarIssue.IsSpelling`); Hunspell alone when offline
- [x] Options: service choice; Premium key with Test button and a link to the access-token page; key stored encrypted (DPAPI, `SecretStore`), not in plain text; picky level
- [x] Turning grammar back on after "No" at the consent question: Options, Grammar page (ticking it there counts as consent)
- [ ] Grammar: Change All for grammar issues? Undo of Ignore Rule (a list of ignored rules in Options)
- [ ] Grammar re-check of a paragraph after the user's own edits in the editor (now: re-sent at the next F7, only changed paragraphs)
- [x] -ise / -ize: `Spelling.OxfordIze` (off): LanguageTool's Oxford -ize suggestion (OXFORD_SPELLING_Z_NOT_S) hidden unless on; Hunspell side = the dictionary (installed en_GB is -ise only; an en_GB-ize can be added). LanguageTool code drops suffixes (en_GB-ize → en-GB)
- [x] Options: -ise / -ize choice; ignored rules listed (by id) with "Switch back on"
- [ ] Ignored rules: show each rule's message or description, not just its id
- [ ] Ellipsis rule: no space before a trailing-off ellipsis, spaces round an omission — check LanguageTool's rule id when it next appears; possibly a ClipX punctuation rule (Check Punctuation, parametrised)

## Options dialog (ClipX 1's dlgPreferences)

- [x] `UI/OptionsDialog`: Display (proportional and fixed-width fonts for plain text, word wrap), General (export/import settings.json), Spelling (dictionaries: supplied, yours, downloadable from wooorm/dictionaries; the checker's options), Grammar (LanguageTool service, key, URL, test, ignored rules). Options button beside Done: click = dialog, right-click = switch fonts (as ClipX 1)
- [ ] Options button image (text for now)
- [ ] Not carried over: Extensions page (file extensions, TLD list), Updates page, tips at startup, sounds, Help button
- [x] Dictionary downloads from LibreOffice's dictionaries (GitHub mirror; one tree request lists all, git hashes show where an installed copy differs); Find box; options grouped under "Options"
- [ ] Dictionary downloads: check a downloaded dictionary loads before listing it; perhaps an -ize English (SCOWL) as a second source
- [ ] Import/Export - ClipX 1 import compatibility; import from registry on first use (?)

## Lists

- [x] Bullets (liBUL) and Numbers (liNUM): options in the side panel (Apply), right-click = last used. Settings `BulletList` and `NumList` with ClipX 1's key names (Character, Tab, PadChars; Style, StartNumber, Renumber, FollowChar, Tab, PadChars)
- [x] Plain text: typed, as ClipX 1; levels from indentation (a tab, or one pad width of spaces); Legal 1. / 1.1. / 1.1.1.; bullets keep levels; "pad to" width for bullets too (at least one space)
- [x] Rich text: live lists (TOM): levels from indentation or an existing list, numbers 1 → a → i → I → A down the levels, one Undo step; Legal and bullets other than • are typed (RichEdit can't do them live)
- [x] Rich → plain: live numbers and bullets written in as text (tab after); Done: plain text and HTML already handled
- [x] Remove bullets (liRMB), Remove numbers (liREM): typed and live; indentation kept (live levels turned back into tabs / pad widths; lines moved along by 1/a/i numbering moved back), so add then remove gives the original text; click or right-click
- [x] Line numbers (liREN): options in the side panel (start, increment, show every nth, skip blank lines, pad right/left, renumber), right-click = last used; setting `NumLines` with ClipX 1's key names. Remove line numbers (liRLN): click or right-click; indentation after the number kept, so add then remove gives the original. Typed in rich text too (formatting kept); numbers line up only in a fixed-width font
- [x] Live numbered lists: blank lines between items become unnumbered items, so the numbering runs on (as plain text); exports keep them as blank lines
- [ ] Typed lists in rich text: hanging indent so wrapped lines line up
- [ ] Settings: import of ClipX 1's .pref (XML) — keys already match for lists

## Format
- [x] Trim white space, Trim trailing space, Single space, Tabs to spaces (columns of 8), Spaces to tabs, Paragraph: act at once (click or right-click)
- [x] Indent / Unindent (tab, spaces or text such as "> "), Word wrap (width, hanging indent, justify; plain text only), Remove extra lines: options in the side panel, right-click = last used; settings Indent, Wordwrap, RemLines (ClipX 1's names)
- [x] All work on whole lines (selection widened) or the whole text; edits in place, so rich text keeps its formatting; one Undo step
- [ ] Rich text: indenting by paragraph indents rather than typed tabs/spaces
- [ ] Tab key: RichTextBox doesn't accept it (AcceptsTab?); ClipX 1 used Ctrl+I for a tab, now italics; choose a key

## Quick search (bottom bar)
- [x] Ctrl+F to the box; Enter finds downwards from the cursor and selects (Shift+Enter upwards); ▲ ▼ previous/next; Match case, Whole words; "Not found", "Reached end of text", "Reached start of text" in the status label
- [x] Find all (a button): every match highlighted (the spelling marker, several at once), count in the status label, cursor unmoved. Esc otherwise back to the text with the match selected
- [x] Search history in the drop-down, latest first, 15 kept, saved in settings.json (`QuickSearchHistory`)
- [x] At the end/start: message and beep, selection unchanged; the next search that way goes round from the other end
- [x] F3 / Shift+F3 = next / previous match while Quick search is active (from Ctrl+F, the box or a search, until Esc, a click in the text or another tool); otherwise Shift+F3 cycles case. Find & Replace does the same while its panel is open
- [x] Find all's highlights go on Esc, another tool, a new search, Next/Previous match (arrows, F3), a click in the text

## Find & Replace (edREP, Ctrl+H or Alt+F; side panel)
- [x] One panel for Find and Replace (UI/Panels/FindReplacePanel). Ctrl+H or Alt+F opens it (inside the panel Alt+F stays From start's key); Ctrl+F stays Quick search, with its own history. In "What do you want to do?" (edREP). Stays open when other tools run, as the spelling panel does
- [x] Modes as option buttons: Simple, Wildcard, Regex (ClipX 1's three tabs). Engine: `TextFinder` (TextEngine/TextFinder.cs): one finder per mode behind Find (next/previous, within a range), FindAll, IsMatchAt, ReplacementFor, ReplaceAll (edits); so the buttons are written once for all modes
- [x] Simple mode (through TextSearch, as Quick search): Match case, Whole words, Smart replace (each replacement in the case of the match, the spelling panel's MatchCase), Special chars (\t \n \xHH \uHHHH \\; a bad code says why); engine tested (23 cases)
- [x] Previous / Next (Shift+F3 / F3, Shift+Enter / Enter in Find); at the end: message and beep, the next search goes round. From start: the next search starts at the start (Previous: the end), then unticks itself
- [x] Find All: every match highlighted and counted, the selection unmoved. Replace: replaces the selected match and goes to the next (the first press only finds); Enter in the Replace box. Replace All: one Undo step, formatting kept (ApplyEdits), count in the panel
- [x] In selection: Find All and Replace All keep to the selection; ticked when the panel opens on more than one line (a selection within a line fills Find instead); finding a match unticks it
- [x] Find and replace histories (15 each) and the options kept in settings.json (`FindHistory`, `ReplaceHistory`, `Search`); the swap button swaps the two texts
- [x] Help button: the help floats just right of the side panel, as the spelling panel's definitions (UI/Panels/HelpPopup: headings, term / meaning columns), and follows the mode (Simple and Wildcard written; Regex says what is to come)
- [x] The panel's messages (counts, Not found, Reached end, a bad regular expression) also go in the window's status bar (the user, 24 Nov 2026), so a small window can't hide them
- [x] Special chars ticked, Simple mode: a group of buttons, Tab \t, New line \n, \\, Hex \x.., Unicode \u.... (two rows, so all show), put the code in the Find or Replace box (the last one typed in), at the cursor. Not in Wildcard mode (the user, 24 Nov 2026: no room; the codes still work and its help lists them)
- [ ] User to try it in ClipX (layout, the help beside the panel, all three modes: not yet seen on screen)
- [x] Wildcard mode (24 Nov 2026): `TextFinder.WildcardToRegex` (ClipX 1's translateWildcardsToRegExp and its help sheet WildCardHelp2) to a .NET Regex, through a RegexFinder shared with Regex mode later (multiline, case as Match case, 3 s time limit, empty matches skipped). ? or . * + & ! % ~ # @ [..] [^..], with * or + after each; \ before a wildcard character finds it. Every wildcard (with its * or +) is a group: $1, $2… and $& in the replacement, $$ a $. All matching is not greedy (lazy). Nothing runs on over a paragraph break (~ is white space within a line). Options: Find at (anywhere, start/end of line, start/end of word), # decimal point and thousands separator ("," "." space ' none; the culture's by default; space = any space, no-break too). Engine tested (78 cases with Simple's)
- [x] ClipX 1's wildcard bugs fixed: &+ and !+ gave one character; &* %* %+ ~* ~+ were greedy while * + were lazy; start/end of line only worked at the start/end of the whole text (no multiline); Whole words with \b failed at a pattern beginning or ending with a space or punctuation (now only where the match begins or ends with a word character, as Simple); ~ (\s) ran on over paragraph breaks; # took 12,34 as one number (now groups of three) and missed .5; @ didn't take ’ (isn’t)
- [x] Not greedy, except the last wildcard of all when it has * or + (the user, 24 Nov 2026): that one is greedy, so c%+ finds c123, ~+ a whole run of spaces, * alone a whole paragraph; <*> still finds each tag. A pattern ending in plain text keeps every * and + lazy
- [ ] Wildcard: a selection within a line fills Find with its wildcard characters escaped (\? \*), so it is found as it is; check in use
- [x] Regex mode (24 Nov 2026): the user's .NET regular expression through the same RegexFinder as wildcards: multiline always (^ $ at every paragraph), Match case (off = IgnoreCase), Whole words round it as for wildcards, Smart replace, 3 s time limit (catastrophic backtracking stopped with a message). Replacement: $1 ${name} $& $$, and \t \n \xHH \uHHHH \\ always (Special chars is off in Regex mode: a regex has its own). Find All / Next skip empty matches; Replace All keeps them, so ^ → "> " quotes every line and $ adds to every line end (in a selection, not the line after it). The status line says while typing what is wrong ("Not enough )'s (at character 2)"). A selection fills Find escaped (Regex.Escape). Help: characters, quantifiers, anchors, look-arounds, groups, replacement, options, and what is new compared with ClipX 1's VBA RegExp. Engine tested (102 cases, all modes)
- [x] Case changes in the replacement, Wildcard and Regex modes (24 Nov 2026, TextEngine RegexReplacement): \U capitals from here, \L small letters, \E as found (\U$1\E $2; \U$1\L$2 title case), in the current culture's case rules; \\U a backslash and a U. With them in it, Smart replace leaves the case alone. \U is no longer a Unicode code (\u is)
- [x] Bug (the user): Wildcard without Special chars, \t found every t. Now a backslash before anything but a wildcard character is found as itself (\t = a backslash and a t), and the replacement's \t stays as typed. Engine tested (115 cases, all modes)
- [x] Wildcard options made compact (the user, 24 Nov 2026): Find at (Anywhere, Start/End of line, Start/End of word) and "# numbers like" (1,234.5 · 1.234,5 · 1 234,5 · 1 234.5 · 1'234.5 · 1234.5 · 1234,5: decimal point and thousands separator together) as two drop-downs, in place of five option buttons and two separator lists
- [x] \1 … \9 (and \0, the whole match) in the replacement, Regex mode only, as $1 … $9 (as Notepad++ and sed); \\1 a backslash and a 1. Not in Wildcard mode, which keeps to $1 (the user, 24 Nov 2026: no more confusion there)
- [x] Ctrl+Alt+Shift+R in Wildcard mode (hidden, as ClipX 1; in the Wildcard help): Regex mode, with ClipX's regular expression in Find (Whole words and Find at included, so Whole words is unticked) and the replacement as Regex mode reads it in Replace (backslashes doubled when Special chars was off, \U \L \E kept). TextFinder.WildcardAsRegex; tested that the regex gives the same result as the wildcards (13 new engine tests, all passing)
- [ ] Perhaps: a list of Find All's results (ClipX 1's Search Results); a toolbar button for edREP
- [x] Find and Replace in PowerButtons with its own texts and options (Replace all, Replace the next one, Find the next one, Find all; From start, Backwards, In selection): `FindReplaceTask`, run by `FindReplaceRunner` without the panel

## Edit
- [x] Decode URL (edUDC), HTML (edHDC), UTF-8 (edUTF), MIME (edMIM): selection exactly or whole text; bytes as UTF-8 where valid, else Windows-1252; all named HTML entities; MIME soft line breaks and encoded words (=?utf-8?Q?...?=)
- [x] Plain characters (edPLA: – → --, — → ---, so it reverses Curly quotes), Curly quotes etc. (edCRL: quotes by context, apostrophes, feet/inches left straight, -- – , --- —, ... …), Regularise line ends (edEOL: soft line breaks, U+2028/2029, NEL, form feeds)
- [x] Strip email quote marks (edSTR): > and | at line starts, any depth, spaces before them; only the one space after each mark goes, so indentation inside a quote is kept; whole lines; engine: LineFormatter.StripQuoteMarks
- [x] Select word, Ctrl+W in the editor (engine: WordSelection; 21 cases tested). Word = letters and digits (combining accents included); other = not word, not white space. No selection: the word at the caret, or just before it at a word's end (Esme has| → has), else the run of other characters (-|-> → -->), never white space; between spaces: the next word on the line. Selection: extends right over white space + the next word or run, or over a run of other characters, or the rest of a word. Never crosses a paragraph or line break. ’ is "other", so wasn’t is wasn + ’ + t
- [ ] User to try Ctrl+W in ClipX

## Statistics (chSTAT)
- [x] Side panel, kept up to date while open: Selection (only when there is one) above Whole text. Words, characters, without spaces, sentences, paragraphs, words per sentence, size (UTF-8 for plain text, RTF for rich text); lines left out (same as paragraphs)
- [x] Readability (English only, from the spelling language): Flesch Reading Ease with Flesch's level, Flesch–Kincaid grade, Gunning Fog. Syllables estimated by English rules; "rough under 100 words"
- [x] Readability for other languages: German (Amstad), French (Kandel–Moles), Spanish (Fernández Huerta), Italian (Flesch–Vacca), Dutch (Douma), Portuguese (Martins), Russian (Oborneva); syllables from LibreOffice's hyphenation patterns (`Hyphenator`, Liang), downloaded with the dictionary as name.hyph.dic (licence name.hyph.licence.txt); Options offers "Update" for an installed dictionary lacking them. Tested: German 38/40, Spanish 28/30, Italian 28/30, Dutch 30/30 words right. English keeps its own rules (89% against the patterns' 59%)
- [ ] Hyphenation for word wrap (patterns are there: `Hyphenator.Breaks(word, forPrint: true)`)

## Tool handling (ClipX 1's MainToolHandler)
- [x] Engine: `Tools/ToolCatalogue` (every tag, ClipX 1's names and descriptions, group, kind, options class), `Tools/TextTools.Run(tag, text, from, to, options)` (31 text tools: edits out), `Tools/ToolUsage` (counts in memory, `ToolUsed` event, `Report()`; never sends anything)
- [x] ClipX: one handler, `Form1.RunTool(tag, how, options)` (UI/Form1.Tools.cs): every tagged button wired once by tag (click = options panel, right-click = last settings), keys (F7, Shift+F3 = ccCYC, Ctrl+R = Repeat last tool), PowerButtons later (`ToolTrigger.PowerButton` with their own options). Counts in usage.json (Usage/Total, PowerButton counts apart), remembers the last tool, closes other options panels, ends Quick search, clears the status bar
- [ ] Usage upload (ask first, like grammar); a view of the statistics
- [x] PowerButtons: a list of tags + options run through RunTool (see PowerButtons below)
- [x] pb* extras (23 Nov 2026): Clear selection, Select all, Copy, Cut (rich text in rich-text mode, else plain), Paste (as Ctrl+V), Insert text, Delete selected text, include a PowerButton
- [x] Icons for the pb* extras (24 Nov 2026), which have no toolbar buttons: icons\pbCLR … pbPBT, an SVG (48 × 48 grid, Inkscape-editable) and its 48-pixel PNG each, in Properties\Resources.resx as the other icons. Form1.ToolIcon takes a toolbar button's image, else the resource named after the tag, so the PB editor's palette and timeline show them. To change one: edit the SVG and export the PNG (or replace the PNG) and rebuild

## Sorting
- [x] Alphabetic (soALP), Numeric (soNUM), Smart (soSMA, new), Reverse (soREV), Nudge up/down (soNUP/soNUD; Alt+Up/Alt+Down in the text), List sort (soKEY: comma separated items on a line). Lines move whole with their own formatting (TOM FormattedText), one Undo step; the selected lines, or the whole text; blank lines removed when sorting (as ClipX 1). Engine: `LineSorter`; sorts ignore case and indentation, use the language's alphabet (é with e), keep equal lines in order
- [ ] Smart sort: check the order against the Oracle Style Guide (now: symbols, then numbers, then letters; numbers inside lines by value, item 2 before item 10)

## Styles (new in ClipX2)
- [x] Font list (installed fonts, type to find), size list (or type a size), Bold, Italic: rich text only (off in plain text); through TOM, so a mixed selection keeps its other formatting (bold on mixed fonts keeps each font). Controls follow the selection (blank where mixed). Ctrl+B / Ctrl+I / Ctrl+U go through the tool handler too (stBLD, stITA, stUND); Ctrl+R repeats the last font or size
- [x] Increase / decrease indent (stIIN, stDIN): rich text, each paragraph's left indent ±36 pt (relative indents kept, not past the margin); plain text, a tab (or spaces, from the Indent settings) typed in or taken off
- [ ] Icons for the Styles buttons (text and drawn stand-ins for now)
- [x] Small two-row toolbar beside the font lists (table layout, 22 px buttons a pixel apart, 2 px under the bottom row: 50 high in the 58 tab page): B, I, U, S, x², x₂, Clear formatting (selection or whole text; lists and indents stay), Font colour ▾ (Word's standard colours, More colours…), align left/centre/right/justify (advanced typography on, so justify shows), indent ⇥ ⇤, Highlight ▾. Split buttons: click = last colour. States shown at the caret. Tags stSTR stSUP stSUB stCLR stALL stALC stALR stALJ stCOL stHIL
- [x] Heading level list (stHDG), labelled, under font and size; Ctrl+M = Clear formatting (stCLR; with nothing selected, only what is typed next; the button clears the whole text); Styles buttons show the typing format at the caret: Normal, Heading 1–6. Sets the editor's built-in heading tag (RTF "heading N": Word's Heading styles) and the look (24/18/14/12/10/9 pt bold, as HTML); Normal takes them off. Shows the caret's level (blank when mixed). HTML h1–h6 in and out; plain text just the words. No custom styles: RichEdit drops them
- [x] Live list items: Increase/Decrease indent and Tab/Shift+Tab change the level (bullet • ◦ ▪, number 1 → a → i follow); other paragraphs move. Tab/Shift+Tab indent when at a line start (or in its leading white space) or over several lines, else a tab is typed. Plain text: the caret's line (or the selected lines), typed bullets • ◦ ▪ by level
- [x] Bullets by level, • ◦ ▪ (live: RichEdit takes a list type above 32 as the bullet character; typed too); any single bullet character can be live now. Live lists take levels from tabs and spaces (mixed), an existing list, or paragraph indents. Done: ◦ and ▪ exported (plain text, HTML circle/square)
- [x] Typed numbered lists: hierarchical, 1 → a → i → I → A from the chosen style (indented lines numbered, a sub-list restarting under each item; levels from the list's shallowest line; indentation kept, so Remove numbers gives the original back; Legal keeps its indentation too). Tab / Increase indent in plain text recognise the typed list (style, punctuation, start, padding from its first top-level number; Legal if any number is) and renumber it; one Undo step. Numbers without punctuation (a, iv + tab or two spaces) recognised. Live lists: an item moving level takes that level's start number (else the editor starts a new list)

## Pasting
- [ ] Should not embed files, but extract text if possible
- [ ] Vet pastes to block images etc.

## What do you want to do?
- [x] Right-hand tab: label and box on the left (Alt+Q goes to it), icons (Settings; later Help, About) docked right. Suggestions under the box as you type: name and description, best first; Up/Down, Enter (or click) runs it through RunTool as its button would, Esc closes
- [x] Settings and About small (16-pixel icons, whatever size they come in), one above the other, Help (?) full height beside them, as the Style tab's small buttons (the user, 24 Nov 2026; toolStrip1 as a table, Form1.InitAppButtons); the box takes the width left. Checked in an off-screen render. Icons pending (the user)
- [x] Engine: `Tools/ToolFinder` (names, button captions and tooltips, synonyms, descriptions, groups; prefixes, plurals, typos; filler words ignored; "get rid of"/"delete"/"strip" = remove; Remove… tools lower unless removing) and `Tools/ToolKeywords` (synonyms per tool, US spellings, examples such as %20, â€™)
- [x] App actions searchable: Settings, Done, Plain or rich text, Quick search, Refresh, Repeat, Cycle case (tags clipxOPT clipxFIN clipxMOD clipxQSR)
- [ ] Synonyms in other languages, with the translations
- [ ] Perhaps: note searches that find nothing (locally, or with usage statistics if the user agrees) to improve the synonyms
- [ ] Perhaps: suggest help topics as well as tools, once there is help

## PowerButtons (23 Nov 2026)
- [x] Model (UI/PowerButtons.cs): `PowerButton` (Id, Name, Icon, Tasks), `PowerTask` (Tag, Use = Own / LastUsed / Ask, Options as the tool's own options class, JSON by tag), `ButtonIcon` (64-pixel PNG, base64, plus an uncommitted text layer `IconText`), `FindReplaceTask`. In settings.json (`PowerButtons`, `PowerButtonColumns`), so Export/Import settings carries them. Included buttons (pbPBT) by Id, so moving buttons doesn't break them; loops refused (editor) and stopped (run)
- [x] Panel on the right (panel2, UI/PowerButtonBar.cs): 32-pixel square buttons in a column; drag the panel's left edge (or right-click beside the buttons: More/Fewer columns) for up to 6 columns, the text box gives the room. Click runs, right-click changes, drag moves; "+" makes a new one. Hover help in the side panel: name, Alt+n, the tasks
- [x] Keys: Alt+1…Alt+9 run the first nine, Shift+Alt+1…9 change them (the user, 23 Nov 2026; ClipX 1 had Alt+F1…F8)
- [x] Running (Form1.PowerButtons.cs): each task through `RunTool(tag, ToolTrigger.PowerButton, options)`; one Undo step for the whole run (`ClipXRichTextBox.BeginUndoGroup`: tools' own groups join it); stops at a tool ClipX2 hasn't got or a rich-text tool in plain text, and says which; Ctrl+R repeats the whole button (as ClipX 1); spelling pauses the run until its panel closes
- [x] Ask tasks (ClipX 1's "Prompt for settings"): the run stops, the tool's side panel opens (no note since the fixes below); its own Apply (Indent, Replace All…) carries the run on, no Continue button (the user, 23 Nov 2026); closing the panel stops the run. Views raise `SidePanelView.Applied`; the host's `ViewHidden`, `ShowBanner`
- [x] Editor (UI/PowerButtonEditor.cs, TaskTimeline.cs; code-only forms): name, icon, Delete PowerButton (whole button, confirmed); tools by group with their toolbar icons, dragged onto the timeline (or double-clicked); cards dragged to re-order, Delete or drag off to remove, Ctrl+Left/Right to move; the selected task's options below: the tool's own side panel in edit mode (`SidePanelView.EditOptions` / `EditedOptions`, its Apply buttons hidden), plus Find and Replace's "When the PowerButton runs", and simple controls for Insert text, include a PowerButton, font, size, heading, colours. These options / Last-used options / Ask when run. A new task starts with the tool's last-used options. Try on the text (Ask tasks use the last-used options)
- [x] Icon editor (UI/IconEditor.cs): 64-pixel icon on an 8× canvas with grid; pencil, eraser, fill; line, rectangle, ellipse and text stay a layer to move (drag, arrows) and resize (+ −) and centre until committed (Enter) or dropped (Esc); text: 1–4 characters, a font (symbol fonts listed, character grid, font dialog), bold, italic; 20 colours and more; background (rounded square behind everything); clear; import image (PNG, ICO, BMP, GIF, JPEG, TIFF) or paste; undo/redo (Ctrl+Z / Ctrl+Y); previews at 32 and 48. An uncommitted text layer is kept with the icon, adjustable next time
- [x] ClipX 1's PowerButtons imported (UI/PowerButtonImport.cs) from its registry entries or an exported .pref (right-click beside the buttons): Details parsed where ", Key=" follows (values may hold commas), DlgOption 0/1/2 → Ask/Own/Last used, Mode 1/2/3, Anchor, separators, Indent's RadioButton, liREN's reversed IgnoreBlanks; icons as ClipX 1's green (#016331) with the characters as a text layer (twips ÷ 15, 2×), symbol-font characters from Windows-1252 to U+F0xx. Tested on ClipX1.pref and the registry: all 8 buttons, icons look as in ClipX 1
- [x] Tested off-screen (scratch program): runs, one Undo, nesting, Repeat, Ask (Apply carries on, closing stops), rich-only stop, editor keeps options between tasks, icon editor shapes, layouts rendered (panel 1 and 3 columns, editor, icon editor, Ask note)
- [ ] User to try it on screen: dragging (palette → timeline, re-order, off), the panel's left edge, the icon editor's mouse drawing, Alt+1…9
- [ ] Check Punctuation's options (PunctuationOptions) in PowerButtons once they are shown anywhere
- [x] Ask for Find (edSRC) carries on after Next (its Go) only
- [ ] Translation: the editor's and icon editor's texts to the string resources with the rest

### First fixes after trying it (23 Nov 2026)
- [x] Ask: the yellow note under the panel is gone (it could cover Apply); the Apply that carries on reads "Go", in orange, with the focus, and closes the panel (Find and Replace too). `SidePanelView.GoButton(tag)` / `ShowGo` / `HideGo`; Indent's Go is Unindent for foUNI, Find's is Next, Replace's is Replace All
- [x] Ask: the panel's close cross (or Esc) asks "Stop PowerButton n (name) running?" Yes/No; Yes stops the run. `SidePanelHost.ConfirmClose`
- [x] ClipX 1's "Confirm close" (dlgConfirmClose) as UI/ConfirmCloseDialog.cs: the question and three wide buttons one above the other
- [x] PowerButton editor: the close icon or Esc with unsaved changes asks Save changes / Discard changes / Cancel; Save checks the button as OK does. Dirty: name, tasks added, moved or removed, Own/Last/Ask, icon, any option changed (compared as JSON, so only looking at tasks isn't a change). Cancel still discards without asking
- [x] Icon editor: the same question on the close icon or Esc (Esc drops a layer first). Dirty: something done and the icon not as it came (so undoing back isn't a change)
- [x] Icon editor: while a text or shape layer waits for Commit or Drop, the other tools, Background, Clear all, Import and Paste image are greyed out; Commit and Drop show in orange; the text box is off while a shape waits, line width and Filled while text waits; an Undo that brings a layer back selects its tool
- [x] ClipX's window: the close icon (or Alt+F4) with the text changed asks as ClipX 1: Discard changes (the default) / Copy & Close / Cancel. Changed = the text, or in rich text its RTF, or plain/rich, differs from how it came from the clipboard (startup or Refresh), so undoing back counts as unchanged; Find All's and spelling's highlights don't count. Done doesn't ask
- [x] Tested off-screen: Go and its colour, the cross Yes/No, no question when nothing changed, Cancel keeps the window open, Discard and Save, the tool locks, Undo bringing a layer back; dialogs rendered
- [ ] User to try on screen: the Go button, the close questions, the icon editor's greyed tools
- [ ] Changing tool (another toolbar button) while an Ask waits still stops the run without asking

### More requests (23 Nov 2026)
- [x] Icon editor: each tool button has a small picture beside its name (pencil, eraser, bucket, line, rectangle, ellipse, A), drawn in code (`IconEditor.ToolImage`), greyed with the button
- [x] Icon editor: Centre centres both ways. It centres the ink, the characters as drawn (`IconPainter.InkBounds`), not the text's box, which has the font's room above and below; new characters are centred the same way
- [x] Options, General: Export and Import in XML too, ClipX 1's .pref format (UI/SettingsPref.cs): the file type chosen (JSON | XML (.pref)) says which; Import tells them apart by the content. Export writes every setting as Key/Value (strings in CDATA, lists as entries 1, 2, 3…, UTF-16 with tabs, as ClipX 1), plus ClipX 1's Preferences section (fonts). Import reads that back exactly, or a ClipX 1 .pref: its tool settings (BulletList, NumList, NumLines, Indent, Wordwrap, RemLines), find options (Search: AllowSpecial, Anchor, separators), fonts (Preferences) and PowerButtons; ClipX2's other settings stay. Tested: round trip identical (with ]]>, <&>, quotes, empty strings), JSON still, ClipX1.pref
- [ ] ClipX 1 can read the tool settings and fonts of a .pref from ClipX2, but not its PowerButtons (ClipX2 keeps them in its own form)
- [x] Options, Display: "Fonts for plain text" at the top; below, "Default font for rich text" with its font and Choose… (`DisplaySettings.RichTextFont`, default Segoe UI 10.9 pt as before): used when ClipX starts with an empty rich text (`ApplyDisplaySettings`)

## To Do
- [ ] Help
- [ ] Updates
- [ ] About...
- [ ] Style tool
- [ ] Upload usage
- [ ] SVG icons
- [ ] Resize toolbars and switch from single to 2-tier mode and vv.
- [ ] Control Icons
- [ ] Favourites
- [ ] Install Segoe Fluent Icons font
- [ ] Code signing

### To do next (noted 23 Nov 2026, done 24 Nov 2026)
- [x] Icon editor: the Text tool's picture (the "A") larger (19 px Georgia Bold) and higher (`IconEditor.ToolImage`)
- [x] Closing ClipX ("Confirm close"): Copy & Close first (the default), Discard changes second, as in the PowerButton editor and icon editor; `ConfirmCloseDialog` no longer has a Discard-first layout
- [x] PowerButton editor: a waste-bin icon on "Delete PowerButton" (drawn in code, `PowerButtonEditor.TrashImage`)
- [x] Icon editor: the previews (32 and 48) each sit on a tile with a thin border; the tile turns a darker grey when most of the icon's outer edge would match the dialog's grey (background cleared, or near-grey/white) (`IconEditor.EdgeMatches`)

### Icon editor layout (24 Nov 2026)
- [x] The Text tool's A 4 px lower
- [x] Things that go together are together: left, the drawing tools, then Import image and Paste image, then a "Lines and shapes" group (Line width, Filled), then the colours with Set Background (was Background) under them; Grid and the pointer's position under the canvas; right, the previews with Clear all beside them, the adjust box, the Text group; bottom row "< Undo" and "Redo >" set a little apart from OK and Cancel. The window is 50 px taller (980 × 690), no wider
- [x] Text group greyed out unless the Text tool is chosen; Lines and shapes greyed out for Fill and Text (Filled only for Rectangle and Ellipse)
- [x] Undo and Redo greyed out when there is nothing to undo or redo
- [x] The layer's Commit and Drop are now Accept and Cancel ("Adjust the text, then accept")
- [x] Tweaks: the adjust box's margins even (12 px all round); Lines and shapes moved to the right, between the adjust box and Text, on one row; the colours moved up and the window shortened to 980 × 616; under the canvas the real size ("64 × 64 px", its tooltip saying why: shown at 32 × 32, sharp at higher display scaling) at the left, Grid centred, the pointer's x, y at the right; messages ("Nothing to move…", "no picture on the clipboard") at the bottom left, beside Undo
- [x] More tweaks: the Text group's bottom and the palette's bottom level with the canvas's; the adjust box, Lines and shapes and Text spaced evenly down the right; Set Background level with Grid
- [x] Bug: importing a file that isn't a picture (a .txt) crashed: GDI+ throws ExternalException, which wasn't caught. `IconEditor.LoadImageFile` now catches every non-fatal error and says "It isn't a picture in a format Windows can read"; Paste image the same
- [x] Icon fonts (UI/IconFonts.cs): the list offers only installed fonts; Segoe Fluent Icons added (Windows 11's icon font, C:\Windows\Fonts\SegoeIcons.ttf; on Windows 10 it must be downloaded); a font typed (that exists) or chosen in the Font dialog joins the list, kept in settings.json (`IconEditorFonts`); a typed name that isn't installed shows in red with a message (the text is drawn in a plain font)
- [x] The "…" character grid (UI/CharacterPicker.cs) shows every character the font has (GDI GetFontUnicodeRanges), scrolling and drawn only where in view: Segoe Fluent Icons 2,033, MDL2 Assets 1,830, Segoe UI Symbol 4,182; the one under the mouse large with its code (U+E721, "private use (an icon)"); arrows and Enter work. Symbol fonts (Wingdings) show their U+F021–U+F0FF block once. Emoji beyond U+FFFF aren't listed
- [x] Character grid's jump list (UI/UnicodeBlocks.cs): the Unicode blocks the font has, by name (Arial 44, Segoe UI Symbol 38, Segoe UI Emoji 22 groups; Wingdings and Webdings one, "Symbols (symbol font)"; the icon fonts one, Private Use Area); click one to bring its first row to the top; the block at the top of the grid, or of the character selected, is highlighted; a blue mark in the grid where each block starts. One click now selects a character (shown large, with its code and block); double-click, Enter or OK uses it
- [x] Bug: a click in the jump list scrolled the grid back to the character clicked before. The scrolling panel scrolled to the grid whenever the grid got the focus, undoing the jump; now it never scrolls on focus (`StayPanel`), and a jump selects the group's first character. Tested with the window focused (and the test fails on the old code)
- [x] Window 26 px wider (1006); the right-hand groups 262 wide; the adjust box evenly spaced (12 | nudge | 18 | + − ◎ | 18 | Accept, Cancel | 12)
- [ ] Note: the editor, icon editor and character picker are built in code, so they can't be adjusted in the Visual Studio designer; new forms should be designer forms where possible
