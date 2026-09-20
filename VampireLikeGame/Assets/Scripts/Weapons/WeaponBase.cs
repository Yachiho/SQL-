using UnityEngine;

namespace VampireLike
{
    /// <summary>
    /// Shared cooldown/leveling logic for every weapon. Concrete weapons only
    /// implement Fire() — how the attack actually happens.
    /// </summary>
    public abstract class WeaponBase : MonoBehaviour
    {
        protected WeaponData Data { get; private set; }
        public int Level { get; private set; } = 1;

        private float _cooldownTimer;

        public virtual void Init(WeaponData data, int level)
        {
            Data = data;
            Level = level;
            _cooldownTimer = 0f;
        }

        public virtual void SetLevel(int level)
        {
            Level = Mathf.Clamp(level, 1, Data.maxLevel);
        }

        protected virtual void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;
            if (Player.Instance == null) return;

            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer <= 0f)
            {
                Fire();
                _cooldownTimer = GetCooldown();
            }
        }

        protected float GetDamage()
        {
            float raw = Data.baseDamage + Data.damagePerLevel * (Level - 1);
            return raw * Player.Instance.Stats.DamageMultiplier;
        }

        protected float GetCooldown()
        {
            float raw = Mathf.Max(0.1f, Data.baseCooldown - Data.cooldownReductionPerLevel * (Level - 1));
            return raw * Player.Instance.Stats.CooldownMultiplier;
        }

        protected float GetAreaRadius()
        {
            float raw = Data.baseAreaRadius + Data.areaRadiusPerLevel * (Level - 1);
            return raw * Player.Instance.Stats.AreaMultiplier;
        }

        protected abstract void Fire();
    }
}
