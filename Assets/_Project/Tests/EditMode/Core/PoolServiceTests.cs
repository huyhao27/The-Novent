using Novent.Core.Pooling;
using Novent.Tests.Support;
using NUnit.Framework;
using UnityEngine;

namespace Novent.Tests.Core
{
    public class PoolServiceTests
    {
        private GameObject _root;
        private GameObject _prefab;
        private PoolService _pools;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("PoolRoot");
            _prefab = new GameObject("PooledPrefab");
            _prefab.AddComponent<TestPoolable>();
            _pools = new PoolService(_root.transform);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_prefab);
        }

        [Test]
        public void Spawn_PlacesActiveInstanceAndCallsOnSpawned()
        {
            var instance = _pools.Spawn(_prefab, new Vector3(1f, 2f, 3f), Quaternion.identity);

            Assert.That(instance.activeInHierarchy, Is.True);
            Assert.That(instance.transform.position, Is.EqualTo(new Vector3(1f, 2f, 3f)));
            Assert.That(instance.GetComponent<TestPoolable>().SpawnedCount, Is.EqualTo(1));
        }

        [Test]
        public void Despawn_ThenSpawn_ReusesSameInstance()
        {
            var first = _pools.Spawn(_prefab, Vector3.zero, Quaternion.identity);
            _pools.Despawn(first);

            var second = _pools.Spawn(_prefab, Vector3.one, Quaternion.identity);

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.GetComponent<TestPoolable>().DespawnedCount, Is.EqualTo(1));
        }

        [Test]
        public void Despawn_HidesInstanceWithoutDestroyingIt()
        {
            var instance = _pools.Spawn(_prefab, Vector3.zero, Quaternion.identity);

            _pools.Despawn(instance);

            Assert.That(instance != null, Is.True);
            Assert.That(instance.activeInHierarchy, Is.False);
        }

        [Test]
        public void Despawn_Twice_SecondCallIgnored()
        {
            var instance = _pools.Spawn(_prefab, Vector3.zero, Quaternion.identity);
            _pools.Despawn(instance);

            _pools.Despawn(instance);

            Assert.That(instance != null, Is.True);
            Assert.That(instance.GetComponent<TestPoolable>().DespawnedCount, Is.EqualTo(1));
            Assert.That(_pools.Spawn(_prefab, Vector3.zero, Quaternion.identity), Is.SameAs(instance));
        }

        [Test]
        public void Spawn_FromInsideOnSpawned_BothInstancesNotified()
        {
            var child = new GameObject("ChildPrefab");
            child.AddComponent<TestPoolable>();
            TestPoolable spawnedChild = null;
            // Instantiate does not copy delegates, so the callback is set on the pooled instance itself.
            var parent = _pools.Spawn(_prefab, Vector3.zero, Quaternion.identity);
            var parentPoolable = parent.GetComponent<TestPoolable>();
            parentPoolable.OnSpawnedAction =
                () => spawnedChild = _pools.Spawn(child, Vector3.zero, Quaternion.identity).GetComponent<TestPoolable>();
            _pools.Despawn(parent);

            _pools.Spawn(_prefab, Vector3.zero, Quaternion.identity);

            Assert.That(parentPoolable.SpawnedCount, Is.EqualTo(2));
            Assert.That(spawnedChild.SpawnedCount, Is.EqualTo(1));
            Object.DestroyImmediate(child);
        }
    }
}
