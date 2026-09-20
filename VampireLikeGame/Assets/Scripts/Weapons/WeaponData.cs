using UnityEngine;

namespace VampireLike
{
    public enum WeaponType
    {
        Projectile,
        Area
    }

    [CreateAssetMenu(menuName = "VampireLike/Weapon", fileName = "NewWeapon")]
    public class WeaponData : ScriptableObject
    {
        [Header("General")]
        public string weaponName = "Weapon";
        public Sprite icon;
        public WeaponType weaponType = WeaponType.Projectile;
        public int maxLevel = 8;

        [Header("Scaling")]
        public float baseDamage = 10f;
        public float damagePerLevel = 3f;
        public float baseCooldown = 1f;
        public float cooldownReductionPerLevel = 0.05f;

        [Header("Projectile settings")]
        public GameObject projectilePrefab;
        public float projectileSpeed = 10f;
        public int pierce = 1;
        public int projectileCount = 1;
        [Tooltip("Levels (1-indexed) at which an extra projectile is added.")]
        public int[] extraProjectileAtLevels = { 3, 6 };

        [Header("Area settings")]
        public float baseAreaRadius = 2f;
        public float areaRadiusPerLevel = 0.25f;

        public int GetProjectileCountAtLevel(int level)
        {
            int count = projectileCount;
            if (extraProjectileAtLevels == null) return count;

            foreach (int lvl in extraProjectileAtLevels)
            {
                if (level >= lvl) count++;
            }
            return count;
        }
    }
}
