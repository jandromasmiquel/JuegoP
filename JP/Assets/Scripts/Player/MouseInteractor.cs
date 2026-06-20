using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInteractor : MonoBehaviour
{
    [SerializeField] private float maxDistance = 2f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Cursor Configuration")]
    [SerializeField] private Texture2D interactableCursor;
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;

    private Camera mainCamera;
    private InputAction clickAction;
    private bool isHoveringInteractable;

    private void Awake()
    {
        mainCamera = Camera.main;
        clickAction = new InputAction("Click", InputActionType.Button, "<Mouse>/leftButton");
        clickAction.performed += OnClick;
    }

    private void OnEnable()
    {
        clickAction.Enable();
    }

    private void OnDisable()
    {
        clickAction.Disable();
        ResetCursor();
    }

    private void OnDestroy()
    {
        clickAction.performed -= OnClick;
        clickAction.Dispose();
    }

    private void Update()
    {
        UpdateCursorHover();
    }

    private void UpdateCursorHover()
    {
        if (Mouse.current == null || mainCamera == null)
        {
            ResetCursor();
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldMousePosition, Vector2.zero, 0f, interactableLayer);

        if (hit.collider != null)
        {
            // Medir distancia al punto más cercano del colisionador
            float distance = Vector2.Distance(transform.position, hit.collider.ClosestPoint(transform.position));
            if (distance <= maxDistance)
            {
                SetInteractableCursor();
                return;
            }
        }

        ResetCursor();
    }

    private void SetInteractableCursor()
    {
        if (isHoveringInteractable) return;
        isHoveringInteractable = true;

        if (interactableCursor != null)
        {
            Cursor.SetCursor(interactableCursor, cursorHotspot, CursorMode.Auto);
        }
    }

    private void ResetCursor()
    {
        if (!isHoveringInteractable) return;
        isHoveringInteractable = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        if (Mouse.current == null || mainCamera == null)
        {
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldMousePosition, Vector2.zero, 0f, interactableLayer);

        if (hit.collider == null)
        {
            return;
        }

        // Medir distancia al punto más cercano del colisionador (evita fallos por pivote lejano)
        float distance = Vector2.Distance(transform.position, hit.collider.ClosestPoint(transform.position));
        if (distance > maxDistance)
        {
            Debug.Log($"Muy lejos para interactuar ({distance:F2}m > {maxDistance}m)");
            return;
        }

        IInteractable interactable = hit.collider.GetComponent<IInteractable>();
        interactable?.Interact();
    }
}
