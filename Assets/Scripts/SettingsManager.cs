using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance; // The instance of the settings manager

    [Header("Header & Footer Settings Controls")]
    [SerializeField] Button[] mainButtons;


    // Below are the components that are used on the controls portion of the settings page
    [Header("Player Control Settings Page")]
    [SerializeField] Slider sensitivitySlider;
    [SerializeField] Button[] buttons;
    [SerializeField] TextMeshProUGUI instructions;

    // Awake is called when the script is loaded in, before start
    private void Awake()
    {
        if (instance == null) // Ensure that only one settings manager exists. If none exist, make this instance the only instance
        {
            instance = this;
        }
        else
        {
            Destroy(instance); // If an instance already exists, destroy the duplicate instace
        }
    }

    /// <summary>
    /// Allows the user to change what key binds they want to use when playing the game.
    /// </summary>
    /// <param name="pressedButton"></param>
    public void SetKey(Button pressedButton)
    {
        // Store the value of the current KeyCode in case the user changes their mind
        string value = pressedButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
        KeyCode current = (KeyCode)System.Enum.Parse(typeof(KeyCode), value);

        // Prevent the user from pressing another button while selecting a key, this also includes the apply, back, and header buttons.
        foreach (Button b in mainButtons)
        {
            if (b != null)
                b.enabled = false;
        }

        foreach (Button b in buttons)
        {
            if (b != null && pressedButton != b) // Makes sure that only the pressed button is still active.
            {
                b.enabled = false;
            }
        }
        sensitivitySlider.enabled = false; // Prevent the user from using the slider component while selecting a key
        instructions.enabled = true; // Show the instructions that tell the user what to do

    }

}
