using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;
using VRTK;
using UnityEngine.Rendering.HighDefinition;

public class DashboardController_UserStudy : MonoBehaviour
{
    public Transform OriginalVisParent;
    public Transform Shoulder;
    public Transform Shelves;
    public Transform FloorSurface;

    // body tracking
    public Transform HumanWaist;
    public Transform MainHand;

    // dashboards
    public Transform HeadLevelDisplay;
    public Transform WaistLevelDisplay;
    public Transform GroundDisplay;
    public Transform ShelvesDisplay;

    [Header("Variables")]
    public float Show3VisDelta = 0.5f;
    public float speed = 3;
    public float filterFrequency = 120f;
    public float betweenVisDelta = 0.05f;
    public Vector3 shoulderPosition;
    public float armLength = 0.6f;

    [Header("Experiment Setup")]
    public int VisNumber = 6;
    public ReferenceFrames Landmark;
    public ReferenceFrames DetailedView;
    public float LandmarkSizeOnGround = 0.5f;
    public float LandmarkSizeOnBody = 0.2f;
    public float LandmarkSizeOnShelves = 1f;

    // body tracking
    private Transform CameraTransform;
    private Vector3 previousHumanWaistPosition;
    private Vector3 previousHumanWaistRotation;
    private Vector3 previousHumanHandPosition;

    // one euro filter
    private Vector3 filteredWaistPosition;
    private Vector3 filteredWaistRotation;
    private Vector3 filteredHandPosition;
    private OneEuroFilter<Vector3> vector3Filter;

    // experiment use
    private bool DemoFlag = true;
    private bool ShelvesPositionConfirmed = false;
    private List<Transform> visParentList;
    private List<Transform> originalLandmarks;

    private List<Transform> selectedVis;
    private List<Transform> explicitlySelectedVis;

    private Dictionary<string, Transform> currentLandmarks;
    private Dictionary<string, Transform> currentDetailedViews;

    private float highlighterIntensity = 0;


    private void Awake()
    {
        // enable landmarks and detailed views based on configuration 
        switch (Landmark) {
            case ReferenceFrames.Body:
                Shoulder.gameObject.SetActive(true);
                WaistLevelDisplay.gameObject.SetActive(true);
                highlighterIntensity = 10;
                break;
            case ReferenceFrames.Floor:
                GroundDisplay.gameObject.SetActive(true);
                FloorSurface.gameObject.SetActive(true);
                highlighterIntensity = 10;
                break;
            case ReferenceFrames.Shelves:
                Shelves.gameObject.SetActive(true);
                Shelves.position = new Vector3(0, 0.5f, 1.65f);
                ShelvesDisplay.gameObject.SetActive(true);
                highlighterIntensity = 0.5f;
                break;
        }

        switch (DetailedView) {
            case ReferenceFrames.Body:
                HeadLevelDisplay.gameObject.SetActive(true);
                break;
            case ReferenceFrames.Shelves:
                Shelves.gameObject.SetActive(true);
                ShelvesDisplay.gameObject.SetActive(true);
                ShelvesDisplay.position = Shelves.position;
                Shelves.GetChild(6).gameObject.SetActive(true);
                Shelves.GetChild(7).gameObject.SetActive(true);
                break;
        }

        // landmarks
        visParentList = new List<Transform>();
        originalLandmarks = new List<Transform>();

        selectedVis = new List<Transform>();
        explicitlySelectedVis = new List<Transform>();

        currentLandmarks = new Dictionary<string, Transform>();
        currentDetailedViews = new Dictionary<string, Transform>();
        
        foreach (Transform t in OriginalVisParent)
        {
            visParentList.Add(t);
        }

        // one euro filter
        vector3Filter = new OneEuroFilter<Vector3>(filterFrequency);

        if (Landmark == ReferenceFrames.Body) // vis on body as landmarks
            WaistLevelDisplay.position = Shoulder.position;

        // initiate landmarks
        originalLandmarks = GetRandomItemsFromList(visParentList, VisNumber);
        PositionLandmarks(Landmark, originalLandmarks);
    }

    private void Update()
    {
        if (Camera.main != null && CameraTransform == null)
            CameraTransform = Camera.main.transform;

        // OneEuroFilter
        filteredWaistPosition = vector3Filter.Filter(HumanWaist.position);
        filteredWaistRotation = vector3Filter.Filter(HumanWaist.eulerAngles);
        filteredHandPosition = vector3Filter.Filter(MainHand.position);

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
                selectedVis = RearrangeDisplayBasedOnAngle(selectedVis);
                currentDetailedViews = RearrangeVisOnDashBoard(selectedVis, currentDetailedViews);
                if (DetailedView == ReferenceFrames.Body)
                {
                    HeadLevelDisplay.DetachChildren();
                    for (int i = 0; i < currentDetailedViews.Count; i++)
                    {
                        currentDetailedViews.Values.ToList()[currentDetailedViews.Count - i - 1].SetParent(HeadLevelDisplay);
                    }
                } else if (DetailedView == ReferenceFrames.Shelves) {
                    ShelvesDisplay.DetachChildren();
                    for (int i = 0; i < currentDetailedViews.Count; i++)
                    {
                        currentDetailedViews.Values.ToList()[currentDetailedViews.Count - i - 1].SetParent(ShelvesDisplay);
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

                UpdateVisFromPositionChange(MainHand.transform);
            }
        }

        if (DetailedView == ReferenceFrames.Shelves && !ShelvesPositionConfirmed && Camera.main != null) {
            if (Camera.main.transform.position.y > 1.3f) {
                Shelves.position = new Vector3(0, Camera.main.transform.position.y, 1.65f);
                ShelvesDisplay.position = Shelves.position;
                ShelvesPositionConfirmed = true;
            }
        }

        UpdateHighlighter();
    }

    #region Experiment Use
    #region Generate Function
    private Transform GenerateDetailedView(Transform t)
    {
        GameObject visOnDetailedView = Instantiate(t.gameObject);
        visOnDetailedView.name = t.name;

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
            visOnDetailedView.transform.SetParent(ShelvesDisplay);
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

        // setup components
        visOnDetailedView.GetComponent<Rigidbody>().isKinematic = true;
        visOnDetailedView.GetComponent<BoxCollider>().enabled = false;

        currentDetailedViews.Add(visOnDetailedView.name, visOnDetailedView.transform);

        return visOnDetailedView.transform;
    }

    private void PositionLandmarks(ReferenceFrames landmark, List<Transform> originalLandmarks)
    {
        if (landmark == ReferenceFrames.Floor) // landmarks on floor
        {
            foreach (Transform t in originalLandmarks)
            {
                // Instantiate game object
                GameObject newLandmark = Instantiate(t.gameObject, Vector3.zero, Quaternion.identity);
                newLandmark.name = t.name;

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
                GameObject newLandmark = Instantiate(t.gameObject, Vector3.zero, Quaternion.identity, ShelvesDisplay);
                newLandmark.name = t.name;

                // setup transform
                newLandmark.transform.localScale = new Vector3(LandmarkSizeOnShelves, LandmarkSizeOnShelves, LandmarkSizeOnShelves);
                newLandmark.transform.localEulerAngles = new Vector3(0, 0, 0); 

                // setup vis model
                Vis newVis = new Vis(newLandmark.name)
                {
                    OnShelves = true
                };
                newLandmark.GetComponent<Vis>().CopyEntity(newVis);

                currentLandmarks.Add(newLandmark.name, newLandmark.transform);
            }
            ShelvesDisplay.GetComponent<ReferenceFrameController_UserStudy>().InitialiseLandmarkPositions(currentLandmarks.Values.ToList(), landmark);
        }

    }
    #endregion

    #region Update Function
    private void UpdateVisFromPositionChange(Transform implicitObject)
    {
        selectedVis = GetVisFromInteraction(implicitObject);

        if (selectedVis.Count > 0)
            currentDetailedViews = RearrangeVisOnDashBoard(selectedVis, currentDetailedViews);
    }

    private void UpdateHighlighter()
    {
        // highlight selected Vis
        foreach (Transform landmark in currentLandmarks.Values)
        {
            Light highlighter = landmark.GetChild(2).GetComponent<Light>();
            if (landmark.GetComponent<Vis>().Moving)
            {
                highlighter.color = Color.yellow;
                landmark.GetChild(2).GetComponent<HDAdditionalLightData>().SetIntensity(highlighterIntensity);
            }
            else if (explicitlySelectedVis.Contains(landmark))
            {
                highlighter.color = Color.blue;
                landmark.GetChild(2).GetComponent<HDAdditionalLightData>().SetIntensity(highlighterIntensity);
            }
            else if (selectedVis.Contains(landmark))
            {
                highlighter.color = Color.green;
                landmark.GetComponent<Vis>().Highlighted = true;
                landmark.GetChild(2).GetComponent<HDAdditionalLightData>().SetIntensity(highlighterIntensity);
            }
            else
            {
                highlighter.color = Color.white;
                landmark.GetComponent<Vis>().Highlighted = false;
                landmark.GetChild(2).GetComponent<HDAdditionalLightData>().SetIntensity(0);
            }
        }

        foreach (Transform detailedView in currentDetailedViews.Values.ToList())
        {
            detailedView.GetChild(2).GetComponent<HDAdditionalLightData>().SetIntensity(0);
        }
    }

    private void UpdateDetailedViews(Dictionary<string, Transform> newVis, Dictionary<string, Transform> oldVis)
    {
        if (oldVis.Count == 0)
        {
            foreach (Transform t in newVis.Values.ToList()) {
                GenerateDetailedView(t);
            }
        }
        else
        {
            // add new vis
            foreach (Transform t in newVis.Values.ToList())
            {
                if (!oldVis.Keys.Contains(t.name)) {
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

    private List<Transform> RearrangeDisplayBasedOnAngle(List<Transform> markers)
    {
        List<Transform> finalList = new List<Transform>();
        if (markers != null && markers.Count > 0)
        {
            Dictionary<Transform, float> markerAnglesToHuman = new Dictionary<Transform, float>();

            foreach (Transform t in markers)
                markerAnglesToHuman.Add(t, Vector3.SignedAngle(HumanWaist.forward, t.position - HumanWaist.position, Vector3.up));

            foreach (KeyValuePair<Transform, float> item in markerAnglesToHuman.OrderBy(key => key.Value))
                finalList.Add(item.Key);
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

            showOnDashboard = RearrangeDisplayBasedOnAngle(showOnDashboard);
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
                for (int i = 0; i < 3 - previousSelectedVis.Count; i++)
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

    public void GetShoulderPosition() {
        shoulderPosition = MainHand.position;
        Shoulder.position = shoulderPosition;
    }

    public void GetArmLength() {
        armLength = Vector3.Distance(shoulderPosition, MainHand.position);
        Shoulder.GetChild(0).localScale = Vector3.one * armLength * 2;
    }

    public void AddExplicitSelection(Transform t) {
        t.GetComponent<Vis>().Selected = true;
        explicitlySelectedVis.Add(t);
        if (explicitlySelectedVis.Count > 3) {
            explicitlySelectedVis[0].GetComponent<Vis>().Selected = false;
            explicitlySelectedVis.RemoveAt(0);
        } 
    }

    public void RemoveExplicitSelection(Transform t)
    {
        if (explicitlySelectedVis.Contains(t)) {
            t.GetComponent<Vis>().Selected = false;
            explicitlySelectedVis.Remove(t);
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
        Vector3 currentHandPosition = filteredHandPosition;
        if (currentHandPosition == previousHumanHandPosition)
            return false;
        previousHumanHandPosition = currentHandPosition;
        return true;
    }
    #endregion
    #endregion
}
