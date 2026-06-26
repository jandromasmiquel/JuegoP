using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum RoomState
{
    Normal,
    Nightmare
}

public class RoomStateManager : MonoBehaviour
{
    [Header("Room Groups")]
    [SerializeField] private GameObject entireRoomGroup;
    [SerializeField] private GameObject normalGroup;
    [SerializeField] private GameObject nightmareGroup;

    [Header("Lighting")]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private float intensityNormal = 0.8f;
    [SerializeField] private float intensityNightmare = 0.2f;
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorNightmare = new Color(0.1f, 0.1f, 0.3f);

    [Header("State")]
    [SerializeField] private RoomState initialState = RoomState.Nightmare;

    public event Action<RoomState> StateChanged;
    public RoomState CurrentState { get; private set; }
    public bool IsNormal => CurrentState == RoomState.Normal;
    public bool IsNightmare => CurrentState == RoomState.Nightmare;

    private void Start()
    {
        SetState(initialState);
    }

    public void ToggleWorld()
    {
        SetState(IsNightmare ? RoomState.Normal : RoomState.Nightmare);
    }

    private bool isRoomVisible = true;

    public void SetRoomVisibility(bool visible)
    {
        isRoomVisible = visible;
        UpdateWorldState();
    }

    public void SetState(RoomState state)
    {
        CurrentState = state;
        UpdateWorldState();
        StateChanged?.Invoke(CurrentState);
    }

    public void ApplyLighting()
    {
        if (globalLight != null)
        {
            globalLight.enabled = true;
            globalLight.intensity = IsNightmare ? intensityNightmare : intensityNormal;
            globalLight.color = IsNightmare ? colorNightmare : colorNormal;
        }
    }

    private void UpdateWorldState()
    {
        if (!isRoomVisible)
        {
            if (entireRoomGroup != null)
            {
                entireRoomGroup.SetActive(false);
            }
            else
            {
                if (normalGroup != null)
                {
                    normalGroup.SetActive(false);
                }
                if (nightmareGroup != null)
                {
                    nightmareGroup.SetActive(false);
                }
            }
            return;
        }

        if (entireRoomGroup != null)
        {
            entireRoomGroup.SetActive(true);
        }

        if (normalGroup != null)
        {
            normalGroup.SetActive(IsNormal);
        }

        if (nightmareGroup != null)
        {
            nightmareGroup.SetActive(IsNightmare);
        }
    }
}
