using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepScript : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip footstepClip;

    [Header("Movement Settings")]
    public float walkStepInterval = 0.5f;     // slower intervals
    public float runStepInterval = 0.3f;      // faster intervals
    public float runSpeedThreshold = 3.5f;   // adjust based on your player’s speed


    private float stepTimer;

    void Update()
    {

        // --- Movement Speed ---
        float speed = new Vector3(GetComponent<Rigidbody>().velocity.x, 0f, GetComponent<Rigidbody>().velocity.z).magnitude;

        if (speed > 0.1f) // player is moving
        {
            bool isRunning = speed > runSpeedThreshold;

            float currentInterval = isRunning ? runStepInterval : walkStepInterval;

            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = currentInterval;
            }
        }
        else
        {
            stepTimer = 0f; // reset when stopping
        }
    }

    void PlayFootstep()
    {
        audioSource.pitch = Random.Range(0.85f, 1.15f); // makes it natural
        audioSource.PlayOneShot(footstepClip);
    }
}