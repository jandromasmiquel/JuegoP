using UnityEngine;

public class Puerta : MonoBehaviour, IInteractable
{
    [Header("Connected Rooms")]
    [SerializeField] private RoomStateManager roomA;
    [SerializeField] private RoomStateManager roomB;

    [Header("Spawn Points")]
    [SerializeField] private Transform spawnPointA;
    [SerializeField] private Transform spawnPointB;

    [Header("Door Configuration")]
    [SerializeField] private Collider2D physicsCollider; // Colisionador sólido para bloquear al jugador
    [SerializeField] private GameObject visualOpen;
    [SerializeField] private GameObject visualClosed;

    private bool isInteractable;
    private RoomStateManager targetRoomForTeleport;
    private Transform targetSpawnPoint;

    public bool Connects(RoomStateManager r1, RoomStateManager r2)
    {
        return (roomA == r1 && roomB == r2) || (roomA == r2 && roomB == r1);
    }

    public void UpdateDoorState(RoomStateManager currentRoom)
    {
        if (roomA == null || roomB == null)
        {
            Debug.LogWarning($"[Puerta] La puerta '{gameObject.name}' no tiene asignadas ambas habitaciones (Room A y Room B) en el inspector.");
            return;
        }

        // Si el jugador no está en ninguna de las habitaciones de la puerta, se bloquea por defecto
        if (currentRoom != roomA && currentRoom != roomB)
        {
            SetDoorAppearance(false); // Cerrada
            isInteractable = false;
            targetRoomForTeleport = null;
            targetSpawnPoint = null;
            return;
        }

        RoomStateManager contiguousRoom = (currentRoom == roomA) ? roomB : roomA;
        Transform contiguousSpawnPoint = (currentRoom == roomA) ? spawnPointB : spawnPointA;

        if (currentRoom.IsNightmare)
        {
            // El jugador está en pesadilla, la puerta está cerrada y no se puede interactuar (debe solucionar la habitación actual)
            SetDoorAppearance(false); // Cerrada
            isInteractable = false;
            targetRoomForTeleport = null;
            targetSpawnPoint = null;
        }
        else
        {
            // La habitación actual está normal
            if (contiguousRoom.IsNormal)
            {
                // La contigua está normal -> Puerta abierta, paso libre caminando
                SetDoorAppearance(true); // Abierta
                isInteractable = false;
                targetRoomForTeleport = null;
                targetSpawnPoint = null;
            }
            else
            {
                // La contigua está en pesadilla -> Puerta cerrada, hay que interactuar para cruzar (teletransporte)
                SetDoorAppearance(false); // Cerrada
                isInteractable = true;
                targetRoomForTeleport = contiguousRoom;
                targetSpawnPoint = contiguousSpawnPoint;
            }
        }
    }

    private void SetDoorAppearance(bool open)
    {
        if (physicsCollider != null)
        {
            // Si está abierta, desactivamos el colisionador físico para que pase caminando.
            // Si está cerrada, lo activamos para bloquear el movimiento físico.
            physicsCollider.enabled = !open;
        }

        if (visualOpen != null)
        {
            visualOpen.SetActive(open);
        }

        if (visualClosed != null)
        {
            visualClosed.SetActive(!open);
        }
    }

    public void Interact()
    {
        if (!isInteractable || targetRoomForTeleport == null || targetSpawnPoint == null)
        {
            Debug.Log("[Puerta] La puerta está cerrada y no puedes cruzar ahora mismo.");
            return;
        }

        // Almacenamos temporalmente en variables locales antes de que se actualice el estado sincrónicamente
        RoomStateManager destinationRoom = targetRoomForTeleport;
        Transform spawnPoint = targetSpawnPoint;
        string destinationName = destinationRoom.gameObject.name;

        // Teletransportar al jugador al punto de spawn correspondiente buscando el tracker en la escena
        PlayerRoomTracker tracker = FindAnyObjectByType<PlayerRoomTracker>();
        if (tracker != null)
        {
            GameObject player = tracker.gameObject;
            
            // Desactivamos la velocidad del Rigidbody2D temporalmente para evitar inercias
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.position = spawnPoint.position;
            }
            player.transform.position = spawnPoint.position;

            // Aseguramos que la habitación de destino se ponga en modo Pesadilla y se ejecute su lógica
            destinationRoom.SetState(RoomState.Nightmare);

            // Actualizamos la habitación actual del jugador inmediatamente en su tracker
            tracker.SetCurrentRoom(destinationRoom);

            Debug.Log($"[Puerta] Jugador cruzó a {destinationName} en modo pesadilla.");
        }
        else
        {
            Debug.LogError("[Puerta] No se encontró el componente PlayerRoomTracker en la escena.");
        }
    }
}
