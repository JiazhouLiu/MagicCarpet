using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootCollision : MonoBehaviour
{

    public DashboardController_PhysicalTouch DC;

    private void OnTriggerExit(Collider other)
    {
        if (other.name.Contains("Metropolitan")) {
            if (name == "LeftFoot")
                DC.RemoveLeftFootPhysical(other.transform);
            else
                DC.RemoveRightFootPhysical(other.transform);
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("Metropolitan"))
        {
            if (name == "LeftFoot")
                DC.RegisterLeftFootPhysical(other.transform);
            else
                DC.RegisterRightFootPhysical(other.transform);
        }
    }
}
