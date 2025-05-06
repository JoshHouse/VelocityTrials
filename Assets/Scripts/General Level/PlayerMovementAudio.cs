using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource movementSource;
    [SerializeField] private AudioSource environmentSource;
    
    [Header("Movement Audio Clips")]
    [SerializeField] private AudioClip walkingSound;
    [SerializeField] private AudioClip sprintingSound;
    [SerializeField] private AudioClip crouchingSound;
    [SerializeField] private AudioClip slidingSound;
    [SerializeField] private AudioClip airborneSound;
    [SerializeField] private AudioClip landingSound;
    [SerializeField] private AudioClip wallrunningSound;
    [SerializeField] private AudioClip wallClimbingSound;
    [SerializeField] private AudioClip grappleSound;

    // Reference to AudioManager
    private AudioManager audioManager;
    private bool usingInternalMusicSource = false;

    [Header("Volume Settings")]
    [SerializeField] [Range(0f, 1f)] private float movementVolume = 0.7f;
    [SerializeField] [Range(0f, 1f)] private float environmentVolume = 0.7f;

    // Reference to player movement manager
    private PlayerMovementManager playerMovementManager;
    private GroundedMovementScript groundedMovement;
    private SlidingScript slidingScript;
    
    // Movement state tracking
    private PlayerMovementManager.MovementState currentMovementState;
    private PlayerMovementManager.MovementState previousMovementState;
    private bool isMoving = false;
    private bool wasMoving = false;
    private bool wasGrounded = false;

    private void Awake()
    {
        // Find AudioManager or create one if needed
        audioManager = FindObjectOfType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogWarning("AudioManager not found. Using internal music source instead.");
            usingInternalMusicSource = true;
        }

        // Initialize audio sources if not set
        if (movementSource == null)
        {
            GameObject movementObject = new GameObject("Movement Source");
            movementObject.transform.parent = transform;
            movementSource = movementObject.AddComponent<AudioSource>();
            movementSource.loop = true;
            movementSource.playOnAwake = false;
            movementSource.volume = movementVolume;
        }

        if (environmentSource == null)
        {
            GameObject envObject = new GameObject("Environment Source");
            envObject.transform.parent = transform;
            environmentSource = envObject.AddComponent<AudioSource>();
            environmentSource.loop = false;
            environmentSource.playOnAwake = false;
            environmentSource.volume = environmentVolume;
        }
        
        // Get player movement manager reference
        playerMovementManager = FindObjectOfType<PlayerMovementManager>();
        if (playerMovementManager != null)
        {
            groundedMovement = playerMovementManager.GetComponent<GroundedMovementScript>();
            if (groundedMovement != null)
            {
                slidingScript = groundedMovement.sS;
            }
        }
    }

    private void Update()
    {
        // Verify we have reference to player movement
        if (playerMovementManager == null)
        {
            playerMovementManager = FindObjectOfType<PlayerMovementManager>();
            if (playerMovementManager == null) return;
        }
            
        // Check for landing
        bool isGroundedNow = playerMovementManager.grounded;
        if (!wasGrounded && isGroundedNow)
        {
            PlayLandingSound();
        }
        
        wasGrounded = isGroundedNow;
        
        // Update movement audio based on current state
        UpdateMovementAudio();
    }

    // Play looping movement sound
    public void PlayMovementSound(AudioClip clip)
    {
        if (clip != null && (movementSource.clip != clip || !movementSource.isPlaying))
        {
            if (!usingInternalMusicSource)
            {
                audioManager.PlaySFXClip(clip, playerMovementManager.transform);
            }
            else
            {
                movementSource.clip = clip;
                if (!movementSource.isPlaying)
                {
                    movementSource.Play();
                }
            }
        }
    }

    // Stop movement sound
    public void StopMovementSound()
    {
        if (movementSource.isPlaying)
        {
            movementSource.Stop();
        }
    }

    // Play landing sound (one-shot)
    public void PlayLandingSound()
    {
        if (landingSound != null)
        {
            environmentSource.PlayOneShot(landingSound);
        }
    }
    
    // Update movement audio based on current player state
    private void UpdateMovementAudio()
    {
        if (playerMovementManager == null) return;
        
        // Save previous state
        previousMovementState = currentMovementState;
        wasMoving = isMoving;
        
        // Get current movement state
        currentMovementState = playerMovementManager.movementState;
        
        // Check if player is actually moving (has horizontal velocity)
        Rigidbody rb = playerMovementManager.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            isMoving = horizontalVelocity.magnitude > 0.1f;
        }
        
        // Handle state changes
        if (currentMovementState != previousMovementState || isMoving != wasMoving)
        {
            HandleMovementStateChange();
        }
    }
    
    // Handle audio changes when movement state changes
    private void HandleMovementStateChange()
    {
        // Stop current sound if player stops moving (except for certain states)
        if (!isMoving && 
            currentMovementState != PlayerMovementManager.MovementState.airborne && 
            currentMovementState != PlayerMovementManager.MovementState.grappleSwinging &&
            currentMovementState != PlayerMovementManager.MovementState.grapplePulling &&
            currentMovementState != PlayerMovementManager.MovementState.wallRunning &&
            currentMovementState != PlayerMovementManager.MovementState.wallClimbing)
        {
            StopMovementSound();
            return;
        }

        // Play appropriate sound based on movement state
        switch (currentMovementState)
        {
            case PlayerMovementManager.MovementState.walking:
                if (isMoving) PlayMovementSound(walkingSound);
                break;
            case PlayerMovementManager.MovementState.sprinting:
                if (isMoving) PlayMovementSound(sprintingSound);
                break;
            case PlayerMovementManager.MovementState.crouching:
                if (isMoving) PlayMovementSound(crouchingSound);
                break;
            case PlayerMovementManager.MovementState.sliding:
                PlayMovementSound(slidingSound); 
                break;
            case PlayerMovementManager.MovementState.airborne:
                PlayMovementSound(airborneSound);
                break;
            case PlayerMovementManager.MovementState.wallRunning:
                PlayMovementSound(wallrunningSound);
                break;
            case PlayerMovementManager.MovementState.wallClimbing:
                PlayMovementSound(wallClimbingSound);
                break;
            case PlayerMovementManager.MovementState.grappleSwinging:
            case PlayerMovementManager.MovementState.grapplePulling:
                PlayMovementSound(grappleSound);
                break;
            default:
                StopMovementSound();
                break;
        }
    }
}