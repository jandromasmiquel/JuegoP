using System.Collections;
using UnityEngine;

public class DimoniIntroSequence : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform playerStartPoint;

    [Header("Dimoni Cameo")]
    [SerializeField] private GameObject dimoniIntroObject;
    [SerializeField] private Transform dimoniStartPoint;
    [SerializeField] private Transform dimoniEndPoint;
    [SerializeField] private float dimoniRunDuration = 1.2f;

    [Header("Timing")]
    [SerializeField] private float startDelay = 0.4f;
    [SerializeField] private float endDelay = 0.25f;
    [SerializeField] private bool playOnStart = true;

    private bool hasPlayed;

    private void Start()
    {
        if (playOnStart)
        {
            Play();
        }
    }

    public void Play()
    {
        if (hasPlayed)
        {
            return;
        }

        hasPlayed = true;
        StartCoroutine(SequenceRoutine());
    }

    private IEnumerator SequenceRoutine()
    {
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<PlayerController>();
        }

        if (playerController != null)
        {
            playerController.SetControlsEnabled(false);

            if (playerStartPoint != null)
            {
                Rigidbody2D playerRb = playerController.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.linearVelocity = Vector2.zero;
                    playerRb.position = playerStartPoint.position;
                }

                playerController.transform.position = playerStartPoint.position;
            }
        }

        if (dimoniIntroObject != null && dimoniStartPoint != null)
        {
            dimoniIntroObject.transform.position = dimoniStartPoint.position;
            dimoniIntroObject.SetActive(true);
        }

        yield return new WaitForSeconds(startDelay);

        if (dimoniIntroObject != null && dimoniStartPoint != null && dimoniEndPoint != null)
        {
            float elapsed = 0f;
            while (elapsed < dimoniRunDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dimoniRunDuration);
                dimoniIntroObject.transform.position = Vector3.Lerp(dimoniStartPoint.position, dimoniEndPoint.position, t);
                yield return null;
            }

            dimoniIntroObject.transform.position = dimoniEndPoint.position;
            dimoniIntroObject.SetActive(false);
        }

        yield return new WaitForSeconds(endDelay);

        if (playerController != null)
        {
            playerController.SetControlsEnabled(true);
        }
    }
}
