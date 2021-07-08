using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class ExperimentManager : MonoBehaviour
{
    [Header("Experiment")]
    public int ParticipantID;
    public int TrialNo;
    public Transform leftHand;
    public VRTK_ControllerEvents leftControllerEvents;
    public int leftHandIndex;
    public Transform rightHand;
    public VRTK_ControllerEvents rightControllerEvents;
    public int rightHandIndex;
    public Transform leftFoot;
    public Transform rightFoot;
    public Transform TaskBoard;
    public LogManager logManager;
    [Header("Body-Tracking")]
    public float armLength;
    public Transform waist;
    public Transform SphereCenter;
    [Header("FoR transform")]
    public Transform Wall;
    public Transform TableTop;
    public Transform FloorSurface;
    [Header("Dashboards")]
    public Transform HeadLevelDisplay;
    public Transform WaistLevelDisplay;
    public Transform GroundDisplay;
    public Transform WallDisplay;
    public Transform TableTopDisplay;

    [HideInInspector]
    public string TrialID;
    [HideInInspector]
    public int QuestionID;
    [HideInInspector]
    public bool Reset = false;
    [HideInInspector]
    public bool NextBtnPressed = false;
    [HideInInspector]
    public bool PrevBtnPressed = false;


    [Header("Reference")]
    public Transform LandmarkParent;
    public Transform DetailedViewParent;

    private Transform CurrentLandmarkParent;
    private Transform CurrentDetailedViewParent;
    private ReferenceFrames CurrentLandmarkFOR;
    private ReferenceFrames CurrentDetailedViewFOR;

    private bool startFlag = true;
    private bool vibFlag = false;

    private float timer = 0;
    private bool timerPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        leftControllerEvents.ButtonOneReleased += LeftTaskBoardToggle;
        rightControllerEvents.ButtonOneReleased += RightTaskBoardToggle;
    }

    // Update is called once per frame
    void Update()
    {
        TrialID = GetTrialID();

        GetCurrentRoF();

        if (startFlag && !Reset) {
            startFlag = false;
            Reset = true;
        }

        if (!Reset && NextBtnPressed) {
            Reset = true;
            NextBtnPressed = false;
        }

        if (!Reset && PrevBtnPressed)
        {
            Reset = true;
            PrevBtnPressed = false;
        }

        if (!timerPaused)
            timer += Time.deltaTime;

        if (timer > 120 && !vibFlag) { // if over 2 min, vibration and show question board
            vibFlag = true;

            logManager.WriteInteractionToLog("TaskBoard", "Left Taskboard Show");
            TaskBoard.gameObject.SetActive(true);
            TaskBoard.SetParent(leftHand);
            TaskBoard.localPosition = new Vector3(0, 0.03f, 0);
            TaskBoard.localEulerAngles = Vector3.zero;
        }

        if (timer > 120 && timer <= 122) {
            SteamVR_Controller.Input(leftHandIndex).TriggerHapticPulse(1500);
            SteamVR_Controller.Input(rightHandIndex).TriggerHapticPulse(1500);
        }

        if (Input.GetKeyDown("n"))
            NextQuestion();

        if (Input.GetKeyDown("p"))
            PrevQuestion();
    }

    #region Getter functions
    private void GetCurrentRoF() {
        if (ParticipantID == 0)
        {
            QuestionID = TrialNo;
            CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
            CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
            if (TrialNo == 1)
            {
                CurrentLandmarkFOR = ReferenceFrames.Shelves;
                CurrentDetailedViewFOR = ReferenceFrames.Shelves;
            }
            else if (TrialNo == 2)
            {
                CurrentLandmarkFOR = ReferenceFrames.Body;
                CurrentDetailedViewFOR = ReferenceFrames.Shelves;
            }
            else if (TrialNo == 3)
            {
                CurrentLandmarkFOR = ReferenceFrames.Floor;
                CurrentDetailedViewFOR = ReferenceFrames.Shelves;
            }
            else
            {
                CurrentLandmarkFOR = ReferenceFrames.Floor;
                CurrentDetailedViewFOR = ReferenceFrames.Body;
            }
        }
        else {
            switch (ParticipantID % 4)
            {
                case 1:
                    QuestionID = TrialNo;
                    CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                    CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                    if (TrialNo <= 4)
                    {
                        CurrentLandmarkFOR = ReferenceFrames.Shelves;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else if (TrialNo <= 8)
                    {
                        CurrentLandmarkFOR = ReferenceFrames.Body;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else if (TrialNo <= 12)
                    {
                        CurrentLandmarkFOR = ReferenceFrames.Floor;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else
                    {
                        CurrentLandmarkFOR = ReferenceFrames.Floor;
                        CurrentDetailedViewFOR = ReferenceFrames.Body;
                    }
                    break;
                case 2:
                    if (TrialNo <= 4)
                    {
                        QuestionID = TrialNo + 4;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Body;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else if (TrialNo <= 8)
                    {
                        QuestionID = TrialNo + 8;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Floor;
                        CurrentDetailedViewFOR = ReferenceFrames.Body;
                    }
                    else if (TrialNo <= 12)
                    {
                        QuestionID = TrialNo - 8;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Shelves;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else
                    {
                        QuestionID = TrialNo - 4;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Floor;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    break;
                case 3:
                    if (TrialNo <= 4)
                    {
                        QuestionID = TrialNo + 12;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Floor;
                        CurrentDetailedViewFOR = ReferenceFrames.Body;
                    }
                    else if (TrialNo <= 8)
                    {
                        QuestionID = TrialNo + 4;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Floor;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else if (TrialNo <= 12)
                    {
                        QuestionID = TrialNo - 4;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Body;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else
                    {
                        QuestionID = TrialNo - 12;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Shelves;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    break;
                case 0:
                    if (TrialNo <= 4)
                    {
                        QuestionID = TrialNo + 8;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Floor;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else if (TrialNo <= 8)
                    {
                        QuestionID = TrialNo - 4;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Shelves;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else if (TrialNo <= 12)
                    {
                        QuestionID = TrialNo + 4;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Floor;
                        CurrentDetailedViewFOR = ReferenceFrames.Body;
                    }
                    else
                    {
                        QuestionID = TrialNo - 8;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Body;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public Transform GetCurrentLandmarkParent() {
        return CurrentLandmarkParent;
    }

    public Transform GetCurrentDetailedViewParent()
    {
        return CurrentDetailedViewParent;
    }

    public ReferenceFrames GetCurrentLandmarkFOR()
    {
        return CurrentLandmarkFOR;
    }

    public ReferenceFrames GetCurrentDetailedViewFOR()
    {
        return CurrentDetailedViewFOR;
    }

    public float GetCurrentTimer() {
        return timer;
    }

    public string GetTrialID() {
        if ((TrialNo - 1) % 4 == 0)
            return "Training";
        else
            return (TrialNo - ((TrialNo - 1) / 4) - 1).ToString();
    }

    #endregion

    #region Setter functions
    private void LeftTaskBoardToggle(object sender, ControllerInteractionEventArgs e) {
        if (TaskBoard.gameObject.activeSelf)
        {
            logManager.WriteInteractionToLog("TaskBoard", "Left Taskboard Hide");
            TaskBoard.gameObject.SetActive(false);
        }
        else
        {
            logManager.WriteInteractionToLog("TaskBoard", "Left Taskboard Show");
            TaskBoard.gameObject.SetActive(true);
            TaskBoard.SetParent(leftHand);
            TaskBoard.localPosition = new Vector3(0, 0.03f, 0);
            TaskBoard.localEulerAngles = Vector3.zero;
        }
    }

    private void RightTaskBoardToggle(object sender, ControllerInteractionEventArgs e)
    {
        if (TaskBoard.gameObject.activeSelf) {
            logManager.WriteInteractionToLog("TaskBoard", "Right Taskboard Hide");
            TaskBoard.gameObject.SetActive(false);
        }
        else {
            logManager.WriteInteractionToLog("TaskBoard", "Right Taskboard Show");
            TaskBoard.gameObject.SetActive(true);
            TaskBoard.SetParent(rightHand);
            TaskBoard.localPosition = new Vector3(0, 0.03f, 0);
            TaskBoard.localEulerAngles = Vector3.zero;
        }
    }

    public void UpdateTrialID() {
        NextBtnPressed = true;
    }

    public void NextQuestion() {
        logManager.WriteInteractionToLog("Completion", timer.ToString());
        TrialNo++;
        
        if (ParticipantID == 0)
        {
            if (TrialNo <= 4)
            {
                NextBtnPressed = true;
                timer = 0;
                timerPaused = false;
                vibFlag = false;
            }
            else
            {
                logManager.QuitGame();
            }
        }
        else {
            if (TrialNo <= 16)
            {
                NextBtnPressed = true;
                timer = 0;
                timerPaused = false;
                vibFlag = false;
            }
            else
            {
                logManager.QuitGame();
            }
        }
    }

    public void PrevQuestion()
    {
        TrialNo--;

        if (ParticipantID == 0)
        {
            if (TrialNo > 0)
            {
                PrevBtnPressed = true;
                timer = 0;
                timerPaused = false;
                vibFlag = false;
            }
            else
            {
                logManager.QuitGame();
            }
        }
        else
        {
            if (TrialNo > 0)
            {
                PrevBtnPressed = true;
                timer = 0;
                timerPaused = false;
                vibFlag = false;
            }
            else
            {
                logManager.QuitGame();
            }
        }
    }

    public void PauseTimer() {
        timerPaused = true;
    }

    public void ResumeTimer() {
        timerPaused = false;
    }
    #endregion
}
