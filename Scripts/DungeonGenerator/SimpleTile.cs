using UnityEngine;

namespace PoEClone2D.Testing
{
    public class SimpleTile : MonoBehaviour
    {
        [Header("Tile Settings")]
        [SerializeField] private bool isWalkable = true;
        [SerializeField] private bool isObstacle = false;
        [SerializeField] private float moveCost = 1f;

        [Header("Visuals")]
        [SerializeField] private Sprite[] tileVariants;
        [SerializeField] private Color highlightColor = Color.yellow;

        private SpriteRenderer spriteRenderer;
        private Color originalColor;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            // Random variant if available
            if (tileVariants != null && tileVariants.Length > 0 && spriteRenderer != null)
            {
                spriteRenderer.sprite = tileVariants[Random.Range(0, tileVariants.Length)];
            }
        }

        public bool IsWalkable() => isWalkable;
        public bool IsObstacle() => isObstacle;
        public float GetMoveCost() => moveCost;

        public void Highlight()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = highlightColor;
            }
        }

        public void Unhighlight()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isObstacle)
            {
                Debug.Log($"Collision with obstacle tile at {transform.position}");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log($"Player entered tile at {transform.position}");
            }
        }
    }
}