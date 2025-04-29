using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{

    public static GameManager instance; // Singleton of the GameManager class
    public enum GameStates // The various states that the game can be in during operation
    {
        OPENING = 0,
        MENU = 1,
        TESTING = 2,
        IN_LEVEL_NORMAL = 3,
        IN_LEVEL_TIME = 4,
        EXIT = 5,
        GAME_OVER = 6,
    }
    public enum LevelType { NORMAL, TIME } // The two types of levels tha there can be
    public int gameState { get; private set; } // Controls what state the overall game is in, which can only be set in this class
    public float pSensitivity { get; private set; } // Controls the sensitivity of the player's camera

    /*---------------------------------Input Manager Variables--------------------------------------*/
    private PlayerInput pInput;                             // Player Input component
    private InputAction moveAction;                         // Input action for movement
    private InputAction jumpAction;                         // Input action for jumping
    private InputAction sprintAction;                       // Input action for sprinting
    private InputAction crouchAction;                       // Input action for crouching
    private InputAction slideAction;                        // Input action for sliding
    private InputAction grapplePullAction;                  // Input action for grappling (pulling)
    private InputAction grappleSwingAction;                 // Input action for grappling (swinging)

    public Vector2 moveInput { get; private set; }          // Holds button presses for movement and converts them to the required floats
                        /*------Jumping------*/
    public bool jumpPressed { get; private set; }           // Jump button was pressed
    public bool jumpHeld { get; private set; }              // Jump button was held
    public bool jumpReleased { get; private set; }          // Jump button was released

                        /*------Sprinting------*/
    public bool sprintPressed { get; private set; }         // Sprint button was pressed
    public bool sprintHeld { get; private set; }            // Sprint button was held
    public bool sprintReleased { get; private set; }        // Sprint button was released

                        /*------Crouching------*/
    public bool crouchPressed { get; private set; }         // Crouch button was pressed
    public bool crouchHeld { get; private set; }            // Crouch button was held
    public bool crouchReleased { get; private set; }        // Crouch button was released

                       /*------Sliding------*/
    public bool slidePressed { get; private set; }          // Slide button was pressed
    public bool slideHeld { get; private set; }             // Slide button was held
    public bool slideReleased { get; private set; }         // Slide button was released

                        /*------Grapple (Pull)------*/
    public bool gPullPressed { get; private set; }          // Grapple (Pull) button was pressed
    public bool gPullHeld { get; private set; }             // Grapple (Pull) button was held
    public bool gPullReleased { get; private set; }         // Grapple (Pull) button was released

                        /*------Grapple (Swing)------*/
    public bool gSwingPressed { get; private set; }         // Grapple (Swing) button was pressed
    public bool gSwingHeld { get; private set; }            // Grapple (Swing) button was held
    public bool gSwingReleased { get; private set; }        // Grapple (Swing) button was released
    /*----------------------------------------------------------------------------------------------*/

    private void Awake() 
    {
        /*------Singleton Initialization-------*/
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
        pInput = GetComponent<PlayerInput>();
        SetUpInputActions();
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
        UpdateInputs(); // Updates each input variable every frame
    }

    private void SetUpInputActions()
    {
        // Sets each of the InputAction by finding the specified name of each action in the Player Input component
        moveAction = pInput.actions["Movement"];
        jumpAction = pInput.actions["Jumping"];
        sprintAction = pInput.actions["Sprinting"];
        crouchAction = pInput.actions["Crouching"];
        slideAction = pInput.actions["Sliding"];
        grapplePullAction = pInput.actions["Grapple (Pull)"];
        grappleSwingAction = pInput.actions["Grapple (Swing)"];
    }

    private void UpdateInputs()
    {
        moveInput = moveAction.ReadValue<Vector2>(); // Reads the x and y movements and stores them in the vector2 variable

        // Returns true if the specified key was pressed down this frame
        jumpPressed = jumpAction.WasPressedThisFrame();
        sprintPressed = sprintAction.WasPressedThisFrame();
        crouchPressed = crouchAction.WasPressedThisFrame();
        slidePressed = slideAction.WasPressedThisFrame();
        gPullPressed = grapplePullAction.WasPressedThisFrame();
        gSwingPressed = grappleSwingAction.WasPressedThisFrame();

        // Returns true if the specified key is being held down
        jumpHeld = jumpAction.IsPressed();
        sprintHeld = sprintAction.IsPressed();
        crouchHeld = crouchAction.IsPressed();
        slideHeld = slideAction.IsPressed();
        gPullHeld = grapplePullAction.IsPressed();
        gSwingHeld = grappleSwingAction.IsPressed();

        // Returns true if the specified key was released this frame
        jumpReleased = jumpAction.WasReleasedThisFrame();
        sprintReleased = sprintAction.WasReleasedThisFrame();
        crouchReleased = crouchAction.WasReleasedThisFrame();
        slideReleased = slideAction.WasReleasedThisFrame();
        gPullReleased = grapplePullAction.WasReleasedThisFrame();
        gSwingReleased = grappleSwingAction.WasReleasedThisFrame();
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
}
