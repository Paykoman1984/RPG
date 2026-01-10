using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace PoEClone2D.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SimplePlayerController), typeof(PlayerCombat))]
    public class PlayerDash : MonoBehaviour
    {
        [Header("Dash Settings")]
        [SerializeField] private float dashSpeed = 50f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 0.4f;
        [SerializeField] private bool canDashDuringAttack = true;
        [SerializeField] private bool cancelAttackOnDash = true;

        [Header("Trail Settings")]
        [SerializeField] private bool enableTrail = true;
        [SerializeField] private Color trailColor = Color.white;
        [SerializeField] private float trailUpdateInterval = 0.02f;
        [SerializeField] private int maxTrailPoints = 10;

        // Components
        private Rigidbody2D rb;
        private SimplePlayerController playerController;
        private PlayerCombat playerCombat;
        private SpriteRenderer spriteRenderer;

        // Input
        private InputAction dashAction;

        // State
        private bool isDashing = false;
        private bool canDash = true;
        private float dashTimer = 0f;
        private float cooldownTimer = 0f;
        private Vector2 dashDirection;

        // Trail
        private LineRenderer trailRenderer;
        private Queue<Vector3> trailPositions = new Queue<Vector3>();
        private float trailUpdateTimer = 0f;
        private bool trailActive = false;

        // Animation
        private Animator animator;
        private static readonly int DashAnimParam = Animator.StringToHash("Dashing");

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerController = GetComponent<SimplePlayerController>();
            playerCombat = GetComponent<PlayerCombat>();
            animator = GetComponentInChildren<Animator>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            SetupTrail();
            SetupInput();
        }

        private void SetupTrail()
        {
            if (!enableTrail) return;

            // Check if LineRenderer already exists
            trailRenderer = GetComponent<LineRenderer>();
            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<LineRenderer>();
            }

            // Simple material setup
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"))
            {
                color = Color.white
            };

            // Trail appearance
            trailRenderer.startColor = trailColor;
            trailRenderer.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
            trailRenderer.startWidth = 0.3f;
            trailRenderer.endWidth = 0.1f;
            trailRenderer.positionCount = 0;
            trailRenderer.enabled = false;
            trailRenderer.useWorldSpace = true;

            // Ensure it's visible in 2D
            trailRenderer.sortingLayerName = "Default";
            trailRenderer.sortingOrder = -1; // Behind player
        }

        private void SetupInput()
        {
            dashAction = new InputAction("Dash", InputActionType.Button);
            dashAction.AddBinding("<Keyboard>/leftCtrl");
            dashAction.AddBinding("<Keyboard>/rightCtrl");

            dashAction.performed += OnDashPerformed;
            dashAction.Enable();
        }

        private void OnDashPerformed(InputAction.CallbackContext context)
        {
            if (canDash && (!playerCombat.IsPlayerAttacking() || canDashDuringAttack))
            {
                StartDash();
            }
        }

        private void StartDash()
        {
            // Get dash direction
            Vector2 movementInput = playerController.GetMovementInput();
            if (movementInput.magnitude > 0.1f)
            {
                dashDirection = movementInput.normalized;
            }
            else
            {
                dashDirection = playerController.IsFacingRight ? Vector2.right : Vector2.left;
            }

            // Cancel attack if needed
            if (cancelAttackOnDash && playerCombat.IsPlayerAttacking())
            {
                playerCombat.CancelAttack();
            }

            // Set state
            isDashing = true;
            canDash = false;
            dashTimer = 0f;
            cooldownTimer = dashCooldown;
            trailUpdateTimer = 0f;

            // Reset trail
            ResetTrail();

            // Disable movement control
            playerController.EnableMovement(false);

            // Set animation
            if (animator != null)
            {
                animator.SetBool(DashAnimParam, true);
            }

            Debug.Log($"Dash started: {dashDirection}");
        }

        private void ResetTrail()
        {
            if (!enableTrail) return;

            // Clear all trail positions
            trailPositions.Clear();
            trailActive = true;

            // Re-enable and reset LineRenderer
            if (trailRenderer != null)
            {
                trailRenderer.enabled = true;
                trailRenderer.positionCount = 0;

                // Reset colors (important!)
                trailRenderer.startColor = trailColor;
                trailRenderer.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
            }
        }

        private void Update()
        {
            UpdateDash();
            UpdateCooldown();

            if (trailActive)
            {
                UpdateTrail();
            }
        }

        private void FixedUpdate()
        {
            if (isDashing)
            {
                rb.linearVelocity = dashDirection * dashSpeed;
            }
        }

        private void UpdateDash()
        {
            if (!isDashing) return;

            dashTimer += Time.deltaTime;

            if (dashTimer >= dashDuration)
            {
                EndDash();
            }
        }

        private void UpdateTrail()
        {
            if (!enableTrail || trailRenderer == null) return;

            trailUpdateTimer += Time.deltaTime;

            // Add new trail point at intervals
            if (trailUpdateTimer >= trailUpdateInterval)
            {
                trailUpdateTimer = 0f;

                // Add current position to trail
                trailPositions.Enqueue(transform.position);

                // Limit number of trail points
                while (trailPositions.Count > maxTrailPoints)
                {
                    trailPositions.Dequeue();
                }

                // Update LineRenderer
                UpdateTrailRenderer();
            }
        }

        private void UpdateTrailRenderer()
        {
            if (trailRenderer == null) return;

            trailRenderer.positionCount = trailPositions.Count;
            int index = 0;
            foreach (Vector3 position in trailPositions)
            {
                trailRenderer.SetPosition(index, position);
                index++;
            }
        }

        private void UpdateCooldown()
        {
            if (!canDash && !isDashing)
            {
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0f)
                {
                    canDash = true;
                }
            }
        }

        private void EndDash()
        {
            isDashing = false;

            // Stop movement
            rb.linearVelocity = Vector2.zero;

            // Re-enable movement
            playerController.EnableMovement(true);

            // Reset animation
            if (animator != null)
            {
                animator.SetBool(DashAnimParam, false);
            }

            // Start fading out trail
            if (enableTrail)
            {
                StartCoroutine(FadeOutTrail());
            }

            Debug.Log("Dash ended");
        }

        private System.Collections.IEnumerator FadeOutTrail()
        {
            if (trailRenderer == null) yield break;

            float fadeTime = 0.2f;
            float timer = 0f;

            Color startColor = trailRenderer.startColor;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (timer < fadeTime && trailRenderer != null)
            {
                timer += Time.deltaTime;
                float t = timer / fadeTime;

                Color currentColor = Color.Lerp(startColor, endColor, t);
                trailRenderer.startColor = currentColor;
                trailRenderer.endColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);

                yield return null;
            }

            // Completely reset trail state
            trailActive = false;
            if (trailRenderer != null)
            {
                trailRenderer.enabled = false;
                trailRenderer.positionCount = 0;
            }
            trailPositions.Clear();
        }

        public bool IsPlayerDashing()
        {
            return isDashing;
        }

        private void OnDestroy()
        {
            dashAction?.Dispose();
        }

        private void OnEnable()
        {
            dashAction?.Enable();
        }

        private void OnDisable()
        {
            dashAction?.Disable();
            if (isDashing)
            {
                EndDash();
            }
        }

        // Debug GUI
        private void OnGUI()
        {
            GUI.Label(new Rect(10, 250, 300, 20), $"Dash: {isDashing}");
            GUI.Label(new Rect(10, 270, 300, 20), $"Trail Active: {trailActive}");
            GUI.Label(new Rect(10, 290, 300, 20), $"Trail Points: {trailPositions.Count}");
            GUI.Label(new Rect(10, 310, 300, 20), $"Cooldown: {cooldownTimer:F2}");
        }
    }
}