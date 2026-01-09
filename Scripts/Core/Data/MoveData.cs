// Assets/Scripts/Core/Data/MovementData.cs
using UnityEngine;

[System.Serializable]
public class MovementData
{
    public Vector2 moveInput;
    public Vector2 lastMoveDirection = Vector2.down;
    public bool isMoving => moveInput.magnitude > 0.1f;

    public void UpdateInput(Vector2 input)
    {
        moveInput = input.normalized;
        if (moveInput.magnitude > 0.1f)
        {
            lastMoveDirection = moveInput;
        }
    }

    public Vector2 Get4WayDirection(Vector2 input)
    {
        if (input.magnitude < 0.1f) return Vector2.down;

        float absX = Mathf.Abs(input.x);
        float absY = Mathf.Abs(input.y);

        if (absX > absY * 0.8f)
            return new Vector2(Mathf.Sign(input.x), 0);
        else
            return new Vector2(0, Mathf.Sign(input.y));
    }
}