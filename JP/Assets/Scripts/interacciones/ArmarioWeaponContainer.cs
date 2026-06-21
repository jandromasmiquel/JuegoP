using UnityEngine;

public class ArmarioWeaponContainer : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private ItemData weaponItem;
    [SerializeField] private string emptyMessage = "El armario esta vacio.";

    private bool hasWeapon = true;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = FindAnyObjectByType<PlayerInventory>();
        }
    }

    public void Interact()
    {
        if (!hasWeapon)
        {
            Debug.Log(emptyMessage);
            return;
        }

        if (inventory == null || weaponItem == null)
        {
            Debug.LogWarning("ArmarioWeaponContainer sin inventario o weaponItem asignado.");
            return;
        }

        if (!inventory.AddItem(weaponItem))
        {
            return;
        }

        hasWeapon = false;
        Debug.Log($"Has encontrado: {weaponItem.DisplayName}");
    }
}
