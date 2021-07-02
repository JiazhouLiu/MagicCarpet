using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class SelectedAnswer : MonoBehaviour
{
    [SerializeField]
    private TaskManager TM;
    [SerializeField]
    private VRTK_InteractableObject interactableObject;
    [SerializeField]
    private bool selected;

    private bool selecting = false;

    // Start is called before the first frame update
    void Start()
    {
        // Subscribe to events
        interactableObject.InteractableObjectUsed -= VisUsed;
        //interactableObject.InteractableObjectUnused -= VisUnused;
        interactableObject.InteractableObjectUsed += VisUsed;
        //interactableObject.InteractableObjectUnused += VisUnused;
    }

    // Update is called once per frame
    void Update()
    {
        if (selected) {
            if (TM.transform.parent.name.Contains("left"))
                SteamVR_Controller.Input(TM.EM.rightHandIndex).TriggerHapticPulse(1500);
            else
                SteamVR_Controller.Input(TM.EM.leftHandIndex).TriggerHapticPulse(1500);

            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, 0.01f, transform.localPosition.z), Time.deltaTime * 10);
        }
        else
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, 1f, transform.localPosition.z), Time.deltaTime * 10);

        if (interactableObject.IsUsing())
        {
            selecting = true;
        }
        else
        {
            if (selecting) {
                selecting = false;
                ButtonFunction();
            }
        }
    }

    private void ButtonFunction() {
        switch (name)
        {
            case "Confirm":
                TM.EM.logManager.WriteInteractionToLog("TaskBoard Button", "Confirm");
                selected = false;

                TM.ConfirmBoard.gameObject.SetActive(false);
                TM.QuestionBoard.gameObject.SetActive(true);

                TM.AnswerButton.gameObject.SetActive(true);
                TM.NextButton.gameObject.SetActive(false);
                TM.RedoButton.gameObject.SetActive(false);

                TM.Answered = false;
                TM.gameObject.SetActive(false);
                TM.EM.NextQuestion();
                break;
            case "GoBack":
                TM.EM.logManager.WriteInteractionToLog("TaskBoard Button", "Go Back");

                selected = false;
                TM.ConfirmBoard.gameObject.SetActive(false);
                TM.QuestionBoard.gameObject.SetActive(true);

                TM.AnswerButton.gameObject.SetActive(true);
                TM.NextButton.gameObject.SetActive(false);
                TM.RedoButton.gameObject.SetActive(false);

                TM.EM.ResumeTimer();
                break;
            case "Answer":
                TM.EM.logManager.WriteInteractionToLog("TaskBoard Button", "Answer");

                selected = false;
                TM.AnswerButton.gameObject.SetActive(false);
                TM.NextButton.gameObject.SetActive(true);
                TM.RedoButton.gameObject.SetActive(true);

                TM.EM.PauseTimer();
                break;
            case "Re-do":
                TM.EM.logManager.WriteInteractionToLog("TaskBoard Button", "Re-do");

                selected = false;
                TM.ConfirmBoard.gameObject.SetActive(false);
                TM.QuestionBoard.gameObject.SetActive(true);

                TM.AnswerButton.gameObject.SetActive(true);
                TM.NextButton.gameObject.SetActive(false);
                TM.RedoButton.gameObject.SetActive(false);

                TM.EM.ResumeTimer();
                break;
            case "Next":
                TM.EM.logManager.WriteInteractionToLog("TaskBoard Button", "Next");

                selected = false;
                TM.ConfirmBoard.gameObject.SetActive(true);
                TM.QuestionBoard.gameObject.SetActive(false);

                TM.AnswerButton.gameObject.SetActive(true);
                TM.NextButton.gameObject.SetActive(false);
                TM.RedoButton.gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    //private void VisUnused(object sender, InteractableObjectEventArgs e) {
    //    switch (name)
    //    {
    //        case "Confirm":
    //            TM.EM.logManager.WriteInteractionToLog("TaskBoard Button", "Confirm");
    //            selected = false;
                
    //            TM.ConfirmBoard.gameObject.SetActive(false);
    //            TM.QuestionBoard.gameObject.SetActive(true);

    //            TM.AnswerButton.gameObject.SetActive(true);
    //            TM.NextButton.gameObject.SetActive(false);
    //            TM.RedoButton.gameObject.SetActive(false);

    //            TM.Answered = false;
    //            TM.gameObject.SetActive(false);
    //            TM.EM.NextQuestion();
    //            break;
    //        case "GoBack":
    //            TM.EM.logManager.WriteInteractionToLog("TaskBoard Button", "Go Back");

    //            selected = false;
    //            TM.ConfirmBoard.gameObject.SetActive(false);
    //            TM.QuestionBoard.gameObject.SetActive(true);

    //            TM.AnswerButton.gameObject.SetActive(true);
    //            TM.NextButton.gameObject.SetActive(false);
    //            TM.RedoButton.gameObject.SetActive(false);

    //            TM.EM.ResumeTimer();
    //            break;
    //        case "Answer":
    //            TM.EM.logManager.WriteInteractionToLog("TaskBoard Button", "Answer");

    //            selected = false;
    //            TM.AnswerButton.gameObject.SetActive(false);
    //            TM.NextButton.gameObject.SetActive(true);
    //            TM.RedoButton.gameObject.SetActive(true);

    //            TM.EM.PauseTimer();
    //            break;
    //        case "Re-do":
    //            TM.EM.logManager.WriteInteractionToLog("TaskBoard Button", "Re-do");

    //            selected = false;
    //            TM.ConfirmBoard.gameObject.SetActive(false);
    //            TM.QuestionBoard.gameObject.SetActive(true);

    //            TM.AnswerButton.gameObject.SetActive(true);
    //            TM.NextButton.gameObject.SetActive(false);
    //            TM.RedoButton.gameObject.SetActive(false);

    //            TM.EM.ResumeTimer();
    //            break;
    //        case "Next":
    //            TM.EM.logManager.WriteInteractionToLog("TaskBoard Button", "Next");

    //            selected = false;
    //            TM.ConfirmBoard.gameObject.SetActive(true);
    //            TM.QuestionBoard.gameObject.SetActive(false);

    //            TM.AnswerButton.gameObject.SetActive(true);
    //            TM.NextButton.gameObject.SetActive(false);
    //            TM.RedoButton.gameObject.SetActive(false);
    //            break;
    //        default:
    //            break;
    //    }
    //}

    private void VisUsed(object sender, InteractableObjectEventArgs e)
    {
        if (selected)
            selected = false;
        else
            selected = true;
    }
}
