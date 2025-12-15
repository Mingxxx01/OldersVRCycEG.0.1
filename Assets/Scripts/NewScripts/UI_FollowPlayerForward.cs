using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_FollowPlayerForward : MonoBehaviour
{
    public Transform playerTransform;
    public float distance = 2.0f;
    public float heightOffset = 1.5f;

    void Update()
    {
        if (playerTransform == null) return;

        // record player position
        Vector3 forward = playerTransform.forward;
        forward.y = 0; 
        forward.Normalize();

        Vector3 targetPosition = playerTransform.position + forward * distance + Vector3.up * heightOffset;

        // initialise ui position
        transform.position = targetPosition;

        // make the ui face the player
        transform.LookAt(playerTransform.position + Vector3.up * heightOffset);
        transform.forward = -transform.forward;

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0); // keep the ui vertical
    }
}
