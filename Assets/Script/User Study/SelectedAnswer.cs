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
    private VRTK_ControllerEvents leftCE;
    [SerializeField]
    private VRTK_ControllerEvents rightCE;
    [SerializeField]
    private bool selected;

    private bool selecting = false;

    // Start is called before the first frame update
    void Start()
    {
        // Subscribe to events
        interactableObject.InteractableObjectUsed -= VisUsed;
        interactableObject.InteractableObjectUsed += VisUsed;
    }

    // Update is called once per frame
    void Update()
    {
        if (selected)
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, 0.01f, transform.localPosition.z), Time.deltaTime * 10);
        else
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, 1f, transform.localPosition.z), Time.deltaTime * 10);

        if (interactableObject.IsUsing())
            selecting = true;
        else
        {
            if (selecting) {
                selecting = false;
                ButtonFunction();
            }
        }

        if (leftCE.triggerClicked)
            SteamVR_Controller.Input(TM.EM.leftHandIndex).TriggerHapticPulse(1500);
        if (rightCE.triggerClicked)
            SteamVR_Controller.Input(TM.EM.rightHandIndex).TriggerHapticPulse(1500);
    }

    private void ButtonFunction() {
        switch (name)
        {
            case "Start":
                TM.EM.logManager.WriteInteractionToLog("TaskBoard Button", "Start");
                selected = false;

                TM.StartBoard.gameObject.SetActive(false); // hide start board
                TM.QuestionBoard.gameObject.SetActive(true); // show question board

                TM.EM.StartTimer(); // start timer
                break;
            case "Answer":
                TM.EM.logManager.WriteInteractionToLog("TaskBoard Button", "Answer");
                selected = false;

                TM.ConfirmBoard.gameObject.SetActive(true); // show confirmation board
                TM.QuestionBoard.gameObject.SetActive(false); // hide question board

                TM.EM.PauseTimer(); // pause timer
                break;
            case "Confirm":
                TM.EM.logManager.WriteInteractionToLog("TaskBoard Button", "Confirm");
                selected = false;

                TM.StartBoard.gameObject.SetActive(true); // show start board
                TM.ConfirmBoard.gameObject.SetActive(false); // hide confirmation board

                TM.EM.NextQuestion(); // next question
                break;
            case "GoBack":
                TM.EM.logManager.WriteInteractionToLog("TaskBoard Button", "Go Back");

                selected = false;
                TM.ConfirmBoard.gameObject.SetActive(false); // hide confirmation board
                TM.QuestionBoard.gameObject.SetActive(true); // show question board

                TM.EM.ResumeTimer(); // resume timer
                break;
            default:
                break;
        }
    }
    private void VisUsed(object sender, InteractableObjectEventArgs e)
    {
        if (selected)
            selected = false;
        else
            selected = true;
    }
}
