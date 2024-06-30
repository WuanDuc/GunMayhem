
using System.Collections.Generic;
using UnityEngine;

public static class SoundManager
{
    public static float volumn = 1f;
    public enum Sound
    {
        Fire,
        Landing,
        BoomExplose,
        Theme
    }
    private static Dictionary<Sound, float> soundTimerDictionary;

    private static GameObject soundGameObject;
    private static AudioSource audioSource;

    public static void Initialize()
    {
        soundTimerDictionary = new Dictionary<Sound, float>();
    }
    public static void PlaySound(Sound sound)
    {
        if (CanPlaySound(sound))
        {
            if (soundGameObject == null)
            {
                soundGameObject = new GameObject();
                audioSource = soundGameObject.AddComponent<AudioSource>();
            }
            audioSource.volume = volumn;
            audioSource.PlayOneShot(GetAudioClip(sound));
        }
    }
    private static bool CanPlaySound(Sound sound)
    {
        switch (sound)
        {
            default:
                return true;
            

        }
    }
    private static AudioClip GetAudioClip(Sound sound)
    {
        foreach (GameAssets.SoundAudioClip soundAudioClip in GameAssets.Instance.soundAudioClips)
        {
            if (soundAudioClip.sound == sound)
            {
                return soundAudioClip.audioClip;
            }
        }
        return null;
    }
    public static float GetSoundLength(Sound sound)
    {
        AudioClip audioClip = GetAudioClip(sound);
        return (audioClip != null) ? audioClip.length : 0;
    }
    public static void StopSound()
    {
        audioSource.Stop();
    }

}
