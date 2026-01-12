using UnityEngine;
using UnityEngine.InputSystem;
using PoEClone2D.Combat;

namespace PoEClone2D.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private SimplePlayerController playerController;
        [SerializeField] private Animator animator;
        [SerializeField] private MeleeAttack meleeAttack;
        [SerializeField] private AttackData defaultAttack;

        private InputAction attackAction;
        private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");

        // State
        public bool IsCurrentlyAttacking { get; private set; }
        private float lastAttackTime = 0f;
        private bool attackEnabled = true;

        private void Awake()
        {
            playerController = GetComponent<SimplePlayerController>();
            animator = GetComponentInChildren<Animator>();
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
                meleeAttack = attackObj.AddComponent<MeleeAttack>();
            }

            if (defaultAttack != null)
            {
                meleeAttack.Initialize(defaultAttack);
            }

            // Subscribe to attack completion event
            meleeAttack.OnAttackComplete += OnAttackCompleted;
        }

        private void SetupInput()
        {
            attackAction = new InputAction("Attack", InputActionType.Button);
            attackAction.AddBinding("<Mouse>/leftButton");
            attackAction.performed += OnAttackPerformed;
            attackAction.Enable();
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (attackEnabled && meleeAttack.CanAttack() && !IsCurrentlyAttacking)
            {
                StartAttack();
            }
        }

        private void StartAttack()
        {
            Vector2 direction = GetAttackDirection();
            meleeAttack.StartAttack(direction);

            // Set state
            IsCurrentlyAttacking = true;
            lastAttackTime = Time.time;

            // Start animation
            animator.SetBool(IsAttacking, true);

            // Disable movement
            playerController.EnableMovement(false);

            Debug.Log("Attack started");
        }

        private Vector2 GetAttackDirection()
        {
            Vector2 movementInput = playerController.GetMovementInput();
            if (movementInput.magnitude > 0.1f)
                return movementInput.normalized;

            return playerController.IsFacingRight ? Vector2.right : Vector2.left;
        }

        // Called when MeleeAttack completes
        private void OnAttackCompleted()
        {
            EndAttack();
        }

        private void EndAttack()
        {
            if (!IsCurrentlyAttacking) return;

            IsCurrentlyAttacking = false;

            // End animation
            animator.SetBool(IsAttacking, false);

            // Re-enable movement
            playerController.EnableMovement(true);

            Debug.Log("Attack ended - movement restored");
        }

        private void Update()
        {
            // Safety check: If attack should be complete but isn't
            if (IsCurrentlyAttacking && !meleeAttack.IsAttacking())
            {
                Debug.LogWarning("Attack state mismatch! Forcing attack end.");
                EndAttack();
            }

            // Safety check: If animation says attacking but MeleeAttack doesn't
            if (animator.GetBool(IsAttacking) && !IsCurrentlyAttacking)
            {
                Debug.LogWarning("Animation mismatch! Resetting attack animation.");
                animator.SetBool(IsAttacking, false);
            }
        }

        public void CancelAttack()
        {
            meleeAttack?.CancelAttack();
            EndAttack();
        }

        public void SetAttackEnabled(bool enabled)
        {
            attackEnabled = enabled;
        }

        private void OnDestroy()
        {
            attackAction?.Dispose();
            if (meleeAttack != null)
                meleeAttack.OnAttackComplete -= OnAttackCompleted;
        }

        private void OnEnable() => attackAction?.Enable();
        private void OnDisable() => attackAction?.Disable();

        public bool IsPlayerAttacking() => IsCurrentlyAttacking;
    }
}