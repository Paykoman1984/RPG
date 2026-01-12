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
        private bool hasBeenDamaged = false; // Track if enemy has been damaged
        private bool isInAggroRange = false; // Track aggro state

        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D col;
        [SerializeField] private Animator animator;
        [SerializeField] private Color damageFlashColor = Color.red;
        [SerializeField] private float flashDuration = 0.1f;

        [Header("Knockback Settings")]
        [SerializeField] private float knockbackResistance = 0.2f;
        [SerializeField] private float maxKnockbackForce = 20f;
        [SerializeField] private float knockbackDuration = 0.3f;
        [SerializeField] private float postKnockbackStun = 0.2f;

        [Header("AI Settings")]
        [SerializeField] private float chaseRange = 5f;
        [SerializeField] private float chaseSpeed = 2f;
        [SerializeField] private float stopDistance = 1.5f;
        [SerializeField] private float attackRange = 1f;

        [Header("Combat")]
        [SerializeField] private float invincibilityDuration = 0.3f;

        [Header("Death")]
        [SerializeField] private GameObject deathEffectPrefab;
        [SerializeField] private float deathEffectDuration = 1f;
        [SerializeField] private bool instantDeath = true;

        // Animation Parameters
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int IsStunned = Animator.StringToHash("IsStunned");
        private static readonly int DeathTrigger = Animator.StringToHash("DeathTrigger");

        // State
        private Color originalColor;
        private Vector2 knockbackDirection;
        private float knockbackForce;
        private float knockbackTimer = 0f;
        private bool isInvincible = false;
        private float invincibilityTimer = 0f;
        private bool isStunned = false;
        private float stunTimer = 0f;
        private bool isBeingKnockedBack = false;
        private bool isDead = false;

        // AI
        private Transform playerTransform;
        private Vector2 movementDirection = Vector2.zero;

        // Public properties
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercent => Mathf.Clamp01(currentHealth / maxHealth);
        public bool IsDead => isDead;
        public float ChaseRange => chaseRange;

        private void Awake()
        {
            InitializeComponents();
            currentHealth = maxHealth;

            if (enemyHealthBar != null)
            {
                enemyHealthBar.Initialize(maxHealth, currentHealth);
            }

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            FindPlayer();
        }

        private void Start()
        {
            Debug.Log($"TestEnemy Start: Health = {currentHealth}/{maxHealth}");
        }

        private void Update()
        {
            if (isDead) return;

            UpdateTimers();
            UpdateAnimations();
            UpdateAggroState(); // Check aggro range and update health bar

            // Only handle AI if not being knocked back and not stunned
            if (!isBeingKnockedBack && !isStunned)
            {
                HandleAI();
            }
        }

        private void UpdateAggroState()
        {
            if (playerTransform == null || enemyHealthBar == null) return;

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            bool wasInAggroRange = isInAggroRange;
            isInAggroRange = distanceToPlayer <= chaseRange;

            // Health bar visibility logic:
            // 1. Show when first damaged AND in aggro range
            // 2. Hide when leaving aggro range (if was previously shown)
            // 3. Show again when re-entering aggro range (if was previously damaged)

            if (hasBeenDamaged)
            {
                if (isInAggroRange && !wasInAggroRange)
                {
                    // Just entered aggro range - show health bar
                    enemyHealthBar.ShowHealthBar();
                    Debug.Log("Health bar shown - entered aggro range");
                }
                else if (!isInAggroRange && wasInAggroRange)
                {
                    // Just left aggro range - hide health bar
                    enemyHealthBar.HideHealthBar();
                    Debug.Log("Health bar hidden - left aggro range");
                }
            }
        }

        private void InitializeComponents()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            if (col != null)
                col = GetComponent<Collider2D>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        private void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        public void TakeDamage(float damage)
        {
            if (isDead || isInvincible)
            {
                Debug.Log($"TestEnemy ignoring damage: isDead={isDead}, isInvincible={isInvincible}");
                return;
            }

            // Mark that enemy has been damaged
            if (!hasBeenDamaged)
            {
                hasBeenDamaged = true;
                Debug.Log("First damage taken - health bar will now show in aggro range");
            }

            currentHealth -= damage;

            if (enemyHealthBar != null)
            {
                enemyHealthBar.Change(-damage);

                // Always show health bar immediately when taking damage (if in aggro range)
                if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= chaseRange)
                {
                    enemyHealthBar.ShowHealthBar();
                }
            }

            Debug.Log($"TestEnemy took {damage} damage. Health: {currentHealth}/{maxHealth} ({(currentHealth / maxHealth) * 100:F1}%)");

            // Start invincibility (unless this is the killing blow)
            if (currentHealth > 0)
            {
                isInvincible = true;
                invincibilityTimer = invincibilityDuration;
            }

            // Visual feedback
            StartCoroutine(FlashDamage());

            if (currentHealth <= 0)
            {
                PerformDeath();
            }
        }

        public void ApplyKnockback(Vector2 force)
        {
            if (isDead) return;

            Debug.Log($"Applying knockback with force: {force}, magnitude: {force.magnitude}");

            // Apply knockback resistance
            Vector2 adjustedForce = force * (1f - knockbackResistance);

            // Cap the force
            float forceMagnitude = Mathf.Min(adjustedForce.magnitude, maxKnockbackForce);

            // Store knockback info
            knockbackDirection = adjustedForce.normalized;
            knockbackForce = forceMagnitude;
            knockbackTimer = knockbackDuration;

            // Clear any existing movement
            rb.linearVelocity = Vector2.zero;
            movementDirection = Vector2.zero;

            // Set state
            isBeingKnockedBack = true;
            isStunned = true;

            // Update animation
            if (animator != null)
            {
                animator.SetBool(IsStunned, true);
                animator.SetBool(IsRunning, false);
            }

            Debug.Log($"KNOCKBACK! Direction: {knockbackDirection}, Force: {knockbackForce}");
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            // Handle movement in FixedUpdate for physics consistency
            if (isBeingKnockedBack)
            {
                HandleKnockback();
            }
            else if (!isStunned && movementDirection.magnitude > 0.1f)
            {
                HandleMovement();
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        private void UpdateTimers()
        {
            // Update invincibility
            if (isInvincible)
            {
                invincibilityTimer -= Time.deltaTime;
                if (invincibilityTimer <= 0)
                {
                    isInvincible = false;
                }
            }

            // Update knockback
            if (isBeingKnockedBack)
            {
                knockbackTimer -= Time.deltaTime;
                if (knockbackTimer <= 0)
                {
                    // Knockback ended, start post-knockback stun
                    isBeingKnockedBack = false;
                    rb.linearVelocity = Vector2.zero;
                    isStunned = true;
                    stunTimer = postKnockbackStun;
                }
            }

            // Update stun
            if (isStunned && !isBeingKnockedBack)
            {
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0)
                {
                    isStunned = false;
                    if (animator != null)
                    {
                        animator.SetBool(IsStunned, false);
                    }
                }
            }
        }

        private void UpdateAnimations()
        {
            if (animator == null || isDead) return;

            // Update running animation
            bool isMoving = movementDirection.magnitude > 0.1f && !isBeingKnockedBack && !isStunned;
            animator.SetBool(IsRunning, isMoving);

            // Flip sprite
            if (spriteRenderer != null)
            {
                if (isBeingKnockedBack && knockbackDirection.magnitude > 0.1f)
                {
                    spriteRenderer.flipX = knockbackDirection.x > 0;
                }
                else if (isMoving)
                {
                    spriteRenderer.flipX = movementDirection.x < 0;
                }
            }
        }

        private void HandleAI()
        {
            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= chaseRange && distanceToPlayer > stopDistance)
            {
                // Chase player (but maintain stop distance)
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
                Vector2 targetPosition = (Vector2)playerTransform.position - directionToPlayer * stopDistance;
                movementDirection = (targetPosition - (Vector2)transform.position).normalized;
            }
            else
            {
                // Stop chasing
                movementDirection = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
            }
        }

        private void HandleMovement()
        {
            if (movementDirection.magnitude > 0.1f)
            {
                rb.linearVelocity = movementDirection * chaseSpeed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        private void HandleKnockback()
        {
            if (knockbackTimer <= 0) return;

            // Calculate knockback velocity with easing
            float t = 1f - (knockbackTimer / knockbackDuration);
            float easeOut = 1f - Mathf.Pow(1f - t, 3);
            float currentForce = Mathf.Lerp(knockbackForce, 0f, easeOut);

            // Apply knockback velocity
            rb.linearVelocity = knockbackDirection * currentForce;
        }

        private IEnumerator FlashDamage()
        {
            if (spriteRenderer != null)
            {
                Color original = spriteRenderer.color;
                spriteRenderer.color = damageFlashColor;
                yield return new WaitForSeconds(flashDuration);
                spriteRenderer.color = original;
            }
        }

        private void PerformDeath()
        {
            if (isDead) return;

            isDead = true;
            Debug.Log("Enemy died!");

            // Hide health bar
            if (enemyHealthBar != null)
            {
                enemyHealthBar.HideHealthBar();
            }

            // Stop all movement immediately
            rb.linearVelocity = Vector2.zero;
            movementDirection = Vector2.zero;
            isBeingKnockedBack = false;
            isStunned = false;

            // Disable collision
            if (col != null)
                col.enabled = false;

            // Disable Rigidbody physics
            if (rb != null)
                rb.simulated = false;

            // Death animation
            if (animator != null)
            {
                animator.SetBool(IsRunning, false);
                animator.SetBool(IsStunned, false);
                animator.SetTrigger(DeathTrigger);
            }

            // Visual effect
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }

            // Spawn death effect
            if (deathEffectPrefab != null)
            {
                GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, deathEffectDuration);
            }

            if (instantDeath)
            {
                // Instant destruction
                Destroy(gameObject, 0.1f);
            }
            else
            {
                // Delayed destruction (for death animation)
                Destroy(gameObject, 1f);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isDead) return;

            // If we hit something during knockback, stop immediately
            if (isBeingKnockedBack)
            {
                bool isPlayer = collision.gameObject.layer == LayerMask.NameToLayer("Player");

                if (!isPlayer)
                {
                    // Hit wall/obstacle
                    isBeingKnockedBack = false;
                    rb.linearVelocity = Vector2.zero;
                    knockbackTimer = 0f;

                    // Apply impact stun
                    isStunned = true;
                    stunTimer = 0.3f;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (isDead) return;

            // Draw chase range (aggro range)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, chaseRange);

            // Draw stop distance
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stopDistance);

            // Draw attack range
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Visualize health bar state
            if (Application.isPlaying)
            {
                Gizmos.color = hasBeenDamaged ? Color.green : Color.gray;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, new Vector3(0.5f, 0.5f, 0.1f));

                if (isInAggroRange)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireCube(transform.position + Vector3.up * 2.5f, new Vector3(0.3f, 0.3f, 0.1f));
                }
            }
        }

        public string GetDebugInfo()
        {
            if (isDead) return "DEAD";

            return $"Health: {currentHealth}/{maxHealth} ({HealthPercent * 100:F1}%)\n" +
                   $"State: {(isBeingKnockedBack ? "KNOCKBACK" : isStunned ? "STUNNED" : "NORMAL")}\n" +
                   $"Chasing: {movementDirection.magnitude > 0.1f}\n" +
                   $"Damaged: {hasBeenDamaged}, In Aggro: {isInAggroRange}";
        }
    }
}