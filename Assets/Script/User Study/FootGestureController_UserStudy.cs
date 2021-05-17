using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum RepositionMethod { 
    Sliding,
    DragAndDrop
};

public enum RotationMethod
{
    SpinFoot,
    DragToRotate,
    AttachToToe
};

public class FootGestureController_UserStudy : MonoBehaviour
{
    [Header("Prefabs or OBJ in Scene")]
    public DashboardController_UserStudy DC;
    public Transform GroundLandmarks;
    public FootToeCollision FTC;
    public ShoeRecieve SR;
    public Transform PressFeedback;
    public Transform FeedbackCircle;

    [Header("Main Foot")]
    public Transform mainFoot;
    public Transform mainFootToe;

    [Header("Variable")]
    public RepositionMethod moveMethod;
    public RotationMethod rotationMethod;

    [Header("PressureSensor")]
    public int firmPressThreashold = 0;
    public int pressToSelectThreshold = 0;
    public int holdThreshold = 500;

    private Vector3 previousDirection;
    private Vector3 previousToePosition;
    private Dictionary<string, Vector3> previousFromCenterToFoot;
    private Dictionary<Transform, Transform> previousParent;

    // pressure sensor
    private bool normalPressFlag = false;
    private bool firmPressFlag = false;
    private bool dragAndDropFlag = false;
    private bool holdingFlag = false;

    private List<Transform> interactingOBJ;
    private List<Transform> movingOBJ;

    // Start is called before the first frame update
    void Start()
    {
        interactingOBJ = new List<Transform>();
        movingOBJ = new List<Transform>();
        previousFromCenterToFoot = new Dictionary<string, Vector3>();
        previousParent = new Dictionary<Transform, Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        // pressure feedback
        if (SR.value.Length > 0 && float.Parse(SR.value) < 3500f) {
            PressFeedback.gameObject.SetActive(true);
            PressFeedback.position = mainFootToe.position;
            FeedbackCircle.localScale = Vector3.one * ((4095f - float.Parse(SR.value))/ 4095f * 0.09f + 0.01f);
            if (float.Parse(SR.value) <= pressToSelectThreshold)
                FeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(0, 0, 1, 0.4f));
            else if (float.Parse(SR.value) < holdThreshold)
                FeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(1, 0.92f, 0.016f, 0.4f));
            else
                FeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(1, 0, 0, 0.4f));
        }else
            PressFeedback.gameObject.SetActive(false);

        PressureSensorDetector();

        //VisRotation();
    }

    #region Vis Rotation
    private void VisRotation() {
        if (rotationMethod == RotationMethod.SpinFoot)
        {
            if (FTC.TouchedObjs.Count > 0)
            {
                foreach (Transform t in FTC.TouchedObjs)
                {
                    Vector3 currentDirection = mainFootToe.position - mainFoot.position;
                    float angle = Vector3.SignedAngle(currentDirection, previousDirection, Vector3.up);

                    t.RotateAround(t.position, Vector3.up, -angle);
                }
            }

            previousDirection = mainFootToe.position - mainFoot.position;
        }
        else if (rotationMethod == RotationMethod.DragToRotate)
        {
            if (FTC.TouchedObjs.Count > 0)
            {
                foreach (Transform t in FTC.TouchedObjs)
                {
                    Vector3 currentFromCenterToFoot = mainFoot.position - t.position;
                    if (previousFromCenterToFoot.Count > 0)
                    {
                        float angle = Vector3.SignedAngle(currentFromCenterToFoot, previousFromCenterToFoot[t.name], Vector3.up);

                        t.RotateAround(t.position, Vector3.up, -angle);
                    }

                }
            }
        }
        else if (rotationMethod == RotationMethod.AttachToToe) {
            if (holdingFlag)
            {
                if (FTC.TouchedObjs.Count > 0)
                {
                    foreach (Transform t in FTC.TouchedObjs)
                    {
                        if (!previousParent.ContainsKey(t))
                            previousParent.Add(t, t.parent);
                        t.parent = mainFootToe;
                    }
                }
            }
            else {
                foreach (Transform t in FTC.TouchedObjs)
                {
                    if (previousParent.ContainsKey(t))
                        t.parent = previousParent[t];
                }
            }
            
        }

        foreach (Transform t in GroundLandmarks)
        {
            if (!previousFromCenterToFoot.ContainsKey(t.name))
            {
                previousFromCenterToFoot.Add(t.name, mainFoot.position - t.position);
            }
            else
            {
                previousFromCenterToFoot[t.name] = mainFoot.position - t.position;
            }
        }
    }
    #endregion

    #region Pressure Sensor Detection
    private void PressureSensorDetector()
    {
        // pressure sensor
        if (SR.value.Length > 0 && int.Parse(SR.value) < pressToSelectThreshold && !normalPressFlag)
        {
            normalPressFlag = true;
            Debug.Log("Press");
            RunPressToSelect();
        }
        if (normalPressFlag && SR.value.Length > 0 && int.Parse(SR.value) > pressToSelectThreshold)
        {
            normalPressFlag = false;
        }


        if (moveMethod == RepositionMethod.Sliding)
        {
            if (SR.value.Length > 0)
            {
                if (int.Parse(SR.value) < holdThreshold)
                {
                    if (!holdingFlag)
                    {
                        previousToePosition = mainFootToe.position;
                        holdingFlag = true;
                    }
                }
                else
                {
                    if (holdingFlag && Vector3.Distance(previousToePosition, mainFootToe.position) < 0.01f) {
                        holdingFlag = false;
                    }
                        
                }
            }
            RunPressToSlide();
            //if (holdingFlag)
            //{
            //    Debug.Log("Holding");

            //    RunPressToSlide();
            //}
            //else {
            //    foreach (Transform t in GroundLandmarks) {
            //        t.GetComponent<Vis>().Moving = false;
            //    }
            //}
        }
        else if(moveMethod == RepositionMethod.DragAndDrop)
        {
            if (SR.value.Length > 0 && int.Parse(SR.value) < firmPressThreashold && !firmPressFlag)
            {
                firmPressFlag = true;
                if (dragAndDropFlag) {
                    dragAndDropFlag = false;

                    movingOBJ.Clear();
                } 
                else {
                    dragAndDropFlag = true;
                    if (FTC.TouchedObjs.Count > 0)
                    {
                        foreach (Transform t in FTC.TouchedObjs)
                        {
                            if (!movingOBJ.Contains(t))
                                movingOBJ.Add(t);
                        }
                    }
                }
                    
                Debug.Log("Firm Press");
            }

            if (SR.value.Length > 0 && int.Parse(SR.value) > firmPressThreashold && firmPressFlag)
                firmPressFlag = false;

            if (dragAndDropFlag)
                RunPressToDragAndDrop();
            else {
                foreach (Transform t in GroundLandmarks)
                {
                    t.GetComponent<Vis>().Moving = false;
                }
            }
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
                if (t.GetComponent<Vis>().Selected)
                    DC.RemoveExplicitSelection(t);
                else
                    DC.AddExplicitSelection(t);
            }
        }
    }

    private void RunPressToSlide()
    {
        if (holdingFlag)
        {
            if (FTC.TouchedObjs.Count > 0)
            {
                foreach (Transform t in FTC.TouchedObjs)
                {
                    if (!previousParent.ContainsKey(t)) {
                        previousParent.Add(t, t.parent);
                        t.parent = mainFootToe;
                        t.GetComponent<Vis>().Moving = false;
                    }  
                }
            }
        }
        else
        {
            foreach (Transform t in FTC.TouchedObjs)
            {
                if (previousParent.ContainsKey(t)) {
                    t.parent = previousParent[t];
                    t.GetComponent<Vis>().Moving = false;
                }
                    
            }
        }

        //Vector3 moveV3 = mainFootToe.position - previousToePosition;
        //Vector3 moveV2 = new Vector3(moveV3.x, 0, moveV3.z);

        //if (FTC.TouchedObjs.Count > 0)
        //{
        //    foreach (Transform t in FTC.TouchedObjs) {
        //        t.position += moveV2;
        //        t.GetComponent<Vis>().Moving = true;
        //    }
                
        //}


        //previousToePosition = mainFootToe.position;
    }

    private void RunPressToDragAndDrop()
    {
        if (movingOBJ.Count > 0)
        {
            foreach (Transform t in movingOBJ) {
                Vector3 footForward = mainFoot.position + mainFoot.right * t.localScale.x;
                Vector3 footForwardV2 = new Vector3(footForward.x, 0.05f, footForward.z);
                t.position = footForwardV2;
                t.GetComponent<Vis>().Moving = true;
            }
                
        }
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
