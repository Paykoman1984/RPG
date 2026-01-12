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

        [Header("Dash Invincibility")]
        [SerializeField] private bool enableInvincibility = true;
        [SerializeField] private float invincibilityDuration = 0.3f;

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
        private Collider2D playerCollider;

        // Input
        private InputAction dashAction;

        // State
        private bool isDashing = false;
        private bool canDash = true;
        private float dashTimer = 0f;
        private float cooldownTimer = 0f;
        private Vector2 dashDirection;
        private bool isInvincible = false;
        private float invincibilityTimer = 0f;

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
            playerCollider = GetComponent<Collider2D>();
            animator = GetComponentInChildren<Animator>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            SetupTrail();
            SetupInput();
        }

        private void SetupTrail()
        {
            if (!enableTrail) return;

            trailRenderer = GetComponent<LineRenderer>();
            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<LineRenderer>();
            }

            trailRenderer.material = new Material(Shader.Find("Sprites/Default"))
            {
                color = Color.white
            };

            trailRenderer.startColor = trailColor;
            trailRenderer.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
            trailRenderer.startWidth = 0.3f;
            trailRenderer.endWidth = 0.1f;
            trailRenderer.positionCount = 0;
            trailRenderer.enabled = false;
            trailRenderer.useWorldSpace = true;
            trailRenderer.sortingLayerName = "Default";
            trailRenderer.sortingOrder = -1;
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

            // Apply invincibility
            if (enableInvincibility)
            {
                isInvincible = true;
                invincibilityTimer = invincibilityDuration;

                // Make player pass through enemies during dash
                if (playerCollider != null)
                {
                    // Store original layer and change to a layer that doesn't collide with enemies
                    StartCoroutine(TemporaryLayerChange());
                }
            }

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

        private System.Collections.IEnumerator TemporaryLayerChange()
        {
            // Change to a layer that doesn't collide with enemies
            int originalLayer = gameObject.layer;
            int dashLayer = LayerMask.NameToLayer("PlayerDash");

            if (dashLayer == -1)
            {
                // Create PlayerDash layer if it doesn't exist
                Debug.LogWarning("PlayerDash layer doesn't exist. Creating it...");
                // You'll need to create this layer in Project Settings -> Tags and Layers
                // For now, we'll use a temporary workaround
                gameObject.layer = LayerMask.NameToLayer("Default");
            }
            else
            {
                gameObject.layer = dashLayer;
            }

            // Wait for dash to complete
            yield return new WaitForSeconds(dashDuration);

            // Restore original layer
            gameObject.layer = originalLayer;
        }

        private void ResetTrail()
        {
            if (!enableTrail) return;

            trailPositions.Clear();
            trailActive = true;

            if (trailRenderer != null)
            {
                trailRenderer.enabled = true;
                trailRenderer.positionCount = 0;
                trailRenderer.startColor = trailColor;
                trailRenderer.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
            }
        }

        private void Update()
        {
            UpdateDash();
            UpdateCooldown();
            UpdateInvincibility();

            if (trailActive)
            {
                UpdateTrail();
            }
        }

        private void FixedUpdate()
        {
            if (isDashing)
            {
                // Check for collisions and adjust dash direction if needed
                Vector2 adjustedDirection = AdjustDashForCollisions(dashDirection);
                rb.linearVelocity = adjustedDirection * dashSpeed;
            }
        }

        private Vector2 AdjustDashForCollisions(Vector2 direction)
        {
            // Simple collision check to prevent getting stuck
            float checkDistance = 0.5f;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, checkDistance,
                LayerMask.GetMask("Wall", "Environment", "Obstacle"));

            if (hit.collider != null)
            {
                // Try to slide along the surface
                Vector2 slideDirection = Vector2.Perpendicular(direction) * Mathf.Sign(Random.Range(-1f, 1f));
                return slideDirection.normalized;
            }

            return direction;
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

        private void UpdateInvincibility()
        {
            if (isInvincible)
            {
                invincibilityTimer -= Time.deltaTime;
                if (invincibilityTimer <= 0)
                {
                    isInvincible = false;
                }

                // Visual feedback for invincibility (optional)
                if (spriteRenderer != null)
                {
                    float alpha = Mathf.PingPong(Time.time * 10f, 0.5f) + 0.5f;
                    spriteRenderer.color = new Color(1, 1, 1, alpha);
                }
            }
            else if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }
        }

        private void UpdateTrail()
        {
            if (!enableTrail || trailRenderer == null) return;

            trailUpdateTimer += Time.deltaTime;

            if (trailUpdateTimer >= trailUpdateInterval)
            {
                trailUpdateTimer = 0f;

                trailPositions.Enqueue(transform.position);

                while (trailPositions.Count > maxTrailPoints)
                {
                    trailPositions.Dequeue();
                }

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

        public bool IsPlayerInvincible()
        {
            return isInvincible;
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
    }
}