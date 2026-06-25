using UnityEngine;

[CreateAssetMenu(menuName = "JuegoP/Inventory/Weapon Data")]
public class WeaponItemData : ItemData, IItemEquipable
{
    [Header("Estadísticas de Combate")]
    [SerializeField] private int damage = 25;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private float attackRadius = 0.75f;
    [SerializeField] private LayerMask damageMask;

    private float nextAttackTime;

    public int Damage => damage;
    public float Cooldown => cooldown;
    public float AttackRadius => attackRadius;

    // BUG 2 FIX: Los ScriptableObjects persisten sus valores entre sesiones en el Editor.
    // Si en una sesión anterior se atacó a t=30s, nextAttackTime queda como 31.
    // La siguiente vez que se entra en Play Mode, Time.time=0 pero nextAttackTime=31,
    // haciendo imposible atacar. OnEnable() resetea el cooldown al entrar en Play Mode.
    private void OnEnable()
    {
        nextAttackTime = 0f;
    }

    // ¡Aquí está la magia! El ScriptableObject ejecuta el ataque directamente
    public bool EnUsar(PlayerController player, Transform origenUso)
    {
        if (Time.time < nextAttackTime) return false;

        nextAttackTime = Time.time + cooldown;

        // Ejecutamos el barrido físico desde el punto que nos pase el jugador (su torso/mano)
        Collider2D[] hits = Physics2D.OverlapCircleAll(origenUso.position, attackRadius, damageMask);
        foreach (Collider2D hit in hits)
        {
            Health health = hit.GetComponentInParent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }

        Debug.Log($"[Datos] Golpe virtual con {DisplayName} haciendo {damage} de daño.");
        return true;
    }
}