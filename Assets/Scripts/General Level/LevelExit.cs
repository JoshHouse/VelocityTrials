using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelExit : MonoBehaviour
{
    // Current Scene index
    static int thisSceneIndex;

    // Number of built Scenes
    static int builtScenesCount;

    // Reference to the game'ss current state (Normal or Time Attack)
    GameManager.GameStates currState;

    // Reference to Best Time Manager for recording Best Times
    private BTManager bestTimeManager;

    // Timer's UI elements
    private TextMeshProUGUI timerUI;
    private Image timerPanel;

    // Time player is taking to complete level
    private float timer = 0.0f;


    // Start is called before the first frame update
    void Start()
    {
        // Get current scene index
        thisSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Get built scenes count
        builtScenesCount = SceneManager.sceneCountInBuildSettings;

        // Get Game's currentState
        currState = (GameManager.GameStates)Enum.GetValues(typeof(GameManager.GameStates)).GetValue((GameManager.instance.gameState));

        // Get Best Time Manager
        bestTimeManager = GameManager.instance.GetComponent<BTManager>();

        // Get timerUI and timerPanel
        timerUI = GameObject.Find("/UI/TimerUI").GetComponent<TextMeshProUGUI>();
        timerPanel = GameObject.Find("/UI/TimerPanel").GetComponent<Image>();
    }

    // Update is called every frame, if the MonoBehaviour is enabled
    private void Update()
    {
        // Incrment timer
        timer += Time.deltaTime;

        // If in Time Attack, display timer
        if (currState == GameManager.GameStates.IN_LEVEL_TIME)
        {
            timerUI.text = "Timer: " + string.Format("{0:0.00}", timer);
        }
        // If in Normal Mode, leave blank
        else
        {
            timerUI.text = "";
            timerPanel.enabled = false;
        }       
    }



    // Upon reaching Level Exit Door, load the next level and store the current best time (if better)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bestTimeManager.AddNewBestTime(thisSceneIndex, timer); // Submit Player's completion time for finishing level

            if (thisSceneIndex + 1 < builtScenesCount)
            {
                if (currState == GameManager.GameStates.IN_LEVEL_NORMAL) // Player is currently in a normal level
                {
                    PlayerPrefs.SetInt("NLevel-" + (thisSceneIndex + 1), 1);
                    PlayerPrefs.SetInt("TLevel-" + thisSceneIndex, 1);
                    SceneManager.LoadScene(thisSceneIndex + 1);
                } else if (currState == GameManager.GameStates.IN_LEVEL_TIME) // Play is currently in a time attack level
                {
                    bestTimeManager.AddNewBestTime(thisSceneIndex, timer); // Submit Player's completion time for finishing Time Attack

                    if (PlayerPrefs.GetInt("TLevel-" + thisSceneIndex) == 1) // The previous time attack level has already been unlocked
                    {
                        SceneManager.LoadScene(thisSceneIndex + 1); // Load into that next level
                    }
                    else // The previous time attack level has not been unlocked
                    {
                        SceneManager.LoadScene(0); // Load the main menu
                    }
                }
            }
            // If next Scene is not in build settings, load Main Menu
            else
            {
                Debug.Log("Loading Main Menu");
                SceneManager.LoadScene(0);
                GameManager.instance.ChangeGameState(GameManager.GameStates.OPENING);
            }
        }
    }


}
