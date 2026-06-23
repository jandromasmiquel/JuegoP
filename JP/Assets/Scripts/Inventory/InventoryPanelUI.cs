using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanelUI : MonoBehaviour
{
    [SerializeField] private Text titleText;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private InventorySlotUI slotPrefab;

    private readonly List<InventorySlotUI> slotUis = new List<InventorySlotUI>();
    private InventoryContainer container;
    private InventoryUIController controller;

    public void Bind(InventoryContainer newContainer, InventoryUIController newController)
    {
        if (container != null)
        {
            container.Changed -= Refresh;
        }

        container = newContainer;
        controller = newController;

        if (container != null)
        {
            container.Changed += Refresh;
        }

        RebuildSlots();
        Refresh();
    }

    public void SetTitle(string title)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
    }

    public void Refresh()
    {
        for (int i = 0; i < slotUis.Count; i++)
        {
            InventorySlot slot = container != null && i < container.SlotCount ? container.Slots[i] : null;
            slotUis[i].Bind(container, i, slot, controller);
        }
    }

    private void RebuildSlots()
    {
        int requiredSlots = container != null ? container.SlotCount : 0;

        if (requiredSlots > 0 && slotPrefab == null)
        {
            Debug.LogError($"[InventoryPanelUI] slotPrefab is not assigned on {gameObject.name}!", this);
            return;
        }

        while (slotUis.Count < requiredSlots)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, slotsParent != null ? slotsParent : transform);
            slotUis.Add(slot);
        }

        for (int i = 0; i < slotUis.Count; i++)
        {
            slotUis[i].gameObject.SetActive(i < requiredSlots);
        }
    }

    private void OnDestroy()
    {
        if (container != null)
        {
            container.Changed -= Refresh;
        }
    }
}
