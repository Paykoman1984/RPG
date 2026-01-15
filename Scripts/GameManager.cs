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
    }
}