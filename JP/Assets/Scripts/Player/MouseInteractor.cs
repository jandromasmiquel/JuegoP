using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInteractor : MonoBehaviour
{
    [SerializeField] private float maxDistance = 2f;
    [SerializeField] private LayerMask interactableLayer;

    private Camera mainCamera;
    private InputAction clickAction;

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
    }

    private void OnDestroy()
    {
        clickAction.performed -= OnClick;
        clickAction.Dispose();
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

        float distance = Vector2.Distance(transform.position, hit.collider.transform.position);
        if (distance > maxDistance)
        {
            Debug.Log("Muy lejos para interactuar");
            return;
        }

        IInteractable interactable = hit.collider.GetComponent<IInteractable>();
        interactable?.Interact();
    }
}
