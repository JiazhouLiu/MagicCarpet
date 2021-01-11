using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanBodyCollision : MonoBehaviour
{
    MagicCarpetManager mcm;
    private void Start()
    {
        mcm = GameObject.Find("MagicCarpetManager").GetComponent<MagicCarpetManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.name == "marker")
            mcm.LoadView(other.gameObject.GetComponent<Marker>());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "marker")
            mcm.OffMarker(other.gameObject.GetComponent<Marker>());
    }

    private void Update()
    {
        transform.rotation = Quaternion.identity;
    }
}
