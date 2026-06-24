using UnityEngine;

public class PlayerEquipmentManager : MonoBehaviour
{
    [SerializeField] private InventoryUIController uiController;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Transform weaponHoldPoint; // Un objeto vacío hijo del jugador que simule sus manos

    private int lastActiveSlot = -1;
    private GameObject currentEquippedObject;
    private ItemData currentItemData;

    private void Start()
    {
        if (playerInventory != null && playerInventory.Container != null)
        {
            // Nos suscribimos al evento de cambio del inventario por si el ítem equipado se destruye o cambia de cantidad
            playerInventory.Container.Changed += RefreshEquippedItem;
        }
    }

    private void Update()
    {
        // Si el inventario está abierto, bloqueamos el cambio de equipo o el uso
        if (uiController.IsOpen) return;

        // Si el jugador cambió de slot en la hotbar, actualizamos lo que lleva en la mano
        if (uiController.ActiveSlotIndex != lastActiveSlot)
        {
            RefreshEquippedItem();
        }
    }

    public void RefreshEquippedItem()
    {
        int currentSlot = uiController.ActiveSlotIndex;
        lastActiveSlot = currentSlot;

        if (playerInventory == null || playerInventory.Container == null) return;

        var slot = playerInventory.Container.Slots[currentSlot];

        // Caso A: El slot actual está vacío o el ítem es distinto al que teníamos guardado
        if (slot.IsEmpty || slot.item != currentItemData)
        {
            UnequipCurrent();
        }

        // Caso B: Hay un ítem nuevo que no teníamos equipado
        if (!slot.IsEmpty && slot.item != currentItemData)
        {
            EquipItem(slot.item);
        }
    }

    private void EquipItem(ItemData item)
    {
        currentItemData = item;

        // Si es un arma y tiene un Prefab asociado...
        if (item is WeaponItemData weaponData && weaponData.EquippedPrefab != null)
        {
            // Instanciamos el bate (u otra arma) en el punto de las manos
            currentEquippedObject = Instantiate(weaponData.EquippedPrefab, weaponHoldPoint);
            
            // Inicializamos el script del arma pasándole sus propios ScriptableObject Datos
            if (currentEquippedObject.TryGetComponent(out BaseballBatWeapon weaponScript))
            {
                weaponScript.Initialize(weaponData);
            }
        }
    }

    private void UnequipCurrent()
    {
        if (currentEquippedObject != null)
        {
            Destroy(currentEquippedObject);
        }
        currentEquippedObject = null;
        currentItemData = null;
    }

    private void OnDestroy()
    {
        if (playerInventory != null && playerInventory.Container != null)
        {
            playerInventory.Container.Changed -= RefreshEquippedItem;
        }
    }
}