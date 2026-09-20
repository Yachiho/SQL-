using System;
using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    [Serializable]
    public class ActiveWeapon
    {
        public WeaponData Data;
        public WeaponBase Behaviour;
        public int Level => Behaviour.Level;
    }

    /// <summary>Attach to the player. Adds new weapons and levels up existing ones on request from the upgrade system.</summary>
    public class WeaponManager : MonoBehaviour
    {
        [SerializeField] private int maxWeaponSlots = 6;

        private readonly List<ActiveWeapon> _activeWeapons = new();

        public IReadOnlyList<ActiveWeapon> ActiveWeapons => _activeWeapons;
        public bool HasFreeSlot => _activeWeapons.Count < maxWeaponSlots;

        public bool HasWeapon(WeaponData data) => TryGetActive(data, out _);

        public int GetLevel(WeaponData data) => TryGetActive(data, out var active) ? active.Level : 0;

        public bool IsMaxLevel(WeaponData data) => TryGetActive(data, out var active) && active.Level >= data.maxLevel;

        public void AddWeapon(WeaponData data)
        {
            if (HasWeapon(data) || !HasFreeSlot) return;

            var weaponObject = new GameObject($"Weapon_{data.weaponName}");
            weaponObject.transform.SetParent(transform, false);

            WeaponBase behaviour = data.weaponType switch
            {
                WeaponType.Projectile => weaponObject.AddComponent<ProjectileWeapon>(),
                WeaponType.Area => weaponObject.AddComponent<AreaWeapon>(),
                _ => throw new ArgumentOutOfRangeException()
            };

            behaviour.Init(data, 1);
            _activeWeapons.Add(new ActiveWeapon { Data = data, Behaviour = behaviour });
        }

        public void LevelUpWeapon(WeaponData data)
        {
            if (!TryGetActive(data, out var active)) return;
            active.Behaviour.SetLevel(active.Behaviour.Level + 1);
        }

        private bool TryGetActive(WeaponData data, out ActiveWeapon active)
        {
            foreach (var weapon in _activeWeapons)
            {
                if (weapon.Data == data)
                {
                    active = weapon;
                    return true;
                }
            }
            active = null;
            return false;
        }
    }
}
