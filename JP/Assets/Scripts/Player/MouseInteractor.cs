using UnityEngine;

public class MouseInteractor : MonoBehaviour
{
    public float maxDistance = 2f;
    public LayerMask interactableLayer;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Clic izquierdo
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            // 1. ¿El objeto clicado está en la capa interactuable?
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, interactableLayer);
            
            if (hit.collider != null)
            {
                // 2. Comprobar distancia
                float distance = Vector2.Distance(transform.position, hit.collider.transform.position);
                if (distance <= maxDistance)
                {
                    var interactable = hit.collider.GetComponent<IInteractable>();
                    interactable?.Interact();
                }
                else {
                    Debug.Log("Muy lejos para interactuar");
                }
            }
        }
    }
}