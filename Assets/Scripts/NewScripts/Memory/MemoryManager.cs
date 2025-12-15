using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MemoryManager : MonoBehaviour
{
    public static MemoryManager Instance;

    public GameObject memoryPanel;
    //public TextMeshProUGUI memoryText;
    public TMP_Text memoryText;
    //public GameObject tapPrompt;
    public AudioSource audioSource;
    public Button MemoryButton; 


    private List<MemorySegment> currentSegments;
    private int currentIndex = 0;
    private Coroutine playbackRoutine;
    private PathFollower pathFollwer;
    private bool waitingForTap = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        //memoryText.gameObject.SetActive(false);
        memoryPanel.SetActive(false);
        //tapPrompt.SetActive(false);
        MemoryButton.gameObject.SetActive(false);

    }

    public void StartMemory(MemorySegment[] segments, PathFollower pf)
    {
        currentSegments = new List<MemorySegment>(segments);
        currentIndex = 0;
        pathFollwer = pf;
        Debug.Log($"Received player: {pf}");
        BGMManager.Instance?.LowerVolumeForDialogue();

        if (playbackRoutine != null) StopCoroutine(playbackRoutine);
        playbackRoutine = StartCoroutine(PlayNextSegment());
    }

    void Update()
    {
        //if (waitingForTap && Input.GetMouseButtonDown(0))
        //{
        //    waitingForTap = false;
        //    tapPrompt.SetActive(false);

        //    currentIndex++;
        //    if (currentIndex >= currentSegments.Count)
        //    {
        //        EndMemory();
        //    }
        //    else
        //    {
        //        playbackRoutine = StartCoroutine(PlayNextSegment());
        //    }
        //}
    }

    IEnumerator PlayNextSegment()
    {
        MemorySegment seg = currentSegments[currentIndex];
        bool isLastSegment = currentIndex == currentSegments.Count - 1;

        memoryText.text = seg.text;
        memoryPanel.SetActive(true);

        if (seg.audio != null)
        {
            audioSource.clip = seg.audio;
            audioSource.Play();
        }

        yield return new WaitForSeconds(seg.duration);

        if (audioSource.isPlaying)
            audioSource.Stop();

        waitingForTap = true;
        //tapPrompt.SetActive(true);
        //tapPrompt.GetComponentInChildren<TextMeshProUGUI>().text = currentIndex == currentSegments.Count - 1
        //    ? "Tap to exit memory"
        //    : "Tap to continue";

        MemoryButton.gameObject.SetActive(true);
        // write text in the memoryButton
        MemoryButton.GetComponentInChildren<TextMeshProUGUI>().text = isLastSegment ? "CLOSE" : "CONTINUE";

        // clear old events and add new events
        MemoryButton.onClick.RemoveAllListeners();
        MemoryButton.onClick.AddListener(OnContinueButtonClicked);
    }

    void OnContinueButtonClicked()
    {
        MemoryButton.gameObject.SetActive(false);
        //tapPrompt.SetActive(false);
        waitingForTap = false;

        currentIndex++;
        if (currentIndex >= currentSegments.Count)
        {
            EndMemory();
        }
        else
        {
            playbackRoutine = StartCoroutine(PlayNextSegment());
        }
    }



    void EndMemory()
    {
        //memoryText.gameObject.SetActive(false);
        memoryPanel.SetActive(false);
        //tapPrompt.SetActive(false);
        waitingForTap = false;
        Debug.Log("Memory ended. Trying to resume movement...");
        //if (player != null)
        //{
        //    player?.SetDialogueState(false); // 重新启动前进逻辑
        //}
        BGMManager.Instance?.RestoreVolume();
        pathFollwer?.SetDialogueState(false);
  
        currentSegments.Clear();
    }
}
