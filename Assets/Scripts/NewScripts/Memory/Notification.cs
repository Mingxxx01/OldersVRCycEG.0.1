using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Notification : MonoBehaviour
{
    [Header("Notification")]
    [TextArea]
    public string content;
    public AudioClip notificationAudio;

    [Header("Notification audio clips")]
    public GameObject notificationPanel;
    //public TextMeshProUGUI notificationText;
    public TMP_Text notificationText;
    public AudioSource audioSource;
    public float displayDuration = 10f;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(ShowNotification());
        }
    }

    IEnumerator ShowNotification()
    {
        if (notificationText != null)
        {
            notificationText.text = content;
            //TextUI.gameObject.SetActive(true);
            notificationPanel.SetActive(true);
        }

        // play audio clips
        if (audioSource != null && notificationAudio != null)
        {
            audioSource.clip = notificationAudio;
            audioSource.Play();
        }

        yield return new WaitForSeconds(displayDuration);

        // close text UI and audio
        if (notificationText != null)
        {
            //TextUI.gameObject.SetActive(false);
            notificationPanel.SetActive(false);
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
