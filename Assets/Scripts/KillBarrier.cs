using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBarrier : MonoBehaviour
{
    // Reference to player that can be "killed"
    public GameObject player;

    // Particles that shoot upwards when the player touches this Kill Barrier, simulating exploded pieces of Dummy2048
    public ParticleSystem deathParticles;

    // Position of player when Start() is called (where they are loaded in)
    private Vector3 spawnPos;

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

        // Get player's spawn position
        spawnPos = player.transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        // If player enters trigger, respawn them and play death effects where they died
        if (other.CompareTag("Player"))
        {
            // Play particle effect at death position
            deathParticles.transform.position = player.transform.position;
            deathParticles.Play();

            // Respawn player and nullify all velocity
            player.GetComponent<Rigidbody>().velocity = Vector3.zero;
            player.transform.position = spawnPos;
        }
    }
}
