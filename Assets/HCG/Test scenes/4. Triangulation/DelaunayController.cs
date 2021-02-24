using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;
using UnityEditor;

public class DelaunayController : MonoBehaviour
{
    public GameObject linePrefab;
    public GameObject colorFillPrefab;
    public Transform EdgeParent;
    public Transform Human;
    ////public Transform HumanWaist;
    public Transform LeftFoot;
    public Transform RightFoot;
    public Transform DashBoard;
    public Transform PinnedDashBoard;

    [Header("Delaunay")]
    public int seed = 0;
    public float halfMapSize = 1f;
    public int numberOfPoints = 20;
    //Constraints

    //One constraints where the vertices are connected to form the entire constraint
    public List<Vector3> constraints;

    //Constraints by using children to a parent, which we have to drag
    //Should be sorted counter-clock-wise
    public Transform hullConstraintParent;
    //Should be sorted clock-wise
    public List<Transform> holeConstraintParents;

    [Header("Variables")]
    public bool ShowColor = false;
    public bool ShowEdge = false;
    public float Show3VisDelta = 0.5f;
    public float speed = 3;
    public float filterFrequency = 120f;
    public float betweenVisDelta = 0.05f;

    //The mesh so we can generate when we press a button and display it in DrawGizmos
    private Mesh triangulatedMesh;

    private HalfEdgeData2 CurrentTriangles;
    private Vector3 previousHumanPosition;
    private Vector3 previousHumanRotation;

    // body tracking
    private Vector3 previousLeftFootPosition;
    private Vector3 previousLeftFootRotation;
    private Vector3 previousRightFootPosition;
    private Vector3 previousRightFootRotation;
    ////private Vector3 previousHumanWaistRotation;

    private int previousGroundMarkerNumber;

    private List<Vis> allVis;

    private List<Transform> currentEdges;

    private List<Transform> currentVis;
    private Dictionary<string, Transform> currentVisOnDashboard;
    private Dictionary<string, Transform> currentPinnedOnDashboard;

    private List<Transform> currentVisFromLeft;
    private List<Transform> currentVisFromRight;

    private bool canMove = false;

    private bool DemoFlagLeft = true;
    private bool DemoFlagRight = true;

    // one euro filter
    private Vector3 filteredHumanPosition;
    private Vector3 filteredLeftFootPosition;
    private Vector3 filteredRightFootPosition;
    private Vector3 filteredHumanRotation;
    private Vector3 filteredLeftFootRotation;
    private Vector3 filteredRightFootRotation;
    private Vector3 filteredWaistRotation;
    private OneEuroFilter<Vector3> vector3Filter;

    private void Start()
    {
        allVis = new List<Vis>();
        currentVisOnDashboard = new Dictionary<string, Transform>();
        currentPinnedOnDashboard = new Dictionary<string, Transform>();
        currentVis = new List<Transform>();
        currentVisFromLeft = new List<Transform>();
        currentVisFromRight = new List<Transform>();
        previousGroundMarkerNumber = hullConstraintParent.childCount;
        vector3Filter = new OneEuroFilter<Vector3>(filterFrequency);

        foreach (Transform t in hullConstraintParent) {
            Vis newVis = new Vis(t.name, t.position, t.localScale);
            t.GetComponent<Vis>().CopyEntity(newVis);
            allVis.Add(newVis);
        }

        if (EdgeParent.childCount > 0) {
            foreach (Transform t in EdgeParent)
                Destroy(t.gameObject);
        }

        GenerateTriangulation();


    }

    private void Update()
    {
        filteredHumanPosition = vector3Filter.Filter(Human.position);
        filteredHumanRotation = vector3Filter.Filter(Human.eulerAngles);

        filteredLeftFootPosition = vector3Filter.Filter(LeftFoot.position);
        filteredLeftFootRotation = vector3Filter.Filter(LeftFoot.eulerAngles);
        filteredRightFootPosition = vector3Filter.Filter(RightFoot.position);
        filteredRightFootRotation = vector3Filter.Filter(RightFoot.eulerAngles);

        ////filteredWaistRotation = vector3Filter.Filter(HumanWaist.eulerAngles);


        if (CheckMarkerMoving(hullConstraintParent))
            GenerateTriangulation();

        //if (CheckHumanMoving())
        //    currentVis = SetUpDashBoardScale(); // returned 1, 2, or 3 vis

        if (CheckHumanFeetMoving("left") || DemoFlagLeft) {
            DemoFlagLeft = false;
            currentVisFromLeft = SetUpDashBoardScale(LeftFoot); // returned multiple vis from left foot
            if (CheckMatch(currentVisFromLeft, currentVisFromRight)) {
                SameTriReconfigureScale(currentVisFromLeft);
            }
                
            currentVis = CombineFeetVisAndAssignPosition(currentVisFromLeft, currentVisFromRight);
            if (currentVis.Count > 0)
                currentVisOnDashboard = RearrangeVisOnDashBoard(currentVis, currentVisOnDashboard);
        }

        if (CheckHumanFeetMoving("right") || DemoFlagRight)
        {
            DemoFlagRight = false;
            currentVisFromRight = SetUpDashBoardScale(RightFoot); // returned multiple vis from left foot
            if (CheckMatch(currentVisFromLeft, currentVisFromRight)) {
                SameTriReconfigureScale(currentVisFromRight);
            }
                
            currentVis = CombineFeetVisAndAssignPosition(currentVisFromLeft, currentVisFromRight);
            if (currentVis.Count > 0)
                currentVisOnDashboard = RearrangeVisOnDashBoard(currentVis, currentVisOnDashboard);
        }

        if (CheckHumanRotating())
        {
            currentVis = RearrangeDisplayBasedOnAngle(currentVis);
            currentVisOnDashboard = RearrangeVisOnDashBoard(currentVis, currentVisOnDashboard);
        }

        ////if (CheckHumanWaistRotating())
        ////{
        ////    currentVis = RearrangeDisplayBasedOnAngle(currentVis);
        ////    currentVisOnDashboard = RearrangeVisOnDashBoard(currentVis, currentVisOnDashboard);
        ////}

        // transition for positions and scale
        if (currentVisOnDashboard != null && currentVisOnDashboard.Count > 0)
        {
            foreach (Transform vis in currentVisOnDashboard.Values)
            {
                if (vis.localPosition != vis.GetComponent<Vis>().InAirPosition)
                    vis.localPosition = Vector3.Lerp(vis.localPosition, vis.GetComponent<Vis>().InAirPosition, Time.deltaTime * speed);
            }

            foreach (Transform vis in currentVisOnDashboard.Values)
            {
                if (vis.localScale != vis.GetComponent<Vis>().InAirScale)
                    vis.localScale = Vector3.Lerp(vis.localScale, vis.GetComponent<Vis>().InAirScale, Time.deltaTime * speed);
            }
        }

        //// pinned Vis part
        //if (currentPinnedOnDashboard != null && currentPinnedOnDashboard.Count > 0)
        //{
        //    float betweenVis = 0.1f;
        //    foreach (Transform t in currentPinnedOnDashboard.Values)
        //    {
        //        if (currentPinnedOnDashboard.Count == 1)
        //        {
        //            t.GetComponent<Vis>().InAirPosition = Vector3.zero;
        //        }
        //        else if (currentPinnedOnDashboard.Count == 2)
        //        {
        //            if (currentPinnedOnDashboard.Values.ToList().IndexOf(t) == 0)
        //                t.GetComponent<Vis>().InAirPosition = new Vector3((-t.GetComponent<Vis>().InAirScale.x / 2 - betweenVis) * 10, 0, 0);
        //            else
        //                t.GetComponent<Vis>().InAirPosition = new Vector3((t.GetComponent<Vis>().InAirScale.x / 2 + betweenVis) * 10, 0, 0);
        //        }
        //        else if (currentPinnedOnDashboard.Count == 3)
        //        {

        //            Vector3 middleOneScale = currentPinnedOnDashboard.Values.ToList()[1].GetComponent<Vis>().InAirScale;

        //            if (currentPinnedOnDashboard.Values.ToList().IndexOf(t) == 0)
        //                t.GetComponent<Vis>().InAirPosition = new Vector3((-(t.GetComponent<Vis>().InAirScale.x + middleOneScale.x) / 2 - betweenVis) * 10, 0, 0);
        //            else if (currentPinnedOnDashboard.Values.ToList().IndexOf(t) == 1)
        //                t.GetComponent<Vis>().InAirPosition = Vector3.zero;
        //            else if (currentPinnedOnDashboard.Values.ToList().IndexOf(t) == 2)
        //                t.GetComponent<Vis>().InAirPosition = new Vector3(((t.GetComponent<Vis>().InAirScale.x + middleOneScale.x) / 2 + betweenVis) * 10, 0, 0);
        //        }
        //    }
        //}

        if (Input.GetKeyDown("z"))
        {
            if (currentVisOnDashboard.Count > 0)
            {
                Transform pinnedVis = currentVisOnDashboard.Values.ToList()[0];
                GroundToPin(pinnedVis);
                currentVis.Remove(pinnedVis);
            }
        }
        //if (Input.GetKeyDown("x"))
        //{
        //    if (currentVisOnDashboard.Count > 1)
        //    {
        //        Transform pinnedVis = currentVisOnDashboard.Values.ToList()[1];
        //        GroundToPin(pinnedVis);
        //    }
        //}
        //if (Input.GetKeyDown("c"))
        //{
        //    if (currentVisOnDashboard.Count > 2)
        //    {
        //        Transform pinnedVis = currentVisOnDashboard.Values.ToList()[2];
        //        GroundToPin(pinnedVis);
        //    }
        //}

        //if (Input.GetKeyDown("b"))
        //{
        //    if (currentPinnedOnDashboard.Count > 0)
        //    {
        //        Transform pinnedVis = currentPinnedOnDashboard.Values.ToList()[0];
        //        PinToGround(pinnedVis);
        //    }
        //}
        //if (Input.GetKeyDown("n"))
        //{
        //    if (currentPinnedOnDashboard.Count > 1)
        //    {
        //        Transform pinnedVis = currentPinnedOnDashboard.Values.ToList()[1];
        //        PinToGround(pinnedVis);
        //    }
        //}
        //if (Input.GetKeyDown("m"))
        //{
        //    if (currentPinnedOnDashboard.Count > 2)
        //    {
        //        Transform pinnedVis = currentPinnedOnDashboard.Values.ToList()[2];
        //        PinToGround(pinnedVis);
        //    }
        //}

        //if (currentPinnedOnDashboard != null && currentPinnedOnDashboard.Count > 0)
        //{
        //    foreach (Transform vis in currentPinnedOnDashboard.Values)
        //    {
        //        if (vis.localPosition != vis.GetComponent<Vis>().InAirPosition)
        //            vis.localPosition = Vector3.Lerp(vis.localPosition, vis.GetComponent<Vis>().InAirPosition, Time.deltaTime * speed);
        //    }

        //    foreach (Transform vis in currentPinnedOnDashboard.Values)
        //    {
        //        if (vis.localScale != vis.GetComponent<Vis>().InAirScale)
        //            vis.localScale = Vector3.Lerp(vis.localScale, vis.GetComponent<Vis>().InAirScale, Time.deltaTime * speed);
        //    }
        //}

    }

    // combine two feet vis from SetUpDashBoardScale and assign position
    private List<Transform> CombineFeetVisAndAssignPosition(List<Transform> leftVis, List<Transform> rightVis) {

        if (leftVis.Count == 0 && rightVis.Count == 0)
        {
            // remove old vis
            foreach (Transform t in currentVisOnDashboard.Values.ToList())
            {
                if (hullConstraintParent.Find(t.name) != null)
                {
                    hullConstraintParent.Find(t.name).GetComponent<Vis>().OnDashBoard = false;
                    Destroy(currentVisOnDashboard[t.name].gameObject);
                    currentVisOnDashboard.Remove(t.name);
                }
            }
            return new List<Transform>();
        }
        else if (leftVis.Count == 0)
        {
            DashBoard.GetComponent<DashBoard>().ForwardParameter = 4;
            if (currentVisOnDashboard.Count > 0) {
                // remove old vis
                List<string> visNameFromRight = new List<string>();
                foreach (Transform t in rightVis)
                    visNameFromRight.Add(t.name);


                foreach (string s in currentVisOnDashboard.Keys.ToList())
                {
                    if (!visNameFromRight.Contains(s))
                    {
                        hullConstraintParent.Find(s).GetComponent<Vis>().OnDashBoard = false;
                        Destroy(currentVisOnDashboard[s].gameObject);
                        currentVisOnDashboard.Remove(s);
                    }
                }
            }

            SetUpDashBoardPosition(rightVis, 0);

            return rightVis;
        }
        else if (rightVis.Count == 0)
        {
            DashBoard.GetComponent<DashBoard>().ForwardParameter = 4;
            if (currentVisOnDashboard.Count > 0)
            {
                // remove old vis
                List<string> visNameFromLeft = new List<string>();
                foreach (Transform t in leftVis)
                    visNameFromLeft.Add(t.name);


                foreach (string s in currentVisOnDashboard.Keys.ToList())
                {
                    if (!visNameFromLeft.Contains(s) && hullConstraintParent.Find(s) != null)
                    {
                        hullConstraintParent.Find(s).GetComponent<Vis>().OnDashBoard = false;
                        Destroy(currentVisOnDashboard[s].gameObject);
                        currentVisOnDashboard.Remove(s);
                    }
                }
            }


            SetUpDashBoardPosition(leftVis, 0);

            return leftVis;
        }
        else {
            // setup dashboard forward para to make sure 4-6 vis on dashboard
            DashBoard.GetComponent<DashBoard>().ForwardParameter = 8;
            // final return list contains ground vis
            List<Transform> wholeListofVis = new List<Transform>();
            // list of vis after join
            List<Transform> jointListofVis = new List<Transform>();
            List<Transform> restLeftVis = new List<Transform>();
            List<Transform> restRightVis = new List<Transform>();

            float sumOfMiddleVisScaleX = 0;
            float sumOfLeftVisScaleX = 0;
            float sumOfRightVisScaleX = 0;

            foreach (Transform t in leftVis) {
                if (!rightVis.Contains(t))
                {
                    restLeftVis.Add(t);
                    sumOfLeftVisScaleX += t.GetComponent<Vis>().InAirScale.x + betweenVisDelta;
                    wholeListofVis.Add(t);
                }
                else {
                    jointListofVis.Add(t);
                    sumOfMiddleVisScaleX += t.GetComponent<Vis>().InAirScale.x + betweenVisDelta;
                    wholeListofVis.Add(t);
                }
            }

            foreach (Transform t in rightVis)
            {
                if (!leftVis.Contains(t)) {
                    restRightVis.Add(t);
                    sumOfRightVisScaleX += t.GetComponent<Vis>().InAirScale.x + betweenVisDelta;
                    wholeListofVis.Add(t);
                }
            }

            //if (sumOfMiddleVisScaleX != 0)
            //    sumOfMiddleVisScaleX -= betweenVisDelta;
            //if (sumOfLeftVisScaleX != 0)
            //    sumOfLeftVisScaleX -= betweenVisDelta;
            //if (sumOfRightVisScaleX != 0)
            //    sumOfRightVisScaleX -= betweenVisDelta;

            if (restLeftVis.Count == 0 && restRightVis.Count == 0)
                DashBoard.GetComponent<DashBoard>().ForwardParameter = 4;

            if (currentVisOnDashboard.Count > 0)
            {
                // remove old vis
                List<string> visName = new List<string>();
                foreach (Transform t in wholeListofVis)
                    visName.Add(t.name);


                foreach (string s in currentVisOnDashboard.Keys.ToList())
                {
                    if (!visName.Contains(s))
                    {
                        hullConstraintParent.Find(s).GetComponent<Vis>().OnDashBoard = false;
                        Destroy(currentVisOnDashboard[s].gameObject);
                        currentVisOnDashboard.Remove(s);
                    }
                }
            }

            // assign positions to joint vis
            jointListofVis = RearrangeDisplayBasedOnAngle(jointListofVis);

            SetUpDashBoardPosition(jointListofVis, 0);

            // assign positions to left & right vis
            SetUpDashBoardPosition(restLeftVis, - (((sumOfLeftVisScaleX + sumOfMiddleVisScaleX) / 2 + betweenVisDelta) * 10));
            SetUpDashBoardPosition(restRightVis, (((sumOfRightVisScaleX + sumOfMiddleVisScaleX) / 2 + betweenVisDelta) * 10));

            return wholeListofVis;
        }
    }

    private void SetUpDashBoardPosition(List<Transform> vis, float deltaX) {
        foreach (Transform t in vis)
        {
            if (vis.Count == 1)
            {
                t.GetComponent<Vis>().InAirPosition = Vector3.zero;
            }
            else if (vis.Count == 2)
            {
                if (vis.IndexOf(t) == 0)
                    t.GetComponent<Vis>().InAirPosition = new Vector3((-t.GetComponent<Vis>().InAirScale.x / 2 - betweenVisDelta) * 10, 0, 0);
                else
                    t.GetComponent<Vis>().InAirPosition = new Vector3((t.GetComponent<Vis>().InAirScale.x / 2 + betweenVisDelta) * 10, 0, 0);
            }
            else if (vis.Count == 3)
            {

                Vector3 middleOneScale = vis[1].GetComponent<Vis>().InAirScale;

                if (vis.IndexOf(t) == 0)
                    t.GetComponent<Vis>().InAirPosition = new Vector3((-(t.GetComponent<Vis>().InAirScale.x + middleOneScale.x) / 2 - betweenVisDelta) * 10, 0, 0);
                else if (vis.IndexOf(t) == 1)
                    t.GetComponent<Vis>().InAirPosition = Vector3.zero;
                else if (vis.IndexOf(t) == 2)
                    t.GetComponent<Vis>().InAirPosition = new Vector3(((t.GetComponent<Vis>().InAirScale.x + middleOneScale.x) / 2 + betweenVisDelta) * 10, 0, 0);
            }

            
            t.GetComponent<Vis>().InAirPosition = new Vector3(t.GetComponent<Vis>().InAirPosition.x + deltaX, 0, 0);
            if (currentVisOnDashboard.ContainsKey(t.name))
                currentVisOnDashboard[t.name].GetComponent<Vis>().CopyEntity(t.GetComponent<Vis>());
        }
    }


    private void SameTriReconfigureScale(List<Transform> vis) {
        GameObject go = new GameObject();
        go.transform.position = (LeftFoot.position + RightFoot.position) / 2;

        vis = SetUpDashBoardScale(go.transform);

        GameObject.Destroy(go);
    }


    // Tracking one foot to determine what to display, can return no more than 3 vis
    private List<Transform> SetUpDashBoardScale(Transform foot)
    {
        Dictionary<string, Transform> FootInMarkers = CheckFootInTriangles(foot);

        List<Transform> showOnDashboard = new List<Transform>();


        if (FootInMarkers.Count > 0)
        {
            foreach (Transform t in CheckDistanceToEdge(foot, FootInMarkers.Values.ToList()))
                showOnDashboard.Add(t);
        }

        if (showOnDashboard.Count > 0)
        {
            Dictionary<string, Transform> newVisDict = new Dictionary<string, Transform>();
            foreach (Transform t in showOnDashboard)
            {
                newVisDict.Add(t.name, t);
            }

            CheckSameVisOnDashboard(newVisDict, currentVisOnDashboard);

            if (showOnDashboard.Count > 1)
            {
                if (showOnDashboard.Count == 3)
                {
                    List<float> calculatedRatio = new List<float>();
                    for (int i = 0; i < 3; i++)
                    {
                        int s = (i - 1 < 0) ? 2 : i - 1;
                        int t = (i + 1 > 2) ? 0 : i + 1;

                        calculatedRatio.Add((1 / CalculateProportionalScale(foot, showOnDashboard[i],
                            showOnDashboard[s], showOnDashboard[t])));
                    }

                    for (int i = 0; i < 3; i++)
                        showOnDashboard[i].GetComponent<Vis>().InAirScale =
                            (calculatedRatio[i] / calculatedRatio.Sum()) * Vector3.one;
                }
                else // show 2 vis
                {
                    List<float> VisDistanceToFoot = new List<float>();
                    foreach (Transform t in showOnDashboard)
                        VisDistanceToFoot.Add((1 / Vector3.Distance(t.position, foot.position)));

                    for (int i = 0; i < 2; i++)
                        showOnDashboard[i].GetComponent<Vis>().InAirScale =
                            (VisDistanceToFoot[i] / VisDistanceToFoot.Sum()) * Vector3.one;
                }
            }
            else// show 1 vis
                showOnDashboard[0].GetComponent<Vis>().InAirScale = Vector3.one;

            showOnDashboard = RearrangeDisplayBasedOnAngle(showOnDashboard);
            return showOnDashboard;
        }
        else
            return new List<Transform>();
    }

    // Calculate In Air Scale
    private List<Transform> SetUpDashBoardScale() {
        Dictionary<string, Transform> InMarkers = CheckHumanInTriangles();

        if (InMarkers.Count > 0)
        {
            // check human-edge distance to decide to show 1, 2 or 3 vis
            List<Transform> showOnDashboard = CheckDistanceToEdge(InMarkers.Values.ToList());
            if (showOnDashboard != null)
            {
                Dictionary<string, Transform> newVisDict = new Dictionary<string, Transform>();
                foreach (Transform t in showOnDashboard)
                {
                    newVisDict.Add(t.name, t);
                }

                CheckSameVisOnDashboard(newVisDict, currentVisOnDashboard);

            
                if (showOnDashboard.Count > 1)
                {
                    if (showOnDashboard.Count == 3)
                    {
                        List<float> calculatedRatio = new List<float>();
                        for (int i = 0; i < 3; i++)
                        {
                            int s = (i - 1 < 0) ? 2 : i - 1;
                            int t = (i + 1 > 2) ? 0 : i + 1;

                            calculatedRatio.Add((1 / CalculateProportionalScale(showOnDashboard[i],
                                showOnDashboard[s], showOnDashboard[t])));
                        }

                        for (int i = 0; i < 3; i++)
                            showOnDashboard[i].GetComponent<Vis>().InAirScale =
                                (calculatedRatio[i] / calculatedRatio.Sum()) * Vector3.one;
                    }
                    else
                    {
                        List<float> VisDistanceToHuman = new List<float>();
                        foreach (Transform t in showOnDashboard)
                            VisDistanceToHuman.Add((1 / Vector3.Distance(t.position, Human.position)));

                        for (int i = 0; i < 2; i++)
                            showOnDashboard[i].GetComponent<Vis>().InAirScale =
                                (VisDistanceToHuman[i] / VisDistanceToHuman.Sum()) * Vector3.one;
                    }
                }
                else// show 1 vis
                    showOnDashboard[0].GetComponent<Vis>().InAirScale = Vector3.one;

                showOnDashboard = RearrangeDisplayBasedOnAngle(showOnDashboard);
                return showOnDashboard;
            }
            else
                return null;
        }
        else {
            // delete old vis
            foreach (Transform t in currentVisOnDashboard.Values.ToList())
            {
                if (hullConstraintParent.Find(t.name) != null) {
                    hullConstraintParent.Find(t.name).GetComponent<Vis>().OnDashBoard = false;
                    Destroy(currentVisOnDashboard[t.name].gameObject);
                    currentVisOnDashboard.Remove(t.name);
                }
                
            }
            return null;
        }
            
    }

    private void CheckSameVisOnDashboard(Dictionary<string, Transform> newVis, Dictionary<string, Transform> oldVis) {
        if (oldVis.Count == 0) {
            foreach (Transform t in newVis.Values.ToList())
            {
                t.GetComponent<Vis>().OnDashBoard = true;
                GameObject visOnDashBoard = Instantiate(t.gameObject, DashBoard);
                visOnDashBoard.transform.position = t.position;
                visOnDashBoard.transform.localEulerAngles = Vector3.zero;
                visOnDashBoard.transform.localScale = Vector3.one * 0.1f;
                visOnDashBoard.name = t.name;

                SpriteRenderer sr = visOnDashBoard.GetComponent<SpriteRenderer>();
                sr.sortingOrder = 0;

                currentVisOnDashboard.Add(t.name, visOnDashBoard.transform);
            }
        }
        else {
            // add new vis
            foreach (Transform t in newVis.Values.ToList())
            {
                if (oldVis.Keys.Contains(t.name))
                {
                    oldVis[t.name].GetComponent<Vis>().CopyEntity(t.GetComponent<Vis>());
                }
                else
                {
                    t.GetComponent<Vis>().OnDashBoard = true;
                    GameObject visOnDashBoard = Instantiate(t.gameObject, DashBoard);
                    visOnDashBoard.transform.position = t.position;
                    visOnDashBoard.transform.localEulerAngles = Vector3.zero;
                    visOnDashBoard.transform.localScale = Vector3.one * 0.1f;
                    visOnDashBoard.name = t.name;

                    //SpriteRenderer sr = visOnDashBoard.GetComponent<SpriteRenderer>();
                    //sr.sortingOrder = 0;

                    currentVisOnDashboard.Add(t.name, visOnDashBoard.transform);
                }
            }

            //// delete old vis
            //foreach (Transform t in currentVisOnDashboard.Values.ToList())
            //{
            //    if (!newVis.Keys.Contains(t.name))
            //    {
            //        if (hullConstraintParent.Find(t.name) != null)
            //        {
            //            hullConstraintParent.Find(t.name).GetComponent<Vis>().OnDashBoard = false;
            //            Destroy(currentVisOnDashboard[t.name].gameObject);
            //            currentVisOnDashboard.Remove(t.name);
            //        }
            //    }
            //}
        }
    }

    private float CalculateProportionalScale(Transform foot, Transform targetVis, Transform prevVis, Transform nextVis)
    {
        float footToTarget = Vector3.Distance(foot.position, targetVis.position);
        float leftEdgeLength = Vector3.Distance(targetVis.position, prevVis.position);
        float rightEdgeLength = Vector3.Distance(targetVis.position, nextVis.position);

        return 2 * footToTarget / (leftEdgeLength + rightEdgeLength);
    }

    private float CalculateProportionalScale(Transform targetVis, Transform prevVis, Transform nextVis) {
        float humanToTarget = Vector3.Distance(Human.position, targetVis.position);
        float leftEdgeLength = Vector3.Distance(targetVis.position, prevVis.position);
        float rightEdgeLength = Vector3.Distance(targetVis.position, nextVis.position);

        return 2 * humanToTarget / (leftEdgeLength + rightEdgeLength);
    }

    // TODO: change human to waist
    private List<Transform> RearrangeDisplayBasedOnAngle(List<Transform> markers)
    {
        List<Transform> finalList = new List<Transform>();
        if (markers != null && markers.Count > 0) {
            Dictionary<Transform, float> markerAnglesToHuman = new Dictionary<Transform, float>();

            foreach (Transform t in markers)
                markerAnglesToHuman.Add(t, Vector3.SignedAngle(Human.forward, t.position - Human.position, Vector3.up));

            foreach (KeyValuePair<Transform, float> item in markerAnglesToHuman.OrderBy(key => key.Value))
                finalList.Add(item.Key);
        }
        return finalList;
    }

    private Dictionary<string, Transform> RearrangeVisOnDashBoard(List<Transform> newOrderedList, Dictionary<string, Transform> oldDict) {
        Dictionary<string, Transform> orderedDict = new Dictionary<string, Transform>();

        foreach (Transform t in newOrderedList) {
            if(oldDict.ContainsKey(t.name))
                orderedDict.Add(t.name, oldDict[t.name]);
        }

        return orderedDict;
    }

    // true if in the middle of triangle and show 3 vis, false if near edge and show 2 vis
    private List<Transform> CheckDistanceToEdge(List<Transform> markers) {
        Vector3 Human2DPosition = new Vector3(Human.position.x, 0, Human.position.z);
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

    private Dictionary<string, Transform> CheckHumanInTriangles()
    {
        Dictionary<string, Transform> ValidMarkers = new Dictionary<string, Transform>();

        foreach (HalfEdgeFace2 face in CurrentTriangles.faces)
        {
            Vector3 a = face.edge.v.position.ToVector3();
            Vector3 b = face.edge.nextEdge.v.position.ToVector3();
            Vector3 c = face.edge.prevEdge.v.position.ToVector3();
            if (PointInTriangle(Human.position, a, b, c))
            {
                foreach (Transform t in hullConstraintParent)
                {
                    if (Vector3.Distance(t.position, a) < 0.1f ||
                        Vector3.Distance(t.position, b) < 0.1f ||
                        Vector3.Distance(t.position, c) < 0.1f) {
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
                
                foreach (Transform t in hullConstraintParent)
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

    // check if headset is rotating
    private bool CheckHumanRotating() {
        Vector3 currentRotation = filteredHumanRotation;
        if (currentRotation == previousHumanRotation)
            return false;
        previousHumanRotation = currentRotation;
        return true;
    }

    ////// BODY-TRACKING: check if waist is rotating
    ////private bool CheckHumanWaistRotating() {
    ////    Vector3 currentWaistRotation = filteredWaistRotation;
    ////    if (currentWaistRotation == previousHumanWaistRotation)
    ////        return false;
    ////    previousHumanWaistRotation = currentWaistRotation;
    ////    return true;
    ////}

    // check if headset is moving
    private bool CheckHumanMoving()
    {
        Vector3 currentPosition = filteredHumanPosition;
        //Vector3 currentPosition = Human.position;
        if (currentPosition == previousHumanPosition)
            return false;
        previousHumanPosition = currentPosition;
        return true;
    }

    // BODY-TRACKING: check if any foot is moving
    private bool CheckHumanFeetMoving(string foot) {
        if (foot == "left") {
            Vector3 currentLeftPosition = filteredLeftFootPosition;
            if (currentLeftPosition == previousLeftFootPosition)
                return false;
            previousLeftFootPosition = currentLeftPosition;
        }

        if (foot == "right") {
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
        if (parent.childCount != previousGroundMarkerNumber) {
            previousGroundMarkerNumber = parent.childCount;
            return true;
        }

        foreach (Transform t in hullConstraintParent) {
            if (t.position != t.GetComponent<Vis>().GroundPosition) {
                t.GetComponent<Vis>().GroundPosition = t.position;
                return true;
            }
        }

        return false;
    }

    private void DisplayTriangleEdges()
    {
        if (Application.isPlaying) {
            if (EdgeParent.childCount > 0)
            {
                foreach (Transform t in EdgeParent)
                    Destroy(t.gameObject);
            }
        }
        if (Application.isEditor) {
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

    private void PinToGround(Transform t)
    {
        GameObject visOnGround = Instantiate(t.gameObject, hullConstraintParent);
        visOnGround.transform.position = new Vector3(Human.position.x, 0, Human.position.z);
        visOnGround.GetComponent<Vis>().GroundPosition = visOnGround.transform.position;
        visOnGround.transform.localEulerAngles = new Vector3(90, 0, 0);
        visOnGround.transform.localScale = t.GetComponent<Vis>().GroundScale;
        visOnGround.GetComponent<Vis>().PinOnDashBoard = false;
        visOnGround.name = t.name;

        currentPinnedOnDashboard.Remove(t.name);
        Destroy(t.gameObject);
    }

    private void GroundToPin(Transform t)
    {
        Transform groundOriginal = hullConstraintParent.Find(t.name);
        if (groundOriginal != null)
        {
            Destroy(groundOriginal.gameObject);
            t.SetParent(PinnedDashBoard);
            currentPinnedOnDashboard.Add(t.name, t);
            t.GetComponent<Vis>().PinOnDashBoard = true;
            t.GetComponent<Vis>().InAirScale = Vector3.one * 0.33f;
        }
    }

    /// <summary>
    /// Below codes are from the library, minor changes such as edges
    /// </summary>
    public void GenerateTriangulation()
    {
        // edges customisation
        if (currentEdges!= null && currentEdges.Count > 0 && EdgeParent.childCount > 0)
        {

            foreach (Transform t in currentEdges)
                DestroyImmediate(t.gameObject);
            currentEdges.Clear();
        }
        currentEdges = new List<Transform>();
        //Get the random points
        //HashSet<Vector2> randomPoints = TestAlgorithmsHelpMethods.GenerateRandomPoints2D(seed, halfMapSize, numberOfPoints);

        //To MyVector2
        //HashSet<MyVector2> randomPoints_2d = new HashSet<MyVector2>(randomPoints.Select(x => x.ToMyVector2()));

        /*
        List<MyVector2> constraints_2d = constraints.Select(x => x.ToMyVector2()).ToList();

        //Normalize to range 0-1
        //We should use all points, including the constraints because the hole may be outside of the random points
        List<MyVector2> allPoints = new List<MyVector2>();

        allPoints.AddRange(new List<MyVector2>(points_2d));
        allPoints.AddRange(constraints_2d);

        AABB2 normalizingBox = new AABB2(new List<MyVector2>(points_2d));

        float dMax = HelpMethods.CalculateDMax(normalizingBox);

        HashSet<MyVector2> points_2d_normalized = HelpMethods.Normalize(points_2d, normalizingBox, dMax);

        List<MyVector2> constraints_2d_normalized = HelpMethods.Normalize(constraints_2d, normalizingBox, dMax);
        */


        //Hull
        List<Vector3> hullPoints = TestAlgorithmsHelpMethods.GetPointsFromParent(hullConstraintParent);

        List<MyVector2> hullPoints_2d = hullPoints.Select(x => x.ToMyVector2()).ToList(); ;

        //Holes
        HashSet<List<MyVector2>> allHolePoints_2d = new HashSet<List<MyVector2>>();

        foreach (Transform holeParent in holeConstraintParents)
        {
            List<Vector3> holePoints = TestAlgorithmsHelpMethods.GetPointsFromParent(holeParent);

            if (holePoints != null)
            {
                List<MyVector2> holePoints_2d = holePoints.Select(x => x.ToMyVector2()).ToList();

                allHolePoints_2d.Add(holePoints_2d);
            }
        }


        //Normalize to range 0-1
        //We should use all points, including the constraints because the hole may be outside of the random points
        List<MyVector2> allPoints = new List<MyVector2>();

        //allPoints.AddRange(randomPoints_2d);

        allPoints.AddRange(hullPoints_2d);

        foreach (List<MyVector2> hole in allHolePoints_2d)
        {
            allPoints.AddRange(hole);
        }

        Normalizer2 normalizer = new Normalizer2(allPoints);

        List<MyVector2> hullPoints_2d_normalized = normalizer.Normalize(hullPoints_2d);

        HashSet<List<MyVector2>> allHolePoints_2d_normalized = new HashSet<List<MyVector2>>();

        foreach (List<MyVector2> hole in allHolePoints_2d)
        {
            List<MyVector2> hole_normalized = normalizer.Normalize(hole);

            allHolePoints_2d_normalized.Add(hole_normalized);
        }



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

        if (Application.isPlaying) {
            if (ShowEdge)
                DisplayTriangleEdges();

            if (triangulatedMesh != null && ShowColor)
            {
                DisplayMeshWithRandomColors(triangulatedMesh, seed);
            }
        }
        //currentVis = SetUpDashBoardScale();
    }


    //private void OnDrawGizmos()
    //{
    //    if (triangulatedMesh != null && ShowColor)
    //    {
    //        TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors(triangulatedMesh, seed);
    //    }


    //    ////Display the obstacles
    //    //if (constraints != null)
    //    //{
    //    //    //DebugResults.DisplayConnectedPoints(obstacle, Color.black);
    //    //}


    //    //////Display drag constraints
    //    //DisplayDragConstraints();
    //}

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


                ////Color the triangle
                Color newColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.2f);
                //Gizmos.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.5f);

                ////float grayScale = Random.Range(0f, 1f);

                ////Gizmos.color = new Color(grayScale, grayScale, grayScale, 1f);


                ////Display it
                GameObject go = Instantiate(colorFillPrefab, EdgeParent);
                go.GetComponent<MeshFilter>().mesh = triangleMesh;
                go.GetComponent<MeshRenderer>().material.color = newColor;
                //Gizmos.DrawMesh(triangleMesh);
            }
        }
    }



    private void DisplayDragConstraints()
    {
        if (hullConstraintParent != null)
        {
            List<Vector3> points = TestAlgorithmsHelpMethods.GetPointsFromParent(hullConstraintParent);

            TestAlgorithmsHelpMethods.DisplayConnectedPoints(points, Color.white, true);
        }

        //if (holeConstraintParents != null)
        //{
        //    foreach (Transform holeParent in holeConstraintParents)
        //    {
        //        List<Vector3> points = TestAlgorithmsHelpMethods.GetPointsFromParent(holeParent);

        //        TestAlgorithmsHelpMethods.DisplayConnectedPoints(points, Color.white, true);
        //    }
        //}
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

    private bool CheckMatch(List<Transform> LeftVis, List<Transform> RightVis)
    {
        if (LeftVis.Count != RightVis.Count)
            return false;
        List<string> restLeftVisNames = new List<string>();
        List<string> restRightVisNames = new List<string>();

        List<string> LeftVisNames = new List<string>();
        List<string> RightVisNames = new List<string>();

        foreach (Transform t in LeftVis)
        {
            LeftVisNames.Add(t.name);
        }

        foreach (Transform t in RightVis)
        {
            RightVisNames.Add(t.name);
        }

        foreach (string s in LeftVisNames)
        {
            if (!RightVisNames.Contains(s))
                restLeftVisNames.Add(s);
        }

        foreach (string s in RightVisNames)
        {
            if (!LeftVisNames.Contains(s))
                restRightVisNames.Add(s);
        }

        if (restLeftVisNames.Count == 0 && restRightVisNames.Count == 0)
            return true;
        else
            return false;
    }
}