// Assets/Scripts/Core/Interfaces/IInputHandler.cs
using UnityEngine;

public interface IInputHandler
{
    void Enable();
    void Disable();
    event System.Action<Vector2> OnMoveInput;
    event System.Action OnAttackInput;
    event System.Action OnDashInput;
    event System.Action OnInteractInput;
}