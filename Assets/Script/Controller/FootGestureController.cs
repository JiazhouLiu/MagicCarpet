using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

enum Gesture
{
    SlideToRight,
    SlideToLeft,
    ToeRaised,
    Kick,
    Shake,
    None
}

public class FootGestureController : MonoBehaviour
{
    [Header("Prefabs or OBJ in Scene")]
    public Transform directionIndicator;
    public Transform WaistDashboard;
    public Transform GroundMarkerParent;
    public FootCollision FC;
    public DashboardController DC;

    [Header("Main Foot")]
    public Transform mainFoot;
    public Transform mainFootToe;
    public Transform mainFootHeel;
    public Transform mainFootToeComponent;

    [Header("Variable")]
    public int windowFrames = 5;    // buff frames before detecting sliding
    public float scaleFactor = 0.01f;     // scale object multiplier
    public int GlobalStaticPosCounter = 100;    // smooth people stop moving during sliding
    public float GlobalAngleToCancelGes = 20; // foot with related angle will cancel sliding
    public float GlobalRaiseFootToCancelSliding = 0.18f; // foot over this height will cancel sliding
    public float KickVelocityRecognizer = 1f; // foot velocity to recongnize kicking
    public float footHoldingHeightDiff = 0.1f; // foot height difference during window frames, max - min 
    public float BlindSelectionRange = 0.3f;

    // lists
    private List<Vector3> mainFootLocPositions;
    private List<Vector3> mainFootToePositions;

    private List<float> mainFootHeight;
    private List<float> mainFootToeHeight;
    private List<float> mainFootHeelHeight;
    private List<float> eachFrameTime;

    private Gesture currentGesture;
    private bool passedWindow = false;
    private int staticPosCounter;

    private List<Transform> interactingOBJ;

    // Start is called before the first frame update
    void Start()
    {
        mainFootLocPositions = new List<Vector3>();
        mainFootToePositions = new List<Vector3>();
        eachFrameTime = new List<float>();
        staticPosCounter = GlobalStaticPosCounter;
        interactingOBJ = new List<Transform>();
        mainFootHeight = new List<float>();
        mainFootToeHeight = new List<float>();
        mainFootHeelHeight = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
        // testing kicking away
        if (Input.GetKeyDown("z")) {
            foreach (Transform t in interactingOBJ) {
                t.SetParent(null);
                t.GetComponent<Rigidbody>().isKinematic = false;
                t.GetComponent<Rigidbody>().useGravity = true;
                t.GetComponent<Rigidbody>().AddForce(-t.forward * 100);
            }
        }

        mainFootLocPositions.Add(mainFoot.localPosition);
        mainFootToePositions.Add(mainFootToe.position);
        mainFootHeight.Add(mainFoot.position.y);
        mainFootToeHeight.Add(mainFootToe.position.y);
        mainFootHeelHeight.Add(mainFootHeel.position.y);
        eachFrameTime.Add(Time.deltaTime);

        if (mainFootLocPositions.Count > windowFrames)
            mainFootLocPositions.RemoveAt(0);
        if (mainFootToePositions.Count > windowFrames)
            mainFootToePositions.RemoveAt(0);
        if (mainFootHeight.Count > windowFrames)
            mainFootHeight.RemoveAt(0);
        if (mainFootToeHeight.Count > windowFrames)
            mainFootToeHeight.RemoveAt(0);
        if (mainFootHeelHeight.Count > windowFrames)
            mainFootHeelHeight.RemoveAt(0);
        if (eachFrameTime.Count > windowFrames)
            eachFrameTime.RemoveAt(0);

        if (mainFootLocPositions.Count == windowFrames)
            GestureProcessing();
    }

    #region Gesture Recognizer
    private void GestureProcessing()
    {
        if (!passedWindow)
        {
            currentGesture = GestureWindowDetector();

            if (currentGesture != Gesture.None)
            {
                passedWindow = true;
                if (currentGesture == Gesture.SlideToLeft || currentGesture == Gesture.SlideToRight)
                    directionIndicator.gameObject.SetActive(true);
            }
            else
                ResetIndicator();
        }
        else
        {
            Debug.Log(currentGesture.ToString());
            if (currentGesture == Gesture.SlideToLeft)
            {
                SetGestureIndicator(directionIndicator.Find("ArrowLeft"));
                if (!SlideGestureCheck(currentGesture))
                {
                    passedWindow = false;
                    ResetIndicator();
                }
            }

            if (currentGesture == Gesture.SlideToRight)
            {
                SetGestureIndicator(directionIndicator.Find("ArrowRight"));
                if (!SlideGestureCheck(currentGesture))
                {
                    passedWindow = false;
                    ResetIndicator();
                }
            }

            if (currentGesture == Gesture.ToeRaised)
            {
                SetGestureIndicator(mainFootToeComponent);
                if (ToeTapGestureCheck())
                {
                    RunToeTapToSelect();
                    passedWindow = false;
                    ResetIndicator();
                }
            }

            if (currentGesture == Gesture.Kick)
            {
                RunFootKickToArchive();
                passedWindow = false;
            }

            if (currentGesture == Gesture.Shake) {
                RunFootShakeToArchive();
                passedWindow = false;
            }
        }
    }

    private Gesture GestureWindowDetector()
    {

        // sliding
        List<float> anglesToRight = new List<float>();
        List<float> anglesToFront = new List<float>();
        List<float> distance = new List<float>();

        // toe touch
        List<float> footToeHeight = new List<float>();
        List<float> footHeelHeight = new List<float>();
        List<float> footHeight = new List<float>();

        List<float> footVelocity = new List<float>();
        List<float> footToeVelocity = new List<float>();

        for (int i = 0; i < windowFrames - 1; i++)
        {
            anglesToRight.Add(Vector3.Angle(mainFootLocPositions[i + 1] - mainFootLocPositions[i], mainFoot.up)); // angles between right direction of right foot
            anglesToFront.Add(Vector3.Angle(mainFootLocPositions[i + 1] - mainFootLocPositions[i], mainFoot.right)); // angles between front direction of right foot
            distance.Add(Vector3.Distance(mainFootLocPositions[i + 1], mainFootLocPositions[i]));  // distance foot moved

            footVelocity.Add(Vector3.Distance(mainFootLocPositions[i + 1], mainFootLocPositions[i]) / eachFrameTime[i]); // footVelocity calculation
            footToeVelocity.Add(Vector3.Distance(mainFootToePositions[i + 1], mainFootToePositions[i]) / eachFrameTime[i]); // footVelocity calculation
        }
        for (int i = 0; i < windowFrames; i++)
        {
            footHeight.Add(mainFootHeight[i]); // foot height change
            footToeHeight.Add(mainFootToeHeight[i]); // foot toe height change
            footHeelHeight.Add(mainFootHeelHeight[i]); // foot heel height change
        }

        bool slideLeft = true;
        bool slideRight = true;
        bool toeRaised = true;

        // sliding
        foreach (float angle in anglesToRight.ToArray())
        {
            if (angle == 0 || distance[anglesToRight.IndexOf(angle)] < 0.001f) // if stationary
                anglesToRight.Remove(angle); // remove 0 for validation
            else
            {
                if (footHeight[anglesToRight.IndexOf(angle)] > GlobalRaiseFootToCancelSliding)
                {
                    slideRight = false;
                    slideLeft = false;
                }
                if (angle > GlobalAngleToCancelGes) // if not to right
                    slideRight = false;
                if (angle < (180 - GlobalAngleToCancelGes)) // if not to left
                    slideLeft = false;
            }
        }

        if (anglesToRight.Count > windowFrames / 2)
        { // if valid angles are more than half
            if (slideLeft && slideRight)
                Debug.Log("ERROR");

            if (slideLeft)
                return Gesture.SlideToLeft;

            if (slideRight)
                return Gesture.SlideToRight;
        }

        // toe touch
        foreach (float toeHeight in footToeHeight)
        {
            if (!(footHeight[footToeHeight.IndexOf(toeHeight)] < 0.15f && mainFootHeel.position.y < 0.02f && toeHeight > 0.1f))
                toeRaised = false;
        }

        if (toeRaised)
            return Gesture.ToeRaised;

        // kicking
        if(footVelocity.Max() > KickVelocityRecognizer)
            return Gesture.Kick;

        // shaking
        if (footHeight.Max() - footHeight.Min() < footHoldingHeightDiff && footHeight.Min() > 0.2f && footToeVelocity.Max() > 1)
            return Gesture.Shake;

        return Gesture.None;
    }
    #endregion

    #region Foot Kick Gesture
    private void RunFootKickToMove()
    {

    }
    private void RunFootKickToArchive()
    {
        List<Transform> blindSelection = CheckNearestVisOnGround();
        if (interactingOBJ.Count > 0)
        {
            foreach (Transform t in interactingOBJ)
                ArchiveVisToBelt(t);
        }
        else if (blindSelection.Count > 0) {
            foreach (Transform t in blindSelection)
                ArchiveVisToBelt(t);
        }
    }
    #endregion

    #region Foot Shake Gesture
    private void RunFootShakeToArchive() {
        List<Transform> blindSelection = CheckNearestVisOnGround();
        if (interactingOBJ.Count > 0)
        {
            foreach (Transform t in interactingOBJ)
                ArchiveVisToBelt(t);
        }
        else if (blindSelection.Count > 0)
        {
            foreach (Transform t in blindSelection)
                ArchiveVisToBelt(t);
        }
    }
    #endregion

    #region Foot Toe Tap Gesture
    private bool ToeTapGestureCheck()
    {
        // toe touch
        List<float> footToeHeight = new List<float>();
        List<float> footHeelHeight = new List<float>();
        List<float> footHeight = new List<float>();

        // angles between right direction of right foot and different frames
        for (int i = 0; i < windowFrames - 1; i++)
        {
            footHeight.Add(mainFoot.position.y);
            footToeHeight.Add(mainFootToe.position.y);
            footHeelHeight.Add(mainFootHeel.position.y);
        }

        // toe touch
        foreach (float toeHeight in footToeHeight)
        {
            if (footHeight[footToeHeight.IndexOf(toeHeight)] < 0.15f && mainFootHeel.position.y < 0.03f && toeHeight <= 0.1f)
                return true;
        }
        return false;
    }

    private void RunToeTapToSelect()
    {
        if (FC.TouchedObj != null) {
            if (!DeregisterInteractingOBJ(FC.TouchedObj))
                RegisterInteractingOBJ(FC.TouchedObj); 
        }
    }

    #endregion

    #region Foot Sliding Gesture
    private bool SlideGestureCheck(Gesture gesture)
    {
        List<float> angles = new List<float>();
        List<float> distance = new List<float>();
        List<float> footHeight = new List<float>();

        // angles between right direction of right foot and different frames
        for (int i = 0; i < windowFrames - 1; i++)
        {
            angles.Add(Vector3.Angle(mainFootLocPositions[i + 1] - mainFootLocPositions[i], mainFoot.up));
            distance.Add(Vector3.Distance(mainFootLocPositions[i + 1], mainFootLocPositions[i]));
            footHeight.Add(mainFoot.position.y);
        }

        if (footHeight.Max() > GlobalRaiseFootToCancelSliding)
            return false;
        else {
            foreach (float angle in angles.ToArray())
            {
                if (angle == 0 && distance[angles.IndexOf(angle)] == 0) // if stationary
                    angles.Remove(angle); // remove 0 for validation
                else
                {
                    if (gesture == Gesture.SlideToRight)
                    {
                        if (angle > GlobalAngleToCancelGes) // if not to right
                            return false;
                        else
                            RunSlidingToScale(distance[angles.IndexOf(angle)]);
                    }

                    if (gesture == Gesture.SlideToLeft)
                    {
                        if (angle < (180 - GlobalAngleToCancelGes)) // if not to left
                            return false;
                        else
                            RunSlidingToScale(-distance[angles.IndexOf(angle)]);
                    }
                }
            }
        }

        if (angles.Count == 0)
        {
            if (staticPosCounter-- == 0)
            {
                staticPosCounter = GlobalStaticPosCounter;
                return false;
            }
        }

        return true;
    }

    private void RunSlidingToScale(float d)
    {
        List<Transform> blindSelection = CheckNearestVisOnGround();
        if (interactingOBJ.Count > 0)
        {
            foreach (Transform obj in interactingOBJ)
            {
                Vector3 resultScale = obj.localScale + d * Vector3.one * scaleFactor;
                if (resultScale.x > 0 && resultScale.x < 2)
                    obj.localScale = resultScale;
            }
        }
        else if (blindSelection.Count > 0)
        {
            foreach (Transform obj in blindSelection)
            {
                Vector3 resultScale = obj.localScale + d * Vector3.one * scaleFactor;
                if (resultScale.x > 0 && resultScale.x < 2)
                    obj.localScale = resultScale;
            }
        }
    }
    #endregion

    #region Utilities
    private void RegisterInteractingOBJ(Transform t)
    {
        interactingOBJ.Add(t);
        if (t.GetComponent<Vis>() != null)
            t.GetComponent<Vis>().Selected = true;
    }

    private bool DeregisterInteractingOBJ(Transform t)
    {
        if (interactingOBJ.Contains(t)) {
            if (t.GetComponent<Vis>() != null)
                t.GetComponent<Vis>().Selected = false;
            interactingOBJ.Remove(t);
            return true;
        }
        else
            return false;
    }

    private void ResetIndicator()
    {
        foreach (Transform t in directionIndicator)
            t.GetComponent<MeshRenderer>().material.SetColor("_EmissiveColor", Color.black);
        directionIndicator.gameObject.SetActive(false);
        mainFootToeComponent.GetComponent<MeshRenderer>().material.SetColor("_EmissiveColor", Color.black);
    }

    private void SetGestureIndicator(Transform t)
    { 
        t.GetComponent<MeshRenderer>().material.SetColor("_EmissiveColor", Color.green);
    }

    private void ArchiveVisToBelt(Transform vis) {
        DC.RemoveFromHeadDashboard(vis);
        vis.transform.SetParent(WaistDashboard);
        vis.GetComponent<Vis>().OnGround = false;
        vis.GetComponent<Vis>().OnWaistDashBoard = true;
    }

    private List<Transform> CheckNearestVisOnGround() {
        List<Transform> rangeSelected = new List<Transform>();

        foreach (Transform t in GroundMarkerParent) {
            Vector3 CameraPosition2D = new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z);
            Vector3 VisPosition2D = new Vector3(t.position.x, 0, t.position.z);
            if (Vector3.Distance(CameraPosition2D, VisPosition2D) < BlindSelectionRange)
                rangeSelected.Add(t);
        }
        return rangeSelected;
    }
    #endregion
}
