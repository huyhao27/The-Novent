using System;
using Novent.Core;
using Novent.Core.Pooling;
using Novent.Core.Save;
using Novent.Core.Session;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Novent.Boot
{
    /// <summary>
    /// Composition root: the only place shared services are created. Runs before the first scene
    /// of every Play, so any scene can be played directly. Add a new service here, in dependency order.
    /// </summary>
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var root = new GameObject("[Services]");
            Object.DontDestroyOnLoad(root);

            Services.Register(new SaveService(new FileSaveStore()));
            Services.Register(new PoolService(root.transform));
            // Replaced by the netcode session when the player hosts or joins; solo play stays offline.
            Services.Register<INetSession>(new OfflineSession(Environment.TickCount));

            Services.MarkReady();
        }
    }
}
