---
paths:
  - ".claude/memory/**"
---

# Writing team memory

This folder is committed and the whole team's Claude sessions read it. Every line is paid for by
every future session: write what would change a decision, delete what no longer does.

| File | Holds | Who edits |
|---|---|---|
| `INDEX.md` | One row per file: what it is for, status, last update | Anyone, same commit as the file |
| `knowledge.md` | Team decisions + reason + rejected alternative, invariants, traps | Via PR like code — the team follows it |
| `project-overview.md` | Game, scope, systems + owner, milestones | Lead / via PR |
| `story.md` | Story notes, each section `nháp` or `đã chốt` | Designer decides; Claude records |
| `log/<git-user>.md` | That developer's session log, newest on top, ≤ 15 entries | Only its owner — no merge conflicts |
| `plans/YYYY-MM-DD-<slug>.md` | A plan in progress (novent-plan) | Its author; delete when shipped |
| `dream-log.md` | Accepted/rejected dream proposals | novent-dream |

- Test for a line: would a fresh session decide worse without it? Code facts a grep finds,
  file lists, "đã sửa X" without why — do not write.
- Entry shape: **Luật** (one checkable sentence) · **Vì sao** (the mechanism) · **Nếu vi phạm**
  (symptom) · **Nguồn** (file:line, doc, date).
- Superseded decisions are struck through with the new one and date, not deleted.
- Machine-specific facts (your Editor path, your installed tools) go to your personal auto memory,
  not here — teammates' machines differ.
- No personal data about teammates (phone, email, grades).
- Vietnamese for content; file names and code identifiers in English.
