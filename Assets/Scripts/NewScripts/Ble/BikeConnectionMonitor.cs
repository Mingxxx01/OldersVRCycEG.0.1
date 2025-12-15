using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BikeConnectionMonitor : MonoBehaviour
{
    public BikeBluetoothController bikeController;
    public PathFollower pathFollower;
    public FTMS_IndoorBike bike;

    private float lastRPM = 0f;
    private float lastSpeed = 0f;
    private float idleTimer = 0f;
    private float checkInterval = 0.5f; // detect frequency
    private float timer = 0f;

    public float disconnectThreshold = 30f;
    private bool wasDisconnected = false;

    void Update()
    {
        if (bikeController == null || pathFollower == null) return;

        if (pathFollower.IsInDialogue() || (bikeController?.bike?.isSubscribed == false))
        {
            idleTimer = 0f;
            wasDisconnected = false;
            return;
        }

        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;

            float currentRPM = bikeController.CurrentRPM;
            float currentSpeed = bikeController.CurrentSpeed;

            float rpmDelta = Mathf.Abs(currentRPM - lastRPM);
            float speedDelta = Mathf.Abs(currentSpeed - lastSpeed);

            bool hasChanged = rpmDelta > 0.1f || speedDelta > 0.1f;

            if (hasChanged)
            {
                idleTimer = 0f;

                if (wasDisconnected)
                {
                    Debug.Log("From BikeConnectionMonitor: Pedal input resumed."); 
                    wasDisconnected = false;
                }
            }
            else
            {
                idleTimer += checkInterval;
            }

            lastRPM = currentRPM;
            lastSpeed = currentSpeed;

            if (idleTimer >= disconnectThreshold && !wasDisconnected)
            {
                Debug.LogWarning("From BikeConnectionMonitor: No input detected, bike maybe disconnected.");
                wasDisconnected = true;
            }
        }
    }
}
