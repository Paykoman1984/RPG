using UnityEngine;

namespace PoEClone2D
{
    public class EnemyLoot : MonoBehaviour
    {
        [Header("Loot Settings")]
        public LootTable lootTable;
        public bool useLootTable = true;

        [Header("Legacy Settings")]
        public GameObject[] itemDropPrefabs;
        [Range(0f, 1f)] public float dropChance = 1f;
        public int minDrops = 1;
        public int maxDrops = 1;

        public void DropLoot(Vector3 position)
        {
            if (useLootTable && lootTable != null)
            {
                if (ItemDropManager.Instance != null)
                {
                    ItemDropManager.Instance.DropLootFromTable(position, lootTable);
                }
            }
            else if (itemDropPrefabs != null && itemDropPrefabs.Length > 0)
            {
                DropLegacyLoot(position);
            }
        }

        private void DropLegacyLoot(Vector3 position)
        {
            int numDrops = Random.Range(minDrops, maxDrops + 1);
            for (int i = 0; i < numDrops; i++)
            {
                if (Random.value <= dropChance)
                {
                    GameObject prefab = itemDropPrefabs[Random.Range(0, itemDropPrefabs.Length)];
                    if (prefab != null)
                    {
                        Instantiate(prefab, position, Quaternion.identity);
                    }
                }
            }
        }
    }
}