using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerManager : MonoBehaviour
{

    [SerializeField] private AudioMixer mixer;

    private void Awake()
    {
        mixer.SetFloat("bgmVolume", PlayerPrefs.GetFloat("bgmVol"));
        mixer.SetFloat("sfxVolume", PlayerPrefs.GetFloat("sfxVol"));
        mixer.SetFloat("voiceVolume", PlayerPrefs.GetFloat("voiceVol"));
        mixer.SetFloat("masterVolume", (float)PlayerPrefs.GetInt("mute"));
    }

    public void SetBGMVolume(float volume)
    {
        mixer.SetFloat("bgmVolume", Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat("bgmVol", volume);
    }

    public void setSFXVolume(float volume)
    {
        mixer.SetFloat("sfxVolume", Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat("sfxVol", volume);
    }

    public void setVoiceVolume(float volume)
    {
        mixer.SetFloat("voiceVolume", Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat("voiceVol", volume);
    }

    public void Mute()
    {
        mixer.SetFloat("masterVolume", (float) PlayerPrefs.GetInt("mute"));
    }

}
