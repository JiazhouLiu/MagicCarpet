using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootControlSwitch : MonoBehaviour
{
    public DataManagerFloorMenu_Map dmfm;
    private bool switchOn = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "LeftFoot" || other.name == "RightFoot") {
            if (switchOn)
            {
                switchOn = false;
                GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 50f / 225f);
            }
            else {
                switchOn = true;
                GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, 50f / 225f);
            }
                
            dmfm.OnKickSwitch(switchOn);
        }
            
    }
}
