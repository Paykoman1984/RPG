using PoEClone2D.Combat;
using UnityEngine;

public class AnimationEventBridge : MonoBehaviour
{
    [SerializeField] private MeleeAttack meleeAttack;

    private void Awake()
    {
        if (meleeAttack == null)
            meleeAttack = GetComponentInParent<MeleeAttack>();
    }

    // ONLY USE THIS ONE EVENT in your animation
    public void EnableHitboxes()
    {
        Debug.Log("ANIMATION: EnableHitboxes called");
        meleeAttack?.EnableHitboxes();
    }

    // ONLY USE THIS ONE EVENT to complete attack
    public void CompleteAttack()
    {
        Debug.Log("ANIMATION: CompleteAttack called");
        meleeAttack?.CompleteAttack();
    }
}