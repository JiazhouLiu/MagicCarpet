using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootMenuController : MonoBehaviour
{
    public DashboardController dc;
    public DashboardController_PhysicalTouch dcpt;
    public Transform leftFoot;
    public Transform rightFoot;
    public Transform waist;
    public List<Transform> groundVisParent;
    public Transform headVisParent;
    public Transform carpet;
    public Transform menuItems;

    [Header("Variable")]
    public float StandStillTime = 3f;
    public float footRaiseHeight = 0.2f;
    public float footMoveDistance = 0.005f;
    public float changeScaleDelta = 0.3f;
    public float changeSpeed = 1f;

    private float standStillTimer = 0;
    private bool footMenu = false;

    private Vector3 previousLeftFootPosition;
    private Vector3 previousRightFootPosition;

    // Start is called before the first frame update
    void Start()
    {
        previousLeftFootPosition = leftFoot.position;
        previousRightFootPosition = rightFoot.position;
    }

    // Update is called once per frame
    void Update()
    {
        // follow the waist
        transform.position = new Vector3(waist.position.x, 0.01f, waist.position.z);
        transform.localEulerAngles = new Vector3(0, waist.localEulerAngles.y, 0);

        // raise foot to cancel footmenu
        if (leftFoot.position.y > footRaiseHeight || rightFoot.position.y > footRaiseHeight) 
        {
            if (footMenu) {
                footMenu = false;
                if(dc != null)
                    dc.footMenu = false;
                if(dcpt != null)
                    dcpt.footMenu = false;
            }
            standStillTimer = 0;
        }
        else {
            if (!footMenu)
            {
                if (Vector3.Distance(leftFoot.position, previousLeftFootPosition) > footMoveDistance || Vector3.Distance(rightFoot.position, previousRightFootPosition) > footMoveDistance)
                    standStillTimer = 0;
                else
                    standStillTimer += Time.deltaTime;
            }
        }

        if (standStillTimer > StandStillTime) {
            footMenu = true;
            if (dc != null)
                dc.footMenu = true;
            if (dcpt != null)
                dcpt.footMenu = true;
            standStillTimer = 0;
        }

        previousLeftFootPosition = leftFoot.position;
        previousRightFootPosition = rightFoot.position;

        // make menu visible
        if (footMenu)
        {
            carpet.gameObject.SetActive(true);
            menuItems.gameObject.SetActive(true);

            // foot menu functions
            CheckAndChangeLandmarksScale();
        }
        else {
            carpet.gameObject.SetActive(false);
            menuItems.gameObject.SetActive(false);
        }

        
    }

    private void CheckAndChangeLandmarksScale() {
        //Debug.Log(Vector3.Angle(leftFoot.right, rightFoot.position - leftFoot.position));
        //Debug.Log(Vector3.Angle(rightFoot.right, rightFoot.position - leftFoot.position));
        if (Vector3.Angle(leftFoot.right, rightFoot.position - leftFoot.position) < 10 && Vector3.Angle(rightFoot.right, rightFoot.position - leftFoot.position) < 10) // if two feet are parallel
        {
            float diff = Vector3.Distance(leftFoot.position, rightFoot.position) - changeScaleDelta;
            foreach (Transform t in groundVisParent) {
                if (dc != null)
                {
                    foreach (Transform child in t) {
                        Vector3 result = child.localScale + Vector3.one * 0.01f * changeSpeed * diff;
                        if (result.x <= 1.5f && result.x >= 0.5f)
                        {
                            child.localScale = result;
                        }
                    }
                }
                if (dcpt != null) {
                    Vector3 result = t.localScale + Vector3.one * 0.01f * changeSpeed * diff;
                    if (result.x <= 1.5f && result.x >= 0.5f)
                    {
                        t.localScale = result;
                    }
                }
            }           
        }
    }
}
