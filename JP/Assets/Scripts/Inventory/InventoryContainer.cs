using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryContainer : MonoBehaviour
{
    [SerializeField] private int slotCount = 5;
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();

    public event Action Changed;
    public IReadOnlyList<InventorySlot> Slots => slots;
    public int SlotCount => slots.Count;

    private void Awake()
    {
        EnsureSlotCount();
    }

    private void OnValidate()
    {
        EnsureSlotCount();
    }

    public bool AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        EnsureSlotCount();

        if (item.Stackable)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlot slot = slots[i];
                if (slot.item != item || slot.amount >= item.MaxStack)
                {
                    continue;
                }

                int space = item.MaxStack - slot.amount;
                int moved = Mathf.Min(space, amount);
                slot.amount += moved;
                amount -= moved;

                if (amount <= 0)
                {
                    Changed?.Invoke();
                    return true;
                }
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty)
            {
                continue;
            }

            int moved = item.Stackable ? Mathf.Min(item.MaxStack, amount) : 1;
            slots[i].Set(item, moved);
            amount -= moved;

            if (amount <= 0)
            {
                Changed?.Invoke();
                return true;
            }
        }

        Changed?.Invoke();
        return false;
    }

    public bool HasItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        int total = 0;
        foreach (InventorySlot slot in slots)
        {
            if (slot.item == item)
            {
                total += slot.amount;
            }
        }

        return total >= amount;
    }

    public bool TryRemoveItem(ItemData item, int amount = 1)
    {
        if (!HasItem(item, amount))
        {
            return false;
        }

        for (int i = slots.Count - 1; i >= 0 && amount > 0; i--)
        {
            InventorySlot slot = slots[i];
            if (slot.item != item)
            {
                continue;
            }

            int removed = Mathf.Min(slot.amount, amount);
            slot.amount -= removed;
            amount -= removed;

            if (slot.amount <= 0)
            {
                slot.Clear();
            }
        }

        Changed?.Invoke();
        return true;
    }

    public void SwapSlots(int firstIndex, int secondIndex)
    {
        if (!IsValidIndex(firstIndex) || !IsValidIndex(secondIndex) || firstIndex == secondIndex)
        {
            return;
        }

        (slots[firstIndex], slots[secondIndex]) = (slots[secondIndex], slots[firstIndex]);
        Changed?.Invoke();
    }

    public bool MoveSlotTo(InventoryContainer target, int sourceIndex, int targetIndex)
    {
        if (target == null || !IsValidIndex(sourceIndex) || !target.IsValidIndex(targetIndex))
        {
            return false;
        }

        InventorySlot source = slots[sourceIndex];
        InventorySlot destination = target.slots[targetIndex];

        if (source.IsEmpty)
        {
            return false;
        }

        (slots[sourceIndex], target.slots[targetIndex]) = (destination, source);
        Changed?.Invoke();
        target.Changed?.Invoke();
        return true;
    }

    public bool IsValidIndex(int index)
    {
        EnsureSlotCount();
        return index >= 0 && index < slots.Count;
    }

    private void EnsureSlotCount()
    {
        slotCount = Mathf.Max(1, slotCount);

        while (slots.Count < slotCount)
        {
            slots.Add(new InventorySlot());
        }

        while (slots.Count > slotCount)
        {
            slots.RemoveAt(slots.Count - 1);
        }
    }
}
