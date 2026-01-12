using UnityEngine;

namespace PoEClone2D.Combat
{
    public interface IDamageable
    {
        void TakeDamage(float damage);
        void ApplyKnockback(Vector2 force);
    }
}