using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class TaskManager : MonoBehaviour
{
    public ExperimentManager EM;
    public Transform QuestionButton;
    public Transform AnswerButton;
    public Transform QuestionBoard;
    public Transform ConfirmBoard;
    public TextAsset QuestionFile;
    [HideInInspector]
    public List<string> questions;

    private int questionID;
    private int prevQuestionID;

    private bool Answered = false;

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

        if (QuestionButton.GetComponent<VRTK_InteractableObject>().IsUsing()) {
            QuestionButton.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(QuestionButton.localPosition.x, 0, QuestionButton.localPosition.z), Time.deltaTime * 10);
            AnswerButton.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(AnswerButton.localPosition.x, 0.01f, AnswerButton.localPosition.z), Time.deltaTime * 10);

            if (Answered) // show confirm page
            {
                ConfirmBoard.gameObject.SetActive(true);
                QuestionBoard.gameObject.SetActive(false);
                EM.PauseTimer();
            }
        }

        if (AnswerButton.GetComponent<VRTK_InteractableObject>().IsUsing()) {
            if (!Answered) // participants try to answer, pause the timer
            {
                QuestionButton.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(QuestionButton.localPosition.x, 0.01f, QuestionButton.localPosition.z), Time.deltaTime * 10);
                AnswerButton.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(AnswerButton.localPosition.x, 0f, AnswerButton.localPosition.z), Time.deltaTime * 10);

                AnswerButton.GetChild(0).GetChild(0).GetComponent<Text>().text = "Re-do";
                QuestionButton.GetChild(0).GetChild(0).GetComponent<Text>().text = "Next";

                Answered = true;

                EM.PauseTimer();
            }
            else { // participants try to re-do, resume the timer
                Answered = false;
                AnswerButton.GetChild(0).GetChild(0).GetComponent<Text>().text = "Answer";
                QuestionButton.GetChild(0).GetChild(0).GetComponent<Text>().text = "Question";

                EM.ResumeTimer();
            }
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
