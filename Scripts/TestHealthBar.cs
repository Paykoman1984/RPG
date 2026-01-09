using UnityEngine;

public class SimpleEnemyHealthBar : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);
    [SerializeField] private float width = 1f;
    [SerializeField] private float height = 0.2f;

    [Header("Colors")]
    [SerializeField] private Color fullColor = Color.green;
    [SerializeField] private Color lowColor = Color.red;
    [SerializeField] private Color backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);

    private Health healthComponent;
    private GameObject healthBar;
    private Transform healthFill;
    private Material fillMaterial;

    void Start()
    {
        healthComponent = GetComponent<Health>();
        if (healthComponent == null)
        {
            healthComponent = GetComponentInParent<Health>();
        }

        if (healthComponent == null)
        {
            Debug.LogError($"No Health component found on {gameObject.name}");
            return;
        }

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

        // Set background material - USE UNLIT COLOR SHADER
        Renderer bgRenderer = bg.GetComponent<Renderer>();
        Material bgMat = new Material(Shader.Find("Unlit/Color"));
        bgMat.color = backgroundColor;
        bgRenderer.material = bgMat;

        // Create fill
        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fill.name = "Fill";
        fill.transform.SetParent(healthBar.transform);
        fill.transform.localPosition = new Vector3(0, 0, -0.01f); // In front of background
        fill.transform.localScale = new Vector3(width, height * 0.8f, 0.1f);
        Destroy(fill.GetComponent<Collider>());

        // Set fill material - USE UNLIT COLOR SHADER
        Renderer fillRenderer = fill.GetComponent<Renderer>();
        fillMaterial = new Material(Shader.Find("Unlit/Color"));
        fillMaterial.color = fullColor;
        fillRenderer.material = fillMaterial;

        healthFill = fill.transform;

        // Make sure it's visible
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

        // Update position (center it as width changes)
        Vector3 pos = healthFill.localPosition;
        pos.x = -(width - scale.x) / 2f;
        healthFill.localPosition = pos;

        // Update color
        if (fillMaterial != null)
        {
            fillMaterial.color = Color.Lerp(lowColor, fullColor, percent);
        }

        // Show only if damaged
        healthBar.SetActive(percent < 1f);
    }

    void OnDeath()
    {
        if (healthBar != null)
        {
            healthBar.SetActive(false);
        }
    }

    void Update()
    {
        // Make health bar face camera (billboard effect)
        if (healthBar != null && Camera.main != null)
        {
            healthBar.transform.rotation = Camera.main.transform.rotation;
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