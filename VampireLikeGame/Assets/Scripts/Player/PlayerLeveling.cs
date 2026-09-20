using System;
using UnityEngine;

namespace VampireLike
{
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerLeveling : MonoBehaviour
    {
        [SerializeField] private int baseXpToLevel = 5;
        [SerializeField] private float xpCurveGrowth = 1.18f;

        public static event Action<int, float, float> OnXpChanged; // level, currentXp, xpToNextLevel
        public static event Action<int> OnLevelUp;

        private PlayerStats _stats;
        private int _level = 1;
        private float _currentXp;
        private float _xpToNextLevel;

        public int Level => _level;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            _xpToNextLevel = baseXpToLevel;
        }

        private void Start()
        {
            OnXpChanged?.Invoke(_level, _currentXp, _xpToNextLevel);
        }

        public void AddExperience(float amount)
        {
            _currentXp += amount * _stats.XpGainMultiplier;

            while (_currentXp >= _xpToNextLevel)
            {
                _currentXp -= _xpToNextLevel;
                _level++;
                _xpToNextLevel = Mathf.Ceil(_xpToNextLevel * xpCurveGrowth);
                OnLevelUp?.Invoke(_level);
            }

            OnXpChanged?.Invoke(_level, _currentXp, _xpToNextLevel);
        }
    }
}
