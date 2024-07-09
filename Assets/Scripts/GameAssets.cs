using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    private static GameAssets instance;
    public static GameAssets Instance
    {
        get
        {
            if (instance == null) instance = new GameAssets();
            return instance;
        }
    }
    private void Awake()
    {
        instance = this;
        SoundManager.Initialize();
    }
    public SoundAudioClip[] soundAudioClips;

    [System.Serializable]
    public class SoundAudioClip
    {
        public SoundManager.Sound sound;
        public AudioClip audioClip;
    }
}