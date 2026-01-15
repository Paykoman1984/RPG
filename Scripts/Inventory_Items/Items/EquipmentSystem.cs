using UnityEngine;
using System.Collections.Generic;
using PoEClone2D.Items;

namespace PoEClone2D
{
    public class EquipmentSystem : MonoBehaviour
    {
        [System.Serializable]
        public class EquipmentSlot
        {
            public ItemSlot slotType;
            public ItemData equippedItem;
        }

        [Header("Equipment Slots")]
        [SerializeField] private List<EquipmentSlot> equipmentSlots = new List<EquipmentSlot>();

        [Header("References")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private InventorySystem inventorySystem;

        // Track currently equipped items
        private Dictionary<ItemSlot, ItemData> equippedItems = new Dictionary<ItemSlot, ItemData>();

        // Events
        public System.Action<ItemData> OnItemEquipped;
        public System.Action<ItemData> OnItemUnequipped;

        void Start()
        {
            InitializeEquipmentSlots();

            if (playerStats == null)
                playerStats = FindAnyObjectByType<PlayerStats>();

            if (inventorySystem == null)
                inventorySystem = FindAnyObjectByType<InventorySystem>();

            Debug.Log("Equipment System initialized");
        }

        private void InitializeEquipmentSlots()
        {
            // Initialize all equipment slots
            foreach (ItemSlot slot in System.Enum.GetValues(typeof(ItemSlot)))
            {
                if (slot != ItemSlot.None)
                {
                    equippedItems[slot] = null;
                }
            }
        }

        // Equip an item
        public bool EquipItem(ItemData item)
        {
            if (item == null || item.equipSlot == ItemSlot.None)
            {
                Debug.LogWarning($"Cannot equip {item?.itemName}: No valid equip slot");
                return false;
            }

            // Check if slot is already occupied
            if (equippedItems[item.equipSlot] != null)
            {
                // Unequip current item first
                UnequipItem(item.equipSlot);
            }

            // Equip the new item
            equippedItems[item.equipSlot] = item;

            // Apply stat bonuses
            ApplyItemStats(item, true);

            // Trigger event
            OnItemEquipped?.Invoke(item);

            Debug.Log($"<color=green>✓ Equipped:</color> {item.itemName} in {item.equipSlot}");
            return true;
        }

        // Unequip an item
        public bool UnequipItem(ItemSlot slot)
        {
            if (slot == ItemSlot.None || equippedItems[slot] == null)
            {
                Debug.LogWarning($"No item to unequip from {slot}");
                return false;
            }

            ItemData item = equippedItems[slot];

            // Remove stat bonuses
            ApplyItemStats(item, false);

            // Clear the slot
            equippedItems[slot] = null;

            // Try to add back to inventory
            bool addedToInventory = inventorySystem?.AddItem(item) ?? false;

            // Trigger event
            OnItemUnequipped?.Invoke(item);

            if (addedToInventory)
            {
                Debug.Log($"<color=yellow>↷ Unequipped:</color> {item.itemName} (returned to inventory)");
            }
            else
            {
                Debug.Log($"<color=yellow>↷ Unequipped:</color> {item.itemName} (inventory full)");
            }

            return addedToInventory;
        }

        // Apply or remove item stats
        private void ApplyItemStats(ItemData item, bool apply)
        {
            if (item == null || playerStats == null) return;

            float multiplier = apply ? 1 : -1;

            // Apply base stats based on item type
            switch (item.itemType)
            {
                case ItemType.Weapon:
                    Debug.Log($"Weapon equipped: {item.minDamage}-{item.maxDamage} damage");
                    break;

                case ItemType.Armor:
                    int healthBonus = Mathf.RoundToInt(item.armor);
                    playerStats.SetMaxHealth(playerStats.maxHealth + (healthBonus * (int)multiplier));
                    Debug.Log($"Armor gives {(apply ? "+" : "-")}{healthBonus} max HP");
                    break;
            }

            // Apply custom stat modifiers
            foreach (var modifier in item.statModifiers)
            {
                ApplyStatModifier(modifier, multiplier);
            }
        }

        private void ApplyStatModifier(StatModifier modifier, float multiplier)
        {
            if (playerStats == null) return;

            float value = modifier.value * multiplier;
            int intValue = Mathf.RoundToInt(value);

            switch (modifier.statName.ToLower())
            {
                case "strength":
                case "str":
                    playerStats.strength += intValue;
                    Debug.Log($"{(value > 0 ? "+" : "")}{intValue} Strength (Total: {playerStats.strength})");
                    break;

                case "dexterity":
                case "dex":
                    playerStats.dexterity += intValue;
                    Debug.Log($"{(value > 0 ? "+" : "")}{intValue} Dexterity (Total: {playerStats.dexterity})");
                    break;

                case "intelligence":
                case "int":
                    playerStats.intelligence += intValue;
                    Debug.Log($"{(value > 0 ? "+" : "")}{intValue} Intelligence (Total: {playerStats.intelligence})");
                    break;

                case "health":
                case "maxhealth":
                    playerStats.SetMaxHealth(playerStats.maxHealth + intValue);
                    Debug.Log($"{(value > 0 ? "+" : "")}{intValue} Max Health");
                    break;

                default:
                    Debug.Log($"Stat modifier: {modifier.statName} {(value > 0 ? "+" : "")}{value}");
                    break;
            }
        }

        // Get currently equipped item in a slot
        public ItemData GetEquippedItem(ItemSlot slot)
        {
            if (equippedItems.ContainsKey(slot))
                return equippedItems[slot];
            return null;
        }

        // Check if an item is equipped
        public bool IsItemEquipped(ItemData item)
        {
            foreach (var equipped in equippedItems.Values)
            {
                if (equipped == item)
                    return true;
            }
            return false;
        }

        // Get all equipped items
        public Dictionary<ItemSlot, ItemData> GetAllEquippedItems()
        {
            return new Dictionary<ItemSlot, ItemData>(equippedItems);
        }

        // Debug commands
        [ContextMenu("Print Equipment Status")]
        public void PrintEquipmentStatus()
        {
            Debug.Log("=== EQUIPMENT STATUS ===");
            foreach (var kvp in equippedItems)
            {
                if (kvp.Value != null)
                {
                    Debug.Log($"{kvp.Key}: {kvp.Value.itemName}");
                }
                else
                {
                    Debug.Log($"{kvp.Key}: Empty");
                }
            }
            Debug.Log("========================");
        }

        [ContextMenu("Test Equip Sword")]
        public void TestEquipSword()
        {
            ItemData sword = ScriptableObject.CreateInstance<ItemData>();
            sword.itemName = "Test Sword";
            sword.itemType = ItemType.Weapon;
            sword.equipSlot = ItemSlot.MainHand;
            sword.minDamage = 10;
            sword.maxDamage = 15;
            sword.AddStatModifier("Strength", 5);
            sword.AddStatModifier("Damage", 3);

            EquipItem(sword);
        }

        [ContextMenu("Test Equip Armor")]
        public void TestEquipArmor()
        {
            ItemData armor = ScriptableObject.CreateInstance<ItemData>();
            armor.itemName = "Test Armor";
            armor.itemType = ItemType.Armor;
            armor.equipSlot = ItemSlot.Body;
            armor.armor = 20;
            armor.AddStatModifier("Strength", 3);
            armor.AddStatModifier("Health", 30);

            EquipItem(armor);
        }
    }
}
