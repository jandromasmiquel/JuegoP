using UnityEngine;

[CreateAssetMenu(menuName = "JuegoP/Inventory/Heal Item Data")]
public class HealItemData : ItemData, IItemEquipable
{
    [Header("Heal Settings")]
    [SerializeField] private int healAmount = 25;
    [SerializeField] private float castTime = 2.0f;
    [SerializeField] private float cooldown = 1.0f;

    private float nextUseTime;

    public bool EnUsar(PlayerController player, Transform lineOfSight)
    {
        if (Time.time < nextUseTime) return false;

        if (player.TryGetComponent<Health>(out var playerHealth))
        {
            if (playerHealth.CurrentHealth >= playerHealth.MaxHealth) return false;

            // Iniciamos el casteo en el jugador
            player.StartDelayedHeal(healAmount, castTime, this);

            // El cooldown se aplica al intento
            nextUseTime = Time.time + castTime + cooldown;
        }

        return false; // IMPORTANTE: Devolvemos false para NO consumir el ítem aún
    }
}