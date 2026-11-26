# Templates — The Novent modules

## Contents
- Feature (model + view)
- Config
- Event
- Save
- Panel
- Service
- Net (after netcode is chosen)
- asmdef: Gameplay, Net, Editor, PlayMode tests

Replace `Oxygen` with the feature name. Numbers come from the GDD; cite the section once.

## Feature

`Assets/_Project/Scripts/Gameplay/Oxygen/OxygenModel.cs` — plain C#, testable:

```csharp
using System;

namespace Novent.Gameplay.Oxygen
{
    /// <summary>Oxy is both time and health (GDD CORE LOOP). Owned by its player.</summary>
    public sealed class OxygenModel
    {
        private readonly OxygenConfig _config;

        public OxygenModel(OxygenConfig config)
        {
            _config = config;
            Current = config.Capacity;
        }

        public float Current { get; private set; }

        public bool IsEmpty => Current <= 0f;

        public event Action<float> Changed;

        public void Tick(float deltaTime)
        {
            if (IsEmpty)
            {
                return;
            }
            Current = Math.Max(0f, Current - _config.DrainPerSecond * deltaTime);
            Changed?.Invoke(Current);
        }
    }
}
```

`OxygenView.cs` — the MonoBehaviour that owns the model and feeds it time:

```csharp
using UnityEngine;

namespace Novent.Gameplay.Oxygen
{
    public sealed class OxygenView : MonoBehaviour
    {
        [SerializeField] private OxygenConfig _config;

        public OxygenModel Model { get; private set; }

        private void Awake()
        {
            Model = new OxygenModel(_config);
        }

        private void Update()
        {
            Model.Tick(Time.deltaTime);
        }
    }
}
```

## Config

```csharp
using UnityEngine;

namespace Novent.Gameplay.Oxygen
{
    [CreateAssetMenu(menuName = "Novent/Config/Oxygen", fileName = "OxygenConfig")]
    public sealed class OxygenConfig : ScriptableObject
    {
        [Tooltip("Seconds of Oxy in a full tank. Placeholder until the GDD Sheet value is imported.")]
        [SerializeField, Min(1f)] private float _capacity = 120f;

        [Tooltip("Oxy lost per second while outside the station.")]
        [SerializeField, Min(0f)] private float _drainPerSecond = 1f;

        public float Capacity => _capacity;
        public float DrainPerSecond => _drainPerSecond;
    }
}
```

Create the asset with `mcp__UnityMCP__manage_asset` (or `Create > Novent > Config > Oxygen`) in
`Assets/_Project/Configs/`, then assign it.

## Event

`Assets/_Project/Scripts/Gameplay/Oxygen/OxygenEvents.cs`:

```csharp
using Novent.Core.Session;

namespace Novent.Gameplay.Oxygen
{
    public readonly struct SuffocationStartedEvent
    {
        public SuffocationStartedEvent(PlayerId player)
        {
            Player = player;
        }

        public PlayerId Player { get; }
    }
}
```

Publish: `EventBus<SuffocationStartedEvent>.Publish(new SuffocationStartedEvent(player));`
Listen (UI, audio):

```csharp
private void OnEnable() => EventBus<SuffocationStartedEvent>.Subscribe(HandleSuffocation);
private void OnDisable() => EventBus<SuffocationStartedEvent>.Unsubscribe(HandleSuffocation);
```

## Save

```csharp
[Serializable]
public sealed class StationSaveData
{
    public const string Key = "station";

    public int tally;
    public List<string> repairedBays = new();
}
```

Write: `save.Set(StationSaveData.Key, data); save.Save();` at checkpoints (after a trip, after
trade hour, on sleep) — never per frame. Read: `save.TryGet(StationSaveData.Key, out StationSaveData data)`.
Field names are the save format: renaming one loses old values → bump
`SaveService.CurrentVersion` and migrate. World slot is written by the host only.

## Panel

```csharp
using UnityEngine;
using UnityEngine.UI;

namespace Novent.UI.Pause
{
    public sealed class PausePanel : Panel
    {
        [SerializeField] private Button _resumeButton;

        private void OnEnable() => _resumeButton.onClick.AddListener(HandleResume);
        private void OnDisable() => _resumeButton.onClick.RemoveListener(HandleResume);

        private void HandleResume() => Novent.Core.Services.Get<PanelService>().Close(this);
    }
}
```

Prefab: root has `RectTransform` + `CanvasGroup` + the panel script, saved under
`Assets/_Project/Prefabs/UI/`, added to the `PanelRegistry` asset. Open with
`Services.Get<PanelService>().Open<PausePanel>()`. The scene's Canvas has `UIRoot` with the registry assigned.

## Service

1. Plain class in the module that owns it (`Novent.Gameplay.Trade.TradeSchedule`).
2. Register in `Assets/_Project/Scripts/Boot/GameBootstrap.cs` after the services it needs:
   `Services.Register(new TradeSchedule(Services.Get<SaveService>()));`
3. If `Novent.Boot` does not yet reference that module's asmdef, add it to `references`.
4. Consumers cache it: `_trade = Services.Get<TradeSchedule>();` in `Awake`.

## Net (after netcode is chosen)

The netcode session implements `INetSession` and replaces the offline one:
`Services.Unregister<INetSession>(); Services.Register<INetSession>(session);`.
`Scripts/Net/<Feature>/<Feature>Net.cs` is a `NetworkBehaviour` that holds the synced values,
forwards `Request…` from clients to the host, calls the model's `Apply…` on the host, and pushes
the result to the model on clients. Netcode types never appear outside `Scripts/Net/`.

## asmdef: Gameplay

`Assets/_Project/Scripts/Gameplay/Novent.Gameplay.asmdef`:

```json
{
    "name": "Novent.Gameplay",
    "rootNamespace": "Novent.Gameplay",
    "references": ["Novent.Core", "Unity.InputSystem"],
    "autoReferenced": true
}
```

Then add `"Novent.Gameplay"` to `Novent.UI`, `Novent.Boot` and `Novent.Tests.EditMode` references.

## asmdef: Net

```json
{
    "name": "Novent.Net",
    "rootNamespace": "Novent.Net",
    "references": ["Novent.Core", "Novent.Gameplay", "<netcode assembly, e.g. Unity.Netcode.Runtime>"],
    "autoReferenced": true
}
```

Add `"Novent.Net"` to `Novent.Boot`.

## asmdef: Editor

```json
{
    "name": "Novent.Editor",
    "rootNamespace": "Novent.Editor",
    "references": ["Novent.Core", "Novent.Gameplay", "Novent.UI"],
    "includePlatforms": ["Editor"],
    "autoReferenced": false
}
```

## asmdef: PlayMode tests

`Assets/_Project/Tests/PlayMode/Novent.Tests.PlayMode.asmdef`: same as the EditMode one but
`"name": "Novent.Tests.PlayMode"` and `"includePlatforms": []`.
