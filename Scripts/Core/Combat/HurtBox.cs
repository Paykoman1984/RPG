using UnityEngine;

public class HurtBox : MonoBehaviour
{
    [Header("Health Component")]
    [SerializeField] private Health healthComponent;

    [Header("Owner Reference")]
    [SerializeField] private GameObject _owner; // Serialized field for inspector

    // Property for accessing owner with fallback
    public GameObject owner => _owner != null ? _owner : gameObject;

    [Header("Damage Settings")]
    [SerializeField] private bool canTakeDamage = true;
    [SerializeField] private bool isInvulnerable = false;
    [SerializeField] private float damageMultiplier = 1f;

    [Header("Feedback")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private AudioClip hitSound;

    // Events for external listeners
    public System.Action<DamageInfo> OnDamageTaken;
    public System.Action<DamageInfo> OnDamageBlocked;
    public System.Action<DamageInfo> OnDamageEvaded;
    public System.Action OnDeath;

    private bool isDead = false;

    private void Awake()
    {
        // Try to find Health component if not assigned
        if (healthComponent == null)
        {
            healthComponent = GetComponent<Health>();
            if (healthComponent == null)
            {
                healthComponent = GetComponentInParent<Health>();
            }
        }

        // Set owner if not assigned
        if (_owner == null)
        {
            _owner = gameObject;
        }

        // Subscribe to Health component's death event
        if (healthComponent != null)
        {
            healthComponent.OnDeath += OnHealthDeath;
        }
    }

    private void Start()
    {
        // Log for debugging
        if (healthComponent == null)
        {
            Debug.LogWarning($"HurtBox on {gameObject.name}: No Health component found! Damage won't affect health bars.");
        }
        else
        {
            Debug.Log($"HurtBox on {gameObject.name} connected to Health component: {healthComponent.CurrentHealth}/{healthComponent.MaxHealth}");
        }
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (!canTakeDamage || isInvulnerable || isDead)
            return;

        // Check for evasion/block mechanics
        if (damageInfo.canEvade && CheckEvasion(damageInfo))
        {
            OnDamageEvaded?.Invoke(damageInfo);
            ShowEvasionFeedback(damageInfo);
            return;
        }

        if (damageInfo.canBlock && CheckBlock(damageInfo))
        {
            OnDamageBlocked?.Invoke(damageInfo);
            ShowBlockFeedback(damageInfo);
            return;
        }

        // Calculate total damage with multiplier
        float totalDamage = damageInfo.GetTotalDamage() * damageMultiplier;

        Debug.Log($"{gameObject.name} taking {totalDamage} damage");

        // Apply damage to Health component
        if (healthComponent != null)
        {
            healthComponent.TakeDamage(totalDamage);
        }
        else
        {
            Debug.LogWarning($"No Health component on {gameObject.name} to take damage!");
        }

        // Trigger damage taken event
        OnDamageTaken?.Invoke(damageInfo);

        // Show feedback
        ShowDamageFeedback(damageInfo);
    }

    private bool CheckEvasion(DamageInfo damageInfo)
    {
        if (damageInfo.alwaysHit) return false;

        // Add your evasion logic here
        // Example: Check character's evasion stat
        // return Random.value < evasionChance;
        return false;
    }

    private bool CheckBlock(DamageInfo damageInfo)
    {
        // Add your block logic here
        // Example: Check character's block chance
        // return Random.value < blockChance;
        return false;
    }

    private void ShowDamageFeedback(DamageInfo damageInfo)
    {
        if (hitEffectPrefab != null)
        {
            Vector3 spawnPosition = new Vector3(damageInfo.hitPoint.x, damageInfo.hitPoint.y, transform.position.z);
            Instantiate(hitEffectPrefab, spawnPosition, Quaternion.identity);
        }

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }

        // Optional: Trigger hit animation if you have an Animator AND the parameter exists
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            // Check if "Hit" parameter exists before setting it
            bool hasHitParam = false;
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == "Hit" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasHitParam = true;
                    break;
                }
            }

            if (hasHitParam)
            {
                animator.SetTrigger("Hit");
            }
        }
    }

    private void ShowEvasionFeedback(DamageInfo damageInfo)
    {
        // Show "MISS" text or dodge effect
        Debug.Log("Attack evaded by " + gameObject.name);

        // You could instantiate a "MISS" text effect here
    }

    private void ShowBlockFeedback(DamageInfo damageInfo)
    {
        // Show block effect or "BLOCKED" text
        Debug.Log("Attack blocked by " + gameObject.name);

        // You could instantiate a shield/spark effect here
    }

    // Called when Health component triggers death
    private void OnHealthDeath()
    {
        if (isDead) return;

        Die(null);
    }

    private void Die(GameObject killer = null)
    {
        isDead = true;

        Debug.Log(gameObject.name + " died! Killer: " + (killer != null ? killer.name : "Unknown"));

        // Trigger death event
        OnDeath?.Invoke();

        // Optional: Disable the hurtbox
        canTakeDamage = false;

        // Optional: Play death animation if parameter exists
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            // Check if "IsDead" parameter exists
            bool hasIsDeadParam = false;
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == "IsDead" && param.type == AnimatorControllerParameterType.Bool)
                {
                    hasIsDeadParam = true;
                    break;
                }
            }

            if (hasIsDeadParam)
            {
                animator.SetBool("IsDead", true);
            }
        }

        // Optional: Destroy after delay
        // Destroy(gameObject, 2f);
        // OR disable: gameObject.SetActive(false);
    }

    // Public methods for controlling invulnerability
    public void SetInvulnerable(bool invulnerable) => isInvulnerable = invulnerable;
    public void SetDamageMultiplier(float multiplier) => damageMultiplier = multiplier;
    public void EnableDamage() => canTakeDamage = true;
    public void DisableDamage() => canTakeDamage = false;

    // Health management methods - now delegate to Health component
    public void Heal(float amount)
    {
        if (isDead || healthComponent == null) return;
        healthComponent.Heal(amount);
    }

    public void SetMaxHealth(float value)
    {
        if (healthComponent != null)
        {
            // Note: Health component might not have SetMaxHealth method
            // You may need to add this to your Health.cs
        }
    }

    public void ResetHealth()
    {
        if (healthComponent != null)
        {
            // You'll need to add Reset method to Health.cs
        }
        isDead = false;
        canTakeDamage = true;
    }

    // Getters - now get from Health component
    public float GetHealth() => healthComponent != null ? healthComponent.CurrentHealth : 0;
    public float GetMaxHealth() => healthComponent != null ? healthComponent.MaxHealth : 0;
    public float GetHealthPercentage() => healthComponent != null ? healthComponent.CurrentHealth / healthComponent.MaxHealth : 0;
    public bool CanTakeDamage() => canTakeDamage;
    public bool IsInvulnerable() => isInvulnerable;
    public bool IsDead() => isDead || (healthComponent != null && !healthComponent.IsAlive);
    public bool IsAlive() => !IsDead();

    // For debugging in Inspector
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            // Draw health bar above object in scene view
            Vector3 pos = transform.position + Vector3.up * 1.5f;
            float healthPercent = GetHealthPercentage();

            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos + Vector3.right * healthPercent);
        }
    }

    private void OnDestroy()
    {
        if (healthComponent != null)
        {
            healthComponent.OnDeath -= OnHealthDeath;
        }
    }
}