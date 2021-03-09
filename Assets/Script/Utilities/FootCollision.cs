using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootCollision : MonoBehaviour
{

    public DelaunayController dc;

    private void OnTriggerExit(Collider other)
    {
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
