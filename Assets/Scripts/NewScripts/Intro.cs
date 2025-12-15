using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Intro : MonoBehaviour
{
    public GameObject startPanel;
    public TextMeshProUGUI introText;
    public Button startButton;

    private AudioSource audioSource;

    private readonly string[] introLines = new string[]
    {
        //"You're Lao Li, a retired widower living with your son. Years ago, a crash took your wife; your once-close friend Lao Wu vanished after a money fight. Today—sunny, breezy—your son hikes with friends. Home alone, you eye the blue sky, grab your bike, and ride, chasing that forgotten free feeling.",
        "I am David, a retired widower living with my son, Jim. I also have a best friend - Steven, who always accompany with you during any time.Today, under a sunny, breezy sky, Jim is off hiking with friends while my savor the gentle wind at the start of min afternoon bike ride."
    };

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        startButton.onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        startButton.gameObject.SetActive(false);
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        for (int i = 0; i < introLines.Length; i++)
        {
            introText.text = introLines[i];

            string clipPath = $"AudioClips/intro_{i}";
            AudioClip clip = Resources.Load<AudioClip>(clipPath);
            if (clip != null)
            {
                BGMManager.Instance?.LowerVolumeForDialogue();
                audioSource.clip = clip;
                audioSource.Play();
                yield return new WaitForSeconds(clip.length + 0.5f);
            }
            else
            {
                Debug.LogWarning($"Intro audio not found at {clipPath}");
                yield return new WaitForSeconds(2f);
            }
        }

        introText.text = "I am David, a retired widower living with my son, Jim. I also have a best friend - Steven, who always accompany with you during any time.Today, under a sunny, breezy sky, Jim is off hiking with friends while my savor the gentle wind at the start of min afternoon bike ride.";

        // waiting for 2 sceonds after audio finished, then hide the startpanel
        yield return new WaitForSeconds(4f);

        BGMManager.Instance?.RestoreVolume();
        startPanel.SetActive(false);
        EnableGameSystems(); // start game
    }

    void EnableGameSystems()
    {
        PathFollower pf = FindObjectOfType<PathFollower>();
        if (pf != null)
        {
            pf.enabled = true;
            pf.isIntroLocked = false; // unlock movement
        }

        BikeBluetoothController bt = FindObjectOfType<BikeBluetoothController>();
        if (bt != null) bt.enabled = true;

        DialogueManager dm = FindObjectOfType<DialogueManager>();
        if (dm != null) dm.enabled = true;
    }
}
