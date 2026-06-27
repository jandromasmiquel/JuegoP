using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f; // Velocidad dinámica actual
    [SerializeField] private float normalSpeed = 5f;
    [SerializeField] private float nightmareSpeed = 3f;
    [SerializeField] private float rotationOffset = 0f;

    [Header("Visuals & Animation")]
    [SerializeField] private Transform visualsContainer; // Objeto vacío "Visuals"
    [SerializeField] private Animator piernasAnimator;   // Objeto hijo "piernas"
    [SerializeField] private Animator torsoAnimator;     // Objeto hijo "torso"

    //CURACIÓN
    private Coroutine healCoroutine;
    private bool isHealing = false;
    public static event System.Action<bool, float> OnActionProgressChanged;

    private Rigidbody2D rb;
    private Camera mainCamera;
    private InputAction moveAction;
    private InputAction useItemAction; // Responderá al Clic Derecho
    private Vector2 moveInput;
    private bool controlsEnabled = true;

    // Referencias locales de Sistemas
    private InventoryUIController inventoryUI;
    private PlayerRoomTracker roomTracker;
    private RoomStateManager roomSuscrita;

    private IItemEquipable itemEquipadoVirtual;
    private ItemData itemDataActivo;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        inventoryUI = FindAnyObjectByType<InventoryUIController>();
        roomTracker = GetComponent<PlayerRoomTracker>();

        // BUG 1 FIX: Bloqueamos la rotación física del Rigidbody2D por completo.
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

        useItemAction = new InputAction("UseItem", InputActionType.Button, binding: "<Mouse>/rightButton");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        useItemAction.Enable();

        // Nos suscribimos al tracker para saber cuándo el jugador cambia de habitación física
        if (roomTracker != null)
        {
            roomTracker.RoomChanged += OnRoomChanged;
        }
    }

    private void OnDisable()
    {
        moveAction.Disable();
        useItemAction.Disable();

        if (roomTracker != null)
        {
            roomTracker.RoomChanged -= OnRoomChanged;
        }
        DesvincularDeSalaActual();
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
            return;
        }

        moveInput = moveAction.ReadValue<Vector2>();
        
        float currentSpeed = moveInput.magnitude * speed;
        UpdateAnimatorsSpeed(currentSpeed);

        RotateTowardsMouse();

        bool inventoryOpen = inventoryUI != null && inventoryUI.IsOpen;

        // Ejecución de herramientas con el Clic Derecho
        if (useItemAction.WasPressedThisFrame() && torsoAnimator != null && !inventoryOpen)
        {
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

    public void ActualizarItemEnMano(ItemData item)
    {
        itemDataActivo = item;
        itemEquipadoVirtual = item as IItemEquipable;

        if (torsoAnimator == null) return;

        if (item == null)
        {
            torsoAnimator.runtimeAnimatorController = null;
            return;
        }

        torsoAnimator.runtimeAnimatorController = item.torsoOverride;
    }

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
        moveInput = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        UpdateAnimatorsSpeed(0f);
    }

    // Método público original adaptado
    public void AlterarVelocidadPorEstado(RoomState estado)
    {
        speed = (estado == RoomState.Nightmare) ? nightmareSpeed : normalSpeed;
    }

    // GESTIÓN DE EVENTOS DE HABITACIÓN
    private void OnRoomChanged(RoomStateManager nuevaSala)
    {
        DesvincularDeSalaActual();

        if (nuevaSala != null)
        {
            roomSuscrita = nuevaSala;
            // Escuchamos si la sala actual cambia entre Normal y Pesadilla en tiempo real
            roomSuscrita.StateChanged += OnRoomStateChanged; 
            
            // Forzamos la actualización inmediata de la velocidad al pisar la sala
            AlterarVelocidadPorEstado(roomSuscrita.CurrentState);
        }
    }

    private void OnRoomStateChanged(RoomState nuevoEstado)
    {
        AlterarVelocidadPorEstado(nuevoEstado);
    }

    private void DesvincularDeSalaActual()
    {
        if (roomSuscrita != null)
        {
            roomSuscrita.StateChanged -= OnRoomStateChanged;
            roomSuscrita = null;
        }
    }



    // CURACION
    public void StartDelayedHeal(int amount, float delay, HealItemData itemData)
    {
        if (isHealing) return; 
        healCoroutine = StartCoroutine(DelayedHealRoutine(amount, delay, itemData));
    }

    private IEnumerator DelayedHealRoutine(int amount, float delay, HealItemData itemData)
    {
        isHealing = true;
        float elapsed = 0f;

        Debug.Log("Comenzando a vendarse...");

        while (elapsed < delay)
        {
            // Si el jugador se mueve, llamamos al nuevo método centralizado
            if (moveInput.sqrMagnitude > 0.01f)
            {
                CancelHeal();
                yield break;
            }

            elapsed += Time.deltaTime;
            float normalizedProgress = elapsed / delay;
            
            // Enviamos el progreso (0 a 1)
            OnActionProgressChanged?.Invoke(true, normalizedProgress);

            yield return null;
        }

        // --- ÉXITO ---
        isHealing = false;
        OnActionProgressChanged?.Invoke(false, 0f);

        if (TryGetComponent<Health>(out var playerHealth))
        {
            playerHealth.Heal(amount);
        }

        if (TryGetComponent<PlayerInventory>(out var playerInventory))
        {
            playerInventory.TryUseItem(itemData, 1);
        }
    }

    // MÉTODO CENTRALIZADO DE CANCELACIÓN
    public void CancelHeal()
    {
        if (!isHealing) return;

        Debug.Log("¡Curación abortada!");
        
        if (healCoroutine != null)
        {
            StopCoroutine(healCoroutine);
        }
        
        isHealing = false;
        
        // Avisamos a la UI de que borre el progreso de inmediato
        OnActionProgressChanged?.Invoke(false, 0f);

        // Aquí meteremos más adelante el STOP del audio
    }

    // Getter público para que el inventario consulte si nos estamos curando
    public bool IsHealing => isHealing;
}