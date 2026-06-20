using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RoomTrigger : MonoBehaviour
{
    [SerializeField] private RoomStateManager roomManager;
    public RoomStateManager RoomManager => roomManager;

    private void Awake()
    {
        // Try to automatically find the RoomStateManager on this object or in its parents
        if (roomManager == null)
        {
            roomManager = GetComponentInParent<RoomStateManager>();
        }

        // Ensure the collider is configured as a trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerRoomTracker tracker = other.GetComponent<PlayerRoomTracker>();
        if (tracker == null)
        {
            tracker = other.GetComponentInParent<PlayerRoomTracker>();
        }

        if (tracker != null && roomManager != null)
        {
            tracker.SetCurrentRoom(roomManager);
        }
    }
}
