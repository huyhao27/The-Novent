using System;
using System.Collections.Generic;

namespace Novent.Core.Session
{
    /// <summary>
    /// What gameplay needs to know about the multiplayer session. Netcode-specific code implements
    /// it in Novent.Net; OfflineSession covers solo play and tests. Solo play is a host with no clients.
    /// </summary>
    public interface INetSession
    {
        /// <summary>True on the machine that owns the world and applies shared-state changes.</summary>
        bool IsHost { get; }

        PlayerId LocalPlayer { get; }

        IReadOnlyList<PlayerId> Players { get; }

        /// <summary>Chosen by the host and shared with every client; feeds SeededRandom.</summary>
        int WorldSeed { get; }

        event Action<PlayerId> PlayerJoined;

        event Action<PlayerId> PlayerLeft;
    }
}
