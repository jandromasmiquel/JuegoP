using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text amountText;

    private InventoryContainer container;
    private InventoryUIController controller;
    private int slotIndex;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    public void Bind(InventoryContainer newContainer, int newSlotIndex, InventorySlot slot, InventoryUIController newController)
    {
        container = newContainer;
        controller = newController;
        slotIndex = newSlotIndex;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        bool hasItem = slot != null && !slot.IsEmpty;

        if (iconImage != null)
        {
            iconImage.enabled = hasItem;
            iconImage.sprite = hasItem ? slot.item.Icon : null;
        }

        if (amountText != null)
        {
            amountText.text = hasItem && slot.amount > 1 ? slot.amount.ToString() : string.Empty;
        }
    }

    private void OnClick()
    {
        controller?.OnSlotClicked(container, slotIndex);
    }
}
