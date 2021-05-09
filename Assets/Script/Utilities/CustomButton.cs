using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(DashboardController_UserStudy))]
public class CustomButton : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DashboardController_UserStudy myScript = (DashboardController_UserStudy)target;
        if (GUILayout.Button("Get Shoulder Position"))
        {
            myScript.GetShoulderPosition();
        }

        if (GUILayout.Button("Get Arm Length"))
        {
            myScript.GetArmLength();
        }
    }

}