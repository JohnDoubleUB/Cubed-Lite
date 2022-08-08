using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AudioManager : MonoBehaviour
{
    public static AudioManager current;

    [SerializeField]
    private AudioSource music;

    public float SoundVolumeMultiplier = 1;
    public float pitchVariation = 0.2f;

    private string MusicToggleStatePref = "M_ToggleState";

    public bool MusicEnabled 
    {
        get { return music.isPlaying; }
    }


    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;

        if (SaveSystem.TryLoadUnityPrefInt(MusicToggleStatePref, out int pref))
        {
            ToggleMusic(System.Convert.ToBoolean(pref));
        }

    }

    private void Update()
    {
    }

    public AudioSource PlayClipAt(AudioClip clip, Vector3 pos, float volume, bool withPitchVariation = true, float delayInSeconds = 0f, float customPitch = 1f)
    {
        return PlayClipAt(clip, pos, volume, withPitchVariation ? Random.Range(customPitch - pitchVariation, customPitch + pitchVariation) : customPitch, delayInSeconds);
    }

    public AudioSource PlayClipAt(AudioClip clip, Vector3 pos, float volume, float pitch, float delayInSeconds)
    {
        GameObject tempGO = new GameObject("TempAudio"); // create the temp object
        tempGO.transform.position = pos; // set its position
        AudioSource aSource = tempGO.AddComponent<AudioSource>(); // add an audio source
        aSource.spatialBlend = 0;
        aSource.clip = clip; // define the clip
        aSource.pitch = pitch;
        aSource.volume = volume * SoundVolumeMultiplier;
        aSource.PlayDelayed(delayInSeconds); // start the sound
        Destroy(tempGO, clip.length + delayInSeconds); // destroy object after clip duration
        return aSource; // return the AudioSource reference
    }


    public bool ToggleMusic() 
    {
        ToggleMusic(!MusicEnabled, true);
        return MusicEnabled;
    }

    private void ToggleMusic(bool on, bool savePref = false)
    {
        if (!on)
            music.Pause();
        else
            music.Play();

        if (savePref) 
        {
            PlayerPrefs.SetInt(MusicToggleStatePref, on ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
