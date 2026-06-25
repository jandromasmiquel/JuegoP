using UnityEngine;

public class RoomVisibilityManager : MonoBehaviour
{
    private PlayerRoomTracker playerTracker;
    private RoomStateManager[] allRooms;
    private Puerta[] allDoors;
    private CameraController cameraCtrl; // Guardamos la referencia de la cámara

    private void Awake()
    {
        // Encontrar automáticamente los componentes en la escena
        playerTracker = FindAnyObjectByType<PlayerRoomTracker>();
        allRooms = FindObjectsByType<RoomStateManager>();
        allDoors = FindObjectsByType<Puerta>();
        cameraCtrl = FindAnyObjectByType<CameraController>(); // Buscamos nuestro script de zoom
    }

    private void OnEnable()
    {
        if (playerTracker != null)
        {
            playerTracker.RoomChanged += OnPlayerRoomChanged;
        }

        foreach (RoomStateManager room in allRooms)
        {
            room.StateChanged += OnRoomStateChanged;
        }
    }

    private void OnDisable()
    {
        if (playerTracker != null)
        {
            playerTracker.RoomChanged -= OnPlayerRoomChanged;
        }

        foreach (RoomStateManager room in allRooms)
        {
            room.StateChanged -= OnRoomStateChanged;
        }
    }

    private void Start()
    {
        UpdateSystemState();
    }

    private void OnPlayerRoomChanged(RoomStateManager newRoom)
    {
        UpdateSystemState();
    }

    private void OnRoomStateChanged(RoomState newState)
    {
        UpdateSystemState();
    }

    public void UpdateSystemState()
    {
        if (playerTracker == null)
        {
            return;
        }

        RoomStateManager currentRoom = playerTracker.CurrentRoom;
        if (currentRoom == null)
        {
            return;
        }

        if (cameraCtrl != null)
        {
            cameraCtrl.AlterarZoomPorEstado(currentRoom.CurrentState); 
            // Revisa si en tu RoomStateManager la propiedad se llama 'CurrentState' o 'currentState'
        }

        // 1. Actualizar visibilidad de cada habitación
        foreach (RoomStateManager room in allRooms)
        {
            if (room == currentRoom)
            {
                // La habitación actual del jugador siempre es visible y aplica su iluminación
                room.SetRoomVisibility(true);
                room.ApplyLighting();
            }
            else
            {
                // Una habitación distinta sólo es visible si es contigua y ambas (actual y contigua) están Normales
                bool isContiguousAndNormal = false;
                foreach (Puerta door in allDoors)
                {
                    if (door.Connects(currentRoom, room))
                    {
                        if (currentRoom.IsNormal && room.IsNormal)
                        {
                            isContiguousAndNormal = true;
                            break;
                        }
                    }
                }
                room.SetRoomVisibility(isContiguousAndNormal);
            }
        }

        // 2. Actualizar estado de las puertas
        foreach (Puerta door in allDoors)
        {
            door.UpdateDoorState(currentRoom);
        }
    }
}