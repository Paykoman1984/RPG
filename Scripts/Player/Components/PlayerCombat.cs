// Assets/Scripts/Player/Components/PlayerCombat.cs
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private PlayerDamageDealer damageDealer;
    public bool IsAttacking { get; private set; }

    // REMOVED: IsDashing property (now handled by PlayerController)

    public event System.Action OnAttackStarted;
    public event System.Action OnAttackEnded;
    public event System.Action OnAttackCancelled;

    // REMOVED: Unused dash events
    // public event System.Action OnDashStarted;
    // public event System.Action OnDashEnded;

    private float nextAttackTime = 0f;
    private float attackStartTime = 0f;
    private float attackAnimationLength = 0.5f;

    public bool CanAttack()
    {
        return !IsAttacking && Time.time >= nextAttackTime;
    }

    public void StartAttack(Vector2 direction)
    {
        IsAttacking = true;
        attackStartTime = Time.time;
        nextAttackTime = Time.time + 0.5f; // Default cooldown

        //Trigger damage dealer
        if (damageDealer != null)
        {
            damageDealer.PerformAttack(direction);
        }

        OnAttackStarted?.Invoke();
    }

    public void EndAttack()
    {
        IsAttacking = false;
        OnAttackEnded?.Invoke();
    }

    public void CancelAttack()
    {
        IsAttacking = false;
        OnAttackCancelled?.Invoke();
    }

    public void UpdateCombatState()
    {
        // Check if attack should end naturally
        if (IsAttacking && Time.time - attackStartTime >= attackAnimationLength)
        {
            EndAttack();
        }
    }

    public void SetAttackAnimationLength(float length)
    {
        attackAnimationLength = length;
    }

    // Optional: Clean up the unused interface methods
    public void StartDash(Vector2 direction)
    {
        // Not used - dash is handled by PlayerController
    }

    public void CancelDash()
    {
        // Not used - dash is handled by PlayerController
    }

    public void SetPlayerData(PlayerData data)
    {
        // Optional: You could use this if you want data-driven combat
    }
}