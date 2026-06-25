using UnityEngine;

[CreateAssetMenu(fileName = "NuevoBate", menuName = "Inventario/Items/Bate")]
public class BateItemData : ItemData, IItemEquipable
{
    [Header("Estadísticas de Combate")]
    [SerializeField] private int damage = 25;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private float attackRadius = 0.75f;
    [SerializeField] private LayerMask damageMask;

    [Header("Audio IDs (Registrados en AudioManager)")]
    [SerializeField] private string swingAudioID = "bate_swing";
    [SerializeField] private string hitAudioID = "bate_impacto";

    private float nextAttackTime;

    public int Damage => damage;
    public float Cooldown => cooldown;
    public float AttackRadius => attackRadius;

    private void OnEnable()
    {
        nextAttackTime = 0f;
    }

    public bool EnUsar(PlayerController player, Transform origenUso)
    {
        if (Time.time < nextAttackTime) return false;

        nextAttackTime = Time.time + cooldown;

        // 1. REPRODUCIR SWING: Siempre que atacas, suena el aire del bate.
        // Usamos PlaySFX3D en la posición del jugador para que se escuche espacial.
        AudioManager.Instance.PlaySFX3D(swingAudioID, origenUso.position);

        bool haGolpeadoAlgo = false;

        // Ejecutamos el barrido físico
        Collider2D[] hits = Physics2D.OverlapCircleAll(origenUso.position, attackRadius, damageMask);
        foreach (Collider2D hit in hits)
        {
            Health health = hit.GetComponentInParent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                haGolpeadoAlgo = true; // Marcamos que hubo contacto válido
            }
        }

        // 2. REPRODUCIR IMPACTO: Solo si golpeamos carne/enemigo
        if (haGolpeadoAlgo)
        {
            AudioManager.Instance.PlaySFX3D(hitAudioID, origenUso.position);
        }

        Debug.Log($"[Datos] Golpe virtual con {DisplayName} haciendo {damage} de daño.");
        return true;
    }
}