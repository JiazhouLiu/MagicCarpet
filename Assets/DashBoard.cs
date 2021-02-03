using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashBoard : MonoBehaviour
{
    public Transform Camera;
    public float AdjustedHeight;
    public float ForwardParameter;
    public bool curved;

    // Update is called once per frame
    void Update()
    {
        transform.position = Camera.TransformPoint(Camera.localPosition + Vector3.forward * ForwardParameter);
        transform.position = new Vector3(transform.position.x, AdjustedHeight, transform.position.z);

        transform.LookAt(Camera);
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + 180, 0);

        if (curved)
        {
            foreach (Transform t in transform) {
                t.LookAt(Camera);
                t.localEulerAngles = new Vector3(0, t.localEulerAngles.y + 180, 0);
            }
        }
        else {
            foreach (Transform t in transform) {
                t.localEulerAngles = new Vector3(0, 0, 0);
            }
        }
    }
}
