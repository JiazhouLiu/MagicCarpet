using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [HideInInspector]
    public List<string> questions;

    [Header("Reference")]
    public Transform LandmarkParent;
    public Transform DetailedViewParent;
    public TextAsset QuestionFile;
    public Transform QuestionBoard;

    private Transform CurrentLandmarkParent;
    private Transform CurrentDetailedViewParent;
    private ReferenceFrames CurrentLandmarkFOR;
    private ReferenceFrames CurrentDetailedViewFOR;

    private bool startFlag = true;

    private char lineSeperater = '\n'; // It defines line seperate character
    private char fieldSeperator = ','; // It defines field seperate chracter

    // Start is called before the first frame update
    void Start()
    {
        questions = new List<string>();
        ReadQuestionsFromFile();
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

        DisplayQuestionOnBoard(questions[QuestionID - 1]);
        
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

    private void ReadQuestionsFromFile() {
        string[] lines = QuestionFile.text.Split(lineSeperater);

        questions.AddRange(lines);
    }

    private void DisplayQuestionOnBoard(string question) {
        string[] lines = question.Split(lineSeperater);
        string final = "";
        foreach (string s in lines)
            final += s;
        QuestionBoard.GetChild(0).GetChild(0).GetComponent<Text>().text = final;
    }
}
