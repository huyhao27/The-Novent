---
name: novent-plan
description: Writes an executable plan to .claude/memory/plans/ before a multi-file change in The Novent — goal, milestone, reuse check, module placement, co-op authority, team ownership, numbered steps with real paths and Unity wiring, verification, risks. Load before touching 3+ files, a new game system, a refactor or a milestone breakdown, and on "lên plan", "kế hoạch", "làm hệ thống...", "refactor lại", "chia nhỏ task", "chia phase", "tuần này làm gì". Use it whenever the work will not fit in one file.
---

# Plan so a fresh session can execute it

A plan is finished when a Claude with empty context can run it top to bottom without asking.
Vaguer than that is notes. For this team graduation project, a plan also has to answer: does this
serve the current milestone or is it polish that can wait, how does it behave with 2 players online,
and does it collide with what a teammate is building?

Reply to the user in Vietnamese.

## Plan or just do it

| Situation | Action |
|---|---|
| One file, mechanical, obvious verification | Just do it |
| 3+ files, a new system, a refactor | Plan first |
| Scenes, prefabs, ScriptableObject data, Build Settings involved | Plan first — these fail silently |
| Save format, core architecture, netcode, anything many systems will depend on | Plan first and get approval — the rest of the team builds on it |

## Explore first — in a subagent

Delegate the broad search; it returns paths and signatures. Before step 1, know:

1. What exists already (reuse search from the `novent-context` skill).
2. The closest sibling to copy the shape of.
3. The decisions already in `knowledge.md` and the current milestone in `project-overview.md`.

A step that cannot name its file means exploration is not done.

## Format

Write `.claude/memory/plans/YYYY-MM-DD-<slug>.md`:

```markdown
# <Task>

## Goal
1-2 câu: kết quả, không phải cách làm.

## Milestone
Phục vụ mốc nào trong project-overview.md; nếu là polish thì ghi rõ.

## Reuse check
Đã có gì sẽ dùng lại (đường dẫn). Đã tìm gì mà không thấy.

## Module
Thư mục + asmdef theo skill `novent-architecture`; Core service dùng lại; asmdef mới (nếu có).

## Co-op
State mới/đổi: chung hay riêng từng người, ai có quyền đổi (host/owner), late-join nhận thế nào, có lưu không.
Tiêu chí co-op lấy từ mục "Two-player Co-op and Acceptance Criteria" của hệ thống trong GDD.

## Team
Hệ thống/scene này của ai; file nào có thể đụng việc của người khác; nhánh làm việc.

## Out of scope
Những thứ liền kề cố ý không đụng.

## Steps
1. ...

## Verification
Lệnh/thao tác thật chứng minh nó chạy.

## Risks
Cái gì có thể vỡ, và cách lùi.
```

## Step quality bar

Each step is a few minutes of work, names exact paths, says what changes, ends observable:

> 3. Add `Assets/_Project/Scripts/Gameplay/Dialogue/DialogueLine.cs` (ScriptableObject: speaker,
>    text, portrait). Observable: compiles; `Create > Novent > Dialogue Line` appears.

Banned in a step: "TBD", "similar to step N", "handle edge cases", "write tests" without naming
the behaviour. Unity wiring (add component, assign field, add scene to Build Settings, create
the asset) is its own step — it is where things silently fail.

## Verification section

Concrete: compile (`mcp__UnityMCP__refresh_unity` + `mcp__UnityMCP__read_console`), the EditMode
tests that cover the new Model (`mcp__UnityMCP__run_tests`), which scene
to play and what to do, what should appear, and whether it survives a scene reload. Anything
touching shared state also names the 2-player check (host + client, what each must see, and a
late join). How to run and read them: the `novent-verify` skill.

## Approval

Architecture and save-format plans: approved before step 1. Routine work: present and start —
each step ends in something observable, so the user can stop you anywhere. If the plan changes
mid-way, edit the file and say what changed. When it finishes, fold what survives into
`knowledge.md` and delete the plan (the `novent-memory` skill).
