using System;
using UnityEngine;

namespace VampireLike
{
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private float invincibilityDuration = 0.5f;

        public static event Action<float, float> OnHealthChanged; // current, max

        private PlayerStats _stats;
        private float _currentHealth;
        private float _invincibleUntil;

        public bool IsDead { get; private set; }

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
        }

        private void Start()
        {
            _currentHealth = _stats.MaxHealth;
            OnHealthChanged?.Invoke(_currentHealth, _stats.MaxHealth);
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            _currentHealth = Mathf.Min(_stats.MaxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth, _stats.MaxHealth);
        }

        public void TakeDamage(float amount, Vector2 sourcePosition)
        {
            if (IsDead || Time.time < _invincibleUntil) return;

            float mitigated = Mathf.Max(1f, amount - _stats.Armor);
            _currentHealth -= mitigated;
            _invincibleUntil = Time.time + invincibilityDuration;

            OnHealthChanged?.Invoke(Mathf.Max(0f, _currentHealth), _stats.MaxHealth);

            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;
            GameManager.Instance?.GameOver();
        }
    }
}
