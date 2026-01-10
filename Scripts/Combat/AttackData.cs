using UnityEngine;

namespace PoEClone2D.Combat
{
    [CreateAssetMenu(fileName = "NewAttackData", menuName = "Combat/Attack Data")]
    public class AttackData : ScriptableObject
    {
        [Header("Basic Settings")]
        public float damage = 10f;
        public float range = 0.5f;
        public float attackRate = 1f; // Attacks per second

        [Header("Timing")]
        public float windupTime = 0.1f;
        public float activeTime = 0.2f;
        public float recoveryTime = 0.2f;

        [Header("Knockback")]
        public bool hasKnockback = true;
        public float knockbackForce = 5f;

        [Header("Area Attack")]
        public bool isAreaAttack = false;
        public float areaRadius = 1f;

        [Header("Multiple Hits")]
        public bool allowMultipleHits = false;

        [Header("Visuals")]
        public GameObject hitEffectPrefab;

        // Calculated property
        public float AttackInterval => 1f / attackRate;
    }
}