using UnityEngine;

public class AttackPositionDebug : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestAttackPositions();
        }
    }

    void TestAttackPositions()
    {
        Debug.Log("=== ATTACK POSITION TEST ===");
        Debug.Log($"Player Position: {transform.position}");

        // Test all directions
        Vector2[] testDirections = {
            Vector2.right,
            Vector2.left,
            Vector2.up,
            Vector2.down
        };

        foreach (Vector2 dir in testDirections)
        {
            Vector2 spawnPos = (Vector2)transform.position + (dir * 0.3f);
            Debug.Log($"Direction {dir}: Spawn at {spawnPos}");

            // Visualize in scene
            Debug.DrawLine(transform.position, spawnPos, Color.red, 2f);
            Debug.DrawRay(spawnPos, Vector3.up * 0.1f, Color.green, 2f);
        }
    }
}