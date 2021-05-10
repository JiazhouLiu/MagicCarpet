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

    private bool isTouchingDisplaySurface = false;
    private DisplaySurface touchingDisplaySurface;

    private Transform previousParent;
    private Vector3 previousPosition;
    private Vector3 previousRotation;
    private Vector3 previousScale;

    private void Awake()
    {
        // Subscribe to events
        interactableObject.InteractableObjectGrabbed += VisGrabbed;
        interactableObject.InteractableObjectUngrabbed += VisUngrabbed;
    }

    private void Update()
    {
        if (interactableObject.IsGrabbed())
        {
            transform.localScale = Vector3.one * 0.5f;
            transform.localEulerAngles = Vector3.zero;

            // Check if the vis is being pulled from the waist Dashboard
            if (visualisation.OnWaist)
            {
                visualisation.OnWaist = false;
            }
        }
        //else
        //{
        //    GetComponent<Rigidbody>().useGravity = false;
        //    GetComponent<Rigidbody>().isKinematic = true;
        //}
    }

    private void VisGrabbed(object sender, InteractableObjectEventArgs e)
    {
        previousParent = transform.parent;
        previousPosition = transform.localPosition;
        previousRotation = transform.localEulerAngles;
        previousScale = transform.localScale;
    }

    private void VisUngrabbed(object sender, InteractableObjectEventArgs e)
    {
        transform.SetParent(null);

        if (isTouchingDisplaySurface)
            AttachToDisplayScreen();
        else
            ReturnToLastState();
    }

    private void OnDestroy()
    {
        //Unsubscribe to events
        interactableObject.InteractableObjectGrabbed -= VisGrabbed;
        interactableObject.InteractableObjectUngrabbed -= VisUngrabbed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DisplaySurface") && (DC.Landmark == ReferenceFrames.Shelves || DC.Landmark == ReferenceFrames.Floor))
        {
            isTouchingDisplaySurface = true;
            touchingDisplaySurface = other.GetComponent<DisplaySurface>();
            currentRigidbody.isKinematic = true;

            AttachToDisplayScreen();
        }
        else if (other.CompareTag("DisplaySurface") && DC.Landmark == ReferenceFrames.Body)
        {
            isTouchingDisplaySurface = false;
            touchingDisplaySurface = null;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("DisplaySurface") && (DC.Landmark == ReferenceFrames.Shelves || DC.Landmark == ReferenceFrames.Floor))
        {
            isTouchingDisplaySurface = false;
            touchingDisplaySurface = null;
        }
        else if (other.CompareTag("DisplaySurface") && DC.Landmark == ReferenceFrames.Body) {
            isTouchingDisplaySurface = true;
            touchingDisplaySurface = other.GetComponent<DisplaySurface>();
            currentRigidbody.isKinematic = true;

            AttachToDisplayScreen();
        }
    }

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
        AnimateTowards(pos, rot, 0f);

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

    public void AnimateTowards(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        ColliderActiveState = false;

        transform.DOLocalMove(targetPos, duration).SetEase(Ease.OutQuint).OnComplete(() =>
        {
            ColliderActiveState = true;
        });

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
}
