using UnityEngine;

public interface IItemEquipable
{
    // El ítem recibe el transform del jugador para saber desde dónde se usa y emite true/false
    bool EnUsar(PlayerController player, Transform origenUso);
}