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
        private bool isHovering = false;

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
                else if (item.isStackable && item.currentStack == 1)
                {
                    stackText.gameObject.SetActive(false);
                }
                else
                {
                    stackText.gameObject.SetActive(false);
                }

                // CRITICAL: Update tooltip if we're hovering over this slot
                if (isHovering && ui != null)
                {
                    // Force update the tooltip with current data
                    ui.ForceUpdateTooltip(item, slotIndex);
                }
            }
            else
            {
                iconImage.sprite = null;
                iconImage.color = new Color(1, 1, 1, 0.2f);
                stackText.gameObject.SetActive(false);

                // If item is null and we're hovering, clear the hover
                if (isHovering)
                {
                    isHovering = false;
                    if (ui != null)
                    {
                        ui.ClearHover();
                    }
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (currentItem == null) return;

            eventData.Use();

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // LEFT CLICK - INSTANT RESPONSE
                if (currentItem.itemType == ItemType.Consumable)
                {
                    // Use consumable immediately
                    Debug.Log($"Using consumable: {currentItem.itemName}");
                    UseConsumable(currentItem);
                    inventory.ReduceItemStack(slotIndex);

                    // Immediately update tooltip after using item
                    if (isHovering && ui != null)
                    {
                        // Get updated item after reduction
                        ItemData updatedItem = inventory.GetItem(slotIndex);
                        if (updatedItem != null)
                        {
                            ui.ForceUpdateTooltip(updatedItem, slotIndex);
                        }
                        else
                        {
                            // Item was completely removed
                            ui.ClearHover();
                            isHovering = false;
                        }
                    }
                }
                else if (currentItem.itemType == ItemType.Weapon || currentItem.itemType == ItemType.Armor)
                {
                    // EQUIP immediately on single click
                    Debug.Log($"Equipping {currentItem.itemName}");
                    EquipItem(currentItem);
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // RIGHT CLICK - INSTANT DELETE
                Debug.Log($"Deleting {currentItem.itemName}");
                inventory.RemoveItem(slotIndex);

                // Immediately clear tooltip since item is gone
                if (isHovering && ui != null)
                {
                    ui.ClearHover();
                    isHovering = false;
                }
            }
        }

        private void EquipItem(ItemData item)
        {
            if (item == null) return;

            Debug.Log($"=== EQUIPPING {item.itemName} ===");

            // TODO: Implement your actual equipment system
            inventory.RemoveItem(slotIndex);

            Debug.Log($"Successfully equipped {item.itemName}");
        }

        private void UseConsumable(ItemData item)
        {
            if (item == null) return;

            Debug.Log($"Used {item.itemName} - Heals {item.healAmount} HP");

            PlayerStats playerStats = FindAnyObjectByType<PlayerStats>();
            if (playerStats != null && item.healAmount > 0)
            {
                playerStats.Heal(item.healAmount);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;

            if (currentItem != null && ui != null)
            {
                Vector2 screenPos = eventData.position;
                ui.ShowTooltip(currentItem, screenPos, slotIndex);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;

            if (ui != null)
            {
                ui.ClearHover();
            }
        }
    }
}