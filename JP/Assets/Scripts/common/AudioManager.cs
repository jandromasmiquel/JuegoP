using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources de la UI / Globales")]
    [SerializeField] private AudioSource sfx2DSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Base de Datos de Sonidos de Prueba")]
    [SerializeField] private List<SoundClip> soundClips;

    private Dictionary<string, AudioClip> audioDatabase = new Dictionary<string, AudioClip>();

    [System.Serializable]
    public struct SoundClip
    {
        public string id;
        public AudioClip clip;
    }

    private void Awake()
    {
        // Regla del Singleton Persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Volcar la lista en un diccionario para búsquedas rápidas por ID string
        foreach (var sound in soundClips)
        {
            if (!audioDatabase.ContainsKey(sound.id))
            {
                audioDatabase.Add(sound.id, sound.clip);
            }
        }
    }

    // 1. REPRODUCIR SONIDO 2D (Interfaces, menús, faders, música interna del player)
    public void PlaySFX2D(string id, float volume = 1f)
    {
        if (audioDatabase.TryGetValue(id, out AudioClip clip))
        {
            sfx2DSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] El sonido con ID '{id}' no existe.");
        }
    }

    // 2. REPRODUCIR SONIDO 3D (Interruptores, enemigos, pasos en el mundo)
    // Instancia un objeto temporal que se destruye solo al terminar el sonido
    public void PlaySFX3D(string id, Vector3 worldPosition, float volume = 1f, float spatialBlend = 1f)
    {
        if (audioDatabase.TryGetValue(id, out AudioClip clip))
        {
            GameObject tempGO = new GameObject($"TempAudio_{id}");
            tempGO.transform.position = worldPosition;

            AudioSource source = tempGO.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = spatialBlend; // 1.0f significa Audio 3D Puro
            source.minDistance = 2f;
            source.maxDistance = 15f;
            source.rolloffMode = AudioRolloffMode.Linear;

            source.Play();
            Destroy(tempGO, clip.length); // Se limpia solo de la jerarquía
        }
    }

    // 3. CONTROL DE MÚSICA / AMBIENTE (Loop continuo)
    public void PlayMusic(string id, float volume = 0.5f)
    {
        if (audioDatabase.TryGetValue(id, out AudioClip clip))
        {
            if (musicSource.clip == clip && musicSource.isPlaying) return;
            
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.volume = volume;
            musicSource.Play();
        }
    }

    public AudioClip GetAudioClip(string id)
    {
        if (audioDatabase.TryGetValue(id, out AudioClip clip)) return clip;
        return null;
    }
}