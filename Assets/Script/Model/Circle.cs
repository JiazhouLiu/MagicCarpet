using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Circle : MonoBehaviour
{
    public int vertexCount = 100;
    public float lineWidth = 0.2f;
    
    private float radius;

    private LineRenderer lineRenderer;
    private ViewController VC;
    // Start is called before the first frame update
    private void Awake()
    {
        VC = GameObject.Find("SmallMultiples").GetComponent<ViewController>();
        radius = VC.Radius;

        lineRenderer = GetComponent<LineRenderer>();
        SetupCircle();
    }

    private void Update()
    {
        radius = VC.Radius;
        SetupCircle();
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
