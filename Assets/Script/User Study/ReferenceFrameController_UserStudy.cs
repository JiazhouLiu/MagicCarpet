using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ReferenceFrameController_UserStudy : MonoBehaviour
{
    [Header("Prefabs")]
    public ExperimentManager EM;
    public DashboardController_UserStudy DC_UserStudy;
    public Transform mappedTransform;

    [Header("Circle")]
    public LineRenderer circle;
    public float display3VisAngle = 30;

    [Header("Variables")]
    public DisplayDashboard display = new DisplayDashboard();
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
        WaistTransform = EM.waist;
    }

    // Update is called once per frame
    void Update()
    {
        // main camera setup
        if (Camera.main != null)
            CameraTransform = Camera.main.transform;

        if (DC_UserStudy.DetailedView == ReferenceFrames.Body && 
            display == DisplayDashboard.HeadDisplay && CameraTransform != null)
        {// script for head-level dashboard

            //Vector3 oldAngle = CameraTransform.eulerAngles;
            //CameraTransform.eulerAngles = new Vector3(0, oldAngle.y, oldAngle.z);
            //Vector3 forward = CameraTransform.forward;
            //CameraTransform.eulerAngles = oldAngle;

            //// configure dashboard position 
            //transform.position = CameraTransform.TransformPoint(Vector3.zero) + forward * ForwardParameter;

            Vector3 oldAngle = WaistTransform.eulerAngles;
            WaistTransform.eulerAngles = new Vector3(0, oldAngle.y, oldAngle.z);
            Vector3 forward = WaistTransform.forward;
            WaistTransform.eulerAngles = oldAngle;

            // configure dashboard position 
            transform.position = WaistTransform.TransformPoint(Vector3.zero) + forward * ForwardParameter;
            transform.position = new Vector3(transform.position.x, CameraTransform.position.y, transform.position.z);

            // configure dashboard rotation
            transform.localEulerAngles = WaistTransform.localEulerAngles;
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);

            // configure waist vis positions
            int i = 0;
            foreach (Transform t in transform)
            {
                float n = ((transform.childCount - 1) / 2f);
                t.transform.localPosition = new Vector3((n - i) * (VisSize + HSpacing * VisSize), 0, 0);
                t.localEulerAngles = new Vector3(0, 0, 0);
                i++;
            }

            foreach (Transform t in transform)
                t.localScale = Vector3.Lerp(t.localScale, Vector3.one * VisSize * headDashboardSizeMagnifier, Time.deltaTime * animationSpeed);
        }

        if (DC_UserStudy.DetailedView == ReferenceFrames.Shelves &&
            display == DisplayDashboard.Wall) {

            // configure vis positions
            int i = 0;
            foreach (Transform t in transform)
            {
                Vector3 landmarkPosition = t.GetComponent<Vis>().GroundPosition;
                float n = ((transform.childCount - 1) / 2f);
                t.transform.localPosition = Vector3.Lerp(t.transform.localPosition, new Vector3((n - i) * (VisSize + HSpacing * VisSize), 0.3f, -0.05f), animationSpeed * Time.deltaTime);
                t.localEulerAngles = Vector3.Lerp(t.localEulerAngles, new Vector3(0, 0, 0), animationSpeed * Time.deltaTime * 10);
                i++;
            }

            foreach (Transform t in transform)
                t.localScale = Vector3.Lerp(t.localScale, Vector3.one * VisSize, Time.deltaTime * animationSpeed);
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
                    nextPos = new Vector3((ForwardParameter * Mathf.Cos(display3VisAngle * Mathf.Deg2Rad) / VisSize), 0, ForwardParameter * Mathf.Sin(display3VisAngle * Mathf.Deg2Rad) / VisSize);
                else if (vis.GetSiblingIndex() == 1)
                    nextPos = new Vector3(0, 0, ForwardParameter / VisSize);
                else
                    nextPos = new Vector3((-ForwardParameter * Mathf.Cos(display3VisAngle * Mathf.Deg2Rad) / VisSize), 0, ForwardParameter * Mathf.Sin(display3VisAngle * Mathf.Deg2Rad) / VisSize);
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

    public void InitialiseLandmarkPositions(List<Transform> currentLandmarks, ReferenceFrames currentRF) {
        List<Vector3> landmarkPositions = new List<Vector3>();

        if (currentRF == ReferenceFrames.Floor) // landmarks on floor
        {
            foreach (Transform t in currentLandmarks)
            {
                // get random available positions and assign it to landmarks
                Vector3 newPosition = GetAvaiableRandomPosition(landmarkPositions, ReferenceFrames.Floor);
                landmarkPositions.Add(newPosition);

                t.position = newPosition;

                t.GetComponent<Rigidbody>().isKinematic = false;
                t.GetComponent<Rigidbody>().AddForce(Vector3.down * 10, ForceMode.Force);
            }
        }
        else if (currentRF == ReferenceFrames.Body) // landmarks on waist level display
        {
            foreach (Transform t in currentLandmarks)
            {
                // get random available positions and assign it to landmarks
                Vector3 newPosition = GetAvaiableRandomPosition(landmarkPositions, ReferenceFrames.Body);

                landmarkPositions.Add(newPosition);

                t.localPosition = newPosition;

                //t.GetComponent<Rigidbody>().isKinematic = false;
                //t.GetComponent<Rigidbody>().AddForce(newPosition.normalized * 50, ForceMode.Force);
            }
        }
        else if (currentRF == ReferenceFrames.Shelves) // landmarks on shelves
        {
            foreach (Transform t in currentLandmarks)
            {
                // get random available positions and assign it to landmarks
                Vector3 newPosition = GetAvaiableRandomPosition(landmarkPositions, ReferenceFrames.Shelves);
                landmarkPositions.Add(newPosition);

                t.localPosition = newPosition + Vector3.up * 0.2f;

                t.GetComponent<Rigidbody>().isKinematic = false;
                t.GetComponent<Rigidbody>().AddForce(t.InverseTransformPoint(-t.up * 10), ForceMode.Force);
            }
        }
    }

    private Vector3 GetAvaiableRandomPosition(List<Vector3> currentList, ReferenceFrames currentRF)
    {
        Vector3 tmpPosition = Vector3.zero;
        float landmarkSize = 0;

        if (currentRF == ReferenceFrames.Floor)
        {
            landmarkSize = DC_UserStudy.LandmarkSizeOnGround;
            tmpPosition = new Vector3(Random.Range(-1.5f + (DC_UserStudy.LandmarkSizeOnGround / 2), 1.5f - (DC_UserStudy.LandmarkSizeOnGround / 2)), 0.1f, 
                Random.Range(-0.5f + (DC_UserStudy.LandmarkSizeOnGround / 2), 0.5f - (DC_UserStudy.LandmarkSizeOnGround / 2)));
        }
        else if (currentRF == ReferenceFrames.Shelves)
        {
            landmarkSize = DC_UserStudy.LandmarkSizeOnShelves;
            tmpPosition = new Vector3(Random.Range(-0.9f, 0.9f), 0, Random.Range(-0.2f, 0.2f));
        }
        else if (currentRF == ReferenceFrames.Body)
        {
            landmarkSize = DC_UserStudy.LandmarkSizeOnBody;
            Vector3 direction = new Vector3(Random.Range(-0.9f, 0.9f), Random.Range(-0.7f, 0), Random.Range(0, 0.9f));
            tmpPosition = direction.normalized * EM.armLength;
        }

        if (currentList.Count > 0)
        {
            foreach (Vector3 v in currentList)
            {
                if (Vector3.Distance(v, tmpPosition) < landmarkSize * 1.1f)
                {
                    tmpPosition = GetAvaiableRandomPosition(currentList, currentRF);
                }
            }
        }

        return tmpPosition;
    }
}
