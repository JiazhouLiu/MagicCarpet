using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    [Header("Experiment")]
    public int ParticipantID;
    public int TrialNo;
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

    private Transform CurrentLandmarkParent;
    private Transform CurrentDetailedViewParent;
    private ReferenceFrames CurrentLandmarkFOR;
    private ReferenceFrames CurrentDetailedViewFOR;

    private bool startFlag = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TrialID = GetTrialID();
        switch (ParticipantID % 4) {
            case 1:
                QuestionID = TrialNo;
                CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                if (TrialNo <= 7)
                {
                    CurrentLandmarkFOR = ReferenceFrames.Shelves;
                    CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                }
                else if (TrialNo <= 14)
                {
                    CurrentLandmarkFOR = ReferenceFrames.Body;
                    CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                }
                else if (TrialNo <= 21)
                {
                    CurrentLandmarkFOR = ReferenceFrames.Floor;
                    CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                }
                else {
                    CurrentLandmarkFOR = ReferenceFrames.Floor;
                    CurrentDetailedViewFOR = ReferenceFrames.Body;
                }
                break;
            case 2:
                if (TrialNo <= 7)
                {
                    QuestionID = TrialNo + 7;
                    CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                    CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                    CurrentLandmarkFOR = ReferenceFrames.Body;
                    CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                }
                else if (TrialNo <= 14)
                {
                    QuestionID = TrialNo + 14;
                    CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                    CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                    CurrentLandmarkFOR = ReferenceFrames.Floor;
                    CurrentDetailedViewFOR = ReferenceFrames.Body;
                }
                else if (TrialNo <= 21)
                {
                    QuestionID = TrialNo - 14;
                    CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                    CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                    CurrentLandmarkFOR = ReferenceFrames.Shelves;
                    CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                }
                else
                {
                    QuestionID = TrialNo - 7;
                    CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                    CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                    CurrentLandmarkFOR = ReferenceFrames.Floor;
                    CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                }
                break;
            case 3:
                if (TrialNo <= 7)
                {
                    QuestionID = TrialNo + 21;
                    CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                    CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                    CurrentLandmarkFOR = ReferenceFrames.Floor;
                    CurrentDetailedViewFOR = ReferenceFrames.Body;
                }
                else if (TrialNo <= 14)
                {
                    QuestionID = TrialNo + 7;
                    CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                    CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                    CurrentLandmarkFOR = ReferenceFrames.Floor;
                    CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                }
                else if (TrialNo <= 21)
                {
                    QuestionID = TrialNo - 7;
                    CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                    CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                    CurrentLandmarkFOR = ReferenceFrames.Body;
                    CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                }
                else
                {
                    QuestionID = TrialNo - 21;
                    CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                    CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                    CurrentLandmarkFOR = ReferenceFrames.Shelves;
                    CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                }
                break;
            case 0:
                if (TrialNo <= 7)
                {
                    QuestionID = TrialNo + 14;
                    CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                    CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                    CurrentLandmarkFOR = ReferenceFrames.Floor;
                    CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                }
                else if (TrialNo <= 14)
                {
                    QuestionID = TrialNo - 7;
                    CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                    CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                    CurrentLandmarkFOR = ReferenceFrames.Shelves;
                    CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                }
                else if (TrialNo <= 21)
                {
                    QuestionID = TrialNo + 7;
                    CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                    CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                    CurrentLandmarkFOR = ReferenceFrames.Floor;
                    CurrentDetailedViewFOR = ReferenceFrames.Body;
                }
                else
                {
                    QuestionID = TrialNo - 14;
                    CurrentLandmarkParent = LandmarkParent.Find(QuestionID.ToString());
                    CurrentDetailedViewParent = DetailedViewParent.Find(QuestionID.ToString());
                    CurrentLandmarkFOR = ReferenceFrames.Body;
                    CurrentDetailedViewFOR = ReferenceFrames.Shelves;
                }
                break;
            default:
                break;
        }

        if (startFlag && !Reset) {
            startFlag = false;
            Reset = true;
        }

        if (!Reset && NextBtnPressed) {
            Reset = true;
            NextBtnPressed = false;
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

    private string GetTrialID() {
        if (TrialNo % 7 == 1 || TrialNo % 7 == 2)
            return "Training";
        else
            return (TrialNo - (((TrialNo - 1) / 7) + 1) * 2).ToString();
    }

    public void UpdateTrialID() {
        NextBtnPressed = true;
    }
}
