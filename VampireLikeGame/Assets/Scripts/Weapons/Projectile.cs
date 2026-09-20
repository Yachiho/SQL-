using UnityEngine;

namespace VampireLike
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float maxLifetime = 5f;

        private Rigidbody2D _rigidbody;
        private float _damage;
        private int _pierceRemaining;
        private float _speed;
        private Vector2 _direction;
        private float _spawnTime;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.gravityScale = 0f;
        }

        public void Launch(Vector2 origin, Vector2 direction, float speed, float damage, int pierce)
        {
            transform.position = origin;
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _pierceRemaining = pierce;
            _spawnTime = Time.time;

            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void FixedUpdate()
        {
            _rigidbody.MovePosition(_rigidbody.position + _direction * (_speed * Time.fixedDeltaTime));

            if (Time.time - _spawnTime >= maxLifetime)
            {
                Despawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Only enemies are valid targets: player-fired projectiles must never
            // hit the player's own IDamageable implementation.
            if (!other.TryGetComponent<EnemyController>(out var enemy)) return;
            if (enemy.IsDead) return;

            enemy.TakeDamage(_damage, _rigidbody.position);
            _pierceRemaining--;

            if (_pierceRemaining <= 0)
            {
                Despawn();
            }
        }

        private void Despawn()
        {
            if (ObjectPool.Instance != null)
            {
                ObjectPool.Instance.Release(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void OnSpawned() { }
        public void OnDespawned() { }
    }
}
