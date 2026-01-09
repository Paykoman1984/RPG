using UnityEngine;
using System.Collections.Generic;

public class Hitbox : MonoBehaviour
{
    [Header("Hitbox Settings")]
    public float damage = 10f;
    public GameObject owner;
    public LayerMask targetLayers;
    public float activeDuration = 0.2f;

    [Header("Hit Prevention")]
    public bool preventMultipleHits = true;
    public float hitCooldown = 0.3f;

    private Collider2D hitboxCollider;
    private bool isActive = false;
    private HashSet<GameObject> alreadyHit = new HashSet<GameObject>();
    private List<GameObject> hitBuffer = new List<GameObject>();

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
        alreadyHit.Clear();
        hitBuffer.Clear();

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

        // Clear the hit list
        alreadyHit.Clear();
        hitBuffer.Clear();

        // Destroy instead of just deactivating
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive || owner == null) return;

        GameObject target = other.gameObject;

        // Add to buffer for processing
        if (!hitBuffer.Contains(target))
        {
            hitBuffer.Add(target);
            ProcessHitBuffer();
        }
    }

    private void ProcessHitBuffer()
    {
        foreach (GameObject target in hitBuffer.ToArray())
        {
            if (target == null) continue;

            // Skip if we've already hit this target
            if (preventMultipleHits && alreadyHit.Contains(target))
            {
                Debug.Log($"Already hit {target.name}, skipping...");
                hitBuffer.Remove(target);
                continue;
            }

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

                // Mark as hit if preventing multiple hits
                if (preventMultipleHits)
                {
                    alreadyHit.Add(target);
                }

                // Try to get HurtBox component from target or its children
                HurtBox hurtBox = target.GetComponent<HurtBox>();
                if (hurtBox == null)
                {
                    hurtBox = target.GetComponentInChildren<HurtBox>();
                }

                if (hurtBox != null)
                {
                    // Create DamageInfo
                    DamageInfo damageInfo = new DamageInfo(owner, damage);
                    damageInfo.hitPoint = transform.position;
                    damageInfo.hitDirection = (target.transform.position - owner.transform.position).normalized;

                    // Apply damage
                    hurtBox.TakeDamage(damageInfo);
                    Debug.Log($"✓ Applied {damage} damage to {target.name}");
                }
                else
                {
                    // Try to get Health component directly
                    Health health = target.GetComponent<Health>();
                    if (health == null)
                    {
                        health = target.GetComponentInParent<Health>();
                    }

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

            hitBuffer.Remove(target);
        }
    }

    private bool ShouldHitTarget(GameObject target)
    {
        if (target == null) return false;

        Debug.Log($"--- ShouldHitTarget Check ---");
        Debug.Log($"Target: {target.name}");
        Debug.Log($"Target layer: {target.layer} ({LayerMask.LayerToName(target.layer)})");

        // Check layer mask first (fastest check)
        bool layerMatches = (targetLayers.value & (1 << target.layer)) != 0;
        Debug.Log($"Layer matches: {layerMatches}");

        if (!layerMatches)
        {
            Debug.Log($"Layer mismatch! Target is on '{LayerMask.LayerToName(target.layer)}'");
            return false;
        }

        // Check if hitting self or owner
        if (owner != null)
        {
            // Direct match
            if (target == owner)
            {
                Debug.Log($"Skipping: Target is owner");
                return false;
            }

            // Child of owner
            if (target.transform.IsChildOf(owner.transform))
            {
                Debug.Log($"Skipping: Target is child of owner");
                return false;
            }

            // Check parent chain for ownership
            Transform parentCheck = target.transform.parent;
            while (parentCheck != null)
            {
                if (parentCheck.gameObject == owner)
                {
                    Debug.Log($"Skipping: Target is owned by owner (in parent chain)");
                    return false;
                }
                parentCheck = parentCheck.parent;
            }
        }

        // Additional safety check: make sure target has a Health or HurtBox component
        HurtBox hurtBox = target.GetComponent<HurtBox>();
        if (hurtBox == null)
        {
            hurtBox = target.GetComponentInChildren<HurtBox>();
        }

        Health health = target.GetComponent<Health>();
        if (health == null)
        {
            health = target.GetComponentInParent<Health>();
        }

        if (hurtBox == null && health == null)
        {
            Debug.Log($"Skipping: Target has no HurtBox or Health component");
            return false;
        }

        Debug.Log($"Should hit: TRUE");
        return true;
    }

    private void DebugHitInfo(GameObject target, bool willHit)
    {
        if (owner == null) return;

        string hitStatus = willHit ? "✓ WILL HIT" : "✗ WON'T HIT";
        string log = $"\n=== HITBOX DEBUG ===\n" +
                     $"Hitbox: {gameObject.name}\n" +
                     $"Owner: {owner.name}\n" +
                     $"Target: {target.name}\n" +
                     $"Target Layer: {target.layer} ({LayerMask.LayerToName(target.layer)})\n" +
                     $"Distance: {Vector3.Distance(transform.position, target.transform.position):F2}\n" +
                     $"Status: {hitStatus}\n" +
                     $"=====================";

        Debug.Log(log);

        // Draw debug line in scene view
        Debug.DrawLine(transform.position, target.transform.position,
                       willHit ? Color.green : Color.red, 1f);
    }
}