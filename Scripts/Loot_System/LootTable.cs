using UnityEngine;
using System.Collections.Generic;

namespace PoEClone2D
{
    [CreateAssetMenu(fileName = "NewLootTable", menuName = "PoE Clone/Loot/LootTable")]
    public class LootTable : ScriptableObject
    {
        [System.Serializable]
        public class LootEntry
        {
            public ItemData item;
            [Range(0f, 1f)] public float dropChance = 0.5f;
            public int minQuantity = 1;
            public int maxQuantity = 1;
        }

        public List<LootEntry> lootEntries = new List<LootEntry>();

        public List<ItemDrop> GetRandomLoot()
        {
            List<ItemDrop> droppedItems = new List<ItemDrop>();

            foreach (LootEntry entry in lootEntries)
            {
                if (entry.item != null && Random.value <= entry.dropChance)
                {
                    int quantity = Random.Range(entry.minQuantity, entry.maxQuantity + 1);
                    ItemDrop drop = new ItemDrop
                    {
                        itemData = entry.item,
                        quantity = quantity
                    };
                    droppedItems.Add(drop);
                }
            }

            return droppedItems;
        }
    }

    [System.Serializable]
    public class ItemDrop
    {
        public ItemData itemData;
        public int quantity = 1;
    }
}