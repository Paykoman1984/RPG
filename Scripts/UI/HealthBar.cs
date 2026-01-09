using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health healthComponent;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Text healthText;

    [Header("Colors")]
    [SerializeField] private Color fullColor = Color.green;
    [SerializeField] private Color emptyColor = Color.red;

    private void Start()
    {
        Debug.Log("HealthBar Start called");

        // Auto-find Health component on Player
        if (healthComponent == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                healthComponent = player.GetComponent<Health>();
                Debug.Log($"Found Health component on player: {healthComponent != null}");
            }
        }

        // Get Slider if not assigned
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
            Debug.Log($"Got Slider component: {healthSlider != null}");
        }

        if (healthComponent != null && healthSlider != null)
        {
            Debug.Log($"Setting up HealthBar for {healthComponent.gameObject.name}");
            Debug.Log($"Initial Health: {healthComponent.CurrentHealth}/{healthComponent.MaxHealth}");

            // Setup slider range
            healthSlider.minValue = 0;
            healthSlider.maxValue = healthComponent.MaxHealth;
            healthSlider.value = healthComponent.CurrentHealth;

            // Subscribe to events
            healthComponent.OnHealthChanged += UpdateHealthBar;
            healthComponent.OnDeath += OnPlayerDeath;

            // Force initial update
            UpdateHealthBar(healthComponent.CurrentHealth);

            Debug.Log($"PlayerHealthBar initialized: {healthComponent.CurrentHealth}/{healthComponent.MaxHealth}");
        }
        else
        {
            if (healthComponent == null) Debug.LogError("HealthBar: Missing Health component!");
            if (healthSlider == null) Debug.LogError("HealthBar: Missing Slider component!");
        }
    }

    private void UpdateHealthBar(float currentHealth)
    {
        if (healthSlider != null)
        {
            Debug.Log($"HealthBar Update: {currentHealth}/{healthComponent.MaxHealth}");

            healthSlider.value = currentHealth;

            // Update color
            if (fillImage != null)
            {
                float healthPercent = currentHealth / healthComponent.MaxHealth;
                fillImage.color = Color.Lerp(emptyColor, fullColor, healthPercent);
            }

            // Update text
            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{healthComponent.MaxHealth}";
            }
        }
        else
        {
            Debug.LogWarning("HealthBar: Slider is null!");
        }
    }

    private void OnPlayerDeath()
    {
        Debug.Log("HealthBar: Player died!");

        // Optional: Change color to indicate death
        if (fillImage != null)
        {
            fillImage.color = Color.gray;
        }
    }

    private void OnDestroy()
    {
        if (healthComponent != null)
        {
            healthComponent.OnHealthChanged -= UpdateHealthBar;
            healthComponent.OnDeath -= OnPlayerDeath;
        }
    }
}