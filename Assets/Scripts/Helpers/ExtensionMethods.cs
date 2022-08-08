using UnityEngine;

public static class ExtensionMethods
{
    public static AudioSource PlayClipAtTransform(this Transform transform, AudioClip clip, bool parentToTransform = true, float volume = 1f, bool withPitchVariation = true, float delayInSeconds = 0f, float customPitch = 1f)
    {
        if (AudioManager.current != null)
        {
            AudioSource audio = AudioManager.current.PlayClipAt(clip, transform.position, volume, withPitchVariation, delayInSeconds, customPitch);
            if (parentToTransform) audio.transform.parent = transform;

            return audio;
        }

        return null;
    }
}
