using UnityEngine;

namespace VampireLike
{
    /// <summary>
    /// Idle until the player gets within pickup range, then flies toward
    /// them and grants XP on contact. Distance check runs in FixedUpdate
    /// against Player.Instance rather than relying on trigger colliders,
    /// so the pickup radius can change live as PlayerStats.PickupRadius grows.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ExperienceGem : MonoBehaviour, IPoolable
    {
        [SerializeField] private float attractSpeed = 8f;
        [SerializeField] private float collectDistance = 0.3f;

        private Rigidbody2D _rigidbody;
        private float _xpValue;
        private bool _attracted;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.gravityScale = 0f;
        }

        public void SetValue(float xpValue) => _xpValue = xpValue;

        private void FixedUpdate()
        {
            if (Player.Instance == null) return;

            Vector2 toPlayer = (Vector2)Player.Instance.transform.position - _rigidbody.position;
            float distance = toPlayer.magnitude;

            if (!_attracted && distance <= Player.Instance.Stats.PickupRadius)
            {
                _attracted = true;
            }

            if (!_attracted) return;

            if (distance <= collectDistance)
            {
                Collect();
                return;
            }

            _rigidbody.MovePosition(_rigidbody.position + toPlayer.normalized * (attractSpeed * Time.fixedDeltaTime));
        }

        private void Collect()
        {
            Player.Instance.Leveling.AddExperience(_xpValue);

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
            _attracted = false;
        }

        public void OnDespawned()
        {
        }
    }
}
