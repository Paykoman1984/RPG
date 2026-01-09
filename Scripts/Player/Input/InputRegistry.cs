// Assets/Scripts/Player/Input/InputRegistry.cs
using UnityEngine;

public static class InputRegistry
{
    private static Vector2 moveInput = Vector2.zero;
    private static bool attackPressed = false;
    private static bool dashPressed = false;
    private static bool interactPressed = false;

    // Properties
    public static Vector2 MoveInput => moveInput;
    public static bool AttackPressed => attackPressed;
    public static bool DashPressed => dashPressed;
    public static bool InteractPressed => interactPressed;

    // Registration methods
    public static void RegisterMoveInput(Vector2 input)
    {
        moveInput = input.normalized; // Ensure normalized
        // Debug.Log($"InputRegistry: MoveInput registered = {moveInput}");
    }

    public static void RegisterAttackInput(bool pressed)
    {
        attackPressed = pressed;
        if (pressed)
        {
            // Debug.Log("InputRegistry: Attack registered");
            PlayerEvents.TriggerAttack(Vector2.zero);
        }
    }

    public static void RegisterDashInput(bool pressed)
    {
        dashPressed = pressed;
        if (pressed)
        {
            // Debug.Log("InputRegistry: Dash registered");
            PlayerEvents.TriggerDash(Vector2.zero);
        }
    }

    public static void RegisterInteractInput(bool pressed)
    {
        interactPressed = pressed;
        if (pressed) Debug.Log("InputRegistry: Interact registered");
    }

    // Clear methods
    public static void ClearInputs()
    {
        // Don't clear moveInput here - it should persist
        attackPressed = false;
        dashPressed = false;
        interactPressed = false;
    }

    public static void ClearAllInputs()
    {
        moveInput = Vector2.zero;
        attackPressed = false;
        dashPressed = false;
        interactPressed = false;
    }

    // Debug method
    public static void DebugState()
    {
        Debug.Log($"InputRegistry State - Move: {moveInput}, Attack: {attackPressed}, Dash: {dashPressed}");
    }
}