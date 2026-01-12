using System.Collections.Generic;
using UnityEngine;

namespace PoEClone2D.Combat
{
    public class Hitbox : MonoBehaviour
    {
        [Header("Hitbox Settings")]
        [SerializeField] private Collider2D hitboxCollider;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private bool drawDebug = true;

        [Header("Attack Data")]
        [SerializeField] private AttackData attackData;
        [SerializeField] private Transform owner;

        // State
        private bool isActive = false;
        private HashSet<GameObject> hitThisActivation = new HashSet<GameObject>();

        // Events
        public System.Action<IDamageable> OnHit;

        private void Awake()
        {
            if (hitboxCollider == null)
                hitboxCollider = GetComponent<Collider2D>();

            // Start disabled
            if (hitboxCollider != null)
            {
                hitboxCollider.enabled = false;
                hitboxCollider.isTrigger = true;
            }
        }

        public void Initialize(Transform ownerTransform, AttackData data)
        {
            owner = ownerTransform;
            attackData = data;
        }

        public void SetTargetLayers(LayerMask layers)
        {
            targetLayers = layers;
        }

        // SINGLE ENABLE METHOD - NO TWO-PHASE BULLSHIT
        public void EnableHitbox()
        {
            Debug.Log($"HITBOX: Enabled on {gameObject.name}");

            // Clear previous hits
            hitThisActivation.Clear();

            // Enable collider
            isActive = true;
            if (hitboxCollider != null)
                hitboxCollider.enabled = true;
        }

        // SINGLE DISABLE METHOD
        public void DisableHitbox()
        {
            Debug.Log($"HITBOX: Disabled on {gameObject.name}");

            isActive = false;
            if (hitboxCollider != null)
                hitboxCollider.enabled = false;

            hitThisActivation.Clear();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActive) return;

            Debug.Log($"⚡ HITBOX TRIGGER: {other.name}");

            // Check layer
            int otherLayer = 1 << other.gameObject.layer;
            bool isInTargetLayer = (targetLayers.value & otherLayer) != 0;

            if (!isInTargetLayer)
            {
                Debug.Log($"Layer mismatch: {LayerMask.LayerToName(other.gameObject.layer)}");
                return;
            }

            // Prevent hitting owner
            if (owner != null && (other.transform == owner || other.transform.IsChildOf(owner)))
            {
                Debug.Log($"Skipping owner");
                return;
            }

            // Prevent duplicate hits
            if (hitThisActivation.Contains(other.gameObject))
            {
                Debug.Log($"Already hit");
                return;
            }

            // Get IDamageable
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable == null)
                damageable = other.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                ProcessHit(damageable, other.transform.position);
                hitThisActivation.Add(other.gameObject);
            }
            else
            {
                Debug.LogWarning($"No IDamageable on {other.name}");
            }
        }

        private void ProcessHit(IDamageable damageable, Vector3 hitPosition)
        {
            Debug.Log($"🎯 HIT CONFIRMED! {attackData.damage} damage to {damageable}");

            // Apply damage
            damageable.TakeDamage(attackData.damage);

            // Apply knockback - FIXED DIRECTION
            if (attackData != null && attackData.hasKnockback)
            {
                // Get knockback direction based on hitbox orientation (attack direction)
                Vector2 knockbackDirection;

                if (owner != null)
                {
                    // Option 1: Knockback in the direction the hitbox is facing (attack direction)
                    knockbackDirection = transform.right; // Hitbox is rotated to face attack direction

                    // Option 2: Or use hitbox's forward direction if you prefer
                    // knockbackDirection = transform.up;

                    Debug.Log($"Knockback direction from hitbox rotation: {knockbackDirection}");
                }
                else
                {
                    // Fallback: Away from hitbox center
                    knockbackDirection = (hitPosition - transform.position).normalized;
                    if (knockbackDirection.magnitude < 0.1f)
                        knockbackDirection = Vector2.right;
                }

                // Apply force
                damageable.ApplyKnockback(knockbackDirection * attackData.knockbackForce);
                Debug.Log($"Applied knockback: {knockbackDirection * attackData.knockbackForce}");
            }

            // Trigger events
            OnHit?.Invoke(damageable);

            // Visual effects
            if (attackData != null && attackData.hitEffectPrefab != null)
            {
                Instantiate(attackData.hitEffectPrefab, hitPosition, Quaternion.identity);
            }
        }

        public void SetAttackData(AttackData data) => attackData = data;

        // Debug visualization
        private void OnDrawGizmos()
        {
            if (!drawDebug || hitboxCollider == null) return;

            Gizmos.color = isActive ? new Color(1, 0, 0, 0.3f) : new Color(0, 1, 0, 0.1f);

            if (hitboxCollider is BoxCollider2D box)
            {
                Vector2 size = box.size;
                Gizmos.DrawCube(transform.position + (Vector3)box.offset, size);

                Gizmos.color = isActive ? Color.red : Color.green;
                Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, size);
            }
            else if (hitboxCollider is CircleCollider2D circle)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circle.offset, circle.radius);

                Gizmos.color = isActive ? Color.red : Color.green;
                Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }

#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f,
                isActive ? "ACTIVE" : "INACTIVE");
#endif
        }
    }
}