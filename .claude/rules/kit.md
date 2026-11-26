---
paths:
  - ".claude/skills/**"
  - ".claude/rules/**"
  - ".claude/agents/**"
  - ".claude/hooks/**"
  - ".claude/CLAUDE.md"
  - ".claude/settings.json"
---

# Editing the team's AI kit

Every line here is loaded by four teammates' sessions; a bad trigger or a long description costs
every one of them. Run `python .claude/tools/check_kit.py` after any change — it must print `OK`.

| Put it in | When |
|---|---|
| `CLAUDE.md` (≤ ~120 lines) | Needed in every session: stack, where code goes, workflow |
| `rules/<area>.md` with `paths:` | A rule tied to a folder or file type — loads only when such a file is read/edited |
| `skills/novent-<x>/SKILL.md` | A multi-step procedure; loads on demand |
| `hooks/guard.py` | A rule that must never be broken (deny/ask before the edit) |
| `hooks/skill_router.py` `ROUTES` | A real phrase that should have loaded a skill and did not |
| `agents/<name>.md` | Work with long output (search, review) that should stay out of the main context |
| `memory/` | Facts and decisions, not procedures |

Skill rules:
- Frontmatter has only `name` (= folder name) and `description` (≤ ~600 chars, no `": "` inside,
  third person, what + when, the team's real Vietnamese phrases, ends with "Use it whenever …").
- Body < 300 lines, English, imperative, one clause of *why*; `Reply to the user in Vietnamese.` near
  the top; tables for lookup, prose for judgment; templates the model shows the user in Vietnamese.
- `references/` one level deep; a reference over 100 lines starts with `## Contents`.
- MCP tools by full name (`mcp__UnityMCP__read_console`); paths with `/`; no machine paths
  (`D:/…`) and no personal names — use `<repo>` or relative paths.
- Cross-reference another skill by name ("use the `novent-test` skill"), never `@path`.
- A rule that only matters at the end of work (verify) needs an action trigger in `CLAUDE.md`;
  descriptions alone do not load it.
