using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
[RequireComponent(typeof(CanvasGroup))]
public class SpeedUpPromptManager : MonoBehaviour
{
    public TextMeshProUGUI speedUpText;
    public string message = "Please speed up your ride!";
    public float blinkDuration = 1f; // each blink message duration

    private Coroutine blinkRoutine;
    private CanvasGroup canvasGroup;

    private PathFollower pathFollower;
    private float previousSpeedMultiplier = 1f;
    public float accelerationDelay = 3f; // delay of speed boosting
    private Coroutine accelerationRoutine;



    void Awake()
    {
        canvasGroup = speedUpText.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = speedUpText.gameObject.AddComponent<CanvasGroup>();
        }

        speedUpText.gameObject.SetActive(false);
        canvasGroup.alpha = 0;

        pathFollower = FindObjectOfType<PathFollower>();
    }

    public void StartBlinking()
    {
        speedUpText.text = message;
        speedUpText.gameObject.SetActive(true);

        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkTextSmooth());

        if (accelerationRoutine != null)
            StopCoroutine(accelerationRoutine);
        accelerationRoutine = StartCoroutine(DelayedBoost());
    }

    public void StopBlinking()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0;

        speedUpText.gameObject.SetActive(false);

        if (accelerationRoutine != null)
        {
            StopCoroutine(accelerationRoutine);
            accelerationRoutine = null;
        }

        if (pathFollower != null)
        {
            pathFollower.externalSpeedMultiplier = previousSpeedMultiplier; // resume original speed
            Debug.Log("Speed boost stopped");
        }
    }

    private IEnumerator DelayedBoost()
    {
        yield return new WaitForSeconds(accelerationDelay);

        if (pathFollower != null)
        {
            previousSpeedMultiplier = pathFollower.externalSpeedMultiplier;
            pathFollower.externalSpeedMultiplier = 1.1f; // boost the speed by 10%
            Debug.Log("Speed boost activated");
        }
    }


    private IEnumerator BlinkTextSmooth()
    {
        float t = 0f;
        bool fadingIn = true;

        while (true)
        {
            t += Time.deltaTime / (blinkDuration / 2f);

            if (fadingIn)
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            else
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            if (t >= 1f)
            {
                t = 0f;
                fadingIn = !fadingIn;
            }

            yield return null;
        }
    }
}
