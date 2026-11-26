---
name: unity-mcp
description: How to drive the Unity Editor through MCP for Unity (server UnityMCP) cheaply and safely — the compile→console loop, tests, play mode, scene/prefab/component inspection and edits, screenshots — plus the batchmode fallback when the Editor is closed. Load it before calling any mcp__UnityMCP__ tool, when the Editor must compile, run tests, enter play mode or change a scene/prefab, and on "compile thử", "chạy test", "xem console", "bấm play", "sửa prefab", "thêm component", "chụp màn hình scene", "MCP không kết nối". Use it whenever Unity Editor state must be read or changed.
---

# Driving Unity through MCP

The Editor is the only compiler and the only source of truth for scenes and prefabs. Ask
it, do not guess — but ask narrowly, because every MCP response lands in the context.

Reply to the user in Vietnamese.

## 0. Is the bridge up?

`claude mcp list` shows `UnityMCP: http://127.0.0.1:8080/mcp (HTTP)`. If it says failed:
the Editor is closed, still importing, or the bridge is stopped (Unity menu
`Window > MCP for Unity` → Start Session). Say which and stop — do not fall back to editing
scene YAML. For compile/test checks without the Editor, use section 5.

One Editor holds the project lock (`Temp/UnityLockfile`). A git worktree is a different
project folder: it needs its own Editor or batchmode, never both.

**Several Editors open (parallel worktrees).** All Editors register with the one HTTP server
as `<ProductName>@<hash-of-project-path>`. With more than one connected, the server refuses
to guess: call `mcp__UnityMCP__set_active_instance` first with the instance for *your*
working directory (`.claude/worktree.md` names it when present; otherwise
read the resource listing instances and pick the one whose path matches `pwd`). Each
Claude session keeps its own active instance.

**Tool groups.** Only the core tools are registered by default. `mcp__UnityMCP__run_tests`/`mcp__UnityMCP__get_test_job`
live in the `testing` group and `mcp__UnityMCP__execute_code` in a scripting group; if a tool below is
missing, enable its group with `mcp__UnityMCP__manage_tools` rather than assuming it does
not exist. Enable only what the task needs — every registered tool costs context.

## 1. The compile loop — after every C# edit batch of one step

0. `mcp__UnityMCP__read_console` `{action: "clear"}` so old errors are not mistaken for new ones.
1. `mcp__UnityMCP__refresh_unity` `{scope: "scripts", compile: "request", wait_for_ready: true}`.
2. `mcp__UnityMCP__read_console` `{action: "get", types: ["error"], count: 20,
   include_stacktrace: false}`. Add `filter_text` with the file name when the log is noisy;
   add `"warning"` only when asked.
3. Zero errors → next step. Errors → fix the first one, repeat. Later errors are often
   cascades of the first.

Domain reload drops the connection for a few seconds; retry once, then wait.

## 2. Tool map — pick the narrowest tool

| Need | Tool (action) |
|---|---|
| Is it compiling / playing / importing? | resource `get_editor_state` |
| Enter / leave play mode | `mcp__UnityMCP__manage_editor` (`play`, `stop`, `pause`) |
| Run tests | `mcp__UnityMCP__run_tests` `{mode: "EditMode", include_failed_tests: true}` (filter with `test_names`/`assembly_names`) → job id → poll `mcp__UnityMCP__get_test_job` `{job_id}` |
| Find objects | `mcp__UnityMCP__find_gameobjects` (by name/tag/component/path, paged) |
| Scene hierarchy / save / load | `mcp__UnityMCP__manage_scene` (`get_hierarchy`, `save`, `load`, `get_build_settings`) |
| Add/remove scenes in Build Settings | `mcp__UnityMCP__manage_build` (action `scenes`) |
| Screenshot of the Game or Scene view | `mcp__UnityMCP__manage_camera` (`screenshot`; `capture_source: scene_view` for the Scene view; `include_image: true` to see it inline) |
| Create / modify / delete a GameObject | `mcp__UnityMCP__manage_gameobject` (`create`, `modify`, `delete`, `duplicate`) |
| Add / remove / set a component field | `mcp__UnityMCP__manage_components` |
| Prefab open / edit / save | `mcp__UnityMCP__manage_prefabs` |
| Search / create / move / rename assets (keeps `.meta` + GUIDs right) | `mcp__UnityMCP__manage_asset` (`search`, `create`, `move`, `rename`, `delete`, `get_info`) |
| Set ScriptableObject fields | `mcp__UnityMCP__manage_scriptable_object` |
| Create / patch a C# file with validation | `mcp__UnityMCP__manage_script` (`create`, `validate`, `mcp__UnityMCP__apply_text_edits`) — plain `Edit` on `.cs` is fine too; then compile |
| Many small edits | `mcp__UnityMCP__batch_execute` — one round-trip instead of ten |
| Menu command (e.g. custom tool, Addressables build) | `mcp__UnityMCP__execute_menu_item` |
| Profiler / rendering stats | `mcp__UnityMCP__manage_profiler`, resource `get_rendering_stats` |
| Build player | `mcp__UnityMCP__manage_build` |

`mcp__UnityMCP__execute_code` runs arbitrary C# inside the Editor. Use it only for
read-only queries no other tool answers, show the snippet in the reply, and never use it to
delete assets, touch files outside the project, or change `ProjectSettings`.

## 3. Rules that save hours

| Rule | Why |
|---|---|
| Change scenes/prefabs through MCP or hand the user click-by-click steps; never edit `.unity`/`.prefab`/`.asset` YAML | fileIDs and GUIDs will not line up; Unity may silently drop the object |
| After changing a scene: `mcp__UnityMCP__manage_scene` `save`. After a prefab: save the prefab stage | Unsaved changes vanish on the next domain reload or when another agent reloads |
| Move/rename assets with `mcp__UnityMCP__manage_asset`, not `mv` | `mv` without the `.meta` breaks every reference |
| Renaming a `[SerializeField]` field → add `[FormerlySerializedAs("old")]` | Otherwise the value is dropped in every prefab and scene |
| Ask for a page, not the world: small `count`/`pageSize`, a filter, a target path | A full hierarchy dump of a big scene costs tens of thousands of tokens |
| Stop play mode before editing scripts or scenes | Changes made in play mode are discarded |
| `mcp__UnityMCP__run_tests` is async — poll `mcp__UnityMCP__get_test_job` until finished; do not start a second run | Overlapping runs get stuck; `mcp__UnityMCP__run_tests` with `clear_stuck` recovers |

## 4. Screenshots and "does it look right?"

`mcp__UnityMCP__manage_camera` `screenshot` gives an image of the Game view (overlay UI included when no
camera is named) or, with `capture_source: scene_view`, the Scene view; `include_image: true` returns
it inline (≤ 640 px by default).
Use it to verify layout/UI changes; describe what you see and what you expected. A
screenshot is evidence for "it renders", not for "the logic works".

## 5. Fallback: batchmode (Editor closed, worktree, overnight)

Batchmode cannot run while an Editor has the same project open (lock file). Use the
kit's script, which wraps these:

```powershell
# compile only (exit code != 0 on compile errors), log to Logs/batch-compile.log
powershell -File .claude/skills/unity-mcp/scripts/unity-batch.ps1 -Project . -Mode compile
# EditMode tests → Logs/test-results-editmode.xml (NUnit XML)
powershell -File .claude/skills/unity-mcp/scripts/unity-batch.ps1 -Project . -Mode editmode
```

Read the log with `grep -n "error CS" Logs/batch-compile.log | head -20`, and the result XML
with `grep -oE 'total="[0-9]+"|failed="[0-9]+"' Logs/test-results-editmode.xml`. A fresh
worktree has no `Library/`, so the first batchmode run reimports everything (minutes to
tens of minutes) — budget for it and do not mistake it for a hang.

## Common mistakes

| Instead of | Do this |
|---|---|
| "It should compile" | `mcp__UnityMCP__refresh_unity` + `mcp__UnityMCP__read_console` errors, this turn |
| Dumping the full hierarchy to find one object | `mcp__UnityMCP__find_gameobjects` by name/component |
| Editing prefab YAML to add a component | `mcp__UnityMCP__manage_components` on the prefab |
| Reading 500 console lines including warnings | errors only, small count, filtered |
| Starting play mode, then editing scripts | stop first |
| Treating a closed Editor as "MCP broken" and editing YAML | say so; use batchmode for compile/tests |
