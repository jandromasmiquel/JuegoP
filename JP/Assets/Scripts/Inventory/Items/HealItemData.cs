using UnityEngine;

[CreateAssetMenu(menuName = "JuegoP/Inventory/Heal Item Data")]
public class HealItemData : ItemData, IItemEquipable
{
    [Header("Heal Settings")]
    [SerializeField] private int healAmount = 25;
    [SerializeField] private float castTime = 2.0f;
    [SerializeField] private float cooldown = 1.0f;

    private float nextUseTime;

    // Con esto nos aseguramos de que cada vez que des a Play o cargues el juego,
    // el cooldown de este objeto concreto empiece limpio en cero.
    private void OnEnable()
    {
        nextUseTime = 0f;
    }

    public bool EnUsar(PlayerController player, Transform lineOfSight)
    {
        // Debug para ver qué está pasando exactamente en la consola
        Debug.Log($"[HealItem] Time.time: {Time.time} | nextUseTime: {nextUseTime}");

        if (Time.time < nextUseTime) 
        {
            Debug.Log("[HealItem] Bloqueado por Cooldown");
            return false;
        }

        if (player.TryGetComponent<Health>(out var playerHealth))
        {
            if (playerHealth.CurrentHealth >= playerHealth.MaxHealth) return false;

            player.StartDelayedHeal(healAmount, castTime, this);

            nextUseTime = Time.time + castTime + cooldown;
        }

        return false; 
    }
}