using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcInteractShowInfo : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject uiObject;            // Main info popup
    public GameObject talkPrompt;          // "Press E to Talk" UI

    [Header("Timing")]
    public float fadeDuration = 1f;
    public float displayTime = 15f;
    public KeyCode interactKey = KeyCode.E;

    [Header("NPC Behavior")]
    public Transform npcModel;             // assign the NPC model, not the root
    public float rotateSpeed = 3f;         // how fast the NPC turns

    [Header("Audio")]
    public AudioSource audioSource;       // assign in inspector
    public AudioClip interactSound;       // sound to play when pressing E

    [Header("Checklist")]
    public string objectiveID;   // Must match the ID in ChecklistManager



    private CanvasGroup canvasGroup;
    private CanvasGroup promptCanvasGroup;

    private Coroutine currentFade;
    private Coroutine promptFade;

    private bool playerInRange = false;
    private bool isShowing = false;
    private Transform playerTransform;
    private bool shouldRotate = false;


    void Start()
    {
        // Setup main UI
        canvasGroup = uiObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = uiObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        uiObject.SetActive(false);

        // Setup talk prompt UI
        if (talkPrompt != null)
        {
            promptCanvasGroup = talkPrompt.GetComponent<CanvasGroup>();
            if (promptCanvasGroup == null)
                promptCanvasGroup = talkPrompt.AddComponent<CanvasGroup>();

            promptCanvasGroup.alpha = 0f;
            talkPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            ShowInfo();
            shouldRotate = true;   // start rotating toward player

        }

        if (shouldRotate && npcModel != null && playerTransform != null)
        {
            Vector3 direction = playerTransform.position - npcModel.position;
            direction.y = 0; // stay upright

            Quaternion targetRot = Quaternion.LookRotation(direction);
            npcModel.rotation = Quaternion.Slerp(
                npcModel.rotation,
                targetRot,
                Time.deltaTime * rotateSpeed
            );
        }

    }

    void OnTriggerEnter(Collider player)
    {
        if (player.CompareTag("Player"))
        {
            playerInRange = true;
            playerTransform = player.transform;

            // Fade in talk prompt
            if (talkPrompt != null)
            {
                if (promptFade != null) StopCoroutine(promptFade);
                talkPrompt.SetActive(true);
                promptFade = StartCoroutine(FadeCanvasGroup(promptCanvasGroup, 1f));
            }
        }
    }

    void OnTriggerExit(Collider player)
    {
        if (player.CompareTag("Player"))
        {
            playerInRange = false;

            // Fade out talk prompt
            if (talkPrompt != null)
            {
                if (promptFade != null) StopCoroutine(promptFade);
                promptFade = StartCoroutine(FadeCanvasGroup(promptCanvasGroup, 0f, disableAfterFade: talkPrompt));
            }
        }
    }

    void ShowInfo()
    {
        if (isShowing) return;

        isShowing = true;

        // Fade out prompt immediately
        if (talkPrompt != null)
        {
            if (promptFade != null) StopCoroutine(promptFade);
            promptFade = StartCoroutine(FadeCanvasGroup(promptCanvasGroup, 0f, disableAfterFade: talkPrompt));
        }

        // Show main info UI
        if (currentFade != null) StopCoroutine(currentFade);

        uiObject.SetActive(true);
        currentFade = StartCoroutine(FadeCanvas(1f));

        RotateTowardsPlayer();

        CancelInvoke(nameof(HideInfo));
        Invoke(nameof(HideInfo), displayTime);

        // Play interaction sound
        if (audioSource != null && interactSound != null)
        {
            audioSource.PlayOneShot(interactSound);
        }

        ChecklistManager.Instance.MarkCompleted(objectiveID);

    }

    void HideInfo()
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeCanvas(0f));
        isShowing = false;
        shouldRotate = false;  // stop rotating


        // Bring prompt back if player is still in range
        if (playerInRange && talkPrompt != null)
        {
            if (promptFade != null) StopCoroutine(promptFade);
            talkPrompt.SetActive(true);
            promptFade = StartCoroutine(FadeCanvasGroup(promptCanvasGroup, 1f));
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

        if (Mathf.Approximately(targetAlpha, 0f))
            uiObject.SetActive(false);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, GameObject disableAfterFade = null)
    {
        float startAlpha = cg.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        cg.alpha = targetAlpha;

        if (disableAfterFade != null && Mathf.Approximately(targetAlpha, 0f))
            disableAfterFade.SetActive(false);
    }

    void RotateTowardsPlayer()
    {
        if (npcModel == null || playerTransform == null) return;

        Vector3 direction = playerTransform.position - npcModel.position;
        direction.y = 0; // keep rotation horizontal only

        Quaternion targetRot = Quaternion.LookRotation(direction);

        // Smooth rotation
        StartCoroutine(SmoothRotate(targetRot));
    }

    IEnumerator SmoothRotate(Quaternion targetRot)
    {
        while (Quaternion.Angle(npcModel.rotation, targetRot) > 0.1f)
        {
            npcModel.rotation = Quaternion.Slerp(npcModel.rotation, targetRot, Time.deltaTime * rotateSpeed);
            yield return null;
        }

        npcModel.rotation = targetRot;
    }
}