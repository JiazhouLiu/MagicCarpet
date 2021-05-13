using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;
using VRTK;
using DG.Tweening;
using UnityEngine.UI;

public class VisInteractionController_UserStudy : MonoBehaviour
{
    [SerializeField]
    private DashboardController_UserStudy DC;
    [SerializeField]
    private VRTK_InteractableObject interactableObject;
    [SerializeField]
    private Vis visualisation;
    [SerializeField]
    private BoxCollider currentBoxCollider;
    [SerializeField]
    private Rigidbody currentRigidbody;
    [SerializeField]
    private MeshRenderer backgroundMR;

    public bool isGrabbing = false;
    private bool startOfExp = true;
    private bool isTouchingDisplaySurface = false;
    private DisplaySurface touchingDisplaySurface;

    private Transform previousParent;
    private Vector3 previousPosition;
    private Vector3 previousRotation;
    private Vector3 lastRotation;
    private Vector3 previousScale;

    private Vis beforeGrabbing;
    private Vector3 movingPosition;
    private bool moveInside = false;
    private bool initialisePosition = true;

    private void Awake()
    {
        // Subscribe to events
        interactableObject.InteractableObjectGrabbed += VisGrabbed;
        interactableObject.InteractableObjectUngrabbed += VisUngrabbed;

        interactableObject.InteractableObjectUsed += VisUsed;

        beforeGrabbing = new Vis();
    }

    private void Update()
    {
        if (interactableObject.IsGrabbed())
        {
            //transform.localScale = Vector3.one * 0.5f;
            //transform.localEulerAngles = new Vector3(45, 0, 0);
            //lastRotation = DC.TableTopDisplay.InverseTransformPoint(transform.eulerAngles);
            Quaternion localRotation = Quaternion.Inverse(DC.TableTopDisplay.rotation) * transform.rotation;
            lastRotation = localRotation.eulerAngles;
        }

        if (DC.Landmark == ReferenceFrames.Body && transform.parent != null && transform.parent.name == "Body Reference Frame - Waist Level Display")
        {
            transform.LookAt(DC.Shoulder);
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x + 180, transform.localEulerAngles.y, transform.localEulerAngles.z + 180);
        }

        if(!isGrabbing)
            DetectOutOfScreenAndAdjustPosition();            
    }

    #region Interaction Event: Trigger, grabbing, detection
    private void VisGrabbed(object sender, InteractableObjectEventArgs e)
    {
        isGrabbing = true;
        previousParent = transform.parent;
        previousPosition = transform.localPosition;
        previousRotation = transform.localEulerAngles;
        previousScale = transform.localScale;
        beforeGrabbing.CopyEntity(GetComponent<Vis>());
        GetComponent<Vis>().OnGround = false;
        GetComponent<Vis>().OnWaist = false;
        GetComponent<Vis>().OnShelves = false;
        //GetComponent<Vis>().Selected = false;
        GetComponent<Vis>().showHighlighted = false;
        GetComponent<Vis>().Moving = true;
    }

    private void VisUngrabbed(object sender, InteractableObjectEventArgs e)
    {
        isGrabbing = false;
        transform.SetParent(null);
        GetComponent<Vis>().Moving = false;
        GetComponent<Vis>().CopyEntity(beforeGrabbing);

        if (DC.Landmark == ReferenceFrames.Body)
        {
            if (isTouchingDisplaySurface)
                AttachToDisplayScreen();
            else if (moveInside)
            {
                transform.SetParent(previousParent);
                transform.localScale = previousScale;
            }
            else
                ReturnToLastState();
        }
        else if (DC.Landmark == ReferenceFrames.Floor)
        {
            if (isTouchingDisplaySurface)
                AttachToDisplayScreen();
            else
                ReturnToLastState();
        }
        else if (DC.Landmark == ReferenceFrames.Shelves) {
            if (isTouchingDisplaySurface)
                AttachToDisplayScreen();
            else
            {
                BoxCollider b = DC.TableTop.GetComponent<BoxCollider>();
                Vector3 v1 = DC.TableTop.transform.TransformPoint(b.center + new Vector3(b.size.x, b.size.y, b.size.z) * 0.5f);
                Vector3 v2 = DC.TableTop.transform.TransformPoint(b.center + new Vector3(b.size.x, b.size.y, -b.size.z) * 0.5f);
                Vector3 v3 = DC.TableTop.transform.TransformPoint(b.center + new Vector3(-b.size.x, b.size.y, b.size.z) * 0.5f);
                Plane surfacePlane = new Plane(v1,v2,v3);
                Vector3 closestPointOnPlane = surfacePlane.ClosestPointOnPlane(transform.position);
                transform.SetParent(previousParent);
                transform.position = closestPointOnPlane;
                transform.localEulerAngles = new Vector3(90, lastRotation.y, lastRotation.z);
                transform.localScale = previousScale;
            }
        }
    }

    private void VisUsed(object sender, InteractableObjectEventArgs e)
    {
        if (GetComponent<Vis>().Selected)
            DC.RemoveExplicitSelection(transform);
        else
            DC.AddExplicitSelection(transform);
    }

    private void OnDestroy()
    {
        //Unsubscribe to events
        interactableObject.InteractableObjectGrabbed -= VisGrabbed;
        interactableObject.InteractableObjectUngrabbed -= VisUngrabbed;
        interactableObject.InteractableObjectUsed -= VisUsed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DisplaySurface") && (DC.Landmark == ReferenceFrames.Shelves || DC.Landmark == ReferenceFrames.Floor))
        {
            isTouchingDisplaySurface = true;
            touchingDisplaySurface = other.GetComponent<DisplaySurface>();
            currentRigidbody.isKinematic = true;

            if (!isGrabbing)
                AttachToDisplayScreen();

            if (initialisePosition)
                initialisePosition = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("InteractableObj") &&
            transform.parent.name != "Original Visualisation List") {
        }
        
        if (!initialisePosition && other.CompareTag("InteractableObj") && !isGrabbing &&
            !other.GetComponent<VisInteractionController_UserStudy>().isGrabbing &&
            transform.parent.name != "Original Visualisation List")
        {
            other.GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().isKinematic = false;
            if (DC.Landmark == ReferenceFrames.Shelves) {
                Vector3 forceDirection = transform.localPosition - other.transform.localPosition;
                GetComponent<Rigidbody>().AddForce((new Vector3(forceDirection.x, 0, forceDirection.z)).normalized * 50, ForceMode.Force);
            }
            else
                GetComponent<Rigidbody>().AddForce((transform.position - other.transform.position).normalized * 100, ForceMode.Force);
        }

        if (other.CompareTag("DisplaySurface") && DC.Landmark == ReferenceFrames.Body)
        {
            if (isGrabbing)
            {
                movingPosition = transform.localPosition;
                moveInside = true;
            }
            else
            {
                if (!initialisePosition) {
                    currentRigidbody.isKinematic = false;
                    //movingPosition = DC.RefineMovingPosition(transform, movingPosition);
                    currentRigidbody.AddForce(movingPosition.normalized * 100, ForceMode.Force);
                }
                
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("DisplaySurface") && (DC.Landmark == ReferenceFrames.Shelves || DC.Landmark == ReferenceFrames.Floor))
        {
            isTouchingDisplaySurface = false;
            touchingDisplaySurface = null;
        }
        else if (other.CompareTag("DisplaySurface") && DC.Landmark == ReferenceFrames.Body)
        {
            if (!isGrabbing)
            {
                isTouchingDisplaySurface = true;
                touchingDisplaySurface = other.GetComponent<DisplaySurface>();
                currentRigidbody.isKinematic = true;

                AttachToDisplayScreen();
            }
            else
            {
                isTouchingDisplaySurface = false;
                touchingDisplaySurface = null;
                moveInside = false;
            }

            if (initialisePosition)
                initialisePosition = false;
        }

        if (!initialisePosition && other.CompareTag("InteractableObj") && !isGrabbing &&
            !other.GetComponent<VisInteractionController_UserStudy>().isGrabbing &&
            transform.parent.name != "Original Visualisation List")
        {
            other.GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    private void DetectOutOfScreenAndAdjustPosition() {
        if (DC.Landmark == ReferenceFrames.Floor)
        {
            if (transform.localPosition.x < -1.75f) // too far to the left
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(-1.75f, transform.localPosition.y, transform.localPosition.z), 10 * Time.deltaTime);
            }
            if (transform.localPosition.x > 1.75f) // too far to the right
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(1.75f, transform.localPosition.y, transform.localPosition.z), 10 * Time.deltaTime);
            }
            if (transform.localPosition.z < -1.75f)// too far to the back
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, transform.localPosition.y, -1.75f), 10 * Time.deltaTime);
            }
            if (transform.localPosition.z > 1.75f) // too far to the front
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, transform.localPosition.y, 1.75f), 10 * Time.deltaTime);
            }
        }
        else if (DC.Landmark == ReferenceFrames.Shelves)
        {
            if (transform.localPosition.x < -0.9f) // too far to the left
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(-0.9f, transform.localPosition.y, transform.localPosition.z), 10 * Time.deltaTime);
            }
            if (transform.localPosition.x > 0.9f) // too far to the right
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0.9f, transform.localPosition.y, transform.localPosition.z), 10 * Time.deltaTime);
            }
            if (transform.localPosition.z < -0.2f)
            {  // too far to the bottom
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, transform.localPosition.y, -0.2f), 10 * Time.deltaTime);
            }
            if (transform.localPosition.z > 0.2f) // too far to the top
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, transform.localPosition.y, 0.2f), 10 * Time.deltaTime);
            }
        }
    }
    #endregion

    #region vis related
    private void ReturnToLastState() {
        transform.SetParent(previousParent);
        transform.localPosition = previousPosition;
        transform.localEulerAngles = previousRotation;
        transform.localScale = previousScale;
    }

    private void AttachToDisplayScreen()
    {
        if(transform.parent != touchingDisplaySurface.mappedReferenceFrame)
            transform.SetParent(touchingDisplaySurface.mappedReferenceFrame);

        Vector3 pos;
        Quaternion rot;
        touchingDisplaySurface.CalculatePositionOnScreen(this, out pos, out rot, DC.Landmark);
        if (DC.Landmark != ReferenceFrames.Body) {
            AnimateTowards(pos, rot, 0f);
        }

        isTouchingDisplaySurface = false;
        touchingDisplaySurface = null;

        if (DC.Landmark == ReferenceFrames.Body) {
            transform.localScale = DC.LandmarkSizeOnBody * Vector3.one;
            visualisation.OnGround = false;
            visualisation.OnWaist = true;
            visualisation.OnHead = false;
            visualisation.OnShelves = false;
        }
        else if (DC.Landmark == ReferenceFrames.Shelves) {
            transform.localScale = DC.LandmarkSizeOnShelves * Vector3.one;
            visualisation.OnGround = false;
            visualisation.OnWaist = false;
            visualisation.OnHead = false;
            visualisation.OnShelves = true;
        }
    }
    #endregion

    #region Utilities
    public void AnimateTowards(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        ColliderActiveState = false;


        if (DC.Landmark == ReferenceFrames.Body) {
            transform.DOMove(targetPos, duration).SetEase(Ease.OutQuint).OnComplete(() =>
            {
                ColliderActiveState = true;
            });
        }
        else if(DC.Landmark == ReferenceFrames.Shelves)
        {
            transform.localPosition = new Vector3(targetPos.x, 0.04f, targetPos.z);
        }
        else {
            transform.DOLocalMove(targetPos, duration).SetEase(Ease.OutQuint).OnComplete(() =>
            {
                ColliderActiveState = true;
            });
        }
        transform.DOLocalRotate(targetRot.eulerAngles, duration).SetEase(Ease.OutQuint);
    }

    public bool ColliderActiveState
    {
        get { return GetComponent<Collider>().enabled; }
        set
        {
            if (value == ColliderActiveState)
                return;
        }
    }
    #endregion
}
