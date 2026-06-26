using UnityEngine;

public class Interruptor : Interactable
{
    [SerializeField] private RoomStateManager roomStateManager;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private ItemData repairItem;
    [SerializeField] private bool estaReparado;

    private AudioSource audioSourceLocal;

    private void Awake()
    {
        audioSourceLocal = GetComponent<AudioSource>();
        if (!estaReparado)
        {
            audioSourceLocal.Play();
        }
        if (roomStateManager == null)
        {
            roomStateManager = FindAnyObjectByType<RoomStateManager>();
        }

        if (inventory == null)
        {
            inventory = FindAnyObjectByType<PlayerInventory>();
        }
    }
    private void Reset()
    {
        interactAudioID = "switch"; 
    }

    protected override void OnInteract()
    {
        // Solo permitir interactuar (sea reparar o encender) si el jugador está dentro de la misma habitación
        PlayerRoomTracker tracker = FindAnyObjectByType<PlayerRoomTracker>();
        if (tracker == null || tracker.CurrentRoom != roomStateManager)
        {
            Debug.Log("[Interruptor] No puedes usar el interruptor desde fuera de la habitación.");
            return;
        }

        if (!estaReparado)
        {
            TryRepair();
            return;
        }

        if (roomStateManager == null)
        {
            Debug.LogWarning("Interruptor sin RoomStateManager asignado.");
            return;
        }

        // Fundido a blanco si pasaremos a Normal, fundido a negro si pasaremos a Pesadilla
        Color fadeColor = roomStateManager.IsNightmare ? Color.white : Color.black;

        ScreenFader.Instance.DoTransition(fadeColor, 0.4f, 0.1f, 0.4f, () =>
        {
            roomStateManager.ToggleWorld();
        });
    }

    private void TryRepair()
    {
        if (inventory == null)
        {
            Debug.LogWarning("No hay PlayerInventory en la escena.");
            return;
        }

        if (repairItem == null)
        {
            Debug.LogWarning("Interruptor sin repairItem asignado.");
            return;
        }

        if (!inventory.TryUseItem(repairItem))
        {
            Debug.Log("Esta roto, necesito una pieza");
            return;
        }

        estaReparado = true;
        audioSourceLocal.Stop();
        Debug.Log("Interruptor reparado");
    }
}
