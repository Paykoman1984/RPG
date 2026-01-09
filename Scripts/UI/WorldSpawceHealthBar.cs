using UnityEngine;

public class WorldSpawceHealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);
    [SerializeField] private float width = 1f;
    [SerializeField] private float height = 0.15f;

    [Header("Colors")]
    [SerializeField] private Color fullColor = Color.green;
    [SerializeField] private Color lowColor = Color.red;
    [SerializeField] private Color backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);

    private Health healthComponent;
    private GameObject healthBar;
    private Transform healthFill;
    private Material fillMaterial;

    // Store original enemy scale to detect flips
    private Vector3 originalEnemyScale;
    private bool isFlipped = false;

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

        // Store original scale
        originalEnemyScale = transform.lossyScale;

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
        // Create parent - DONT parent it to the enemy that flips!
        healthBar = new GameObject("HealthBar");
        // Set position in world space, not as child for scaling
        healthBar.transform.position = transform.position + offset;
        healthBar.transform.rotation = Quaternion.identity;

        // Create background
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "Background";
        bg.transform.SetParent(healthBar.transform);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localRotation = Quaternion.Euler(90, 0, 0); // Face up
        bg.transform.localScale = new Vector3(width, height, 1f);
        Destroy(bg.GetComponent<Collider>());

        // Set background material
        Renderer bgRenderer = bg.GetComponent<Renderer>();
        Material bgMat = new Material(Shader.Find("Unlit/Color"));
        bgMat.color = backgroundColor;
        bgRenderer.material = bgMat;

        // Create fill
        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fill.name = "Fill";
        fill.transform.SetParent(healthBar.transform);
        fill.transform.localPosition = new Vector3(0, 0, -0.01f);
        fill.transform.localRotation = Quaternion.Euler(90, 0, 0);
        fill.transform.localScale = new Vector3(width, height * 0.8f, 1f);
        Destroy(fill.GetComponent<Collider>());

        // Set fill material
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
            Destroy(healthBar); // Destroy instead of just hiding
        }
    }

    void Update()
    {
        if (healthBar == null) return;

        // Update position to follow enemy (but don't parent it!)
        healthBar.transform.position = transform.position + offset;

        // Make health bar face camera (billboard effect)
        if (Camera.main != null)
        {
            healthBar.transform.rotation = Camera.main.transform.rotation;

            // Keep it horizontal
            Vector3 euler = healthBar.transform.rotation.eulerAngles;
            healthBar.transform.rotation = Quaternion.Euler(0, euler.y, 0);
        }

        // Check if enemy flipped
        CheckEnemyFlip();
    }

    void CheckEnemyFlip()
    {
        // Check if enemy scale changed (flipped)
        float currentScaleX = transform.lossyScale.x;
        bool nowFlipped = Mathf.Sign(currentScaleX) != Mathf.Sign(originalEnemyScale.x);

        if (nowFlipped != isFlipped)
        {
            isFlipped = nowFlipped;
            Debug.Log($"Enemy flipped: {isFlipped}");

            // If health bar is parented, unparent it to prevent flipping
            if (healthBar.transform.parent == transform)
            {
                healthBar.transform.SetParent(null, true);
            }
        }
    }

    void OnDestroy()
    {
        if (healthBar != null)
        {
            Destroy(healthBar);
        }

        if (healthComponent != null)
        {
            healthComponent.OnHealthChanged -= UpdateHealth;
            healthComponent.OnDeath -= OnDeath;
        }
    }
}