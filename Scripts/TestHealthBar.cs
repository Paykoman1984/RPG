using UnityEngine;

public class TestHealthBar : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);
    [SerializeField] private float width = 1.5f;
    [SerializeField] private float height = 0.15f;

    [Header("Colors")]
    [SerializeField] private Color fullColor = Color.green;
    [SerializeField] private Color lowColor = Color.red;
    [SerializeField] private Color backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);

    [Header("Fill Direction")]
    [SerializeField] private bool fillFromRight = true; // Set to TRUE for right-to-left fill

    private Health healthComponent;
    private GameObject healthBar;
    private Transform healthFill;
    private Material fillMaterial;
    private Material bgMaterial;

    void Start()
    {
        Debug.Log($"=== SimpleEnemyHealthBar Start for {gameObject.name} ===");

        healthComponent = GetComponent<Health>();
        if (healthComponent == null)
        {
            healthComponent = GetComponentInParent<Health>();
            Debug.Log($"Health component found in parent: {healthComponent != null}");
        }

        if (healthComponent == null)
        {
            Debug.LogError($"No Health component found on {gameObject.name}");
            return;
        }

        Debug.Log($"Health component: {healthComponent.name}, Health: {healthComponent.CurrentHealth}/{healthComponent.MaxHealth}");

        CreateHealthBar();

        // Subscribe to events
        healthComponent.OnHealthChanged += UpdateHealth;
        healthComponent.OnDeath += OnDeath;

        // Initial update
        UpdateHealth(healthComponent.CurrentHealth);

        Debug.Log($"Health bar created for {gameObject.name}");
    }

    void CreateHealthBar()
    {
        Debug.Log($"Creating health bar with fillFromRight: {fillFromRight}");

        // Create parent
        healthBar = new GameObject("HealthBar");
        healthBar.transform.SetParent(transform);
        healthBar.transform.localPosition = offset;
        healthBar.transform.localRotation = Quaternion.identity;

        // Create background
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bg.name = "Background";
        bg.transform.SetParent(healthBar.transform);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(width, height, 0.05f);
        Destroy(bg.GetComponent<Collider>());

        // Create background material
        Renderer bgRenderer = bg.GetComponent<Renderer>();
        bgMaterial = new Material(Shader.Find("Standard"));
        bgMaterial.color = backgroundColor;
        bgRenderer.material = bgMaterial;

        Debug.Log($"Background material created with color: {backgroundColor}");

        // Create fill
        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fill.name = "Fill";
        fill.transform.SetParent(healthBar.transform);

        // Position based on fill direction
        if (fillFromRight)
        {
            // Start from right side
            fill.transform.localPosition = new Vector3(width / 2f, 0, -0.01f);
            fill.transform.localScale = new Vector3(width, height * 0.8f, 0.1f);
        }
        else
        {
            // Start from left side (default)
            fill.transform.localPosition = new Vector3(-width / 2f, 0, -0.01f);
            fill.transform.localScale = new Vector3(width, height * 0.8f, 0.1f);
        }

        Destroy(fill.GetComponent<Collider>());

        // Create fill material
        Renderer fillRenderer = fill.GetComponent<Renderer>();
        fillMaterial = new Material(Shader.Find("Standard"));
        fillMaterial.color = fullColor;
        fillRenderer.material = fillMaterial;

        Debug.Log($"Fill material created with color: {fullColor}");
        Debug.Log($"Fill position: {fill.transform.localPosition}, Scale: {fill.transform.localScale}");

        healthFill = fill.transform;
        healthBar.SetActive(true);
    }

    void UpdateHealth(float currentHealth)
    {
        if (healthFill == null || healthComponent == null) return;

        float percent = healthComponent.MaxHealth > 0 ? currentHealth / healthComponent.MaxHealth : 0;

        Debug.Log($"{gameObject.name} health: {currentHealth}/{healthComponent.MaxHealth} ({percent:P0})");

        // Update fill width
        Vector3 scale = healthFill.localScale;
        scale.x = width * percent;
        healthFill.localScale = scale;

        // Update position based on fill direction
        Vector3 pos = healthFill.localPosition;

        if (fillFromRight)
        {
            // Fill from right to left
            pos.x = width / 2f - scale.x / 2f;
        }
        else
        {
            // Fill from left to right
            pos.x = -width / 2f + scale.x / 2f;
        }

        healthFill.localPosition = pos;

        Debug.Log($"Fill updated - Scale: {scale.x}, Position: {pos.x}");

        // Update color
        if (fillMaterial != null)
        {
            Color newColor = Color.Lerp(lowColor, fullColor, percent);
            fillMaterial.color = newColor;
            Debug.Log($"Fill color updated: {newColor}");
        }

        // FIX: ALWAYS show health bar (or show if alive)
        bool shouldShow = percent > 0f && healthComponent.CurrentHealth > 0;
        healthBar.SetActive(shouldShow);
        Debug.Log($"Health bar active: {shouldShow} (health: {currentHealth} > 0)");
    }

    void OnDeath()
    {
        Debug.Log($"Health bar destroyed for {gameObject.name}");
        if (healthBar != null)
        {
            Destroy(healthBar);
        }
    }

    void Update()
    {
        // Simple billboard effect - face camera but stay upright
        if (healthBar != null && Camera.main != null)
        {
            // Only rotate around Y axis to face camera
            Vector3 lookDirection = Camera.main.transform.position - healthBar.transform.position;
            lookDirection.y = 0; // Keep horizontal

            if (lookDirection != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(lookDirection);
                // Keep the X and Z rotation fixed
                healthBar.transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
            }
        }
    }

    void OnDestroy()
    {
        if (healthComponent != null)
        {
            healthComponent.OnHealthChanged -= UpdateHealth;
            healthComponent.OnDeath -= OnDeath;
        }
    }
}