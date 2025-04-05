using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Audio
{

    public enum Type
    {
        BGM,
        SFX,
        VOICE,
    }

    [Header("General Information")]
    public string id;                   // Gives the audio object an id that can be set to call the audio object with.
    public string name;                 // Sets a name for the created audio object. Allows for a possible "Now playing: [name]" for BGM.
    public string credit;               // Credit to whoever is the owner of the audio file. If royalty-free or created by a sound editor, it may be best to set this as "Original"

    [Header("Audio Information")]
    public AudioMixerGroup mixerGroup; // The audio file's mixer group. Used to control BGM, SFX,and Voice audio separately
    public Type soundType;
    [HideInInspector] public AudioSource source; // The Audio's Source
    public AudioClip clip; // The actual audio clip to the played
    [Range(0f, 1f)] public float volume; // What volume the clip should be set at overall. This is different from the mixer's volume
    [Range(0.1f, 3f)] public float pitch; // The pitch that the audio clip should be set at overall.


}
