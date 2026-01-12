using UnityEngine;
using PoEClone2D.Combat;
using System.Collections;

namespace PoEClone2D.Enemy
{
    public class TestEnemy : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 50f;
        private float currentHealth;

        [SerializeField] private EnemyHealthBar enemyHealthBar;
        private bool hasBeenDamaged = false;

        [Header("Combat")]
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private GameObject attackHitboxPrefab;
        [SerializeField] private Transform attackOrigin;

        [Header("Movement")]
        [SerializeField] private float chaseRange = 5f;
        [SerializeField] private float chaseSpeed = 3f;
        [SerializeField] private float stopDistance = 0.8f;

        [Header("Death Effects")]
        [SerializeField] private ParticleSystem deathParticlePrefab;
        [SerializeField] private float deathEffectDuration = 1f;
        [SerializeField] private GameObject deathEffectContainer;
        [SerializeField] private bool disableSpriteOnDeath = true;

        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D col;
        [SerializeField] private Animator animator;
        [SerializeField] private Color damageFlashColor = Color.red;
        [SerializeField] private float flashDuration = 0.1f;

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugGUI = true;
        [SerializeField] private bool logAllEvents = true;

        // Animation Parameters
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int AttackTrigger = Animator.StringToHash("Attack");

        // State
        private Color originalColor;
        private bool isDead = false;
        private bool canAttack = true;
        private bool isAttacking = false;

        // AI
        private Transform playerTransform;
        private Vector2 movementDirection = Vector2.zero;
        private float attackTimer = 0f;

        // Particle effect tracking
        private ParticleSystem currentDeathEffect;

        // Debug info
        private float timeSinceLastHit = 0f;

        // Attack safety
        private Coroutine attackSafetyCoroutine;

        // Track when attack started for stuck detection
        private float attackStartTime = 0f;

        // Public properties
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercent => Mathf.Clamp01(currentHealth / maxHealth);
        public bool IsDead => isDead;
        public float ChaseRange => chaseRange;

        private void Awake()
        {
            currentHealth = maxHealth;

            if (enemyHealthBar != null)
            {
                enemyHealthBar.Initialize(maxHealth, currentHealth);
            }

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            // Create death effect container if not assigned
            if (deathEffectContainer == null && deathParticlePrefab != null)
            {
                deathEffectContainer = new GameObject("EnemyDeathEffects");
                deathEffectContainer.transform.SetParent(null);
                DontDestroyOnLoad(deathEffectContainer);
            }

            FindPlayer();

            if (logAllEvents) Debug.Log($"<color=yellow>[TestEnemy]</color> Awake: Health = {currentHealth}/{maxHealth}");
        }

        private void Start()
        {
            Debug.Log($"<color=yellow>[TestEnemy]</color> Start: Initialized at {transform.position}");
            Debug.Log($"<color=yellow>[TestEnemy]</color> Rigidbody: Mass={rb.mass}, Drag={rb.linearDamping}, Gravity={rb.gravityScale}");
        }

        private void Update()
        {
            if (isDead)
            {
                if (logAllEvents) Debug.Log($"<color=red>[TestEnemy]</color> DEAD - Skipping all updates");
                return;
            }

            timeSinceLastHit += Time.deltaTime;

            // Update attack timer
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    canAttack = true;
                    if (logAllEvents) Debug.Log($"<color=green>[TestEnemy]</color> Attack cooldown finished");
                }
            }

            UpdateAI();
            UpdateAnimations();

            // EMERGENCY FIX: Force exit attack if stuck
            FixStuckAttackAnimation();

            // Debug state - less frequent logging
            if (Time.frameCount % 300 == 0) // Log every 5 seconds (60fps * 5)
            {
                Debug.Log($"<color=gray>[TestEnemy]</color> Frame {Time.frameCount}: Health={currentHealth}/{maxHealth}, CanAttack={canAttack}, IsAttacking={isAttacking}, Velocity={rb.linearVelocity.magnitude:F2}");
            }
        }

        private void UpdateAI()
        {
            if (playerTransform == null)
            {
                if (playerTransform == null && !isDead)
                {
                    Debug.LogWarning("<color=orange>[TestEnemy]</color> PlayerTransform is null!");
                }
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // Face player
            if (spriteRenderer != null)
            {
                Vector2 direction = (playerTransform.position - transform.position).normalized;
                bool shouldFlip = direction.x < 0;
                if (spriteRenderer.flipX != shouldFlip)
                {
                    spriteRenderer.flipX = shouldFlip;
                    if (logAllEvents) Debug.Log($"<color=blue>[TestEnemy]</color> Flipped to face {(shouldFlip ? "left" : "right")}");
                }
            }

            // Check if player is in range
            if (distanceToPlayer <= chaseRange)
            {
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;

                // Attack if in range
                if (distanceToPlayer <= attackRange && CanAttackPlayer())
                {
                    Attack();
                }
                // Chase if not in attack range
                else if (distanceToPlayer > stopDistance)
                {
                    movementDirection = directionToPlayer;
                    if (logAllEvents) Debug.Log($"<color=blue>[TestEnemy]</color> Chasing player: {directionToPlayer}");
                }
                else
                {
                    movementDirection = Vector2.zero;
                    if (logAllEvents) Debug.Log($"<color=blue>[TestEnemy]</color> Within stop distance, holding position");
                }
            }
            else
            {
                movementDirection = Vector2.zero;
                if (logAllEvents) Debug.Log($"<color=blue>[TestEnemy]</color> Player out of chase range ({distanceToPlayer:F2} > {chaseRange})");
            }
        }

        private bool CanAttackPlayer()
        {
            if (!canAttack || isAttacking) return false;

            if (playerTransform == null) return false;

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // Only attack if player is in range and not already attacking
            return distanceToPlayer <= attackRange && attackTimer <= 0;
        }

        private void Attack()
        {
            if (isAttacking)
            {
                Debug.LogWarning("<color=orange>[TestEnemy]</color> Already attacking!");
                return;
            }

            canAttack = false;
            attackTimer = attackCooldown;
            movementDirection = Vector2.zero;
            isAttacking = true;
            attackStartTime = Time.time; // Track when attack started

            Debug.Log($"<color=red>[TestEnemy]</color> ATTACKING! Cooldown: {attackCooldown}s");

            if (animator != null)
            {
                animator.ResetTrigger(AttackTrigger);
                animator.SetTrigger(AttackTrigger);
            }

            // Start safety coroutine
            if (attackSafetyCoroutine != null)
                StopCoroutine(attackSafetyCoroutine);
            attackSafetyCoroutine = StartCoroutine(AttackSafetyCoroutine());
        }

        private IEnumerator AttackSafetyCoroutine()
        {
            // Wait for maximum reasonable attack duration
            float maxAttackTime = 3.0f; // 3 seconds max
            yield return new WaitForSeconds(maxAttackTime);

            if (isAttacking)
            {
                Debug.LogWarning("<color=orange>[TestEnemy]</color> Attack safety timeout - forcing completion");
                OnAttackEnd(); // Call the end method to clean up
            }
        }

        public void OnAttackHitFrame()
        {
            Debug.Log($"<color=red>[TestEnemy]</color> Attack hit frame! Creating hitbox...");

            if (attackHitboxPrefab != null && attackOrigin != null)
            {
                GameObject hitbox = Instantiate(attackHitboxPrefab, attackOrigin.position, Quaternion.identity);

                EnemyAttackHitbox hitboxScript = hitbox.GetComponent<EnemyAttackHitbox>();
                if (hitboxScript != null)
                {
                    // Set damage ONLY
                    hitboxScript.SetDamage(attackDamage);

                    // Set owner (so enemy doesn't hit themselves)
                    hitboxScript.SetOwner(gameObject);

                    Debug.Log($"<color=red>[TestEnemy]</color> Hitbox created: {attackDamage} damage");
                }
                else
                {
                    Debug.LogError("<color=red>[TestEnemy]</color> Attack hitbox prefab doesn't have EnemyAttackHitbox script!");
                }

                Destroy(hitbox, 0.2f);
            }
            else
            {
                Debug.LogError("<color=red>[TestEnemy]</color> Attack hitbox prefab or attack origin not set!");
            }
        }

        public void OnAttackEnd()
        {
            isAttacking = false;

            // Stop safety coroutine
            if (attackSafetyCoroutine != null)
            {
                StopCoroutine(attackSafetyCoroutine);
                attackSafetyCoroutine = null;
            }

            Debug.Log($"<color=yellow>[TestEnemy]</color> Attack animation ended");
        }

        private void FixStuckAttackAnimation()
        {
            if (animator == null) return;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool isInAttackState = stateInfo.IsName("Attack"); // Check for "Attack" state name

            // Only fix if we're REALLY stuck (animation finished long ago)
            if (isInAttackState && stateInfo.normalizedTime >= 1.0f)
            {
                float extraTime = stateInfo.normalizedTime - 1.0f;

                // Only warn and fix if animation is 100% over AND we've been in this state for 0.5 extra seconds
                if (extraTime > 0.5f)
                {
                    Debug.LogWarning($"<color=orange>[TestEnemy]</color> Attack animation stuck {extraTime:F1}s after completion. Resetting.");

                    // Reset attack state
                    isAttacking = false;
                    animator.Play("Idle", 0, 0f); // Play "Idle" state

                    // Clear movement
                    movementDirection = Vector2.zero;
                    rb.linearVelocity = Vector2.zero;

                    // Stop safety coroutine
                    if (attackSafetyCoroutine != null)
                    {
                        StopCoroutine(attackSafetyCoroutine);
                        attackSafetyCoroutine = null;
                    }
                }
            }

            // Also check if animation finished but we still think we're attacking
            if (isAttacking && !isInAttackState)
            {
                // This is normal - animation might have transitioned to idle
                // Check how long we've been "attacking" without an attack animation
                float timeSinceAttackStart = Time.time - attackStartTime;
                if (timeSinceAttackStart > 1.0f)
                {
                    Debug.Log($"<color=yellow>[TestEnemy]</color> Attack logic active for {timeSinceAttackStart:F1}s but animation in different state. This might be normal.");
                    // Don't force fix - let the animation events handle it
                }
            }
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            if (movementDirection.magnitude > 0.1f)
            {
                rb.linearVelocity = movementDirection * chaseSpeed;
                if (logAllEvents) Debug.Log($"<color=cyan>[TestEnemy]</color> Moving: {movementDirection} * {chaseSpeed} = {rb.linearVelocity}");
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        private void UpdateAnimations()
        {
            if (animator == null || isDead) return;

            // Simple: if moving, run; if not moving, idle
            bool isMoving = movementDirection.magnitude > 0.1f;
            animator.SetBool(IsRunning, isMoving);

            if (logAllEvents && Time.frameCount % 600 == 0) // Every 10 seconds
            {
                Debug.Log($"<color=gray>[TestEnemy]</color> Animation: IsRunning={isMoving}, IsAttacking={isAttacking}");
            }
        }

        private void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log($"<color=green>[TestEnemy]</color> Found player at {playerTransform.position}");
            }
            else
            {
                Debug.LogError("<color=red>[TestEnemy]</color> No player found with tag 'Player'!");
            }
        }

        public void TakeDamage(float damage)
        {
            if (isDead)
            {
                Debug.Log($"<color=gray>[TestEnemy]</color> Ignoring damage - already dead");
                return;
            }

            timeSinceLastHit = 0f;

            if (!hasBeenDamaged)
            {
                hasBeenDamaged = true;
                Debug.Log($"<color=yellow>[TestEnemy]</color> First damage taken - health bar will now show in aggro range");
            }

            float previousHealth = currentHealth;
            currentHealth -= damage;

            if (enemyHealthBar != null)
            {
                enemyHealthBar.Change(-damage);

                if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= chaseRange)
                {
                    enemyHealthBar.ShowHealthBar();
                }
            }

            Debug.Log($"<color=orange>[TestEnemy]</color> TOOK DAMAGE: {damage} (Before: {previousHealth}, After: {currentHealth}/{maxHealth})");

            // Flash damage visual
            StartCoroutine(FlashDamage());

            if (currentHealth <= 0)
            {
                PerformDeath();
            }
        }

        private IEnumerator FlashDamage()
        {
            if (spriteRenderer != null && spriteRenderer.enabled)
            {
                Color original = spriteRenderer.color;
                spriteRenderer.color = damageFlashColor;
                Debug.Log($"<color=magenta>[TestEnemy]</color> Flashing damage color");
                yield return new WaitForSeconds(flashDuration);
                spriteRenderer.color = original;
                Debug.Log($"<color=magenta>[TestEnemy]</color> Restored original color");
            }
            else
            {
                Debug.LogWarning("<color=orange>[TestEnemy]</color> Cannot flash - spriteRenderer missing or disabled");
            }
        }

        public void ApplyKnockback(Vector2 force)
        {
            // Knockback disabled - just log
            Debug.Log($"<color=gray>[TestEnemy]</color> Knockback disabled (would have been: {force})");
        }

        private void PlayDeathEffect()
        {
            if (deathParticlePrefab != null)
            {
                ParticleSystem deathEffect = Instantiate(
                    deathParticlePrefab,
                    transform.position,
                    Quaternion.identity
                );

                if (deathEffectContainer != null)
                {
                    deathEffect.transform.SetParent(deathEffectContainer.transform);
                }

                currentDeathEffect = deathEffect;
                deathEffect.Play();

                Destroy(deathEffect.gameObject, deathEffectDuration);

                Debug.Log($"<color=red>[TestEnemy]</color> Death particle effect played");
            }
            else
            {
                Debug.LogWarning("<color=orange>[TestEnemy]</color> No death particle prefab assigned!");
            }
        }

        private void DisableEnemyComponents()
        {
            Debug.Log($"<color=red>[TestEnemy]</color> Disabling enemy components");

            if (disableSpriteOnDeath && spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            if (col != null)
            {
                col.enabled = false;
            }

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }

            if (animator != null)
            {
                animator.enabled = false;
            }

            if (enemyHealthBar != null)
            {
                enemyHealthBar.gameObject.SetActive(false);
            }

            // Stop attack safety coroutine
            if (attackSafetyCoroutine != null)
            {
                StopCoroutine(attackSafetyCoroutine);
                attackSafetyCoroutine = null;
            }
        }

        private void PerformDeath()
        {
            isDead = true;
            Debug.Log($"<color=red>[TestEnemy]</color> DIED at position {transform.position}");

            DisableEnemyComponents();
            PlayDeathEffect();

            StartCoroutine(DestroyAfterDeath());
        }

        private IEnumerator DestroyAfterDeath()
        {
            Debug.Log($"<color=red>[TestEnemy]</color> Will be destroyed in {deathEffectDuration * 0.8f:F2}s");

            yield return new WaitForSeconds(deathEffectDuration * 0.8f);

            if (spriteRenderer != null && spriteRenderer.enabled)
            {
                float fadeDuration = 0.2f;
                float elapsedTime = 0f;
                Color startColor = spriteRenderer.color;

                while (elapsedTime < fadeDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                    spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                    yield return null;
                }
            }

            Debug.Log($"<color=red>[TestEnemy]</color> Final destruction");
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (currentDeathEffect != null && currentDeathEffect.gameObject != null)
            {
                Destroy(currentDeathEffect.gameObject);
            }

            // Stop coroutine
            if (attackSafetyCoroutine != null)
            {
                StopCoroutine(attackSafetyCoroutine);
            }

            Debug.Log($"<color=gray>[TestEnemy]</color> GameObject destroyed");
        }

        // Debug GUI - FIXED CAMERA REFERENCE
        private void OnGUI()
        {
            if (!showDebugGUI) return;

            // Use UnityEngine.Camera to avoid namespace conflict
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null) return;

            GUI.color = Color.white;
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 12;
            style.normal.textColor = Color.white;

            Vector2 screenPos = mainCamera.WorldToScreenPoint(transform.position);
            screenPos.y = Screen.height - screenPos.y;

            string debugText = $"Enemy Debug:\n";
            debugText += $"Health: {currentHealth:F0}/{maxHealth:F0}\n";
            debugText += $"CanAttack: {canAttack}\n";
            debugText += $"IsAttacking: {isAttacking}\n";
            debugText += $"Cooldown: {attackTimer:F1}s\n";
            debugText += $"Velocity: {rb.linearVelocity.magnitude:F2}\n";
            debugText += $"Last Hit: {timeSinceLastHit:F1}s ago";

            GUI.Label(new Rect(screenPos.x, screenPos.y - 100, 200, 140), debugText, style);
        }

        // Gizmos for visual debugging
        private void OnDrawGizmosSelected()
        {
            // Draw chase range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, chaseRange);

            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Draw stop distance
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, stopDistance);

            // Draw movement direction
            if (movementDirection.magnitude > 0.1f)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, movementDirection * 2f);
            }

            // Draw velocity vector
            if (rb != null && rb.linearVelocity.magnitude > 0.1f)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(transform.position, rb.linearVelocity.normalized * rb.linearVelocity.magnitude * 0.5f);
            }
        }
    }
}