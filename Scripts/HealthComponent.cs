// Assets/Scripts/Core/Combat/HealthComponent.cs
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Regeneration")]
    [SerializeField] private bool canRegenerate = false;
    [SerializeField] private float regenRate = 1f;
    [SerializeField] private float regenDelay = 3f;

    [Header("UI/Feedback")]
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private GameObject deathEffectPrefab;

    // Events
    public System.Action<float, GameObject> OnHealthChanged;
    public System.Action<GameObject> OnDeath;
    public System.Action<float, DamageInfo> OnDamageTaken;

    private float timeSinceLastDamage = 0f;
    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (canRegenerate && !isDead)
        {
            timeSinceLastDamage += Time.deltaTime;
            if (timeSinceLastDamage >= regenDelay && currentHealth < maxHealth)
            {
                RegenerateHealth(regenRate * Time.deltaTime);
            }
        }
    }

    // Fixed: Removed default value or use a different approach
    public void TakeDamage(float amount)
    {
        TakeDamage(amount, new DamageInfo(null, amount));
    }

    public void TakeDamage(float amount, DamageInfo damageInfo)
    {
        if (isDead) return;

        timeSinceLastDamage = 0f;
        currentHealth -= amount;

        // Clamp health
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die(damageInfo.source);
        }

        // Trigger events - FIXED: damageInfo is a struct, not nullable
        OnDamageTaken?.Invoke(amount, damageInfo);

        // Pass the source if it exists
        GameObject damageSource = damageInfo.source;
        OnHealthChanged?.Invoke(currentHealth, damageSource);

        // Show damage feedback
        if (damageInfo.hitPoint != Vector2.zero)
        {
            ShowDamageText(amount, damageInfo.hitPoint);
        }
        else
        {
            ShowDamageText(amount, transform.position);
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        OnHealthChanged?.Invoke(currentHealth, null);
    }

    private void RegenerateHealth(float amount)
    {
        Heal(amount);
    }

    private void Die(GameObject killer = null)
    {
        isDead = true;

        // Trigger death event
        OnDeath?.Invoke(killer);

        // Show death effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // Destroy or disable object
        // Destroy(gameObject, 0.1f);
        // OR: gameObject.SetActive(false);
    }

    private void ShowDamageText(float amount, Vector3 position)
    {
        if (damageTextPrefab != null)
        {
            GameObject textObj = Instantiate(damageTextPrefab, position, Quaternion.identity);
            // You might have a DamageText component on the prefab
            // textObj.GetComponent<DamageText>().SetText(amount.ToString("F0"));
        }
    }

    // Getters
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => maxHealth > 0 ? currentHealth / maxHealth : 0;
    public bool IsDead() => isDead;

    // Setters
    public void SetMaxHealth(float value)
    {
        maxHealth = value;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    public void SetCurrentHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        if (currentHealth <= 0 && !isDead)
            Die();
    }

    // Reset health on respawn
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
    }
}