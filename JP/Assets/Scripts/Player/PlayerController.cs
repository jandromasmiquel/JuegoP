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
    private InputAction useItemAction; // Acción para usar herramientas/armas
    private Vector2 moveInput;
    private bool controlsEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

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

        // Configuración del clic izquierdo para usar objetos
        useItemAction = new InputAction("UseItem", InputActionType.Button, binding: "<Mouse>/leftButton");
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
            return;
        }

        moveInput = moveAction.ReadValue<Vector2>();
        
        // Calculamos la velocidad y se la enviamos a las piernas
        float currentSpeed = moveInput.magnitude * speed;
        UpdateAnimatorsSpeed(currentSpeed);

        RotateTowardsMouse();

        // Detectamos si el jugador hace clic izquierdo para usar el ítem equipado
        if (useItemAction.WasPressedThisFrame() && torsoAnimator != null)
        {
            torsoAnimator.SetTrigger("Usar_item");
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput.normalized * speed;
    }

    private void RotateTowardsMouse()
    {
        Transform targetRotationTransform = visualsContainer != null ? visualsContainer : transform;

        if (Mouse.current == null || mainCamera == null)
        {
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(
            new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z)
        );

        Vector2 lookDirection = (Vector2)worldMousePosition - (Vector2)targetRotationTransform.position;
        
        if (lookDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        
        targetRotationTransform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
    }

    private void UpdateAnimatorsSpeed(float currentSpeed)
    {
        // Solo las piernas necesitan el parámetro "speed" para su Blend Tree
        if (piernasAnimator != null)
        {
            piernasAnimator.SetFloat("speed", currentSpeed);
        }
    }

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
        moveInput = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        UpdateAnimatorsSpeed(0f);
    }
}