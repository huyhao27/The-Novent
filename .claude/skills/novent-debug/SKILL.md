---
name: novent-debug
description: Finds the root cause of a bug in The Novent before fixing it — real Console error first, narrow to one file or asset, one-sentence cause, fix at the cause, re-check the symptom — with diagnostics for Unity and co-op bug classes (unassigned reference, double subscription, execution order, missing service, Editor-vs-build, host-only logic, late-join desync). Load on pasted NullReferenceException, MissingReferenceException, error CS…, and on "bị lỗi", "không chạy", "sao nó null", "bắn 2 lần", "máy kia không thấy", "fix giúp". Use it whenever something is reported broken.
---

# Debugging

Find the mechanism before writing the fix. If you cannot say why it happens, you do not know
your change fixes it — only that the symptom moved.

Reply to the user in Vietnamese.

## The loop

1. **Reproduce** — get the actual error: `mcp__UnityMCP__read_console` (errors), or ask the user to paste the Console. For co-op bugs, ask which side shows it (host, client, late joiner) and get both Consoles.
2. **Narrow** — to one file, one line, or one asset.
3. **Root cause** — one sentence, before any edit.
4. **Fix at the cause**, not the symptom.
5. **Verify the original symptom is gone** — the `novent-verify` skill.
6. **Check siblings** — the same missing reference usually exists in the next prefab too.

## Narrowing in Unity

- `git log -p` / `git diff` on the touched folder: what changed since it last worked.
- Disable the suspect component in play mode; see if the symptom changes.
- One temporary `Debug.Log` at the decision point — delete it before closing.
- **Suspect the asset when the code reads correct.** Unassigned field, wrong layer, missing
  collider, inactive object: check with `mcp__UnityMCP__find_gameobjects` before rewriting logic.

## Bug classes

| Symptom | Most likely cause | Diagnostic |
|---|---|---|
| `NullReferenceException` on a `[SerializeField]` | Not assigned in this prefab/scene, or field renamed (value dropped) | Inspector via MCP; `git diff` for a rename |
| `MissingReferenceException` | Object destroyed (scene change) but still referenced by a static/event | Find who holds it; unsubscribe in `OnDisable`/`OnDestroy` |
| Event handler runs twice / N times | Subscribed in `OnEnable`/`Start` without matching unsubscribe; or a `DontDestroyOnLoad` duplicate after reloading the scene | Count subscribe vs unsubscribe; search hierarchy for duplicates |
| Works sometimes, null at start | Execution order: reading another object in `Awake` before it initialised | Move the read to `Start`, or set Script Execution Order deliberately |
| Collision/trigger never fires | No `Rigidbody` on either side, `isTrigger` mismatch, 2D vs 3D components mixed, layer matrix off | Inspect both objects' components and layers |
| Input does nothing | Wrong input backend, action map not enabled, UI `EventSystem` missing or using the old module | Check `activeInputHandler` and the EventSystem |
| UI click ignored | Another raycast target on top, no `GraphicRaycaster`, `CanvasGroup.blocksRaycasts` | Screenshot + hierarchy order |
| Animation stuck | Transition has exit time / wrong condition, parameter name typo (string) | Read the Animator parameters and transitions |
| Works in Editor, fails in build | Scene not in Build Settings, `Resources`/path case-sensitivity, `#if UNITY_EDITOR` code, IL2CPP stripping | Read the Player log, not the Editor Console |
| Save not restored | Saved to a path that differs between Editor and build, or loaded before the system initialised | Log `Application.persistentDataPath` and load order |
| Works on host, nothing happens on client | Logic runs only where it has authority; client changed local state instead of requesting the server; RPC rejected by ownership | Log `IsServer`/`IsOwner` at the decision point |
| Client sees a different value than host | Value set locally on a client, or kept in a plain field instead of synced state; independent `Random` | Compare the value on both instances; find every writer |
| Late joiner missing state | State sent as a one-shot RPC instead of persistent synced state | Join a client after the change |
| Object exists on host only / "prefab not registered" | `Instantiate` instead of server spawn; prefab missing from the NetworkManager list | NetworkManager prefab list; spawn call site |
| Action happens twice in co-op | Both host and client run the same gameplay logic | Gate the logic to the server |
| `Service X is not registered` | Not added in `GameBootstrap`, or a scene-owned service (`PanelService`) read in a scene without `UIRoot` | `Scripts/Boot/GameBootstrap.cs`; `mcp__UnityMCP__find_gameobjects` for `UIRoot` |
| `subscribed twice to …Event` warning | `OnEnable` ran again without `OnDisable` (object re-enabled, duplicate component) | Count subscribe/unsubscribe pairs in that class |
| Old handlers or values survive into the next Play | Enter Play Mode Options without domain reload: a static was not reset | Reset it in `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` |
| Scene/prefab broken after pull | Two people edited the same scene; merge kept one side | `git log` on the asset; re-apply the lost change via prefab |

## Two failed fixes → stop

Report what was tried and what each ruled out, the mechanism you now believe, and the one
piece of information you need (a Console paste, a screenshot, a value in the Inspector).

## Before closing

Remove temporary logs. If the cause was non-obvious, record it in `knowledge.md` with the
`novent-memory` skill so the next session does not rediscover it.
