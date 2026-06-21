using System;

[Serializable]
public class InventorySlot
{
    public ItemData item;
    public int amount;

    public bool IsEmpty => item == null || amount <= 0;

    public void Set(ItemData newItem, int newAmount = 1)
    {
        item = newItem;
        amount = newItem == null ? 0 : Math.Max(1, newAmount);
    }

    public void Clear()
    {
        item = null;
        amount = 0;
    }
}
