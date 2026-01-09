// Assets/Scripts/Core/Interfaces/IAttackable.cs
using UnityEngine;

public interface IAttackable
{
    bool IsAttacking { get; }
    float AttackCooldown { get; }
    bool CanAttack();
    void StartAttack(Vector2 direction);
    void CancelAttack();
}