using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class MagicCarpetManagerMap : MonoBehaviour
{
    public GameObject markerPrefab;
    public GameObject DataManagerGO;
    public Transform visParent;
    public Transform visHolderParent;
    public Transform markerParent;
    public Transform human;
    public GameObject leftController;
    public GameObject rightController;
    public float markerRadius = 0.2f;

    private int counter = 1;

    private bool startLoad = false;
    private bool onMarker = false;
    private Marker currentMarker;
    private List<GameObject> multiples;

    VRTK_ControllerEvents rightCE;
    VRTK_ControllerEvents leftCE;
    ObjectGeneratorNoColumn og;
    DataManager3DMap dm3D;

    private void Awake()
    {
        multiples = new List<GameObject>();

        dm3D = DataManagerGO.GetComponent<DataManager3DMap>();
        og = visParent.GetComponent<ObjectGeneratorNoColumn>();
        rightCE = rightController.GetComponent<VRTK_ControllerEvents>();
        leftCE = leftController.GetComponent<VRTK_ControllerEvents>();

    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("e") || rightCE.triggerClicked)
        {
            Debug.Log("SAVE");
            if (!onMarker)
                SaveView();
        }

        if (Input.GetKeyDown("q") || rightCE.gripClicked)
        {
            Debug.Log("SHUFFLE");
            og.ShuffleSmallMultiples(multiples);
        }

        if (Input.GetKeyDown("t") || rightCE.touchpadPressed)
        {
            Debug.Log("MOVE FORWARD");
            og.MoveSmallMultiples(multiples, Vector3.forward);
        }

        if (Input.GetKeyDown("g") || rightCE.touchpadPressed)
        {
            Debug.Log("MOVE BACK");
            og.MoveSmallMultiples(multiples, Vector3.back);
        }

        if (Input.GetKeyDown("r") || rightCE.buttonTwoPressed)
        {
            Debug.Log("REMOVE");
            if (onMarker && currentMarker != null)
            {
                Destroy(currentMarker.gameObject);
                onMarker = false;
            }
        }

        if (Input.GetKeyDown("n") && dm3D.forwardParameter > 10)
            dm3D.forwardParameter -= 2;
        if (Input.GetKeyDown("m") && dm3D.forwardParameter < 50)
            dm3D.forwardParameter += 2;

        if (startLoad)
        {
            bool allMoved = true;

            for (int i = 0; i < visParent.childCount; i++)
            {
                Transform sm = visParent.GetChild(i);
                sm.position = currentMarker.savedSMPositions[i];

                List<Transform> currentDataPoints = currentMarker.savedDataPoints[sm.name];
                for (int j = 0; j < currentDataPoints.Count; j++)
                {
                    currentDataPoints[j].SetParent(sm);
                    currentDataPoints[j].localPosition = Vector3.Lerp(currentDataPoints[j].localPosition,
                        currentMarker.savedDataPointPositions[sm.name][j], Time.deltaTime * dm3D.speed);

                    if (Vector3.Distance(currentDataPoints[j].localPosition,
                        currentMarker.savedDataPointPositions[sm.name][j]) > 0.01f)
                        allMoved = false;
                }
            }

            if (visHolderParent.childCount > 0)
            {
                foreach (Transform t in visHolderParent)
                    Destroy(t.gameObject);
            }

            if (allMoved)
            {
                startLoad = false;
                Debug.Log("all moved");
            }

        }
    }

    public void UpdateCurrentSM(List<GameObject> sm)
    {
        if (multiples.Count > 0)
        {
            foreach (GameObject go in multiples)
                Destroy(go);
            multiples.Clear();
        }

        multiples = sm;
    }

    public void SaveViewFromController()
    {
        if (!onMarker)
        {
            Debug.Log("Save");
            SaveView();
        }
    }


    private void SaveView()
    {
        GameObject savePoint = Instantiate(markerPrefab, new Vector3(human.position.x, 0, human.position.z), Quaternion.identity);
        savePoint.transform.SetParent(markerParent);
        savePoint.name = "marker";

        float parentScaleDeltaX = 1 / markerParent.parent.localScale.x;
        float parentScaleDeltaZ = 1 / markerParent.parent.localScale.z;

        savePoint.transform.localScale = new Vector3(parentScaleDeltaX * markerRadius,
            0.05f, parentScaleDeltaZ * markerRadius);

        Marker marker = savePoint.GetComponent<Marker>();
        marker.savePointIndex = counter;
        counter++;

        List<Transform> sm = new List<Transform>();
        List<Vector3> smPositions = new List<Vector3>();
        Dictionary<string, List<Transform>> dataPoints = new Dictionary<string, List<Transform>>();
        Dictionary<string, List<Vector3>> dataPointPositions = new Dictionary<string, List<Vector3>>();
        for (int i = 0; i < visParent.childCount; i++)
        {
            sm.Add(visParent.GetChild(i));
            smPositions.Add(visParent.GetChild(i).position);



            List<Transform> dataPointsList = new List<Transform>();
            List<Vector3> dataPointPositionsList = new List<Vector3>();

            for (int j = 0; j < visParent.GetChild(i).childCount; j++)
            {
                dataPointsList.Add(visParent.GetChild(i).GetChild(j));
                dataPointPositionsList.Add(visParent.GetChild(i).GetChild(j).localPosition);
            }

            dataPoints.Add(visParent.GetChild(i).name, dataPointsList);
            dataPointPositions.Add(visParent.GetChild(i).name, dataPointPositionsList);
        }
        marker.colNumber = dm3D.facetedColumns;
        marker.rowNumber = dm3D.facetedRows;
        marker.savedSMs = sm;
        marker.savedSMPositions = smPositions;
        marker.savedDataPoints = dataPoints;
        marker.savedDataPointPositions = dataPointPositions;
    }

    public void LoadView(Marker marker)
    {
        Debug.Log("LOAD");
        startLoad = true;
        onMarker = true;
        currentMarker = marker;

        for (int i = 0; i < visParent.childCount; i++)
        {
            Transform sm = visParent.GetChild(i);

            GameObject go = new GameObject();
            go.transform.SetParent(visHolderParent);
            go.transform.position = sm.position;
            go.transform.localScale = sm.localScale;

            int childCount = sm.childCount;
            for (int j = 0; j < childCount; j++)
            {
                sm.GetChild(0).SetParent(go.transform);
            }
        }

        dm3D.facetedColumns = currentMarker.colNumber;
        dm3D.facetedRows = currentMarker.rowNumber;
        multiples = og.UpdateSM(multiples, currentMarker.colNumber, currentMarker.rowNumber);
    }

    public void OffMarker(Marker marker)
    {
        if (marker == currentMarker)
            onMarker = false;
    }
}
