using UnityEngine;

public class PlayerDamageDealer : MonoBehaviour
{
    [Header("Attack Settings")]
    public GameObject attackHitboxPrefab;
    public float baseDamage = 20f;
    public LayerMask enemyLayers;
    public float attackRange = 1.5f; // Base range

    [Header("Hitbox Settings")]
    public Vector2 hitboxSize = new Vector2(2.0f, 2.0f); // Large hitbox

    [Header("Direction Adjustments")]
    public float leftAttackExtraRange = 0.5f;
    public float leftAttackSizeMultiplier = 1.5f;
    public float rightAttackExtraRange = 0.2f; // Added for right attacks too

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private bool drawGizmos = true;

    private Vector2 lastAttackDirection = Vector2.down;

    void Start()
    {
        if (attackHitboxPrefab == null)
        {
            Debug.LogError("PlayerDamageDealer: NO ATTACK HITBOX PREFAB ASSIGNED!");
        }
    }

    public void PerformAttack(Vector2 direction)
    {
        if (attackHitboxPrefab == null)
        {
            Debug.LogError("PlayerDamageDealer: Cannot perform attack - Missing prefab!");
            return;
        }

        // Handle zero direction
        if (direction == Vector2.zero)
        {
            direction = lastAttackDirection;
        }
        else
        {
            lastAttackDirection = direction.normalized;
        }

        direction = direction.normalized;

        if (showDebug)
        {
            Debug.Log($"=== PLAYER ATTACK ===");
            Debug.Log($"Direction: {direction}");
            Debug.Log($"Player position: {transform.position}");
        }

        // Calculate final range and size with direction adjustments
        float finalRange = attackRange;
        Vector2 finalSize = hitboxSize;

        // Direction-specific adjustments
        bool isLeftAttack = direction.x < -0.7f;
        bool isRightAttack = direction.x > 0.7f;

        if (isLeftAttack)
        {
            finalRange += leftAttackExtraRange;
            finalSize *= leftAttackSizeMultiplier;
            if (showDebug) Debug.Log($"LEFT ATTACK: Range +{leftAttackExtraRange}, Size x{leftAttackSizeMultiplier}");
        }
        else if (isRightAttack)
        {
            finalRange += rightAttackExtraRange;
            if (showDebug) Debug.Log($"RIGHT ATTACK: Range +{rightAttackExtraRange}");
        }

        // FIX: Calculate spawn position RELATIVE to player
        Vector2 spawnPosition = (Vector2)transform.position + (direction * finalRange);

        if (showDebug)
        {
            Debug.Log($"Final range: {finalRange}");
            Debug.Log($"Final hitbox size: {finalSize}");
            Debug.Log($"Hitbox spawn position: {spawnPosition}");
            Debug.Log($"Distance from player: {Vector2.Distance(transform.position, spawnPosition)}");
            Debug.Log($"Expected distance: {finalRange} (should match above)");
        }

        // Create hitbox
        GameObject hitboxObj = Instantiate(attackHitboxPrefab, spawnPosition, Quaternion.identity);
        hitboxObj.transform.localScale = new Vector3(finalSize.x, finalSize.y, 1f);

        // Configure hitbox
        Hitbox hitbox = hitboxObj.GetComponent<Hitbox>();
        if (hitbox != null)
        {
            hitbox.damage = baseDamage;
            hitbox.owner = gameObject;
            hitbox.targetLayers = enemyLayers;

            // Add debug info to hitbox
            if (showDebug)
            {
                hitbox.gameObject.name = $"PlayerHitbox_{direction.x:F1}_{direction.y:F1}";
            }

            hitbox.Activate(gameObject);

            if (showDebug)
            {
                Debug.Log($"✓ Hitbox activated!");
                Debug.Log($"✓ Player→Hitbox distance: {Vector2.Distance(transform.position, spawnPosition):F2}");
                Debug.Log($"✓ Direction: ({direction.x:F2}, {direction.y:F2})");
            }
        }
        else
        {
            Debug.LogError("PlayerDamageDealer: Instantiated hitbox doesn't have Hitbox component!");
        }
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos || !Application.isPlaying) return;

        // Draw player position
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        // Draw attack range
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw left attack extended range
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRange + leftAttackExtraRange);

        // Draw direction lines
        Gizmos.color = Color.red;
        Vector3 directionEnd = transform.position + (Vector3)lastAttackDirection * attackRange;
        Gizmos.DrawLine(transform.position, directionEnd);

        // Draw hitbox preview at the correct position
        float previewRange = attackRange;
        Vector2 previewSize = hitboxSize;

        if (lastAttackDirection.x < -0.7f)
        {
            previewRange += leftAttackExtraRange;
            previewSize *= leftAttackSizeMultiplier;
            Gizmos.color = Color.magenta;
        }
        else if (lastAttackDirection.x > 0.7f)
        {
            previewRange += rightAttackExtraRange;
            Gizmos.color = Color.cyan;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }

        Vector3 previewPos = transform.position + (Vector3)lastAttackDirection * previewRange;
        Gizmos.DrawWireCube(previewPos, new Vector3(previewSize.x, previewSize.y, 0.1f));

        // Draw line from player to hitbox
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, previewPos);

        // Label
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f,
            $"Player\nDir: {lastAttackDirection}");
        UnityEditor.Handles.Label(previewPos,
            $"Hitbox\nSize: {previewSize:F1}\nDist: {previewRange:F1}");
#endif
    }
}