using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerFootsteps : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource footstepSource;
    public AudioClip[] grassSteps;
    public AudioClip[] gravelSteps;

    [Header("Settings")]
    public float stepInterval = 10f;
    public float movementThreshold = 0.05f;

    private Terrain terrain;
    private float stepTimer = 0f;

    private Vector3 lastPosition;

    void Start()
    {
        terrain = Terrain.activeTerrain;
        lastPosition = transform.position;
    }

    void Update()
    {
        // Calculate how far the player moved since last frame
        float speed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
        Debug.Log("Speed: " + speed);


        if (speed > movementThreshold)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();

                // Adjust the interval dynamically:
                // Faster speed = shorter interval (but with a lower limit)
                float dynamicInterval = Mathf.Clamp(2.5f / speed, 0.35f, 0.8f);
                stepTimer = dynamicInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }


    void PlayFootstep()
    {
        Debug.Log("Footstep triggered!");


        string groundType = GetTerrainTexture();

        AudioClip clip = null;
        if (groundType == "Grass" && grassSteps.Length > 0)
            clip = grassSteps[Random.Range(0, grassSteps.Length)];
        else if (groundType == "Gravel" && gravelSteps.Length > 0)
            clip = gravelSteps[Random.Range(0, gravelSteps.Length)];

        if (clip != null)
        {
            footstepSource.pitch = Random.Range(0.95f, 1.05f);
            Debug.Log("Selected clip: " + clip);
            footstepSource.PlayOneShot(clip);
        }
    }

    string GetTerrainTexture()
    {
        if (terrain == null) return "Unknown";

        // Convert player position to terrain coordinates
        Vector3 terrainPos = terrain.transform.position;
        TerrainData terrainData = terrain.terrainData;

        int mapX = Mathf.RoundToInt(((transform.position.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
        int mapZ = Mathf.RoundToInt(((transform.position.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

        // Get the mix of textures at this point
        mapX = Mathf.Clamp(mapX, 0, terrainData.alphamapWidth - 1);
        mapZ = Mathf.Clamp(mapZ, 0, terrainData.alphamapHeight - 1);

        float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        // Each layer corresponds to a texture in the Terrain’s layer list
        int textureIndex = 0;
        float strongest = 0;

        for (int i = 0; i < terrainData.alphamapLayers; i++)
        {
            if (splatmapData[0, 0, i] > strongest)
            {
                textureIndex = i;
                strongest = splatmapData[0, 0, i];
            }
        }

        // Map index to texture name
        string texName = terrainData.terrainLayers[textureIndex].diffuseTexture.name;
        Debug.Log("Detected texture: " + texName);

        if (texName.ToLower().Contains("grass")) return "Grass";
        if (texName.ToLower().Contains("dirt") || texName.ToLower().Contains("gravel")) return "Gravel";

        return "Unknown";
    }
}
