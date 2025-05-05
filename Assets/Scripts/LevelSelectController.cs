using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectController : MonoBehaviour
{
    // References to UI Elements to be controlled
    [SerializeField] Button thisButton;
    [SerializeField] RawImage lockImage;
    [SerializeField] TextMeshProUGUI levelNo;
    [SerializeField] TextMeshProUGUI bestTimeUI;

    // References to level's name, number, and best time text
    public string levelName;
    public int levelNum;
    public string bestTimeText;

    private void OnGUI()
    {
        // Find out of this level being controlled is unlocked or not, then act accordingly
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

    // Toggles the state of the level button depending on if it being locked or unlocked 
    public void IsLevelUnlocked()
    {
        if (PlayerPrefs.GetInt(levelName) == 0)
        {
            // Display only lock, non-interactable
            thisButton.interactable = false;
            lockImage.enabled = true;
            levelNo.enabled = false;
            bestTimeUI.enabled = false;
        }
        else
        {
            // Display only levelNo and bestTimeUI , interactable
            thisButton.interactable = true;
            lockImage.enabled = false;
            levelNo.enabled = true;
            bestTimeUI.enabled = true;

            // bestTimeText is set to "" by CreateLevelButtons.cs if level is normal, so update UI text regardless
            bestTimeUI.text = bestTimeText;
        }
    }

    // Load the level in Normal or Time Attack mode
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
