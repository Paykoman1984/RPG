// Assets/Scripts/Player/PlayerController.cs
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public float attackCooldown = 0.5f;
    public bool freezeMovementOnAttack = true;
    public bool canDashCancelAttack = true;

    [Header("Components")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerAnimation playerAnim;
    [SerializeField] private PlayerInputHandler input;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down;

    // Dash state
    private bool isDashing = false;
    private float dashEndTime = 0f;
    private float nextDashTime = 0f;

    private void Awake()
    {
        Debug.Log("PlayerController Awake - Getting components");

        // Get components
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
        playerAnim = GetComponent<PlayerAnimation>();
        input = GetComponent<PlayerInputHandler>();

        Debug.Log($"Components - RB: {rb != null}, Movement: {movement != null}, Combat: {combat != null}, Anim: {playerAnim != null}, Input: {input != null}");

        // Setup Rigidbody if missing
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.linearDamping = 3f;
            rb.freezeRotation = true;
            Debug.Log("Created Rigidbody2D");
        }

        // Create missing components
        if (movement == null)
        {
            movement = gameObject.AddComponent<PlayerMovement>();
            Debug.Log("Created PlayerMovement");
        }

        if (combat == null)
        {
            combat = gameObject.AddComponent<PlayerCombat>();
            Debug.Log("Created PlayerCombat");
        }

        if (playerAnim == null)
        {
            playerAnim = gameObject.AddComponent<PlayerAnimation>();
            Debug.Log("Created PlayerAnimation");
        }

        if (input == null)
        {
            input = gameObject.AddComponent<PlayerInputHandler>();
            Debug.Log("Created PlayerInputHandler");
        }

        // Subscribe to input events
        if (input != null)
        {
            input.OnMoveInput += HandleMoveInput;
            input.OnAttackInput += HandleAttackInput;
            input.OnDashInput += HandleDashInput;
            Debug.Log("Subscribed to input events");
        }

        Debug.Log("PlayerController initialized");
    }

    private void Update()
    {
        // Update dash state
        if (isDashing && Time.time >= dashEndTime)
        {
            EndDash();
        }

        // Update combat state
        if (combat != null)
        {
            combat.UpdateCombatState();
        }

        // Clear one-time inputs
        InputRegistry.ClearInputs();

        // Update animation
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (playerAnim != null)
        {
            playerAnim.UpdateAnimator(
                moveInput.magnitude > 0.1f,  // isMoving
                combat != null && combat.IsAttacking,  // isAttacking
                isDashing,  // isDashing
                lastMoveDirection  // direction
            );
        }
    }

    private void FixedUpdate()
    {
        if (combat == null || movement == null) return;

        // Apply movement based on state
        if (isDashing)
        {
            // Dash in last direction
            Vector2 dashDir = lastMoveDirection.magnitude > 0.1f ? lastMoveDirection : Vector2.down;
            rb.linearVelocity = dashDir.normalized * dashSpeed;
            Debug.Log($"Dashing: Direction={dashDir}, Velocity={rb.linearVelocity}");
        }
        else if (combat.IsAttacking && freezeMovementOnAttack)
        {
            // Freeze during attack
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            // Normal movement
            if (moveInput.magnitude > 0.1f)
            {
                rb.linearVelocity = moveInput * moveSpeed;
                movement.Move(moveInput); // Update movement component
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                movement.Stop();
            }
        }
    }

    private void HandleMoveInput(Vector2 input)
    {
        Debug.Log($"Move input: {input}");

        moveInput = input;

        if (input.magnitude > 0.1f)
        {
            lastMoveDirection = input;
        }

        // Update InputRegistry
        InputRegistry.RegisterMoveInput(input);

        // Pass to movement component (if not dashing)
        if (movement != null && !isDashing && (!combat.IsAttacking || !freezeMovementOnAttack))
        {
            movement.Move(input);
        }
    }

    private void HandleAttackInput()
    {
        Debug.Log("Attack input");

        if (combat != null && combat.CanAttack() && !isDashing)
        {
            combat.StartAttack(lastMoveDirection);
        }
    }

    private void HandleDashInput()
    {
        Debug.Log("Dash input");

        // Check cooldown
        if (Time.time < nextDashTime)
        {
            Debug.Log($"Dash on cooldown: {nextDashTime - Time.time:F2}s remaining");
            return;
        }

        // Check if already dashing
        if (isDashing)
        {
            Debug.Log("Already dashing!");
            return;
        }

        // Check if attacking and can cancel
        bool isAttacking = combat != null && combat.IsAttacking;
        if (isAttacking && !canDashCancelAttack)
        {
            Debug.Log("Cannot dash while attacking (dash cancel disabled)");
            return;
        }

        // Cancel attack if needed
        if (isAttacking && combat != null)
        {
            combat.CancelAttack();
            Debug.Log("Attack cancelled for dash");
        }

        // Start dash
        StartDash();
    }

    private void StartDash()
    {
        isDashing = true;
        dashEndTime = Time.time + dashDuration;
        nextDashTime = Time.time + dashCooldown;

        Debug.Log($"Dash started! Will last {dashDuration}s");
    }

    private void EndDash()
    {
        isDashing = false;
        rb.linearVelocity *= 0.7f; // Slow down after dash

        Debug.Log("Dash ended");
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (input != null)
        {
            input.OnMoveInput -= HandleMoveInput;
            input.OnAttackInput -= HandleAttackInput;
            input.OnDashInput -= HandleDashInput;
        }
    }
}