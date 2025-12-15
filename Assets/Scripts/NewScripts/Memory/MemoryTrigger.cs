using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryTrigger : MonoBehaviour
{
    public MemorySegment[] memorySegments;
    public PathFollower pathFollower;

    private bool triggered = false;
    void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;

            PathFollower follower = other.GetComponentInParent<PathFollower>();
            if (follower == null)
            {
                Debug.LogWarning("Failed to find PathFollower on Player!");
            }
            else
            {
                follower.SetDialogueState(true); // continue cycling
            }

            MemoryManager.Instance.StartMemory(memorySegments, follower);
            Destroy(gameObject);
        }
    }

}

[System.Serializable]
public class MemorySegment
{
    [TextArea] public string text;
    public AudioClip audio;
    public float duration = 5f;
}
