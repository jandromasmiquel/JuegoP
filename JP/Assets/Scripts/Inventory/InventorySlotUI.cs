using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text amountText;
    [SerializeField] private bool isHotbarSlot = false; 

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
            if (isHotbarSlot)
            {
                button.enabled = false;
                // BUG 2 FIX: Los hotbar slots no deben interceptar eventos de ratón
                // para que el PlayerController pueda recibir el click derecho correctamente
                Image btnImage = button.GetComponent<Image>();
                if (btnImage != null) btnImage.raycastTarget = false;
            }
            else
            {
                button.enabled = true;
                // 🆕 Quitamos el onClick.AddListener tradicional para que no se dupliquen eventos con el clic izquierdo
                button.onClick.RemoveAllListeners();
            }
        }

        bool hasItem = slot != null && !slot.IsEmpty;

        if (iconImage != null)
        {
            iconImage.enabled = hasItem;
            iconImage.sprite = hasItem ? slot.item.Icon : null;
            // Los iconos de hotbar tampoco deben bloquear el raycast
            if (isHotbarSlot) iconImage.raycastTarget = false;
        }

        if (amountText != null)
        {
            amountText.text = hasItem && slot.amount > 1 ? slot.amount.ToString() : string.Empty;
        }
    }

    public void SetSelected(bool isSelected)
    {
        GetComponent<Image>().color = isSelected ? Color.yellow : Color.white;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isHotbarSlot) return; 

        // Pasamos el eventData completo al controlador para que sepa qué botón se pulsó
        controller?.OnSlotClicked(container, slotIndex, eventData);
    }
}