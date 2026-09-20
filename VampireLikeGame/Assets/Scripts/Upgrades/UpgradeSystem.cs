using System;
using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    /// <summary>
    /// Listens for player level-ups, builds a shortlist of valid upgrade
    /// cards (filtering out maxed weapons and new weapons when slots are
    /// full), and applies whichever card the UI reports back as chosen.
    /// </summary>
    public class UpgradeSystem : MonoBehaviour
    {
        [SerializeField] private List<UpgradeData> allUpgrades = new();
        [SerializeField] private int optionsPerLevelUp = 3;
        [SerializeField] private WeaponManager weaponManager;
        [SerializeField] private PlayerStats playerStats;

        public static event Action<List<UpgradeData>> OnUpgradeOptionsReady;

        private void OnEnable() => PlayerLeveling.OnLevelUp += HandleLevelUp;
        private void OnDisable() => PlayerLeveling.OnLevelUp -= HandleLevelUp;

        private void HandleLevelUp(int newLevel)
        {
            var options = GetRandomOptions(optionsPerLevelUp);
            if (options.Count == 0) return;

            GameManager.Instance?.PauseForLevelUp();
            OnUpgradeOptionsReady?.Invoke(options);
        }

        public List<UpgradeData> GetRandomOptions(int count)
        {
            var valid = new List<UpgradeData>();
            foreach (var upgrade in allUpgrades)
            {
                if (IsValid(upgrade)) valid.Add(upgrade);
            }

            Shuffle(valid);
            if (valid.Count > count) valid.RemoveRange(count, valid.Count - count);
            return valid;
        }

        private bool IsValid(UpgradeData upgrade)
        {
            if (upgrade.category != UpgradeCategory.Weapon) return true;
            if (upgrade.weaponData == null) return false;

            bool owned = weaponManager.HasWeapon(upgrade.weaponData);
            if (owned) return !weaponManager.IsMaxLevel(upgrade.weaponData);
            return weaponManager.HasFreeSlot;
        }

        public void ApplyUpgrade(UpgradeData upgrade)
        {
            if (upgrade.category == UpgradeCategory.Weapon)
            {
                if (weaponManager.HasWeapon(upgrade.weaponData))
                {
                    weaponManager.LevelUpWeapon(upgrade.weaponData);
                }
                else
                {
                    weaponManager.AddWeapon(upgrade.weaponData);
                }
            }
            else
            {
                if (upgrade.isMultiplier)
                {
                    playerStats.ApplyMultiplierBonus(upgrade.statType, upgrade.value);
                }
                else
                {
                    playerStats.ApplyFlatBonus(upgrade.statType, upgrade.value);
                }
            }

            GameManager.Instance?.ResumeAfterLevelUp();
        }

        private static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
