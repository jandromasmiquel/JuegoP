using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Sub-Paneles de la UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject notificationPanel;

    [Header("Componentes de Texto")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI notificationText;

    [Header("Módulo de Vida")]
    [SerializeField] private Slider healthSlider; // Tu barra de vida visual

    [Header("Módulo de Efectos Visuales (Sprites)")]
    [SerializeField] private List<UIScreenEffect> screenEffects = new List<UIScreenEffect>();

    private Health playerHealth;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Buscamos específicamente al objeto que tenga el Tag de jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                // Nos suscribimos SOLO a los eventos del Player
                playerHealth.Changed += UpdateHealthUI;
                playerHealth.Damaged += HandlePlayerDamage;
                playerHealth.Healed += HandlePlayerHeal; // Escuchamos la curación

                // Forzamos la primera actualización manual para rellenar la barra de vida al arrancar
                UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }
    }

    private void OnDestroy()
    {
        // Limpieza estricta para evitar fugas de memoria
        if (playerHealth != null)
        {
            playerHealth.Changed -= UpdateHealthUI;
            playerHealth.Damaged -= HandlePlayerDamage;
            playerHealth.Healed -= HandlePlayerHeal;
        }
    }



    // ======================================================================
    // MÓDULO DE SALUD Y EFECTOS
    // ======================================================================
    
    public void TriggerEffect(string id)
    {
        // Busca el efecto correspondiente en la lista (Cura, Daño, Veneno...) y lo activa
        UIScreenEffect effect = screenEffects.FirstOrDefault(e => e.EffectID == id);
        if (effect != null)
        {
            effect.Play();
        }
        else
        {
            Debug.LogWarning($"El efecto de pantalla con ID '{id}' no está registrado en el UIManager.");
        }
    }

    private void HandlePlayerDamage()
    {
        TriggerEffect("Damage"); // Activa tu sprite de impacto de sangre modular

        // Meneo de cámara nativo de Cinemachine si el Player tiene el Impulse Source
        if (playerHealth.TryGetComponent<Unity.Cinemachine.CinemachineImpulseSource>(out var impulse))
        {
            impulse.GenerateImpulse();
        }
    }

    private void HandlePlayerHeal()
    {
        TriggerEffect("Heal"); // Activa tu sprite de aura verde modular
    }

    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
        }
    }

    // ======================================================================
    // MÓDULO DE DIÁLOGOS
    // ======================================================================
    public void ShowDialogue(string text)
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = text;
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
    }

    // ======================================================================
    // MÓDULO DE NOTIFICACIONES DEL ENTORNO
    // ======================================================================
    public void ShowWorldNotification(string message)
    {
        notificationPanel.SetActive(true);
        notificationText.text = message;

        CancelInvoke(nameof(HideWorldNotification));
        Invoke(nameof(HideWorldNotification), 2.5f);
    }

    private void HideWorldNotification()
    {
        notificationPanel.SetActive(false);
    }
}