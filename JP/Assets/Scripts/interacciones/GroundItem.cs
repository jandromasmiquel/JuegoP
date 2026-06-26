using UnityEngine;

public class GroundItem : Interactable
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
    private void Reset()
    {
        interactAudioID = "item_pickup_generic"; 
    }

    protected override void OnInteract()
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