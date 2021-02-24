using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DataManagerFloorMenu_Map : MonoBehaviour
{
    public TextAsset DataSource;
    public GameObject markPrefab;
    public GameObject groundMarkPrefab;
    public Transform visParent;
    public Transform hiddenVisParent;
    public Transform groundMarkParent;
    public Transform floorMenuParent;
    public MagicCarpetManager_FloorMenu mcm;
    public Transform LeftFoot;
    public Transform RightFoot;

    [Header("Variables")]
    public int dataPointLimit = 0;
    public float markSize = 0.1f;
    public float mapSize = 4f;
    public float speed = 1;
    public int facetedRows = 1;
    public int facetedColumns = 1;
    public float forwardParameter = 10;

    private List<GameObject> MarkCollection;
    private List<GameObject> CurrentSM;
    //private List<GameObject> CurrentMarkCollection; // filtering
    private List<Transform> MapLocations;
    private List<Housing> PropertyCollection;
    private List<Vector3> MapInAirPositions;
    private List<Transform> FloorButtonCollection;

    // one euro filter
    private Vector3 filteredLeftFootPosition;
    private Vector3 filteredRightFootPosition;
    private Vector3 filteredLeftFootRotation;
    private Vector3 filteredRightFootRotation;
    private OneEuroFilter<Vector3> vector3Filter;

    private Vector3 previousLeftFootPosition;
    private Vector3 previousLeftFootRotation;
    private Vector3 previousRightFootPosition;
    private Vector3 previousRightFootRotation;
    private string previousClosestRegion = "";

    private bool canMove = false;
    private bool showCase = true;
    private bool DemoFlag = true;

    private int smallMultiplesNumber;

    private readonly char lineSeperater = '\n'; // It defines line seperate character
    private readonly char fieldSeperator = ','; // It defines field seperate chracter

    private float CurrentMinSelectedValue = 100000;
    private float CurrentMaxSelectedValue = -100000;

    // Start is called before the first frame update
    void Start()
    {
        MarkCollection = new List<GameObject>();
        CurrentSM = new List<GameObject>();
        //CurrentMarkCollection = new List<GameObject>();
        PropertyCollection = new List<Housing>();
        MapLocations = new List<Transform>();
        FloorButtonCollection = new List<Transform>();
        MapInAirPositions = new List<Vector3>();
        vector3Filter = new OneEuroFilter<Vector3>(120f);

        ReadData(DataSource);

        //ShowCaseScenario(PropertyCollection);

        ShowButton();
        ShowMap();
        
    }

    private void ReadData(TextAsset ta)
    {

        string[] lines = ta.text.Split(lineSeperater);
        int dataLength = 1;

        if (dataPointLimit == 0)
            dataLength = lines.Length;
        else
            dataLength = dataPointLimit;

        for (int i = 1; i < dataLength; i++)
        {

            if (lines[i].Length > 10)
            {
                //GameObject mark = Instantiate(markPrefab, new Vector3(0, 0, 0), Quaternion.identity, visParent);
                //mark.transform.localScale = Vector3.one * markSize;

                Housing property = new Housing(i, lines[i].Split(fieldSeperator)[0], lines[i].Split(fieldSeperator)[1],
                int.Parse(lines[i].Split(fieldSeperator)[2]), lines[i].Split(fieldSeperator)[3],
                lines[i].Split(fieldSeperator)[4], DateTime.Parse(lines[i].Split(fieldSeperator)[5]),
                int.Parse(lines[i].Split(fieldSeperator)[6]), int.Parse(lines[i].Split(fieldSeperator)[7]),
                int.Parse(lines[i].Split(fieldSeperator)[8]), int.Parse(lines[i].Split(fieldSeperator)[9]),
                int.Parse(lines[i].Split(fieldSeperator)[10]), lines[i].Split(fieldSeperator)[11], float.Parse(lines[i].Split(fieldSeperator)[12]),
                float.Parse(lines[i].Split(fieldSeperator)[13]), lines[i].Split(fieldSeperator)[14]);

                //mark.GetComponent<Housing>().CopyEntity(property);
                //MarkCollection.Add(mark);
                PropertyCollection.Add(property);
            }

        }
    }

    private void Update()
    {
        filteredLeftFootPosition = vector3Filter.Filter(LeftFoot.position);
        filteredLeftFootRotation = vector3Filter.Filter(LeftFoot.eulerAngles);
        filteredRightFootPosition = vector3Filter.Filter(RightFoot.position);
        filteredRightFootRotation = vector3Filter.Filter(RightFoot.eulerAngles);

        if (CheckHumanFeetMoving("left") || CheckHumanFeetMoving("right") || DemoFlag)
        {
            FindActiveButton();
            DemoFlag = false;
            canMove = true;
        }

        

        if (canMove)
        {
            foreach (GameObject mark in MarkCollection)
            {
                Housing h = mark.GetComponent<Housing>();
                if (CurrentMinSelectedValue != 100000 && CurrentMaxSelectedValue != -100000)
                {
                    if (h.YearBuilt < CurrentMaxSelectedValue && h.YearBuilt > CurrentMinSelectedValue)
                    {
                        h.Air = true;
                        h.Ground = false;
                    }
                    else
                    {
                        h.Air = false;
                        h.Ground = true;
                    }
                }
                else if(OutOfMenu())
                {
                    h.Air = true;
                    h.Ground = false;
                }
            }

            bool allMoved = true;
            foreach (GameObject mark in MarkCollection)
            {
                Housing h = mark.GetComponent<Housing>();

                if (h.Air) {
                    mark.transform.position = Vector3.Lerp(mark.transform.position,
                                        new Vector3(h.InAirXPosition, h.InAirYPosition, h.InAirZPosition), Time.deltaTime * speed);

                    if (Vector3.Distance(mark.transform.localPosition,
                            new Vector3(h.InAirXPosition, h.InAirYPosition, h.InAirZPosition)) > 0.01f)
                    {
                        allMoved = false;
                    }
                }


                if (h.Ground) {
                    mark.transform.position = Vector3.Lerp(mark.transform.position,
                                        new Vector3(h.GroundXPosition, h.GroundYPosition, h.GroundZPosition), Time.deltaTime * speed);

                    if (Vector3.Distance(mark.transform.localPosition,
                            new Vector3(h.GroundXPosition, h.GroundYPosition, h.GroundZPosition)) > 0.01f)
                    {
                        allMoved = false;
                    }
                }

                if ((h.Air && h.Ground) || (!h.Air && !h.Ground)) {
                    Debug.Log("error");
                }
            }

            if (allMoved)
            {
                canMove = false;
            }
        }
    }

    private void ShowMap()
    {
        float minLat = GetLowestLat(PropertyCollection);
        float maxLat = GetHighestLat(PropertyCollection);
        float minLong = GetLowestLong(PropertyCollection);
        float maxLong = GetHighestLong(PropertyCollection);

        float yMultiplier = mapSize;
        float xMultiplier = (maxLat - minLat) / (maxLong - minLong) * yMultiplier;

        foreach (Housing h in PropertyCollection)
        {
            float xPosition = yMultiplier * (h.Longtitude - minLong) / (maxLong - minLong) - 1;
            float yPosition = xMultiplier * (h.Latitude - minLat) / (maxLat - minLat) - 1;

            MapInAirPositions.Add(new Vector3(xPosition, yPosition, 0));
            h.InAirXPosition = xPosition - 3;
            h.InAirYPosition = yPosition;
            h.InAirZPosition = 3;

            foreach (Transform button in FloorButtonCollection) {
                float minValue = button.GetComponent<FloorMenu>().MinValue;
                float maxValue = button.GetComponent<FloorMenu>().MaxValue;
                if (h.YearBuilt < maxValue && h.YearBuilt > minValue) {
                    h.GroundXPosition = button.position.x;
                    h.GroundYPosition = button.position.y;
                    h.GroundZPosition = button.position.z;
                }
            }
            h.Ground = true;
            h.Air = false;

            GameObject mark = Instantiate(groundMarkPrefab, Vector3.zero,
            Quaternion.identity, groundMarkParent);

            mark.transform.position = new Vector3(h.GroundXPosition, h.GroundYPosition, h.GroundZPosition);

            mark.transform.localScale = Vector3.one * markSize;

            Color newCol = Color.black;
            switch (h.RegionName)
            {
                case "Eastern Metropolitan":
                    ColorUtility.TryParseHtmlString("#e41a1c", out newCol);
                    h.MarkColor = newCol;
                    break;
                case "Eastern Victoria":
                    ColorUtility.TryParseHtmlString("#377eb8", out newCol);
                    h.MarkColor = newCol;
                    break;
                case "Northern Metropolitan":
                    ColorUtility.TryParseHtmlString("#4daf4a", out newCol);
                    h.MarkColor = newCol;
                    break;
                case "Northern Victoria":
                    ColorUtility.TryParseHtmlString("#984ea3", out newCol);
                    h.MarkColor = newCol;
                    break;
                case "South-Eastern Metropolitan":
                    ColorUtility.TryParseHtmlString("#ff7f00", out newCol);
                    h.MarkColor = newCol;
                    break;
                case "Southern Metropolitan":
                    ColorUtility.TryParseHtmlString("#ffff33", out newCol);
                    h.MarkColor = newCol;
                    break;
                case "Western Metropolitan":
                    ColorUtility.TryParseHtmlString("#a65628", out newCol);
                    h.MarkColor = newCol;
                    break;
                case "Western Victoria":
                    ColorUtility.TryParseHtmlString("#f781bf", out newCol);
                    h.MarkColor = newCol;
                    break;
                default:
                    break;
            }
            mark.GetComponent<MeshRenderer>().material.color = h.MarkColor;
            mark.GetComponent<Housing>().CopyEntity(h);
            MapLocations.Add(mark.transform);
            MarkCollection.Add(mark);
        }
    }

    private void ShowButton()
    {
        int minYear = GetLowestYear(PropertyCollection);
        int maxYear = GetHighestYear(PropertyCollection);
        int yearDelta = 0;
        if ((maxYear - minYear) % 16 == 0)
            yearDelta = (maxYear - minYear) / 16;
        else
            yearDelta = (maxYear - minYear) / 16 + 1;

        for (int i = 0; i < 16; i++)
        {
            string min = (minYear + i * yearDelta) + "";
            string max = (minYear + (i + 1) * yearDelta - 1) + "";
            floorMenuParent.GetChild(i).GetChild(0).GetChild(0).GetComponent<Text>().text = min + " - " + max;
            floorMenuParent.GetChild(i).GetComponent<FloorMenu>().MinValue = minYear + i * yearDelta;
            floorMenuParent.GetChild(i).GetComponent<FloorMenu>().MaxValue = minYear + (i + 1) * yearDelta - 1;
            FloorButtonCollection.Add(floorMenuParent.GetChild(i));
        }
    }

    private void FindActiveButton()
    {
        Transform leftFootOnButton = null;
        Transform rightFootOnButton = null;

        CurrentMinSelectedValue = 100000;
        CurrentMaxSelectedValue = -100000;

        foreach (Transform button in FloorButtonCollection)
        {
            if (Vector3.Distance(button.position, LeftFoot.position) <= 0.15f)
                leftFootOnButton = button;

            if (Vector3.Distance(button.position, RightFoot.position) <= 0.15f)
                rightFootOnButton = button;
        }
        if (leftFootOnButton != null)
        {
            if (leftFootOnButton.GetComponent<FloorMenu>().MinValue < CurrentMinSelectedValue)
                CurrentMinSelectedValue = leftFootOnButton.GetComponent<FloorMenu>().MinValue;
            if (leftFootOnButton.GetComponent<FloorMenu>().MaxValue > CurrentMaxSelectedValue)
                CurrentMaxSelectedValue = leftFootOnButton.GetComponent<FloorMenu>().MaxValue;
        }

        if (rightFootOnButton != null)
        {
            if (rightFootOnButton.GetComponent<FloorMenu>().MinValue < CurrentMinSelectedValue)
                CurrentMinSelectedValue = rightFootOnButton.GetComponent<FloorMenu>().MinValue;
            if (rightFootOnButton.GetComponent<FloorMenu>().MaxValue > CurrentMaxSelectedValue)
                CurrentMaxSelectedValue = rightFootOnButton.GetComponent<FloorMenu>().MaxValue;
        }
    }

    //private void UpdateFacetingVariable(int newRow, int newColumn)
    //{
    //    if (newRow > 0 && newColumn > 0)
    //    {
    //        facetedColumns = newColumn;
    //        facetedRows = newRow;

    //        //ShowCaseScenario(PropertyCollection);
    //        UpdateVisGridTransform();
    //    }
    //}

    //private void ShowCaseScenario(List<Housing> privatePC)
    //{
    //    showCase = true;

    //    if (facetedRows > 0 && facetedColumns > 0)
    //        smallMultiplesNumber = facetedRows * facetedColumns;
    //    else
    //        smallMultiplesNumber = 1;
    //    CurrentSM = og.UpdateSM(CurrentSM, facetedColumns, facetedRows);
    //    mcm.UpdateCurrentSM(CurrentSM);

    //    // position channel
    //    int minBed = GetLowestBedroom(privatePC);
    //    int maxBed = GetHighestBedroom(privatePC);
    //    int minBath = GetLowestBathroom(privatePC);
    //    int maxBath = GetHighestBathroom(privatePC);
    //    int minPrice = GetLowestPrice(privatePC);
    //    int maxPrice = GetHighestPrice(privatePC);

    //    // faceting channel
    //    int minYear = GetLowestYear(privatePC);
    //    int maxYear = GetHighestYear(privatePC) + 1;

    //    for (int i = 0; i < MarkCollection.Count; i++) {
    //        GameObject mark = MarkCollection[i];
    //        Housing h = mark.GetComponent<Housing>();

    //        if (h.Show)
    //        {
    //            // setup position channel
    //            h.XPosition = (float)(h.Bedroom - minBed) / (maxBed - minBed);
    //            h.YPosition = (float)(h.Price - minPrice) / (maxPrice - minPrice);
    //            h.ZPosition = (float)(h.Bathroom - minBath) / (maxBath - minBath);

    //            int facetDelta = ((maxYear - minYear) / smallMultiplesNumber) + 1;
    //            // setup small multiples
    //            for (int j = 0; j < smallMultiplesNumber; j++)
    //            {
    //                if (h.YearBuilt >= minYear + (facetDelta * j) && h.YearBuilt < minYear + (facetDelta * (j + 1)))
    //                {
    //                    mark.transform.SetParent(CurrentSM[j].transform);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            // setup position channel
    //            h.XPosition = MapInAirPositions[i].x;
    //            h.YPosition = MapInAirPositions[i].y;
    //            h.ZPosition = MapInAirPositions[i].z;

    //            Color newCol = Color.black;
    //            switch (h.RegionName)
    //            {
    //                case "Eastern Metropolitan":
    //                    ColorUtility.TryParseHtmlString("#e41a1c", out newCol);
    //                    h.MarkColor = newCol;
    //                    break;
    //                case "Eastern Victoria":
    //                    ColorUtility.TryParseHtmlString("#377eb8", out newCol);
    //                    h.MarkColor = newCol;
    //                    break;
    //                case "Northern Metropolitan":
    //                    ColorUtility.TryParseHtmlString("#4daf4a", out newCol);
    //                    h.MarkColor = newCol;
    //                    break;
    //                case "Northern Victoria":
    //                    ColorUtility.TryParseHtmlString("#984ea3", out newCol);
    //                    h.MarkColor = newCol;
    //                    break;
    //                case "South-Eastern Metropolitan":
    //                    ColorUtility.TryParseHtmlString("#ff7f00", out newCol);
    //                    h.MarkColor = newCol;
    //                    break;
    //                case "Southern Metropolitan":
    //                    ColorUtility.TryParseHtmlString("#ffff33", out newCol);
    //                    h.MarkColor = newCol;
    //                    break;
    //                case "Western Metropolitan":
    //                    ColorUtility.TryParseHtmlString("#a65628", out newCol);
    //                    h.MarkColor = newCol;
    //                    break;
    //                case "Western Victoria":
    //                    ColorUtility.TryParseHtmlString("#f781bf", out newCol);
    //                    h.MarkColor = newCol;
    //                    break;
    //                default:
    //                    break;
    //            }
    //            mark.GetComponent<MeshRenderer>().material.color = h.MarkColor;

    //            mark.transform.SetParent(groundMarkParent);
    //        }
    //    }

    //    if (privatePC.Count == 0)
    //    {
    //        if (visParent.childCount > 0) {
    //            foreach (Transform t in visParent)
    //                Destroy(t.gameObject);
    //        }
    //    }

    //    canMove = true;
    //}



    //private void FindNearestPoint()
    //{
    //    float minDistance = 1;
    //    Transform closestPoint = null;
    //    int closestPointIndex = -1;

    //    for (int i = 0; i < MapInAirPositions.Count; i++) {
    //        if (Vector3.Distance(MapInAirPositions[i], mcm.human.transform.position) < minDistance)
    //        {
    //            minDistance = Vector3.Distance(MapInAirPositions[i], mcm.human.transform.position);
    //            closestPointIndex = i;
    //        }
    //    }

    //    if (closestPointIndex != -1)
    //    {
    //        closestPoint = MarkCollection[closestPointIndex].transform;
    //    }

    //    string newClosestRegion = "";
    //    if (closestPoint != null)
    //        newClosestRegion = closestPoint.GetComponent<Housing>().RegionName;
    //    else
    //        Debug.Log("Out of Map");

    //    Debug.Log(newClosestRegion);

    //    if (newClosestRegion != previousClosestRegion) {
    //        List<Housing> newPC = new List<Housing>();

    //        foreach (GameObject go in MarkCollection)
    //        {
    //            if (go.GetComponent<Housing>().RegionName == newClosestRegion)
    //            {
    //                go.GetComponent<Housing>().Show = true;
    //                newPC.Add(go.GetComponent<Housing>());
    //            }
    //            else
    //                go.GetComponent<Housing>().Show = false;
    //        }

    //        previousClosestRegion = newClosestRegion;
    //        ShowCaseScenario(newPC);
    //    }
    //}



    //private void UpdateVisGridTransform()
    //{
    //    //visParent.position = humanBody.TransformPoint(humanBody.localPosition + Vector3.forward * forwardParameter);
    //    visParent.position = Vector3.Lerp(visParent.position, new Vector3(humanBody.TransformPoint(humanBody.localPosition + Vector3.forward * forwardParameter).x, 
    //        og.AdjustedHeight, humanBody.TransformPoint(humanBody.localPosition + Vector3.forward * forwardParameter).z), Time.deltaTime * speed);

    //    visParent.LookAt(humanBody);
    //    visParent.localEulerAngles = new Vector3(0, visParent.localEulerAngles.y + 180, 0);
    //}

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

    private bool OutOfMenu() {
        if ((LeftFoot.position.x < -0.15f || LeftFoot.position.z < -1.2f || LeftFoot.position.x > 1.2 || LeftFoot.position.z > 0.15) &&
            (RightFoot.position.x < -0.15f || RightFoot.position.z < -1.2f || RightFoot.position.x > 1.2 || RightFoot.position.z > 0.15)) {
            return true;
        }
        return false;
    }


    public int GetHighestBedroom(List<Housing> list)
    {
        return list.Count > 0 ? list.Max(t => t.Bedroom) : 0; //could also return -1
    }

    public int GetLowestBedroom(List<Housing> list)
    {
        return list.Count > 0 ? list.Min(t => t.Bedroom) : 0; //could also return -1
    }

    public int GetHighestBathroom(List<Housing> list)
    {
        return list.Count > 0 ? list.Max(t => t.Bathroom) : 0; //could also return -1
    }

    public int GetLowestBathroom(List<Housing> list)
    {
        return list.Count > 0 ? list.Min(t => t.Bathroom) : 0; //could also return -1
    }

    public int GetHighestCar(List<Housing> list)
    {
        return list.Count > 0 ? list.Max(t => t.Car) : 0; //could also return -1
    }

    public int GetLowestCar(List<Housing> list)
    {
        return list.Count > 0 ? list.Min(t => t.Car) : 0; //could also return -1
    }

    public int GetHighestLandSize(List<Housing> list)
    {
        return list.Count > 0 ? list.Max(t => t.Landsize) : 0; //could also return -1
    }

    public int GetLowestLandSize(List<Housing> list)
    {
        return list.Count > 0 ? list.Min(t => t.Landsize) : 0; //could also return -1
    }

    public int GetHighestPrice(List<Housing> list)
    {
        return list.Count > 0 ? list.Max(t => t.Price) : 0; //could also return -1
    }

    public int GetLowestPrice(List<Housing> list)
    {
        return list.Count > 0 ? list.Min(t => t.Price) : 0; //could also return -1
    }

    public int GetHighestYear(List<Housing> list)
    {
        return list.Count > 0 ? list.Max(t => t.YearBuilt) : 0; //could also return -1
    }

    public int GetLowestYear(List<Housing> list)
    {
        return list.Count > 0 ? list.Min(t => t.YearBuilt) : 0; //could also return -1
    }

    public float GetHighestLat(List<Housing> list)
    {
        return list.Count > 0 ? list.Max(t => t.Latitude) : 0; //could also return -1
    }

    public float GetLowestLat(List<Housing> list)
    {
        return list.Count > 0 ? list.Min(t => t.Latitude) : 0; //could also return -1
    }

    public float GetHighestLong(List<Housing> list)
    {
        return list.Count > 0 ? list.Max(t => t.Longtitude) : 0; //could also return -1
    }

    public float GetLowestLong(List<Housing> list)
    {
        return list.Count > 0 ? list.Min(t => t.Longtitude) : 0; //could also return -1
    }
}
