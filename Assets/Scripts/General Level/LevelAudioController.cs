using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAudioController : MonoBehaviour
{
    [Header("Level Background Music")]
    [SerializeField] private AudioClip levelMusic;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    
    private AudioManager audioManager;
    private Transform spawnPoint;
    private bool usingInternalMusicSource = false;

    private void Awake()
    {
        // Create a spawn point for the audio
        spawnPoint = transform;
        
        // Find AudioManager or create one if needed
        audioManager = FindObjectOfType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogWarning("AudioManager not found. Using internal music source instead.");
            usingInternalMusicSource = true;
        }
        
        // Initialize audio source if not set
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("Level Music Source");
            musicObj.transform.parent = transform;
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;
        }
    }

    private void Start()
    {
        // Play the level music
        if (levelMusic != null)
        {
            if (usingInternalMusicSource)
            {
                // Use the internal music source if AudioManager isn't available
                musicSource.clip = levelMusic;
                musicSource.Play();
            }
            else if (audioManager != null)
            {
                // Use AudioManager if available
                audioManager.PlayBGM(levelMusic, spawnPoint);
            }
        }
    }
}