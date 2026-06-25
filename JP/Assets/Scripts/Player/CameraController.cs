using UnityEngine;
using Unity.Cinemachine; // Asegúrate de tener este namespace según tu versión

public class CameraController : MonoBehaviour
{
    [Header("Zoom Config")]
    [SerializeField] private float normalZoom = 5f;
    [SerializeField] private float nightmareZoom = 3.5f;
    [SerializeField] private float zoomSpeed = 4f;

    private CinemachineCamera vCam;
    private float targetZoom;

    private void Awake()
    {
        vCam = GetComponent<CinemachineCamera>();
        targetZoom = normalZoom;
    }

    private void Update()
    {
        if (vCam == null) return;

        // Transición suave del zoom de la cámara
        if (!Mathf.Approximately(vCam.Lens.OrthographicSize, targetZoom))
        {
            vCam.Lens.OrthographicSize = Mathf.MoveTowards(
                vCam.Lens.OrthographicSize, 
                targetZoom, 
                zoomSpeed * Time.deltaTime
            );
        }
    }

    public void AlterarZoomPorEstado(RoomState estado)
    {
        targetZoom = (estado == RoomState.Nightmare) ? nightmareZoom : normalZoom;
    }
}