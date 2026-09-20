using UnityEngine;

namespace VampireLike
{
    /// <summary>Pulses damage to every enemy within range of the player (garlic/aura style).</summary>
    public class AreaWeapon : WeaponBase
    {
        [SerializeField] private LayerMask enemyLayerMask = ~0;

        private Collider2D[] _hitBuffer = new Collider2D[64];

        protected override void Fire()
        {
            Vector2 origin = Player.Instance.transform.position;
            float radius = GetAreaRadius();
            float damage = GetDamage();

            int hitCount = Physics2D.OverlapCircleNonAlloc(origin, radius, _hitBuffer, enemyLayerMask);

            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i] != null && _hitBuffer[i].TryGetComponent<EnemyController>(out var enemy) && !enemy.IsDead)
                {
                    enemy.TakeDamage(damage, origin);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (Player.Instance == null) return;
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.35f);
            Gizmos.DrawWireSphere(Player.Instance.transform.position, GetAreaRadius());
        }
    }
}
