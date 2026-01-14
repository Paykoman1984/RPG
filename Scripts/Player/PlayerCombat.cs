using UnityEngine;
using UnityEngine.InputSystem;
using PoEClone2D.Combat;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace PoEClone2D.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private SimplePlayerController playerController;
        [SerializeField] private Animator animator;
        [SerializeField] private MeleeAttack meleeAttack;
        [SerializeField] private AttackData defaultAttack;
        [SerializeField] private AnimationEventBridge animationEventBridge;

        // Attack buffering
        [Header("Attack Buffering")]
        [SerializeField] private float attackBufferTime = 0.3f; // 300ms buffer
        private float attackBufferTimer = 0f;

        private InputAction attackAction;
        private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");

        public bool IsCurrentlyAttacking { get; private set; }
        private bool attackEnabled = true;
        private bool isInitialized = false;

        // Track attack start time for safety
        private float attackStartTime = 0f;

        // Cache for UI checking
        private Mouse currentMouse;
        private List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

        private void Awake()
        {
            playerController = GetComponent<SimplePlayerController>();
            animator = GetComponentInChildren<Animator>();

            // Get or create MeleeAttack
            meleeAttack = GetComponentInChildren<MeleeAttack>();
            if (meleeAttack == null)
            {
                GameObject attackObj = new GameObject("MeleeAttack");
                attackObj.transform.SetParent(transform);
                attackObj.transform.localPosition = Vector3.zero;
                meleeAttack = attackObj.AddComponent<MeleeAttack>();
                //Debug.Log("PlayerCombat: Created MeleeAttack component");
            }

            // Get or create AnimationEventBridge
            animationEventBridge = GetComponentInChildren<AnimationEventBridge>();
            if (animationEventBridge == null)
            {
                GameObject bridgeObj = new GameObject("AnimationEventBridge");
                bridgeObj.transform.SetParent(transform);
                bridgeObj.transform.localPosition = Vector3.zero;
                animationEventBridge = bridgeObj.AddComponent<AnimationEventBridge>();
                Debug.Log("PlayerCombat: Created AnimationEventBridge component");
            }

            SetupInput();
            currentMouse = Mouse.current;
        }

        private void Start()
        {
            // Subscribe to inventory state changes
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnInventoryStateChanged.AddListener(OnInventoryStateChanged);
                //Debug.Log("PlayerCombat: Subscribed to GameStateManager events");
            }
            else
            {
                //Debug.LogWarning("PlayerCombat: GameStateManager not found!");
            }

            // Ensure everything is properly initialized
            StartCoroutine(DelayedInitialization());
        }

        private System.Collections.IEnumerator DelayedInitialization()
        {
            // Wait for 2 frames to ensure all components are ready
            yield return null;
            yield return null;

            // Initialize with default attack data
            if (defaultAttack != null && meleeAttack != null)
            {
                meleeAttack.Initialize(defaultAttack);
                //Debug.Log($"PlayerCombat: Initialized MeleeAttack with {defaultAttack.name}");
            }
            else if (defaultAttack == null)
            {
                //Debug.LogError("PlayerCombat: Default AttackData is not assigned!");
            }

            ResetAnimationState();
            isInitialized = true;
            //Debug.Log("PlayerCombat: Fully initialized and ready");
        }

        private void ResetAnimationState()
        {
            if (animator != null)
            {
                animator.SetBool(IsAttacking, false);
                //Debug.Log("PlayerCombat: Animation state reset to Idle");
            }
        }

        private void SetupInput()
        {
            attackAction = new InputAction("Attack", InputActionType.Button);
            attackAction.AddBinding("<Mouse>/leftButton");
            attackAction.AddBinding("<Keyboard>/space");
            attackAction.performed += OnAttackPerformed;
            attackAction.Enable();
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (!isInitialized)
            {
                //Debug.LogWarning("PlayerCombat: Not initialized yet! Ignoring attack.");
                return;
            }

            // Check if inventory is open via GameStateManager
            if (GameStateManager.Instance != null && GameStateManager.Instance.IsInventoryOpen())
            {
                //Debug.Log("PlayerCombat: Inventory is open, ignoring attack");
                return;
            }

            // Check if clicking on UI using manual raycast
            if (IsPointerOverUI())
            {
                //Debug.Log("PlayerCombat: Clicking on UI, ignoring attack");
                return;
            }

            // Check if we can attack immediately
            if (attackEnabled && meleeAttack != null && meleeAttack.CanAttack() && !IsCurrentlyAttacking)
            {
                //Debug.Log($"PlayerCombat: Attack conditions met - immediate attack");
                StartAttack();
            }
            else if (attackEnabled) // Can't attack now, buffer it
            {
                //Debug.Log($"PlayerCombat: Can't attack now, buffering for {attackBufferTime}s");
                attackBufferTimer = attackBufferTime;
            }
        }

        private bool IsPointerOverUI()
        {
            // Manual raycast to check UI at the exact moment of click
            if (currentMouse == null || EventSystem.current == null)
                return false;

            Vector2 mousePos = currentMouse.position.ReadValue();

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = mousePos,
                pointerId = -1
            };

            uiRaycastResults.Clear();
            EventSystem.current.RaycastAll(pointerData, uiRaycastResults);

            // Check if any UI element was hit
            foreach (var result in uiRaycastResults)
            {
                if (result.gameObject.GetComponent<RectTransform>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void StartAttack()
        {
            //Debug.Log("=== STARTING ATTACK ===");

            Vector2 attackDirection = GetAttackDirection();

            if (meleeAttack != null)
            {
                meleeAttack.StartAttack(attackDirection);
            }
            else
            {
                //Debug.LogError("PlayerCombat: MeleeAttack is null!");
                return;
            }

            IsCurrentlyAttacking = true;
            attackStartTime = Time.time;
            attackBufferTimer = 0f; // Clear any buffered attack

            // Set animation state
            if (animator != null)
            {
                animator.SetBool(IsAttacking, true);
                //Debug.Log($"PlayerCombat: Set IsAttacking to true");
            }

            // Disable movement during attack
            if (playerController != null)
            {
                playerController.EnableMovement(false);
            }

            //Debug.Log("PlayerCombat: Attack animation started");
        }

        private Vector2 GetAttackDirection()
        {
            if (playerController != null)
            {
                Vector2 movementInput = playerController.GetMovementInput();
                if (movementInput.magnitude > 0.1f)
                    return movementInput.normalized;

                return playerController.IsFacingRight ? Vector2.right : Vector2.left;
            }

            return Vector2.right; // Default direction
        }

        private void Update()
        {
            // Handle attack buffering
            if (attackBufferTimer > 0)
            {
                attackBufferTimer -= Time.deltaTime;

                // If buffer is active and we can attack now, do it
                if (attackBufferTimer > 0 && attackEnabled && meleeAttack != null &&
                    meleeAttack.CanAttack() && !IsCurrentlyAttacking)
                {
                    //Debug.Log("PlayerCombat: Executing buffered attack");
                    attackBufferTimer = 0f;
                    StartAttack();
                }
            }

            // Emergency recovery if attack gets stuck
            if (IsCurrentlyAttacking)
            {
                float attackDuration = Time.time - attackStartTime;

                // If attack has been going on for too long, force end
                if (attackDuration > 2.0f) // 2 seconds max
                {
                    //Debug.LogWarning($"PlayerCombat: Attack stuck for {attackDuration:F1}s! Forcing end.");
                    EndAttack();
                }

                // Check if animation is stuck
                if (animator != null)
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

                    // Check if we're in attack animation
                    bool isInAttackAnim = stateInfo.IsName("Player_Attack") ||
                                         stateInfo.IsName("Attack") ||
                                         animator.GetBool(IsAttacking);

                    // If animation finished but we're still in attack state
                    if (isInAttackAnim && stateInfo.normalizedTime >= 1.0f)
                    {
                        float extraTime = attackDuration - stateInfo.length;
                        if (extraTime > 0.3f) // 0.3 seconds after animation should end
                        {
                            //Debug.LogWarning($"PlayerCombat: Attack animation finished {extraTime:F1}s ago but still active. Ending.");
                            EndAttack();
                        }
                    }
                }
            }
        }

        // ===== EVENT HANDLER =====
        private void OnInventoryStateChanged(bool isInventoryOpen)
        {
            // Disable/enable attacks based on inventory state
            SetAttackEnabled(!isInventoryOpen);

            //Debug.Log($"PlayerCombat: Inventory state changed - Attacks {(isInventoryOpen ? "disabled" : "enabled")}");

            // Also disable movement when inventory is open
            if (playerController != null)
            {
                playerController.EnableMovement(!isInventoryOpen);
            }

            // If inventory opens during an attack, cancel it
            if (isInventoryOpen && IsCurrentlyAttacking)
            {
                //Debug.Log("PlayerCombat: Inventory opened during attack - cancelling attack");
                CancelAttack();
            }
        }

        // ===== SOUND METHODS =====
        public void PlayAttackHitSound(Vector3 hitPosition)
        {
            if (defaultAttack != null && defaultAttack.HitSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    defaultAttack.HitSound,
                    hitPosition,
                    defaultAttack.HitVolume * (defaultAttack.RandomizePitch ?
                        Random.Range(defaultAttack.MinPitch, defaultAttack.MaxPitch) : 1f)
                );
                //Debug.Log("PlayerCombat: Playing HitSound on impact");
            }
        }

        // Called by AnimationEventBridge at end of attack
        public void OnAttackCompleted()
        {
            //Debug.Log("PlayerCombat: Attack animation completed via AnimationEventBridge");
            EndAttack();
        }

        private void EndAttack()
        {
            if (!IsCurrentlyAttacking)
            {
                // Already ended, ignore
                return;
            }

            //Debug.Log("=== ENDING ATTACK ===");

            IsCurrentlyAttacking = false;

            // Reset animation
            if (animator != null)
            {
                animator.SetBool(IsAttacking, false);
                //Debug.Log($"PlayerCombat: Set IsAttacking to false");
            }

            // Re-enable movement
            if (playerController != null)
            {
                playerController.EnableMovement(true);
            }

            // Complete attack in melee system
            if (meleeAttack != null)
            {
                meleeAttack.CompleteAttack();
            }

            //Debug.Log("PlayerCombat: Attack fully ended");
        }

        public void CancelAttack()
        {
            meleeAttack?.CancelAttack();
            EndAttack();
        }

        public void SetAttackEnabled(bool enabled)
        {
            attackEnabled = enabled;

            // If disabling during an attack, cancel it
            if (!enabled && IsCurrentlyAttacking)
            {
                CancelAttack();
            }
        }

        private void OnDestroy()
        {
            attackAction?.Dispose();

            // Unsubscribe from events
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnInventoryStateChanged.RemoveListener(OnInventoryStateChanged);
                //Debug.Log("PlayerCombat: Unsubscribed from GameStateManager events");
            }
        }

        private void OnEnable() => attackAction?.Enable();
        private void OnDisable() => attackAction?.Disable();

        public bool IsPlayerAttacking() => IsCurrentlyAttacking;
    }
}