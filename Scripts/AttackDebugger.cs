using UnityEngine;

public class AttackDebugger : MonoBehaviour
{
    public PlayerDamageDealer damageDealer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("=== TEST ATTACK DOWN ===");
            damageDealer.PerformAttack(Vector2.down);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("=== TEST ATTACK RIGHT ===");
            damageDealer.PerformAttack(Vector2.right);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("=== TEST ATTACK LEFT ===");
            damageDealer.PerformAttack(Vector2.left);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("=== TEST ATTACK UP ===");
            damageDealer.PerformAttack(Vector2.up);
        }
    }

    void OnDrawGizmos()
    {
        if (damageDealer == null) return;

        // Draw debug spheres for expected hitbox positions
        Gizmos.color = Color.red;
        Vector3 playerPos = damageDealer.transform.position;

        // Down
        Vector3 downPos = playerPos + Vector3.down * 1.5f;
        Gizmos.DrawWireSphere(downPos, 0.3f);

        // Right
        Vector3 rightPos = playerPos + Vector3.right * 1.5f;
        Gizmos.DrawWireSphere(rightPos, 0.3f);

        // Left
        Vector3 leftPos = playerPos + Vector3.left * 2.0f; // With left bonus
        Gizmos.DrawWireSphere(leftPos, 0.3f);

        // Up
        Vector3 upPos = playerPos + Vector3.up * 1.5f;
        Gizmos.DrawWireSphere(upPos, 0.3f);
    }
}