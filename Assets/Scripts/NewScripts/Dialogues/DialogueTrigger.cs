using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [System.Serializable]
    public class StartIdPerScript
    {
        public string scriptKey;     // the key of csvfile
        public string startDialogueId;
    }

    public List<StartIdPerScript> startIds = new(); 

    public bool hasTriggered = false;

    public Transform player;
    public float triggerDistance = 1.0f;

    void Update()
    {
        if (hasTriggered) return;

        if (Vector3.Distance(transform.position, player.position) < triggerDistance)
        {
            hasTriggered = true;
            string key = DialogueManager.Instance?.CurrentScriptKey;

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("no current script key on DialogueManager.");
                return;
            }

            // find the specific dialogue id from the current running csvfile
            string startId = null;
            foreach (var s in startIds)
            {
                if (s.scriptKey == key)
                {
                    startId = s.startDialogueId;
                    break;
                }
            }

            if (string.IsNullOrEmpty(startId))
            {
                Debug.LogError($"no startDialogueId configured for script key '{key}'.");
                return;
            }

            Debug.Log($"Triggered dialogue for script '{key}', startId: {startId}");
            DialogueManager.Instance.StartDialogue(startId);
        }
    }
}
