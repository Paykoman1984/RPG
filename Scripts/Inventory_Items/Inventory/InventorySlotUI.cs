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

            eventData.Use();

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Debug.Log($"Clicked on {currentItem.itemName} in slot {slotIndex}");

                // Single click to use/equip
                if (currentItem.itemType == ItemType.Consumable)
                {
                    Debug.Log($"Using consumable: {currentItem.itemName}");
                    inventory.UseItem(slotIndex);
                }
                else if (currentItem.itemType == ItemType.Weapon || currentItem.itemType == ItemType.Armor)
                {
                    Debug.Log($"Attempting to equip: {currentItem.itemName}");
                    inventory.UseItem(slotIndex); // This will try to equip
                }
                else
                {
                    Debug.Log($"Item type {currentItem.itemType} not handled");
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                Debug.Log($"Deleting {currentItem.itemName}");
                inventory.RemoveItem(slotIndex);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentItem != null && ui != null)
            {
                Vector2 screenPos = eventData.position;
                ui.ShowTooltip(currentItem, screenPos, slotIndex);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (ui != null)
            {
                ui.ClearHover();
            }
        }
    }
}
