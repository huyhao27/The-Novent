---
paths:
  - "Assets/_Project/Scripts/Gameplay/**/*.cs"
  - "Assets/_Project/Scripts/Net/**/*.cs"
---

# Co-op authority — every piece of state has one owner

Co-op 1–4 online is in the MVP. Code that changes shared state on whichever machine runs it works
in the Editor and desyncs the moment a second player joins. Single-player is host + 0 clients —
there is no "offline version first".

## Decide before coding any new state

| Question | Default (GDD CORE LOOP § Luật co-op) |
|---|---|
| Shared or per-player? | Shared: station, storage, Tally, upgrades, gear level, blueprints, map, story progress, port rank, crates, Driftlife in the world. Per player: Oxy, in-trip bag, carried tools |
| Who may change it? | Shared → host only. Per-player → its owner; anything affecting others (share Oxy, hand an item) is validated by the host |
| How does a late joiner get it? | Persistent synced state, never a one-shot message |
| Saved? | Shared → host's world save. Per-player trip state follows the pull-back rules |
| Random? | Seeded by the host (map chunks, spawns, loot) so every client sees the same world |

## Code shape

- Gameplay systems expose **requests** (`RequestRepair(stationId)`) and **read-only state +
  change events**. UI and input call requests; they never write state.
- A request runs `Apply…` on the host after validating it (range, ownership, cost); clients see
  the result through synced state.
- Netcode types (`NetworkBehaviour`, RPC attributes, `NetworkVariable`) live only in
  `Novent.Net` and in thin `…Net` components beside a system. Pure logic classes stay
  netcode-free so they can be unit-tested and the netcode can still change.
- One-shot RPC/messages only for effects (VFX, SFX, hit flash). Values go through synced state.
- Networked objects are spawned by the host from registered prefabs; `Instantiate` on a client
  creates an object only that client sees.
- Singletons exist once per machine: say whether a manager is host-only logic or a local view.
- `UnityEngine.Random` for gameplay outcomes is forbidden; use the seeded random service.

## Verify

A co-op change is done when host + 1 client (Multiplayer Play Mode or a second build) both see
the right value, and a client that joins late sees it too. Claude usually cannot run two
instances — list the exact host/client steps under "Chưa verify".
