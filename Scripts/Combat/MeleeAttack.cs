using UnityEngine;

namespace PoEClone2D.Combat
{
    public class MeleeAttack : MonoBehaviour
    {
        [Header("Hitbox References")]
        public Hitbox[] hitboxes;
        public LayerMask enemyLayers;

        [Header("Attack Settings")]
        private AttackData attackData;
        private float lastAttackTime = 0f;
        private bool isAttacking = false;

        // Events
        public System.Action OnAttackStart;
        public System.Action OnAttackActive;
        public System.Action OnAttackComplete;

        public void Initialize(AttackData data)
        {
            attackData = data;

            // Initialize all hitboxes
            if (hitboxes != null)
            {
                foreach (var hitbox in hitboxes)
                {
                    if (hitbox != null)
                    {
                        hitbox.Initialize(transform, data);
                        hitbox.SetTargetLayers(enemyLayers);
                    }
                }
            }
        }

        public bool CanAttack()
        {
            return !isAttacking && Time.time >= lastAttackTime + attackData.AttackInterval;
        }

        public void StartAttack(Vector2 direction)
        {
            if (!CanAttack() || attackData == null) return;

            // Position hitboxes based on direction
            PositionHitboxes(direction);

            isAttacking = true;
            lastAttackTime = Time.time;

            OnAttackStart?.Invoke();
            Debug.Log($"⚔️ ATTACK STARTED: Direction {direction}");
        }

        private void PositionHitboxes(Vector2 direction)
        {
            if (hitboxes == null || hitboxes.Length == 0) return;

            foreach (var hitbox in hitboxes)
            {
                if (hitbox != null)
                {
                    // Position hitbox in attack direction
                    float offset = attackData.range * 0.8f;
                    hitbox.transform.localPosition = direction * offset;

                    // Rotate hitbox if needed
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    hitbox.transform.localRotation = Quaternion.Euler(0, 0, angle);
                }
            }
        }

        // SINGLE ANIMATION EVENT - Call this from animation
        public void EnableHitboxes()
        {
            if (!isAttacking) return;

            if (hitboxes != null)
            {
                foreach (var hitbox in hitboxes)
                {
                    hitbox?.EnableHitbox();
                }
            }

            OnAttackActive?.Invoke();
            Debug.Log("🔥 HITBOXES ENABLED");
        }

        public void CompleteAttack()
        {
            Debug.Log("✅ ATTACK COMPLETE");

            // Disable all hitboxes
            DisableHitboxes();

            isAttacking = false;
            OnAttackComplete?.Invoke();
        }

        private void DisableHitboxes()
        {
            if (hitboxes != null)
            {
                foreach (var hitbox in hitboxes)
                {
                    hitbox?.DisableHitbox();
                }
            }
        }

        public void CancelAttack()
        {
            DisableHitboxes();
            isAttacking = false;
            OnAttackComplete?.Invoke();
            Debug.Log("❌ ATTACK CANCELLED");
        }

        public bool IsAttacking() => isAttacking;

        // Debug visualization
        private void OnDrawGizmosSelected()
        {
            if (hitboxes == null || !Application.isPlaying) return;

            Gizmos.color = isAttacking ? Color.red : Color.yellow;
            foreach (var hitbox in hitboxes)
            {
                if (hitbox != null)
                {
                    Collider2D col = hitbox.GetComponent<Collider2D>();
                    if (col != null && col.enabled)
                    {
                        if (col is BoxCollider2D box)
                        {
                            Gizmos.DrawWireCube(hitbox.transform.position, box.size);
                        }
                        else if (col is CircleCollider2D circle)
                        {
                            Gizmos.DrawWireSphere(hitbox.transform.position, circle.radius);
                        }
                    }
                }
            }
        }
    }
}