using UnityEngine;

namespace PoEClone2D.Enemy
{
    public class EnemyAnimationEventBridge : MonoBehaviour
    {
        [SerializeField] private TestEnemy enemyController;

        private void Awake()
        {
            if (enemyController == null)
            {
                enemyController = GetComponentInParent<TestEnemy>();
            }

            if (enemyController == null)
            {
                Debug.LogError("EnemyAnimationEventBridge: No TestEnemy found in parent!");
            }
        }

        // Called by Animation Event when attack should hit
        public void OnAttackHitFrame()
        {
            if (enemyController != null)
            {
                enemyController.OnAttackHitFrame();
            }
        }

        // Called by Animation Event when attack animation ends
        public void OnAttackEnd()
        {
            if (enemyController != null)
            {
                enemyController.OnAttackEnd();
            }
        }
    }
}