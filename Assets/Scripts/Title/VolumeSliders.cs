using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSliders : MonoBehaviour
{

    // Referemce tp the components within the Audio settings section from Unity.
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider voiceSlider;

    // Start is called before the first frame update
    void Start()
    {
        // Load in the slider's values. If no player prefs key is detected, set the dafault to the max value.
        masterSlider.value = PlayerPrefs.GetFloat("masterVol", masterSlider.maxValue);
        bgmSlider.value = PlayerPrefs.GetFloat("bgmVol", bgmSlider.maxValue);
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVol", sfxSlider.maxValue);
        voiceSlider.value = PlayerPrefs.GetFloat("voiceVol", voiceSlider.maxValue);
    }

    /// <summary>
    /// Restores the volume to max by setting the sliders to their max values
    /// </summary>
    public void RestoreDefaults()
    {
        masterSlider.value = masterSlider.maxValue;
        bgmSlider.value = bgmSlider.maxValue;
        sfxSlider.value = sfxSlider.maxValue;
        voiceSlider.value = voiceSlider.maxValue;
    }

}
