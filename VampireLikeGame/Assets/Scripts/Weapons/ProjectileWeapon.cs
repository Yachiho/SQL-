using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    /// <summary>Auto-targets the nearest enemies and fires one projectile at each.</summary>
    public class ProjectileWeapon : WeaponBase
    {
        private static readonly List<EnemyController> ScratchTargets = new();

        protected override void Fire()
        {
            int count = Data.GetProjectileCountAtLevel(Level);
            Vector2 origin = Player.Instance.transform.position;

            FindNearestEnemies(origin, count, ScratchTargets);
            if (ScratchTargets.Count == 0) return;

            float damage = GetDamage();

            for (int i = 0; i < count; i++)
            {
                // Cycle through however many distinct targets we found; if there
                // are fewer enemies than requested shots, re-target the closest one.
                var target = ScratchTargets[i % ScratchTargets.Count];
                Vector2 direction = ((Vector2)target.transform.position - origin).normalized;

                var instance = ObjectPool.Instance != null
                    ? ObjectPool.Instance.Get(Data.projectilePrefab, origin, Quaternion.identity)
                    : Instantiate(Data.projectilePrefab, origin, Quaternion.identity);

                if (instance.TryGetComponent<Projectile>(out var projectile))
                {
                    projectile.Launch(origin, direction, Data.projectileSpeed, damage, Data.pierce);
                }
            }
        }

        private static void FindNearestEnemies(Vector2 origin, int maxCount, List<EnemyController> results)
        {
            results.Clear();
            var enemies = EnemyController.ActiveEnemies;
            if (enemies.Count == 0) return;

            // Small N per shot (usually <= 6) so a simple partial selection sort
            // beats allocating and sorting the whole enemy list every fire.
            int take = Mathf.Min(maxCount, enemies.Count);
            var used = new bool[enemies.Count];

            for (int pick = 0; pick < take; pick++)
            {
                int bestIndex = -1;
                float bestDistSqr = float.MaxValue;

                for (int i = 0; i < enemies.Count; i++)
                {
                    if (used[i] || enemies[i] == null || enemies[i].IsDead) continue;

                    float distSqr = ((Vector2)enemies[i].transform.position - origin).sqrMagnitude;
                    if (distSqr < bestDistSqr)
                    {
                        bestDistSqr = distSqr;
                        bestIndex = i;
                    }
                }

                if (bestIndex == -1) break;
                used[bestIndex] = true;
                results.Add(enemies[bestIndex]);
            }
        }
    }
}
