using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationOffset = 0f;

    private Rigidbody2D rb;
    private Camera mainCamera;
    private InputAction moveAction;
    private Vector2 moveInput;
    private bool controlsEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

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
    }

    private void OnEnable()
    {
        moveAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
    }

    private void OnDestroy()
    {
        moveAction.Dispose();
    }

    private void Update()
    {
        if (!controlsEnabled)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = moveAction.ReadValue<Vector2>();
        RotateTowardsMouse();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput.normalized * speed;
    }

    private void RotateTowardsMouse()
    {
        if (Mouse.current == null || mainCamera == null)
        {
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(
            new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z)
        );

        Vector2 lookDirection = (Vector2)worldMousePosition - rb.position;
        
        if (lookDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        // Calculamos el ángulo base
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        
        // Aplicamos el offset. Si tu sprite mira hacia arriba, -90f suele ser el valor correcto.
        rb.MoveRotation(angle + rotationOffset);
    }

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
        moveInput = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
