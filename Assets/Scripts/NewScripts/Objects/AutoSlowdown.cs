using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSlowdownZone : MonoBehaviour
{
    public PathFollower playerFollower;
    //public DialogueTrigger dlgTrigger;
    //public Transform player;

    public float slowRadius = 6f;       // deceleration range
    [Range(0.1f, 1f)]
    public float slowFactor = 0.2f;       // slow down to x%

    private bool isSlowing = false;

    void Update()
    {
        if (!playerFollower) return;

        float dist = Vector3.Distance(transform.position, playerFollower.transform.position);

        if (dist <= slowRadius)
        {
            if (!isSlowing)
            {
                isSlowing = true;
                playerFollower.SetTemporarySlowdown(true, slowFactor);
            }
        }
        else if (isSlowing)
        {
            isSlowing = false;
            playerFollower.SetTemporarySlowdown(false, 1f);
        }
    }
}
