using UnityEngine;

namespace VampireLike
{
    /// <summary>
    /// Single access point for "the player" so enemies, gems and weapons
    /// don't each need their own FindObjectOfType/tag lookups.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerHealth))]
    [RequireComponent(typeof(PlayerLeveling))]
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }

        public PlayerStats Stats { get; private set; }
        public PlayerHealth Health { get; private set; }
        public PlayerLeveling Leveling { get; private set; }

        private void Awake()
        {
            Instance = this;
            Stats = GetComponent<PlayerStats>();
            Health = GetComponent<PlayerHealth>();
            Leveling = GetComponent<PlayerLeveling>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
