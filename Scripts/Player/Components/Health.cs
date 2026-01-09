using UnityEngine;

[System.Serializable]
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Events")]
    public System.Action<float> OnHealthChanged; // Passes CURRENT health value
    public System.Action<float> OnHealthPercentChanged; // Passes percentage (0-1)
    public System.Action OnDeath;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0f;

    void Start()
    {
        // FIRST set current health
        currentHealth = maxHealth;

        Debug.Log($"{gameObject.name} Health Start: {currentHealth}/{maxHealth}");

        // THEN trigger events
        OnHealthChanged?.Invoke(currentHealth);
        OnHealthPercentChanged?.Invoke(currentHealth / maxHealth);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
        OnHealthPercentChanged?.Invoke(1f);
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
        OnHealthPercentChanged?.Invoke(currentHealth / maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        float oldHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // Trigger events with BOTH values
        OnHealthChanged?.Invoke(currentHealth);
        OnHealthPercentChanged?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        OnHealthChanged?.Invoke(currentHealth);
        OnHealthPercentChanged?.Invoke(currentHealth / maxHealth);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        OnDeath?.Invoke();
    }

    // For debugging
    [ContextMenu("Take 10 Damage")]
    private void DebugTakeDamage() => TakeDamage(10);

    [ContextMenu("Heal 20 Health")]
    private void DebugHeal() => Heal(20);
}