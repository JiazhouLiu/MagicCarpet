using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ShoeRecieve))]
public class CustomButton : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ShoeRecieve myScript = (ShoeRecieve)target;
        if (GUILayout.Button("Abort Thread"))
        {
            myScript.AbortThread();
        }
    }

}