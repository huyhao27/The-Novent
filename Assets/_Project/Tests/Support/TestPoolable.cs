using System;
using Novent.Core.Pooling;
using UnityEngine;

namespace Novent.Tests.Support
{
    /// <summary>Counts pool callbacks; OnSpawnedAction lets a test re-enter the pool from inside a callback.</summary>
    public sealed class TestPoolable : MonoBehaviour, IPoolable
    {
        public int SpawnedCount { get; private set; }

        public int DespawnedCount { get; private set; }

        public Action OnSpawnedAction { get; set; }

        public void OnSpawned()
        {
            SpawnedCount++;
            OnSpawnedAction?.Invoke();
        }

        public void OnDespawned()
        {
            DespawnedCount++;
        }
    }
}
