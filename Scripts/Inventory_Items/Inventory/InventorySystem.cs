using System;
using UnityEngine;

namespace PoEClone2D
{
    public class InventorySystem : MonoBehaviour
    {
        [SerializeField] private int inventorySize = 30;
        private ItemData[] items;

        public Action onInventoryChanged;

        void Start()
        {
            items = new ItemData[inventorySize];
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
                        Debug.Log($"Stacked {item.itemName}");
                        return true;
                    }
                }
            }

            // Find empty slot
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    items[i] = Instantiate(item); // Copy ScriptableObject
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
            sword.equipSlot = ItemSlot.MainHand; // MAKE SURE THIS IS SET!
            sword.minDamage = 10;
            sword.maxDamage = 15;

            // IMPORTANT: Make sure icon is set if you have one
            // sword.icon = YourSwordIconReference;

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
    }
}