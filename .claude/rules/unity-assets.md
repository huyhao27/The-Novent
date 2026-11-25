---
paths:
  - "**/*.unity"
  - "**/*.prefab"
  - "**/*.asset"
  - "**/*.meta"
  - "ProjectSettings/**"
  - "Packages/**"
---

# Unity assets — change them through Unity, never as text

Scenes, prefabs, ScriptableObject assets and `.meta` files are YAML that Unity owns. A text edit
loads fine and then drops objects or breaks GUID references on every teammate's machine. The
`guard.py` hook blocks such edits; this is the why and the way round.

| Need | Do |
|---|---|
| Find an object / read a component | `mcp__UnityMCP__find_gameobjects`, `mcp__UnityMCP__manage_scene` (`get_hierarchy`, small page) |
| Add/modify a GameObject or component | `mcp__UnityMCP__manage_gameobject`, `mcp__UnityMCP__manage_components`, then `mcp__UnityMCP__manage_scene` `save` |
| Edit a prefab | `mcp__UnityMCP__manage_prefabs`, save the prefab stage |
| Create/move/rename/delete an asset | `mcp__UnityMCP__manage_asset` (keeps `.meta` + GUID) |
| Set ScriptableObject values | `mcp__UnityMCP__manage_scriptable_object` |
| Editor closed / no MCP | Give the user numbered click steps; do not fall back to YAML |
| Find who references a GUID | `grep -rl "<guid>" Assets --include=*.unity --include=*.prefab --include=*.asset` (read-only) |

- Shared scenes (MainMenu, Station, Expedition) have one editor at a time. Add a prefab and drop one
  instance in the scene instead of building inside the scene.
- New scene → add it to Build Settings in the same task.
- `ProjectSettings/` and `Packages/manifest.json` change the project for the whole team: only
  when the user asked, and say which setting/package changed.
- Never read `Library/`, `Temp/`, `Logs/`, `obj/` — generated, huge, useless.
