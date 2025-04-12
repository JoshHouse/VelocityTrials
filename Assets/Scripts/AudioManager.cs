using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] private AudioSource bgmObject;
    [SerializeField] private AudioSource sfxObject;
    [SerializeField] private AudioSource voiceObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(instance);
        }
    }

    public void PlayVoiceClip(AudioClip clip, Transform spawnPoint)
    {
        AudioSource audioSource = Instantiate(voiceObject, spawnPoint.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }

}
