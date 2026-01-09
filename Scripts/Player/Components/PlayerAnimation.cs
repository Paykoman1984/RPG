// Assets/Scripts/Player/Components/PlayerAnimation.cs
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 currentDirection = Vector2.down;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
        {
            Debug.LogError("PlayerAnimation: No Animator found on GameObject!");
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("PlayerAnimation: No SpriteRenderer found on GameObject!");
        }

        Debug.Log("PlayerAnimation initialized");
    }

    public void UpdateAnimator(bool isMoving, bool isAttacking, bool isDashing, Vector2 direction)
    {
        if (animator == null)
        {
            Debug.LogWarning("PlayerAnimation: Animator is null!");
            return;
        }

        // Store direction
        currentDirection = direction;

        // Get 4-way direction for animation
        Vector2 animDir = Get4WayDirection(direction);

        // Update sprite flipping
        UpdateSpriteFlip(animDir);

        // Set animation parameters (only if they exist)
        SetFloatIfExists("MoveX", animDir.x);
        SetFloatIfExists("MoveY", animDir.y);
        SetBoolIfExists("IsMoving", isMoving && !isAttacking && !isDashing);
        SetBoolIfExists("IsAttacking", isAttacking);
        SetBoolIfExists("IsDashing", isDashing);

        Debug.Log($"Animator: Dir={animDir}, Moving={isMoving}, Attacking={isAttacking}, Dashing={isDashing}");
    }

    private Vector2 Get4WayDirection(Vector2 input)
    {
        if (input.magnitude < 0.1f) return currentDirection;

        float absX = Mathf.Abs(input.x);
        float absY = Mathf.Abs(input.y);

        if (absX > absY * 0.8f)
            return new Vector2(Mathf.Sign(input.x), 0);
        else
            return new Vector2(0, Mathf.Sign(input.y));
    }

    private void UpdateSpriteFlip(Vector2 animDir)
    {
        if (spriteRenderer == null) return;

        // Only flip for left/right movement
        if (animDir.x != 0)
        {
            spriteRenderer.flipX = animDir.x < 0;
            Debug.Log($"Sprite flip: {spriteRenderer.flipX} (animDir.x = {animDir.x})");
        }
    }

    // Safe parameter setters
    private void SetFloatIfExists(string paramName, float value)
    {
        if (HasParameter(paramName))
        {
            animator.SetFloat(paramName, value);
        }
    }

    private void SetBoolIfExists(string paramName, bool value)
    {
        if (HasParameter(paramName))
        {
            animator.SetBool(paramName, value);
        }
    }

    private bool HasParameter(string paramName)
    {
        if (animator == null || animator.parameterCount == 0) return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }

        Debug.LogWarning($"Animator parameter '{paramName}' not found!");
        return false;
    }

    public float AutoDetectAttackAnimationLength()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("No Animator or RuntimeAnimatorController found");
            return 0.5f;
        }

        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name.ToLower().Contains("attack"))
            {
                Debug.Log($"Found attack animation: '{clip.name}' ({clip.length:F2}s)");
                return clip.length;
            }
        }

        Debug.LogWarning("No attack animation found. Using default 0.5s");
        return 0.5f;
    }
}