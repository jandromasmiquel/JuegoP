using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UIScreenEffect : MonoBehaviour
{
    [SerializeField] private string effectID; // "Damage", "Heal", "Poison", etc.
    [SerializeField] private float duration = 0.4f;

    private CanvasGroup canvasGroup;
    private float timer;

    public string EffectID => effectID;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; // Empiezan invisibles
    }

    public void Play()
    {
        canvasGroup.alpha = 1f;
        timer = duration;
    }

    private void Update()
    {
        if (canvasGroup.alpha > 0f)
        {
            timer -= Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(timer / duration);
        }
    }
}