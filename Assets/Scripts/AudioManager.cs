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

    public void PlaySFXClip(AudioClip clip, Transform spawnPoint)
    {
        AudioSource audioSource = Instantiate(sfxObject, spawnPoint.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        Destroy(audioSource, clipLength);
    }

    public void PlayVoiceClip(AudioClip clip, Transform spawnPoint)
    {
        // Create the voice game object to play the voice clip. The object's audio source is set to be slightly louder than the sfx audio source.
        AudioSource audioSource = Instantiate(voiceObject, spawnPoint.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.Play();
        float clipLength = audioSource.clip.length; // Stores the clip's length in a variable
        Destroy(audioSource.gameObject, clipLength); // Destroys the game object after the clip is finished playing
    }

    public void PlayBGM(AudioClip clip, Transform spawnPoint)
    {
        // If there is already background music playing, find the background music object and destroy it.
        StopBGM();

        // Create the bgm game object to play background music from. The object has an audio source that is designed to loop.
        AudioSource audioSource = Instantiate(bgmObject, spawnPoint.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.Play();
    }

    private void StopBGM()
    {
        // Unity consistently names the cloned prefab BGMObject(Clone), so search for it
        GameObject currBGM = GameObject.Find("BGMObject(Clone)");
        if (currBGM != null)
            Destroy(currBGM); // If the clone exists, destroy it
    }

}
