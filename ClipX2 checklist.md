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
- [ ] PowerButtons (customisable buttons combining several tools, by tag)

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
- [ ] Word: same test with Word's RTF and Word's Paste
- [ ] Word RTF with tables or pictures: ClipX's reader turns tables into tabs and leaves pictures out (RichEdit kept them). Use RichEdit's reader when there are tables/pictures?
- [ ] Comments (Writer's annotations) and bookmarks: lost; carry them?

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
- [x] F3 / Shift+F3 = next / previous match while Quick search is active (from Ctrl+F, the box or a search, until Esc, a click in the text or another tool); otherwise Shift+F3 cycles case. Search and Replace to do the same when active
- [x] Find all's highlights go on Esc, another tool, a new search, Next/Previous match (arrows, F3), a click in the text

## Edit
- [x] Decode URL (edUDC), HTML (edHDC), UTF-8 (edUTF), MIME (edMIM): selection exactly or whole text; bytes as UTF-8 where valid, else Windows-1252; all named HTML entities; MIME soft line breaks and encoded words (=?utf-8?Q?...?=)
- [x] Plain characters (edPLA: – → --, — → ---, so it reverses Curly quotes), Curly quotes etc. (edCRL: quotes by context, apostrophes, feet/inches left straight, -- – , --- —, ... …), Regularise line ends (edEOL: soft line breaks, U+2028/2029, NEL, form feeds)
- [x] Strip email quote marks (edSTR): > and | at line starts, any depth, spaces before them; only the one space after each mark goes, so indentation inside a quote is kept; whole lines; engine: LineFormatter.StripQuoteMarks

## Statistics (chSTAT)
- [x] Side panel, kept up to date while open: Selection (only when there is one) above Whole text. Words, characters, without spaces, sentences, paragraphs, words per sentence, size (UTF-8 for plain text, RTF for rich text); lines left out (same as paragraphs)
- [x] Readability (English only, from the spelling language): Flesch Reading Ease with Flesch's level, Flesch–Kincaid grade, Gunning Fog. Syllables estimated by English rules; "rough under 100 words"
- [x] Readability for other languages: German (Amstad), French (Kandel–Moles), Spanish (Fernández Huerta), Italian (Flesch–Vacca), Dutch (Douma), Portuguese (Martins), Russian (Oborneva); syllables from LibreOffice's hyphenation patterns (`Hyphenator`, Liang), downloaded with the dictionary as name.hyph.dic (licence name.hyph.licence.txt); Options offers "Update" for an installed dictionary lacking them. Tested: German 38/40, Spanish 28/30, Italian 28/30, Dutch 30/30 words right. English keeps its own rules (89% against the patterns' 59%)
- [ ] Hyphenation for word wrap (patterns are there: `Hyphenator.Breaks(word, forPrint: true)`)

## Tool handling (ClipX 1's MainToolHandler)
- [x] Engine: `Tools/ToolCatalogue` (every tag, ClipX 1's names and descriptions, group, kind, options class), `Tools/TextTools.Run(tag, text, from, to, options)` (31 text tools: edits out), `Tools/ToolUsage` (counts in memory, `ToolUsed` event, `Report()`; never sends anything)
- [x] ClipX: one handler, `Form1.RunTool(tag, how, options)` (UI/Form1.Tools.cs): every tagged button wired once by tag (click = options panel, right-click = last settings), keys (F7, Shift+F3 = ccCYC, Ctrl+R = Repeat last tool), PowerButtons later (`ToolTrigger.PowerButton` with their own options). Counts in usage.json (Usage/Total, PowerButton counts apart), remembers the last tool, closes other options panels, ends Quick search, clears the status bar
- [ ] Usage upload (ask first, like grammar); a view of the statistics
- [ ] PowerButtons: a list of tags + options run through RunTool
- [ ] Not yet written: edSRC/edREP (Search and Replace), pb* extras

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
- [x] Engine: `Tools/ToolFinder` (names, button captions and tooltips, synonyms, descriptions, groups; prefixes, plurals, typos; filler words ignored; "get rid of"/"delete"/"strip" = remove; Remove… tools lower unless removing) and `Tools/ToolKeywords` (synonyms per tool, US spellings, examples such as %20, â€™)
- [x] App actions searchable: Settings, Done, Plain or rich text, Quick search, Refresh, Repeat, Cycle case (tags clipxOPT clipxFIN clipxMOD clipxQSR)
- [ ] Synonyms in other languages, with the translations
- [ ] Perhaps: note searches that find nothing (locally, or with usage statistics if the user agrees) to improve the synonyms
- [ ] Perhaps: suggest help topics as well as tools, once there is help

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

