using PoEClone2D.Player;
using UnityEngine;

public class CombatTester : MonoBehaviour
{
    public GameObject player;
    public GameObject enemy;
    public float testDistance = 2f;

    private void OnGUI()
    {
        if (GUILayout.Button("Position Enemy Close"))
        {
            enemy.transform.position = player.transform.position + Vector3.right * testDistance;
        }

        if (GUILayout.Button("Position Enemy Far"))
        {
            enemy.transform.position = player.transform.position + Vector3.right * 5f;
        }

        if (GUILayout.Button("Attack Now"))
        {
            var combat = player.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                // Force attack
                Debug.Log("Forcing attack...");
                // You might need to expose a public method in PlayerCombat to trigger attack
            }
        }
    }
}