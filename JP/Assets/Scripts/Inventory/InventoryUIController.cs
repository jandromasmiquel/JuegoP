using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private GameObject root;
    [SerializeField] private InventoryPanelUI playerPanel;
    [SerializeField] private InventoryPanelUI externalPanel;
    [SerializeField] private Image dragIcon; // Arrastra aquí una imagen temporal que crees en el Canvas

    private InventoryContainer externalContainer;
    private InputAction toggleAction;
    private InventoryContainer selectedContainer;
    private int selectedSlotIndex = -1;
    private PlayerController playerController;

    public bool IsOpen => root != null && root.activeSelf;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        }

        playerController = FindAnyObjectByType<PlayerController>();

        toggleAction = new InputAction("Toggle Inventory", InputActionType.Button, "<Keyboard>/tab");
        toggleAction.performed += _ => TogglePlayerInventory();

        if (root != null)
        {
            root.SetActive(false);
        }

        if (dragIcon != null)
        {
            dragIcon.raycastTarget = false;
            dragIcon.gameObject.SetActive(false);
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
        if (ScreenFader.Instance != null && ScreenFader.Instance.IsFading)
        {
            return;
        }

        externalContainer = container;
        Open();
        BindPanels();

        if (externalPanel != null)
        {
            externalPanel.SetTitle(title);
        }
    }

    private void TogglePlayerInventory()
    {
        if (ScreenFader.Instance != null && ScreenFader.Instance.IsFading)
        {
            return;
        }

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

        if (playerController != null)
        {
            playerController.SetControlsEnabled(false);
        }

        // Nos aseguramos de que el cursor sea visible al abrir el inventario
        Cursor.visible = true;
    }

    private void Close()
    {
        externalContainer = null;
        selectedContainer = null;
        selectedSlotIndex = -1;

        if (dragIcon != null)
        {
            dragIcon.gameObject.SetActive(false);
        }

        if (root != null)
        {
            root.SetActive(false);
        }

        if (playerController != null)
        {
            playerController.SetControlsEnabled(true);
        }

        // SEGURIDAD: Si cierran el inventario a mitad de un arrastre, 
        // forzamos que el cursor vuelva a ser visible para el gameplay o menús.
        Cursor.visible = true; 
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

    private void Update()
    {
        // Esto hace que el icono siga al ratón siempre que haya algo seleccionado
        if (selectedContainer != null && dragIcon != null && Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Canvas canvas = dragIcon.canvas;
            if (canvas != null)
            {
                RectTransform parentRect = dragIcon.transform.parent as RectTransform;
                if (parentRect != null)
                {
                    Vector2 localPos;
                    Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, mousePos, cam, out localPos);
                    dragIcon.rectTransform.anchoredPosition = localPos;
                }
            }
        }
    }

    public void OnSlotClicked(InventoryContainer container, int slotIndex)
    {
        // 1. Si clicamos en una vacía y no tenemos nada seleccionado, no hacemos NADA
        if (selectedContainer == null && (container.Slots[slotIndex].IsEmpty)) 
        {
            // SOLUCIÓN 1: Si pulsamos una casilla vacía sin llevar nada arrastrando, 
            // forzamos a Unity a quitarle el foco al botón para que no cambie de color.
            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
            return;
        }

        // 2. Lógica de selección
        if (selectedContainer == null)
        {
            selectedContainer = container;
            selectedSlotIndex = slotIndex;
            // Activamos el icono de arrastre (dragIcon) con el sprite del item seleccionado
            if (dragIcon != null)
            {
                dragIcon.sprite = container.Slots[slotIndex].item.Icon;
                Color c = dragIcon.color;
                c.a = 1f;
                dragIcon.color = c;
                dragIcon.enabled = true;
                dragIcon.gameObject.SetActive(true);
                dragIcon.transform.SetAsLastSibling();
            }

            // OCULTAR CURSOR: Empezamos a arrastrar, ocultamos el puntero del sistema
            Cursor.visible = false;
            return;
        }

        // 3. Ejecución del movimiento (Swap o Move)
        if (selectedContainer == container)
            container.SwapSlots(selectedSlotIndex, slotIndex);
        else
            selectedContainer.MoveSlotTo(container, selectedSlotIndex, slotIndex);

        // 4. Limpieza
        selectedContainer = null;
        selectedSlotIndex = -1;
        if (dragIcon != null)
        {
            dragIcon.gameObject.SetActive(false); // Ocultamos el icono
        }

        // MOSTRAR CURSOR: El objeto ya se ha colocado, devolvemos el cursor a la normalidad
        Cursor.visible = true;

        // SOLUCIÓN 2: Como ya hemos soltado el objeto en su destino, limpiamos por completo
        // el foco de la UI de Unity. Así, el slot de origen y el de destino vuelven a su color normal.
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);

        Refresh();
    }
}