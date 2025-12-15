using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedUpTrigger : MonoBehaviour
{
    public enum TriggerType { Start, End }
    public TriggerType type;

    public SpeedUpPromptManager promptManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (type == TriggerType.Start)
        {
            promptManager.StartBlinking();
        }
        else if (type == TriggerType.End)
        {
            promptManager.StopBlinking();
        }
    }
}

