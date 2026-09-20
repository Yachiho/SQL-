using System;
using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    [Serializable]
    public class SpawnWave
    {
        public string waveName = "Wave";
        [Tooltip("Elapsed run time (seconds) at which this wave becomes active.")]
        public float startTime;
        public EnemyData enemyData;
        [Tooltip("Seconds between spawns of this enemy type once the wave is active.")]
        public float spawnInterval = 1f;
        [Tooltip("How many of this enemy to spawn each interval tick.")]
        public int spawnCountPerTick = 1;
    }

    /// <summary>
    /// Spawns enemies in a ring just outside the camera around the player.
    /// Which enemy types are active, and how fast, is entirely data-driven
    /// through the SpawnWave list so difficulty tuning doesn't touch code.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private List<SpawnWave> waves = new();
        [SerializeField] private float spawnRadius = 12f;
        [SerializeField] private int maxAliveEnemies = 200;

        private readonly Dictionary<SpawnWave, float> _nextSpawnTime = new();

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
            if (Player.Instance == null || ObjectPool.Instance == null) return;

            float elapsed = GameManager.Instance.ElapsedTime;

            foreach (var wave in waves)
            {
                if (elapsed < wave.startTime || wave.enemyData == null || wave.enemyData.prefab == null) continue;

                if (!_nextSpawnTime.TryGetValue(wave, out float nextTime))
                {
                    nextTime = wave.startTime;
                }

                if (elapsed < nextTime) continue;

                for (int i = 0; i < wave.spawnCountPerTick; i++)
                {
                    SpawnEnemy(wave.enemyData);
                }

                _nextSpawnTime[wave] = elapsed + wave.spawnInterval;
            }
        }

        private void SpawnEnemy(EnemyData data)
        {
            if (EnemyController.ActiveEnemies.Count >= maxAliveEnemies) return;

            Vector2 spawnPosition = (Vector2)Player.Instance.transform.position + RandomPointOnRing(spawnRadius);
            var instance = ObjectPool.Instance.Get(data.prefab, spawnPosition, Quaternion.identity);

            if (instance.TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.Init(data);
            }
        }

        private static Vector2 RandomPointOnRing(float radius)
        {
            float angle = UnityEngine.Random.value * Mathf.PI * 2f;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }
    }
}
