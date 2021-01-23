using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanBodyCollisionPCD : MonoBehaviour
{
    MagicCarpetManager_PCD mcm;
    private void Start()
    {
        mcm = GameObject.Find("MagicCarpetManager").GetComponent<MagicCarpetManager_PCD>();
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
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }
}
