using PoEClone2D;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private InventoryUI inventoryUI;
    private InventorySystem inventorySystem;

    void Start()
    {
        // Find references at start
        inventoryUI = FindAnyObjectByType<InventoryUI>();
        inventorySystem = FindAnyObjectByType<InventorySystem>();

        if (inventoryUI == null)
            Debug.LogWarning("GameManager: No InventoryUI found in scene");
        if (inventorySystem == null)
            Debug.LogWarning("GameManager: No InventorySystem found in scene");
    }

    void Update()
    {
        // Toggle inventory
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))
        {
            if (inventoryUI != null)
            {
                inventoryUI.ToggleInventory();
                Debug.Log("Toggled inventory with I/Tab");
            }
            else
            {
                Debug.LogWarning("Cannot toggle inventory - InventoryUI not found");
            }
        }

        // Add test sword
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (inventorySystem != null)
            {
                inventorySystem.AddTestSword();
                Debug.Log("Added test sword with T");
            }
            else
            {
                Debug.LogWarning("Cannot add sword - InventorySystem not found");
            }
        }

        // Add test potion
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (inventorySystem != null)
            {
                inventorySystem.AddTestPotion();
                Debug.Log("Added test potion with P");
            }
            else
            {
                Debug.LogWarning("Cannot add potion - InventorySystem not found");
            }
        }
    }
}