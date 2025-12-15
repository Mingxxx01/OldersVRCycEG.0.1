using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [System.Serializable]
    public class DialogueScript
    {
        public string key;            // give an unique key value to each csvfile
        public TextAsset csvFile;     // allow to define mutiple csvfile in inspector
    }

    public List<DialogueScript> scripts = new();  
    public int defaultScriptIndex = 0;            // default csvfile
    public string CurrentScriptKey { get; private set; } // get the key of the current selected csvfile

    public DialogueUI dialogueUI;
    public PathFollower pathFollower;
    private AudioSource audioSource;

    private Dictionary<string, DialogueEntry> dialogueMap = new();
    private Dictionary<string, List<DialogueEntry>> choiceGroups = new();
    public string CurrentDialogueId { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            // LoadCSV();

            // load the default csvfile
            if (scripts != null && scripts.Count > 0)
            {
                int idx = Mathf.Clamp(defaultScriptIndex, 0, scripts.Count - 1);
                SelectScriptByIndex(idx);
            }
            else
            {
                Debug.LogError("no scripts configured");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // select and load the current csvfile using the index
    public void SelectScriptByIndex(int index)
    {
        if (index < 0 || index >= scripts.Count)
        {
            Debug.LogError($"invalid script index: {index}");
            return;
        }
        var s = scripts[index];
        SelectScriptByKey(s.key);
    }

    // select and load the current csvfile using the csv key
    public void SelectScriptByKey(string key)
    {
        var s = scripts.FirstOrDefault(x => x.key == key);
        if (s == null)
        {
            Debug.LogError($"script key not found: {key}");
            return;
        }
        CurrentScriptKey = s.key;
        LoadCSV(s.csvFile);
    }

    void LoadCSV(TextAsset csvFile)
    {
        dialogueMap.Clear();
        choiceGroups.Clear();

        if (csvFile == null)
        {
            Debug.LogError("CSV file is null.");
            return;
        }

        Regex csvSplit = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))"); // read and clean data from csv

        var lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // Skip header of csv file
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] parts = csvSplit.Split(line);
            if (parts.Length < 9)
            {
                Debug.LogWarning($"skipped line {i + 1}: only {parts.Length} columns ({line}), not 9 colums!");
                continue;
            }

            var entry = new DialogueEntry
            {
                id = parts[0].Trim().Replace("\r", ""),
                branch = parts[1].Trim(),
                speaker = parts[2].Trim(),
                text = parts[3].Trim().Trim('"'),
                choice_group = parts[4].Trim().Replace("\r", ""),
                choice_text = parts[5].Trim().Replace("\"", ""),
                next_id = parts[6].Trim(),
                next_choice = parts[7].Trim().Replace("\r", ""),
                is_end = parts[8].Trim().ToLower() == "true",
            };

            dialogueMap[entry.id] = entry;

            if (!string.IsNullOrEmpty(entry.choice_group))
            {
                if (!choiceGroups.ContainsKey(entry.choice_group))
                    choiceGroups[entry.choice_group] = new List<DialogueEntry>();
                choiceGroups[entry.choice_group].Add(entry);
                Debug.Log($"added choice from CSV to group '{entry.choice_group}' → {entry.choice_text}");
            }
        }

        Debug.Log($"Script '{CurrentScriptKey}' loaded. Entries: {dialogueMap.Count}, choice groups: {choiceGroups.Count}");
    }

    public void StartDialogue(string startId)
    {
        BGMManager.Instance?.LowerVolumeForDialogue(); // turn down bgm volume when in dialogue state
        pathFollower?.SetDialogueState(true);
        DisplayEntry(startId);
    }

    void DisplayEntry(string id)
    {
        //Debug.Log($"Displaying entry ID: {id}");
        CurrentDialogueId = id;

        if (!dialogueMap.ContainsKey(id))
        {
            Debug.LogError($"Dialogue ID '{id}' not found!");
            return;
        }

        var entry = dialogueMap[id];

        Debug.Log($"Entry found: {entry.id}, speaker: {entry.speaker}, next_choice: '{entry.next_choice}'");

        StopCurrentAudio(); // stop the current audio before display any dialogue

        PlayAudioIfExists(entry); // play dialogue audio if speaker is not player

        if (entry.speaker == "PLAYER" && !string.IsNullOrEmpty(entry.choice_group))
        {
            dialogueUI.ShowChoices(entry, choiceGroups[entry.choice_group], OnChoiceSelected);
        }
        else if (!string.IsNullOrEmpty(entry.next_choice))
        {
            //Debug.Log($"");
            // show options under NPC's dialogue

            if (choiceGroups.ContainsKey(entry.next_choice))
            {
                dialogueUI.ShowChoices(entry, choiceGroups[entry.next_choice], OnChoiceSelected);
            }
            else
            {
                Debug.LogWarning($"next_choice group not found: {entry.next_choice}");
                DisplayEntry(entry.next_id);
            }
        }
        else
        {

            // non-selectable dialogue
            //bool shouldAutoContinue = string.IsNullOrEmpty(entry.next_choice) && entry.is_end;

            bool isFinalDlg = entry.is_end && string.IsNullOrEmpty(entry.next_choice);

            dialogueUI.ShowLine(entry, () =>
            {
                StopCurrentAudio(); // stop the audio if Continue/Close button clicked
                if (entry.is_end) EndDialogue();
                else DisplayEntry(entry.next_id);
                //Debug.Log($"next Dialogue ID is '{entry.next_id}'");
            }, isFinalDlg);
        }
    }

    private void PlayAudioIfExists(DialogueEntry entry)
    {
        if (entry.speaker.Trim().ToUpper() == "PLAYER") return;

        // find audio of dialogue to play：Resources/AudioClips/<id>
        string clipPath = $"AudioClips/{entry.id}";
        AudioClip clip = Resources.Load<AudioClip>(clipPath);

        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"Audio clip not found at Resources/{clipPath}.mp3");
        }
    }

    //private void PlayAudioIfExists(DialogueEntry entry)
    //{
    //    if (entry.speaker.Trim().ToUpper() == "PLAYER") return;

    //    // get the correct audio folder
    //    string folder = "AudioClips"; 
    //    if (Instance != null && !string.IsNullOrEmpty(CurrentScriptKey))
    //    {
    //        var scriptInfo = scripts.FirstOrDefault(s => s.key == CurrentScriptKey);
    //        if (scriptInfo != null && !string.IsNullOrEmpty(scriptInfo.audioFolder))
    //        {
    //            folder = scriptInfo.audioFolder;
    //        }
    //    }

    //    
    //    string clipPath = $"{folder}/{entry.id}";
    //    AudioClip clip = Resources.Load<AudioClip>(clipPath);

    //    if (clip != null)
    //    {
    //        audioSource.clip = clip;
    //        audioSource.Play();
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"Audio clip not found at Resources/{clipPath}");
    //    }
    //}

    private void StopCurrentAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    void OnChoiceSelected(DialogueEntry choice)
    {
        if (choice.is_end || string.IsNullOrEmpty(choice.next_id))
            EndDialogue();
        //Debug.Log($"End of dialogue branch.");
        //dialogueUI.Hide();
        //pathFollower?.SetDialogueState(false);
        else
            DisplayEntry(choice.next_id);
    }

    void EndDialogue()
    {
        BGMManager.Instance?.RestoreVolume(); //turn up the bgm volume during riding
        dialogueUI.Hide();
        pathFollower?.SetDialogueState(false);
    }
}
