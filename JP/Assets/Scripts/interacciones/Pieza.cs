using UnityEngine;

public class Pieza : MonoBehaviour, IInteractable
{
    public static bool tienePieza = false;
    public void Interact() {
        tienePieza = true;
        Destroy(gameObject); // Recogida
        Debug.Log("Pieza recogida");
    }
}