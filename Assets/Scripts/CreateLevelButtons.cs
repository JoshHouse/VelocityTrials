using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CreateLevelButtons : MonoBehaviour
{

    [SerializeField] GameManager.LevelType LevelType;
    [SerializeField] GameObject buttonPrefab;

    private int levelCount = 30;
    private GameObject[] levelBtns;

    private void Awake()
    {
        levelBtns = new GameObject[levelCount];
        CreateButtons();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

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

    private void SetStartValues(LevelSelectController controller, int currLvl)
    {
        switch (LevelType)
        {
            case GameManager.LevelType.NORMAL:
                controller.levelName = "NLevel-" + currLvl;
                if (currLvl == 1)
                    PlayerPrefs.SetInt(controller.levelName, 1);
                break;
            case GameManager.LevelType.TIME:
                controller.levelName = "TLevel-" + currLvl;
                break;
            default:
                Debug.LogError("Error: Invalid LevelType Detected");
                break;
        }

        controller.levelNum = currLvl;
    }

}
