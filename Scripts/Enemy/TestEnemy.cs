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
        [SerializeField] private ParticleSystem deathParticlePrefab; // Death particle effect
        [SerializeField] private float deathEffectDuration = 1f;
        [SerializeField] private GameObject deathEffectContainer; // Optional: to keep scene organized
        [SerializeField] private bool disableSpriteOnDeath = true; // Turn off sprite on death

        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D col;
        [SerializeField] private Animator animator;
        [SerializeField] private Color damageFlashColor = Color.red;
        [SerializeField] private float flashDuration = 0.1f;

        // Animation Parameters
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int AttackTrigger = Animator.StringToHash("Attack");

        // State
        private Color originalColor;
        private bool isDead = false;
        private bool canAttack = true;

        // AI
        private Transform playerTransform;
        private Vector2 movementDirection = Vector2.zero;
        private float attackTimer = 0f;

        // Particle effect tracking
        private ParticleSystem currentDeathEffect;

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
                deathEffectContainer.transform.SetParent(null); // Make it a root object
                DontDestroyOnLoad(deathEffectContainer); // Optional: persist between scenes
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

            // Update attack timer
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    canAttack = true;
                }
            }

            UpdateAI();
            UpdateAnimations();

            // EMERGENCY FIX: Force exit attack if stuck
            FixStuckAttackAnimation();
        }

        private void UpdateAI()
        {
            if (playerTransform == null) return;

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // Face player
            if (spriteRenderer != null)
            {
                Vector2 direction = (playerTransform.position - transform.position).normalized;
                spriteRenderer.flipX = direction.x < 0;
            }

            // Check if player is in range
            if (distanceToPlayer <= chaseRange)
            {
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;

                // Attack if in range
                if (distanceToPlayer <= attackRange && canAttack)
                {
                    Attack();
                }
                // Chase if not in attack range
                else if (distanceToPlayer > stopDistance)
                {
                    movementDirection = directionToPlayer;
                }
                else
                {
                    movementDirection = Vector2.zero;
                }
            }
            else
            {
                movementDirection = Vector2.zero;
            }
        }

        private void Attack()
        {
            canAttack = false;
            attackTimer = attackCooldown;
            movementDirection = Vector2.zero;

            Debug.Log("Enemy attacking!");

            if (animator != null)
            {
                animator.ResetTrigger(AttackTrigger);
                animator.SetTrigger(AttackTrigger);
            }
        }

        public void OnAttackHitFrame()
        {
            Debug.Log("Attack hit frame!");

            if (attackHitboxPrefab != null && attackOrigin != null)
            {
                GameObject hitbox = Instantiate(attackHitboxPrefab, attackOrigin.position, Quaternion.identity);

                // Get the EnemyAttackHitbox component (NOT AttackHitbox)
                EnemyAttackHitbox hitboxScript = hitbox.GetComponent<EnemyAttackHitbox>();
                if (hitboxScript != null)
                {
                    // Set damage
                    hitboxScript.SetDamage(attackDamage);

                    // Set owner (so enemy doesn't hit themselves)
                    hitboxScript.SetOwner(gameObject);

                    // Set knockback direction based on which way enemy is facing
                    Vector2 knockbackDir = spriteRenderer.flipX ? Vector2.left : Vector2.right;
                    hitboxScript.SetKnockback(5f, knockbackDir);

                    Debug.Log($"Enemy hitbox created with {attackDamage} damage, knockback: {knockbackDir * 5f}");
                }
                else
                {
                    Debug.LogWarning("Attack hitbox prefab doesn't have EnemyAttackHitbox script!");
                }

                Destroy(hitbox, 0.2f);
            }
            else
            {
                Debug.LogWarning("Attack hitbox prefab or attack origin not set!");
            }
        }

        public void OnAttackEnd()
        {
            Debug.Log("Attack animation ended");
            // Animation should automatically transition back to Idle
            // via Animator transition settings
        }

        private void FixStuckAttackAnimation()
        {
            if (animator == null) return;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // If we're in Attack state but animation finished long ago
            if (stateInfo.IsName("Base Layer.Attack") && stateInfo.normalizedTime > 1.0f)
            {
                Debug.Log($"EMERGENCY: Attack stuck at time {stateInfo.normalizedTime:F1}, forcing Idle");

                // Force reset everything
                animator.ResetTrigger(AttackTrigger);
                animator.Play("Base Layer.Idle", 0, 0f);

                // Clear movement
                movementDirection = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
            }
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            if (movementDirection.magnitude > 0.1f)
            {
                rb.linearVelocity = movementDirection * chaseSpeed;
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
            if (isDead) return;

            if (!hasBeenDamaged)
            {
                hasBeenDamaged = true;
                Debug.Log("First damage taken - health bar will now show in aggro range");
            }

            currentHealth -= damage;

            if (enemyHealthBar != null)
            {
                enemyHealthBar.Change(-damage);

                if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= chaseRange)
                {
                    enemyHealthBar.ShowHealthBar();
                }
            }

            Debug.Log($"TestEnemy took {damage} damage. Health: {currentHealth}/{maxHealth}");

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
                spriteRenderer.color = damageFlashColor;
                yield return new WaitForSeconds(flashDuration);
                spriteRenderer.color = originalColor;
            }
        }

        public void ApplyKnockback(Vector2 force)
        {
            if (isDead) return;

            Debug.Log($"Enemy knockback applied: {force}");

            // Clear existing velocity first
            rb.linearVelocity = Vector2.zero;

            // Apply the force
            rb.AddForce(force, ForceMode2D.Impulse);

            // Also apply a small upward force for better visual effect
            rb.AddForce(Vector2.up * Mathf.Abs(force.y) * 0.3f, ForceMode2D.Impulse);
        }

        private void PlayDeathEffect()
        {
            if (deathParticlePrefab != null)
            {
                // Instantiate death particles at enemy position
                ParticleSystem deathEffect = Instantiate(
                    deathParticlePrefab,
                    transform.position,
                    Quaternion.identity
                );

                // Set parent to container if available
                if (deathEffectContainer != null)
                {
                    deathEffect.transform.SetParent(deathEffectContainer.transform);
                }

                // Store reference
                currentDeathEffect = deathEffect;

                // Play the effect
                deathEffect.Play();

                // Destroy after duration
                Destroy(deathEffect.gameObject, deathEffectDuration);

                Debug.Log("Enemy death particle effect played");
            }
            else
            {
                Debug.LogWarning("No death particle prefab assigned!");
            }
        }

        private void DisableEnemyComponents()
        {
            // Disable sprite renderer if enabled
            if (disableSpriteOnDeath && spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            // Disable collider to prevent further interactions
            if (col != null)
            {
                col.enabled = false;
            }

            // Stop movement
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false; // Disable physics
            }

            // Disable animator
            if (animator != null)
            {
                animator.enabled = false;
            }

            // Disable health bar if it exists
            if (enemyHealthBar != null)
            {
                enemyHealthBar.gameObject.SetActive(false);
            }
        }

        private void PerformDeath()
        {
            isDead = true;
            Debug.Log("Enemy died!");

            // Disable enemy components
            DisableEnemyComponents();

            // Play death particle effect
            PlayDeathEffect();

            // Destroy the enemy GameObject after delay
            StartCoroutine(DestroyAfterDeath());
        }

        private IEnumerator DestroyAfterDeath()
        {
            // Wait for particle effect to play
            yield return new WaitForSeconds(deathEffectDuration * 0.8f); // Slightly shorter than particle duration

            Debug.Log("Destroying enemy GameObject");

            // Optional: Fade out if sprite is still enabled
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

            Destroy(gameObject);
        }

        // Clean up any particle effects when enemy is destroyed
        private void OnDestroy()
        {
            // Clean up any remaining particle effects
            if (currentDeathEffect != null && currentDeathEffect.gameObject != null)
            {
                Destroy(currentDeathEffect.gameObject);
            }
        }
    }
}