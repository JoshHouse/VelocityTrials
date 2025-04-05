using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    public enum GameStates // The various states that the game can be in during operation
    {
        OPENING = 0,
        MENU = 1,
        TESTING = 2,
    }
    public int gameState { get; private set; }
    public float pSensitivity { get; private set; }

    public Vector2 pMovement { get; private set; }
    public bool pJumping { get; private set; }
    public bool pCrouching { get; private set; }
    public bool pSprinting { get; private set; }
    public bool pSliding { get; private set; }
    public bool pGrappling { get; private set; }

    PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private InputAction slideAction;
    private InputAction grappleAction;

    private void Awake() 
    {
        if (instance == null)
        {
            instance = this; // Instantiate this instance as the only instance of Game Manager
            DontDestroyOnLoad(gameObject); // Prevent this instance from being destroyed on load
        }
        else
        {
            Destroy(instance); // If another instance of Game Manager exists, destroy this instance
        }

        // On awake, go ahead and fetch the player input component, then use it to set the player's actions for their controls
        playerInput = GetComponent<PlayerInput>();
        SetPlayerActions();
        pSensitivity = PlayerPrefs.GetFloat("Sensitivity", 800);
    }


    // Start is called before the first frame update
    void Start()
    {

        gameState = (int) GameStates.OPENING;
        if (gameState == (int)GameStates.OPENING)
        {
            StartCoroutine(TitleManager.instance.OpeningAnimation(1f));
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeGameState(GameStates state)
    {
        gameState = (int) state;
        Debug.Log("Game State changed to: " + Enum.GetValues(typeof(GameStates)).GetValue(gameState));
    }

    /// <summary>
    /// Allows the sensitivity to be updated by an exterior script
    /// </summary>
    /// <param name="value"></param>
    public void UpdateSensitivity(float value)
    {
        PlayerPrefs.SetFloat("Sensitivity", value);
        pSensitivity = PlayerPrefs.GetFloat("Sensitivity");
    }

    private void SetPlayerActions()
    {
        moveAction = playerInput.actions["Movement"];
        jumpAction = playerInput.actions["Jumping"];
        sprintAction = playerInput.actions["Sprinting"];
        crouchAction = playerInput.actions["Crouching"];
        slideAction = playerInput.actions["Sliding"];
        grappleAction = playerInput.actions["Grappling Hook"];
    }

    private void UpdatePlayerActions()
    {
        pMovement = moveAction.ReadValue<Vector2>();
        pJumping = jumpAction.WasPressedThisFrame();
        pCrouching = crouchAction.IsPressed();
        pSprinting = sprintAction.IsPressed();
        pSliding = slideAction.IsPressed();
        pGrappling = grappleAction.IsPressed();
    }
}
