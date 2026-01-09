// Assets/Scripts/Core/Data/CombatData.cs
using UnityEngine;

[System.Serializable]
public class CombatData
{
    public bool isAttacking = false;
    public bool isDashing = false;
    public float nextAttackTime = 0f;
    public float attackStartTime = 0f;
    public float dashEndTime = 0f;
    public float nextDashTime = 0f;
    public float attackAnimationLength = 0.5f;

    public bool CanAttack(float currentTime)
    {
        return !isAttacking && !isDashing && currentTime >= nextAttackTime;
    }

    public bool CanDash(float currentTime)
    {
        return !isDashing && currentTime >= nextDashTime;
    }
}