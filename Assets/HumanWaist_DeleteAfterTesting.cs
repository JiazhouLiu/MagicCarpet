using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Camera.main != null) {
            transform.position = new Vector3(Camera.main.transform.position.x, 0.5f, Camera.main.transform.position.z);
            transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
        }
    }
}
