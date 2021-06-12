using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SelectedAnswer : MonoBehaviour
{
    [SerializeField]
    private TaskManager TM;
    [SerializeField]
    private VRTK_InteractableObject interactableObject;
    [SerializeField]
    private bool selected;

    // Start is called before the first frame update
    void Start()
    {
        // Subscribe to events
        interactableObject.InteractableObjectUsed += VisUsed;
    }

    // Update is called once per frame
    void Update()
    {
        if (selected)
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, 0.01f, transform.localPosition.z), Time.deltaTime * 10);
        else
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, 1f, transform.localPosition.z), Time.deltaTime * 10);
    }

    private void VisUsed(object sender, InteractableObjectEventArgs e)
    {
        if (selected)
            selected = false;
        else
            selected = true;

        if (name == "True")
            TM.EM.NextQuestion();
        else {
            TM.ConfirmBoard.gameObject.SetActive(false);
            TM.QuestionBoard.gameObject.SetActive(true);
            TM.EM.ResumeTimer();
        }
    }
}
