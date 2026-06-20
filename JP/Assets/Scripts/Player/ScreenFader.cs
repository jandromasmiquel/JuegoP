using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    private static ScreenFader instance;

    public static ScreenFader Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("ScreenFaderManager");
                instance = obj.AddComponent<ScreenFader>();
            }
            return instance;
        }
    }

    private Canvas canvas;
    private Image fadeImage;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        CreateFadeCanvas();
    }

    private void CreateFadeCanvas()
    {
        // Crear GameObject para el Canvas
        GameObject canvasObj = new GameObject("ScreenFadeCanvas");
        canvasObj.transform.SetParent(transform);

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Asegura que se dibuje encima de todo

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Crear GameObject para la imagen del fundido
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = Color.clear;

        // Hacer que la imagen cubra toda la pantalla
        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public void DoTransition(Color fadeColor, float fadeOutDuration, float holdDuration, float fadeInDuration, Action onMidpoint)
    {
        StopAllCoroutines();
        StartCoroutine(TransitionRoutine(fadeColor, fadeOutDuration, holdDuration, fadeInDuration, onMidpoint));
    }

    private IEnumerator TransitionRoutine(Color fadeColor, float fadeOutDuration, float holdDuration, float fadeInDuration, Action onMidpoint)
    {
        float elapsed = 0f;
        Color startColor = Color.clear;

        // 1. Fundido hacia el color objetivo
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            fadeImage.color = Color.Lerp(startColor, fadeColor, t);
            yield return null;
        }
        fadeImage.color = fadeColor;

        // 2. Acción intermedia (cambio de estado / teletransporte)
        onMidpoint?.Invoke();

        // 3. Mantener el color en pantalla
        if (holdDuration > 0)
        {
            yield return new WaitForSeconds(holdDuration);
        }

        // 4. Fundido de vuelta a transparente
        elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            fadeImage.color = Color.Lerp(fadeColor, Color.clear, t);
            yield return null;
        }
        fadeImage.color = Color.clear;
    }
}
