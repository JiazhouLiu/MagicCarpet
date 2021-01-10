using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube: MonoBehaviour
{
    public float returnSpeed = 5;
    public SphereCollider sc;

    private Rigidbody rb;
    private bool onCollision = false;
    private Vector3 originalPosition;
    private ObjectGenerator og;

    private void Start()
    {
        originalPosition = transform.position;
        rb = GetComponent<Rigidbody>();

        og = GameObject.Find("SmallMultiplesGenerator").GetComponent<ObjectGenerator>();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 speed = GetComponent<Rigidbody>().velocity;
        GetComponent<Rigidbody>().velocity = Vector3.Lerp(speed, Vector3.zero, Time.deltaTime * 100);

        // return to original position if no collision
        //ReturnToOriPos();

        // Bound between multiples
        BetweenForce();

        // set orientation
        if (!onCollision) {
            ReturnToOriPos();
            transform.localEulerAngles = Vector3.zero;
        }
        else
            SetCubeRotation(transform, GameObject.Find("HumanCollider").transform);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "HumanCollider") {
            onCollision = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == "HumanCollider")
        {
            onCollision = false;
        }
    }


    // Set Multiple Rotation
    private void SetCubeRotation(Transform obj, Transform target)
    {
        // face to camera
        obj.LookAt(target);

        obj.localEulerAngles = new Vector3(0, obj.localEulerAngles.y + 180, 0);
    }


    private void ReturnToOriPos() {
        if (transform.position != originalPosition)
        {
            rb.AddForce((originalPosition - transform.position) * returnSpeed, ForceMode.VelocityChange);
        }
    }

    private void BetweenForce() {
        int selfIndex = transform.GetSiblingIndex();
        int parentIndex = transform.parent.GetSiblingIndex();

        Transform rightSibling = null;
        Transform leftSibling = null;
        Transform topSibling = null;
        Transform bottomSibling = null;

        if (parentIndex != og.ColumnNumber - 1)
            rightSibling = transform.parent.parent.GetChild(parentIndex + 1).GetChild(selfIndex);

        if(parentIndex != 0)
            leftSibling = transform.parent.parent.GetChild(parentIndex - 1).GetChild(selfIndex);

        if(selfIndex != og.RowNumber - 1)
            bottomSibling = transform.parent.GetChild(selfIndex + 1);

        if (selfIndex != 0)
            topSibling = transform.parent.GetChild(selfIndex - 1);

        CheckSiblingDistance(rightSibling);
        CheckSiblingDistance(leftSibling);
        CheckSiblingDistance(topSibling);
        CheckSiblingDistance(bottomSibling); 
    }

    private void CheckSiblingDistance(Transform t) {
        if (t != null) {
            if (Vector3.Distance(transform.position, t.position) > (sc.radius * 2)) {
                rb.AddForce((t.position - transform.position) * returnSpeed * 3, ForceMode.VelocityChange);
            }
        }
    }
}
