using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBarrier : MonoBehaviour
{
    // Reference to player that can be "killed"
    public GameObject player;

    // Particles that shoot upwards when the player touches this Kill Barrier, simulating exploded pieces of Dummy2048
    public ParticleSystem deathParticles;

    // AudioSource that plays explosion sound upon death
    private AudioSource explosionAudioSource;

    // Position of player when Start() is called (where they are loaded in)
    private Vector3 spawnPos;

    // Reference to AudioManager
    private AudioManager audioManager;
    private bool usingInternalMusicSource = false;

    // Start is called before the first frame update
    void Start()
    {
        // Find player if not initialized in Unity Editor
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        // Set particles to default particles if none specified
        if (deathParticles == null)
        {
            deathParticles = GetComponent<ParticleSystem>();
        }

        // Set explosionAudioSource to attached AudioSource
        explosionAudioSource = GetComponent<AudioSource>();

        // Get player's spawn position
        spawnPos = player.transform.position;

        // Find AudioManager or create one if needed
        audioManager = FindObjectOfType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogWarning("AudioManager not found. Using internal music source instead.");
            usingInternalMusicSource = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // If player enters trigger, respawn them and play death effects where they died
        if (other.CompareTag("Player"))
        {
            // Play particle effect at death position
            deathParticles.transform.position = player.transform.position;
            deathParticles.Play();

            // Play explosion SFX
            if (!usingInternalMusicSource)
            {
                audioManager.PlaySFXClip(explosionAudioSource.clip, player.transform);
            }
            else
            {
                explosionAudioSource.Play();
            }

            // Respawn player and nullify all velocity
            player.GetComponent<Rigidbody>().velocity = Vector3.zero;
            player.transform.position = spawnPos;
        }
    }
}
