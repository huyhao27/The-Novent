---
paths:
  - "Assets/_Project/**/*.cs"
---

# C# rules — The Novent

Examiners read this code at the defence and four developers edit it in parallel: consistent and
plain beats clever. Change what the task needs and nothing else.

## Naming

| Element | Convention |
|---|---|
| Types, methods, properties, events, consts, public fields | `PascalCase` |
| Private fields, including `[SerializeField] private` | `_camelCase` |
| Static private fields | `s_camelCase` |
| Locals, parameters | `camelCase` |
| Interfaces | `IThing`; ScriptableObject definitions `ThingDef`, configs `ThingConfig` |
| Bools | a question: `IsDead`, `_canInteract` |
| Methods | verb phrase: `SpawnWave`, `TryAddItem` (`Try…` returns bool + `out`) |
| Events | past tense, no `On` prefix on the event itself: `Died`, `ItemAdded`; handlers `HandleDied` |
| Event bus payloads | `readonly struct` ending in `Event`: `TallyChangedEvent` |
| Enums | singular; `[Flags]` plural; explicit values on anything serialized |
| Namespace | `Novent.<Module>` matching the folder under `Assets/_Project/Scripts/` |

One public type per file, file name = type name. Braces on their own line (Allman), 4 spaces.

## Comments

A comment says what the code cannot: why this and not the obvious alternative, a tuned number
and its source, a GDD rule ("GDD §Oxy: 12 s suffocation"), an engine workaround with its cause, a
trap for the next reader. Never: restating the next line, one-word section labels, commented-out
code, AI breadcrumbs (`// Added`, `// Fixed`, `// NEW`). New comments in English.
`<summary>` only on public API other modules call and on serialized fields whose unit is not obvious.

## Unity habits

- `[SerializeField] private` + read-only property, never a public field for the Inspector.
- Cache components in `Awake`. No `Find*`, `GetComponent`, LINQ, string concat or `new` of
  reference types in `Update`/`FixedUpdate`/`LateUpdate`.
- Subscribe in `OnEnable`, unsubscribe in `OnDisable` — always as a pair, same file, adjacent.
- Unity 6 APIs: `FindAnyObjectByType` / `FindObjectsByType(FindObjectsSortMode.None)` (not
  `FindObjectOfType`), `Rigidbody.linearVelocity` (not `velocity`), `Awaitable` for async
  (no coroutine ↔ async mixing inside one class). Input only through the Input System actions
  asset — `UnityEngine.Input` throws in this project.
- Tunable numbers live in a ScriptableObject or a serialized field with a `[Tooltip]` unit,
  never as a literal in logic. Numbers come from the GDD/Sheet, not invented.
- Null checks on `UnityEngine.Object` use `== null` / `!obj`, never `?.` or `??`
  (they skip Unity's destroyed-object check).
- `[RequireComponent]` for hard dependencies on the same GameObject.
- Renaming a serialized field → `[FormerlySerializedAs("old")]`. Serialized enums → append only.

## Diff discipline

No drive-by renames, reformatting, `using` reordering or regions. Commit messages say what and
why. Never add `Co-Authored-By: Claude` or any Claude attribution.

Architecture (where code goes, how modules talk) is in `architecture.md`; co-op authority in
`coop.md`. The full procedure lives in the `novent-code-style` skill.
