using UnityEngine;
using UnityEngine.InputSystem;

public class BaseballBatWeapon : MonoBehaviour
{
    [Header("Configuración Automática (Desde ScriptableObject)")]
    private WeaponItemData weaponData;
    
    [Header("Referencias de la Escena/Prefab")]
    [SerializeField] private LayerMask damageMask;
    [SerializeField] private Transform attackOrigin;
    
    private Animator playerAnimator;
    private InputAction attackAction;
    private float nextAttackTime;

    private void Awake()
    {
        if (attackOrigin == null) attackOrigin = transform;

        // Configuramos la acción de ataque (te recomiendo cambiar a LeftButton para atacar y dejar el Right para interacciones)
        attackAction = new InputAction("AttackAction", InputActionType.Button, "<Mouse>/leftButton");
        attackAction.performed += _ => TryAttack();
    }

    public void Initialize(WeaponItemData data)
    {
        weaponData = data;
        // Buscamos el Animator en el objeto padre (el Jugador)
        playerAnimator = GetComponentInParent<Animator>();
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
        attackAction.Dispose();
    }

    public void TryAttack()
    {
        // Seguridad por si no se ha inicializado
        if (weaponData == null || Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + weaponData.Cooldown;

        // Lanzamos la animación en el jugador
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Attack"); // Asegúrate de que el parámetro en tu Animator se llame igual
        }

        // Calculamos los impactos usando los datos del ScriptableObject
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackOrigin.position, weaponData.AttackRadius, damageMask);
        foreach (Collider2D hit in hits)
        {
            Health health = hit.GetComponentInParent<Health>();
            if (health != null)
            {
                health.TakeDamage(weaponData.Damage);
            }
        }

        Debug.Log($"Golpe con {weaponData.DisplayName} haciendo {weaponData.Damage} de daño.");
    }

    private void OnDrawGizmosSelected()
    {
        if (weaponData == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin.position, weaponData.AttackRadius);
    }
}