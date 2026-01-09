// Assets/Scripts/Player/Components/PlayerMovement.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour, IMovable
{
    public float MoveSpeed { get; private set; } = 5f;
    public Vector2 MoveDirection { get; private set; }
    public bool IsMoving { get; private set; }

    private Rigidbody2D rb;
    private Vector2 lastMoveDirection = Vector2.down;

    public event System.Action<Vector2> OnMove;
    public event System.Action OnStop;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Debug.Log("PlayerMovement initialized");
    }

    public void Move(Vector2 direction)
    {
        MoveDirection = direction;

        if (direction.magnitude > 0.1f)
        {
            IsMoving = true;
            lastMoveDirection = direction;
            OnMove?.Invoke(direction);

            Debug.Log($"PlayerMovement.Move: {direction}");
        }
        else
        {
            Stop();
        }
    }

    public void Stop()
    {
        IsMoving = false;
        OnStop?.Invoke();
    }

    // Interface methods - simplified
    public void Dash(Vector2 direction)
    {
        // Handled by PlayerController
    }

    public bool CanDash()
    {
        return true;
    }

    public Vector2 GetLastDirection() => lastMoveDirection;

    // Remove SetPlayerData if not needed
    public void SetPlayerData(PlayerData data)
    {
        if (data != null)
        {
            MoveSpeed = data.moveSpeed;
        }
    }
}