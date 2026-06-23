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

    // 1. MODIFICA tu método MoveSlotTo existente para que combine los stacks en lugar de hacer solo un Swap ciego
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

        // Si el destino tiene el mismo ítem y es stackeable, los combinamos
        if (!destination.IsEmpty && destination.item == source.item && source.item.Stackable)
        {
            int space = source.item.MaxStack - destination.amount;
            if (space > 0)
            {
                int moved = Mathf.Min(space, source.amount);
                destination.amount += moved;
                source.amount -= moved;

                if (source.amount <= 0)
                {
                    source.Clear();
                }

                Changed?.Invoke();
                target.Changed?.Invoke();
                return true;
            }
        }

        // Si no se pueden combinar (son distintos o el destino está vacío), hacemos el Swap clásico que ya tenías
        (slots[sourceIndex], target.slots[targetIndex]) = (destination, source);
        Changed?.Invoke();
        target.Changed?.Invoke();
        return true;
    }

    // 2. NUEVO MÉTODO: Intenta mover un slot entero a OTRÓ contenedor (Busca completar stacks primero, luego huecos vacíos)
    public void MoveItemInstant(int sourceIndex, InventoryContainer target)
    {
        if (target == null || !IsValidIndex(sourceIndex)) return;
        
        InventorySlot source = slots[sourceIndex];
        if (source.IsEmpty) return;

        // Fase 1: Si es stackeable, intentar llenar slots que ya tengan este ítem en el objetivo
        if (source.item.Stackable)
        {
            for (int i = 0; i < target.slots.Count; i++)
            {
                InventorySlot targetSlot = target.slots[i];
                if (targetSlot.item == source.item && targetSlot.amount < source.item.MaxStack)
                {
                    int space = source.item.MaxStack - targetSlot.amount;
                    int moved = Mathf.Min(space, source.amount);
                    
                    targetSlot.amount += moved;
                    source.amount -= moved;

                    if (source.amount <= 0)
                    {
                        source.Clear();
                        Changed?.Invoke();
                        target.Changed?.Invoke();
                        return;
                    }
                }
            }
        }

        // Fase 2: Si aún queda cantidad, buscar el primer hueco completamente vacío en el objetivo
        for (int i = 0; i < target.slots.Count; i++)
        {
            InventorySlot targetSlot = target.slots[i];
            if (targetSlot.IsEmpty)
            {
                int maxMove = source.item.Stackable ? source.item.MaxStack : 1;
                int moved = Mathf.Min(maxMove, source.amount);

                targetSlot.Set(source.item, moved);
                source.amount -= moved;

                if (source.amount <= 0)
                {
                    source.Clear();
                    Changed?.Invoke();
                    target.Changed?.Invoke();
                    return;
                }
            }
        }

        // Si se movió algo pero no todo, refrescamos los cambios
        Changed?.Invoke();
        target.Changed?.Invoke();
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
    public bool MoveSingleItemFrom(InventoryContainer sourceContainer, int sourceIndex)
    {
        if (sourceContainer == null || !sourceContainer.IsValidIndex(sourceIndex)) return false;

        // Accedemos a la lista interna de slots del contenedor origen
        InventorySlot sourceSlot = sourceContainer.slots[sourceIndex];
        if (sourceSlot.IsEmpty) return false;

        // Intentamos añadir 1 sola unidad usando el AddItem de este contenedor destino
        if (AddItem(sourceSlot.item, 1))
        {
            sourceSlot.amount--;
            if (sourceSlot.amount <= 0)
            {
                sourceSlot.Clear();
            }

            // Notificamos que el contenedor de origen cambió
            sourceContainer.Changed?.Invoke(); 
            return true;
        }

        return false;
    }
}
