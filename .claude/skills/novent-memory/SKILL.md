---
name: novent-memory
description: Keeps The Novent's committed team memory in .claude/memory/ — decisions with reasons in knowledge.md, per-developer session logs in log/<git-user>.md, story notes, plans — and prunes what no longer changes a decision. Load after a decision settles, a trap is found, scope or story changes, before a session ends, and on "lưu memory", "ghi nhớ", "note lại tiến độ", "cập nhật cốt truyện", "hôm trước làm tới đâu", "trước khi đổi session ghi lại". Use it whenever work produced a lesson worth keeping.
---

# Team memory that pays for itself

Every line in `.claude/memory/` is committed and re-read by every teammate's future sessions:
write what would change a decision, delete what no longer does. The layout and entry shape are
also in `.claude/rules/memory.md` (auto-loaded when a memory file is opened).

Reply to the user in Vietnamese.

## Who reads what

| File | Read by | Written by | Lifetime |
|---|---|---|---|
| `INDEX.md` | Every session (injected by the `SessionStart` hook) | Anyone, same commit as the file it lists | Permanent |
| `knowledge.md` 🔴 | Tasks touching a decision | Via PR — the team follows it | Permanent |
| `project-overview.md` 🔴 | Scope/system/schedule questions | Lead, via PR | Updated with scope |
| `story.md` | Narrative work | Records the designer's decisions | Grows |
| `log/<git-user>.md` | Its owner (top entry injected at start) | Only its owner — no merge conflicts | ≤ 15 entries |
| `plans/YYYY-MM-DD-<slug>.md` | The session executing it | `novent-plan` | Delete when shipped |
| `dream-log.md` | `novent-dream` | `novent-dream` | ~30 days |

Personal, machine-specific facts (your Unity install path, tools only you installed, your own
preferences) go to Claude's auto memory for this project, not to the repo.

## When to write

| Moment | Write |
|---|---|
| A decision settles | `knowledge.md` entry: rule, why, rejected alternative, source/date. Mark `Phạm vi: cả team` and remind the user to tell the team (chat/PR) |
| A trap is found the hard way | `knowledge.md` entry with the symptom and the exact symbol/number |
| Something tried and reverted | One line with why, so nobody retries it |
| Scope, owner or milestone changes | `project-overview.md` |
| Story decided in chat | `story.md`, marked `đã chốt`; Claude's ideas only under `## Đề xuất (chưa duyệt)` |
| Session ends | Top of `log/<git-user>.md` (`git config user.name`, lowercased, spaces → `-`): 3–6 lines |

Test for every line: **would a fresh session decide worse without it?** Not worth a line: anything
the code or `git log` says, file lists, "đã sửa X" without why, teammates' private details.

## Committing memory

Memory changes ride with the work: commit them on the current feature branch
(`git add .claude/memory && git commit -m "memory: <what>"`). A change to `knowledge.md` or
`project-overview.md` reaches the team through that branch's PR. No branch in progress → a small
`chore/memory-<topic>` branch. Never commit memory straight to `main`.

## Pruning

Prune in the same edit that adds. A file past ~200 lines gets compressed. Delete finished plans and
done checklists. A decision that carried a reason is struck through, not erased:
`~~Coroutine~~ → Awaitable 2026-12-01 vì Unity 6 khuyến nghị, ít GC hơn.` Say in one line what was cut.

## Templates

Knowledge entry:

```markdown
## <Bất biến hoặc cái bẫy>
- **Luật**: <1 câu kiểm chứng được>
- **Vì sao**: <cơ chế thật>
- **Nếu vi phạm**: <triệu chứng>
- **Nguồn**: <file:line, tài liệu, hoặc "đã thử rồi revert <ngày>"> · **Phạm vi**: cả team | <hệ thống>
```

Session log entry (newest on top):

```markdown
## 2026-12-01 — <việc chính>
- **Đã làm**: <kết quả, nhánh, commit>
- **Verify**: <lệnh đã chạy + kết quả>; chưa verify: <gì>
- **Tiếp theo**: <bước kế, file>
- **Vướng**: <nếu có>
```
