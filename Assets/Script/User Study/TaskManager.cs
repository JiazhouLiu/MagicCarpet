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
    [HideInInspector]
    public List<string> questions;

    private int questionID;
    private int prevQuestionID;

    public bool Answered = false;

    private bool QuestionButtonUsed = false;
    private bool AnswerButtonUsed = false;

    private char lineSeperater = '\n'; // It defines line seperate character
    private char fieldSeperator = ','; // It defines field seperate chracter

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
    }

    private void ReadQuestionsFromFile()
    {
        string[] lines = QuestionFile.text.Split(lineSeperater);

        questions.AddRange(lines);
    }

    private void DisplayQuestionOnBoard(string question)
    {
        string[] lines = question.Split(lineSeperater);
        string final = "";
        foreach (string s in lines)
            final += s;
        QuestionBoard.GetChild(0).GetChild(0).GetComponent<Text>().text = final;
    }
}
