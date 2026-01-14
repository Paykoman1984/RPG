using PoEClone2D.Combat;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PoEClone2D.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color damageFlashColor = Color.red;
        [SerializeField] private float flashDuration = 0.1f;

        [Header("Death Effects")]
        [SerializeField] private ParticleSystem deathParticlePrefab; // Assign a death particle effect
        [SerializeField] private float deathEffectDuration = 1f;
        [SerializeField] private GameObject deathEffectContainer; // Optional: to keep scene organized

        [Header("Respawn Effects")]
        [SerializeField] private ParticleSystem respawnParticlePrefab; // Assign a respawn particle effect
        [SerializeField] private float respawnEffectDuration = 1f;

        [Header("Invincibility")]
        [SerializeField] private float invincibilityDuration = 0.5f;
        [SerializeField] private bool isInvincible = false;
        [SerializeField] private float invincibilityTimer = 0f;

        [Header("Respawn Settings")]
        [SerializeField] private Transform respawnPoint; // Assign initial spawn point
        [SerializeField] private float respawnDelay = 2f; // Delay before respawn
        [SerializeField] private float respawnInvincibilityDuration = 3f; // Invincibility after respawn

        [Header("References")]
        [SerializeField] private SimplePlayerController playerController;
        [SerializeField] private PlayerCombat playerCombat;
        [SerializeField] private Collider2D playerCollider; // Optional: disable collider on death

        // Events
        public System.Action<float> OnHealthChanged;
        public System.Action OnPlayerDeath;
        public System.Action OnPlayerRespawn;

        private Color originalColor;
        private Vector3 initialPosition;
        private bool isDead = false;
        private Coroutine respawnCoroutine;
        private bool isSpriteRendererEnabled = true;

        //Health Bar UI
        public Slider slider;

        private void Awake()
        {
            // Get references if not set
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (playerController == null)
                playerController = GetComponent<SimplePlayerController>();

            if (playerCombat == null)
                playerCombat = GetComponent<PlayerCombat>();

            if (playerCollider == null)
                playerCollider = GetComponent<Collider2D>();

            // Create death effect container if not assigned
            if (deathEffectContainer == null && deathParticlePrefab != null)
            {
                deathEffectContainer = new GameObject("DeathEffects");
                deathEffectContainer.transform.SetParent(null); // Make it a root object
                DontDestroyOnLoad(deathEffectContainer); // Optional: persist between scenes
            }

            // Store initial position as respawn point if not set
            if (respawnPoint == null)
            {
                // Create an empty GameObject to store the initial position
                GameObject respawnObject = new GameObject("RespawnPoint");
                respawnObject.transform.position = transform.position;
                respawnObject.transform.rotation = transform.rotation;
                respawnPoint = respawnObject.transform;
            }
            else
            {
                initialPosition = respawnPoint.position;
            }

            // Initialize
            currentHealth = maxHealth;
            initialPosition = transform.position;

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
                isSpriteRendererEnabled = spriteRenderer.enabled;
            }

            Debug.Log($"Player Health initialized: {currentHealth}/{maxHealth}");
        }

        private void Start()
        {
            slider.maxValue = maxHealth;
            slider.value = currentHealth;
        }

        private void Update()
        {
            // Update invincibility timer
            if (isInvincible)
            {
                invincibilityTimer -= Time.deltaTime;
                if (invincibilityTimer <= 0)
                {
                    isInvincible = false;
                }
            }
        }

        public void TakeDamage(float damage)
        {
            // Don't take damage if invincible or dead
            if (isInvincible || isDead)
            {
                Debug.Log($"Player ignoring damage (invincible or dead)");
                return;
            }

            // Apply damage
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            slider.value = currentHealth;

            Debug.Log($"Player took {damage} damage! Health: {currentHealth}/{maxHealth}");

            // Visual feedback - FLASH IMMEDIATELY
            if (spriteRenderer != null)
            {
                StartCoroutine(DamageFlash());
            }

            // Invincibility frames (only if not dead)
            if (currentHealth > 0)
            {
                isInvincible = true;
                invincibilityTimer = invincibilityDuration;
            }

            // Notify health change
            OnHealthChanged?.Invoke(currentHealth / maxHealth);

            // Check for death
            if (currentHealth <= 0 && !isDead)
            {
                Die();
            }
        }

        private IEnumerator DamageFlash()
        {
            if (spriteRenderer == null) yield break;

            Color originalColor = spriteRenderer.color;

            // Flash to red
            spriteRenderer.color = damageFlashColor;

            // Wait briefly
            yield return new WaitForSeconds(flashDuration);

            // Return to original color
            spriteRenderer.color = originalColor;

            // Optional: Flash again for more visible effect
            yield return new WaitForSeconds(0.05f);
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(flashDuration / 2);
            spriteRenderer.color = originalColor;
        }

        public void ApplyKnockback(Vector2 force)
        {
            // Knockback disabled - just log
            Debug.Log($"Player knockback disabled (would have been: {force})");
        }

        private void Die()
        {
            if (isDead) return; // Prevent multiple death calls

            isDead = true;
            Debug.Log("Player died!");

            // Disable sprite renderer
            if (spriteRenderer != null)
            {
                isSpriteRendererEnabled = spriteRenderer.enabled;
                spriteRenderer.enabled = false;
                Debug.Log("SpriteRenderer disabled");
            }

            // Disable player collider (optional)
            if (playerCollider != null)
            {
                playerCollider.enabled = false;
            }

            // Disable player control
            if (playerController != null)
            {
                playerController.EnableMovement(false);
            }

            // Disable combat
            if (playerCombat != null)
            {
                playerCombat.SetAttackEnabled(false);
            }

            // Play death particle effect
            PlayDeathEffect();

            // Trigger death event
            OnPlayerDeath?.Invoke();

            // Start respawn coroutine
            if (respawnCoroutine != null)
            {
                StopCoroutine(respawnCoroutine);
            }
            respawnCoroutine = StartCoroutine(RespawnCoroutine());
        }

        private void PlayDeathEffect()
        {
            if (deathParticlePrefab != null)
            {
                // Instantiate death particles at player position
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

                // Play the effect
                deathEffect.Play();

                // Destroy after duration
                Destroy(deathEffect.gameObject, deathEffectDuration);

                Debug.Log("Death particle effect played");
            }
            else
            {
                Debug.LogWarning("No death particle prefab assigned!");
            }
        }

        private IEnumerator RespawnCoroutine()
        {
            Debug.Log($"Respawning in {respawnDelay} seconds...");

            // Wait for respawn delay
            yield return new WaitForSeconds(respawnDelay);

            // Reset player to initial position
            Respawn();
        }

        private void Respawn()
        {
            Debug.Log("Respawning player!");

            // Play respawn particle effect at respawn point
            PlayRespawnEffect();

            // Reset position
            if (respawnPoint != null)
            {
                transform.position = respawnPoint.position;
            }
            else
            {
                transform.position = initialPosition;
            }

            // Reset Rigidbody2D velocity
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // Enable sprite renderer
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = isSpriteRendererEnabled;
                spriteRenderer.color = originalColor;
                Debug.Log("SpriteRenderer enabled");
            }

            // Enable player collider
            if (playerCollider != null)
            {
                playerCollider.enabled = true;
            }

            // Reset health and status
            ResetHealth();
            isDead = false;

            // Enable player control
            if (playerController != null)
            {
                playerController.EnableMovement(true);
            }

            // Enable combat
            if (playerCombat != null)
            {
                playerCombat.SetAttackEnabled(true);
            }

            // Apply respawn invincibility
            isInvincible = true;
            invincibilityTimer = respawnInvincibilityDuration;

            // Visual feedback for respawn
            StartCoroutine(RespawnVisualFeedback());

            // Trigger respawn event
            OnPlayerRespawn?.Invoke();

            Debug.Log("Player respawned!");
        }

        private void PlayRespawnEffect()
        {
            if (respawnParticlePrefab != null)
            {
                Vector3 spawnPosition = respawnPoint != null ? respawnPoint.position : initialPosition;

                // Instantiate respawn particles at respawn position
                ParticleSystem respawnEffect = Instantiate(
                    respawnParticlePrefab,
                    spawnPosition,
                    Quaternion.identity
                );

                // Play the effect
                respawnEffect.Play();

                // Destroy after duration
                Destroy(respawnEffect.gameObject, respawnEffectDuration);

                Debug.Log("Respawn particle effect played");
            }
            else
            {
                Debug.LogWarning("No respawn particle prefab assigned!");
            }
        }

        private IEnumerator RespawnVisualFeedback()
        {
            if (spriteRenderer != null && spriteRenderer.enabled)
            {
                // Flash white a few times
                for (int i = 0; i < 3; i++)
                {
                    spriteRenderer.color = Color.white;
                    yield return new WaitForSeconds(0.2f);
                    spriteRenderer.color = originalColor;
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }

        // Optional: Healing method
        public void Heal(float amount)
        {
            // Don't heal if dead
            if (isDead) return;

            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            Debug.Log($"Player healed {amount}! Health: {currentHealth}/{maxHealth}");

            OnHealthChanged?.Invoke(currentHealth / maxHealth);
        }

        // Optional: Get health percentage
        public float GetHealthPercent()
        {
            return currentHealth / maxHealth;
        }

        // Optional: Check if player is alive
        public bool IsAlive()
        {
            return currentHealth > 0 && !isDead;
        }

        // Optional: Reset health (for respawn)
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isInvincible = false;

            if (spriteRenderer != null && spriteRenderer.enabled)
            {
                spriteRenderer.color = originalColor;
            }

            OnHealthChanged?.Invoke(1f);
        }

        // Public method to manually set respawn point (for checkpoints, etc.)
        public void SetRespawnPoint(Transform newRespawnPoint)
        {
            respawnPoint = newRespawnPoint;
            Debug.Log($"Respawn point updated to: {respawnPoint.position}");
        }

        // Public method to manually trigger respawn (for debugging)
        public void ForceRespawn()
        {
            if (isDead)
            {
                // Stop any running respawn coroutine
                if (respawnCoroutine != null)
                {
                    StopCoroutine(respawnCoroutine);
                }
                Respawn();
            }
            else
            {
                Debug.Log("Player is not dead, cannot force respawn.");
            }
        }

        // Getter for isDead
        public bool IsDead()
        {
            return isDead;
        }

        // Method to check if sprite renderer is enabled (for other scripts)
        public bool IsSpriteRendererEnabled()
        {
            return spriteRenderer != null && spriteRenderer.enabled;
        }
    }
}