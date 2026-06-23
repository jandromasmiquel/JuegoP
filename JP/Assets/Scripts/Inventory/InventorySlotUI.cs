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

        // Añade esto en InventorySlotUI.cs
    public void SetSelected(bool isSelected)
    {
        // Puedes cambiar el color o poner un borde
        GetComponent<Image>().color = isSelected ? Color.yellow : Color.white;
    }

    private void OnClick()
    {
        // Solo permitimos seleccionar si el slot NO está vacío
        // (O si ya tienes algo seleccionado, para poder soltarlo)
        controller?.OnSlotClicked(container, slotIndex);
    }
}
