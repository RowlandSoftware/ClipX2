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
- [ ] Options: service choice; Premium key dialog with Test button and a link to the access-token page; key stored encrypted (DPAPI on Windows), not in settings.json; picky level
- [ ] Turning grammar back on after "No" at the consent question: needs the Options dialog (for now, `Grammar.Enabled` in settings.json)
- [ ] Grammar: Change All for grammar issues? Undo of Ignore Rule (a list of ignored rules in Options)
- [ ] Grammar re-check of a paragraph after the user's own edits in the editor (now: re-sent at the next F7, only changed paragraphs)
- [x] -ise / -ize: `Spelling.OxfordIze` (off): LanguageTool's Oxford -ize suggestion (OXFORD_SPELLING_Z_NOT_S) hidden unless on; Hunspell side = the dictionary (installed en_GB is -ise only; an en_GB-ize can be added). LanguageTool code drops suffixes (en_GB-ize → en-GB)
- [ ] Options: -ise / -ize choice (and offer an -ize dictionary); list of ignored rules with their messages, to switch back on
- [ ] Ellipsis rule: no space before a trailing-off ellipsis, spaces round an omission — check LanguageTool's rule id when it next appears; possibly a ClipX punctuation rule (Check Punctuation, parametrised)
