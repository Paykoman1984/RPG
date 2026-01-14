using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace PoEClone2D
{
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI stackText;

        private int slotIndex;
        private InventorySystem inventory;
        private InventoryUI ui;
        private ItemData currentItem;

        // Improved double click tracking
        private float lastClickTime = 0f;
        private const float DOUBLE_CLICK_THRESHOLD = 0.4f; // 400ms for double click
        private bool isDoubleClickPending = false;

        public void Initialize(int index, InventorySystem inv, InventoryUI uiManager)
        {
            slotIndex = index;
            inventory = inv;
            ui = uiManager;
            UpdateSlot(null);
        }

        public void UpdateSlot(ItemData item)
        {
            currentItem = item;

            if (item != null)
            {
                if (item.icon != null)
                    iconImage.sprite = item.icon;
                iconImage.color = Color.white;

                if (item.isStackable && item.currentStack > 1)
                {
                    stackText.text = item.currentStack.ToString();
                    stackText.gameObject.SetActive(true);
                }
                else
                {
                    stackText.gameObject.SetActive(false);
                }
            }
            else
            {
                iconImage.sprite = null;
                iconImage.color = new Color(1, 1, 1, 0.2f);
                stackText.gameObject.SetActive(false);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (currentItem == null) return;

            // Mark event as used to prevent propagation to game world
            eventData.Use();

            Debug.Log($"Clicked {currentItem.itemName} (Button: {eventData.button})");

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                float currentTime = Time.time;
                float timeSinceLastClick = currentTime - lastClickTime;

                Debug.Log($"Time since last click: {timeSinceLastClick:F3}s");

                // Check for double click
                if (timeSinceLastClick < DOUBLE_CLICK_THRESHOLD)
                {
                    // DOUBLE CLICK DETECTED!
                    Debug.Log($"DOUBLE-CLICK CONFIRMED: Equipping {currentItem.itemName}");

                    // Clear the timer
                    lastClickTime = 0f;
                    isDoubleClickPending = false;

                    // Equip the item
                    EquipItem(currentItem);
                    return;
                }

                // First click - start the double click timer
                lastClickTime = currentTime;
                isDoubleClickPending = true;

                // Start coroutine to handle single click if double click doesn't happen
                StartCoroutine(HandleSingleClickAfterDelay());
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // Right click: Remove item
                Debug.Log($"RIGHT-CLICK: Removing {currentItem.itemName}");
                inventory.RemoveItem(slotIndex);

                // Clear double click state
                lastClickTime = 0f;
                isDoubleClickPending = false;
            }
        }

        private System.Collections.IEnumerator HandleSingleClickAfterDelay()
        {
            // Wait to see if this becomes a double click
            yield return new WaitForSecondsRealtime(DOUBLE_CLICK_THRESHOLD);

            // If we're still waiting for a double click, process as single click
            if (isDoubleClickPending)
            {
                isDoubleClickPending = false;

                // Left click: Use if consumable
                if (currentItem.itemType == ItemType.Consumable)
                {
                    Debug.Log($"SINGLE CLICK: Using consumable: {currentItem.itemName}");
                    UseConsumable(currentItem);
                    inventory.RemoveItem(slotIndex);
                }
                // For weapons/armor, just show info on single click
                else if (currentItem.itemType == ItemType.Weapon || currentItem.itemType == ItemType.Armor)
                {
                    Debug.Log($"SINGLE CLICK: Selected {currentItem.itemName} - Double click to equip");
                }
            }
        }

        private void EquipItem(ItemData item)
        {
            if (item == null) return;

            // Check if item is equippable
            if (item.itemType == ItemType.Weapon || item.itemType == ItemType.Armor)
            {
                Debug.Log($"=== EQUIPPING {item.itemName} ===");
                Debug.Log($"Type: {item.itemType}");
                Debug.Log($"Equip Slot: {item.equipSlot}");

                if (item.itemType == ItemType.Weapon)
                {
                    Debug.Log($"Damage: {item.minDamage}-{item.maxDamage}");
                }

                // TODO: Implement your actual equipment system here
                // For now, just remove from inventory as proof it works
                inventory.RemoveItem(slotIndex);

                Debug.Log($"Successfully equipped {item.itemName} to {item.equipSlot}");
            }
            else
            {
                Debug.LogWarning($"Cannot equip {item.itemName} - Item type {item.itemType} is not equippable");
            }
        }

        private void UseConsumable(ItemData item)
        {
            if (item == null) return;

            // TODO: Implement consumable effects
            Debug.Log($"Used {item.itemName} - Heals {item.healAmount} HP");

            // Example: Heal player
            PlayerStats playerStats = FindAnyObjectByType<PlayerStats>();
            if (playerStats != null && item.healAmount > 0)
            {
                playerStats.Heal(item.healAmount);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentItem != null && ui != null)
            {
                // Get mouse position in screen coordinates
                Vector2 screenPos = eventData.position;

                // Show tooltip with top-left corner at mouse position
                ui.ShowTooltip(currentItem, screenPos);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (ui != null)
                ui.HideTooltip();
        }
    }
}