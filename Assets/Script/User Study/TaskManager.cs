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

    private bool QuestionButtonUsed = false;
    private bool AnswerButtonUsed = false;

    private char lineSeperater = '\n'; // It defines line seperate character
    private char fieldSeperator = ','; // It defines field seperate chracter
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
    }

    private void UpdateUI(int questionID) {
        DisplayQuestionOnBoard(questions[questionID - 1]);
        if (TrainingScene)
            TitleText.text = "Training Question " + questionID + "/4";
        else {
            if ((questionID - 1) % 5 == 0)
                TitleText.text = "Training Question " + questionID + "/20";
            else
                TitleText.text = "Experiment Question " + questionID + "/20";
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
