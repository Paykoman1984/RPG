using UnityEngine;

namespace PoEClone2D
{
    public class DebugHotkeys : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventorySystem inventory;
        [SerializeField] private EquipmentSystem equipment;

        [Header("Hotkey Settings")]
        [SerializeField] private bool enableDebugHotkeys = true;

        void Start()
        {
            if (inventory == null)
                inventory = FindAnyObjectByType<InventorySystem>();

            if (equipment == null)
                equipment = FindAnyObjectByType<EquipmentSystem>();

            Debug.Log("DebugHotkeys loaded. Press:\n" +
                     "1: Add Test Sword\n" +
                     "2: Add Test Potion\n" +
                     "3: Add Test Armor\n" +
                     "4: Damage Player\n" +
                     "5: Print Equipment Status");
        }

        void Update()
        {
            if (!enableDebugHotkeys) return;

            // These work anywhere in the game
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AddTestSword();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                AddTestPotion();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                AddTestArmor();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                DamagePlayer();
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                PrintEquipmentStatus();
            }
        }

        void AddTestSword()
        {
            if (inventory == null) return;

            ItemData sword = ScriptableObject.CreateInstance<ItemData>();
            sword.itemName = "Iron Sword";
            sword.description = "Basic sword";
            sword.itemType = ItemType.Weapon;
            sword.equipSlot = ItemSlot.MainHand;
            sword.minDamage = 10;
            sword.maxDamage = 15;
            sword.AddStatModifier("Strength", 5);
            sword.AddStatModifier("Damage", 3);

            inventory.AddItem(sword);
            Debug.Log("Test sword added (press I to open inventory)");
        }

        void AddTestPotion()
        {
            if (inventory == null) return;

            ItemData potion = ScriptableObject.CreateInstance<ItemData>();
            potion.itemName = "Health Potion";
            potion.description = "Heals 50 HP";
            potion.itemType = ItemType.Consumable;
            potion.isStackable = true;
            potion.maxStack = 5;
            potion.healAmount = 50;

            inventory.AddItem(potion);
            Debug.Log("Test potion added (press I to open inventory)");
        }

        void AddTestArmor()
        {
            if (inventory == null) return;

            ItemData armor = ScriptableObject.CreateInstance<ItemData>();
            armor.itemName = "Leather Armor";
            armor.description = "Basic leather armor";
            armor.itemType = ItemType.Armor;
            armor.equipSlot = ItemSlot.Body;
            armor.armor = 20;
            armor.AddStatModifier("Strength", 3);
            armor.AddStatModifier("Health", 30);

            inventory.AddItem(armor);
            Debug.Log("Test armor added (press I to open inventory)");
        }

        void DamagePlayer()
        {
            var playerHealth = FindAnyObjectByType<PoEClone2D.Player.PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(30);
                Debug.Log("Player damaged by 30 HP");
            }
        }

        void PrintEquipmentStatus()
        {
            if (equipment != null)
            {
                equipment.PrintEquipmentStatus();
            }
        }
    }
}
