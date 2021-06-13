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
    public Transform rightHand;
    public VRTK_ControllerEvents rightControllerEvents;
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


    [Header("Reference")]
    public Transform LandmarkParent;
    public Transform DetailedViewParent;
    public Transform QuestionBoard;

    private Transform CurrentLandmarkParent;
    private Transform CurrentDetailedViewParent;
    private ReferenceFrames CurrentLandmarkFOR;
    private ReferenceFrames CurrentDetailedViewFOR;

    private bool startFlag = true;

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

        if (!timerPaused)
            timer += Time.deltaTime;

    }

    #region Getter functions
    private void GetCurrentRoF() {
        if (ParticipantID == 0)
        {
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
                    if (TrialNo <= 5)
                    {
                        CurrentLandmarkFOR = ReferenceFrames.Shelves;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else if (TrialNo <= 10)
                    {
                        CurrentLandmarkFOR = ReferenceFrames.Body;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else if (TrialNo <= 15)
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
                    if (TrialNo <= 5)
                    {
                        QuestionID = TrialNo + 5;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Body;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else if (TrialNo <= 10)
                    {
                        QuestionID = TrialNo + 10;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Floor;
                        CurrentDetailedViewFOR = ReferenceFrames.Body;
                    }
                    else if (TrialNo <= 15)
                    {
                        QuestionID = TrialNo - 10;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Shelves;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else
                    {
                        QuestionID = TrialNo - 5;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Floor;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    break;
                case 3:
                    if (TrialNo <= 5)
                    {
                        QuestionID = TrialNo + 15;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Floor;
                        CurrentDetailedViewFOR = ReferenceFrames.Body;
                    }
                    else if (TrialNo <= 10)
                    {
                        QuestionID = TrialNo + 5;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Floor;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else if (TrialNo <= 15)
                    {
                        QuestionID = TrialNo - 5;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Body;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else
                    {
                        QuestionID = TrialNo - 15;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Shelves;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    break;
                case 0:
                    if (TrialNo <= 5)
                    {
                        QuestionID = TrialNo + 10;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Floor;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else if (TrialNo <= 10)
                    {
                        QuestionID = TrialNo - 5;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Shelves;
                        CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                    }
                    else if (TrialNo <= 15)
                    {
                        QuestionID = TrialNo + 5;
                        CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                        CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                        CurrentLandmarkFOR = ReferenceFrames.Floor;
                        CurrentDetailedViewFOR = ReferenceFrames.Body;
                    }
                    else
                    {
                        QuestionID = TrialNo - 10;
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
        return CurrentDetailedViewFOR; ;
    }

    public float GetCurrentTimer() {
        return timer;
    }

    private string GetTrialID() {
        if ((TrialNo - 1) % 5 == 0)
            return "Training";
        else
            return (TrialNo - ((TrialNo - 1) / 5) + 1).ToString();
    }

    #endregion

    #region Setter functions
    private void LeftTaskBoardToggle(object sender, ControllerInteractionEventArgs e) {
        if (TaskBoard.gameObject.activeSelf)
        {
            logManager.WriteInteractionToLog("Left Taskboard Hide");
            TaskBoard.gameObject.SetActive(false);
        }
        else
        {
            logManager.WriteInteractionToLog("Left Taskboard Show");
            TaskBoard.gameObject.SetActive(true);
            TaskBoard.SetParent(leftHand);
            TaskBoard.localPosition = Vector3.zero;
        }
    }

    private void RightTaskBoardToggle(object sender, ControllerInteractionEventArgs e)
    {
        if (TaskBoard.gameObject.activeSelf) {
            logManager.WriteInteractionToLog("Right Taskboard Hide");
            TaskBoard.gameObject.SetActive(false);
        }
        else {
            logManager.WriteInteractionToLog("Right Taskboard Show");
            TaskBoard.gameObject.SetActive(true);
            TaskBoard.SetParent(rightHand);
            TaskBoard.localPosition = Vector3.zero;
        }
    }

    public void UpdateTrialID() {
        NextBtnPressed = true;
    }

    public void NextQuestion() {
        TrialNo++;
        timer = 0;
    }

    public void PauseTimer() {
        timerPaused = true;
    }

    public void ResumeTimer() {
        timerPaused = false;
    }
    #endregion
}
