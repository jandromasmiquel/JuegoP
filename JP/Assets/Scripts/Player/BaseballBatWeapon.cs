using UnityEngine;
using UnityEngine.InputSystem;

public class BaseballBatWeapon : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private ItemData weaponItem;
    [SerializeField] private LayerMask damageMask;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private float attackRadius = 0.75f;
    [SerializeField] private int damage = 25;
    [SerializeField] private float cooldown = 0.45f;
    [SerializeField] private Animator animator;
    [SerializeField] private string attackTrigger = "Attack";

    private InputAction attackAction;
    private float nextAttackTime;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<PlayerInventory>();
        }

        if (attackOrigin == null)
        {
            attackOrigin = transform;
        }

        attackAction = new InputAction("Baseball Bat Attack", InputActionType.Button, "<Mouse>/rightButton");
        attackAction.performed += OnAttack;
    }

    private void OnEnable()
    {
        attackAction.Enable();
    }

    private void OnDisable()
    {
        attackAction.Disable();
    }

    private void OnDestroy()
    {
        attackAction.performed -= OnAttack;
        attackAction.Dispose();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        TryAttack();
    }

    public void TryAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        if (inventory == null || weaponItem == null || !inventory.HasItem(weaponItem))
        {
            Debug.Log("No tienes el bate equipado/en inventario.");
            return;
        }

        nextAttackTime = Time.time + cooldown;

        if (animator != null && !string.IsNullOrWhiteSpace(attackTrigger))
        {
            animator.SetTrigger(attackTrigger);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackOrigin.position, attackRadius, damageMask);
        foreach (Collider2D hit in hits)
        {
            Health health = hit.GetComponentInParent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }

        Debug.Log("Golpe de bate");
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = attackOrigin != null ? attackOrigin : transform;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin.position, attackRadius);
    }
}
