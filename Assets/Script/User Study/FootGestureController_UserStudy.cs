using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum RepositionMethod { 
    Sliding,
    DragAndDrop
};

public class FootGestureController_UserStudy : MonoBehaviour
{
    [Header("Prefabs or OBJ in Scene")]
    public ExperimentManager EM;
    public DashboardController_UserStudy DC;
    public LogManager logManager;
    public Transform GroundLandmarks; 

    [Header("Two Feet")]
    //public Transform mainFoot;
    //public Transform mainFootToe;
    // left foot
    public Transform leftFoot;
    public Transform leftFootToe;
    public FootToeCollision leftFootToeCollision;
    public ShoeRecieve leftSR;
    public Transform leftPressFeedback;
    public Transform leftFeedbackCircle;
    // right foot
    public Transform rightFoot;
    public Transform rightFootToe;
    public FootToeCollision rightFootToeCollision;
    public ShoeRecieve rightSR;
    public Transform rightPressFeedback;
    public Transform rightFeedbackCircle;

    [Header("PressureSensor")]
    public int pressToSelectThresholdLeft = 0;
    public int holdThresholdLeft = 500;
    public int pressToSelectThresholdRight = 0;
    public int holdThresholdRight = 500;


    // pressure sensor
    [HideInInspector] public bool leftNormalPressFlag = false;
    [HideInInspector] public bool rightNormalPressFlag = false;
    [HideInInspector] public bool leftHoldingFlag = false;
    [HideInInspector] public bool rightHoldingFlag = false;

    private List<Transform> interactingOBJ;
    private List<Transform> movingOBJ;

    // Start is called before the first frame update
    void Start()
    {
        interactingOBJ = new List<Transform>();
        movingOBJ = new List<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        // pressure feedback right
        if (EM.GetCurrentLandmarkFOR() == ReferenceFrames.Floor && rightSR.value.Length > 0 && float.Parse(rightSR.value) < 3000f && 
            ((leftFootToeCollision.TouchedObjs.Count > 0) || (rightFootToeCollision.TouchedObjs.Count > 0))) {
            rightPressFeedback.gameObject.SetActive(true);
            rightPressFeedback.transform.eulerAngles = Vector3.zero;

            float delta = 4095f - pressToSelectThresholdRight;

            rightFeedbackCircle.localScale = Vector3.one * ((4095f - float.Parse(rightSR.value))/ delta * 0.09f + 0.01f);
            if (rightFeedbackCircle.localScale.x > 1)
                leftFeedbackCircle.localScale = Vector3.one;

            if (float.Parse(rightSR.value) <= pressToSelectThresholdRight)
                rightFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(0, 0, 1, 0.4f));
            else if (float.Parse(rightSR.value) < holdThresholdRight)
                rightFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(1, 0.92f, 0.016f, 0.4f));
            else
                rightFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(1, 0, 0, 0.4f));
        }else
            rightPressFeedback.gameObject.SetActive(false);

        // pressure feedback left
        if (EM.GetCurrentLandmarkFOR() == ReferenceFrames.Floor && leftSR.value.Length > 0 && float.Parse(leftSR.value) < 3000f &&
            ((leftFootToeCollision.TouchedObjs.Count > 0) || (rightFootToeCollision.TouchedObjs.Count > 0)))
        {
            leftPressFeedback.gameObject.SetActive(true);
            leftPressFeedback.transform.eulerAngles = Vector3.zero;

            float delta = 4095f - pressToSelectThresholdLeft;

            leftFeedbackCircle.localScale = Vector3.one * ((4095f - float.Parse(leftSR.value)) / delta * 0.09f + 0.01f);
            if (leftFeedbackCircle.localScale.x > 1)
                leftFeedbackCircle.localScale = Vector3.one;

            if (float.Parse(leftSR.value) <= pressToSelectThresholdLeft)
                leftFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(0, 0, 1, 0.4f));
            else if (float.Parse(leftSR.value) < holdThresholdLeft)
                leftFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(1, 0.92f, 0.016f, 0.4f));
            else
                leftFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(1, 0, 0, 0.4f));
        }
        else
            leftPressFeedback.gameObject.SetActive(false);

        PressureSensorDetector();
    }

    #region Pressure Sensor Detection
    private void PressureSensorDetector()
    {
        // pressure sensor
        if (leftSR.value.Length > 0 && int.Parse(leftSR.value) <= pressToSelectThresholdLeft && !leftNormalPressFlag)
        {
            leftNormalPressFlag = true;
            //Debug.Log("Press - left");
            logManager.WriteInteractionToLog("Left Foot Press");
            RunPressToSelect();
        }
        if (leftNormalPressFlag && leftSR.value.Length > 0 && int.Parse(leftSR.value) > pressToSelectThresholdLeft)
        {
            leftNormalPressFlag = false;
        }

        if (rightSR.value.Length > 0 && int.Parse(rightSR.value) <= pressToSelectThresholdRight && !rightNormalPressFlag)
        {
            rightNormalPressFlag = true;
            //Debug.Log("Press - right");
            logManager.WriteInteractionToLog("Right Foot Press");
            RunPressToSelect();
        }
        if (rightNormalPressFlag && rightSR.value.Length > 0 && int.Parse(rightSR.value) > pressToSelectThresholdRight)
        {
            rightNormalPressFlag = false;
        }


        if (leftSR.value.Length > 0)
        {
            if ((int.Parse(leftSR.value) < holdThresholdLeft) && (int.Parse(leftSR.value) > pressToSelectThresholdLeft))
            {
                if (!leftHoldingFlag)
                    leftHoldingFlag = true;
                logManager.WriteInteractionToLog("Left Foot Sliding");
            }
            else
            {
                if (leftHoldingFlag)
                    leftHoldingFlag = false;
            }
            RunPressToSlide();
        }

        if (rightSR.value.Length > 0) { 
            if ((int.Parse(rightSR.value) < holdThresholdRight) && (int.Parse(rightSR.value) > pressToSelectThresholdRight))
            {
                if (!rightHoldingFlag)
                    rightHoldingFlag = true;
                logManager.WriteInteractionToLog("Right Foot Sliding");
            }
            else
            {
                if (rightHoldingFlag)
                    rightHoldingFlag = false;
            }
            RunPressToSlide();
        }  
    }
    #endregion

    #region Foot Press using pressure sensor
    private void RunPressToSelect()
    {
        if (leftFootToeCollision.TouchedObjs.Count > 0)
        {
            foreach (Transform t in leftFootToeCollision.TouchedObjs)
            {
                if (t.GetComponent<Vis>().Selected)
                    DC.RemoveExplicitSelection(t);
                else
                    DC.AddExplicitSelection(t);
            }
        }

        if (rightFootToeCollision.TouchedObjs.Count > 0)
        {
            foreach (Transform t in rightFootToeCollision.TouchedObjs)
            {
                if (t.GetComponent<Vis>().Selected)
                    DC.RemoveExplicitSelection(t);
                else
                    DC.AddExplicitSelection(t);
            }
        }
    }

    private void RunPressToSlide()
    {
        if (leftHoldingFlag)
        {
            if (leftFootToeCollision.TouchedObjs.Count > 0)
            {
                foreach (Transform t in leftFootToeCollision.TouchedObjs)
                {
                    if (!movingOBJ.Contains(t))
                        movingOBJ.Add(t);
                        
                    t.parent = leftFoot;
                    t.GetComponent<Vis>().Moving = true;
                }
            }
        }
        else
        {
            if (movingOBJ.Count > 0) {
                foreach (Transform t in movingOBJ) {
                    t.parent = EM.GroundDisplay;
                    t.GetComponent<Vis>().Moving = false;
                }

                movingOBJ.Clear();
            }                
        }

        if (rightHoldingFlag)
        {
            if (rightFootToeCollision.TouchedObjs.Count > 0)
            {
                foreach (Transform t in rightFootToeCollision.TouchedObjs)
                {
                    if (!movingOBJ.Contains(t))
                        movingOBJ.Add(t);

                    t.parent = rightFoot;
                    t.GetComponent<Vis>().Moving = true;
                }
            }
        }
        else
        {
            if (movingOBJ.Count > 0)
            {
                foreach (Transform t in movingOBJ)
                {
                    t.parent = EM.GroundDisplay;
                    t.GetComponent<Vis>().Moving = false;
                }

                movingOBJ.Clear();
            }
        }
    }
    #endregion

    #region Utilities
    private void RegisterInteractingOBJ(Transform t)
    {
        interactingOBJ.Add(t);
        if (t.GetComponent<Vis>() != null)
        {
            t.GetComponent<Vis>().Selected = true;
        }
    }

    private bool DeregisterInteractingOBJ(Transform t)
    {
        if (interactingOBJ.Contains(t))
        {
            interactingOBJ.Remove(t);
            return true;
        }
        else
            return false;
    }

    private void DeregisterInteractingOBJ()
    {
        foreach (Transform t in interactingOBJ.ToList())
        {
            interactingOBJ.Remove(t);
        }
    }
    #endregion
}
