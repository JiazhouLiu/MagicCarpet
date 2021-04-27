using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootToeCollision : MonoBehaviour
{
    public List<Transform> TouchedObjs;

    private void Update()
    {

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteractableObj"))
        {
            if (TouchedObjs.Contains(other.transform))
                TouchedObjs.Remove(other.transform);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InteractableObj"))
        {
            if (!TouchedObjs.Contains(other.transform))
                TouchedObjs.Add(other.transform);
        }
    }
}
