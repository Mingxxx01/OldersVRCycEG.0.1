using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    public GameObject objectToActivate;
    //public GameObject additionalObjectToActivate;
    //public GameObject objectToInactivate;
    
    public float triggerDistance = 2f;
    private Transform player;
    private bool triggered = false;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(false);
            //additionalObjectToActivate.SetActive(false);
            //objectToInactivate.SetActive(true);
        }

    }

    void Update()
    {
        if (triggered || player == null || objectToActivate == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);
        if (distance <= triggerDistance)
        {
            objectToActivate.SetActive(true);
            //additionalObjectToActivate.SetActive(true);
            //objectToInactivate.SetActive(false);
            triggered = true;
            Debug.Log($"Activated: {objectToActivate.name} at {transform.name}");
        }
    }
}
