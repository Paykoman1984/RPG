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

        [Header("Area Attack")]
        public bool isAreaAttack = false;
        public float areaRadius = 1f;

        [Header("Multiple Hits")]
        public bool allowMultipleHits = false;

        [Header("Visuals")]
        public GameObject hitEffectPrefab;

        [Header("Audio")]
        [SerializeField] private AudioClip attackSound;      // Sound when attack starts/swing
        [SerializeField] private AudioClip hitSound;         // Sound when attack hits (ONLY plays on hit)
        [SerializeField] private AudioClip missSound;        // Sound when attack misses (optional)
        [SerializeField][Range(0f, 1f)] private float attackVolume = 0.7f;
        [SerializeField][Range(0f, 1f)] private float hitVolume = 0.8f;
        [SerializeField] private bool randomizePitch = true;
        [SerializeField][Range(0.8f, 1.2f)] private float minPitch = 0.9f;
        [SerializeField][Range(0.8f, 1.2f)] private float maxPitch = 1.1f;

        // Calculated property
        public float AttackInterval => 1f / attackRate;

        // Audio properties with getters
        public AudioClip AttackSound => attackSound;
        public AudioClip HitSound => hitSound;
        public AudioClip MissSound => missSound;
        public float AttackVolume => attackVolume;
        public float HitVolume => hitVolume;
        public bool RandomizePitch => randomizePitch;
        public float MinPitch => minPitch;
        public float MaxPitch => maxPitch;
    }
}