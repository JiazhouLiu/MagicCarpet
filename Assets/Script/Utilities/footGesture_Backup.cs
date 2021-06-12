//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.Linq;

//public enum RepositionMethod
//{
//    Sliding,
//    DragAndDrop
//};

//public class FootGestureController_UserStudy : MonoBehaviour
//{
//    [Header("Prefabs or OBJ in Scene")]
//    public ExperimentManager EM;
//    public DashboardController_UserStudy DC;
//    public Transform GroundLandmarks;

//    [Header("Two Feet")]
//    //public Transform mainFoot;
//    //public Transform mainFootToe;
//    // left foot
//    public Transform leftFoot;
//    public Transform leftFootToe;
//    public FootToeCollision leftFootToeCollision;
//    public ShoeRecieve leftSR;
//    public Transform leftPressFeedback;
//    public Transform leftFeedbackCircle;
//    // right foot
//    public Transform rightFoot;
//    public Transform rightFootToe;
//    public FootToeCollision rightFootToeCollision;
//    public ShoeRecieve rightSR;
//    public Transform rightPressFeedback;
//    public Transform rightFeedbackCircle;

//    [Header("Variable")]
//    public RepositionMethod moveMethod;

//    [Header("PressureSensor")]
//    public int firmPressThreashold = 0;
//    public int pressToSelectThreshold = 0;
//    public int holdThreshold = 500;

//    private Vector3 previousDirection;
//    private Vector3 previousToePosition;
//    private Dictionary<string, Vector3> previousFromCenterToFoot;

//    // pressure sensor
//    private bool leftnormalPressFlag = false;
//    private bool rightnormalPressFlag = false;
//    private bool firmPressFlag = false;
//    private bool dragAndDropFlag = false;
//    private bool holdingFlag = false;

//    private List<Transform> interactingOBJ;
//    private List<Transform> movingOBJ;

//    // Start is called before the first frame update
//    void Start()
//    {
//        interactingOBJ = new List<Transform>();
//        movingOBJ = new List<Transform>();
//        previousFromCenterToFoot = new Dictionary<string, Vector3>();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        // pressure feedback right
//        if (rightSR.value.Length > 0 && float.Parse(rightSR.value) < 3000f)
//        {
//            rightPressFeedback.gameObject.SetActive(true);
//            rightPressFeedback.transform.eulerAngles = Vector3.zero;

//            rightFeedbackCircle.localScale = Vector3.one * ((4095f - float.Parse(rightSR.value)) / 4095f * 0.09f + 0.01f);
//            if (float.Parse(rightSR.value) <= pressToSelectThreshold)
//                rightFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(0, 0, 1, 0.4f));
//            else if (float.Parse(rightSR.value) < holdThreshold)
//                rightFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(1, 0.92f, 0.016f, 0.4f));
//            else
//                rightFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(1, 0, 0, 0.4f));
//        }
//        else
//            rightPressFeedback.gameObject.SetActive(false);

//        // pressure feedback left
//        if (leftSR.value.Length > 0 && float.Parse(leftSR.value) < 3000f)
//        {
//            leftPressFeedback.gameObject.SetActive(true);
//            leftPressFeedback.transform.eulerAngles = Vector3.zero;

//            leftFeedbackCircle.localScale = Vector3.one * ((4095f - float.Parse(leftSR.value)) / 4095f * 0.09f + 0.01f);
//            if (float.Parse(leftSR.value) <= pressToSelectThreshold)
//                leftFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(0, 0, 1, 0.4f));
//            else if (float.Parse(leftSR.value) < holdThreshold)
//                leftFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(1, 0.92f, 0.016f, 0.4f));
//            else
//                leftFeedbackCircle.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", new Color(1, 0, 0, 0.4f));
//        }
//        else
//            leftPressFeedback.gameObject.SetActive(false);

//        PressureSensorDetector();
//    }

//    #region Pressure Sensor Detection
//    private void PressureSensorDetector()
//    {
//        // pressure sensor
//        if (leftSR.value.Length > 0 && int.Parse(leftSR.value) <= pressToSelectThreshold && !leftnormalPressFlag)
//        {
//            leftnormalPressFlag = true;
//            Debug.Log("Press - left");
//            RunPressToSelect();
//        }
//        if (leftnormalPressFlag && leftSR.value.Length > 0 && int.Parse(leftSR.value) > pressToSelectThreshold)
//        {
//            leftnormalPressFlag = false;
//        }

//        if (leftSR.value.Length > 0 && int.Parse(leftSR.value) <= pressToSelectThreshold && !rightnormalPressFlag)
//        {
//            rightnormalPressFlag = true;
//            Debug.Log("Press");
//            RunPressToSelect();
//        }
//        if (rightnormalPressFlag && leftSR.value.Length > 0 && int.Parse(leftSR.value) > pressToSelectThreshold)
//        {
//            rightnormalPressFlag = false;
//        }


//        if (moveMethod == RepositionMethod.Sliding)
//        {
//            if (SR.value.Length > 0)
//            {
//                if (int.Parse(SR.value) < holdThreshold)
//                {
//                    if (!holdingFlag)
//                    {
//                        holdingFlag = true;
//                    }
//                }
//                else
//                {
//                    if (holdingFlag)
//                    {
//                        holdingFlag = false;
//                    }

//                }
//            }
//            RunPressToSlide();
//        }
//        else if (moveMethod == RepositionMethod.DragAndDrop)
//        {
//            if (SR.value.Length > 0 && int.Parse(SR.value) < firmPressThreashold && !firmPressFlag)
//            {
//                firmPressFlag = true;
//                if (dragAndDropFlag)
//                {
//                    dragAndDropFlag = false;

//                    movingOBJ.Clear();
//                }
//                else
//                {
//                    dragAndDropFlag = true;
//                    if (FTC.TouchedObjs.Count > 0)
//                    {
//                        foreach (Transform t in FTC.TouchedObjs)
//                        {
//                            if (!movingOBJ.Contains(t))
//                                movingOBJ.Add(t);
//                        }
//                    }
//                }

//                Debug.Log("Firm Press");
//            }

//            if (SR.value.Length > 0 && int.Parse(SR.value) > firmPressThreashold && firmPressFlag)
//                firmPressFlag = false;

//            if (dragAndDropFlag)
//                RunPressToDragAndDrop();
//            else
//            {
//                foreach (Transform t in GroundLandmarks)
//                {
//                    t.GetComponent<Vis>().Moving = false;
//                }
//            }
//        }

//    }
//    #endregion

//    #region Foot Press using pressure sensor
//    private void RunPressToSelect()
//    {
//        if (FTC.TouchedObjs.Count > 0)
//        {
//            foreach (Transform t in FTC.TouchedObjs)
//            {
//                if (t.GetComponent<Vis>().Selected)
//                    DC.RemoveExplicitSelection(t);
//                else
//                    DC.AddExplicitSelection(t);
//            }
//        }
//    }

//    private void RunPressToSlide()
//    {
//        if (holdingFlag)
//        {
//            if (FTC.TouchedObjs.Count > 0)
//            {
//                foreach (Transform t in FTC.TouchedObjs)
//                {
//                    if (!movingOBJ.Contains(t))
//                        movingOBJ.Add(t);

//                    t.parent = mainFoot;
//                    t.GetComponent<Vis>().Moving = true;
//                }
//            }
//        }
//        else
//        {
//            if (movingOBJ.Count > 0)
//            {
//                foreach (Transform t in movingOBJ)
//                {
//                    t.parent = EM.GroundDisplay;
//                    //if (t.position.y > 0.025f)
//                    //{
//                    //    t.GetComponent<Rigidbody>().isKinematic = false;
//                    //    t.GetComponent<Rigidbody>().AddForce(Vector3.down * 1, ForceMode.Force);
//                    //}
//                    //else if (t.position.y < 0.025f) {
//                    //    t.GetComponent<Rigidbody>().isKinematic = false;
//                    //    t.GetComponent<Rigidbody>().AddForce(Vector3.up * 1, ForceMode.Force);
//                    //}

//                    t.GetComponent<Vis>().Moving = false;
//                }

//                movingOBJ.Clear();
//            }
//        }

//        //Vector3 moveV3 = mainFootToe.position - previousToePosition;
//        //Vector3 moveV2 = new Vector3(moveV3.x, 0, moveV3.z);

//        //if (FTC.TouchedObjs.Count > 0)
//        //{
//        //    foreach (Transform t in FTC.TouchedObjs) {
//        //        t.position += moveV2;
//        //        t.GetComponent<Vis>().Moving = true;
//        //    }

//        //}


//        //previousToePosition = mainFootToe.position;
//    }

//    private void RunPressToDragAndDrop()
//    {
//        if (movingOBJ.Count > 0)
//        {
//            foreach (Transform t in movingOBJ)
//            {
//                Vector3 footForward = mainFoot.position + mainFoot.right * t.localScale.x;
//                Vector3 footForwardV2 = new Vector3(footForward.x, 0.05f, footForward.z);
//                t.position = footForwardV2;
//                t.GetComponent<Vis>().Moving = true;
//            }

//        }
//    }

//    private void RunPressToRotate()
//    {

//    }
//    #endregion

//    #region Utilities
//    private void RegisterInteractingOBJ(Transform t)
//    {
//        interactingOBJ.Add(t);
//        if (t.GetComponent<Vis>() != null)
//        {
//            t.GetComponent<Vis>().Selected = true;
//        }
//    }

//    private bool DeregisterInteractingOBJ(Transform t)
//    {
//        if (interactingOBJ.Contains(t))
//        {
//            interactingOBJ.Remove(t);
//            return true;
//        }
//        else
//            return false;
//    }

//    private void DeregisterInteractingOBJ()
//    {
//        foreach (Transform t in interactingOBJ.ToList())
//        {
//            interactingOBJ.Remove(t);
//        }
//    }
//    #endregion
//}
