using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepCamera : MonoBehaviour
{

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
