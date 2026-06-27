using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class RadialProgressIndicator : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image fillImage;
    
    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;
    private Camera canvasCamera;
    private bool isTrackingMouse = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            canvasRectTransform = parentCanvas.GetComponent<RectTransform>();
            canvasCamera = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : parentCanvas.worldCamera;
        }

        PlayerController.OnActionProgressChanged += HandleProgressUpdate;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        PlayerController.OnActionProgressChanged -= HandleProgressUpdate;
        RestoreCursor();
    }

    private void HandleProgressUpdate(bool mostrar, float progreso)
    {
        isTrackingMouse = mostrar;
        gameObject.SetActive(mostrar);

        if (mostrar)
        {
            HideSystemCursor();

            if (fillImage != null)
            {
                fillImage.fillAmount = progreso;
            }

            UpdatePositionToMouse();
        }
        else
        {
            RestoreCursor();
        }
    }

    private void Update()
    {
        if (!isTrackingMouse) return;

        HideSystemCursor();
        UpdatePositionToMouse();
    }

    private void HideSystemCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void RestoreCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void UpdatePositionToMouse()
    {
        if (Mouse.current == null || canvasRectTransform == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform, mousePos, canvasCamera, out Vector2 localPos))
        {
            rectTransform.anchoredPosition = localPos;
        }
    }
}