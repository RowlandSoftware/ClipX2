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

- [x] "Not in dictionary:" / "Repeated word:" label; the word in its own sunken label (labelWord, over the right half, hidden when no word is shown). Click it to copy the word into "Change to" for editing
- [x] Context: none in the panel; the word is highlighted in the main text (scrolled into view) and named in the "Not in dictionary:" line. The panel's context box was removed, giving the suggestions list its room
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
- [ ] Keyboard: done: F7 next/skip, Ctrl+L, Ctrl+Shift+L, Ctrl+S and Ctrl+T in "Change to" (as in the editor; `UI/TextBoxKeys`), Enter = Change, Esc = close and return to the text, access keys (Delete is Alt+E, as Alt+D is Done). Still to decide: Shift+F7 previous?
- [x] Focus: keep the caret in the text, or in the panel? (Panel for keyboard use, but the word must stay visibly selected: `HideSelection = false` on the editor)
- [x] Highlight the current word in the text without changing its formatting: `ClipXRichTextBox.SetMarker` paints a translucent yellow box over it after RichEdit paints (not formatting, so not in Undo or the export; any edit clears it). ClipX 1 made it red and bold in its own copy of the text

- [x] Definitions: clicking a suggestion or "Change to" shows Wiktionary definitions (REST API, entry for the spelling language, cached for the session) in a box floating over the editor beside the panel (`DefinitionPopup`, `WiktionaryLookup`). Hidden when the next word is shown
- [x] Definitions: for English, plain definitions with examples from the Simple English Wiktionary first (one request for all forms), else the full Wiktionary; solid form tried for spaced/hyphenated words; other languages named when the word is only in those
- [ ] Definitions: Options setting for `SimpleDefinitionsFirst` (on); double-click a word in the box to look it up, with Back
- [ ] Definitions: web look-up in the browser as an option; a setting (`LookUpDefinitions`, on) to turn it off, in the Options dialog

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

## Suggestion order (value add over Writer)

- [x] Stage 1: rank by the text and the session (`SuggestionRanker`): words in the text (more uses, stronger), corrections accepted this session (kept across checks), and their other forms (adversary → adversaries, adversary's), added if Hunspell missed them; promoted only when about as close as Hunspell's own (teh → the not pushed down by "tech")
- [ ] Stage 2: word frequency lists (user to supply, per language) for the rest of the order
- [ ] Stage 3: own candidates with a transposition-friendly distance (non-adjacent swaps; user collecting examples as test cases), checked by Hunspell
- [ ] Stage 4: neighbouring words (bigrams), with real-word errors in the grammar discussion
- [ ] Other forms: doubled consonants (stop → stopped) not covered; accepted corrections could be kept between runs
