---
name: novent-architecture
description: Places new code in The Novent's module structure and reuses its Core services — Services/GameBootstrap, EventBus, PoolService, SaveService, SeededRandom, INetSession, Panel/PanelService/UIRoot — when adding a feature folder, service, UI screen or popup, event, config ScriptableObject, saved data or asmdef. Load before creating a new class or folder under Assets/_Project/Scripts, and on "thêm hệ thống", "tạo module", "làm màn hình UI", "thêm popup", "lưu dữ liệu", "thêm service", "kiến trúc", "asmdef". Use it whenever a new type or folder is about to be created.
---

# Put new code where the structure expects it

The structure exists so four people can add features without touching each other's files and
so the netcode can still change. Reuse a Core service before writing a manager; copy a sibling
feature's shape before inventing one. The invariants are in `.claude/rules/architecture.md`
(auto-loaded under `Assets/_Project/Scripts/`); this skill is the how-to.

Reply to the user in Vietnamese.

## When this matters

- A second "manager" for something Core already does (save, pool, events) forks the project.
- Gameplay that references UI or netcode types cannot be tested and must be rewritten when the
  netcode is chosen.
- A new asmdef with the wrong references compiles alone and breaks the next teammate's pull.

## 1. Find the slot

| Adding | Goes in | Template |
|---|---|---|
| Feature rules (Oxy, bag, station bays, trade) | `Scripts/Gameplay/<Feature>/<Feature>Model.cs` (plain C#) | references/templates.md § Feature |
| Its MonoBehaviour (input, physics, visuals) | `Scripts/Gameplay/<Feature>/<Feature>View.cs` or a clear role name (`PlayerMotor`) | § Feature |
| Tuning numbers | `Scripts/Gameplay/<Feature>/<Feature>Config.cs` (SO) + asset in `Assets/_Project/Configs/` | § Config |
| Something happened (for UI/audio/story) | `<Feature>Events.cs` beside the publisher: `readonly struct …Event` | § Event |
| Saved state | `[Serializable] <Feature>SaveData` + key const, via `SaveService` | § Save |
| Screen / popup | `Scripts/UI/<Feature>/<Name>Panel.cs : Panel` + prefab in `PanelRegistry` | § Panel |
| A service used across features | Plain class in the module that owns it, registered in `GameBootstrap` | § Service |
| Networked sync of a feature | `Scripts/Net/<Feature>/<Feature>Net.cs` (only after netcode is chosen) | § Net |
| Editor tool / Sheet importer | `Scripts/Editor/` (`Novent.Editor`, Editor-only) | § Editor asmdef |

First file in `Gameplay/`, `Net/` or `Editor/` creates that module's asmdef from the template.

## 2. Reuse check (30 seconds)

`grep -rn --include=*.cs "class .*<Noun>" Assets/_Project/Scripts` and the Core list in the rule
file. Need persistence → `SaveService`. Many short-lived objects → `PoolService`. Randomness
that decides outcomes → `Services.Get<INetSession>().WorldSeed` → `new SeededRandom(seed).Fork("<feature>")`.
Cross-feature notification → `EventBus<T>`.

## 3. Shared state goes through request → apply

```csharp
public void RequestRepair(string bayId)            // called by UI/input on any machine
{
    if (_session.IsHost) { ApplyRepair(bayId); return; }
    // Until the netcode lands every session is host; the Net adapter will forward this to the host.
}

private void ApplyRepair(string bayId) { /* validate cost, change state, publish BayRepairedEvent */ }
```

Decide shared vs per-player first (`.claude/rules/coop.md`). UI never writes state.

## 4. Wire it in Unity

Compile after each new file (the `unity-mcp` skill). A new MonoBehaviour is not done until it is
on a prefab/scene object with references assigned; a new SO type is not done until its asset
exists and is assigned; a new panel is not done until its prefab is in the `PanelRegistry`.
Use MCP tools or give the user click steps — never edit YAML.

## 5. Test the model

New Model logic gets EditMode tests (the `novent-test` skill); the test asmdef references
`Novent.Gameplay` once it exists.

## Common mistakes

| Instead of | Do this |
|---|---|
| `XxxManager : MonoBehaviour` singleton with `Instance` | Plain service in `GameBootstrap`, or a component referenced by `[SerializeField]` |
| `FindAnyObjectByType<SaveService>()` | `Services.Get<SaveService>()` cached in `Awake` |
| One `GameEvents.cs` for everyone | `<Feature>Events.cs` beside the publisher (no merge conflicts) |
| Netcode attributes in `Gameplay/` | Model stays plain; sync in `Net/<Feature>/` |
| Writing runtime values into a Config SO | Copy into the Model at start; SOs are read-only |
| `UnityEngine.Random` for loot/spawn | Forked `SeededRandom` from the world seed |
| Gameplay referencing `Novent.UI` | UI listens to events / reads the model; dependencies point down |
| Opening a panel by `SetActive(true)` | `Services.Get<PanelService>().Open<T>()` |

## References

- `references/templates.md` — read when creating a feature, config, event, save data, panel,
  service, or a new asmdef (Gameplay / Net / Editor / PlayMode tests).
