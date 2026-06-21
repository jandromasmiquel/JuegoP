using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private GameObject root;
    [SerializeField] private InventoryPanelUI playerPanel;
    [SerializeField] private InventoryPanelUI externalPanel;

    private InventoryContainer externalContainer;
    private InputAction toggleAction;
    private InventoryContainer selectedContainer;
    private int selectedSlotIndex = -1;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        }

        toggleAction = new InputAction("Toggle Inventory", InputActionType.Button, "<Keyboard>/tab");
        toggleAction.performed += _ => TogglePlayerInventory();

        if (root != null)
        {
            root.SetActive(false);
        }
    }

    private void OnEnable()
    {
        toggleAction.Enable();
    }

    private void OnDisable()
    {
        toggleAction.Disable();
    }

    private void OnDestroy()
    {
        toggleAction.Dispose();
    }

    public void OpenExternalContainer(InventoryContainer container, string title)
    {
        externalContainer = container;
        Open();
        BindPanels();

        if (externalPanel != null)
        {
            externalPanel.SetTitle(title);
            externalPanel.Bind(container, this);
            externalPanel.gameObject.SetActive(true);
        }
    }

    public void OnSlotClicked(InventoryContainer container, int slotIndex)
    {
        if (container == null)
        {
            return;
        }

        if (selectedContainer == null)
        {
            selectedContainer = container;
            selectedSlotIndex = slotIndex;
            return;
        }

        if (selectedContainer == container)
        {
            container.SwapSlots(selectedSlotIndex, slotIndex);
        }
        else
        {
            selectedContainer.MoveSlotTo(container, selectedSlotIndex, slotIndex);
        }

        selectedContainer = null;
        selectedSlotIndex = -1;
        Refresh();
    }

    private void TogglePlayerInventory()
    {
        if (root != null && root.activeSelf)
        {
            Close();
        }
        else
        {
            externalContainer = null;
            Open();
            BindPanels();
        }
    }

    private void Open()
    {
        if (root != null)
        {
            root.SetActive(true);
        }
    }

    private void Close()
    {
        externalContainer = null;
        selectedContainer = null;
        selectedSlotIndex = -1;

        if (root != null)
        {
            root.SetActive(false);
        }
    }

    private void BindPanels()
    {
        if (playerPanel != null && playerInventory != null)
        {
            playerPanel.SetTitle("Inventario");
            playerPanel.Bind(playerInventory.Container, this);
        }

        if (externalPanel != null)
        {
            externalPanel.gameObject.SetActive(externalContainer != null);
            if (externalContainer != null)
            {
                externalPanel.Bind(externalContainer, this);
            }
        }
    }

    private void Refresh()
    {
        if (playerPanel != null)
        {
            playerPanel.Refresh();
        }

        if (externalPanel != null)
        {
            externalPanel.Refresh();
        }
    }
}
