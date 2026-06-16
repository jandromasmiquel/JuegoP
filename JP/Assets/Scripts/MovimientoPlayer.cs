using UnityEngine;

public class MovimientoPlayer : MonoBehaviour
{public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        // Importante: No bloquees la rotación en el Rigidbody si quieres que rote el cuerpo
        rb.freezeRotation = true; 
    }

    void Update() {
        // 1. Movimiento (Sigue usando esto)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        transform.position += new Vector3(moveX, moveY, 0) * speed * Time.deltaTime;

        // 2. Rotación directa (Ignorando la física)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = (Vector2)mousePos - (Vector2)transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void FixedUpdate() {
        // 3. Aplicar movimiento
        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
    }
}