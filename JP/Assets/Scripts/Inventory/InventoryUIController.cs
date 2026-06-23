using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    [Header("Inventarios Base")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private GameObject root;
    [SerializeField] private InventoryPanelUI playerPanel;
    [SerializeField] private InventoryPanelUI externalPanel;
    [SerializeField] private Image dragIcon; // Arrastra aquí una imagen temporal que crees en el Canvas

    [Header("Hotbar Visual (3 slots pasivos)")]
    [SerializeField] private InventorySlotUI[] hotbarSlots = new InventorySlotUI[3]; // Arrastra aquí tus 3 slots de la Hotbar
    [SerializeField] private RectTransform selectionHighlight; // Arrastra aquí el marco/borde resaltado

    private InventoryContainer externalContainer;
    private InputAction toggleAction;
    private InventoryContainer selectedContainer;
    private int selectedSlotIndex = -1;
    private PlayerController playerController;
    private Canvas cachedCanvas;
    private Camera canvasCamera;

    private int activeSlotIndex = 0; // Controla cuál de los 3 slots de la hotbar está seleccionado

    // NUEVO: Bandera para saber si el ítem flotante actual se cogió entero (Click Izquierdo) o solo una unidad (Click Derecho)
    private bool isHoldingSingleFromRightClick = false;

    public bool IsOpen => root != null && root.activeSelf;
    public int ActiveSlotIndex => activeSlotIndex; // Propiedad pública para que otros scripts sepan qué llevas en la mano

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

            // Reparentamos el dragIcon al Canvas directamente para que no dependa de root
            cachedCanvas = dragIcon.GetComponentInParent<Canvas>();
            if (cachedCanvas != null)
            {
                dragIcon.transform.SetParent(cachedCanvas.transform, false);
                if (cachedCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    canvasCamera = cachedCanvas.worldCamera;
                }
            }

            dragIcon.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // Alineación responsiva de los paneles en el Canvas
        AlignPanels();

        // Forzamos un refresco inicial de la hotbar al arrancar el juego
        UpdateHotbarVisuals();

        // Forzamos a Unity a calcular las posiciones de la UI (layout) antes de posicionar el highlight por primera vez
        Canvas.ForceUpdateCanvases();
        UpdateHighlightPosition();
        if (playerInventory != null && playerInventory.Container != null)
        {
            playerInventory.Container.Changed += UpdateHotbarVisuals;
        }
    }

    private void AlignPanels()
    {
        float verticalAnchor = 1f;

        if (playerPanel != null)
        {
            RectTransform rect = playerPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0f, verticalAnchor);
                rect.anchorMax = new Vector2(0f, verticalAnchor);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f);
            }
        }

        if (externalPanel != null)
        {
            RectTransform rect = externalPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(1f, verticalAnchor);
                rect.anchorMax = new Vector2(1f, verticalAnchor);
                rect.pivot = new Vector2(1f, 0.5f);
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f);
            }
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
        if (playerInventory != null && playerInventory.Container != null)
        {
            playerInventory.Container.Changed -= UpdateHotbarVisuals;
        }
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

        Cursor.visible = true;
    }

    private void Close()
    {
        // Si cerramos el inventario con un ítem a medias en el ratón, lo devolvemos
        if (selectedContainer != null)
        {
            ReturnHeldItemToSource();
        }

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

        UpdateHotbarVisuals();
    }

    private void Update()
    {
        if (!IsOpen && Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) ChangeActiveSlot(0);
            else if (Keyboard.current.digit2Key.wasPressedThisFrame) ChangeActiveSlot(1);
            else if (Keyboard.current.digit3Key.wasPressedThisFrame) ChangeActiveSlot(2);
        }

        if (selectedContainer != null && dragIcon != null && Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    dragIcon.rectTransform, mousePos, canvasCamera, out Vector3 worldPos))
            {
                dragIcon.transform.position = worldPos;
            }
        }
    }

    private void ChangeActiveSlot(int newIndex)
    {
        if (newIndex < 0 || newIndex >= hotbarSlots.Length) return;

        activeSlotIndex = newIndex;
        UpdateHighlightPosition();
        Debug.Log($"Slot activo cambiado al: {activeSlotIndex}");
    }

    private void UpdateHighlightPosition()
    {
        if (selectionHighlight != null && hotbarSlots[activeSlotIndex] != null)
        {
            selectionHighlight.position = hotbarSlots[activeSlotIndex].GetComponent<RectTransform>().position;
        }
    }

    private void UpdateHotbarVisuals()
    {
        if (playerInventory == null || playerInventory.Container == null) return;

        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null && i < playerInventory.Container.Slots.Count)
            {
                var logicalSlot = playerInventory.Container.Slots[i];
                hotbarSlots[i].Bind(playerInventory.Container, i, logicalSlot, this);
            }
        }
    }
    public void OnSlotClicked(InventoryContainer container, int slotIndex, UnityEngine.EventSystems.PointerEventData eventData = null)
    {

        bool isShiftPressed = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
        bool isCtrlPressed = Keyboard.current != null && Keyboard.current.ctrlKey.isPressed;
        bool isAltPressed = Keyboard.current != null && Keyboard.current.altKey.isPressed;
        bool isRightClick = eventData != null && eventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Right;

        InventorySlot clickedSlot = container.Slots[slotIndex];

        // --- INTERCEPCIÓN DE SHIFT + CLICK (Traspaso rápido completo) ---
        if (isShiftPressed && !isCtrlPressed && selectedContainer == null)
        {
            if (clickedSlot.IsEmpty) return;

            InventoryContainer targetContainer = null;
            if (container == playerInventory.Container)
            {
                if (externalContainer != null) targetContainer = externalContainer;
            }
            else if (container == externalContainer)
            {
                targetContainer = playerInventory.Container;
            }

            if (targetContainer != null)
            {
                container.MoveItemInstant(slotIndex, targetContainer);
                UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
                Refresh();
            }
            return;
        }

        // --- INTERCEPCIÓN 1: CTRL + SHIFT + CLICK (Tirar TODO el stack al suelo) ---
        if (isCtrlPressed && isShiftPressed && selectedContainer == null)
        {
            if (!clickedSlot.IsEmpty)
            {
                Debug.Log($"[Drop Stack] Soltando TODO el stack al suelo: {clickedSlot.amount}x {clickedSlot.item.DisplayName}");
                clickedSlot.Clear();
                container.SwapSlots(slotIndex, slotIndex); 
                UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
                Refresh();
            }
            return;
        }

        // --- INTERCEPCIÓN 2: CTRL + CLICK (Traspasar solo 1 unidad al otro inventario) ---
        if (isCtrlPressed && !isShiftPressed && selectedContainer == null)
        {
            if (!clickedSlot.IsEmpty)
            {
                InventoryContainer targetContainer = null;
                if (container == playerInventory.Container)
                {
                    if (externalContainer != null) targetContainer = externalContainer;
                }
                else if (container == externalContainer)
                {
                    targetContainer = playerInventory.Container;
                }

                if (targetContainer != null)
                {
                    bool exito = targetContainer.MoveSingleItemFrom(container, slotIndex);
                    if (exito)
                    {
                        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
                        Refresh();
                    }
                }
            }
            return;
        }

        // --- INTERCEPCIÓN EXTRA 2: ALT + CLICK (Equipar rápido a la Hotbar) ---
        if (isAltPressed && selectedContainer == null && container == playerInventory.Container)
        {
            if (!clickedSlot.IsEmpty)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (playerInventory.Container.Slots[i].IsEmpty || 
                        (playerInventory.Container.Slots[i].item == clickedSlot.item && playerInventory.Container.Slots[i].amount < clickedSlot.item.MaxStack))
                    {
                        container.MoveSlotTo(playerInventory.Container, slotIndex, i);
                        break;
                    }
                }
                UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
                Refresh();
            }
            return;
        }


        // ====================================================================
        // 2. LÓGICA DE ARRASTRE / RECOGIDA (IZQUIERDO Y DERECHO)
        // ====================================================================
        
        // FASE A: NO TENEMOS NINGÚN ÍTEM EN EL RATÓN (Recoger del inventario)
        if (selectedContainer == null)
        {
            if (clickedSlot.IsEmpty)
            {
                UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
                return;
            }

            selectedContainer = container;
            selectedSlotIndex = slotIndex;

            // CLIC DERECHO: Si es stackeable, dejamos el original con cantidad - 1 y cogemos solo 1
            // 🆕 Añadido soporte por si el ítem no tiene propiedad "Stackable" pero su cantidad es > 1
            if (isRightClick && clickedSlot.amount > 1)
            {
                isHoldingSingleFromRightClick = true;
                clickedSlot.amount--; // Le quitamos uno al montón
            }
            else
            {
                // Clic izquierdo o un solo elemento restante: Levantamos todo de forma convencional
                isHoldingSingleFromRightClick = false;
            }

            // Activamos visualmente el icono flotante del ratón
            if (dragIcon != null)
            {
                dragIcon.sprite = clickedSlot.item.Icon;
                Color c = dragIcon.color;
                c.a = 1f;
                dragIcon.color = c;
                dragIcon.enabled = true;
                dragIcon.gameObject.SetActive(true);
                dragIcon.transform.SetAsLastSibling();
            }

            Cursor.visible = false;
            
            // Forzamos la actualización visual del inventario para que el texto de la casilla baje inmediatamente
            container.SwapSlots(slotIndex, slotIndex); 
            Refresh(); 
            return;
        }

        // FASE B: SÍ TENEMOS UN ÍTEM EN EL RATÓN (Soltar en un destino)
        if (selectedContainer != null)
        {
            InventorySlot sourceSlot = selectedContainer.Slots[selectedSlotIndex];

            // Si por alguna razón el origen quedó corrupto
            if (sourceSlot.IsEmpty && !isHoldingSingleFromRightClick)
            {
                ClearHeldState();
                return;
            }

            // Caso 1: Soltar en el MISMO slot de origen
            if (selectedContainer == container && selectedSlotIndex == slotIndex)
            {
                if (isHoldingSingleFromRightClick)
                {
                    // Si se había extraído una sola unidad, la devolvemos al stack original
                    sourceSlot.amount++;
                }
                ClearHeldState();
                Refresh();
                return;
            }

            // Caso 2: El slot de destino está COMPLETAMENTE VACÍO
            if (clickedSlot.IsEmpty)
            {
                if (isHoldingSingleFromRightClick)
                {
                    // Depositamos la unidad que teníamos retenida del Clic Derecho
                    clickedSlot.Set(sourceSlot.item, 1);
                }
                else
                {
                    // Movemos todo de manera tradicional
                    selectedContainer.MoveSlotTo(container, selectedSlotIndex, slotIndex);
                }
            }
            // Caso 3: El slot de destino TIENE UN ÍTEM
            else
            {
                if (isHoldingSingleFromRightClick)
                {
                    // Si es el mismo tipo de ítem y cabe, añadimos la unidad suelta
                    if (clickedSlot.item == sourceSlot.item && clickedSlot.item.Stackable && clickedSlot.amount < clickedSlot.item.MaxStack)
                    {
                        clickedSlot.amount++;
                    }
                    else
                    {
                        // Si son distintos, cancelamos y devolvemos la unidad al origen para evitar borrar ítems
                        sourceSlot.amount++;
                    }
                }
                else
                {
                    // Clic izquierdo normal: Ejecutamos intercambio o mezcla estándar
                    selectedContainer.MoveSlotTo(container, selectedSlotIndex, slotIndex);
                }
            }

            ClearHeldState();
            Refresh();
        }
    }

    private void ClearHeldState()
    {
        selectedContainer = null;
        selectedSlotIndex = -1;
        isHoldingSingleFromRightClick = false;

        if (dragIcon != null)
        {
            dragIcon.gameObject.SetActive(false);
        }

        Cursor.visible = true;
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
    }

    private void ReturnHeldItemToSource()
    {
        if (selectedContainer != null && selectedSlotIndex >= 0)
        {
            InventorySlot source = selectedContainer.Slots[selectedSlotIndex];
            if (isHoldingSingleFromRightClick && !source.IsEmpty)
            {
                source.amount++;
            }
            // Si era un drag completo, el objeto nunca se borró del origen con tu sistema actual de Swap, por lo que no hace falta hacer nada más.
        }
    }
}