using System;
using UnityEngine;

namespace VampireLike
{
    /// <summary>
    /// Translates an IdolData's Vocal/Dance/Visual leaning into the existing
    /// PlayerStats bonuses at run start: Vocal -> damage, Dance -> speed and
    /// attack cooldown, Visual -> fan pickup radius and XP gain. Everything
    /// downstream (weapons, upgrades) is untouched; this is just a flavor
    /// translation layer on top of it.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class IdolProfile : MonoBehaviour
    {
        [SerializeField] private IdolData idolData;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public static event Action<IdolData> OnIdolAssigned;

        public string IdolName => idolData != null ? idolData.idolName : "Producer";

        private void Start()
        {
            if (idolData == null) return;

            var stats = GetComponent<PlayerStats>();

            stats.ApplyMultiplierBonus(StatType.Damage, idolData.vocal * 0.004f);
            stats.ApplyFlatBonus(StatType.MoveSpeed, idolData.dance * 0.02f);
            stats.ApplyMultiplierBonus(StatType.CooldownReduction, idolData.dance * 0.002f);
            stats.ApplyFlatBonus(StatType.PickupRadius, idolData.visual * 0.015f);
            stats.ApplyMultiplierBonus(StatType.XpGain, idolData.visual * 0.002f);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = idolData.themeColor;
            }

            OnIdolAssigned?.Invoke(idolData);
        }
    }
}
