using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;
using VRTK;
using UnityEngine.Rendering.HighDefinition;

public class DashboardController_UserStudy : MonoBehaviour
{
    public ExperimentManager EM;

    [Header("Variables")]
    public float Show3VisDelta = 0.5f;
    public float speed = 3;
    public float filterFrequency = 120f;
    public float betweenVisDelta = 0.05f;    
    public float ImplicitDistance = 0.2f;

    [Header("Experiment Setup")]
    public int VisNumber = 6;
    public float LandmarkSizeOnGround = 0.5f;
    public float LandmarkSizeOnBody = 0.2f;
    public float LandmarkSizeOnShelves = 1f;

    [Header("Log Purpose")]
    [HideInInspector] public string GrabbedVis = "";
    [HideInInspector] public string PinnedVis = "";
    [HideInInspector] public string LandmarkNames = "";
    [HideInInspector] public string LandmarkPositions = "";
    [HideInInspector] public string LandmarkRotations = "";
    [HideInInspector] public string LandmarkState = "";

    // reference frames transform
    private Transform Wall;
    private Transform TableTop;
    private Transform FloorSurface;

    // dashboards
    private Transform HeadLevelDisplay;
    private Transform WaistLevelDisplay;
    private Transform GroundDisplay;
    private Transform WallDisplay;
    private Transform TableTopDisplay;

    // body tracking
    private float armLength;
    private Transform HumanWaist;
    private Transform LeftHand;
    private Transform RightHand;
    private Transform CameraTransform;
    private Transform Shoulder;
    private Vector3 previousHumanWaistPosition;
    private Vector3 previousHumanWaistRotation;
    private Vector3 previousLeftHandPosition;
    private Vector3 previousRightHandPosition;

    // one euro filter
    private Vector3 filteredWaistPosition;
    private Vector3 filteredWaistRotation;
    private Vector3 filteredLeftHandPosition;
    private Vector3 filteredRightHandPosition;
    private OneEuroFilter<Vector3> vector3Filter;

    // experiment use
    private Transform OriginalLandmarkParent;
    private Transform OriginalDetailedViewParent;

    public ReferenceFrames Landmark;
    public ReferenceFrames DetailedView;

    private bool DemoFlag = true;
    private List<Transform> landmarkParentList;
    private Dictionary<string, Transform> detailedViewParentList;
    private List<Transform> originalLandmarks;

    private List<Transform> selectedVis;
    private List<Transform> explicitlySelectedVis;

    private Dictionary<string, Transform> currentLandmarks;
    private Dictionary<string, Transform> currentDetailedViews;

    private float highlighterIntensity = 10;
    private bool InitialiseTable = false;
    private bool InitialiseShoulder = false;

    private void Awake()
    {
        Wall = EM.Wall;
        TableTop = EM.TableTop;
        FloorSurface = EM.FloorSurface;

        HeadLevelDisplay = EM.HeadLevelDisplay;
        WaistLevelDisplay = EM.WaistLevelDisplay;
        GroundDisplay = EM.GroundDisplay;
        WallDisplay = EM.WallDisplay;
        TableTopDisplay = EM.TableTopDisplay;

        armLength = EM.armLength;
        HumanWaist = EM.waist;
        LeftHand = EM.leftHand;
        RightHand = EM.rightHand;
        Shoulder = EM.SphereCenter;

        landmarkParentList = new List<Transform>();
        detailedViewParentList = new Dictionary<string, Transform>();
        originalLandmarks = new List<Transform>();

        selectedVis = new List<Transform>();
        explicitlySelectedVis = new List<Transform>();

        currentLandmarks = new Dictionary<string, Transform>();
        currentDetailedViews = new Dictionary<string, Transform>();
        // one euro filter
        vector3Filter = new OneEuroFilter<Vector3>(filterFrequency);
    }

    private void Update()
    {
        if (EM.Reset)
        {
            EM.Reset = false;
            InitialiseEnvironment();
        }

        if (Camera.main != null && CameraTransform == null)
            CameraTransform = Camera.main.transform;

        if (Landmark == ReferenceFrames.Shelves && !InitialiseTable && HumanWaist.position.y != 0)
        {
            InitialiseTable = true;
            TableTop.position = new Vector3(TableTop.position.x, HumanWaist.position.y, TableTop.position.z);
            TableTopDisplay.position = TableTop.position;

            RePositionLandmarks(ReferenceFrames.Shelves);
        }

        if (Landmark == ReferenceFrames.Body && !InitialiseShoulder && HumanWaist.position != Vector3.zero && Camera.main != null)
        {
            float waistToEye = Camera.main.transform.position.y - HumanWaist.position.y;

            InitialiseShoulder = true;
            Shoulder.localPosition = new Vector3(0, waistToEye * 3f / 4f, 0);
            WaistLevelDisplay.position = Shoulder.position;

            RePositionLandmarks(ReferenceFrames.Body);
        }

        // OneEuroFilter
        filteredWaistPosition = vector3Filter.Filter(HumanWaist.position);
        filteredWaistRotation = vector3Filter.Filter(HumanWaist.eulerAngles);
        filteredLeftHandPosition = LeftHand.position;
        filteredRightHandPosition = RightHand.position;

        if (Landmark == ReferenceFrames.Floor) // vis on floor/shelves as landmarks
        {
            // update vis to show
            if (CheckHumanWaistMoving() || DemoFlag)
            {
                DemoFlag = false;

                UpdateVisFromPositionChange(HumanWaist.transform);
            }

            if (CheckHumanWaistRotating())
            {
                selectedVis = RearrangeDisplayBasedOnLandmarkPosition(selectedVis);
                currentDetailedViews = RearrangeVisOnDashBoard(selectedVis, currentDetailedViews);
                if (DetailedView == ReferenceFrames.Body)
                {
                    HeadLevelDisplay.DetachChildren();
                    for (int i = 0; i < currentDetailedViews.Count; i++)
                    {
                        currentDetailedViews.Values.ToList()[currentDetailedViews.Count - i - 1].SetParent(HeadLevelDisplay);
                    }
                }
                else if (DetailedView == ReferenceFrames.Shelves)
                {
                    WallDisplay.DetachChildren();
                    for (int i = 0; i < currentDetailedViews.Count; i++)
                    {
                        currentDetailedViews.Values.ToList()[currentDetailedViews.Count - i - 1].SetParent(WallDisplay);
                    }
                }
            }
        }
        else if (Landmark == ReferenceFrames.Body || Landmark == ReferenceFrames.Shelves) // vis on body as landmarks
        {
            WaistLevelDisplay.position = Shoulder.position;
            WaistLevelDisplay.rotation = Shoulder.rotation;

            // update vis to show
            if (CheckHumanHandMoving() || DemoFlag)
            {
                DemoFlag = false;

                UpdateVisFromPositionChange(LeftHand.transform, RightHand.transform);
                if (DetailedView == ReferenceFrames.Shelves)
                {
                    WallDisplay.DetachChildren();
                    for (int i = 0; i < currentDetailedViews.Count; i++)
                    {
                        currentDetailedViews.Values.ToList()[currentDetailedViews.Count - i - 1].SetParent(WallDisplay);
                    }
                }
            }
        }

        UpdateHighlighter();

        foreach (string dvNames in currentDetailedViews.Keys)
            currentDetailedViews[dvNames].GetComponent<Vis>().CopyEntity(currentLandmarks[dvNames].GetComponent<Vis>());
    }

    #region Experiment Use
    #region Generate Function
    private void InitialiseEnvironment()
    {
        InitialiseTable = false;
        InitialiseShoulder = false;
        DemoFlag = true;

        previousHumanWaistPosition = Vector3.zero;
        previousHumanWaistRotation = Vector3.zero;
        previousLeftHandPosition = Vector3.zero;
        previousRightHandPosition = Vector3.zero;

        filteredWaistPosition = Vector3.zero;
        filteredWaistRotation = Vector3.zero;
        filteredLeftHandPosition = Vector3.zero;
        filteredRightHandPosition = Vector3.zero;

        // landmarks
        landmarkParentList.Clear();
        detailedViewParentList.Clear();
        originalLandmarks.Clear();

        selectedVis.Clear();
        explicitlySelectedVis.Clear();

        foreach (Transform t in currentLandmarks.Values.ToList())
            Destroy(t.gameObject);
        currentLandmarks.Clear();

        foreach (Transform t in currentDetailedViews.Values.ToList())
            Destroy(t.gameObject);
        currentDetailedViews.Clear();

        OriginalLandmarkParent = EM.GetCurrentLandmarkParent();
        OriginalDetailedViewParent = EM.GetCurrentDetailedViewParent();

        Landmark = EM.GetCurrentLandmarkFOR();
        DetailedView = EM.GetCurrentDetailedViewFOR();

        Shoulder.gameObject.SetActive(false);
        TableTop.gameObject.SetActive(false);
        FloorSurface.gameObject.SetActive(false);
        Wall.gameObject.SetActive(false);

        WaistLevelDisplay.gameObject.SetActive(false);
        GroundDisplay.gameObject.SetActive(false);
        TableTopDisplay.gameObject.SetActive(false);
        HeadLevelDisplay.gameObject.SetActive(false);
        WallDisplay.gameObject.SetActive(false);

        // enable landmarks and detailed views based on configuration 
        switch (Landmark)
        {
            case ReferenceFrames.Body:
                Shoulder.gameObject.SetActive(true);
                Shoulder.localScale = Vector3.one * armLength * 2;
                WaistLevelDisplay.gameObject.SetActive(true);
                break;
            case ReferenceFrames.Floor:
                GroundDisplay.gameObject.SetActive(true);
                FloorSurface.gameObject.SetActive(true);
                break;
            case ReferenceFrames.Shelves:
                TableTop.gameObject.SetActive(true);
                TableTopDisplay.gameObject.SetActive(true);
                TableTopDisplay.position = TableTop.position;
                TableTopDisplay.rotation = TableTop.rotation;
                break;
        }

        switch (DetailedView)
        {
            case ReferenceFrames.Body:
                HeadLevelDisplay.gameObject.SetActive(true);
                break;
            case ReferenceFrames.Shelves:
                Wall.gameObject.SetActive(true);
                WallDisplay.gameObject.SetActive(true);
                WallDisplay.position = Wall.position;
                break;
        }

        foreach (Transform t in OriginalLandmarkParent)
        {
            landmarkParentList.Add(t);
        }

        foreach (Transform t in OriginalDetailedViewParent)
        {
            detailedViewParentList.Add(t.name, t);
        }

        if (Landmark == ReferenceFrames.Body) // vis on body as landmarks
            WaistLevelDisplay.position = Shoulder.position;

        // initiate landmarks
        originalLandmarks = GetRandomItemsFromList(landmarkParentList, VisNumber);
        PositionLandmarks(Landmark, originalLandmarks);
    }

    private Transform GenerateDetailedView(Transform t)
    {
        if (detailedViewParentList.ContainsKey(t.name))
        {
            GameObject visOnDetailedView = Instantiate(detailedViewParentList[t.name].gameObject);
            visOnDetailedView.name = t.name;
            visOnDetailedView.GetComponent<Vis>().CopyEntity(t.GetComponent<Vis>());

            if (DetailedView == ReferenceFrames.Body)
            {
                visOnDetailedView.transform.SetParent(HeadLevelDisplay);
                visOnDetailedView.GetComponent<Vis>().OnHead = true;
                if (Landmark == ReferenceFrames.Floor)
                    visOnDetailedView.GetComponent<Vis>().OnGround = false;
                if (Landmark == ReferenceFrames.Shelves)
                    visOnDetailedView.GetComponent<Vis>().OnShelves = false;
            }
            else if (DetailedView == ReferenceFrames.Shelves)
            {
                visOnDetailedView.transform.SetParent(WallDisplay);
                visOnDetailedView.GetComponent<Vis>().OnShelves = true;
                visOnDetailedView.GetComponent<Vis>().GroundPosition = t.position;
                if (Landmark == ReferenceFrames.Floor)
                    visOnDetailedView.GetComponent<Vis>().OnGround = false;
                if (Landmark == ReferenceFrames.Body)
                    visOnDetailedView.GetComponent<Vis>().OnWaist = false;
            }

            // setup transform
            visOnDetailedView.transform.position = t.position;
            visOnDetailedView.transform.rotation = t.rotation;
            visOnDetailedView.transform.localScale = Vector3.one * 0.1f;
            visOnDetailedView.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", Color.white);
            visOnDetailedView.GetComponent<VisInteractionController_UserStudy>().enabled = false;

            // setup components
            visOnDetailedView.GetComponent<Rigidbody>().isKinematic = true;
            visOnDetailedView.GetComponent<BoxCollider>().enabled = false;

            currentDetailedViews.Add(visOnDetailedView.name, visOnDetailedView.transform);

            return visOnDetailedView.transform;
        }
        else
        {
            Debug.LogError("No mapped detailed view found!!");
            return null;
        }

    }

    private void PositionLandmarks(ReferenceFrames landmark, List<Transform> originalLandmarks)
    {
        if (landmark == ReferenceFrames.Floor) // landmarks on floor
        {
            foreach (Transform t in originalLandmarks)
            {
                // Instantiate game object
                GameObject newLandmark = Instantiate(t.gameObject, Vector3.zero, Quaternion.identity, GroundDisplay);
                newLandmark.name = t.name;
                newLandmark.GetComponent<VisInteractionController_UserStudy>().enabled = true;
                newLandmark.GetComponent<BoxCollider>().isTrigger = true;

                // setup transform
                newLandmark.transform.localScale = new Vector3(LandmarkSizeOnGround, LandmarkSizeOnGround, LandmarkSizeOnGround);
                newLandmark.transform.localEulerAngles = new Vector3(90, 0, 0);

                // setup vis model
                Vis newVis = new Vis(newLandmark.name)
                {
                    OnGround = true
                };
                newLandmark.GetComponent<Vis>().CopyEntity(newVis);

                currentLandmarks.Add(newLandmark.name, newLandmark.transform);
            }
            GroundDisplay.GetComponent<ReferenceFrameController_UserStudy>().InitialiseLandmarkPositions(currentLandmarks.Values.ToList(), landmark);
        }
        else if (landmark == ReferenceFrames.Body) // landmarks on waist level display
        {
            foreach (Transform t in originalLandmarks)
            {
                // Instantiate game object
                GameObject newLandmark = Instantiate(t.gameObject, Shoulder.transform.position, Quaternion.identity, WaistLevelDisplay);
                newLandmark.name = t.name;
                newLandmark.GetComponent<VisInteractionController_UserStudy>().enabled = true;
                newLandmark.GetComponent<BoxCollider>().isTrigger = true;

                // setup transform
                newLandmark.transform.localScale = new Vector3(LandmarkSizeOnBody, LandmarkSizeOnBody, LandmarkSizeOnBody);

                // setup vis model
                Vis newVis = new Vis(newLandmark.name)
                {
                    OnWaist = true
                };
                newLandmark.GetComponent<Vis>().CopyEntity(newVis);

                currentLandmarks.Add(newLandmark.name, newLandmark.transform);
            }
            WaistLevelDisplay.GetComponent<ReferenceFrameController_UserStudy>().InitialiseLandmarkPositions(currentLandmarks.Values.ToList(), landmark);
        }
        else if (landmark == ReferenceFrames.Shelves) // landmarks on shelves
        {
            foreach (Transform t in originalLandmarks)
            {
                // Instantiate game object
                GameObject newLandmark = Instantiate(t.gameObject, Vector3.zero, Quaternion.identity, TableTopDisplay);
                newLandmark.name = t.name;
                newLandmark.GetComponent<VisInteractionController_UserStudy>().enabled = true;
                newLandmark.GetComponent<BoxCollider>().isTrigger = true;

                // setup transform
                newLandmark.transform.localScale = new Vector3(LandmarkSizeOnShelves, LandmarkSizeOnShelves, LandmarkSizeOnShelves);
                newLandmark.transform.localEulerAngles = new Vector3(90, 0, 0);

                // setup vis model
                Vis newVis = new Vis(newLandmark.name)
                {
                    OnShelves = true
                };
                newLandmark.GetComponent<Vis>().CopyEntity(newVis);

                currentLandmarks.Add(newLandmark.name, newLandmark.transform);
            }
            TableTopDisplay.GetComponent<ReferenceFrameController_UserStudy>().InitialiseLandmarkPositions(currentLandmarks.Values.ToList(), landmark);
        }

    }
    #endregion

    #region Update Function
    private void RePositionLandmarks(ReferenceFrames rf)
    {
        if (rf == ReferenceFrames.Body)
            WaistLevelDisplay.GetComponent<ReferenceFrameController_UserStudy>().InitialiseLandmarkPositions(currentLandmarks.Values.ToList(), ReferenceFrames.Body);
        if (rf == ReferenceFrames.Shelves)
            TableTopDisplay.GetComponent<ReferenceFrameController_UserStudy>().InitialiseLandmarkPositions(currentLandmarks.Values.ToList(), ReferenceFrames.Shelves);
    }
    private void UpdateVisFromPositionChange(Transform implicitObject)
    {
        selectedVis = GetVisFromInteraction(implicitObject);

        if (selectedVis.Count > 0)
        {
            //if (Landmark == ReferenceFrames.Shelves || Landmark == ReferenceFrames.Body)
            selectedVis = RearrangeDisplayBasedOnLandmarkPosition(selectedVis);
            currentDetailedViews = RearrangeVisOnDashBoard(selectedVis, currentDetailedViews);

            foreach (Transform t in selectedVis)
            {
                currentDetailedViews[t.name].GetComponent<Vis>().CopyEntity(t.GetComponent<Vis>());
            }
        }
    }

    private void UpdateVisFromPositionChange(Transform leftHand, Transform rightHand)
    {
        selectedVis = GetVisFromInteraction(leftHand, rightHand);

        if (selectedVis.Count > 0)
        {
            if (Landmark == ReferenceFrames.Shelves || Landmark == ReferenceFrames.Body)
                    selectedVis = RearrangeDisplayBasedOnLandmarkPosition(selectedVis);
            currentDetailedViews = RearrangeVisOnDashBoard(selectedVis, currentDetailedViews);

            foreach (Transform t in selectedVis)
            {
                currentDetailedViews[t.name].GetComponent<Vis>().CopyEntity(t.GetComponent<Vis>());
            }
        }

    }

    private void UpdateHighlighter()
    {
        // log related
        string grabbedVis = "";
        string pinnedVis = "";
        int grabNumber = 0;
        int pinNumber = 0;
        string names = "";
        string positions = "";
        string rotations = "";
        string states = "";

        // highlight selected Vis
        foreach (Transform landmark in currentLandmarks.Values)
        {
            names += landmark.name + ",";
            positions += landmark.position.x + "," + landmark.position.y + "," + landmark.position.z + ",";
            rotations += landmark.eulerAngles.x + "," + landmark.eulerAngles.y + "," + landmark.eulerAngles.z + ",";

            Light highlighter = landmark.GetChild(2).GetComponent<Light>();
            if (landmark.GetComponent<Vis>().Moving)
            {
                highlighter.color = Color.yellow;
                landmark.GetChild(2).GetComponent<HDAdditionalLightData>().SetIntensity(highlighterIntensity);
                grabbedVis += landmark.name + ",";
                grabNumber++;
                states += "moving;";
            }
            else if (explicitlySelectedVis.Contains(landmark))
            {
                highlighter.color = Color.blue;
                landmark.GetChild(2).GetComponent<HDAdditionalLightData>().SetIntensity(highlighterIntensity);
                pinnedVis += landmark.name + ",";
                pinNumber++;
                states += "grabbing;";
            }
            else if (selectedVis.Contains(landmark))
            {
                highlighter.color = Color.green;
                landmark.GetComponent<Vis>().Highlighted = true;
                currentDetailedViews[landmark.name].GetComponent<Vis>().Highlighted = true;
                landmark.GetChild(2).GetComponent<HDAdditionalLightData>().SetIntensity(highlighterIntensity);
                states += "highlighting;";
            }
            else
            {
                highlighter.color = Color.white;
                landmark.GetComponent<Vis>().Highlighted = false;
                landmark.GetChild(2).GetComponent<HDAdditionalLightData>().SetIntensity(0);
            }
            states += ",";
        }

        if (grabNumber > 0) { 
            if(grabNumber == 2)
                GrabbedVis = grabbedVis.Remove(grabbedVis.Length - 1);
            else
                GrabbedVis = grabbedVis;
        }else
            GrabbedVis = ",";

        if (pinNumber > 0) { 
            if(pinNumber == 3)
                PinnedVis = pinnedVis.Remove(pinnedVis.Length - 1);
            else if(pinNumber == 2)
                PinnedVis = pinnedVis;
            else
                PinnedVis = pinnedVis + ",";
        }
        else
            PinnedVis = ",,";
        
        if (names.Length > 0)
            LandmarkNames = names.Remove(names.Length - 1);
        if (positions.Length > 0)
            LandmarkPositions = positions.Remove(positions.Length - 1);
        if (rotations.Length > 0)
            LandmarkRotations = rotations.Remove(rotations.Length - 1);
        if (states.Length > 0)
            LandmarkState = states.Remove(states.Length - 1);

        foreach (Transform detailedView in currentDetailedViews.Values.ToList())
        {
            detailedView.GetChild(2).GetComponent<HDAdditionalLightData>().SetIntensity(0);

            if (detailedView.GetComponent<Vis>().Selected)
            {
                // configure line between selected views
                ConnectLandmarkWithDV(currentLandmarks[detailedView.name], detailedView, "selected");
                detailedView.GetComponent<Vis>().VisBorder.gameObject.SetActive(true);
                foreach (Transform t in detailedView.GetComponent<Vis>().VisBorder)
                {
                    t.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", Color.blue);
                }
            }
            else if (detailedView.GetComponent<Vis>().Highlighted)
            {
                // configure line between selected views
                ConnectLandmarkWithDV(currentLandmarks[detailedView.name], detailedView, "highlighted");
                detailedView.GetComponent<Vis>().VisBorder.gameObject.SetActive(true);
                foreach (Transform t in detailedView.GetComponent<Vis>().VisBorder)
                {
                    t.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", Color.green);
                }
            }
            else
            {
                detailedView.GetComponent<Vis>().VisBorder.gameObject.SetActive(false);
            }

        }

        foreach (Transform landmark in currentLandmarks.Values.ToList())
        {
            if (!landmark.GetComponent<Vis>().Selected && !landmark.GetComponent<Vis>().Highlighted)
            {
                landmark.Find("LineToDV").GetComponent<LineRenderer>().SetPosition(0, Vector3.zero);
                landmark.Find("LineToDV").GetComponent<LineRenderer>().SetPosition(1, Vector3.zero);
                landmark.Find("LineToDV").GetComponent<LineRenderer>().SetPosition(2, Vector3.zero);
            }
        }
    }

    private void UpdateDetailedViews(Dictionary<string, Transform> newVis, Dictionary<string, Transform> oldVis)
    {
        if (oldVis.Count == 0)
        {
            foreach (Transform t in newVis.Values.ToList())
            {
                GenerateDetailedView(t);
            }
        }
        else
        {
            // add new vis
            foreach (Transform t in newVis.Values.ToList())
            {
                if (!oldVis.Keys.Contains(t.name))
                {
                    GenerateDetailedView(t);
                }
            }

            foreach (Transform t in oldVis.Values.ToList())
            {
                if (!newVis.Keys.Contains(t.name))
                {
                    string removedName = t.name;
                    Destroy(t.gameObject);
                    currentDetailedViews.Remove(removedName);
                }
            }
        }
    }

    //private List<Transform> RearrangeDisplayBasedOnAngle(List<Transform> markers)
    //{
    //    List<Transform> finalList = new List<Transform>();
    //    if (markers != null && markers.Count > 0)
    //    {
    //        Dictionary<Transform, float> markerAnglesToHuman = new Dictionary<Transform, float>();

    //        foreach (Transform t in markers)
    //            markerAnglesToHuman.Add(t, Vector3.SignedAngle(HumanWaist.forward, t.position - HumanWaist.position, Vector3.up));

    //        foreach (KeyValuePair<Transform, float> item in markerAnglesToHuman.OrderBy(key => key.Value))
    //            finalList.Add(item.Key);
    //    }
    //    return finalList;
    //}

    private List<Transform> RearrangeDisplayBasedOnLandmarkPosition(List<Transform> markers)
    {
        List<Transform> finalList = new List<Transform>();
        if (Landmark == ReferenceFrames.Shelves)
        {
            if (markers != null && markers.Count > 0)
            {
                Dictionary<Transform, float> markerLocPositionX = new Dictionary<Transform, float>();

                foreach (Transform t in markers)
                {
                    float positionX;
                    if (t.parent == TableTopDisplay)
                        positionX = t.localPosition.x;
                    else
                        positionX = TableTopDisplay.InverseTransformPoint(t.position).x;
                    markerLocPositionX.Add(t, positionX);
                }

                foreach (KeyValuePair<Transform, float> item in markerLocPositionX.OrderBy(key => key.Value))
                    finalList.Add(item.Key);
            }
        } else if (Landmark == ReferenceFrames.Floor) {
            if (markers != null && markers.Count > 0)
            {
                Dictionary<Transform, float> markerLocPositionX = new Dictionary<Transform, float>();

                foreach (Transform t in markers)
                {
                    float positionX;
                    if (t.parent == GroundDisplay)
                        positionX = t.localPosition.x;
                    else
                        positionX = GroundDisplay.InverseTransformPoint(t.position).x;
                    markerLocPositionX.Add(t, positionX);
                }

                foreach (KeyValuePair<Transform, float> item in markerLocPositionX.OrderBy(key => key.Value)) {
                    finalList.Add(item.Key);
                }
                    
            }
        }
        else
        {
            if (markers != null && markers.Count > 0)
            {
                Dictionary<Transform, float> markerLocAngleFromCenter = new Dictionary<Transform, float>();

                foreach (Transform t in markers)
                {
                    Vector3 position2D;
                    if (t.parent == WaistLevelDisplay)
                        position2D = new Vector3(t.localPosition.x, 0, t.localPosition.z);
                    else
                        position2D = new Vector3(WaistLevelDisplay.InverseTransformPoint(t.position).x, 0, WaistLevelDisplay.InverseTransformPoint(t.position).z);
                    float angleFromForward = Vector3.SignedAngle(Vector3.forward, position2D, Vector3.up);
                    markerLocAngleFromCenter.Add(t, angleFromForward);
                }

                foreach (KeyValuePair<Transform, float> item in markerLocAngleFromCenter.OrderBy(key => key.Value))
                    finalList.Add(item.Key);
            }
        }

        return finalList;
    }

    private Dictionary<string, Transform> RearrangeVisOnDashBoard(List<Transform> newOrderedList, Dictionary<string, Transform> oldDict)
    {
        Dictionary<string, Transform> orderedDict = new Dictionary<string, Transform>();

        foreach (Transform t in newOrderedList)
        {
            if (oldDict.ContainsKey(t.name))
                orderedDict.Add(t.name, oldDict[t.name]);
        }

        return orderedDict;
    }

    #endregion

    #region Get Function
    // public get current landmarks
    public List<Transform> GetCurrentLandmarks()
    {
        return currentLandmarks.Values.ToList();
    }
    // Tracking human body to determine what to display, can return no more than 3 vis
    private List<Transform> GetVisFromInteraction(Transform implicitObject)
    {
        List<Transform> showOnDashboard = new List<Transform>();

        if (explicitlySelectedVis.Count > 3)
            Debug.LogError("Too many manually selected VIS!!!");

        List<Transform> nearestVisMix = GetNearestVis(implicitObject, explicitlySelectedVis);

        foreach (Transform t in nearestVisMix)
            showOnDashboard.Add(t);

        // if some vis to show
        if (showOnDashboard.Count > 0)
        {
            // remove duplicates
            showOnDashboard = showOnDashboard.Distinct().ToList();

            // list to dictionary
            Dictionary<string, Transform> newVisDict = new Dictionary<string, Transform>();
            foreach (Transform t in showOnDashboard)
            {
                if (t != null)
                    newVisDict.Add(t.name, t);
            }

            // update vis on detailed views
            UpdateDetailedViews(newVisDict, currentDetailedViews);

            //showOnDashboard = RearrangeDisplayBasedOnAngle(showOnDashboard);
            showOnDashboard = RearrangeDisplayBasedOnLandmarkPosition(showOnDashboard);
            return showOnDashboard;
        }
        else
            return new List<Transform>();
    }

    private List<Transform> GetVisFromInteraction(Transform leftHand, Transform rightHand)
    {
        List<Transform> showOnDashboard = new List<Transform>();

        if (explicitlySelectedVis.Count > 3)
            Debug.LogError("Too many manually selected VIS!!!");

        List<Transform> nearestVisMix = GetNearestVis(leftHand, rightHand, explicitlySelectedVis);

        foreach (Transform t in nearestVisMix)
            showOnDashboard.Add(t);

        // if some vis to show
        if (showOnDashboard.Count > 0)
        {
            // remove duplicates
            showOnDashboard = showOnDashboard.Distinct().ToList();

            // list to dictionary
            Dictionary<string, Transform> newVisDict = new Dictionary<string, Transform>();
            foreach (Transform t in showOnDashboard)
            {
                if (t != null)
                    newVisDict.Add(t.name, t);
            }

            // update vis on detailed views
            UpdateDetailedViews(newVisDict, currentDetailedViews);

            showOnDashboard = RearrangeDisplayBasedOnLandmarkPosition(showOnDashboard);
            //showOnDashboard = RearrangeDisplayBasedOnAngle(showOnDashboard);
            return showOnDashboard;
        }
        else
            return new List<Transform>();
    }

    private List<Transform> GetNearestVis(Transform reference, List<Transform> previousSelectedVis)
    {
        List<Transform> originalList = new List<Transform>();
        List<Transform> nearestList = new List<Transform>();
        // load original list
        foreach (Transform t in currentLandmarks.Values)
        {
            if (!previousSelectedVis.Contains(t))
                originalList.Add(t);
        }

        if (previousSelectedVis.Count < 3)
        {
            if (originalList.Count > 2)
            {
                // get nearest vis
                for (int i = 0; i < (3 - previousSelectedVis.Count); i++)
                {
                    float minDis = 10000;
                    Transform nearestOne = null;
                    foreach (Transform t in originalList)
                    {
                        if (Vector3.Distance(t.position, reference.position) < minDis)
                        {
                            minDis = Vector3.Distance(t.position, reference.position);
                            nearestOne = t;
                        }
                    }

                    if (nearestOne != null)
                    {
                        nearestList.Add(nearestOne);
                        originalList.Remove(nearestOne);
                    }
                    else
                        Debug.Log("Error");
                }

                foreach (Transform t in previousSelectedVis)
                {
                    nearestList.Add(t);
                }
            }
        }
        else
        {
            nearestList = previousSelectedVis;
        }

        return nearestList;
    }

    private List<Transform> GetNearestVis(Transform leftHand, Transform rightHand, List<Transform> previousSelectedVis)
    {
        List<Transform> originalList = new List<Transform>();
        List<Transform> nearestList = new List<Transform>();
        // load original list
        foreach (Transform t in currentLandmarks.Values)
        {
            if (!previousSelectedVis.Contains(t))
                originalList.Add(t);
        }

        if (previousSelectedVis.Count < 3)
        {
            if (originalList.Count > 2)
            {
                // get nearest vis
                for (int i = 0; i < (3 - previousSelectedVis.Count); i++)
                {
                    float minDis = 10000;
                    Transform nearestOne = null;
                    foreach (Transform t in originalList)
                    {
                        if (Vector3.Distance(t.position, leftHand.position) < minDis)
                        {
                            minDis = Vector3.Distance(t.position, leftHand.position);
                            nearestOne = t;
                        }

                        if (Vector3.Distance(t.position, rightHand.position) < minDis)
                        {
                            minDis = Vector3.Distance(t.position, rightHand.position);
                            nearestOne = t;
                        }
                    }

                    if (nearestOne != null)
                    {
                        nearestList.Add(nearestOne);
                        originalList.Remove(nearestOne);
                    }
                    else
                        Debug.Log("Error");
                }

                foreach (Transform t in previousSelectedVis)
                {
                    nearestList.Add(t);
                }
            }
        }
        else
        {
            nearestList = previousSelectedVis;
        }

        return nearestList;
    }

    public static List<Transform> GetRandomItemsFromList(List<Transform> list, int number)
    {
        // this is the list we're going to remove picked items from
        List<Transform> tmpList = new List<Transform>(list);
        // this is the list we're going to move items to
        List<Transform> newList = new List<Transform>();

        // make sure tmpList isn't already empty
        while (newList.Count < number && tmpList.Count > 0)
        {
            int index = Random.Range(0, tmpList.Count);
            newList.Add(tmpList[index]);
            tmpList.RemoveAt(index);
        }

        return newList;
    }

    //public void GetShoulderPosition()
    //{
    //    shoulderPosition = TrackedShoulderPosition.position;
    //    Shoulder.position = shoulderPosition;
    //}

    //public void GetArmLength()
    //{
    //    armLength = Vector3.Distance(shoulderPosition, MainHand.position);
    //    Shoulder.GetChild(0).localScale = Vector3.one * armLength * 2;

    //    RePositionLandmarks(ReferenceFrames.Body);
    //}

    public void AddExplicitSelection(Transform t)
    {
        t.GetComponent<Vis>().Selected = true;
        if (currentDetailedViews.ContainsKey(t.name))
            currentDetailedViews[t.name].GetComponent<Vis>().Selected = true;

        explicitlySelectedVis.Add(t);
        if (explicitlySelectedVis.Count > 3)
        {
            RemoveExplicitSelection(explicitlySelectedVis[0]);
            //explicitlySelectedVis[0].GetComponent<Vis>().Selected = false;
            //explicitlySelectedVis[0].Find("LineToDV").GetComponent<LineRenderer>().SetPosition(0, Vector3.zero);
            //explicitlySelectedVis[0].Find("LineToDV").GetComponent<LineRenderer>().SetPosition(1, Vector3.zero);
            //explicitlySelectedVis[0].Find("LineToDV").GetComponent<LineRenderer>().SetPosition(2, Vector3.zero);
            //explicitlySelectedVis.RemoveAt(0);
        }
    }

    public void RemoveExplicitSelection(Transform t)
    {
        if (explicitlySelectedVis.Contains(t))
        {
            t.GetComponent<Vis>().Selected = false;
            t.Find("LineToDV").GetComponent<LineRenderer>().SetPosition(0, Vector3.zero);
            t.Find("LineToDV").GetComponent<LineRenderer>().SetPosition(1, Vector3.zero);
            t.Find("LineToDV").GetComponent<LineRenderer>().SetPosition(2, Vector3.zero);
            
            explicitlySelectedVis.Remove(t);
        }
    }

    private void ConnectLandmarkWithDV(Transform landmark, Transform detailedView, string mode)
    {
        Transform landmarkBorder = null;

        if (Vector3.Distance(landmark.GetComponent<Vis>().VisBorder.GetChild(0).position, detailedView.position) <
            Vector3.Distance(landmark.GetComponent<Vis>().VisBorder.GetChild(1).position, detailedView.position))
        {
            landmarkBorder = landmark.GetComponent<Vis>().VisBorder.GetChild(0);
        }
        else
            landmarkBorder = landmark.GetComponent<Vis>().VisBorder.GetChild(1);

        Transform detailedViewBorder = detailedView.GetComponent<Vis>().VisBorder.GetChild(1);

        Vector3 landmarkToTable = TableTopDisplay.InverseTransformPoint(landmarkBorder.position);
        Vector3 tableBorder = new Vector3(landmarkToTable.x, 0.03f, 0.3f);
        if (landmarkBorder != null & detailedViewBorder != null)
        {
            landmark.Find("LineToDV").GetComponent<LineRenderer>().SetPosition(0, landmarkBorder.position);
            if (landmark.GetComponent<Vis>().Moving || Landmark != ReferenceFrames.Shelves)
                landmark.Find("LineToDV").GetComponent<LineRenderer>().SetPosition(1, (landmarkBorder.position + detailedViewBorder.position) / 2);
            else
                landmark.Find("LineToDV").GetComponent<LineRenderer>().SetPosition(1, TableTopDisplay.TransformPoint(tableBorder));
            landmark.Find("LineToDV").GetComponent<LineRenderer>().SetPosition(2, detailedViewBorder.position);

            if (mode == "selected")
                landmark.Find("LineToDV").GetComponent<LineRenderer>().material.SetColor("_UnlitColor", Color.blue);
            else
                landmark.Find("LineToDV").GetComponent<LineRenderer>().material.SetColor("_UnlitColor", Color.green);
        }

    }
    #endregion

    #region Checker Function
    // BODY-TRACKING: check if waist is moving
    private bool CheckHumanWaistMoving()
    {
        Vector3 currentWaistPosition = filteredWaistPosition;
        if (currentWaistPosition == previousHumanWaistPosition)
            return false;
        previousHumanWaistPosition = currentWaistPosition;
        return true;
    }

    // BODY-TRACKING: check if waist is rotating
    private bool CheckHumanWaistRotating()
    {
        Vector3 currentWaistRotation = filteredWaistRotation;
        if (currentWaistRotation == previousHumanWaistRotation)
            return false;
        previousHumanWaistRotation = currentWaistRotation;
        return true;
    }

    private bool CheckHumanHandMoving()
    {
        Vector3 currentLeftHandPosition = filteredLeftHandPosition;
        Vector3 currentRightHandPosition = filteredRightHandPosition;

        if (currentLeftHandPosition == previousLeftHandPosition && currentRightHandPosition == previousRightHandPosition)
            return false;
        else
        {
            previousLeftHandPosition = currentLeftHandPosition;
            previousRightHandPosition = currentRightHandPosition;

            bool flag = false;

            foreach (Transform t in currentLandmarks.Values)
            {
                if (Vector3.Distance(t.position, currentLeftHandPosition) < ImplicitDistance)
                    flag = true;
            }

            foreach (Transform t in currentLandmarks.Values)
            {
                if (Vector3.Distance(t.position, currentRightHandPosition) < ImplicitDistance)
                    flag = true;
            }

            if (!flag)
                return false;
        }
        return true;
    }

    private void DetectOverlapAndFix()
    {
        foreach (Transform t in currentLandmarks.Values)
        {
            List<Transform> tmpList = currentLandmarks.Values.ToList();
            tmpList.Remove(t);
            foreach (Transform t2 in tmpList)
            {
                if (Vector3.Angle(t.localPosition, t2.position) < 20)
                {
                    Vector3 projection = Vector3.Project(t2.position, t.localPosition);
                    float d = projection.magnitude / 2;
                    Vector3 newDirection = (t2.position - projection).normalized * d + projection;
                    t2.localPosition = newDirection.normalized * armLength;
                }
            }
        }
    }

    //public Vector3 RefineMovingPosition(Transform self, Vector3 previousMovingPosition)
    //{
    //    bool allPassed = true;
    //    Vector3 newPosition = previousMovingPosition;

    //    List<Transform> tmpList = new List<Transform>();
    //    tmpList = currentLandmarks.Values.ToList();
    //    tmpList.Remove(self);

    //    foreach (Transform t in tmpList)
    //    {
    //        if (Vector3.Angle(t.localPosition, previousMovingPosition) < 30)
    //        {
    //            allPassed = false;
    //            //Vector3 projection = Vector3.Project(previousMovingPosition, t.localPosition);
    //            //float d = projection.magnitude / 2;
    //            //Vector3 newDirection = (previousMovingPosition - projection).normalized * d + projection;
    //            //newPosition = newDirection;
    //            float newX = 0;
    //            float newY = 0;
    //            float newZ = 0;
    //            if (previousMovingPosition.x > 0)
    //                newX = Random.Range(0, 0.01f);
    //            else
    //                newX = Random.Range(-0.01f, 0);

    //            if (previousMovingPosition.y > 0)
    //                newY = Random.Range(0, 0.01f);
    //            else
    //                newY = Random.Range(-0.01f, 0);

    //            if (previousMovingPosition.z > 0)
    //                newZ = Random.Range(0, 0.01f);
    //            else
    //                newZ = Random.Range(-0.01f, 0);
    //            newPosition = previousMovingPosition + new Vector3(newX, newY, newZ);
    //            break;
    //        }
    //    }

    //    if (allPassed)
    //        return newPosition;
    //    else
    //        RefineMovingPosition(self, newPosition);

    //    return Vector3.zero;
    //}
    #endregion
    #endregion
}
