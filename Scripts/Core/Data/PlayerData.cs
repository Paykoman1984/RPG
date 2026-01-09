// Assets/Scripts/Core/Data/PlayerData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Data/Player/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public bool freezeMovementOnAttack = true;

    [Header("Combat")]
    public float attackCooldown = 0.5f;
    public bool canDashCancelAttack = true;

    [Header("Animation")]
    public float animationSpeed = 1f;
}