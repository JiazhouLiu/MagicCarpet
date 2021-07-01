using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FootGestureController_UserStudy : MonoBehaviour
{
    [Header("Prefabs or OBJ in Scene")]
    public ExperimentManager EM;
    public DashboardController_UserStudy DC;
    public LogManager logManager;
    public Transform GroundLandmarks; 

    [Header("Two Feet")]
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
    public int releaseThresholdLeft = 1000;
    public int pressToSelectThresholdRight = 0;
    public int holdThresholdRight = 500;
    public int releaseThresholdRight = 1000;

    // pressure sensor
    [HideInInspector] public bool leftNormalPressFlag = false;
    [HideInInspector] public bool rightNormalPressFlag = false;
    [HideInInspector] public bool leftHoldingFlag = false;
    [HideInInspector] public bool rightHoldingFlag = false;

    private Vector3 previousLeftPosition;
    private bool leftMoving = false;
    private Vector3 previousRightPosition;
    private bool rightMoving = false;

    private Transform movingOBJ;

    // Start is called before the first frame update
    void Start()
    {
        previousLeftPosition = leftFoot.position;
        previousRightPosition = rightFoot.position;
    }

    // Update is called once per frame
    void Update()
    {
        FootInteractionFeedback();

        PressureSensorDetector();
        previousLeftPosition = leftFoot.position;
        previousRightPosition = rightFoot.position;
    }

    #region Pressure Sensor Detection
    private void PressureSensorDetector()
    {
        // Press Detect - Left
        if (leftSR.value.Length > 0 && int.Parse(leftSR.value) <= pressToSelectThresholdLeft && !leftNormalPressFlag)
            leftNormalPressFlag = true;
        if (leftNormalPressFlag && leftSR.value.Length > 0 && int.Parse(leftSR.value) > releaseThresholdLeft)
        {
            leftNormalPressFlag = false;
            if (!leftMoving && !rightMoving)
            {
                logManager.WriteInteractionToLog("Foot Interaction", "Left Foot Press");
                RunPressToSelect();
            }
        }

        // Press Detect - Right
        if (rightSR.value.Length > 0 && int.Parse(rightSR.value) <= pressToSelectThresholdRight && !rightNormalPressFlag)
            rightNormalPressFlag = true;
        if (rightNormalPressFlag && rightSR.value.Length > 0 && int.Parse(rightSR.value) > releaseThresholdRight)
        {
            rightNormalPressFlag = false;
            if (!leftMoving && !rightMoving)
            {
                logManager.WriteInteractionToLog("Foot Interaction", "Right Foot Press");
                RunPressToSelect();
            }
        }

        // Sliding Detect - Left
        if (leftSR.value.Length > 0)
        {
            if (int.Parse(leftSR.value) < holdThresholdLeft)
                leftHoldingFlag = true;
            else
                leftHoldingFlag = false;
        }

        // Sliding Detect - Right
        if (rightSR.value.Length > 0)
        {
            if (int.Parse(rightSR.value) < holdThresholdRight)
                rightHoldingFlag = true;
            else
                rightHoldingFlag = false;
        }

        if (Vector3.Distance(leftFoot.position, previousLeftPosition) > 0.005f && leftHoldingFlag) // left moving
            leftMoving = true;
        else if (Vector3.Distance(leftFoot.position, previousLeftPosition) <= 0.005f && leftSR.value.Length > 0 && int.Parse(leftSR.value) > releaseThresholdLeft) // left still
            leftMoving = false;

        if(leftFoot.position.y > 0.1f)
            leftMoving = false;

        if (Vector3.Distance(rightFoot.position, previousRightPosition) > 0.005f && rightHoldingFlag) // right moving
            rightMoving = true;
        else if (Vector3.Distance(rightFoot.position, previousRightPosition) <= 0.005f && rightSR.value.Length > 0 && int.Parse(rightSR.value) > releaseThresholdRight) // right still
            rightMoving = false;

        if (rightFoot.position.y > 0.1f)
            rightMoving = false;

        RunPressToSlide();
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
        if (leftMoving)
        {
            if (leftFootToeCollision.TouchedObjs.Count > 0)
            {
                movingOBJ = leftFootToeCollision.TouchedObjs[0];

                movingOBJ.parent = leftFoot;
                movingOBJ.GetComponent<Vis>().Moving = true;

                logManager.WriteInteractionToLog("Foot Interaction", "Left Sliding " + movingOBJ.name);
            }
        }
        else
        {
            if (movingOBJ != null) {
                movingOBJ.parent = EM.GroundDisplay;
                movingOBJ.GetComponent<Vis>().Moving = false;
            }                
        }

        if (rightMoving)
        {
            if (rightFootToeCollision.TouchedObjs.Count > 0)
            {
                movingOBJ = rightFootToeCollision.TouchedObjs[0];

                movingOBJ.parent = rightFoot;
                movingOBJ.GetComponent<Vis>().Moving = true;

                logManager.WriteInteractionToLog("Foot Interaction", "Right Sliding " + movingOBJ.name);
            }
        }
        else
        {
            if (movingOBJ != null)
            {
                movingOBJ.parent = EM.GroundDisplay;
                movingOBJ.GetComponent<Vis>().Moving = false;
            }
        }
    }
    #endregion

    #region Utilities
    private void FootInteractionFeedback() {
        // pressure feedback right
        if (EM.GetCurrentLandmarkFOR() == ReferenceFrames.Floor && rightSR.value.Length > 0 && float.Parse(rightSR.value) < 2000f &&
            rightFootToeCollision.TouchedObjs.Count > 0)
        {
            rightPressFeedback.gameObject.SetActive(true);
            rightPressFeedback.transform.eulerAngles = Vector3.zero;

            float delta = 4095f - pressToSelectThresholdRight;

            rightFeedbackCircle.localScale = Vector3.one * ((4095f - float.Parse(rightSR.value)) / delta * 0.09f + 0.01f);
            if (rightFeedbackCircle.localScale.x > 1)
                rightFeedbackCircle.localScale = Vector3.one;

            if (float.Parse(rightSR.value) <= pressToSelectThresholdRight && !rightMoving)
                rightFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(0, 0, 1, 0.4f));
            else if (float.Parse(rightSR.value) < holdThresholdRight)
                rightFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(1, 0.92f, 0.016f, 0.4f));
            else
                rightFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(1, 0, 0, 0.4f));
        }
        else
            rightPressFeedback.gameObject.SetActive(false);

        // pressure feedback left
        if (EM.GetCurrentLandmarkFOR() == ReferenceFrames.Floor && leftSR.value.Length > 0 && float.Parse(leftSR.value) < 2000f &&
            leftFootToeCollision.TouchedObjs.Count > 0)
        {
            leftPressFeedback.gameObject.SetActive(true);
            leftPressFeedback.transform.eulerAngles = Vector3.zero;

            float delta = 4095f - pressToSelectThresholdLeft;

            leftFeedbackCircle.localScale = Vector3.one * ((4095f - float.Parse(leftSR.value)) / delta * 0.09f + 0.01f);
            if (leftFeedbackCircle.localScale.x > 1)
                leftFeedbackCircle.localScale = Vector3.one;

            if (float.Parse(leftSR.value) <= pressToSelectThresholdLeft && !leftMoving)
                leftFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(0, 0, 1, 0.4f));
            else if (float.Parse(leftSR.value) < holdThresholdLeft)
                leftFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(1, 0.92f, 0.016f, 0.4f));
            else
                leftFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(1, 0, 0, 0.4f));
        }
        else
            leftPressFeedback.gameObject.SetActive(false);
    }
    #endregion
}
