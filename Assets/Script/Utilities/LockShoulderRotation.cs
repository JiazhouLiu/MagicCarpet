using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockShoulderRotation : MonoBehaviour
{
    public Transform Waist;
    // Update is called once per frame
    void Update()
    {
        transform.rotation = Waist.rotation;
    }
}
