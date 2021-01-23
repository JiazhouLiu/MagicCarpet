using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataManager3DMap : MonoBehaviour
{
    public TextAsset DataSource;
    public GameObject markPrefab;
    public GameObject groundMarkPrefab;
    public Transform visParent;
    public Transform hiddenVisParent;
    public Transform groundMarkParent;
    public ObjectGeneratorNoColumn og;
    public MagicCarpetManager mcm;
    public Transform humanBody;

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
    private List<Vector3> MapGroundPositions;


    private Vector3 previousBodyPosition;
    private Vector3 previousBodyRotation;
    private string previousClosestRegion = "";

    private bool canMove = false;
    private bool showCase = true;

    private int smallMultiplesNumber;

    private readonly char lineSeperater = '\n'; // It defines line seperate character
    private readonly char fieldSeperator = ','; // It defines field seperate chracter

    // Start is called before the first frame update
    void Start()
    {
        MarkCollection = new List<GameObject>();
        CurrentSM = new List<GameObject>();
        //CurrentMarkCollection = new List<GameObject>();
        PropertyCollection = new List<Housing>();
        MapLocations = new List<Transform>();
        MapGroundPositions = new List<Vector3>();


        ReadData(DataSource);

        //ShowCaseScenario(PropertyCollection);
        previousBodyPosition = humanBody.position;
        previousBodyRotation = humanBody.eulerAngles;
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
        if (CheckHumanMoving()  || Input.GetKeyDown("n") || Input.GetKeyDown("m")) {
            UpdateVisGridTransform();
            FindNearestPoint();
        }

        if(CheckHumanRotating())
            UpdateVisGridTransform();

        if (canMove)
        {
            bool allMoved = true;
            foreach (GameObject mark in MarkCollection)
            {
                Housing h = mark.GetComponent<Housing>();

                mark.transform.localPosition = Vector3.Lerp(mark.transform.localPosition,
                                        new Vector3(h.XPosition, h.YPosition, h.ZPosition), Time.deltaTime * speed);

                if (Vector3.Distance(mark.transform.localPosition,
                        new Vector3(h.XPosition, h.YPosition, h.ZPosition)) > 0.01f)
                {
                    allMoved = false;
                }

            }

            if (allMoved)
            {
                canMove = false;
            }
        }

        if (Input.GetKeyDown("i"))
            UpdateFacetingVariable(facetedRows + 1, facetedColumns);
        if (Input.GetKeyDown("k"))
            UpdateFacetingVariable(facetedRows - 1, facetedColumns);
        if (Input.GetKeyDown("l"))
            UpdateFacetingVariable(facetedRows, facetedColumns + 1);
        if (Input.GetKeyDown("j"))
            UpdateFacetingVariable(facetedRows, facetedColumns - 1);
    }

    private void UpdateFacetingVariable(int newRow, int newColumn)
    {
        if (newRow > 0 && newColumn > 0)
        {
            facetedColumns = newColumn;
            facetedRows = newRow;

            //smallMultiplesNumber = facetedRows * facetedColumns;
            ShowCaseScenario(PropertyCollection);
            UpdateVisGridTransform();
            //CurrentSM = og.UpdateSM(CurrentSM, facetedColumns, facetedRows);
            //mcm.UpdateCurrentSM(CurrentSM);

            //// faceting channel
            //int minYear = GetLowestYear(PropertyCollection);
            //int maxYear = GetHighestYear(PropertyCollection) + 1;

            //foreach (GameObject mark in MarkCollection)
            //{
            //    Housing h = mark.GetComponent<Housing>();


            //    int facetDelta = ((maxYear - minYear) / smallMultiplesNumber) + 1;
            //    // setup small multiples
            //    for (int i = 0; i < smallMultiplesNumber; i++)
            //    {
            //        if (h.YearBuilt >= minYear + (facetDelta * i) && h.YearBuilt < minYear + (facetDelta * (i + 1)))
            //            mark.transform.SetParent(CurrentSM[i].transform);
            //    }
            //}
            //canMove = true;
        }
    }

    private void ShowCaseScenario(List<Housing> privatePC)
    {
        showCase = true;
        // TODO: faceted variable larger than required row * col
        if (facetedRows > 0 && facetedColumns > 0)
            smallMultiplesNumber = facetedRows * facetedColumns;
        else
            smallMultiplesNumber = 1;
        CurrentSM = og.UpdateSM(CurrentSM, facetedColumns, facetedRows);
        mcm.UpdateCurrentSM(CurrentSM);

        // position channel
        int minBed = GetLowestBedroom(privatePC);
        int maxBed = GetHighestBedroom(privatePC);
        int minBath = GetLowestBathroom(privatePC);
        int maxBath = GetHighestBathroom(privatePC);
        int minPrice = GetLowestPrice(privatePC);
        int maxPrice = GetHighestPrice(privatePC);

        // faceting channel
        int minYear = GetLowestYear(privatePC);
        int maxYear = GetHighestYear(privatePC) + 1;

        for (int i = 0; i < MarkCollection.Count; i++) {
            GameObject mark = MarkCollection[i];
            Housing h = mark.GetComponent<Housing>();

            if (h.Show)
            {
                // setup position channel
                h.XPosition = (float)(h.Bedroom - minBed) / (maxBed - minBed);
                h.YPosition = (float)(h.Price - minPrice) / (maxPrice - minPrice);
                h.ZPosition = (float)(h.Bathroom - minBath) / (maxBath - minBath);

                Color newCol = Color.black;
                switch (h.Type)
                {
                    case "House":
                        ColorUtility.TryParseHtmlString("#1b9e77", out newCol);
                        h.MarkColor = newCol;
                        break;
                    case "Townhouse":
                        ColorUtility.TryParseHtmlString("#d95f02", out newCol);
                        h.MarkColor = newCol;
                        break;
                    case "Unit":
                        ColorUtility.TryParseHtmlString("#7570b3", out newCol);
                        h.MarkColor = newCol;
                        break;
                    default:
                        break;
                }

                mark.GetComponent<MeshRenderer>().material.color = h.MarkColor;

                int facetDelta = ((maxYear - minYear) / smallMultiplesNumber) + 1;
                // setup small multiples
                for (int j = 0; j < smallMultiplesNumber; j++)
                {
                    if (h.YearBuilt >= minYear + (facetDelta * j) && h.YearBuilt < minYear + (facetDelta * (j + 1)))
                    {
                        mark.transform.SetParent(CurrentSM[j].transform);
                    }
                }
            }
            else
            {
                // setup position channel
                h.XPosition = MapGroundPositions[i].x;
                h.YPosition = MapGroundPositions[i].y;
                h.ZPosition = MapGroundPositions[i].z;

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

                mark.transform.SetParent(groundMarkParent);
            }
        }

        canMove = true;
    }

    private void ShowMap()
    {
        float minLat = GetLowestLat(PropertyCollection);
        float maxLat = GetHighestLat(PropertyCollection);
        float minLong = GetLowestLong(PropertyCollection);
        float maxLong = GetHighestLong(PropertyCollection);

        float zMultiplier = mapSize;
        float xMultiplier = (maxLat - minLat) / (maxLong - minLong) * zMultiplier;

        foreach (Housing h in PropertyCollection)
        {
            float xPosition = zMultiplier * (h.Longtitude - minLong) / (maxLong - minLong) - 1;
            float zPosition = xMultiplier * (h.Latitude - minLat) / (maxLat - minLat) - 1;

            MapGroundPositions.Add(new Vector3(xPosition, 0, zPosition));
            GameObject mark = Instantiate(groundMarkPrefab, new Vector3(xPosition, 0, zPosition),
            Quaternion.identity, groundMarkParent);

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

    private void FindNearestPoint()
    {
        float minDistance = 10000;
        Transform closestPoint = null;
        int closestPointIndex = -1;

        for (int i = 0; i < MapGroundPositions.Count; i++) {
            if (Vector3.Distance(MapGroundPositions[i], mcm.human.transform.position) < minDistance)
            {
                minDistance = Vector3.Distance(MapGroundPositions[i], mcm.human.transform.position);
                closestPointIndex = i;
            }
        }

        if (closestPointIndex != -1) {
            closestPoint = MarkCollection[closestPointIndex].transform;
        }

        string newClosestRegion = "";
        if (closestPoint != null)
            newClosestRegion = closestPoint.GetComponent<Housing>().RegionName;

        Debug.Log(newClosestRegion);

        if (newClosestRegion != previousClosestRegion) {
            List<Housing> newPC = new List<Housing>();

            foreach (GameObject go in MarkCollection)
            {
                if (go.GetComponent<Housing>().RegionName == newClosestRegion)
                {
                    go.GetComponent<Housing>().Show = true;
                    newPC.Add(go.GetComponent<Housing>());
                }
                else
                    go.GetComponent<Housing>().Show = false;
            }

            previousClosestRegion = newClosestRegion;
            ShowCaseScenario(newPC);
        }
    }

    private bool CheckHumanMoving() {
        Vector3 currentPosition = humanBody.position;
        if (currentPosition == previousBodyPosition)
            return false;
        previousBodyPosition = currentPosition;
        return true;
    }

    private bool CheckHumanRotating()
    {
        Vector3 currentRotation = humanBody.eulerAngles ;
        if (currentRotation == previousBodyRotation)
            return false;
        previousBodyRotation = currentRotation;
        return true;
    }

    private void UpdateVisGridTransform()
    {
        visParent.position = humanBody.TransformPoint(humanBody.localPosition + Vector3.forward * forwardParameter);
        visParent.position = new Vector3(visParent.position.x, og.AdjustedHeight, visParent.position.z);
        visParent.LookAt(humanBody);
        visParent.localEulerAngles = new Vector3(0, visParent.localEulerAngles.y + 180, 0);
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
