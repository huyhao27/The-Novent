---
name: novent-verify
description: Blocks false "done" claims in The Novent — requires evidence produced this turn — MCP compile + console, test results, inspected scene/prefab, play-mode screenshot — and splits the report into verified vs what the user must check in Unity, including the two-player co-op check. Load at the start of any turn that edits .cs, prefab, scene or ScriptableObject, including "implement đi", "sửa luôn đi", "tiếp đi", and on "xong chưa", "chạy được không", "chắc chưa", "double check". Use it whenever a change is about to be reported as done.
---

# Verification gate

No completion claim without fresh evidence produced in this turn. "It should compile" is not
compiling.

Reply to the user in Vietnamese.

## The gate

1. Name the command or observation that would prove the claim.
2. Run it now — evidence from an earlier turn expired when the code changed.
3. Read the whole output, including error and failure counts.
4. Only then state the result. Not run this turn → say it is unverified.

## What counts as evidence

| Claim | Counts | Does not count |
|---|---|---|
| Compiles | `mcp__UnityMCP__refresh_unity` + `mcp__UnityMCP__read_console` → 0 errors; batchmode log with no `error CS` (`powershell -File .claude/skills/unity-mcp/scripts/unity-batch.ps1 -Project . -Mode compile`); the user's clean Console | "syntax looks right" |
| Tests pass | `mcp__UnityMCP__run_tests` → poll `mcp__UnityMCP__get_test_job` → 0 failures; or `unity-batch.ps1 -Mode editmode` → `failed="0"` in `Logs/test-results-editmode.xml` | a previous run |
| No broken `.meta` / GUIDs / conflict markers | `unity projects verify <project> --json` → `data.ok: true`, empty `findings` | "git status looks fine" |
| Feature works | Play mode (`mcp__UnityMCP__manage_editor` `play`) + Console/screenshot showing the behaviour, or the user confirming | the method is called somewhere |
| Scene/prefab change applied | `mcp__UnityMCP__find_gameobjects` / `mcp__UnityMCP__manage_scene` `get_hierarchy` shows it, and `save` succeeded | YAML was edited |
| Reference wired | The Inspector value read back via MCP, or no `UnassignedReferenceException` in play | the field exists |
| Scene loads in build | It is in `get_build_settings` | `LoadScene("Name")` compiles |
| Save/load works | Save, stop, play again, value restored | `JsonUtility.ToJson` returns text |
| Bug fixed | The original repro now absent | the code looks better |
| Works in co-op | Host + 1 client (Multiplayer Play Mode 2.0 virtual player, or a second build): the change is visible and correct on **both**, and on a client that joins late | it works on the host |
| Shared state is authoritative | A client trying the action changes the value only via the server; both sides read the same number afterwards | the RPC exists |
| Merged cleanly | `git status` clean after pull; scene opens without missing references | "no conflict markers" |
| Nothing else broke | Diff reviewed file by file; main scene still enters play mode without errors | "the change was small" |

No MCP and no batchmode → ask the user for the Console, and say that is what you are doing.

Claude usually cannot drive two game instances. A co-op check you did not run goes under
"Chưa verify" with exact steps for host and client — never folded into "đã chạy".

## Report honestly

```
Đã verify:
- Compile sạch: refresh_unity + read_console → 0 errors.
- <check khác đã chạy, kèm lệnh>

Chưa verify (cần bạn mở Unity):
- <claim>: mở scene <path>, bấm Play, <thao tác>, kỳ vọng <kết quả>.
```

## Red flags in your own output

"should", "probably", "chắc là được", "về cơ bản là xong", "Xong rồi!" before a check — delete
and go run the check. A subagent saying "done" is not evidence; read its diff yourself.
