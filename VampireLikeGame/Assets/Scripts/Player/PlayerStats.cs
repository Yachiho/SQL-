using System;
using UnityEngine;

namespace VampireLike
{
    public enum StatType
    {
        MaxHealth,
        MoveSpeed,
        Damage,
        CooldownReduction,
        AreaSize,
        PickupRadius,
        Armor,
        XpGain
    }

    /// <summary>
    /// Central, upgradeable stat block. Weapons and health read from here
    /// instead of hard-coding numbers, so every upgrade card just calls
    /// ApplyFlatBonus/ApplyMultiplier on the relevant stat.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Base values")]
        [SerializeField] private float baseMaxHealth = 100f;
        [SerializeField] private float baseMoveSpeed = 4.5f;
        [SerializeField] private float basePickupRadius = 1.5f;

        public float MaxHealth { get; private set; }
        public float MoveSpeed { get; private set; }
        public float DamageMultiplier { get; private set; } = 1f;
        public float CooldownMultiplier { get; private set; } = 1f; // lower = faster attacks
        public float AreaMultiplier { get; private set; } = 1f;
        public float PickupRadius { get; private set; }
        public float Armor { get; private set; } = 0f; // flat damage reduction per hit
        public float XpGainMultiplier { get; private set; } = 1f;

        public event Action OnStatsChanged;

        private void Awake()
        {
            MaxHealth = baseMaxHealth;
            MoveSpeed = baseMoveSpeed;
            PickupRadius = basePickupRadius;
        }

        public void ApplyFlatBonus(StatType stat, float amount)
        {
            switch (stat)
            {
                case StatType.MaxHealth: MaxHealth += amount; break;
                case StatType.MoveSpeed: MoveSpeed += amount; break;
                case StatType.PickupRadius: PickupRadius += amount; break;
                case StatType.Armor: Armor += amount; break;
                default: throw new ArgumentOutOfRangeException(nameof(stat), stat, "Not a flat stat");
            }
            OnStatsChanged?.Invoke();
        }

        public void ApplyMultiplierBonus(StatType stat, float additivePercent)
        {
            switch (stat)
            {
                case StatType.Damage: DamageMultiplier += additivePercent; break;
                case StatType.CooldownReduction: CooldownMultiplier = Mathf.Max(0.2f, CooldownMultiplier - additivePercent); break;
                case StatType.AreaSize: AreaMultiplier += additivePercent; break;
                case StatType.XpGain: XpGainMultiplier += additivePercent; break;
                default: throw new ArgumentOutOfRangeException(nameof(stat), stat, "Not a multiplier stat");
            }
            OnStatsChanged?.Invoke();
        }
    }
}
