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

        private readonly Transform _root;
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _poolsByPrefab = new();
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _poolsByInstance = new();
        private readonly List<IPoolable> _poolableBuffer = new();

        public PoolService(Transform root)
        {
            _root = root;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var pool = GetPool(prefab);
            var instance = pool.Get();
            _poolsByInstance[instance] = pool;
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            NotifyPoolables(instance, spawned: true);
            return instance;
        }

        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return Spawn(prefab.gameObject, position, rotation).GetComponent<T>();
        }

        /// <summary>Returns an instance to its pool; objects that did not come from a pool are destroyed.</summary>
        public void Despawn(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }
            if (!_poolsByInstance.Remove(instance, out var pool))
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
                createFunc: () => CreateInstance(prefab),
                actionOnRelease: Deactivate,
                actionOnDestroy: Object.Destroy,
                collectionCheck: Application.isEditor,
                defaultCapacity: DefaultCapacity,
                maxSize: MaxPoolSize);
            _poolsByPrefab[prefab] = pool;
            return pool;
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            var instance = Object.Instantiate(prefab, _root);
            instance.SetActive(false);
            return instance;
        }

        private void Deactivate(GameObject instance)
        {
            instance.SetActive(false);
            instance.transform.SetParent(_root, false);
        }

        private void NotifyPoolables(GameObject instance, bool spawned)
        {
            instance.GetComponentsInChildren(true, _poolableBuffer);
            foreach (var poolable in _poolableBuffer)
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
            _poolableBuffer.Clear();
        }
    }
}
