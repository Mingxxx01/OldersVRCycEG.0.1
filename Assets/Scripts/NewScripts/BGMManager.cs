using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour
{

    public static BGMManager Instance;

    private AudioSource bgmSource;
    private float originalVolume = 0.4f;
    private const float dialogueVolume = 0.05f;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = originalVolume;

        AudioClip bgm = Resources.Load<AudioClip>("AudioClips/elder_bgm");
        if (bgm)
        {
            bgmSource.clip = bgm;
            bgmSource.Play();
            Debug.Log("BGM started");
        }
        else
        {
            Debug.LogError("elder_bgm.mp3 not found in Resources/AudioClips/");
        }
    }

    public void LowerVolumeForDialogue()
    {
        if (bgmSource)
            bgmSource.volume = dialogueVolume;
    }

    public void RestoreVolume()
    {
        if (bgmSource)
            bgmSource.volume = originalVolume;
    }
}
