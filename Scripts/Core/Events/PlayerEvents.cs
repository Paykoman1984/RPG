// Assets/Scripts/Core/Events/PlayerEvents.cs
using UnityEngine;

public static class PlayerEvents
{
    public delegate void PlayerActionEvent(Vector2 direction);
    public delegate void PlayerStateEvent(bool state);
    public delegate void PlayerSimpleEvent();

    // Movement Events
    public static event PlayerActionEvent OnPlayerMove;
    public static event PlayerActionEvent OnPlayerDash;
    public static event PlayerSimpleEvent OnPlayerStop;

    // Combat Events
    public static event PlayerActionEvent OnPlayerAttack;
    public static event PlayerSimpleEvent OnPlayerAttackStart;
    public static event PlayerSimpleEvent OnPlayerAttackEnd;
    public static event PlayerSimpleEvent OnPlayerAttackCancelled;

    // State Events
    public static event PlayerStateEvent OnAttackingStateChanged;
    public static event PlayerStateEvent OnDashingStateChanged;
    public static event PlayerStateEvent OnMovingStateChanged;

    // Public triggers
    public static void TriggerMove(Vector2 direction) => OnPlayerMove?.Invoke(direction);
    public static void TriggerDash(Vector2 direction) => OnPlayerDash?.Invoke(direction);
    public static void TriggerStop() => OnPlayerStop?.Invoke();
    public static void TriggerAttack(Vector2 direction) => OnPlayerAttack?.Invoke(direction);
    public static void TriggerAttackStart() => OnPlayerAttackStart?.Invoke();
    public static void TriggerAttackEnd() => OnPlayerAttackEnd?.Invoke();
    public static void TriggerAttackCancelled() => OnPlayerAttackCancelled?.Invoke();
    public static void TriggerAttackingState(bool state) => OnAttackingStateChanged?.Invoke(state);
    public static void TriggerDashingState(bool state) => OnDashingStateChanged?.Invoke(state);
    public static void TriggerMovingState(bool state) => OnMovingStateChanged?.Invoke(state);
}