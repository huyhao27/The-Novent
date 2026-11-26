using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Novent.Core.Pooling
{
    /// <summary>
    /// One ObjectPool per prefab for local-only objects (VFX, damage numbers, debris).
    /// Networked objects are spawned by the host through the netcode library, not here.
    /// </summary>
    public sealed class PoolService
    {
        private const int DefaultCapacity = 8;
        private const int MaxPoolSize = 256;

        private readonly Transform _activeRoot;
        private readonly Transform _storage;
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _poolsByPrefab = new();
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _poolsByInstance = new();
        private readonly HashSet<GameObject> _idle = new();

        public PoolService(Transform root)
        {
            _activeRoot = root;
            // Idle instances live under an inactive parent: a new instance does not run Awake/OnEnable
            // until it is first spawned, and returning one to storage disables it without SetActive churn.
            _storage = new GameObject("[PoolStorage]").transform;
            _storage.SetParent(root, false);
            _storage.gameObject.SetActive(false);
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var pool = GetPool(prefab);
            var instance = pool.Get();
            _idle.Remove(instance);
            _poolsByInstance[instance] = pool;
            instance.transform.SetParent(_activeRoot, false);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            NotifyPoolables(instance, spawned: true);
            return instance;
        }

        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return Spawn(prefab.gameObject, position, rotation).GetComponent<T>();
        }

        /// <summary>
        /// Returns an instance to its pool. Despawning twice is ignored; objects that did not come
        /// from a pool are destroyed.
        /// </summary>
        public void Despawn(GameObject instance)
        {
            if (ReferenceEquals(instance, null) || _idle.Contains(instance))
            {
                return;
            }
            var pooled = _poolsByInstance.Remove(instance, out var pool);
            if (instance == null)
            {
                // Destroyed elsewhere (scene cleanup, a script): only the bookkeeping entry is left to drop.
                return;
            }
            if (!pooled)
            {
                Object.Destroy(instance);
                return;
            }
            NotifyPoolables(instance, spawned: false);
            pool.Release(instance);
        }

        public void Prewarm(GameObject prefab, int count)
        {
            var pool = GetPool(prefab);
            var created = new List<GameObject>(count);
            for (var i = 0; i < count; i++)
            {
                created.Add(pool.Get());
            }
            foreach (var instance in created)
            {
                pool.Release(instance);
            }
        }

        private ObjectPool<GameObject> GetPool(GameObject prefab)
        {
            if (_poolsByPrefab.TryGetValue(prefab, out var pool))
            {
                return pool;
            }
            pool = new ObjectPool<GameObject>(
                createFunc: () => Object.Instantiate(prefab, _storage),
                actionOnRelease: Store,
                actionOnDestroy: Object.Destroy,
                collectionCheck: Application.isEditor,
                defaultCapacity: DefaultCapacity,
                maxSize: MaxPoolSize);
            _poolsByPrefab[prefab] = pool;
            return pool;
        }

        private void Store(GameObject instance)
        {
            instance.transform.SetParent(_storage, false);
            _idle.Add(instance);
        }

        private static void NotifyPoolables(GameObject instance, bool spawned)
        {
            // A pooled list per call: an OnSpawned that spawns another pooled object re-enters here.
            using (ListPool<IPoolable>.Get(out var poolables))
            {
                instance.GetComponentsInChildren(true, poolables);
                foreach (var poolable in poolables)
                {
                    if (spawned)
                    {
                        poolable.OnSpawned();
                    }
                    else
                    {
                        poolable.OnDespawned();
                    }
                }
            }
        }
    }
}
