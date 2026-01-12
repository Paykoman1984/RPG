using UnityEngine;
using UnityEngine.InputSystem;

namespace PoEClone2D.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SimplePlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        // Input
        private InputAction moveAction;
        private Vector2 movementInput;

        // Components
        private Rigidbody2D rb;
        private PlayerCombat playerCombat;

        // Animation Parameters
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");

        // State
        private bool isMovementEnabled = true;
        private bool isFacingRight = true;

        public Vector2 MovementInput => movementInput;
        public bool IsFacingRight => isFacingRight;
        public bool IsMovementEnabled => isMovementEnabled;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerCombat = GetComponent<PlayerCombat>();

            // Configure Rigidbody to prevent unwanted collisions
            ConfigureRigidbody();

            // Get references if not set
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            SetupInput();
        }

        private void ConfigureRigidbody()
        {
            // Set up rigidbody for better control
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.freezeRotation = true;

            // Use a lower gravity scale for less downward force
            rb.gravityScale = 0f;
        }

        private void SetupInput()
        {
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            moveAction.Enable();
        }

        private void Update()
        {
            movementInput = moveAction.ReadValue<Vector2>();

            // Normalize diagonal movement
            if (movementInput.magnitude > 1f)
                movementInput.Normalize();

            // Handle flipping based on X input
            if (Mathf.Abs(movementInput.x) > 0.1f)
            {
                // Update facing direction
                bool newFacingRight = movementInput.x > 0;
                if (newFacingRight != isFacingRight)
                {
                    isFacingRight = newFacingRight;
                    UpdateSpriteFlip();
                }
            }

            // Update animation based on movement
            UpdateAnimations();
        }

        // In FixedUpdate method, add this check to prevent sliding into enemies:
        private void FixedUpdate()
        {
            // Check if we're dashing - if so, don't apply normal movement
            PlayerDash playerDash = GetComponent<PlayerDash>();
            if (playerDash != null && playerDash.IsPlayerDashing())
            {
                return; // Dash handles movement
            }

            // Only move if movement is enabled
            if (isMovementEnabled && movementInput.magnitude > 0.1f)
            {
                // Check if we're colliding with an enemy ahead
                Vector2 moveDirection = movementInput.normalized;
                float checkDistance = 0.3f; // Small distance to check ahead

                RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, checkDistance,
                    LayerMask.GetMask("Enemy"));

                if (hit.collider == null)
                {
                    rb.linearVelocity = movementInput * moveSpeed;
                }
                else
                {
                    // Stop movement if about to hit enemy
                    rb.linearVelocity = Vector2.zero;
                }
            }
            else if (!isMovementEnabled || movementInput.magnitude <= 0.1f)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        private void UpdateAnimations()
        {
            if (animator != null)
            {
                bool isMoving = isMovementEnabled && movementInput.magnitude > 0.1f;
                animator.SetBool(IsRunning, isMoving);
            }
        }

        private void UpdateSpriteFlip()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !isFacingRight;
            }
        }

        // Public methods
        public void EnableMovement(bool enable)
        {
            isMovementEnabled = enable;
            if (!enable)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        public Vector2 GetMovementInput()
        {
            return movementInput;
        }

        public void SetFacingRight(bool faceRight)
        {
            if (isFacingRight != faceRight)
            {
                isFacingRight = faceRight;
                UpdateSpriteFlip();
            }
        }

        // Check if player is attacking (using the public method from PlayerCombat)
        public bool IsPlayerAttacking()
        {
            return playerCombat != null && playerCombat.IsPlayerAttacking();
        }

        private void OnDestroy()
        {
            moveAction?.Dispose();
        }

        private void OnEnable()
        {
            moveAction?.Enable();
        }

        private void OnDisable()
        {
            moveAction?.Disable();
            rb.linearVelocity = Vector2.zero;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            // Prevent player from getting stuck on enemies
            if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                // Slight push away from enemy to prevent sticking
                Vector2 pushDirection = (transform.position - collision.transform.position).normalized * 0.1f;
                rb.position += pushDirection;
            }
        }
    }
}