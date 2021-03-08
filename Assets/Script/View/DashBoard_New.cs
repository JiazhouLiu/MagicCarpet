using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashBoard_New : MonoBehaviour
{
    public VisController vc;
    //public float AdjustedHeight;
    public float ForwardParameter;
    public bool curved;
    public bool lerp;
    //public float upAngle;
    public float size;
    public float visSizeDelta; // adjust image size to normal
    public float HSpacing;
    public float animationSpeed;

    [Header("Circle")]
    public LineRenderer circle;

    private Transform CameraTransform;
    private Transform WaistTransform;

    private float displayCircleFullArc = 0;
    private float displayCircleRadius = 0;
    private float visDistanceInCircle = 0;
    private float smallestAngle = 180;

    //private int prevChildCount = 0;

    private List<Vector3> circlePointPositions;
    private List<Vector3> visPositions;

    private void Awake()
    {
        // sync Camera/Human Game Object
        WaistTransform = vc.HumanWaist;

        circlePointPositions = new List<Vector3>();
        visPositions = new List<Vector3>();

        transform.localScale = new Vector3(size, size, size);
    }

    // Update is called once per frame
    void Update()
    {
        // main camera setup
        if (Camera.main != null && CameraTransform == null)
            CameraTransform = Camera.main.transform;

        foreach (Transform t in transform)
            t.localScale = Vector3.one * 0.1f;

        if (name.Contains("Waist"))
        { // script for waist-level dashboard
            // configure curved display (vis)
            if (curved)
            {
                // configure dashboard position
                if (lerp) transform.position = Vector3.Lerp(transform.position, WaistTransform.position, 
                    Time.deltaTime * animationSpeed);
                else transform.position = WaistTransform.position;

                //transform.position = new Vector3(transform.position.x, CameraTransform.transform.position.y + AdjustedHeight, transform.position.z);

                

                foreach (Transform t in transform)
                {
                    UpdateVisPosition(t);

                    t.LookAt(CameraTransform);
                    t.localEulerAngles = new Vector3(-t.localEulerAngles.x, t.localEulerAngles.y + 180, 0);
                }


                // configure dashboard rotation (body-fixed)
                transform.localEulerAngles = WaistTransform.localEulerAngles;

            }
            else {
                Vector3 forward = WaistTransform.forward;

                // configure dashboard position
                //if (lerp) transform.position = Vector3.Lerp(transform.position, 
                //    WaistTransform.TransformPoint(Vector3.zero) + forward * ForwardParameter, Time.deltaTime * animationSpeed);
                //else
                    transform.position = WaistTransform.TransformPoint(Vector3.zero) + forward * ForwardParameter;

                //transform.position = new Vector3(transform.position.x, CameraTransform.transform.position.y + AdjustedHeight, transform.position.z);

                // configure dashboard rotation
                transform.LookAt(WaistTransform);
                transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + 180, 0);

                // configure waist vis positions
                int i = 0;
                foreach (Transform t in transform)
                {
                    float n = ((transform.childCount - 1) / 2f);
                    t.transform.localPosition = new Vector3((n - i) * (visSizeDelta + HSpacing * size), 0, 0);
                    t.localEulerAngles = new Vector3(0, 0, 0);
                    i++;
                }
            }
        }

        if (name.Contains("Head") && CameraTransform != null)
        {// script for head-level dashboard
            // configure curved display (vis)
            if (curved)
            {
                // configure dashboard position (head position)
                if (lerp) transform.position = Vector3.Lerp(transform.position, CameraTransform.position,
                    Time.deltaTime * animationSpeed);
                else transform.position = CameraTransform.position;

                //transform.position = new Vector3(transform.position.x, CameraTransform.transform.position.y + AdjustedHeight, transform.position.z);

                // configure dashboard rotation (body-fixed)
                transform.localEulerAngles = WaistTransform.localEulerAngles;

                foreach (Transform t in transform)
                {
                    UpdateVisPosition(t);

                    t.LookAt(CameraTransform);
                    t.localEulerAngles = new Vector3(-t.localEulerAngles.x, t.localEulerAngles.y + 180, 0);
                }
            }
            else
            {
                Vector3 oldAngle = CameraTransform.eulerAngles;
                CameraTransform.eulerAngles = new Vector3(0, oldAngle.y, oldAngle.z);
                Vector3 forward = CameraTransform.forward;
                CameraTransform.eulerAngles = oldAngle;

                // configure dashboard position 
                //if (lerp) transform.position = Vector3.Lerp(transform.position, 
                //    CameraTransform.TransformPoint(Vector3.zero) + forward * ForwardParameter, Time.deltaTime * animationSpeed);
                //else
                    transform.position = CameraTransform.TransformPoint(Vector3.zero) + forward * ForwardParameter;

                //transform.position = new Vector3(transform.position.x, CameraTransform.transform.position.y + AdjustedHeight, transform.position.z);

                // configure dashboard rotation
                transform.LookAt(CameraTransform);
                transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + 180, 0);

                // configure waist vis positions
                int i = 0;
                foreach (Transform t in transform)
                {
                    float n = ((transform.childCount - 1) / 2f);
                    t.transform.localPosition = new Vector3((n - i) * (visSizeDelta + HSpacing * size), 0,0);
                    t.localEulerAngles = new Vector3(0, 0, 0);
                    i++;
                }
            }
        } 
    }

    // update multiple position
    private void UpdateVisPosition(Transform vis)
    {
        if (transform.childCount > 3)
        {
            if (circle.positionCount > transform.childCount)
            {
                Vector3 nextPos = circle.GetPosition(vis.GetSiblingIndex()) / size;
                //Debug.Log(vis.name + " " + nextPos);
                vis.localPosition = Vector3.Lerp(vis.localPosition, nextPos, Time.deltaTime * animationSpeed);
                //vis.RotateAround(WaistTransform.position, Vector3.up, WaistTransform.localEulerAngles.y);
            }
        }
        else if (transform.childCount == 3)
        {
            Vector3 nextPos = Vector3.zero;
            if (vis.GetSiblingIndex() == 0)
                nextPos = new Vector3((ForwardParameter * Mathf.Cos(30 * Mathf.Deg2Rad) / size), 0, ForwardParameter * Mathf.Sin(30 * Mathf.Deg2Rad) / size);
            else if(vis.GetSiblingIndex() == 1)
                nextPos = new Vector3(0, 0, ForwardParameter / size);
            else
                nextPos = new Vector3((-ForwardParameter * Mathf.Cos(30 * Mathf.Deg2Rad) / size), 0, ForwardParameter * Mathf.Sin(30 * Mathf.Deg2Rad) / size);
            vis.localPosition = Vector3.Lerp(vis.localPosition, nextPos, Time.deltaTime * animationSpeed);
        }
        else if (transform.childCount == 2)
        {
            Vector3 nextPos = Vector3.zero;
            if (vis.GetSiblingIndex() == 0) 
                nextPos = new Vector3((ForwardParameter * Mathf.Cos(60 * Mathf.Deg2Rad) / size), 0, ForwardParameter * Mathf.Sin(60 * Mathf.Deg2Rad) / size);
            else
                nextPos = new Vector3((-ForwardParameter * Mathf.Cos(60 * Mathf.Deg2Rad) / size), 0, ForwardParameter* Mathf.Sin(60 * Mathf.Deg2Rad) / size);
            vis.localPosition = Vector3.Lerp(vis.localPosition, nextPos, Time.deltaTime * animationSpeed);
        }
        else if (transform.childCount == 1) {
            vis.localPosition = Vector3.Lerp(vis.localPosition, new Vector3(0,0,ForwardParameter / size), Time.deltaTime * animationSpeed);
        }
        
    }
}
