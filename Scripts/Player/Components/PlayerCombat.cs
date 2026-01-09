using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private PlayerDamageDealer damageDealer;
    public bool IsAttacking { get; private set; }

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f; // Half second cooldown
    [SerializeField] private float attackAnimationLength = 0.5f;
    [SerializeField] private bool canAttackDuringCooldown = false; // IMPORTANT: Set to FALSE

    private Vector2 lastAttackDirection = Vector2.down;
    private float nextAttackTime = 0f;
    private float attackStartTime = 0f;

    public event System.Action OnAttackStarted;
    public event System.Action OnAttackEnded;
    public event System.Action OnAttackCancelled;

    void Awake()
    {
        if (damageDealer == null)
        {
            damageDealer = GetComponent<PlayerDamageDealer>();
            if (damageDealer == null)
            {
                Debug.LogWarning("PlayerCombat: No PlayerDamageDealer assigned or found");
            }
        }

        Debug.Log("PlayerCombat initialized");
    }

    public bool CanAttack()
    {
        // FIX: Check both cooldown AND if not currently attacking
        if (IsAttacking) return false;

        // Check cooldown
        if (Time.time < nextAttackTime)
        {
            if (!canAttackDuringCooldown)
            {
                return false;
            }
        }

        return true;
    }

    public void StartAttack(Vector2 direction)
    {
        // FIX: Double-check we can attack
        if (!CanAttack())
        {
            Debug.Log($"PlayerCombat: Cannot attack - IsAttacking: {IsAttacking}, Time: {Time.time}, NextAttackTime: {nextAttackTime}");
            return;
        }

        // Handle zero direction
        if (direction.magnitude < 0.1f)
        {
            direction = lastAttackDirection;
        }
        else
        {
            lastAttackDirection = direction.normalized;
        }

        IsAttacking = true;
        attackStartTime = Time.time;
        nextAttackTime = Time.time + attackCooldown; // Set cooldown

        Debug.Log($"PlayerCombat: Starting attack with direction {direction}");
        Debug.Log($"PlayerCombat: Next attack available at {nextAttackTime} (current: {Time.time})");

        // Trigger damage dealer
        if (damageDealer != null)
        {
            damageDealer.PerformAttack(direction.normalized);
        }
        else
        {
            Debug.LogError("PlayerCombat: No damage dealer to perform attack!");
        }

        OnAttackStarted?.Invoke();

        // Auto-end attack after animation length
        Invoke(nameof(EndAttack), attackAnimationLength);
    }

    public void EndAttack()
    {
        if (!IsAttacking) return;

        IsAttacking = false;
        OnAttackEnded?.Invoke();
        Debug.Log("PlayerCombat: Attack ended");
    }

    public void CancelAttack()
    {
        IsAttacking = false;
        OnAttackCancelled?.Invoke();
    }

    public void UpdateCombatState()
    {
        // No longer needed since we use Invoke
    }

    public void SetAttackAnimationLength(float length)
    {
        attackAnimationLength = length;
    }

    public Vector2 GetLastAttackDirection()
    {
        return lastAttackDirection;
    }

    public void SetPlayerData(PlayerData data)
    {
        if (data != null)
        {
            attackCooldown = data.attackCooldown;
            attackAnimationLength = 0.5f;
        }
    }
}