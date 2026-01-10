using UnityEngine;

namespace PoEClone2D.Combat
{
    public class MeleeAttack : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private LayerMask enemyLayers;

        [Header("Attack Settings")]
        private AttackData attackData;
        private float lastAttackTime = 0f;
        private bool isAttacking = false;
        private float attackTimer = 0f;
        private AttackPhase currentPhase = AttackPhase.Idle;
        private Vector2 currentAttackDirection;

        private enum AttackPhase
        {
            Idle,
            Windup,
            Active,
            Recovery
        }

        // Events for animation/effects
        public System.Action OnAttackStart;
        public System.Action OnAttackWindupComplete;
        public System.Action OnAttackActive;
        public System.Action OnAttackComplete;

        // Damage dealing control
        private bool canDealDamage = false;
        private bool hasDealtDamage = false;

        // Public getter for attack range
        public float CurrentAttackRange => attackData?.range ?? 0f;

        public void Initialize(AttackData data)
        {
            attackData = data;

            // Create attack point if not assigned
            if (attackPoint == null)
            {
                GameObject point = new GameObject("AttackPoint");
                point.transform.SetParent(transform);
                point.transform.localPosition = new Vector3(0.5f, 0, 0);
                attackPoint = point.transform;
            }

            // Set this GameObject's layer to PlayerAttack for hit detection
            gameObject.layer = LayerMask.NameToLayer("PlayerAttack");
        }

        public bool CanAttack()
        {
            return !isAttacking && Time.time >= lastAttackTime + attackData.AttackInterval;
        }

        public void StartAttack(Vector2 direction)
        {
            if (!CanAttack() || attackData == null) return;

            Debug.Log($"Starting attack in direction: {direction}");

            // Store direction for the attack
            currentAttackDirection = direction;

            // Position attack point based on direction
            if (attackPoint != null)
            {
                // Calculate position based on direction
                float offsetDistance = 0.5f;
                Vector2 offset = direction.normalized * offsetDistance;

                attackPoint.localPosition = offset;

                Debug.Log($"Attack point positioned at: {attackPoint.position}, Local: {attackPoint.localPosition}");
            }

            isAttacking = true;
            canDealDamage = false;
            hasDealtDamage = false;
            attackTimer = 0f;
            currentPhase = AttackPhase.Windup;
            lastAttackTime = Time.time;

            OnAttackStart?.Invoke();
        }

        private void Update()
        {
            if (!isAttacking || attackData == null) return;

            attackTimer += Time.deltaTime;

            switch (currentPhase)
            {
                case AttackPhase.Windup:
                    if (attackTimer >= attackData.windupTime)
                    {
                        currentPhase = AttackPhase.Active;
                        attackTimer = 0f;
                        canDealDamage = true; // Enable damage when entering Active phase
                        hasDealtDamage = false;
                        OnAttackWindupComplete?.Invoke();

                        // Perform initial attack check immediately
                        if (canDealDamage && !hasDealtDamage)
                        {
                            PerformAttack();
                        }
                    }
                    break;

                case AttackPhase.Active:
                    // Fallback: Auto-attack if animation event missed
                    if (!hasDealtDamage && canDealDamage && attackTimer >= 0.05f)
                    {
                        PerformAttack();
                    }

                    if (attackTimer >= attackData.activeTime)
                    {
                        currentPhase = AttackPhase.Recovery;
                        attackTimer = 0f;
                        canDealDamage = false;
                    }
                    break;

                case AttackPhase.Recovery:
                    if (attackTimer >= attackData.recoveryTime)
                    {
                        isAttacking = false;
                        currentPhase = AttackPhase.Idle;
                        OnAttackComplete?.Invoke();
                    }
                    break;
            }
        }

        // ANIMATION EVENT: Called from animation at hit frame
        public void OnAttackHit()
        {
            Debug.Log($"ANIMATION EVENT: OnAttackHit - Phase: {currentPhase}, CanDeal: {canDealDamage}, HasDealt: {hasDealtDamage}");

            if (isAttacking && currentPhase == AttackPhase.Active && canDealDamage && !hasDealtDamage)
            {
                PerformAttack();
                hasDealtDamage = true;
            }
            else if (isAttacking && currentPhase == AttackPhase.Active && !canDealDamage && !hasDealtDamage)
            {
                // Animation event came before EnableDamage, but we're in Active phase
                Debug.Log("Animation event early - forcing attack");
                PerformAttack();
            }
            else
            {
                Debug.LogWarning($"Attack hit event ignored. State: Attacking={isAttacking}, Phase={currentPhase}, CanDeal={canDealDamage}, HasDealt={hasDealtDamage}");
            }
        }

        // Called when attack animation starts damage window
        public void EnableDamage()
        {
            if (isAttacking && currentPhase == AttackPhase.Active)
            {
                canDealDamage = true;
                hasDealtDamage = false;
                Debug.Log("Damage window opened");

                // Immediately check for attack if we haven't dealt damage yet
                if (canDealDamage && !hasDealtDamage)
                {
                    PerformAttack();
                }
            }
        }

        // Called when attack animation ends damage window
        public void DisableDamage()
        {
            canDealDamage = false;
            Debug.Log("Damage window closed");
        }

        private void PerformAttack()
        {
            if (hasDealtDamage && !attackData.allowMultipleHits)
            {
                Debug.LogWarning("Tried to perform attack but already dealt damage this attack");
                return;
            }

            Debug.Log("PERFORMING ATTACK: Dealing damage at animation hit frame!");
            OnAttackActive?.Invoke();

            // Check if enemyLayers is set properly
            if (enemyLayers.value == 0)
            {
                Debug.LogWarning("Enemy layers not set in MeleeAttack component!");
                return;
            }

            // Use the non-deprecated Physics2D.OverlapCircleAll method
            Collider2D[] hitEnemies;

            if (attackData.isAreaAttack && attackData.areaRadius > 0)
            {
                hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackData.areaRadius, enemyLayers);
            }
            else
            {
                hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackData.range, enemyLayers);
            }

            Debug.Log($"Found {hitEnemies.Length} enemies in attack range");

            // Damage each enemy
            foreach (Collider2D enemy in hitEnemies)
            {
                // Skip if enemy is null or is the player/attack point itself
                if (enemy == null || enemy.transform == attackPoint || enemy.transform.IsChildOf(transform))
                    continue;

                Debug.Log($"Hit: {enemy.gameObject.name} at position: {enemy.transform.position}");

                if (enemy.TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    Debug.Log($"Dealing {attackData.damage} damage to {enemy.gameObject.name}");
                    damageable.TakeDamage(attackData.damage);

                    // Apply knockback
                    if (attackData.hasKnockback)
                    {
                        Vector2 knockbackDirection = (enemy.transform.position - attackPoint.position).normalized;

                        // Ensure knockbackDirection is not zero
                        if (knockbackDirection.magnitude < 0.1f)
                        {
                            knockbackDirection = currentAttackDirection;
                        }

                        Debug.Log($"Knockback direction: {knockbackDirection}, Force: {attackData.knockbackForce}");
                        damageable.ApplyKnockback(knockbackDirection * attackData.knockbackForce);
                    }
                }
                else
                {
                    Debug.LogWarning($"{enemy.gameObject.name} doesn't have IDamageable component!");
                }
            }

            // Mark that we've dealt damage
            hasDealtDamage = true;

            // Visual effects
            if (attackData.hitEffectPrefab != null)
            {
                Instantiate(attackData.hitEffectPrefab, attackPoint.position, Quaternion.identity);
            }
        }

        public bool IsAttacking()
        {
            return isAttacking;
        }

        public void CancelAttack()
        {
            if (isAttacking)
            {
                isAttacking = false;
                currentPhase = AttackPhase.Idle;
                canDealDamage = false;
                hasDealtDamage = false;
                OnAttackComplete?.Invoke();
                Debug.Log("Attack cancelled!");
            }
        }

        // Debug visualization
        private void OnDrawGizmosSelected()
        {
            if (attackPoint != null && attackData != null)
            {
                Gizmos.color = Color.red;
                if (attackData.isAreaAttack && attackData.areaRadius > 0)
                {
                    Gizmos.DrawWireSphere(attackPoint.position, attackData.areaRadius);
                }
                else
                {
                    Gizmos.DrawWireSphere(attackPoint.position, attackData.range);
                }
            }
        }

        private void OnDrawGizmos()
        {
            // Always show attack point in Scene view
            if (attackPoint != null && attackData != null)
            {
                Gizmos.color = new Color(1, 0, 0, 0.3f);
                Gizmos.DrawSphere(attackPoint.position, 0.1f);

                // Show attack range when attacking
                if (Application.isPlaying && isAttacking && currentPhase == AttackPhase.Active)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(attackPoint.position, attackData.range);
                }
            }
        }
    }
}