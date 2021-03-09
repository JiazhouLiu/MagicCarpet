using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanWaist_DeleteAfterTesting : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(Camera.main.transform.position.x, 0.5f, Camera.main.transform.position.z);
    }
}
