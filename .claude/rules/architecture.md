---
paths:
  - "Assets/_Project/Scripts/**"
  - "Assets/_Project/**/*.asmdef"
---

# Architecture — modules, services, one-way dependencies

Four developers work in parallel and the netcode is not chosen yet. The structure keeps each
feature in its own folder, gameplay rules in plain C# (testable, netcode-free), and every
shared service created in one place. Copy the shape of an existing sibling before inventing one.

## Modules (one asmdef each, namespace = asmdef name + subfolder)

| Folder `Assets/_Project/Scripts/` | Assembly | May reference | Holds |
|---|---|---|---|
| `Core/` | `Novent.Core` | Unity only | `Services`, `EventBus<T>`, `PoolService`, `SaveService`, `SeededRandom`, `INetSession`/`OfflineSession`, `PlayerId` |
| `Gameplay/<Feature>/` | `Novent.Gameplay` | Core | Feature rules + MonoBehaviour views: `Player/`, `Oxygen/`, `Inventory/`, `Station/`, `Trade/`, `Driftlife/`… |
| `UI/` (+ `UI/<Feature>/`) | `Novent.UI` | Core, Gameplay | `Panel`, `PanelService`, `PanelRegistry`, `UIRoot`, feature panels |
| `Net/` (+ `Net/<Feature>/`) | `Novent.Net` | Core, Gameplay, netcode package | `INetSession` implementation, networked adapters. The only place netcode types appear |
| `Boot/` | `Novent.Boot` | everything | `GameBootstrap` — the composition root |
| `Editor/` | `Novent.Editor` (Editor only) | anything | Tools, importers (Sheet → SO), cheats |

Dependencies point down the table and never back: Core knows nothing of Gameplay, Gameplay knows
nothing of UI or Net. A missing asmdef for `Gameplay`/`Net`/`Editor` is created by the first file
that needs it — templates in the `novent-architecture` skill.

## Services

- Created and registered **only** in `GameBootstrap` (runs before the first scene, every Play),
  except scene-owned ones (`UIRoot` registers `PanelService` and unregisters on destroy).
- Read with `Services.Get<T>()` in `Awake`/`Start`/constructor and cache it; never in field
  initializers, never per frame. A missing service throws a named error — fix the registration.
- A service is a plain C# class. An interface only where two implementations exist or a test
  needs a fake (`ISaveStore`, `INetSession`).
- No new singleton `MonoBehaviour` managers and no `FindAnyObjectByType` to reach systems.

## Communication

| Need | Use |
|---|---|
| A system asks another to do something | Direct call on the service/component it holds a reference to |
| Many listeners react to something that happened (UI, audio, story flags) | `EventBus<TEvent>.Publish(new XxxEvent(...))`; `readonly struct` events in `<Feature>Events.cs` beside the publisher |
| Inspector wiring inside one prefab | `[SerializeField]` reference |
| Player/UI changes shared state | `RequestXxx(...)` on the gameplay system → host validates → `ApplyXxx` (see `coop.md`) |

The event bus is local to one machine — never treat it as network sync. Subscribe in `OnEnable`,
unsubscribe in `OnDisable`.

## Data

- Design numbers: one ScriptableObject per feature (`OxygenConfig`, `DriftlifeDef`) under
  `Assets/_Project/Configs/`, imported from the GDD Sheet when available. SOs are read-only at
  runtime; runtime state lives in plain C# objects.
- Saved state: a `[Serializable]` `<Feature>SaveData` class written through `SaveService` with a
  stable key. World slot belongs to the host; each player keeps a profile slot. Changing a saved
  shape → bump `SaveService.CurrentVersion` and add a migration step.
- The world seed is saved with the world; the session for that world is created from it, never
  from a fresh `TickCount`, so a reloaded world regenerates the same map and loot.
- IDs designers edit are strings; enums stored as ints are append-only.

## Pools and spawning

`PoolService` is for local cosmetics (VFX, damage numbers, debris). Networked objects are
spawned by the host through the netcode library.

## Scenes

`Assets/_Project/Scenes/`: `MainMenu`, `Station`, `Expedition` (+ test scenes under `Scenes/Dev/`).
Shared scenes hold prefab instances, not hand-built hierarchies. Every scene must work when
played directly — `GameBootstrap` runs before any scene, so services are always there.
