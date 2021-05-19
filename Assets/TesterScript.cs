using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesterScript : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        gameObject.SetActive(true);
        Debug.Log(transform.eulerAngles);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }
}
