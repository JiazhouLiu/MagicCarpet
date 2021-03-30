using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;
using VRTK;

public class DashboardController : MonoBehaviour
{
    public GameObject linePrefab;
    public GameObject colorFillPrefab;
    public Transform EdgeParent;
    private VRTK_InteractableObject interactableObject;

    public Transform HumanWaist;
    public Transform LeftFoot;
    public Transform RightFoot;

    public Transform HeadDashboard;
    public Transform WaistDashboard;
    public Transform GroundVisParent;

    [Header("Delaunay")]
    public int seed = 0;
    public float halfMapSize = 1f;
    public int numberOfPoints = 20;
    //Constraints

    //One constraints where the vertices are connected to form the entire constraint
    public List<Vector3> constraints;

    [Header("Variables")]
    public bool Delaunay = true;
    public bool ShowColor = false;
    public bool ShowEdge = false;
    public float Show3VisDelta = 0.5f;
    public float speed = 3;
    public float filterFrequency = 120f;
    public float betweenVisDelta = 0.05f;

    [Header("Footmenu")]
    public bool footMenu = false;

    private Transform CameraTransform;
    //The mesh so we can generate when we press a button and display it in DrawGizmos
    private Mesh triangulatedMesh;

    private HalfEdgeData2 CurrentTriangles;
    private Vector3 previousCameraPosition;
    private Vector3 previousCameraRotation;

    // body tracking
    private Vector3 previousLeftFootPosition;
    private Vector3 previousLeftFootRotation;
    private Vector3 previousRightFootPosition;
    private Vector3 previousRightFootRotation;
    private Vector3 previousHumanWaistPosition;
    private Vector3 previousHumanWaistRotation;

    private int previousGroundMarkerNumber = 0;

    private List<Transform> currentEdges;

    private List<Transform> selectedVis;
    private Dictionary<string, Transform> currentVisOnHeadDashboard;
    private Dictionary<string, Transform> currentVisOnWaistDashboard;

    private List<Transform> selectedVisFromLeft;
    private List<Transform> selectedVisFromRight;

    //private List<Transform> currentVisFromLeftFootPhysical;
    //private List<Transform> currentVisFromRightFootPhysical;

    private bool DemoFlagLeft = true;
    private bool DemoFlagRight = true;
    private bool isThrowing = false;

    private float deletionTimer = 0;

    // one euro filter
    private Vector3 filteredCameraPosition;
    private Vector3 filteredCameraRotation;
    private Vector3 filteredWaistPosition;
    private Vector3 filteredWaistRotation;
    private Vector3 filteredLeftFootPosition;
    private Vector3 filteredLeftFootRotation;
    private Vector3 filteredRightFootPosition;
    private Vector3 filteredRightFootRotation;
    private OneEuroFilter<Vector3> vector3Filter;

    private void Awake()
    {
        currentVisOnHeadDashboard = new Dictionary<string, Transform>();
        currentVisOnWaistDashboard = new Dictionary<string, Transform>();
        selectedVis = new List<Transform>();
        selectedVisFromLeft = new List<Transform>();
        selectedVisFromRight = new List<Transform>();
        //currentVisFromLeftFootPhysical = new List<Transform>();
        //currentVisFromRightFootPhysical = new List<Transform>();
        vector3Filter = new OneEuroFilter<Vector3>(filterFrequency);
        CurrentTriangles = new HalfEdgeData2();

        // initiate vis model
        foreach (Transform t in WaistDashboard)
        {
            Vis newVis = new Vis(t.name);
            newVis.OnWaistDashBoard = true;
            t.GetComponent<Vis>().CopyEntity(newVis);
            currentVisOnWaistDashboard.Add(t.name, t);
        }

        if (EdgeParent.childCount > 0)
        {
            foreach (Transform t in EdgeParent)
                Destroy(t.gameObject);
        }

        if (GroundVisParent.childCount > 2)
            GenerateTriangulation();
    }

    private void Update()
    {
        if (Camera.main != null && CameraTransform == null)
            CameraTransform = Camera.main.transform;

        // OneEuroFilter
        if (CameraTransform != null)
        {
            filteredCameraPosition = vector3Filter.Filter(CameraTransform.position);
            filteredCameraRotation = vector3Filter.Filter(CameraTransform.eulerAngles);
        }
        filteredLeftFootPosition = vector3Filter.Filter(LeftFoot.position);
        filteredLeftFootRotation = vector3Filter.Filter(LeftFoot.eulerAngles);
        filteredRightFootPosition = vector3Filter.Filter(RightFoot.position);
        filteredRightFootRotation = vector3Filter.Filter(RightFoot.eulerAngles);
        filteredWaistPosition = vector3Filter.Filter(HumanWaist.position);
        filteredWaistRotation = vector3Filter.Filter(HumanWaist.eulerAngles);

        
        //if (!footMenu) {
            // detect ground marker change
            if (CheckMarkerMoving(GroundVisParent))
            {
                if (GroundVisParent.childCount > 0)
                    GenerateTriangulation();
            }

            // check vis triggered from left foot
            if (CheckHumanFeetMoving("left") || DemoFlagLeft)
            {
                DemoFlagLeft = false;
                if (CurrentTriangles.faces.Count > 0)
                    selectedVisFromLeft = SetUpDashBoardScale(LeftFoot); // returned multiple vis from left foot
                else
                    selectedVisFromLeft = new List<Transform>();

                selectedVis = CombineFeetVisAndRemoveOld(selectedVisFromLeft, selectedVisFromRight);
                if (selectedVis.Count > 0)
                    currentVisOnHeadDashboard = RearrangeVisOnDashBoard(selectedVis, currentVisOnHeadDashboard);
            }

            // check vis triggered from right foot
            if (CheckHumanFeetMoving("right") || DemoFlagRight)
            {
                DemoFlagRight = false;
                if (CurrentTriangles.faces.Count > 0)
                    selectedVisFromRight = SetUpDashBoardScale(RightFoot); // returned multiple vis from left foot
                else
                    selectedVisFromRight = new List<Transform>();

                selectedVis = CombineFeetVisAndRemoveOld(selectedVisFromLeft, selectedVisFromRight);
                if (selectedVis.Count > 0)
                    currentVisOnHeadDashboard = RearrangeVisOnDashBoard(selectedVis, currentVisOnHeadDashboard);
            }

            // highlight selected Vis
            foreach (Transform groundVis in GroundVisParent)
            {
                Light highlighter = groundVis.GetChild(2).GetComponent<Light>();
                if (selectedVis.Contains(groundVis) || groundVis.GetComponent<Vis>().Selected)
                {
                    groundVis.GetComponent<Vis>().Highlighted = true;
                    highlighter.intensity = 50;
                }
                else
                {
                    groundVis.GetComponent<Vis>().Highlighted = false;
                    highlighter.intensity = 0;
                }
            }

            // reorder vis on dashboard based on angle to the waist
            if (CheckHumanWaistRotating())
            {
                selectedVis = RearrangeDisplayBasedOnAngle(selectedVis);
                currentVisOnHeadDashboard = RearrangeVisOnDashBoard(selectedVis, currentVisOnHeadDashboard);

                HeadDashboard.DetachChildren();
                for (int i = 0; i < currentVisOnHeadDashboard.Count; i++)
                {
                    currentVisOnHeadDashboard.Values.ToList()[currentVisOnHeadDashboard.Count - i - 1].SetParent(HeadDashboard);
                }

            }
        //}
        

        //// testing
        //if (Input.GetKeyDown("z")) {
        //    string firstKey = currentVisOnWaistDashboard.Keys.ToList()[0];
        //    Transform firstTransform = currentVisOnWaistDashboard.Values.ToList()[0];
        //    firstTransform.SetParent(GroundVisParent);
        //    currentVisOnWaistDashboard.Remove(firstKey);
        //    firstTransform.position = new Vector3(HumanWaist.position.x, 0.1f, HumanWaist.position.z);
        //    firstTransform.localScale = Vector3.one;
        //    firstTransform.localEulerAngles = new Vector3(90, HumanWaist.localEulerAngles.y, 0);
        //}
    }

    public void PinToGround(Transform t)
    {
        GameObject visOnGround = Instantiate(t.gameObject, GroundVisParent);
        visOnGround.transform.position = new Vector3(HumanWaist.position.x, 0, HumanWaist.position.z);
        visOnGround.GetComponent<Vis>().GroundPosition = visOnGround.transform.position;
        visOnGround.transform.localEulerAngles = new Vector3(90, 0, 0);
        visOnGround.transform.localScale = t.GetComponent<Vis>().GroundScale;
        visOnGround.GetComponent<Vis>().showOnWaistDashBoard = false;
        visOnGround.name = t.name;

        currentVisOnWaistDashboard.Remove(t.name);
        Destroy(t.gameObject);
    }

    public void GroundToPin(Transform t)
    {
        Transform groundOriginal = GroundVisParent.Find(t.name);
        if (groundOriginal != null)
        {
            Destroy(groundOriginal.gameObject);
            t.SetParent(WaistDashboard);
            currentVisOnWaistDashboard.Add(t.name, t);
            t.GetComponent<Vis>().showOnWaistDashBoard = true;
            t.GetComponent<Vis>().HeadDashboardScale = Vector3.one * 0.33f;
        }
    }

    public void ReturnToPocket(VisController vis) {
        vis.transform.SetParent(WaistDashboard);
        vis.GetComponent<Vis>().OnWaistDashBoard = true;
    }

    public void RemoveFromHeadDashboard(VisController vis, Transform previousParent) {
        Transform removedGroundVis = null;
        if (GroundVisParent.Find(vis.name))
        {
            removedGroundVis = GroundVisParent.Find(vis.name);

            if (selectedVisFromLeft.Contains(removedGroundVis))
                selectedVisFromLeft.Remove(removedGroundVis);
            if (selectedVisFromRight.Contains(removedGroundVis))
                selectedVisFromRight.Remove(removedGroundVis);
        }
        if (currentVisOnHeadDashboard.ContainsKey(vis.name))
            currentVisOnHeadDashboard.Remove(vis.name);

        if (previousParent.GetComponent<DashBoard_New>().display == DisplayDashboard.HeadDisplay) 
        { // pick from head, remove ground one
            Destroy(removedGroundVis.gameObject);
            return;
        } else if (previousParent.GetComponent<DashBoard_New>().display == DisplayDashboard.GroundMarkers)
        { // pick from ground, remove head one
            if (HeadDashboard.Find(vis.name))
                Destroy(HeadDashboard.Find(vis.name).gameObject);
        }
    }

    #region VIS management

    // combine two feet vis from SetUpDashBoardScale and assign position
    private List<Transform> CombineFeetVisAndRemoveOld(List<Transform> leftVis, List<Transform> rightVis)
    {

        if (leftVis.Count == 0 && rightVis.Count == 0)
        {

            // remove old vis
            foreach (Transform t in currentVisOnHeadDashboard.Values.ToList())
            {
                string removedName = t.name;
                Destroy(t.gameObject);
                currentVisOnHeadDashboard.Remove(removedName);
            }
            return new List<Transform>();
        }
        else if (leftVis.Count == 0)
        {
            if (currentVisOnHeadDashboard.Count > 0)
            {
                // remove old vis
                List<string> visNameFromRight = new List<string>();
                foreach (Transform t in rightVis)
                    visNameFromRight.Add(t.name);

                foreach (string s in currentVisOnHeadDashboard.Keys.ToList())
                {
                    if (!visNameFromRight.Contains(s))
                    {
                        GroundVisParent.Find(s).GetComponent<Vis>().OnHeadDashBoard = false;
                        Destroy(currentVisOnHeadDashboard[s].gameObject);
                        currentVisOnHeadDashboard.Remove(s);
                    }
                }
            }

            return rightVis;
        }
        else if (rightVis.Count == 0)
        {
            if (currentVisOnHeadDashboard.Count > 0)
            {
                // remove old vis
                List<string> visNameFromLeft = new List<string>();
                foreach (Transform t in leftVis)
                    visNameFromLeft.Add(t.name);

                foreach (string s in currentVisOnHeadDashboard.Keys.ToList())
                {
                    if (!visNameFromLeft.Contains(s) && GroundVisParent.Find(s) != null)
                    {
                        GroundVisParent.Find(s).GetComponent<Vis>().OnHeadDashBoard = false;
                        Destroy(currentVisOnHeadDashboard[s].gameObject);
                        currentVisOnHeadDashboard.Remove(s);
                    }
                }
            }

            return leftVis;
        }
        else
        {
            // final return list contains ground vis
            List<Transform> wholeListofVis = new List<Transform>();

            foreach (Transform t in leftVis)
            {
                if (!rightVis.Contains(t))
                    wholeListofVis.Add(t);
            }

            foreach (Transform t in rightVis)
                wholeListofVis.Add(t);

            if (currentVisOnHeadDashboard.Count > 0)
            {
                // remove old vis
                List<string> visName = new List<string>();
                foreach (Transform t in wholeListofVis)
                    visName.Add(t.name);

                foreach (string s in currentVisOnHeadDashboard.Keys.ToList())
                {
                    if (!visName.Contains(s))
                    {
                        GroundVisParent.Find(s).GetComponent<Vis>().OnHeadDashBoard = false;
                        Destroy(currentVisOnHeadDashboard[s].gameObject);
                        currentVisOnHeadDashboard.Remove(s);
                    }
                }
            }

            return wholeListofVis;
        }
    }


    // Tracking one foot to determine what to display, can return no more than 3 vis
    private List<Transform> SetUpDashBoardScale(Transform foot)
    {
        Dictionary<string, Transform> FootInMarkers = new Dictionary<string, Transform>();
        FootInMarkers = CheckFootInTriangles(foot);

        List<Transform> showOnDashboard = new List<Transform>();

        if (Delaunay)
        {
            // get foot in triangles
            if (FootInMarkers.Count > 0)
            {
                List<Transform> CheckDistance = CheckDistanceToEdge(foot, FootInMarkers.Values.ToList());
                if (CheckDistance != null)
                {
                    foreach (Transform t in CheckDistance)
                        showOnDashboard.Add(t);
                }
            }
        }
        else
        {
            List<Transform> nearest3Vis = GetNearestVis(foot);
            foreach (Transform t in nearest3Vis)
                showOnDashboard.Add(t);
        }

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

            // check duplicates with current dashboard
            CheckSameVisOnDashboard(newVisDict, currentVisOnHeadDashboard); // check if vis is already on dashboard

            showOnDashboard = RearrangeDisplayBasedOnAngle(showOnDashboard);
            return showOnDashboard;
        }
        else
            return new List<Transform>();
    }

    private List<Transform> GetNearestVis(Transform foot)
    {
        List<Transform> finalList = new List<Transform>();
        List<Transform> originalList = new List<Transform>();
        List<Transform> nearest3List = new List<Transform>();
        // load original list
        foreach (Transform t in GroundVisParent)
            originalList.Add(t);

        if (originalList.Count > 2)
        {
            // get three nearest vis
            for (int i = 0; i < 3; i++)
            {
                float minDis = 10000;
                Transform nearestOne = null;
                foreach (Transform t in originalList)
                {
                    if (Vector3.Distance(t.position, foot.position) < minDis)
                    {
                        minDis = Vector3.Distance(t.position, foot.position);
                        nearestOne = t;
                    }
                }

                if (nearestOne != null)
                {
                    nearest3List.Add(nearestOne);
                    originalList.Remove(nearestOne);
                }
                else
                    Debug.Log("Error");
            }

            // check three nearest vis position
            Vector3 vectorToFirst = nearest3List[0].position - foot.position;
            Vector3 vectorToSecond = nearest3List[1].position - foot.position;
            Vector3 vectorToThird = nearest3List[2].position - foot.position;

            if (Vector3.Angle(vectorToFirst, vectorToSecond) > 90)
            {
                if (!finalList.Contains(nearest3List[0]))
                    finalList.Add(nearest3List[0]);
                if (!finalList.Contains(nearest3List[1]))
                    finalList.Add(nearest3List[1]);
            }

            if (Vector3.Angle(vectorToThird, vectorToSecond) > 90)
            {
                if (!finalList.Contains(nearest3List[2]))
                    finalList.Add(nearest3List[2]);
                if (!finalList.Contains(nearest3List[1]))
                    finalList.Add(nearest3List[1]);
            }

            if (Vector3.Angle(vectorToFirst, vectorToThird) > 90)
            {
                if (!finalList.Contains(nearest3List[0]))
                    finalList.Add(nearest3List[0]);
                if (!finalList.Contains(nearest3List[2]))
                    finalList.Add(nearest3List[2]);
            }
        }

        return finalList;
    }

    private void CheckSameVisOnDashboard(Dictionary<string, Transform> newVis, Dictionary<string, Transform> oldVis)
    {
        if (oldVis.Count == 0)
        {
            foreach (Transform t in newVis.Values.ToList())
            {
                GameObject visOnDashBoard = Instantiate(t.gameObject, HeadDashboard);
                visOnDashBoard.GetComponent<Vis>().OnHeadDashBoard = true;
                visOnDashBoard.GetComponent<Vis>().OnGround = false;
                visOnDashBoard.transform.position = t.position;
                visOnDashBoard.transform.localEulerAngles = Vector3.zero;
                visOnDashBoard.transform.localScale = Vector3.one * 0.1f;
                visOnDashBoard.name = t.name;

                currentVisOnHeadDashboard.Add(t.name, visOnDashBoard.transform);
            }
        }
        else
        {
            // add new vis
            foreach (Transform t in newVis.Values.ToList())
            {
                if (!oldVis.Keys.Contains(t.name))
                {
                    GameObject visOnDashBoard = Instantiate(t.gameObject, HeadDashboard);
                    visOnDashBoard.GetComponent<Vis>().OnHeadDashBoard = true;
                    visOnDashBoard.GetComponent<Vis>().OnGround = false;
                    visOnDashBoard.transform.position = t.position;
                    visOnDashBoard.transform.localEulerAngles = Vector3.zero;
                    visOnDashBoard.transform.localScale = Vector3.one * 0.1f;
                    visOnDashBoard.name = t.name;

                    currentVisOnHeadDashboard.Add(t.name, visOnDashBoard.transform);
                }
            }
        }
    }

    private float CalculateProportionalScale(Transform targetVis, Transform prevVis, Transform nextVis)
    {
        float humanToTarget = Vector3.Distance(HumanWaist.position, targetVis.position);
        float leftEdgeLength = Vector3.Distance(targetVis.position, prevVis.position);
        float rightEdgeLength = Vector3.Distance(targetVis.position, nextVis.position);

        return 2 * humanToTarget / (leftEdgeLength + rightEdgeLength);
    }

    // TODO: change human to waist
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

    #region Checking Functions

    // true if in the middle of triangle and show 3 vis, false if near edge and show 2 vis
    private List<Transform> CheckDistanceToEdge(Transform foot, List<Transform> markers)
    {
        Vector3 Human2DPosition = new Vector3(foot.position.x, 0, foot.position.z);
        float dis1 = DistancePointLine(Human2DPosition, markers[0].position, markers[1].position);
        float dis2 = DistancePointLine(Human2DPosition, markers[1].position, markers[2].position);
        float dis3 = DistancePointLine(Human2DPosition, markers[0].position, markers[2].position);

        if (dis1 >= Show3VisDelta && dis2 >= Show3VisDelta && dis3 >= Show3VisDelta)
            return markers;
        else if (dis1 >= Show3VisDelta && dis2 >= Show3VisDelta)
            return new List<Transform>() { markers[0], markers[2] };
        else if (dis1 >= Show3VisDelta && dis3 >= Show3VisDelta)
            return new List<Transform>() { markers[1], markers[2] };
        else if (dis2 >= Show3VisDelta && dis3 >= Show3VisDelta)
            return new List<Transform>() { markers[0], markers[1] };
        else if (dis1 >= Show3VisDelta)
            return new List<Transform>() { markers[2] };
        else if (dis2 >= Show3VisDelta)
            return new List<Transform>() { markers[0] };
        else if (dis3 >= Show3VisDelta)
            return new List<Transform>() { markers[1] };
        else
            return null;
    }

    private Dictionary<string, Transform> CheckFootInTriangles(Transform foot)
    {
        Dictionary<string, Transform> ValidMarkers = new Dictionary<string, Transform>();

        foreach (HalfEdgeFace2 face in CurrentTriangles.faces)
        {
            Vector3 a = face.edge.v.position.ToVector3();
            Vector3 b = face.edge.nextEdge.v.position.ToVector3();
            Vector3 c = face.edge.prevEdge.v.position.ToVector3();
            if (PointInTriangle(foot.position, a, b, c))
            {

                foreach (Transform t in GroundVisParent)
                {
                    if (Vector3.Distance(t.position, a) < 0.11f ||
                        Vector3.Distance(t.position, b) < 0.11f ||
                        Vector3.Distance(t.position, c) < 0.11f)
                    {

                        if (!ValidMarkers.ContainsKey(t.name))
                            ValidMarkers.Add(t.name, t);
                    }
                }
            }
        }

        if (ValidMarkers.Count == 3)
            return ValidMarkers;
        else
            return new Dictionary<string, Transform>();
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

    // BODY-TRACKING: check if any foot is moving
    private bool CheckHumanFeetMoving(string foot)
    {
        if (foot == "left")
        {
            Vector3 currentLeftPosition = filteredLeftFootPosition;
            if (currentLeftPosition == previousLeftFootPosition)
                return false;
            previousLeftFootPosition = currentLeftPosition;
        }

        if (foot == "right")
        {
            Vector3 currentRightPosition = filteredRightFootPosition;
            if (currentRightPosition == previousRightFootPosition)
                return false;
            previousRightFootPosition = currentRightPosition;
        }

        return true;
    }

    // Check if ground marker is moved
    private bool CheckMarkerMoving(Transform parent)
    {
        if (parent.childCount != previousGroundMarkerNumber)
        {
            previousGroundMarkerNumber = parent.childCount;
            return true;
        }

        foreach (Transform t in GroundVisParent)
        {
            if (t.position != t.GetComponent<Vis>().GroundPosition)
            {
                t.GetComponent<Vis>().GroundPosition = t.position;
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Delaunay Triangulation
    /// <summary>
    /// Delaunay Triangulation
    /// </summary>
    private void DisplayTriangleEdges()
    {
        if (Application.isPlaying)
        {
            if (EdgeParent.childCount > 0)
            {
                foreach (Transform t in EdgeParent)
                    Destroy(t.gameObject);
            }
        }
        if (Application.isEditor)
        {
            if (EdgeParent.childCount > 0)
            {
                foreach (Transform t in EdgeParent)
                    DestroyImmediate(t.gameObject);
            }
        }


        foreach (HalfEdge2 edge in CurrentTriangles.edges)
        {
            Vector3 currentEdgeDirection = edge.v.position.ToVector3();
            Vector3 nextEdgeDirection = edge.nextEdge.v.position.ToVector3();
            GameObject newEdge = Instantiate(linePrefab,
                (currentEdgeDirection + nextEdgeDirection) / 2, Quaternion.identity, EdgeParent);
            newEdge.transform.localPosition = new Vector3(newEdge.transform.localPosition.x, 0, newEdge.transform.localPosition.z);

            newEdge.transform.localScale = new Vector3(Vector3.Distance(nextEdgeDirection, currentEdgeDirection) / 10,
                0.1f, 0.005f);

            LineRenderer line = newEdge.GetComponent<LineRenderer>();
            line.SetPosition(0, currentEdgeDirection);
            line.SetPosition(1, nextEdgeDirection);

            currentEdges.Add(newEdge.transform);
        }
    }

    public void GenerateTriangulation()
    {
        // edges customisation
        if (currentEdges != null && currentEdges.Count > 0 && EdgeParent.childCount > 0)
        {

            foreach (Transform t in currentEdges)
                DestroyImmediate(t.gameObject);
            currentEdges.Clear();
        }
        currentEdges = new List<Transform>();

        //Hull
        List<Vector3> hullPoints = TestAlgorithmsHelpMethods.GetPointsFromParent(GroundVisParent);

        List<MyVector2> hullPoints_2d = hullPoints.Select(x => x.ToMyVector2()).ToList(); ;

        //Normalize to range 0-1
        //We should use all points, including the constraints because the hole may be outside of the random points
        List<MyVector2> allPoints = new List<MyVector2>();

        //allPoints.AddRange(randomPoints_2d);

        allPoints.AddRange(hullPoints_2d);

        Normalizer2 normalizer = new Normalizer2(allPoints);

        List<MyVector2> hullPoints_2d_normalized = normalizer.Normalize(hullPoints_2d);

        HashSet<List<MyVector2>> allHolePoints_2d_normalized = new HashSet<List<MyVector2>>();

        //
        // Generate the triangulation
        //

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

        timer.Start();

        //Algorithm 1. Delaunay by triangulate all points with some bad algorithm and then flip edges until we get a delaunay triangulation 
        //HalfEdgeData2 triangleData_normalized = _Delaunay.FlippingEdges(points_2d_normalized, new HalfEdgeData2());


        //Algorithm 2. Delaunay by inserting point-by-point while flipping edges after inserting a single point 
        //HalfEdgeData2 triangleData_normalized = _Delaunay.PointByPoint(points_2d_normalized, new HalfEdgeData2());


        //Algorithm 3. Constrained delaunay
        HalfEdgeData2 triangleData_normalized = _Delaunay.ConstrainedBySloan(null, hullPoints_2d_normalized, allHolePoints_2d_normalized, shouldRemoveTriangles: true, new HalfEdgeData2());

        timer.Stop();

        //Debug.Log($"Generated a delaunay triangulation in {timer.ElapsedMilliseconds / 1000f} seconds");


        //UnNormalize
        HalfEdgeData2 triangleData = normalizer.UnNormalize(triangleData_normalized);
        //Debug.Log(triangleData.faces.Count);

        // Customise By Joe
        CurrentTriangles = triangleData;


        //From half-edge to triangle
        HashSet<Triangle2> triangles_2d = _TransformBetweenDataStructures.HalfEdge2ToTriangle2(triangleData);

        //From triangulation to mesh

        //Make sure the triangles have the correct orientation
        triangles_2d = HelpMethods.OrientTrianglesClockwise(triangles_2d);

        //From 2d to 3d
        HashSet<Triangle3> triangles_3d = new HashSet<Triangle3>();

        foreach (Triangle2 t in triangles_2d)
        {
            triangles_3d.Add(new Triangle3(t.p1.ToMyVector3_Yis3D(), t.p2.ToMyVector3_Yis3D(), t.p3.ToMyVector3_Yis3D()));
        }

        triangulatedMesh = _TransformBetweenDataStructures.Triangle3ToCompressedMesh(triangles_3d);

        if (Application.isPlaying)
        {
            if (ShowEdge)
                DisplayTriangleEdges();

            if (triangulatedMesh != null && ShowColor)
            {
                DisplayMeshWithRandomColors(triangulatedMesh, seed);
            }
        }
    }

    //Random color
    //Seed is determining the random color
    public void DisplayMeshWithRandomColors(Mesh mesh, int seed)
    {
        DisplayMesh(mesh, true, seed, Color.black);
    }

    //Display some mesh where each triangle could have a random color
    private void DisplayMesh(Mesh mesh, bool useRandomColor, int seed, Color meshColor)
    {
        if (mesh == null)
        {
            Debug.Log("Cant display the mesh because there's no mesh!");
            return;
        }

        //Display the entire mesh with a single color
        if (!useRandomColor)
        {
            Gizmos.color = meshColor;

            mesh.RecalculateNormals();

            Gizmos.DrawMesh(mesh);
        }
        //Display the individual triangles with a random color
        else
        {
            int[] meshTriangles = mesh.triangles;

            Vector3[] meshVertices = mesh.vertices;

            Random.InitState(seed);

            for (int i = 0; i < meshTriangles.Length; i += 3)
            {
                //Make a single mesh triangle
                Vector3 p1 = meshVertices[meshTriangles[i + 0]];
                Vector3 p2 = meshVertices[meshTriangles[i + 1]];
                Vector3 p3 = meshVertices[meshTriangles[i + 2]];

                Mesh triangleMesh = new Mesh();

                triangleMesh.vertices = new Vector3[] { p1, p2, p3 };

                triangleMesh.triangles = new int[] { 0, 1, 2 };

                triangleMesh.RecalculateNormals();

                //Color the triangle
                Color newColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.2f);

                //Display it
                GameObject go = Instantiate(colorFillPrefab, EdgeParent);
                go.GetComponent<MeshFilter>().mesh = triangleMesh;
                go.GetComponent<MeshRenderer>().material.color = newColor;
            }
        }
    }

    // ultilities functions
    private bool SameSide(Vector3 p1, Vector3 p2, Vector3 a, Vector3 b)
    {
        Vector3 cp1 = Vector3.Cross(b - a, p1 - a);
        Vector3 cp2 = Vector3.Cross(b - a, p2 - a);

        if (Vector3.Dot(cp1, cp2) >= 0)
            return true;

        return false;
    }

    private bool PointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        if (SameSide(p, a, b, c) && SameSide(p, b, a, c) && SameSide(p, c, a, b))
            return true;
        return false;
    }

    private float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        return Vector3.Magnitude(ProjectPointLine(point, lineStart, lineEnd) - point);
    }

    private static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 relativePoint = point - lineStart;
        Vector3 lineDirection = lineEnd - lineStart;
        float length = lineDirection.magnitude;
        Vector3 normalizedLineDirection = lineDirection;
        if (length > .000001f)
            normalizedLineDirection /= length;

        float dot = Vector3.Dot(normalizedLineDirection, relativePoint);
        dot = Mathf.Clamp(dot, 0.0F, length);

        return lineStart + normalizedLineDirection * dot;
    }
    #endregion
}