using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class ObjectGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject MultiplePrefab;

    [Header("Predefined Variables")]
    public float HSpacing;
    public float VSpacing;
    public float AdjustedHeight;
    public float MultipleSize;
    public int RowNumber;
    public int ColumnNumber;

    [Header("Configurable Variables")]
    public int speed;

    private List<GameObject> multiples;
    private List<GameObject> columns;
    private List<Transform> movingColumns;

    private List<Vector3> columnsPositions;

    // calculations
    private float shelfWidth = 0;
    private float shelfHeight = 0;

    // Start is called before the first frame update
    void Start()
    {
        // calculate global variables
        shelfWidth = MultipleSize * ColumnNumber + (ColumnNumber - 1) * HSpacing;
        shelfHeight = MultipleSize * RowNumber + (RowNumber - 1) * VSpacing;


        // initiate variables
        multiples = new List<GameObject>();
        columns = new List<GameObject>();
        movingColumns = new List<Transform>();
        columnsPositions = new List<Vector3>();

        // generate multiples
        multiples = GenerateCards();

        SetGridPositions(multiples);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("h"))
        {
            Debug.Log("Shuffle");
            ShuffleSmallMultiples();
        }
    }

    private void ShuffleSmallMultiples() {
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
    private List<GameObject> GenerateCards()
    {
        List<GameObject> multiples = new List<GameObject>();

        for (int i = 0; i < ColumnNumber; i++)
        {
            GameObject column = new GameObject();
            column.transform.parent = transform;
            column.name = "Column " + (i + 1);
            column.AddComponent<Column>();
            columns.Add(column);

            for (int j = 0; j < RowNumber; j++)
            {
                // calculate index number
                int index = j * ColumnNumber + i;

                // generate game object
                GameObject multiple = Instantiate(MultiplePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                multiple.name = "Multiple " + (index + 1);
                multiple.transform.parent = column.transform;
                multiple.transform.localPosition = new Vector3(-MultipleSize / 2, 0, -MultipleSize / 2);
                multiple.transform.localScale = new Vector3(MultipleSize, MultipleSize, MultipleSize);
                multiple.AddComponent<PositionLocalConstraints>();

                //multiple.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = (index + 1) + "";
            }
        }

        for (int k = 0; k < RowNumber * ColumnNumber; k++)
        {
            multiples.Add(GameObject.Find("Multiple " + (k + 1)));
        }


        return multiples;
    }

    // Set Grid Positions based on current layout
    private void SetGridPositions(List<GameObject> localCards)
    {
        // set grid position
        transform.localPosition = new Vector3(0, AdjustedHeight, 0);

        // set column position
        SetColumnDefaultPosition();

        // set multiple vertical position
        SetMultipleDefaultPosition(localCards);
    }

    // Set Column Position
    private void SetColumnDefaultPosition()
    {
        int i = 0;
        foreach (GameObject column in columns)
        {
            float n = ((ColumnNumber - 1) / 2f);
            //Debug.Log((ColumnNumber - 1) / 2);
            column.transform.localPosition = new Vector3((i - n) * (HSpacing + MultipleSize), 0, 0);

            columnsPositions.Add(column.transform.localPosition);

            i++;
        }
    }

    // Set Multiple Position
    private void SetMultipleDefaultPosition(List<GameObject> localCards)
    {
        for (int i = 0; i < ColumnNumber * RowNumber; i++)
        {
            int n = RowNumber - (i / ColumnNumber) - 1;
            localCards[i].transform.localPosition = new Vector3(localCards[i].transform.localPosition.x, n * (VSpacing + MultipleSize), localCards[i].transform.localPosition.z);
        }
    }
}
