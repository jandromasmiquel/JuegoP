using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Módulo de Daño Visual")]
    [SerializeField] private CanvasGroup damageFlashCanvasGroup; // Panel rojo a pantalla completa
    [SerializeField] private float flashDuration = 0.4f;
    private float flashTimer;

    private Health playerHealth;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                // Adaptado a tus eventos nativos:
                playerHealth.Changed += UpdateHealthUI;
                playerHealth.Damaged += TriggerDamageEffects;

                UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }

        if (damageFlashCanvasGroup != null) damageFlashCanvasGroup.alpha = 0f;
    }

    private void Update()
    {
        // Gestión del fade-out del flash de daño de forma lineal sin necesidad de Corrutinas
        if (damageFlashCanvasGroup != null && damageFlashCanvasGroup.alpha > 0)
        {
            flashTimer -= Time.deltaTime;
            damageFlashCanvasGroup.alpha = Mathf.Clamp01(flashTimer / flashDuration);
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            // Limpieza con tus eventos nativos:
            playerHealth.Changed -= UpdateHealthUI;
            playerHealth.Damaged -= TriggerDamageEffects;
        }
    }

    // ======================================================================
    // MÓDULO DE SALUD Y EFECTOS DE DAÑO
    // ======================================================================
    
    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
        }
    }

    private void TriggerDamageEffects()
    {
        // 1. Activamos el flash de daño en pantalla (el Update se encarga de desvanecerlo)
        if (damageFlashCanvasGroup != null)
        {
            damageFlashCanvasGroup.alpha = 1f;
            flashTimer = flashDuration;
        }

        // 2. AGREGADO CINEMACHINE: Meneo de cámara nativo
        // Buscamos el componente Impulse Source en el Player si lo tienes configurado
        var impulse = playerHealth.GetComponent<Unity.Cinemachine.CinemachineImpulseSource>();
        if (impulse != null)
        {
            impulse.GenerateImpulse(); // Hace vibrar la cámara Cinemachine automáticamente
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