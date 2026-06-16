using UnityEngine;

public class MovimientoPlayer : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        // IMPORTANTE: NO marques "Freeze Rotation Z" en el Inspector si quieres que rote
        rb.freezeRotation = false; 
    }

    void Update() {
        // 1. Movimiento (usamos transform para mover, o MovePosition)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 movement = new Vector2(moveX, moveY).normalized;
        rb.linearVelocity = movement * speed;
    }

    void FixedUpdate() {
        // 2. Rotación hacia el ratón (usando física)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = (Vector2)mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        
        // MoveRotation hace que el hitbox rote suavemente con la física
        rb.MoveRotation(Quaternion.Euler(0, 0, angle));
    }
}