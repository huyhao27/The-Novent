---
name: novent-implement
description: Executes an approved plan or a small coding task in The Novent one compiled step at a time — edit, refresh through MCP for Unity, read errors, fix, next — and finishes the Unity side (component on the prefab, references assigned, asset created, scene in Build Settings). Load as soon as work moves to writing code, including bare follow-ups "implement đi", "làm đi", "code đi", "tiếp đi", "sửa luôn đi", "bắt đầu đi", and "thêm script", "viết controller". Use it whenever code is about to be written or edited.
---

# Implementation — one step, one compile

Do one step, prove it compiles, then do the next. In Unity most of the work is not in the
`.cs` file — a script that compiles still does nothing until it is on a GameObject, its
references are assigned and its scene is in the build.

Reply to the user in Vietnamese.

## When this matters

- Ten edits then one compile turns a typo into a 20-error cascade across files.
- There is no CI. The only compiler is the Unity Editor (or batchmode).
- The deadline is fixed: a feature that compiles but is not wired costs a demo, not a review.
- The game is online co-op: a feature that only works on the host is half a feature.
- Four developers share the repo: an unpulled branch or a scene edited by two people costs a day.

## Before the first edit

`git status` and `git pull` (or `fetch` + compare) on the game repo. Work on a feature branch, not
`main`, unless the team decided otherwise in `knowledge.md`. If the target files changed upstream
or belong to a teammate's open work, stop and tell the user.

## The loop — repeat per step

1. Make the edit for exactly one step.
2. Compile: `mcp__UnityMCP__refresh_unity` (`scope: scripts, compile: request`), then
   `mcp__UnityMCP__read_console` filtered to errors. No MCP → ask the user to focus Unity and
   paste the Console, or use batchmode: `powershell -File .claude/skills/unity-mcp/scripts/unity-batch.ps1 -Project . -Mode compile`. Never assume the result.
3. Fix until zero errors. Later errors are usually cascades of the first.
4. Only then start the next step.

## Before writing a new class

Run the reuse search from the `novent-context` skill, then place the type with the
`novent-architecture` skill (module, asmdef, Core service to reuse). Copy the closest sibling's
shape rather than inventing a new pattern. New logic in a Model gets an EditMode test (`novent-test`).

Keep the diff minimal: no drive-by renames, reformatting or `using` reordering. For comments
and naming, use the `novent-code-style` skill.

## Unity execution rules

| Situation | What actually has to happen |
|---|---|
| New MonoBehaviour | Add it to the GameObject/prefab (MCP `mcp__UnityMCP__manage_components`) and assign its references, or give the user exact click steps |
| New ScriptableObject type | Add `[CreateAssetMenu]`, create the asset (`mcp__UnityMCP__manage_asset`), assign it where it is read |
| New scene | Add it to Build Settings (`mcp__UnityMCP__manage_build` action `scenes`), or `SceneManager.LoadScene` fails in the build |
| Wiring into a scene/prefab | `mcp__UnityMCP__manage_gameobject` / `mcp__UnityMCP__manage_prefabs`, then save. Never hand-edit `.unity`/`.prefab`/`.asset` YAML |
| Moving or deleting a `.cs` / asset | Move or delete its `.meta` with it (or use `mcp__UnityMCP__manage_asset`) — otherwise every reference breaks |
| Renaming a `[SerializeField]` field | Add `[FormerlySerializedAs("old")]`, or the value is silently dropped everywhere |
| Changing an enum that is serialized | Append only — reordering changes stored int values |
| Changing the save data shape | Version it and keep loading old saves, or say the old save is discarded |
| Input | Use the project's input setup detected by `novent-context`; do not mix `Input.*` into a new-Input-System project |
| Shared state (station, storage, Tally, story, port rank, Driftlife, crates) | Changed only on the server/host; clients send a request (RPC) and see the result through synced state. Decide shared vs per-player first (`.claude/rules/coop.md`) |
| Per-player state (Oxygen, bag, tools) | Owned by that player; anything that affects others (share Oxygen, hand over an item) is validated by the server |
| New networked prefab (NGO) | Add `NetworkObject`, register it in the NetworkManager's prefab list, spawn it from the server — `Instantiate` alone exists on one machine only |
| Persistent synced values | `NetworkVariable` (late joiners receive it); one-shot RPCs only for effects (VFX, sounds) |
| Randomness affecting the world | Seed from the host; never `UnityEngine.Random` independently on each client |
| Singleton managers | Exists once per machine — say whether it is server-only logic or a local view |
| A hook answers `[novent-guard]` deny | The edit targeted Unity YAML, a package or a forbidden API — use the MCP tool or the replacement it names; never route around it with Bash |
| Commit | Small commits on the feature branch, message says what and why. **No `Co-Authored-By: Claude` or other Claude attribution** |

## Stop and report instead of improvising

- The plan rests on an assumption that turned out false.
- The step needs an edit inside a package or third-party pack — wrap it game-side instead.
- The same fix failed twice — switch to the `novent-debug` skill, do not try a third variation.

## Report at the end

1. **Files changed** — one line each: what and why.
2. **Verified** — what was checked, with which command. Use the `novent-verify` skill.
3. **Not verified** — what the user must check in Unity, with exact clicks (scene, object, field, expected result).

## Common mistakes

| Tendency | Do instead |
|---|---|
| Write all files, compile once | Compile after every step |
| "Đã tạo `AudioManager`" while the scene has none | Put it in the scene/bootstrap prefab, or list the click steps |
| Hand-edit prefab YAML | `mcp__UnityMCP__manage_components`, or hand it to the user |
| Rename a serialized field while cleaning up | Leave it, or add `[FormerlySerializedAs]` |
| Add a scene and load it by name | Add it to Build Settings first |
| A third fix after two failures | Stop, report what each attempt disproved |
| Change Tally/storage directly in a UI button handler | Send a request to the server; UI reads the synced value |
| `Instantiate` a networked object on a client | Spawn from the server |
| Large edit to a shared scene | Build it as a prefab, add one instance to the scene |
