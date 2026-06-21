using UnityEngine;

public class GroundItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData item;
    [SerializeField] private int amount = 1;
    [SerializeField] private PlayerInventory playerInventory;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        }
    }

    public void Interact()
    {
        if (item == null)
        {
            Debug.LogWarning($"{name} no tiene ItemData asignado.");
            return;
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("No hay PlayerInventory en la escena.");
            return;
        }

        if (!playerInventory.AddItem(item, amount))
        {
            Debug.Log("No cabe en el inventario.");
            return;
        }

        Destroy(gameObject);
    }
}
