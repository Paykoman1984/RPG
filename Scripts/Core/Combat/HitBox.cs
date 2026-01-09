using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Hitbox Settings")]
    public float damage = 10f;
    public GameObject owner;
    public LayerMask targetLayers;
    public float activeDuration = 0.2f;

    private Collider2D hitboxCollider;
    private bool isActive = false;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
            Debug.Log($"Hitbox.Awake: Collider found - {hitboxCollider.GetType().Name}, initially disabled");
        }
        else
        {
            Debug.LogWarning("Hitbox: No Collider2D found!");
        }
    }

    public void Activate(GameObject ownerObject)
    {
        Debug.Log("=== HITBOX ACTIVATE ===");
        Debug.Log($"Hitbox: {gameObject.name}");
        Debug.Log($"Owner: {(ownerObject != null ? ownerObject.name : "NULL")}");

        owner = ownerObject;

        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = true;
            Debug.Log($"Collider enabled: {hitboxCollider.enabled}");
        }

        isActive = true;

        // Auto-deactivate after duration
        Invoke(nameof(Deactivate), activeDuration);
        Debug.Log($"Will auto-deactivate in {activeDuration} seconds");

        // Log target layers for debugging
        string layerNames = "";
        for (int i = 0; i < 32; i++)
        {
            if (targetLayers == (targetLayers | (1 << i)))
            {
                layerNames += $"{i}:{LayerMask.LayerToName(i)}, ";
            }
        }
        Debug.Log($"Target layers: {layerNames}");
    }

    public void Deactivate()
    {
        Debug.Log($"Hitbox.Deactivate: {gameObject.name}");

        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }

        isActive = false;

        // Optional: Destroy instead of just deactivating
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive || owner == null) return;

        GameObject target = other.gameObject;
        Debug.Log($"=== HITBOX TRIGGER ===");
        Debug.Log($"Hitbox: {gameObject.name} at position {transform.position}");
        Debug.Log($"Hitbox scale: {transform.localScale}");
        Debug.Log($"Target: {target.name} at position {target.transform.position}");
        Debug.Log($"Target layer: {target.layer} ({LayerMask.LayerToName(target.layer)})");
        Debug.Log($"Distance to target: {Vector2.Distance(transform.position, target.transform.position)}");

        // Check if we should hit this target
        if (ShouldHitTarget(target))
        {
            Debug.Log($"✓ HITTING target: {target.name}");

            // Try to get HurtBox component
            HurtBox hurtBox = target.GetComponent<HurtBox>();
            if (hurtBox != null)
            {
                // Create DamageInfo
                DamageInfo damageInfo = new DamageInfo(owner, damage);
                damageInfo.hitPoint = other.ClosestPoint(transform.position);
                damageInfo.hitDirection = (target.transform.position - owner.transform.position).normalized;

                // Apply damage
                hurtBox.TakeDamage(damageInfo);
                Debug.Log($"✓ Applied {damage} damage to {target.name}");
            }
            else
            {
                // Try to get Health component directly
                Health health = target.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                    Debug.Log($"✓ Applied {damage} damage to {target.name} via Health component");
                }
                else
                {
                    Debug.LogWarning($"✗ No HurtBox or Health component found on {target.name}");
                }
            }
        }
        else
        {
            Debug.Log($"✗ NOT hitting target: {target.name}");
        }
    }

    private bool ShouldHitTarget(GameObject target)
    {
        Debug.Log($"--- ShouldHitTarget Check ---");
        Debug.Log($"Target: {target.name}");
        Debug.Log($"Target layer: {target.layer} ({LayerMask.LayerToName(target.layer)})");

        // Check layer mask
        bool layerMatches = (targetLayers.value & (1 << target.layer)) != 0;
        Debug.Log($"Layer matches: {layerMatches}");

        if (!layerMatches)
        {
            Debug.Log($"Layer mismatch! Target is on '{LayerMask.LayerToName(target.layer)}'");
            return false;
        }

        // Check if hitting self or owner
        if (target == owner || target.transform.IsChildOf(owner.transform))
        {
            Debug.Log($"Skipping: Target is owner or child of owner");
            return false;
        }

        // Check if target has same tag as owner (allies shouldn't hit each other)
        if (owner != null && target.CompareTag(owner.tag))
        {
            Debug.Log($"Skipping: Target has same tag as owner ({owner.tag})");
            return false;
        }

        Debug.Log($"Should hit: TRUE");
        return true;
    }
}