using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerInputHandler inputHandler;

    [Header("Settings")]
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashSpeed = 15f;

    // State
    public bool IsDashing { get; private set; }
    public bool IsAttacking => playerCombat != null && playerCombat.IsAttacking;
    private float lastDashTime;
    private Vector2 lastMoveDirection = Vector2.down;

    // Attack input limiting
    private float lastAttackInputTime = 0f;
    private float minimumAttackInputDelay = 0.3f; // 300ms between attack inputs

    // Events
    public event System.Action OnDashStarted;
    public event System.Action OnDashEnded;

    private void Awake()
    {
        Debug.Log("PlayerController Awake - Getting components");
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Get or create Rigidbody2D
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                Debug.Log("Created Rigidbody2D");
            }
        }

        // Get or create PlayerMovement
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
            if (playerMovement == null)
            {
                playerMovement = gameObject.AddComponent<PlayerMovement>();
                Debug.Log("Created PlayerMovement");
            }
        }

        // Get or create PlayerCombat
        if (playerCombat == null)
        {
            playerCombat = GetComponent<PlayerCombat>();
            if (playerCombat == null)
            {
                playerCombat = gameObject.AddComponent<PlayerCombat>();
                Debug.Log("Created PlayerCombat");
            }
        }

        // Get or create PlayerAnimation
        if (playerAnimation == null)
        {
            playerAnimation = GetComponent<PlayerAnimation>();
            if (playerAnimation == null)
            {
                playerAnimation = gameObject.AddComponent<PlayerAnimation>();
                Debug.Log("Created PlayerAnimation");
            }
        }

        // Get or create PlayerInputHandler
        if (inputHandler == null)
        {
            inputHandler = GetComponent<PlayerInputHandler>();
            if (inputHandler == null)
            {
                inputHandler = gameObject.AddComponent<PlayerInputHandler>();
                Debug.Log("Created PlayerInputHandler");
            }
        }

        // Subscribe to input events
        if (inputHandler != null)
        {
            inputHandler.OnMoveInput += HandleMoveInput;
            inputHandler.OnAttackInput += HandleAttackInput;
            inputHandler.OnDashInput += HandleDashInput;
            Debug.Log("Subscribed to input events");
        }

        // Setup PlayerMovement with Rigidbody
        if (playerMovement != null)
        {
            playerMovement.SetRigidbody(rb);
        }

        Debug.Log("PlayerController initialized");
    }

    private void Start()
    {
        // Initialize dash cooldown
        lastDashTime = -dashCooldown;

        // Ensure Rigidbody is properly configured
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
        }
    }

    private void Update()
    {
        // Update combat state
        if (playerCombat != null)
        {
            playerCombat.UpdateCombatState();
        }

        // Update animation
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        // Handle movement in FixedUpdate for physics consistency
        if (playerMovement != null && !IsDashing && !IsAttacking)
        {
            // Get current input
            Vector2 moveInput = inputHandler != null ? inputHandler.GetCurrentInput() : Vector2.zero;

            // Move the player
            playerMovement.Move(moveInput);

            // Update last move direction if moving
            if (moveInput.magnitude > 0.1f)
            {
                lastMoveDirection = moveInput.normalized;
            }
        }
    }

    private void UpdateAnimation()
    {
        if (playerAnimation == null) return;

        // Determine if we're moving
        bool isMoving = false;
        Vector2 animDirection = lastMoveDirection;

        if (inputHandler != null)
        {
            Vector2 currentInput = inputHandler.GetCurrentInput();
            if (currentInput.magnitude > 0.1f)
            {
                isMoving = true;
                animDirection = currentInput.normalized;
            }
            else if (rb != null && rb.linearVelocity.magnitude > 0.1f)
            {
                isMoving = true;
                animDirection = rb.linearVelocity.normalized;
            }
        }

        // Update animation parameters
        playerAnimation.UpdateAnimator(
            isMoving,
            IsAttacking,
            IsDashing,
            animDirection
        );
    }

    // Input Handlers
    private void HandleMoveInput(Vector2 input)
    {
        Debug.Log($"Move input: ({input.x:F2}, {input.y:F2})");

        // Store last move direction when moving
        if (input.magnitude > 0.1f)
        {
            lastMoveDirection = input.normalized;
        }

        // Process movement immediately (for responsive controls)
        if (!IsDashing && !IsAttacking)
        {
            if (playerMovement != null)
            {
                playerMovement.Move(input);
            }
        }
    }

    private void HandleAttackInput()
    {
        if (playerCombat == null || IsDashing) return;

        // Add a minimum time between attack inputs
        float timeSinceLastAttack = Time.time - lastAttackInputTime;
        if (timeSinceLastAttack < minimumAttackInputDelay)
        {
            Debug.Log($"Attack input ignored - too fast: {timeSinceLastAttack:F2}s < {minimumAttackInputDelay:F2}s");
            return;
        }

        if (playerCombat.CanAttack())
        {
            Debug.Log("Attack input");

            // Update last attack input time
            lastAttackInputTime = Time.time;

            // Calculate attack direction
            Vector2 attackDir = GetAttackDirection();
            Debug.Log($"PlayerController: Attacking with direction {attackDir}");

            // Start the attack
            playerCombat.StartAttack(attackDir);
        }
        else
        {
            Debug.Log($"Cannot attack - Combat state: CanAttack={playerCombat.CanAttack()}");
        }
    }

    private Vector2 GetAttackDirection()
    {
        if (inputHandler != null)
        {
            // Use the input handler's attack direction
            return inputHandler.GetAttackDirection();
        }

        // Fallback logic
        Vector2 currentInput = inputHandler != null ? inputHandler.GetCurrentInput() : Vector2.zero;

        if (currentInput.magnitude > 0.1f)
        {
            return currentInput.normalized;
        }

        if (lastMoveDirection.magnitude > 0.1f)
        {
            return lastMoveDirection.normalized;
        }

        return Vector2.down;
    }

    private void HandleDashInput()
    {
        if (CanDash())
        {
            StartDash();
        }
    }

    // Dash Methods
    private bool CanDash()
    {
        return !IsDashing &&
               !IsAttacking &&
               Time.time >= lastDashTime + dashCooldown &&
               inputHandler != null &&
               inputHandler.GetCurrentInput().magnitude > 0.1f;
    }

    private void StartDash()
    {
        IsDashing = true;
        lastDashTime = Time.time;

        Vector2 dashDirection = inputHandler.GetCurrentInput().normalized;

        // Start dash movement
        if (playerMovement != null)
        {
            playerMovement.StartDash(dashDirection, dashSpeed, dashDuration);
        }

        // Auto-end dash after duration
        Invoke(nameof(EndDash), dashDuration);

        OnDashStarted?.Invoke();
        Debug.Log($"Dash started in direction: {dashDirection}");
    }

    private void EndDash()
    {
        IsDashing = false;

        if (playerMovement != null)
        {
            playerMovement.EndDash();
        }

        OnDashEnded?.Invoke();
        Debug.Log("Dash ended");
    }

    // Public Methods
    public Vector2 GetLastMoveDirection()
    {
        return lastMoveDirection;
    }

    public void SetDashParameters(float cooldown, float duration, float speed)
    {
        dashCooldown = cooldown;
        dashDuration = duration;
        dashSpeed = speed;
    }

    // Cleanup
    private void OnDestroy()
    {
        if (inputHandler != null)
        {
            inputHandler.OnMoveInput -= HandleMoveInput;
            inputHandler.OnAttackInput -= HandleAttackInput;
            inputHandler.OnDashInput -= HandleDashInput;
        }
    }
}