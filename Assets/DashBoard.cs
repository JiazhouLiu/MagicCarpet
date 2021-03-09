using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashBoard : MonoBehaviour
{
    public VisController dc;
    public float AdjustedHeight;
    public float ForwardParameter;
    public bool curved;

    private Transform Camera;

    // Update is called once per frame
    void Update()
    {
        // sync Camera/Human Game Object
        if (dc != null && Camera == null)
            //Camera = dc.Human;
        if (Camera != null) {
            // configure dashboard position (TODO: fix slope bug [head up and vis comes closer])
            transform.position = Vector3.Lerp(transform.position, Camera.TransformPoint(Camera.localPosition + Vector3.forward * ForwardParameter), Time.deltaTime * dc.speed) ;
            //transform.position = Camera.TransformPoint(Camera.localPosition + Vector3.forward * ForwardParameter);
            transform.position = new Vector3(transform.position.x, AdjustedHeight, transform.position.z);

            // configure dashboard rotation
            transform.LookAt(Camera);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + 180, 0);

            // configure curved display (vis)
            if (curved)
            {
                foreach (Transform t in transform)
                {
                    t.LookAt(Camera);
                    t.localEulerAngles = new Vector3(0, t.localEulerAngles.y + 180, 0);
                }
            }
            else
            {
                foreach (Transform t in transform)
                {
                    t.localEulerAngles = new Vector3(0, 0, 0);
                }
            }
        }
        
    }
}
