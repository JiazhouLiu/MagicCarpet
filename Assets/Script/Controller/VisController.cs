using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;
using VRTK;
using DG.Tweening;
using UnityEngine.UI;

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
    [SerializeField]
    private MeshRenderer backgroundMR;

    [Header("Variables")]
    public float speed = 3;

    private bool isThrowing = false;
    private bool isTouchingDisplaySurface = false;

    private DisplaySurface touchingDisplaySurface;

    private float deletionTimer = 0;

    private Transform previousParent;


    private void Awake()
    {
        // Subscribe to events
        interactableObject.InteractableObjectGrabbed += VisGrabbed;
        interactableObject.InteractableObjectUngrabbed += VisUngrabbed;
    }

    private void Update()
    {
        if (visualisation.OnGround && !visualisation.Highlighted && !visualisation.Selected) {
            backgroundMR.material.SetColor("_BaseColor", new Color(1, 1, 1, 1));
        }
        else if (visualisation.OnGround || interactableObject.IsGrabbed() || visualisation.Selected)
            backgroundMR.material.SetColor("_BaseColor", Color.white);
        else
            backgroundMR.material.SetColor("_BaseColor", new Color(1, 1, 1, 0.5f)); 

        if (interactableObject.IsGrabbed())
        {
            //transform.SetParent(interactableObject.GetGrabbingObject().transform);
            transform.localScale = Vector3.one * 0.5f;
            transform.localEulerAngles = Vector3.zero;

            // Check if the vis is being pulled from the waist Dashboard
            if (visualisation.OnWaist)
            {
                visualisation.OnWaist = false;

                //DataLogger.Instance.LogActionData(this, OriginalOwner, photonView.Owner, "Vis Created", ID);
            }
            // Check if the chart is being pulled from the head Dashboard
            else if (visualisation.OnHead || visualisation.OnGround)
            {
               
                visualisation.OnHead = false;
                visualisation.OnGround = false;

                //remove vis from ground parent

                DC.RemoveFromHeadDashboard(this, previousParent);

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
                DC.ReturnToPocket(this);
                GetComponent<Rigidbody>().useGravity = false;
                GetComponent<Rigidbody>().isKinematic = true;
                //transform.DOScale(0, 1f).OnComplete(() => DC.ReturnToPocket(this));
                //Debug.Log("deleting");
                //DataLogger.Instance.LogActionData(this, OriginalOwner, photonView.Owner, "Vis Destroyed", ID);
            }
            else
            {
                deletionTimer += Time.deltaTime;
            }
        }
        else {
            GetComponent<Rigidbody>().useGravity = false;
            GetComponent<Rigidbody>().isKinematic = true;
        }

    }

    private void VisGrabbed(object sender, InteractableObjectEventArgs e)
    {
        previousParent = transform.parent;

        currentRigidbody.velocity = Vector3.zero;
    }

    private void VisUngrabbed(object sender, InteractableObjectEventArgs e)
    {
        transform.SetParent(null);
        //DataLogger.Instance.LogActionData(this, OriginalOwner, photonView.Owner, "Vis Grab end", ID);

        // Check to see if the chart was thrown
        float speed = GetComponent<Rigidbody>().velocity.sqrMagnitude;
        //Debug.Log(speed);
        if (speed > 10f)
        {
            //GetComponent<Rigidbody>().AddForce(GetComponent<Rigidbody>().velocity * 10, ForceMode.Acceleration);
            GetComponent<Rigidbody>().useGravity = true;
            GetComponent<Rigidbody>().isKinematic = false;
            isThrowing = true;
            deletionTimer = 0;
            
            //DataLogger.Instance.LogActionData(this, OriginalOwner, photonView.Owner, "Vis Thrown", ID);
        }
        else
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            // If it wasn't thrown, check to see if it is being placed on the display screen
            if (isTouchingDisplaySurface)
            {
                AttachToDisplayScreen();
            }
            else
                DC.ReturnToPocket(this);
                //AnimateTowards(originalPos, originalRot, 0.4f);
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
            //Debug.Log("enter display surface");
            isTouchingDisplaySurface = true;
            touchingDisplaySurface = other.GetComponent<DisplaySurface>();

            // If the chart was thrown at the screen, attach it to the screen
            if (isThrowing)
            {
                //Debug.Log("throw to display surface");
                isThrowing = false;
                currentRigidbody.useGravity = false;
                currentRigidbody.isKinematic = true;
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

        //Debug.Log("Attach to screen method");

        touchingDisplaySurface.CalculatePositionOnScreen(this, out pos, out rot);

        //transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * speed);
        //transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, rot.eulerAngles, Time.deltaTime * speed);
        AnimateTowards(pos, rot, 0f);

        isTouchingDisplaySurface = false;
        touchingDisplaySurface = null;

        transform.SetParent(DC.GroundVisParent);
        transform.localScale = Vector3.one;
        visualisation.OnGround = true;
        visualisation.OnWaist = false;
        visualisation.OnHead = false;

        //DataLogger.Instance.LogActionData(this, OriginalOwner, photonView.Owner, "Vis Attached to Wall", ID);
    }

    public void AttachToDisplayScreen(DisplaySurface ds)
    {
        Vector3 pos;
        Quaternion rot;

        //Debug.Log("Attach to screen method");

        ds.CalculatePositionOnScreen(this, out pos, out rot);

        //transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * speed);
        //transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, rot.eulerAngles, Time.deltaTime * speed);
        AnimateTowards(pos, rot, 0f);

        isTouchingDisplaySurface = false;
        ds = null;

        transform.SetParent(DC.GroundVisParent);
        transform.localScale = Vector3.one;
        GetComponent<Vis>().OnGround = true;
        GetComponent<Vis>().OnWaist = false;
        GetComponent<Vis>().OnHead = false;

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
}