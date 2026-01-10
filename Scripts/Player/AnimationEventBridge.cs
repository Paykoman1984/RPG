using PoEClone2D.Combat;
using UnityEngine;

namespace PoEClone2D.Player
{
    public class AnimationEventBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MeleeAttack meleeAttack;

        private void Awake()
        {
            // Try to find references if not set
            if (meleeAttack == null)
            {
                meleeAttack = GetComponentInParent<MeleeAttack>();
            }
        }

        // ============ ANIMATION EVENTS ============

        public void OnAttackHit()
        {
            if (meleeAttack != null)
            {
                meleeAttack.OnAttackHit();
            }
        }

        public void EnableDamage()
        {
            if (meleeAttack != null)
            {
                meleeAttack.EnableDamage();
            }
        }

        public void DisableDamage()
        {
            if (meleeAttack != null)
            {
                meleeAttack.DisableDamage();
            }
        }
    }
}