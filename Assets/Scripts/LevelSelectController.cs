using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectController : MonoBehaviour
{

    [SerializeField] Button thisButton;
    [SerializeField] RawImage lockImage;
    [SerializeField] TextMeshProUGUI levelNo;
    public string levelName;
    public int levelNum;

    private void OnGUI()
    {
        if (!PlayerPrefs.HasKey(levelName))
        {
            PlayerPrefs.SetInt(levelName, 0);
        }
        IsLevelUnlocked();
    }

    private void FixedUpdate()
    {
        levelNo.text = levelNum.ToString();
    }

    public void IsLevelUnlocked()
    {
        if (PlayerPrefs.GetInt(levelName) == 0)
        {
            thisButton.interactable = false;
            lockImage.enabled = true;
            levelNo.enabled = false;
        }
        else
        {
            thisButton.interactable = true;
            lockImage.enabled = false;
            levelNo.enabled = true;
        }
    }

    public void LoadLevel()
    {
        char gamemode = levelName[0];
        int sceneIndex = levelNum;

        switch (gamemode)
        {
            case 'N':
                StartCoroutine(TitleManager.instance.EnterLevel(sceneIndex, GameManager.GameStates.IN_LEVEL_NORMAL));
                break;
            case 'T':
                StartCoroutine(TitleManager.instance.EnterLevel(sceneIndex, GameManager.GameStates.IN_LEVEL_TIME));
                break;
            default:
                Debug.LogError("Error: Failed to load game state. Game state does not exist.");
                break;
        }
        
    }

}
