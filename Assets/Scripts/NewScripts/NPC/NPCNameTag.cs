using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPCNameTag : MonoBehaviour
{
    public string npcName;
    public Vector3 nameOffset = new Vector3(0, 3f, 0);  // put names on the npcs' header
    public GameObject nameTagPrefab;     // name style
    public Canvas mainCanvas;

    public OVRCameraRig ovrCameraRig;
    public PathFollower playerFollower;
    private TextMeshProUGUI nameText;
    private RectTransform nameUI;

    void Start()
    {
        // instantiate name
        GameObject tagInstance = Instantiate(nameTagPrefab, mainCanvas.transform);
        nameUI = tagInstance.GetComponent<RectTransform>();
        nameText = tagInstance.GetComponentInChildren<TextMeshProUGUI>();
        nameText.text = npcName;
    }

    void LateUpdate()
    {
        if (nameUI == null) return;

        Vector3 worldPos = transform.position + nameOffset;
        Camera centerEyeCam = ovrCameraRig.centerEyeAnchor.GetComponent<Camera>();
        Vector3 screenPos = centerEyeCam.WorldToScreenPoint(worldPos);
        //Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        nameUI.position = screenPos;

        // scaling method
        float distance = Vector3.Distance(playerFollower.transform.position, transform.position);
        float scale = Mathf.Clamp(5f / distance, 0.1f, 0.6f);
        nameUI.localScale = new Vector3(scale, scale, 1f);

        // hide npcs' name when the player is not facing them or the distance exceeds a certain range
        Vector3 toCamera = Camera.main.transform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, toCamera);
        bool isTooFar = distance > 20f;
        bool isFacingAway = angle > 90f;

        bool shouldShow = !isTooFar && !isFacingAway;
        nameUI.gameObject.SetActive(shouldShow);
    }

}
