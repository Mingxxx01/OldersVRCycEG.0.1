using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class PathFollower : MonoBehaviour
{
    [Header("Path setting")]
    public Transform[] pathMain;
    public Transform[] pathA;
    public Transform[] pathB;

    public BikeBluetoothController bikeController;

    [Header("UI for choosing branch")]
    public GameObject routeChoicePanel;
    public Button routeAButton;
    public Button routeBButton;
    public Button continueButton;

    public AudioSource routesAudioSource;
    public AudioClip initialAudioClip;

    public TMP_Text initalTextBox;
    public TMP_Text choiceATextBox;
    public TMP_Text choiceBTextBox;
    //public TMP_Text middleTextBox;
    //public TMP_Text instructionTextBox;
    private enum PathState { Main, A, B }
    private PathState currentPathState = PathState.Main;

    [Header("Parameters about cycling")]
    public float reachThreshold = 0.2f;
    public float maxAcceleration = 1.5f;
    public float rotationSpeed = 3.5f;

    //private bool isPaused = false;
    private bool isTemporarilySlowed = false;
    private float slowdownFactor = 1f;
    public float slowdownRate = 5.0f; // deceleration

    private Vector3[] currentPath;
    //private Vector3 smoothedForward = Vector3.forward;

    private int currentIndex = 0;
    private bool waitingForChoice = false;

    private Rigidbody rb;
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    [HideInInspector] public float externalSpeedMultiplier = 1.0f;


    [System.Serializable]
    public struct SlowdownRange
    {
        public int startIndex;
        public int endIndex;
    }

    [System.Serializable]
    public class PathSlowdownConfig
    {
        public string pathName; // "Main", "A", "B"
        public SlowdownRange[] ranges;
    }

    public PathSlowdownConfig[] slowdownConfigs;

    //public SlowdownRange[] slowdownRanges;
    private float rpmSnapshot = 0f;
    private float distanceSnapshot = 0f;
    private float averagePreDialogueSpeed = 0f;
    private bool waitingForPedalInput = false;
    private float timeSinceDialogueExit = 0f;

    //private float preDialogueSpeed = 0f;
    private bool autoResumeTriggered = false;
    private Vector3 initialPosition;
    private bool initialPositionCaptured = false;
    //private bool isInAutoResume = false;


    private bool isInDialogue = false;
    public bool isIntroLocked = true;



    public List<string> initialTexts = new List<string>();
    public List<AudioClip> initialAudioClips = new List<AudioClip>();
    private int currentIntroIndex = 0;


    public void SetInitialIntro(List<string> texts, List<AudioClip> clips = null)
    {
        initialTexts = texts ?? new List<string>();
        initialAudioClips = clips ?? new List<AudioClip>();
    }




    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        routeChoicePanel?.SetActive(false);
        StartPath(pathMain);

        if (!initialPositionCaptured)
        {
            initialPosition = transform.position;
            initialPositionCaptured = true;
        }
    }

    // the player's initial position in the map
    private bool IsAtInitialPosition()
    {
        float distance = Vector3.Distance(transform.position, initialPosition);
        return distance < 2f;
    }


    void StartPath(Transform[] points)
    {
        currentPath = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
            currentPath[i] = points[i].position;

        currentIndex = 0;
    }

    public void SetTemporarySlowdown(bool active, float factor = 1f)
    {
        isTemporarilySlowed = active;
        slowdownFactor = Mathf.Clamp01(factor);
    }


    //public void PauseMovement()
    //{
    //    isPaused = true;
    //    rb.velocity = Vector3.zero;
    //}

    //public void ResumeMovement()
    //{
    //    isPaused = false;
    //}


    //void Update()
    //{
    //    //if (isPaused)
    //    //{
    //    //    rb.velocity = Vector3.zero;
    //    //    return;
    //    //}

    //    if (waitingForChoice || currentPath == null || currentIndex >= currentPath.Length)
    //    {
    //        rb.velocity = Vector3.zero;
    //        return;
    //    }

    //    //if (waitingForChoice || currentPath == null || currentIndex >= currentPath.Length)
    //    //{
    //    //    rb.velocity = Vector3.zero;
    //    //    return;
    //    //}

    //    float rawSpeed = bikeController?.CurrentSpeed ?? 0f;
    //    targetSpeed = (rawSpeed / 3.6f) / 3;

    //    direction.Normalize();
    //    rb.velocity = direction * currentSpeed;

    //    // Turning: use the current speed direction
    //    if (rb.velocity.magnitude > 0.01f)
    //    {
    //        Quaternion targetRot = Quaternion.LookRotation(rb.velocity.normalized);
    //        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed);
    //    }
    //}

    void FixedUpdate()
    {

        if (isInDialogue || waitingForChoice || currentPath == null || currentIndex >= currentPath.Length || isIntroLocked)
        {
            rb.velocity = Vector3.zero;
            //if (waitingForPedalInput && HasPedalInputChanged())
            //{
            //    Debug.Log("Detected pedal input change, resuming movement.");
            //    waitingForPedalInput = false;
            //}
            return;
        }

        if (waitingForPedalInput)
        {
            //timeSinceDialogueExit += Time.fixedDeltaTime;

            //if (timeSinceDialogueExit > 0.5f && HasPedalInputChanged())
            //{
            //    waitingForPedalInput = false;
            //    Debug.Log("Detected pedal input change, resuming movement.");
            //}

            rb.velocity = Vector3.zero;
            timeSinceDialogueExit += Time.fixedDeltaTime;

            if (timeSinceDialogueExit > 1.5f && HasPedalInputChanged())
            {
                waitingForPedalInput = false;
                Debug.Log("Detected pedal input change, resuming movement.");
            }
            else if (!HasPedalInputChanged() && !IsAtInitialPosition() && !autoResumeTriggered && timeSinceDialogueExit > 7f)
            {
                // auto resuming riding if no new riding action input

                //currentSpeed = Mathf.MoveTowards(currentSpeed, 8f, maxAcceleration * Time.fixedDeltaTime);
                autoResumeTriggered = true;
                waitingForPedalInput = false;
                //isInAutoResume = true;
                Debug.Log($"No input after 7s, so auto resuming at previous speed. GetRecentAverageSpeed(): {bikeController.GetRecentAverageSpeed()}; averagePreDialogueSpeed: {averagePreDialogueSpeed}");
            }
            return;
        }

        //if (autoResumeTriggered)
        //{
        //    Debug.Log($"currentspeed: {currentSpeed}");
        //}


        float rawSpeed = bikeController?.CurrentSpeed ?? 0f;
        float targetSpeed = (rawSpeed / 3.6f) * externalSpeedMultiplier * 0.22f;
        if (isTemporarilySlowed)
        {
            targetSpeed *= slowdownFactor;
            //Debug.Log($"targetSpeed={targetSpeed:F2}, slowed={isTemporarilySlowed}, factor={slowdownFactor}");

        }

        Vector3 targetPos = currentPath[currentIndex];
        Vector3 toTarget = targetPos - transform.position;
        float distance = toTarget.magnitude;

        //float slowDownDistance = 2.5f;
        //float slowFactor = 1f;
        //if (distance < slowDownDistance)
        //{
        //    float ratio = distance / slowDownDistance;
        //    slowFactor = Mathf.Lerp(0.6f, 1f, ratio); // slow down before turning
        //    targetSpeed *= slowFactor;
        //}

        if (autoResumeTriggered && rawSpeed > 0.1f && HasPedalInputChanged())
        {
            targetSpeed = (rawSpeed / 3.6f) * externalSpeedMultiplier * 0.22f;
            autoResumeTriggered = false;
            Debug.Log("Live Bluetooth resumed, overriding auto speed.");
        }

        // Check if in slowdown range
        //foreach (var range in slowdownRanges)
        //{
        //    if (currentIndex >= range.startIndex && currentIndex <= range.endIndex)
        //    {
        //        targetSpeed *= 0.5f;
        //        break;
        //    }
        //}

        string currentPathKey = currentPathState.ToString(); // "Main", "A", "B"

        //foreach (var config in slowdownConfigs)
        //{
        //    if (config.pathName.Equals(currentPathKey, System.StringComparison.OrdinalIgnoreCase))
        //    {
        //        foreach (var range in config.ranges)
        //        {
        //            if (currentIndex >= range.startIndex && currentIndex <= range.endIndex)
        //            {
        //                targetSpeed *= 0.6f;
        //                break;
        //            }
        //        }
        //    }
        //}



        if (isTemporarilySlowed && currentSpeed > targetSpeed)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, slowdownRate * Time.fixedDeltaTime);
        }
        else if (autoResumeTriggered == true)
        {
            targetSpeed = 4.5f;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, maxAcceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, maxAcceleration * Time.fixedDeltaTime);
        }
        //currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, maxAcceleration * Time.fixedDeltaTime);


        if (distance < reachThreshold)
        {
            currentIndex++;
            if (currentPathState == PathState.Main && currentIndex == currentPath.Length)
            {
                ShowRouteChoice();
            }
            return;
        }

        // get move direction
        Vector3 moveDirection = toTarget.normalized;
        rb.velocity = moveDirection * currentSpeed;

        // Turning: use the current speed direction instead of the next path point direction
        if (rb.velocity.magnitude > 0.1f)
        {
            Vector3 forwardDir = rb.velocity.normalized;
            Quaternion desiredRot = Quaternion.LookRotation(forwardDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.fixedDeltaTime * rotationSpeed);
        }

        //if (rb.velocity.magnitude > 0.1f)
        //{
        //    Vector3 forwardDir = rb.velocity.normalized;

        //    smoothedForward = Vector3.Slerp(smoothedForward, forwardDir, Time.fixedDeltaTime * rotationSpeed);

        //    Quaternion desiredRot = Quaternion.LookRotation(smoothedForward);
        //    transform.rotation = desiredRot;
        //}

    }

    public void SetDialogueState(bool state)
    {
        isInDialogue = state;
        Debug.Log($"SetDialogueState({state}) called.");

        //if (!state) 
        //{
        //    currentSpeed = 0f;
        //    SetTemporarySlowdown(false); 
        //}

        if (state)
        {
            // Enter the dialogue and record the current rpm and distance data.
            rpmSnapshot = bikeController?.CurrentRPM ?? 0f;
            distanceSnapshot = bikeController?.CurrentDistance ?? 0f;
            //preDialogueSpeed = currentSpeed;
            // record the speed brfore starting dialogue
            averagePreDialogueSpeed = bikeController?.GetRecentAverageSpeed() ?? currentSpeed;
        }
        else
        {
            // Exit the dialogue and wait for the riding action to trigger movement
            currentSpeed = 0f;
            waitingForPedalInput = true;
            autoResumeTriggered = false;
            timeSinceDialogueExit = 0f;
            SetTemporarySlowdown(false);  // unlock slow down state and enable normal acceleration
        }
    }


    public bool IsInDialogue()
    {
        return isInDialogue;
    }

    // check if the player is pedalling
    private bool HasPedalInputChanged()
    {
        if (bikeController == null) return false;

        float currentRPM = bikeController.CurrentRPM;
        float currentDistance = bikeController.CurrentDistance;

        float rpmDelta = Mathf.Abs(currentRPM - rpmSnapshot);
        float distanceDelta = Mathf.Abs(currentDistance - distanceSnapshot);

        return rpmDelta > 1f || distanceDelta > 0.3f;
    }

    //route choosing: A/B
    void ShowRouteChoice()
    {
        waitingForChoice = true;
        //rb.velocity = Vector3.zero;
        BGMManager.Instance?.LowerVolumeForDialogue();
        SetDialogueState(true);

        routeChoicePanel.SetActive(true);

        initalTextBox.gameObject.SetActive(true);
        choiceATextBox.gameObject.SetActive(false);
        choiceBTextBox.gameObject.SetActive(false);
        //middleTextBox.gameObject.SetActive(false);
        routeAButton.gameObject.SetActive(false);
        routeBButton.gameObject.SetActive(false);
        //instructionTextBox.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);

        //routesAudioSource.clip = initialAudioClip;
        //routesAudioSource.Play();

        ////routeAButton.onClick.AddListener(() => ChoosePath(PathState.A));
        ////routeBButton.onClick.AddListener(() => ChoosePath(PathState.B));
        //StartCoroutine(WaitForContinue());

        // Start choosing text sequence (text + audio)
        currentIntroIndex = 0;
        StopAllCoroutines();
        StartCoroutine(PlayInitialSequence());
    }

    // play audios
    IEnumerator PlayInitialSequence()
    {
        if (initialTexts == null || initialTexts.Count == 0)
        {
            if (initalTextBox != null)
            {
                // enter text in inspector
            }
            if (initialAudioClip != null)
            {
                routesAudioSource.clip = initialAudioClip;
                routesAudioSource.Play();
                yield return new WaitWhile(() => routesAudioSource.isPlaying);
            }
            yield return new WaitForSeconds(2f);
            OnContinueToChoice();
            yield break;
        }

        for (currentIntroIndex = 0; currentIntroIndex < initialTexts.Count; currentIntroIndex++)
        {
            // Update text
            if (initalTextBox != null)
                initalTextBox.text = initialTexts[currentIntroIndex] ?? string.Empty;

            // select audio: per-index clip if exists, else fallback to initialAudioClip
            AudioClip clipToPlay = null;
            if (initialAudioClips != null && currentIntroIndex < initialAudioClips.Count)
            {
                clipToPlay = initialAudioClips[currentIntroIndex];
            }
            if (clipToPlay == null)
            {
                clipToPlay = initialAudioClip; // fallback
            }

            if (clipToPlay != null)
            {
                routesAudioSource.clip = clipToPlay;
                routesAudioSource.Play();
                yield return new WaitWhile(() => routesAudioSource.isPlaying);
            }
            else
            {
                // if no audio for this segment
                yield return new WaitForSeconds(1.0f);
            }

            // small time gap between segments
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(2f);
        OnContinueToChoice();
    }


    //IEnumerator WaitForContinue()
    //{
    //    // wait for audios to end
    //    yield return new WaitWhile(() => routesAudioSource.isPlaying);

    //    // 2s delay after all audio ends
    //    yield return new WaitForSeconds(2f);

    //    continueButton.gameObject.SetActive(true);
    //    continueButton.onClick.RemoveAllListeners();
    //    continueButton.onClick.AddListener(OnContinueToChoice);

    //}

    void OnContinueToChoice()
    {
        initalTextBox.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
        //instructionTextBox.gameObject.SetActive(true);
        choiceATextBox.gameObject.SetActive(true);
        choiceBTextBox.gameObject.SetActive(true);
        routeAButton.gameObject.SetActive(true);
        routeBButton.gameObject.SetActive(true);

        // create button click events
        routeAButton.onClick.RemoveAllListeners();
        routeBButton.onClick.RemoveAllListeners();
        routeAButton.onClick.AddListener(() => ChoosePath(PathState.A));
        routeBButton.onClick.AddListener(() => ChoosePath(PathState.B));
    }


    void ChoosePath(PathState choice)
    {
        waitingForChoice = false;
        routeChoicePanel.SetActive(false);
        routeAButton.onClick.RemoveAllListeners();
        routeBButton.onClick.RemoveAllListeners();

        currentPathState = choice;
        BGMManager.Instance?.RestoreVolume();
        SetDialogueState(false);
        switch (choice)
        {
            case PathState.A:
                StartPath(pathA);
                break;
            case PathState.B:
                StartPath(pathB);
                break;
        }
    }
}



