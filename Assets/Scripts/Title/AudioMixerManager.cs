using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerManager : MonoBehaviour
{

    [SerializeField] private AudioMixer mixer;

    private void Start()
    {
        float masterVol = PlayerPrefs.GetFloat("masterVol", 0) / 100;
        float bgmVol = PlayerPrefs.GetFloat("bgmVol", 0) / 100;
        float sfxVol = PlayerPrefs.GetFloat("sfxVol", 0) / 100;
        float voiceVol = PlayerPrefs.GetFloat("voiceVol", 0) / 100;

        mixer.SetFloat("masterVolume", Mathf.Log10(masterVol) * 20f);
        mixer.SetFloat("bgmVolume", Mathf.Log10(bgmVol) * 20f);
        mixer.SetFloat("sfxVolume", Mathf.Log10(sfxVol) * 20f);
        mixer.SetFloat("voiceVolume", Mathf.Log10(voiceVol) * 20f);
    }

    public void SetMasterVolume(float volume)
    {
        float mixerVolume = Mathf.Log10(volume / 100) * 20;
        mixer.SetFloat("masterVolume", mixerVolume);
        PlayerPrefs.SetFloat("masterVol", volume);
    }

    public void SetBGMVolume(float volume)
    {
        float mixerVolume = Mathf.Log10(volume / 100) * 20;
        mixer.SetFloat("bgmVolume", mixerVolume);
        PlayerPrefs.SetFloat("bgmVol", volume);
    }

    public void setSFXVolume(float volume)
    {
        float mixerVolume = Mathf.Log10(volume / 100) * 20;
        mixer.SetFloat("sfxVolume", mixerVolume);
        PlayerPrefs.SetFloat("sfxVol", volume);
    }

    public void setVoiceVolume(float volume)
    {
        float mixerVolume = Mathf.Log10(volume / 100) * 20;
        mixer.SetFloat("voiceVolume", mixerVolume);
        PlayerPrefs.SetFloat("voiceVol", volume);
    }

}
