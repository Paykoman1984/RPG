using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace PoEClone2D
{
    public class ItemPickup : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public ItemData itemData;

        private SpriteRenderer sr;
        private bool isPickedUp = false;
        private float spawnTime;
        private Vector3 originalScale;
        private Vector3 originalPosition;
        private bool isHovered = false;

        void Start()
        {
            spawnTime = Time.time;
            sr = GetComponent<SpriteRenderer>();
            originalScale = transform.localScale;
            originalPosition = transform.position;

            // Make sure we have a SpriteRenderer
            if (sr == null)
            {
                sr = gameObject.AddComponent<SpriteRenderer>();
            }

            // Set visual
            if (itemData != null && itemData.icon != null)
            {
                sr.sprite = itemData.icon;
            }
            sr.color = GetRarityColor(itemData?.rarity ?? ItemRarity.Normal);
            sr.sortingOrder = 10;

            // Ensure we have a collider for clicking
            if (GetComponent<BoxCollider2D>() == null)
            {
                BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1f, 1f);
            }

            Debug.Log($"Item dropped: {itemData?.itemName}");
        }

        void Update()
        {
            if (isPickedUp) return;

            float time = Time.time - spawnTime;

            // Gentle up/down bobbing animation
            float yOffset = Mathf.Sin(time * 2f) * 0.1f;
            transform.position = originalPosition + new Vector3(0, yOffset, 0);

            // Hover scaling
            float targetScale = isHovered ? 1.2f : 1f;
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale * targetScale, Time.deltaTime * 10f);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && !isPickedUp)
            {
                eventData.Use(); // Prevent attack animation
                PickupItem();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            if (sr != null)
            {
                sr.color = Color.Lerp(GetRarityColor(itemData?.rarity ?? ItemRarity.Normal), Color.white, 0.3f);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            if (sr != null && itemData != null)
            {
                sr.color = GetRarityColor(itemData.rarity);
            }
        }

        private Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Normal => Color.white,
                ItemRarity.Magic => new Color(0.3f, 0.6f, 1f), // Blue
                ItemRarity.Rare => new Color(1f, 0.8f, 0f), // Yellow/Gold
                ItemRarity.Unique => new Color(0.8f, 0.2f, 0.8f), // Purple
                ItemRarity.Currency => new Color(0.2f, 0.8f, 0.2f), // Green
                _ => Color.white
            };
        }

        private void PickupItem()
        {
            if (isPickedUp || itemData == null) return;

            // Add proximity check - player must be near the item to pick it up
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float pickupRange = 2f; // Adjust this value as needed
                float distance = Vector3.Distance(transform.position, player.transform.position);

                if (distance > pickupRange)
                {
                    Debug.Log($"<color=yellow>Too far to pickup {itemData.itemName} ({distance:F1} units away)</color>");
                    return;
                }
            }

            // FIXED: Using FindAnyObjectByType instead of FindObjectOfType
            InventorySystem inventory = FindAnyObjectByType<InventorySystem>();
            if (inventory != null)
            {
                bool success = inventory.AddItem(itemData);
                if (success)
                {
                    isPickedUp = true;
                    Debug.Log($"<color=green>✓ Picked up:</color> {itemData.itemName}");

                    // Visual feedback before destruction
                    if (sr != null)
                    {
                        sr.color = Color.white;
                    }

                    Destroy(gameObject, 0.1f);
                }
                else
                {
                    Debug.Log("<color=orange>Inventory full!</color>");
                    StartCoroutine(ShakeEffect());
                }
            }
        }

        private IEnumerator ShakeEffect()
        {
            Vector3 originalPos = transform.position;
            float duration = 0.3f;
            float magnitude = 0.1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = originalPos.x + Random.Range(-magnitude, magnitude);
                float y = originalPos.y + Random.Range(-magnitude, magnitude);
                transform.position = new Vector3(x, y, originalPos.z);
                yield return null;
            }

            transform.position = originalPos;
        }
    }
}