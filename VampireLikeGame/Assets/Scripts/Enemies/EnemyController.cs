using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour, IDamageable, IPoolable
    {
        [SerializeField] private GameObject experienceGemPrefab;

        /// <summary>All currently alive enemies, kept up to date via OnSpawned/OnDespawned so
        /// weapons can find the nearest target without an OverlapCircle allocation every shot.</summary>
        public static readonly List<EnemyController> ActiveEnemies = new();

        private EnemyData _data;
        private Rigidbody2D _rigidbody;
        private float _currentHealth;
        private float _nextContactDamageTime;

        public bool IsDead { get; private set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
        }

        public void Init(EnemyData data)
        {
            _data = data;
            _currentHealth = data.maxHealth;
            IsDead = false;
        }

        private void FixedUpdate()
        {
            if (IsDead || Player.Instance == null) return;
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            Vector2 toPlayer = (Vector2)Player.Instance.transform.position - _rigidbody.position;
            Vector2 direction = toPlayer.sqrMagnitude > 0.0001f ? toPlayer.normalized : Vector2.zero;
            _rigidbody.MovePosition(_rigidbody.position + direction * (_data.moveSpeed * Time.fixedDeltaTime));
        }

        private void OnCollisionStay2D(Collision2D collision) => TryDamagePlayer(collision.collider);
        private void OnTriggerStay2D(Collider2D other) => TryDamagePlayer(other);

        private void TryDamagePlayer(Component other)
        {
            if (IsDead || Time.time < _nextContactDamageTime) return;
            if (Player.Instance == null) return;
            if (!other.TryGetComponent<PlayerHealth>(out var playerHealth)) return;

            playerHealth.TakeDamage(_data.contactDamage, _rigidbody.position);
            _nextContactDamageTime = Time.time + _data.contactDamageInterval;
        }

        public void TakeDamage(float amount, Vector2 sourcePosition)
        {
            if (IsDead) return;

            _currentHealth -= amount;
            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            IsDead = true;

            if (experienceGemPrefab != null)
            {
                var gem = ObjectPool.Instance != null
                    ? ObjectPool.Instance.Get(experienceGemPrefab, transform.position, Quaternion.identity)
                    : Instantiate(experienceGemPrefab, transform.position, Quaternion.identity);

                if (gem.TryGetComponent<ExperienceGem>(out var xpGem))
                {
                    xpGem.SetValue(_data.experienceValue);
                }
            }

            if (ObjectPool.Instance != null)
            {
                ObjectPool.Instance.Release(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void OnSpawned()
        {
            IsDead = false;
            _nextContactDamageTime = 0f;
            ActiveEnemies.Add(this);
        }

        public void OnDespawned()
        {
            ActiveEnemies.Remove(this);
        }

        private void OnDestroy()
        {
            ActiveEnemies.Remove(this);
        }
    }
}
