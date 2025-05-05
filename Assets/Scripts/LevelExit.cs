using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    // Current Scene index
    static int thisSceneIndex;

    // Number of built Scenes
    static int builtScenesCount;

    // Reference to Best Time Manager for recording Best Times
    private BTManager bestTimeManager;


    // Start is called before the first frame update
    void Start()
    {
        // Get current scene index
        thisSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Get built scenes count
        builtScenesCount = SceneManager.sceneCountInBuildSettings;

        // Get Best Time Manager
        bestTimeManager = GameManager.instance.GetComponent<BTManager>();
    }

    // Upon reaching Level Exit Door, load the next level and store the current best time (if better)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (thisSceneIndex + 1 < builtScenesCount)
            {
                // Check the games current state
                GameManager.GameStates currState = (GameManager.GameStates) Enum.GetValues(typeof(GameManager.GameStates)).GetValue((GameManager.instance.gameState));
                if (currState == GameManager.GameStates.IN_LEVEL_NORMAL) // Player is currently in a normal level
                {
                    PlayerPrefs.SetInt("NLevel-" + (thisSceneIndex + 1), 1);
                    PlayerPrefs.SetInt("TLevel-" + thisSceneIndex, 1);
                    SceneManager.LoadScene(thisSceneIndex + 1);
                } else if (currState == GameManager.GameStates.IN_LEVEL_TIME) // Play is currently in a time attack level
                {
                    bestTimeManager.AddNewBestTime(thisSceneIndex, 1000f); // Submit Player's completion time for finishing Time Attack

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
            }
        }
    }


}
