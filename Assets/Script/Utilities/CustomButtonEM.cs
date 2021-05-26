using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ExperimentManager))]
public class CustomButtonEM : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ExperimentManager myScript = (ExperimentManager)target;
        if (GUILayout.Button("Update"))
        {
            myScript.UpdateTrialID();
        }
    }

}
