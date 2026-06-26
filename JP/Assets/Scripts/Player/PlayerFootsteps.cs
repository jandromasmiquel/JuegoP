using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("Configuración de Detección")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float detectionRadius = 0.2f;

    [Header("IDs de Audio (Registrados en AudioManager)")]
    [SerializeField] private string defaultSurfaceID = "paso_madera_1";
    
    // Cacheamos el AudioSource local del Player para usar PlayOneShot
    private AudioSource localAudioSource;

    private void Awake()
    {
        localAudioSource = GetComponent<AudioSource>();
        if (localAudioSource == null)
        {
            // Si se nos olvida ponerlo en el inspector, lo añadimos nosotros
            localAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configuración para pasos en el Player
        localAudioSource.spatialBlend = 0f; // 2D puro
        localAudioSource.playOnAwake = false;
        localAudioSource.loop = false;
    }

    // ESTE MÉTODO LO LLAMARÁ LA ANIMACIÓN
    public void PlayFootstep()
    {
        // Lanzamos un pequeño círculo en la posición de los pies del jugador
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, groundLayer);
        
        string surfaceAudioID = defaultSurfaceID;

        if (hit != null)
        {   
            Debug.Log("hit: " + hit.name + " tag: " + hit.tag);
            // Evaluamos el Tag del objeto del suelo que hemos pisado
            switch (hit.tag)
            {
                case "suelo_madera":
                    surfaceAudioID = "paso_madera_" + Random.Range(1, 3); // SueloMadera tiene 2 clips, elegimos uno al azar
                    break;
                case "suelo_charco":
                    surfaceAudioID = "paso_agua_" + Random.Range(1, 3);
                    break;
                case "suelo_metal":
                    surfaceAudioID = "paso_metal_" + Random.Range(1, 3);
                    break;
                case "suelo_baldosa":
                    surfaceAudioID = "paso_baldosa_" + Random.Range(1, 3);
                    break;
                default:
                    surfaceAudioID = defaultSurfaceID;
                    break;
            }
        }

        // Buscamos el clip en nuestra base de datos global del AudioManager
        AudioClip clipToPlay = AudioManager.Instance.GetAudioClip(surfaceAudioID);

        if (clipToPlay != null)
        {
            // Usamos PlayOneShot para que los sonidos de los pasos se solapen de forma orgánica
            // y variamos ligeramente el pitch para que no suene robótico
            localAudioSource.pitch = Random.Range(0.85f, 1.15f);
            localAudioSource.PlayOneShot(clipToPlay, 0.8f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Para ver el radio de detección de suelo en el editor
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}