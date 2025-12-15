using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;
using static UnityEngine.EventSystems.EventTrigger;

public class DialogueUI : MonoBehaviour
{
    public GameObject panel;
    //public Text speakerText;
    public TMP_Text contentText;
    public TMP_Text speakerName;
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;
    public Button nextBtn;
    public Button closeBtn;


    //public void ShowLine(DialogueEntry entry, Action onContinue)
    //{
    //    panel.SetActive(true);
    //    //speakerText.text = entry.speaker;
    //    //contentText.text = entry.text;
    //    contentText.text = $"{entry.speaker}: {entry.text}, {entry.next_choice}";


    //    ClearChoices();

    //    // continue showing npc's words
    //    //Button NextBtn = panel.GetComponentInChildren<Button>();
    //    NextBtn.onClick.RemoveAllListeners();
    //    NextBtn.onClick.AddListener(() => onContinue?.Invoke());
    //}

    // 
    public void ShowLine(DialogueEntry entry, System.Action onContinue, bool isFinalDlg = false)
    {
        panel.SetActive(true);
        //contentText.text = $"{entry.speaker}: {entry.text}";
        if (!string.IsNullOrEmpty(entry.speaker))
        {

            speakerName.text = $"{entry.speaker}: ";
            contentText.text = entry.text;
        }
        else
        {
            speakerName.text = "";
            contentText.text = entry.text;
        }
        ClearChoices();

        // continue showing npc's words
        nextBtn.gameObject.SetActive(false);
        closeBtn.gameObject.SetActive(false);

        nextBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.RemoveAllListeners();
        //NextBtn.onClick.AddListener(() => onContinue?.Invoke());
        //nextBtn.onClick.AddListener(() =>
        //{
        //    nextBtn.gameObject.SetActive(false); // hide the button after clicks
        //    onContinue?.Invoke();
        //});

        //if (autoContinue)
        //{
        //    StartCoroutine(Advance(onContinue, 3f));
        //}
        //else if (isFinalDlg)
        //{
        //    StartCoroutine(ShowCloseAfterDelay(3f, onContinue));
        //}
        if (isFinalDlg)
        {
            StartCoroutine(ShowCloseAfterDelay(3f, onContinue));
        }
        else
        {
            nextBtn.onClick.AddListener(() =>
            {
                nextBtn.gameObject.SetActive(false); // hide the button after clicks
                onContinue?.Invoke();
            });
            StartCoroutine(ShowContinueAfterDelay(3f));
        }
    }

    //private IEnumerator Advance(System.Action onContinue, float delay)
    //{
    //    // close the panel after 3s, if it's the last dialogue
    //    yield return new WaitForSeconds(delay);
    //    onContinue?.Invoke();
    //}

    //show the Close button
    private IEnumerator ShowCloseAfterDelay(float delay, System.Action onClose)
    {
        // show the CLOSE buuton when the last dialogue
        yield return new WaitForSeconds(delay);
        closeBtn.gameObject.SetActive(true);
        closeBtn.onClick.AddListener(() =>
        {
            closeBtn.gameObject.SetActive(false);
            onClose?.Invoke(); 
        });
    }

    //show the Ctue button
    private IEnumerator ShowContinueAfterDelay(float delay)
    {
        // show the CONTINUE buuton after a 3s delay
        yield return new WaitForSeconds(delay);
        nextBtn.gameObject.SetActive(true);
    }

    //show the options follow the npc's word
    public void ShowChoices(DialogueEntry preDialogue, List<DialogueEntry> options, Action<DialogueEntry> onSelect)
    {
        panel.SetActive(true);
        Debug.Log($"Called with {options.Count} options from entry {preDialogue.id}");

        //speakerName.text = $"{preDialogue.speaker}: ";
        //contentText.text = preDialogue.text;
        if (!string.IsNullOrEmpty(preDialogue.speaker))
        {

            speakerName.text = $"{preDialogue.speaker}: ";
            contentText.text = preDialogue.text;
        }
        else
        {
            speakerName.text = "";
            contentText.text = preDialogue.text;
        }
        //contentText.text = $"{preDialogue.speaker}: {preDialogue.text}";
        ClearChoices();

        StartCoroutine(DelayedShowChoices(options, onSelect, 3f)); // delay display options
    }
    // add a little delay before showing the options
    private IEnumerator DelayedShowChoices(List<DialogueEntry> options, Action<DialogueEntry> onSelect, float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var opt in options)
        {
            Debug.Log($"generate button: {opt.choice_text}");
            var btnObj = Instantiate(choiceButtonPrefab, choiceContainer);

            TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
            if (btnText == null)
            {
                Debug.LogWarning("can't find text component");
            }
            else
            {
                btnText.text = opt.choice_text;
            }

            var btn = btnObj.GetComponent<Button>();
            btnObj.SetActive(true);
            btn.onClick.AddListener(() => onSelect?.Invoke(opt));
        }
    }


    //clear the options
    void ClearChoices()
    {
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void Hide()
    {
        panel.SetActive(false);
        ClearChoices();
    }
}

