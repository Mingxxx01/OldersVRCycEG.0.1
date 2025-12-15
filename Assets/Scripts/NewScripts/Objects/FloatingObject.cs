using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    public float floatSpeed = 1f;        // floating speed
    public float floatHeight = 0.5f;     // floating range
    public float rotationSpeed = 30f;    // rotatio around the z-axis

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}