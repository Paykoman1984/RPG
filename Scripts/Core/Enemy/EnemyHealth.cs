using UnityEngine;

public class SimpleEnemyHealth : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);
    [SerializeField] private float width = 1f;
    [SerializeField] private float height = 0.1f;

    [Header("References")]
    [SerializeField] private Health healthComponent;

    // Visual components
    private GameObject healthBarObject;
    private Transform backgroundTransform;
    private Transform fillTransform;
    private Material fillMaterial;

    void Start()
    {
        // Get Health component
        if (healthComponent == null)
        {
            healthComponent = GetComponent<Health>();
        }

        if (healthComponent == null)
        {
            Debug.LogError($"No Health component found on {gameObject.name}");
            return;
        }

        Debug.Log($"Setting up health bar for {gameObject.name}, Health: {healthComponent.CurrentHealth}/{healthComponent.MaxHealth}");

        CreateHealthBar();

        // Subscribe to events
        healthComponent.OnHealthChanged += UpdateHealthDisplay;
        healthComponent.OnDeath += OnEnemyDeath;

        // Initial update
        UpdateHealthDisplay(healthComponent.CurrentHealth);
    }

    void CreateHealthBar()
    {
        // Create parent object
        healthBarObject = new GameObject("HealthBar");
        healthBarObject.transform.SetParent(transform);
        healthBarObject.transform.localPosition = offset;
        healthBarObject.transform.localRotation = Quaternion.identity;

        // Create background (dark gray)
        GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);
        background.name = "HealthBackground";
        background.transform.SetParent(healthBarObject.transform);
        background.transform.localPosition = Vector3.zero;
        background.transform.localScale = new Vector3(width, height, 0.05f);

        // Remove collider
        Destroy(background.GetComponent<Collider>());

        // Set material
        Renderer bgRenderer = background.GetComponent<Renderer>();
        bgRenderer.material.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

        backgroundTransform = background.transform;

        // Create fill (red/green health)
        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fill.name = "HealthFill";
        fill.transform.SetParent(healthBarObject.transform);
        fill.transform.localPosition = new Vector3(0, 0, -0.02f); // In front of background
        fill.transform.localScale = new Vector3(width, height, 0.1f);

        // Remove collider
        Destroy(fill.GetComponent<Collider>());

        // Set material
        Renderer fillRenderer = fill.GetComponent<Renderer>();
        fillMaterial = new Material(Shader.Find("Standard"));
        fillMaterial.color = Color.green;
        fillRenderer.material = fillMaterial;

        fillTransform = fill.transform;

        // Initially hide if full health
        healthBarObject.SetActive(false);
    }

    void UpdateHealthDisplay(float currentHealth)
    {
        if (fillTransform == null || healthComponent == null) return;

        float healthPercent = currentHealth / healthComponent.MaxHealth;

        Debug.Log($"{gameObject.name} health: {currentHealth}/{healthComponent.MaxHealth} ({healthPercent:P0})");

        // Update fill width
        Vector3 scale = fillTransform.localScale;
        scale.x = width * healthPercent;
        fillTransform.localScale = scale;

        // Update fill position (so it shrinks from center)
        Vector3 position = fillTransform.localPosition;
        position.x = -(width - scale.x) / 2f;
        fillTransform.localPosition = position;

        // Update color
        if (fillMaterial != null)
        {
            if (healthPercent > 0.5f)
                fillMaterial.color = Color.green;
            else if (healthPercent > 0.25f)
                fillMaterial.color = Color.yellow;
            else
                fillMaterial.color = Color.red;
        }

        // Show only if damaged
        healthBarObject.SetActive(healthPercent < 1f);
    }

    void OnEnemyDeath()
    {
        Debug.Log($"{gameObject.name} died - hiding health bar");
        if (healthBarObject != null)
        {
            healthBarObject.SetActive(false);
        }
    }

    void Update()
    {
        // Make health bar face camera
        if (healthBarObject != null && Camera.main != null)
        {
            healthBarObject.transform.rotation = Camera.main.transform.rotation;
        }
    }

    void OnDestroy()
    {
        if (healthComponent != null)
        {
            healthComponent.OnHealthChanged -= UpdateHealthDisplay;
            healthComponent.OnDeath -= OnEnemyDeath;
        }
    }
}