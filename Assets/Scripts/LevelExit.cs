using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    // Current Scene index (preferably in build settings)
    static int thisSceneIndex;
    static int builtScenesCount;

    // Start is called before the first frame update
    void Start()
    {
        // Get current scene index
        thisSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Get built scenes count
        builtScenesCount = SceneManager.sceneCountInBuildSettings;
    }

    // Upon reaching Level Exit Door, load the next level and store the current best time (if better)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (thisSceneIndex + 1 < builtScenesCount)
            {
                SceneManager.LoadScene(thisSceneIndex + 1);
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
