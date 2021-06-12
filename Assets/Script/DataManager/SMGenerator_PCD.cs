using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMGenerator_PCD : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject MultiplePrefab;
    public GameObject DataManagerGO;

    [Header("Predefined Variables")]
    public float HSpacing;
    public float VSpacing;
    public float AdjustedHeight;
    public float MultipleSize;
    public float zPosition;

    [Header("Configurable Variables")]
    private float speed;

    private List<GameObject> multiples;
    private int RowNumber;
    private int ColumnNumber;

    // Start is called before the first frame update
    void Start()
    {
        // initiate variables
        multiples = new List<GameObject>();

        RowNumber = DataManagerGO.GetComponent<DataManagerPCD>().facetedRows;
        ColumnNumber = DataManagerGO.GetComponent<DataManagerPCD>().facetedColumns;
        speed = DataManagerGO.GetComponent<DataManagerPCD>().speed;
    }
    private void Awake()
    {
        multiples = new List<GameObject>();
    }

    public List<GameObject> UpdateSM(List<GameObject> current_multiples, int column, int row)
    {

        multiples = current_multiples;
        ColumnNumber = column;
        RowNumber = row;

        if (multiples != null && multiples.Count > 0)
        {
            foreach (GameObject go in multiples)
            {
                Destroy(go);
            }
            multiples.Clear();
        }

        multiples = GenerateMultiples();

        SetGridPositions(multiples);

        return multiples;
    }

    public List<GameObject> DuplicateSM(int column, int row)
    {
        ColumnNumber = column;
        RowNumber = row;

        multiples = GenerateMultiples();

        SetGridPositions(multiples);

        return multiples;
    }

    public void MoveSmallMultiples(List<GameObject> current_multiples, Vector3 direction)
    {
        multiples = current_multiples;
        for (int i = 0; i < multiples.Count; i++)
        {
            Vector3 desPos = multiples[i].transform.position + direction * 3;
            multiples[i].transform.position = Vector3.Lerp(multiples[i].transform.position,
                desPos, Time.deltaTime * speed);
        }
    }

    public void ShuffleSmallMultiples(List<GameObject> current_multiples)
    {
        multiples = current_multiples;
        for (int i = 0; i < multiples.Count; i++)
        {
            Vector3 tempPos = multiples[i].transform.position;
            int randomIndex = Random.Range(i, multiples.Count);

            multiples[i].transform.position = multiples[randomIndex].transform.position;
            multiples[randomIndex].transform.position = tempPos;
        }
    }

    // Generate Cards
    private List<GameObject> GenerateMultiples()
    {
        List<GameObject> multiples = new List<GameObject>();

        for (int i = 0; i < RowNumber; i++)
        {
            for (int j = 0; j < ColumnNumber; j++)
            {
                // calculate index number
                int index = i * ColumnNumber + j;

                // generate game object
                GameObject multiple = Instantiate(MultiplePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                multiple.name = "Multiple " + (index + 1);
                multiple.transform.parent = transform;
                multiple.transform.localPosition = new Vector3(-MultipleSize / 2, 0, -MultipleSize / 2);
                multiple.transform.localScale = new Vector3(MultipleSize, MultipleSize, MultipleSize);
                multiple.transform.localEulerAngles = Vector3.zero;
                multiple.AddComponent<PositionLocalConstraints>();

                multiples.Add(multiple);
            }
        }
        return multiples;
    }

    // Set Grid Positions based on current layout
    private void SetGridPositions(List<GameObject> localCards)
    {
        for (int i = 0; i < RowNumber; i++)
        {
            for (int j = 0; j < ColumnNumber; j++)
            {
                int index = i * ColumnNumber + j;
                localCards[index].transform.localPosition = SetMultipleDefaultPosition(index, i, j);
            }
        }
        transform.parent.localPosition = new Vector3(0, AdjustedHeight, zPosition);
    }

    // Set Multiple Position
    private Vector3 SetMultipleDefaultPosition(int index, int row, int col)
    {
        float xValue;
        float yValue;
        float zValue;

        xValue = (index - (row * ColumnNumber) - (ColumnNumber / 2.0f - 0.5f)) * (HSpacing + MultipleSize);
        yValue = (RowNumber - (row + 1)) * (VSpacing + MultipleSize);
        zValue = 0;

        return new Vector3(xValue, yValue, zValue);
    }
}
