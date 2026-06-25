using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationOffset = 0f;

    [Header("Visuals & Animation")]
    [SerializeField] private Transform visualsContainer; // Objeto vacío "Visuals"
    [SerializeField] private Animator piernasAnimator;   // Objeto hijo "piernas"
    [SerializeField] private Animator torsoAnimator;     // Objeto hijo "torso"

    private Rigidbody2D rb;
    private Camera mainCamera;
    private InputAction moveAction;
    private InputAction useItemAction; // Ahora responderá al Clic Derecho
    private Vector2 moveInput;
    private bool controlsEnabled = true;

    // Referencias genéricas del Sistema de Herramientas
    private InventoryUIController inventoryUI;

    private IItemEquipable itemEquipadoVirtual;
    private ItemData itemDataActivo;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        inventoryUI = FindAnyObjectByType<InventoryUIController>();

        // BUG 1 FIX: Bloqueamos la rotación física del Rigidbody2D por completo.
        // La rotación visual se gestiona manualmente en RotateTowardsMouse().
        // Sin esto, colisiones durante el movimiento acumulan velocidad angular
        // que hace girar al personaje en bucle cuando el inventario está abierto.
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Configuración del movimiento
        moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        // CAMBIO: Ahora el binding escucha explícitamente el CLIC DERECHO
        useItemAction = new InputAction("UseItem", InputActionType.Button, binding: "<Mouse>/rightButton");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        useItemAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        useItemAction.Disable();
    }

    private void OnDestroy()
    {
        moveAction.Dispose();
        useItemAction.Dispose();
    }

    private void Update()
    {
        if (!controlsEnabled)
        {
            moveInput = Vector2.zero;
            UpdateAnimatorsSpeed(0f);
            // BUG 1 FIX: No rotar cuando los controles están deshabilitados (inventario abierto)
            return;
        }

        moveInput = moveAction.ReadValue<Vector2>();
        
        float currentSpeed = moveInput.magnitude * speed;
        UpdateAnimatorsSpeed(currentSpeed);

        RotateTowardsMouse();

        // BUG 2 FIX: Verificar que el inventario esté cerrado antes de usar el ítem
        // (Los hotbar slots ya tienen raycastTarget=false para no bloquear el input)
        bool inventoryOpen = inventoryUI != null && inventoryUI.IsOpen;

        // Ejecución de herramientas con el Clic Derecho
        if (useItemAction.WasPressedThisFrame() && torsoAnimator != null && !inventoryOpen)
        {
            // Le decimos al ScriptableObject que se ejecute desde la posición del Torso
            if (itemEquipadoVirtual != null)
            {
                if (itemEquipadoVirtual.EnUsar(this, torsoAnimator.transform))
                {
                    torsoAnimator.SetTrigger("Usar_item");
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!controlsEnabled)
        {
            // BUG 1 FIX: Garantizamos que la física se detiene completamente
            // cuando los controles están deshabilitados (inventario abierto, intro, etc.)
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            return;
        }

        rb.linearVelocity = moveInput.normalized * speed;
    }

    private void RotateTowardsMouse()
    {
        Transform targetRotationTransform = visualsContainer != null ? visualsContainer : transform;

        if (Mouse.current == null || mainCamera == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(
            new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z)
        );

        Vector2 lookDirection = (Vector2)worldMousePosition - (Vector2)targetRotationTransform.position;
        
        if (lookDirection.sqrMagnitude < 0.001f) return;

        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        targetRotationTransform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
    }

    private void UpdateAnimatorsSpeed(float currentSpeed)
    {
        if (piernasAnimator != null) piernasAnimator.SetFloat("speed", currentSpeed);
    }

    // El inventario sigue llamando a este mismo método para actualizar lo que llevas en la mano
    public void ActualizarItemEnMano(ItemData item)
    {
        itemDataActivo = item;
        itemEquipadoVirtual = item as IItemEquipable; // Si el ScriptableObject tiene lógica, lo engancha

        if (torsoAnimator == null) return;

        if (item == null)
        {
            torsoAnimator.runtimeAnimatorController = null;
            return;
        }

        // Cambiamos el set de animaciones del torso (Bate, Pistola, Desarmado...)
        torsoAnimator.runtimeAnimatorController = item.torsoOverride;
    }

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
        moveInput = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            // BUG 1 FIX: Limpiar velocidad angular acumulada por colisiones
            // para evitar que el personaje siga girando en bucle
            rb.angularVelocity = 0f;
        }

        UpdateAnimatorsSpeed(0f);
    }
}