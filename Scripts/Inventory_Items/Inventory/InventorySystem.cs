using System;
using UnityEngine;

namespace PoEClone2D
{
    public class InventorySystem : MonoBehaviour
    {
        [SerializeField] private int inventorySize = 30;
        private ItemData[] items;

        // References
        private PoEClone2D.Player.PlayerHealth playerHealth;
        [SerializeField] private EquipmentSystem equipmentSystem;

        public Action onInventoryChanged;

        void Start()
        {
            items = new ItemData[inventorySize];

            // Find player health
            playerHealth = FindAnyObjectByType<PoEClone2D.Player.PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogWarning("No PlayerHealth found in scene. Potions won't work!");
            }

            // Find equipment system
            if (equipmentSystem == null)
            {
                equipmentSystem = FindAnyObjectByType<EquipmentSystem>();
                if (equipmentSystem == null)
                {
                    Debug.LogWarning("No EquipmentSystem found. Equipment won't work!");
                }
            }

            Debug.Log($"Inventory: {inventorySize} slots ready");
        }

        public bool AddItem(ItemData item)
        {
            if (item == null) return false;

            // Try to stack
            if (item.isStackable)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] != null && items[i].itemName == item.itemName &&
                        items[i].currentStack < items[i].maxStack)
                    {
                        items[i].currentStack++;
                        onInventoryChanged?.Invoke();
                        Debug.Log($"Stacked {item.itemName} (now {items[i].currentStack}/{items[i].maxStack})");
                        return true;
                    }
                }
            }

            // Find empty slot
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    items[i] = Instantiate(item);
                    items[i].currentStack = 1;
                    onInventoryChanged?.Invoke();
                    Debug.Log($"Added {item.itemName} to slot {i}");
                    return true;
                }
            }

            Debug.Log("Inventory full");
            return false;
        }

        public void RemoveItem(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < items.Length && items[slotIndex] != null)
            {
                Debug.Log($"Removed {items[slotIndex].itemName}");
                items[slotIndex] = null;
                onInventoryChanged?.Invoke();
            }
        }

        // Method to use an item from inventory
        public void UseItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= items.Length || items[slotIndex] == null)
            {
                Debug.LogWarning($"Cannot use item: Invalid slot {slotIndex}");
                return;
            }

            ItemData item = items[slotIndex];
            Debug.Log($"Attempting to use item: {item.itemName} (Type: {item.itemType})");

            // Handle different item types
            bool wasUsed = false;

            switch (item.itemType)
            {
                case ItemType.Consumable:
                    wasUsed = UseConsumable(item, slotIndex);
                    break;

                case ItemType.Weapon:
                case ItemType.Armor:
                    wasUsed = EquipItem(item, slotIndex);
                    break;

                case ItemType.Currency:
                    Debug.Log($"Currency used: {item.itemName}");
                    wasUsed = true;
                    ReduceItemStack(slotIndex);
                    break;

                default:
                    Debug.Log($"Cannot use item type: {item.itemType}");
                    break;
            }

            if (wasUsed)
            {
                onInventoryChanged?.Invoke();
            }
        }

        // Use consumable (potion)
        private bool UseConsumable(ItemData item, int slotIndex)
        {
            if (item.itemType != ItemType.Consumable)
            {
                Debug.LogWarning($"Item {item.itemName} is not a consumable!");
                return false;
            }

            // Check if we have player health reference
            if (playerHealth == null)
            {
                playerHealth = FindAnyObjectByType<PoEClone2D.Player.PlayerHealth>();
                if (playerHealth == null)
                {
                    Debug.LogError("Cannot use potion: No PlayerHealth found!");
                    return false;
                }
            }

            // Apply effect based on item properties
            if (item.healAmount > 0)
            {
                Debug.Log($"Using {item.itemName} to heal {item.healAmount} HP");

                // Call Heal method on PlayerHealth
                playerHealth.Heal(item.healAmount);

                // Reduce stack or remove item
                ReduceItemStack(slotIndex);
                return true;
            }
            else
            {
                Debug.LogWarning($"Consumable {item.itemName} has no healAmount set!");
                return false;
            }
        }

        // Equip weapon or armor
        private bool EquipItem(ItemData item, int slotIndex)
        {
            if (item.itemType != ItemType.Weapon && item.itemType != ItemType.Armor)
            {
                Debug.LogWarning($"Cannot equip {item.itemName}: Not equipment");
                return false;
            }

            if (equipmentSystem == null)
            {
                Debug.LogError("Cannot equip item: No EquipmentSystem found!");
                return false;
            }

            Debug.Log($"Attempting to equip {item.itemName}...");

            // Try to equip the item
            bool equipped = equipmentSystem.EquipItem(item);

            if (equipped)
            {
                Debug.Log($"Successfully equipped {item.itemName}");
                // Remove from inventory if equipped successfully
                RemoveItem(slotIndex);
                return true;
            }
            else
            {
                Debug.LogWarning($"Failed to equip {item.itemName}");
                return false;
            }
        }

        // Reduce stack size by 1 instead of removing entire stack
        public void ReduceItemStack(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < items.Length && items[slotIndex] != null)
            {
                if (items[slotIndex].isStackable && items[slotIndex].currentStack > 1)
                {
                    // Reduce stack by 1
                    items[slotIndex].currentStack--;
                    Debug.Log($"Reduced {items[slotIndex].itemName} stack to {items[slotIndex].currentStack}");
                }
                else
                {
                    // Remove item if stack is 1 or item is not stackable
                    Debug.Log($"Removing last item: {items[slotIndex].itemName}");
                    items[slotIndex] = null;
                }
            }
        }

        public ItemData GetItem(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < items.Length)
                return items[slotIndex];
            return null;
        }

        public int GetInventorySize() => inventorySize;

        // TEST METHODS
        [ContextMenu("Add Test Sword")]
        public void AddTestSword()
        {
            ItemData sword = ScriptableObject.CreateInstance<ItemData>();
            sword.itemName = "Iron Sword";
            sword.description = "Basic sword";
            sword.itemType = ItemType.Weapon;
            sword.equipSlot = ItemSlot.MainHand;
            sword.minDamage = 10;
            sword.maxDamage = 15;
            sword.AddStatModifier("Strength", 5);
            sword.AddStatModifier("Damage", 3);
            AddItem(sword);
        }

        [ContextMenu("Add Test Potion")]
        public void AddTestPotion()
        {
            ItemData potion = ScriptableObject.CreateInstance<ItemData>();
            potion.itemName = "Health Potion";
            potion.description = "Heals 50 HP";
            potion.itemType = ItemType.Consumable;
            potion.isStackable = true;
            potion.maxStack = 5;
            potion.healAmount = 50;
            AddItem(potion);
        }

        [ContextMenu("Add Test Armor")]
        public void AddTestArmor()
        {
            ItemData armor = ScriptableObject.CreateInstance<ItemData>();
            armor.itemName = "Leather Armor";
            armor.description = "Basic leather armor";
            armor.itemType = ItemType.Armor;
            armor.equipSlot = ItemSlot.Body;
            armor.armor = 20;
            armor.AddStatModifier("Strength", 3);
            armor.AddStatModifier("Health", 30);
            AddItem(armor);
        }

        [ContextMenu("Damage Player & Add Potion")]
        public void DamageAndAddPotion()
        {
            // First damage the player
            var health = FindAnyObjectByType<PoEClone2D.Player.PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(30);
                Debug.Log($"Player damaged by 30 HP.");
            }

            // Then add a potion
            AddTestPotion();
        }

        [ContextMenu("Debug Health Connection")]
        public void DebugHealthConnection()
        {
            Debug.Log("=== DEBUGGING HEALTH CONNECTION ===");

            // Check PlayerHealth
            var ph = FindAnyObjectByType<PoEClone2D.Player.PlayerHealth>();
            if (ph != null)
            {
                Debug.Log($"PlayerHealth found: {ph.gameObject.name}");
            }
            else
            {
                Debug.LogError("No PlayerHealth found!");
            }
        }
    }
}