using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BikeBluetoothController : MonoBehaviour
{
    public string deviceName = "WattbikePT28004316";

    private string serviceUuid = "{babf1723-cedb-444c-88c3-c672c7a59806}";
    private string readUuid = "{babf1724-cedb-444c-88c3-c672c7a59806}";
    private string writeUuid = "{babf1725-cedb-444c-88c3-c672c7a59806}";

    public FTMS_IndoorBike bike;
    public PathFollower pathFollower;
    public float CurrentSpeed => bike?.has_speed == true ? bike.speed : 0f;
    public float CurrentPower => bike?.has_power == true ? bike.power : 0f;
    public float CurrentResistance => bike?.has_resistance == true ? bike.resistance : 0f;
    public float CurrentRPM => bike?.rpm ?? 0f;
    public float CurrentDistance => bike?.distance ?? 0f;
    //public bool IsConnected => bike?.connected ?? false;

    //private float lastHeartbeatTime = 0f;
    //private float heartbeatInterval = 4f;

    private Queue<float> speedSamples = new Queue<float>();
    private float sampleInterval = 0.2f;
    private float sampleTimer = 0f;
    private float sampleWindow = 5f;

    [TextArea]
    public string debugOutput;

    //void Start()
    //{
    //    bike = new FTMS_IndoorBike(this);
    //    StartCoroutine(bike.connect(deviceName, serviceUuid, readUuid, writeUuid));
    //}

    void Start()
    {
        ConnectToBike();
    }
    public void ConnectToBike()
    {
        if (bike == null)
        {
            bike = new FTMS_IndoorBike(this);
        }

        Debug.Log("Attempting (re)connection...");
        StartCoroutine(bike.connect(deviceName, serviceUuid, readUuid, writeUuid));
    }

    void Update()
    {
        if (pathFollower != null && pathFollower.IsInDialogue()) return;


        bike?.Update();
        debugOutput = bike?.output;


        float speed = bike?.speed ?? 0f;
        sampleTimer += Time.deltaTime;
        if (sampleTimer >= sampleInterval)
        {
            sampleTimer = 0f;
            speedSamples.Enqueue(speed);

            // preserve data in the window
            while (speedSamples.Count > 0 && speedSamples.Count * sampleInterval > sampleWindow)
            {
                speedSamples.Dequeue();
            }
        }

        //if (bike != null)
        //{
        //    bike.Update();
        //    debugOutput = bike?.output;
        //    if (Time.time - lastHeartbeatTime > heartbeatInterval)
        //    {
        //        bike.write_resistance((int)(bike.resistance)); 
        //        lastHeartbeatTime = Time.time;
        //    }
        //}
    }

    public float GetRecentAverageSpeed()
    {
        if (speedSamples.Count == 0) return 0f;

        float sum = 0f;
        foreach (var s in speedSamples)
            sum += s;

        return sum / speedSamples.Count;
    }

    public void SetResistance(int level)
    {
        bike?.write_resistance(level);
    }

    private void OnApplicationQuit()
    {
        bike?.quit();
    }
}
