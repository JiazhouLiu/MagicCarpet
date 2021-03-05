using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashBoard : MonoBehaviour
{
    public DelaunayController dc;
    public float AdjustedHeight;
    public float ForwardParameter;
    public bool curved;
    public bool lerp;
    public float upAngle;
    public float size;
    

    public Transform Camera;

    // Update is called once per frame
    void Update()
    {
        // sync Camera/Human Game Object
        if (dc != null && Camera == null)
            Camera = dc.Human;
        if (Camera != null)
        {
            transform.localScale = new Vector3(size, size, size);
            Vector3 oldAngle = Camera.transform.eulerAngles;
            Camera.transform.eulerAngles = new Vector3(0, oldAngle.y, oldAngle.z);
            Vector3 forward = Camera.transform.forward;
            Camera.transform.eulerAngles = oldAngle;
            // configure dashboard position (TODO: fix slope bug [head up and vis comes closer])
            if (lerp) transform.position = Vector3.Lerp(transform.position, Camera.TransformPoint(Vector3.zero) + forward * ForwardParameter, Time.deltaTime * 3);
            else transform.position = Camera.TransformPoint(Vector3.zero) + forward * ForwardParameter;
            //transform.position = Camera.TransformPoint(Vector3.zero) + forward * ForwardParameter;
            //transform.position = Camera.TransformPoint(Camera.localPosition + Vector3.forward * ForwardParameter);
            transform.position = new Vector3(transform.position.x, Camera.transform.position.y + AdjustedHeight, transform.position.z);

            // configure dashboard rotation
            transform.LookAt(Camera);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + 180, 0);

            // configure curved display (vis)
            if (curved)
            {
                foreach (Transform t in transform)
                {
                    t.LookAt(Camera);
                    t.localEulerAngles = new Vector3(upAngle, t.localEulerAngles.y + 180, -t.localEulerAngles.z);
                }
            }
            /*else
             {
                 foreach (Transform t in transform)
                 {
                     t.localEulerAngles = new Vector3(0, 0, 0);
                 }
             }*/
        }

    }
}


