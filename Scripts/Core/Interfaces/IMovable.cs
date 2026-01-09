// Assets/Scripts/Core/Interfaces/IMovable.cs
using UnityEngine;

public interface IMovable
{
    float MoveSpeed { get; }
    Vector2 MoveDirection { get; }
    bool IsMoving { get; }
    void Move(Vector2 direction);
    void Stop();
    void Dash(Vector2 direction);
    bool CanDash();
}