using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    /// <summary>
    /// Generic component pool keyed by prefab. Enemies and projectiles are
    /// spawned through this instead of Instantiate/Destroy to avoid GC spikes
    /// once dozens of enemies are alive at once.
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
        private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null) return;
            var queue = GetOrCreateQueue(prefab);
            for (int i = 0; i < count; i++)
            {
                var instance = CreateInstance(prefab);
                instance.SetActive(false);
                queue.Enqueue(instance);
            }
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var queue = GetOrCreateQueue(prefab);
            GameObject instance = queue.Count > 0 ? queue.Dequeue() : CreateInstance(prefab);

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);

            if (instance.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnSpawned();
            }

            return instance;
        }

        public void Release(GameObject instance)
        {
            if (instance == null) return;

            if (instance.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnDespawned();
            }

            instance.SetActive(false);

            if (_instanceToPrefab.TryGetValue(instance, out var prefab))
            {
                GetOrCreateQueue(prefab).Enqueue(instance);
            }
            else
            {
                // Instance was not created through the pool; just destroy it.
                Destroy(instance);
            }
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            var instance = Instantiate(prefab, transform);
            _instanceToPrefab[instance] = prefab;
            return instance;
        }

        private Queue<GameObject> GetOrCreateQueue(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out var queue))
            {
                queue = new Queue<GameObject>();
                _pools[prefab] = queue;
            }
            return queue;
        }
    }

    /// <summary>Optional hook for pooled objects that need to reset state on spawn/despawn.</summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
