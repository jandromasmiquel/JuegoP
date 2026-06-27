using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class RadialProgressIndicator : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image fillImage;
    
    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;
    private bool isTrackingMouse = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            canvasRectTransform = parentCanvas.GetComponent<RectTransform>();
        }

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        PlayerController.OnActionProgressChanged += HandleProgressUpdate;
    }

    private void OnDisable()
    {
        PlayerController.OnActionProgressChanged -= HandleProgressUpdate;
        // Al destruirse o desactivarse el script por completo, nos aseguramos de devolver el cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void HandleProgressUpdate(bool mostrar, float progreso)
    {
        isTrackingMouse = mostrar;
        gameObject.SetActive(mostrar);

        if (mostrar)
        {
            // Forzamos la desaparición del cursor del sistema
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined; // Evita que el ratón se salga de la ventana de juego
            
            if (fillImage != null)
            {
                fillImage.fillAmount = progreso;
            }
            
            UpdatePositionToMouse();
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void Update()
    {
        if (isTrackingMouse)
        {
            // Mantenemos el cursor oculto rigurosamente en cada frame
            if (Cursor.visible) Cursor.visible = false;
            
            UpdatePositionToMouse();
        }
    }

    private void UpdatePositionToMouse()
    {
        if (Mouse.current == null || canvasRectTransform == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        
        // Calculamos la posición local respecto al canvas padre para que sea milimétrico
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform, mousePos, null, out Vector2 localPos))
        {
            rectTransform.anchoredPosition = localPos;
        }
    }
}