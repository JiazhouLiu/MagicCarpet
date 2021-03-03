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
    public Transform groundMarkParent;
    public Transform floorMenuParent;
    public Transform LeftFoot;
    public Transform RightFoot;

    [Header("Variables")]
    public int dataPointLimit = 0;
    public float markSize = 0.1f;
    public float mapSize = 4f;
    public float speed = 1;
    public int facetedRows = 1;
    public int facetedColumns = 1;
    public float forwardParameter = 10; // distance from dashboard to user
    public float feetDistanceDelta = 0.3f;

    private List<GameObject> MarkCollection;
    private List<GameObject> CurrentSM;
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

    private bool canMove = false;
    private bool DemoFlag = true;

    private readonly char lineSeperater = '\n'; // It defines line seperate character
    private readonly char fieldSeperator = ','; // It defines field seperate chracter

    private float CurrentMinSelectedValue = 100000;
    private float CurrentMaxSelectedValue = -100000;

    private bool FootControlSwitchOn = false;

    //testing
    private bool switchTester = false;

    // Start is called before the first frame update
    void Start()
    {
        MarkCollection = new List<GameObject>();
        CurrentSM = new List<GameObject>();
        PropertyCollection = new List<Housing>();
        MapLocations = new List<Transform>();
        FloorButtonCollection = new List<Transform>();
        MapInAirPositions = new List<Vector3>();
        vector3Filter = new OneEuroFilter<Vector3>(120f);

        ReadData(DataSource);

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
                        allMoved = false;
                }


                if (h.Ground) {
                    mark.transform.position = Vector3.Lerp(mark.transform.position,
                                        new Vector3(h.GroundXPosition, h.GroundYPosition, h.GroundZPosition), Time.deltaTime * speed);

                    if (Vector3.Distance(mark.transform.localPosition,
                            new Vector3(h.GroundXPosition, h.GroundYPosition, h.GroundZPosition)) > 0.01f)
                        allMoved = false;
                }

                if ((h.Air && h.Ground) || (!h.Air && !h.Ground))
                    Debug.Log("error");
            }

            if (allMoved)
                canMove = false;
        }

        if (Input.GetKeyDown("z")) {
            floorMenuParent.localScale -= Vector3.one * 0.05f;
            foreach (GameObject mark in MarkCollection)
            {
                Housing h = mark.GetComponent<Housing>();
                RefreshButtonGroundPosition(h);
            }
        }


        if (Input.GetKeyDown("x")) {
            floorMenuParent.localScale += Vector3.one * 0.05f;
            foreach (GameObject mark in MarkCollection)
            {
                Housing h = mark.GetComponent<Housing>();
                RefreshButtonGroundPosition(h);
            }
        }

        if (Input.GetKeyDown("k"))
        {
            if (switchTester)
                switchTester = false;
            else
                switchTester = true;
                
            OnKickSwitch(switchTester);
        }

    }

    public void OnKickSwitch(bool switchOn) {
        FootControlSwitchOn = switchOn;
        if (switchOn)
        {
            foreach (Transform t in FloorButtonCollection)
                t.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, 50f / 225f);
        }
        else {
            foreach (Transform t in FloorButtonCollection)
                t.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 50f / 225f);
        }
    }

    private string DetectFootGesture() {

        return "";
    }

    private void RefreshButtonGroundPosition(Housing h) {
        foreach (Transform button in FloorButtonCollection)
        {
            float minValue = button.GetComponent<FloorMenu>().MinValue;
            float maxValue = button.GetComponent<FloorMenu>().MaxValue;
            if (h.YearBuilt <= maxValue && h.YearBuilt >= minValue)
            {
                h.GroundXPosition = button.position.x;
                h.GroundYPosition = button.position.y;
                h.GroundZPosition = button.position.z;
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

            RefreshButtonGroundPosition(h);

            h.Ground = true;
            h.Air = false;

            GameObject mark = Instantiate(groundMarkPrefab, Vector3.zero,
            Quaternion.identity, groundMarkParent);

            mark.transform.position = new Vector3(h.GroundXPosition, h.GroundYPosition, h.GroundZPosition);

            mark.transform.localScale = Vector3.one * markSize;

            Color newCol = Color.black;
            switch (h.RegionName.Trim())
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
        // reset mesh color
        foreach (Transform floorMenu in floorMenuParent)
            floorMenu.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 50f / 255f);

        if (leftFootOnButton != null)
        {
            if (leftFootOnButton.GetComponent<FloorMenu>().MinValue < CurrentMinSelectedValue)
                CurrentMinSelectedValue = leftFootOnButton.GetComponent<FloorMenu>().MinValue;
            if (leftFootOnButton.GetComponent<FloorMenu>().MaxValue > CurrentMaxSelectedValue)
                CurrentMaxSelectedValue = leftFootOnButton.GetComponent<FloorMenu>().MaxValue;


            leftFootOnButton.GetComponent<MeshRenderer>().material.color = new Color(0,1,0,50f/255f);
        }

        if (rightFootOnButton != null)
        {
            if (rightFootOnButton.GetComponent<FloorMenu>().MinValue < CurrentMinSelectedValue)
                CurrentMinSelectedValue = rightFootOnButton.GetComponent<FloorMenu>().MinValue;
            if (rightFootOnButton.GetComponent<FloorMenu>().MaxValue > CurrentMaxSelectedValue)
                CurrentMaxSelectedValue = rightFootOnButton.GetComponent<FloorMenu>().MaxValue;

            rightFootOnButton.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, 50f / 255f);
        }

        if (rightFootOnButton != null && leftFootOnButton != null) {
            int rightIndex = rightFootOnButton.GetSiblingIndex();
            int leftIndex = leftFootOnButton.GetSiblingIndex();

            for (int i = Mathf.Min(leftIndex, rightIndex); i <= Mathf.Max(leftIndex, rightIndex); i++) {
                floorMenuParent.GetChild(i).GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, 50f / 255f);
            }
        }
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
