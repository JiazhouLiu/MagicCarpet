using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothVertex : MonoBehaviour
{
    Cloth cloth;
    Vector3[] vertexList;
    List<GameObject> points;
    // Start is called before the first frame update
    void Start()
    {
        cloth = GetComponent<Cloth>();
        vertexList = cloth.vertices;
        points = new List<GameObject>();

        for (int i = 0; i < vertexList.Length; i++)
        {
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.localScale = Vector3.one * 0.5f;
            point.transform.localEulerAngles = Vector3.zero;
            point.transform.parent = transform;


            point.transform.localPosition = vertexList[i];
            points.Add(point);
        }
    }

    // Update is called once per frame
    void Update()
    {
        vertexList = cloth.vertices;
        for (int i = 0; i < vertexList.Length; i++)
        {
            GameObject point = points[i];
            point.transform.localPosition = vertexList[i];
        }
    }
}
