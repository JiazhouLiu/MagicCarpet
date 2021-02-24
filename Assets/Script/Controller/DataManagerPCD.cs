using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataManagerPCD : MonoBehaviour
{
    public TextAsset DataSource;
    public GameObject markPrefab;
    public Transform visParent;
    public SMGenerator_PCD og;
    public MagicCarpetManager_PCD mcm;

    [Header("Variables")]
    public int dataPointLimit = 0;
    public float markSize = 0.1f;
    public float speed = 1;
    public int facetedRows = 1;
    public int facetedColumns = 1;
    public float forwardParameter = 10;

    private List<GameObject> MarkCollection;
    private List<GameObject> CurrentSM;
    private List<Housing> PropertyCollection;

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
        PropertyCollection = new List<Housing>();

        ReadData(DataSource);

        ShowCaseScenario();
    }

    private void ReadData(TextAsset ta)
    {

        string[] lines = ta.text.Split(lineSeperater);
        int dataLength;
        if (dataPointLimit == 0)
            dataLength = lines.Length;
        else
            dataLength = dataPointLimit;

        for (int i = 1; i < dataLength; i++)
        {

            if (lines[i].Length > 10)
            {
                GameObject mark = Instantiate(markPrefab, new Vector3(0, 0, 0),
            Quaternion.identity, visParent);
                mark.transform.localScale = Vector3.one * markSize;

                Housing property = new Housing(i, lines[i].Split(fieldSeperator)[0], lines[i].Split(fieldSeperator)[1],
                int.Parse(lines[i].Split(fieldSeperator)[2]), lines[i].Split(fieldSeperator)[3],
                lines[i].Split(fieldSeperator)[4], DateTime.Parse(lines[i].Split(fieldSeperator)[5]),
                int.Parse(lines[i].Split(fieldSeperator)[6]), int.Parse(lines[i].Split(fieldSeperator)[7]),
                int.Parse(lines[i].Split(fieldSeperator)[8]), int.Parse(lines[i].Split(fieldSeperator)[9]),
                int.Parse(lines[i].Split(fieldSeperator)[10]), lines[i].Split(fieldSeperator)[11], float.Parse(lines[i].Split(fieldSeperator)[12]),
                float.Parse(lines[i].Split(fieldSeperator)[13]), lines[i].Split(fieldSeperator)[14]);

                mark.GetComponent<Housing>().CopyEntity(property);
                MarkCollection.Add(mark);
                PropertyCollection.Add(property);
            }

        }
    }

    private void Update()
    {

        if (canMove)
        {
            bool allMoved = true;
            foreach (GameObject mark in MarkCollection)
            {
                Housing h = mark.GetComponent<Housing>();

                mark.transform.localPosition = Vector3.Lerp(mark.transform.localPosition,
                                        new Vector3(h.GroundXPosition, h.GroundYPosition, h.GroundZPosition), Time.deltaTime * speed);

                if (Vector3.Distance(mark.transform.localPosition,
                        new Vector3(h.GroundXPosition, h.GroundYPosition, h.GroundZPosition)) > 0.01f)
                    allMoved = false;

            }

            if (allMoved)
                canMove = false;
        }

        if (Input.GetKeyDown("x"))
        {
            if (showCase)
                ShowCaseScenario2();
            else
                ShowCaseScenario();
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

            smallMultiplesNumber = facetedRows * facetedColumns;

            CurrentSM = og.UpdateSM(CurrentSM, facetedColumns, facetedRows);
            mcm.UpdateCurrentSM(CurrentSM);

            // faceting channel
            int minYear = GetLowestYear(PropertyCollection);
            int maxYear = GetHighestYear(PropertyCollection) + 1;

            foreach (GameObject mark in MarkCollection)
            {
                Housing h = mark.GetComponent<Housing>();


                int facetDelta = ((maxYear - minYear) / smallMultiplesNumber) + 1;
                // setup small multiples
                for (int i = 0; i < smallMultiplesNumber; i++)
                {
                    if (h.YearBuilt >= minYear + (facetDelta * i) && h.YearBuilt < minYear + (facetDelta * (i + 1)))
                        mark.transform.SetParent(CurrentSM[i].transform);
                }
            }
            canMove = true;


        }
    }

    private void ShowCaseScenario()
    {
        showCase = true;

        if (facetedRows > 0 && facetedColumns > 0)
            smallMultiplesNumber = facetedRows * facetedColumns;
        else
            smallMultiplesNumber = 1;
        CurrentSM = og.UpdateSM(CurrentSM, facetedColumns, facetedRows);
        mcm.UpdateCurrentSM(CurrentSM);

        // position channel
        int minBed = GetLowestBedroom(PropertyCollection);
        int maxBed = GetHighestBedroom(PropertyCollection);
        int minBath = GetLowestBathroom(PropertyCollection);
        int maxBath = GetHighestBathroom(PropertyCollection);
        int minPrice = GetLowestPrice(PropertyCollection);
        int maxPrice = GetHighestPrice(PropertyCollection);

        // faceting channel
        int minYear = GetLowestYear(PropertyCollection);
        int maxYear = GetHighestYear(PropertyCollection) + 1;

        // setup color channel
        foreach (GameObject mark in MarkCollection)
        {
            Housing h = mark.GetComponent<Housing>();
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
        }

        foreach (GameObject mark in MarkCollection)
        {
            Housing h = mark.GetComponent<Housing>();


            mark.SetActive(true);
            // setup position channel
            h.GroundXPosition = (float)(h.Bedroom - minBed) / (maxBed - minBed);
            h.GroundYPosition = (float)(h.Price - minPrice) / (maxPrice - minPrice);
            h.GroundZPosition = (float)(h.Bathroom - minBath) / (maxBath - minBath);

            int facetDelta = ((maxYear - minYear) / smallMultiplesNumber) + 1;
            // setup small multiples
            for (int i = 0; i < smallMultiplesNumber; i++)
            {
                if (h.YearBuilt >= minYear + (facetDelta * i) && h.YearBuilt < minYear + (facetDelta * (i + 1)))
                    mark.transform.SetParent(CurrentSM[i].transform);
            }
        }

        canMove = true;
    }

    private void ShowCaseScenario2()
    {
        showCase = false;
        // TODO: faceted variable larger than required row * col
        if (facetedRows > 0 && facetedColumns > 0)
            smallMultiplesNumber = facetedRows * facetedColumns;
        else
            smallMultiplesNumber = 1;
        CurrentSM = og.UpdateSM(CurrentSM, facetedColumns, facetedRows);
        mcm.UpdateCurrentSM(CurrentSM);

        // position channel
        int minCar = GetLowestCar(PropertyCollection);
        int maxCar = GetHighestCar(PropertyCollection);
        int minLandSize = GetLowestLandSize(PropertyCollection);
        int maxLandSize = GetHighestLandSize(PropertyCollection);
        int minPrice = GetLowestPrice(PropertyCollection);
        int maxPrice = GetHighestPrice(PropertyCollection);

        // faceting channel
        int minYear = GetLowestYear(PropertyCollection);
        int maxYear = GetHighestYear(PropertyCollection) + 1;

        // setup color channel
        foreach (GameObject mark in MarkCollection)
        {
            Housing h = mark.GetComponent<Housing>();
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
        }

        foreach (GameObject mark in MarkCollection)
        {
            Housing h = mark.GetComponent<Housing>();

            mark.SetActive(true);
            // setup position channel
            h.GroundXPosition = (float)(h.Car - minCar) / (maxCar - minCar);
            h.GroundYPosition = (float)(h.Price - minPrice) / (maxPrice - minPrice);
            h.GroundZPosition = (float)(h.Landsize - minLandSize) / (maxLandSize - minLandSize);

            int facetDelta = ((maxYear - minYear) / smallMultiplesNumber) + 1;
            // setup small multiples
            for (int i = 0; i < smallMultiplesNumber; i++)
            {
                if (h.YearBuilt >= minYear + (facetDelta * i) && h.YearBuilt < minYear + (facetDelta * (i + 1)))
                    mark.transform.SetParent(CurrentSM[i].transform);
            }
        }

        canMove = true;
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
