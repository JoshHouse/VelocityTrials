using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSliders : MonoBehaviour
{

    // Referemce tp the components within the Audio settings section from Unity.
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider voiceSlider;
    [SerializeField] Button muteButton;

    private Color muteOff;
    private Color muteOn;
    private Image muteImage; // Reference to the button's image component for color changing

    // Start is called before the first frame update
    void Start()
    {
        muteOff = Color.red;
        muteOn = Color.green;
        muteImage = muteButton.GetComponent<Image>();

        // Check the mute state at the start
        if (!PlayerPrefs.HasKey("mute"))
        {
            PlayerPrefs.SetInt("mute", 0); // No mute PlayerPrefs was found, so create one and set it to 0.
        }
        else // PlayerPrefs for mute was found
        {
            // The player should never see the buttons at the start of the game, so there's no need to lerp these. Just set the right color at the start.
            if (PlayerPrefs.GetInt("mute") == 0) // Current state is off.
            {
                muteImage.color = muteOff;
            }
            else // Current state is on
            {
                muteImage.color = muteOn;
            }
        }

        bgmSlider.value = PlayerPrefs.GetFloat("bgmVol", 0);
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVol", 0);
        voiceSlider.value = PlayerPrefs.GetFloat("voiceVol", 0);
    }

    public void RestoreDefaults()
    {
        bgmSlider.value = bgmSlider.maxValue;
        sfxSlider.value = sfxSlider.maxValue;
        voiceSlider.value = voiceSlider.maxValue;
    }

    public void ToggleMute()
    {
        StartCoroutine(MuteRoutine(PlayerPrefs.GetInt("mute"))); // Call the coroutine for lerping
    }

    /// <summary>
    /// Performs a lerp between the mute button's off color and on color, or vice versa depending on the current state of that color. The mute button also cannot be 
    /// rapidly pressed during the lerp to prevent issues with the player attempting to spam it.
    /// </summary>
    /// <param name="muteState"></param>
    /// <returns></returns>
    private IEnumerator MuteRoutine(int muteState)
    {
        muteButton.enabled = false; // Prevent any button presses during the coroutine
        float currTime = 0f;
        float moveTime = 0.25f;

        if (muteState == 0) // Mute state is set to the off integer
        {
            while (currTime < moveTime) // Lerp from the off color to the on color
            {
                muteImage.color = Color.Lerp(muteOff, muteOn, currTime / moveTime);
                currTime += Time.deltaTime;
                yield return null;
            }
            PlayerPrefs.SetInt("mute", 1); // set the mute state to on
        }
        else // Mute state is set to the on integer
        {
            while (currTime < moveTime) // Lerp from the on color to the off color
            {
                muteImage.color = Color.Lerp(muteOn, muteOff, currTime/moveTime);
                currTime += Time.deltaTime;
                yield return null;
            }
            PlayerPrefs.SetInt("mute", 0); // set the mute state to off
        }

        muteButton.enabled = true; // Enable the button at the end of the coroutine
        yield return null;
    }

}
