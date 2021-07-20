using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class DataPlayback : MonoBehaviour
{
    [Header("Scene Object Reference")]
    public Transform LandmarksParent;
    public Transform DetailedViewsParent;
    public GameObject WallDisplay;
    public GameObject TableTop;
    public GameObject Floor;
    public GameObject Cockpit;
    public Transform HeadFOR;
    public Transform WaistFOR;
    public Transform FloorFOR;
    public Transform WallFOR;
    public Transform TabletopFOR;

    [Header("Data Files")]
    public TextAsset HeadTransformData;
    public TextAsset LeftControllerTransformData;
    public TextAsset RightControllerTransformData;
    public TextAsset WaistTransformData;
    public TextAsset LeftFootTransformData;
    public TextAsset RightFootTransformData;
    public TextAsset CameraTransformData;
    public TextAsset LandmarksData;
    public TextAsset DetailedViewsData;

    [Header("Replay Objects")]
    public Transform HeadObject;
    public Transform LeftControllerObject;
    public Transform RightControllerObject;
    public Transform WaistObject;
    public Transform LeftFootObject;
    public Transform RightFootObject;
    public Transform CameraObject;
    public Transform[] LandmarkObjects;
    public Transform[] DetailedViewObjects;

    [Header("Replay Settings")]
    public bool UseRealTime = true;
    [Range(0, 1)] public float TimeScrubber;
    public float CurrentTime = 0;

    private bool isLiveReplayRunning = false;
    private bool isLiveReplayPaused = false;
    private float prevTimeScrubber;

    private OneEuroFilter<Vector3> cameraTransformPositionFilter;
    private OneEuroFilter<Quaternion> cameraTransformRotationFilter;

    private int prevQuestionID = 0;
    private int currentQuestionID = 0;

    public void StartLiveReplay()
    {
        if (!isLiveReplayRunning)
        {
            isLiveReplayPaused = false;
            StartCoroutine(LiveReplay());
        }
        else if (isLiveReplayPaused)
        {
            isLiveReplayPaused = false;
        }
    }

    public IEnumerator LiveReplay()
    {
        isLiveReplayRunning = true;

        // Read and split data
        string[] headTransformDataLines = HeadTransformData.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<string[]> headTransformData = new List<string[]>();
        for (int i = 1; i < headTransformDataLines.Length; i++)
            headTransformData.Add(headTransformDataLines[i].Split('\t'));

        string[] leftControllerTransformsDataLines = LeftControllerTransformData.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<string[]> leftControllerTransformData = new List<string[]>();
        for (int i = 1; i < leftControllerTransformsDataLines.Length; i++)
            leftControllerTransformData.Add(leftControllerTransformsDataLines[i].Split('\t'));

        string[] rightControllerTransformsDataLines = RightControllerTransformData.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<string[]> rightControllerTransformData = new List<string[]>();
        for (int i = 1; i < rightControllerTransformsDataLines.Length; i++)
            rightControllerTransformData.Add(rightControllerTransformsDataLines[i].Split('\t'));

        string[] waistTransformsDataLines = WaistTransformData.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<string[]> waistTransformData = new List<string[]>();
        for (int i = 1; i < waistTransformsDataLines.Length; i++)
            waistTransformData.Add(waistTransformsDataLines[i].Split('\t'));

        string[] leftFootTransformsDataLines = LeftFootTransformData.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<string[]> leftFootTransformData = new List<string[]>();
        for (int i = 1; i < leftFootTransformsDataLines.Length; i++)
            leftFootTransformData.Add(leftFootTransformsDataLines[i].Split('\t'));

        string[] rightFootTransformsDataLines = HeadTransformData.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<string[]> rightFootTransformData = new List<string[]>();
        for (int i = 1; i < rightFootTransformsDataLines.Length; i++)
            rightFootTransformData.Add(rightFootTransformsDataLines[i].Split('\t'));

        string[] cameraTransformDataLines = CameraTransformData.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<string[]> cameraTransformData = new List<string[]>();
        for (int i = 1; i < cameraTransformDataLines.Length; i++)
            cameraTransformData.Add(cameraTransformDataLines[i].Split('\t'));

        string[] landmarksDataLines = LandmarksData.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<string[]> landmarksData = new List<string[]>();
        for (int i = 1; i < landmarksDataLines.Length; i++)
            landmarksData.Add(landmarksDataLines[i].Split('\t'));

        string[] detailedViewsDataLines = DetailedViewsData.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<string[]> detailedViewData = new List<string[]>();
        for (int i = 1; i < detailedViewsDataLines.Length; i++)
            detailedViewData.Add(detailedViewsDataLines[i].Split('\t'));

        float totalTime = float.Parse(headTransformData[headTransformData.Count - 1][0]);

        cameraTransformPositionFilter = new OneEuroFilter<Vector3>(10);
        cameraTransformRotationFilter = new OneEuroFilter<Quaternion>(10);

        int landmarksLineIndex = 1;
        int detailedViewsLineIndex = 1;

        for (int i = 0; i < headTransformData.Count; i += (isLiveReplayPaused) ? 0 : 1)
        {
            if (TimeScrubber != prevTimeScrubber)
            {
                // Find the nearest i index that matches the new time
                float newTime = TimeScrubber * totalTime;

                int startIdx = Mathf.FloorToInt(TimeScrubber * headTransformData.Count) - 50;
                startIdx = Mathf.Max(startIdx, 0);
                for (int j = startIdx; j < headTransformData.Count; j++)
                {
                    float thisTime = float.Parse(headTransformData[j][0]);
                    if (thisTime > newTime)
                    {
                        newTime = float.Parse(headTransformData[Mathf.Max(j - 1, 0)][0]);
                        i = j;
                        break;
                    }
                }

                landmarksLineIndex = 1;
                detailedViewsLineIndex = 1;
                prevTimeScrubber = TimeScrubber;
            }

            if (isLiveReplayPaused)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }

            float currentTime = float.Parse(headTransformData[i][0]);

            CurrentTime = currentTime;
            TimeScrubber = currentTime / totalTime;
            prevTimeScrubber = TimeScrubber;

            // Set the positions and rotations of the head
            string[] headTransformsLine = headTransformData[i];
            HeadObject.position = new Vector3(float.Parse(headTransformsLine[1]), float.Parse(headTransformsLine[2]), float.Parse(headTransformsLine[3]));
            HeadObject.rotation = new Quaternion(float.Parse(headTransformsLine[4]), float.Parse(headTransformsLine[5]), float.Parse(headTransformsLine[6]), float.Parse(headTransformsLine[7]));

            // Set the positions and rotations of the head
            string[] leftControllerTransformsLine = leftControllerTransformData[i];
            LeftControllerObject.position = new Vector3(float.Parse(leftControllerTransformsLine[1]), float.Parse(leftControllerTransformsLine[2]), float.Parse(leftControllerTransformsLine[3]));
            LeftControllerObject.rotation = new Quaternion(float.Parse(leftControllerTransformsLine[4]), float.Parse(leftControllerTransformsLine[5]), 
                float.Parse(leftControllerTransformsLine[6]), float.Parse(leftControllerTransformsLine[7]));

            // Set the positions and rotations of the head
            string[] rightControllerTransformsLine = rightControllerTransformData[i];
            RightControllerObject.position = new Vector3(float.Parse(rightControllerTransformsLine[1]), float.Parse(rightControllerTransformsLine[2]), float.Parse(rightControllerTransformsLine[3]));
            RightControllerObject.rotation = new Quaternion(float.Parse(rightControllerTransformsLine[4]), float.Parse(rightControllerTransformsLine[5]), 
                float.Parse(rightControllerTransformsLine[6]), float.Parse(rightControllerTransformsLine[7]));

            // Set the positions and rotations of the head
            string[] waistTransformsLine = waistTransformData[i];
            WaistObject.position = new Vector3(float.Parse(waistTransformsLine[1]), float.Parse(waistTransformsLine[2]), float.Parse(waistTransformsLine[3]));
            WaistObject.rotation = new Quaternion(float.Parse(waistTransformsLine[4]), float.Parse(waistTransformsLine[5]), 
                float.Parse(waistTransformsLine[6]), float.Parse(waistTransformsLine[7]));

            // Set the positions and rotations of the head
            string[] leftFootTransformsLine = leftFootTransformData[i];
            LeftFootObject.position = new Vector3(float.Parse(leftFootTransformsLine[1]), float.Parse(leftFootTransformsLine[2]), float.Parse(leftFootTransformsLine[3]));
            LeftFootObject.rotation = new Quaternion(float.Parse(leftFootTransformsLine[4]), float.Parse(leftFootTransformsLine[5]), 
                float.Parse(leftFootTransformsLine[6]), float.Parse(leftFootTransformsLine[7]));

            // Set the positions and rotations of the head
            string[] rightFootTransformsLine = rightFootTransformData[i];
            RightFootObject.position = new Vector3(float.Parse(rightFootTransformsLine[1]), float.Parse(rightFootTransformsLine[2]), float.Parse(rightFootTransformsLine[3]));
            RightFootObject.rotation = new Quaternion(float.Parse(rightFootTransformsLine[4]), float.Parse(rightFootTransformsLine[5]), 
                float.Parse(rightFootTransformsLine[6]), float.Parse(rightFootTransformsLine[7]));

            // Set the position and rotation of the external camera
            string[] cameraTransformLine = cameraTransformData[i];
            Vector3 newCameraPosition = new Vector3(float.Parse(cameraTransformLine[1]), float.Parse(cameraTransformLine[2]), float.Parse(cameraTransformLine[3]));
            Quaternion newCameraRotation = new Quaternion(float.Parse(cameraTransformLine[4]), float.Parse(cameraTransformLine[5]), float.Parse(cameraTransformLine[6]), float.Parse(cameraTransformLine[7])); ;
            // Filter camera position and rotation since it jitters in the Vicon
            CameraObject.position = cameraTransformPositionFilter.Filter<Vector3>(newCameraPosition);
            CameraObject.rotation = cameraTransformRotationFilter.Filter<Quaternion>(newCameraRotation);

            /////// Landmarks //////
            // Find the line index of the landmarks data which has the same time as the current head transforms line
            for (int j = landmarksLineIndex; j < landmarksDataLines.Length; j++)
            {
                string[] landmarksLine = landmarksData[j];

                if (currentTime == float.Parse(landmarksLine[0]))
                {
                    landmarksLineIndex = j;
                    currentQuestionID = int.Parse(landmarksLine[1]);

                    if (currentQuestionID != prevQuestionID)
                    { // Get current Landmarks Objects
                        prevQuestionID = currentQuestionID;

                        // clear previous landmarks
                        foreach (Transform t in LandmarkObjects)
                            Destroy(t.gameObject);
                        if (LandmarkObjects.Length != 0)
                            Debug.Log("Clear Issue!!!");
                        Floor.SetActive(false);
                        TableTop.SetActive(false);
                        Cockpit.SetActive(false);

                        Transform landmarkParent = LandmarksParent.Find(currentQuestionID.ToString());

                        // enable Frame of Reference Object
                        if (landmarksLine[3] == "Floor")
                        {
                            Floor.SetActive(true);
                            int l = 0;
                            foreach (Transform t in landmarkParent)
                            {
                                Transform newLandmark = Instantiate(t, Vector3.zero, Quaternion.identity, FloorFOR);
                                newLandmark.name = t.name;

                                LandmarkObjects[l] = newLandmark;
                                l++;
                            }
                        }
                        else if (landmarksLine[3] == "Tabletop")
                        {
                            TableTop.SetActive(true);
                            TableTop.transform.position = new Vector3(TableTop.transform.position.x, float.Parse(landmarksLine[13]), TableTop.transform.position.z);

                            int l = 0;
                            foreach (Transform t in landmarkParent)
                            {
                                Transform newLandmark = Instantiate(t, Vector3.zero, Quaternion.identity, TabletopFOR);
                                newLandmark.name = t.name;

                                LandmarkObjects[l] = newLandmark;
                                l++;
                            }
                        }
                        else if (landmarksLine[3] == "Body") {
                            Cockpit.SetActive(true);

                            int l = 0;
                            foreach (Transform t in landmarkParent)
                            {
                                Transform newLandmark = Instantiate(t, Vector3.zero, Quaternion.identity, WaistFOR);
                                newLandmark.name = t.name;

                                LandmarkObjects[l] = newLandmark;
                                l++;
                            }
                        }
                    }
                    break;
                }
            }

            //Now that we've found it, set the properties of the landmarks
            for (int j = 0; j < LandmarkObjects.Length; j++)
            {
                string[] landmarksLine = landmarksData[landmarksLineIndex + j];

                foreach (Transform t in LandmarkObjects) {
                    if (t.name == landmarksLine[2]) {
                        t.transform.position = new Vector3(float.Parse(landmarksLine[4]), float.Parse(landmarksLine[5]), float.Parse(landmarksLine[6]));
                        t.transform.rotation = new Quaternion(float.Parse(landmarksLine[7]), float.Parse(landmarksLine[8]), 
                            float.Parse(landmarksLine[9]), float.Parse(landmarksLine[10]));
                        t.transform.localScale = new Vector3(float.Parse(landmarksLine[11]), float.Parse(landmarksLine[11]), float.Parse(landmarksLine[11]));

                        switch (landmarksLine[12]) {
                            case "moving":
                                t.GetChild(2).gameObject.SetActive(true);
                                foreach (Transform border in t.GetChild(2)) {
                                    border.GetComponent<MeshRenderer>().material.color = Color.yellow;
                                }
                                break;
                            case "selected":
                                t.GetChild(2).gameObject.SetActive(true);
                                foreach (Transform border in t.GetChild(2))
                                {
                                    border.GetComponent<MeshRenderer>().material.color = Color.blue;
                                }
                                break;
                            case "highlighted":
                                t.GetChild(2).gameObject.SetActive(true);
                                foreach (Transform border in t.GetChild(2))
                                {
                                    border.GetComponent<MeshRenderer>().material.color = Color.green;
                                }
                                break;
                            case "":
                                t.GetChild(2).gameObject.SetActive(false);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            /////// Detailed Views //////
            // Find the line index of the detailed views data which has the same time as the current head transforms line
            for (int j = detailedViewsLineIndex; j < detailedViewsDataLines.Length; j++)
            {
                string[] detailedViewsLine = detailedViewData[j];

                if (currentTime == float.Parse(detailedViewsLine[0]))
                {
                    detailedViewsLineIndex = j;
                    currentQuestionID = int.Parse(detailedViewsLine[1]);

                    if (currentQuestionID != prevQuestionID)
                    { // Get current Detailed Views Objects
                        prevQuestionID = currentQuestionID;

                        // clear previous landmarks
                        foreach (Transform t in DetailedViewObjects)
                            Destroy(t.gameObject);
                        if (DetailedViewObjects.Length != 0)
                            Debug.Log("Clear Issue!!!");
                        WallDisplay.SetActive(false);

                        Transform detailedViewParent = DetailedViewsParent.Find(currentQuestionID.ToString());

                        // enable Frame of Reference Object
                        if (detailedViewsLine[3] == "Wall")
                        {
                            WallDisplay.SetActive(true);

                            int l = 0;
                            foreach (Transform t in detailedViewParent)
                            {
                                Transform newDV = Instantiate(t, Vector3.zero, Quaternion.identity, WallFOR);
                                newDV.name = t.name;

                                DetailedViewObjects[l] = newDV;
                                l++;
                            }
                        }
                    }
                    break;
                }
            }

            //Now that we've found it, set the properties of the Detailed Views
            for (int j = 0; j < DetailedViewObjects.Length; j++)
            {
                string[] detailedViewsLine = detailedViewData[detailedViewsLineIndex + j];

                foreach (Transform t in DetailedViewObjects)
                {
                    if (t.name == detailedViewsLine[2])
                    {
                        t.transform.position = new Vector3(float.Parse(detailedViewsLine[4]), float.Parse(detailedViewsLine[5]), float.Parse(detailedViewsLine[6]));
                        t.transform.rotation = new Quaternion(float.Parse(detailedViewsLine[7]), float.Parse(detailedViewsLine[8]),
                            float.Parse(detailedViewsLine[9]), float.Parse(detailedViewsLine[10]));
                        t.transform.localScale = new Vector3(float.Parse(detailedViewsLine[11]), float.Parse(detailedViewsLine[11]), float.Parse(detailedViewsLine[11]));

                        switch (detailedViewsLine[12])
                        {
                            case "selected":
                                t.GetChild(2).gameObject.SetActive(true);
                                foreach (Transform border in t.GetChild(2))
                                {
                                    border.GetComponent<MeshRenderer>().material.color = Color.blue;
                                }
                                break;
                            case "highlighted":
                                t.GetChild(2).gameObject.SetActive(true);
                                foreach (Transform border in t.GetChild(2))
                                {
                                    border.GetComponent<MeshRenderer>().material.color = Color.green;
                                }
                                break;
                            case "":
                                t.GetChild(2).gameObject.SetActive(false);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }


            yield return null;
        }

        isLiveReplayRunning = false;
        isLiveReplayPaused = false;
    }

    public void PauseLiveReplay()
    {
        isLiveReplayPaused = true;
    }

    public void RestartLiveReplay()
    {
        if (isLiveReplayRunning)
        {
            StopCoroutine(LiveReplay());
            isLiveReplayRunning = false;
        }

        StartLiveReplay();
    }
}
