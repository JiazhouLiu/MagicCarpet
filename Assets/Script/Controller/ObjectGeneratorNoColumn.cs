using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class ObjectGeneratorNoColumn : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject MultiplePrefab;
    public GameObject DataManagerGO;

    [Header("Predefined Variables")]
    public float HSpacing;
    public float VSpacing;
    public float AdjustedHeight;
    public float MultipleSize;
    public int RowNumber;
    public int ColumnNumber;
    public float zPosition;

    [Header("Configurable Variables")]
    public int speed;

    private List<GameObject> multiples;

    // calculations
    private float shelfWidth = 0;
    private float shelfHeight = 0;

    // Start is called before the first frame update
    void Start()
    {
        //// calculate global variables
        //shelfWidth = MultipleSize * ColumnNumber + (ColumnNumber - 1) * HSpacing;
        //shelfHeight = MultipleSize * RowNumber + (RowNumber - 1) * VSpacing;

        // initiate variables
        multiples = new List<GameObject>();

        if (DataManagerGO.GetComponent<DataManager3D>() != null) {
            RowNumber = DataManagerGO.GetComponent<DataManager3D>().facetedRows;
            ColumnNumber = DataManagerGO.GetComponent<DataManager3D>().facetedColumns;
        }

        if (DataManagerGO.GetComponent<DataManager3DMap>() != null)
        {
            RowNumber = DataManagerGO.GetComponent<DataManager3DMap>().facetedRows;
            ColumnNumber = DataManagerGO.GetComponent<DataManager3DMap>().facetedColumns;
        }

        //// generate multiples
        //multiples = GenerateCards();

        //SetGridPositions(multiples);
    }
    private void Awake()
    {
        multiples = new List<GameObject>();
    }

    public List<GameObject> UpdateSM(List<GameObject> current_multiples, int column, int row) {

        multiples = current_multiples;
        ColumnNumber = column;
        RowNumber = row;

        // calculate global variables
        shelfWidth = MultipleSize * ColumnNumber + (ColumnNumber - 1) * HSpacing;
        shelfHeight = MultipleSize * RowNumber + (RowNumber - 1) * VSpacing;

        if (multiples.Count > 0) {
            foreach (GameObject go in multiples) {
                Destroy(go);
            }
            multiples.Clear();
        }

        multiples = GenerateMultiples();

        SetGridPositions(multiples);

        return multiples;
    }

    public void MoveSmallMultiples(List<GameObject> current_multiples) {
        multiples = current_multiples;
        for (int i = 0; i < multiples.Count; i++)
        {
            Vector3 desPos = multiples[i].transform.position + Vector3.forward * 3;
            multiples[i].transform.position = Vector3.Lerp(multiples[i].transform.position,
                desPos, Time.deltaTime * speed);
        }
    }

    public void ShuffleSmallMultiples(List<GameObject> current_multiples) {
        multiples = current_multiples;
        for (int i = 0; i < multiples.Count; i++)
        {
            GameObject temp = multiples[i];
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
                //GameObject multiple = new GameObject();
                GameObject multiple = Instantiate(MultiplePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                multiple.name = "Multiple " + (index + 1);
                multiple.transform.parent = transform;
                multiple.transform.localPosition = new Vector3(-MultipleSize / 2, 0, -MultipleSize / 2);
                multiple.transform.localScale = new Vector3(MultipleSize, MultipleSize, MultipleSize);
                multiple.AddComponent<PositionLocalConstraints>();

                //multiple.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = (index + 1) + "";
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

        transform.localPosition = new Vector3(0, AdjustedHeight, zPosition);
    }

    // Set Multiple Position
    private Vector3 SetMultipleDefaultPosition(int index, int row, int col)
    {
        float xValue = 0;
        float yValue = 0;
        float zValue = 0;

        xValue = (index - (row * ColumnNumber) - (ColumnNumber / 2.0f - 0.5f)) * (HSpacing + MultipleSize);
        yValue = (RowNumber - (row + 1)) * (VSpacing + MultipleSize);
        zValue = 0;

        //  FullCircle:
        //  xValue = -Mathf.Cos((index - (row * Columns)) * Mathf.PI / (Columns / 2.0f)) * ((Columns - 1) * hDelta / (2.0f * Mathf.PI));
        //  yValue = (Rows - (row + 1)) * vDelta;
        //  zValue = Mathf.Sin((index - (row * Columns)) * Mathf.PI / (Columns / 2.0f)) * ((Columns - 1) * hDelta / (2.0f * Mathf.PI));

        return new Vector3(xValue, yValue, zValue);
    }
}
