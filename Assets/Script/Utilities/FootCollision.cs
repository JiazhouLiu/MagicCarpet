using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootCollision : MonoBehaviour
{

    public VisController dc;

    private void OnTriggerStay(Collider other)
    {
        //Color newColor = other.gameObject.GetComponent<MeshRenderer>().material.color;
        //newColor.a = 100f / 255f;
        //other.gameObject.GetComponent<MeshRenderer>().material.color = newColor;
        //Debug.Log(other.name);
    }

    private void OnTriggerExit(Collider other)
    {
        //Color newColor = other.gameObject.GetComponent<MeshRenderer>().material.color;
        //newColor.a = 0f / 255f;
        //other.gameObject.GetComponent<MeshRenderer>().material.color = newColor;

        if (name == "LeftFoot")
            dc.RemoveLeftFootPhysical(other.transform);
        else
            dc.RemoveRightFootPhysical(other.transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (name == "LeftFoot")
            dc.RegisterLeftFootPhysical(other.transform);
        else
            dc.RegisterRightFootPhysical(other.transform);
    }
}
