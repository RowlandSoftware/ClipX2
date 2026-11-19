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
  
- [ ] Convert formatted text to plain (if plain is selected) on startup and when toggle
- [ ] Convert plain lists etc. to RTF as above
- [ ] HTML to plain or RTF on startup

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
- [ ] Line numbers (liREN), Remove line numbers (liRLN)
- [x] Live numbered lists: blank lines between items become unnumbered items, so the numbering runs on (as plain text); exports keep them as blank lines
- [ ] Typed lists in rich text: hanging indent so wrapped lines line up; Tab/Shift+Tab to change a live item's level
- [ ] Settings: import of ClipX 1's .pref (XML) — keys already match for lists

## Format
- [x] Trim white space, Trim trailing space, Single space, Tabs to spaces (columns of 8), Spaces to tabs, Paragraph: act at once (click or right-click)
- [x] Indent / Unindent (tab, spaces or text such as "> "), Word wrap (width, hanging indent, justify; plain text only), Remove extra lines: options in the side panel, right-click = last used; settings Indent, Wordwrap, RemLines (ClipX 1's names)
- [x] All work on whole lines (selection widened) or the whole text; edits in place, so rich text keeps its formatting; one Undo step
- [ ] Rich text: indenting by paragraph indents rather than typed tabs/spaces
- [ ] Tab key: RichTextBox doesn't accept it (AcceptsTab?); ClipX 1 used Ctrl+I for a tab, now italics; choose a key
