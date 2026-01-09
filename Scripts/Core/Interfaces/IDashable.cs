// Assets/Scripts/Core/Interfaces/IDashable.cs
using UnityEngine;

public interface IDashable
{
    bool IsDashing { get; }
    float DashCooldown { get; }
    float DashDuration { get; }
    bool CanDashCancelAttack { get; }
    bool CanDash();
    void StartDash(Vector2 direction);
    void CancelDash();
}