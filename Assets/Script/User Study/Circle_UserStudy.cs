using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Circle_UserStudy : MonoBehaviour
{
    public Transform HeadDashboard;
    public Transform WaistDashboard;
    public int vertexCount = 1000;
    public float lineWidth = 0f;
    public DisplayDashboard display = new DisplayDashboard();

    private float radius;
    private LineRenderer lineRenderer;
    private int prevVertexCount;

    // Start is called before the first frame update
    private void Awake()
    {
        if (display == DisplayDashboard.HeadDisplay)
        {
            radius = HeadDashboard.GetComponent<ReferenceFrameController_UserStudy>().ForwardParameter;

            if (HeadDashboard.childCount > 1)
            {
                vertexCount = HeadDashboard.childCount * 2 - 2;
            }
        }
        else if (display == DisplayDashboard.WaistDisplay)
        {
            radius = WaistDashboard.GetComponent<ReferenceFrameController_UserStudy>().ForwardParameter;

            if (WaistDashboard.childCount > 1)
            {
                vertexCount = WaistDashboard.childCount * 2 - 2;
            }
        }

        prevVertexCount = vertexCount;
        lineRenderer = GetComponent<LineRenderer>();
        SetupCircle();
    }

    private void Update()
    {
        if (display == DisplayDashboard.HeadDisplay)
        {
            radius = HeadDashboard.GetComponent<ReferenceFrameController_UserStudy>().ForwardParameter;

            if (HeadDashboard.childCount > 1)
                vertexCount = HeadDashboard.childCount * 2 - 2;
        }
        else if (display == DisplayDashboard.WaistDisplay)
        {
            radius = WaistDashboard.GetComponent<ReferenceFrameController_UserStudy>().ForwardParameter;

            if (WaistDashboard.childCount > 1)
                vertexCount = WaistDashboard.transform.childCount * 2 - 2;
        }

        if (prevVertexCount != vertexCount)
        {
            SetupCircle();
            prevVertexCount = vertexCount;
        }

    }

    private void SetupCircle()
    {
        lineRenderer.widthMultiplier = lineWidth;

        float deltaTheta = (2f * Mathf.PI) / vertexCount;
        float theta = 0;

        lineRenderer.positionCount = vertexCount;
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            Vector3 pos = new Vector3(radius * Mathf.Cos(theta), 0f, radius * Mathf.Sin(theta));
            lineRenderer.SetPosition(i, pos);
            theta += deltaTheta;
        }
    }
}
