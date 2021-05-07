using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ReferenceFrameController_UserStudy : MonoBehaviour
{
    [Header("Prefabs")]
    public DashboardController_UserStudy DC_UserStudy;
    public Transform GroundVisParent;

    [Header("Circle")]
    public LineRenderer circle;
    public float display3VisAngle = 30;

    [Header("Variables")]
    public DisplayDashboard display = new DisplayDashboard();
    public bool Curved;
    public float ForwardParameter;
    public float VisSize;
    public float HSpacing;
    public float animationSpeed;
    public float headDashboardSizeMagnifier;


    private Transform CameraTransform;
    private Transform WaistTransform;

    private void Awake()
    {
        // sync Camera/Human Game Object
        WaistTransform = DC_UserStudy.HumanWaist;
    }

    // Update is called once per frame
    void Update()
    {
        // main camera setup
        if (Camera.main != null && CameraTransform == null)
            CameraTransform = Camera.main.transform;

        if (display == DisplayDashboard.WaistDisplay)
        { // script for waist-level dashboard
            // configure curved display (vis)
            foreach (Transform t in transform)
                t.localScale = Vector3.Lerp(t.localScale, Vector3.one, Time.deltaTime * animationSpeed);

            if (Curved)
            {
                // configure dashboard position
                transform.position = Vector3.Lerp(transform.position, WaistTransform.position,
                    Time.deltaTime * animationSpeed);

                foreach (Transform t in transform)
                {
                    UpdateVisPosition(t);

                    t.LookAt(CameraTransform);
                    t.localEulerAngles = new Vector3(-t.localEulerAngles.x, t.localEulerAngles.y + 180, 0);
                }

                // configure dashboard rotation (body-fixed)
                transform.localEulerAngles = WaistTransform.localEulerAngles;
            }
            else
            {
                Vector3 forward = WaistTransform.forward;

                // configure dashboard position
                transform.position = WaistTransform.TransformPoint(Vector3.zero) + forward * ForwardParameter;

                // configure dashboard rotation
                transform.LookAt(WaistTransform);
                transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + 180, 0);

                // configure waist vis positions
                int i = 0;
                foreach (Transform t in transform)
                {
                    float n = ((transform.childCount - 1) / 2f);
                    t.transform.localPosition = new Vector3((n - i) * (VisSize + HSpacing * VisSize), 0, 0);
                    t.localEulerAngles = new Vector3(0, 0, 0);
                    i++;
                }
            }
        }

        if (display == DisplayDashboard.HeadDisplay && CameraTransform != null)
        {// script for head-level dashboard

            // configure curved display (vis)
            if (Curved)
            {
                // configure dashboard position (head position)
                transform.position = Vector3.Lerp(transform.position, CameraTransform.position,
                    Time.deltaTime * animationSpeed);

                // configure dashboard rotation (body-fixed)
                transform.localEulerAngles = WaistTransform.localEulerAngles;
                transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);


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
                transform.position = CameraTransform.TransformPoint(Vector3.zero) + forward * ForwardParameter;

                // configure dashboard rotation
                transform.LookAt(CameraTransform);
                transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + 180, 0);

                // configure waist vis positions
                int i = 0;
                foreach (Transform t in transform)
                {
                    float n = ((transform.childCount - 1) / 2f);
                    t.transform.localPosition = new Vector3((n - i) * (VisSize + HSpacing * VisSize), 0, 0);
                    t.localEulerAngles = new Vector3(0, 0, 0);
                    i++;
                }
            }

            // configure vis scale based on position
            if (transform.childCount > 0)
            {
                UpdateVisScale();
            }
            else
            {
                foreach (Transform t in transform)
                    t.localScale = Vector3.Lerp(t.localScale, Vector3.one * headDashboardSizeMagnifier, Time.deltaTime * animationSpeed);
            }

        }

        if (display == DisplayDashboard.GroundMarkers && CameraTransform != null)
        {
            //foreach (Transform chart in transform) {
            //    if (Vector3.Distance(new Vector3(chart.position.x, 0, chart.position.z), new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z)) > 0.3f) { // north pole issue
            //        chart.LookAt(Camera.main.transform);
            //        chart.localEulerAngles = new Vector3(chart.localEulerAngles.x, chart.localEulerAngles.y + 180, chart.localEulerAngles.z);
            //        chart.localEulerAngles = new Vector3(90, chart.localEulerAngles.y, chart.localEulerAngles.z);
            //    }
            //}
        }
    }

    /// <summary>
    /// update vis position
    /// </summary>
    /// <param name="vis"> single vis attached to this transform</param>
    private void UpdateVisPosition(Transform vis)
    {
        if (transform.childCount > 3)
        {
            if (circle.positionCount > transform.childCount)
            {
                Vector3 nextPos = circle.GetPosition(vis.GetSiblingIndex()) / VisSize;
                //Debug.Log(vis.name + " " + nextPos);
                vis.localPosition = Vector3.Lerp(vis.localPosition, nextPos, Time.deltaTime * animationSpeed);
                //vis.RotateAround(WaistTransform.position, Vector3.up, WaistTransform.localEulerAngles.y);
            }
        }
        else if (transform.childCount == 3)
        {
            if (display == DisplayDashboard.HeadDisplay)
            {
                Vector3 nextPos = Vector3.zero;
                if (vis.GetSiblingIndex() == 0)
                    nextPos = new Vector3((ForwardParameter * Mathf.Cos(display3VisAngle * Mathf.Deg2Rad) / VisSize), Camera.main.transform.position.y, ForwardParameter * Mathf.Sin(display3VisAngle * Mathf.Deg2Rad) / VisSize);
                else if (vis.GetSiblingIndex() == 1)
                    nextPos = new Vector3(0, Camera.main.transform.position.y, ForwardParameter / VisSize);
                else
                    nextPos = new Vector3((-ForwardParameter * Mathf.Cos(display3VisAngle * Mathf.Deg2Rad) / VisSize), Camera.main.transform.position.y, ForwardParameter * Mathf.Sin(display3VisAngle * Mathf.Deg2Rad) / VisSize);
                vis.localPosition = Vector3.Lerp(vis.localPosition, nextPos, Time.deltaTime * animationSpeed);
            }
            else
            {
                Vector3 nextPos = Vector3.zero;
                if (vis.GetSiblingIndex() == 0)
                    nextPos = new Vector3((ForwardParameter * Mathf.Cos(30 * Mathf.Deg2Rad) / VisSize), 0, ForwardParameter * Mathf.Sin(30 * Mathf.Deg2Rad) / VisSize);
                else if (vis.GetSiblingIndex() == 1)
                    nextPos = new Vector3(0, 0, ForwardParameter / VisSize);
                else
                    nextPos = new Vector3((-ForwardParameter * Mathf.Cos(30 * Mathf.Deg2Rad) / VisSize), 0, ForwardParameter * Mathf.Sin(30 * Mathf.Deg2Rad) / VisSize);
                vis.localPosition = Vector3.Lerp(vis.localPosition, nextPos, Time.deltaTime * animationSpeed);
            }
        }
        else if (transform.childCount == 2)
        {
            Vector3 nextPos = Vector3.zero;
            if (vis.GetSiblingIndex() == 0)
                nextPos = new Vector3((ForwardParameter * Mathf.Cos(60 * Mathf.Deg2Rad) / VisSize), 0, ForwardParameter * Mathf.Sin(60 * Mathf.Deg2Rad) / VisSize);
            else
                nextPos = new Vector3((-ForwardParameter * Mathf.Cos(60 * Mathf.Deg2Rad) / VisSize), 0, ForwardParameter * Mathf.Sin(60 * Mathf.Deg2Rad) / VisSize);
            vis.localPosition = Vector3.Lerp(vis.localPosition, nextPos, Time.deltaTime * animationSpeed);
        }
        else if (transform.childCount == 1)
        {
            vis.localPosition = Vector3.Lerp(vis.localPosition, new Vector3(0, 0, ForwardParameter / VisSize), Time.deltaTime * animationSpeed);
        }

    }

    /// <summary>
    /// update vis scale based on waist position
    /// </summary>
    /// <param name="vis">single vis attached to this transform</param>
    private void UpdateVisScale()
    {
        if (transform.childCount > 1)
        {
            List<Transform> selectedGroundChildren = new List<Transform>();

            List<string> dashBoardChildrenNames = new List<string>();
            foreach (Transform t in transform)
                dashBoardChildrenNames.Add(t.name);

            foreach (Transform groundVis in GroundVisParent)
            {
                if (dashBoardChildrenNames.Contains(groundVis.name))
                {
                    //Debug.Log(groundVis.name);
                    selectedGroundChildren.Add(groundVis);
                }
            }

            if (selectedGroundChildren.Count != transform.childCount)
                Debug.Log("ERRORRRR");

            #region VisSize won't change when proximics change
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<Vis>().HeadDashboardScale = Vector3.one * headDashboardSizeMagnifier;
                selectedGroundChildren[i].GetComponent<Vis>().HeadDashboardScale = Vector3.one * headDashboardSizeMagnifier;
            }
            #endregion

            #region VisSize will change when proximics change
            //Dictionary<string, float> calculatedRatio = new Dictionary<string, float>();

            //for (int i = 0; i < selectedGroundChildren.Count; i++)
            //    calculatedRatio.Add(selectedGroundChildren[i].name, 1 / CalculateProportionalScale(WaistTransform, selectedGroundChildren[i], selectedGroundChildren));

            //// update VIS model scale
            //for (int i = 0; i < transform.childCount; i++)
            //{
            //    transform.GetChild(i).GetComponent<Vis>().HeadDashboardScale =
            //        (calculatedRatio[transform.GetChild(i).name] / calculatedRatio.Values.Sum()) * Vector3.one * headDashboardSizeMagnifier;
            //    selectedGroundChildren[i].GetComponent<Vis>().HeadDashboardScale =
            //        (calculatedRatio[selectedGroundChildren[i].name] / calculatedRatio.Values.Sum()) * Vector3.one * headDashboardSizeMagnifier;
            //}
            #endregion
        }
        else
        {// show 1 vis
            // update VIS model scale
            transform.GetChild(0).GetComponent<Vis>().HeadDashboardScale = Vector3.one * headDashboardSizeMagnifier;
            GroundVisParent.Find(transform.GetChild(0).name).GetComponent<Vis>().HeadDashboardScale = Vector3.one * headDashboardSizeMagnifier;
        }

        foreach (Transform vis in transform)
            vis.localScale = Vector3.Lerp(vis.localScale,
                vis.GetComponent<Vis>().HeadDashboardScale, Time.deltaTime * animationSpeed);
    }

    private float CalculateProportionalScale(Transform waist, Transform targetVis, List<Transform> visList)
    {
        List<Transform> copyList = visList.ToList();
        copyList.Remove(targetVis);
        float edgeLengthSum = 0;

        float waistToTarget = Vector3.Distance(waist.position, targetVis.position);
        foreach (Transform t in copyList)
            edgeLengthSum += Vector3.Distance(targetVis.position, t.position);

        return copyList.Count * waistToTarget / edgeLengthSum;
    }

    private Vector3 SmoothChangingVector3(Vector3 origin, int decimalPoint)
    {
        float newX = 0;
        float newY = 0;
        float newZ = 0;

        if (decimalPoint == 0)
        {
            newX = float.Parse(origin.x.ToString("F0"));
            newY = float.Parse(origin.y.ToString("F0"));
            newZ = float.Parse(origin.z.ToString("F0"));
        }
        else if (decimalPoint == 2)
        {
            newX = float.Parse(origin.x.ToString("F2"));
            newY = float.Parse(origin.y.ToString("F2"));
            newZ = float.Parse(origin.z.ToString("F2"));
        }

        return new Vector3(newX, newY, newZ);
    }
}
