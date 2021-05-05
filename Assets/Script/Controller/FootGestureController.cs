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
    Sliding,
    Still,
    SingleTap,
    DoubleTap,
    ToeSliding,
    ToeRotation,
    None
}

public class FootGestureController : MonoBehaviour
{
    [Header("Prefabs or OBJ in Scene")]
    public Transform directionIndicator;
    public Transform WaistDashboard;
    public Transform FootDashboard;
    public Transform GroundMarkerParent;
    public FootCollision FC;
    public FootToeCollision FTC;
    public DashboardController DC;
    public DisplaySurface ds;
    public ShoeRecieve SR;
    public Transform testCube;

    [Header("Main Foot")]
    public Transform mainFoot;
    public Transform mainFootToe;
    public Transform mainFootHeel;
    public Transform mainFootToeComponent;

    [Header("Variable")]
    public int windowFrames = 10;    // buff frames before detecting sliding
    public float scaleFactor = 0.5f;     // scale object multiplier
    public float panFactor = 2f;
    public int GlobalStaticPosCounter = 100;    // smooth people stop moving during sliding
    public float GlobalAngleToCancelGes = 20; // foot with related angle will cancel sliding
    public float GlobalRaiseFootToCancelSliding = 0.1f; // foot over this height will cancel sliding
    public float KickVelocityRecognizer = 3f; // foot velocity to recongnize kicking
    public float footHoldingHeightDiff = 0.1f; // foot height difference during window frames, max - min 
    public float BlindSelectionRange = 1f;
    public bool SlideToPan = false;
    public bool SingleTap = true;
    public bool SelectToMove = false;
    public float TapSpeed = 0.3f;
    public float ToeSlideMoveMultiplier = 1.5f;
    public float filterFrequency = 120f;

    // lists
    private List<Vector3> mainFootLocPositions;
    private List<Vector3> mainFootToePositions;
    private List<Vector3> mainFootHeelPositions;
    private List<Vector3> mainFootRotation;

    private List<float> mainFootHeight;
    private List<float> mainFootToeHeight;
    private List<float> mainFootHeelHeight;
    private List<float> eachFrameTime;

    private Gesture currentGesture;
    private float[] currentSlidingAngles;
    private bool passedWindow = false;
    private int staticPosCounter;

    private float stillTimer = 0; // check foot is still
    private Vector3 previousPosition;
    private Vector3 previousRotation;

    // toe sliding
    private Vector3 previousToePosition;

    // foot tap
    private List<Transform> footTapTouchedObj;
    private float singleTapTimer = 0; // check single tap
    private float doubleTapTimer = 0; // check single tap
    private int footTapCounter = -1;
    private bool newToeTap = true;
    private bool ToeOnVis = false;

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
        mainFootLocPositions = new List<Vector3>();
        mainFootToePositions = new List<Vector3>();
        mainFootHeelPositions = new List<Vector3>();
        mainFootRotation = new List<Vector3>();

        eachFrameTime = new List<float>();
        staticPosCounter = GlobalStaticPosCounter;
        interactingOBJ = new List<Transform>();
        footTapTouchedObj = new List<Transform>();
        mainFootHeight = new List<float>();
        mainFootToeHeight = new List<float>();
        mainFootHeelHeight = new List<float>();
        currentSlidingAngles = new float[2];

        currentSelectedVis = new List<Transform>();

        vector3Filter = new OneEuroFilter<Vector3>(filterFrequency);
    }

    // Update is called once per frame
    void Update()
    {
        // one euro filter
        filteredFootRotation = vector3Filter.Filter(mainFoot.eulerAngles);

        mainFootLocPositions.Add(mainFoot.localPosition);
        mainFootToePositions.Add(mainFootToe.position);
        mainFootHeelPositions.Add(mainFootHeel.position);
        mainFootRotation.Add(mainFoot.localEulerAngles);
        mainFootHeight.Add(mainFoot.position.y);
        mainFootToeHeight.Add(mainFootToe.position.y);
        mainFootHeelHeight.Add(mainFootHeel.position.y);
        eachFrameTime.Add(Time.deltaTime);

        if (mainFootLocPositions.Count > windowFrames)
            mainFootLocPositions.RemoveAt(0);
        if (mainFootToePositions.Count > windowFrames)
            mainFootToePositions.RemoveAt(0);
        if (mainFootHeelPositions.Count > windowFrames)
            mainFootHeelPositions.RemoveAt(0);
        if (mainFootRotation.Count > windowFrames)
            mainFootRotation.RemoveAt(0);
        if (mainFootHeight.Count > windowFrames)
            mainFootHeight.RemoveAt(0);
        if (mainFootToeHeight.Count > windowFrames)
            mainFootToeHeight.RemoveAt(0);
        if (mainFootHeelHeight.Count > windowFrames)
            mainFootHeelHeight.RemoveAt(0);
        if (eachFrameTime.Count > windowFrames)
            eachFrameTime.RemoveAt(0);

        //if (mainFootLocPositions.Count == windowFrames)
            //GestureProcessing();

        //if (interactingOBJ.Count > 0) {
        //    foreach (Transform t in interactingOBJ) {
        //        t.SetParent(mainFoot);
        //        t.transform.localPosition = new Vector3(0, 0, 0);
        //        t.transform.position = new Vector3(t.transform.position.x, 0.05f, t.transform.position.z);

        //        t.transform.eulerAngles = new Vector3(90, t.transform.eulerAngles.y, 0);
        //        t.transform.localEulerAngles = new Vector3(t.transform.localEulerAngles.x, t.transform.localEulerAngles.y, 90);
        //    }
        //}

        //Debug.Log(mainFoot.eulerAngles);
        //RunToeRotation();
        

        Vector3 rotationV3 = filteredFootRotation - previousRotation;
        Vector3 rotationV2 = new Vector3(0, rotationV3.y, 0);

        testCube.eulerAngles += rotationV3;
        //testCube.localEulerAngles = new Vector3(0, testCube.localEulerAngles.y, 0);
        previousRotation = filteredFootRotation;

        // pressure sensor
        if (SR.value.Length > 0 && int.Parse(SR.value) < 3700 && !physicalPressFlag) {
            physicalPressFlag = true;
            Debug.Log("Press");
            RunPressToSelect();
        }
        if (physicalPressFlag && SR.value.Length > 0 && int.Parse(SR.value) > 4000) {
            physicalPressFlag = false;
        }

        if (SR.value.Length > 0 && int.Parse(SR.value) < 3700)
            holdingFlag = true;
        if (SR.value.Length > 0 && int.Parse(SR.value) > 4000)
            holdingFlag = false;

        if (holdingFlag) {
            Debug.Log("Holding");
            //RunToeSliding();
        }

        previousPosition = mainFoot.position;
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
                previousToePosition = mainFootToe.position;
                previousRotation = mainFoot.localEulerAngles;
                // set indicator for sliding
                if (currentGesture == Gesture.SlideToLeft || currentGesture == Gesture.SlideToRight)
                    directionIndicator.gameObject.SetActive(true);
            }
            else
                ResetIndicator();
        }
        else
        {
            Debug.Log(currentGesture.ToString());


            //if (currentGesture == Gesture.Sliding)
            //{
            //    if (!SlideGestureCheck(currentGesture))
            //    {
            //        passedWindow = false;
            //        ResetIndicator();
            //    }
            //}

            //if (currentGesture == Gesture.SlideToLeft)
            //{
            //    SetGestureIndicator(directionIndicator.Find("ArrowLeft"));
            //    if (!SlideGestureCheck(currentGesture))
            //    {
            //        passedWindow = false;
            //        ResetIndicator();
            //    }
            //}

            //if (currentGesture == Gesture.SlideToRight)
            //{
            //    SetGestureIndicator(directionIndicator.Find("ArrowRight"));
            //    if (!SlideGestureCheck(currentGesture))
            //    {
            //        passedWindow = false;
            //        ResetIndicator();
            //    }
            //}

            //if (currentGesture == Gesture.ToeRaised)
            //{
            //    SetGestureIndicator(mainFootToeComponent);
            //    if (ToeTapGestureCheck())
            //    {
            //        RunToeTapToSelect();
            //        passedWindow = false;
            //        ResetIndicator();
            //    }
            //}

            //if (currentGesture == Gesture.Kick)
            //{
            //    RunFootKickToArchive();
            //    passedWindow = false;
            //}

            //if (currentGesture == Gesture.Still)
            //{
            //    RunStandStillToSplitCollapse();
            //    passedWindow = false;
            //}

            //if (currentGesture == Gesture.Shake) {
            //    RunFootShakeToArchive();
            //    passedWindow = false;
            //}

            if (SingleTap) {
                if (currentGesture == Gesture.SingleTap)
                {
                    RunTapToSelect();
                    passedWindow = false;
                }
                else {
                    passedWindow = false;
                }
            } else {
                if (currentGesture == Gesture.DoubleTap)
                {
                    RunTapToSelect();
                    passedWindow = false;
                }
                else {
                    passedWindow = false;
                }
            }

            if (currentGesture == Gesture.ToeSliding) {
                
                RunToeSliding();
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

        List<float> footRotation = new List<float>();

        for (int i = 0; i < windowFrames - 1; i++)
        {
            anglesToRight.Add(Vector3.Angle(mainFootLocPositions[i + 1] - mainFootLocPositions[i], mainFoot.up)); // angles between right direction of right foot
            anglesToFront.Add(Vector3.Angle(mainFootLocPositions[i + 1] - mainFootLocPositions[i], mainFoot.right)); // angles between front direction of right foot
            distance.Add(Vector3.Distance(mainFootLocPositions[i + 1], mainFootLocPositions[i]));  // distance foot moved

            footVelocity.Add(Vector3.Distance(mainFootLocPositions[i + 1], mainFootLocPositions[i]) / eachFrameTime[i]); // footVelocity calculation
            footToeVelocity.Add(Vector3.Distance(mainFootToePositions[i + 1], mainFootToePositions[i]) / eachFrameTime[i]); // footVelocity calculation

            footRotation.Add((mainFootRotation[i + 1] - mainFootRotation[i]).y);
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

        #region Sliding
        //// sliding to pan
        //if (SlideToPan)
        //{
        //    //Debug.Log("moving" + (distance.Max() > 0.001f));
        //    //Debug.Log("direction" + (anglesToFront.Max() - anglesToFront.Min()));
        //    //Debug.Log("ground" + (footHeight.Max() < GlobalRaiseFootToCancelSliding));
        //    if ((distance.Max() > 0.001f) && // moving
        //        (anglesToFront.Max() - anglesToFront.Min() < GlobalAngleToCancelGes) && // keep same direction
        //        (footHeight.Max() < GlobalRaiseFootToCancelSliding)){ // must remain on ground
        //        currentSlidingAngles[0] = anglesToFront.Min();
        //        currentSlidingAngles[1] = anglesToFront.Max();
        //        return Gesture.Sliding;
        //    }
        //}
        //else
        //{ // sliding to zoom
        //    foreach (float angle in anglesToRight.ToArray())
        //    {
        //        if (angle == 0 || distance[anglesToRight.IndexOf(angle)] < 0.001f) // if stationary
        //            anglesToRight.Remove(angle); // remove 0 for validation
        //        else
        //        {
        //            if (footHeight[anglesToRight.IndexOf(angle)] > GlobalRaiseFootToCancelSliding)
        //            {
        //                slideRight = false;
        //                slideLeft = false;
        //            }
        //            if (angle > GlobalAngleToCancelGes) // if not to right
        //                slideRight = false;
        //            if (angle < (180 - GlobalAngleToCancelGes)) // if not to left
        //                slideLeft = false;
        //        }
        //    }

        //    if (anglesToRight.Count > windowFrames / 2)
        //    { // if valid angles are more than half
        //        if (slideLeft && slideRight)
        //            Debug.Log("ERROR");

        //        if (slideLeft)
        //            return Gesture.SlideToLeft;

        //        if (slideRight)
        //            return Gesture.SlideToRight;
        //    }
        //}
        #endregion

        #region Toe Touch
        //// toe touch
        //foreach (float toeHeight in footToeHeight)
        //{
        //    if (!(footHeight[footToeHeight.IndexOf(toeHeight)] < 0.15f && mainFootHeel.position.y < 0.02f && toeHeight > 0.1f))
        //        toeRaised = false;
        //}

        //if (toeRaised)
        //    return Gesture.ToeRaised;
        #endregion

        #region Kicking
        //// kicking
        //if ((distance.Max() > 0.001f) && // moving
        //    (anglesToFront.Max() - anglesToFront.Min() < GlobalAngleToCancelGes) &&  // keep in a direction
        //    (footHeight.Max() - footHeight.Min() > 0.1f) && // height diff
        //    (footVelocity.Min() < 1) && // remove accidentally trigger
        //    (footVelocity.Max() > KickVelocityRecognizer)) // detect speed
        //    return Gesture.Kick;
        #endregion

        #region Shaking
        //// shaking
        //if (footHeight.Max() - footHeight.Min() < footHoldingHeightDiff && footHeight.Min() > 0.2f && footToeVelocity.Max() > 1)
        //    return Gesture.Shake;
        #endregion

        #region Foot Still
        //// foot still
        //if (stillTimer > 1)
        //{
        //    stillTimer = 0;
        //    return Gesture.Still;
        //}
        //else
        //{
        //    stillTimer += Time.deltaTime;
        //    //Debug.Log(Vector3.Distance(previousPosition, mainFoot.position));
        //    if (Vector3.Distance(previousPosition, mainFoot.position) > 0.01f)
        //        stillTimer = 0;
        //}
        #endregion

        #region Foot Tap
        // foot tap (single, double)
        if (SingleTap)
        {
            //Debug.Log(ToeOnVis + " " + singleTapTimer);
            if (newToeTap) {
                if (singleTapTimer > TapSpeed)
                {
                    singleTapTimer = 0;
                    footTapTouchedObj.Clear();
                    newToeTap = false;
                    return Gesture.None;
                }
                else
                {
                    if (FTC.TouchedObjs.Count > 0)
                    {
                        singleTapTimer += Time.deltaTime;
                        if (!ToeOnVis) {
                            
                            foreach (Transform t in FTC.TouchedObjs)
                            {
                                if (!footTapTouchedObj.Contains(t))
                                    footTapTouchedObj.Add(t);
                            }
                            ToeOnVis = true;
                        }
                        
                    }
                    else
                    {
                        ToeOnVis = false;
                        if (singleTapTimer != 0)
                        {
                            singleTapTimer = 0;

                            return Gesture.SingleTap;
                        }
                    }
                }
            }

            if(FTC.TouchedObjs.Count == 0)
                newToeTap = true;

        }
        else {
            //Debug.Log(footTapCounter);
            if (footTapCounter == -1) {
                if (FTC.TouchedObjs.Count == 0)
                {
                    footTapCounter++;
                }
            }
            else if (footTapCounter == 0)
            {
                if (doubleTapTimer > TapSpeed)
                {
                    footTapTouchedObj.Clear();
                    footTapCounter = -1;
                    doubleTapTimer = 0;
                }
                else
                {
                    doubleTapTimer += Time.deltaTime;
                    if (FTC.TouchedObjs.Count > 0)
                    {
                        footTapCounter++;
                        doubleTapTimer = 0;
                        foreach (Transform t in FTC.TouchedObjs)
                        {
                            if (!footTapTouchedObj.Contains(t))
                                footTapTouchedObj.Add(t);
                        }
                    }
                }
            }
            else if (footTapCounter == 1)
            {
                if (doubleTapTimer > TapSpeed)
                {
                    footTapTouchedObj.Clear();
                    footTapCounter = -1;
                    doubleTapTimer = 0;
                }
                else {
                    doubleTapTimer += Time.deltaTime;
                    if (FTC.TouchedObjs.Count == 0)
                    {
                        doubleTapTimer = 0;
                        footTapCounter++;
                    }
                }
            }
            else if (footTapCounter == 2) {
                if (doubleTapTimer > TapSpeed)
                {
                    footTapTouchedObj.Clear();
                    footTapCounter = -1;
                    doubleTapTimer = 0;
                }
                else
                {
                    doubleTapTimer += Time.deltaTime;
                    if (FTC.TouchedObjs.Count > 0)
                    {
                        List<Transform> finalList = new List<Transform>();
                        foreach (Transform t in footTapTouchedObj)
                        {
                            if (FTC.TouchedObjs.Contains(t))
                                finalList.Add(t);
                        }

                        if (finalList.Count > 0)
                        {
                            footTapTouchedObj = finalList;
                            footTapCounter = -1;
                            doubleTapTimer = 0;
                            return Gesture.DoubleTap;
                        }
                        else
                        {
                            footTapTouchedObj.Clear();
                            footTapCounter = -1;
                            doubleTapTimer = 0;
                        }
                    }
                }
            }
        }
        #endregion
        

        #region Foot Toe Sliding
        if ((distance.Max() > 0.003f) && // moving
                (footToeHeight.Max() < GlobalRaiseFootToCancelSliding)) // must remain on ground
        { 
            return Gesture.ToeSliding;
        }
        #endregion

        //Debug.Log(footRotation.Max());
        #region Foot Toe Rotation
        if ((distance.Max() > 0.003f) && // moving
                (footToeHeight.Max() < GlobalRaiseFootToCancelSliding)) // must remain on ground
        {
            //return Gesture.ToeRotation;
        }
        #endregion

        return Gesture.None;
    }
    #endregion

    #region Foot Press using pressure sensor
    private void RunPressToSelect() {
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
    #endregion

    #region Foot Rotation
    private void RunToeRotation() {
        Vector3 rotationV3 = mainFoot.localEulerAngles - previousRotation;
        Vector3 rotationV2 = new Vector3(0, rotationV3.y, 0);
        List<float> footToeHeight = new List<float>();
        List<float> distance = new List<float>();

        for (int i = 0; i < windowFrames; i++)
            footToeHeight.Add(mainFootToeHeight[i]); // foot toe height change

        for (int i = 0; i < windowFrames - 1; i++)
            distance.Add(Vector3.Distance(mainFootLocPositions[i + 1], mainFootLocPositions[i]));  // distance foot moved

        testCube.localEulerAngles += rotationV3;
        //testCube.localEulerAngles = new Vector3(0, testCube.localEulerAngles.y, 0);

        if (SelectToMove)
        {
            if (interactingOBJ.Count > 0)
            {
                foreach (Transform t in interactingOBJ)
                    t.localEulerAngles += rotationV2;
            }
        }
        else
        {
            if (FTC.TouchedObjs.Count > 0)
            {
                foreach (Transform t in FTC.TouchedObjs)
                    t.localEulerAngles += rotationV2;
            }
        }

        if ((distance.Max() < 0.001f) || // not moving
                (footToeHeight.Max() > GlobalRaiseFootToCancelSliding)) // must remain on ground
        {
            passedWindow = false;
            //Debug.Log("!!!");
        }

        previousRotation = mainFoot.localEulerAngles;
        
    }
    

        #endregion

    #region Foot Toe Sliding
    private void RunToeSliding() {
        Vector3 moveV3 = mainFootToe.position - previousToePosition;
        Vector3 moveV2 = new Vector3(moveV3.x, 0, moveV3.z);
        List<float> footToeHeight = new List<float>();
        List<float> distance = new List<float>();

        if (mainFootToeHeight.Count == windowFrames) {
            for (int i = 0; i < windowFrames; i++)
                footToeHeight.Add(mainFootToeHeight[i]); // foot toe height change
        }

        for (int i = 0; i < windowFrames - 1; i++)
            distance.Add(Vector3.Distance(mainFootLocPositions[i + 1], mainFootLocPositions[i]));  // distance foot moved

        if (SelectToMove)
        {
            if (interactingOBJ.Count > 0)
            {
                foreach (Transform t in interactingOBJ)
                    t.position += moveV2 * ToeSlideMoveMultiplier;
            }
        }
        else
        {
            if (FTC.TouchedObjs.Count > 0)
            {
                foreach (Transform t in FTC.TouchedObjs)
                    t.position += moveV2 * ToeSlideMoveMultiplier;
            }
        }

        if ((distance.Max() < 0.001f) || // not moving
                (footToeHeight.Max() > GlobalRaiseFootToCancelSliding)) // must remain on ground
        {
            passedWindow = false;
            //Debug.Log("!!!");
        }
            
        previousToePosition = mainFootToe.position;
    }
    #endregion

    #region New Foot Tap Gesture
    private void RunTapToSelect()
    {
        if (footTapTouchedObj.Count > 0) {
            foreach (Transform t in footTapTouchedObj.ToList()) {
                if (!DeregisterInteractingOBJ(t))
                    RegisterInteractingOBJ(t);

                footTapTouchedObj.Remove(t);
            }
        }
    }
    #endregion

    #region Foot Still Gesture
    private void RunStandStillToSplitCollapse() {
        if (FC.TouchedObjs.Count > 1) {
            foreach (Transform t in FC.TouchedObjs.ToList())
            {
                ArchiveVis(t);
                if(interactingOBJ.Contains(t))
                    interactingOBJ.Remove(t);

                if (FC.TouchedObjs.Contains(t))
                    FC.TouchedObjs.Remove(t);

                t.SetParent(FootDashboard);
            }
        }
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
            foreach (Transform t in interactingOBJ.ToList()) {
                ArchiveVisToBelt(t);
                interactingOBJ.Remove(t);

                if (FC.TouchedObjs.Contains(t))
                    FC.TouchedObjs.Remove(t);
            }
        }
        else if (blindSelection.Count > 0) {
            foreach (Transform t in blindSelection)
                ArchiveVisToBelt(t);
        }
    }
    #endregion

    #region Foot Shake Gesture
    //private void RunFootShakeToArchive() {
    //    List<Transform> blindSelection = CheckNearestVisOnGround();
    //    if (interactingOBJ.Count > 0)
    //    {
    //        foreach (Transform t in interactingOBJ.ToList())
    //        {
    //            ArchiveVisToBelt(t);
    //            interactingOBJ.Remove(t);
    //        }
    //    }
    //    else if (blindSelection.Count > 0)
    //    {
    //        foreach (Transform t in blindSelection)
    //            ArchiveVisToBelt(t);
    //    }
    //}
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
        if (FC.TouchedObjs.Count > 0)
        {
            foreach (Transform t in FC.TouchedObjs) {
                if (!DeregisterInteractingOBJ(t))
                    RegisterInteractingOBJ(t);
            }
        }
        else {
            if (interactingOBJ.Count > 0)
                DeregisterInteractingOBJ();
        }
    }

    #endregion

    #region Foot Sliding Gesture
    private bool SlideGestureCheck(Gesture gesture)
    {
        List<float> angles = new List<float>();
        List<float> anglesToFront = new List<float>();
        List<Vector3> direction = new List<Vector3>();
        List<float> distance = new List<float>();
        List<float> footHeight = new List<float>();

        // angles between right direction of right foot and different frames
        for (int i = 0; i < windowFrames - 1; i++)
        {
            angles.Add(Vector3.Angle(mainFootLocPositions[i + 1] - mainFootLocPositions[i], mainFoot.up));
            anglesToFront.Add(Vector3.Angle(mainFootLocPositions[i + 1] - mainFootLocPositions[i], mainFoot.right));
            distance.Add(Vector3.Distance(mainFootLocPositions[i + 1], mainFootLocPositions[i]));
            direction.Add(new Vector3((mainFootLocPositions[i + 1] - mainFootLocPositions[i]).normalized.x, 0, (mainFootLocPositions[i + 1] - mainFootLocPositions[i]).normalized.z));
            footHeight.Add(mainFoot.position.y);
        }

        if (footHeight.Max() > GlobalRaiseFootToCancelSliding)
            return false;
        else
        {
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
            foreach (float angle in anglesToFront.ToArray())
            {
                if (gesture == Gesture.Sliding)
                {
                    if (angle < currentSlidingAngles[1] && angle > currentSlidingAngles[0])
                        RunSlidingToPan(direction[anglesToFront.IndexOf(angle)] * distance[anglesToFront.IndexOf(angle)]);
                    else
                        return false;
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
                if (resultScale.x > 0.3f && resultScale.x < 2)
                    obj.localScale = resultScale;
            }
        }
        else if (blindSelection.Count > 0)
        {
            foreach (Transform obj in blindSelection)
            {
                Vector3 resultScale = obj.localScale + d * Vector3.one * scaleFactor;
                if (resultScale.x > 0.3f && resultScale.x < 2)
                    obj.localScale = resultScale;
            }
        }
    }

    private void RunSlidingToPan(Vector3 v)
    {
        if (interactingOBJ.Count > 0)
        {
            foreach (Transform obj in interactingOBJ)
                obj.localPosition += v * panFactor;
        }
    }
    #endregion

    #region Utilities
    private void RegisterInteractingOBJ(Transform t)
    {
        interactingOBJ.Add(t);
        if (t.GetComponent<Vis>() != null) {
            t.GetComponent<Vis>().Selected = true;
            currentSelectedVis.Add(t);
            if (currentSelectedVis.Count > 3) {
                DeregisterInteractingOBJ(currentSelectedVis.First());
            }

        }
            

        //Light highlighter = t.GetChild(2).GetComponent<Light>();
        //highlighter.color = Color.blue;
        //highlighter.intensity = 50;

        //DC.RemoveFromHeadDashboard(t);
        //t.GetComponent<Vis>().OnGround = false;
        //t.GetComponent<Vis>().OnWaistDashBoard = true;
    }

    private bool DeregisterInteractingOBJ(Transform t)
    {
        if (interactingOBJ.Contains(t)) {
            if (t.GetComponent<Vis>() != null) {
                if(currentSelectedVis.Contains(t))
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

    private void DeregisterInteractingOBJ() {
        foreach (Transform t in interactingOBJ.ToList()) {
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

    private void ArchiveVis(Transform vis)
    {
        DC.RemoveFromHeadDashboard(vis);
        vis.GetComponent<Vis>().OnGround = false;
        vis.GetComponent<Vis>().OnWaistDashBoard = false;
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
