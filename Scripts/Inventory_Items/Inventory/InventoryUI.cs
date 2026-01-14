using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PoEClone2D
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventorySystem inventory;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject slotPrefab;

        [Header("Tooltip")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private Vector2 tooltipOffset = new Vector2(10f, 0f); // Small right offset

        private InventorySlotUI[] slots;
        private RectTransform tooltipRect;

        // Track current hovered item
        private ItemData currentlyHoveredItem;
        private int currentlyHoveredSlot = -1;

        void Start()
        {
            if (inventory == null)
                inventory = FindAnyObjectByType<InventorySystem>();

            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);

            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
                tooltipRect = tooltipPanel.GetComponent<RectTransform>();

                // Set pivot to top-left so position matches top-left corner
                if (tooltipRect != null)
                {
                    tooltipRect.pivot = new Vector2(0f, 1f); // Top-left pivot
                    Debug.Log("Tooltip pivot set to top-left (0,1)");
                }
            }

            InitializeSlots();

            if (inventory != null)
                inventory.onInventoryChanged += OnInventoryChanged;

            Debug.Log("InventoryUI ready. Press I to open, T/P for items");
        }

        void InitializeSlots()
        {
            if (slotContainer == null || slotPrefab == null)
            {
                Debug.LogError("Slot container or prefab not set!");
                return;
            }

            // Clear old slots
            foreach (Transform child in slotContainer)
                Destroy(child.gameObject);

            // Create slots
            int slotCount = inventory.GetInventorySize();
            slots = new InventorySlotUI[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotContainer);
                slotObj.name = $"Slot_{i}";

                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    slotUI.Initialize(i, inventory, this);
                    slots[i] = slotUI;
                }
            }

            Debug.Log($"Created {slotCount} inventory slots");
        }

        public void RefreshUI()
        {
            if (slots == null || inventory == null) return;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null)
                {
                    ItemData item = inventory.GetItem(i);
                    slots[i].UpdateSlot(item);
                }
            }

            // Update tooltip if we're hovering over the same slot
            UpdateTooltipIfHovering();
        }

        private void OnInventoryChanged()
        {
            RefreshUI();
        }

        private void UpdateTooltipIfHovering()
        {
            if (currentlyHoveredSlot >= 0 && currentlyHoveredSlot < slots.Length)
            {
                ItemData currentItem = inventory.GetItem(currentlyHoveredSlot);

                if (currentItem == null)
                {
                    // Item was removed, hide tooltip
                    HideTooltip();
                    currentlyHoveredItem = null;
                    currentlyHoveredSlot = -1;
                }
                else if (currentlyHoveredItem != currentItem ||
                        (currentItem.isStackable && currentlyHoveredItem?.currentStack != currentItem.currentStack))
                {
                    // Item changed or stack count changed, update tooltip
                    currentlyHoveredItem = currentItem;
                    UpdateTooltip(currentItem);
                }
            }
            else if (tooltipPanel != null && tooltipPanel.activeSelf)
            {
                // We have a tooltip showing but no hovered slot, hide it
                HideTooltip();
            }
        }

        private void UpdateTooltip(ItemData item)
        {
            if (tooltipPanel == null || item == null || tooltipRect == null || !tooltipPanel.activeSelf)
                return;

            // Build updated tooltip text
            string tooltip = BuildTooltipText(item);
            tooltipText.text = tooltip;

            // Force layout update
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);

            Debug.Log($"Tooltip updated for {item.itemName} (Stack: {item.currentStack})");
        }

        private string BuildTooltipText(ItemData item)
        {
            string tooltip = $"<b>{item.itemName}</b>\n";
            tooltip += $"{item.description}\n";
            tooltip += $"Type: {item.itemType}";

            // Show stack info if stackable
            if (item.isStackable)
            {
                tooltip += $"\nStack: {item.currentStack}/{item.maxStack}";
            }

            if (item.itemType == ItemType.Weapon)
            {
                tooltip += $"\nDamage: {item.minDamage}-{item.maxDamage}";
                tooltip += $"\nEquip Slot: {item.equipSlot}";
            }
            else if (item.itemType == ItemType.Consumable)
            {
                tooltip += $"\nHeals: {item.healAmount} HP";
            }
            else if (item.itemType == ItemType.Armor)
            {
                tooltip += $"\nEquip Slot: {item.equipSlot}";
            }

            return tooltip;
        }

        // Public method to check if inventory is open
        public bool IsInventoryOpen()
        {
            return inventoryPanel != null && inventoryPanel.activeSelf;
        }

        // Called by InventorySlotUI when mouse enters
        public void ShowTooltip(ItemData item, Vector2 screenPosition, int slotIndex)
        {
            if (tooltipPanel == null || item == null || tooltipRect == null)
            {
                return;
            }

            // Store hover information
            currentlyHoveredItem = item;
            currentlyHoveredSlot = slotIndex;

            // Build tooltip text
            string tooltip = BuildTooltipText(item);
            tooltipText.text = tooltip;

            // Force update layout to get proper size
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);

            // SIMPLE POSITIONING: Set tooltip position directly to screen position
            // For Screen Space - Overlay canvas (most common for UI)
            tooltipPanel.transform.position = screenPosition + tooltipOffset;

            // Show tooltip
            tooltipPanel.SetActive(true);

            Debug.Log($"Tooltip shown for {item.itemName} at slot {slotIndex}");
        }

        // Add this method to force tooltip update
        // Add this method to InventoryUI.cs
        public void ForceUpdateTooltip(ItemData item, int slotIndex)
        {
            if (tooltipPanel == null || item == null || !tooltipPanel.activeSelf || currentlyHoveredSlot != slotIndex)
                return;

            // Update the stored item reference
            currentlyHoveredItem = item;

            // Build updated tooltip text
            string tooltip = BuildTooltipText(item);
            tooltipText.text = tooltip;

            // Force layout update
            if (tooltipRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
            }

            Debug.Log($"Tooltip force-updated for {item.itemName} (Stack: {item.currentStack})");
        }

        // Called by InventorySlotUI when mouse exits
        public void ClearHover()
        {
            currentlyHoveredItem = null;
            currentlyHoveredSlot = -1;
            HideTooltip();
        }

        public void HideTooltip()
        {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
                Debug.Log("Tooltip hidden");
            }
        }

        public void ToggleInventory()
        {
            if (inventoryPanel != null)
            {
                bool newState = !inventoryPanel.activeSelf;
                inventoryPanel.SetActive(newState);

                // Notify GameStateManager about inventory state change
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.SetInventoryOpen(newState);
                }

                if (newState)
                {
                    RefreshUI();
                    Debug.Log("Inventory opened");
                }
                else
                {
                    HideTooltip();
                    ClearHover();
                    Debug.Log("Inventory closed");
                }
            }
        }

        void Update()
        {
            // Hotkeys - SIMPLE AND RELIABLE
            if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleInventory();
            }

            if (Input.GetKeyDown(KeyCode.T) && inventory != null)
            {
                Debug.Log("Adding test sword...");
                inventory.AddTestSword();
            }

            if (Input.GetKeyDown(KeyCode.P) && inventory != null)
            {
                Debug.Log("Adding test potion...");
                inventory.AddTestPotion();
            }
        }

        void OnDestroy()
        {
            if (inventory != null)
                inventory.onInventoryChanged -= OnInventoryChanged;
        }
    }
}