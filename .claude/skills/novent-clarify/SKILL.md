---
name: novent-clarify
description: Pins down what a request means before code is written in The Novent — unnamed targets, ambiguous referents, conflicts with the GDD, memory or code, missing acceptance criteria, co-op unspecified — and returns a sharpened brief plus at most three questions. Load on non-trivial or vague requests such as "làm cái menu", "thêm enemy", "làm giống game X", "như cũ", "sửa hết", "fix cái đó đi", "cho nó mượt hơn". Use it whenever a request touches more than one file or names no exact target.
---

# Clarify before building

Treat the prompt as a first draft. Close every gap the repo and the design docs can close, and
ask only about what remains.

Reply to the user in Vietnamese.

## Step 1 — Restate

One or two sentences: the interpretation you are about to act on, the scene/system/files you
expect to touch, and the assumptions inside it. A divergence between restatement and prompt *is*
the question.

## Step 2 — Hunt for these defects

| Defect | Signal | Do |
|---|---|---|
| No target | No scene, prefab, system or level named | Find candidates yourself, ask only to pick |
| Ambiguous referent | "cái đó", "như cũ", "giống game X" | Resolve against the repo, memory and recent diff; for "giống game X" ask which *mechanic*, never copy the other game's content |
| Conflicts with the docs | Request differs from `Docs/` GDD or `story.md` | Name both, ask which wins, then update the memory |
| Conflicts with a decision | `knowledge.md` already settled it | Read it — the user may have forgotten |
| No acceptance criterion | "mượt hơn", "cho đúng" | Ask what observable result means done |
| Hidden scale | "hết", "toàn bộ", "tất cả level" | Count it yourself, confirm the number |
| Unstated platform/perf | Nothing said | Use the target platform in `project-overview.md`; state it |
| Off-milestone | Polish while the core loop is not done | Say so in one line, then do what the user decides |
| Co-op unspecified | Nothing said about 2 players | Read the system's "Two-player Co-op" section in the `.docx`; state shared vs per-player and who has authority |
| Owner unclear | The system may be a teammate's | Check `project-overview.md` assignments and `git log`; ask if it is someone else's |
| Wrong GDD version | Request uses names/numbers from a `v0.1` tab (₵, jetpack, quán ăn) | Point to the v0.5 equivalent and ask |

## Step 3 — Answer what you can yourself

Grep the code, read `INDEX.md` → matching memory rows, grep `Docs/`. Only what survives becomes a
question.

## Step 4 — Ask few, ask well

At most 3 questions, ordered by impact, multiple choice with a recommended option and a one-clause
reason, plus "hoặc cách khác?". One round only. Small low-stakes ambiguity: state the assumption in
one line and go.

## Step 5 — Brief, then hand to `novent-plan`

```
Goal / Scope in-out / Constraints / Touched files / Acceptance criteria / Open questions (kèm giả định đang dùng)
```

Nobody answers → proceed with the most reasonable reading, written at the top as
"Giả định đã dùng: …".

## Do not

Interrogate; ask what a grep answers; ask permission for the obvious; stall a 5-minute change.
