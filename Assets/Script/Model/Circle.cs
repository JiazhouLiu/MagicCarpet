﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DisplayDashboard // your custom enumeration
{
    HeadDisplay,
    WaistDisplay
};

[RequireComponent(typeof(LineRenderer))]
public class Circle : MonoBehaviour
{
    public int vertexCount = 100;
    public float lineWidth = 0.2f;
    public DisplayDashboard display = new DisplayDashboard();

    private float radius;

    private LineRenderer lineRenderer;

    private int prevVertexCount;
    // Start is called before the first frame update
    private void Awake()
    {

        if (GameObject.Find("SmallMultiples") != null)
            radius = GameObject.Find("SmallMultiples").GetComponent<ViewController>().Radius;
        else {
            if (display == DisplayDashboard.HeadDisplay)
            {
                radius = GameObject.Find("DashBoard - Head Level").GetComponent<DashBoard_New>().ForwardParameter;
                if (GameObject.Find("DashBoard - Head Level").transform.childCount > 1)
                {
                    vertexCount = GameObject.Find("DashBoard - Head Level").transform.childCount * 2 - 2;
                }
            }
            else {
                radius = GameObject.Find("DashBoard - Waist Level").GetComponent<DashBoard_New>().ForwardParameter;
                if (GameObject.Find("DashBoard - Waist Level").transform.childCount > 1)
                {
                    vertexCount = GameObject.Find("DashBoard - Waist Level").transform.childCount * 2 - 2;
                }
            }
        }

        prevVertexCount = vertexCount;
        lineRenderer = GetComponent<LineRenderer>();
        SetupCircle();
    }

    private void Update()
    {
        if (GameObject.Find("SmallMultiples") != null)
            radius = GameObject.Find("SmallMultiples").GetComponent<ViewController>().Radius;
        else
        {
            if (display == DisplayDashboard.HeadDisplay)
            {
                radius = GameObject.Find("DashBoard - Head Level").GetComponent<DashBoard_New>().ForwardParameter;
                if (GameObject.Find("DashBoard - Head Level").transform.childCount > 1)
                {
                    vertexCount = GameObject.Find("DashBoard - Head Level").transform.childCount * 2 - 2;
                }
            }
            else
            {
                radius = GameObject.Find("DashBoard - Waist Level").GetComponent<DashBoard_New>().ForwardParameter;
                if (GameObject.Find("DashBoard - Waist Level").transform.childCount > 1)
                {
                    vertexCount = GameObject.Find("DashBoard - Waist Level").transform.childCount * 2 - 2;
                }
            }
        }
        if (prevVertexCount != vertexCount) {
            SetupCircle();
            prevVertexCount = vertexCount;
        }
            
    }

    private void SetupCircle() {
        lineRenderer.widthMultiplier = lineWidth;

        float deltaTheta = (2f * Mathf.PI) / vertexCount;
        float theta = 0;

        lineRenderer.positionCount = vertexCount;
        for (int i = 0; i < lineRenderer.positionCount; i++) {
            Vector3 pos = new Vector3(radius * Mathf.Cos(theta), 0f, radius * Mathf.Sin(theta));
            lineRenderer.SetPosition(i, pos);
            theta += deltaTheta;
        }
    }

//#if UNITY_EDITOR
//    private void OnDrawGizmos()
//    {
//        float deltaTheta = (2f * Mathf.PI) / vertexCount;
//        float theta = 0f;

//        Vector3 oldPos = transform.position;
//        for (int i = 0; i < vertexCount + 1; i++)
//        {
//            Vector3 pos = new Vector3(radius * Mathf.Cos(theta), 0f, radius * Mathf.Sin(theta));
//            Gizmos.DrawLine(oldPos, transform.position + pos);

//            theta += deltaTheta;
//        }
//    }
//#endif
}
