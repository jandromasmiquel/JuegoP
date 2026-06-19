using UnityEngine;

public class Interruptor : MonoBehaviour, IInteractable
{
    [SerializeField] private RoomStateManager roomStateManager;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private string repairItemId = "interruptor_piece";
    [SerializeField] private bool estaReparado;

    private void Awake()
    {
        if (roomStateManager == null)
        {
            roomStateManager = FindAnyObjectByType<RoomStateManager>();
        }

        if (inventory == null)
        {
            inventory = FindAnyObjectByType<PlayerInventory>();
        }
    }

    public void Interact()
    {
        if (!estaReparado)
        {
            TryRepair();
            return;
        }

        if (roomStateManager == null)
        {
            Debug.LogWarning("Interruptor sin RoomStateManager asignado.");
            return;
        }

        roomStateManager.ToggleWorld();
    }

    private void TryRepair()
    {
        if (inventory == null)
        {
            Debug.LogWarning("No hay PlayerInventory en la escena.");
            return;
        }

        if (!inventory.TryUseItem(repairItemId))
        {
            Debug.Log("Esta roto, necesito una pieza");
            return;
        }

        estaReparado = true;
        Debug.Log("Interruptor reparado");
    }
}
