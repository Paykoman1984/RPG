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

        private InventorySlotUI[] slots;
        private Canvas canvas;
        private RectTransform tooltipRect;

        void Start()
        {
            if (inventory == null)
                inventory = FindAnyObjectByType<InventorySystem>();

            // Get canvas
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                canvas = FindAnyObjectByType<Canvas>();

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
                inventory.onInventoryChanged += RefreshUI;

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
        }

        // Public method to check if inventory is open
        public bool IsInventoryOpen()
        {
            return inventoryPanel != null && inventoryPanel.activeSelf;
        }

        // SIMPLE tooltip method
        public void ShowTooltip(ItemData item, Vector2 screenPosition)
        {
            if (tooltipPanel == null || item == null || tooltipRect == null)
            {
                return;
            }

            // Build tooltip text
            string tooltip = $"<b>{item.itemName}</b>\n";
            tooltip += $"{item.description}\n";
            tooltip += $"Type: {item.itemType}";

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

            tooltipText.text = tooltip;

            // Force update layout to get proper size
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);

            // Position tooltip with top-left corner EXACTLY at mouse position
            tooltipRect.position = screenPosition;

            // Show tooltip
            tooltipPanel.SetActive(true);

            Debug.Log($"Tooltip shown at {screenPosition} (top-left corner at mouse)");
        }

        public void HideTooltip()
        {
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
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
                inventory.onInventoryChanged -= RefreshUI;
        }
    }
}