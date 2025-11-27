using System.Collections;
using UnityEngine;

public class ShowUI : MonoBehaviour
{
    public GameObject uiObject;
    public float fadeDuration = 1f;  // how long the fade takes (in seconds)

    private CanvasGroup canvasGroup;
    private Coroutine currentFade;

    void Start()
    {
        // Make sure UI starts hidden
        canvasGroup = uiObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = uiObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        uiObject.SetActive(false);
    }

    void OnTriggerEnter(Collider player)
    {
        if (player.CompareTag("Player"))
        {
            if (currentFade != null) StopCoroutine(currentFade);
            uiObject.SetActive(true);
            currentFade = StartCoroutine(FadeCanvas(1f)); // Fade In
        }
    }

    void OnTriggerExit(Collider player)
    {
        if (player.CompareTag("Player"))
        {
            if (currentFade != null) StopCoroutine(currentFade);
            currentFade = StartCoroutine(FadeCanvas(0f)); // Fade Out
        }
    }

    IEnumerator FadeCanvas(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        // When fully faded out, disable the UI object
        if (Mathf.Approximately(targetAlpha, 0f))
            uiObject.SetActive(false);
    }
}