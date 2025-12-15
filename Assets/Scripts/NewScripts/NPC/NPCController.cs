using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NPCController : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform player;
    public Transform triggerPoint;
    public Animator npcAnimator;
    public float triggerDistance = 2f;

    [System.Serializable]
    public class NPCConfigPerScript
    {
        public string scriptKey;                 // key of csvfile
        public float hideDelay = 0f;
        //public Animator npcAnimator;
        //public string talkingAnimationName = "Talk1";

        public List<string> hideDuringDialogueIds = new List<string>();
        public List<string> reactToDialogueIds = new List<string>();
        public string reactAnimationName = "Idle";
    }

    public List<NPCConfigPerScript> perScriptConfigs = new(); //define each csv script

    
    private NPCConfigPerScript activeConfig;

    private bool hasAppeared = false;
    //private bool hasPlayedTalk = false;
    //private bool hasBeenHidden = false;
    private Renderer[] renderers;
    private bool hidePending = false;
    private bool reactTriggered = false;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        SetVisible(false);

        ResolveActiveConfig();
    }

    void ResolveActiveConfig()
    {
        string key = DialogueManager.Instance != null ? DialogueManager.Instance.CurrentScriptKey : null;
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("no current script key found on DialogueManager, using the first config as fallback.");
            activeConfig = perScriptConfigs.FirstOrDefault();
        }
        else
        {
            activeConfig = perScriptConfigs.FirstOrDefault(c => c.scriptKey == key);
            if (activeConfig == null)
            {
                Debug.LogWarning($"no per-script config found for the key '{key}', using the first config as fallback.");
                activeConfig = perScriptConfigs.FirstOrDefault();
            }
        }
    }

    void Update()
    {
        if (DialogueManager.Instance != null && activeConfig != null &&
            activeConfig.scriptKey != DialogueManager.Instance.CurrentScriptKey)
        {
            ResolveActiveConfig();
            hidePending = false;
            reactTriggered = false;
        }
        //make the npc visible when the player approaching
        if (!hasAppeared && triggerPoint && player)
        {
            float dist = Vector3.Distance(player.position, triggerPoint.position);
            if (dist < triggerDistance)
            {
                //Debug.Log($"current dialog_id: {DialogueManager.Instance.CurrentDialogueId}, appear npc.");
                hasAppeared = true;
                SetVisible(true);
                //StartCoroutine(PlayTalkAnimationAfterDelay(1.8f));
            }
        }

        if (DialogueManager.Instance != null && activeConfig != null)
        {
            string currentId = DialogueManager.Instance.CurrentDialogueId;
            
            // hide the npc after diaglogue
            // check if the current dialogue_id in the list
            if (!string.IsNullOrEmpty(currentId))
            {
                //Debug.Log($"current dialog_id: {DialogueManager.Instance.CurrentDialogueId}, hide npc.");
                if (activeConfig.hideDuringDialogueIds != null && activeConfig.hideDuringDialogueIds.Contains(currentId))
                {
                    //Debug.Log($"current dialog_id: {DialogueManager.Instance.CurrentDialogueId}, hide npc.");
                    if (!hidePending)
                    {
                        hidePending = true;
                        StartCoroutine(HideAfterDelay(activeConfig.hideDelay));
                    }
                }
                else if (activeConfig.reactToDialogueIds != null && activeConfig.reactToDialogueIds.Contains(currentId) && !reactTriggered)
                {
                    reactTriggered = true;
                    if (npcAnimator != null && !string.IsNullOrEmpty(activeConfig.reactAnimationName))
                    {
                        npcAnimator.Play(activeConfig.reactAnimationName);
                    }
                }
            }
        }
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        foreach (var r in renderers)
            r.enabled = visible;
    }
}
