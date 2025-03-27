using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    

    public enum GameStates
    {
        OPENING = 0,
        MENU = 1,
        TESTING = 2,
    }
    public int gameState { get; private set; }

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
    }

    public KeyCode pJumpButton {  get; private set; }
    public KeyCode pSprintButton { get; private set; }
    public KeyCode pCrouchButton { get; private set; }
    public KeyCode pGrappleButton { get; private set; }
    public float pSensitivity {  get; private set; }


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

    public void ChangeGameState(int stateNum)
    {
        gameState = (int) Enum.GetValues(typeof(GameStates)).GetValue(stateNum);
        Debug.Log("Game State changed to: " + Enum.GetValues(typeof(GameStates)).GetValue(gameState));
    }

    public void SetPlayerControls()
    {
        // TODO: Add Functionality
    }
}
