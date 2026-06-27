using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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
    private bool clickTriggered;
    private PlayerController playerController;

    private void Awake()
    {
        mainCamera = Camera.main;
        playerController = GetComponent<PlayerController>();
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

        if (clickTriggered)
        {
            clickTriggered = false;
            ExecuteClick();
        }
    }

    private void UpdateCursorHover()
    {
        if (playerController != null && playerController.IsHealing)
        {
            ResetCursor();
            return;
        }

        if (Mouse.current == null || mainCamera == null)
        {
            ResetCursor();
            return;
        }

        if (ScreenFader.Instance != null && ScreenFader.Instance.IsFading)
        {
            ResetCursor();
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
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
        clickTriggered = true;
    }

    private void ExecuteClick()
    {
        if (Mouse.current == null || mainCamera == null)
        {
            return;
        }

        if (ScreenFader.Instance != null && ScreenFader.Instance.IsFading)
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
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

        // Busca el interactuable en el objeto impactado o sube por la jerarquía hasta el padre si es necesario
        Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
        if (interactable != null) interactable.TriggerInteract();
    }
}
