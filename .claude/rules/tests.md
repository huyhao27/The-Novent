---
paths:
  - "Assets/_Project/Tests/**"
  - "Assets/_Project/**/*Tests.cs"
---

# Tests — Unity Test Framework

Tests are the only verification that runs without a human in Play mode, and the defence asks how
the game was tested. Test the logic that decides outcomes; leave feel and visuals to playtests.

## Where

| Kind | Folder / assembly | Runs |
|---|---|---|
| EditMode — pure logic, data, save round-trips | `Assets/_Project/Tests/EditMode/` · `Novent.Tests.EditMode.asmdef` (Editor platform only) | Fast, no scene |
| PlayMode — MonoBehaviour lifecycle, physics, scene flow | `Assets/_Project/Tests/PlayMode/` · `Novent.Tests.PlayMode.asmdef` | Slower, enters play mode |

Test asmdefs reference the module under test plus `UnityEngine.TestRunner`,
`UnityEditor.TestRunner`, and set `"overrideReferences": true` with `nunit.framework.dll`,
`"defineConstraints": ["UNITY_INCLUDE_TESTS"]`. One test class per production class:
`<Class>Tests`, methods `Method_Condition_Expected`.

## What to test first

Inventory/bag capacity (Khối), Tally arithmetic and costs, Oxy drain and suffocation timer,
save/load round-trip and version migration, request validation (a client cannot spend shared
Tally), seeded generation (same seed → same chunks), event bus subscribe/unsubscribe.

## Rules

- Keep logic in plain C# classes so EditMode tests need no scene. A test that needs a scene
  for arithmetic means the logic is in the wrong place.
- No `Thread.Sleep`, no real time: inject the clock/delta, or use `[UnityTest]` with
  `yield return null` frames in PlayMode.
- Reset static state in `[SetUp]`/`[TearDown]` (service registry, event bus) — tests leak
  into each other otherwise.
- Run: `mcp__UnityMCP__run_tests` `{mode: "EditMode"}` → poll `mcp__UnityMCP__get_test_job`;
  without the Editor: `.claude/skills/unity-mcp/scripts/unity-batch.ps1 -Mode editmode`.
- A red test is fixed in the code, not by loosening the assert — unless the GDD number changed.
