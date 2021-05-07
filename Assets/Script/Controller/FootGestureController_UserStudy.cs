using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FootGestureController_UserStudy : MonoBehaviour
{
    [Header("Prefabs or OBJ in Scene")]
    public Transform GroundLandmarks;
    public FootToeCollision FTC;
    public ShoeRecieve SR;
    public Transform testCube;

    [Header("Main Foot")]
    public Transform mainFoot;
    public Transform mainFootToe;

    [Header("Variable")]
    public int windowFrames = 10;    // buff frames before detecting sliding
    public float BlindSelectionRange = 1f;
    public float ToeSlideMoveMultiplier = 2f;
    public float filterFrequency = 120f;

    [Header("PressureSensor")]
    public int pressThreshold = 3700;
    public int holdThreshold = 4000;

    private Vector3 previousRotation;
    private Vector3 previousToePosition;

    // pressure sensor
    private bool physicalPressFlag = false;
    private bool holdingFlag = false;

    private List<Transform> interactingOBJ;
    private List<Transform> currentSelectedVis;

    // one euro filter
    private Vector3 filteredFootRotation;
    private OneEuroFilter<Vector3> vector3Filter;

    // Start is called before the first frame update
    void Start()
    {
        interactingOBJ = new List<Transform>();
        currentSelectedVis = new List<Transform>();

        vector3Filter = new OneEuroFilter<Vector3>(filterFrequency);
    }

    // Update is called once per frame
    void Update()
    {
        // one euro filter
        filteredFootRotation = vector3Filter.Filter(mainFoot.eulerAngles);

        PressureSensorDetector();

        previousRotation = filteredFootRotation;



        // testing rotation with cube
        //Debug.Log(mainFoot.eulerAngles);
        Vector3 rotationV3 = filteredFootRotation - previousRotation;
        Vector3 rotationV2 = new Vector3(0, rotationV3.y, 0);

        testCube.eulerAngles += rotationV3;
        //testCube.localEulerAngles = new Vector3(0, testCube.localEulerAngles.y, 0); 
    }


    #region Pressure Sensor Detection
    private void PressureSensorDetector()
    {
        // pressure sensor
        if (SR.value.Length > 0 && int.Parse(SR.value) < pressThreshold && !physicalPressFlag)
        {
            physicalPressFlag = true;
            Debug.Log("Press");
            RunPressToSelect();
        }
        if (physicalPressFlag && SR.value.Length > 0 && int.Parse(SR.value) > holdThreshold)
        {
            physicalPressFlag = false;
        }
        if (SR.value.Length > 0)
        {
            if (int.Parse(SR.value) < holdThreshold)
                holdingFlag = true;
            else
                holdingFlag = false;
        }

        if (holdingFlag)
        {
            Debug.Log("Holding");
            previousToePosition = mainFootToe.position;
            RunPressToMove();
        }
    }
    #endregion


    #region Foot Press using pressure sensor
    private void RunPressToSelect()
    {
        if (FTC.TouchedObjs.Count > 0)
        {
            foreach (Transform t in FTC.TouchedObjs)
            {
                if (!DeregisterInteractingOBJ(t))
                    RegisterInteractingOBJ(t);
            }
        }
        else
        {
            if (interactingOBJ.Count > 0)
                DeregisterInteractingOBJ();
        }
    }

    private void RunPressToMove()
    {
        Vector3 moveV3 = mainFootToe.position - previousToePosition;
        Vector3 moveV2 = new Vector3(moveV3.x, 0, moveV3.z);
        List<float> footToeHeight = new List<float>();
        List<float> distance = new List<float>();

        if (FTC.TouchedObjs.Count > 0)
        {
            foreach (Transform t in FTC.TouchedObjs)
                t.position += moveV2 * ToeSlideMoveMultiplier;
        }


        previousToePosition = mainFootToe.position;
    }

    private void RunPressToRotate()
    {

    }
    #endregion

    #region Utilities
    private void RegisterInteractingOBJ(Transform t)
    {
        interactingOBJ.Add(t);
        if (t.GetComponent<Vis>() != null)
        {
            t.GetComponent<Vis>().Selected = true;
            currentSelectedVis.Add(t);
            if (currentSelectedVis.Count > 3)
            {
                DeregisterInteractingOBJ(currentSelectedVis.First());
            }

        }

        //DC.RemoveFromHeadDashboard(t);
        //t.GetComponent<Vis>().OnGround = false;
        //t.GetComponent<Vis>().OnWaistDashBoard = true;
    }

    private bool DeregisterInteractingOBJ(Transform t)
    {
        if (interactingOBJ.Contains(t))
        {
            if (t.GetComponent<Vis>() != null)
            {
                if (currentSelectedVis.Contains(t))
                    currentSelectedVis.Remove(t);
                t.GetComponent<Vis>().Selected = false;
            }

            interactingOBJ.Remove(t);

            //Light highlighter = t.GetChild(2).GetComponent<Light>();
            //highlighter.intensity = 0;

            //t.GetComponent<VisController>().AttachToDisplayScreen(ds);

            return true;
        }
        else
            return false;
    }

    private void DeregisterInteractingOBJ()
    {
        foreach (Transform t in interactingOBJ.ToList())
        {
            if (t.GetComponent<Vis>() != null)
            {
                if (currentSelectedVis.Contains(t))
                    currentSelectedVis.Remove(t);
                t.GetComponent<Vis>().Selected = false;
            }

            interactingOBJ.Remove(t);

            //Light highlighter = t.GetChild(2).GetComponent<Light>();
            //highlighter.intensity = 0;
            //t.GetComponent<VisController>().AttachToDisplayScreen(ds);
        }
    }

    private List<Transform> CheckNearestVisOnGround()
    {
        List<Transform> rangeSelected = new List<Transform>();

        foreach (Transform t in GroundLandmarks)
        {
            Vector3 CameraPosition2D = new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z);
            Vector3 VisPosition2D = new Vector3(t.position.x, 0, t.position.z);
            if (Vector3.Distance(CameraPosition2D, VisPosition2D) < BlindSelectionRange)
                rangeSelected.Add(t);
        }
        return rangeSelected;
    }
    #endregion
}
