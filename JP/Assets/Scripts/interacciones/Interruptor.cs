using UnityEngine;

public class Interruptor : MonoBehaviour, IInteractable
{
    public bool estaReparado = false;
    
    public void Interact() {
        if (!estaReparado && Pieza.tienePieza) {
            estaReparado = true;
            Debug.Log("Interruptor reparado");
        } else if (estaReparado) {
            FindAnyObjectByType<GameStateManager>().ToggleWorld();
        } else {
            Debug.Log("Está roto, necesito una pieza");
        }
    }
}