using UnityEngine;
using UnityEngine.InputSystem;
using PoEClone2D.Combat;

namespace PoEClone2D.Player
{
    [RequireComponent(typeof(SimplePlayerController))]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SimplePlayerController playerController;
        [SerializeField] private Animator animator;
        [SerializeField] private MeleeAttack meleeAttack;

        [Header("Attack Settings")]
        [SerializeField] private AttackData defaultAttack;

        // Input
        private InputAction attackAction;

        // Animation Parameters
        private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");

        // State
        public bool IsCurrentlyAttacking { get; private set; }
        private float lastAttackTime = 0f;
        private bool attackEnabled = true;

        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<SimplePlayerController>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (meleeAttack == null)
                meleeAttack = GetComponentInChildren<MeleeAttack>();

            InitializeAttack();
            SetupInput();
        }

        private void InitializeAttack()
        {
            if (meleeAttack == null)
            {
                GameObject attackObj = new GameObject("MeleeAttack");
                attackObj.transform.SetParent(transform);
                attackObj.transform.localPosition = Vector3.zero;
                meleeAttack = attackObj.AddComponent<MeleeAttack>();
            }

            if (defaultAttack != null)
            {
                meleeAttack.Initialize(defaultAttack);
            }
        }

        private void SetupInput()
        {
            // Attack input - LEFT MOUSE BUTTON ONLY
            attackAction = new InputAction("Attack", InputActionType.Button);
            attackAction.AddBinding("<Mouse>/leftButton");
            // Removed space bar binding

            attackAction.performed += OnAttackPerformed;
            attackAction.Enable();
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            // Check if we can attack (not already attacking, cooldown passed, and attack is enabled)
            if (attackEnabled && !IsCurrentlyAttacking && Time.time >= lastAttackTime + defaultAttack.AttackInterval)
            {
                StartAttack();
            }
        }

        private void StartAttack()
        {
            // Get attack direction
            Vector2 attackDirection = GetAttackDirection();

            // Start the attack
            meleeAttack.StartAttack(attackDirection);

            // Set state
            IsCurrentlyAttacking = true;
            lastAttackTime = Time.time;

            // Update animator
            animator.SetBool(IsAttacking, true);

            // Disable movement during attack
            playerController.EnableMovement(false);
        }

        private Vector2 GetAttackDirection()
        {
            // Use movement input if moving
            Vector2 movementInput = playerController.GetMovementInput();
            if (movementInput.magnitude > 0.1f)
            {
                return movementInput.normalized;
            }

            // Otherwise use facing direction
            return playerController.IsFacingRight ? Vector2.right : Vector2.left;
        }

        private void Update()
        {
            // Check if attack animation should end
            if (IsCurrentlyAttacking && !meleeAttack.IsAttacking())
            {
                EndAttack();
            }
        }

        private void EndAttack()
        {
            IsCurrentlyAttacking = false;

            // Update animator
            animator.SetBool(IsAttacking, false);

            // Re-enable movement
            playerController.EnableMovement(true);
        }

        // Public method to cancel attack (for dash)
        public void CancelAttack()
        {
            if (IsCurrentlyAttacking)
            {
                // Cancel the melee attack
                meleeAttack.CancelAttack();

                // End attack state
                EndAttack();
            }
        }

        // Enable/disable attacking
        public void SetAttackEnabled(bool enabled)
        {
            attackEnabled = enabled;
        }

        private void OnDestroy()
        {
            attackAction?.Dispose();
        }

        private void OnEnable()
        {
            attackAction?.Enable();
        }

        private void OnDisable()
        {
            attackAction?.Disable();
        }

        // Public getter for other components
        public bool IsPlayerAttacking()
        {
            return IsCurrentlyAttacking;
        }
    }
}