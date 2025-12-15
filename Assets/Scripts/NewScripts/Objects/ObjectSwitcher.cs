using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSwitcher : MonoBehaviour
{
    public List<GameObject> toHide;
    public List<GameObject> toShow;
    public bool triggerOnce = true;
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;

        hasTriggered = true;

        foreach (GameObject obj in toHide)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        foreach (GameObject obj in toShow)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }
}
