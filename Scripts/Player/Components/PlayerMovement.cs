using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private bool isDashing = false;

    public void SetRigidbody(Rigidbody2D rigidbody)
    {
        rb = rigidbody;
        Debug.Log("PlayerMovement: Rigidbody2D assigned");
    }

    public void Move(Vector2 input)
    {
        if (rb == null || isDashing)
        {
            Debug.LogWarning("PlayerMovement: Cannot move - rb is null or dashing");
            return;
        }

        Vector2 movement = input.normalized * moveSpeed;
        rb.linearVelocity = movement;

        // Debug log only when actually moving
        if (input.magnitude > 0.1f)
        {
            Debug.Log($"PlayerMovement.Move: ({input.x:F2}, {input.y:F2}) at speed {moveSpeed}");
        }
    }

    public void StartDash(Vector2 direction, float speed, float duration)
    {
        isDashing = true;
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
            Debug.Log($"PlayerMovement: Dashing at speed {speed} in direction {direction}");
        }
    }

    public void EndDash()
    {
        isDashing = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        Debug.Log("PlayerMovement: Dash ended");
    }

    void Awake()
    {
        Debug.Log("PlayerMovement initialized");

        // Try to get Rigidbody2D if not set
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogWarning("PlayerMovement: No Rigidbody2D found on awake");
            }
        }
    }
}