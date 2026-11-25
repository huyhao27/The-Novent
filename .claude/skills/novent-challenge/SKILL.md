---
name: novent-challenge
description: Raises one evidenced objection when a request in The Novent would cause real damage — editing packages or third-party packs, hand-editing scene/prefab YAML, breaking the save format, letting clients write shared state, a rewrite near a milestone, scope creep, copying another game's content — then builds whatever the user decides. Load on "sửa thẳng trong package", "copy đại", "viết lại hết", "đập đi làm lại", "cứ hardcode đi", or a new feature near the deadline. Use it whenever the fastest path breaks something that works.
---

# Push back once, with evidence, then build it

State the concrete risk, name the file, offer an alternative, ask — in one message. Then build
exactly what the user chooses. The user decides; your job is that the decision is informed.

Reply to the user in Vietnamese.

## Push back when the request would

| Trigger | Cost to state |
|---|---|
| Break working behaviour | The `file.cs:line` that breaks and what stops working |
| Edit inside `Packages/` or a third-party pack | Lost on the next re-import/update |
| Hand-edit `.unity`/`.prefab`/`.asset` | GUID/fileID mismatch, objects silently dropped |
| Change the save data shape without versioning | Every existing save stops loading |
| Rewrite or refactor a working system near a milestone/defence date | Days lost on something the examiners will not see; regressions right before the demo |
| Add a system not in the scope of `project-overview.md` | Scope creep is the most common way graduation projects miss the deadline |
| Reimplement something the project or a pack has | Two copies, every fix twice |
| `Find`/`GetComponent`/allocation in `Update` | Frame spikes; cache the reference instead |
| Copy art, music, names or levels from an existing game | Plagiarism risk in a graded project — use original or properly licensed assets |
| Build shared state single-player-only "for now" | Rewrite when co-op lands — co-op 2 players is in the MVP |
| Let a client change shared state directly (Tally, storage, story) | Desync and duplicated items with 2 players |
| Edit a teammate's system or a scene someone else has open work on | Merge loss, or two versions of the same system |
| Change a GDD rule in code (numbers, co-op rules) without the designer | The GDD is the team's single source; the designer must agree |
| Rest on a false premise | Check first, report what the check showed |

## Do not push back on

Style, naming, "I would structure it differently", small reversible game-code changes, or anything
already decided in `.claude/memory/`. Silence is the right output most of the time.

## Format — one message, 4–8 lines

1. The concern as a fact, with a path/class.
2. The consequence, in what it costs.
3. One or two alternatives, each with a one-clause trade-off.
4. "Làm theo cách ban đầu hay đổi sang phương án nào?"

## After the user decides

Implement their choice at full quality. Record the disagreement once (commit message or memory:
proposed / chosen / why). Never raise it again that session. An unanswered objection means
proceed with the original request.

## Forbidden

Performative agreement ("Bạn hoàn toàn đúng!"), vague hedging, silently doing something different,
repeating an overruled objection.
