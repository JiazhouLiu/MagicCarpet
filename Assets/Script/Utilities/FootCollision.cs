using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootCollision : MonoBehaviour
{
    public DashboardController_PhysicalTouch DC;
    public Transform TouchedObj;

    private void OnTriggerExit(Collider other)
    {
        if (DC != null)
        {
            if (other.name.Contains("Metropolitan"))
            {
                if (name == "LeftFoot")
                    DC.RemoveLeftFootPhysical(other.transform);
                else
                    DC.RemoveRightFootPhysical(other.transform);
            }
        }
        else {
            if (other.CompareTag("InteractableObj")) {
                TouchedObj = null;
            }
        } 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (DC != null)
        {
            if (other.name.Contains("Metropolitan"))
            {
                if (name == "LeftFoot")
                    DC.RegisterLeftFootPhysical(other.transform);
                else
                    DC.RegisterRightFootPhysical(other.transform);
            }
        }
        else {
            if (other.CompareTag("InteractableObj"))
            {
                
                TouchedObj = other.transform;
            }
        }
    }
}
