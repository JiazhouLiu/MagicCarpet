using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using VRTK;

public class LogManager : MonoBehaviour
{
    public ExperimentManager EM;
    public DashboardController_UserStudy DC;
    public FootGestureController_UserStudy FC;
    public TaskManager TM;

    private int ParticipantID;
    private float lastTimePast;

    // body tracking
    private Transform leftHand;
    private VRTK_ControllerEvents leftControllerEvents;
    private Transform rightHand;
    private VRTK_ControllerEvents rightControllerEvents;
    private Transform waist;
    private Transform leftFoot;
    private Transform rightFoot;

    // update
    private StreamWriter writerRaw;
    private StreamWriter writerTrackedObj;
    private StreamWriter writerInteraction;
    private StreamWriter writerTask;

    // Start is called before the first frame update
    void Start()
    {
        ParticipantID = EM.ParticipantID;
        leftHand = EM.leftHand;
        rightHand = EM.rightHand;
        waist = EM.waist;
        leftFoot = EM.leftFoot;
        rightFoot = EM.rightFoot;
        leftControllerEvents = EM.leftControllerEvents;
        rightControllerEvents = EM.rightControllerEvents;

        string writerRawFilePath = "Assets/UserStudy/ExperimentData/ExperimentLog/Participant " + ParticipantID + "/Participant_" + ParticipantID + "_RawData.csv";
        string writerTrackedObjFilePath = "Assets/UserStudy/ExperimentData/ExperimentLog/Participant " + ParticipantID + "/Participant_" + ParticipantID + "_TrackedObj.csv";
        string writerInteractionFilePath = "Assets/UserStudy/ExperimentData/ExperimentLog/Participant " + ParticipantID + "/Participant_" + ParticipantID + "_Interaction.csv";
        string writerTaskFilePath = "Assets/UserStudy/ExperimentData/ExperimentLog/Participant " + ParticipantID + "/Participant_" + ParticipantID + "_TaskRelated.csv";

        if (EM.TrialNo == 0)
        {
            EM.TrialNo ++;
            // Raw data log
            writerRaw = new StreamWriter(writerRawFilePath, false);
            string rawFileHeader = "TimeSinceStart,TrialNo,TrialID,ParticipantID,Landmark,DetailedView,GrabbedVis1,GrabbedVis2,PinnedVis1,PinnedVis2,PinnedVis3," +
                "CameraPosition.x,CameraPosition.y,CameraPosition.z,CameraEulerAngles.x,CameraEulerAngles.y,CameraEulerAngles.z," +
                "LeftControllerPosition.x,LeftControllerPosition.y,LeftControllerPosition.z,LeftControllerEulerAngles.x,LeftControllerEulerAngles.y,LeftControllerEulerAngles.z," +
                "RightControllerPosition.x,RightControllerPosition.y,RightControllerPosition.z,RightControllerEulerAngles.x,RightControllerEulerAngles.y,RightControllerEulerAngles.z," +
                "WaistPosition.x,WaistPosition.y,WaistPosition.z,WaistEulerAngles.x,WaistEulerAngles.y,WaistEulerAngles.z," +
                "LeftShoePosition.x,LeftShoePosition.y,LeftShoePosition.z,LeftShoeEulerAngles.x,LeftShoeEulerAngles.y,LeftShoeEulerAngles.z," +
                "RightShoePosition.x,RightShoePosition.y,RightShoePosition.z,RightShoeEulerAngles.x,RightShoeEulerAngles.y,RightShoeEulerAngles.z," +
                "LeftGripPressed,LeftTriggerPressed,LeftFootSliding,LeftFootPressed,RightGripPressed,RightTriggerPressed,RightFootSliding,RightFootPressed";
            writerRaw.WriteLine(rawFileHeader);
            //writerRaw.Close();

            // tracked obj log
            writerTrackedObj = new StreamWriter(writerTrackedObjFilePath, false);
            string trackedObjFileHeader = "TimeSinceStart,TrialNo,TrialID,ParticipantID,Landmark,DetailedView," +
                "CameraPosition.x,CameraPosition.y,CameraPosition.z,CameraEulerAngles.x,CameraEulerAngles.y,CameraEulerAngles.z," +
                "LeftControllerPosition.x,LeftControllerPosition.y,LeftControllerPosition.z,LeftControllerEulerAngles.x,LeftControllerEulerAngles.y,LeftControllerEulerAngles.z," +
                "RightControllerPosition.x,RightControllerPosition.y,RightControllerPosition.z,RightControllerEulerAngles.x,RightControllerEulerAngles.y,RightControllerEulerAngles.z," +
                "WaistPosition.x,WaistPosition.y,WaistPosition.z,WaistEulerAngles.x,WaistEulerAngles.y,WaistEulerAngles.z," +
                "LeftShoePosition.x,LeftShoePosition.y,LeftShoePosition.z,LeftShoeEulerAngles.x,LeftShoeEulerAngles.y,LeftShoeEulerAngles.z," +
                "RightShoePosition.x,RightShoePosition.y,RightShoePosition.z,RightShoeEulerAngles.x,RightShoeEulerAngles.y,RightShoeEulerAngles.z," +
                "LeftGripPressed,LeftTriggerPressed,LeftFootSliding,LeftFootPressed,RightGripPressed,RightTriggerPressed,RightFootSliding,RightFootPressed";
            writerTrackedObj.WriteLine(trackedObjFileHeader);
            //writerTrackedObj.Close();

            // interaction log
            writerInteraction = new StreamWriter(writerInteractionFilePath, false);
            string interactionFileHeader = "TimeSinceStart,TrialNo,TrialID,ParticipantID,Landmark,DetailedView,Interaction";
            writerInteraction.WriteLine(interactionFileHeader);
            //writerInteraction.Close();

            // Answers data log
            writerTask = new StreamWriter(writerTaskFilePath, false);
            string taskFileHeader = "TimeSinceStart,TrialNo,TrialID,ParticipantID,Landmark,DetailedView," +
                "Landmark1Name,Landmark2Name,Landmark3Name,Landmark4Name,Landmark5Name,Landmark6Name," +
                "Landmark1Pos,Landmark2Pos,Landmark3Pos,Landmark4Pos,Landmark5Pos,Landmark6Pos," +
                "Landmark1Rot,Landmark2Rot,Landmark3Rot,Landmark4Rot,Landmark5Rot,Landmark6Rot," +
                "Landmark1State,Landmark2State,Landmark3State,Landmark4State,Landmark5State,Landmark6State";
            writerTask.WriteLine(taskFileHeader);
            //writerTask.Close();
        }
        else
        {
            string lastFileName = "";

            string folderPath = "Assets/UserStudy/ExperimentData/ExperimentLog/Participant " + ParticipantID + "/";
            DirectoryInfo info = new DirectoryInfo(folderPath);
            FileInfo[] fileInfo = info.GetFiles();

            // get file name
            foreach (FileInfo file in fileInfo)
            {
                if (file.Name.Contains("Participant_" + ParticipantID + "_RawData.csv") && !file.Name.Contains("meta"))
                    lastFileName = file.Name;
            }

            // get last run time stamp
            if (lastFileName == "")
                Debug.LogError("No previous file found!");
            else
            {
                string writerFilePath = "Assets/UserStudy/ExperimentData/ExperimentLog/Participant " + ParticipantID + "/" + lastFileName;
                string lastLine = File.ReadAllLines(writerFilePath)[File.ReadAllLines(writerFilePath).Length - 1];
                float lastTime = float.Parse(lastLine.Split(',')[0]);
                lastTimePast = lastTime;
            }

            // setup writers
            writerRaw = new StreamWriter(writerRawFilePath, true);
            writerTrackedObj = new StreamWriter(writerTrackedObjFilePath, true);
            writerInteraction = new StreamWriter(writerInteractionFilePath, true);
            writerTask = new StreamWriter(writerTaskFilePath, true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        WritingToLog();
    }

    private void WritingToLog()
    {
        if (writerRaw != null && Camera.main != null && leftHand != null && rightHand != null)
        {
            writerRaw.WriteLine(GetFixedTime() +  "," + GetTrialNumber() + "," + GetTrialID() + "," + EM.ParticipantID + "," + GetLandmark() + "," + GetDetailedView() + "," +
                GetGrabbedVis() + "," + GetPinnedVis() + "," + 
                VectorToString(Camera.main.transform.position) + "," + VectorToString(Camera.main.transform.eulerAngles) + "," +
                VectorToString(leftHand.position) + "," + VectorToString(leftHand.eulerAngles) + "," +
                VectorToString(rightHand.position) + "," + VectorToString(rightHand.eulerAngles) + "," +
                VectorToString(waist.position) + "," + VectorToString(waist.eulerAngles) + "," +
                VectorToString(leftFoot.position) + "," + VectorToString(leftFoot.eulerAngles) + "," +
                VectorToString(rightFoot.position) + "," + VectorToString(rightFoot.eulerAngles) + "," +
                GetLeftGripPressed() + "," + GetLeftTriggerPressed() + "," + GetLeftFootSliding() + "," + GetLeftFootPressed() + "," +
                GetRightGripPressed() + "," + GetRightTriggerPressed() + "," + GetRightFootSliding() + "," + GetRightFootPressed());
            writerRaw.Flush();
        }

        if (writerTrackedObj != null && Camera.main != null && leftHand != null && rightHand != null)
        {
            writerTrackedObj.WriteLine(GetFixedTime() + "," + GetTrialNumber() + "," + GetTrialID() + "," + EM.ParticipantID + "," + GetLandmark() + "," + GetDetailedView() + "," +
                VectorToString(Camera.main.transform.position) + "," + VectorToString(Camera.main.transform.eulerAngles) + "," +
                VectorToString(leftHand.position) + "," + VectorToString(leftHand.eulerAngles) + "," +
                VectorToString(rightHand.position) + "," + VectorToString(rightHand.eulerAngles) + "," +
                VectorToString(waist.position) + "," + VectorToString(waist.eulerAngles) + "," +
                VectorToString(leftFoot.position) + "," + VectorToString(leftFoot.eulerAngles) + "," +
                VectorToString(rightFoot.position) + "," + VectorToString(rightFoot.eulerAngles) + "," +
                GetLeftGripPressed() + "," + GetLeftTriggerPressed() + "," + GetLeftFootSliding() + "," + GetLeftFootPressed() + "," +
                GetRightGripPressed() + "," + GetRightTriggerPressed() + "," + GetRightFootSliding() + "," + GetRightFootPressed());
            writerTrackedObj.Flush();
        }

        if (writerTask != null)
        {
            writerTask.WriteLine(GetFixedTime() + "," + GetTrialNumber() + "," + GetTrialID() + "," + EM.ParticipantID + "," + GetLandmark() + "," + GetDetailedView() + "," +
                GetLandmarkName() + "," + GetLandmarkPosition() + "," + GetLandmarkRotation() + "," + GetLandmarkState());
            writerTask.Flush();
        }
    }

    public void WriteInteractionToLog(string info)
    {
        if (writerInteraction != null)
        {
            writerInteraction.WriteLine(GetFixedTime() + "," + GetTrialNumber() + "," + GetTrialID() + "," + EM.ParticipantID + "," + GetLandmark() + "," + GetDetailedView() + "," +
                   info);
            writerInteraction.Flush();
        }
    }

    #region get log details
    float GetFixedTime()
    {
        float finalTime = 0;
        if (lastTimePast != 0)
            finalTime = lastTimePast + Time.fixedTime;
        else
            finalTime = Time.fixedTime;
        return finalTime;
    }

    private string GetTrialNumber()
    {
        return EM.TrialNo.ToString();
    }

    private string GetTrialID()
    {
        return EM.GetTrialID();
    }

    private string GetLandmark()
    {
        return EM.GetCurrentLandmarkFOR().ToString();
    }

    private string GetDetailedView()
    {
        return EM.GetCurrentDetailedViewFOR().ToString();
    }

    private string GetGrabbedVis() 
    {
        return DC.GrabbedVis;
    }

    private string GetPinnedVis()
    {
        return DC.PinnedVis;
    }

    private bool GetLeftTriggerPressed()
    {
        if (leftControllerEvents.triggerClicked)
            return true;
        else
            return false;
    }

    private bool GetLeftGripPressed()
    {
        if (leftControllerEvents.gripPressed)
            return true;
        else
            return false;
    }

    private bool GetLeftFootSliding() {
        return FC.leftHoldingFlag;
    }

    private bool GetLeftFootPressed() {
        return FC.leftNormalPressFlag;
    }

    private bool GetRightTriggerPressed()
    {
        if (rightControllerEvents.triggerClicked)
            return true;
        else
            return false;
    }

    private bool GetRightGripPressed()
    {
        if (rightControllerEvents.gripPressed)
            return true;
        else
            return false;
    }

    private bool GetRightFootSliding()
    {
        return FC.rightHoldingFlag;
    }

    private bool GetRightFootPressed()
    {
        return FC.rightNormalPressFlag;
    }

    private string GetLandmarkName()
    {
        return DC.LandmarkNames;
    }

    private string GetLandmarkPosition() {
        return DC.LandmarkPositions;
    }

    private string GetLandmarkRotation()
    {
        return DC.LandmarkRotations;
    }

    private string GetLandmarkState()
    {
        return DC.LandmarkState;
    }

    string VectorToString(Vector3 v)
    {
        string text;
        text = v.x + "," + v.y + "," + v.z;
        return text;
    }
    #endregion

    public void QuitGame()
    {
        // save any game data here
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        ClearWriter();
        UnityEditor.EditorApplication.isPlaying = false;
#else
         ClearWriter();
         Application.Quit();
#endif
    }

    private void ClearWriter() {
        writerRaw.Close();
        writerTrackedObj.Close();
        writerInteraction.Close();
        writerTask.Close();
    }

    private void OnDestroy()
    {
        ClearWriter();
    }
}
