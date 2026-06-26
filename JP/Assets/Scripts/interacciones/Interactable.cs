using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Configuración de Audio Base")]
    [SerializeField] protected string interactAudioID;

    // Este es el método que llamará el jugador (el punto de entrada)
    public void TriggerInteract()
    {
        // 1. Gestiona el sonido de forma automática si tiene uno asignado
        if (!string.IsNullOrEmpty(interactAudioID))
        {
            AudioManager.Instance.PlaySFX3D(interactAudioID, transform.position);
        }

        // 2. Llama a la lógica específica de cada objeto
        OnInteract();
    }

    // Cada script hijo tendrá que rellenar este método con su lógica obligatoriamente
    protected abstract void OnInteract();
}