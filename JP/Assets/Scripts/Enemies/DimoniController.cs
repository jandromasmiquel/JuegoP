using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DimoniController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.8f;
    [SerializeField] private float stopDistance = 0.75f;
    [SerializeField] private float steeringProbeDistance = 1.2f;

    [Header("Attack")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange = 0.9f;
    [SerializeField] private float attackCooldown = 1.0f;

    private Rigidbody2D rb;
    private Health targetHealth;
    private float nextAttackTime;
    private bool isActive = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (target == null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (target != null)
        {
            targetHealth = target.GetComponent<Health>();
        }
    }

    private void FixedUpdate()
    {
        if (!isActive || target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;

        if (distance <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            TryAttack();
            return;
        }

        if (distance <= stopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = ChooseMoveDirection(toTarget.normalized);
        rb.linearVelocity = direction * moveSpeed;
    }

    private Vector2 ChooseMoveDirection(Vector2 desiredDirection)
    {
        if (!Physics2D.Raycast(transform.position, desiredDirection, steeringProbeDistance, obstacleMask))
        {
            return desiredDirection;
        }

        Vector2 left = new Vector2(-desiredDirection.y, desiredDirection.x);
        Vector2 right = new Vector2(desiredDirection.y, -desiredDirection.x);

        bool leftBlocked = Physics2D.Raycast(transform.position, left, steeringProbeDistance, obstacleMask);
        bool rightBlocked = Physics2D.Raycast(transform.position, right, steeringProbeDistance, obstacleMask);

        if (!leftBlocked && rightBlocked)
        {
            return left;
        }

        if (!rightBlocked && leftBlocked)
        {
            return right;
        }

        return Vector2.zero;
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;

        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
        }

        Debug.Log("Dimoni golpea al jugador");
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (!active && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
