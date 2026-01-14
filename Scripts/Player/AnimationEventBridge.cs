using PoEClone2D.Combat;
using PoEClone2D.Player;
using UnityEngine;

public class AnimationEventBridge : MonoBehaviour
{
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private MeleeAttack meleeAttack;
    [SerializeField] private Animator animator;

    private void Awake()
    {
        // Try to find components automatically
        if (playerCombat == null)
            playerCombat = GetComponentInParent<PlayerCombat>();

        if (meleeAttack == null)
            meleeAttack = GetComponentInParent<MeleeAttack>();

        if (animator == null)
            animator = GetComponent<Animator>();

        Debug.Log($"AnimationEventBridge: Found playerCombat={playerCombat != null}, meleeAttack={meleeAttack != null}, animator={animator != null}");
    }

    // ANIMATION EVENT: Called at windup
    public void OnAttackWindup()
    {
        Debug.Log("ANIMATION: Attack windup event received");
        // Empty - no sound at windup
    }

    // ANIMATION EVENT: Called at impact frame
    public void OnAttackImpact()
    {
        Debug.Log("ANIMATION: Attack impact frame");
        if (meleeAttack != null)
        {
            meleeAttack.EnableHitboxes();
        }
        else
        {
            Debug.LogError("MeleeAttack reference missing!");
        }
    }

    // ANIMATION EVENT: Called at end of animation
    public void OnAttackComplete()
    {
        Debug.Log("ANIMATION: Attack complete event received");

        // Call PlayerCombat first
        if (playerCombat != null)
        {
            playerCombat.OnAttackCompleted();
        }
        else
        {
            Debug.LogError("PlayerCombat reference missing!");
        }

        // Then call MeleeAttack
        if (meleeAttack != null)
        {
            meleeAttack.CompleteAttack();
        }
        else
        {
            Debug.LogError("MeleeAttack reference missing!");
        }
    }
}