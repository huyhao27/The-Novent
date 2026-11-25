using System;
using System.Collections.Generic;

namespace Novent.Core.Session
{
    /// <summary>Solo session: this machine is the host and the only player.</summary>
    public sealed class OfflineSession : INetSession
    {
        private readonly PlayerId[] _players = { PlayerId.Local };

        public OfflineSession(int worldSeed)
        {
            WorldSeed = worldSeed;
        }

        public bool IsHost => true;

        public PlayerId LocalPlayer => PlayerId.Local;

        public IReadOnlyList<PlayerId> Players => _players;

        public int WorldSeed { get; }

        // Nobody joins or leaves a solo session; empty accessors avoid keeping unused delegates.
        public event Action<PlayerId> PlayerJoined
        {
            add { }
            remove { }
        }

        public event Action<PlayerId> PlayerLeft
        {
            add { }
            remove { }
        }
    }
}
