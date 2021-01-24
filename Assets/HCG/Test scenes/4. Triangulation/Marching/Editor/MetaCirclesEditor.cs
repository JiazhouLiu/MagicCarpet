using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MetaCirclesController))]
public class MetaCirclesEditor : Editor
{
    private MetaCirclesController triangulatePoints;



    private void OnEnable()
    {
        triangulatePoints = target as MetaCirclesController;

        //Hide the main GOs move/rot/scale handle
        Tools.hidden = true;
    }



    private void OnDisable()
    {
        //Un-hide the main GOs move/ rot / scale handle
        Tools.hidden = false;
    }



    private void OnSceneGUI()
    {
        //So you we cant click on anything else in the scene
        HandleUtility.AddDefaultControl(0);
    }



    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        //Update when changing value in inspector
        if (base.DrawDefaultInspector())
        {
            triangulatePoints.GenerateMap();

            EditorUtility.SetDirty(target);
        }
        
        if (GUILayout.Button("Generate Metacircles"))
        {
            triangulatePoints.GenerateMap();

            EditorUtility.SetDirty(target);
        }
    }
}
