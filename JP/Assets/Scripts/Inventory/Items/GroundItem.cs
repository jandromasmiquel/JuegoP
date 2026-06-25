using UnityEngine;

public class GroundItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData item;
    [SerializeField] private int amount = 1;
    [SerializeField] private PlayerInventory playerInventory;

    [Header("Audio")]
    [Tooltip("ID del sonido en el AudioManager. Si se deja vacío, usará un sonido genérico.")]
    [SerializeField] private string pickupAudioID = "item_pickup_generic";

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        }
    }

    public void Interact()
    {
        if (item == null)
        {
            Debug.LogWarning($"{name} no tiene ItemData asignado.");
            return;
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("No hay PlayerInventory en la escena.");
            return;
        }

        if (!playerInventory.AddItem(item, amount))
        {
            Debug.Log("No cabe en el inventario.");
            return;
        }

        // REPRODUCIR SONIDO ANTES DE DESTRUIR EL OBJETO
        // Usamos PlaySFX3D en la posición del ítem para que el jugador escuche espacialmente dónde lo ha cogido
        if (!string.IsNullOrEmpty(pickupAudioID))
        {
            AudioManager.Instance.PlaySFX3D(pickupAudioID, transform.position);
        }

        Destroy(gameObject);
    }
}