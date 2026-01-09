using UnityEngine;

public class AttackDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLines = true;
    [SerializeField] private Color hitColor = Color.green;
    [SerializeField] private Color missColor = Color.red;

    private PlayerDamageDealer damageDealer;

    void Start()
    {
        damageDealer = GetComponent<PlayerDamageDealer>();
    }

    void Update()
    {
        if (!showDebugLines || damageDealer == null) return;

        // Draw attack range
        float range = 0.5f; // Match your attackRange
        Vector2[] directions = {
            Vector2.right,
            Vector2.left,
            Vector2.up,
            Vector2.down,
            new Vector2(0.7f, 0.7f).normalized,
            new Vector2(-0.7f, 0.7f).normalized,
            new Vector2(0.7f, -0.7f).normalized,
            new Vector2(-0.7f, -0.7f).normalized
        };

        foreach (Vector2 dir in directions)
        {
            Vector2 endPos = (Vector2)transform.position + (dir * range);
            Debug.DrawLine(transform.position, endPos, Color.yellow);
            Debug.DrawRay(endPos, Vector2.up * 0.05f, Color.yellow);
        }

        // Draw enemy positions
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            Color lineColor = distance <= range ? hitColor : missColor;

            Debug.DrawLine(transform.position, enemy.transform.position, lineColor);

            // Draw enemy bounds
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyCollider != null)
            {
                Bounds bounds = enemyCollider.bounds;
                Debug.DrawLine(
                    new Vector2(bounds.min.x, bounds.min.y),
                    new Vector2(bounds.max.x, bounds.min.y),
                    lineColor
                );
                Debug.DrawLine(
                    new Vector2(bounds.max.x, bounds.min.y),
                    new Vector2(bounds.max.x, bounds.max.y),
                    lineColor
                );
                Debug.DrawLine(
                    new Vector2(bounds.max.x, bounds.max.y),
                    new Vector2(bounds.min.x, bounds.max.y),
                    lineColor
                );
                Debug.DrawLine(
                    new Vector2(bounds.min.x, bounds.max.y),
                    new Vector2(bounds.min.x, bounds.min.y),
                    lineColor
                );
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugLines) return;

        // Draw player attack zone
        Gizmos.color = new Color(0, 1, 0, 0.1f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Draw ideal hit positions
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.right * 0.5f, 0.1f);
        Gizmos.DrawWireSphere(transform.position + Vector3.left * 0.5f, 0.1f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.1f);
        Gizmos.DrawWireSphere(transform.position + Vector3.down * 0.5f, 0.1f);
    }
}