// Assets/Scripts/Player/Input/PlayerInputHandler.cs
using UnityEngine;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    public event Action<Vector2> OnMoveInput;
    public event Action OnAttackInput;
    public event Action OnDashInput;
    public event Action OnInteractInput;

    private void Update()
    {
        // Movement input (continuous)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 moveInput = new Vector2(h, v);

        if (moveInput.magnitude > 0.1f)
        {
            OnMoveInput?.Invoke(moveInput.normalized);
            InputRegistry.RegisterMoveInput(moveInput.normalized);
        }
        else if (moveInput.magnitude == 0)
        {
            // Send zero input when no keys pressed
            OnMoveInput?.Invoke(Vector2.zero);
            InputRegistry.RegisterMoveInput(Vector2.zero);
        }

        // Action inputs (one-time)
        if (Input.GetMouseButtonDown(0))
        {
            OnAttackInput?.Invoke();
            InputRegistry.RegisterAttackInput(true);
            Debug.Log("Attack input registered");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnDashInput?.Invoke();
            InputRegistry.RegisterDashInput(true);
            Debug.Log("Dash input registered");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            OnInteractInput?.Invoke();
            InputRegistry.RegisterInteractInput(true);
        }
    }
}