using UnityEngine;

public class PlayerDamageDealer : MonoBehaviour
{
    [Header("Attack Settings")]
    public GameObject attackHitboxPrefab;
    public float baseDamage = 20f;
    public LayerMask enemyLayers;
    public float minAttackRange = 0.3f;  // Minimum distance
    public float maxAttackRange = 0.6f;  // Maximum distance - REDUCED!

    [Header("Hitbox Settings")]
    public Vector2 hitboxSize = new Vector2(0.8f, 0.8f); // Increased size

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private Vector2 lastAttackDirection = Vector2.right;

    void Start()
    {
        if (attackHitboxPrefab == null)
        {
            Debug.LogError("NO ATTACK HITBOX PREFAB ASSIGNED!");
        }
    }

    public void PerformAttack(Vector2 direction)
    {
        if (attackHitboxPrefab == null)
        {
            Debug.LogError("Cannot perform attack: Missing prefab!");
            return;
        }

        // Handle zero direction
        if (direction == Vector2.zero)
        {
            direction = lastAttackDirection;
        }
        else
        {
            lastAttackDirection = direction;
        }

        direction = direction.normalized;

        if (showDebug)
        {
            Debug.Log($"=== PLAYER ATTACK ===");
            Debug.Log($"Direction: {direction}");
            Debug.Log($"Player position: {transform.position}");
        }

        // FIX: Use adaptive range - check for nearby enemies first
        float actualRange = GetAdaptiveAttackRange(direction);

        Vector2 spawnPosition = (Vector2)transform.position + (direction * actualRange);

        // Keep at player's Y level
        spawnPosition.y = transform.position.y;

        if (showDebug)
        {
            Debug.Log($"Adaptive range: {actualRange}");
            Debug.Log($"Hitbox spawn position: {spawnPosition}");
            Debug.Log($"Distance from player: {Vector2.Distance(transform.position, spawnPosition)}");
        }

        // Create hitbox
        GameObject hitboxObj = Instantiate(attackHitboxPrefab, spawnPosition, Quaternion.identity);

        // Scale the hitbox to be larger
        hitboxObj.transform.localScale = new Vector3(hitboxSize.x, hitboxSize.y, 1f);

        // Configure the hitbox
        Hitbox hitbox = hitboxObj.GetComponent<Hitbox>();
        if (hitbox != null)
        {
            hitbox.damage = baseDamage;
            hitbox.owner = gameObject;
            hitbox.targetLayers = enemyLayers;
            hitbox.Activate(gameObject);

            if (showDebug) Debug.Log($"Hitbox activated! Damage: {baseDamage}, Size: {hitboxSize}, Range: {actualRange}");
        }
        else
        {
            Debug.LogError("Instantiated hitbox prefab doesn't have Hitbox component!");
        }
    }

    private float GetAdaptiveAttackRange(Vector2 direction)
    {
        // Raycast to find enemies in attack direction
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            transform.position,
            direction,
            maxAttackRange,
            enemyLayers
        );

        if (hits.Length > 0)
        {
            // Find the closest enemy
            float closestDistance = maxAttackRange;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject != gameObject)
                {
                    float distance = Vector2.Distance(transform.position, hit.point);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                    }
                }
            }

            // Place hitbox slightly in front of the closest enemy
            float adaptiveRange = Mathf.Max(minAttackRange, closestDistance - 0.1f);
            adaptiveRange = Mathf.Min(adaptiveRange, maxAttackRange);

            if (showDebug) Debug.Log($"Found enemy at {closestDistance:F2}, using adaptive range: {adaptiveRange:F2}");
            return adaptiveRange;
        }

        // No enemy found, use default range
        return maxAttackRange;
    }

    void OnDrawGizmos()
    {
        if (!showDebug || !Application.isPlaying) return;

        // Draw attack range
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, maxAttackRange);
        Gizmos.DrawWireSphere(transform.position, minAttackRange);

        // Draw current direction
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)lastAttackDirection * maxAttackRange);

        // Draw hitbox size preview
        Gizmos.color = Color.green;
        Vector3 worldPos = transform.position + (Vector3)lastAttackDirection * maxAttackRange;
        Gizmos.DrawWireCube(worldPos, new Vector3(hitboxSize.x, hitboxSize.y, 0.1f));
    }
}