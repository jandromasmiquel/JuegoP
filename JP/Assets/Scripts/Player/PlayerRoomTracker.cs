using UnityEngine;
using System;

public class PlayerRoomTracker : MonoBehaviour
{
    [SerializeField] private RoomStateManager initialRoom;
    
    public RoomStateManager CurrentRoom { get; private set; }

    public event Action<RoomStateManager> RoomChanged;

    private void Start()
    {
        if (initialRoom != null)
        {
            SetCurrentRoom(initialRoom);
        }
        else
        {
            // Intentar detectar automáticamente en qué habitación inicia el jugador
            Collider2D[] overlaps = Physics2D.OverlapPointAll(transform.position);
            foreach (Collider2D col in overlaps)
            {
                RoomTrigger trigger = col.GetComponent<RoomTrigger>();
                if (trigger != null && trigger.RoomManager != null)
                {
                    SetCurrentRoom(trigger.RoomManager);
                    break;
                }
            }
        }
    }

    public void SetCurrentRoom(RoomStateManager room)
    {
        if (room == null || CurrentRoom == room)
        {
            return;
        }
        
        CurrentRoom = room;
        Debug.Log($"[PlayerRoomTracker] Player entered room: {room.gameObject.name}");
        RoomChanged?.Invoke(CurrentRoom);
    }
}
