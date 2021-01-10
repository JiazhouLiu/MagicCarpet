using UnityEngine;
using System.Collections;

// Applies an explosion force to all nearby rigidbodies
public class HumanCollider : MonoBehaviour
{
    public float radius = 5.0F;
    public float power = 10.0F;
    public float capHeight = 5.0f;

    void Start()
    {
        //Vector3 explosionPos = transform.position;
        ////Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        //Collider[] colliders = Physics.OverlapCapsule((explosionPos + Vector3.up * capHeight / 2), (explosionPos - Vector3.up * capHeight / 2), radius);
        //foreach (Collider hit in colliders)
        //{
        //    Rigidbody rb = hit.GetComponent<Rigidbody>();

        //    if (rb != null)
        //    {
        //        rb.AddExplosionForce(power, explosionPos, radius, 3.0F);
        //    }
        //}
    }

    private void Update()
    {
        // change layout shortcut
        if (Input.GetKeyDown("q"))
            ChangeRadius(false);

        if (Input.GetKeyDown("e"))
            ChangeRadius(true);
    }

    private void ChangeRadius(bool push)
    {
        if (push)
        {
            if (radius < 100) {
                radius += 0.1f;
                //GetComponent<CapsuleCollider>().radius += 0.1f;
                GetComponent<SphereCollider>().radius += 0.1f;
            }
                
        }
        else
        {
            if (radius > 0) {
                radius -= 0.1f;
                //GetComponent<CapsuleCollider>().radius -= 0.1f;
                GetComponent<SphereCollider>().radius -= 0.1f;
            }
        }
    }
}