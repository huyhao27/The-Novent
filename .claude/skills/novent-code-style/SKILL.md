---
name: novent-code-style
description: Reviews and cleans C# in The Novent against the team's conventions — above all the comment rule (say why, never restate code, no commented-out code or AI breadcrumbs), naming, Unity habits and minimal diffs — with before/after examples. Load for "review code này", "dọn comment", "thêm comment", "code cho sạch", "đặt tên sao cho đúng", "chuẩn hoá code", and before handing code to the reviewer agent. Use it whenever code quality, naming or comments are the question.
---

# C# style — review and clean-up

The rules themselves are in `.claude/rules/csharp.md` (loaded automatically when a `.cs` file under
`Assets/_Project` is read or edited). This skill is how to apply them to existing code without
making the diff worse. Examiners read this code at the defence: clean and consistent beats clever.

Reply to the user in Vietnamese.

## Review order

1. **Behaviour risks first** — per-frame allocations/`Find*`/`GetComponent` in `Update`, unpaired
   subscribe/unsubscribe, `?.`/`??` on `UnityEngine.Object`, renamed serialized fields without
   `FormerlySerializedAs`, shared state written outside a host `Apply…` (`.claude/rules/coop.md`).
2. **Comments** — the rule below.
3. **Naming** that misleads (a bool that is not a question, `Manager` that is a model, event named
   `OnX` instead of past tense).
4. Leave taste alone. Report findings as `path:line — vấn đề — sửa`; fix only what the user asked.

For a whole branch or PR, hand it to the `novent-reviewer` agent instead of reading every file here.

## The comment rule, by example

A comment earns its place only by saying what the code cannot: why, a constraint, a tuned number and
its source, a GDD rule, a workaround and its cause, a trap for the next reader.

| Before | After | Why |
|---|---|---|
| `// Check if the player is dead`<br>`if (_oxygen.IsEmpty)` | `if (_oxygen.IsEmpty)` | Restates the line |
| `// ui` above a block of fields | nothing; split the class if it needs labels | One-word section labels are noise |
| `// _speed = 5f; old value` | delete it | Git keeps history |
| `// Added null check (fix bug #12)` | delete it; say it in the commit message | AI/changelog breadcrumb |
| `const float k = 0.2f;` | `// Matches the hit-flash animation length (12 frames at 60 fps).`<br>`private const float HitFlashSeconds = 0.2f;` | Name says what, comment says where the number came from |
| `if (!IsHost) return;` | `// Clients only request; the host applies so all machines see one Tally (GDD CORE LOOP § Luật co-op).`<br>`if (!_session.IsHost) return;` | A *why* the next reader would otherwise "fix" |

Three comments worth imitating:

```csharp
// string.GetHashCode is randomised per process on .NET Core runtimes, so it would give
// each machine a different stream; FNV-1a is stable everywhere.

// Write beside the old file and swap, so a crash mid-write never leaves half a save.

// A missing prefab reference is "fake null": the type test alone would accept it.
```

## Clean-up without noise

- Touch only lines the task needs. No reformatting, `using` reordering, renames or regions on the side.
- Rename a public API or serialized field only as its own task, with `FormerlySerializedAs` for fields.
- Do not mass-translate existing comments; new ones in English.
- A method that needs a dozen comments to be readable needs extracting into named methods instead.

## Commits

Message says what changed and why. Never `Co-Authored-By: Claude` or other Claude attribution
(`.claude/settings.json` already disables it — do not add it by hand).

## Common mistakes

| Tendency | Instead |
|---|---|
| Annotating each step of an edited method | Add nothing; extract a method if unreadable |
| Reformatting the whole file "while here" | Revert formatting hunks |
| `public` field to see it in the Inspector | `[SerializeField] private` + read-only property |
| Fixing style in files the task did not touch | List them for the user; do not edit |
| Review that praises or summarises the diff | Findings only, most severe first |
