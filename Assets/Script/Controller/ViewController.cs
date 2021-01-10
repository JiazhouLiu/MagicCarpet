using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ViewController : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject MultiplePrefab;
    public GameObject tooltipPrefab;
    public GameObject CameraObj;
    public GameObject TouchPoint;
    public LineRenderer circle;

    [Header("Predefined Variables")]
    public float HSpacing;
    public float VSpacing;
    public float AdjustedHeight;
    public float Radius;
    public float MultipleSize;
    public int RowNumber;
    public int ColumnNumber;

    [Header("Configurable Variables")]
    public int speed;

    private List<GameObject> multiples;
    private List<GameObject> columns;
    private List<Transform> movingColumns;
    private List<Vector3> circlePointPositions;
    private List<Vector3> columnsPositions;

    // calculations
    private float shelfWidth = 0;
    private float shelfHeight = 0;
    private float shelfCircleFullArc = 0;
    private float shelfCircleRadius = 0;
    private float multipleDistanceInCircle = 0;
    private float smallestAngle = 180;

    // circle

    // Start is called before the first frame update
    void Start()
    {
        // calculate global variables
        shelfWidth = MultipleSize * ColumnNumber + (ColumnNumber - 1) * HSpacing;
        shelfHeight = MultipleSize * RowNumber + (RowNumber - 1) * VSpacing;
        shelfCircleFullArc = (MultipleSize + HSpacing) * ColumnNumber;
        shelfCircleRadius = shelfCircleFullArc / (2 * Mathf.PI);
        multipleDistanceInCircle = 2 * Mathf.Sin(90f * (MultipleSize + HSpacing) / (Mathf.PI * Radius)) * Radius;

        // initiate variables
        multiples = new List<GameObject>();
        columns = new List<GameObject>();
        movingColumns = new List<Transform>();
        circlePointPositions = new List<Vector3>();
        columnsPositions = new List<Vector3>();

        for (int i = 0; i < ColumnNumber; i++)
        {
            circlePointPositions.Add(new Vector3(Radius * Mathf.Sin(i * 30), 0, Radius * Mathf.Cos(i * 30)));
        }

        // generate multiples
        multiples = GenerateCards();

        SetGridPositions(multiples);
    }

    // Update is called once per frame
    void Update()
    {
        //foreach (GameObject go in columns)
        //{
        //    CheckDistance(go.transform);
        //}

        CheckCircleStatus();


        // change layout shortcut
        if (Input.GetKeyDown("q"))
            ChangeRadius(false);

        if (Input.GetKeyDown("e"))
            ChangeRadius(true);
    }

    private void ChangeRadius(bool push)
    {
        if (push)
        {
            if (Radius < 100)
                Radius += 0.1f;
        }
        else
        {
            if (Radius > 0)
                Radius -= 0.1f;
        }

        multipleDistanceInCircle = 2 * Mathf.Sin(90f * (MultipleSize + HSpacing) / (Mathf.PI * Radius)) * Radius;

        circlePointPositions.Clear();
        for (int i = 0; i < ColumnNumber; i++)
        {
            circlePointPositions.Add(new Vector3(Radius * Mathf.Sin(i * 30), 0, Radius * Mathf.Cos(i * 30)));
        }
    }

    // Generate Cards
    private List<GameObject> GenerateCards()
    {
        List<GameObject> multiples = new List<GameObject>();
        GameObject barChartManager = GameObject.Find("BarChartManager");

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
                //GameObject multiple = Instantiate(MultiplePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                GameObject multiple = new GameObject
                {
                    name = "Multiple " + (index + 1)
                };
                barChartManager.transform.GetChild(0).SetParent(multiple.transform);
                multiple.transform.parent = column.transform;
                multiple.transform.localPosition = new Vector3(-MultipleSize / 2, 0, -MultipleSize / 2);
                multiple.transform.localScale = new Vector3(MultipleSize, MultipleSize, MultipleSize);
                multiple.AddComponent<PositionLocalConstraints>();

                //setup tooltip
                //GameObject tooltip = (GameObject)Instantiate(tooltipPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                //tooltip.transform.SetParent(multiple.transform);
                //tooltip.transform.localPosition = new Vector3(-1f, 0.8f, 0);
                //tooltip.transform.localEulerAngles = new Vector3(0, 0, 90);

                //VRTK_ObjectTooltip tt = tooltip.GetComponent<VRTK_ObjectTooltip>();
                //tt.containerSize = new Vector2(300, 60);
                //tt.fontSize = 24;
                ////tt.displayText = GetBarChartName(i + 1);
                //tt.alwaysFaceHeadset = false;
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

    // dymanic position change
    private void SetMultiplePosition()
    {
        Vector3[] circlePositions = new Vector3[circle.positionCount];
        circle.GetPositions(circlePositions);

        movingColumns.Clear();
        // sort moving columns
        foreach (GameObject go in columns)
        {
            if (go.GetComponent<Column>().moving)
            {
                if (!movingColumns.Contains(go.transform))
                {
                    movingColumns.Add(go.transform);
                }
            }
            else
            {
                if (movingColumns.Contains(go.transform))
                {
                    movingColumns.Remove(go.transform);
                }
            }
        }

        if (movingColumns.Count > 0)
        {
            int indexDelta = 0;
            int startingIndex = 0;

            Vector3 startingPos = Vector3.zero;
            Vector3 endingPos = Vector3.zero;

            int circlePointsCount = circle.GetComponent<Circle>().vertexCount;

            int min = circlePointsCount;
            int max = 0;


            if (movingColumns.Count >= 2)
            {
                for (int j = 0; j < circlePositions.Length; j++)
                {
                    if (transform.InverseTransformPoint(circle.transform.TransformPoint(circlePositions[j])).z > 0)
                    {
                        if (j < min)
                        {
                            min = j;
                        }
                        if (j > max)
                        {
                            max = j;
                        }
                    }
                }

                if (min != circlePointsCount && max != 0)
                {
                    if (transform.InverseTransformPoint(circle.transform.TransformPoint(circlePositions[min])).x <
                        transform.InverseTransformPoint(circle.transform.TransformPoint(circlePositions[max])).x)
                    {
                        startingPos = circle.transform.TransformPoint(circlePositions[min]);
                        endingPos = circle.transform.TransformPoint(circlePositions[max]);
                        indexDelta = (max - min) / (movingColumns.Count - 1);
                        startingIndex = min;
                    }
                    else
                    {
                        startingPos = circle.transform.TransformPoint(circlePositions[max]);
                        endingPos = circle.transform.TransformPoint(circlePositions[min]);
                        indexDelta = (min - max) / (movingColumns.Count - 1);
                        startingIndex = max;
                    }
                }

                int increment = 0;
                foreach (Transform t in movingColumns)
                {

                    Vector3 nextPos = circle.transform.TransformPoint(circlePositions[startingIndex + (indexDelta * increment)]);

                    t.position = Vector3.Lerp(t.position,
                        new Vector3(nextPos.x, t.position.y, nextPos.z), Time.deltaTime * speed);
                    increment++;
                }

            }
        }
    }

    // update multiple position
    private void UpdateMultiplePosition(Transform t, int increment, Vector3[] circlePositions, int startingIndex, int indexDelta)
    {
        Vector3 nextPos = circle.transform.TransformPoint(circlePositions[startingIndex + (indexDelta * increment)]);

        t.position = Vector3.Lerp(t.position,
            new Vector3(nextPos.x, t.position.y, nextPos.z), Time.deltaTime * speed);
    }

    // Set Multiple Rotation
    private void SetMultipleRotation(Transform obj)
    {
        // face to camera
        obj.LookAt(CameraObj.transform);

        obj.localEulerAngles = new Vector3(0, obj.localEulerAngles.y + 180, 0);
    }

    // update function check distance
    private void CheckDistance(Transform t)
    {
        Vector3 cameraPosition_xz = new Vector3(CameraObj.transform.position.x, 0, CameraObj.transform.position.z);

        float angleToForward = Vector3.SignedAngle(CameraObj.transform.forward, (t.position - CameraObj.transform.position), Vector3.up);
        if (Mathf.Abs(angleToForward) < smallestAngle)
            smallestAngle = angleToForward;

        if (Vector3.Distance(t.position, cameraPosition_xz) <= Radius + 0.18f)
        {
            t.GetComponent<Column>().moving = true;

            // set rotation
            SetMultipleRotation(t);
            // set position
            SetMultiplePosition();
            //t.position = new Vector3(cameraPosition_xz.x + DistanceDelta * Mathf.Sin(angleToForward), t.position.y, cameraPosition_xz.z + DistanceDelta * Mathf.Cos(angleToForward));

            TouchPoint.transform.position = CameraObj.transform.position + Vector3.forward * Radius;
        }
        else if (Vector3.Distance(t.position, cameraPosition_xz) > Radius + 0.2f)
        {
            if (t.localPosition.z < 0.01f)
            {
                t.GetComponent<Column>().moving = false;

                t.localPosition = Vector3.Lerp(t.localPosition,
                             columnsPositions[columns.IndexOf(t.gameObject)], Time.deltaTime * 5 * speed);
                t.localEulerAngles = Vector3.zero;
            }
            else
            {
                // set rotation
                SetMultipleRotation(t);
                // set position
                SetMultiplePosition();
            }
            //CheckColumnDistance(t.gameObject);
        }
    }

    private void CheckColumnDistance(GameObject column)
    {
        Transform t = column.transform;
        if (columns.IndexOf(column) != 0 && columns.IndexOf(column) != (ColumnNumber - 1))
        {
            if (Vector3.Distance(column.transform.position, columns[columns.IndexOf(column) + 1].transform.position) > (MultipleSize + HSpacing))
            {
                t.localPosition = Vector3.Lerp(t.localPosition, new Vector3(t.localPosition.x + speed * 0.1f, t.localPosition.y, t.localPosition.z), Time.deltaTime * 10);
                //Debug.Log(column.name);
            }
            else if (Vector3.Distance(column.transform.position, columns[columns.IndexOf(column) - 1].transform.position) > (MultipleSize + HSpacing))
            {
                t.localPosition = Vector3.Lerp(t.localPosition, new Vector3(t.localPosition.x - speed * 0.1f, t.localPosition.y, t.localPosition.z), Time.deltaTime * 10);
            }
            else
            {
                t.localPosition = columnsPositions[columns.IndexOf(t.gameObject)];
            }
        }
    }

    private void CheckCircleStatus()
    {
        Vector3[] circlePositions = new Vector3[circle.positionCount];
        circle.GetPositions(circlePositions);

        int circlePointsCount = circle.GetComponent<Circle>().vertexCount;

        int min = circlePointsCount;
        int max = 0;

        int indexDelta = 0;
        int leftIndex = 0;
        int rightIndex = 0;

        Vector3 leftPos = Vector3.zero;
        Vector3 rightPos = Vector3.zero;

        for (int j = 0; j < circlePositions.Length; j++)
        {
            if (transform.InverseTransformPoint(circle.transform.TransformPoint(circlePositions[j])).z > 0)
            {
                if (j < min)
                {
                    min = j;
                }
                if (j > max)
                {
                    max = j;
                }
            }
        }

        // if circle joins
        if (min != circlePointsCount && max != 0)
        {
            if (transform.InverseTransformPoint(circle.transform.TransformPoint(circlePositions[min])).x <
                transform.InverseTransformPoint(circle.transform.TransformPoint(circlePositions[max])).x)
            {
                leftPos = circle.transform.TransformPoint(circlePositions[min]);
                rightPos = circle.transform.TransformPoint(circlePositions[max]);
                leftIndex = min;
                rightIndex = max;
            }
            else
            {
                leftPos = circle.transform.TransformPoint(circlePositions[max]);
                rightPos = circle.transform.TransformPoint(circlePositions[min]);
                leftIndex = max;
                rightIndex = min;
            }

            if (transform.InverseTransformPoint(leftPos).x > ((ColumnNumber * MultipleSize + (ColumnNumber - 1) * HSpacing) / 2) ||
                transform.InverseTransformPoint(rightPos).x < -((ColumnNumber * MultipleSize + (ColumnNumber - 1) * HSpacing) / 2))
            {
                foreach (GameObject column in columns)
                {
                    Transform t = column.transform;
                    // update list
                    t.GetComponent<Column>().moving = false;
                    if (movingColumns.Contains(t))
                    {
                        movingColumns.Remove(t);
                    }

                    // update rotation
                    t.localPosition = Vector3.Lerp(t.localPosition,
                                 columnsPositions[columns.IndexOf(t.gameObject)], Time.deltaTime * 5 * speed);
                    t.localEulerAngles = Vector3.zero;
                }
            }
            else {
                movingColumns.Clear();
                // update moving columns
                foreach (GameObject column in columns)
                {
                    Transform t = column.transform;
                    if (((t.localPosition.x) > transform.InverseTransformPoint(leftPos).x) &&
                        ((t.localPosition.x) < transform.InverseTransformPoint(rightPos).x))
                    {
                        // update list
                        t.GetComponent<Column>().moving = true;
                        if (!movingColumns.Contains(t))
                        {
                            movingColumns.Add(t);
                        }

                        // update rotation
                        SetMultipleRotation(t);
                    }
                    else
                    {
                        // update list
                        t.GetComponent<Column>().moving = false;
                        if (movingColumns.Contains(t))
                        {
                            movingColumns.Remove(t);
                        }

                        // update rotation
                        t.localPosition = Vector3.Lerp(t.localPosition,
                                     columnsPositions[columns.IndexOf(t.gameObject)], Time.deltaTime * 5 * speed);
                        t.localEulerAngles = Vector3.zero;
                    }
                }

                if (movingColumns.Count == 1)
                {
                    Vector3 nextPos = circle.transform.TransformPoint(circlePositions[(leftIndex + rightIndex) / 2]);

                    movingColumns[0].position = Vector3.Lerp(movingColumns[0].position,
                        new Vector3(nextPos.x, movingColumns[0].position.y, nextPos.z), Time.deltaTime * speed);
                }
                else if (movingColumns.Count > 1)
                {
                    // update multiple position
                    int increment = 0;
                    indexDelta = (rightIndex - leftIndex) / (movingColumns.Count - 1);

                    foreach (Transform t in movingColumns)
                    {
                        // update multiple position
                        UpdateMultiplePosition(t, increment, circlePositions, leftIndex, indexDelta);
                        increment++;
                    }
                }
            }

            
        }
        else {
            foreach (GameObject column in columns)
            {
                Transform t = column.transform;
                // update list
                t.GetComponent<Column>().moving = false;
                if (movingColumns.Contains(t))
                {
                    movingColumns.Remove(t);
                }

                // update rotation
                t.localPosition = Vector3.Lerp(t.localPosition,
                             columnsPositions[columns.IndexOf(t.gameObject)], Time.deltaTime * 5 * speed);
                t.localEulerAngles = Vector3.zero;
            }
        }
    }
}
