using UnityEngine;
using System.Collections.Generic;
using PoEClone2D.Items;

namespace PoEClone2D
{
    public enum ItemType { Weapon, Armor, Consumable, Currency, Gem, Quest }
    public enum ItemRarity { Normal, Magic, Rare, Unique, Currency }
    public enum ItemSlot { None, Head, Body, Hands, Feet, MainHand, OffHand, Belt, Amulet, Ring1, Ring2 }

    [CreateAssetMenu(fileName = "NewItem", menuName = "PoE Clone/Items/Base Item")]
    public class ItemData : ScriptableObject
    {
        public string itemName = "New Item";
        public string description = "Description";
        public ItemType itemType = ItemType.Consumable;
        public ItemRarity rarity = ItemRarity.Normal;
        public ItemSlot equipSlot = ItemSlot.None;
        public Sprite icon;

        public bool isStackable = false;
        public int maxStack = 1;
        [HideInInspector] public int currentStack = 1;

        // Weapon stats
        public float minDamage = 0;
        public float maxDamage = 0;
        public float attackSpeed = 1.0f;

        // Armor stats
        public float armor = 0;

        // Stat modifiers (for any item type)
        public List<StatModifier> statModifiers = new List<StatModifier>();

        // Consumable stats
        public int healAmount = 0;

        // Helper method to add stat modifiers
        public void AddStatModifier(string statName, float value, StatModifier.StatModifierType type = StatModifier.StatModifierType.Flat)
        {
            statModifiers.Add(new StatModifier(statName, value, type));
        }

        // Get total stat value by name
        public float GetStatValue(string statName)
        {
            float total = 0;
            foreach (var modifier in statModifiers)
            {
                if (modifier.statName == statName)
                {
                    total += modifier.value;
                }
            }
            return total;
        }
    }
}
