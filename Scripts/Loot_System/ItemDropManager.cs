using UnityEngine;
using System.Collections.Generic;

namespace PoEClone2D
{
    public class ItemDropManager : MonoBehaviour
    {
        public static ItemDropManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private Transform itemDropContainer;
        [SerializeField] private float baseDropRadius = 1f;

        private List<GameObject> activePickups = new List<GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeContainer();
                Debug.Log("ItemDropManager initialized");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeContainer()
        {
            if (itemDropContainer == null)
            {
                GameObject container = new GameObject("ItemDrops");
                itemDropContainer = container.transform;
            }
        }

        public void DropLootFromTable(Vector3 position, LootTable lootTable)
        {
            if (lootTable == null)
            {
                Debug.LogWarning("Cannot drop loot: Missing loot table");
                return;
            }

            List<ItemDrop> drops = lootTable.GetRandomLoot();

            if (drops.Count == 0)
            {
                Debug.Log("No items dropped from loot table");
                return;
            }

            Debug.Log($"Dropping {drops.Count} item(s) from loot table");

            foreach (ItemDrop drop in drops)
            {
                for (int i = 0; i < drop.quantity; i++)
                {
                    DropItemAtPosition(position, drop.itemData);
                }
            }
        }

        public void DropItemAtPosition(Vector3 position, ItemData item)
        {
            if (item == null)
            {
                Debug.LogError("ItemDropManager: Cannot drop null item!");
                return;
            }

            Vector2 randomOffset = Random.insideUnitCircle * baseDropRadius;
            Vector3 dropPosition = position + new Vector3(randomOffset.x, randomOffset.y, 0);

            CreatePickup(dropPosition, item);
        }

        private void CreatePickup(Vector3 position, ItemData item)
        {
            GameObject pickup = new GameObject($"Pickup_{item.itemName}");
            pickup.transform.position = position;
            pickup.transform.parent = itemDropContainer;

            // CORRECTED: Use SimpleClickPickup instead of ItemPickup
            SimpleClickPickup pickupScript = pickup.AddComponent<SimpleClickPickup>();
            pickupScript.itemData = item;

            // Add to active pickups list for cleanup
            activePickups.Add(pickup);

            Debug.Log($"Created pickup: {item.itemName}");
        }

        private Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Normal => Color.white,
                ItemRarity.Magic => Color.blue,
                ItemRarity.Rare => Color.yellow,
                ItemRarity.Unique => Color.magenta,
                ItemRarity.Currency => Color.green,
                _ => Color.white
            };
        }

        public void CleanupPickups()
        {
            foreach (GameObject pickup in activePickups)
            {
                if (pickup != null)
                    Destroy(pickup);
            }
            activePickups.Clear();
        }
    }
}