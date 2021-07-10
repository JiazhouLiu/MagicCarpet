using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class TaskManager : MonoBehaviour
{
    public ExperimentManager EM;
    public Transform AnswerButton;
    public Transform RedoButton;
    public Transform NextButton;
    public Transform QuestionBoard;
    public Transform ConfirmBoard;
    public TextAsset QuestionFile;
    public Text TitleText;
    public Text BodyText;
    public bool TrainingScene = false;
    [HideInInspector]
    public List<string> questions;

    private int questionID;
    private int prevQuestionID;
    [HideInInspector]
    public bool Answered = false;
    [HideInInspector]
    public bool initialised = false;

    private char lineSeperater = '\n'; // It defines line seperate character
    private char AlineSeperater = '&';

    // Start is called before the first frame update
    void Awake()
    {
        questions = new List<string>();

        prevQuestionID = 0;
        ReadQuestionsFromFile();
    }

    // Update is called once per frame
    void Update()
    {
        questionID = EM.QuestionID;
        if (questionID != prevQuestionID) {
            prevQuestionID = questionID;
            UpdateUI(questionID);
        }

        if (!initialised && Camera.main != null) {
            initialised = true;

            Vector3 oldAngle = Camera.main.transform.eulerAngles;
            Camera.main.transform.eulerAngles = new Vector3(0, oldAngle.y, oldAngle.z);
            Vector3 forward = Camera.main.transform.forward;
            Camera.main.transform.eulerAngles = oldAngle;

            transform.position = Camera.main.transform.TransformPoint(Vector3.zero) + forward * EM.armLength;
            transform.position -= Vector3.up * 0.1f;

            transform.LookAt(Camera.main.transform);
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x - 90, transform.localEulerAngles.y, transform.localEulerAngles.z + 180);
        }
    }

    private void UpdateUI(int questionID) {
        DisplayQuestionOnBoard(questions[questionID - 1]);
        if (TrainingScene)
            TitleText.text = "Training Question " + questionID + "/4";
        else {
            if ((EM.TrialNo - 1) % 4 == 0)
                TitleText.text = "Training Question " + EM.TrialNo + "/16";
            else
                TitleText.text = "Experiment Question " + EM.TrialNo + "/16";
        }
    }

    private void ReadQuestionsFromFile()
    {
        string[] lines = QuestionFile.text.Split(lineSeperater);

        questions.AddRange(lines);
    }

    private void DisplayQuestionOnBoard(string question)
    {
        string[] lines = question.Split(AlineSeperater);
        string final = "";
        foreach (string s in lines)
            final += s + "\n";
        BodyText.text = final;
    }
}
