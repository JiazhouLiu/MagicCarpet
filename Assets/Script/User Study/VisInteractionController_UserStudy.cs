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
    private Vector3 previousScale;

    private Vis beforeGrabbing;
    private Vector3 movingPosition;
    private bool moveInside = false;

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
            transform.localScale = Vector3.one * 0.5f;
            transform.localEulerAngles = Vector3.zero;
        }

        if (DC.Landmark == ReferenceFrames.Body && transform.parent.name == "Body Reference Frame - Waist Level Display")
        {
            transform.LookAt(DC.Shoulder);
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x + 180, transform.localEulerAngles.y, transform.localEulerAngles.z + 180);
        }

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
        else if (DC.Landmark == ReferenceFrames.Shelves) {
            if (transform.localPosition.y < 0.15f) {  // too far to the bottom
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, 0.15f, transform.localPosition.z), 10 * Time.deltaTime);
            }
            if(transform.localPosition.y > 0.85f) // too far to the top
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, 0.85f, transform.localPosition.z), 10 * Time.deltaTime);
            }
            if (transform.localPosition.x < -1.3f) // too far to the left
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(-1.3f, transform.localPosition.y, transform.localPosition.z), 10 * Time.deltaTime);
            }
            if (transform.localPosition.x > 1.3f) // too far to the right
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(1.3f, transform.localPosition.y, transform.localPosition.z), 10 * Time.deltaTime);
            }
        }
    }

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
        GetComponent<Vis>().Selected = false;
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
            else if(moveInside){
                currentRigidbody.AddForce(movingPosition * 10, ForceMode.Force);
            }else
                ReturnToLastState();
        }
        else {
            if (isTouchingDisplaySurface)
                AttachToDisplayScreen();
            else
                ReturnToLastState();
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

            if(!isGrabbing)
                AttachToDisplayScreen();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("InteractableObj") && !isGrabbing && !other.GetComponent<VisInteractionController_UserStudy>().isGrabbing && transform.parent.name != "Original Visualisation List")
        {
            other.GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().AddForce((transform.position - other.transform.position) * 100, ForceMode.Force);
        }

        if (other.CompareTag("DisplaySurface") && DC.Landmark == ReferenceFrames.Body && isGrabbing) {
            movingPosition = transform.localPosition;
            moveInside = true;
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
            else {
                isTouchingDisplaySurface = false;
                touchingDisplaySurface = null;
                moveInside = false;
            }  
        }

        if (other.CompareTag("InteractableObj") && !isGrabbing && !other.GetComponent<VisInteractionController_UserStudy>().isGrabbing && transform.parent.name != "Original Visualisation List")
        {
            other.GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Rigidbody>().isKinematic = true;
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

    public void AnimateTowards(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        ColliderActiveState = false;


        if (DC.Landmark == ReferenceFrames.Body)
        {
            transform.DOMove(targetPos, duration).SetEase(Ease.OutQuint).OnComplete(() =>
            {
                ColliderActiveState = true;
            });
        }
        else {
            transform.DOLocalMove(targetPos, duration).SetEase(Ease.OutQuint).OnComplete(() =>
            {
                ColliderActiveState = true;
            });
        }
        

        transform.DOLocalRotate(targetRot.eulerAngles, duration).SetEase(Ease.OutQuint);
    }

    private Vector3 MovePositionInsideScreen(Vector3 localPos, Vector3 localVertex)
    {
        // Case 1: vertex is too far to the left
        if (localVertex.x <= -0.5f)
        {
            float delta = Mathf.Abs(-0.5f - localVertex.x);
            localPos.x += delta;
        }
        // Case 2: vertex is too far to the right
        else if (0.5f <= localVertex.x)
        {
            float delta = localVertex.x - 0.5f;
            localPos.x -= delta;
        }
        // Case 3: vertex is too far to the top
        if (0.5f <= localVertex.y)
        {
            float delta = localVertex.y - 0.5f;
            localPos.y -= delta;
        }
        // Case 4: vertex is too far to the bottom
        else if (localVertex.y <= -0.5f)
        {
            float delta = Mathf.Abs(-0.5f - localVertex.y);
            localPos.y += delta;
        }
        // Case 5: vertex is behind the screen
        if (0f <= localVertex.z)
        {
            float delta = localVertex.z;
            localPos.z -= delta;
        }

        return localPos;
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
