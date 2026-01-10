using UnityEngine;
using PoEClone2D.Combat;

namespace PoEClone2D.Enemy
{
    public class TestEnemy : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 50f;
        [SerializeField] private float currentHealth;

        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D col;
        [SerializeField] private Color damageFlashColor = Color.red;
        [SerializeField] private float flashDuration = 0.1f;

        [Header("Knockback")]
        [SerializeField] private float knockbackResistance = 0.7f;
        [SerializeField] private float maxKnockbackVelocity = 10f;
        [SerializeField] private float knockbackDecay = 5f; // How fast knockback slows down

        [Header("Combat")]
        [SerializeField] private float invincibilityDuration = 0.3f;

        private Color originalColor;
        private Vector2 knockbackVelocity;
        private bool isInvincible = false;
        private float invincibilityTimer = 0f;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            if (col == null)
                col = GetComponent<Collider2D>();

            currentHealth = maxHealth;

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        public void TakeDamage(float damage)
        {
            if (isInvincible)
            {
                Debug.Log("Enemy is invincible, ignoring damage");
                return;
            }

            currentHealth -= damage;

            // Start invincibility
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;

            // Visual feedback
            StartCoroutine(FlashDamage());

            Debug.Log($"Enemy took {damage} damage. Health: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void ApplyKnockback(Vector2 force)
        {
            // Apply knockback resistance
            Vector2 adjustedForce = force * (1f - knockbackResistance);

            // Add to knockback velocity (but cap it)
            knockbackVelocity += adjustedForce;

            if (knockbackVelocity.magnitude > maxKnockbackVelocity)
            {
                knockbackVelocity = knockbackVelocity.normalized * maxKnockbackVelocity;
            }

            Debug.Log($"Knockback applied: {force}, Current velocity: {knockbackVelocity}");
        }

        private void Update()
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

            // Decay knockback over time
            if (knockbackVelocity.magnitude > 0.1f)
            {
                knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, knockbackDecay * Time.deltaTime);
                rb.linearVelocity = knockbackVelocity;
            }
            else
            {
                knockbackVelocity = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
            }
        }

        private System.Collections.IEnumerator FlashDamage()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = damageFlashColor;
                yield return new WaitForSeconds(flashDuration);
                spriteRenderer.color = originalColor;
            }
        }

        private void Die()
        {
            Debug.Log("Enemy died!");

            // Disable collisions
            if (col != null) col.enabled = false;
            if (rb != null) rb.simulated = false;

            // Visual effects
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }

            Destroy(gameObject, 1f);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Stop knockback when hitting walls/other enemies
            bool isWall = false;

            // Try to get tag if it exists
            if (!string.IsNullOrEmpty(collision.gameObject.tag))
            {
                isWall = collision.gameObject.CompareTag("Wall");
            }

            bool isEnemy = collision.gameObject.layer == LayerMask.NameToLayer("Enemy");

            // Also check for Ground/Environment layers
            bool isEnvironment = collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                                 collision.gameObject.layer == LayerMask.NameToLayer("Environment") ||
                                 collision.gameObject.layer == LayerMask.NameToLayer("Default"); // Default layer might be walls

            if (isWall || isEnemy || isEnvironment)
            {
                knockbackVelocity = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            // Prevent player from pushing enemy
            if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                // Don't let player physics push the enemy
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}