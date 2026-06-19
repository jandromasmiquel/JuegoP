using UnityEngine;

public class Pieza : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemId = "interruptor_piece";
    [SerializeField] private PlayerInventory inventory;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = FindAnyObjectByType<PlayerInventory>();
        }
    }

    public void Interact()
    {
        if (inventory == null)
        {
            Debug.LogWarning("No hay PlayerInventory en la escena.");
            return;
        }

        inventory.AddItem(itemId);
        Destroy(gameObject);
    }
}
