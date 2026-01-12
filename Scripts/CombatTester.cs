using PoEClone2D.Combat;
using PoEClone2D.Enemy;
using PoEClone2D.Player;
using UnityEngine;

public class CombatTester : MonoBehaviour
{
    public TestEnemy testEnemy;
    public PlayerHealth playerHealth;
    public AttackData playerAttackData;

    [ContextMenu("Test Player Hit Enemy")]
    public void TestPlayerHitEnemy()
    {
        if (testEnemy != null)
        {
            testEnemy.TakeDamage(10f);
            testEnemy.ApplyKnockback(Vector2.right * 10f);
            Debug.Log("Player hit enemy test executed");
        }
    }

    [ContextMenu("Test Enemy Hit Player")]
    public void TestEnemyHitPlayer()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(5f);
            playerHealth.ApplyKnockback(Vector2.left * 8f);
            Debug.Log("Enemy hit player test executed");
        }
    }
}