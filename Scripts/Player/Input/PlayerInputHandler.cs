using UnityEngine;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    public event Action<Vector2> OnMoveInput;
    public event Action OnAttackInput;
    public event Action OnDashInput;
    public event Action OnInteractInput;

    private Vector2 currentInput = Vector2.zero;
    private Vector2 lastNonZeroInput = Vector2.down;

    public Vector2 GetCurrentInput()
    {
        return currentInput;
    }

    public Vector2 GetLastNonZeroInput()
    {
        return lastNonZeroInput;
    }

    public Vector2 GetAttackDirection()
    {
        if (currentInput.magnitude > 0.1f)
        {
            return currentInput.normalized;
        }

        return lastNonZeroInput;
    }

    private void Update()
    {
        // Get raw input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 rawInput = new Vector2(horizontal, vertical);

        // Store current input
        currentInput = rawInput;

        // Update last non-zero input
        if (rawInput.magnitude > 0.1f)
        {
            lastNonZeroInput = rawInput.normalized;
        }

        // Always trigger move event (even zero for stopping)
        OnMoveInput?.Invoke(rawInput);

        // Action inputs
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J))
        {
            OnAttackInput?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.K))
        {
            OnDashInput?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            OnInteractInput?.Invoke();
        }
    }

    // For interface compatibility
    public void Enable()
    {
        enabled = true;
    }

    public void Disable()
    {
        enabled = false;
        currentInput = Vector2.zero;
        OnMoveInput?.Invoke(Vector2.zero);
    }
}