---
name: novent-test
description: Writes and runs Unity Test Framework tests for The Novent — EditMode for logic (bag/Khối, Tally, Oxy, save round-trip, request validation, seeded generation), PlayMode for lifecycle and scene flow — creating the test assembly if missing, running via MCP for Unity or batchmode, and reading failures. Load when new logic lands in Core or Gameplay, for a regression test on a bug fix, and on "viết test", "thêm unit test", "chạy test", "kiểm thử", "test có pass không", "TDD". Use it whenever tests are written, run or reported.
---

# Tests that prove game logic

A test is worth writing when it pins down a rule the game depends on and that a playtest would
catch late: a capacity, a cost, a timer, a save shape, who may change shared state.

Reply to the user in Vietnamese.

## When this matters

- New logic in `Novent.Core` or a gameplay module — the reviewer agent flags it without a test.
- A bug fix: write the failing test first, then fix; the test stops it coming back.
- Save format or a GDD number changes: the test is where the old value is caught.
- The defence asks how the game was tested; a green EditMode suite is the answer.

## 1. Find the testable unit

Logic belongs in a plain C# class (`InventoryModel`, `TallyLedger`, `OxygenTank`) that a
MonoBehaviour wraps. If the rule is inside `Update` of a MonoBehaviour, extract it first (one
small step, compile) — testing through a scene for arithmetic is slow and flaky.

## 2. Make sure the test assembly exists

`Assets/_Project/Tests/EditMode/Novent.Tests.EditMode.asmdef` (create if missing; Unity makes the `.meta`):

```json
{
  "name": "Novent.Tests.EditMode",
  "rootNamespace": "Novent.Tests",
  "references": ["Novent.Core", "UnityEngine.TestRunner", "UnityEditor.TestRunner"],
  "includePlatforms": ["Editor"],
  "overrideReferences": true,
  "precompiledReferences": ["nunit.framework.dll"],
  "autoReferenced": false,
  "defineConstraints": ["UNITY_INCLUDE_TESTS"]
}
```

Add the module under test to `references`. PlayMode: same in `Tests/PlayMode/` with name
`Novent.Tests.PlayMode` and `"includePlatforms": []`.

A test needs its own MonoBehaviour (a fake `Panel`, an `IPoolable`)? Put it in
`Assets/_Project/Tests/Support/` (assembly `Novent.Tests.Support`, already referenced). In the
Editor-only test assembly `AddComponent` fails with "it is an editor script" and the test then dies
on a NullReferenceException far from the cause. Set private serialized fields of a test asset with
`UnityEditor.SerializedObject` (see `Tests/EditMode/UI/PanelServiceTests.cs`).

## 3. Write the test

```csharp
using NUnit.Framework;
using Novent.Core.Events;

namespace Novent.Tests
{
    public class EventBusTests
    {
        private readonly struct PingEvent { public readonly int Value; public PingEvent(int v) { Value = v; } }

        [TearDown]
        public void TearDown() => EventBus<PingEvent>.Clear();

        [Test]
        public void Publish_AfterUnsubscribe_HandlerNotCalled()
        {
            var calls = 0;
            void Handler(PingEvent e) => calls++;
            EventBus<PingEvent>.Subscribe(Handler);
            EventBus<PingEvent>.Unsubscribe(Handler);

            EventBus<PingEvent>.Publish(new PingEvent(1));

            Assert.That(calls, Is.EqualTo(0));
        }
    }
}
```

- Name `Method_Condition_Expected`; Arrange / Act / Assert separated by a blank line.
- Numbers come from the GDD (cite it in the test name or one comment), not invented.
- Reset static state (services, event bus) in `[TearDown]`.
- PlayMode: `[UnityTest] public IEnumerator …` with `yield return null` per frame; create objects
  in code and destroy them in `[TearDown]`; never load a shared gameplay scene for a unit rule.
- Time: inject a clock or pass `deltaTime` in; no `Thread.Sleep`, no `WaitForSeconds` loops.

## 4. Compile, run, read

1. Compile: `mcp__UnityMCP__refresh_unity` → `mcp__UnityMCP__read_console` errors (the `unity-mcp` skill).
2. Run: `mcp__UnityMCP__run_tests` `{mode: "EditMode", include_failed_tests: true}` (add
   `assembly_names` to narrow). Tool missing → enable the `testing` group with
   `mcp__UnityMCP__manage_tools`. Poll `mcp__UnityMCP__get_test_job` until finished.
3. No Editor: `powershell -File .claude/skills/unity-mcp/scripts/unity-batch.ps1 -Project . -Mode editmode`,
   then `grep -oE 'total="[0-9]+"|passed="[0-9]+"|failed="[0-9]+"' Logs/test-results-editmode.xml`.
4. A failure: read the message and the first stack line in game code; fix the code (or the test if
   the test encoded a wrong rule — say which). Re-run only the failing assembly.

## Report

`Test: <N> pass / <M> fail (EditMode, <lệnh>)` plus one line per new test saying the rule it pins.
Not run this turn → "chưa chạy", never "sẽ pass".

## Common mistakes

| Instead of | Do this |
|---|---|
| Testing private methods via reflection | Test the public behaviour; extract a class if needed |
| One giant test per class | One rule per test |
| Asserting on `Debug.Log` text | Assert on state or a returned value |
| Loosening an assert until green | Find why the value differs; GDD number changed → say so |
| Tests that depend on order | Each test arranges its own state; `[TearDown]` resets statics |
| Starting a second `mcp__UnityMCP__run_tests` while one runs | Poll the first job; `clear_stuck` only if it hangs |
