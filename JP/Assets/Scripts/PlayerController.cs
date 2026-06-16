using UnityEngine;
using UnityEngine.InputSystem; // Importante para Unity 6

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update() {
        // Movimiento básico (seguimos usando axis para simplicidad si no has configurado los Action Maps)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        rb.linearVelocity = new Vector2(moveX, moveY).normalized * speed; // En Unity 6 se usa 'linearVelocity'

        // ROTACIÓN hacia el ratón
        Vector2 mousePos = Mouse.current.position.ReadValue(); // Esto es lo nuevo en Unity 6
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
        
        Vector2 lookDir = (Vector2)worldMousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 180f;
        
        rb.MoveRotation(angle);
    }
}