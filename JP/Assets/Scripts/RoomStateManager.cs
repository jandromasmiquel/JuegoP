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

    public void SetState(RoomState state)
    {
        CurrentState = state;
        UpdateWorldState();
        StateChanged?.Invoke(CurrentState);
    }

    private void UpdateWorldState()
    {
        if (normalGroup != null)
        {
            normalGroup.SetActive(IsNormal);
        }

        if (nightmareGroup != null)
        {
            nightmareGroup.SetActive(IsNightmare);
        }

        if (globalLight != null)
        {
            globalLight.intensity = IsNightmare ? intensityNightmare : intensityNormal;
            globalLight.color = IsNightmare ? colorNightmare : colorNormal;
        }
    }
}
