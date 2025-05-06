using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CreateLevelButtons : MonoBehaviour
{
    // Type of Level and the button prefab used to dispaly them
    [SerializeField] GameManager.LevelType LevelType;
    [SerializeField] GameObject buttonPrefab;

    // Reference to Best Time Manager for Time Attack Levels
    private BTManager bestTimeManager;

    // Number of levels to display
    private int levelCount = 30;

    // Array of buttons in the Scene
    private GameObject[] levelBtns;

    // Start is called before the first frame update
    void Start()
    {
        // Get Best Time Manager
        bestTimeManager = GameManager.instance.GetComponent<BTManager>();

        // Create Level Button Objects
        levelBtns = new GameObject[levelCount];
        CreateButtons();
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Initialize the button gameobjects
    private void CreateButtons()
    {
        for (int i = 0; i < levelCount; i++)
        {
            GameObject button = Instantiate(buttonPrefab);
            button.name = "LevelBtn-" + (i + 1);
            SetStartValues(button.GetComponent<LevelSelectController>(), i + 1);
            button.transform.SetParent(transform, false);
            levelBtns[i] = button;
        }
    }

    // Initialize the controllers of all of the buttons depending on their type 
    private void SetStartValues(LevelSelectController controller, int currLvl)
    {
        switch (LevelType)
        {
            case GameManager.LevelType.NORMAL:
                controller.levelName = "NLevel-" + currLvl;
                if (currLvl == 1)
                    PlayerPrefs.SetInt(controller.levelName, 1);
                controller.bestTimeText = "";
                break;
            case GameManager.LevelType.TIME:
                controller.levelName = "TLevel-" + currLvl;
                controller.bestTimeText = bestTimeManager.GetBestTimeText(currLvl);
                break;
            default:
                Debug.LogError("Error: Invalid LevelType Detected");
                break;
        }

        controller.levelNum = currLvl;
    }

}
