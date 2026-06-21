using UnityEngine;

[RequireComponent(typeof(InventoryContainer))]
public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private InventoryContainer container;

    public InventoryContainer Container => container;

    private void Awake()
    {
        if (container == null)
        {
            container = GetComponent<InventoryContainer>();
        }
    }

    public bool AddItem(ItemData item, int amount = 1)
    {
        return container != null && container.AddItem(item, amount);
    }

    public bool HasItem(ItemData item, int amount = 1)
    {
        return container != null && container.HasItem(item, amount);
    }

    public bool TryUseItem(ItemData item, int amount = 1)
    {
        return container != null && container.TryRemoveItem(item, amount);
    }
}
