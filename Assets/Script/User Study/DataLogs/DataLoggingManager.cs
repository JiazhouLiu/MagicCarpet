using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataLoggingManager : MonoBehaviour
{
    [Header("Logging Settings")]
    public string FilePath;
    public string ParticipantID;
    public float TimeBetweenLogs = 0.05f;

    [Header("Logged Objects")]
    public Transform HeadTransform;
    public Transform LeftControllerTransform;
    public Transform RightControllerTransform;
    public Transform WaistTransform;
    public Transform LeftFootTransform;
    public Transform RightFootTransform;
    public Transform TaskBoardTransform;
    public Transform TableTopTransform;
    public Transform CameraTransform;

    [Header("Reference")]
    public DashboardController_UserStudy DC;
    public ExperimentManager EM;

    [HideInInspector]
    public bool IsLogging = false;

    private StreamWriter headTransformStreamWriter;
    private StreamWriter leftControllerTransformStreamWriter;
    private StreamWriter rightControllerTransformStreamWriter;
    private StreamWriter waistTransformStreamWriter;
    private StreamWriter leftFootTransformStreamWriter;
    private StreamWriter rightFootTransformStreamWriter;
    private StreamWriter taskBoardTransformStreamWriter;
    private StreamWriter cameraTransformStreamWriter;
    private StreamWriter landmarksTransformStreamWriter;
    private StreamWriter detailedViewsTransformStreamWriter;
    private float startTime;
    private float timer = 0f;
    private const string format = "F4";

    private void Start()
    {
        if (!FilePath.EndsWith("/"))
            FilePath += "/";
    }

    public void StartLogging()
    {
        // Head transform
        string path = string.Format("{0}P{1}_HeadTransform.txt", FilePath, ParticipantID);
        headTransformStreamWriter = new StreamWriter(path, true);
        headTransformStreamWriter.WriteLine("Timestamp\tPosition.x\tPosition.y\tPosition.z\tRotation.x\tRotation.y\tRotation.z\tRotation.w");

        // Left Controller transform
        path = string.Format("{0}P{1}_LeftControllerTransform.txt", FilePath, ParticipantID);
        leftControllerTransformStreamWriter = new StreamWriter(path, true);
        leftControllerTransformStreamWriter.WriteLine("Timestamp\tPosition.x\tPosition.y\tPosition.z\tRotation.x\tRotation.y\tRotation.z\tRotation.w");

        // Right Controller transform
        path = string.Format("{0}P{1}_RightControllerTransform.txt", FilePath, ParticipantID);
        rightControllerTransformStreamWriter = new StreamWriter(path, true);
        rightControllerTransformStreamWriter.WriteLine("Timestamp\tPosition.x\tPosition.y\tPosition.z\tRotation.x\tRotation.y\tRotation.z\tRotation.w");

        // Waist transform
        path = string.Format("{0}P{1}_WaistTransform.txt", FilePath, ParticipantID);
        waistTransformStreamWriter = new StreamWriter(path, true);
        waistTransformStreamWriter.WriteLine("Timestamp\tPosition.x\tPosition.y\tPosition.z\tRotation.x\tRotation.y\tRotation.z\tRotation.w");

        // Left Foot transform
        path = string.Format("{0}P{1}_LeftFootTransform.txt", FilePath, ParticipantID);
        leftFootTransformStreamWriter = new StreamWriter(path, true);
        leftFootTransformStreamWriter.WriteLine("Timestamp\tPosition.x\tPosition.y\tPosition.z\tRotation.x\tRotation.y\tRotation.z\tRotation.w");

        // Right Foot transform
        path = string.Format("{0}P{1}_RightFootTransform.txt", FilePath, ParticipantID);
        rightFootTransformStreamWriter = new StreamWriter(path, true);
        rightFootTransformStreamWriter.WriteLine("Timestamp\tPosition.x\tPosition.y\tPosition.z\tRotation.x\tRotation.y\tRotation.z\tRotation.w");

        // Task Board transform
        path = string.Format("{0}P{1}_TaskBoardTransform.txt", FilePath, ParticipantID);
        taskBoardTransformStreamWriter = new StreamWriter(path, true);
        taskBoardTransformStreamWriter.WriteLine("Timestamp\tPosition.x\tPosition.y\tPosition.z\tRotation.x\tRotation.y\tRotation.z\tRotation.w");

        // Exteral camera transform
        path = string.Format("{0}P{1}_CameraTransform.txt", FilePath, ParticipantID);
        cameraTransformStreamWriter = new StreamWriter(path, true);
        cameraTransformStreamWriter.WriteLine("Timestamp\tPosition.x\tPosition.y\tPosition.z\tRotation.x\tRotation.y\tRotation.z\tRotation.w");

        // Landmarks transforms and properties
        path = string.Format("{0}P{1}_Landmarks.txt", FilePath, ParticipantID);
        landmarksTransformStreamWriter = new StreamWriter(path, true);
        landmarksTransformStreamWriter.WriteLine("Timestamp\tQuestionID\tName\tFOR\tPosition.x\tPosition.y\tPosition.z\tRotation.x\tRotation.y\tRotation.z\tRotation.w\tScaleP\tStatus\tTableHeight");

        // Detailed View transforms and properties
        path = string.Format("{0}P{1}_DetailedViews.txt", FilePath, ParticipantID);
        detailedViewsTransformStreamWriter = new StreamWriter(path, true);
        detailedViewsTransformStreamWriter.WriteLine("Timestamp\tQuestionID\tName\tFOR\tPosition.x\tPosition.y\tPosition.z\tRotation.x\tRotation.y\tRotation.z\tRotation.w\tScaleP\tStatus");


        IsLogging = true;
        startTime = Time.time;

        Debug.Log("Logging Started");
    }

    public void FixedUpdate()
    {
        if (IsLogging)
        {
            timer += Time.fixedDeltaTime;
            if (timer >= TimeBetweenLogs)
            {
                timer = 0;
                LogData();
            }
        }
    }

    public void StopLogging()
    {
        IsLogging = false;
        timer = 0;

        headTransformStreamWriter.Close();
        leftControllerTransformStreamWriter.Close();
        rightControllerTransformStreamWriter.Close();
        waistTransformStreamWriter.Close();
        leftFootTransformStreamWriter.Close();
        rightFootTransformStreamWriter.Close();
        taskBoardTransformStreamWriter.Close();
        cameraTransformStreamWriter.Close();
        landmarksTransformStreamWriter.Close();
        detailedViewsTransformStreamWriter.Close();

        Debug.Log("Logging Stopped");
    }

    private void LogData()
    {
        float timestamp = Time.time - startTime;

        // head log
        headTransformStreamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
            timestamp,
            HeadTransform.position.x.ToString(format),
            HeadTransform.position.y.ToString(format),
            HeadTransform.position.z.ToString(format),
            HeadTransform.rotation.x.ToString(format),
            HeadTransform.rotation.y.ToString(format),
            HeadTransform.rotation.z.ToString(format),
            HeadTransform.rotation.w.ToString(format)
        );

        // left controller log
        leftControllerTransformStreamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
            timestamp,
            LeftControllerTransform.position.x.ToString(format),
            LeftControllerTransform.position.y.ToString(format),
            LeftControllerTransform.position.z.ToString(format),
            LeftControllerTransform.rotation.x.ToString(format),
            LeftControllerTransform.rotation.y.ToString(format),
            LeftControllerTransform.rotation.z.ToString(format),
            LeftControllerTransform.rotation.w.ToString(format)
        );

        // right controller log
        rightControllerTransformStreamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
            timestamp,
            RightControllerTransform.position.x.ToString(format),
            RightControllerTransform.position.y.ToString(format),
            RightControllerTransform.position.z.ToString(format),
            RightControllerTransform.rotation.x.ToString(format),
            RightControllerTransform.rotation.y.ToString(format),
            RightControllerTransform.rotation.z.ToString(format),
            RightControllerTransform.rotation.w.ToString(format)
        );

        // waist log
        waistTransformStreamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
            timestamp,
            WaistTransform.position.x.ToString(format),
            WaistTransform.position.y.ToString(format),
            WaistTransform.position.z.ToString(format),
            WaistTransform.rotation.x.ToString(format),
            WaistTransform.rotation.y.ToString(format),
            WaistTransform.rotation.z.ToString(format),
            WaistTransform.rotation.w.ToString(format)
        );

        // left foot log
        leftFootTransformStreamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
            timestamp,
            LeftFootTransform.position.x.ToString(format),
            LeftFootTransform.position.y.ToString(format),
            LeftFootTransform.position.z.ToString(format),
            LeftFootTransform.rotation.x.ToString(format),
            LeftFootTransform.rotation.y.ToString(format),
            LeftFootTransform.rotation.z.ToString(format),
            LeftFootTransform.rotation.w.ToString(format)
        );

        // right foot log
        rightFootTransformStreamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
            timestamp,
            RightFootTransform.position.x.ToString(format),
            RightFootTransform.position.y.ToString(format),
            RightFootTransform.position.z.ToString(format),
            RightFootTransform.rotation.x.ToString(format),
            RightFootTransform.rotation.y.ToString(format),
            RightFootTransform.rotation.z.ToString(format),
            RightFootTransform.rotation.w.ToString(format)
        );

        // task board log
        taskBoardTransformStreamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
            timestamp,
            TaskBoardTransform.position.x.ToString(format),
            TaskBoardTransform.position.y.ToString(format),
            TaskBoardTransform.position.z.ToString(format),
            TaskBoardTransform.rotation.x.ToString(format),
            TaskBoardTransform.rotation.y.ToString(format),
            TaskBoardTransform.rotation.z.ToString(format),
            TaskBoardTransform.rotation.w.ToString(format)
        );

        // external camera log
        cameraTransformStreamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
           timestamp,
           CameraTransform.position.x.ToString(format),
           CameraTransform.position.y.ToString(format),
           CameraTransform.position.z.ToString(format),
           CameraTransform.rotation.x.ToString(format),
           CameraTransform.rotation.y.ToString(format),
           CameraTransform.rotation.z.ToString(format),
           CameraTransform.rotation.w.ToString(format)
        );

        // landmarks log
        if (DC.currentLandmarks != null && DC.currentLandmarks.Count > 0)
        {
            for (int i = 0; i < DC.currentLandmarks.Count; i++)
            {
                Transform landmark = DC.currentLandmarks.Values.ToList()[i];

                int questionID = EM.QuestionID;
                string name = DC.currentLandmarks.Keys.ToList()[i];

                string status = "";
                if (landmark.GetComponent<Vis>().Moving)
                    status = "moving";
                else if (landmark.GetComponent<Vis>().Selected)
                    status = "selected";
                else if (landmark.GetComponent<Vis>().Highlighted)
                    status = "highlighted";

                string landmarkFOR = "";
                switch (DC.Landmark)
                {
                    case ReferenceFrames.Body:
                        landmarkFOR = "Body";
                        break;
                    case ReferenceFrames.Floor:
                        landmarkFOR = "Floor";
                        break;
                    case ReferenceFrames.Shelves:
                        landmarkFOR = "Tabletop";
                        break;
                    default:
                        break;
                }

                landmarksTransformStreamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}",
                    timestamp,
                    questionID,
                    name,
                    landmarkFOR,
                    landmark.position.x.ToString(format),
                    landmark.position.y.ToString(format),
                    landmark.position.z.ToString(format),
                    landmark.rotation.x.ToString(format),
                    landmark.rotation.y.ToString(format),
                    landmark.rotation.z.ToString(format),
                    landmark.rotation.w.ToString(format),
                    landmark.localScale.x.ToString(format),
                    status,
                    TableTopTransform.position.y
                );
            }
            landmarksTransformStreamWriter.Flush();
        }

        // detailed views log
        if (DC.currentDetailedViews != null && DC.currentDetailedViews.Count > 0)
        {
            for (int i = 0; i < DC.currentDetailedViews.Count; i++)
            {
                Transform detailedView = DC.currentDetailedViews.Values.ToList()[i];

                int questionID = EM.QuestionID;
                string name = DC.currentDetailedViews.Keys.ToList()[i];

                string status = "";
                if (detailedView.GetComponent<Vis>().Selected)
                    status = "selected";
                else if (detailedView.GetComponent<Vis>().Highlighted)
                    status = "highlighted";

                string detailedViewFOR = "";
                switch (DC.DetailedView)
                {
                    case ReferenceFrames.Body:
                        detailedViewFOR = "Body";
                        break;
                    case ReferenceFrames.Shelves:
                        detailedViewFOR = "Wall";
                        break;
                    default:
                        break;
                }

                detailedViewsTransformStreamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}",
                    timestamp,
                    questionID,
                    name,
                    detailedViewFOR,
                    detailedView.position.x.ToString(format),
                    detailedView.position.y.ToString(format),
                    detailedView.position.z.ToString(format),
                    detailedView.rotation.x.ToString(format),
                    detailedView.rotation.y.ToString(format),
                    detailedView.rotation.z.ToString(format),
                    detailedView.rotation.w.ToString(format),
                    detailedView.localScale.x.ToString(format),
                    status
                );
            }
            detailedViewsTransformStreamWriter.Flush();
        }

        headTransformStreamWriter.Flush();
        leftControllerTransformStreamWriter.Flush();
        rightControllerTransformStreamWriter.Flush();
        waistTransformStreamWriter.Flush();
        leftFootTransformStreamWriter.Flush();
        rightFootTransformStreamWriter.Flush();
        taskBoardTransformStreamWriter.Flush();
        cameraTransformStreamWriter.Flush();
    }

    public void OnApplicationQuit()
    {
        if (IsLogging)
            StopLogging();
    }
}
