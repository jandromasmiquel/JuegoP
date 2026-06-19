using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private readonly HashSet<string> items = new HashSet<string>();

    public void AddItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return;
        }

        items.Add(itemId);
        Debug.Log($"Objeto recogido: {itemId}");
    }

    public bool HasItem(string itemId)
    {
        return !string.IsNullOrWhiteSpace(itemId) && items.Contains(itemId);
    }

    public bool TryUseItem(string itemId)
    {
        if (!HasItem(itemId))
        {
            return false;
        }

        items.Remove(itemId);
        Debug.Log($"Objeto usado: {itemId}");
        return true;
    }
}
