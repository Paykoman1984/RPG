using UnityEngine;
using System.Collections;

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
        private Vector2 currentDirection;
        private bool isInitialized = false;

        // Track auto-complete coroutine
        private Coroutine autoCompleteCoroutine;

        // Events
        public System.Action OnAttackStart;
        public System.Action OnAttackActive;
        public System.Action OnAttackComplete;

        private void Start()
        {
            // Auto-initialize if attackData is already set
            if (attackData != null && !isInitialized)
            {
                Initialize(attackData);
            }
        }

        public void Initialize(AttackData data)
        {
            if (data == null)
            {
                Debug.LogError("MeleeAttack: Cannot initialize with null AttackData!");
                return;
            }

            attackData = data;
            isInitialized = true;

            // CRITICAL: Set lastAttackTime to allow immediate first attack
            // Set it to negative attack interval so Time.time - lastAttackTime >= attackInterval immediately
            lastAttackTime = -data.AttackInterval;

            Debug.Log($"🎯 MeleeAttack initialized with {data.name}");
            Debug.Log($"🎯 AttackInterval: {data.AttackInterval:F2}s, lastAttackTime set to {lastAttackTime:F2}");

            // Initialize all hitboxes
            if (hitboxes != null && hitboxes.Length > 0)
            {
                Debug.Log($"🎯 Found {hitboxes.Length} hitboxes");
                foreach (var hitbox in hitboxes)
                {
                    if (hitbox != null)
                    {
                        hitbox.Initialize(transform, data);
                        hitbox.SetTargetLayers(enemyLayers);
                        Debug.Log($"🎯 Hitbox initialized: {hitbox.name}");
                    }
                    else
                    {
                        Debug.LogWarning("MeleeAttack: Found null hitbox in array!");
                    }
                }
            }
            else
            {
                Debug.LogWarning("MeleeAttack: No hitboxes assigned or hitboxes array is empty!");
            }
        }

        public bool CanAttack()
        {
            if (!isInitialized)
            {
                Debug.LogWarning($"⚠️ MeleeAttack not initialized!");
                return false;
            }

            if (attackData == null)
            {
                Debug.LogError("❌ No AttackData assigned!");
                return false;
            }

            float timeSinceLastAttack = Time.time - lastAttackTime;
            float requiredInterval = attackData.AttackInterval;
            bool canAttack = !isAttacking && timeSinceLastAttack >= requiredInterval;

            // Only log when attack is on cooldown (for debugging)
            if (!canAttack && !isAttacking)
            {
                Debug.Log($"🎯 Attack on cooldown: {timeSinceLastAttack:F2}/{requiredInterval:F2}s");
            }

            return canAttack;
        }

        public void StartAttack(Vector2 direction)
        {
            if (!CanAttack())
            {
                Debug.LogError($"❌ Cannot start attack: CanAttack() returned false");
                return;
            }

            if (attackData == null)
            {
                Debug.LogError("❌ Cannot start attack: No AttackData!");
                return;
            }

            Debug.Log($"⚔️ ATTACK STARTED: Direction {direction}, Time: {Time.time:F2}");

            currentDirection = direction;

            // Position hitboxes based on direction
            PositionHitboxes(direction);

            isAttacking = true;
            lastAttackTime = Time.time;

            OnAttackStart?.Invoke();

            // Start a coroutine to auto-complete attack if animation events fail
            if (autoCompleteCoroutine != null)
                StopCoroutine(autoCompleteCoroutine);
            autoCompleteCoroutine = StartCoroutine(AutoCompleteAttack());
        }

        private IEnumerator AutoCompleteAttack()
        {
            // Calculate total attack duration based on AttackData timing
            float totalAttackTime = attackData.windupTime + attackData.activeTime + attackData.recoveryTime;

            // Add a small safety buffer
            float maxAttackTime = totalAttackTime + 0.2f;

            Debug.Log($"🎯 Auto-complete will check in {maxAttackTime:F2}s (windup: {attackData.windupTime}s, active: {attackData.activeTime}s, recovery: {attackData.recoveryTime}s)");

            // Wait for the expected attack duration
            yield return new WaitForSeconds(maxAttackTime);

            // Check if attack is still active
            if (isAttacking)
            {
                // FIXED: Don't log warning if it's within reasonable time
                float timeSinceStart = Time.time - lastAttackTime;

                // Only log if it's significantly overdue (more than 0.5s past expected)
                if (timeSinceStart > maxAttackTime + 0.5f)
                {
                    Debug.LogWarning($"⚠️ Attack didn't complete automatically after {timeSinceStart:F2} seconds. Forcing completion.");
                }
                else
                {
                    Debug.Log($"🎯 Auto-completing attack after {timeSinceStart:F2}s (normal completion)");
                }

                CompleteAttack();
            }
        }

        private void PositionHitboxes(Vector2 direction)
        {
            if (hitboxes == null || hitboxes.Length == 0)
            {
                Debug.LogError("❌ No hitboxes to position!");
                return;
            }

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

                    Debug.Log($"🎯 Positioned hitbox {hitbox.name} at {hitbox.transform.localPosition}");
                }
            }
        }

        // Call this from ANIMATION EVENT
        public void EnableHitboxes()
        {
            Debug.Log($"🎯 EnableHitboxes called. isAttacking={isAttacking}");

            if (!isAttacking)
            {
                // Attack might have been auto-completed or cancelled
                // Check if we're close to the attack start time before logging warning
                float timeSinceAttackStart = Time.time - lastAttackTime;
                if (timeSinceAttackStart < 0.5f) // Only warn if it's been less than 0.5 seconds
                {
                    Debug.LogWarning("⚠️ Tried to enable hitboxes but attack is not active!");
                }
                else
                {
                    Debug.Log("🎯 EnableHitboxes called after attack completed (normal)");
                }
                return;
            }

            if (hitboxes != null && hitboxes.Length > 0)
            {
                bool anyHitboxEnabled = false;
                foreach (var hitbox in hitboxes)
                {
                    if (hitbox != null)
                    {
                        hitbox.EnableHitbox();
                        anyHitboxEnabled = true;
                        Debug.Log($"🎯 Enabled hitbox: {hitbox.name}");
                    }
                }

                if (!anyHitboxEnabled)
                {
                    Debug.LogError("❌ No valid hitboxes to enable!");
                }
            }
            else
            {
                Debug.LogError("❌ No hitboxes to enable!");
            }

            OnAttackActive?.Invoke();
            Debug.Log("🔥 HITBOXES ENABLED");
        }

        // Call this from ANIMATION EVENT
        public void CompleteAttack()
        {
            Debug.Log($"🎯 CompleteAttack called. isAttacking={isAttacking}");

            if (!isAttacking)
            {
                // Already completed, just return silently
                Debug.Log($"🎯 Attack already completed, ignoring duplicate call");
                return;
            }

            Debug.Log("✅ ATTACK COMPLETE");

            // Disable all hitboxes
            DisableHitboxes();

            isAttacking = false;

            // Stop auto-complete coroutine if running
            if (autoCompleteCoroutine != null)
            {
                StopCoroutine(autoCompleteCoroutine);
                autoCompleteCoroutine = null;
            }

            OnAttackComplete?.Invoke();
        }

        private void DisableHitboxes()
        {
            if (hitboxes != null)
            {
                foreach (var hitbox in hitboxes)
                {
                    if (hitbox != null)
                    {
                        hitbox.DisableHitbox();
                    }
                }
            }
        }

        public void CancelAttack()
        {
            if (!isAttacking) return;

            DisableHitboxes();
            isAttacking = false;

            // Stop auto-complete coroutine
            if (autoCompleteCoroutine != null)
            {
                StopCoroutine(autoCompleteCoroutine);
                autoCompleteCoroutine = null;
            }

            OnAttackComplete?.Invoke();
            Debug.Log("❌ ATTACK CANCELLED");
        }

        public bool IsAttacking() => isAttacking;

        // NEW: Add this method to manually trigger auto-completion (useful for debugging)
        public void ForceCompleteAttack()
        {
            if (!isAttacking) return;

            Debug.Log("🎯 Manually forcing attack completion");
            CompleteAttack();
        }

        // Add this for debugging
        public void OnDrawGizmosSelected()
        {
            if (isAttacking && hitboxes != null)
            {
                Gizmos.color = Color.red;
                foreach (var hitbox in hitboxes)
                {
                    if (hitbox != null)
                    {
                        Gizmos.DrawWireSphere(hitbox.transform.position, 0.2f);
                    }
                }
            }
        }

        // NEW: Cleanup when destroyed
        private void OnDestroy()
        {
            if (autoCompleteCoroutine != null)
            {
                StopCoroutine(autoCompleteCoroutine);
                autoCompleteCoroutine = null;
            }
        }
    }
}