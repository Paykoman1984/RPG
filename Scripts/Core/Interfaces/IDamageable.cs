// Assets/Scripts/Core/Interfaces/IDamageable.cs
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(DamageInfo damageInfo);
    void Heal(float amount);
    bool IsAlive { get; }
    float CurrentHealth { get; }
    float MaxHealth { get; }
    GameObject gameObject { get; } // Access to GameObject
}