using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;
using VRTK;
using DG.Tweening;

public class VisController : MonoBehaviour
{
    [SerializeField]
    private DashboardController DC;
    [SerializeField]
    private VRTK_InteractableObject interactableObject;
    [SerializeField]
    private Vis visualisation;
    [SerializeField]
    private BoxCollider currentBoxCollider;
    [SerializeField]
    private Rigidbody currentRigidbody;

    [Header("Variables")]
    public float speed = 3;

    private Vector3 originalWorldPos;
    private Vector3 originalPos;
    private Quaternion originalRot;

    private Transform CameraTransform;
    private List<Transform> selectedVis;

    private bool isThrowing = false;
    private bool isTouchingDisplaySurface = false;

    private DisplaySurface touchingDisplaySurface;

    private float deletionTimer = 0;


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
            transform.SetParent(interactableObject.GetGrabbingObject().transform);
            transform.localScale = Vector3.one;

            // Check if the vis is being pulled from the waist Dashboard
            if (ShowingOnWaistDashboard)
            {
                visualisation.OnWaistDashBoard = false;

                //DataLogger.Instance.LogActionData(this, OriginalOwner, photonView.Owner, "Vis Created", ID);
            }
            // Check if the chart is being pulled from the head Dashboard
            else if (ShowingOnHeadDashboard)
            {
                visualisation.OnHeadDashBoard = false;

                //remove vis from ground parent
                if (DC.GroundVisParent.Find(name)) {
                    Destroy(DC.GroundVisParent.Find(name));
                    DC.RemoveFromHeadDashboard(this);
                }

                //DataLogger.Instance.LogActionData(this, OriginalOwner, photonView.Owner, "Vis Created", ID);
                //VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(interactableObject.GetGrabbingObject()), 0.4f);
            }
        }
        else if (isThrowing)
        {
            if (1 < deletionTimer)
            {
                isThrowing = false;
                ColliderActiveState = false;
                transform.DOScale(0, 1f).OnComplete(() => DC.ReturnToPocket(this));

                //DataLogger.Instance.LogActionData(this, OriginalOwner, photonView.Owner, "Vis Destroyed", ID);
            }
            else
            {
                deletionTimer += Time.deltaTime;
            }
        }
    }

    private void VisGrabbed(object sender, InteractableObjectEventArgs e)
    {
        originalWorldPos = transform.position;
        originalPos = transform.localPosition;
        originalRot = transform.localRotation;
        //DataLogger.Instance.LogActionData(this, OriginalOwner, photonView.Owner, "Vis Grab start", ID);
    }

    private void VisUngrabbed(object sender, InteractableObjectEventArgs e)
    {
        //DataLogger.Instance.LogActionData(this, OriginalOwner, photonView.Owner, "Vis Grab end", ID);

        // Check to see if the chart was thrown
        float speed = currentRigidbody.velocity.sqrMagnitude;

        if (speed > 10f)
        {
            currentRigidbody.useGravity = true;
            isThrowing = true;
            deletionTimer = 0;

            //DataLogger.Instance.LogActionData(this, OriginalOwner, photonView.Owner, "Vis Thrown", ID);
        }
        else
        {
            currentRigidbody.velocity = Vector3.zero;
            currentRigidbody.angularVelocity = Vector3.zero;

            // If it wasn't thrown, check to see if it is being placed on the display screen
            if (isTouchingDisplaySurface)
            {
                AttachToDisplayScreen();
            }
            else
                AnimateTowards(originalPos, originalRot, 0.4f);      
        }
    }

    private void OnDestroy()
    {
        //Unsubscribe to events
        interactableObject.InteractableObjectGrabbed -= VisGrabbed;
        interactableObject.InteractableObjectUngrabbed -= VisUngrabbed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DisplaySurface"))
        {
            isTouchingDisplaySurface = true;
            touchingDisplaySurface = other.GetComponent<DisplaySurface>();

            // If the chart was thrown at the screen, attach it to the screen
            if (isThrowing)
            {
                isThrowing = false;
                currentRigidbody.useGravity = false;
                currentRigidbody.velocity = Vector3.zero;
                currentRigidbody.angularVelocity = Vector3.zero;
                AttachToDisplayScreen();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("DisplaySurface"))
        {
            isTouchingDisplaySurface = false;
            touchingDisplaySurface = null;
        }
    }

    private void AttachToDisplayScreen()
    {
        Vector3 pos;
        Quaternion rot;

        touchingDisplaySurface.CalculatePositionOnScreen(this, out pos, out rot);

        AnimateTowards(pos, rot, 0.2f);

        isTouchingDisplaySurface = false;
        touchingDisplaySurface = null;

        //DataLogger.Instance.LogActionData(this, OriginalOwner, photonView.Owner, "Vis Attached to Wall", ID);
    }

    public void AnimateTowards(Vector3 targetPos, Quaternion targetRot, float duration, bool toDestroy = false)
    {
        ColliderActiveState = false;

        if (toDestroy)
            transform.DOMove(targetPos, duration).SetEase(Ease.OutQuint).OnComplete(() => DC.ReturnToPocket(this));
        else
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

    public bool ShowingOnHeadDashboard
    {
        get { return visualisation.OnHeadDashBoard; }
        set
        {
            if (value == ShowingOnHeadDashboard)
                return;
        }
    }

    public bool ShowingOnWaistDashboard
    {
        get { return visualisation.OnWaistDashBoard; }
        set
        {
            if (value == ShowingOnWaistDashboard)
                return;
        }
    }
}