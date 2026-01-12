using PoEClone2D.Combat;
using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private float lifetime = 0.2f;
    [SerializeField] private bool destroyOnHit = true;
    [SerializeField] private bool canHitMultipleTargets = false;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private Vector2 knockbackDirection = Vector2.right;

    private GameObject owner;
    private bool hasHit = false;
    private Collider2D hitboxCollider;

    private void Start()
    {
        // Get collider reference
        hitboxCollider = GetComponent<Collider2D>();

        if (hitboxCollider == null)
        {
            Debug.LogWarning("EnemyAttackHitbox: No Collider2D found!");
        }

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    public void SetOwner(GameObject newOwner)
    {
        owner = newOwner;
    }

    public void SetKnockback(float force, Vector2 direction)
    {
        knockbackForce = force;
        knockbackDirection = direction.normalized;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canHitMultipleTargets && hasHit) return;
        if (owner != null && other.gameObject == owner) return;

        // Check if we should hit this object
        if (((1 << other.gameObject.layer) & hitLayers) != 0)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Apply damage
                damageable.TakeDamage(damage);

                // Apply knockback
                Vector2 actualKnockback = knockbackDirection * knockbackForce;
                damageable.ApplyKnockback(actualKnockback);

                hasHit = true;
                Debug.Log($"Enemy hit {other.name} for {damage} damage! Knockback: {actualKnockback}");

                // Disable collider to prevent multiple hits
                if (!canHitMultipleTargets && hitboxCollider != null)
                {
                    hitboxCollider.enabled = false;
                }

                // Destroy immediately if configured
                if (destroyOnHit)
                {
                    Destroy(gameObject, 0.05f);
                }
            }
        }
    }

    // For debugging
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        // Draw knockback direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)knockbackDirection * 0.5f);
    }
}