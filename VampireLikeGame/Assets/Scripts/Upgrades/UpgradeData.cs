using UnityEngine;

namespace VampireLike
{
    public enum UpgradeCategory
    {
        Weapon,
        Stat
    }

    [CreateAssetMenu(menuName = "VampireLike/Upgrade", fileName = "NewUpgrade")]
    public class UpgradeData : ScriptableObject
    {
        public string displayName = "Upgrade";
        [TextArea] public string description = "";
        public Sprite icon;
        public UpgradeCategory category = UpgradeCategory.Stat;

        [Header("Weapon upgrade (category = Weapon)")]
        public WeaponData weaponData;

        [Header("Stat upgrade (category = Stat)")]
        public StatType statType;
        public bool isMultiplier;
        public float value = 1f;
    }
}
