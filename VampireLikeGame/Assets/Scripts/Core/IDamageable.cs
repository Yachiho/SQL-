using UnityEngine;

namespace VampireLike
{
    public interface IDamageable
    {
        void TakeDamage(float amount, Vector2 sourcePosition);
        bool IsDead { get; }
    }
}
