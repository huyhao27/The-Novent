---
name: novent-dream
description: End-of-day dreaming pass for The Novent — digests today's Claude Code transcripts locally (secrets redacted), finds repeated corrections, skills that should have loaded, recurring traps and unrecorded decisions, and proposes numbered memory/rule/skill updates each backed by a verbatim quote — written only after the user approves. Trigger only when asked — "dream", "mơ đi", "chạy dream", "tổng kết cuối ngày", "rút kinh nghiệm hôm nay", "học từ hôm nay", "/novent-dream".
---

# Dreaming — learn from the day, out of band

This pass runs after the work, over many sessions at once, and looks for what no single session
could see. It only **proposes**; nothing is written without the user's yes.

Reply to the user in Vietnamese.

## 1. Build the digest — never read raw transcripts

From the project root:

```bash
python -I .claude/skills/novent-dream/scripts/dream_digest.py --out <scratchpad>/dream-digest.md
```

`--since` defaults to the day after the last `## YYYY-MM-DD` in `.claude/memory/dream-log.md`
(today if none); pass it for "dream cả tuần". The script redacts secrets locally and anchors every
line as `<session8>:L<line>`. Digest over ~1500 lines → give it to a subagent with section 2 as
the brief.

## 2. Find patterns

Read the digest, `INDEX.md`, `knowledge.md`, `.claude/CLAUDE.md`, `dream-log.md` (rejected items
are not proposed again without new evidence). Look for:

| Signal | Usually becomes |
|---|---|
| `FLAG possible correction` — the user restating a rule Claude broke | `knowledge.md` line, or a fix in the skill that should have enforced it |
| Same correction in ≥2 sessions | Strong candidate — "lặp N lần" |
| `skills claimed` ≠ `skills loaded` | Tighten that skill's `description` |
| Prompt that needed a skill with no matching load | Add the user's literal phrase to that skill's `description`, or to `.claude/hooks/skill_router.py` |
| Same `tool error` cause repeated | Environment fact in `knowledge.md` / `CLAUDE.md` |
| A decision settled in chat, not written | Decision + reason in `knowledge.md` or the task file |
| Story/design decided in chat, not in `story.md` | Add it, marked `đã chốt` |
| Memory line the day proved false | Mark superseded |

Drop a candidate with no quote, a one-off, already in memory, or a 30-second grep away.

## 3. Pick the narrowest layer

| Layer | Path | Bar |
|---|---|---|
| Personal | Claude's auto memory for this project (not the repo) | Only true on this developer's machine or for their habits |
| Own log | `.claude/memory/log/<git-user>.md` | Where this developer stopped |
| Plan / task | `.claude/memory/plans/…` | Live work |
| Knowledge | `.claude/memory/knowledge.md` | Will bite any teammate again |
| Path rule | `.claude/rules/<area>.md` | A coding rule tied to a folder or file type |
| Hook | `.claude/hooks/guard.py` (deny/ask), `skill_router.py` (phrases) | A rule broken twice despite being written down, or a skill that failed to trigger on a real phrase |
| Injected context | `.claude/CLAUDE.md` (≤ ~120 lines) | Seen in ≥2 sessions and needed in every session |
| Project skill | `.claude/skills/novent-*/SKILL.md` (body or `description`) | A procedure, not a one-line fact |

Team layers (knowledge, rules, hooks, CLAUDE.md, skills) change what every teammate's Claude does:
say so in the proposal.

## 4. Present — then stop and wait

```
### Đề xuất dream — <ngày> (<N> session, <M> lượt)

**1. [knowledge.md] <tiêu đề>** — lặp 2 lần
- Ghi: `<nội dung cuối cùng>`
- Bằng chứng: 0c3ddb6d:L84 "<trích nguyên văn ≤ 25 từ>"
- Vì sao: <chi phí tương lai nó ngăn>

Trả lời: `ok 1 3`, `sửa 2: …`, `bỏ 4`, hoặc `ok hết`.
```

Max ~10 items. Write nothing before the answer.

## 5. Apply what was approved

- Write exactly what was approved; prune per the `novent-memory` skill; keep INDEX in sync.
- Append to `.claude/memory/dream-log.md`: date, sessions, accepted, rejected (with reason).
- Commit in the game repo on the current feature branch, or on `chore/dream-<date>` when on
  `main`: `git add .claude && git commit -m "dream <date>: <items>"`. Team layers reach the others
  through that branch's PR — never push to `main`. Personal items go to auto memory, no commit.
- Report in 2–3 lines: what went where, commit hash, what was skipped.
