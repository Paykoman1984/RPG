// Assets/Scripts/Enemy/EnemyController.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(HurtBox))]
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public float detectionRange = 5f;

    [Header("Combat")]
    public GameObject attackHitboxPrefab;
    public float attackDamage = 15f;

    [Header("Loot")]
    public GameObject[] lootPrefabs;
    public float lootDropChance = 0.3f;

    private Transform player;
    private HurtBox hurtbox;
    private Rigidbody2D rb;
    private Animator animator;
    private float lastAttackTime;
    private bool isDead = false;

    private void Awake()
    {
        hurtbox = GetComponent<HurtBox>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Subscribe to death event - FIXED SIGNATURE
        CombatEvents.OnEntityDied += OnEntityDied;

        // Also subscribe to HurtBox's death event directly
        if (hurtbox != null)
        {
            hurtbox.OnDeath += OnHurtboxDeath;
        }
    }

    private void Start()
    {
        // Initialize last attack time
        lastAttackTime = Time.time - attackCooldown;
    }

    private void Update()
    {
        if (isDead || hurtbox == null || !hurtbox.IsAlive()) return; // FIXED: Added parentheses

        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectionRange)
            {
                // Face player
                Vector2 direction = (player.position - transform.position).normalized;
                UpdateDirection(direction);

                if (distanceToPlayer <= attackRange)
                {
                    // Attack
                    if (Time.time - lastAttackTime >= attackCooldown)
                    {
                        Attack();
                        lastAttackTime = Time.time;
                    }

                    // Stop moving when in attack range
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector2.zero;
                    }

                    if (animator != null)
                    {
                        animator.SetBool("IsMoving", false);
                    }
                }
                else
                {
                    // Move toward player
                    if (rb != null)
                    {
                        rb.linearVelocity = direction * moveSpeed;
                    }

                    if (animator != null)
                    {
                        animator.SetBool("IsMoving", true);
                    }
                }
            }
            else
            {
                // Idle
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
                if (animator != null)
                {
                    animator.SetBool("IsMoving", false);
                }
            }
        }
    }

    private void UpdateDirection(Vector2 direction)
    {
        // Flip sprite based on direction
        if (direction.x != 0)
        {
            transform.localScale = new Vector3(
                Mathf.Sign(direction.x) * Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }

        // Update animator parameters for 4-direction animation
        if (animator != null)
        {
            Vector2 animDir = Get4WayDirection(direction);
            animator.SetFloat("MoveX", animDir.x);
            animator.SetFloat("MoveY", animDir.y);
        }
    }

    private Vector2 Get4WayDirection(Vector2 input)
    {
        if (input.magnitude < 0.1f) return Vector2.down;

        float absX = Mathf.Abs(input.x);
        float absY = Mathf.Abs(input.y);

        if (absX > absY)
            return new Vector2(Mathf.Sign(input.x), 0);
        else
            return new Vector2(0, Mathf.Sign(input.y));
    }

    public void Attack()
    {
        if (attackHitboxPrefab == null || player == null) return;

        // Create attack hitbox
        Vector2 attackPosition = (Vector2)transform.position +
            ((Vector2)(player.position - transform.position).normalized * attackRange * 0.5f);

        GameObject hitboxObj = Instantiate(attackHitboxPrefab, attackPosition, Quaternion.identity);
        Hitbox hitbox = hitboxObj.GetComponent<Hitbox>();

        if (hitbox != null)
        {
            hitbox.damage = attackDamage;
            hitbox.owner = gameObject;
            hitbox.targetLayers = LayerMask.GetMask("Player", "PlayerHurtbox");
            hitbox.Activate(gameObject);
        }

        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    // FIXED: Now matches the EntityDeathEvent delegate signature
    private void OnEntityDied(GameObject entity)
    {
        if (entity == gameObject || (hurtbox != null && entity == hurtbox.owner))
        {
            HandleDeath();
        }
    }

    // New: Direct event handler for HurtBox death
    private void OnHurtboxDeath()
    {
        HandleDeath();
    }

    private void HandleDeath()
    {
        if (isDead) return;

        isDead = true;

        // Stop moving
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Play death animation
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }

        // Drop loot
        DropLoot();

        // Disable enemy behavior
        enabled = false;

        // Optionally destroy after delay
        Destroy(gameObject, 2f);
    }

    private void DropLoot()
    {
        if (lootPrefabs.Length == 0 || Random.value > lootDropChance) return;

        GameObject loot = lootPrefabs[Random.Range(0, lootPrefabs.Length)];
        if (loot != null)
        {
            Instantiate(loot, transform.position, Quaternion.identity);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        CombatEvents.OnEntityDied -= OnEntityDied;

        if (hurtbox != null)
        {
            hurtbox.OnDeath -= OnHurtboxDeath;
        }
    }

    // Optional: Visualize detection and attack ranges in Scene view
    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}