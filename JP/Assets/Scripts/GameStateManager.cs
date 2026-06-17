using UnityEngine;
using UnityEngine.Rendering.Universal; // Importante para controlar Light2D

public class GameStateManager : MonoBehaviour
{
    public GameObject normalGroup;
    public GameObject nightmareGroup;
    public Light2D globalLight; // Arrastra aquí tu luz desde el Inspector

    [Header("Configuración de Luces")]
    public float intensityNormal = 0.8f;
    public float intensityNightmare = 0.2f;
    public Color colorNormal = Color.white;
    public Color colorNightmare = new Color(0.1f, 0.1f, 0.3f); // Un tono azulado oscuro

    private bool isNightmare = true;

    void Start() => UpdateWorldState();

    public void ToggleWorld()
    {
        isNightmare = !isNightmare;
        UpdateWorldState();
    }

    void UpdateWorldState()
    {
        normalGroup.SetActive(!isNightmare);
        nightmareGroup.SetActive(isNightmare);

        // Cambiar propiedades de la luz única
        globalLight.intensity = isNightmare ? intensityNightmare : intensityNormal;
        globalLight.color = isNightmare ? colorNightmare : colorNormal;
    }
}