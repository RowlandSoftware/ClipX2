# Spelling checker: checklist for the side panel

The engine (`TextEngine/Spelling`) and a first SpellingPanel (`UI/Panels`) are done; ticked items are in. This lists the rest, and the decisions to
make along the way. F7 and the Spelling button open the panel; F7 again skips the word shown.

## What the engine already gives the UI

`SpellChecker` (one per ClipX session; created in the background at startup by `Form1.LoadSpellChecker`):

| Call | Use in the panel |
|---|---|
| `FindNext(text, from, wrap)` | Next issue after the caret/current issue; wraps to the top |
| `FindIssues(text[, start, length])` | All issues (a list view, a count, squiggles) |
| `Suggest(issue, max)` | Suggestions list (call only for the issue on show: ~20–90 ms) |
| `Change(issue, replacement)` | One `TextEdit`, in the word's case (teh → the, Teh → The) |
| `ChangeAll(text, issue, replacement)` | Edits for every occurrence, each in its own case |
| `Delete(text, issue)` | Removes the word and one space (repeated word: the space before it) |
| `IgnoreOnce(issue)` / `IgnoreAll(word)` | Skip one occurrence / the word for the session |
| `AddToDictionary(word)` | Saves to `%AppData%\ClipX2\persdict.dat` and takes effect at once |
| `NotifyEdits(edits)` | Call after applying edits, so Ignore once follows its word |
| `Reset()` | Forget ignore/change lists |
| `SpellingOptions` | IgnoreUppercase, IgnoreWordsWithNumbers, IgnoreAddresses, CheckRepeatedWords |

Apply edits with `richTextBox1.ApplyEdits(edits)` (formatting kept, one Undo step), as Check
Punctuation does. Issues are `Misspelt` or `RepeatedWord`.

## Panel layout (ClipX 1's dlgSpelling, minus what the panel makes unnecessary)

- [x] "Not in dictionary:" / "Repeated word:" label with the word
- [x] Context line: the sentence or a few words either side, word highlighted (ClipX 1 copied the whole text into its own box; the panel can't obscure the text, so the main editor's selection may be enough)
- [x] "Change to:" box, filled with the first suggestion; editable
- [x] Suggestions list; click fills "Change to", double-click changes
- [x] Buttons: Ignore, Ignore All, Add to Dictionary, Change, Change All, Delete (shown for repeated words, or when "Change to" is empty, as in ClipX 1)
- [ ] Undo Last: ClipX 1 kept its own stack; here the editor's Undo may do (each Change is one Undo step), but Ignore/Ignore All/Add would need their own undo
- [x] Close/Stop
- [x] Language picker (`SpellingDictionary.AvailableLocales(folder)`; en_GB and en_US shipped)
- [ ] Status: "No spelling errors found" / "Spelling check complete" are done (in the "Not in dictionary" line); a running "n issues" count is not

## Behaviour

- [x] Start from the caret, or from the top? (Word: from the caret, then offers to continue from the top.) The engine supports both
- [x] With a selection, check only the selection (`FindIssues(text, start, length)`), as other tools do
- [x] Moving on: after Change/Ignore, find the next issue after the current one
- [x] User edits the text directly while the panel is open: re-find from the caret (positions are not kept between calls, so this just works); Ignore-once positions need `NotifyEdits` only for edits made by the panel
- [x] Change: if the replacement itself fails the check, ClipX 1 changed it and re-checked; Change All asked "may contain misspellings — change all?"
- [x] Smart replace (ClipX 1's checkbox): "Smart replace" check box on the "Change to:" line, on by default; off, the replacement goes in exactly as typed (`Change`/`ChangeAll` take `matchCase`). Not yet remembered between sessions
- [ ] Keyboard: done: F7 next/skip, Enter = Change, Esc = close and return to the text, access keys (Delete is Alt+E, as Alt+D is Done). Still to decide: Shift+F7 previous?
- [x] Focus: keep the caret in the text, or in the panel? (Panel for keyboard use, but the word must stay visibly selected: `HideSelection = false` on the editor)
- [x] Highlight the current word in the text without changing its formatting (ClipX 1 made it red and bold in its copy; we must not touch the real formatting, which is exported)

## Options (preferences dialog, later)

- [ ] Ignore words in capitals, with numbers, web/file addresses; check repeated words
- [ ] Dictionary language; default en_GB
- [ ] Edit the personal dictionary (`PersonalDictionary.Words`, `Remove`)
- [ ] Newer dictionaries: the shipped en_GB is from 2013. Current ones are at wordlist.aspell.net / LibreOffice's dictionaries repository; drop-in replacements

## Possible later

- [ ] Check as you type, with wavy underlines. RichEdit can draw them (TOM underline style `tomWave` plus underline colour), but that is formatting: it would have to be stripped on export and on Undo, so it needs care. Maybe only on request
- [ ] Prompts for Check Punctuation's guesses could use the same panel (same pattern: issue, context, choices)
- [ ] Grammar/style (ClipX 1 tried After the Deadline and Ginger, both online and now gone; LanguageTool is the current option, keeping to British style, lightly applied and not dictatorial)

## Known engine behaviour to review in testing

- 'Tis / 'twas: flagged unless in the dictionary (en_GB lacks them); add to the personal dictionary
- Hyphenated words: correct if the whole or every part is known (Ignore All counts); otherwise each wrong part is flagged in turn (parker, then bowles), never the whole word. Change All on a part changes it inside other hyphenated words too
- Possessives: O'Neill's passes if O'Neill is known
- Anything with a dot between letters is treated as an address and skipped, so "end.The" (missing space) isn't flagged — Check Punctuation catches that
- Repeated words are only flagged on the same line, with nothing but spaces between
