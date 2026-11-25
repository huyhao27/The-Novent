---
name: novent-context
description: Ground truth for The Novent (team Unity 6 co-op graduation project) — what memory says, what the project already has (Core services, packages, packs), who owns what, and the reuse-first search, so nothing is rebuilt and nothing collides with a teammate. Load at the start of any task that reads or changes code, scenes or design docs, and on "project có gì rồi", "đã có script nào chưa", "tìm chỗ xử lý", "đọc tài liệu đồ án", "cốt truyện thế nào". Use it whenever work touches The Novent.
---

# The Novent — project context

Know what the project already has and what the team already decided before touching anything.
The expensive failures here are rebuilding something that exists, writing shared state
single-player-only, colliding with a teammate's open work, and spending days on what the defence
does not need.

Reply to the user in Vietnamese.

## 1. Memory — the brief is already in context

The `SessionStart` hook injected the memory INDEX and your last session log. From there:

1. Open `knowledge.md` (team decisions, traps) and `project-overview.md` (scope, systems, owners,
   milestones) when the task touches a decision, a system or a schedule.
2. Open only the other rows that match the task (`story.md` for narrative work).
3. Design rules: the GDD in `Docs/` — main tabs (v0.5). `v0.1 …` tabs are an abandoned concept;
   never take numbers from them. Every system section in the `.docx` ends with "Two-player Co-op
   and Acceptance Criteria" and "Edge Cases" — read those before coding that system.
4. Say back in 2–3 sentences what you understood. If memory contradicts the code, the code wins —
   fix the memory line (the `novent-memory` skill).

## 2. What already exists — reuse before writing

| Need | Already in the project |
|---|---|
| Reach a shared system | `Services.Get<T>()` (registered in `Scripts/Boot/GameBootstrap.cs`) |
| Notify many listeners | `EventBus<T>` (`Scripts/Core/Events/`) |
| Short-lived local objects | `PoolService` |
| Persist anything | `SaveService` + a `[Serializable]` section (`FileSaveStore` / `MemorySaveStore`) |
| Randomness that must match on every machine | `SeededRandom` forked from `INetSession.WorldSeed` |
| Am I host? who is playing? | `INetSession` (`OfflineSession` until the netcode lands) |
| Screens and popups | `Panel` + `PanelService` + `PanelRegistry` + `UIRoot` |
| Unity packages | `grep -oE '"com\.[a-z0-9.-]+"' Packages/manifest.json` (URP 17.3, Input System 1.19, Test Framework 1.6, Timeline, AI Navigation, MCP for Unity) |

Before any new class, manager, UI element or shader:

1. `grep -rn --include=*.cs -i "<noun>" Assets/_Project` — game code.
2. `grep -rli --include=*.cs "<noun>" Assets/Plugins Library/PackageCache 2>/dev/null | head` — packs and packages.
3. `project-overview.md` § systems list for the agreed owner.
4. Nothing found → write it where the `novent-architecture` skill says.

A search across many folders → the `novent-explorer` agent (returns `file:line`, keeps the noise out).

## 3. Who is working where

`git fetch -q && git log --oneline -8 --all -- <folder>` and `git branch -r` before editing a
system. Commits on another branch touching the same folder mean a teammate is there: say who
(`git log --format='%an %ar' -3 -- <folder>`) and ask before editing. Shared scenes have one
editor at a time.

## 4. Settings the code never mentions

Unity 6000.3.15f1 · URP (shaders/materials must be URP; post-processing via Volumes) · Input System
only (`UnityEngine.Input` throws) · Force Text. Netcode not chosen — see `knowledge.md`; write
shared state with one owner anyway (`.claude/rules/coop.md`). If `ProjectSettings/ProjectVersion.txt`
or `Packages/manifest.json` no longer match this, the files win — update `knowledge.md`.

## 5. Token discipline

| Rule | Why |
|---|---|
| Grep the member, read ±40 lines | Whole-file reads of big classes burn the window |
| Broad search → `novent-explorer` agent | Search output stays out of the main context |
| Scenes/prefabs via `mcp__UnityMCP__find_gameobjects` / `mcp__UnityMCP__manage_scene` (small pages) | `.unity`/`.prefab` YAML is huge and unreadable |
| Never open `.meta` (except one `guid:` line), `Library/`, `Temp/`, `*.csproj` | Generated noise; settings deny most of it |
| GDD: grep a converted `.md` in `Docs/` if present; convert a `.docx`/`.xlsx` once and keep the `.md` | Re-reading a 40-page GDD each session costs more than the task |

## Common mistakes

| Instead of | Do this |
|---|---|
| Listing `Assets/` recursively | The reuse greps above |
| A new `XxxManager` singleton | A Core service or the `novent-architecture` slot |
| Editing a pack's prefab in place | Duplicate into `Assets/_Project`, edit the copy |
| Building polish before the core loop runs end to end | Check the milestone in `project-overview.md` |
| Treating `story.md` as final | Read each section's status line |
| "Làm offline trước, mạng tính sau" for shared state | One owner from day one; solo = host with no clients |
| Taking numbers from a `v0.1` tab | The v0.5 tab of the same topic |
